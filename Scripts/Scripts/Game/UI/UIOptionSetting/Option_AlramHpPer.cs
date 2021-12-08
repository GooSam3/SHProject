using UnityEngine;
using UnityEngine.UI;

public class Option_AlramHpPer : OptionSetting
{
    [SerializeField] private ZSlider HPPerSlider;
    [SerializeField] private Text PerText;

    public override void LoadOption()
    {
        base.LoadOption();

        HPPerSlider.value = ZGameOption.Instance.AlramHPPer * 10f;
    }

    public void ChangeHPPer(float _changedValue)
    {
        PerText.text = string.Format(DBLocale.GetText("Option_Alert_HP_Value2"), _changedValue * 10f);

        if (_changedValue != ZGameOption.Instance.AlramHPPer * 10f)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 체력 알림 설정 : " + _changedValue * 10f);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Alram_HpPer, _changedValue / 10f);
        }
    }
}
