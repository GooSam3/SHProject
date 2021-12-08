using GameDB;
using UnityEngine;

public class UIQuestScrollRewardItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] private ZUIIconNormal Icon;
	[SerializeField] float DelayInterval = 2f;

	private UIQuestAccepRewardPopup mRewardOwner = null;
	private int mSlotIndex = 0;
	//-------------------------------------------------------------------
	public void DoRewardItem(Item_Table _itemInfo,  uint _count, int _slotIndex, UIQuestScrollReward.E_UIRewardType _rewardType)
	{
		if (_itemInfo == null) return;
		mSlotIndex = _slotIndex;
		Icon.DoUIIconSetting(_itemInfo, _count);

		if (_rewardType == UIQuestScrollReward.E_UIRewardType.Select)
		{
			Icon.DoUIIconSelect(true);
		}
		else
		{
			Icon.DoUIIconSelect(false);
		}

		if (_rewardType == UIQuestScrollReward.E_UIRewardType.Sub_Random_FrontSide)
		{
			ZUIIconRotation iconRotation = Icon as ZUIIconRotation;
			if (iconRotation)
			{
				iconRotation.DoUIIconRotationEnd();
			} 
		}
		else if (_rewardType == UIQuestScrollReward.E_UIRewardType.Sub_Random_BackSide)
		{
			ZUIIconRotation iconRotation = Icon as ZUIIconRotation;
			if (iconRotation)
			{
				iconRotation.SetUIIConRotationSelectEvent(HandleRewardItemRandomSelect);				
			}
		}
	}

	public void SetRewardItemOwner(UIQuestAccepRewardPopup _rewardOwner)
	{
		mRewardOwner = _rewardOwner;
	}

	public void DoRewardItemRotation(bool _forward, bool _delayRotation, bool _selectItem)
	{
		ZUIIconRotation iconRotation = Icon as ZUIIconRotation;
		if (iconRotation)
		{
			if (_forward)
			{
				if (_delayRotation)
				{
					iconRotation.DoUIIconRotationForward(DelayInterval);
				}
				else
				{
					iconRotation.DoUIIconRotationForward(0);
				}
			}
			else
			{
				if (_delayRotation)
				{
					iconRotation.DoUIIconRotationBackward(DelayInterval);
				}
				else
				{
					iconRotation.DoUIIconRotationBackward(0);
				}
			}
		}
		Icon.DoUIIconSelect(_selectItem);
	}


	public void DoRewardItemCompleteCheck(bool _check)
	{
		Icon.DoUIIconCheck(_check);
	}

	//----------------------------------------------------------------
	private void HandleRewardItemRandomSelect(uint _itemID)
	{
		Icon.SetIconInputEnable(false);
		mRewardOwner?.HandleRewardPopupRewardDelay(_itemID);
	}
}
