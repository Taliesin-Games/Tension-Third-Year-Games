using System;
using UnityEngine;

public class MaterialPulse : MonoBehaviour
{
    [SerializeField] Renderer rend;

    [Serializable]
    struct MaterialPulseSettings
    {
        [SerializeField] public int materialIndex;
        [SerializeField] public float speed;
        [SerializeField] public float maxAlpha;
        [SerializeField] public float maxEmission;
        [SerializeField] public bool pulseColour;
        [SerializeField] public bool pulseEmission;
    }

    [SerializeField] MaterialPulseSettings[] settings;

    Material[] mats;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        mats = rend.materials;

        // Make sure the emission keyword is on
        foreach (var mat in mats)
        {
            mat.EnableKeyword("_EMISSION");
        }

        for(int i = 0; i < settings.Length; i++)
        {
            var s = settings[i];
            if (mats[s.materialIndex] == null) continue;
            s.maxAlpha = mats[s.materialIndex].color.a;
        }
    }

    void Update()
    {
        foreach (var setting in settings)
        {
            if(mats[setting.materialIndex] ==null) continue;
            PulseMaterial(mats[setting.materialIndex], setting.speed, setting.maxAlpha, setting.maxEmission, setting.pulseColour, setting.pulseEmission);
        }

    }

    private void PulseMaterial(Material material, float speed, float maxAlpha, float maxEmission, bool pulseColour, bool pulseEmission)
    {
        float t = (Mathf.Sin(Time.time * speed) * 0.5f + 0.5f);

        // ---------- Fade Base Color ----------
        if (pulseColour)
        {
            float alpha = t * maxAlpha;
            Color baseColor = material.color;
            baseColor.a = alpha;
            material.color = baseColor;
        }
        else
        {
            material.color= new Color(material.color.r, material.color.g, material.color.b, 0);
        }

        // ---------- Fade Emission ----------
        if (pulseEmission)
        {
            Color emissionBase = Color.green; // Or any glow colour you want
            Color emission = emissionBase * (t * maxEmission);

            material.SetColor(EmissionColorID, emission);
        }
        
    }
}
