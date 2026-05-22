using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellPanelUIController : MonoBehaviour
{
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;


    private List<IconCooldownNumberController> currentSlots;

    SpellCaster spellCaster;
    private SpellCaster SpellCaster
    {
        get
        {
            if (spellCaster == null)
            {
                spellCaster = Player.Instance?.GetComponent<SpellCaster>();
            }
            return spellCaster;
        }
        set { spellCaster = value; }
    }


    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<IconCooldownNumberController>(true).ToList();

        int SlotsIntendedTotal = SpellCaster.GetSpells().Count();
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

    public void Initialise()
    {
        StartCoroutine(DelayedInitialise());
    }

    IEnumerator DelayedInitialise()
    {
        while(Player.Instance == null) 
        {
            yield return null;
        }

        EnsureCorrectSlotCount();
    }
    public void UpdateUI() 
    {
        if (SpellCaster == null || gridUI == null) 
        {
            return;    
        }

        EnsureCorrectSlotCount();

        foreach (SpellBase spell in SpellCaster.GetSpells()) {
            int index = SpellCaster.GetSpells().IndexOf(spell);
            //set cooldown to 0 for now, will need to be updated to reflect actual cooldowns
            currentSlots[index].SetValues(0f, spell.ManaCost, spell.Icon);
        }

    }

}
