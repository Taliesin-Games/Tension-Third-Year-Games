using UnityEngine;
using UnityEngine.Rendering;

public class Resource : MonoBehaviour
{

    [SerializeField] float maxValue = 100;
    float currentValue;

    // Cahced reference
    HUD hud;
    public float CurrentValue
    {
        get { return currentValue; }
        private set
        {
            currentValue = value;
            if (hud == null) return;
            hud.UpdateResource(currentValue, maxValue, GetType());
        }
    }

    protected virtual void Start()
    {
        hud = HUD.Instance;
        // Must be assigned in awake or execution order must ensure its created first
        // Alternatively I have aslo used some smart properties that return the instance,
        // but if the instance is null search/create/find as appropriate.
        // This only runs the heavy find once but doen't cache until needed which can sometimes be beneficial.
        // It also heeps searching for teh valid HUD instance inside the HUD
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

}
