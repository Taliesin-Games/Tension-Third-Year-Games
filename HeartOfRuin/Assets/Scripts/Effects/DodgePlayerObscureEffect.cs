using System.Collections;
using UnityEngine;



public class DodgePlayerObscureEffect : MonoBehaviour
{

    [Header("References")]
    [Tooltip("Material used for the obscure object (should support _ObscureAlpha)")]
    [SerializeField] Material obscureMaterial;

    [Header("Obscure Object Settings")]
    [Tooltip("Mesh used for the obscure object")]
    [SerializeField] Mesh objectMesh;
    [Tooltip("Offset of the obscure object from the player position")]
    [SerializeField] Vector3 objectOffset = new Vector3(0f, 0.05f, 0f);
    [Tooltip("Start scale of the obscure object")]
    [SerializeField] Vector3 startScale = new Vector3(0.1f, 0.05f, 0.1f);
    [Tooltip("End scale of the obscure object")]
    [SerializeField] Vector3 endScale = new Vector3(6f, 0.2f, 6f);
    [Tooltip("Duration of each object before it disappears")]
    [SerializeField] float duration = 0.6f; 
    [Tooltip("Curve controlling the fade out of the object over time")]
    [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [Tooltip("Curve controlling the scale of the obscure over time")]
    [SerializeField] AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            triggerEffect();
        }
    }
    

    void triggerEffect()
    {
        Debug.Log("DodgePlayerObscureEffect triggered");
        StartCoroutine(SpawnObscure());
    }

    IEnumerator SpawnObscure()
    {
        float elapsed = 0f;
        Matrix4x4 matrix = Matrix4x4.identity;
        MaterialPropertyBlock props = new MaterialPropertyBlock();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scaleT = scaleCurve.Evaluate(t);
            float fadeT = fadeCurve.Evaluate(t);

            Vector3 currentScale = Vector3.Lerp(startScale, endScale, scaleT);
            float alpha = fadeT;

            props.SetFloat("_Alpha", alpha);

            Vector3 pos = transform.position + objectOffset;
            matrix = Matrix4x4.TRS(pos, Quaternion.identity, currentScale);
            Graphics.DrawMesh(objectMesh, matrix, obscureMaterial, 0, null, 0, props);
            yield return null;
        }
    }
}
