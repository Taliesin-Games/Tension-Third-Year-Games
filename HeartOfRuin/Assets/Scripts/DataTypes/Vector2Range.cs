
using UnityEngine;

namespace BMD.DataTypes
{

    [System.Serializable]
    public struct Vector2Range
    {
        public Vector2 Min;
        public Vector2 Max;
        public readonly Vector2 Mean => (Min + Max) * 0.5f;

        public Vector2Range(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }


        public bool Contains(Vector2 value)
        {
            return value.x >= Min.x && value.x <= Max.x
                && value.y >= Min.y && value.y <= Max.y;
        }
            
    }

}