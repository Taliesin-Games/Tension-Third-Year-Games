using UnityEngine;

/// <summary>
/// Simplified alterative to CharacterMovementModule that only handles character rotation.
/// </summary> 
// TODO move rotation logic from movement module to here
namespace BMD
{
    public class CharacterRotationModule : CharacterModule
    {

        private Animator animator;
        private BMD.CharacterController controller;

        public override void PreInitialize(BMD.CharacterController controller)
        {
            this.controller = controller;
            animator = controller.GetComponent<Animator>();
        }
        public override void Initialize(BMD.CharacterController controller)
        {

        }

        public override void Tick(float deltaTime)
        {
            var c = new CharacterFlightModule();
            c.Initialize(controller);
        }
        public override void FixedTick(float fixedDeltaTime)
        {

        }
        public override void Dispose()
        {

        }
    }
}

