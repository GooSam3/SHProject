using UnityEngine;

public class Option_AutoScreenSaverMode : OptionSetting
{
    [SerializeField] private ZToggle Min5Button, Min30Button, Min60Button, OffButton;
    public override void LoadOption()
    {
        base.LoadOption();
        
        switch (ZGameOption.Instance.Auto_Screen_Saver_Time)
        {
            case 0:
                OffButton.SelectToggle();
                break;
            case 5 * TimeHelper.MinuteSecond:
                Min5Button.SelectToggle();
                break;
            case 30 * TimeHelper.MinuteSecond:
                Min30Button.SelectToggle();
                break;
            case 60 * TimeHelper.MinuteSecond:
                Min60Button.SelectToggle();
                break;
        }
    }

    public void SetAutoScreenSaverTime(float _time)
    {
        if(ZGameOption.Instance.Auto_Screen_Saver_Time != (_time * TimeHelper.MinuteSecond))
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 절전 모드 설정 : " + _time * TimeHelper.MinuteSecond);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AutoScreenSaverTime, _time * TimeHelper.MinuteSecond);
        }
    }
}
