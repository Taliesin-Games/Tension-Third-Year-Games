using System;
using UnityEngine;

[Serializable]
public abstract class SpellBase : ScriptableObject, ISpell
{
    [SerializeField] protected string spellName = "spell";
    [SerializeField] protected int manaCost = 1;
    [SerializeField] DamageStruct damageScalings;
    public string SpellName => spellName;
    public int ManaCost => manaCost;

    public virtual void Cast(SpellContext context) { }
}
