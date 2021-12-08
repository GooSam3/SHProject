using UnityEngine;
using UnityEngine.UI;

public class Option_AlramHpPotionCount : OptionSetting
{
    [SerializeField] private ZSlider HPPotionSlider;
    [SerializeField] private Text CountText;

    public override void LoadOption()
    {
        base.LoadOption();

        HPPotionSlider.value = ZGameOption.Instance.AlramHPPotion / 10;
    }

    public void ChangeHPPotion(float _changedValue)
    {
        if (_changedValue <= 0)
        {
            CountText.text = string.Format("꺼짐");
        }
        else
        {
            CountText.text = string.Format(DBLocale.GetText("Option_Alert_HP_Value1"), _changedValue * 10f);
        }

        if (_changedValue * 10 != ZGameOption.Instance.AlramHPPotion)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 회복 물약 개수 알림 설정 : " + _changedValue * 10f);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Alram_HpPotion, (uint)(_changedValue * 10f));
        }
    }
}
