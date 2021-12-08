using GameDB;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIQuestAccepRewardPopup : CUGUIWidgetBase
{
	public enum E_RewardType
	{
		Confirm,    // 확인 버튼만 나오고 종료 		
		Accept,     // 수락 버튼 나오고 수락 요청 	
		Reward,    // 보상 버튼. 
	}

	[SerializeField] ZImage QuestIcon = null;
	[SerializeField] ZText QuestTitle = null;
	[SerializeField] ZText QuestDesc = null;
	[SerializeField] CWidgetContainer QuestType = null;
	[SerializeField] GameObject RewardSingle = null;
	[SerializeField] GameObject RewardRandom = null;
	[SerializeField] GameObject ButtonAcceptCancle = null;
	[SerializeField] GameObject ButtonReward = null;
	[SerializeField] GameObject ButtonConfirm = null;
	[SerializeField] UIQuestScrollReward ScrollBasic = null;
	[SerializeField] UIQuestScrollReward ScrollMultyBasic = null;
	[SerializeField] UIQuestScrollReward ScrollMultyRandom = null;
	[SerializeField] ZButton UIButtonAccept = null;
	[SerializeField] ZButton UIButtonCancel = null;
	[SerializeField] ZButton UIButtonReward = null;
	[SerializeField] ZButton UIButtonConfirm = null;

	private bool mSendPacket = false;
	private uint mSelectedItemIdx = 0;
	private int	mChoiceSlotIdx = 0;
	private UIFrameQuest mUIFrameQuest = null;
	private uint mQuestTID = 0;
	private UnityAction mEventRandomRewardEnd = null;
	private Quest_Table mQuestTable = null;
	private E_RewardType mRewardType = E_RewardType.Accept;
	//---------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		RewardSingle.SetActive(false);
		RewardRandom.SetActive(false);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
		ScrollBasic.SetScrollRewardOwner(this);
		ScrollMultyBasic.SetScrollRewardOwner(this);
		ScrollMultyRandom.SetScrollRewardOwner(this);
	}

	//---------------------------------------------------------------
	public void DoUIRewardPopup(uint _questTID, E_RewardType _rewardType, int _selectedRandomReward)
	{
		mSendPacket = false;
		mQuestTID = _questTID;
		mSelectedItemIdx = _selectedRandomReward < 0 ? 0 : (uint)_selectedRandomReward;
		mQuestTable = DBQuest.GetQuestData(_questTID);
		if (mQuestTable == null) return; 
		mRewardType = _rewardType;

		ProcessRewardType(mRewardType, mQuestTable, _selectedRandomReward);
	}

	public void DoUIRewardRandomPlay(List<uint> _listItemID, List<uint> _listItemCount, UnityAction _processEnd)
	{
		mEventRandomRewardEnd = _processEnd;

		List<ListGroup_Table> listListGroup = DBGacha.GetCachaListGroupID(mQuestTable.RandomRewardDropGroup);
		List<uint> listSuffleItemID = new List<uint>();
		List<uint> listSuffleItemCount = new List<uint>();

		int rewardIndex = 0;
		for (int i = 0; i < listListGroup.Count; i++)
		{

			for (int j = 0; j < _listItemID.Count; j++)
			{
				if (listListGroup[i].ItemID == _listItemID[j])
				{
					rewardIndex = i;
				}
			}

			listSuffleItemID.Add(listListGroup[i].ItemID);
			listSuffleItemCount.Add(listListGroup[i].ItemCount);
		}

		uint swapValue = listSuffleItemID[mChoiceSlotIdx];
		listSuffleItemID[mChoiceSlotIdx] = listSuffleItemID[rewardIndex];
		listSuffleItemID[rewardIndex] = swapValue;

		swapValue = listSuffleItemCount[mChoiceSlotIdx];
		listSuffleItemCount[mChoiceSlotIdx] = listSuffleItemCount[rewardIndex];
		listSuffleItemCount[rewardIndex] = swapValue;

		// X번 섞어준다. 
		for (int i = 0; i < listSuffleItemID.Count; i++)
		{
			int swapFirst = Random.Range(0, listSuffleItemID.Count);
			int swapSecond = Random.Range(0, listSuffleItemID.Count);

			if (swapFirst != mChoiceSlotIdx && swapSecond != mChoiceSlotIdx)
			{
				swapValue = listSuffleItemID[swapSecond];
				listSuffleItemID[swapSecond] = listSuffleItemID[swapFirst];
				listSuffleItemID[swapFirst] = swapValue;

				swapValue = listSuffleItemCount[swapSecond];
				listSuffleItemCount[swapSecond] = listSuffleItemCount[swapFirst];
				listSuffleItemCount[swapFirst] = swapValue;
			}
		}

		UIButtonConfirm.interactable = true;
		ScrollMultyRandom.DoUIQuestScrollRewardArrange(mChoiceSlotIdx, listSuffleItemID, listSuffleItemCount);
	}
	//-----------------------------------------------------------------
	private void ProcessRewardType(E_RewardType _rewardType, Quest_Table _questTable, int _selectedRandomReward)
	{
		if (_rewardType == E_RewardType.Confirm)
		{
			if (_questTable.UIQuestConfirmTarget != 0)
			{
				Quest_Table confirmQuest = DBQuest.GetQuestData(_questTable.UIQuestConfirmTarget);
				if (confirmQuest != null)
				{
					gameObject.SetActive(true);
					ShowReward(_selectedRandomReward, _rewardType, confirmQuest);
				}
				else
				{
					gameObject.SetActive(false);
					SkipRewardPopup(_rewardType);
				}
			}
			else
			{
				gameObject.SetActive(false);
				SkipRewardPopup(_rewardType);
			}
		}
		else if (_rewardType == E_RewardType.Reward)
		{
			if (HasRewardItem(mQuestTable))
			{
				gameObject.SetActive(true);
				ShowReward(_selectedRandomReward, _rewardType, mQuestTable);
			}
			else
			{
				gameObject.SetActive(false);
				SkipRewardPopup(_rewardType);
			}
		}
		else if (_rewardType == E_RewardType.Accept)
		{
			gameObject.SetActive(true);
			ShowReward(_selectedRandomReward, _rewardType, mQuestTable);
		}
	}

	private void ShowReward(int _selectedRandomReward, E_RewardType _rewardType, Quest_Table _showQuestTable)
	{
		if (_showQuestTable.RandomRewardDropGroup != 0)
		{
			ShowRewardRandom(_showQuestTable, _selectedRandomReward, _rewardType);
			ShowRewardButton(_rewardType, true);
		}
		else
		{
			ShowRewardSingle(_showQuestTable, _selectedRandomReward);
			ShowRewardButton(_rewardType, false);
		}
		ShowQuestInfo(_showQuestTable);
	}

	private void SkipRewardPopup(E_RewardType _rewardType)
	{
		if (_rewardType == E_RewardType.Accept)
		{
			HandleRewardPopupAccept();
		}
		else if (_rewardType == E_RewardType.Confirm)
		{			
			HandleRewardPopupConfirm();
		}
		else if (_rewardType == E_RewardType.Reward)
		{
			HandleRewardPopupReward();
		}
	}

	private void ShowRewardSingle(Quest_Table _questTable, int _selectItemIdx)
	{
		RewardSingle.SetActive(true);
		RewardRandom.SetActive(false);

		ScrollBasic.DoUIQuestScrollReward(_questTable, UIQuestScrollReward.E_UIRewardType.Main, true, _selectItemIdx);
	}

	private void ShowRewardRandom(Quest_Table _questTable, int _selectItemIdx, E_RewardType _rewardType)
	{
		RewardSingle.SetActive(false);
		RewardRandom.SetActive(true);
		ScrollMultyBasic.DoUIQuestScrollReward(_questTable, UIQuestScrollReward.E_UIRewardType.Sub, true, _selectItemIdx);

		if (_rewardType == E_RewardType.Accept)
		{
			ScrollMultyRandom.DoUIQuestScrollReward(_questTable, UIQuestScrollReward.E_UIRewardType.Sub_Random_FrontSide, true);
		}
		else 
		{
			ScrollMultyRandom.DoUIQuestScrollReward(_questTable, UIQuestScrollReward.E_UIRewardType.Sub_Random_BackSide, true);
		}
	}

	private void ShowRewardButton(E_RewardType _rewardType, bool _randomReward)
	{
		UIButtonAccept.interactable = true;
		UIButtonCancel.interactable = true;
		UIButtonReward.interactable = true;
		UIButtonConfirm.interactable = true;

		ButtonAcceptCancle.SetActive(false);
		ButtonReward.SetActive(false);
		ButtonConfirm.SetActive(false);
		
		if (_rewardType == E_RewardType.Accept)
		{
			ButtonAcceptCancle.SetActive(true);
			QuestType.SwitchContainerObject(E_RewardType.Accept.ToString());
		}
		else if (_rewardType == E_RewardType.Confirm)
		{
			ButtonConfirm.SetActive(true);
			QuestType.SwitchContainerObject(E_RewardType.Confirm.ToString());
		}
		else if (_rewardType == E_RewardType.Reward)
		{
			if (_randomReward)
			{
				ButtonConfirm.SetActive(true);
				UIButtonConfirm.interactable = false;
			} 
			else
			{
				ButtonReward.SetActive(true);
			}
			QuestType.SwitchContainerObject(E_RewardType.Reward.ToString());
		} 
	}   

	private void ShowQuestInfo(Quest_Table _questTable)
	{
		Sprite spriteIcon = ZManagerUIPreset.Instance.GetSprite(_questTable.ChapterIcon);
		if (spriteIcon)
		{
			QuestIcon.sprite = spriteIcon;
		}

		QuestTitle.text = _questTable.QuestTextID;
		QuestDesc.text = _questTable.QuestSimpleText;
	}

	private void SendQuestAcceptPacket(Quest_Table _questTable)
	{
		if (mSendPacket) return;
		mSendPacket = true;
		ZWebManager.Instance.WebGame.REQ_QuestAccept(_questTable.QuestID, (questTID) => {
			mSendPacket = false;

			if (_questTable.UIQuestType == E_UIQuestType.Main)
			{
				mUIFrameQuest.DoUIQuestDialog(mQuestTID, UIQuestDialog.E_DialogType.Confirm);
			}
			else if (_questTable.UIQuestType == E_UIQuestType.Sub)
			{
				mUIFrameQuest.DoUIQuestDialog(mQuestTID, UIQuestDialog.E_DialogType.Confirm);
			}
			else if (_questTable.UIQuestType == E_UIQuestType.Temple)
			{
				mUIFrameQuest.DoUIQuestDialog(mQuestTID, UIQuestDialog.E_DialogType.Confirm);
			}
		});
	} 

	private bool HasRewardItem(Quest_Table _questTable)
	{
		bool hasReward = true;
		
		if (_questTable.RewardGoldCount == 0 && _questTable.RewardExp == 0 && _questTable.RewardItemID.Count == 0 
			&& _questTable.RewardPetID == 0 && _questTable.SelectRewardItemID.Count == 0 && _questTable.RandomRewardDropGroup == 0 && _questTable.RewardAbilityID == 0
			&& _questTable.SelectRewardItemID.Count == 0)
		{
			hasReward = false;
		}

		return hasReward;
	}

	

	//----------------------------------------------------------------------------------
	public void HandleRewardPopupAccept()
	{
		SendQuestAcceptPacket(mQuestTable);
	}

	public void HandleRewardPopupCancle()
	{
		mUIFrameQuest.Close();
	}

	public void HandleRewardPopupReward()
	{
		if (mSendPacket) return;
		mSendPacket = true;

		UIButtonReward.interactable = false;
		ZWebManager.Instance.WebGame.REQ_QuestReward(mQuestTID, mSelectedItemIdx, (itemList) =>
		{
			mSendPacket = false;		
		});
	}

	public void HandleRewardPopupConfirm()
	{
		if (mEventRandomRewardEnd != null)
		{
			mEventRandomRewardEnd.Invoke();
			mEventRandomRewardEnd = null;
		}

		mUIFrameQuest.Close();
		

		if (mQuestTable.CompleteCheck == E_CompleteCheck.Tutorial)
		{
			TutorialSystem.Instance.StartTutorial(mQuestTID);
		}
	}

	public void HandleRewardPopupRewardDelay(uint _itemID)
	{
		if (mSendPacket) return;

		List<ListGroup_Table> listListGroup = DBGacha.GetCachaListGroupID(mQuestTable.RandomRewardDropGroup);
		for (int i = 0; i < listListGroup.Count; i++)
		{
			if (listListGroup[i].ItemID == _itemID)
			{
				mChoiceSlotIdx = i;
				break;
			}
		}

		mSendPacket = true;
		UIButtonReward.interactable = false;
		ZWebManager.Instance.WebGame.REQ_QuestReward(mQuestTID, mSelectedItemIdx, (itemList) =>
		{
			mSendPacket = false;
		}, true);
	}


}
