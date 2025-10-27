using System.Collections;
using UnityEngine;

public class AppPath 
{
    public static string ConfigDir { get; private set; } = "Data/Config";
    public static string GameConfig { get; private set; } = "/Game";
    public static string DebugConfig { get; private set; } = "/Debug";
    public static string PrefabDir { get; private set; } = "Prefabs";
    public static string GetCsvPath()
    {
        var rootPath = System.IO.Path.GetFullPath(System.IO.Path.Join(Application.dataPath, "..", ".."));
        var csvPath = System.IO.Path.Join(rootPath, "Data", "Csv");
        return csvPath;
    }
}
