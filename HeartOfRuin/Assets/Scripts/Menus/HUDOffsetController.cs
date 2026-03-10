using UnityEngine;




public class HUDOffsetController : MonoBehaviour
{
    public static HUDOffsetController Instance;

    public float CurrentOffset { get; private set; }

    [SerializeField] RectTransform canvasRect;
    [SerializeField] float PercentWidth = 0.4f;
    [SerializeField] float animationSpeed = 100f;

    float targetOffset;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        CurrentOffset = Mathf.Lerp(CurrentOffset, targetOffset, Time.deltaTime * animationSpeed);
    }

    public void SetOffsetEnabled(bool open)
    {
        float canvasWidth = canvasRect.rect.width;

        targetOffset = open ? canvasWidth * PercentWidth : 0f;
    }
}
