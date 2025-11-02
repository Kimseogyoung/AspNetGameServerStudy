using System.IO;
using System.Security.Principal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class EditorMenu
{
    [MenuItem("Custom/Scenes/Open Intro Scene")]
    static void OpenIntroScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/IntroScene.unity");
    }

    [MenuItem("Custom/Scenes/Open InGame Scene")]
    static void OpenLobbyScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/InGameScene.unity");
    }

    [MenuItem("Custom/PlayerPref/Clear PlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Custom/PlayerPref/Open PlayerPrefs")]
    static void OpenPlayerPrefs()
    {
        File.Open($"C:\\{WindowsIdentity.GetCurrent().Name}\\Software\\Unity\\UnityEditor\\{Application.companyName}\\{Application.productName}", FileMode.OpenOrCreate);
    }

    [MenuItem("Custom/Config/Open DebugConfig")]
    static void OpenDebugConfig()
    {
        File.Open(Path.Join(Application.dataPath, AppPath.DebugConfig), FileMode.OpenOrCreate);
    }

    [MenuItem("Custom/Config/Open GameConfig")]
    static void OpenGameConfig()
    {
        File.Open(Path.Join(Application.dataPath, AppPath.GameConfig), FileMode.OpenOrCreate);
    }

    [MenuItem("Custom/Open TMPFontReplacerWindow")]
    static void OpenTMPFontReplacerWindow()
    {
        TMPFontReplacerWindow.ShowWindow();
    }
}
