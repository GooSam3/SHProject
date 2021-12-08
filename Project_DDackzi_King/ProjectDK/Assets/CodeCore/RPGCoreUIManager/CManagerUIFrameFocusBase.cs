using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CManagerUIFrameFocusBase : CManagerUIFrameBase
{
	public enum EUIFrameFocusType
	{
		None,
		Invisible,		  // 항상 보여진다. 별도로 관리되지 않는다.
		ToolTip,           // 항상 Topmost 로 배열된다. 툴팁은 하나만 존재한다. 
		Panel,             // Show 순서에 따라 정렬되어 출력된다.
		FullScreenPanel,    // Show시 모든  Panel을 Hide 시킨다.		
		Popup,             // Panel보다 항상 위에 그려진다.   
		PopupExclusive,	  // Popup을 모두 Hide시킨다. 
	}

	private LinkedList<CUIFrameBase> m_listUIOrderFrame = new LinkedList<CUIFrameBase>();
	private LinkedList<CUIFrameBase> m_listUIOrderPopup = new LinkedList<CUIFrameBase>();

	//--------------------------------------------------------------


	//--------------------------------------------------------------
	protected void ProtMgrUIFrameFocusShow(string strFrameName)
	{
		CUIFrameBase pUIFrame = FindUIFrame(strFrameName);
		if (pUIFrame != null)
		{
			EUIFrameFocusType eFocusType = pUIFrame.GetUIFrameFocusType();
			switch(eFocusType)
			{
				case EUIFrameFocusType.Invisible:
					break;
				case EUIFrameFocusType.ToolTip:
					break;
				case EUIFrameFocusType.Panel:
					PrivMgrUIFrameFocusPanel(pUIFrame);
					break;
				case EUIFrameFocusType.FullScreenPanel:
					break;
				case EUIFrameFocusType.Popup:
					break;
				case EUIFrameFocusType.PopupExclusive:
					break;
			}
		}
		
	}

	//---------------------------------------------------------------
	private void PrivMgrUIFrameFocusPanel(CUIFrameBase pUIFrame)
	{
		int iOrder = ExtractUIOrderPanel();
		pUIFrame.ImportUIFrameShow(iOrder);
		m_listUIOrderFrame.AddLast(pUIFrame);
		OnMgrUIFrameShow(pUIFrame);
	}





	private int ExtractUIOrderPanel()
	{
		return m_listUIOrderFrame.Count * 10;
	}
	//------------------------------------------------------------------
	protected virtual void OnMgrUIFrameShow(CUIFrameBase pUIFrame) { }	
}
