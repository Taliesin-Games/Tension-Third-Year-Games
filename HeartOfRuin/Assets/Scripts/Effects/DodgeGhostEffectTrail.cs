using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeGhostEffectTrail : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SkinnedMeshRenderer of the character to create ghosts from")]
    [SerializeField] SkinnedMeshRenderer skinnedRenderer;
    [Tooltip("Material used for the ghost (should support _GhostAlpha)")]
    [SerializeField] Material ghostMaterial;

    [Header("Ghost Settings")]
    [Tooltip("Number of ghosts to create during dodge")]
    [SerializeField] int ghostCount = 4;
    [SerializeField] float spawnInterval = 0.05f;
    [Tooltip("Duration of each ghost before it disappears")]
    [SerializeField] float duration = 0.6f; // total lifetime of ghosts
    [Tooltip("Curve controlling the fade out of the ghosts over time")]
    [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Optional scale over lifetime (for stretching/expanding ghost trails)")]
    [SerializeField] AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1, 1);


    class GhostInstance
    {
        public Matrix4x4 matrix;
        public float lifetime;
        public float timeAlive;
    }

    private Mesh bakedMesh;
    private List<GhostInstance> ghosts = new();
    private MaterialPropertyBlock block;
    private bool spawning;

    void Start()
    {
        if (skinnedRenderer == null)
            skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        bakedMesh = new Mesh();
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(GhostRoutine());

        UpdateGhosts();
        DrawGhosts();
    }

    IEnumerator GhostRoutine()
    {
        spawning = true;
        for (int i = 0; i < ghostCount; i++)
        {
            CreateGhost();
            yield return new WaitForSeconds(spawnInterval);
        }
        spawning = false;
    }

    void CreateGhost()
    {
        if (skinnedRenderer == null || ghostMaterial == null)
            return;

        skinnedRenderer.BakeMesh(bakedMesh);

        GhostInstance g = new GhostInstance
        {
            matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale),
            lifetime = duration,
            timeAlive = 0f
        };
        ghosts.Add(g);
    }

    void UpdateGhosts()
    {
        for (int i = ghosts.Count - 1; i >= 0; i--)
        {
            var g = ghosts[i];
            g.timeAlive += Time.deltaTime;

            float t = g.timeAlive / g.lifetime;
            if (t >= 1f)
            {
                ghosts.RemoveAt(i);
                continue;
            }

            // Apply optional scale change over time
            float scale = scaleCurve.Evaluate(t);
            Vector3 s = transform.lossyScale * scale;
            g.matrix = Matrix4x4.TRS(
                g.matrix.GetColumn(3), // keep original world position
                transform.rotation,    // keep player’s current rotation (or store original if you want to freeze it too)
                s
            );

            ghosts[i] = g;
        }
    }

    void DrawGhosts()
    {
        if (ghosts.Count == 0 || ghostMaterial == null)
            return;

        Matrix4x4[] matrices = new Matrix4x4[Mathf.Min(1023, ghosts.Count)];
        float[] alphas = new float[matrices.Length];

        for (int i = 0; i < ghosts.Count; i++)
        {
            var g = ghosts[i];
            float t = g.timeAlive / g.lifetime;
            matrices[i] = g.matrix;
            alphas[i] = fadeCurve.Evaluate(t);
        }

        for (int i = 0; i < ghosts.Count; i += 1023)
        {
            int count = Mathf.Min(1023, ghosts.Count - i);
            block.SetFloat("_GhostAlpha", 1f); // default per-material control
            Graphics.DrawMeshInstanced(bakedMesh, 0, ghostMaterial, matrices, count, block,
                UnityEngine.Rendering.ShadowCastingMode.Off, false);
        }
    }
}
