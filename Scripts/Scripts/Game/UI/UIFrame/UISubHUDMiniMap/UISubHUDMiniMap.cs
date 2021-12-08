using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UISubHUDMiniMap : ZUIFrameBase
{
	public enum E_MapMarkerType
	{
		None,
		MyPlayer,
		PartyPlayer,
		Player,
		EnemyPlayer,
		MonsterNormal,
		MonsterElite,		
		Gimmick,
		NPC_Store,
		NPC_Storage,
		NPC_Trade,
		NPC_Cleric,
		NPC_InterPortal,
		NPC_Quest,
		NPC_SkillStore,
		NPC_Raid,
	}

	protected class SMapMarkerInfo
	{
		public E_MapMarkerType		MarkerType;
		public uint				PawnTID;
		public bool				IsNPC;
		public ZPawn				FollowPawn;
		public Transform			FollowTransform;
		public RectTransform		MarkerTransform;
	}
	[Serializable]
	protected class SMarkerTemplate
	{
		public E_MapMarkerType		MarkerType = E_MapMarkerType.None;
		public RectTransform	    Template = null;
	}
	[Serializable]
	protected class SMinimapInfo
	{
		public uint			StageID = 0;
		public Vector2		StageSize = Vector2.zero;
		public Vector2		StageScale = Vector2.one;
		public Vector2		StageOffset = Vector2.zero;
	}

	[SerializeField] private float		UpdateDalay = 0.06f;
	[SerializeField] private Graphic	InputBlock; 
	[SerializeField] private ZText		PvPTypeName;
	[SerializeField] private ZText		StageName;
	[SerializeField] private ZText		ChannelName;
	[SerializeField] private Text		MapLocation;
	[SerializeField] private ZButton	NPCTraceButton;
	[SerializeField] private UIScrollMinimapNPCTrace NPCTraceWidget = null;
	[SerializeField] private UIMinimapChannelPopup   ChannelPopup = null;
	[SerializeField] private ZUIScrollMinimap		MinimapScroll		= null;
	[SerializeField] private List<SMarkerTemplate>	MarkerTemplate	= new List<SMarkerTemplate>();
	[SerializeField] private List<SMinimapInfo>		MinimapInfo		= new List<SMinimapInfo>();

	private Stage_Table mStageTable = null;
	private UIFrameQuest mUIFrameQuest = null;
	private Dictionary<GameObject, SMapMarkerInfo> m_dicMarker = new Dictionary<GameObject, SMapMarkerInfo>();

	//--------------------------------------------------------------------
	[SerializeField] private GameObject stageRamineTimeRoot;
	[SerializeField] private ZText stageRamineTime;
	private float secondCheckTimer = 0;
	private ulong endServerTime = 0;
	private string remainTimeTitleFormat;
	//--------------------------------------------------------------------
	protected override void OnInitialize()
	{
		base.OnInitialize();
		InputBlock.gameObject.SetActive(false);
		NPCTraceWidget.SetMonoActive(false);

		mUIFrameQuest = UIManager.Instance.Find<UIFrameQuest>();

		for (int i = 0; i < MarkerTemplate.Count; i++)
		{
			MarkerTemplate[i].Template.gameObject.SetActive(false);
			MarkerTemplate[i].Template.SetAnchor(AnchorPresets.TopLeft);
		}

		RegistEventCallBack();

		ChannelPopup.DoUIWidgetShowHide(false);

		HideStageRamainTime();
	}

	protected override void OnRemove()
	{
		base.OnRemove();
		DoMinimapMapMarkerClearAll();
		if (ZPawnManager.hasInstance)
		{
			ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleEnterEntry);
			ZPawnManager.Instance.DoRemoveEventRemoveEntity(HandleExitEntry);			
		}
	}

	//---------------------------------------------------------------------
	public void DoMinimapRefresh()
	{
		uint stageTID = ZGameModeManager.Instance.StageTid;
	
		DBStage.TryGet(stageTID, out mStageTable);
		if (mStageTable == null) return;

		// TO DO : 출시전에 제거할것 
	//	SetStageInputBlock(mStageTable);
		SetStageTitle(mStageTable);
		SetStageMinimap(mStageTable);
		SetStageRemainTime(mStageTable);
		SetStageNPCTrace(mStageTable);
		SetChannelInfo();

		CancelInvoke();		
		InvokeRepeating("UpdateMapMarker", 0, UpdateDalay);
	}

	public void DoMinimapRefreshChannel()
	{
		SetChannelInfo();
	}

	public void DoMinimapRefreshQuestTarget()
	{
		Dictionary<GameObject, SMapMarkerInfo>.Enumerator it = m_dicMarker.GetEnumerator();
		while (it.MoveNext())
		{
			SMapMarkerInfo marker = it.Current.Value;
			if (marker.FollowPawn != null && marker.IsNPC == true)
			{
				if (marker.MarkerType == E_MapMarkerType.NPC_Quest)
				{
					MakeMarkerNPC(marker.FollowPawn);
				}
			}
		}
	}

	public void DoMinimapMapMarkerDelete(GameObject _followTarget)
	{
		if (m_dicMarker.ContainsKey(_followTarget))
		{
			SMapMarkerInfo markerInfo = m_dicMarker[_followTarget];
			Destroy(markerInfo.MarkerTransform.gameObject);
			m_dicMarker.Remove(_followTarget);
		}
	}

	public void DoMinimapMapMarkerClearAll()
	{
		Dictionary<GameObject, SMapMarkerInfo>.Enumerator it = m_dicMarker.GetEnumerator();
		while (it.MoveNext())
		{
			SMapMarkerInfo markerInfo = it.Current.Value;
			Destroy(markerInfo.MarkerTransform.gameObject);
		}
		m_dicMarker.Clear();
	}

	protected virtual void Update()
	{					
		if( stageRamineTime.isActiveAndEnabled ) {
			UpdateStageRemainTime();
		}
	}

	//----------------------------------------------------------------------------------
	private void UpdateMapMarker()
	{
		if (MinimapScroll.IsMinimapLoad() == false) return;

		Dictionary<GameObject, SMapMarkerInfo>.Enumerator it = m_dicMarker.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.Value.FollowTransform == null)
			{
				m_dicMarker.Remove(it.Current.Key);
				it = m_dicMarker.GetEnumerator();
			}
			else
			{
				RefreshMapMarker(it.Current.Value);
			}
		}
	}

	private void RefreshMapMarker(SMapMarkerInfo _mapMarker)
	{
		Vector2 position = new Vector2(_mapMarker.FollowTransform.position.x, _mapMarker.FollowTransform.position.z);
		switch(_mapMarker.MarkerType)
		{
			case E_MapMarkerType.MyPlayer:
				MapLocation.text = string.Format("{0,3:N0}  {1,3:N0}", position.x, position.y);
				MinimapScroll.DoMinimapCenterPosition(new Vector2(position.x, position.y));
				Vector3 eulerAngle = Vector3.zero;
				eulerAngle.z = -_mapMarker.FollowTransform.rotation.eulerAngles.y;
				_mapMarker.MarkerTransform.rotation = Quaternion.Euler(eulerAngle);
				break;
		}

		Vector2 scrollPosition = MinimapScroll.ExtractPositionWorldToScroll(position) * -1;
		_mapMarker.MarkerTransform.anchoredPosition = scrollPosition;
	}

	private RectTransform FindMapMarker(E_MapMarkerType _markerType)
	{
		RectTransform find = null;
		for (int i = 0; i < MarkerTemplate.Count; i++)
		{
			if (MarkerTemplate[i].MarkerType == _markerType)
			{
				find = MarkerTemplate[i].Template;
				break;
			}
		}
		return find;
	}

	private SMinimapInfo FindMinimapInfo(uint _stageTID)
	{
		SMinimapInfo Find = null;
		for (int i = 0; i < MinimapInfo.Count; i++)
		{
			if (MinimapInfo[i].StageID == _stageTID)
			{
				Find = MinimapInfo[i];
				break;
			}
		}
		return Find;
	}
	
	private void SetStageTitle(Stage_Table _stageTable)
	{	
		StageName.text = _stageTable.StageTextID;
	}

	private void SetStageInputBlock(Stage_Table _stageTable)
	{
		if (mStageTable.StageType == E_StageType.Tutorial)
		{
			InputBlock.gameObject.SetActive(true);
		}
		else
		{
			InputBlock.gameObject.SetActive(false);
		}
	}

	private void SetStageMinimap(Stage_Table _stageTable)
	{
		SMinimapInfo minimapInfo = FindMinimapInfo(_stageTable.StageID);
		if (minimapInfo != null)
		{
			MinimapScroll.DoMinimapSetTexture(_stageTable.MinmapFileName, minimapInfo.StageSize, minimapInfo.StageScale, minimapInfo.StageOffset);
		}
		else
		{
			ZLog.LogWarn(ZLogChannel.UI, $"{_stageTable.StageID}용 {nameof(SMinimapInfo)} 정보가 설정되어 있지 않습니다.");
		}
	}

	private void RegistEventCallBack()
	{
		ZPawnManager.Instance.DoAddEventCreateEntity(HandleEnterEntry);
		ZPawnManager.Instance.DoAddEventRemoveEntity(HandleExitEntry);
	}

	private void MakeMapMarker(ZPawn _markerPawn)
	{
	//	if (_markerPawn == null) return;

		E_UnitType uinitType = _markerPawn.EntityType;
		uint tableID = _markerPawn.EntityData.TableId;
		switch (uinitType)
		{
			case E_UnitType.Character:
				if (_markerPawn == ZPawnManager.Instance.MyEntity)
				{
					MakeMapMarkerAdd(E_MapMarkerType.MyPlayer, _markerPawn, tableID);
				}
				else
				{
					MakeMapMarkerAdd(E_MapMarkerType.Player, _markerPawn, tableID);
				}
				break;

			case E_UnitType.Monster:
				MakeMapMarkerAdd(E_MapMarkerType.MonsterNormal, _markerPawn, tableID);
				break;

			case E_UnitType.Gimmick:
				MakeMapMarkerAdd(E_MapMarkerType.Gimmick, _markerPawn, tableID);
				break;

			case E_UnitType.NPC:
				MakeMarkerNPC(_markerPawn);
				break;
		}
	}

	private void MakeMapMarkerAdd(E_MapMarkerType _markerType, ZPawn _followTarget, uint _pawnTID)
	{
		RectTransform template = FindMapMarker(_markerType);
		if (template == null) return;
		SMapMarkerInfo markerInfo = null;
		if (m_dicMarker.ContainsKey(_followTarget.gameObject))
		{
			markerInfo = m_dicMarker[_followTarget.gameObject];
		}
		else
		{
			markerInfo = new SMapMarkerInfo();
			m_dicMarker[_followTarget.gameObject] = markerInfo;
		}

		if (markerInfo.MarkerTransform != null)
		{
			Destroy(markerInfo.MarkerTransform.gameObject);
		}
		markerInfo.MarkerTransform = Instantiate(template, template.parent);
		markerInfo.MarkerTransform.gameObject.SetActive(true);
		markerInfo.FollowTransform = _followTarget.transform;
		markerInfo.FollowPawn = _followTarget;
		markerInfo.MarkerType = _markerType;
		markerInfo.PawnTID = _pawnTID;
		markerInfo.IsNPC = (_followTarget as ZPawnNpc) != null; 
	}

	private void MakeMarkerNPC(ZPawn _markerPawn)
	{
		ZPawnNpc npc = _markerPawn as ZPawnNpc;
		if (null == npc || null == npc.NpcData.TableData)
		{
			ZLog.LogWarn(ZLogChannel.UI, $"[NameTag] ==================Invalid Pawn = null");  
			return;
		}
		uint tableID = npc.NpcData.TableId;
		UIFrameQuest uiQuest = UIManager.Instance.Find<UIFrameQuest>();
		uint stageID = ZGameModeManager.Instance.StageTid;
		uint npcID = npc.NpcData.TableData.NPCID;
		if (uiQuest.CheckQuestNPC(stageID, npcID))			
		{
			MakeMapMarkerAdd(E_MapMarkerType.NPC_Quest, _markerPawn, tableID);
		}
		else
		{
			E_JobType jobType = npc.NpcData.TableData.JobType;
			MakeMarkerNPCJob(_markerPawn, jobType, tableID);
		}
	}

	private void MakeMarkerNPCJob(ZPawn _markerPawn, E_JobType _jobType ,uint _tableID)
	{
		switch (_jobType)
		{
			case E_JobType.Cleric:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_Cleric, _markerPawn, _tableID);
				break;
			case E_JobType.InterPortal:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_InterPortal, _markerPawn, _tableID);
				break;
			case E_JobType.Raid_Interaction:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_Raid, _markerPawn, _tableID);
				break;
			case E_JobType.SkillStore:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_SkillStore, _markerPawn, _tableID);
				break;
			case E_JobType.Storage:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_Storage, _markerPawn, _tableID);
				break;
			case E_JobType.Store:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_Store, _markerPawn, _tableID);
				break;
			case E_JobType.Trade:
				MakeMapMarkerAdd(E_MapMarkerType.NPC_Trade, _markerPawn, _tableID);
				break;
			case E_JobType.Quest:
				HideMarker(_markerPawn);
				break;
		}
	}

	private void HideMarker(ZPawn _followTarget)
	{
		if (m_dicMarker.ContainsKey(_followTarget.gameObject))
		{
			SMapMarkerInfo marker = m_dicMarker[_followTarget.gameObject];
			marker.MarkerTransform.gameObject.SetActive(false);
		}
	}

	private void SetChannelInfo()
	{
		ChannelName.gameObject.SetActive(false);
		PvPTypeName.gameObject.SetActive(false);
		ZGameModeManager.Instance.RefreshChannelList(() =>
		{
			ChannelName.gameObject.SetActive(true);
			PvPTypeName.gameObject.SetActive(true);
			ChannelName.text = $"{ZNet.Data.Me.CurCharData.LastChannelId} 채널";

			if (ZGameModeManager.Instance.IsChaosChannel())
			{
				PvPTypeName.text = "Stage_Type_02";
				PvPTypeName.color = new Color(1, 0.274f, 0.407f);
			}
			else if (ZGameModeManager.Instance.IsPvPChannel())
			{
				PvPTypeName.text = "Stage_Type_03";
				PvPTypeName.color = new Color(1, 0.533f, 0.2745f);
			}
			else
			{
				PvPTypeName.text = "Stage_Type_01";
				PvPTypeName.color = Color.white;
			}
		});
	}

	private void RefreshNPCQuestMarker()
	{

	}

	private void SetStageNPCTrace(Stage_Table _stageTable)
	{
		if (_stageTable.StageType != E_StageType.Town)
		{
			NPCTraceButton.gameObject.SetActive(false);
			return;
		}

		NPCTraceButton.gameObject.SetActive(true);
	}

	//------------------------------------------------------------------------------------
	public void HandleWorldMap()
	{
		UIManager.Instance.Open<UIFrameWorldMap>((_name, _uiFrame) => { _uiFrame.DoUILocalMapOpen(ZGameModeManager.Instance.StageTid); });
	}

	private void HandleEnterEntry(uint _entryID, ZPawn _pawn)
	{
		MakeMapMarker(_pawn);
	}

	private void HandleExitEntry(uint _entryID)
	{
		ZPawn pawn = ZPawnManager.Instance.GetEntity(_entryID);
        if(null != pawn)
		    DoMinimapMapMarkerDelete(pawn.gameObject);
	}

	public void HandleChannelChange()
	{
		if (ChannelPopup.DoUIWidgetShowHideSwitch())
		{
			ChannelPopup.DoMinimapChannelPopup(mStageTable, ZNet.Data.Me.CurCharData.LastChannelId);
		}
	}

	public void HandleNPCTrace()
	{
		if (NPCTraceWidget.gameObject.activeSelf == true)
		{
			NPCTraceWidget.DoUIWidgetShowHide(false);
		}
		else
		{
			NPCTraceWidget.DoUIWidgetShowHide(true);
			NPCTraceWidget.DoNPCTaceInitialize();
		}
	}

	//------------------------------------------------------------------------------------
	
	private void SetStageRemainTime( Stage_Table stageTable )
	{
		HideStageRamainTime();

		if( stageTable.StageType == E_StageType.Tower ) {
			var abilityAction = ZPawnManager.Instance.MyEntity.FindAbilityAction(stageTable.InBuffID);
			if( abilityAction != null ) {
				string titleFormat = string.Format( "{0} {1}", DBLocale.GetText( "WMap_Despair_1FDesc" ), DBLocale.GetText( "Despair_Tower_Time" ) );
				ShowStageRaminTime( abilityAction.EndServerTime, titleFormat );
			}
		}
	}

	private void ShowStageRaminTime( ulong _endServerTime, string _timeTitleFormat = "{0}"  )
	{
		stageRamineTimeRoot.SetActive( true );

		remainTimeTitleFormat = _timeTitleFormat;
		endServerTime = _endServerTime;
	}

	private void UpdateStageRemainTime()
	{
		secondCheckTimer += Time.deltaTime;
		if( secondCheckTimer >= 0.95f ) {
			secondCheckTimer = 0;

			ulong nowSec = TimeManager.NowSec;
			ulong reaminTime = endServerTime - nowSec;

			if( endServerTime <= nowSec ) {
				reaminTime = 0;
			}

			string time = TimeHelper.GetRemainFullTime( reaminTime );
			stageRamineTime.text = string.Format( remainTimeTitleFormat, time );

			if( reaminTime < 30 ) {
				stageRamineTime.color = Color.red;
			}
			else {
				stageRamineTime.color = Color.white;
			}
		}
	}

	private void HideStageRamainTime()
	{
		secondCheckTimer = 0;
		endServerTime = 0;
		remainTimeTitleFormat = "{0}";
		stageRamineTimeRoot.SetActive( false );
	}
}