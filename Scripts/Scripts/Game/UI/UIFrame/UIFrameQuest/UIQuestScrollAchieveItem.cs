using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestScrollAchieveItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] ZScrollBar		Progress = null;
	[SerializeField] ZButton			Complete = null;
	[SerializeField] ZUIButtonCommand	ButtonReward = null;
	[SerializeField] Text			ProgressCount = null;
	[SerializeField] ZText			QuestTitle = null;
	[SerializeField] ZText			QuestDescription = null;
	[SerializeField] ZImage		QuestIcon = null;
	[SerializeField] ZImage		QuestAlram = null;
	[SerializeField] UIQuestScrollReward ScrollReward = null;

	private CanvasGroup mCanvasGroup = null;
	private UIFrameQuest mUIFrameQuest = null;
	private uint mQuestID = 0;
	//-----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mCanvasGroup = GetComponent<CanvasGroup>();
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
	}

	//-----------------------------------------------------------------
	public void DoAchieveRefreshItem(UIQuestScrollAchieve.SQuestAchieve _questAchieve)
	{
		RewardItemInfo(_questAchieve.QuestTable);
		if (_questAchieve.UIState == UIQuestScrollBase.E_QuestUIState.Reward)
		{
			RewardItem();
		}
		else if (_questAchieve.UIState == UIQuestScrollBase.E_QuestUIState.Complete)
		{
			CompleteItem();
		}
		else
        {
            if (_questAchieve.QuestTable.CompleteCheck == E_CompleteCheck.Level && ZNet.Data.Me.FindCurCharData != null)
			{
				RefreshItem(ZNet.Data.Me.CurCharData.LastLevel, _questAchieve.QuestTable.TargetLevel);
			}
			else
			{
				RefreshItem(_questAchieve.CurrentCount, _questAchieve.MaxCount);
			}
		}
	}

    //-----------------------------------------------------------------
	private void RewardItemInfo(Quest_Table _questTable)
	{
		mQuestID = _questTable.QuestID;

		Sprite iconSprite = ZManagerUIPreset.Instance.GetSprite(_questTable.ChapterIcon);
		if (iconSprite)
		{
			QuestIcon.sprite = iconSprite;
		}
		 
		QuestTitle.text = _questTable.QuestTextID;
		QuestDescription.text = _questTable.QuestSimpleText;
		ScrollReward.DoUIQuestScrollReward(_questTable, UIQuestScrollReward.E_UIRewardType.Achieve, false);
	
	}

    private void CompleteItem()
	{
		mCanvasGroup.alpha = 0.25f;
		Progress.gameObject.SetActive(false);
		Complete.gameObject.SetActive(true);
		Complete.interactable = false;
		QuestAlram.gameObject.SetActive(false);
		mUIFrameQuest.DoUIQuestAlarmFocus(UIFrameQuest.E_QuestCategory.Achievement, false);
	}

	private void RewardItem()
	{
		mCanvasGroup.alpha = 1f;
		Progress.gameObject.SetActive(false);
		Complete.gameObject.SetActive(true);
		Complete.interactable = true;
		QuestAlram.gameObject.SetActive(true);
		mUIFrameQuest.DoUIQuestAlarmFocus(UIFrameQuest.E_QuestCategory.Achievement, true);
	}

	private void RefreshItem(uint _count, uint _countMax)
	{
		mCanvasGroup.alpha = _count == 0 ? 0.25f : 1f;
        Progress.gameObject.SetActive(true);
		Complete.gameObject.SetActive(false);

		if (_countMax == 0) _countMax = 1;

		float rate = _count / _countMax;
		Progress.size = rate;
		ProgressCount.text = string.Format("{0} / {1}", _count.ToString(), _countMax.ToString());
	}
	//----------------------------------------------------------------
	public void HandleRequestReward()
	{
		UIFrameQuest frameQuest = mUIFrameParent as UIFrameQuest;
		frameQuest.DoUIQuestRewardPopup(mQuestID, UIQuestAccepRewardPopup.E_RewardType.Reward, 0);
	}
}
