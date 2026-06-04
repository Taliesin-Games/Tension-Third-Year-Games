using UnityEngine;
using UnityEngine.UI;

public class HourglassUIElement : MonoBehaviour
{
    [SerializeField] Image HourglassTop;
    [SerializeField] Image HourglassBottom;



    void Update()
    {
        if(GameManager.Instance != null)
        {
            float ratio = GameManager.Instance.TensionCompletionRatio;
            HourglassTop.fillAmount = 1 - ratio;
            HourglassBottom.fillAmount = ratio;
        }
    }
}
