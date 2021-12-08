using UnityEngine;

public class Option_NightPush : OptionSetting
{
    [SerializeField] private ZToggle OnButton, OffButton;

    public override void OnEnable()
    {
        base.OnEnable();
        ZGameOption.Instance.OnOptionChanged += OptionChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        ZGameOption.Instance.OnOptionChanged -= OptionChanged;
    }

    public override void LoadOption()
    {
        base.LoadOption();

        //[박윤성] 푸시를 끄면 야간푸시도 꺼지게변경
        if (!ZGameOption.Instance.bPushEnable)
        {
            OffButton.SelectToggle();
        }
        else
        {
            if (ZGameOption.Instance.bNightPushEnable)
            {
                OnButton.SelectToggle();
            }
            else
            {
                OffButton.SelectToggle();
            }
        }
    }

    private void OptionChanged(ZGameOption.OptionKey _optionKey)
    {
        if (_optionKey == ZGameOption.OptionKey.Option_Push)
        {
            LoadOption();
        }
    }

    public void SetReceiveNightPush(bool _isOn)
    {
        if (_isOn != ZGameOption.Instance.bNightPushEnable)
        {
            ZLog.Log(ZLogChannel.UI, $"## OptionSetting ## 야간 푸쉬 설정 : {_isOn}");

			ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_NightPush, _isOn);

			NTCore.PushAPI.RequestPushAgreeTerms(ZGameOption.Instance.bPushEnable, ZGameOption.Instance.bNightPushEnable);
		}
    }
}
