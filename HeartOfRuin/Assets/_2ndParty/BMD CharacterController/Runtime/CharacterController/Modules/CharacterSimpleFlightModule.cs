using UnityEngine;

namespace BMD
{


    public class CharacterSimpleFlightModule : CharacterModule
    {
        #region Configuration
        [Header("Flight Settings")]
        [SerializeField] float flySpeed = 10.0f;
        #endregion

        #region Cached References
        CharacterController controller;

        private UnityEngine.CharacterController unityController;
        #endregion
        public override void PreInitialize(BMD.CharacterController controller)
        {
            this.controller = controller;
            unityController = controller.GetComponent<UnityEngine.CharacterController>();
        }
        
        public override void Initialize(BMD.CharacterController controller)
        {

        }

        public override void Tick(float deltaTime)
        {
        
        }
        public override void FixedTick(float fixedDeltaTime)
        {
            ApplyMovement(fixedDeltaTime);
        }
        public override void Dispose()
        {

        }

        private void ApplyMovement(float dt)
        {
            unityController.Move(controller.MoveDirection * dt * flySpeed);
        }
        
    }
}