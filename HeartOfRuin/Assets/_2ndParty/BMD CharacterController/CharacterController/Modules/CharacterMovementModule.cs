using System.Collections;
using UnityEngine;
using Utils;
namespace BMD
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public class CharacterMovementModule : MonoBehaviour, ICharacterModule
    {
        const float MIN_WALK_SPEED = 0.1f;
        const float SPEED_OFFSET = 0.1f;
        const float DIRECTION_INPUT_GRACE_PERIOD = 0.05f;
        const float GROUND_SNAP_VELOCITY = -2f;

        #region Configuration
        [Header("Character Movement Settings")]
        [Tooltip("Speed settings for character walking.")]
        [SerializeField] float walkSpeed = 2f;            // Speed of the character movement - Speed at which animaiton will beging to transition from walk to run
        [Tooltip("Speed settings for character run. eg Full positive movement input")]
        [SerializeField] float runSpeed = 6f;             // Speed of the character when running
        [Tooltip("Speed settings for various character sprint")]
        [SerializeField] float sprintSpeed = 10f;         // Speed of the character when sprinting
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] float movementAcceleration = 10.0f;
        [SerializeField] float minMoveInputMagnitude = 0.05f; // Minimum input magnitude to consider movement

        [Header("Rotation Settings")]
        [Tooltip("Toggle rotation")]
        [SerializeField] bool rotationEnabled = true; 
        [Tooltip("Rotation speed in degrees per second")]
        [SerializeField] float rotationSpeed = 10f;

        [SerializeField, Tooltip("When moving slower than this speed, rotation snaps instantly")]
        float instantTurnThreshold = 0.05f;

        [Header("Jump and fall Settings")]
        [SerializeField] bool  canJump = true;      // Whether the character can jump
        [SerializeField] float jumpForce = 5f;      // Force applied when jumping
        [SerializeField] int   aerialJumps = 1;     // Number of additional jumps allowed in the air
        [SerializeField] bool  airControl = true;   // Whether the character can control movement in the air
        [Range(0, 1)]
        [SerializeField] float airControlFactor = 0.5f; // Factor by which air control is applied to movement speed
        [SerializeField] float gravityScale = 1f;       // Scale factor for gravity applied to the character
        [SerializeField] float terminalVelocity = 53f;  // Maximum downward velocity due to gravity
        [Tooltip("Coyote Time Settings, applies if falling off object even if jump is disabled")]
        [SerializeField] float coyoteTime = 0.1f;       // Time window after leaving ground during which a jump can still be performed

        [Header("Dodge Settings")]
        [SerializeField] bool  canDodge = true;     // Whether the character can dodge
        [SerializeField] bool  allowAirDodge = true; // Whether dodging is allowed in the air
        [SerializeField] float dodgeRange = 5f;     // Distance covered during a dodge
        [Range(0.0f, 5f), Tooltip("Amount of time the dodge takes to perform. 0 is instant transmission.")]
        [SerializeField] float dodgeTime = 0.2f;    // Duration of the dodge action
        [Tooltip("Invulnerability timings relative to dodge start time")]
        [SerializeField] float invlunerabilityStartTime = 0.0f; // Time after dodge start when invulnerability begins
        [Tooltip("Invulnerability timings relative to dodge start time")]
        [SerializeField] float invlunerabilityEndTime = 0.1f;   // Time after dodge start when invulnerability ends
        [SerializeField] float dodgeCooldown = 2f;  // Cooldown time between dodges

        [Header("Roll Settings")]
        [SerializeField] bool canRoll = true;       // Whether the character can dodge
        [Range(0.0f, 20f), Tooltip("Distance covered by the roll")]
        [SerializeField] float rollRange = 5f;      // Distance covered during a dodge
        [SerializeField] float rollTime = 1f;     // Duration of the dodge action
        [SerializeField] float rollCooldown = 2f;   // Cooldown time between dodges

        #endregion

        #region Cached references
        private CharacterController controller;
        private UnityEngine.CharacterController unityController;
        #endregion

        #region Runtime Variables
        float verticalVelocity;
        float moveSpeed = 0.0f;
        Vector3 currentHorizontalVelocity = Vector3.zero;  // Declared here to avoid creating new vectors each frame, garbage collection optimisation.
        int currentAerialJumps = 0;
        bool isSprintHeld = false;
        bool isSprinting = false;
        bool isRolling = false;
        bool isDodging = false;
        bool isInvulnerable = false;
        float dodgeRollMoveSpeed = 0.0f; // Speed during dodge/roll movement, calculated when needed.
        Vector3 dodgeRollDirection = Vector3.zero; // The direction of input at time of dodge or roll
        Coroutine dodgeRollCoroutine;

        // Timings
        float lastGroundedTime = 0f;
        float nextDodgeTime = 0.0f;
        float dodgeEndTime = 0.0f;
        float nextRollTime = 0.0f;
        float rollEndTime = 0.0f;
        float becomeVulnerableTime = 0.0f;
        float becomeInvulnerableTime = 0.0f;
        float dodgeRollGraceTime = 0.0f;
        #endregion
        #region Properties
        private CharacterState CurrentState
        {
            get { return controller.CurrentState; }
            set { controller.CurrentState = value; }
        }
        // Enable sprinting and do not disable until speed drops below walk speed
        bool IsSprinting            { get { return isSprintHeld || isSprinting && unityController.velocity.magnitude > walkSpeed; } }
        bool IsConsideredGrounded   { get { return unityController.isGrounded || (Time.time - lastGroundedTime) <= coyoteTime; } }   // Reusable property for coyote time check
        public bool IsInvulnerable  { get { return isInvulnerable; } }
        public (float walk, float run, float sprint) LocomotionScales => (walkSpeed, runSpeed, sprintSpeed);
        public float TurnAngle { get; private set; }

        #endregion
        public void Initialize(CharacterController controller)
        {
            InitializeReferences(controller);
            InitializeSignals(controller);
            InitializeSanityChecks();
            
        }
        private void InitializeSanityChecks()
        {
            runSpeed = Mathf.Max(runSpeed, walkSpeed);
            sprintSpeed = Mathf.Max(sprintSpeed, runSpeed);
            walkSpeed = Mathf.Max(walkSpeed, MIN_WALK_SPEED);

            invlunerabilityStartTime = Mathf.Clamp(invlunerabilityStartTime, 0f, dodgeTime);
            invlunerabilityEndTime = Mathf.Clamp(invlunerabilityEndTime, invlunerabilityStartTime, dodgeTime);
            dodgeCooldown = Mathf.Max(dodgeTime, dodgeCooldown);

            rollCooldown = Mathf.Max(rollTime, rollCooldown);
            rollRange = Mathf.Max(float.Epsilon, rollRange);       // Must not be 0
            rollTime = Mathf.Max(float.Epsilon, rollTime);         // Must not be 0

        }
        private void InitializeSignals(CharacterController controller)
        {
            controller.OnJumpRequested += HandleJumpRequested;
            controller.OnDodgeRequested += HandleDodgeRequested;
            controller.OnRollRequested += HandleRollRequested;
            controller.OnSprintDown += HandleSprintDown;
            controller.OnSprintUp += HandleSprintUp;
        }
        private void InitializeReferences(CharacterController controller)
        {
            this.controller = controller;
            unityController = controller.GetComponent<UnityEngine.CharacterController>();
        }
        public void Tick(float deltaTime)
        {
            isSprinting = IsSprinting;

            if(isRolling && Time.time >= rollEndTime) isRolling = false;
            if(isDodging && Time.time >= dodgeEndTime) isDodging = false;

            HandleInvulnerability();

        }
        public void FixedTick(float fixedDeltaTime)
        {
            
            ApplyVerticalMovement(fixedDeltaTime);                      // 1) update verticalVelocity, landing, coyote
            DetermineMovement(out var dir, out var s, fixedDeltaTime);  // 2) decide horizontal direction & speed (idle/walk/run/sprint/dodge/roll)
            ApplyMovement(dir, s, fixedDeltaTime);                      // 3) combine horiz + vertical, move CC, cache horiz vel
            RotateCharacterTowardsMovement(fixedDeltaTime);             // 4) rotate (instant during dodge/roll)
            UpdateState();                                              // 5) emit state changes
        }
        #region Input Event Handlers
        private void HandleJumpRequested()
        {
            if (!canJump) return;

            if (IsConsideredGrounded)
            {
                verticalVelocity = jumpForce;
                currentAerialJumps = 0;
                controller.NotifyJumpPerformed();
            }
            else if (currentAerialJumps < 1) // could use controller.GetAerialJumpCount()
            {
                verticalVelocity = jumpForce;
                currentAerialJumps++;
                controller.NotifyJumpPerformed();
            }
        }
        private void HandleDodgeRequested()
        {
            if (!canDodge || Time.time < nextDodgeTime) return;

            if (!IsConsideredGrounded && !allowAirDodge) return; // Dodging only allowed on ground for now

            nextDodgeTime = Time.time + dodgeCooldown;
            dodgeEndTime = Time.time + dodgeTime;
            isDodging = true;
            becomeInvulnerableTime = Time.time + invlunerabilityStartTime;
            becomeVulnerableTime = Time.time + invlunerabilityEndTime;
            dodgeRollGraceTime = Time.time + DIRECTION_INPUT_GRACE_PERIOD;

            dodgeRollMoveSpeed = dodgeRange / dodgeTime;
            dodgeRollDirection = controller.MoveDirection.normalized;

            if (dodgeRollCoroutine != null) StopCoroutine(dodgeRollCoroutine);
            dodgeRollCoroutine = StartCoroutine(SetDodgeRollDirection());

            controller.NotifyDodgePerformed();
        }
        private void HandleRollRequested()
        {
            if (!canRoll || Time.time < nextRollTime) return;
            if (!IsConsideredGrounded) return; // Rolling only allowed on ground for now

            nextRollTime = Time.time + rollCooldown;
            rollEndTime = Time.time + rollTime;
            isRolling = true;

            dodgeRollMoveSpeed = rollRange / rollTime;
            dodgeRollDirection = controller.MoveDirection.normalized;
            dodgeRollGraceTime = Time.time + DIRECTION_INPUT_GRACE_PERIOD;

            if (dodgeRollCoroutine != null) StopCoroutine(dodgeRollCoroutine);
            dodgeRollCoroutine = StartCoroutine(SetDodgeRollDirection());

            controller.NotifyRollPerformed();
        }
        private void HandleSprintDown()
        {
            isSprintHeld = true;
        }
        private void HandleSprintUp()
        {
            isSprintHeld = false;
        }
        #endregion
        IEnumerator SetDodgeRollDirection()
        {
            // Loop until dodge roll ends updating direction based on input
            while ((Time.time < dodgeRollGraceTime) && (isDodging || isRolling))
            {
                if ( controller.MoveDirection != Vector3.zero)
                    dodgeRollDirection = controller.MoveDirection.normalized;
                yield return new WaitForFixedUpdate();
            }
        }
        private void HandleInvulnerability()
        {
            // Handle invulnerability timing
            if (isInvulnerable)
            {
                if (Time.time >= becomeVulnerableTime)
                    isInvulnerable = false;
            }
            else
            {
                if (Time.time >= becomeInvulnerableTime)
                    isInvulnerable = true;
            }
        }
        private void ApplyVerticalMovement(float dt)
        {
            // Update last grounded time first
            if (unityController.isGrounded)
                lastGroundedTime = Time.time;

            // Scale gravity only on Y (character “weight” / fall speed)
            float weightedGravityY = Physics.gravity.y * gravityScale;

            // Ground snap & landing notify
            if (IsConsideredGrounded && verticalVelocity < 0f)
            {
                // Only fire landed if we were actually falling faster than the snap
                if (verticalVelocity < GROUND_SNAP_VELOCITY)
                    controller.NotifyJumpLanded();

                verticalVelocity = -2f;  // keep CC stably grounded
                currentAerialJumps = 0;  // if you track aerial jumps, reset here
                return;
            }

            // Airborne: integrate gravity and clamp to terminal velocity
            verticalVelocity += weightedGravityY * dt;
            if (verticalVelocity < -terminalVelocity)
                verticalVelocity = -terminalVelocity;
        }

        private void DetermineMovement(out Vector3 moveDir, out float resolvedSpeed, float dt)
        {
            // Default: use controller.MoveDirection (already camera/world relative)
            moveDir = controller.MoveDirection;
            float inputMagnitude = moveDir.magnitude;

            // Special states: dodge/roll lock direction & speed
            if (isDodging || isRolling)
            {
                moveDir = dodgeRollDirection;              // locked when action started
                resolvedSpeed = dodgeRollMoveSpeed;
                return;
            }

            if (inputMagnitude < minMoveInputMagnitude || (!IsConsideredGrounded && !airControl))
            {
                moveDir = Vector3.zero;
                resolvedSpeed = 0f;
                return;
            }
            
            if (airControl && !IsConsideredGrounded)
                inputMagnitude *= airControlFactor;

            // Ternary operator to choose between runSpeed and sprintSpeed, walk speed is not used here and is only for threshholds within this module
            float targetSpeed = isSprinting ? sprintSpeed : runSpeed;

            // Smooth accel/decel toward target * inputMagnitude
            float currentHorizontalSpeed = currentHorizontalVelocity.magnitude;
            
            if (currentHorizontalSpeed < targetSpeed - SPEED_OFFSET ||
            currentHorizontalSpeed > targetSpeed + SPEED_OFFSET)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                resolvedSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    dt * movementAcceleration);

                // round speed to 3 decimal places
                resolvedSpeed = Mathf.Round(resolvedSpeed * 1000f) / 1000f;
            }
            else
            {
                resolvedSpeed = targetSpeed;
            }

            resolvedSpeed = Mathf.Max(0f, resolvedSpeed);   // Preventions friction bugs from making speed negative
            moveSpeed = resolvedSpeed;
        }
        private void ApplyMovement(Vector3 moveDir, float moveSpeed, float dt)
        {
            Vector3 move = moveDir * moveSpeed;
            move.y = verticalVelocity;

            unityController.Move(move * dt);

            // Cache horizontal velocity for rotation/state
            currentHorizontalVelocity.x = unityController.velocity.x;
            currentHorizontalVelocity.y = 0f;
            currentHorizontalVelocity.z = unityController.velocity.z;
        }
        private void RotateCharacterTowardsMovement(float dt)
        {
            if (!rotationEnabled) return;

            // Vector 3 comparison uses approximation to account for floating point errors
            if (currentHorizontalVelocity == Vector3.zero) return; // Nothing to rotate towards


            Vector3 targetDir = currentHorizontalVelocity.normalized;
            Vector3 currentDir = unityController.transform.forward;

            TurnAngle = Vector3.SignedAngle(currentDir, targetDir, Vector3.up);

            // Compute target rotation
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);

            // Snap instantly if barely moving (prevents jitter)
            if (isDodging || isRolling || currentHorizontalVelocity.magnitude < instantTurnThreshold)
            {
                unityController.transform.rotation = targetRotation;
                return;
            }

            // Smooth rotation
            unityController.transform.rotation = Quaternion.Slerp(
                unityController.transform.rotation,
                targetRotation,
                rotationSpeed * dt
            );
        }
        private void UpdateState()
        {
            CharacterState newState;
                     
            if (IsConsideredGrounded)
            {
                if (moveSpeed < MIN_WALK_SPEED)
                    newState = CharacterState.Idle;
                else if (moveSpeed <= walkSpeed)
                    newState = CharacterState.Walking;
                else if (moveSpeed <= runSpeed)
                    newState = CharacterState.Running;
                else
                    newState = CharacterState.Sprinting;

            }
            else
                newState = verticalVelocity > 0f ? CharacterState.Jumping : CharacterState.Falling;

            // Do nothing if state unchanged
            if (newState != CurrentState)
            {
                CurrentState = newState;
                controller.NotifyStateChanged(CurrentState);
            }
        }
        public void Dispose()
        {
            if(dodgeRollCoroutine != null) StopCoroutine(dodgeRollCoroutine);

            controller.OnJumpRequested -= HandleJumpRequested;
            controller.OnSprintDown -= HandleSprintDown;
            controller.OnSprintUp -= HandleSprintUp;

            controller.OnDodgeRequested -= HandleDodgeRequested;
            controller.OnRollRequested -= HandleRollRequested;
        }
    }
}
