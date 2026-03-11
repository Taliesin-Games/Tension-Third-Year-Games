using TMPro;
using UnityEngine;

public class LabelledNumberUI : MonoBehaviour
{
    [SerializeField] string labelValue = "Label";
    [SerializeField] int numberValue = 0;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI labelText;

    private void Start()
    {
        UpdateUI();
    }

    public void SetLabel(string newLabel)
    {
        labelValue = newLabel;
        UpdateUI();
    }

    public void SetNumber(int newNumber)
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
            numberText.text = numberValue.ToString();
        }
    }
}