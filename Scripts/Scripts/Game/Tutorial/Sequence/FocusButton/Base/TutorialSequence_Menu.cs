using UnityEngine.UI;
using UnityEngine;

/// <summary> hud 상단 메뉴버튼 터치 </summary>
public abstract class TutorialSequence_Menu : TutorialSequence_FocusButton<UISubHUDMenu>
{
	/// <summary> 버튼의 이름 </summary>
	protected string ButtonName { get { return TutorialTable.GuideParams[0]; } }

	protected override Selectable GetSelectable()
	{
		return OwnerUI.transform.Find(Path).gameObject.FindChildComponent<Selectable>(ButtonName, true);
	}

	protected override Transform GetHighlightObject()
	{
		return mSelectable.transform;;
	}

	protected override bool Check()
	{
		return true;
	}
}