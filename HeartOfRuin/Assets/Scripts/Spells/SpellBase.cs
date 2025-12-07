using System;
using UnityEngine;

[Serializable]
public abstract class SpellBase : ScriptableObject, ISpell
{
    [SerializeField] protected string spellName = "spell";
    [SerializeField] protected int manaCost = 1;
    [SerializeField] protected DamageStruct damageScalings;
    public string SpellName => spellName;
    public int ManaCost => manaCost;

    protected SpellContext lastCastContext;

    public abstract void Cast(SpellContext context);

    public abstract void DealDamage(GameObject target);
    

}
