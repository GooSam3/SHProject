using GameDB;
using System.Collections;
using UnityEngine;

public class UISubHudTrialInfo : ZUIFrameBase
{
    [SerializeField] private ZText StageNameTxt;
    [SerializeField] private ZText NormalMonsterKillCountTxt;
    [SerializeField] private ZText SpawnRemainTime;
    [SerializeField] private ZText AutoReturnRemainTime;
    [SerializeField] private ZButton ExitButton;
    [SerializeField] private GameObject DungeonEndUI;

    private int NormalMonsterSpawnCount = 0;
    private ulong AutoReturnEndTime = 0;
    private string StageName = string.Empty;
    private string BossName = string.Empty;
    TrialSanctuaryContainer Container;
    
    public void Init()
    {
        InitVariable();

        DungeonEndUI.SetActive(false);
        StageNameTxt.text = StageName;
    }

    private void InitVariable()
    {
        if(ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.TrialSanctuary)
        {
            return;
        }

        StageName = DBLocale.GetText(ZGameModeManager.Instance.Table.StageTextID);
        Monster_Table table = DBMonster.Get(ZGameModeManager.Instance.Table.SummonBossID);
        BossName = DBLocale.GetText(table.MonsterTextID);
    }

    protected override void OnShow(int _LayerOrder)
    {
        //UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
        
        Container = ZNet.Data.Me.CurCharData.TrialSanctuaryContainer;

        Init();

        base.OnShow(_LayerOrder);

        var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeTrialSanctuary>();

        if(gameMode == null)
        {
            ZGameModeManager.Instance.RemoveEventCompleteMapDataLoad(InitNormalMonsterKillCount);
            return;
        }

        gameMode.AddEventBossSpawnTime(InvokeBossSpawnRemainTime);
        gameMode.AddEventAutoReturnCountDown(AutoReturnCountDown);
        gameMode.AddEventCountMonsterKill(UpdateNormalMonsterKillCount);

        ZGameModeManager.Instance.AddEventCompleteMapdataLoad(InitNormalMonsterKillCount);
        UpdateNormalMonsterKillCount();
    }

    protected override void OnHide()
    {
        ZGameModeManager.Instance.RemoveEventCompleteMapDataLoad(InitNormalMonsterKillCount);

        base.OnHide();
    }

    protected override void OnRemove()
    {
        ZGameModeManager.Instance.RemoveEventCompleteMapDataLoad(InitNormalMonsterKillCount);

        base.OnRemove();
    }

    private void InitNormalMonsterKillCount()
    {
        NormalMonsterSpawnCount = 0;

        for (int i = 0; i < ZGameModeManager.Instance.CurrentMapData.MonsterInfos.Count; i++)
        {
            if (DBMonster.TryGet(ZGameModeManager.Instance.CurrentMapData.MonsterInfos[i].TableTID, out Monster_Table table))
            {
                NormalMonsterSpawnCount += table.SpawnCnt;
            }
        }

        UpdateNormalMonsterKillCount();
    }

    private void UpdateNormalMonsterKillCount()
    {
        if(ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.TrialSanctuary)
        {
            return;
        }

        if (NormalMonsterSpawnCount <= 0)
        {
            return;
        }

        if(Container.NormalMonsterKillCount == NormalMonsterSpawnCount)
        {
            CancelInvoke(nameof(UpdateBossSpawnRemainTime));
            SpawnRemainTime.text = string.Format(DBLocale.GetText("Instance_Dungeon_BossSummon_Time"), "");
        }

        NormalMonsterKillCountTxt.text = string.Format(DBLocale.GetText("Instance_Dungeon_Quest_Des"), StageName, Container.NormalMonsterKillCount, NormalMonsterSpawnCount, BossName, Container.BossMonsterKillCount, 1);
    }

    private void InvokeBossSpawnRemainTime()
    {
        CancelInvoke(nameof(UpdateBossSpawnRemainTime));
        InvokeRepeating(nameof(UpdateBossSpawnRemainTime), 0f, 1.0f);
    }

    private void UpdateBossSpawnRemainTime()
    {
        if (Container.BossSpawnTime < TimeManager.NowSec)
        {
            CancelInvoke(nameof(UpdateBossSpawnRemainTime));
            SpawnRemainTime.text = string.Format(DBLocale.GetText("Instance_Dungeon_BossSummon_Time"), "");
            return;
        }
        
        string time = TimeHelper.GetRemainFullTime(Container.BossSpawnTime - TimeManager.NowSec);
        
        if(string.IsNullOrEmpty(time))
        {
            time = "0초";
        }

        SpawnRemainTime.text = string.Format(DBLocale.GetText("Instance_Dungeon_BossSummon_Time"), time);
    }

    public void AutoReturnCountDown()
    {
        CancelInvoke(nameof(UpdateBossSpawnRemainTime));
        CancelInvoke(nameof(UpdateAutoReturnCountDown));
        DungeonEndUI.SetActive(true);

        InvokeRepeating(nameof(UpdateAutoReturnCountDown), 0f, 1.0f);
    }

    private void UpdateAutoReturnCountDown()
    {
        if (Container.ReturnTownRemainTime < TimeManager.NowSec)
        {
            CancelInvoke(nameof(UpdateAutoReturnCountDown));
            GoToTown();
            return;
        }

        float remainTime = Mathf.Clamp(Container.ReturnTownRemainTime - TimeManager.NowSec, 0, Container.ReturnTownRemainTime - TimeManager.NowSec);
        string strRemainTime = TimeHelper.GetRemainFullTime((ulong)remainTime);
        
        if(string.IsNullOrEmpty(strRemainTime))
        {
            strRemainTime = "0초";
        }

        AutoReturnRemainTime.text = strRemainTime;
    }

    public void GoToTown()
    {
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
}
