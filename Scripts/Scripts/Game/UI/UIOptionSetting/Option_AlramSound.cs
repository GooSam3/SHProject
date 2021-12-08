using UnityEngine;
using UnityEngine.UI;

public class Option_AlramSound : OptionSetting
{
    [SerializeField] private ZSlider SoundPerSlider;
    [SerializeField] private Text PerText;
    [SerializeField] private CanvasGroup AlphaCanvasGroup;

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

        AlphaCanvasGroup.enabled = !ZGameOption.Instance.bAlramBeAttacked_PC;
        AlphaCanvasGroup.blocksRaycasts = ZGameOption.Instance.bAlramBeAttacked_PC;

        SoundPerSlider.value = ZGameOption.Instance.AlramSound * 10f;
    }

    private void OptionChanged(ZGameOption.OptionKey _optionKey)
    {
        if(_optionKey == ZGameOption.OptionKey.Option_Alram_BeAttacked)
        {
            LoadOption();
        }
    }

    public void ChangeAlramPer(float _changedValue)
    {
        PerText.text = string.Format("{0:0}% 이상", _changedValue * 10f);

        if (_changedValue != ZGameOption.Instance.AlramSound * 10f)
        {
            ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 피격음 알림 사운드 설정 : " + _changedValue * 10f);
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_AlramSound, _changedValue / 10f);
        }
    }
}
