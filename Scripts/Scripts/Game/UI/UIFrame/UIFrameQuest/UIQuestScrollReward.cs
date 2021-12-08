using GameDB;
using System.Collections.Generic;

public class UIQuestScrollReward : CUGUIScrollRectBase
{
	public enum E_UIRewardType
	{
		None,
		Main,
		Sub,
		Sub_Random_BackSide,
		Sub_Random_FrontSide,
		Temple,
		Achieve,
		Select,
	}
	private UIQuestAccepRewardPopup mRewardPopupOwner = null;
	//-------------------------------------------------------------------
	public void DoUIQuestScrollReward(Quest_Table _questTable, E_UIRewardType _uiRewardType, bool _showSelectedItem, int _selectedItemIdx = -1)
	{
		ProtUIScrollSlotItemReturnAll();

		if (_uiRewardType == E_UIRewardType.Sub_Random_FrontSide || _uiRewardType == E_UIRewardType.Sub_Random_BackSide)
		{
			if (_questTable.RandomRewardDropGroup != 0)
			{
				List<ListGroup_Table> listDropGroup = DBGacha.GetCachaListGroupID(_questTable.RandomRewardDropGroup);

				if (listDropGroup == null)
				{
					ZLog.LogError(ZLogChannel.UI, $"[UIQuest Reward] reward table invalid. QuestID : {_questTable.QuestID} / Group : { _questTable.RandomRewardDropGroup }");
					return;
				}

				SetMonoActive(true);
				MakeRewardRandomItemList(listDropGroup, _uiRewardType);
			}
			else
			{
				SetMonoActive(false); 
			}
		}
		else
		{
			MakeRewardExp(_questTable.RewardExp);
			MakeRewardGold(_questTable.RewardGoldCount);
			MakeRewardItemList(_questTable.RewardItemID, _questTable.RewardItemCount, _uiRewardType);
			MakeRewardChangeID(_questTable.RewardChangeID);
			MakeRewardPetID(_questTable.RewardPetID);
			MakeRewardAbility(_questTable.RewardAbilityID, _questTable.RewardAbilityCount);
//			MakeRewardTempleStage(_questTable.TempleStageID);

			if (_showSelectedItem)
			{
				MakeRewardSelect(_questTable.SelectRewardItemID, _questTable.SelectRewardItemCount, _selectedItemIdx);
			}
		}
	}

	public void DoUIQuestScrollRewardArrange(int _choiceSlotIndex, List<uint> _listItemID, List<uint> _listItemCount)
	{
		ProtUIScrollSlotItemReturnAll();

		for (int i = 0; i < _listItemID.Count; i++)
		{
			UIQuestScrollRewardItem item = MakeRewardItem(_listItemID[i], _listItemCount[i], i, E_UIRewardType.Sub_Random_BackSide);
			if (i == _choiceSlotIndex)
			{
				item.DoRewardItemRotation(true, false, true);
			}
			else
			{
				item.DoRewardItemRotation(true, true, false);
			}
		}
	}

	public void DoUIQuestScrollRewardItemCheck(bool _check)
	{
		List<UIQuestScrollRewardItem> listItem = ExtractUIScrollSlotItemList<UIQuestScrollRewardItem>();
		for (int i = 0; i < listItem.Count; i++)
		{
			listItem[i].DoRewardItemCompleteCheck(_check);
		}
	}

	//-------------------------------------------------------------------
	private void MakeRewardExp(uint _value)
	{
		if (_value == 0) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table expItem = null;
		DBItem.GetItem(1900, out expItem);
		item.DoRewardItem(expItem, _value, 0, E_UIRewardType.None);
	}

	private void MakeRewardGold(uint _value)
	{
		if (_value == 0) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table goldItem = null;
		DBItem.GetItem(2000, out goldItem);
		item.DoRewardItem(goldItem, _value, 0, E_UIRewardType.None);
	}

	private void MakeRewardItemList(List<uint> _itemList, List<uint> _itemCount, E_UIRewardType _rewardType)
	{
		for (int  i = 0; i < _itemList.Count; i++)
		{
			MakeRewardItem(_itemList[i], _itemCount[i], i, _rewardType);
		}
	}

	private void MakeRewardSelect(List<uint> _itemList, List<uint> _itemCount, int _selectedItemIdx)
	{
		if (_selectedItemIdx >= 0)
		{
			if (_selectedItemIdx < _itemList.Count)
			{
				MakeRewardItem(_itemList[_selectedItemIdx], _itemCount[_selectedItemIdx], 0, E_UIRewardType.Select);
			}
		}
		else
		{
			MakeRewardItemList(_itemList, _itemCount, E_UIRewardType.Select);
		}
	}

	private void MakeRewardChangeID(uint _changeID)
	{
		if (_changeID == 0) return;

		Change_Table changeTable = null;
		DBChange.TryGet(_changeID, out changeTable);

		if (changeTable == null) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table itemTable = new Item_Table();
		itemTable.IconID = changeTable.Icon;
		itemTable.ItemTextID = changeTable.ChangeTextID;
		item.DoRewardItem(itemTable, 0, 0, E_UIRewardType.None);
	}

	private void MakeRewardPetID(uint _ID)
	{
		if (_ID == 0) return;

		Pet_Table TableInfo = null;
		DBPet.TryGet(_ID, out TableInfo);

		if (TableInfo == null) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table itemTable = new Item_Table();
		itemTable.IconID = TableInfo.Icon;
		itemTable.ItemTextID = TableInfo.PetTextID;
		item.DoRewardItem(itemTable, 0, 0, E_UIRewardType.None);
	}

	private void MakeRewardAbility(uint _abilityID, uint _count)
	{
		if (_abilityID == 0) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Ability_Table abilityTable = null;
		DBAbility.GetAbility((E_AbilityType)_abilityID, out abilityTable);

		if (abilityTable == null) return;

		Item_Table itemTable = new Item_Table();
		itemTable.IconID = abilityTable.AbilityIcon;
		itemTable.ItemTextID = abilityTable.StringName;

		item.DoRewardItem(itemTable, _count, 0, E_UIRewardType.None);
	}

	private void MakeRewardTempleStage(uint _ID)
	{
		if (_ID == 0) return;

		Temple_Table TableInfo = null;
		DBTemple.TryGet(_ID, out TableInfo);

		if (TableInfo == null) return;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table itemTable = new Item_Table();
		itemTable.IconID = TableInfo.Icon;
		itemTable.ItemTextID = TableInfo.TempleAreaName;
		item.DoRewardItem(itemTable, 0, 0,  E_UIRewardType.None);
	}

	private UIQuestScrollRewardItem MakeRewardItem(uint _ID, uint _count, int _slotIndex, E_UIRewardType _rewardType)
	{
		if (_ID == 0) return null;

		UIQuestScrollRewardItem item = CreateRewardSlot();
		Item_Table itemTable = null;
		DBItem.GetItem(_ID, out itemTable);
		item.DoRewardItem(itemTable, _count, _slotIndex, _rewardType);
		return item;
	}

	private void MakeRewardRandomItemList(List<ListGroup_Table> _itemList, E_UIRewardType _rewardType)
	{
		if (_itemList == null) return;

		for (int i = 0; i < _itemList.Count; i++)
		{
			UIQuestScrollRewardItem item = MakeRewardItem(_itemList[i].ItemID, _itemList[i].ItemCount, i, _rewardType);
			if (_rewardType == E_UIRewardType.Sub_Random_BackSide)
			{
				item.DoRewardItemRotation(false, true, false);
			}
		}
	}

	private UIQuestScrollRewardItem CreateRewardSlot()
	{
		UIQuestScrollRewardItem item = ProtUIScrollSlotItemRequest() as UIQuestScrollRewardItem;
		item.SetRewardItemOwner(mRewardPopupOwner);
		return item;
	}

	//------------------------
	public void SetScrollRewardOwner(UIQuestAccepRewardPopup _rewardPopup)
	{
		mRewardPopupOwner = _rewardPopup;
	}

}
