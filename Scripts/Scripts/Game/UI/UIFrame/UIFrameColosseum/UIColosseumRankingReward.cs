using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System.Linq;

public class UIColosseumRankingReward : MonoBehaviour
{
	[SerializeField] private ScrollRect scrollRect;
	[SerializeField] private UIColosseumRankingRewardItem originalItem;
	[SerializeField] private ZText mainTitle;
	[SerializeField] private ZText gredeTitle;
	[SerializeField] private ZText pointTitle;
	[SerializeField] private ZText rankingTitle;
	[SerializeField] private ZText winRewardTitle;
	[SerializeField] private ZText seasonRewardTitle;

	private List<UIColosseumRankingRewardItem> colosseumRankingItemList = new List<UIColosseumRankingRewardItem>();

	public void Initialize()
	{
		gameObject.SetActive(false);

		originalItem.gameObject.SetActive(false);

		mainTitle.text = DBLocale.GetText("WPvP_DuelRanking_Title");
		gredeTitle.text = DBLocale.GetText("WPvP_DuelRanking_Grade");
		pointTitle.text = DBLocale.GetText("WPvP_DuelRanking_Point");
		rankingTitle.text = DBLocale.GetText("WPvP_DuelRanking_Ranking");
		winRewardTitle.text = DBLocale.GetText("Colosseum_WinReward");
		seasonRewardTitle.text = DBLocale.GetText("Colosseum_SeasonReward");
	}

	public void Open(bool isResetPos)
	{
		this.gameObject.SetActive(true);

		var colosseumTableList = DBColosseum.DicColosseum.Values.ToList();
		colosseumTableList.Sort((a, b) => {
			return b.Grade.CompareTo(a.Grade);
		});

		for (int i = 0; i < colosseumRankingItemList.Count; i++) {
			colosseumRankingItemList[i].gameObject.SetActive(false);
		}

		int loadItemCount = colosseumTableList.Count - colosseumRankingItemList.Count;
		for (int i = 0; i < loadItemCount; i++) {
			var obj = GameObject.Instantiate(originalItem);
			obj.transform.SetParent(scrollRect.content.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			colosseumRankingItemList.Add(obj);
			obj.gameObject.SetActive(false);
		}

		if (isResetPos == true) {
			scrollRect.content.anchoredPosition = Vector2.zero;
			scrollRect.velocity = Vector2.zero;
		}

		for (int i = 0; i < colosseumTableList.Count; i++) {
			var item = colosseumRankingItemList[i];
			item.DoUpdate(colosseumTableList[i].Grade, null);
		}
	}

	public void OnClose()
	{
		this.gameObject.SetActive(false);
	}

}
