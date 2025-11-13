using UnityEngine;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

namespace Utils
{
    public class AdvancedFPS : MonoBehaviour
    {
        float[] frameTimes;
        int index = 0;
        int filled = 0;
        float runningTotal = 0f;
        float timeSinceLog = 0f;

        void Start()
        {
            frameTimes = new float[FrameSamples];
        }

        void Update()
        {
            CalculateFPS();
        }

        private void CalculateFPS()
        {
            // Remove the old frame time from the total
            runningTotal -= frameTimes[index];

            // Capture this frame time
            float dt = Time.unscaledDeltaTime;
            frameTimes[index] = dt;

            // Add the new time
            runningTotal += dt;

            // Move circular index
            index++;
            if (index == FrameSamples) index = 0;

            // Track how many samples are valid
            if (filled < FrameSamples)
                filled++;

            // Compute average FPS
            float avgFPS = filled / runningTotal;

            // Logging interval
            timeSinceLog += dt;
            if (FPSLogInterval <= 0 || timeSinceLog >= FPSLogInterval)
            {
                Debug.Log($"Average FPS ({filled} samples): {avgFPS:F2}");
                timeSinceLog = 0f;
            }
        }
    }
}