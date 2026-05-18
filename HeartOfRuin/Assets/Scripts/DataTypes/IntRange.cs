namespace BMD.DataTypes
{
     
    [System.Serializable]
    public struct IntRange
    {
        
        public int Min;
        public int Max;

        public readonly float Mean => (Min + Max) * 0.5f;
        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int value) => value >= Min && value <= Max;
    }

}