using UnityEngine.UI;
using UnityEngine;

/// <summary> 자세히 보기 버튼 터치 </summary>
public class TutorialSequence_TouchSpecialShopWarn : TutorialSequence_FocusButton<UIFrameSpecialShop>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Bt_Detail"; } }

	protected override Selectable GetSelectable()
	{
		if (null == OwnerUI.infoPopUp)
			return null;

		if (false == OwnerUI.infoPopUp.gameObject.activeInHierarchy)
			return null;

		return OwnerUI.infoPopUp.gameObject.FindChildComponent<Selectable>(Path);
	}

	protected override Transform GetHighlightObject()
	{
		return mSelectable.transform;
	}

	protected override bool Check()
	{
		return true;
	}
}