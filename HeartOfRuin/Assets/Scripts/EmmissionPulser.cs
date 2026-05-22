using BMD.DataTypes;
using UnityEngine;

public class EmmissionPulser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private int materialID = 0;

    [Header("Emission Settings")]

    [SerializeField] FloatRange intensity = new(0f,5f);

    [SerializeField] private float pulseSpeed = 2f;

    private Material targetMaterial;

    Color emissionColor = Color.white;
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (meshRenderer == null)
        {
            Debug.LogError($"{nameof(EmmissionPulser)} missing MeshRenderer reference.");
            enabled = false;
            return;
        }

        if (materialID < 0 || materialID >= meshRenderer.materials.Length)
        {
            Debug.LogError($"{nameof(EmmissionPulser)} materialID out of range.");
            enabled = false;
            return;
        }

        targetMaterial = meshRenderer.materials[materialID];
        targetMaterial.EnableKeyword("_EMISSION");

        // Set colour based on existing emission colour in the material, so that the pulsing is based on the original colour.
        emissionColor = targetMaterial.GetColor(EmissionColorID);

    }

    private void Update()
    {
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);

        // Clamp minimum at 0 but allow values above 1
        float tension = Mathf.Max(0f, GameManager.Instance.TensionCompletionRatio);

        // Use tension to drive overall brightness
        float targetMaxIntensity = intensity.Max * tension;

        // Keep a tiny baseline glow when low
        float targetMinIntensity = intensity.Min * Mathf.Clamp01(tension);

        float currentIntensity = Mathf.Lerp(targetMinIntensity, targetMaxIntensity, pulse);

        Color finalColor = emissionColor * currentIntensity;

        targetMaterial.SetColor(EmissionColorID, finalColor);
    }
}