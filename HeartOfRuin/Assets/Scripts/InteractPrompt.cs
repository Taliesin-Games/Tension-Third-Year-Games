using UnityEngine;

public class InteractPrompt : MonoBehaviour
{
    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    public void Initialize()
    {

        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = Camera.current;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
