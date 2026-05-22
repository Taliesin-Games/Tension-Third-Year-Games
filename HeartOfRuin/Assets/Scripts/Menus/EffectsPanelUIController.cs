using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectsPanelUIController : MonoBehaviour
{
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;

    private List<IconCooldownNumberController> currentSlots;


    List<ItemEffect> itemEffects;
    private List<ItemEffect> ItemEffects
    {
        get
        {
            itemEffects = Player.Instance.GetActiveEffects();
            return itemEffects;
        }
        set { itemEffects = value; }
    }


    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<IconCooldownNumberController>(true).ToList();

        int SlotsIntendedTotal = ItemEffects.Count;

        Debug.Log($"Ensuring correct slot count for effects panel. Intended total: {SlotsIntendedTotal}, current total: {currentSlots.Count}");

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
        EnsureCorrectSlotCount();
    }


    public void updateUI()
    {
        if (ItemEffects == null || gridUI == null)
        {
            return;
        }

        EnsureCorrectSlotCount();

        foreach (ItemEffect effect in ItemEffects)
        {
            int index = ItemEffects.IndexOf(effect);
            //set cooldown to 0 for now, will need to be updated to reflect actual cooldowns
            currentSlots[index].SetValues(0f, 0, effect.EffectIcon);
        }

    }
}
