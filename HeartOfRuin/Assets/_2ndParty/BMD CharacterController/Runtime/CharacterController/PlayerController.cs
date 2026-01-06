using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace BMD
{
    public class PlayerController : BMD.CharacterController
    {
        #region Serialized fields
        [Header("Camera Settings")]
        [Range(0.01f, 2f)]
        [SerializeField] float lookSensitivity = 1f;  // Speed of the camera rotation
        [Range(0, 85.0f)]
        [SerializeField] float verticalClamp = 80f; // Maximum vertical angle for camera rotation

        [Header("Camera Follow Settings")]
        [SerializeField] float followDistance = 5f;
        [SerializeField] float followHeight = 2f;
        [Range(-5.0f, 5.0f)]
        [SerializeField] float horizontalOffset = 0f; // Horizontal offset for the camera in third person mode
        [SerializeField] bool isThirdPerson = true; // toggle first/third person
        [SerializeField] float smoothSpeed = 10f;
        [SerializeField] float cameraFollowDamping = 0.05f;

        #endregion

        #region Cached references
        private PlayerControls playerControls;
        private InputAction move;
        private InputAction look;
        private InputAction aimController;
        private InputAction aimMouse;
        private InputAction jump;
        private InputAction roll;
        private InputAction crouch;
        private InputAction sprint;
        private InputAction fire;
        private InputAction attack;
        private InputAction specialAttack;
        #endregion

        #region Runtime variables
        private Vector2 lookInput;
        private float cameraPitch = 0f;

        InputDevice lastDevice;

        // Camera
        private new Camera camera;          // New keyword to hide inherited member, inherited member is depricated anyway.
        private Transform cameraPivot;
        private Transform cameraRoot;
        private Vector3 cameraVelocity;

        // Declared Globally to avoid per-frame allocations
        Vector2 aimDir = Vector2.zero;
        Transform cam;
        Vector3 camRight = Vector3.zero;
        Vector3 camForward = Vector3.zero;
        #endregion
        protected override void Awake()
        {
            base.Awake();
            SetupControls();
            SetupCamera();

        }
        private void SetupCamera()
        {
            camera = GetComponentInChildren<Camera>();
            if (camera == null)
            {
                Debug.LogWarning("No camera found on the player. Please attach a child camera.");
                return;
            }

            // 1. Create and position CameraPivot (yaw control)
            cameraPivot = new GameObject("CameraPivot").transform;
            //cameraPivot.position = transform.position;
            //cameraPivot.rotation = Quaternion.identity;

            // 2. Create and position CameraRoot (pitch control)
            cameraRoot = new GameObject("CameraRoot").transform;
            cameraRoot.SetParent(cameraPivot, false);
            //cameraRoot.localPosition = new Vector3(0f, followHeight, 0f);
            //cameraRoot.localRotation = Quaternion.identity;

            // 3. Reparent and reposition the actual camera
            camera.transform.SetParent(cameraRoot, false);
            //camera.transform.localPosition = new Vector3(horizontalOffset, 0f, -followDistance);
            //camera.transform.localRotation = Quaternion.identity;
        }
        private void SetupControls()
        {
            playerControls = new PlayerControls();
            move = playerControls.Player.Move;
            jump = playerControls.Player.Jump;
            look = playerControls.Player.Look;
            aimController = playerControls.Player.AimController;
            aimMouse = playerControls.Player.AimMouse;
            crouch = playerControls.Player.Crouch;
            roll = playerControls.Player.Roll;
            sprint = playerControls.Player.Sprint;
            fire = playerControls.Player.Fire;
            attack = playerControls.Player.Attack;
            specialAttack = playerControls.Player.SpecialAttack;
        }
        private void OnEnable()
        {
            playerControls.Player.Enable();
            look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            look.canceled += ctx => lookInput = Vector2.zero;
            crouch.performed += ctx => ToggleCrouch();
            roll.performed += ctx => PerformRoll();
            sprint.started += ctx => NotifySprintTriggered(true);
            sprint.canceled += ctx => NotifySprintTriggered(false);

            //InputSystem.onAnyButtonPress.Call(OnAnyButton);
            InputSystem.onActionChange += OnActionChange;
            InputSystem.onEvent += OnInputEvent;
        }
        private void OnDisable()
        {
            playerControls.Player.Disable();

            //InputSystem.onAnyButtonPress.Call(null);
            InputSystem.onActionChange -= OnActionChange;
            
            InputSystem.onEvent -= OnInputEvent;
        }
        protected override void Update()
        {
           
            

            //HandleLook();

            HandleJumpInput();
            HandleAttackInput();
            base.Update();
        }

        

        
        private void UpdateCameraRigFollow()
        {
            if (cameraPivot == null) return;

            Vector3 targetPos = transform.position;
            cameraPivot.position = Vector3.SmoothDamp(
                 cameraPivot.position,
                 transform.position,
                 ref cameraVelocity,
                 cameraFollowDamping
             );
        }
        private void HandleLook()
        {
            Vector2 delta = lookInput * lookSensitivity;

            // Pitch (up/down)
            cameraPitch -= delta.y;
            cameraPitch = Mathf.Clamp(cameraPitch, -verticalClamp, verticalClamp);
            cameraRoot.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

            // Yaw (left/right)
            cameraPivot.Rotate(Vector3.up * delta.x);
        }
        private void HandleJumpInput()
        {
            if (jump.WasPressedThisFrame())
            {
                RequestJump();
            }
        }
        private void HandleAttackInput() 
        {
            if (attack.WasPressedThisFrame())        RequestAttack();
            if (specialAttack.WasPressedThisFrame()) RequestSpecialAttack();
            if (fire.WasPressedThisFrame())          RequestFireWeapon();
        }
        protected override void FixedUpdate()
        {
            Aim();
            SetMoveDirection();

            UpdateCameraRigFollow();
            base.FixedUpdate(); // controller.Tick() and FixedTick() will trigger module updates

        }
        private void SetMoveDirection()
        {
            Vector2 moveInput = move.ReadValue<Vector2>();
            float inputMagnitude = moveInput.magnitude;
            inputMagnitude = Mathf.Pow(inputMagnitude, 1.5f); // smoother start

            // TODO swapped this from camera root while look is frozen for demo.
            //Vector3 moveDir = (cameraRoot.forward * moveInput.y + cameraRoot.right * moveInput.x);
            Vector3 moveDir = (camera.transform.forward * moveInput.y + camera.transform.right * moveInput.x);
            moveDir.y = 0f;
            moveDirection = moveDir.normalized * inputMagnitude;
        }

        private void Aim()
        {
            AimController();
            AimMouse();

            
        }

        private void AimController()
        {
            if (playerControls.bindingMask != InputBinding.MaskByGroup("Gamepad")) return;

            aimDir = aimController.ReadValue<Vector2>();

            cam = camera.transform;

            camRight = cam.right;
            camForward = cam.forward;

            // Flatten to XZ plane
            camRight.y = 0f;
            camForward.y = 0f;

            camRight.Normalize();
            camForward.Normalize();

            aimDirection = camRight * aimDir.x + camForward * aimDir.y;
        }
        private void AimMouse()
        {
            if (playerControls.bindingMask != InputBinding.MaskByGroup("Keyboard&Mouse")) return;

            aimDir = aimMouse.ReadValue<Vector2>();

            aimDir.x /= Screen.width;
            aimDir.y /= Screen.height;

            aimDir = aimDir * 2f - Vector2.one; // Convert from [0,1] to [-1,1]

            cam = camera.transform;

            camRight = cam.right;
            camForward = cam.forward;

            // Flatten to XZ plane
            camRight.y = 0f;
            camForward.y = 0f;

            camRight.Normalize();
            camForward.Normalize();

            aimDirection = camRight * aimDir.x + camForward * aimDir.y;
        }

        protected override void ToggleCrouch()
        {
            if (crouch.WasPressedThisFrame())
            {
                base.ToggleCrouch();
            }
        }
        private void PerformRoll()
        {
            if (roll.WasPressedThisFrame())
            {
                RequestRoll();
            }
        }

        #region Input Switching Helpers

        void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

            if (device is Gamepad)
            {
                var gamepad = (Gamepad)device;

                if (gamepad.rightStick.ReadValue().sqrMagnitude > 0.01f || gamepad.leftStick.ReadValue().sqrMagnitude > 0.01f)
                {
                    OnDeviceChanged(device);
                }
            }
        }

        void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;

            var action = obj as InputAction;
            if (action == null) return;

            if (action.name != "Aim") return;

            var device = action.activeControl?.device;
            if (device == null) return;

            OnDeviceChanged(device);
        }

        void OnAnyButton(InputControl control)
        {
            var device = control.device;

            if (device == lastDevice) return;

            lastDevice = device;
            OnDeviceChanged(device);
        }

        void OnDeviceChanged(InputDevice device)
        {
            if      (device is Gamepad)                     UseGamepad();
            else if (device is Mouse || device is Keyboard) UseKeyboardMouse();
        }
        void UseGamepad()
        {
            playerControls.bindingMask = InputBinding.MaskByGroup("Gamepad");
        }

        void UseKeyboardMouse()
        {
            playerControls.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
        }
        #endregion
    }
}
