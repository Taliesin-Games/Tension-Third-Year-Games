using UnityEngine;
using System.Collections;

namespace BMD.ProcGen
{
    public partial class TerrainGenerator : MonoBehaviour
    {
        /// <summary>
        /// Manages performance throttling, optinally specify a small step to bypass frame based 
        /// throttling until the next step, this is for cases where you want to slow down generation 
        /// but not every single step.
        /// </summary>
        /// <param name="smallStep">If true, bypasses frame based throttling until the next step.</param>
        /// <returns></returns>
        bool SetThrottleYield(bool smallStep = false)
        {
            
            if (stepThroughGeneration)
            {
                Throttle = WaitForDebugStep();
                return Throttle != null;
            }

            if (slowGeneration)
            {
                if (smallStep) Throttle = null;
                Throttle = ThrottleByFrames();
                return Throttle != null;
            }

            generationStepsThisFrame++;
            if (generationStepsThisFrame >= GenerationThrottleAmount)
            {
                generationStepsThisFrame = 0;
                Throttle= ThrottleBySteps();
                return Throttle != null;
            }

            // Defaults to null.
            Throttle = null;

            return Throttle != null;
        }
        /// <summary>
        /// Pasues generation until the user presses the space bar, 
        /// allowing step by step debugging of the generation process.
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForDebugStep()
        {
            // This can repeat more than once a frame, so we protect agains this here.
            while (!Input.GetKeyDown(KeyCode.Space) && !debugStepDoneThisFrame)        // TODO need to look up if the new input system has a single line alternative.
                yield return null;

            AudioSource.PlayClipAtPoint(debugBeep, Vector3.zero);
            debugStepDoneThisFrame = true;  // This gets reset in the update loop
        }
        /// <summary>
        /// Throttles based on generation steps, allowing a certain number of steps per frame. 
        /// This is the default throttling method.
        /// </summary>
        /// <returns></returns>
        IEnumerator ThrottleBySteps()
        {
            yield return null;
        }
        /// <summary>
        ///  Waits a certain number of frames before allowing the next generation step, 
        ///  effectively slowing down the generation process.
        /// </summary>
        /// <returns></returns>
        IEnumerator ThrottleByFrames()
        {
            for (int i = 0; i < GenerationThrottleAmount; i++)
            {
                yield return waitForFixedUpdate;
            }
        }
    }
}

