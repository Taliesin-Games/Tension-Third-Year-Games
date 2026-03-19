using System;
using UnityEngine;

[Serializable]
public abstract class SpellBase : ScriptableObject, ISpell
{
    [SerializeField] protected string spellName = "spell";
    [SerializeField] protected int manaCost = 1;
    [SerializeField] protected DamageStruct damageScalings;
    [SerializeField] protected Sprite icon;
    [SerializeField] protected float cooldown;
    public string SpellName => spellName;
    public int ManaCost => manaCost;

    public Sprite Icon => icon;

    public float Cooldown => cooldown;

    protected SpellContext lastCastContext;

    public abstract void Cast(SpellContext context);

    public abstract void DealDamage(GameObject target);
    

}
