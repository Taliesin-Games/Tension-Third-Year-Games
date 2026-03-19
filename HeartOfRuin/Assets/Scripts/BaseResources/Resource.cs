using System;
using UnityEngine;
using UnityEngine.Rendering;

public struct ResourceChangeEventArgs
{
    public float Percent;
    public float CurrentValue;
    public float MaxValue;
    public ResourceChangeEventArgs(float percent, float currentValue, float maxValue)
    {
        Percent = percent;
        CurrentValue = currentValue;
        MaxValue = maxValue;
    }
}

public class Resource : MonoBehaviour
{

    [SerializeField] float maxValue = 100;
    float currentValue;

    public event Action<ResourceChangeEventArgs> OnResourceChanged;

    public float CurrentValue
    {
        get { return currentValue; }
        private set
        {
            currentValue = value;
            InvokeResourceChanged();
        }
    }

    protected virtual void Start()
    {
        CurrentValue = maxValue;
    }

    protected float GetMaxResource()
    {
        return maxValue;
    }

    protected float GetCurrentResource()
    {
        return CurrentValue;
    }

    protected void increaseResource(float amount)
    {
        if (CurrentValue >= maxValue) return;
        if (amount <= 0) return;
        CurrentValue = Mathf.Min(CurrentValue + amount, maxValue);
    }

    protected void decreaseResource(float amount)
    {
        if (CurrentValue <= 0) return;
        if (amount <= 0) return;
        CurrentValue = Mathf.Max(CurrentValue - amount, 0);
    }



    protected void InvokeResourceChanged()
    {
        float percent = currentValue/ maxValue;
        OnResourceChanged?.Invoke(new ResourceChangeEventArgs(percent, currentValue, maxValue));
    }
}
