using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Settings definition for the pool manager.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolManagerSettings", menuName = "Utils/Pool Manager Settings")]
    public class PoolManagerSettings : ScriptableObject
    {
        [SerializeField] Pool[] pools;
    }
}
