using GameDB;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UIFrameWorldMap : ZUIFrameBase
{
	[System.Serializable]
	private class SWorldMapInfo
	{
		[SerializeField] public string    WorldName = "MainWorld";
		[SerializeField] public string	   ImageName = "None";
		[SerializeField] public int	   WorldIndex = 0;
		[SerializeField] public List<SLocalStageInfo> StageList = new List<SLocalStageInfo>();
	}

	public class SMonsterInfo
	{
		public uint				   MontsetTID = 0;
		public string				   MonsterName;
		public Monster_Table		   MonsterTable = null;
		public List<SMonsterDropItem> ListMonsterDropInfo = new List<SMonsterDropItem>();
	}

	public class SMonsterDropItem
	{
		public enum E_ItemCategory
		{
			StageDrop,		// 스테이지 공통드랍
			MonsterItem,		// 몬스터 고유드랍
			MonsterPetItem	// 펫 전용 드랍 
		}

		public uint			ItemTID = 0;
		public uint			ItemMinCount = 0;
		public uint			ItemMaxCount = 0;
		public E_ItemCategory ItemCategory = E_ItemCategory.StageDrop;
	}

	
	public class SLocalStageInfo
	{
		public uint			  StageID = 0;	
		public UIWorldMapMarker MapMarker; 

		public Stage_Table StageTable = null;
		public List<Portal_Table> ListPortal = new List<Portal_Table>();
		public List<SMonsterInfo> ListMonster = new List<SMonsterInfo>();
		public List<C_WorldMapData> ScrollPortalData = new List<C_WorldMapData>();
	}
	[SerializeField] private ZText						MapTitle = null;
	[SerializeField] private RawImage					ImageWorld = null;	
	[SerializeField] private RawImage					ImageLocal = null;
	[SerializeField] private GameObject					RootWorld = null;
	[SerializeField] private GameObject					RootLocal = null;
    [SerializeField] private ZScrollRect                ScrollLocal = null;
	[SerializeField] private ZUILocalMapMarker			MapMarker = null;
	[SerializeField] private GameObject					PortalListPanel = null;
	[SerializeField] private GameObject					FavoritePanel = null;
	[SerializeField] private UIWorldMapMonsterDrop	    MonsterItemInfoPanel = null;
	[SerializeField] private ZUIWidgetPortalInfo			PortalInfoPanel = null;
	[SerializeField] private ZUIScrollAllPortalList		PortalList = null;
	

	[SerializeField] private ZUIScrollFavoriteList		FavoriteList = null;
	[SerializeField] private UIWorldControlPanel		    ControlPanel = null;
	[SerializeField] private UIWorldMapMarker			WorldMapMarkerTemplate = null;
	[SerializeField] private UIWorldMapAdapter			ScrollWorldMap;
	[SerializeField] private List<SWorldMapInfo> WorldMap = new List<SWorldMapInfo>();

	public UIWorldMapAdapter WorldMapScrollAdapter { get { return ScrollWorldMap; } }

	private List<C_WorldMapData> listWorld = new List<C_WorldMapData>();

	private uint		mCurrentStageID = 0;
	private int		mCurrentWorldIndex = 0;
	private Vector3	mStagePosition = Vector3.zero;
	private SWorldMapInfo mCurrentWorldData = null;
	private LinkedList<uint> m_listFavoritePortalList = new LinkedList<uint>();
	private List<GameObject> m_listPanelInstance = new List<GameObject>();
	public override bool IsBackable => true;
	//---------------------------------------------------------------
	protected override void OnInitialize()
	{
		base.OnInitialize();

		if (GameDBManager.hasInstance)
		{
			InitilizeWorldMap();
			InitializePenel();
			LoadConfigFavoriteList();

			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIWorldMapListItem), obj=>
			{
				ScrollWorldMap.SetEvent(OnClickListSlot);
				ScrollWorldMap.Initialize();

				ZPoolManager.Instance.Return(obj);
			});
		}
		WorldMapMarkerTemplate.SetMonoActive(false);
	}

	protected override void OnRemove()
	{
		base.OnRemove();
		SaveConfigFavoriteList();
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		UIManager.Instance.Open<UISubHUDCharacterState>();
		UIManager.Instance.Open<UISubHUDCurrency>();
	}	

	protected override void OnHide()
	{
		base.OnHide();
		MapMarker.ClearFocus();
		SaveConfigFavoriteList();
	}

	protected override void OnCommandContents(ZCommandUIButton.E_UIButtonCommand _commandID, ZCommandUIButton.E_UIButtonGroup _groupID, int _arguement, CUGUIWidgetBase _commandOwner)
	{
		if (_commandID == ZCommandUIButton.E_UIButtonCommand.Open)
		{
			if (_groupID == (int)ZCommandUIButton.E_UIButtonGroup.Main)
			{
				DoUIWorldMapOpen(mCurrentStageID);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_1)
			{
				DoUIPortalListOpenClose(true);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_2)
			{
				OnClickMapSlot((uint)_arguement);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_3)
			{
				DoUIPortalFavoriteOpenClose(true);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_4)
			{

			}
		} 
		else if (_commandID == ZCommandUIButton.E_UIButtonCommand.OK)
		{
			DoUILocalMapOpen((uint)_arguement);
		}
		else if (_commandID == ZCommandUIButton.E_UIButtonCommand.Close)
		{
			if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_1)
			{
				DoUIPortalListOpenClose(false);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_2)
			{
				DoUIPortalInfoOpenClose(false, 0);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_3)
			{
				DoUIPortalFavoriteOpenClose(false);
			}
			else if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_4)
			{
				DoUIMonsterDropOpenClose(0, 0, false);
			}
		}
		else if (_commandID == ZCommandUIButton.E_UIButtonCommand.ToggleOff)
		{
			if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_1)
			{
				SetWorldMapFavoriteItem((uint)_arguement, true);
			}
		}
		else if (_commandID == ZCommandUIButton.E_UIButtonCommand.ToggleOn)
		{
			if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_1)
			{
				SetWorldMapFavoriteItem((uint)_arguement, false);
			}
		}
	}

    //-----------------------------------------------------------------
    public void DoUILocalMapOpen(uint _openStageID)
    {
        DoUILocalMapOpen(_openStageID,Vector2.one * .5f);
    }

    public void DoUILocalMapOpen(uint _openStageID,Vector2 showPosition)
	{
		mCurrentStageID = _openStageID;
		SLocalStageInfo StageInfo = FindLocalStage(_openStageID);
		if (StageInfo != null)
		{
			LoadWorldMapImage(ImageLocal, StageInfo.StageTable.MapFileName, showPosition);

			ScrollWorldMap.ResetListData(StageInfo.ScrollPortalData);
			ScrollWorldMap.SetSelectItem(null);
			ControlPanel.DoControllWorldMode(UIWorldControlPanel.E_ButtonType.Local);
			FavoritePanel.SetActive(false);
			PortalListPanel.SetActive(false);

			OpenLocalMap(StageInfo, Vector3.zero);
		}
		else
		{
			DoUIWorldMapOpen();
		}

		MapMarker.ClearFocus();
	}

	public void DoUILocalMapOpen()
	{		
		DoUILocalMapOpen(ZGameModeManager.Instance.StageTid);
	}

	public void DoUIWorldMapOpen(uint _CurrentStageID)
	{
		SWorldMapInfo worldMapInfo = FindWorldMap(_CurrentStageID);

		if (worldMapInfo == null && mCurrentWorldIndex < WorldMap.Count)
		{
			worldMapInfo = WorldMap[mCurrentWorldIndex];
		}

		if (worldMapInfo != null)
		{
			if (mCurrentWorldData != worldMapInfo)
			{
				CloseWorldMap(mCurrentWorldData);
			}
		
			OpenWorldMap(worldMapInfo);
			ControlPanel.DoControllWorldMode(UIWorldControlPanel.E_ButtonType.World);
			FavoritePanel.SetActive(false);
			PortalListPanel.SetActive(false);
		}			
	}

	public void DoUIWorldMapOpen()
	{
		DoUIWorldMapOpen(mCurrentStageID);
	}

	public void DoUIPortalListOpenClose(bool _Open)
	{
		if (_Open)
		{
			MapTitle.text = "WorldMap_Portal_List_Title";
			FavoritePanel.SetActive(false);
			ScrollWorldMap.gameObject.SetActive(false);
			PortalListPanel.gameObject.SetActive(true);
			List<uint> listFavorite = m_listFavoritePortalList.ToList();
			if (listFavorite.Count > 0)
			{				
				PortalList.DoPortalListFavorite(listFavorite, 0);
			}
		}
		else
		{
			PortalListPanel.gameObject.SetActive(false);
			if (ControlPanel.GetControlWorldMode() == UIWorldControlPanel.E_ButtonType.World)
			{
				DoUIWorldMapOpen(mCurrentStageID);
			}
			else if (ControlPanel.GetControlWorldMode() == UIWorldControlPanel.E_ButtonType.Local)
			{
				DoUILocalMapOpen(mCurrentStageID);
			}
		}
	}

	public void DoUIPortalInfoOpenClose(bool _Open, uint _targetPortalID)
	{		
		if (_Open)
        {
            bool favorite = m_listFavoritePortalList.Contains(_targetPortalID);
            bool noMonster = false;

            SLocalStageInfo stageInfo = FindStageByPortal(_targetPortalID);
			if (stageInfo != null)
			{
				if (stageInfo.ListMonster.Count == 0)
				{
					noMonster = true;
                }

                //localmap position!
                var newMapSlot = MapMarker.FindMarker(_targetPortalID);

                if (newMapSlot == null)
                {
                    DoUILocalMapOpen(stageInfo.StageID);
                }

                UpdateLocalmapPosition(_targetPortalID);
            }

            SelectPanel(PortalInfoPanel.gameObject);
            PortalInfoPanel.DoPortalInfo(_targetPortalID, favorite, noMonster);
        }
		else
		{
			SelectPanel(null);
		}
    }

    void UpdateLocalmapPosition(uint _targetPortalID)
    {
        var newMapSlot = MapMarker.FindMarker(_targetPortalID);
        
        if (newMapSlot != null)
        {
            var screenRect = ScrollLocal.GetComponent<RectTransform>();

            float posX = newMapSlot.transform.localPosition.x;

            var screenHalfX = screenRect.rect.width / 2f;

            if (posX < (screenRect.rect.width / 2f))//left 
                posX = 0;
            else if (posX > (ScrollLocal.content.sizeDelta.x - screenHalfX))//right
                posX = 1f;
            else
            {
                posX = Mathf.Max(0, posX - screenHalfX);

                posX = posX / (ScrollLocal.content.sizeDelta.x - screenRect.rect.width);
            }

            float posY = newMapSlot.transform.localPosition.y;

            var screenHalfY = screenRect.rect.height / 2f;

            if (posY < screenRect.rect.height / 2f)//down
                posY = 0;
            else if (posY > (ScrollLocal.content.sizeDelta.y - screenHalfY))//up
                posY = 1f;
            else
            {
                posY = Mathf.Max(0, posY - screenHalfY);

                posY = posY / (ScrollLocal.content.sizeDelta.y - screenRect.rect.height);
            }

            localMapshowPosition.x = posX;
            localMapshowPosition.y = posY;

            ScrollLocal.normalizedPosition = localMapshowPosition;// new Vector2(newMapSlot.transform.localPosition.x/ImageLocal.rectTransform.sizeDelta.x, newMapSlot.transform.localPosition.y/ ImageLocal.rectTransform.sizeDelta.y);
        }
    }

	public void DoUIPortalFavoriteOpenClose(bool _open)
    {
		if (_open)
		{
			MapTitle.text = "Potal_Text";
			PortalListPanel.SetActive(false);
			ScrollWorldMap.gameObject.SetActive(false);
			FavoritePanel.gameObject.SetActive(true);
			FavoriteList.DoFavoriteItemList(m_listFavoritePortalList);
		}
		else
		{
			FavoritePanel.gameObject.SetActive(false);
			if (ControlPanel.GetControlWorldMode() == UIWorldControlPanel.E_ButtonType.World)
			{
				DoUIWorldMapOpen(mCurrentStageID);
			}
			else if (ControlPanel.GetControlWorldMode() == UIWorldControlPanel.E_ButtonType.Local)
			{
				DoUILocalMapOpen(mCurrentStageID);
			}			
		}
	}

	public void DoUIMonsterDropOpenClose(uint _portalTID , uint _stageTID, bool _open)
	{
		if (_open)
		{
			SLocalStageInfo stageInfo = FindLocalStage(_stageTID);
			if (stageInfo == null) return;

			SelectPanel(MonsterItemInfoPanel.gameObject);

			Portal_Table portalTable = null;
			for (int i = 0; i < stageInfo.ListPortal.Count; i++)
			{
				if (stageInfo.ListPortal[i].PortalID == _portalTID)
				{
					portalTable = stageInfo.ListPortal[i];
					break;
				}
			}

			if (portalTable != null)
			{
				MonsterItemInfoPanel.DoMonsterDrop(portalTable, stageInfo.ListMonster);
			}
		}
		else
		{
			SelectPanel(null);
		}
	}


	//-----------------------------------------------------------------------
	public void DoMoveLocalMap(uint _PortalID, E_MapMoveType _MoveType)
	{
		Portal_Table PortalTable = null;
		DBPortal.TryGet(_PortalID, out PortalTable);

		if (PortalTable == null) return;

		UIManager.Instance.Close(ID);

		switch(_MoveType)
		{
			case E_MapMoveType.Walk:   // 걷기 자동이동은 구현되지 않았다. 
				MoveToWalk(PortalTable);
				break;
			case E_MapMoveType.Teleport_Danger:
				MoveToTeleportDanger(PortalTable);
				break;
			case E_MapMoveType.Teleport_Safe:
				MoveToTeleportSafe(PortalTable);
				break;
		}
	}

	public void SetWorldMapFavoriteItem(uint _portalTID, bool _Add)
    {
		if (_Add)
        {
			if (m_listFavoritePortalList.Contains(_portalTID) == false)
			{
				m_listFavoritePortalList.AddLast(_portalTID);
				PortalList.DoPortalListFavorite(_portalTID, true);
				PortalInfoPanel.DoPortalInfoFavoriteOnOff(_portalTID, true);
			}
		}
		else
        {
			if (m_listFavoritePortalList.Contains(_portalTID))
            {
				m_listFavoritePortalList.Remove(_portalTID);
				PortalList.DoPortalListFavorite(_portalTID, false);
				PortalInfoPanel.DoPortalInfoFavoriteOnOff(_portalTID, false);
			}
		}
		ScrollWorldMap.RefreshData();
	}

	public bool CheckEnterStage(uint _StageTID)
	{
		if (ZNet.Data.Me.FindCurCharData == null) return false;

		bool CanEnter = true;
		var destStageTable = DBStage.Get(_StageTID);

		if (ZNet.Data.Me.CurCharData.Level != Mathf.Clamp(ZNet.Data.Me.CurCharData.Level, destStageTable.InMinLevel, destStageTable.InMaxLevel))
		{
			CanEnter = false;
		}

		if (!DBStage.IsStageUsable(_StageTID))
		{
			CanEnter = false;
		}

		return CanEnter;
	}

	public bool CheckFavorite(uint _portalID)
	{
		return m_listFavoritePortalList.Contains(_portalID);
	}

    //-----------------------------------------------------------------
	private void OnClickListSlot(C_WorldMapData data)
    {
        switch (data.dataType)
        {
            case C_WorldMapData.E_WorldMapDataType.None:
                break;
            case C_WorldMapData.E_WorldMapDataType.Local:
				//data.localInfo.PortalID
				if (ScrollWorldMap.selectedData != null)
				{
					MapMarker.FindMarker(ScrollWorldMap.selectedData.localInfo.PortalID)?.SetFocus(false);
				}
				ScrollWorldMap.SetSelectItem(data);
				MapMarker.SetFocus(data.localInfo.PortalID);

				DoUIPortalInfoOpenClose(true, data.localInfo.PortalID);

                UpdateLocalmapPosition(data.localInfo.PortalID);
                break;
            case C_WorldMapData.E_WorldMapDataType.World:
				DoUILocalMapOpen(data.worldInfo.StageID);
				break;
        }
    }

	private void OnClickMapSlot(uint portalTid)
    {
		DoUIPortalInfoOpenClose(true, portalTid);

		if(ScrollWorldMap.selectedData !=null)
        {
			MapMarker.FindMarker(ScrollWorldMap.selectedData.localInfo.PortalID)?.SetFocus(false);
		}
		MapMarker.SetFocus(portalTid);

		if (PortalListPanel.activeSelf == true || FavoritePanel.activeSelf == true)
		{
			ControlPanel.SetLocalButtonOn();
			PortalListPanel.SetActive(false);
			FavoritePanel.SetActive(false);
			RootWorld.SetActive(false);
			RootLocal.SetActive(true);

			SLocalStageInfo stageInfo = FindLocalStage(mCurrentStageID);
			MapTitle.text = stageInfo.StageTable.StageTextID;
		}

		ScrollWorldMap.SetSelectItem(FindPortalData(portalTid));
	}

	private C_WorldMapData FindPortalData(uint portalTid)
    {
		var data = ScrollWorldMap.Data.List.Find(item => (item.localInfo != null && item.localInfo.PortalID == portalTid));

		return data;
    }

	//-----------------------------------------------------------------
	private void InitilizeWorldMap()
	{
		for (int i = 0; i < WorldMap.Count; i++)
		{
			InitializeWorldMapStage(WorldMap[i]);
			InitializeWorldPortalList(WorldMap[i]);
		}
	}

	private void InitializeWorldMapStage(SWorldMapInfo _worldMapInfo)
	{
		Dictionary<uint, Stage_Table>.ValueCollection.Enumerator it = DBStage.GetAllStage().GetEnumerator();
		
		while (it.MoveNext())
		{
			if (it.Current.StageType == E_StageType.Field || it.Current.StageType == E_StageType.Town || it.Current.StageType == E_StageType.Tutorial)
			{
				SLocalStageInfo stageInfo = new SLocalStageInfo();
				_worldMapInfo.StageList.Add(stageInfo);

				stageInfo.StageTable = it.Current;
				stageInfo.StageID = stageInfo.StageTable.StageID;
				Vector2 markerPosition = new Vector2();

				if (stageInfo.StageTable.WorldMapPosition.Count > 1)
				{
					markerPosition.x = (float)stageInfo.StageTable.WorldMapPosition[0];
					markerPosition.y = (float)stageInfo.StageTable.WorldMapPosition[1];
				}

				stageInfo.MapMarker = MakeWorldMapMarker(markerPosition);
				listWorld.Add(new C_WorldMapData(stageInfo) { index = _worldMapInfo.StageList.Count });

				//==========================포탈 정보 수집===================================
				Dictionary<uint, Portal_Table>.Enumerator itPortal = GameDBManager.Container.Portal_Table_data.GetEnumerator();

				while (itPortal.MoveNext())
				{
					Portal_Table PortalTable = itPortal.Current.Value;
					if (PortalTable.StageID == stageInfo.StageTable.StageID && PortalTable.PortalType == E_PortalType.LocalMap)
					{
						stageInfo.ListPortal.Add(PortalTable);
						stageInfo.ScrollPortalData.Add(new C_WorldMapData(PortalTable) { index = PortalTable.MapNumber });
					}
				}

				//====================몬스터 정보 수집===================
				InitializeMonsterDropInfo(stageInfo);
			}
		}

		RefreshWorldMapMarker(_worldMapInfo);
	}

	private void InitializeMonsterDropInfo(SLocalStageInfo _localStageInfo)
	{
		List<SMonsterDropItem> listMonsterStageDropItem = new List<SMonsterDropItem>();
		// 공통 드랍 테이블 
		Dictionary<uint, StageDrop_Table>.Enumerator itStageDrop = GameDBManager.Container.StageDrop_Table_data.GetEnumerator();
		while(itStageDrop.MoveNext())
		{
			StageDrop_Table stageDropTable = itStageDrop.Current.Value;
			if (stageDropTable.DropGroupID == _localStageInfo.StageTable.StageDropGroupID)
			{
				uint ListGroupID = stageDropTable.DropListGroupID;
				Dictionary<uint, StageDropList_Table>.Enumerator itStageDropListTable = GameDBManager.Container.StageDropList_Table_data.GetEnumerator();
				while (itStageDropListTable.MoveNext())
				{
					StageDropList_Table stageDropList = itStageDropListTable.Current.Value;
					if (ListGroupID == stageDropList.ListGroupID)
					{
						SMonsterDropItem dropItem = new SMonsterDropItem();
						dropItem.ItemTID = stageDropList.DropItemID;
						dropItem.ItemMinCount = stageDropList.DropItemCount;
						dropItem.ItemMaxCount = stageDropList.DropItemCount;
						dropItem.ItemCategory = SMonsterDropItem.E_ItemCategory.StageDrop;
						listMonsterStageDropItem.Add(dropItem);
					}
				}
			}
		}

		Dictionary<uint, Monster_Table>.Enumerator itMonsterTable = GameDBManager.Container.Monster_Table_data.GetEnumerator();
		while (itMonsterTable.MoveNext())
		{
			Monster_Table monsterTable = itMonsterTable.Current.Value;
			uint stageTID = monsterTable.PlaceStageID;
			if (stageTID == _localStageInfo.StageID)
			{
				SMonsterInfo monsterInfo = new SMonsterInfo();
				monsterInfo.MontsetTID = monsterTable.MonsterID;
				monsterInfo.MonsterName = monsterTable.MonsterTextID;
				monsterInfo.MonsterTable = monsterTable;
				_localStageInfo.ListMonster.Add(monsterInfo);
				InitializeMonsterDropItem(monsterTable.DropGroupID, monsterInfo.ListMonsterDropInfo);

				for (int i = 0; i < listMonsterStageDropItem.Count; i++)
				{
					monsterInfo.ListMonsterDropInfo.Add(listMonsterStageDropItem[i]);
				}
			}
		}
	}

	private void InitializeMonsterDropItem(uint _monsterDropGroupID, List<SMonsterDropItem> _dropItemList)
	{
		Dictionary<uint, MonsterDrop_Table>.Enumerator it = GameDBManager.Container.MonsterDrop_Table_data.GetEnumerator();
		while (it.MoveNext())
		{
			MonsterDrop_Table monsterDropTable = it.Current.Value;
			if (monsterDropTable.DropGroupID == _monsterDropGroupID)
			{
				if (monsterDropTable.DropItemID == 0 || monsterDropTable.DropItemType == E_DropItemType.Gold)
				{

					// 기획이 변경되어 팻 장식은 출력하지 않음
					//InitializeMonsterDropPetItem(monsterDropTable.DropItemGroupID, _dropItemList);
				}
				else
				{
					SMonsterDropItem dropItem = new SMonsterDropItem();
					dropItem.ItemTID = monsterDropTable.DropItemID;
					dropItem.ItemMinCount = monsterDropTable.DropItemMinCount;
					dropItem.ItemMaxCount = monsterDropTable.DropItemMaxCount == 0 ? monsterDropTable.DropItemMinCount : monsterDropTable.DropItemMaxCount;
					dropItem.ItemCategory = SMonsterDropItem.E_ItemCategory.MonsterItem;
					_dropItemList.Add(dropItem);
				}
			}
		}
	}

	private void InitializeMonsterDropPetItem(uint _dropGroupID, List<SMonsterDropItem> _dropItemList)
	{
		Dictionary<uint, DropGroup_Table>.Enumerator it = GameDBManager.Container.DropGroup_Table_data.GetEnumerator();
		while(it.MoveNext())
		{
			DropGroup_Table dropGroup = it.Current.Value;
			if (_dropGroupID == dropGroup.DropGroupID)
			{
				SMonsterDropItem dropItem = new SMonsterDropItem();
				dropItem.ItemTID = dropGroup.DropItemID;
				dropItem.ItemMinCount = dropGroup.DropItemCnt;
				dropItem.ItemMaxCount = dropGroup.DropItemCnt;
				dropItem.ItemCategory = SMonsterDropItem.E_ItemCategory.MonsterPetItem;
				_dropItemList.Add(dropItem);
			}
		}
	}

	private void InitializeWorldPortalList(SWorldMapInfo _WorldMapInfo)
	{
		PortalList.DoPortalListClear();
		
		Dictionary<uint, Portal_Table>.Enumerator it = GameDBManager.Container.Portal_Table_data.GetEnumerator();
		SortedList<uint, Portal_Table> listSorted = new SortedList<uint, Portal_Table>();
		while (it.MoveNext())
		{
			Portal_Table portalTable = it.Current.Value;
			if (portalTable.PortalType == E_PortalType.LocalMap)
			{
				listSorted.Add(portalTable.MapNumber, portalTable);								
			}
		}

		for (int i = 0; i < listSorted.Values.Count; i++)
		{
			PortalList.DoPortalListAdd(listSorted.Values[i].StageID, listSorted.Values[i].PortalID, ConvertPortalName(listSorted.Values[i]), false);
		}

		PortalList.DoPortalListRefresh();
	}

	private void InitializePenel()
	{
		m_listPanelInstance.Clear();	
		m_listPanelInstance.Add(PortalInfoPanel.gameObject);		
		m_listPanelInstance.Add(MonsterItemInfoPanel.gameObject);

		for (int i = 0; i < m_listPanelInstance.Count; i++)
		{
			m_listPanelInstance[i].SetActive(false);
		}
	}

	//------------------------------------------------------------------
	
	private void OpenLocalMap(SLocalStageInfo _LocalStage, Vector3 _playerPosition)
	{
		DoUIPortalInfoOpenClose(false, 0);

		RootWorld.SetActive(false);
		RootLocal.SetActive(true);

		MapTitle.text = _LocalStage.StageTable.StageTextID;
		MapMarker.DoMapMarkerClear();
		for (int i = 0; i < _LocalStage.ListPortal.Count; i++)
		{
			OpenLocalMapMarker(_LocalStage.ListPortal[i]);
		}

		if (_playerPosition != Vector3.zero)
		{
			// Show My Local Position

		}
	}

	private void OpenLocalMapMarker(Portal_Table _PortalInfo)
	{
		if (_PortalInfo.LocalMapPosition.Count == 0) return;

		Vector2Int Position = new Vector2Int((int)_PortalInfo.LocalMapPosition[0], (int)_PortalInfo.LocalMapPosition[1]);
		MapMarker.DoMapMarkerLocate(Position, _PortalInfo.PortalID, _PortalInfo.ItemTextID, _PortalInfo.PortalType);
	}

	private void OpenWorldMap(SWorldMapInfo _WorldMapInfo)
	{
		DoUIPortalInfoOpenClose(false, 0);
		mCurrentWorldData = _WorldMapInfo;
		mCurrentWorldIndex = _WorldMapInfo.WorldIndex;
		SelectPanel(null);
		MapTitle.text = DBLocale.GetText("WorldMap_Title_MainContinent");

		RootWorld.SetActive(true);
		RootLocal.SetActive(false);
		
		ScrollWorldMap.ResetListData(listWorld);
		ScrollWorldMap.SetSelectItem(null);

		LoadWorldMapImage(ImageWorld, _WorldMapInfo.ImageName, Vector2.one * .5f);
		RefreshWorldMapMarker(_WorldMapInfo);
	}

	private void CloseWorldMap(SWorldMapInfo _WorldMapInfo)
	{
		if (_WorldMapInfo == null) return;
	}

	//----------------------------------------------------------------------------
	private void MoveToWalk(Portal_Table _destPortal)
	{
		UIManager.Instance.Close<UIFrameWorldMap>();		
	}

	private void MoveToTeleportDanger(Portal_Table _destPortal)
	{
		UIManager.Instance.Close<UIFrameWorldMap>();
		UIManager.Instance.Open<UIPopupStageMove>((_name, _uiFrame) => { _uiFrame.DoUIStageMove(UIPopupStageMove.E_ChannelType.Chaos, _destPortal.PortalID);});
	}

	private void MoveToTeleportSafe(Portal_Table _destPortal)
	{
		var item = ZNet.Data.Me.CurCharData.GetItem(_destPortal.UseItemID);
		ZGameManager.Instance.TryEnterStage(_destPortal.PortalID, false, item?.item_id??0, _destPortal.UseItemID);
	}

    //-----------------------------------------------------------------------------
    Vector2 localMapshowPosition = Vector2.zero;

    private void LoadWorldMapImage(RawImage _TargetImage, string _ImageName, Vector2 showPosition = default)
	{
        localMapshowPosition = showPosition;

        if (_TargetImage.texture == null || _TargetImage.texture.name != _ImageName)
		{
			if (_TargetImage.texture != null)
			{
				Addressables.Release<Texture>(_TargetImage.texture);
			}

			Addressables.LoadAssetAsync<Texture>(_ImageName).Completed += (AsyncOperationHandle<Texture> _Result) =>
			{
				if (_Result.Status == AsyncOperationStatus.Succeeded)
				{
					_TargetImage.texture = _Result.Result;
                    ScrollLocal.normalizedPosition = localMapshowPosition;

                    //_TargetImage.rectTransform.anchoredPosition = localMapshowPosition;
				}
			};
		}
		else
		{
            ScrollLocal.normalizedPosition = localMapshowPosition;
            //_TargetImage.rectTransform.anchoredPosition = localMapshowPosition;
        }
	}

	private void RefreshWorldMapMarker(SWorldMapInfo _worldMap)
	{
		for (int i = 0; i < _worldMap.StageList.Count; i++)
		{
			SLocalStageInfo stageInfo = _worldMap.StageList[i];
			bool canEnter = CheckEnterStage(stageInfo.StageID);
			canEnter = true;
			bool selectMarker = false;
			if (mCurrentStageID == stageInfo.StageID)
			{
				selectMarker = true;
			}

			string stageName = DBLocale.GetText(stageInfo.StageTable.StageTextID);

			if (stageInfo.MapMarker != null)
			{
				stageInfo.MapMarker.DoMapMarkerRefresh(selectMarker, canEnter, stageInfo.StageID, stageName, i + 1);
			}
		}
	}

	//-----------------------------------------------------------------------------

	private SLocalStageInfo FindLocalStage(uint _StageID)
	{
		SLocalStageInfo FindStage = null;

		for (int w = 0; w < WorldMap.Count; w++)
		{
			for (int i = 0; i < WorldMap[w].StageList.Count; i++)
			{
				if (WorldMap[w].StageList[i].StageID == _StageID)
				{
					FindStage = WorldMap[w].StageList[i];
					break;
				}
			}
		}

		return FindStage;
	}

	private SWorldMapInfo FindWorldMap(uint _StageID)
	{
		SWorldMapInfo FindWorldMap = null;

		for (int i = 0; i < WorldMap.Count; i++)
		{
			for (int j = 0; j < WorldMap[i].StageList.Count; j++)
			{
				if (WorldMap[i].StageList[j].StageID == _StageID)
				{
					FindWorldMap = WorldMap[i];
					break;
				}
			}
		}

		return FindWorldMap;
	}

	private SLocalStageInfo FindStage(uint _stageID)
	{
		SLocalStageInfo Find = null;
		for (int i = 0; i < WorldMap.Count; i++)
		{
			for (int j = 0; j < WorldMap[i].StageList.Count; j++)
			{
				if (WorldMap[i].StageList[j].StageID == _stageID)
				{
					Find = WorldMap[i].StageList[j];
					break;
				}
			}
		}
		return Find;
	}

	private SLocalStageInfo FindStageByPortal(uint _portalID)
	{
		SLocalStageInfo Find = null;
		for (int i = 0; i < WorldMap.Count; i++)
		{
			for (int j = 0; j < WorldMap[i].StageList.Count; j++)
			{
				for (int z = 0; z < WorldMap[i].StageList[j].ListPortal.Count; z++)
				{
					if (WorldMap[i].StageList[j].ListPortal[z].PortalID == _portalID)
					{
						Find = WorldMap[i].StageList[j];
						break;
					}
				}
			}
		}
		return Find;
	}

	private void SelectPanel(GameObject _select)
	{
		for (int i = 0; i < m_listPanelInstance.Count; i++)
		{
			if (m_listPanelInstance[i] == _select)
			{
				m_listPanelInstance[i].SetActive(true);
			}
			else
			{
				m_listPanelInstance[i].SetActive(false);
			}
		}
	}

	private UIWorldMapMarker MakeWorldMapMarker(Vector2 _position)
	{
		UIWorldMapMarker newMarker = Instantiate(WorldMapMarkerTemplate, WorldMapMarkerTemplate.transform.parent);
		AddUIWidget(newMarker);
		newMarker.transform.localPosition = _position;
		newMarker.SetMonoActive(true);
		return newMarker;
	}
	//------------------------------------------------------------------
	private void LoadConfigFavoriteList()
	{
		string FavoriteList = PlayerPrefs.GetString("UIWorldMap_Favorite");
		if (FavoriteList.Length != 0)
		{
			ParseFavoriteList(FavoriteList, m_listFavoritePortalList);
		}
	}

	private void SaveConfigFavoriteList()
	{
		string FavoriteText = WriteFavoriteList(m_listFavoritePortalList);
		if (FavoriteText.Length != 0)
		{
			PlayerPrefs.SetString("UIWorldMap_Favorite", FavoriteText);
		}
		else
		{
			PlayerPrefs.SetString("UIWorldMap_Favorite", "");
		}
	}

	private void ParseFavoriteList(string _favoriteList, LinkedList<uint> _listFavorite)
	{
		StringBuilder Note = new StringBuilder();
		_listFavorite.Clear();

		for(int i = 0; i < _favoriteList.Length; i++)
		{
			if (_favoriteList[i] == ',')
			{
				uint PortalTID = 0;
				uint.TryParse(Note.ToString(), out PortalTID);
				if (PortalTID != 0)
				{
					if (DBPortal.Get(PortalTID) != null)
					{
						_listFavorite.AddLast(PortalTID);
					}
				}
				Note.Clear();
			}
			else
			{
				Note.Append(_favoriteList[i]);
			}
		}
	}

	private string WriteFavoriteList(LinkedList<uint> _listFavorite)
	{
		StringBuilder Note = new StringBuilder();
		LinkedList<uint>.Enumerator it = _listFavorite.GetEnumerator();
		while(it.MoveNext())
		{
			Note.AppendFormat("{0},", it.Current.ToString());
		}

		return Note.ToString();
	}

    //------------------------------------------------------------------
	public static string ConvertPortalName(Portal_Table _portalTable)
	{
		return string.Format("{0}. {1}", _portalTable.MapNumber.ToString(), DBLocale.GetText(_portalTable.ItemTextID));
	}
}
