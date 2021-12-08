using UnityEngine;
using UnityEngine.UI;

public class UIFrameDungeonFailPopup : ZUIFrameBase
{
	[SerializeField] private Text StageTitle;
	[SerializeField] private Text FailText;
	[SerializeField] private Text ReturnButtonText;

	public void Init(E_GameModeType type)
	{
		if (type == E_GameModeType.TrialSanctuary)
			StageTitle.text = DBLocale.GetText(ZGameModeManager.Instance.Table.StageTextID);
		else if(type == E_GameModeType.Infinity)
			StageTitle.text = string.Format(DBLocale.GetText("InfinityDungeon_Name_02"), ZNet.Data.Me.CurCharData.InfinityTowerContainer.ChallengeDungeonStage.StageLevel);

		ReturnButtonText.text = DBLocale.GetText("RETURN");
		FailText.text = DBLocale.GetText("InfinityDungeon_Fail");

		if (UIManager.Instance.Find(out UIPopupSystem system))
		{
			system.Close();
		}
	}

	public void ReturnTown()
	{
		uint portalTid = DBConfig.Town_Portal_ID;

		if(DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
		{
			if(stageTable.DeathPortal > 0)
			{
				portalTid = stageTable.DeathPortal;
			}
			else
			{
				ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})의 DeathPortal이 셋팅되지 않았다.");
			}
		}
		else
		{
			ZLog.LogError(ZLogChannel.Map, $"해당 스테이지({ZGameModeManager.Instance.StageTid})를 찾을 수 없다.");
		}

		ZGameManager.Instance.TryEnterStage(portalTid, false, 0, 0);
		UIManager.Instance.Close<UIFrameDungeonFailPopup>(true);
	}
}
