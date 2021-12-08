using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarRewardInfoListItem : MonoBehaviour
{
	[SerializeField] private ZImage RankImage;
	[SerializeField] private ZText DamageText;
	[SerializeField] private UIBossWarGradeRewardListItem[] RewardItems;

    public void SetData(BossWar_Table data)
	{
		foreach(var item in RewardItems)
		{
			item.gameObject.SetActive(false);
		}

		RankImage.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
		DamageText.text = DBLocale.GetText("WBossWar_AttendReward_PointRange", data.MinDamage);

		for(int i = 0; i < data.RewardID.Count; i++)
		{
			RewardItems[i].gameObject.SetActive(true);
			RewardItems[i].DoUpdate(data.RewardID[i], data.RewardCount[i], OnRewardListItemClick);
		}
	}

	private void OnRewardListItemClick(uint itemTid)
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => {
			var popupItemInfo = obj.GetComponent<UIPopupItemInfo>();
			var frame = UIManager.Instance.Find<UIFrameDungeon>();

			if (frame != null)
			{
				frame.SetInfoPopup(popupItemInfo);

				popupItemInfo.transform.SetParent(frame.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemTid, () => {
					frame = UIManager.Instance.Find<UIFrameDungeon>();
					if (frame != null)
					{
						frame.RemoveInfoPopup();
					}
				});
			}
		});
	}
}
