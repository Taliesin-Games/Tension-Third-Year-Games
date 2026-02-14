using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] ItemSlot itemSlot;
    [SerializeField] float pickupLockoutTimer = 1.2f;
    [SerializeField] GameObject pickupEffect;

    // Exposed VFX parameter name and HDR color settings
    [SerializeField] string vfxColorPropertyName = "Main Colour";
    [SerializeField] float pickupColorIntensity = 4f; // >1 makes it HDR

    // Rarity-based color mapping (override defaults as needed in Inspector)
    [Header("Rarity Colors")]
    [SerializeField] Color commonColor = new Color(1f, 1f, 1f);            // White
    [SerializeField] Color uncommonColor = new Color(0.3f, 1f, 0.3f);       // Green
    [SerializeField] Color rareColor = new Color(0.3f, 0.6f, 1f);           // Blue
    [SerializeField] Color epicColor = new Color(0.7f, 0.3f, 0.9f);         // Purple
    [SerializeField] Color legendaryColor = new Color(1f, 0.6f, 0.1f);      // Orange
    [SerializeField] Color cosmicColor = new Color(0.1f, 1f, 1f);           // Cyan

    bool itemSet = false;
    GameObject worldRepresentation;

    public ItemSlot ItemSlot => itemSlot;

    private void Update()
    {
        handleItemPickupLogic();
    }

    void handleItemPickupLogic()
    {
        if (!itemSet && itemSlot.GetItem() != null)
        {
            if (itemSlot.GetItem().GetItemMesh() != null)
            {
                worldRepresentation = Instantiate(itemSlot.GetItem().GetItemMesh(), gameObject.transform);
            }

            // Set VFX color based on item rarity
            VisualEffect vfx = GetComponent<VisualEffect>();
            if (vfx != null && !string.IsNullOrEmpty(vfxColorPropertyName))
            {
                Color rarityBase = GetRarityColor(itemSlot.GetItem().GetRarity());
                Color hdrColor = rarityBase * pickupColorIntensity;

                if (vfx.HasVector4(vfxColorPropertyName))
                {
                    vfx.SetVector4(vfxColorPropertyName, hdrColor);
                }
            }

            itemSet = true;
        }

        if (itemSet)
        {
            pickupLockoutTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || pickupLockoutTimer > 0)
        {
            return;
        }

        Debug.Log($"Player has entered pickup range of {itemSlot.GetItem().GetItemName()}");
        Inventory inventory = other.GetComponent<Inventory>();
        itemSlot = inventory.AddItem(itemSlot);
        if (itemSlot == null)
        {
            Destroy(gameObject);
        }
    }

    Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.common:
                
                return commonColor;
            case ItemRarity.uncommon:
                return uncommonColor;
            case ItemRarity.rare:
                return rareColor;
            case ItemRarity.epic:
                return epicColor;
            case ItemRarity.legendary:
                return legendaryColor;
            case ItemRarity.cosmic:
                return cosmicColor;
            default:
                return commonColor;
        }
    }
}