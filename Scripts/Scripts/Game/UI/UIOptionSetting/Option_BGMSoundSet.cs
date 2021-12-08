using UnityEngine;
using UnityEngine.UI;

public class Option_BGMSoundSet : OptionSetting
{
	[SerializeField] private ZSlider BGMSlider;
	[SerializeField] private Text PerText;

	public override void LoadOption()
	{
		base.LoadOption();

		BGMSlider.value = ZGameOption.Instance.BGMSound * 10f;
	}

	public void ChangeBGMPer(float _changedValue)
	{
		PerText.text = string.Format("{0:0}%", _changedValue * 10f);

		if (_changedValue != ZGameOption.Instance.BGMSound * 10f)
		{
			ZLog.Log(ZLogChannel.UI, "## OptionSetting ## BGM 사운드 설정 : " + _changedValue * 10f);
			ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Bgm, _changedValue / 10f);
		}
	}
}
