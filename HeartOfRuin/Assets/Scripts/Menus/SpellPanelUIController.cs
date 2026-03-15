using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellPanelUIController : MonoBehaviour
{

    [SerializeField] SpellCaster spellCaster;
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;


    private List<IconCooldownNumberController> currentSlots;

    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<IconCooldownNumberController>(true).ToList();

        int SlotsIntendedTotal = spellCaster.GetSpells().Count();
        if (currentSlots.Count > SlotsIntendedTotal)
        {
            for (int i = SlotsIntendedTotal; i < currentSlots.Count; i++)
            {
                Destroy(currentSlots[i].gameObject);
                currentSlots.RemoveAt(i);
            }
        }

        while (currentSlots.Count < SlotsIntendedTotal)
        {
            GameObject slotObj = Instantiate(UIPrefab, gridUI.transform);
            currentSlots.Add(slotObj.GetComponent<IconCooldownNumberController>());
        }

    }

    public void initialise()
    {
        EnsureCorrectSlotCount();
    }

    public void updateUI() 
    {
        if (spellCaster == null || gridUI == null) 
        {
            return;    
        }

        EnsureCorrectSlotCount();

        foreach (SpellBase spell in spellCaster.GetSpells()) {
            int index = spellCaster.GetSpells().IndexOf(spell);
            //set cooldown to 0 for now, will need to be updated to reflect actual cooldowns
            currentSlots[index].SetValues(0f, spell.ManaCost, spell.Icon);
        }

    }

}
