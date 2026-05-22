using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;






public class DPSPanelUIController : MonoBehaviour
{
    [SerializeField] PanelMode mode = PanelMode.Basic;
    PanelMode modeLastUpdate = PanelMode.None;
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;
    
    private List<LabelledNumberUI> currentSlots = new List<LabelledNumberUI>();
    private LabelledNumberUI basicSlot;
    private List<LabelledNumberUI> AdvancedSlots = new List<LabelledNumberUI>();

    DpsTracker dpsTracker;

    private DpsTracker DpsTracker {  
        get 
        { 
            if (dpsTracker == null)
            {
                dpsTracker = Player.Instance?.GetComponent<DpsTracker>();
            }
            return dpsTracker; 
        } 
        set { dpsTracker = value; }
    }

    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<LabelledNumberUI>(true).ToList();

        int SlotsIntendedTotal = 11;

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

        basicSlot = currentSlots[0];
        AdvancedSlots = currentSlots.Skip(1).ToList();
    }

    public void Initialise()
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
        if (DpsTracker == null || gridUI == null)
            return;

        EnsureCorrectSlotCount();

        if (modeLastUpdate != mode)
        {
            if (mode == PanelMode.None)
            {
                Debug.Log("Setting DPS Panel to None mode");
                basicSlot.gameObject.SetActive(false);
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (mode == PanelMode.Basic)
            {
                Debug.Log("Setting DPS Panel to Basic mode");
                basicSlot.gameObject.SetActive(true);
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (mode == PanelMode.Advanced)
            {
                Debug.Log("Setting DPS Panel to Advanced mode");
                basicSlot.gameObject.SetActive(true);
                foreach (LabelledNumberUI slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(true);
                }
            }
        }

        if (mode == PanelMode.Basic)
        {
            basicSlot.SetLabel("DPS");
            basicSlot.SetNumber((int)DpsTracker.GetDPS(0));
        }

        else if (mode == PanelMode.Advanced)
        {
            DamageStruct DPS = DpsTracker.GetDPS(0);

            basicSlot.SetLabel("DPS");
            basicSlot.SetNumber((int)DPS);

            DamageStruct advancedData = DPS;

            // TODO find a better way to predefine these

            AdvancedSlots[0].SetLabel("None");
            AdvancedSlots[0].SetNumber((int)advancedData.None);
            AdvancedSlots[1].SetLabel("Physical");
            AdvancedSlots[1].SetNumber((int)advancedData.Physical);
            AdvancedSlots[2].SetLabel("Magical");
            AdvancedSlots[2].SetNumber((int)advancedData.Magical);
            AdvancedSlots[3].SetLabel("True");
            AdvancedSlots[3].SetNumber((int)advancedData.True);
            AdvancedSlots[4].SetLabel("Fire");
            AdvancedSlots[4].SetNumber((int)advancedData.Fire);
            AdvancedSlots[5].SetLabel("Lightning");
            AdvancedSlots[5].SetNumber((int)advancedData.Lightning);
            AdvancedSlots[6].SetLabel("Ice");
            AdvancedSlots[6].SetNumber((int)advancedData.Ice);
            AdvancedSlots[7].SetLabel("Earth");
            AdvancedSlots[7].SetNumber((int)advancedData.Earth);
            AdvancedSlots[8].SetLabel("Wind");
            AdvancedSlots[8].SetNumber((int)advancedData.Wind);
            AdvancedSlots[9].SetLabel("Water");
            AdvancedSlots[9].SetNumber((int)advancedData.Water);
        }

        modeLastUpdate = mode;
    }
}