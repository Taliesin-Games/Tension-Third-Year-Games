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

    public virtual void Cast(SpellContext context) { }

    public virtual void DealDamage(GameObject target) { }
}
