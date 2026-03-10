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

    void Update()
    {
        float offset = HUDOffsetController.Instance.CurrentOffset;
        rect.anchoredPosition = new Vector2(
            basePosition.x - offset,
            basePosition.y
        );
    }
}
