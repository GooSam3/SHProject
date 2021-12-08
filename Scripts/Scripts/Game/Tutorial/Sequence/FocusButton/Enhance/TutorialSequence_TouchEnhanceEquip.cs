using UnityEngine;
using UnityEngine.UI;

/// <summary> 장비 강화 팝업에서 강화 버튼을 터치한다. </summary>
public class TutorialSequence_TouchEnhanceEquip : TutorialSequence_FocusButton<UIFrameItemEnhance>
{
	protected override string Path { get { return "ItemEnhance_Popup/Body/Bottom/Bt_Enhance"; } }


	protected override bool Check()
	{
		return true;
	}
}