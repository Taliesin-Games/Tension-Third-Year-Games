using System.Linq.Expressions;
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

    public bool UseMana(float amount)
    {
        if (GetCurrentResource() >= amount)
        {
            DecreaseResource(amount);
            return true;
        }
        return false;
        //Debugger.Log($"{transform.root.name} has used {amount} mana");
    }

    public void restoreMana(float amount)
    {
        IncreaseResource(amount);
        //Debugger.Log($"{transform.root.name} has restored {amount} mana");
    }
}
