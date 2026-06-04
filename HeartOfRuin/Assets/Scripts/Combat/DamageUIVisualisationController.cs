using UnityEngine;



public class DamageUIVisualisationController : MonoBehaviour
{
    public static DamageUIVisualisationController Instance;

    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private bool showDamageNumbers;
    [SerializeField] private bool showEnemyHealthBars;
    [SerializeField] private float healthbarHeightOffset = 2;


    Player player;

    public void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        player = Player.Instance;
    }

    public void VisualiseDamage(float damageAmount, GameObject damageTarget, Health targetHealth)
    {
        if (damageTarget == null)
        {
            return;
        }

        Debug.Log($"Visualising damage: {damageAmount} on target: {damageTarget.name}");

        ShowDamageNumbers(damageAmount, damageTarget);

        ShowHealthBars(damageTarget, targetHealth);

    }


    void ShowDamageNumbers(float damageAmount, GameObject damageTarget)
    {
        //Handle Damage Numbers

        if (damageTarget == null || ! showDamageNumbers)
        {
            return; 

        }

        if (player == null)
        {
            player = Player.Instance;
        }

        GameObject instance = Instantiate(damageNumberPrefab, damageTarget.transform.position, Quaternion.identity);



        if (instance.TryGetComponent<DamageNumbers>(out DamageNumbers DN))
        {        
            Debug.Log("Damage number instantiated at position: " + instance.transform.position);
            DN.Initialize(damageAmount, damageTarget == player.gameObject);
        }
        else
        {
            Debug.LogError("DamageNumberPrefab does not have a DamageNumbers component");
        }
        
    }

    private float GetTargetMaxYBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return target.transform.position.y;
        }

        Bounds combinedBounds = renderers[0].bounds;

        foreach (Renderer rend in renderers)
        {

            if (rend is ParticleSystemRenderer) continue;

            combinedBounds.Encapsulate(rend.bounds);
        }

        return combinedBounds.max.y;
    }

    void ShowHealthBars(GameObject damageTarget, Health targetHealth)
    {
        if (healthBarPrefab == null || !showEnemyHealthBars)
        {
            return;
        }

        if (damageTarget == player.gameObject)
        {
            return; // Don't show health bars for the player
        }

        if (damageTarget.GetComponentInChildren<HealthBarWorld>() != null)
        {
            return; // Health bar already exists for this target
        }

        Vector3 pos = damageTarget.transform.position + (Vector3.up * (healthbarHeightOffset + (GetTargetMaxYBounds(damageTarget)/2)));

        GameObject healthBarInstance = Instantiate(healthBarPrefab, pos, Quaternion.identity);
        HealthBarWorld healthBar = healthBarInstance.GetComponent<HealthBarWorld>();

        if (healthBar != null)
        {
            healthBar.Initialize(targetHealth);
            healthBarInstance.transform.SetParent(damageTarget.transform);
            healthBarInstance.transform.localPosition = (Vector3.up * (healthbarHeightOffset + (GetTargetMaxYBounds(damageTarget) / 2)));
            healthBarInstance.transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("HealthBarPrefab does not have a HealthBarWorld component");
        }
    }

}
