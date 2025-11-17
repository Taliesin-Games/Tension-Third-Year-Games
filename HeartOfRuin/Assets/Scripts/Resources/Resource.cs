using UnityEngine;
using UnityEngine.Rendering;

public class Resource : MonoBehaviour
{

    [SerializeField] float maxValue = 100;
    float currentValue;


    protected virtual void Start()
    {
        currentValue = maxValue;
    }

    protected float GetMaxResource()
    {
        return maxValue;
    }

    protected float GetCurrentResource()
    {
        return currentValue;
    }

    protected void increaseResource(float amount)
    {
        if (currentValue >= maxValue) return;
        if (amount <= 0) return;
        currentValue = Mathf.Min(currentValue + amount, maxValue);
    }

    protected void decreaseResource(float amount)
    {
        if (currentValue <= 0) return;
        if (amount <= 0) return;
        currentValue = Mathf.Max(currentValue - amount, 0);
    }

}
