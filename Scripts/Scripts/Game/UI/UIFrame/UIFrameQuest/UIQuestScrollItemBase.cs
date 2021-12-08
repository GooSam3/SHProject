using System.Collections.Generic;
using UnityEngine;

abstract public class UIQuestScrollItemBase : CUGUIWidgetSlotItemBase
{
	[SerializeField] protected ZImage	DecoLine = null;
	[SerializeField] protected ZImage	ClearCheck = null;
	[SerializeField] protected ZImage	FlipArrow = null;
	[SerializeField] protected ZText	QuestChapter = null;
	[SerializeField] protected ZText	QuestTitle = null;
	[SerializeField] protected ZImage	QuestTitleImage = null;
	[SerializeField] protected ZImage	QuestAlram = null;
	[SerializeField] protected UIQuestSubSlotItem SubSlot = null;

	protected UIQuestScrollBase		mParentsScroll = null;
	protected UIQuestSubSlotItem		mCurrentSlotItem = null;
	protected bool mSelected = false;
	private bool mHasAlarm = false;
	private UIQuestScrollBase.E_QuestUIState mCurrentSlotUIState = UIQuestScrollBase.E_QuestUIState.Hide;
	private UIFrameQuest.E_QuestCategory mQuestCategory = UIFrameQuest.E_QuestCategory.Main;

	protected List<UIQuestSubSlotItem> m_listSubSlotInstance = new List<UIQuestSubSlotItem>();
	private List<UIQuestScrollBase.SQuestInfo> m_listTotalQuestList = new List<UIQuestScrollBase.SQuestInfo>();
	//--------------------------------------------------------
	public void DoQuestScrollItemInitialize(UIQuestScrollBase.SQuestChapterInfo _questChapterInfo, UIQuestScrollBase _parentsScroll, UIFrameQuest.E_QuestCategory _category)
	{
		mParentsScroll = _parentsScroll;
		mQuestCategory = _category;

		ClearCheck.gameObject.SetActive(_questChapterInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete);
		QuestAlram.gameObject.SetActive(false);
		QuestTitle.text = _questChapterInfo.UIChapterName;

		MakeScrollItem(_questChapterInfo);
		OnQuestScrollItemInitialize(_questChapterInfo);
	}

	public void DoQuestScrollClose()
	{
		SlotItemSelect(false);
	}

	public void DoQuestScrollSelectSlotItem(UIQuestSubSlotItem _slotItem, UIQuestScrollBase.SQuestInfo _questInfo)
	{
		if (mCurrentSlotItem != null && mCurrentSlotItem != _slotItem)
		{
			mCurrentSlotItem.DoSubSlotQuestSelect(false);
		}
		mCurrentSlotItem = _slotItem;

		mParentsScroll.DoUIQuestScrollSelectQuest(_questInfo);
	}

	public void DoQuestScrollRefresh()
	{
		bool activate = false;
		bool reward = false;
		int completeCount = 0;
		for (int i = 0; i < m_listTotalQuestList.Count; i++)
		{
			if (m_listTotalQuestList[i].QuestState != UIQuestScrollBase.E_QuestUIState.Hide)
			{
				activate = true;
			}

			if (m_listTotalQuestList[i].QuestState == UIQuestScrollBase.E_QuestUIState.Complete)
			{
				completeCount++;
			}
			else if (m_listTotalQuestList[i].QuestState == UIQuestScrollBase.E_QuestUIState.Reward)
			{
				reward = true;
				break;
			}
		}

		if (completeCount == m_listTotalQuestList.Count)
		{
			mCurrentSlotUIState = UIQuestScrollBase.E_QuestUIState.Complete;
		}
		else if (reward)
		{
			mCurrentSlotUIState = UIQuestScrollBase.E_QuestUIState.Reward;
		}
		else if (activate)
		{
			mCurrentSlotUIState = UIQuestScrollBase.E_QuestUIState.Progress;
		}
		else
		{
			mCurrentSlotUIState = UIQuestScrollBase.E_QuestUIState.Hide;
		}


		if (mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Complete)
		{
			transform.SetAsLastSibling();
			ClearCheck.gameObject.SetActive(true);
			gameObject.SetActive(true);
		}
		else if (mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Hide)
		{
			ClearCheck.gameObject.SetActive(false);
			gameObject.SetActive(false);
		}
		else if (mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Progress || mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Confirm || mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Reward)
		{
			if (mCurrentSlotUIState == UIQuestScrollBase.E_QuestUIState.Reward)
			{
				QuestAlram.gameObject.SetActive(true);
			}

			ClearCheck.gameObject.SetActive(false);
			gameObject.SetActive(true);
			SlotItemSelect(true);
		}
		else
		{
			QuestAlram.gameObject.SetActive(false);
			ClearCheck.gameObject.SetActive(false);
			gameObject.SetActive(true);
		}
	}

	public void DoQuestScrollAlarmRefresh()
	{
		RefreshAlram();
	}

	//----------------------------------------------------------
	public void HandleSelectMainItem()
	{
		SlotItemSelect(!mSelected);
		mParentsScroll.DoUIQuestScrollSelectItem(this);
	}

	//--------------------------------------------------------
	private void MakeScrollItem(UIQuestScrollBase.SQuestChapterInfo _questChapterInfo)
	{
		List<UIQuestScrollBase.SQuestInfo> listShowItem = new List<UIQuestScrollBase.SQuestInfo>();

		for (int i = 0; i < _questChapterInfo.QuestList.Values.Count; i++)
		{
			UIQuestScrollBase.SQuestInfo questInfo = _questChapterInfo.QuestList.Values[i];

			if (questInfo.QuestMajor == questInfo || questInfo.QuestMajor == null)
			{
				listShowItem.Add(questInfo);
			}
			m_listTotalQuestList.Add(questInfo);
		}

		CloneSubSlot(listShowItem.Count);
	
		for (int i = 0; i < listShowItem.Count; i++)
		{
			UIQuestScrollBase.SQuestInfo questInfo = listShowItem[i];
			UIQuestSubSlotItem item = m_listSubSlotInstance[i];
			questInfo.QuestItemUI = item;

			if (i == listShowItem.Count - 1)
			{
				listShowItem[i].QuestItemUI.DoSubSlotQuestInitialize(listShowItem[i], this, true, false);
			}
			else
			{
				listShowItem[i].QuestItemUI.DoSubSlotQuestInitialize(listShowItem[i], this, true, true);
			}
		}		
	}

	private void CloneSubSlot(int _cloneCount)
	{
		m_listSubSlotInstance.Add(SubSlot);
		for (int i = 1; i < _cloneCount; i++)
		{
			UIQuestSubSlotItem newItem = Instantiate(SubSlot, transform);
			newItem.DoUIWidgetInitialize(mUIFrameParent);
			m_listSubSlotInstance.Add(newItem);
		}
	}

	private void ShowHideSubSlotItem(bool _show)
	{
		for (int i = 0; i < m_listTotalQuestList.Count; i++)
		{
			if (m_listTotalQuestList[i].QuestMajor != null)
			{
				UIQuestScrollBase.E_QuestUIState questStage = m_listTotalQuestList[i].QuestState;
				if (questStage == UIQuestScrollBase.E_QuestUIState.Progress || questStage == UIQuestScrollBase.E_QuestUIState.Confirm
					|| questStage == UIQuestScrollBase.E_QuestUIState.Reward)
				{

					for (int j = 0; j < m_listSubSlotInstance.Count; j++)
					{
						if (m_listSubSlotInstance[j].GetQuestTID == m_listTotalQuestList[i].QuestMajor.QuestTID)
						{
							m_listSubSlotInstance[j].SetSubSlotLinkQuest(m_listTotalQuestList[i]);
							break;
						}
					}
				}
			}
		}

		for (int i = 0; i < m_listSubSlotInstance.Count; i++)
		{
			m_listSubSlotInstance[i].DoSubSlotShowHide(_show);
		}
	}

	private void RefreshAlram()
	{
		bool hasAlarm = false;
		for (int i = 0; i < m_listSubSlotInstance.Count; i++)
		{
			if (m_listSubSlotInstance[i].IsAlarm)
			{
				hasAlarm = true;
				break;
			}	
		}

		if (hasAlarm)
		{
			QuestAlram.gameObject.SetActive(true);

			if (mHasAlarm == false)
			{
				mHasAlarm = true;
				UIFrameQuest uiQuest = mUIFrameParent as UIFrameQuest;
				uiQuest.DoUIQuestAlarmFocus(mQuestCategory, true);
			}
		}
		else
		{
			QuestAlram.gameObject.SetActive(false);

			if (mHasAlarm == true)
			{
				mHasAlarm = false;
				UIFrameQuest uiQuest = mUIFrameParent as UIFrameQuest;
				uiQuest.DoUIQuestAlarmFocus(mQuestCategory, false);
			}
		}
	}

	//-----------------------------------------------------------------------
	public void SlotItemSelect(bool _select)
	{ 
		mSelected = _select;
		if (_select)
		{
			Quaternion quet = Quaternion.identity;
			quet.eulerAngles = new Vector3(0, 0, -90f);
			FlipArrow.transform.rotation = quet;
			ShowHideSubSlotItem(true);
		}
		else
		{
			Quaternion quet = Quaternion.identity;
			quet.eulerAngles = new Vector3(0, 0, 90f);
			FlipArrow.transform.rotation = quet;
			ShowHideSubSlotItem(false);
		}

		DecoLine.gameObject.SetActive(_select);

		OnQuestScrollItemSelected(_select);
	}

	//----------------------------------------------------------------------
	protected virtual void OnQuestScrollItemInitialize(UIQuestScrollBase.SQuestChapterInfo _questInfo) { }
	protected virtual bool OnQuestScrollItemAlramCheck() { return false; }
	protected virtual void OnQuestScrollItemSelected(bool _selected) { }
}
