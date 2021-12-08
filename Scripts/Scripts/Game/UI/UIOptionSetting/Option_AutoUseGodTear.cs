using UnityEngine.UI;

public class Option_AutoUseGodTear : OptionSetting
{
    public Image GodTearImage;
    public Slider GodTearSlider;
    public Text GodTearStackText;

    uint OptionValue = 0;

    public System.Action<uint> OnChangeValue;

    public override void LoadOption()
    {
        base.LoadOption();

        GodTearSlider.value = OptionValue = ZGameOption.Instance.GodTearStackCnt / 50;
    }

    public override void SaveOption()
    {
        base.SaveOption();

        if (OptionValue * 50 != ZGameOption.Instance.GodTearStackCnt)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_GodTearStackCnt, (uint)OptionValue * 50);
        }
    }

    public void ChangeGodTearItemCount(float fChangedValue)
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 아르엘의 축복 자동사용 설정" + fChangedValue * 50);
        GodTearStackText.text = string.Format("{0:0}", fChangedValue * 50);

        OptionValue = (uint)fChangedValue;
        ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_GodTearStackCnt, (uint)OptionValue * 50);

        OnChangeValue?.Invoke((uint)OptionValue * 50);
    }
}
