using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uTools;

public class SHUISlotMessageStack : CUIWidgetTemplateItemBase
{
	[SerializeField]
	private CText Message = null;
	[SerializeField]
	private uTweenAlpha TweenAlpha = null;

	private UnityAction<SHUISlotMessageStack> m_delMessageStack = null;
	//----------------------------------------------------
	public void DoSlotMessageStack(string strMessage, UnityAction<SHUISlotMessageStack> delFinish)
	{
		SetMonoActive(true);
		Message.text = strMessage;
		m_delMessageStack = delFinish;
		TweenAlpha.ResetPlay();
	}

	//---------------------------------------------------
	public void HandleMessageStackEnd()
	{
		m_delMessageStack?.Invoke(this);
		SetMonoActive(false);
	}
}
