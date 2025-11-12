using TMPro;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{
    [SerializeField]float floatSpeed = 1f;
    [SerializeField]float lifetime = 1f;
    [SerializeField]Vector3 floatOffset = new Vector3(0, 1.5f, 0);

    [SerializeField]private GameObject text;
    private TextMeshProUGUI textMesh;
    private Camera mainCamera;
    private float timer;

    void Awake()
    {
        mainCamera = Camera.main;
        timer = lifetime;
    }

    public void Initialize(float damage)
    {

        if (text != null)
        {
            textMesh = text.GetComponent<TextMeshProUGUI>();
        }
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        transform.position += floatOffset;
    }

    void Update()
    {
        // Face camera
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0); // because TMP text faces backward

        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        timer -= Time.deltaTime;
        if (timer <= 0)
            Destroy(gameObject);
        else
            textMesh.alpha = timer / lifetime;
    }
}
