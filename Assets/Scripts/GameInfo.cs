using Boo.Lang;
using UnityEngine;

public static class GameInfo
{
    public static bool usingControllers;
    public static int numPlayers;

    public static List<Color> playerColours = new List<Color>
    {
        new Color(1, 0.250f, .286f, 1),
        new Color(0, 0.6580112f, 1, 1),
        new Color(0, 0.7019608f, 0.5058824f, 1),
        new Color(0.3803922f, 0.3803922f, 0.3803922f, 1)
    };
}