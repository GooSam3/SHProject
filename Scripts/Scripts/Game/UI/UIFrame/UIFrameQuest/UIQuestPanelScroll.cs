using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIQuestPanelScroll : CUGUIWidgetBase
{
	[SerializeField] private ZText EmptyMessage = null;
	[SerializeField] private UIQuestScrollMain ScrollMain = null;
	[SerializeField] private UIQuestScrollSub ScrollSub = null;
	[SerializeField] private UIQuestScrollTemple ScrollTemple = null;
	[SerializeField] private UIQuestScrollDaily ScrollDaily = null; // 추후 개발 
	[SerializeField] private UIQuestScrollAchieve ScrollAchieve = null;
	[SerializeField] private UIQuestDescription QuestDescriontion = null;
	[SerializeField] private UIQuestReward QuestReward = null;
	
	//----------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);

		ScrollMain.DoUIQuestScrollInitialize(QuestDescriontion, QuestReward, UIFrameQuest.E_QuestCategory.Main);
		ScrollSub.DoUIQuestScrollInitialize(QuestDescriontion, QuestReward, UIFrameQuest.E_QuestCategory.Sub);
		ScrollTemple.DoUIQuestScrollInitialize(QuestDescriontion, QuestReward, UIFrameQuest.E_QuestCategory.Temple);

		QuestDescriontion.SetMonoActive(false);
		QuestReward.SetMonoActive(false);
	}

	//----------------------------------------------------------------------------
	public void InitializeTableInfo()
	{
		Dictionary<uint, Quest_Table>.Enumerator it = GameDBManager.Container.Quest_Table_data.GetEnumerator();
		while (it.MoveNext())
		{
			Quest_Table tableInfo = it.Current.Value;
			if (tableInfo.UIQuestType == E_UIQuestType.Main)
			{
				ScrollMain.DoUIQuestScrollTable(tableInfo);
			}
			else if (tableInfo.UIQuestType == E_UIQuestType.Temple)
			{
				ScrollTemple.DoUIQuestScrollTable(tableInfo);
			}
			else if (tableInfo.UIQuestType == E_UIQuestType.Sub)
			{
				ScrollSub.DoUIQuestScrollTable(tableInfo);
			}
			else if (tableInfo.UIQuestType == E_UIQuestType.Achievement)
			{
				ScrollAchieve.DoUIQuestAchieveTable(tableInfo);
			}
		}

		ScrollMain.DoUIQuestScrollArrangeTableInfo();
		ScrollTemple.DoUIQuestScrollArrangeTableInfo();
		ScrollSub.DoUIQuestScrollArrangeTableInfo();
		ScrollAchieve.DoUIQuestAchiveArrangeAll();

		SetMonoActive(true);
		DoPanelDialogShowMain();
	}

	//--------------------------------------------------------------------
	public void DoPanelDialogRefreshQuest(UIFrameQuest.SQuestUpdate _questRefresh)
	{
		if (_questRefresh.QuestTable.UIQuestType == E_UIQuestType.Main)
		{
			_questRefresh.AutoReward = true;
			ScrollMain.DoUIQuestRefresh(_questRefresh);
		}
		else if (_questRefresh.QuestTable.UIQuestType == E_UIQuestType.Sub)
		{
			ScrollSub.DoUIQuestRefresh(_questRefresh);
		}
		else if (_questRefresh.QuestTable.UIQuestType == E_UIQuestType.Temple)
		{
			ScrollTemple.DoUIQuestRefresh(_questRefresh);
		}
		else if (_questRefresh.QuestTable.UIQuestType == E_UIQuestType.Achievement)
		{
			ScrollAchieve.DoUIQuestRefresh(_questRefresh);
		}
	}

	public void DoPanelDialogShowMain(uint _focusQuestTID = 0)
	{
		QuestReward.gameObject.SetActive(false);
		QuestDescriontion.gameObject.SetActive(false);
		ScrollMain.SetMonoActive(true);
		
		ScrollTemple.SetMonoActive(false);
		ScrollSub.SetMonoActive(false);
		ScrollAchieve.SetMonoActive(false);

		if (ScrollMain.DoUIQuestScrollSelectCurrent(_focusQuestTID))
		{
			EmptyMessage.gameObject.SetActive(false);
		}
		else
		{
			EmptyMessage.gameObject.SetActive(true);
		}
	}

	public void DoPanelDialogShowTemple(uint _focusQuestTID = 0)
	{
		QuestReward.gameObject.SetActive(false);
		QuestDescriontion.gameObject.SetActive(false);
		ScrollMain.SetMonoActive(false);
		ScrollTemple.SetMonoActive(true);
		ScrollSub.SetMonoActive(false);
		ScrollAchieve.SetMonoActive(false);

		if (ScrollTemple.DoUIQuestScrollSelectCurrent(_focusQuestTID))
		{
			EmptyMessage.gameObject.SetActive(false);
		}
		else
		{
			EmptyMessage.gameObject.SetActive(true);
		}
	}

	public void DoPanelDialogShowSub(uint _focusQuestTID = 0)
	{
		QuestReward.gameObject.SetActive(false);
		QuestDescriontion.gameObject.SetActive(false);
		ScrollMain.SetMonoActive(false);
		ScrollTemple.SetMonoActive(false);
		ScrollSub.SetMonoActive(true);
		ScrollAchieve.SetMonoActive(false);

		if (ScrollSub.DoUIQuestScrollSelectCurrent(_focusQuestTID))
		{
			EmptyMessage.gameObject.SetActive(false);
		}
		else
		{
			EmptyMessage.gameObject.SetActive(true);
		}

	}

	public void DoPanelDialogShowDaily()
	{
		ScrollMain.SetMonoActive(false);
		ScrollTemple.SetMonoActive(false);
		ScrollSub.SetMonoActive(false);
		ScrollAchieve.SetMonoActive(false);
		QuestReward.gameObject.SetActive(false);
		QuestDescriontion.gameObject.SetActive(false);
		EmptyMessage.gameObject.SetActive(false);
	}

	public void DoPanelDialogShowAchieve()
	{
		ScrollMain.SetMonoActive(false);
		ScrollTemple.SetMonoActive(false);
		ScrollSub.SetMonoActive(false);
		ScrollAchieve.SetMonoActive(true);
		QuestReward.gameObject.SetActive(false);
		QuestDescriontion.gameObject.SetActive(false);
		EmptyMessage.gameObject.SetActive(false);
	}

	//---------------------------------------------------------------------
	public bool CheckQuestNPCID(uint _npcMapID, uint _npcID, UIQuestScrollBase.E_QuestUIState _checkType, bool _talkOrOpen)
	{
		if (ScrollMain.CheckQuestTalkNPCID(_npcMapID, _npcID, _checkType, _talkOrOpen))
		{
			return true;
		}
		else if (ScrollSub.CheckQuestTalkNPCID(_npcMapID, _npcID, _checkType, _talkOrOpen))
		{
			return true;
		}
		else if (ScrollTemple.CheckQuestTalkNPCID(_npcMapID, _npcID, _checkType, _talkOrOpen))
		{
			return true;
		}
		return false;
	}

	public bool CheckQuestMonsterID(uint _npcID)
	{ 
		if (ScrollMain.CheckQuestMonsterID(_npcID))
		{
			return true;
		}
		else if (ScrollSub.CheckQuestMonsterID(_npcID))
		{
			return true;
		}
		else if (ScrollTemple.CheckQuestMonsterID(_npcID))
		{
			return true;
		}
		return false;
	}

	public bool CheckQuestCount(uint _questID)
	{
		if (ScrollMain.CheckQuestCountComplete(_questID))
		{
			return true;
		}
		else if (ScrollSub.CheckQuestCountComplete(_questID))
		{
			return true;
		}
		else if (ScrollTemple.CheckQuestCountComplete(_questID))
		{
			return true;
		}
		return false;
	}

	public void ExtractQuestNPCOpen(uint _npcMapID, uint _npcTID, List<UIQuestScrollBase.SQuestInfo> _listQuest)
	{
		ScrollSub.ExtractQuestNPCOpen(_npcMapID, _npcTID, _listQuest);
		ScrollTemple.ExtractQuestNPCOpen(_npcMapID, _npcTID, _listQuest);
	}

	public void SetUIQuestUpdateCallBack(UnityAction<uint, ulong, bool, bool> _questCallBack, UIFrameQuest.E_QuestCategory _questCategory)
	{
		if (_questCategory == UIFrameQuest.E_QuestCategory.Main)
		{
			ScrollMain.SetUIQuestCallBack(_questCallBack);
		}
		else if (_questCategory == UIFrameQuest.E_QuestCategory.Sub)
		{
			ScrollSub.SetUIQuestCallBack(_questCallBack);
		}
		else if (_questCategory == UIFrameQuest.E_QuestCategory.Temple)
		{
			ScrollTemple.SetUIQuestCallBack(_questCallBack);
		}
	} 

} 

 