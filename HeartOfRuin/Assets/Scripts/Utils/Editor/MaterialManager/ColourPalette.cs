using UnityEngine;

public readonly struct ColourPalette
{
    public readonly ColourIds Id;
    public readonly Color Colour;

    ColourPalette(ColourIds id, Color color) { Id = id;            Colour = color; }
    ColourPalette(int id, Color color)       { Id = (ColourIds)id; Colour = color; }
    static ColourPalette()
    {
        int enumCount = System.Enum.GetValues(typeof(ColourIds)).Length;

        if (ByIndex.Length != enumCount)
        {
            Debug.LogError(
                $"ColourPalette mismatch: Enum has {enumCount} values, " +
                $"but ByIndex has {ByIndex.Length} entries."
            );
        }

        for (int i = 0; i < ByIndex.Length; i++)
        {
            if ((int)ByIndex[i].Id != i)
            {
                Debug.LogError(
                    $"ColourPalette ID mismatch at index {i}: " +
                    $"expected {(ColourIds)i}, got {ByIndex[i].Id}"
                );
            }
        }
    }

    public static readonly ColourPalette[] ByIndex =
    {
        new ColourPalette(ColourIds.White,       Color.white),
        new ColourPalette(ColourIds.Black,       Color.black),
        new ColourPalette(ColourIds.Grey,        Color.grey),
        new ColourPalette(ColourIds.Red,         Color.red),
        new ColourPalette(ColourIds.Green,       Color.green),
        new ColourPalette(ColourIds.Blue,        Color.blue),
        new ColourPalette(ColourIds.Cyan,        Color.cyan),
        new ColourPalette(ColourIds.Magenta,     Color.magenta),
        new ColourPalette(ColourIds.Yellow,      Color.yellow),
        new ColourPalette(ColourIds.Gold,        new Color(1.0f, 0.843f, 0.0f)),
        new ColourPalette(ColourIds.Silver,      new Color(0.753f, 0.753f, 0.753f)),
        new ColourPalette(ColourIds.Bronze,      new Color(0.804f, 0.498f, 0.196f)),
        new ColourPalette(ColourIds.Orange,      new Color(1.0f, 0.647f, 0.0f)),
        new ColourPalette(ColourIds.Purple,      new Color(0.502f, 0.0f, 0.502f)),
        new ColourPalette(ColourIds.Teal,        new Color(0.0f, 0.502f, 0.502f)),
        new ColourPalette(ColourIds.Pink,        new Color(1.0f, 0.753f, 0.796f)),
        new ColourPalette(ColourIds.Brown,       new Color(0.647f, 0.165f, 0.165f)),
        new ColourPalette(ColourIds.LightRed,    new Color(1.0f, 0.5f, 0.5f)),
        new ColourPalette(ColourIds.LightGreen,  new Color(0.5f, 1.0f, 0.5f)),
        new ColourPalette(ColourIds.LightBlue,   new Color(0.5f, 0.5f, 1.0f)),
        new ColourPalette(ColourIds.LightCyan,   new Color(0.5f, 1.0f, 1.0f)),
        new ColourPalette(ColourIds.LightMagenta,new Color(1.0f, 0.5f, 1.0f)),
        new ColourPalette(ColourIds.LightYellow, new Color(1.0f, 1.0f, 0.5f)),
        new ColourPalette(ColourIds.DarkRed,     new Color(0.5f, 0.0f, 0.0f)),
        new ColourPalette(ColourIds.DarkGreen,   new Color(0.0f, 0.5f, 0.0f)),
        new ColourPalette(ColourIds.DarkBlue,    new Color(0.0f, 0.0f, 0.5f)),
        new ColourPalette(ColourIds.DarkCyan,    new Color(0.0f, 0.5f, 0.5f)),
        new ColourPalette(ColourIds.DarkMagenta, new Color(0.5f, 0.0f, 0.5f)),
        new ColourPalette(ColourIds.DarkYellow,  new Color(0.5f, 0.5f, 0.0f)),
        new ColourPalette(ColourIds.LightOrange, new Color(1.0f, 0.8f, 0.6f)),
        new ColourPalette(ColourIds.LightPurple, new Color(0.8f, 0.6f, 0.8f)),
        new ColourPalette(ColourIds.LightTeal,   new Color(0.6f, 0.8f, 0.8f)),
        new ColourPalette(ColourIds.LightPink,   new Color(1.0f, 0.8f, 0.9f)),
        new ColourPalette(ColourIds.LightBrown,  new Color(0.8f, 0.6f, 0.4f)),
        new ColourPalette(ColourIds.DarkOrange,  new Color(0.8f, 0.4f, 0.0f)),
        new ColourPalette(ColourIds.DarkPurple,  new Color(0.4f, 0.0f, 0.4f)),
        new ColourPalette(ColourIds.DarkTeal,    new Color(0.0f, 0.4f, 0.4f)),
        new ColourPalette(ColourIds.DarkPink,    new Color(0.8f, 0.4f, 0.5f)),
        new ColourPalette(ColourIds.DarkBrown,   new Color(0.4f, 0.2f, 0.2f))

    };

    public enum ColourIds
    {
        White,
        Black,
        Grey,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow,
        Gold,
        Silver,
        Bronze,
        Orange,
        Purple,
        Teal,
        Pink,
        Brown,
        LightRed,
        LightGreen,
        LightBlue,
        LightCyan,
        LightMagenta,
        LightYellow,
        DarkRed,
        DarkGreen,
        DarkBlue,
        DarkCyan,
        DarkMagenta,
        DarkYellow,
        LightOrange,
        LightPurple,
        LightTeal,
        LightPink,
        LightBrown,
        DarkOrange,
        DarkPurple,
        DarkTeal,
        DarkPink,
        DarkBrown
    }

    public static ColourPalette Get(int i)       => ByIndex[i];
    public static ColourPalette Get(ColourIds c) => ByIndex[(int)c];
    public static Color GetColour(int i)         => ByIndex[i].Colour;
    public static Color GetColour(ColourIds c)   => ByIndex[(int)c].Colour;

    public static Color White => GetColour(ColourIds.White);
    public static Color Black => GetColour(ColourIds.Black);
    public static Color Grey  => GetColour(ColourIds.Grey);
    public static Color Red   => GetColour(ColourIds.Red);
    public static Color Green => GetColour(ColourIds.Green);
    public static Color Blue  => GetColour(ColourIds.Blue);
    public static Color Cyan  => GetColour(ColourIds.Cyan);
    public static Color Magenta => GetColour(ColourIds.Magenta);
    public static Color Yellow => GetColour(ColourIds.Yellow);
    public static Color Gold  => GetColour(ColourIds.Gold);
    public static Color Silver => GetColour(ColourIds.Silver);
    public static Color Bronze => GetColour(ColourIds.Bronze);
    public static Color Orange => GetColour(ColourIds.Orange);
    public static Color Purple => GetColour(ColourIds.Purple);
    public static Color Teal => GetColour(ColourIds.Teal);
    public static Color Pink => GetColour(ColourIds.Pink);
    public static Color Brown => GetColour(ColourIds.Brown);
    public static Color LightRed   => GetColour(ColourIds.LightRed);
    public static Color LightGreen => GetColour(ColourIds.LightGreen);
    public static Color LightBlue  => GetColour(ColourIds.LightBlue);
    public static Color LightCyan  => GetColour(ColourIds.LightCyan);
    public static Color LightMagenta => GetColour(ColourIds.LightMagenta);
    public static Color LightYellow => GetColour(ColourIds.LightYellow);
    public static Color DarkRed   => GetColour(ColourIds.DarkRed);
    public static Color DarkGreen => GetColour(ColourIds.DarkGreen);
    public static Color DarkBlue  => GetColour(ColourIds.DarkBlue);
    public static Color DarkCyan  => GetColour(ColourIds.DarkCyan);
    public static Color DarkMagenta => GetColour(ColourIds.DarkMagenta);
    public static Color DarkYellow => GetColour(ColourIds.DarkYellow);
    public static Color LightOrange => GetColour(ColourIds.LightOrange);
    public static Color LightPurple => GetColour(ColourIds.LightPurple);
    public static Color LightTeal   => GetColour(ColourIds.LightTeal);
    public static Color LightPink   => GetColour(ColourIds.LightPink);
    public static Color LightBrown  => GetColour(ColourIds.LightBrown);
    public static Color DarkOrange  => GetColour(ColourIds.DarkOrange);
    public static Color DarkPurple  => GetColour(ColourIds.DarkPurple);
    public static Color DarkTeal    => GetColour(ColourIds.DarkTeal);
    public static Color DarkPink    => GetColour(ColourIds.DarkPink);
    public static Color DarkBrown   => GetColour(ColourIds.DarkBrown);

}