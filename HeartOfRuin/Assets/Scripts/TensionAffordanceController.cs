using UnityEngine;
using UnityEngine.Rendering;

public class TensionAffordanceController : MonoBehaviour
{
    [Header("Post Processing Volume")]
    [SerializeField] private Volume tensionVolume;
    [SerializeField] private SkyboxColourManager colourManager;

    [Tooltip("How quickly the post-processing interpolates to the target tension.")]
    [SerializeField] private float smoothingSpeed = 5f;

    private void Start()
    {
        if (tensionVolume && GameManager.Instance && colourManager)
        {
            // Ensure we start at the correct visual state
            tensionVolume.weight = GameManager.Instance.TensionCompletionRatio;
            colourManager.SetBlend(GameManager.Instance.TensionCompletionRatio);
        }
        else
        {
            Debug.LogWarning("TensionAffordanceController: No Volume assigned.");
        }
    }

    private void Update()
    {
        if (tensionVolume != null && GameManager.Instance && colourManager)
        {
            // Smoothly adjust the overall weight of the tension volume profile over time
            tensionVolume.weight = Mathf.Lerp(tensionVolume.weight, GameManager.Instance.TensionCompletionRatio, Time.deltaTime * smoothingSpeed);
            colourManager.SetBlend(GameManager.Instance.TensionCompletionRatio);
        }
    }

}
