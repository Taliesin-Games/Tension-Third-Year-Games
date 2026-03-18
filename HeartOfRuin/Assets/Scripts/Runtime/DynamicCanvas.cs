using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCanvas : MonoBehaviour
{
    [SerializeField] CanvasScaler scaler;
    [SerializeField] RectTransform left;
    [SerializeField] RectTransform right;
    [SerializeField] float dividerPosition = 0.6f;
    [SerializeField] float transitionDuration = 0.25f;

    Vector2 screenSize;
    bool isShowingInventory = false;
    float targetDivide;
    float divide;

    private void Start()
    {
        scaler = GetComponent<CanvasScaler>();
        screenSize = scaler.referenceResolution;
    }

    [ContextMenu("Update")]
    public void SetSize()
    {
        float totalWidth = scaler.referenceResolution.x;
        
        float leftWidth = totalWidth * divide;
        float rightWidth = totalWidth * (1 - divide);

        left.sizeDelta = new Vector2(leftWidth, left.sizeDelta.y);
        right.sizeDelta = new Vector2(rightWidth, right.sizeDelta.y);

    }

    [ContextMenu("Show Inventory UI")]
    public void ToggleInventory()
    {
        isShowingInventory = !isShowingInventory;

        //right.gameObject.SetActive(isShowingInventory);

        targetDivide = isShowingInventory ? dividerPosition : 1f;

        StartCoroutine(LerpUI());

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleInventory();
        }
    }
    IEnumerator LerpUI()
    {
        float startDivide = divide;
        float elapsed = 0f;

        isShowingInventory = !isShowingInventory;

        if(isShowingInventory)
        {
            right.gameObject.SetActive(true);
        }


        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            divide = Mathf.Lerp(startDivide, targetDivide, t);
            SetSize();

            yield return null;
        }

        divide = targetDivide;
        SetSize();

       
    }
}
