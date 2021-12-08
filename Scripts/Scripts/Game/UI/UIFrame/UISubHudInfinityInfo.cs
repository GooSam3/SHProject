using GameDB;
using UnityEngine;

public class UISubHudInfinityInfo : ZUIFrameBase
{
    [SerializeField] private ZText StageNameTxt;                // 우측 상단 스테이지 이름 ex)주신의탑 1층
    [SerializeField] private ZText NormalMonsterKillCountTxt;   // 우측 상단 처치 카운트 ex) 몬스터 처치 0/32
    [SerializeField] private ZText AutoReturnRemainTime;        // 던전 끝난 후 마을 귀환 시간
    [SerializeField] private ZText DungeonRemainTimeText;       // 던전 남은 시간
    [SerializeField] private GameObject DungeonEndUI;
    [SerializeField] private UIInfinitySelectBuff SelectBuff;
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private GameObject NextFloorButton;
    [SerializeField] private GameObject AutoNextFloorButton;
    [SerializeField] private GameObject OnlyExitButton;

    private InfinityTowerContainer Container;
    private bool IsAutoNextDungeonPlay = false;
    private string BossStageText = string.Empty;

    public void InitDungeonUI()
	{
        ZLog.Log(ZLogChannel.UI, "## InfinityTower Init DungeonUI !!!");
        DungeonEndUI.SetActive(false);

        StageNameTxt.text = DBLocale.GetText("Infinity_Dungeon_Quest_Title", DBLocale.GetText(Container.ChallengeDungeonStage.StageLevelName, Container.ChallengeDungeonStage.StageLevel));

        if(Container.IsBossStage)
		{
            string[] str = Container.ChallengeDungeonStage.UseRanker.Split(':');
            string rankerName = string.Empty;
            uint characterRank = 0;

            if (uint.TryParse(str[1], out uint rank))
            {
                characterRank = rank;
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, $"{Container.ChallengeDungeonStage.StageID} : {Container.ChallengeDungeonStage.StageLevel} 랭킹 데이터가 유효하지 않습니다.");
            }

            if (uint.TryParse(str[0], out uint characterId))
            {
                if (DBCharacter.TryGet(characterId, out Character_Table table))
                {
                    ZWebManager.Instance.WebGame.REQ_GetExpRankList((uint)table.CharacterType, (recvPacket, recvPacketMsg) =>
                    {
                        for (int i = 0; i < recvPacketMsg.RanksLength; i++)
                        {
                            var rankData = recvPacketMsg.Ranks(i).Value;

                            if (rankData.Rank == characterRank)
                            {
                                rankerName = rankData.Nick;
                            }
                        }

                        BossStageText = DBLocale.GetText(Container.ChallengeDungeonStage.UseRankerName, rankerName);

                        UpdateNormalMonsterKillCount();
                    });
                }
                else
                {
                    ZLog.LogError(ZLogChannel.UI, $"{characterId}에 해당하는 캐릭터 테이블이 없습니다.");
                }
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, $"{Container.ChallengeDungeonStage.StageID} : {Container.ChallengeDungeonStage.StageLevel} 캐릭터 ID가 유효하지 않습니다.");
            }
        }

        UpdateNormalMonsterKillCount();
        InvokeRepeating(nameof(UpdateDungeonRemainTime), 0f, 1.0f);
    }

    protected override void OnShow(int _LayerOrder)
    {
        Container = ZNet.Data.Me.CurCharData.InfinityTowerContainer;
        
        var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeInfinity>();

        if (gameMode != null)
        {
            ZLog.Log(ZLogChannel.UI, "## InfinityTower OnShow AddEvent !!!");
            gameMode.AddEventAutoReturnCountDown(AutoReturnCountDown);
            gameMode.AddEventInstanceFinish(InstanceFinish);
            gameMode.AddEventCountMonsterKill(UpdateNormalMonsterKillCount);

            if (ZGameModeManager.Instance.CurrentMapData.StageTID == Container.ChallengeDungeonStage.StageID)
            {
                gameMode.SetMapData();
                InitDungeonUI();
                gameMode.AddEventCompleteDungeonInfoLoad(InitDungeonUI);
            }
            else
            {
                gameMode.AddEventCompleteDungeonInfoLoad(InitDungeonUI);
            }
        }

        base.OnShow(_LayerOrder);
    }

    public void Refresh()
	{
        var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeInfinity>();

        
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    protected override void OnRemove()
    {
        base.OnRemove();
    }

    private void UpdateNormalMonsterKillCount()
    {
        if(ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Infinity)
        {
            return;
        }

        if(Container.IsBossStage)
		{
            NormalMonsterKillCountTxt.text = BossStageText;
		}
        else
		{
            NormalMonsterKillCountTxt.text = string.Format(DBLocale.GetText("Eliminate_Monsters_Text"), Container.NormalMonsterKillCount, Container.NormalMonsterSpawnCount);
        }
    }

    public void AutoReturnCountDown()
    {
        DungeonEndUI.SetActive(true);
        DungeonRemainTimeText.text = DBLocale.GetText("Infinity_Dungeon_Time", "0초");

        InvokeRepeating(nameof(UpdateAutoReturnCountDown), 0f, 1.0f);
    }

    private void UpdateAutoReturnCountDown()
    {
        if (Container.ReturnTownRemainTime < TimeManager.NowSec)
        {
            CancelInvoke(nameof(UpdateAutoReturnCountDown));

            if(IsAutoNextDungeonPlay)
            {
                CheckSelectableBuff();
            }
            else
            {
                GoToTown();
            }
            return;
        }
        
        float remainTime = Mathf.Clamp(Container.ReturnTownRemainTime - TimeManager.NowSec, 0, Container.ReturnTownRemainTime - TimeManager.NowSec);
        
        string strRemainTime = TimeHelper.GetRemainFullTime((ulong)remainTime);
        
        AutoReturnRemainTime.text = strRemainTime;
    }

    private void UpdateDungeonRemainTime()
    {
        float remainTime = Mathf.Clamp(Container.DungeonEndTime - TimeManager.NowSec, 0, Container.DungeonEndTime - TimeManager.NowSec);
        
        string time = TimeHelper.GetRemainFullTime((ulong)remainTime);

        DungeonRemainTimeText.text = DBLocale.GetText("Infinity_Dungeon_Time", time);
    }

    private void InstanceFinish(bool isClear)
    {
        CancelInvoke(nameof(UpdateDungeonRemainTime));

        ExitButton.SetActive(isClear);
        NextFloorButton.SetActive(isClear);
        AutoNextFloorButton.SetActive(isClear);
        OnlyExitButton.SetActive(!isClear);

        if (!isClear)
        {
            IsAutoNextDungeonPlay = false;
        }
    }

    public void GoToTown()
    {
        SelectBuff.ClosePopup();
        CancelInvoke(nameof(UpdateDungeonRemainTime));
        CancelInvoke(nameof(UpdateAutoReturnCountDown));

        uint portalTid = DBConfig.Town_Portal_ID;

        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if (stageTable.DeathPortal > 0)
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
    }

    public void ExitDungeon()
    {
        UICommon.OpenPopup_ExitTrialSanctuary();
    }

    public void EnterNextStage()
    {
        SelectBuff.ClosePopup();
        CancelInvoke(nameof(UpdateAutoReturnCountDown));

        InfinityDungeon_Table current = Container.DungeonStageList.Find(a => a.DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId);

        if(current != null)
        {
            foreach (var table in Container.DungeonStageList)
            {
                if (table.StageID == current.StageID && table.StageLevel == current.StageLevel + 1)
                {
                    Container.ChallengeDungeonStage = table;
                }
            }
        }

        if (DBStage.TryGet(Container.ChallengeDungeonStage.StageID, out Stage_Table stage))
        {
            var portalTable = DBPortal.Get(stage.DefaultPortal);

            if (portalTable == null)
            {
                ZLog.LogError(ZLogChannel.Default, $"PortalTable is null (DefaultPortal : {stage.DefaultPortal}");
                return;
            }

            AudioManager.Instance.PlaySFX(30004);
            ZGameManager.Instance.TryEnterStage(portalTable.PortalID, false, 0, 0, string.Empty, 0, 0);
        }
    }

    public void CheckSelectableBuff()
    {
        InfinityDungeon_Table challengeDungeon = null;
        bool IsGetBuff = false;

        InfinityDungeon_Table current = Container.DungeonStageList.Find(a => a.DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId);

        if(current != null)
        {
            foreach(var table in GameDBManager.Container.InfinityDungeon_Table_data.Values)
            {
                if(table.StageID == current.StageID && table.StageLevel == current.StageLevel + 1)
                {
                    challengeDungeon = table;
                }
            }
        }

        for(int i = 0; i < Container.InfinityBuffList.Count; i++)
        {
            if(challengeDungeon.InfiBuffGroupID == Container.InfinityBuffList[i].InfiBuffGroupID)
            {
                IsGetBuff = true;
                break;
            }
        }

        if(challengeDungeon.InfiBuffGroupID <= 0)
        {
            IsGetBuff = true;
        }

        if(!IsGetBuff)
        {
            ZWebManager.Instance.WebGame.REQ_GetInfinityDungeonSelectBuffList((recv, recvPacket) =>
            {
                SelectBuff.gameObject.SetActive(true);
                UIManager.Instance.TopMost<UISubHudInfinityInfo>(true);
                SelectBuff.Init(recvPacket, EnterNextStage);
            });
            return;
        }
        else
        {
            EnterNextStage();
        }
    }

    public void OnValueChanged(bool isOn)
    {
        IsAutoNextDungeonPlay = isOn;
    }
}
