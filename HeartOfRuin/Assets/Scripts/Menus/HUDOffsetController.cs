using System;
using UnityEngine;

public class HUDOffsetController : MonoBehaviour
{
    public static HUDOffsetController Instance;

    public event Action<float> OnOffsetChanged;

    public float CurrentOffset { get; private set; }

    [SerializeField] RectTransform canvasRect;
    [SerializeField] float PercentWidth = 0.4f;
    [SerializeField] float animationSpeed = 100f;

    float targetOffset;
    bool isAnimating;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isAnimating) return;

        CurrentOffset = Mathf.Lerp(CurrentOffset, targetOffset, Time.deltaTime * animationSpeed);

        // Snap to target if very close to stop the animation
        if (Mathf.Abs(targetOffset - CurrentOffset) < 0.1f)
        {
            CurrentOffset = targetOffset;
            isAnimating = false;
        }

        OnOffsetChanged?.Invoke(CurrentOffset);
    }

    public void SetOffsetEnabled(bool open)
    {
        float canvasWidth = canvasRect.rect.width;

        targetOffset = open ? canvasWidth * PercentWidth : 0f;
        isAnimating = true;
    }
}
