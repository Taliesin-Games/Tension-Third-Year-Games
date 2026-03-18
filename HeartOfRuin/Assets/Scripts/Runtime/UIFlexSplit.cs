using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UIFlexSplit : MonoBehaviour
{
    public enum SplitDirection
    {
        Horizontal,
        Vertical
    }

    [Header("References")]
    [SerializeField] RectTransform item1;
    [SerializeField] RectTransform item2;

    [Header("Layout")]
    [SerializeField] SplitDirection splitDirection = SplitDirection.Horizontal;

    [SerializeField, Range(0f, 1f)]
    float divide = 0.5f;

    [Header("Behaviour")]
    [SerializeField] bool ignoreSiblings = false;
    [SerializeField] bool forceResizeChildren = false;

    RectTransform selfRect;

    bool item1Enabled = true;
    bool item2Enabled = true;

    List<UIFlexSplit> childFlexes = new List<UIFlexSplit>();

    void Start()
    {
        CacheRectTransform();
        CheckSiblings();
        CacheChildFlexes();
        ForceResize();
    }

    void Update()
    {
        DebugInput();
    }

    private void CacheRectTransform()
    {
        selfRect = GetComponent<RectTransform>();

        if (selfRect == null)
        {
            Debug.LogError($"{name} requires a RectTransform.");
            return;
        }
    }

    private void CheckSiblings()
    {
        if (ignoreSiblings) return;

        int validCount = 0;

        foreach (Transform child in transform)
        {
            if (child == item1 || child == item2)   validCount++;
            else                                    Debug.LogWarning($"{name} has unexpected sibling: {child.name}");
        }

        if (validCount < 2)
        {
            Debug.LogWarning($"{name} does not have both item1 and item2 as children.");
        }
    }

    private void CacheChildFlexes()
    {
        childFlexes.Clear();
        childFlexes.AddRange(GetComponentsInChildren<UIFlexSplit>());

        // Remove self
        childFlexes.Remove(this);
    }

    // -------- PUBLIC API --------

    public void ResizeItem1(float normalizedSize)
    {
        divide = Mathf.Clamp01(normalizedSize);
        ForceResize();
    }

    public void ResizeItem2(float normalizedSize)
    {
        divide = 1f - Mathf.Clamp01(normalizedSize);
        ForceResize();
    }

    public void SetItem1Enabled(bool enabled)
    {
        item1Enabled = enabled;
        if (item1 != null) item1.gameObject.SetActive(enabled);
        ForceResize();
    }

    public void SetItem2Enabled(bool enabled)
    {
        item2Enabled = enabled;
        if (item2 != null) item2.gameObject.SetActive(enabled);
        ForceResize();
    }

    public void ForceResize()
    {
        if (selfRect == null || item1 == null || item2 == null) return;

        float size1 = 0f;
        float size2 = 0f;

        int enabledCount = (item1Enabled ? 1 : 0) + (item2Enabled ? 1 : 0);

        if (enabledCount == 0)
        {
            size1 = 0f;
            size2 = 0f;
        }
        else if (enabledCount == 1)
        {
            if (item1Enabled)
            {
                size1 = 1f;
                size2 = 0f;
            }
            else
            {
                size1 = 0f;
                size2 = 1f;
            }
        }
        else
        {
            size1 = divide;
            size2 = 1f - divide;
        }

        ApplySizes(size1, size2);

        if (forceResizeChildren)
        {
            foreach (var child in childFlexes)
            {
                if (child != null) child.ForceResize();
            }
        }
    }

    // -------- INTERNAL --------

    private void ApplySizes(float size1, float size2)
    {
        if (splitDirection == SplitDirection.Horizontal)
        {
            SetHorizontal(item1, 0f, size1);
            SetHorizontal(item2, size1, size2);
        }
        else
        {
            SetVertical(item1, 0f, size1);
            SetVertical(item2, size1, size2);
        }
    }

    private void SetHorizontal(RectTransform rt, float start, float size)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(start, 0f);
        rt.anchorMax = new Vector2(start + size, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void SetVertical(RectTransform rt, float start, float size)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0f, start);
        rt.anchorMax = new Vector2(1f, start + size);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // -------- DEBUG --------

    private void DebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            divide = Mathf.Clamp01(divide + 0.1f);
            ForceResize();
        }

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            divide = Mathf.Clamp01(divide - 0.1f);
            ForceResize();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetItem1Enabled(!item1Enabled);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetItem2Enabled(!item2Enabled);
        }
    }
}