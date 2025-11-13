using UnityEngine;
using UnityEngine.UI;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

namespace Utils
{
    public class AdvancedFPS : MonoBehaviour
    {
        #region Constants
        const float LOAD_DELAY = 1f; // Delay before starting FPS calculation to allow for initial spikes to settle
        #endregion

        #region Cached References
        Canvas canvas;
        Text fpsText;
        Image fpsPanel;
        #endregion

        #region Variables
        float[] frameTimes;
        int index = 0;
        int filled = 0;
        float runningTotal = 0f;
        float nextLogTime = 0f;
        
        float minFPS = float.MaxValue;
        float maxFPS = float.MinValue;
        
        float startTime = 0f;

        // declared global to prevent reallocation every frame
        float avgFPS;
        float frameTimeUnscaled;
        #endregion



        void Start()
        {
            startTime = Time.time + LOAD_DELAY;
            frameTimes = new float[FrameSamples];

            SetupUI();
        }

        void Update()
        {
            if (Time.time < startTime) return;
            
            frameTimeUnscaled = Time.unscaledDeltaTime;

            CalculateFPS();
            CalculateMinMaxFPS();

            // Logging interval
            if (FPSLogInterval > 0 && Time.time < nextLogTime) return;

            nextLogTime = Time.time + FPSLogInterval;

            UpdateUIText();
            OutputToLog();
        }

        private void CalculateFPS()
        {
            // Remove the old frame time from the total
            runningTotal -= frameTimes[index];

            // Capture this frame time
            frameTimes[index] = frameTimeUnscaled;

            // Add the new time
            runningTotal += frameTimeUnscaled;

            // Move circular index
            index++;
            if (index == FrameSamples) index = 0;

            // Track how many samples are valid
            if (filled < FrameSamples)
                filled++;

            // Compute average FPS
            avgFPS = filled / runningTotal;
        }
        private void CalculateMinMaxFPS()
        {
            if (avgFPS < minFPS) minFPS = avgFPS;
            if (avgFPS > maxFPS) maxFPS = avgFPS;
        }
        private void OutputToLog()
        {
            // Could be extended to output to screen or remote log
            Debug.Log($"FPS - Avg: {avgFPS:F2}, Min: {minFPS:F2}, Max: {maxFPS:F2} ({filled} samples)");
             
        }

        private void UpdateUIText()
        {
            if(!canvas) return;             // Skip if no canvas (UI disabled)
            if (fpsText == null) return;

            // Legacy Text does not support <b> tags everywhere, but it's safe
            fpsText.text =
                $"FPS\n" +
                $"Avg: {avgFPS:F1}\n" +
                $"Min: {minFPS:F1}\n" +
                $"Max: {maxFPS:F1}\n" +
                $"Samples: {filled}";
        }

        private void SetupUI()
        {
            if (!ShowAdvancedFPSOnScreen) return;

            canvas = Debugger.DebuggerCanvas;
            if (canvas == null)
            {
                Debug.LogWarning("AdvancedFPS: DebuggerCanvas not found — FPS UI disabled.");
                return;
            }

            //  Create Panel (background) 
            GameObject panelObj = new GameObject("FPS_Panel");
            panelObj.transform.SetParent(canvas.transform, false);

            fpsPanel = panelObj.AddComponent<Image>();
            fpsPanel.color = new Color(0f, 0f, 0f, 0.45f);   // black, 45% opacity
            fpsPanel.raycastTarget = false;

            RectTransform panelRect = fpsPanel.rectTransform;
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-10f, -10f);


            //  Create Text 
            GameObject textObj = new GameObject("FPS_Text");
            textObj.transform.SetParent(panelObj.transform, false);

            fpsText = textObj.AddComponent<Text>();
            fpsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            fpsText.fontSize = FontSize;
            fpsText.alignment = TextAnchor.UpperLeft;
            fpsText.horizontalOverflow = HorizontalWrapMode.Overflow;
            fpsText.verticalOverflow = VerticalWrapMode.Overflow;
            fpsText.raycastTarget = false;
            fpsText.color = Color.white;

            fpsText.text =
                $"FPS\n" +
                $"Avg: ###.#\n" +
                $"Min: ###.#\n" +
                $"Max: ###.#\n" +
                $"Samples: ##";

            RectTransform textRect = fpsText.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(10f, -5f);

            // Initial resize
            LayoutRebuilder.ForceRebuildLayoutImmediate(fpsText.rectTransform);

            // Resize the background panel to match text (one time only)
            if (fpsPanel != null && fpsText != null)
            {
                float w = fpsText.preferredWidth;
                float h = fpsText.preferredHeight;

                fpsPanel.rectTransform.sizeDelta = new Vector2(w + 20f, h + 10f);
            }
        }
    }
}