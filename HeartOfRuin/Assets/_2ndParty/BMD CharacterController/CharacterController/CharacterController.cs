using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BMD
{
    [RequireComponent(typeof(UnityEngine.CharacterController))] // Ensure that a CharacterController component is attached
    public abstract class CharacterController : MonoBehaviour
    {
        private readonly Dictionary<Type, ICharacterModule> modules = new();

        #region Actions
        public event Action<CharacterState> OnStateChanged;
        public event Action<Vector3> OnMoveDirectionChanged;
        public event Action OnJumpRequested;    // Event fdired attempting to jump
        public event Action OnJumpPerformed;    // Event fired when jump is performed
        public event Action OnLanded;           // Evenet fires when character lands

        public event Action OnSprintDown;
        public event Action OnSprintUp;

        public event Action OnRollRequested;    // Event fired attempting to roll
        public event Action OnRollPerformed;    // Event fired when roll is performed
        public event Action OnRollEnded;        // Event fired when roll ends

        public event Action OnDodgeRequested;   // Event fired attempting to dodge
        public event Action OnDodgePerformed;    // Event fired when dodge is performed
        public event Action OnDodgeEnded;        // Event fired when dodge ends

        #endregion

        #region Constants
        protected const float IDLE_VARIATION_INTERVAL = 2f; // Interval for idle animation variation
        protected const float IDLE_BLEND_SPEED = 0.5f; // Higher = faster blending
        #endregion

        #region Serialized fields
        [Tooltip("Speed settings for various character rotation")]
        [SerializeField] protected float crouchSpeed = 2.5f;    // Speed of the character when crouching
        [SerializeField] protected float crawlSpeed = 1f;       // Speed of the character when crawling
        [SerializeField] protected float pushSpeed = 3f;        // Speed of the character when pushing objects
        [SerializeField] protected float pullSpeed = 3f;        // Speed of the character when pulling objects
        [SerializeField] protected float climbSpeed = 3f;       // Speed of the character when climbing
        [SerializeField] protected float swimSpeed = 4f;        // Speed of the character when swimming
        [SerializeField] protected float swingSpeed = 8f;       // Speed of the character when swinging
        [SerializeField] protected float flySpeed = 12f;        // Speed of the character when flying
        #endregion
    


        #region Cached references
        protected Vector3 gravity = UnityEngine.Physics.gravity; // Gravity vector for the character
        protected UnityEngine.CharacterController unityController; // Reference to the CharacterController component    
        protected Animator animator;
        #endregion

        #region Runtime variables
        protected Vector3 moveDirection = Vector3.zero; // Current movement direction of the character
        public Vector3 MoveDirection => moveDirection;

        protected CharacterState currentState = CharacterState.Idle;
        private Coroutine idleLoopCoroutine;    // Coroutine for handling idle loop animations
        private Coroutine rollCoroutine;        // Coroutine for handling rolling movement

        private float currentIdleBlend = 0f;
        private float targetIdleBlend = 0f;

        #endregion

        #region Properties
        public CharacterState CurrentState 
        {
            get { return currentState; }
            set { currentState = value; }
        }
        /// <summary>
        /// Gets the locomotion scales for walking, running, and sprinting speeds as a tuple.
        /// </summary>
        public (float walk, float run, float sprint) LocomotionScales
        {
            // Returns walks, run and sprint speed from the movement module if exits, if not returns a default scale.
            get
            {
                if (TryGetModule(out CharacterMovementModule module)) return module.LocomotionScales;

                return (walk: 1f, run: 2f, sprint: 3f);
            }
        }
        public float TurnAngle
        {
            get
            {
                // Lightweight fetch calculater turn angle if we have a move module
                if (TryGetModule(out CharacterMovementModule module)) return module.TurnAngle;

                // Expensive (relatively) calculate it based on other factors.
                Vector3 velocity = unityController.velocity;

                // Flatten forward and velocity vectors
                Vector3 flatForward = unityController.transform.forward;
                Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);

                // Avoid NaNs if velocity is nearly zero
                if (flatVelocity.sqrMagnitude > 0.0001f)
                {
                    flatForward.Normalize();
                    flatVelocity.Normalize();

                    float turnAngle = Vector3.SignedAngle(flatForward, flatVelocity, Vector3.up);
                    return turnAngle;
                }

                return 0f;
            }
        }
        #endregion

        #region Signal Helpers
        // --- Signal helpers (so modules can’t fire events directly) ---
        public void NotifyStateChanged(CharacterState state) => OnStateChanged?.Invoke(state);

        // Jump signal helpers
        public void RequestJump() => OnJumpRequested?.Invoke();
        public void NotifyJumpPerformed() => OnJumpPerformed?.Invoke();
        public void NotifyJumpLanded() => OnLanded?.Invoke();

        // Roll signal helpers
        public void RequestRoll() => OnRollRequested?.Invoke();
        public void NotifyRollPerformed() => OnRollPerformed?.Invoke();
        public void NotifyRollEnded() => OnRollEnded?.Invoke();

        //Dodge signal helpers
        public void RequestDodge() => OnDodgeRequested?.Invoke();
        public void NotifyDodgePerformed() => OnDodgePerformed?.Invoke();
        public void NotifyDodgeEnded() => OnDodgeEnded?.Invoke();


        protected void NotifySprintTriggered(bool triggered) 
        {
            if (triggered)
            {
                OnSprintDown?.Invoke();
            }
            else
            {
                OnSprintUp?.Invoke();
            }
        }
        #endregion

        protected virtual void Awake()
        {
            unityController = GetComponent<UnityEngine.CharacterController>();
            animator = GetComponent<Animator>();

            foreach (var module in GetComponents<ICharacterModule>())
            {
                RegisterModule(module);
                module.Initialize(this);
            }
        }
        protected virtual void Start()
        {
            if (unityController == null)
            {
                Debug.LogError("CharacterController component is missing on " + gameObject.name);
            }
            
        }
        protected virtual void Update()
        {
            foreach (var (_, module) in modules)
                module.Tick(Time.deltaTime);
        }
        protected virtual void FixedUpdate()
        {
            // PlayerController sets MoveDirection; movement happens inside modules.
            foreach (var (_, module) in modules)
                module.FixedTick(Time.fixedDeltaTime);

        }

#if UNITY_EDITOR
        [ContextMenu("Add Default Modules")]
        private void AddDefaultModules()
        {
            if (!GetComponent<CharacterMovementModule>())
            {
                gameObject.AddComponent<CharacterMovementModule>();
                Debug.Log("Added default CharacterMovementModule.");
            }
            EditorUtility.SetDirty(this);
        }
#endif

        [ExecuteAlways]
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (GetComponents<ICharacterModule>().Length == 0)
                {
                    gameObject.AddComponent<CharacterMovementModule>();
                    Debug.Log("Auto-added default CharacterMovementModule on new controller.");
                }
            }
#endif
        }
        public void OnIdleLoopComplete()
        {
            float chance = UnityEngine.Random.value; // 0.0 to 1.0
            if (chance < 0.3f) // 30% chance
            {
                animator.SetTrigger("SwitchIdle");

                if (idleLoopCoroutine == null)
                {
                    idleLoopCoroutine = StartCoroutine(IdleLoop());
                }



            }
        }
        protected virtual IEnumerator IdleLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(IDLE_VARIATION_INTERVAL);

                targetIdleBlend = UnityEngine.Random.value; // pick a new idle style
            }
        }
        protected virtual void ToggleCrouch()
        {
            Debug.Log("ToggleCrouch called, but not implemented in base class.");
        }

        private void OnDestroy()
        {
            foreach (var (_,module) in modules)
            {
                module.Dispose();
            }

            modules.Clear();
        }
        public void RegisterModule<T>(T module) where T : ICharacterModule => RegisterModule((ICharacterModule)module);

        public void RegisterModule(ICharacterModule module)
        {
            var type = module.GetType(); // concrete type, e.g., CharacterMovementModule

            if (modules.TryGetValue(type, out var existing))
            {
                Debug.LogError(
                    $"[CharacterController] Duplicate module registration attempted: {type.Name}.\n" +
                    $"Existing: {existing.GetType().Name}, New: {module.GetType().Name}", this);
                return;
            }

            modules[type] = module;
        }

        public bool TryGetModule<T>(out T module) where T : class, ICharacterModule
        {
            if (modules.TryGetValue(typeof(T), out var m))
            {
                module = m as T;
                return true;
            }
            module = null;
            return false;
        }
    }
}
