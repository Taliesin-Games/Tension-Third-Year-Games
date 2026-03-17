using UnityEngine;
using UnityEngine.UI;

public class HealthBarWorld : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Health health;
    private Camera mainCamera;

    public void Initialize(Health targetHealth)
    {
        // Unsubscribe from previous (important for pooling)
        if (health != null)
        {
            health.OnResourceChanged -= UpdateHealthBar;
        }

        health = targetHealth;

        if (health != null)
        {
            health.OnResourceChanged += UpdateHealthBar;
        }

        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Camera.current;
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            // Face camera
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void UpdateHealthBar(ResourceChangeEventArgs args)
    {
        fillImage.fillAmount = args.Percent;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnResourceChanged -= UpdateHealthBar;
    }


}
