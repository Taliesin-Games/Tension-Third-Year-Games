using UnityEngine;

public class DodgeGhostEffectGPUInstance : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SkinnedMeshRenderer of the character to create ghosts from")]
    [SerializeField] SkinnedMeshRenderer skinnedRenderer;
    [Tooltip("Material used for the ghost (should support _GhostAlpha)")]
    [SerializeField] Material ghostMaterial;

    [Header("Ghost Settings")]
    [Tooltip("Number of ghosts to create during dodge")]
    [SerializeField] int ghostCount = 4;
    [Tooltip("End distance of ghosts from player position")]
    [SerializeField] float radius = 1.5f;
    [Tooltip("Duration of each ghost before it disappears")]
    [SerializeField] float duration = 0.6f; // total lifetime of ghosts
    [Tooltip("Curve controlling the fade out of the ghosts over time")]
    [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Curve controlling the movement of the ghosts over time")]
    [SerializeField] AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Mesh bakedMesh;
    private Matrix4x4[] matrices;
    private MaterialPropertyBlock block;
    private float timer = 0f;
    private bool active = false;

    void Start()
    {
        if (skinnedRenderer == null)
            skinnedRenderer = GetComponent<SkinnedMeshRenderer>();

        bakedMesh = new Mesh();
        matrices = new Matrix4x4[ghostCount];
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        // Optional: Trigger ghost creation with a key press for testing
        if (Input.GetKeyDown(KeyCode.R))
        {
            CreateGhosts();
        }
    }

    void LateUpdate()
    {
        if (!active) return;


        //compute normalized time
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        //compute fade based on curve
        float fade = fadeCurve.Evaluate(t);
        //compute movement based on curve
        float move = movementCurve.Evaluate(t);

        skinnedRenderer.BakeMesh(bakedMesh);

        for (int i = 0; i < ghostCount; i++)
        {
            // Distribute ghosts evenly in a circle around the player
            float angle = (i / (float)ghostCount) * Mathf.PI * 2;
            // Calculate offset position as it moves outward
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius * move;

            // Create transformation matrix for this ghost
            matrices[i] = Matrix4x4.TRS(
                transform.position + offset,
                transform.rotation,
                transform.lossyScale
            );
        }

        // Set fade value in material property block
        block.SetFloat("_GhostAlpha", fade);

        // Draw all ghosts in a single call using GPU instancing
        Graphics.DrawMeshInstanced(
            bakedMesh,
            0,
            ghostMaterial,
            matrices,
            ghostCount,
            block,
            UnityEngine.Rendering.ShadowCastingMode.Off,
            false
        );

        // Deactivate after duration
        if (t >= 1f)
            active = false;
    }

    public void CreateGhosts()
    {
        timer = 0f;
        active = true;
    }
}
