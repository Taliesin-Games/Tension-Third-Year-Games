using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HUDOffsetActor : MonoBehaviour
{
    RectTransform rect;
    Vector2 basePosition;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        basePosition = rect.anchoredPosition;
    }

    void Start()
    {
        if (HUDOffsetController.Instance != null)
        {
            HUDOffsetController.Instance.OnOffsetChanged += HandleOffsetChanged;
            
            // Apply current offset in case it changed prior to Start
            HandleOffsetChanged(HUDOffsetController.Instance.CurrentOffset);
        }
    }

    void OnDestroy()
    {
        if (HUDOffsetController.Instance != null)
        {
            HUDOffsetController.Instance.OnOffsetChanged -= HandleOffsetChanged;
        }
    }

    void HandleOffsetChanged(float offset)
    {
        rect.anchoredPosition = new Vector2(
            basePosition.x - offset,
            basePosition.y
        );
    }
}
