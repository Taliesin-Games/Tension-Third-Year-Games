using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [SerializeField] private int tileCountX = 1;
    [SerializeField] private int tileCountY = 1;
    [SerializeField] private int tileIndex = 1;
    [SerializeField] private float framesPerSecond = 24f;

    [Header("Transition Control")]
    [Range(0f, 1f)]
    [SerializeField] private float blend = 0f; // 0 = Set A, 1 = Set B

    private static readonly int GlobalColor1Id = Shader.PropertyToID("_GlobalColor1");
    private static readonly int GlobalColor2Id = Shader.PropertyToID("_GlobalColor2");
    private static readonly int GlobalColor3Id = Shader.PropertyToID("_GlobalColor3");
    private static readonly int TileOffsetId  = Shader.PropertyToID("_TileOffset");
    private static readonly int TileScaleId   = Shader.PropertyToID("_TileScale");

    // Cache of last applied blended colors to avoid redundant global updates during play
    private Color _lastBlended1;
    private Color _lastBlended2;
    private Color _lastBlended3;
    private bool _colorsInitialized;

    private void OnEnable()
    {
        _colorsInitialized = false;
        UpdateShaderGlobals(true);
        computeTile(); // ensure initial tile globals are set too
    }

    private void Update()
    {
#if UNITY_EDITOR
        // In edit mode, keep everything live-updating (timeline/preview)
        if (!Application.isPlaying)
        {
            UpdateShaderGlobals(true); // force in editor so inspector changes reflect immediately
            computeTile();
            return;
        }
#endif
        // In play mode, only push color globals if values actually changed
        UpdateShaderGlobals(false);
        computeTile();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Called whenever a serialized field changes in the editor (edit or play)
        UpdateShaderGlobals(true);
    }
#endif

    private void UpdateShaderGlobals(bool force)
    {
        Color blended1 = Color.Lerp(setAColor1, setBColor1, blend);
        Color blended2 = Color.Lerp(setAColor2, setBColor2, blend);
        Color blended3 = Color.Lerp(setAColor3, setBColor3, blend);

        bool changed =
            !_colorsInitialized ||
            !ColorsApproximatelyEqual(_lastBlended1, blended1) ||
            !ColorsApproximatelyEqual(_lastBlended2, blended2) ||
            !ColorsApproximatelyEqual(_lastBlended3, blended3);

        if (force || changed)
        {
            Shader.SetGlobalColor(GlobalColor1Id, blended1);
            Shader.SetGlobalColor(GlobalColor2Id, blended2);
            Shader.SetGlobalColor(GlobalColor3Id, blended3);

            _lastBlended1 = blended1;
            _lastBlended2 = blended2;
            _lastBlended3 = blended3;
            _colorsInitialized = true;
        }
    }

    private static bool ColorsApproximatelyEqual(Color a, Color b, float epsilon = 1e-4f)
    {
        return
            Mathf.Abs(a.r - b.r) <= epsilon &&
            Mathf.Abs(a.g - b.g) <= epsilon &&
            Mathf.Abs(a.b - b.b) <= epsilon &&
            Mathf.Abs(a.a - b.a) <= epsilon;
    }

    private void computeTile()
    {
        int totalTiles = Mathf.Max(1, tileCountX * tileCountY);

        // Animate using editor time in edit mode, runtime time in play
        float time;

        time = Application.isPlaying ? Time.time : (float)EditorApplication.timeSinceStartup;
        time = Time.time;

        tileIndex = Mathf.FloorToInt(time * Mathf.Max(0f, framesPerSecond)) % totalTiles;

        int tileX = tileIndex % Mathf.Max(1, tileCountX);
        int tileY = tileIndex / Mathf.Max(1, tileCountX);

        float sizeX = 1f / Mathf.Max(1, tileCountX);
        float sizeY = 1f / Mathf.Max(1, tileCountY);
        Vector2 tileSize = new Vector2(sizeX, sizeY);

        float offsetX = tileX * sizeX;
        float offsetY = tileY * sizeY;
        Vector2 tileOffset = new Vector2(offsetX, offsetY);

        Shader.SetGlobalVector(TileOffsetId, tileOffset);
        Shader.SetGlobalVector(TileScaleId, tileSize);
    }
}
