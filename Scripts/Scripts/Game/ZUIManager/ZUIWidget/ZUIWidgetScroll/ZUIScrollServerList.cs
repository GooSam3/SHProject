using System.Collections.Generic;

public class ZUIScrollServerList : CUGUIScrollRectListBase
{

	private List<UIPopupServerSelect.SServerInfomation> m_listServerInfo = new List<UIPopupServerSelect.SServerInfomation>();
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitializePost(_UIFrameParent);
		
		for (int i = 0; i < 40; i++)
		{
			UIPopupServerSelect.SServerInfomation Dummy = new UIPopupServerSelect.SServerInfomation();
			Dummy.ServerName = string.Format("Test Item {0:D3}", i);
			DoServerListAdd(Dummy);
		}
		DoServerListRefresh();
	} 

	protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
	{
		if (_Index < m_listServerInfo.Count && _Index >= 0)
		{
			ZUISlotItemServerSelect ServerSelectItem = _NewItem as ZUISlotItemServerSelect;
			ServerSelectItem.DoServerSelectInfo(m_listServerInfo[_Index]);
		}
	}


	//--------------------------------------------------------------
	public void DoServerListAdd(UIPopupServerSelect.SServerInfomation _ServerInfo)
	{
		m_listServerInfo.Add(_ServerInfo);
	}

	public void DoServerListRefresh()
	{
		ProtUIScrollListInitialize(m_listServerInfo.Count);
	}


}
