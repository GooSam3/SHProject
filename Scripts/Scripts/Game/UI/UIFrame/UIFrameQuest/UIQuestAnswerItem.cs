using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuestAnswerItem : CUGUIWidgetBase
{
	[SerializeField] ZText Answer = null;

	private ZButton		mAnswerButton = null;
	private uint			mAnswerID = 0;
	//-----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mAnswerButton = GetComponent<ZButton>();
	}
	//-------------------------------------------------------------
	public void DoQuestAnswerItem(string _answer, bool _enable)
	{
		Answer.text = _answer;
		mAnswerButton.interactable = _enable;
	}

}
