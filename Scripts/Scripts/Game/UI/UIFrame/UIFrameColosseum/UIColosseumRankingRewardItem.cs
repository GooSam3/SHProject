using GameDB;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIColosseumRankingRewardItem : MonoBehaviour
{
	[SerializeField] private ZImage gradeImage;
	[SerializeField] private ZText scoreText;
	[SerializeField] private ZText rankText;

	[SerializeField] private UITowerRewardListItem winRewardItem;
	[SerializeField] private UITowerRewardListItem[] seasonRewardItemList;

	private uint grade;
	private Action<uint> clickEvent = null;

	public void DoUpdate(uint _grade, Action<uint> _clickEvent)
	{
		this.gameObject.SetActive(true);

		grade = _grade;
		clickEvent = _clickEvent;

		var colosseumTable = DBColosseum.Get(grade);

		gradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(colosseumTable.GradeIcon);
		scoreText.text = string.Format(DBLocale.GetText("WPvP_DuelRanking_PointRange"), colosseumTable.ColosseumPoint);

		if (colosseumTable.Rank > 0) {
			rankText.text = string.Format(DBLocale.GetText("WPvP_DuelRanking_RankingRange"), colosseumTable.Rank, "");
		}
		else {
			rankText.text = string.Empty;
		}

		winRewardItem.DoUpdate(colosseumTable.WinRewardItem[0], colosseumTable.WinRewardCnt[0], OnRewardListItemClick);

		var seasonRewardTable = DBColosseum.GetSeasenReward(_grade);

		for (int i = 0; i < seasonRewardItemList.Length; i++) {
			if (seasonRewardTable.SeasonEndRewardItem.Count > i) {
				seasonRewardItemList[i].gameObject.SetActive(true);
				seasonRewardItemList[i].DoUpdate(seasonRewardTable.SeasonEndRewardItem[i], seasonRewardTable.SeasonEndRewardCnt[i], OnRewardListItemClick);
			}
			else {
				seasonRewardItemList[i].gameObject.SetActive(false);
			}
		}
	}

	public void UIListItemClick()
	{
		ZLog.Log(ZLogChannel.Default, $"UIListItemClick Grade:{grade}");

		clickEvent?.Invoke(grade);
	}

	private void OnRewardListItemClick(uint itemTid)
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => {
			var popupItemInfo = obj.GetComponent<UIPopupItemInfo>();
			var frame = UIManager.Instance.Find<UIFrameColosseum>();

			if (frame != null) {
				frame.SetInfoPopup(popupItemInfo);

				popupItemInfo.transform.SetParent(frame.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemTid, () => {
					frame = UIManager.Instance.Find<UIFrameColosseum>();
					if (frame != null) {
						frame.RemoveInfoPopup();
					}
				});
			}
		});
	}
}
