using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private DPSPanelUIController DPSPanel;
    [SerializeField] private GameObject HealthBar;
    [SerializeField] private GameObject ManaBar;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is not set in the HUD.");
        }
        if (DPSPanel == null)
        {
            Debug.LogError("DPSPanel reference is not set in the HUD.");
        }

        if (player != null)
        {
            // Initialize health, mana, and tension displays here if needed
            DisplayHealth();
            DisplayMana();
            DisplayTension();
        }

        if (DPSPanel != null)
        {
            DPSPanel.initialise();
        }
    }

    private void Update()
    {
        if (DPSPanel != null)
        {
            DPSPanel.UpdateUI();
        }

        DisplayHealth();
        DisplayMana();
    }

    void DisplayHealth()
    {
        if (HealthBar == null || player == null)
        {
            return;
        }

        HealthBar.GetComponent<Image>().fillAmount = player.GetComponent<Health>().GetCurrentResource() / player.GetComponent<Health>().GetMaxResource();
    }

    void DisplayMana()
    {
        if (ManaBar == null || player == null)
        {
            return;
        }

        ManaBar.GetComponent<Image>().fillAmount = player.GetComponent<Mana>().GetCurrentResource() / player.GetComponent<Mana>().GetMaxResource();
    }

    void DisplayTension()
    {

    }
}
