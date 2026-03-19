using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TensionManager : MonoBehaviour
{
    [SerializeField] SkyboxColourManager skyboxController;
    [SerializeField] Volume volume;
    Vignette vignette;

    private void Start()
    {
        if (skyboxController == null)
        {
            Debug.LogError("SkyboxColourManager reference is not set in the TensionManager.");
        }

        if (volume == null)
        {
            Debug.LogError("Volume reference is not set in the TensionManager.");
            return;
        }
        if (!volume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.LogError("Vignette effect is not found in the Volume profile.");
            return;
        }
    }

    public void Update()
    {

        float tensionPercentage = GameManager.Instance.TensionCompletionRatio;

        // Update skybox color based on tension
        if (skyboxController != null)
        {
            skyboxController.SetBlend(tensionPercentage);
        }
        // Update vignette intensity based on tension
        if (vignette != null)
        {
            vignette.intensity.value = tensionPercentage; // Adjust the max intensity as needed
        }
    }
}
