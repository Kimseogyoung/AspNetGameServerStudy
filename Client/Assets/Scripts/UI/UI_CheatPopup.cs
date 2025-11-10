using TMPro;

public class UI_CheatPopup : UI_Popup
{
    private TMP_InputField _cheatInputField;
    private TMP_Text _historyText;

    protected override void InitImp()
    {
        base.InitImp();
        _cheatInputField = Bind<TMP_InputField>(UI.CheatInputField.ToString());
        _historyText = Bind<TMP_Text>(UI.HistoryText.ToString());
    }

    protected override void OnClose()
    {
        _cheatInputField.onEndEdit.RemoveAllListeners();
    }

    protected override void OnOpen()
    {
        _cheatInputField.ActivateInputField();
        _cheatInputField.onEndEdit.AddListener(RunCommand);
    }


    private void RunCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return;
        }

        var args = command.Split(' ');
        if (args.Length <= 0 )
        {
            LOG.E($"Failed RunCommand({command})");
            return;
        }

        // TODO: 구조 개선 필요. 핸들러 등록 방식으로 수정.
        var cheatName = args[0].ToLower();
        switch (cheatName)
        {
            case "reward":
                var rewardAmount = args.Length >= 2 ? int.Parse(args[1]) : 100;
                APP.Ctx.RequestCheatReward("", 0, rewardAmount);
                LOG.I($"RunCommand({command})");
                break;
            case "help":
                // 도움말. 치트 목록 표시
                break;
            default:
                LOG.E($"Not Found Command({command})");
                break;

        }

        _cheatInputField.text = "";
        _cheatInputField.ActivateInputField();
        RefreshHistory(command);
    }

    private void RefreshHistory(string command)
    {
        _historyText.text += "\n" + command;
    }

    public enum UI
    {
        CheatInputField,
        HistoryText
    }
}
