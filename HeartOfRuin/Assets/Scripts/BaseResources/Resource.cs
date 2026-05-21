using System;
using UnityEngine;

public struct ResourceChangeEventArgs
{
    public float Percent;
    public float CurrentValue;
    public float OldValue;
    public float MaxValue;
    readonly public float Delta => CurrentValue - OldValue;
    public ResourceChangeEventArgs(float percent, float currentValue, float oldValue, float maxValue)
    {
        Percent = percent;
        CurrentValue = currentValue;
        OldValue = oldValue;
        MaxValue = maxValue;
    }
}

public class Resource : MonoBehaviour
{

    [SerializeField] float maxValue = 100;
    float currentValue;
    float oldValue;

    public float Normalized => currentValue / maxValue;

    public event Action<ResourceChangeEventArgs> OnResourceChanged;

    public float CurrentValue
    {
        get { return currentValue; }
        private set
        {
            oldValue = currentValue;
            currentValue = value;
            InvokeResourceChanged();
        }
    }

    private void Awake()
    {
        character = GetComponent<Character>();
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

    protected void IncreaseResource(float amount)
    {
        if (CurrentValue >= maxValue) return;
        if (amount <= 0) return;
        CurrentValue = Mathf.Min(CurrentValue + amount, maxValue);
    }

    protected void DecreaseResource(float amount)
    {
        if (CurrentValue <= 0) return;
        if (amount <= 0) return;
        CurrentValue = Mathf.Max(CurrentValue - amount, 0);
    }



    protected void InvokeResourceChanged()
    {
        float percent = currentValue/ maxValue;
        OnResourceChanged?.Invoke(new ResourceChangeEventArgs(percent, currentValue, oldValue, maxValue));
    }
}
