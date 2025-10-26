using UnityEngine;

public class Mana : Resource
{
    [SerializeField] float regenRate = 5f; // Mana regenerated per second

    void Update()
    {
        if(GetCurrentResource() >= GetMaxResource())
        {
            return;
        }
        
        restoreMana(regenRate * Time.deltaTime);
    }

    public void UseMana(float amount)
    {
        decreaseResource(amount);
        //Debugger.Log($"{transform.root.name} has used {amount} mana");
    }

    public void restoreMana(float amount)
    {
        increaseResource(amount);
        //Debugger.Log($"{transform.root.name} has restored {amount} mana");
    }
}
