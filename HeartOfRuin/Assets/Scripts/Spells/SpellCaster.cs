using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{

    [SerializeField] List<SpellBase> spells;
    [SerializeField] int selectedSpell;
    [SerializeField] Vector3 castPosOffset;

    public void TryCastSpell(int spellIndex)
    {
        if (spellIndex > spells.Count || spellIndex < 0)
        {
            return;
        }

        SpellContext spellContext = new SpellContext();
        spellContext.Caster = gameObject;
        spellContext.Direction = gameObject.transform.forward;
        spellContext.CastOrigin = gameObject.transform.position + (gameObject.transform.forward * castPosOffset.z);

        spells[spellIndex].Cast(spellContext);
    }

}
