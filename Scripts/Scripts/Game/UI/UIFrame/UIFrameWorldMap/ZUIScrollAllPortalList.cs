using System.Collections.Generic;

public class ZUIScrollAllPortalList : CUGUIScrollRectListBase
{
	private class SPortalInfo
	{
		public uint		StageID      = 0;
		public uint		PortalID	   = 0;
		public string		PortalName  = "None";
		public bool		Favorite = false;
		public ZUIScrollAllPortalListItem ItemInstance = null;
	}

	private uint mSelectedPortalID = 0;
	private List<SPortalInfo> m_listPortal = new List<SPortalInfo>();
	//---------------------------------------------------------
	protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitializePost(_UIFrameParent);
	}

	protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
	{
		ZUIScrollAllPortalListItem Item = _NewItem as ZUIScrollAllPortalListItem;		
		SPortalInfo PortalInfo = m_listPortal[_Index];
		PortalInfo.ItemInstance = Item;
		Item.SetPortalListItem(PortalInfo.PortalID, PortalInfo.PortalName, PortalInfo.Favorite);		
	}

	//---------------------------------------------------------
	public void DoPortalListAdd(uint _StageID, uint _PortalID, string _PortalName, bool _Favorite)
	{
		SPortalInfo PortalList = new SPortalInfo();
		PortalList.StageID = _StageID;
		PortalList.PortalID = _PortalID;
		PortalList.PortalName = _PortalName;
		PortalList.Favorite = _Favorite;
		m_listPortal.Add(PortalList);
	}

	public void DoPortalListClear()
	{
		m_listPortal.Clear();
	}

	public void DoPortalListRefresh()
	{
		ProtUIScrollListInitialize(m_listPortal.Count);
	}

	public void DoPortalListScrollPosition(uint _itemIndex)
	{
		PortalListArrangeScrollPosition(_itemIndex);
	}

	public void DoPortalListFavorite(List<uint> _listFavorite, uint _lastPortalID)
	{
		ResetAllPortalFavorite();

		for (int i = 0; i < _listFavorite.Count; i++)
		{
			PortalListFavorite(_listFavorite[i], true);
		}
	}

	public void DoPortalListFavorite(uint _portalTID, bool _on)
	{
		PortalListFavorite(_portalTID, _on);
	}

	//----------------------------------------------------------
	private void PortalListArrangeScrollPosition(uint _TargetPortalID)
    {
		int TargetIndex = 0;
		for (int i = 0; i < m_listPortal.Count; i++)
        {
			if (m_listPortal[i].PortalID == _TargetPortalID)
            {
				TargetIndex = i;
				break;
            }
        }

		ProtUIScrollListSetPosition(TargetIndex);
	}

	private void ResetAllPortalFavorite()
	{
		for (int i = 0; i < m_listPortal.Count; i++)
		{
			m_listPortal[i].Favorite = false;
			if (m_listPortal[i].ItemInstance)
			{
				m_listPortal[i].ItemInstance.SetPortalFavorite(false);
			}
		}
	}

	private void PortalListFavorite(uint _portalTID, bool _on)
	{
		for (int i= 0; i < m_listPortal.Count; i++)
		{
			if (m_listPortal[i].PortalID == _portalTID)
			{
				m_listPortal[i].Favorite = _on;
				if (m_listPortal[i].ItemInstance)
				{
					if (m_listPortal[i].ItemInstance.GetPortalID() == _portalTID)
					{
						m_listPortal[i].ItemInstance.SetPortalFavorite(_on);
					}
				}
			}
		}
	}

}
