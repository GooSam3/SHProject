using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
public class SHUISlotMessageFlow : CUIWidgetBase
{
	private CText m_pFlowText = null; 
	private uTweenPosition m_pTweenPosition = null;
	//--------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pTweenPosition = GetComponent<uTweenPosition>();
		m_pTweenPosition.enabled = false;
	}

	//-------------------------------------------------------------------


	//-------------------------------------------------------------------
	public void HandleSlotMessageEnd()
	{
		SetMonoActive(false);
	}
}
