using UnityEngine.UI;

/// <summary> 청약 철회 닫기 </summary>
public class TutorialSequence_TouchSpecialShopWarnClose : TutorialSequence_FocusButton<UIFrameSpecialShop>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "SpecialShop_Panel/Body/Overlay/SpecialItem_Notice_Popup/Body/Bottom/Bt_Confirm"; } }

	protected override bool Check()
	{
		return true;
	}
}