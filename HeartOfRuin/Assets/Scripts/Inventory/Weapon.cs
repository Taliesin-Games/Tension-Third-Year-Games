using UnityEngine;


public class Weapon : ScriptableObject
{
    int Damage = 1;
    int Cooldown = 1;

    void Attack()
    {
        Debug.Log("Attacking with damage: " + Damage);
    }

}
