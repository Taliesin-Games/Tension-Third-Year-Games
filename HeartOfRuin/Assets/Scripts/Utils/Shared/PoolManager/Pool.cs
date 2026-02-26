using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Defines a pool of objects for pooling purposes.
    /// </summary>
    [System.Serializable]
    public class Pool
    {
        [SerializeField] GameObject prefab;

        [Min(1)] [SerializeField] int defaultSize = 10;
        [Min(0)] [SerializeField] int minSize = 5;
        [Min(2)] [SerializeField] int maxSize = 100;

        [SerializeField] int expansionPriority = 0;
        public void DebugTest()
        {
            // Method exists to confirm variables working
            Debug.Log($"DefaultSize: {defaultSize}, minSize: {minSize}, maxSize: {maxSize}, expansionPriority: {expansionPriority}");
        }
    }
    
}