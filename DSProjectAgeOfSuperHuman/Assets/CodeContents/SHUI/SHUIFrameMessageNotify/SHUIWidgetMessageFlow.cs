using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetMessageFlow : CUIWidgetBase
{
	[System.Serializable]
	private class SMessageFlowInfo
	{
		public string Message;
	}
	

	[SerializeField]
	private SHUISlotMessageFlow MessageFirst = null;
	[SerializeField]
	private SHUISlotMessageFlow MessageSecond = null;

	private Queue<SMessageFlowInfo> m_queMessageFlow = new Queue<SMessageFlowInfo>();
	//-----------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		MessageFirst.SetMonoActive(false);
		MessageSecond.SetMonoActive(false);
	}

	//-----------------------------------------------------------------------
	public void DoMessageFlow(string strMessageFlow)
	{

	}

	//-----------------------------------------------------------------------
	public void HandleMessageFlowEnd()
	{

	}
}
