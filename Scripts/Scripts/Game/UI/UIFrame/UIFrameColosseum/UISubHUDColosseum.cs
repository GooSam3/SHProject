using GameDB;
using System.Collections.Generic;
using UnityEngine;
using uTools;
using ZNet.Data;

public sealed class UISubHUDColosseum : ZUIFrameBase
{
	[SerializeField] private UISubHUDColosseumCharItem[] aTeamCharItem;
	[SerializeField] private UISubHUDColosseumCharItem[] bTeamCharItem;
	[SerializeField] private GameObject[] killObj;
	[SerializeField] private ZText aTeamScoreText;
	[SerializeField] private ZText aTeamScoreAniText;
	[SerializeField] private ZText bTeamScoreText;
	[SerializeField] private ZText bTeamScoreAniText;
	[SerializeField] private ZText remainderTime;
	[SerializeField] private GameObject countObj;

	private bool isTimerStart;
	private float secondCheckTimer;
	private ulong endServerTime;

	protected override void OnRefreshOrder(int _layerOrder)
	{
		base.OnRefreshOrder(_layerOrder);

		var hudCharacter = UIManager.Instance.Find<UISubHUDCharacterState>();
		if (hudCharacter != null && hudCharacter.Show) {
			UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
		}
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		var hudCharacter = UIManager.Instance.Find<UISubHUDCharacterState>();
		if (hudCharacter != null && hudCharacter.Show) {
			UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
		}

		for (int i = 0; i < aTeamCharItem.Length; i++) {
			aTeamCharItem[i].gameObject.SetActive(false);
		}
		for (int i = 0; i < bTeamCharItem.Length; i++) {
			bTeamCharItem[i].gameObject.SetActive(false);
		}

		for (int i = 0; i < killObj.Length; i++) {
			killObj[i].gameObject.SetActive(false);
		}

		var stateTable = DBStage.Get(ZGameModeManager.Instance.StageTid);
		remainderTime.text = $"{TimeHelper.GetRemainFullTime(stateTable.ClearLimitTime)}";

		SetScoreUpdate(0, 0);

		var modeColosseum = ZGameModeManager.Instance.CurrentGameMode<ZGameModeColosseum>();
		modeColosseum.SetUserListCallback(() => {
			PrepareCharHudItems();
		});

		modeColosseum.SetCountDownCallback(() => {
			ulong remainCountdown = Me.CurCharData.ColosseumContainer.CountDownEndTime - TimeManager.NowSec;
			if (Me.CurCharData.ColosseumContainer.CountDownEndTime > TimeManager.NowSec && remainCountdown >= 4) {
				StartCountDown();
			}
		});

		modeColosseum.SetRemainTimeCallback(() => {
			ulong nowSec = TimeManager.NowSec;
			if (Me.CurCharData.ColosseumContainer.RemainTimeTargetTime > nowSec) {
				StartRaminTime(Me.CurCharData.ColosseumContainer.RemainTimeTargetTime);
			}
		});
	}

	protected override void OnHide()
	{
		countObj.SetActive(false);

		for (int i = 0; i < aTeamCharItem.Length; i++) {
			aTeamCharItem[i].Hide();
		}
		for (int i = 0; i < bTeamCharItem.Length; i++) {
			bTeamCharItem[i].Hide();
		}
	}

	private void Update()
	{
		if (isTimerStart == false) {
			return;
		}

		secondCheckTimer += Time.deltaTime;
		if (secondCheckTimer >= 0.95f) {
			secondCheckTimer = 0;

			ulong nowSec = TimeManager.NowSec;
			ulong reaminTime = endServerTime - nowSec;

			if (endServerTime <= nowSec) {
				reaminTime = 0;
			}

			string time = TimeHelper.GetRemainFullTime(reaminTime);
			remainderTime.text = $"{time}";

			if (reaminTime < 10) {
				remainderTime.GetComponent<uTweenColor>().enabled = true;
			}
		}
	}

	private void PrepareCharHudItems()
	{
		var container = Me.CurCharData.ColosseumContainer;

		for (int i = 0; i < container.RoomUserList.Count; ++i) {
			var roomUser = container.RoomUserList[i];

			UISubHUDColosseumCharItem item;
			if (roomUser.TeamNo == 0) {
				item = aTeamCharItem[roomUser.TeamOrder];
			}
			else {
				item = bTeamCharItem[roomUser.TeamOrder];
			}
			item.Initialize(roomUser);
			item.Show();
		}
	}

	public void StartCountDown()
	{
		countObj.SetActive(true);
	}

	public void ShowKillCount(uint myKillCount)
	{
		if (myKillCount < 1 || myKillCount > 3) {
			return;
		}

		killObj[myKillCount - 1].SetActive(true);
	}

	public void SetScoreUpdate(int scoreA, int scoreB)
	{
		aTeamScoreAniText.gameObject.SetActive(false);
		bTeamScoreAniText.gameObject.SetActive(false);

		aTeamScoreAniText.text = aTeamScoreText.text = scoreA.ToString();
		bTeamScoreAniText.text = bTeamScoreText.text = scoreB.ToString();

		aTeamScoreAniText.gameObject.SetActive(true);
		bTeamScoreAniText.gameObject.SetActive(true);
	}

	public void StartRaminTime(ulong _endServerTime)
	{
		isTimerStart = true;

		endServerTime = _endServerTime;
	}

	public void StopRamainTime()
	{
		isTimerStart = false;
	}
}