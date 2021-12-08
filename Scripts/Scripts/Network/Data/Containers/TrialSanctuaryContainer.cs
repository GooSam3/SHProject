using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialSanctuaryContainer : ContainerBase
{
    // 보스 소환 남은 시간
    public ulong BossSpawnTime;
    public int NormalMonsterKillCount;
    public ulong ReturnTownRemainTime;
    public uint BossMonsterKillCount;

    public override void Clear()
    {
        
    }
}
