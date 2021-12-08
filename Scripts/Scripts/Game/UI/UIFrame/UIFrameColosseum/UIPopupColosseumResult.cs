using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

public class UIPopupColosseumResult : UIPopupBase
{
	public class ColosseumReward
	{
		public Item_Table ItemTable;
		public uint Count;
	}

	public class ResultInfo
	{
		public Colosseum_Table ColosseumTable;
		public byte IsWin;
		public uint OldScore;
		public uint OldRank;
		public uint Score;
		public uint Rank;
	}

	[SerializeField] private ZText gradeText;
	[SerializeField] private ZImage gradeImage;
	[SerializeField] private ZText scoreText;
	[SerializeField] private ZText rankText;
	[SerializeField] private UITowerRewardListItem originalRewardItem;
	[SerializeField] private ZText rewardName;
	[SerializeField] private ZText gameEndName;
	[SerializeField] private ZText regameName;
	[SerializeField] private GameObject victoryObj;
	[SerializeField] private GameObject loseObj;

	private List<UITowerRewardListItem> rewardListItems = new List<UITowerRewardListItem>();
	private ResultInfo resultInfo = null;

	protected override void OnInitialize()
	{
		originalRewardItem.gameObject.SetActive(false);

		rewardName.text = DBLocale.GetText("WPvP_Duel_ResultReward");
		regameName.text = DBLocale.GetText("Colosseum_Retry");
		gameEndName.text = DBLocale.GetText("Colosseum_Exit");
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		AudioManager.Instance.PlaySFX(30003); //클리어사운드
	}

	public void SetResult(ResultInfo _resultInfo)
	{
		resultInfo = _resultInfo;

		// 승리/패배 텍스트 연출
		if (resultInfo.IsWin == 1) {
			victoryObj.SetActive(true);
		}
		else {
			loseObj.SetActive(true);
		}

		// 점수 및 랭킹표시
		scoreText.text = string.Format("{0}", _resultInfo.Score);
		rankText.text = string.Format("{0}", _resultInfo.Rank);
		gradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(_resultInfo.ColosseumTable.GradeIcon);
		gradeText.text = DBLocale.GetText(_resultInfo.ColosseumTable.GradeName);

		// 보상 정보 세팅
		ShowRewardItemList();
	}

	protected override void OnHide()
	{
		resultInfo = null;

		victoryObj.SetActive(false);
		loseObj.SetActive(false);
	}

	private void ShowRewardItemList()
	{
		for (int i = 0; i < rewardListItems.Count; i++) {
			rewardListItems[i].gameObject.SetActive(false);
		}

		List<ColosseumReward> rewardItemList = SearchRewardItemList();

		int loadItemCount = rewardItemList.Count - rewardListItems.Count;
		for (int i = 0; i < loadItemCount; i++) {
			var obj = Instantiate(originalRewardItem);
			obj.transform.SetParent(originalRewardItem.transform.parent);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			rewardListItems.Add(obj);
			obj.gameObject.SetActive(false);
		}

		for (int i = 0; i < rewardItemList.Count; i++) {
			var item = rewardListItems[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(rewardItemList[i].ItemTable.ItemID, rewardItemList[i].Count, RewardListItemClicked);
		}
	}

	private List<ColosseumReward> SearchRewardItemList()
	{
		var colosseumTable = resultInfo.ColosseumTable;
		List<ColosseumReward> rewardList = new List<ColosseumReward>();

		if (resultInfo.IsWin == 1) {

			// 승리보상
			for (int i = 0; i < colosseumTable.WinRewardItem.Count; ++i) {
				rewardList.Add(new ColosseumReward() {
					ItemTable = DBItem.GetItem(colosseumTable.WinRewardItem[i]),
					Count = colosseumTable.WinRewardCnt[i]
				});
			}
		}
		else {

			// 패배보상
			for (int i = 0; i < colosseumTable.LoseRewardItem.Count; ++i) {
				rewardList.Add(new ColosseumReward() {
					ItemTable = DBItem.GetItem(colosseumTable.LoseRewardItem[i]),
					Count = colosseumTable.LoseRewardCnt[i]
				});
			}
		}

		return rewardList;
	}

	private void RewardListItemClicked(uint itemTid)
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

	public void OnClickGameEnd()
	{
		ZGameManager.Instance.DoForceMapMove(DBConfig.Town_Stage_ID);
	}

	public void OnClickReGame()
	{
		ZGameManager.Instance.DoForceMapMove(DBConfig.Town_Stage_ID);

		ZGameModeManager.Instance.ReserveActionSceneLoadedComplete((modeType) => {
			if (modeType == E_GameModeType.Field) {
				UIManager.Instance.Open<UIFrameColosseum>((str, frame) => {
					ZGameModeColosseum.REQ_AddColosseumQueue(ZGameModeManager.Instance.LastStageTid);
				});
			}
		});
	}

}