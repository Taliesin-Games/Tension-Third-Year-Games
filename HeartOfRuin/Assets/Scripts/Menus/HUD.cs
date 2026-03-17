using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum PanelMode
{
    None,
    Basic,
    Advanced,
}

public class HUD : MonoBehaviour
{
    public static HUD Instance;
    [SerializeField] private Player player;
    [SerializeField] private DPSPanelUIController DPSPanel;
    [SerializeField] private StatPanelUIController StatPanel;
    [SerializeField] private SpellPanelUIController SpellPanel;
    [SerializeField] private GameObject HealthBar;
    [SerializeField] private GameObject ManaBar;

    private Image HealthBarImage;
    private Image ManaBarImage;

    private void Awake()
    {
        Instance = this;
    }
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

        if (StatPanel == null)
        {
            Debug.LogError("StatPanel reference is not set in the HUD.");
        }

        if (SpellPanel == null)
        {
            Debug.LogError("SpellPanel reference is not set in the HUD.");
        }

        if (player != null)
        {
            // Initialize health, mana, and tension displays here if needed
            DisplayTension();
        }

        if (DPSPanel != null)
        {
            DPSPanel.initialise();
        }

        if (StatPanel != null)
        {
            StatPanel.initialise();
        }

        if (SpellPanel != null)
        {
            SpellPanel.initialise();
        }

        if (HealthBar != null)
        {
            HealthBarImage = HealthBar.GetComponent<Image>();
        }

        if (ManaBar != null)
        {
            ManaBarImage = ManaBar.GetComponent<Image>();
        }

    }

    private void Update()
    {
        if (DPSPanel != null)
        {
            DPSPanel.UpdateUI();
        }

        if (StatPanel != null)
        {
            StatPanel.UpdateUI();
        }

        if (SpellPanel != null)
        {
            SpellPanel.updateUI();
        }
    }

    public void UpdateResource(float currentResource, float maxResource, Type resourceType)
    {
        if (resourceType == typeof(Health)) UpdateHealth(currentResource, maxResource);
        else if (resourceType == typeof(Mana)) UpdateMana(currentResource, maxResource);
        else
        {
            Debug.LogError($"Atteampting to update resource of type {resourceType.GetType()} with no subtype method handler exists.");
        }
    }

    // No need to be public now!
    void UpdateHealth(float currentHealth, float maxHealth)
    {
        // Should probably cache the component too but focusing on the player side for now.
        HealthBarImage.fillAmount = currentHealth / maxHealth;
    }
    void UpdateMana(float currentMana, float maxMana)
    {
        ManaBarImage.fillAmount = currentMana / maxMana;
    }

    void DisplayTension()
    {

    }
}
