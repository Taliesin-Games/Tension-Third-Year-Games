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

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;
    [SerializeField] private DPSPanelUIController DPSPanel;
    [SerializeField] private StatPanelUIController StatPanel;
    [SerializeField] private SpellPanelUIController SpellPanel;
    [SerializeField] private EffectsPanelUIController EffectsPanel;
    [SerializeField] private GameObject HealthBar;
    [SerializeField] private GameObject ManaBar;

    private Player player;
    private Health health;
    private Mana mana;

    private Image HealthBarImage;
    private Image ManaBarImage;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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

        if (EffectsPanel == null)
        {
            Debug.LogError("EffectsPanel reference is not set in the HUD.");
        }

        if (DPSPanel != null)
        {
            DPSPanel.Initialise();
        }

        if (StatPanel != null)
        {
            StatPanel.Initialise();
        }

        if (SpellPanel != null)
        {
            SpellPanel.Initialise();
        }

        if (EffectsPanel != null)
        {
            EffectsPanel.Initialise();
        }

        if (HealthBar != null)
        {
            HealthBarImage = HealthBar.GetComponent<Image>();
        }

        if (ManaBar != null)
        {
            ManaBarImage = ManaBar.GetComponent<Image>();
        }        

        player = Player.Instance;
        health = player.GetComponent<Health>();
        mana = player.GetComponent<Mana>();

        if (health != null)
        {
            health.OnResourceChanged += UpdateHealth;
        }

        if (mana != null)
        {
            mana.OnResourceChanged += UpdateMana;
        }

        if (player != null)
        {
            player.NotifyStatChange += PlayerStatUpdate;
            PlayerStatUpdate();
        }
        if (player != null)
        {
            // Initialize health, mana, and tension displays here if needed
            DisplayTension();
        }

    }

    private void Update()
    {
        if(Player.Instance == null) 
        {
            return;
        }

        if (DPSPanel != null)
        {
            DPSPanel.UpdateUI();
        }

        if (SpellPanel != null)
        {
            SpellPanel.UpdateUI();
        }

        if (EffectsPanel != null)
        {
            EffectsPanel.updateUI();
        }
    }

    // No need to be public now!
    void UpdateHealth(ResourceChangeEventArgs args)
    {
        // Should probably cache the component too but focusing on the player side for now.
        HealthBarImage.fillAmount = args.CurrentValue / args.MaxValue;
    }
    void UpdateMana(ResourceChangeEventArgs args)
    {
        ManaBarImage.fillAmount = args.CurrentValue / args.MaxValue;
    }


    void PlayerStatUpdate()
    {
        if (StatPanel != null)
        {
            StatPanel.UpdateUI();
        }

    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnResourceChanged -= UpdateHealth;
        }

        if (mana != null)
        {
            mana.OnResourceChanged -= UpdateMana;
        }

        if (player != null)
        {
            player.NotifyStatChange -= PlayerStatUpdate;
        }

    }
    void DisplayTension()
    {

    }
}
