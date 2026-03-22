using TMPro;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{
    [SerializeField]float floatSpeed = 1f;
    [SerializeField]float lifetime = 1f;
    [SerializeField]Vector3 floatOffset = new Vector3(0, 1.5f, 0);

    [SerializeField] TMP_ColorGradient damageToEnemies;
    [SerializeField] TMP_ColorGradient damageToFriendlies;

    [SerializeField]private GameObject text;
    private TextMeshProUGUI textMesh;
    private Camera mainCamera;
    private float timer;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null ) mainCamera = Camera.current; // TODO we need a fallbakc detection for this to search the players camera reference
       
        timer = lifetime;
        Destroy(this.gameObject, lifetime);
    }

    public void Initialize(float damage, bool isDamageOnFriendly)
    {

        if (text != null)
        {
            textMesh = text.GetComponent<TextMeshProUGUI>();
        }
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        transform.position += floatOffset;


        textMesh.colorGradientPreset = isDamageOnFriendly ? damageToFriendlies : damageToEnemies;


    }

    void Update()
    {
        UpdateCameraTrasnform();

        FadeNumber();
    }

    void UpdateCameraTrasnform()
    {
        if (mainCamera != null )
        {
            // Face camera
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180, 0); // because TMP text faces backward
        }
       
        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    void FadeNumber()
    {
        // Fade out
        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0);
        textMesh.alpha = timer / lifetime;
    }
}
