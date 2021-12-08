using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using WebNet;
using ZDefine;
using GameDB;

public class UIFrameQuest : ZUIFrameBase
{
	public static UIFrameQuest Instance = null; 

	public enum E_QuestCategory
	{
		Main,
		Temple,
		Sub,
		Dalily,
		Achievement,
	}
	[System.Serializable]
	public class SRadioButtonInfo
	{
		public E_QuestCategory Category = E_QuestCategory.Main;
		public ZUIButtonRadio ButtonRadio;
	}

	public class SQuestUpdate
	{
		public ulong QuestSID = 0;
		public uint	QuestTID = 0;
		public uint	Value1 = 0;
		public uint  Value2 = 0;
		public bool  NoCheck = false;
		public bool  AutoReward = false;
		public Quest_Table QuestTable = null;
		public UIQuestScrollBase.E_QuestUIState UIState = UIQuestScrollBase.E_QuestUIState.Hide;
	}

	[SerializeField] GameObject				QuestPanel;
	[SerializeField] UIQuestPanelScroll			QuestScroll;
	[SerializeField] UIQuestPanelList			QuestList;
	[SerializeField] UIQuestAccepRewardPopup		RewardAccept;
	[SerializeField] UIQuestDialog				QuestDialog;
	[SerializeField] UIQuestDialogNPCAccept		QuestDialogAccept;
	[SerializeField] List<SRadioButtonInfo>		RadioButtons;

	private bool mFirstOpen = true;
	private bool mRegistCallBack = false;
	private uint mFocusSubQuestTID = 0;
	private int  mAlarmCount = 0;
	private GameObject mAlarmObject = null;
	private UISubHUDMiniMap mUISubHudMinimap = null;
	private UIFrameNameTag  mUIFrameNameTag = null;
	private E_QuestCategory mCurrentCategory = E_QuestCategory.Main;
	//-------------------------------------------------------------------------------
	public UIQuestCompleteChecker	QuestChecker = new UIQuestCompleteChecker();
	public UIQuestAutoPilot		QuestAutoPilot = new UIQuestAutoPilot();
	//-------------------------------------------------------------------------------
	public override bool IsBackable => true;
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		Instance = this;
		QuestChecker.InitializeQuestChecker(this);
		QuestAutoPilot.InitializeAutoPilot(this);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		QuestPanel.gameObject.SetActive(true);
		QuestScroll.gameObject.SetActive(true);
		QuestList.gameObject.SetActive(false);
		RewardAccept.gameObject.SetActive(false);
		QuestDialog.gameObject.SetActive(false);
		QuestDialogAccept.gameObject.SetActive(false);
		if (GameDBManager.Container != null)
		{
			QuestScroll.InitializeTableInfo();
			QuestDialog.InitializeQuestDialog();
		}
	}

	protected override void OnRemove()
	{
		base.OnRemove();
		RegistOffQuestEventCallBack();
		QuestAutoPilot.RemoveAutoPilot();
	}

	protected override void OnCommandContents(ZCommandUIButton.E_UIButtonCommand _commandID, ZCommandUIButton.E_UIButtonGroup _groupID, int _arguement, CUGUIWidgetBase _commandOwner)
	{
		if (_commandID == ZCommandUIButton.E_UIButtonCommand.RadioOn)
		{
			if (_groupID == ZCommandUIButton.E_UIButtonGroup.Group_1)
			{
				QuestPanel.gameObject.SetActive(true);
				QuestDialogAccept.DoUIWidgetShowHide(false);

				switch (_arguement)
				{
					case 0:
						QuestScroll.gameObject.SetActive(true);
						QuestList.gameObject.SetActive(false);
						mCurrentCategory = E_QuestCategory.Main;
						QuestScroll.DoPanelDialogShowMain();
						break;
					case 1:
						QuestScroll.gameObject.SetActive(true);
						QuestList.gameObject.SetActive(false);
						mCurrentCategory = E_QuestCategory.Temple;
						QuestScroll.DoPanelDialogShowTemple();
						break;
					case 2:
						QuestScroll.gameObject.SetActive(true);
						QuestList.gameObject.SetActive(false);
						mCurrentCategory = E_QuestCategory.Sub;
						QuestScroll.DoPanelDialogShowSub(mFocusSubQuestTID);
						break;
					case 3:
						QuestScroll.gameObject.SetActive(false);
						QuestList.gameObject.SetActive(true);
						mCurrentCategory = E_QuestCategory.Dalily;
						QuestScroll.DoPanelDialogShowDaily();
						break;
					case 4:
						QuestScroll.gameObject.SetActive(false);
						QuestList.gameObject.SetActive(true);
						mCurrentCategory = E_QuestCategory.Achievement;
						QuestScroll.DoPanelDialogShowAchieve();
						break;
				}
			}
		}
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		if (mFirstOpen)
		{
			mFirstOpen = false;
			QuestScroll.DoPanelDialogShowMain();
		}

		UIManager.Instance.Open<UISubHUDCurrency>();
		UIManager.Instance.Open<UISubHUDCharacterState>();
	}

	protected override void OnHide()
	{
		base.OnHide();

	}
	//------------------------------------------------------------------------------
	public void DoUIQuestInitialize()
	{
		if (mRegistCallBack == false)
		{
			mRegistCallBack = true;
			RegistOnQuestEventCallBack();
			mUISubHudMinimap = UIManager.Instance.Find<UISubHUDMiniMap>();
			mUIFrameNameTag = UIManager.Instance.Find<UIFrameNameTag>();
		}
	}

	public void DoUIQuestRefreshOtherTarget()
	{
		if (mUISubHudMinimap)
		{
			mUISubHudMinimap.DoMinimapRefreshQuestTarget();
		}

		if (mUIFrameNameTag) 
		{
			mUIFrameNameTag.DoUINameTagRefreshQuestTarget();
		}
	}

	public void DoUIQuestDialog(uint _rewardQuestID, UIQuestDialog.E_DialogType _dialogType)
	{
		if (Show == false)
			UIManager.Instance.Show<UIFrameQuest>();
		CameraManager.Instance?.DoSetCullingMaskForUI(false);

		QuestPanel.SetActive(false);
		QuestScroll.gameObject.SetActive(false);
		QuestList.gameObject.SetActive(false);
		RewardAccept.gameObject.SetActive(false);
		QuestDialogAccept.DoUIWidgetShowHide(false);

		QuestDialog.DoUIWidgetShowHide(true);
		QuestDialog.DoUIQuestDialogStart(_rewardQuestID, _dialogType);
	}

	public void DoUIQuestDialogSubOpen(uint _stageTID, uint _npcTID)
	{
		if (Show == false)
			UIManager.Instance.Show<UIFrameQuest>();
		CameraManager.Instance?.DoSetCullingMaskForUI(false);
		List<UIQuestScrollBase.SQuestInfo> questList = new List<UIQuestScrollBase.SQuestInfo>();
		ExtractQuestNPCOpen(_stageTID, _npcTID, questList);
		if (questList.Count > 0)
		{
			QuestPanel.SetActive(false);
			QuestScroll.gameObject.SetActive(false);
			QuestList.gameObject.SetActive(false);
			RewardAccept.gameObject.SetActive(false);
			QuestDialog.gameObject.SetActive(false);
			QuestDialogAccept.DoUIWidgetShowHide(true);
			QuestDialogAccept.DoDialogNPCAccept(_stageTID, _npcTID, questList);
		}
	}


	public void DoUIQuestPanel(E_QuestCategory _openCategory)
	{
		if (Show == false)
			UIManager.Instance.Show<UIFrameQuest>();
		CameraManager.Instance?.DoSetCullingMaskForUI(true);
		RewardAccept.gameObject.SetActive(false);
		QuestDialog.gameObject.SetActive(false);
		QuestDialogAccept.DoUIWidgetShowHide(false);

		mCurrentCategory = _openCategory;
		
		for (int i = 0; i < RadioButtons.Count; i++)
		{ 
			if (RadioButtons[i].Category == _openCategory)
			{
				RadioButtons[i].ButtonRadio.DoRadioButtonToggleOn();
				break;
			}
		}

	}

	public void DoUIQuestPanelSub(uint _focusSubQuestTID)
	{
		mFocusSubQuestTID = _focusSubQuestTID;
		DoUIQuestPanel(E_QuestCategory.Sub);
	}

	public void DoUIQuestPanel()
	{
		DoUIQuestPanel(mCurrentCategory);
	}

	public void DoUIQuestRewardPopup(uint _questTID, UIQuestAccepRewardPopup.E_RewardType _rewardType, int _selectRewardIndex = -1)
	{
		if (Show == false)
			UIManager.Instance.Show<UIFrameQuest>();
		CameraManager.Instance?.DoSetCullingMaskForUI(false);
		QuestPanel.SetActive(false);
		QuestScroll.gameObject.SetActive(false);
		QuestList.gameObject.SetActive(false);
		QuestDialog.gameObject.SetActive(false);
		QuestDialogAccept.DoUIWidgetShowHide(false);

		RewardAccept.gameObject.SetActive(true);
		RewardAccept.DoUIRewardPopup(_questTID, _rewardType, _selectRewardIndex);
	}

	public void DoUIQuestRewardRandomPlay(SRewardItemList _itemList, UnityAction _processEnd)
	{
		RewardAccept.DoUIRewardRandomPlay(_itemList.ItemID, _itemList.ItemCount, _processEnd);
	}

	public void DoUIQuestMove(Quest_Table _questTable, bool _walk)
	{
		Close();

		if (_walk)
		{
			if (QuestAutoPilot.pAutoPilot == false)
			{
				QuestAutoPilot.EventAutoPilotStart(_questTable.QuestID);
			}
		}
		else
		{
			QuestAutoPilot.EventWarpAutoPilotStart(_questTable.QuestID);
		}
	}

	//------------------------------------------------------------------------------
	
	public bool CheckQuestMonster(uint _monsterID)
	{
		if (_monsterID == 0) return false;

		return QuestScroll.CheckQuestMonsterID(_monsterID);
	}

	public bool CheckQuestNPC(uint _npcStageID, uint _npcID)
	{
		bool check = false;

		if (CheckQuestNPCAccept(_npcStageID, _npcID)
			|| CheckQuestNPCProgress(_npcStageID, _npcID)
			|| CheckQuestNPCReward(_npcStageID, _npcID))
		{
			check = true;
		}
			
		return check;
	}

	public bool CheckQuestNPCProgress(uint _npcMapID, uint _npcID)
	{
		return QuestScroll.CheckQuestNPCID(_npcMapID, _npcID, UIQuestScrollBase.E_QuestUIState.Progress, true);
	}

	public bool CheckQuestNPCReward(uint _npcMapID, uint _npcID)
	{
		return QuestScroll.CheckQuestNPCID(_npcMapID, _npcID, UIQuestScrollBase.E_QuestUIState.Reward, true);
	}

	public bool CheckQuestNPCAccept(uint _npcMapID, uint _npcID)
	{
		return QuestScroll.CheckQuestNPCID(_npcMapID, _npcID, UIQuestScrollBase.E_QuestUIState.Hide, false);
	}

	public bool CheckQuestNPCComplete(uint _npcMapID, uint _npcID)
	{
		return QuestScroll.CheckQuestNPCID(_npcMapID, _npcID, UIQuestScrollBase.E_QuestUIState.Complete, false);
	}

	public bool CheckQuestCount(uint _questID)
	{
		return QuestScroll.CheckQuestCount(_questID);
	}

	public void SetUIQuestUpdateCallBack(UnityAction<uint, ulong, bool, bool> _questCallBack, E_QuestCategory _questCategory)
	{
		QuestScroll.SetUIQuestUpdateCallBack(_questCallBack, _questCategory);
	}

	public void ExtractQuestNPCOpen(uint _npcMapID, uint _npcTID, List<UIQuestScrollBase.SQuestInfo> _listQuestAccept)
	{
		QuestScroll.ExtractQuestNPCOpen(_npcMapID, _npcTID, _listQuestAccept);
	}

	public void SetAlarmObject(GameObject _alarmObject)
	{
		if (_alarmObject == null) return;
		mAlarmObject = _alarmObject;

		RefreshAlarmCount();
	}

	public void DoUIQuestAlarmFocus(E_QuestCategory _category, bool _on)
	{
		if (_on)
		{
			mAlarmCount++; 
		}
		else
		{
			mAlarmCount--;
			if (mAlarmCount < 0)
			{
				mAlarmCount = 0;
			}	
		}

		RefreshAlarmCount();

		for (int i = 0; i < RadioButtons.Count; i++)
		{
			if (RadioButtons[i].Category == _category)
			{
				if (_on)
				{
					RadioButtons[i].ButtonRadio.DoAlarmActionAdd();
				}
				else
				{
					RadioButtons[i].ButtonRadio.DoAlarmActionDelete();
				}
				break;
			}
		}
	}

	//-----------------------------------------------------------------------------
	public void HandleQuestUpdate(QuestData _questData)
	{
		SQuestUpdate questUpdate = new SQuestUpdate();
		questUpdate.QuestTID = _questData.QuestTid;
		questUpdate.Value1 = _questData.Value1; 
		questUpdate.Value2 = _questData.Value2;
		questUpdate.NoCheck = _questData.NoCheck;
		questUpdate.QuestTable = DBQuest.GetQuestData(_questData.QuestTid);

		if (questUpdate.QuestTable == null) return;

		if (questUpdate.QuestTable.CompleteCheck == E_CompleteCheck.DeliveryItem)
		{
			questUpdate.Value1 = (uint)ZNet.Data.Me.GetCurrency(questUpdate.QuestTable.DeliveryItemID);
		}
		
		if (_questData.NewQuest && _questData.State == E_QuestState.None)
		{
			questUpdate.UIState = UIQuestScrollBase.E_QuestUIState.Confirm;			
		}
		else if (_questData.DeleteQuest)
		{
			questUpdate.UIState = UIQuestScrollBase.E_QuestUIState.Hide;
		}
		else if (_questData.State == E_QuestState.None)
		{
			if (questUpdate.QuestTable.CompleteCheck == E_CompleteCheck.Tutorial) // 최초 시작시 들어오는 듀토리얼은 여기서 처리 
			{
				Close();
				TutorialSystem.Instance.StartTutorial(_questData.QuestTid);
			}
			questUpdate.UIState = UIQuestScrollBase.E_QuestUIState.Progress;

		}
		else if (_questData.State == E_QuestState.Reward)
		{
			questUpdate.UIState = UIQuestScrollBase.E_QuestUIState.Reward;
			if (questUpdate.QuestTable.CompleteCheck == E_CompleteCheck.ClearTemple)
			{
				if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Temple)
				{
					QuestChecker.DoAddCondition(questUpdate.QuestTable, questUpdate.Value1);
					return;
				}
			}
		}
		else if (_questData.State == E_QuestState.Complete)
		{
			questUpdate.UIState = UIQuestScrollBase.E_QuestUIState.Complete;
			if (_questData.GroupComplete)  // 그룹을 모두 완료했을 경우 창을 닫아준다.
			{
				Close(); 
			}
		}

		QuestScroll.DoPanelDialogRefreshQuest(questUpdate);
	}

	private void HandleQuestEventUpdate(){}
	private void HandleQuestMainUpdate(uint _prevQuestID, uint _nextQuestID){}
	//----------------------------------------------------------------------------
	private void RegistOnQuestEventCallBack()
	{
		UIManager.Instance.AddEventUIFrameUpdate(QuestChecker.EventUpdateCondition, true);
		if (ZNet.Data.Me.FindCurCharData == null) return;

		ZNet.Data.Me.CurCharData.QuestUpdate += HandleQuestUpdate;
		ZNet.Data.Me.CurCharData.QuestEventUpdate += HandleQuestEventUpdate;
		ZNet.Data.Me.CurCharData.MainQuestChanged += HandleQuestMainUpdate;
		ZNet.Data.Me.CurCharData.NotifyQuestAll();
	} 

	private void RegistOffQuestEventCallBack()
	{
		UIManager.Instance.AddEventUIFrameUpdate(QuestChecker.EventUpdateCondition, false);
		if (ZNet.Data.Me.FindCurCharData == null) return;

		ZNet.Data.Me.CurCharData.QuestUpdate -= HandleQuestUpdate;
		ZNet.Data.Me.CurCharData.QuestEventUpdate -= HandleQuestEventUpdate;
		ZNet.Data.Me.CurCharData.MainQuestChanged -= HandleQuestMainUpdate;
	}

	private void RefreshAlarmCount()
	{
		if (mAlarmCount > 0)
		{
			mAlarmObject?.SetActive(true);
		}
		else
		{
			mAlarmObject?.SetActive(false);
		}
	}
}
