using UnityEngine;

public class RelativeFluctuations : MonoBehaviour
{
    [Tooltip("The base scale of the object. If left at (0,0,0), it will default to the object's scale on Start.")]
    [SerializeField] private Vector3 baseScale;

    [Tooltip("How fast the scale fluctuates.")]
    [SerializeField] private float frequency = 1f;

    [Tooltip("How much the scale fluctuates relative to the base scale.")]
    [SerializeField] private float amplitude = 0.2f;

    [Tooltip("If true, the object will fluctuate in size. If false, it will remain at the base scale.")]
    [SerializeField] private bool fluctuate = false;

    private float randomOffset;


    void Start()
    {
        // If no base scale was set in the inspector, capture the object's current scale
        if (baseScale == Vector3.zero)
        {
            baseScale = transform.localScale;
        }

        // Give each object a random noise offset so they don't all fluctuate in perfect sync
        randomOffset = Random.Range(0f, 1000f);
    }

    public void SetBaseScale(Vector3 newBaseScale)
    {
        baseScale = newBaseScale;
    }

    public void EnableFluctuation()
    {
        fluctuate = true;
    }

    public void DisableFluctuation()
    {
        fluctuate = false;
        transform.localScale = baseScale; // Reset to base scale when disabling
    }

    void Update()
    {
        // Generate a smooth random value between -1 and 1
        float noise = Mathf.PerlinNoise((Time.time * frequency) + randomOffset, 0f);
        float mappedNoise = (noise - 0.5f) * 2f; // Remaps from [0, 1] to [-1, 1]

        // Calculate the uniform multiplier based on the amplitude
        float scaleMultiplier = 1f + (mappedNoise * amplitude);

        // Apply the new scale evenly across all three axes
        transform.localScale = baseScale * scaleMultiplier;
    }
}
