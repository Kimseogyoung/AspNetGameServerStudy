
public static class APP
{
    // Config
    public static LocalPlayerPrefs LocalPlayerPrefs { get; set; }
    static public Config.Game GameConf { get; set; }
    static public Config.Debug DebugConf { get; set; }

    // Manager
    public static GAME GAME { get; set; }
    public static SceneManager SceneManager { get; set; }
    public static InputManager InputManager { get; set; }
    public static UIManager UI { get; set; }

    // ClientCore
    public static ClientCore.ContextSystem Ctx { get; set; }
    public static ClientCore.ProtoSystem Prt { get; set; }
}

