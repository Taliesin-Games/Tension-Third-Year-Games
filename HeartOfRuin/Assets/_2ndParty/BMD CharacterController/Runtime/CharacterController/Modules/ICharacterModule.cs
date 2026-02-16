

using UnityEngine;

public interface ICharacterModule
{
    Component Component { get; }
    
    void PreInitialize(BMD.CharacterController controller);
    void Initialize(BMD.CharacterController controller);
    void Tick(float deltaTime);
    void FixedTick(float fixedDeltaTime);
    void Dispose();
}