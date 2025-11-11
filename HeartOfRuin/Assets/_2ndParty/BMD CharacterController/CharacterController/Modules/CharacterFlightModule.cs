using UnityEngine;

public class CharacterFlightModule : MonoBehaviour, ICharacterModule
{
    private Animator animator;
    private BMD.CharacterController controller;

    public void Initialize(BMD.CharacterController controller)
    {
        this.controller = controller;
        animator = controller.GetComponent<Animator>();
    }

    public void Tick(float deltaTime)
    {

    }
    public void FixedTick(float fixedDeltaTime)
    {

    }
    public void Dispose()
    {

    }
}
