using UnityEngine;
using GameDB;
using System.Collections.Generic;

public class ZUIScrollFavoriteList : CUGUIScrollRectListBase 
{
	private class SFavoriteInfo
	{
		public uint    PortalTID = 0;
		public string  PortalName;
		public int	  ListIndex = 0;
		public ZUIScrollFavoriteListItem Item = null;
	}

	[SerializeField] ZText PopupTitle = null;

	private List<SFavoriteInfo> m_listFavorite = new List<SFavoriteInfo>();
	private uint mSelectedPortalTID = 0;
	private UIFrameWorldMap mWorldMap = null;
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mWorldMap = _UIFrameParent as UIFrameWorldMap;
	}

	protected override void OnUIScrollRectListRefreshItem(int _Index, CUGUIWidgetSlotItemBase _NewItem)
	{
		ZUIScrollFavoriteListItem Item = _NewItem as ZUIScrollFavoriteListItem;
		SFavoriteInfo Info = m_listFavorite[_Index];
		Info.ListIndex = _Index;
		Info.Item = Item;
		Item.DoFavoriteListItem(Info.PortalTID, Info.PortalName);
	}

	//---------------------------------------------------------------
	public void DoFavoriteItemList(LinkedList<uint> _listFavorite)
	{
		PopupTitle.text = DBLocale.GetText("WorldMap_Title_Favorite");

		m_listFavorite.Clear();
		LinkedList<uint>.Enumerator it = _listFavorite.GetEnumerator();
		SortedList<uint, SFavoriteInfo> listSorted = new SortedList<uint, SFavoriteInfo>();
		while (it.MoveNext())
		{
			Portal_Table portalTable;
			DBPortal.TryGet(it.Current, out portalTable);

			if (portalTable == null) continue;

			SFavoriteInfo Favorite = new SFavoriteInfo();
			Favorite.PortalTID = portalTable.PortalID;
			Favorite.PortalName = UIFrameWorldMap.ConvertPortalName(portalTable);
			listSorted.Add(portalTable.MapNumber, Favorite);
		}

		for (int i = 0; i < listSorted.Values.Count; i++)
		{
			m_listFavorite.Add(listSorted.Values[i]);
		}

		ProtUIScrollListInitialize(m_listFavorite.Count, false);
	}

	//---------------------------------------------------------------
	public void HandleFavoriteItemSelect(ZUIScrollFavoriteListItem _item)
	{
		mSelectedPortalTID = _item.pPortalTID;
	}

	public void HandleFavoriteItemDelete()
	{
		if (mSelectedPortalTID == 0) return;

		RemoveFavoriteInfo(mSelectedPortalTID);
		ProtUIScrollListInitialize(m_listFavorite.Count, false); // 초기화를 해주면 그만큼 줄어든 스크롤을 생성한다.
		mWorldMap.SetWorldMapFavoriteItem(mSelectedPortalTID, false);
		mSelectedPortalTID = 0;
	}

	//---------------------------------------------------------------
	private SFavoriteInfo RemoveFavoriteInfo(uint _portalTID)
	{
		SFavoriteInfo Info = FindFavoriteInfo(_portalTID);
		if (Info != null)
		{
			m_listFavorite.Remove(Info);
		}
		
		return Info;
	}

	private SFavoriteInfo FindFavoriteInfo(uint _portalTID)
	{
		SFavoriteInfo Info = null;

		for (int i = 0; i < m_listFavorite.Count; i++)
		{
			if (m_listFavorite[i].PortalTID == _portalTID)
			{
				Info = m_listFavorite[i];
				break;
			}
		}

		return Info;
	}
}
