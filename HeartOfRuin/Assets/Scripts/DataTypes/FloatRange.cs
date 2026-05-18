namespace BMD.DataTypes
{

    [System.Serializable]
    public struct FloatRange
    {
        public float Min;
        public float Max;
        public readonly float Mean => (Min + Max) * 0.5f;

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int value) => value >= Min && value <= Max;
    }

}