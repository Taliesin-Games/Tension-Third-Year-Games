using UnityEngine;



#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
#endif

namespace BMD
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimatorModule : CharacterModule
    {
        [Tooltip("Rate of change when a parameter affects a blend tree.\n" +
            "Only applies to values inside blend trees. Does NOT affect animator transitions.\n" +
            "Smaller is faster.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] float blendTreeTransitionRate = 0.05f;     // The reat at which float parameters are smoothed
        [SerializeField] float attackLayerTransitionInTime = 0.05f;
        [SerializeField] float attackLayerTransitionOutTime = 0.2f;

        #region Animation Hashes
        // Animation Layers
        private static int attackLayerIndex = -1;

        // State tracking
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int IsFlyingHash = Animator.StringToHash("IsFlying");
        private static readonly int IsSwimmingHash = Animator.StringToHash("IsSwimming");
        private static readonly int IsDodgingHash = Animator.StringToHash("IsDodging");
        private static readonly int IsRollingHash = Animator.StringToHash("IsRolling");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
        private static readonly int CharacterStateHash = Animator.StringToHash("CharacterState");
        
        // Movement blend parameters
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int TurnAngleHash = Animator.StringToHash("TurnAngle");

        // Style blend parameters
        private static readonly int IdleStyleHash = Animator.StringToHash("IdleStyle");
        private static readonly int AttackStyleHash = Animator.StringToHash("AttackStyle");
        private static readonly int Attack2StyleHash = Animator.StringToHash("Attack2Style");

        // Triggers and other parameters
        private static readonly int SwitchIdleHash = Animator.StringToHash("SwitchIdle");
        private static readonly int CrouchTriggerHash = Animator.StringToHash("CrouchTrigger");
        private static readonly int StandTriggerHash = Animator.StringToHash("StandTrigger");
        private static readonly int RollTriggerHash = Animator.StringToHash("RollTrigger");
        private static readonly int DodgeTriggerHash = Animator.StringToHash("DodgeTrigger");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");
        private static readonly int LandTriggerHash = Animator.StringToHash("LandTrigger");
        private static readonly int BlockTriggerHash = Animator.StringToHash("BlockTrigger");
        private static readonly int FireWeaponTriggerHash = Animator.StringToHash("FireWeaponTrigger");
        private static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        private static readonly int Attack2TriggerHash = Animator.StringToHash("Attack2Trigger");
        #endregion

        #region Cached References
        Animator animator;
        BMD.CharacterController controller;
        UnityEngine.CharacterController unityController;
        #endregion

        #region Runtime Variables
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private HashSet<int> validParams;   // store parameter hashes
        private HashSet<int> warnedParams;  // avoid duplicate warnings
#endif
        (float walk, float run, float sprint) locomotionScales;
        bool preInitialized = false;
        bool initialized = false;
        float attackLayerTargetWeight = 0f;
        #endregion

        #region Properties
        CharacterState CurrentState => controller.CurrentState;
        bool IsGrounded => unityController.isGrounded;
        bool IsAttacking => controller.IsAttacking;
        float AttackLayerTransitionTime => IsAttacking ? attackLayerTransitionInTime : attackLayerTransitionOutTime;
        #endregion

        public override void PreInitialize(BMD.CharacterController controller)
        {
            if (preInitialized) return;    // Prevent double initialization
            preInitialized = true;

            CacheReferences(controller);
        }
        public override void Initialize(BMD.CharacterController controller)
        {
            if (initialized) return;    // Prevent double initialization
            initialized = true;

            if (attackLayerIndex == -1) attackLayerIndex = animator.GetLayerIndex("Attack Layer");  // TODO magic string remove!!!

            InitializeSignals(controller);

            locomotionScales = controller.LocomotionScales;
        }

        private void CacheReferences(CharacterController controller)
        {
            this.controller = controller;
            animator = controller.GetComponent<Animator>();
            unityController = controller.GetComponent<UnityEngine.CharacterController>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BuildAnimatorParamCache();
#endif
        }

        private void InitializeSignals(CharacterController controller)
        {
            // Character state changes
            controller.OnStateChanged += HandleStateChanged;

            // Jump events
            controller.OnJumpPerformed += HandleJumpPerformed;
            controller.OnJumpLanded += HandleLanded;

            // Roll events
            controller.OnRollPerformed += HandleRollPerformed;
            controller.OnRollEnded += HandleRollEnded;

            // Dodge events
            controller.OnDodgePerformed += HandleDodgePerformed;
            controller.OnDodgeEnded += HandleDodgeEnded;

            // Attack events
            controller.OnFireWeaponPerformed += HandleFireWeaponPerformed;
            controller.OnFireWeaponEnded     += HandleFireWeaponEnded;

            controller.OnAttackPerformed     += HandleAttackPerformed;
            controller.OnDisableDamageFromWeapon         += HandleAttackEnded;

            controller.OnSpecialAttackPerformed += HandleSpecialAttackPerformed;
            controller.OnSpecialAttackEnded  += HandleSpecialAttackEnded;

        }
        public override void Tick(float deltaTime)
        {
            // Safety check for missing references
            if (animator == null || controller == null|| unityController == null) return;

            LocomotionTick(deltaTime);
            LayerBlendTick(deltaTime);

            animator.SetInteger(CharacterStateHash, (int)CurrentState);
        }
        private void LocomotionTick(float deltaTime)
        {
            // Update movement blend parameters per frame

            // Use actual movement velocity for realistic blending
            Vector3 velocity = unityController.velocity;
            float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;

            // Output locomotionScale values to check scaling
            //Debug.Log($"Locomotion scales - walk: {locomotionScales.walk} run: {locomotionScales.run} sprint: {locomotionScales.sprint}");

            float normalizedSpeed = horizontalSpeed switch
            {
                _ when horizontalSpeed < locomotionScales.walk =>   Mathf.InverseLerp(0f, locomotionScales.walk, horizontalSpeed),
                _ when horizontalSpeed < locomotionScales.run =>    Mathf.InverseLerp(locomotionScales.walk, locomotionScales.run, horizontalSpeed) + 1f,
                _ when horizontalSpeed < locomotionScales.sprint => Mathf.InverseLerp(locomotionScales.run, locomotionScales.sprint, horizontalSpeed) + 2f,
                _ => 3f
            };

            animator.SetFloat(SpeedHash, normalizedSpeed, blendTreeTransitionRate, deltaTime);
            animator.SetFloat(VerticalVelocityHash, velocity.y);
            animator.SetBool(IsGroundedHash, IsGrounded);
            
            animator.SetFloat(TurnAngleHash, controller.TurnAngle, blendTreeTransitionRate, deltaTime);
        }
        private void LayerBlendTick(float deltaTime)
        {
            float currentWeight = animator.GetLayerWeight(attackLayerIndex);
            float newWeight = Mathf.MoveTowards(
                currentWeight, 
                attackLayerTargetWeight, 
                deltaTime / AttackLayerTransitionTime
                );
            animator.SetLayerWeight(attackLayerIndex, newWeight);
        }
        public override void FixedTick(float fixedDeltaTime)
        {
            // Animator does not need fixed-timestep updates
        }

        #region Event Handlers
        // character state change handler
        private void HandleStateChanged(CharacterState state)
        {
            // Auto updated but do we need to do anything extra here?
        }
        // Jump event handlers
        private void HandleJumpPerformed()
        {
            animator.SetTrigger(JumpTriggerHash);
        }
        private void HandleLanded()
        {
            animator.SetTrigger(LandTriggerHash);
            animator.SetBool(IsGroundedHash, true);
        }
        //Roll Event Handlers
        private void HandleRollPerformed()
        {
            animator.SetTrigger(RollTriggerHash);
        }
        private void HandleRollEnded()
        {
            // Additional logic for when roll ends can be added here
        }
        // Dodge Event Handlers
        private void HandleDodgePerformed()
        {
            animator.SetTrigger(DodgeTriggerHash);
        }
        private void HandleDodgeEnded()
        {
            // Additional logic for when dodge ends can be added here
        }

        private void HandleAttackPerformed()
        {
            
            animator.SetBool(IsAttackingHash, true);
            animator.SetTrigger(AttackTriggerHash);
            SetAttackFade(true);
            
        }
        private void HandleAttackEnded()
        {
            // Additional logic for when attack ends can be added here
            animator.SetBool(IsAttackingHash, false);
            SetAttackFade(false);
        }

        private void HandleSpecialAttackPerformed()
        {

            animator.SetBool(IsAttackingHash, true);
            animator.SetTrigger(Attack2TriggerHash);
            SetAttackFade(true);
        }
        private void HandleSpecialAttackEnded()
        {
            // Additional logic for when attack ends can be added here
            animator.SetBool(IsAttackingHash, false);
            SetAttackFade(false);
        }
        private void HandleFireWeaponPerformed()
        {
            animator.SetBool(IsAttackingHash, true);
            animator.SetTrigger(FireWeaponTriggerHash);
            SetAttackFade(true);
        }
        private void HandleFireWeaponEnded()
        {
            // Additional logic for when fire weapon ends can be added here
            animator.SetBool(IsAttackingHash, false);
            SetAttackFade(false);
        }

        #endregion

        #region Animation Triggers
        public void AT_DisableWeaponDamage() { controller.NotifyAttackEnded(); }
        public void AT_SpecialAttackEnded() { controller.NotifySpecialAttackEnded(); }
        public void AT_FireWeaponEnded() { controller.NotifyFireWeaponEnded(); }
        public void AT_EnableWeaponDamage() { controller.NotifyDealDamageFromWeapon(); }
        public void AT_CastSpell() { controller.NotifyCastSpell(); }
        #endregion

        private void SetAttackFade(bool enable = true)
        {
            attackLayerTargetWeight = enable ? 1 : 0;
        }
        public override void Dispose()
        {
            if (controller == null) return;
            
            controller.OnJumpPerformed -= HandleJumpPerformed;
            controller.OnJumpLanded -= HandleLanded;
            controller.OnStateChanged -= HandleStateChanged;

            controller.OnRollPerformed -= HandleRollPerformed;
            controller.OnRollEnded -= HandleRollEnded;

            controller.OnDodgePerformed -= HandleDodgePerformed;
            controller.OnDodgeEnded -= HandleDodgeEnded;

            controller.OnAttackPerformed -= HandleAttackPerformed;
            controller.OnDisableDamageFromWeapon -= HandleAttackEnded;

            controller.OnSpecialAttackPerformed -= HandleSpecialAttackPerformed;
            controller.OnSpecialAttackEnded -= HandleSpecialAttackEnded;

            controller.OnFireWeaponPerformed -= HandleFireWeaponPerformed;
            controller.OnFireWeaponEnded -= HandleFireWeaponEnded;
        }
        #region Parameter Validation
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void BuildAnimatorParamCache()
        {
            if (animator == null)
            {
                Debug.LogWarning($"{name}: Animator not assigned, cannot build parameter cache.");
                return;
            }
            const int NAME_PADDING = 25;
            const int HASH_PADDING = 10;
            const int TYPE_PADDING = 10;

            validParams = new HashSet<int>();
            warnedParams = new HashSet<int>();

            string msg = $"Listing all valid parameters in animator '{animator.runtimeAnimatorController.name}':\n";
            msg += "PARAMETER NAME".PadRight(NAME_PADDING - 5)  // Magic number because we are winging the width at this point
                + "\t"
                + "HASH".PadRight(HASH_PADDING)
                + "\t\t\t"
                + " TYPE".PadRight(TYPE_PADDING)
                + "\n";
            msg += "----------------------------------------------------------\n";


            foreach (var param in animator.parameters)
            {
                int hash = Animator.StringToHash(param.name);
                validParams.Add(hash);
                msg += "- "
                    + param.name.PadRight(NAME_PADDING)
                    + "\t"
                    + " Hash: " + hash.ToString().PadRight(HASH_PADDING)
                    + "\t"
                    + " Type: " + param.type.ToString().PadRight(TYPE_PADDING)
                    + "\n";
            }


            //Debug.Log($"{name}: Cached {validParams.Count} animator parameters for validation.\n{msg}");
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool IsParamValid(int hash)
        {
            if (validParams == null) BuildAnimatorParamCache();

            if (!validParams.Contains(hash))
            {
                if (!warnedParams.Contains(hash))
                {
                    warnedParams.Add(hash);
                    Debug.LogWarning($"Animator parameter '{hash}' not found in {animator.runtimeAnimatorController.name}");
                }
                return false;
            }
            return true;
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Test All Animator Parameters")]

        public void TestAllAnimatorParameters()
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[Animator Test] Animator reference is missing.");
                return;
            }

            Debug.Log("[Animator Test] Setting all animator parameters for verification...");

            // Check validity of all parameters
            IsParamValid(IsGroundedHash);
            IsParamValid(IsCrouchingHash);
            IsParamValid(IsFlyingHash);
            IsParamValid(IsSwimmingHash);
            IsParamValid(IsDodgingHash);
            IsParamValid(IsRollingHash);
            IsParamValid(IsAttackingHash);
            IsParamValid(CharacterStateHash);

            IsParamValid(VerticalVelocityHash);
            IsParamValid(SpeedHash);
            IsParamValid(TurnAngleHash);

            IsParamValid(IdleStyleHash);
            IsParamValid(AttackStyleHash);
            IsParamValid(Attack2StyleHash);

            IsParamValid(SwitchIdleHash);
            IsParamValid(CrouchTriggerHash);
            IsParamValid(StandTriggerHash);
            IsParamValid(RollTriggerHash);
            IsParamValid(DodgeTriggerHash);
            IsParamValid(JumpTriggerHash);
            IsParamValid(LandTriggerHash);
            IsParamValid(BlockTriggerHash);
            IsParamValid(FireWeaponTriggerHash);
            IsParamValid(AttackTriggerHash);
            IsParamValid(Attack2TriggerHash);

            foreach (var warnedHash in warnedParams)
            {
                Debug.LogWarning($"[Animator Test] Missing parameter hash: {warnedHash}");
            }

            animator.Rebind(); // resets animator to default pose

            // Reset triggers first
            animator.ResetTrigger(SwitchIdleHash);
            animator.ResetTrigger(CrouchTriggerHash);
            animator.ResetTrigger(StandTriggerHash);
            animator.ResetTrigger(RollTriggerHash);
            animator.ResetTrigger(DodgeTriggerHash);
            animator.ResetTrigger(JumpTriggerHash);
            animator.ResetTrigger(LandTriggerHash);
            animator.ResetTrigger(BlockTriggerHash);
            animator.ResetTrigger(FireWeaponTriggerHash);
            animator.ResetTrigger(AttackTriggerHash);
            animator.ResetTrigger(Attack2TriggerHash);


            // Booleans and state
            animator.SetBool(IsGroundedHash, true);
            animator.SetBool(IsCrouchingHash, true);
            animator.SetBool(IsFlyingHash, true);
            animator.SetBool(IsSwimmingHash, true);
            animator.SetBool(IsDodgingHash, true);
            animator.SetBool(IsRollingHash, true);
            animator.SetBool(IsAttackingHash, true);

            animator.SetInteger(CharacterStateHash, 1);

            // Floats for movement blend
            animator.SetFloat(VerticalVelocityHash, 5f);
            animator.SetFloat(SpeedHash, 3.5f);
            animator.SetFloat(TurnAngleHash, 45f);

            // Floats for animation styles
            animator.SetFloat(IdleStyleHash, 1f);
            animator.SetFloat(AttackStyleHash, 2f);
            animator.SetFloat(Attack2StyleHash, 3f);

            // Triggers
            animator.SetTrigger(SwitchIdleHash);
            animator.SetTrigger(CrouchTriggerHash);
            animator.SetTrigger(StandTriggerHash);
            animator.SetTrigger(RollTriggerHash);
            animator.SetTrigger(DodgeTriggerHash);
            animator.SetTrigger(JumpTriggerHash);
            animator.SetTrigger(LandTriggerHash);
            animator.SetTrigger(BlockTriggerHash);
            animator.SetTrigger(FireWeaponTriggerHash);
            animator.SetTrigger(AttackTriggerHash);
            animator.SetTrigger(Attack2TriggerHash);
          
            Debug.Log("[Animator Test] All parameters set successfully, check logs for any issues!");
        }

#endif
        #endregion
    }

}