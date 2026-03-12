using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatPanelUIController : MonoBehaviour
{


    [SerializeField] PanelMode mode = PanelMode.Basic;
    PanelMode modeLastUpdate = PanelMode.None;
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;
    [SerializeField] Character Character;
    DamageStruct BonusDamageStruct;
    [SerializeField] CharacterStats Stats;
    private List<LabelledNumberUI> currentSlots = new List<LabelledNumberUI>();
    private List<LabelledNumberUI> basicSlots = new List<LabelledNumberUI>();
    private List<LabelledNumberUI> AdvancedSlots = new List<LabelledNumberUI>();


    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<LabelledNumberUI>(true).ToList();

        int SlotsIntendedTotal = 15;
        int BasicsIntendedTotal = 5;
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
            currentSlots.Add(slotObj.GetComponent<LabelledNumberUI>());
        }


        basicSlots = currentSlots.Take(BasicsIntendedTotal).ToList();
        AdvancedSlots = currentSlots.Skip(BasicsIntendedTotal).ToList();
    }


    public void initialise()
    {
        EnsureCorrectSlotCount();
    }

    public void SetMode(PanelMode newMode)
    {
        mode = newMode;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Character == null || gridUI == null)
            return;

        BonusDamageStruct = Character.GetCharacterDamageBonusPercentage();

        EnsureCorrectSlotCount();

        if (modeLastUpdate != mode)
        {
            if (mode == PanelMode.None)
            {
                Debug.Log("Setting DPS Panel to None mode");
                foreach (LabelledNumberUI slot in basicSlots)
                {
                    slot.gameObject.SetActive(false);
                }
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);

                }
            }

            if (mode == PanelMode.Basic)
            {
                Debug.Log("Setting DPS Panel to Basic mode");
                foreach (LabelledNumberUI slot in basicSlots)
                {
                    slot.gameObject.SetActive(true);
                }
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (mode == PanelMode.Advanced)
            {
                Debug.Log("Setting DPS Panel to Advanced mode");
                foreach (LabelledNumberUI slot in basicSlots)
                {
                    slot.gameObject.SetActive(true);
                }
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(true);
                    slot.numberType = LabelledNumberUI.LabelledNumberType.Float;
                }
            }
        }

        if (mode == PanelMode.Basic)
        {
            basicSlots[0].SetLabel("STR");
            basicSlots[0].SetNumber(Stats.getStrength());
            basicSlots[1].SetLabel("AGI");
            basicSlots[1].SetNumber(Stats.getAgility());
            basicSlots[2].SetLabel("INT");
            basicSlots[2].SetNumber(Stats.getIntelligence());
            basicSlots[3].SetLabel("Crit Rate");
            basicSlots[3].numberType = LabelledNumberUI.LabelledNumberType.Float;
            basicSlots[3].SetNumber(Stats.getCriticalChance() * 100);
            basicSlots[4].SetLabel("Crit Dmg");
            basicSlots[3].numberType = LabelledNumberUI.LabelledNumberType.Float;
            basicSlots[4].SetNumber(Stats.getCriticalDamage() * 100);
        }

        else if (mode == PanelMode.Advanced)
        {
            basicSlots[0].SetLabel("STR");
            basicSlots[0].SetNumber(Stats.getStrength());
            basicSlots[1].SetLabel("AGI");
            basicSlots[1].SetNumber(Stats.getAgility());
            basicSlots[2].SetLabel("INT");
            basicSlots[2].SetNumber(Stats.getIntelligence());
            basicSlots[3].SetLabel("Crit Rate");
            basicSlots[3].numberType = LabelledNumberUI.LabelledNumberType.Float;
            basicSlots[3].SetNumber(Stats.getCriticalChance() * 100);
            basicSlots[4].SetLabel("Crit Dmg");
            basicSlots[3].numberType = LabelledNumberUI.LabelledNumberType.Float;
            basicSlots[4].SetNumber(Stats.getCriticalDamage() * 100);

            DamageStruct BonusDamageStructMultiplied = BonusDamageStruct * 100;

            AdvancedSlots[0].SetLabel("None");
            AdvancedSlots[0].SetNumber(BonusDamageStructMultiplied.None);
            AdvancedSlots[1].SetLabel("Physical");
            AdvancedSlots[1].SetNumber(BonusDamageStructMultiplied.Physical);
            AdvancedSlots[2].SetLabel("Magical");
            AdvancedSlots[2].SetNumber(BonusDamageStructMultiplied.Magical);
            AdvancedSlots[3].SetLabel("True");
            AdvancedSlots[3].SetNumber(BonusDamageStructMultiplied.True);
            AdvancedSlots[4].SetLabel("Fire");
            AdvancedSlots[4].SetNumber(BonusDamageStructMultiplied.Fire);
            AdvancedSlots[5].SetLabel("Lightning");
            AdvancedSlots[5].SetNumber(BonusDamageStructMultiplied.Lightning);
            AdvancedSlots[6].SetLabel("Ice");
            AdvancedSlots[6].SetNumber(BonusDamageStructMultiplied.Ice);
            AdvancedSlots[7].SetLabel("Earth");
            AdvancedSlots[7].SetNumber(BonusDamageStructMultiplied.Earth);
            AdvancedSlots[8].SetLabel("Wind");
            AdvancedSlots[8].SetNumber(BonusDamageStructMultiplied.Wind);
            AdvancedSlots[9].SetLabel("Water");
            AdvancedSlots[9].SetNumber(BonusDamageStructMultiplied.Water);
        }

        modeLastUpdate = mode;
    }



}