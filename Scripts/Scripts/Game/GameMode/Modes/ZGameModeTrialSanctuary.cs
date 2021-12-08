using GameDB;
using MmoNet;
using System;
using ZNet.Data;

public class ZGameModeTrialSanctuary : ZGameModeBase
{
    public override E_GameModeType GameModeType { get { return E_GameModeType.TrialSanctuary; } }
    protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;
    private TrialSanctuaryContainer Container;
    private Action EventBossSpawnTime;
    private Action EventautoReturnCountDown;
    private Action EventCountMonsterKill;

	protected override void EnterGameMode()
	{
        Container = Me.CurCharData.TrialSanctuaryContainer;

        Container.NormalMonsterKillCount = 0;
        Container.BossSpawnTime = 0;
        Container.ReturnTownRemainTime = 0;
        Container.BossMonsterKillCount = 0;
    }

	protected override void ExitGameMode()
    {
        ZPawnManager.Instance.DoRemoveEventDieEntity(EntityDieEvent);
    }
    protected override void StartGameMode()
    {
        ZPawnManager.Instance.DoAddEventDieEntity(EntityDieEvent);
    }

    private void EntityDieEvent(uint entityId, ZPawn pawn)
    {
        if(pawn.IsMyPc)
        {
            return;
        }

        Monster_Table monster = DBMonster.Get(pawn.TableId);

        if (monster.MonsterType == E_MonsterType.InstanceBoss)
        {
            Container.BossMonsterKillCount++;
        }
        else
        {
            Container.NormalMonsterKillCount++;
        }

        EventCountMonsterKill?.Invoke();
    }

    public void AddEventCountMonsterKill(Action action)
    {
        EventCountMonsterKill = action;
    }

    public void AddEventBossSpawnTime(Action action)
    {
        if(Container.BossSpawnTime > 0)
        {
            action?.Invoke();
            return;
        }

        EventBossSpawnTime = action;
    }

    public void AddEventAutoReturnCountDown(Action action)
    {
        if(Container.ReturnTownRemainTime > 0)
        {
            action?.Invoke();
            return;
        }

        EventautoReturnCountDown = action;
    }

    public override void RECV_StageState(S2C_StageState info)
    {
        base.RECV_StageState(info);

        if (info.RemainSec > 0)
        {
            Container.BossSpawnTime = TimeManager.NowSec + info.RemainSec;

            EventBossSpawnTime?.Invoke();
        }

        if (info.ExpireDt > 0)
        {
            Container.ReturnTownRemainTime = info.ExpireDt;

            EventautoReturnCountDown?.Invoke();
        }
    }
}
