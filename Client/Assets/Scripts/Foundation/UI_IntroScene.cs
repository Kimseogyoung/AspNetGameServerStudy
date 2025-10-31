using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_IntroScene : UI_Scene
{
    private TMP_Text _loadingText;

    protected override void InitImp()
    {
        base.InitImp();
        Bind<TMP_Text>(UI.VersionText.ToString()).text = "Version 0.0.0";

        _loadingText = Bind<TMP_Text>(UI.LoadingText.ToString());
        _loadingText.text = "";
    }

    protected override void OnDestroyed()
    {

    }

    public void SetLoadingText(string loadingText)
    {
        _loadingText.text = loadingText;
    }

    enum UI
    {
        VersionText,
        LoadingText
    }
}
