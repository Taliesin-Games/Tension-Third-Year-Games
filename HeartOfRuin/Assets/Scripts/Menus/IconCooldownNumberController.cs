using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class IconCooldownNumberController : MonoBehaviour
{
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI numberText;

    [SerializeField] private float cooldownValue = 0;
    [SerializeField] private int numberValue = 0;
    [SerializeField] private Sprite itemSprite;

    public void SetValues(float cooldown, int number, Sprite sprite)
    {
        cooldownValue = cooldown;
        numberValue = number;
        itemSprite = sprite;
        UpdateUI();
    }

    public void SetCooldown(float cooldown)
    {
        cooldownValue = cooldown;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = cooldownValue;
        }
        if (itemImage != null)
        {
            itemImage.sprite = itemSprite;
        }
        if (numberText != null)
        {
            numberText.text = numberValue.ToString();
        }
    }


}
