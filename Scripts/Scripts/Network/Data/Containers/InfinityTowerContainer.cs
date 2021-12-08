using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityTowerContainer : ContainerBase
{
    public InfinitySchedule_Table CurrentSchedule;
    public InfinityDungeon_Table CurrentDungeonStage;
    public InfinityDungeon_Table ChallengeDungeonStage;
    public List<InfiBuff_Table> InfinityBuffList = new List<InfiBuff_Table>();
    public List<InfinityDungeon_Table> DungeonStageList = new List<InfinityDungeon_Table>();
    public ulong DungeonEndTime;
    public ulong ReturnTownRemainTime;
    public uint NormalMonsterKillCount;
    public uint NormalMonsterSpawnCount;
    public bool IsBossStage;
    public uint LastDailyRewardedDungeonId = 0;

    public override void Clear()
    {
        
    }
}
