using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarScheduleListItem : MonoBehaviour
{
    [SerializeField] private ZText SpawnRemainTimeText;
    [SerializeField] private ZText BossName;
	[SerializeField] private GameObject LockImage;
	[SerializeField] private GameObject SelectedImage;

	private ulong SpawnRemainTime;

	public void OnDisable()
	{
		CancelInvoke(nameof(UpdateBossSpawnRemainTime));
	}

	public void SetData(ulong remainTime)
	{
		Stage_Table stage = DBStage.Get(ZNet.Data.Me.CurCharData.BossWarContainer.StageTid);

		SpawnRemainTime = remainTime;
		BossName.text = stage.StageTextID;

		if (!IsInvoking())
		{
			InvokeRepeating(nameof(UpdateBossSpawnRemainTime), 0f, 1.0f);
		}

		LockImage.SetActive(false);
		SelectedImage.SetActive(false);
	}

	public void CancelInvokeBossRemainTime()
	{
		CancelInvoke(nameof(UpdateBossSpawnRemainTime));
	}

	private void UpdateBossSpawnRemainTime()
	{
		ulong curSecTime = (TimeManager.NowSec + TimeHelper.SecondOffset) % TimeHelper.DaySecond;

		if (SpawnRemainTime < curSecTime)
		{
			CancelInvoke(nameof(UpdateBossSpawnRemainTime));
			LockImage.SetActive(true);
			SpawnRemainTimeText.text = "시간 만료";
			return;
		}

		string time = TimeHelper.GetRemainFullTime(SpawnRemainTime - curSecTime);

		if(string.IsNullOrEmpty(time))
		{
			time = "0초";
		}

		SpawnRemainTimeText.text = $"남은 소환시간 {time}";
	}

	public void ClickBossWarSchedule()
	{

	}
}
