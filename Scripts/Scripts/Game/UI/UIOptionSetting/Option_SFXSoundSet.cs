using UnityEngine;
using UnityEngine.UI;

public class Option_SFXSoundSet : OptionSetting
{
    [SerializeField] private ZSlider SFXSlider;
    [SerializeField] private Text PerText;

    public override void LoadOption()
    {
        base.LoadOption();

        SFXSlider.value = ZGameOption.Instance.SFXSound * 10f;
    }

    public void ChangeSFXPer(float _changedValue)
    {
        PerText.text = string.Format("{0:0}%", _changedValue * 10f);

        if (_changedValue != ZGameOption.Instance.SFXSound * 10f)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## SFX 사운드 설정 : " + _changedValue * 10f);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_SfxSound, _changedValue / 10f);
        }
    }
}
