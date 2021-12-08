using UnityEngine;
using UnityEngine.UI;

/// <summary> 장비 강화 팝업에서 닫기 버튼을 클릭한다. </summary>
public class TutorialSequence_TouchEnhanceClose : TutorialSequence_FocusButton<UIFrameItemEnhance>
{
	protected override string Path { get { return "ItemEnhance_Popup/Body/Board_Popup_OnlyBg/Bt_X"; } }

	protected override float StartDelayTime => 3f;

	protected override bool Check()
	{
		return true;
	}
}