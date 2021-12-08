using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameMessageNotify : SHUIFrameBase
{
	[SerializeField]
	private SHUIWidgetMessageStack MessageStack = null;
	[SerializeField]
	private SHUIWidgetMessageFlow MessageFlow = null;

	//------------------------------------------------------------------------------
	public void DoUIFrameMessageStack(string strMessage)
	{
		MessageStack.DoMessageStack(strMessage);
	}

	public void DoUIFrameMessageFlow(string strMessage)
	{
		MessageFlow.DoMessageFlow(strMessage);
	}
}
