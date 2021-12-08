using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetMessageStack : CUIWidgetTemplate
{
	[SerializeField]
	private int MaxMessageStock = 3;

	private LinkedList<SHUISlotMessageStack> m_listSlotMessageStack = new LinkedList<SHUISlotMessageStack>();
	//----------------------------------------------------------------
	public void DoMessageStack(string strMessage)
	{
		if (m_listSlotMessageStack.Count < MaxMessageStock)
		{
			PrivMessageStackMake(strMessage);
		}
		else
		{
			PrivMessageStackRemove();
			PrivMessageStackMake(strMessage);
		}
	}

	//-------------------------------------------------------------------
	private void PrivMessageStackMake(string strMessage)
	{
		SHUISlotMessageStack pNewMessage = DoTemplateRequestItem(this.transform) as SHUISlotMessageStack;
		pNewMessage.DoSlotMessageStack(strMessage, HandleMessageStackEnd);
		m_listSlotMessageStack.AddLast(pNewMessage);
	}

	private void PrivMessageStackRemove()
	{
		SHUISlotMessageStack pRemoveMessage = m_listSlotMessageStack.First.Value;
		DoTemplateReturn(pRemoveMessage);
		m_listSlotMessageStack.RemoveFirst();
	}

	//---------------------------------------------------------------------
	public void HandleMessageStackEnd(SHUISlotMessageStack pMessageStack)
	{
		m_listSlotMessageStack.Remove(pMessageStack);
		DoTemplateReturn(pMessageStack);
	}
}
