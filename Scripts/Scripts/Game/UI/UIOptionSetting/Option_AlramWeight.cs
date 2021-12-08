using UnityEngine;
using UnityEngine.UI;

public class Option_AlramWeight : OptionSetting
{
    [SerializeField] private ZSlider WeightPerSlider;
    [SerializeField] private Text PerText;

    public override void LoadOption()
    {
        base.LoadOption();

        WeightPerSlider.value = ZGameOption.Instance.AlramWeight * 10f;
    }

    public void ChangeWeightPer(float _changedValue)
    {
        if (_changedValue >= 5)
        {
            PerText.text = string.Format(DBLocale.GetText("Option_Alert_Weight_Value1"), _changedValue * 10f);
        }
        else
        {
            PerText.text = DBLocale.GetText("Option_Alert_Weight_Value2");
            WeightPerSlider.value = 0f;
        }

        if (_changedValue != ZGameOption.Instance.AlramWeight * 10f)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 무게 알림 설정 : " + _changedValue * 10f);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Alram_Weight, _changedValue / 10f);
        }
    }
}
