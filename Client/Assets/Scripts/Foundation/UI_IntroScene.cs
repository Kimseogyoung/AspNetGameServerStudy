using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_IntroScene : UI_Scene
{
    private void Awake()
    {
        Bind<TMP_Text>(UI.VersionText.ToString()).text = "Version 0.0.0";
    }

    enum UI
    {
        VersionText 
    }
}
