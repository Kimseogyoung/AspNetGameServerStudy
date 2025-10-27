using System.Collections;
using UnityEngine;

public class AppPath 
{
    public static string ConfigDir { get; private set; } = "Data/Config";
    public static string GameConfig { get; private set; } = "/Game";
    public static string DebugConfig { get; private set; } = "/Debug";
    public static string PrefabDir { get; private set; } = "Prefabs";
    public static string SpriteDir { get; private set; } = "Sprites";
    public static string GetPrtPath()
    {
        var rootPath = System.IO.Path.GetFullPath(System.IO.Path.Join(Application.dataPath, "..", ".."));
        var dataPath = System.IO.Path.Join(rootPath, "Data");
        var prtPath = System.IO.Path.Join(dataPath, "Csv", "Proto");
        return prtPath;
    }

    public static string GetSpritePath(string path)
    {
        return $"{SpriteDir}/{path}";
    }

    public static string GetPrefabPath(string path)
    {
        return $"{PrefabDir}/{path}";
    }
}
