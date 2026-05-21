using UnityEngine;
using UnityEngine.InputSystem;

namespace BMD
{
    public class PlayerController : BMD.CharacterController
    {
        const float STICK_DEADZONE = 0.1f;

        #region Cached references
        PlayerControls playerControls;
        InputAction moveAction;
        InputAction lookAction;
        InputAction aimAction;
        InputAction zoomAction;
        InputAction jumpAction;
        InputAction rollAction;
        InputAction crouchAction;
        InputAction sprintAction;
        InputAction fireAction;
        InputAction attackAction;
        InputAction specialAttackAction;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            SetupControls();

            if (Camera == null)
            {
                Debug.LogWarning("No camera defined by character controller, attempting to search children");
                RegisterCamera(GetComponentInChildren<Camera>());       // Attempt backup setup, find camera in child to assign to character controller.

                if (Camera == null) Debug.LogWarning("No camera found on the player. Please attach a camera module or child camera.");
                return;
            }
        }

        private void SetupControls()
        {
            playerControls = new PlayerControls();
            moveAction = playerControls.Player.Move;
            jumpAction = playerControls.Player.Jump;
            lookAction = playerControls.Player.Look;
            aimAction = playerControls.Player.Aim;
            zoomAction = playerControls.Player.Zoom;
            crouchAction = playerControls.Player.Crouch;
            rollAction = playerControls.Player.Roll;
            sprintAction = playerControls.Player.Sprint;
            fireAction = playerControls.Player.Fire;
            attackAction = playerControls.Player.Attack;
            specialAttackAction = playerControls.Player.SpecialAttack;
        }
        private void OnEnable()
        {
            playerControls.Player.Enable();
            lookAction.performed += ctx => HandleLookInput(ctx);
            lookAction.canceled += ctx => HandleLookInput();
            zoomAction.performed += ctx => AdjustZoomLevel(-ctx.ReadValue<float>());
            zoomAction.canceled += ctx => AdjustZoomLevel(0f);
            crouchAction.performed += ctx => ToggleCrouch();
            rollAction.performed += ctx => PerformRoll();
            sprintAction.started += ctx => NotifySprintTriggered(true);
            sprintAction.canceled += ctx => NotifySprintTriggered(false);
        }
        private void OnDisable()
        {
            playerControls.Player.Disable();
        }
        protected override void Update()
        {
            HandleJumpInput();
            HandleAttackInput();
            base.Update();
        }

        private void HandleLookInput(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
        private void HandleLookInput() => lookInput = Vector2.zero;
        private void AdjustZoomLevel(float zd) => NotifyZoomChanged(zd);

        private void HandleJumpInput()
        {
            if (jumpAction.WasPressedThisFrame())
            {
                RequestJump();
            }
        }
        private void HandleAttackInput()
        {
            if (IsAttacking) return; // don't allow new attack input until current attack finishes
            if (attackAction.WasPressedThisFrame())
            {
                SetAim();
                RequestAttack();

            }

            if (specialAttackAction.WasPressedThisFrame())
            {
                SetAim();
                RequestSpecialAttack();
            }
            if (fireAction.WasPressedThisFrame())
            {
                SetAim();
                RequestFireWeapon();
            }
        }
        void SetAim()
        {
            Vector2 aimInput = aimAction.ReadValue<Vector2>();
            Debug.Log($"Aim input: {aimInput}");
            if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated(STICK_DEADZONE))
            {
                AimWithStick(aimInput);
            }
            else
            {
                AimWithMouse(aimInput);
            }
        }
        void AimWithStick(Vector2 aimInput)
        {
            if (aimInput.sqrMagnitude < STICK_DEADZONE * STICK_DEADZONE) return;
            // Convert to camera relative aiming direction
            Vector3 aimDir = (Camera.transform.forward * aimInput.y + Camera.transform.right * aimInput.x);
            aimDir.y = 0f;
            aimDir.Normalize();
            aimDirection = aimDir;
        }
        private void AimWithMouse(Vector2 screenPosition)
        {
            Ray ray = Camera.ScreenPointToRay(screenPosition);

            Debug.DrawRay(ray.origin, ray.direction, Color.yellow, 5.0f);

            // Infinite horizontal plane through the player
            Plane aimPlane = new Plane(Vector3.up, transform.position);

            if (!aimPlane.Raycast(ray, out float enter))  return;

            Vector3 worldPoint = ray.GetPoint(enter);
            Debug.DrawRay(ray.origin, ray.direction * enter, Color.green, 5.0f);

            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;

            aimDirection = direction.normalized;

            Debug.DrawRay(transform.position, direction, Color.blue, 5.0f);

            Debug.Log($"Screen Position: {screenPosition}, Aim Direction: {aimDirection}");

            
        }
        protected override void FixedUpdate()
        {
            SetMoveDirection();

            base.FixedUpdate(); // controller.Tick() and FixedTick() will trigger module updates

        }
        private void SetMoveDirection()
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            float inputMagnitude = moveInput.magnitude;
            inputMagnitude = Mathf.Pow(inputMagnitude, 1.5f); // smoother start

            // TODO swapped this from camera root while look is frozen for demo.
            //Vector3 moveDir = (cameraRoot.forward * moveInput.y + cameraRoot.right * moveInput.x);
            Vector3 moveDir = (Camera.transform.forward * moveInput.y + Camera.transform.right * moveInput.x);
            moveDir.y = 0f;
            moveDirection = moveDir.normalized * inputMagnitude;
        }
        protected override void ToggleCrouch()
        {
            if (crouchAction.WasPressedThisFrame())
            {
                base.ToggleCrouch();
            }
        }
        private void PerformRoll()
        {
            if (rollAction.WasPressedThisFrame())
            {
                RequestRoll();
            }
        }

    }
}
