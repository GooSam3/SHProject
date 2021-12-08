using System.Collections.Generic;
public class UIWorldScrollAllLocalMap : CUGUIScrollRectListBase
{
	public class SLocalStageInfo
	{
		public string LocalStageName;
		public uint   StageTID; 
		public bool   CanEnter = true;
		public bool   Selected = false;
		public UIWorldScrollAllLocalMapItem UIInstance = null;
	}

	private List<SLocalStageInfo> m_listTableInfo = new List<SLocalStageInfo>();
	//-------------------------------------------------------------
	public void DoScrollAddStage(string _stageName, uint _stageTID, bool _canEnter, bool _selected, int _index)
	{
		SLocalStageInfo localStageInfo = FindOrAlloc(_stageTID);
		localStageInfo.CanEnter = _canEnter;
		localStageInfo.Selected = _selected;
		localStageInfo.LocalStageName = $"{_index}.{_stageName}";
	}

	public void DoScrollAllRefresh()
	{
		ProtUIScrollListInitialize(m_listTableInfo.Count);
	}

	//----------------------------------------------------------------
	protected override void OnUIScrollRectListRefreshItem(int _index, CUGUIWidgetSlotItemBase _newItem)
	{
		UIWorldScrollAllLocalMapItem item = _newItem as UIWorldScrollAllLocalMapItem;
		SLocalStageInfo localStageInfo = m_listTableInfo[_index];
		localStageInfo.UIInstance = item;
		item.DoAllLocalMapItem(localStageInfo);
	}


	//-------------------------------------------------------------
	private SLocalStageInfo FindOrAlloc(uint _stageTID)
	{
		SLocalStageInfo findMapInfo = null;
		for(int i = 0; i < m_listTableInfo.Count; i++)
		{
			if (m_listTableInfo[i].StageTID == _stageTID)
			{
				findMapInfo = m_listTableInfo[i];
				break;
			}
		}

		if (findMapInfo == null)
		{
			findMapInfo = new SLocalStageInfo();
			m_listTableInfo.Add(findMapInfo);
			findMapInfo.StageTID = _stageTID;			
		}

		return findMapInfo;
	}
}
