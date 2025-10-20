using UnityEngine;

public static class DEF
{
    public const int INVALID_NUM = 0;
    public const int INVALID_IDX = -1;

    public static Color C_BLOCK = Color.black;
    public static Color C_WHITE = Color.white;
    public static Color C_RED = Color.red;
    public static Color C_CLEAR_BLOCK = new(0, 0, 0, 0.4f);
    public static Color C_CLEAR_WHITE = new(1, 1, 1, 0.4f);
    public static Color C_CLEAR = new(0, 0, 0, 0);

    public enum UIEvent
    {
        Click,
        Drag,
    }

    public enum MouseEvent
    {
        Press,
        Click,
    }

    public enum CameraMode
    {
        QuarterView,
    }
}

