using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIColosseumInfo : MonoBehaviour, ITabContents
{
	public class ColosseumReward
	{
		public Item_Table ItemTable;
		public uint Count;
	}

	[SerializeField] private ScrollRect rewardScrollRect;
	[SerializeField] private ScrollRect stageScrollRect;
	[SerializeField] private ZImage myColosseumGradeImage;
	[SerializeField] private ZText userNickName;
	[SerializeField] private ZText stageMainTitle;
	[SerializeField] private ZText stageSubTitle;
	[SerializeField] private ZText timeInfoDesc;
	[SerializeField] private ZText modeRule1Text;
	[SerializeField] private ZText modeRule2Text;
	[SerializeField] private ZText remainderEnterCountText;
	[SerializeField] private ZButton matchingStartButton;
	[SerializeField] private ZButton matchingNowButton;
	[SerializeField] private ZText matchingStartButtonText;
	[SerializeField] private ZText matchingNowButtonText;
	[SerializeField] private ZText myInfoText;
	[SerializeField] private ZText myScoreNumText;
	[SerializeField] private ZText myRankingText;
	[SerializeField] private ZText myRankingNumText;
	[SerializeField] private ZText winRewardText;
	[SerializeField] private UIColosseumRankingReward uiColosseumRankingReward;

	public int Index { get; set; }

	public uint SelectStageTid { get; private set; }

	private List<UITowerRewardListItem> rewardListItems = new List<UITowerRewardListItem>();
	private Dictionary<E_ModelSocket, Transform> dicModelSocket = new Dictionary<E_ModelSocket, Transform>();

	private bool isSpawnLoad;
	private Action spawnLoadEvent;

	public void Initialize()
	{
		uiColosseumRankingReward.Initialize();

		myColosseumGradeImage.enabled = false;

		myInfoText.text = DBLocale.GetText("WPvP_Duel_Info");
		myRankingText.text = DBLocale.GetText("WPvP_Duel_InfoRanking");
		winRewardText.text = DBLocale.GetText("WPvP_Duel_InfoReward");
		modeRule1Text.text = DBLocale.GetText("WPvP_Duel_InfoRewardDesc");
		modeRule2Text.text = DBLocale.GetText("Colosseum_Desc");
		matchingStartButtonText.text = DBLocale.GetText("WPvP_Duel_Match");
		matchingNowButtonText.text = DBLocale.GetText("WPvP_DuelBtn_MatchingSignal");
		timeInfoDesc.text = string.Empty;
		myRankingNumText.text = string.Empty;

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UITowerRewardListItem), delegate {
			isSpawnLoad = true;
			if (spawnLoadEvent != null) {
				spawnLoadEvent.Invoke();
				spawnLoadEvent = null;
			}
		}, 0, 1, false);
	}

	public void Open()
	{
		this.gameObject.SetActive(true);

		if (isSpawnLoad == false) {
			spawnLoadEvent = () => { RefreshSelf(true); };
		}
		else {
			RefreshSelf(true);
		}
	}

	public void Refresh()
	{
		RefreshSelf(false);
	}

	public void Close()
	{
		this.gameObject.SetActive(false);
	}

	private void RefreshSelf(bool bReset)
	{
		DoStageListUpdate(bReset);

		DoInfoUpdate(bReset);
	}

	private void DoStageListUpdate(bool isResetPos)
	{
		var towerList = DBStage.GetStageList(E_StageType.Colosseum);
		towerList.Sort((a, b) => {
			return a.StageID.CompareTo(b.StageID);
		});

		List<GameObject> stageGoList = stageScrollRect.gameObject.FindChildren("Colosseum_Slot");
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

	private void DoInfoUpdate(bool isResetPos)
	{
		var stageTable = DBStage.Get(SelectStageTid);

		var score = Me.CurCharData.ColosseumScore;
		var colosseumTable = DBColosseum.FindByColosseumPoint(score);
		if (colosseumTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"colosseumTable이 null이다, score:{score}");
			return;
		}

		// 보상 정보 세팅
		DoRewardItemUpdate(colosseumTable, isResetPos);

		myColosseumGradeImage.sprite = ZManagerUIPreset.Instance.GetSprite(colosseumTable.GradeIcon);
		myColosseumGradeImage.enabled = true;

		myScoreNumText.text = string.Format(DBLocale.GetText("WPvP_Duel_InfoPoint"), Me.CurCharData.ColosseumScore);

		Me.CurCharData.ColosseumContainer.REQ_GetColosseumRankList((rankingList) => {
			var myRank = rankingList.Find(v => v.CharId == Me.CharID);
			if (myRank != null) {
				myRankingNumText.text = $"{myRank.Rank}";
			}
			else {
				myRankingNumText.text = $"{0}"; //서버로부터 받은 랭킹에 내가 없으면 0위
			}
		});

		// 던전 설명
		stageMainTitle.text = DBLocale.GetText(stageTable.StageTextID);
		stageSubTitle.text = DBLocale.GetText(stageTable.StageType.ToString());

		bool mschingNow = Me.CurCharData.ColosseumContainer.IsMachingNow;
		remainderEnterCountText.gameObject.SetActive(mschingNow == false); //매칭중일때 남은갯수 표시안함
		matchingStartButton.gameObject.SetActive(mschingNow == false);
		matchingNowButton.gameObject.SetActive(mschingNow);

		//캐릭터정보
		userNickName.text = Me.CurCharData.Nickname;

		//입장남은횟수
		int remainRewardCnt = (int)colosseumTable.MaxRewardCnt - (int)Me.CurUserData.ColosseumRewardCnt;
		remainderEnterCountText.text = string.Format(DBLocale.GetText("WPvP_Duel_InfoRewardCount"), remainRewardCnt);

		// 남은시간과 버튼활성
		bool bEnterableTime = false;
		matchingStartButton.interactable = false;

		Me.CurCharData.ServerEventContainer.REQ_GetServerEventList((eventList) => {

			for (int i = 0; i < eventList.Count; i++) {
				var info = eventList[i];
				if (info.Category != WebNet.E_ServerEventCategory.Stage) {
					continue;
				}
				if (info.SubCategory != WebNet.E_ServerEventSubCategory.Colosseum) {
					continue;
				}
				if (info.StartDt > TimeManager.NowSec) {
					continue;
				}
				if (info.EndDt <= TimeManager.NowSec) {
					continue;
				}
				if (info.Args["stage_tid"].Make<uint>() != SelectStageTid) {
					continue;
				}

				var curTime = TimeHelper.Time2DateTimeSec(TimeManager.NowSec);
				ulong curTimeSec =
					(ulong)curTime.Hour * TimeHelper.HourSecond +
					(ulong)curTime.Minute * TimeHelper.MinuteSecond +
					(ulong)curTime.Second;

				foreach (TinyJSON.Variant timeInfo in info.Args["open_time"] as TinyJSON.ProxyArray) {
					ulong startTime = timeInfo["start_time"].Make<ulong>();
					ulong endTime = timeInfo["end_time"].Make<ulong>();
					string startTimeTxt = TimeHelper.GetRemainFullTimeHour(startTime);
					string endTimeTxt = TimeHelper.GetRemainFullTimeHour(endTime);

					//입장시간정보 표시
					timeInfoDesc.text = $"{startTimeTxt}~{endTimeTxt}";

					if (startTime <= curTimeSec && endTime >= curTimeSec) {
						bEnterableTime = true;
						break;
					}
				}
			}

			matchingStartButton.interactable = (bEnterableTime && remainRewardCnt > 0);
		});
	}

	private void DoRewardItemUpdate(Colosseum_Table colosseumTable, bool isResetPos)
	{
		for (int i = 0; i < rewardListItems.Count; i++) {
			rewardListItems[i].gameObject.SetActive(false);
		}

		List<ColosseumReward> rewardItemList = SearchRewardItemList(colosseumTable);

		int loadItemCount = rewardItemList.Count - rewardListItems.Count;
		for (int i = 0; i < loadItemCount; i++) {
			var obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITowerRewardListItem)).GetComponent<UITowerRewardListItem>();
			obj.transform.SetParent(rewardScrollRect.content.transform);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
			rewardListItems.Add(obj);
			obj.gameObject.SetActive(false);
		}

		if (isResetPos == true) {
			rewardScrollRect.content.anchoredPosition = Vector2.zero;
			rewardScrollRect.velocity = Vector2.zero;
		}

		for (int i = 0; i < rewardItemList.Count; i++) {
			var item = rewardListItems[i];
			item.gameObject.SetActive(true);
			item.DoUpdate(rewardItemList[i].ItemTable.ItemID, rewardItemList[i].Count, RewardListItemClicked);
		}
	}

	private List<ColosseumReward> SearchRewardItemList(Colosseum_Table colosseumTable)
	{
		List<ColosseumReward> rewardList = new List<ColosseumReward>();

		for (int i = 0; i < colosseumTable.WinRewardItem.Count; ++i) {
			rewardList.Add(new ColosseumReward() {
				ItemTable = DBItem.GetItem(colosseumTable.WinRewardItem[i]),
				Count = colosseumTable.WinRewardCnt[i]
			});
		}

		return rewardList;
	}

	// 버튼 이벤트

	private void StageListItemClicked(uint stageTid)
	{
		SelectStageTid = stageTid;

		RefreshSelf(true);
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

	public void OnAddColosseumQueue()
	{
		ZGameModeColosseum.REQ_AddColosseumQueue(SelectStageTid);
	}

	public void OnLeaveColosseumQueue()
	{
		ZGameModeColosseum.REQ_LeaveColosseumQueue(SelectStageTid);
	}

	public void OnColosseumRankingRewardPopup()
	{
		uiColosseumRankingReward.Open(true);
	}
}
