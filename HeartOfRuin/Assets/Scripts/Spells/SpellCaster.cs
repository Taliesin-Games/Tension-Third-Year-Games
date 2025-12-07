using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{
    [SerializeField] List<SpellBase> spells;
    [SerializeField] int selectedSpell;
    [SerializeField] Vector3 castPosOffset;
    [SerializeField] Mana mana;
    [SerializeField] DamageComponent damageComponent;

    public void TryCastSpell()
    {
        TryCastSpell(0);
    }
    public void TryCastSpell(int spellIndex)
    {
        if (spellIndex > (spells.Count -1) || spellIndex < 0 || mana == null)
        {
            return;
        }

        if (mana.UseMana(spells[spellIndex].ManaCost))
        {
            SpellContext spellContext = new SpellContext();
            spellContext.Caster = gameObject;
            spellContext.Direction = gameObject.transform.forward;
            spellContext.CastOrigin = 
                gameObject.transform.position +
                (gameObject.transform.right * castPosOffset.x) + 
                (gameObject.transform.up * castPosOffset.y) +
                (gameObject.transform.forward * castPosOffset.z);
            spellContext.damageComponent = damageComponent;
            spells[spellIndex].Cast(spellContext);
        }
    }
}
