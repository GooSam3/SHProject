using UnityEngine;
using UnityEngine.UI;

/// <summary> 장비 강화 팝업에서 재료 선택을 터치한다. </summary>
public class TutorialSequence_TouchEnhanceMaterial : TutorialSequence_FocusButton<UIFrameItemEnhance>
{
	protected override string Path { get { return "ItemEnhance_Popup/Body/Middle/Enhance_Item/MaterialSlot/UIItemSlot"; } }


	protected override bool Check()
	{
		return true;
	}
}