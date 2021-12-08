using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option_BlessBuffPotion : OptionSetting
{
    public Slider GodTearSlider;
    public Text GodTearStackText;

    uint optionValue;

    public System.Action<uint> OnChangeValue;

    public override void LoadOption()
    {
        base.LoadOption();

        GodTearSlider.value = optionValue = ZGameOption.Instance.GodTearStackCnt / 50;
    }

    public void ChangeNormalPotionPer(float ChangedValue)
    {
        GodTearStackText.text = string.Format("{0:0}", ChangedValue * 50);

        optionValue = (uint)(ChangedValue);
        OnChangeValue?.Invoke(optionValue * 50);
    }

    public override void SaveOption()
    {
        base.SaveOption();

        if (optionValue * 50 != ZGameOption.Instance.GodTearStackCnt)
        {
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_GodTearStackCnt, (uint)optionValue * 50);
        }
    }
}
