using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIColosseumRanking : MonoBehaviour, ITabContents
{
	[SerializeField] private ScrollRect stageScrollRect;
	[SerializeField] private UIColosseumRankingItemAdapter ScrollAdapter;
	[SerializeField] private UIColosseumRankingItem rankingItem;
	[SerializeField] private UIColosseumRankingItem myRankingItem;

	public uint SelectStageTid { get; private set; }

	public int Index { get; set; }

	public void Initialize()
	{
		ScrollAdapter.Parameters.ItemPrefab = rankingItem.GetComponent<RectTransform>();
		var pf = ScrollAdapter.Parameters.ItemPrefab;
		pf.SetParent(transform);
		pf.localScale = Vector2.one;
		pf.localPosition = Vector3.zero;
		pf.gameObject.SetActive(false);
		ScrollAdapter.Initialize();
	}

	public void Open()
	{
		this.gameObject.SetActive(true);

		RefreshSelf(true);
	}

	public void Refresh()
	{
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
	}

	private void RefreshSelf(bool bReset)
	{
		DoStageListUpdate(bReset);

		Me.CurCharData.ColosseumContainer.REQ_GetColosseumRankList((rankingList) => {
			ScrollAdapter.SetNormalizedPosition(1);
			ScrollAdapter.Refresh(rankingList);
			ShowMyRanking(rankingList);
		});
	}

	private void ShowMyRanking(List<ColosseumRankInfoConverted> rankingList)
	{
		var myRank = rankingList.Find(v => v.CharId == Me.CharID);
		if (myRank != null) {
			myRankingItem.SetData(myRank);
		}
		else {
			myRankingItem.SetDataMe_NoRank();
		}
	}

	private void DoStageListUpdate(bool isResetPos)
	{
		var towerList = DBStage.GetStageList(E_StageType.Colosseum);
		towerList.Sort((a, b) => {
			return a.StageID.CompareTo(b.StageID);
		});

		List<GameObject> stageGoList = stageScrollRect.gameObject.FindChildren("ColosseumRanking_Slot");
		for (int i = 0; i < stageGoList.Count; ++i) {
			stageGoList[i].SetActive(false);
		}

		for (int i = 0; i < towerList.Count; i++) {

			// 선택된 던전이 없을때 루프의 최초 item 으로 선택
			if (SelectStageTid == 0) {
				SelectStageTid = towerList[i].StageID;
			}

			bool toggleOn = SelectStageTid == towerList[i].StageID;

			var item = stageGoList[i].GetComponent<UIColosseumStageItem>();
			item.DoRefresh(towerList[i].StageID, StageListItemClicked, toggleOn);
		}

		if (isResetPos == true) {
			stageScrollRect.content.anchoredPosition = Vector2.zero;
			stageScrollRect.velocity = Vector2.zero;
		}
	}

	// 버튼 이벤트

	private void StageListItemClicked(uint stageTid)
	{
		SelectStageTid = stageTid;

		RefreshSelf(true);
	}
}
