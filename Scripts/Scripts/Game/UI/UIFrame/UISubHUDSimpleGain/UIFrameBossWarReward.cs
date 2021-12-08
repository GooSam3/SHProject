using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFrameBossWarReward : MonoBehaviour
{
	[SerializeField] private ZText Title;
	[SerializeField] private ZText Description;
	[SerializeField] private ZText RewardTitle;
	[SerializeField] private ZText RewardButton;
	[SerializeField] private UIBossWarGradeRewardListItem[] RewardItems;

	private Action EventClose;

	public void Init(BossWar_Table reward, Action action)
	{
		EventClose += action;

		foreach(var item in RewardItems)
		{
			item.gameObject.SetActive(false);
		}

		for(int i = 0; i < reward.RewardID.Count; i++)
		{
			RewardItems[i].gameObject.SetActive(true);
			RewardItems[i].DoUpdate(reward.RewardID[i], reward.RewardCount[i], OnRewardListItemClick);
		}
	}

	private void OnRewardListItemClick(uint itemTid)
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => {
			var popupItemInfo = obj.GetComponent<UIPopupItemInfo>();
			var frame = UIManager.Instance.Find<UISubHudBossWarInfo>();

			if (frame != null)
			{
				frame.SetInfoPopup(popupItemInfo);

				popupItemInfo.transform.SetParent(frame.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemTid, () => {
					frame = UIManager.Instance.Find<UISubHudBossWarInfo>();
					if (frame != null)
					{
						frame.RemoveInfoPopup();
					}
				});
			}
		});
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
		EventClose?.Invoke();
		EventClose = null;
	}
}
