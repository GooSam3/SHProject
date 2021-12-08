using UnityEngine.UI;
using UnityEngine;

/// <summary> 아이템 정보창 (현재 인벤토리 전용) </summary>
public class TutorialSequence_ItemInfoPopup : TutorialSequence_FocusButton<UIFrameInventory>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return TutorialTable.GuideParams[0]; } }

	protected override Selectable GetSelectable()
	{
		if(null == OwnerUI.InfoPopup)
			return null;

		if (false == OwnerUI.InfoPopup.gameObject.activeInHierarchy)
			return null;

		return OwnerUI.InfoPopup.gameObject.FindChildComponent<Selectable>(Path);
	}

	protected override Transform GetHighlightObject()
	{
		return OwnerUI.InfoPopup.gameObject.FindChildComponent<Selectable>(HighlightPath)?.transform;
	}

	/// <summary> 사용 가능한지 확인해야함 </summary>
	protected override bool Check()
	{
		return true;
	}
}