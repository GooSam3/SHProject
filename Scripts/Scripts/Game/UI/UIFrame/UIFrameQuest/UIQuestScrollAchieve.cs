using GameDB;
using System.Collections.Generic;

public class UIQuestScrollAchieve : CUGUIScrollRectBase
{
	public class SQuestAchieve
	{
		public uint QuestID = 0;
		public uint CurrentCount = 0;
		public uint MaxCount = 0;
		public Quest_Table QuestTable = null;
		public SQuestAchieveGroup QuestGroup = null;
		public UIQuestScrollBase.E_QuestUIState UIState = UIQuestScrollBase.E_QuestUIState.Hide;
	}

	public class SQuestAchieveGroup
	{
		public uint QuestGroup = 0;
		public UIQuestScrollBase.E_QuestUIState UIState = UIQuestScrollBase.E_QuestUIState.Hide;
		public UIQuestScrollAchieveItem ItemUI = null;
		public SortedList<uint, SQuestAchieve> QuestAchieveList = new SortedList<uint, SQuestAchieve>();
	}

	private LinkedList<SQuestAchieveGroup> m_listQuestAchive = new LinkedList<SQuestAchieveGroup>();
	//--------------------------------------------------------
	public void DoUIQuestAchieveTable(Quest_Table _questTable)
	{
		SQuestAchieveGroup achieveGroup = FindOrAllocQuestAchieveGroup(_questTable.QuestGroupID);

		SQuestAchieve newAchive = new SQuestAchieve();
		newAchive.QuestID = _questTable.QuestID;
		newAchive.MaxCount = (uint)_questTable.TargetCount;
		newAchive.QuestTable = _questTable;
		newAchive.QuestGroup = achieveGroup;

		achieveGroup.QuestAchieveList.Add(_questTable.QuestSequence, newAchive);
	}

	public void DoUIQuestAchiveArrangeAll()
	{
		LinkedList<SQuestAchieveGroup>.Enumerator it = m_listQuestAchive.GetEnumerator();
		while(it.MoveNext())
		{
			it.Current.ItemUI.DoAchieveRefreshItem(it.Current.QuestAchieveList.Values[0]);
		}
	}

	public void DoUIQuestRefresh(UIFrameQuest.SQuestUpdate _questRefresh)
	{
		SQuestAchieve questAchieve = FindQuestAchieveGroup(_questRefresh.QuestTID);
		if (questAchieve == null) return;

		questAchieve.CurrentCount = _questRefresh.Value1;
		questAchieve.UIState = _questRefresh.UIState;
		RefreshQuestAchieve(questAchieve.QuestGroup, _questRefresh.QuestTID);
	}

	//--------------------------------------------------------
	private void RefreshQuestAchieve(SQuestAchieveGroup _achieveGroup, uint _questTID)
	{		
		IList<SQuestAchieve> listQuest = _achieveGroup.QuestAchieveList.Values;
		for (int i = 0; i < listQuest.Count; i++)
		{
			SQuestAchieve questAchieve = listQuest[i];
			if (questAchieve.QuestID == _questTID)
			{
				if (i == listQuest.Count - 1) // 마지막 업적까지 완료되면 그룹을 완료 처리한다.
				{
					if (questAchieve.UIState == UIQuestScrollBase.E_QuestUIState.Complete)
					{
						_achieveGroup.UIState = UIQuestScrollBase.E_QuestUIState.Complete;
						_achieveGroup.ItemUI.transform.SetAsLastSibling(); // 마지막으로 보낸다.
						UIFrameQuest questFrame = mUIFrameParent as UIFrameQuest;
						questFrame.DoUIQuestPanel(UIFrameQuest.E_QuestCategory.Achievement);
					}
				}
				else if (questAchieve.UIState == UIQuestScrollBase.E_QuestUIState.Complete)
				{
					UIFrameQuest questFrame = mUIFrameParent as UIFrameQuest;
					questFrame.DoUIQuestPanel(UIFrameQuest.E_QuestCategory.Achievement);
				}
				_achieveGroup.ItemUI.DoAchieveRefreshItem(questAchieve); 
				break;
			}
			else
			{
				questAchieve.UIState = UIQuestScrollBase.E_QuestUIState.Complete;
			}
		}
	}


	private SQuestAchieveGroup FindOrAllocQuestAchieveGroup(uint _group)
	{
		SQuestAchieveGroup findInstance = null;
		LinkedList<SQuestAchieveGroup>.Enumerator it = m_listQuestAchive.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.QuestGroup == _group)
			{
				findInstance = it.Current;
				break;
			}
		}

		if (findInstance == null)
		{
			findInstance = new SQuestAchieveGroup();
			findInstance.QuestGroup = _group;
			findInstance.ItemUI = ProtUIScrollSlotItemRequest() as UIQuestScrollAchieveItem;
			m_listQuestAchive.AddLast(findInstance);
		}

		return findInstance;
	}

	private SQuestAchieve FindQuestAchieveGroup(uint _questTID)
	{
		SQuestAchieve findInstance = null;
		LinkedList<SQuestAchieveGroup>.Enumerator it = m_listQuestAchive.GetEnumerator();
		while (it.MoveNext())
		{
			IList<SQuestAchieve> listQuest = it.Current.QuestAchieveList.Values;
			for (int i = 0; i < listQuest.Count; i++)
			{ 
				if (listQuest[i].QuestID == _questTID)
				{
					findInstance = listQuest[i];
					break;
				}
			}

			if (findInstance != null)
				break;
		}

		return findInstance;
	}
}
