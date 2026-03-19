using System;
using TMPro;
using UnityEngine;

public class LabelledNumberUI : MonoBehaviour
{

    public enum LabelledNumberType
    {
        Integer,
        Float,
    }

    [SerializeField] string labelValue = "Label";
    [SerializeField] float numberValue = 0;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI labelText;
    public LabelledNumberType numberType = LabelledNumberType.Integer;

    private void Start()
    {
        UpdateUI();
    }

    public void SetLabel(string newLabel)
    {
        labelValue = newLabel;
        UpdateUI();
    }

    public void SetNumber(float newNumber)
    {
        numberValue = newNumber;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (labelText != null)
        {
            labelText.text = labelValue;
        }
        if (numberText != null)
        {
            if (numberType == LabelledNumberType.Integer)
            {
                numberText.text = ((int)numberValue).ToString();
            }
            else if (numberType == LabelledNumberType.Float)
            {
                numberText.text = Math.Round(numberValue, 2).ToString();
            }
        }
    }
}