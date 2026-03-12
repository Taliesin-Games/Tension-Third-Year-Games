using System;
using UnityEditor;
using UnityEngine;
using static ColourPalette;

public class ColourCreator : MonoBehaviour
{
    const int MONOCHROME_COLOURS = 3;
    const int BASE_COLOURS = 6;

    // ---- Menu Item ----
    [MenuItem("Tools/Colour Creator/Print Albedo Colour")]
    [MenuItem("Assets/Create/Colour Creator/Print Albedo Colour", false, 2000)]
    private static void PrintAlbedoColour()
    {
        var mat = Selection.activeObject as Material;
        if (mat == null) return;

        // Standard / URP / HDRP safe lookup
        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            Debug.Log($"[{mat.name}] BaseColor = {c}");
        }
        else if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");
            Debug.Log($"[{mat.name}] Color = {c}");
        }
        else
        {
            Debug.LogWarning($"[{mat.name}] No albedo color property found.");
        }
    }

    // ---- Validation (enables / disables menu) ----
    [MenuItem("Tools/Colour Creator/Print Albedo Colour", true)]
    [MenuItem("Assets/Create/Colour Creator/Print Albedo Colour", true)]
    private static bool PrintAlbedoColour_Validate()
    {
        return Selection.activeObject is Material;
    }

    [MenuItem("Tools/Colour Creator/3 Colours", false, 2000)]
    [MenuItem("Assets/Create/Colour Creator/3 Colours", false, 2000)]
    static void Generate3() => GenerateVariants(3);

    [MenuItem("Tools/Colour Creator/6 Colours", false, 2001)]
    [MenuItem("Assets/Create/Colour Creator/6 Colours", false, 2001)]
    static void Generate6() => GenerateVariants(6);

    [MenuItem("Tools/Colour Creator/9 Colours", false, 2002)]
    [MenuItem("Assets/Create/Colour Creator/9 Colours", false, 2002)]
    static void Generate9() => GenerateVariants(9);

    [MenuItem("Tools/Colour Creator/12 Colours", false, 2003)]
    [MenuItem("Assets/Create/Colour Creator/12 Colours", false, 2003)]
    static void Generate12() => GenerateVariants(12);

    [MenuItem("Tools/Colour Creator/17 Colours", false, 2004)]
    [MenuItem("Assets/Create/Colour Creator/17 Colours", false, 2004)]
    static void Generate17() => GenerateVariants(17);

    [MenuItem("Tools/Colour Creator/All Colours", false, 2100)]
    [MenuItem("Assets/Create/Colour Creator/All Colours", false, 2100)]
    static void GenerateAll() => GenerateVariants(int.MaxValue);

    [MenuItem("Tools/Colour Creator/3 Colours", true, 2000)]
    [MenuItem("Assets/Create/Colour Creator/3 Colours", true, 2000)]
    [MenuItem("Tools/Colour Creator/6 Colours", true, 2001)]
    [MenuItem("Assets/Create/Colour Creator/6 Colours", true, 2001)]
    [MenuItem("Tools/Colour Creator/9 Colours", true, 2002)]
    [MenuItem("Assets/Create/Colour Creator/9 Colours", true, 2002)]
    [MenuItem("Tools/Colour Creator/12 Colours", true, 2003)]
    [MenuItem("Assets/Create/Colour Creator/12 Colours", true, 2003)]
    [MenuItem("Tools/Colour Creator/17 Colours", true, 2004)]
    [MenuItem("Assets/Create/Colour Creator/17 Colours", true, 2004)]
    [MenuItem("Tools/Colour Creator/All Colours", true, 2100)]
    [MenuItem("Assets/Create/Colour Creator/All Colours", true, 2100)]
    private static bool Menu_Validate()
    {
        return Selection.activeObject is Material;
    }

    static void GenerateVariants(int count)
    {
        var mat = Selection.activeObject as Material;
        if (mat == null) return;

        int max = Mathf.Min(count, Enum.GetValues(typeof(ColourIds)).Length);

        for (int i = 0; i < max; i++)
        {
            GenerateVariant(i, mat);
        }

        AssetDatabase.Refresh();
    }

    static void GenerateVariant(int i, Material sourceMat)
    {
        ColourIds cID = (ColourIds)i;
        if (!Enum.IsDefined(typeof(ColourIds), cID)) return;

        Color colour = ColourPalette.GetColour(cID);

        // Get source asset path
        string sourcePath = AssetDatabase.GetAssetPath(sourceMat);
        string directory = System.IO.Path.GetDirectoryName(sourcePath);
        string extension = System.IO.Path.GetExtension(sourcePath);

        string variantName = $"{sourceMat.name} {cID}";
        string newPath = System.IO.Path.Combine(directory, variantName + extension);

        if (AssetDatabase.LoadAssetAtPath<Material>(newPath) != null) return;

        // THIS is the material variant
        Material variant = new Material(sourceMat);
        variant.parent = sourceMat;

        // Register undo
        Undo.RegisterCreatedObjectUndo(variant, "Create Material Variant");

        // Set overridden colour only
        if (variant.HasProperty("_BaseColor"))
        {
            colour.a = variant.GetColor("_BaseColor").a;
            variant.SetColor("_BaseColor", colour);
        }
        else if (variant.HasProperty("_Color"))
        {
            colour.a = variant.GetColor("_Color").a;
            variant.SetColor("_Color", colour);
        }


        // ---- Emission handling ----
        if (variant.HasProperty("_EmissionColor"))
        {
            bool emissionEnabled = variant.IsKeywordEnabled("_EMISSION");

            if (emissionEnabled)
            {
                Color emission = variant.GetColor("_EmissionColor");

                // Preserve original emission intensity
                float intensity = emission.maxColorComponent;
                if (intensity <= 0f) intensity = 1f;

                Color emissiveColour = colour * intensity;

                variant.SetColor("_EmissionColor", emissiveColour);

                // Ensure keyword stays enabled
                variant.EnableKeyword("_EMISSION");
            }
        }

        // Save as asset
        AssetDatabase.CreateAsset(variant, newPath);
        AssetDatabase.SaveAssets();
    }
}
