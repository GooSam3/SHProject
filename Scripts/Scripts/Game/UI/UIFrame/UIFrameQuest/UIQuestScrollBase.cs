using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

abstract public class UIQuestScrollBase : CUGUIScrollRectBase
{
	private class ICompareReverse : IComparer<uint>
	{
		public int Compare(uint _one, uint _two)
		{
			if (_one == _two)
				return 0;
			if (_one > _two)
				return -1;
			return 1;
		}
	}

	public enum E_QuestUIState
	{
		Hide,			// 앞으로 수행할 퀘스트라서 숨김 상태
		Confirm,			// 서버로 부터 새로운 퀘스트를 통보받은 상태 
		Progress,	    // 수락해서 진행중인 상태 
		Reward,			// 퀘스트 조건을 만족해서 보상 요청이 가능한 상태 
		Complete,		// 보상을 다 받아서 보기만 할 수 있는 단계
	}

	public class SQuestChapterInfo
	{
		public uint						UIChapter = 0;
		public uint					    UIQuestGroup = 0;
		public string						UIChapterIcon;
		public string						UIChapterName;
		public string						UIChapterTitleName;
		public E_QuestUIState				QuestState = E_QuestUIState.Hide;
		public UIQuestScrollItemBase		UIInstance = null;
		public SortedList<uint, SQuestInfo> QuestList = new SortedList<uint, SQuestInfo>(new ICompareReverse());
	}

	public class SQuestInfo
	{
		public uint					QuestTID;	
		public ulong					CountCurrent;
		public ulong					CountMax;
		public E_QuestUIState			QuestState = E_QuestUIState.Hide;	
		public UIQuestSubSlotItem		QuestItemUI = null;		
		public Quest_Table			QuestTable = null;
		public SQuestInfo				QuestMajor = null;

	}

	protected UIQuestDescription			mQuestDescription = null;
	protected UIQuestReward				mQuestReward = null;
	protected UIFrameQuest					mUIFrameQuest = null;

	private uint							mFocusQuestTID = 0;
	private UIQuestScrollItemBase			mCurrentQuestItem = null;
	private UnityAction<uint, ulong, bool, bool>  mQuestCallBack = null;
	private UIFrameQuest.E_QuestCategory	mQuestCategory = UIFrameQuest.E_QuestCategory.Main;

	protected Dictionary<uint, SQuestChapterInfo>	m_listQuestChapterInstance = new Dictionary<uint, SQuestChapterInfo>();
	protected Dictionary<uint, List<SQuestInfo>>		m_dicQuestGroup = new Dictionary<uint, List<SQuestInfo>>();
	protected SortedList<uint, SQuestChapterInfo>	m_listQuestChapter = new SortedList<uint, SQuestChapterInfo>(new ICompareReverse());
	protected Dictionary<uint, SQuestInfo>          m_dicQuestInfo = new Dictionary<uint, SQuestInfo>();
	//------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
	}
	//------------------------------------------------------------------
	public void DoUIQuestScrollInitialize(UIQuestDescription _questDescription, UIQuestReward _questReward, UIFrameQuest.E_QuestCategory _category)
	{
		mQuestDescription = _questDescription;
		mQuestReward = _questReward;
		mQuestCategory = _category;
		OnUIQuestScrollInitialize();
	}

	public void DoUIQuestScrollTable(Quest_Table _questTable)
	{
		if (_questTable.ChapterName == null) return;

		SQuestChapterInfo chapter = FindQuestChapterOrAlloc(_questTable.UIChapter);
		if (chapter.UIChapterIcon == null)
		{
			chapter.UIChapterIcon = _questTable.ChapterIcon;
		}
		if (chapter.UIChapterName == null)
		{
			chapter.UIChapterName = _questTable.ChapterName;
		}
		if (chapter.UIChapterTitleName == null)
		{
			chapter.UIChapterTitleName = _questTable.ChapterIcon;
		}
		

		SQuestInfo questInfo = new SQuestInfo();
		questInfo.QuestTable = _questTable;
		questInfo.QuestTID = _questTable.QuestID;
		questInfo.CountMax = _questTable.TargetCount;

		if (chapter.QuestList.Keys.Contains(_questTable.QuestSequence))
		{
			Debug.Assert(true, "[Quest] Table의 QuestSequence가 중복되었습니다 : " + _questTable.QuestID);
		}
		else
		{
			chapter.QuestList.Add(_questTable.QuestSequence, questInfo);
		}
	}

	public void DoUIQuestScrollArrangeTableInfo()
	{
		MakeQuestChapter();
		MakeQuestGroup();
		MakeQuestScrollSubItem();
		OnUIQuestScrollArrangeTableInfo();
	}

	public void DoUIQuestScrollSelectItem(UIQuestScrollItemBase _selectedItem)
	{
		if (mCurrentQuestItem != null && mCurrentQuestItem != _selectedItem)
		{
			mCurrentQuestItem.DoQuestScrollClose();
		}
		mCurrentQuestItem = _selectedItem;
		ProtUIScrollMoveTopItem(_selectedItem.transform as RectTransform, 10f);
	}

	public void DoUIQuestScrollSelectQuest(SQuestInfo _selectedQuest)
	{
		mFocusQuestTID = _selectedQuest.QuestTID;
		mQuestDescription.SetMonoActive(true);
		mQuestDescription.DoUIQuestDescription(_selectedQuest.QuestTable, _selectedQuest.QuestState == E_QuestUIState.Complete);
		mQuestReward.SetMonoActive(true);
		mQuestReward.DoUIQuestReward(_selectedQuest);
	}

	public bool DoUIQuestScrollSelectCurrent(uint _focusQuestTID)
	{
		if (_focusQuestTID != 0)
			mFocusQuestTID = _focusQuestTID; 
		bool selected = false;
		bool findQuest = false;
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];

			for (int j = 0; j < questChapter.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];
				if (questInfo.QuestState == E_QuestUIState.Progress || questInfo.QuestState == E_QuestUIState.Reward)
				{
					if (mFocusQuestTID == 0 || mFocusQuestTID == questInfo.QuestTID)
					{
						findQuest = true;
						selected = true;
						DoUIQuestScrollSelectQuest(questInfo);
						questChapter.UIInstance.DoQuestScrollRefresh();
						mCurrentQuestItem = questChapter.UIInstance;
						break;
					}					
				}
			}

			if (findQuest == false)
			{
				questChapter.UIInstance.SlotItemSelect(false);
			}
			else
			{
				findQuest = false;
			}
		}

		return selected;
	}

	public void DoUIQuestRefresh(UIFrameQuest.SQuestUpdate _questRefresh)
	{
		RefreshQuestInternal(_questRefresh);
		RefreshQuestAlarm();
	}

	public bool CheckQuestTalkNPCID(uint _npcMapID, uint _npcID, E_QuestUIState _checkType, bool _talkOrOpen)
	{
		bool questEnable = false;
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];
			for (int j = questChapter.QuestList.Values.Count - 1; j >= 0; j--)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];

				if (questInfo.QuestState == _checkType)
				{
					if (_talkOrOpen)
					{
						if (questInfo.QuestTable.TalkNPCID == _npcID && questInfo.QuestTable.TalkNPCMap == _npcMapID)
						{
							questEnable = true;
							break;
						}
					}
					else
					{
						if (questInfo.QuestTable.QuestOpenNPCID == _npcID && questInfo.QuestTable.QuestOpenNPCMap == _npcMapID)
						{
							if (CompareQuestEnable(questInfo))
							{
								questEnable = true;
								break;
							}
						}
					}
				}
			}

			if (questEnable)
			{
				break;
			}
		}

		return questEnable; 
	}

	public bool CheckQuestMonsterID(uint _monsterID)
	{
		bool questEnable = false;
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];
			for (int j = 0; j < questChapter.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];
				if (questInfo.QuestState == E_QuestUIState.Progress || questInfo.QuestState == E_QuestUIState.Confirm)
				{
					if (questInfo.QuestTable.TermsMonsterID == _monsterID || questInfo.QuestTable.ItemGetMonsterID == _monsterID)
					{
						questEnable = true;
						break;
					}

					if (questInfo.QuestTable.ItemGetObjectID == _monsterID)
					{
						questEnable = true;
						break;
					}
				}
			}

			if (questEnable)
			{
				break;
			}
		}
		return questEnable;
	}

	public bool CheckQuestCountComplete(uint _questID)
	{
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];
			for (int j = 0; j < questChapter.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];
				if (questInfo.QuestTID == _questID)
				{
					if (questInfo.CountCurrent >= questInfo.CountMax)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsQuestComplete(uint _questTID)
	{
		bool complete = false;

		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];
			for (int j = 0; j < questChapter.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];
				if (questInfo.QuestState == E_QuestUIState.Complete)
				{
					complete = true;
					break;
				}
			}

			if (complete)
			{
				break;
			}
		}
		return complete;
	}

	public void ExtractQuestNPCOpen(uint _npcMapID, uint _npcTID, List<UIQuestScrollBase.SQuestInfo> _listQuest)
	{
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapter = m_listQuestChapter.Values[i];
			for (int j = questChapter.QuestList.Values.Count - 1; j >= 0; j--)
			{
				SQuestInfo questInfo = questChapter.QuestList.Values[j];
				if (questInfo.QuestState == E_QuestUIState.Hide)
				{
					if (questInfo.QuestTable.QuestOpenNPCID == _npcTID && questInfo.QuestTable.QuestOpenNPCMap == _npcMapID && questInfo.QuestTable.QuestOpenType == E_QuestOpenType.NPC)
					{
						if (CompareQuestEnable(questInfo))
						{
							_listQuest.Add(questInfo);
						}
					}
				}
			}
		}
	}

	public void SetUIQuestCallBack(UnityAction<uint, ulong, bool, bool> _questCallBack)
	{
		mQuestCallBack = _questCallBack;

		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo chapterInfo = m_listQuestChapter.Values[i];
			for (int j = 0; j < chapterInfo.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = chapterInfo.QuestList.Values[j];
				if (questInfo.QuestState == E_QuestUIState.Confirm || questInfo.QuestState == E_QuestUIState.Progress || questInfo.QuestState == E_QuestUIState.Reward)
				{
					ProgressCallBack(questInfo);
				}
			}
		}
	}

	//------------------------------------------------------------------
	private SQuestChapterInfo FindQuestChapterOrAlloc(uint _chapterIndex)
	{
		SQuestChapterInfo findChapter = null;

		if (m_listQuestChapterInstance.ContainsKey(_chapterIndex))
		{
			findChapter = m_listQuestChapterInstance[_chapterIndex];
		}
		else
		{
			findChapter = new SQuestChapterInfo();
			findChapter.UIChapter = _chapterIndex;
			m_listQuestChapterInstance[_chapterIndex] = findChapter;
		}

		return findChapter;
	}

	private void MakeQuestScrollSubItem()
	{
		IEnumerator<SQuestChapterInfo> it = m_listQuestChapter.Values.GetEnumerator();
		while (it.MoveNext())
		{
			SQuestChapterInfo chapterInfo = it.Current;
			UIQuestScrollItemBase item = ProtUIScrollSlotItemRequest() as UIQuestScrollItemBase;

			chapterInfo.UIInstance = item;
			item.DoQuestScrollItemInitialize(chapterInfo, this, mQuestCategory);			
		}

		it = m_listQuestChapter.Values.GetEnumerator();
		while (it.MoveNext())
		{
			SQuestChapterInfo chapterInfo = it.Current;
			chapterInfo.UIInstance.DoQuestScrollRefresh();
		}
	}

	private void MakeQuestChapter()
	{
		m_listQuestChapter.Clear();
		Dictionary<uint, SQuestChapterInfo>.Enumerator it = m_listQuestChapterInstance.GetEnumerator();
		while (it.MoveNext())
		{
			SQuestChapterInfo chapter = it.Current.Value;
			if (chapter.QuestList.Count > 0)
			{
				uint sequence = chapter.QuestList.Values[0].QuestTID;
				m_listQuestChapter.Add(sequence, chapter);
			}
		}
	}

	private void MakeQuestGroup()
	{
		m_dicQuestGroup.Clear();
		Dictionary<uint, SQuestChapterInfo>.Enumerator it = m_listQuestChapterInstance.GetEnumerator();
		while (it.MoveNext())
		{
			SQuestChapterInfo chapter = it.Current.Value;
			for (int i = 0; i < chapter.QuestList.Values.Count; i++)
			{
				SQuestInfo questInfo = chapter.QuestList.Values[i];

				if (m_dicQuestGroup.ContainsKey(questInfo.QuestTable.QuestGroupID) == false)
				{
					m_dicQuestGroup[questInfo.QuestTable.QuestGroupID] = new List<SQuestInfo>();
				}
				m_dicQuestGroup[questInfo.QuestTable.QuestGroupID].Add(questInfo);
			}
		}

		Dictionary<uint, List<SQuestInfo>>.Enumerator itGroup = m_dicQuestGroup.GetEnumerator();

		while (itGroup.MoveNext())
		{
			List<SQuestInfo> listQuestInfo = itGroup.Current.Value;

			if (listQuestInfo.Count == 0) continue;

			SQuestInfo lastQuestInfo = listQuestInfo[0];

			for (int i = 0; i < listQuestInfo.Count; i++)
			{
				SQuestInfo questInfo = listQuestInfo[i];

				if (questInfo.QuestTable.TaskQuestType == E_TaskQuestType.TaskQuest)
				{
					questInfo.QuestMajor = lastQuestInfo;
				}
			}
		}
	}


	private void RefreshQuestInternal(UIFrameQuest.SQuestUpdate _questRefresh)
	{
		bool findQuest = false;
		bool deleteQuest = false;
		bool complete = false;
		uint completeGroupID = 0;
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			SQuestChapterInfo questChapterInfo = m_listQuestChapter.Values[i];
			for (int j = 0; j < questChapterInfo.QuestList.Values.Count; j++)
			{
				SQuestInfo questInfo = questChapterInfo.QuestList.Values[j];
				if (questInfo.QuestTID == _questRefresh.QuestTID)
				{
					findQuest = true;					
					RefreshQuestState(questInfo, _questRefresh.UIState);
					questInfo.CountCurrent = _questRefresh.Value1;
					completeGroupID = questInfo.QuestTable.QuestGroupID;
					if (_questRefresh.UIState == E_QuestUIState.Progress)
					{
						questChapterInfo.QuestState = E_QuestUIState.Progress;

						if (_questRefresh.NoCheck)
						{
							RegistClientCheckQuest(questInfo);
						}

					}
					else if (_questRefresh.UIState == E_QuestUIState.Reward)
					{
						questChapterInfo.QuestState = E_QuestUIState.Progress;
						if (_questRefresh.AutoReward)
						{
							mUIFrameQuest.DoUIQuestDialog(questInfo.QuestTID, UIQuestDialog.E_DialogType.Reward);
						}
					}
					else if (_questRefresh.UIState == E_QuestUIState.Complete)
					{
						if (j == 0) // 그룹내 모든 퀘스트를 클리어 했다.
						{
							questChapterInfo.QuestState = E_QuestUIState.Complete;
						}
						else
						{
							questChapterInfo.QuestState = E_QuestUIState.Progress;
						}
						mUIFrameQuest.QuestChecker.DoAddNPCShowHide(_questRefresh.QuestTable.QuestCompleteHideStageID, _questRefresh.QuestTable.QuestCompleteHideNpcID, false);
					}
					else if (_questRefresh.UIState == E_QuestUIState.Hide)
					{						
						deleteQuest = true;
					}
					else if (_questRefresh.UIState == E_QuestUIState.Confirm)
					{
						if (_questRefresh.AutoReward)
						{
							mUIFrameQuest.DoUIQuestDialog(questInfo.QuestTID, UIQuestDialog.E_DialogType.ConfirmMain);
						}
						questChapterInfo.QuestState = E_QuestUIState.Progress;
						questInfo.QuestState = E_QuestUIState.Progress;

						if (_questRefresh.NoCheck)
						{
							RegistClientCheckQuest(questInfo);
						}
					}

					complete = true;
					ProgressCallBack(questInfo);
					mQuestReward.DoUIQuestRewardRefresh(questInfo);	
				}
			}

			if (findQuest)
			{
				if (deleteQuest) 
				{

					if (_questRefresh.AutoReward)
					{
						for (int j = 0; j < questChapterInfo.QuestList.Values.Count; j++)
						{
							SQuestInfo questInfo = questChapterInfo.QuestList.Values[j];
							questInfo.CountCurrent = 0;
							questInfo.CountMax = 0;
							questInfo.QuestState = E_QuestUIState.Hide;
						}
						questChapterInfo.QuestState = E_QuestUIState.Hide;
					}
					else
					{
						bool hide = true;
						for (int j = 0; j < questChapterInfo.QuestList.Values.Count; j++)
						{
							SQuestInfo questInfo = questChapterInfo.QuestList.Values[j];

							if (questInfo.QuestTID == _questRefresh.QuestTID)
							{
								questInfo.CountCurrent = 0;
								questInfo.CountMax = 0;
								questInfo.QuestState = E_QuestUIState.Hide;
							}
							else if (questInfo.QuestState != E_QuestUIState.Hide)
							{
								hide = false;
							}
						} 

						if (hide)
						{
							questChapterInfo.QuestState = E_QuestUIState.Hide;
						}
					}

					questChapterInfo.UIInstance.DoQuestScrollRefresh();
					return;
				}
				else
				{
					bool checkFindQuest = false;
					for (int j = 0; j < questChapterInfo.QuestList.Values.Count; j++)
					{
						SQuestInfo questInfo = questChapterInfo.QuestList.Values[j];

						if (questInfo.QuestTID == _questRefresh.QuestTID)
						{
							checkFindQuest = true;
							continue;
						}

						if (_questRefresh.AutoReward)
						{
							if (checkFindQuest)
							{
								questInfo.QuestState = E_QuestUIState.Complete;
								mUIFrameQuest.QuestChecker.DoAddNPCShowHide(questInfo.QuestTable.QuestCompleteHideStageID, questInfo.QuestTable.QuestCompleteHideNpcID, false);
							}
							else
							{
								questInfo.QuestState = E_QuestUIState.Hide;
							}

						}
						else
						{
							if (completeGroupID == questInfo.QuestTable.QuestGroupID)
							{
								if (checkFindQuest)
								{
									questInfo.QuestState = E_QuestUIState.Complete;
									mUIFrameQuest.QuestChecker.DoAddNPCShowHide(questInfo.QuestTable.QuestCompleteHideStageID, questInfo.QuestTable.QuestCompleteHideNpcID, false);
								}
								else
								{
									questInfo.QuestState = E_QuestUIState.Hide;
								}
							}
						}
					}
					questChapterInfo.UIInstance.DoQuestScrollRefresh();

					if (_questRefresh.AutoReward == false)
					{
						return;
					}
				}
			}
			else
			{
				if (_questRefresh.AutoReward)
				{
					if (complete)
					{
						questChapterInfo.QuestState = E_QuestUIState.Complete;
					}
					else
					{
						questChapterInfo.QuestState = E_QuestUIState.Hide;
					}
					questChapterInfo.UIInstance.DoQuestScrollRefresh();
				}
			}
		}
	}

	private void RefreshQuestAlarm()
	{
		for (int i = 0; i < m_listQuestChapter.Values.Count; i++)
		{
			m_listQuestChapter.Values[i].UIInstance.DoQuestScrollAlarmRefresh();
		}
	}

	private void ProgressCallBack(SQuestInfo _questInfo)
	{
		if (mQuestCallBack == null) return;
		
		if (_questInfo.QuestState == E_QuestUIState.Progress)
		{
			mQuestCallBack(_questInfo.QuestTID, _questInfo.CountCurrent, false, false);
		}
		else if (_questInfo.QuestState == E_QuestUIState.Reward)
		{
			mUIFrameQuest.QuestAutoPilot.EventAutoPilotStop(_questInfo.QuestTID);
			mQuestCallBack(_questInfo.QuestTID, _questInfo.CountCurrent, true, false);								
		}
		else if (_questInfo.QuestState == E_QuestUIState.Complete)
		{			
			mQuestCallBack(_questInfo.QuestTID, _questInfo.CountCurrent, true, true);			
		}
		else if (_questInfo.QuestState == E_QuestUIState.Hide)
		{
			mQuestCallBack(_questInfo.QuestTID, _questInfo.CountCurrent, false, true);
		}
	}


	private void RegistClientCheckQuest(SQuestInfo _questInfo)
	{
		mUIFrameQuest.QuestChecker.DoAddCondition(_questInfo.QuestTable, (uint)_questInfo.CountCurrent);
	}

	private void RefreshQuestState(SQuestInfo _questInfo, E_QuestUIState _newState)
	{
		if (_questInfo.QuestState != _newState)
		{
			if(_newState == E_QuestUIState.Confirm)
			{
				_questInfo.QuestState = E_QuestUIState.Progress;
			}
			else
			{
				_questInfo.QuestState = _newState;
			}			 
		}
		mUIFrameQuest.DoUIQuestRefreshOtherTarget();
	}
	//-------------------------------------------------------------------------------------
	private bool CompareQuestCondition(Quest_Table _questTable)
	{
		bool conditionTrue = false;

		if (_questTable.ConditionQuestID1.Count == 0 && _questTable.ConditionQuestID2.Count == 0 && _questTable.ConditionQuestID3.Count == 0)
		{
			conditionTrue = true;
		}

		return conditionTrue;
	}

	private bool CompareQuestJob(E_CharacterType _characterType)
	{
		bool compare = false;
		if (_characterType == E_CharacterType.All || ZPawnManager.Instance.MyEntity.CharacterType == _characterType)
		{
			compare = true;
		}

		return compare;
	}

	private bool CompareQuestLevel(Quest_Table _questTable)
	{
		uint playerLevel = ZNet.Data.Me.CurCharData.LastLevel;
		if (playerLevel < _questTable.ConditionLevel)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	private bool CompareQuestEnable(SQuestInfo _questInfo)
	{
		bool enableQuest = false;

		if (CompareQuestCondition(_questInfo.QuestTable))
		{
			if (CompareQuestJob(_questInfo.QuestTable.QuestCharacterType))
			{
				if (CompareQuestLevel(_questInfo.QuestTable))
				{
					enableQuest = true;
				}
			}
		}
		return enableQuest;
	}

	//-------------------------------------------------------------------
	protected virtual void OnUIQuestScrollInitialize() { }
	protected virtual void OnUIQuestScrollArrangeTableInfo() { }
}
