using UnityEngine;
namespace BMD
{
    public abstract class CharacterModule : MonoBehaviour, ICharacterModule
    {
        public Component Component => this;

        public abstract void PreInitialize(CharacterController controller);
        public abstract void Initialize(CharacterController controller);
        public abstract void Tick(float deltaTime);
        public abstract void FixedTick(float fixedDeltaTime);
        public abstract void Dispose();
    }

}
