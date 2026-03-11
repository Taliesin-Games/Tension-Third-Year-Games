using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum DPSPanelMode
{
    None,
    Basic,
    Advanced,
}




public class DPSPanelUIController : MonoBehaviour
{
    [SerializeField] DPSPanelMode mode = DPSPanelMode.Basic;
    DPSPanelMode modeLastUpdate = DPSPanelMode.None;
    [SerializeField] GameObject gridUI;
    [SerializeField] GameObject UIPrefab;
    [SerializeField] DpsTracker dpsTracker;
    private List<LabelledNumberUI> currentSlots = new List<LabelledNumberUI>();
    private LabelledNumberUI basicSlot;
    private List<LabelledNumberUI> AdvancedSlots = new List<LabelledNumberUI>();

    void EnsureCorrectSlotCount()
    {
        currentSlots = gridUI.GetComponentsInChildren<LabelledNumberUI>(true).ToList();

        if (currentSlots.Count > 11)
        {
            for (int i = 11; i < currentSlots.Count; i++)
            {
                Destroy(currentSlots[i].gameObject);
                currentSlots.RemoveAt(i);
            }
        }

        while (currentSlots.Count < 11)
        {
            GameObject slotObj = Instantiate(UIPrefab, gridUI.transform);
            currentSlots.Add(slotObj.GetComponent<LabelledNumberUI>());
        }

        basicSlot = currentSlots[0];
        AdvancedSlots = currentSlots.Skip(1).ToList();
    }

    public void initialise()
    {
        EnsureCorrectSlotCount();
    }

    public void SetMode(DPSPanelMode newMode)
    {
        mode = newMode;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (dpsTracker == null || gridUI == null)
            return;

        EnsureCorrectSlotCount();

        if (modeLastUpdate != mode)
        {
            if (mode == DPSPanelMode.None)
            {
                Debug.Log("Setting DPS Panel to None mode");
                basicSlot.gameObject.SetActive(false);
                foreach (var slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (mode == DPSPanelMode.Basic)
            {
                Debug.Log("Setting DPS Panel to Basic mode");
                basicSlot.gameObject.SetActive(true);
                foreach (var slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (mode == DPSPanelMode.Advanced)
            {
                Debug.Log("Setting DPS Panel to Advanced mode");
                basicSlot.gameObject.SetActive(true);
                foreach (var slot in AdvancedSlots)
                {
                    slot.gameObject.SetActive(true);
                }
            }
        }

        if (mode == DPSPanelMode.Basic)
        {
            basicSlot.SetLabel("DPS");
            basicSlot.SetNumber((int)dpsTracker.GetDPSCombined());
        }

        else if (mode == DPSPanelMode.Advanced)
        {
            basicSlot.SetLabel("DPS");
            basicSlot.SetNumber((int)dpsTracker.GetDPSCombined());

            var advancedData = dpsTracker.GetDPSByType();

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