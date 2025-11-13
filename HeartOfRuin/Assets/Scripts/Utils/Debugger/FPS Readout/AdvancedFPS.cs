using UnityEngine;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

namespace Utils
{
    public class AdvancedFPS : MonoBehaviour
    {
        const float LOAD_DELAY = 1f; // Delay before starting FPS calculation to allow for initial spikes to settle
        
        float[] frameTimes;
        int index = 0;
        int filled = 0;
        float runningTotal = 0f;
        float timeSinceLog = 0f;
        
        float minFPS = float.MaxValue;
        float maxFPS = float.MinValue;
        
        float startTime = 0f;
        
        // declared global to prevent reallocation every frame
        float avgFPS;
        float frameTimeUnscaled;

        void Start()
        {
            startTime = Time.time + LOAD_DELAY;
            frameTimes = new float[FrameSamples];
        }

        void Update()
        {
            if (Time.time < startTime) return;
            
            frameTimeUnscaled = Time.unscaledDeltaTime;

            CalculateFPS();
            CalculateMinMaxFPS();

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
            // Logging interval
            timeSinceLog += frameTimeUnscaled;
            if (FPSLogInterval <= 0 || timeSinceLog >= FPSLogInterval)
            {
                Debug.Log($"FPS - Avg: {avgFPS:F2}, Min: {minFPS:F2}, Max: {maxFPS:F2} ({filled} samples)");
                timeSinceLog = 0f;
            }
        }
    }
}