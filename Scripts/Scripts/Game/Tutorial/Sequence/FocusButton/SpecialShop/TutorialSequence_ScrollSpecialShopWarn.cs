using UnityEngine.UI;

/// <summary> 청약 철회 스크롤 (일단 터치로 작업) </summary>
public class TutorialSequence_ScrollSpecialShopWarn : TutorialSequence_Focus<UIFrameSpecialShop>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "SpecialShop_Panel/Body/Overlay/SpecialItem_Notice_Popup/Body"; } }
}