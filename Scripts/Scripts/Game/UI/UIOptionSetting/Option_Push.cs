using UnityEngine;

public class Option_Push : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;
    [SerializeField] private CanvasGroup AlphaCanvasGroup;

    public override void LoadOption()
    {
        base.LoadOption();

        
        if (ZGameOption.Instance.bPushEnable)
        {
            OnButton.SelectToggle();
        }
        else
        {
            OffButton.SelectToggle();
        }

        AlphaCanvasGroup.enabled = !ZGameOption.Instance.bPushEnable;
        AlphaCanvasGroup.blocksRaycasts = ZGameOption.Instance.bPushEnable;
    }

    public void SetReceivePush(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bPushEnable)
        {
            ZLog.Log(ZLogChannel.UI, $"## OptionSetting ## 푸쉬 설정 : {_isOn}");

            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Push, _isOn);

			NTCore.PushAPI.RequestPushAgreeTerms(ZGameOption.Instance.bPushEnable, ZGameOption.Instance.bNightPushEnable);

            AlphaCanvasGroup.enabled = !ZGameOption.Instance.bPushEnable;
            AlphaCanvasGroup.blocksRaycasts = ZGameOption.Instance.bPushEnable;
        }
    }

}
