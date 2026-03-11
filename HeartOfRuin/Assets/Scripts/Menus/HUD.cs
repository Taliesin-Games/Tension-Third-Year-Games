using Unity.VisualScripting;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private DPSPanelUIController DPSPanel;

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
    }

    void DisplayHealth()
    {

    }

    void DisplayMana()
    {

    }

    void DisplayTension()
    {

    }
}
