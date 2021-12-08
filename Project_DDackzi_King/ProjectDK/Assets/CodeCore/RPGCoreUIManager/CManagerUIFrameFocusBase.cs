using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CManagerUIFrameFocusBase : CManagerUIFrameBase
{
	public enum EUIFrameFocusType
	{
		None,
		Invisible,		  // �׻� ��������. ������ �������� �ʴ´�.
		ToolTip,           // �׻� Topmost �� �迭�ȴ�. ������ �ϳ��� �����Ѵ�. 
		Panel,             // Show ������ ���� ���ĵǾ� ��µȴ�.
		FullScreenPanel,    // Show�� ���  Panel�� Hide ��Ų��.		
		Popup,             // Panel���� �׻� ���� �׷�����.   
		PopupExclusive,	  // Popup�� ��� Hide��Ų��. 
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
