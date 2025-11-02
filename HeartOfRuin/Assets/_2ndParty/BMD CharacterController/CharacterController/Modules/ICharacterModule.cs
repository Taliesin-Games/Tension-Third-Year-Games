

public interface ICharacterModule
{

    void Initialize(BMD.CharacterController controller);
    void Tick(float deltaTime);
    void FixedTick(float fixedDeltaTime);
    void Dispose();
}