using UnityEngine;

[ExecuteAlways]
public class SkyboxColourManager : MonoBehaviour
{
    [Header("Color Sets")]
    [SerializeField] private Color setAColor1 = Color.white;
    [SerializeField] private Color setAColor2 = Color.gray;
    [SerializeField] private Color setAColor3 = Color.black;

    [SerializeField] private Color setBColor1 = Color.red;
    [SerializeField] private Color setBColor2 = Color.green;
    [SerializeField] private Color setBColor3 = Color.blue;

    [Header("Transition Control")]
    [Range(0f, 1f)]
    [SerializeField] private float blend = 0f; // 0 = Set A, 1 = Set B

    private void OnEnable()
    {
        UpdateShaderGlobals();
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Ensure it runs both in edit mode and play mode
        if (!Application.isPlaying)
        {
            UpdateShaderGlobals();
            return;
        }
#endif
        UpdateShaderGlobals();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Called whenever a serialized field changes in the editor
        UpdateShaderGlobals();
    }
#endif

    private void UpdateShaderGlobals()
    {
        Color blended1 = Color.Lerp(setAColor1, setBColor1, blend);
        Color blended2 = Color.Lerp(setAColor2, setBColor2, blend);
        Color blended3 = Color.Lerp(setAColor3, setBColor3, blend);

        Shader.SetGlobalColor("_GlobalColor1", blended1);
        Shader.SetGlobalColor("_GlobalColor2", blended2);
        Shader.SetGlobalColor("_GlobalColor3", blended3);
    }
}
