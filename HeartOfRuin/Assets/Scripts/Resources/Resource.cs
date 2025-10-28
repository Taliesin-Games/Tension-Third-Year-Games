using UnityEngine;
using UnityEngine.Rendering;

public class Resource : MonoBehaviour
{

    [SerializeField] private float maxResource = 100;
    private float currentResource;

    void Start()
    {
        currentResource = maxResource;
    }

    protected float GetMaxResource()
    {
        return maxResource;
    }

    protected float GetCurrentResource()
    {
        return currentResource;
    }

    protected void increaseResource(float amount)
    {
        if (currentResource >= maxResource) return;
        if (amount <= 0) return;
        currentResource = Mathf.Min(currentResource + amount, maxResource);
    }

    protected void decreaseResource(float amount)
    {
        if (currentResource <= 0) return;
        if (amount <= 0) return;
        currentResource = Mathf.Max(currentResource - amount, 0);
    }

}
