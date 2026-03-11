using UnityEngine;

[System.Serializable]
public struct DamageStruct
{
    public float None;
    public float Physical;
    public float Magical;
    public float True;
    public float Fire;
    public float Lightning;
    public float Ice;
    public float Earth;
    public float Wind;
    public float Water;

    // Multiply DamageStruct by a float (DamageStruct * float)
    public static DamageStruct operator *(DamageStruct a, float multiplier)
    {
        return new DamageStruct
        {
            None = a.None * multiplier,
            Physical = a.Physical * multiplier,
            Magical = a.Magical * multiplier,
            True = a.True * multiplier,
            Fire = a.Fire * multiplier,
            Lightning = a.Lightning * multiplier,
            Ice = a.Ice * multiplier,
            Earth = a.Earth * multiplier,
            Wind = a.Wind * multiplier,
            Water = a.Water * multiplier
        };
    }

    public static DamageStruct operator +(DamageStruct a, DamageStruct b)
    {
        return new DamageStruct
        {
            None = a.None + b.None,
            Physical = a.Physical + b.Physical,
            Magical = a.Magical + b.Magical,
            True = a.True + b.True,
            Fire = a.Fire + b.Fire,
            Lightning = a.Lightning + b.Lightning,
            Ice = a.Ice + b.Ice,
            Earth = a.Earth + b.Earth,
            Wind = a.Wind + b.Wind,
            Water = a.Water + b.Water
        };
    }

    // Multiply DamageStruct by a float (DamageStruct * float)
    public static DamageStruct operator /(DamageStruct a, float divisor)
    {
        return new DamageStruct
        {
            None = a.None / divisor,
            Physical = a.Physical / divisor,
            Magical = a.Magical / divisor,
            True = a.True / divisor,
            Fire = a.Fire / divisor,
            Lightning = a.Lightning / divisor,
            Ice = a.Ice / divisor,
            Earth = a.Earth / divisor,
            Wind = a.Wind / divisor,
            Water = a.Water / divisor
        };
    }

    // Add a float to each field of DamageStruct (DamageStruct + float)
    public static DamageStruct operator +(DamageStruct a, float number)
    {
        return new DamageStruct
        {
            None = a.None + number,
            Physical = a.Physical + number,
            Magical = a.Magical + number,
            True = a.True + number,
            Fire = a.Fire + number,
            Lightning = a.Lightning + number,
            Ice = a.Ice + number,
            Earth = a.Earth + number,
            Wind = a.Wind + number,
            Water = a.Water + number
        };
    }

    // Subtract a float from each field of DamageStruct (DamageStruct - float)
    public static DamageStruct operator -(DamageStruct a, float number)
    {
        return new DamageStruct
        {
            None = a.None - number,
            Physical = a.Physical - number,
            Magical = a.Magical - number,
            True = a.True - number,
            Fire = a.Fire - number,
            Lightning = a.Lightning - number,
            Ice = a.Ice - number,
            Earth = a.Earth - number,
            Wind = a.Wind - number,
            Water = a.Water - number
        };
    }


    public static DamageStruct operator -(DamageStruct a, DamageStruct b)
    {
        return new DamageStruct
        {
            None = a.None - b.None,
            Physical = a.Physical - b.Physical,
            Magical = a.Magical - b.Magical,
            True = a.True - b.True,
            Fire = a.Fire - b.Fire,
            Lightning = a.Lightning - b.Lightning,
            Ice = a.Ice - b.Ice,
            Earth = a.Earth - b.Earth,
            Wind = a.Wind - b.Wind,
            Water = a.Water - b.Water
        };
    }



    //Multiply DamageStruct by another DamageStruct (DamageStruct * DamageStruct)
    public static DamageStruct operator *(DamageStruct a, DamageStruct b)
    {
        return new DamageStruct
        {
            None = a.None * b.None,
            Physical = a.Physical * b.Physical,
            Magical = a.Magical * b.Magical,
            True = a.True * b.True,
            Fire = a.Fire * b.Fire,
            Lightning = a.Lightning * b.Lightning,
            Ice = a.Ice * b.Ice,
            Earth = a.Earth * b.Earth,
            Wind = a.Wind * b.Wind,
            Water = a.Water * b.Water
        };
    }


    //cast damage struct to float by summing all fields
    public static implicit operator float(DamageStruct a)
    {
        return a.None + a.Physical + a.Magical + a.True + a.Fire + a.Lightning + a.Ice + a.Earth + a.Wind + a.Water;
    }

    // Override ToString() for easy debugging
    public override string ToString()
    {
        return $"DamageStruct(None: {None}, Physical: {Physical}, Magical: {Magical}, True: {True}, Fire: {Fire}, Lightning: {Lightning}, Ice: {Ice}, Earth: {Earth}, Wind: {Wind}, Water: {Water})";
    }
}