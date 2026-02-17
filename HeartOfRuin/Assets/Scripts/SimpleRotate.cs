using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] Vector3 rotationAxis = Vector3.up;
    [SerializeField] float rotationSpeed = 10f;
    void Update()
    {
        transform.Rotate(rotationAxis* rotationSpeed * Time.deltaTime);
    }
}
