using UnityEngine;

public class Option_GameFrameRateSet : OptionSetting
{
    [SerializeField] private ZToggle VeryHighToggle, HighToggle, NormalToggle, LowToggle;

    public override void LoadOption()
    {
        base.LoadOption();

        switch (ZGameOption.Instance.Quality)
        {
			case E_Quality.VeryHigh:
                VeryHighToggle.SelectToggle();
                break;
            case E_Quality.High:
                HighToggle.SelectToggle();
                break;
			case E_Quality.Midium:
				NormalToggle.SelectToggle();
                break;
			case E_Quality.Low:
				LowToggle.SelectToggle();
                break;
        }
    }

    public void SetQuality(int _newQuality)
    {
        if(ZGameOption.Instance.Quality != (E_Quality)_newQuality)
        {
            ZLog.Log(ZLogChannel.UI, $"## OptionSetting ## 게임 프레임&그래픽 설정 | newQuality: {_newQuality}");
            ZGameOption.Instance.SetOption(ZGameOption.OptionKey.Option_Quality, _newQuality);
        }
    }
}
