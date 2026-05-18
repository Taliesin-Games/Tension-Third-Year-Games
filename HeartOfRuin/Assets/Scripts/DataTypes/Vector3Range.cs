
using UnityEngine;

namespace BMD.DataTypes
{

    [System.Serializable]
    public struct Vector3Range
    {
        public Vector3 Min;
        public Vector3 Max;
        public readonly Vector3 Mean => (Min + Max) * 0.5f;

        public Vector3Range(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }


        public bool Contains(Vector3 value)
        {
            return value.x >= Min.x && value.x <= Max.x
                && value.y >= Min.y && value.y <= Max.y
                && value.z >= Min.z && value.z <= Max.z;
        }
            
    }

}