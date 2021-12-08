using UnityEngine.UI;

public class Option_RemainMpPotion : OptionSetting
{
    public Slider MpSlider;
    public Text MpPerText;

    float OptionValue;

    public override void LoadOption()
    {
        base.LoadOption();

        MpSlider.value = OptionValue = ZGameOption.Instance.RemainMPPer * 10f;
    }

    public override void SaveOption()
    {
        base.SaveOption();

        if(OptionValue != ZGameOption.Instance.RemainMPPer)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_RemainMpPer, OptionValue / 10f);
        }
    }

    public void ChangeMpPer(float fChangedValue)
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 잔여 MP 설정" + fChangedValue * 10f);

        MpPerText.text = string.Format("{0:0}%", (fChangedValue * 10f));

        OptionValue = fChangedValue;

        ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_RemainMpPer, OptionValue / 10f);
    }
}
