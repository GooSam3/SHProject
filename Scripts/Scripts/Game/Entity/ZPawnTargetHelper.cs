using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 타겟 관련 helper </summary>
public static class ZPawnTargetHelper
{
    private static Dictionary<uint, ZPawn>.ValueCollection m_dicPawns => ZPawnManager.Instance.GetEntitys();
    private static List<ZPawnMonster> MonsterList = new List<ZPawnMonster>();
    private static List<ZPawnRemotePc> OtherPcList = new List<ZPawnRemotePc>();
    private static List<ZObject> GatherObjectList = new List<ZObject>();

    /// <summary> 적인가 체크 (사용자 로직에서 null check  유도를 위해 ZPawn 직접비교 메소드는 추가안함) </summary>
    public static bool IsEnemy( this EntityDataBase entityData, EntityDataBase targetEntityData )
    {
        return IsEnemy( entityData.Team, targetEntityData.Team );
    }

    public static bool IsEnemy( this uint team, uint targetTeam )
    {
        if( ( team & targetTeam ) != team ) {
            return true;
        }
        return false;
    }

    /// <summary> 가장 가까운 적pc를 찾는다. </summary>
    public static ZPawn SearchTargetEnemyPC( ZPawn self )
    {
        List<ZPawnRemotePc> pcList = SearchOtherPcList( self, self.Position, DBConfig.SearchTargetRange );
        SortTargetListByDistance( self.Position, ref pcList );
        foreach( ZPawnRemotePc pc in pcList ) {
            if( self.EntityData.IsEnemy( pc.EntityData ) ) {
                return pc;
            }
        }
        return null;
    }

    /// <summary> 가장 가까운 entity를 찾는다. </summary>
    public static ZPawn SearchNearTarget(ZPawn self, E_UnitType targetEntityType, Vector3 startPos, float distance = Mathf.Infinity)
    {
        EntityBase prevTarget = self.GetTarget();
        ZPawn foundTarget = null;
        Vector3 diff;
        float curDistance;
        float sqrDistance = distance * distance;

        foreach (ZPawn target in m_dicPawns)
        {
            if (null == target)
                continue;

            if (target.EntityType != targetEntityType)
                continue;

            if (self == target)
                continue;

            if (prevTarget == target) //이미 타겟되어있는 놈은 제외
                continue;

            if (target.IsDead)
                continue;

            // 가장 가까운 적 정보 Caching 그리고, 반복
            diff = target.transform.position - startPos;
            curDistance = diff.sqrMagnitude;
            if (curDistance < sqrDistance)
            {
                foundTarget = target;
                sqrDistance = curDistance;
            }
        }

        return foundTarget;
    }

    /// <summary> 우선순위에 따른 타유저 타겟을 찾는다(타겟 버튼 클릭 시) </summary>
    public static ZPawn SearchTargetByTargetPriority( ZPawn self, Vector3 startPos, float distance = Mathf.Infinity )
    {
        List<ZPawnRemotePc> pcList = SearchOtherPcList( self, self.Position, DBConfig.SearchTargetRange );
        SortTargetListByDistance( self.Position, ref pcList );

        ZPawnRemotePc nearPc = null;
        ZPawnRemotePc hostileGuildPc = null;

        foreach (ZPawnRemotePc pc in pcList)
        {
            // 나를 공격한 플레이어
            if ((ZGameOption.Instance.Search_Target_Priority & ZGameOption.SearchTargetPriority.TARGET_BE_ATTACK_PLAYER) != 0)
            {
                if (pc.GetTarget() == self)
                {
                    return pc;
                }
            }

            if(null == hostileGuildPc)
			{
                // 적대 길드
                if ((ZGameOption.Instance.Search_Target_Priority & ZGameOption.SearchTargetPriority.TARGET_ENEMY_GUILD) != 0)
                {
                    if (pc.IsCustomConditionControl(E_CustomConditionControl.HostileGuild))
                    {
                        hostileGuildPc = pc;
                    }
                }
            }

            // 가까운 플레이어
            if(null == nearPc)
			{
                if ((ZGameOption.Instance.Search_Target_Priority & ZGameOption.SearchTargetPriority.TARGET_NEAR_CHARACTER) != 0)
                {
                    nearPc = pc;
                }
            }
        }

        return hostileGuildPc??nearPc;
    }

    /// <summary> 우선순위에 따른 자동사냥 타겟을 찾는다(오토 사냥 시) </summary>
    public static ZPawn SearchAutoBattleTarget(ZPawn self, Vector3 startPos, float distance = Mathf.Infinity)
    {
        // 콜로세움은 몬스터가 아닌 pc를 공격하자
        if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
            return SearchTargetEnemyPC( self );
        }

        List<ZPawnMonster> monsterList = SearchMonsterList(self, self.Position, DBConfig.Auto_Search_Range);
        SortTargetListByDistance(self.Position, ref monsterList);

        ZPawnMonster nearMonster = null;
        ZPawnMonster questMonster = null;

        foreach (ZPawnMonster monster in monsterList)
        {
            // TODO :: 매너모드 체크 (추후 게임 모드나 몬스터 타입에 따른 처리가 필요)
            if(true == ZGameOption.Instance.bManner_Search)
			{
                if (0 < monster.AttackerEntityId && monster.AttackerEntityId != self.EntityId)
                {
                    //일단 일반 몬스터만 처리
                    if (monster.MonsterType == E_MonsterType.Normal)
                        continue;
                }
            }        

            // 선공 몬스터 - 찾으면 그냥 리턴!
            if ((ZGameOption.Instance.AutoBattle_Target & ZGameOption.AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER) != 0)
            {
                if (monster.MonsterData.Table.BattleType == E_BattleType.Hostile)
                {
                    return monster;
                }
            }

            if (null == questMonster)
			{
                // 퀘스트 몬스터 - 선공 우선 찾고 마지막에 리턴
                if ((ZGameOption.Instance.AutoBattle_Target & ZGameOption.AutoBattleTargetPriority.TARGET_QUEST_MONSTER) != 0)
                {
                    if (UIManager.Instance.Find<UIFrameQuest>().CheckQuestMonster(monster.TableId))
                    {
                        questMonster = monster;
                    }
                }
            }

            //기본 몬스터 셋팅
            if (null == questMonster && null == nearMonster)
                nearMonster = monster;
        }

        return questMonster??nearMonster;
    }

    /// <summary> ScanDistance 내의 랜덤한 entity를 찾는다 </summary>
    public static void SearchRandomTargetList(ZPawn owner, ref List<ZPawn> targetList)
    {
        List<ZPawnMonster> monsterList = SearchMonsterList(owner, owner.Position, DBConfig.SearchTargetRange);
        List<ZPawnRemotePc> pcList = SearchOtherPcList(owner, owner.Position, DBConfig.SearchTargetRange);
        SortTargetListByDistance(owner.Position, ref monsterList);
        SortTargetListByDistance(owner.Position, ref pcList);
        ZPawn target = null;

        List<ZPawn> randomTargetList = new List<ZPawn>();

        for (int i = 0; i < DBConfig.SearchTargetCount; i++)
        {
            // 콜로세움은 플레이만 스캔
            if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
                target = ScanPcTarget( pcList, ref randomTargetList );
            }
            else {
                // 몬스터 우선 스캔
                if( ZGameOption.Instance.bMonsterPriority ) {
                    if( ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER ) != 0 ) {
                        target = ScanMonsterTarget( monsterList, ref randomTargetList );
                    }

                    if( target == null && ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER ) != 0 ) {
                        target = ScanPcTarget( pcList, ref randomTargetList );
                    }
                }
                // 플레이어 우선 스캔
                else {
                    if( ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER ) != 0 ) {
                        target = ScanPcTarget( pcList, ref randomTargetList );
                    }

                    if( target == null && ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER ) != 0 ) {
                        target = ScanMonsterTarget( monsterList, ref randomTargetList );
                    }
                }
            }

            if(target != null)
            {
                randomTargetList.Add(target);
            }
        }

        // 기존에 스캔한 리스트를 제외한 후 검색하고 최대 스캔카운트만큼 기존 리스트 포함하여 채운다
        int count = randomTargetList.Count;
        System.Random rnd = new System.Random();

        while(count > 1)
        {
            count--;
            int nRandomNumber = rnd.Next(count + 1);

            ZPawn rndTarget = randomTargetList[nRandomNumber];
            randomTargetList[nRandomNumber] = randomTargetList[count];
            randomTargetList[count] = rndTarget;
        }

        if(randomTargetList.Count < DBConfig.SearchTargetCount)
        {
            for(int i = 0; i < targetList.Count; i++)
            {
                if(!randomTargetList.Contains(targetList[i]))
                {
                    randomTargetList.Add(targetList[i]);
                }

                if(randomTargetList.Count >= DBConfig.SearchTargetCount)
                {
                    break;
                }    
            }
        }

        targetList.Clear();
        targetList = randomTargetList;
    }

    /// <summary> ScanDistance 내의 가까운 순의 Entity를 찾는다 </summary>
    public static ZPawn SearchNearTargetList(ZPawn owner, ref List<ZPawn> ignoreTargetList)
    {
        List<ZPawnMonster> monsterList = SearchMonsterList(owner, owner.Position, DBConfig.SearchTargetRange);
        List<ZPawnRemotePc> pcList = SearchOtherPcList(owner, owner.Position, DBConfig.SearchTargetRange);
        SortTargetListByDistance(owner.Position, ref monsterList);
        SortTargetListByDistance(owner.Position, ref pcList);
        ZPawn target = null;

        // 콜로세움은 플레이만 스캔
        if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
            target = ScanPcTarget( pcList, ref ignoreTargetList );
        }
        else {
            // 몬스터 우선 스캔
            if( ZGameOption.Instance.bMonsterPriority ) {
                if( ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER ) != 0 ) {
                    target = ScanMonsterTarget( monsterList, ref ignoreTargetList );
                }

                if( target == null && ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER ) != 0 ) {
                    target = ScanPcTarget( pcList, ref ignoreTargetList );
                }
            }
            // 플레이어 우선 스캔
            else {
                if( ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_PLAYER ) != 0 ) {
                    target = ScanPcTarget( pcList, ref ignoreTargetList );
                }

                if( target == null && ( ZGameOption.Instance.ScanSearchTarget_Priority & ZGameOption.ScanSearchTargetPriority.TARGET_MONSTER ) != 0 ) {
                    target = ScanMonsterTarget( monsterList, ref ignoreTargetList );
                }

            }
        }

        return target;
    }

    public static ZObject ScanGatherObject(ZPawn owner, uint objectTid = 0)
    {
        SearchGatherObjectList(owner, objectTid, DBConfig.SearchTargetRange);
        SortTargetListByDistance(owner.Position, ref GatherObjectList);

        if (GatherObjectList != null && GatherObjectList.Count > 0)
        {
            return GatherObjectList[0];
        }
        else
        {
            return null;
        }
    }

    private static ZPawnMonster ScanMonsterTarget(List<ZPawnMonster> _monsterList, ref List<ZPawn> targetList)
    {
        foreach(ZPawnMonster monster in _monsterList)
        {
            if(targetList.Contains(monster))
            {
                continue;
            }

            if(ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_QUEST_MONSTER) && UIManager.Instance.Find<UIFrameQuest>().CheckQuestMonster(monster.TableId))
            {
                return monster;
            }

            if (ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_HOSTILE_MONSTER) && monster.MonsterData.Table.BattleType == E_BattleType.Hostile)
            {
                return monster;
            }

            if(ZGameOption.Instance.ScanSearchTarget_Type.HasFlag(ZGameOption.ScanSearchTargetType.TARGET_NORMAL_MONSTER) && !UIManager.Instance.Find<UIFrameQuest>().CheckQuestMonster(monster.TableId) && monster.MonsterData.Table.BattleType != E_BattleType.Hostile)
            {
                return monster;
            }
        }

        return null;
    }

    private static ZPawnRemotePc ScanPcTarget(List<ZPawnRemotePc> _pcList, ref List<ZPawn> targetList)
    {
        foreach (ZPawnRemotePc pc in _pcList)
        {
            if(targetList.Contains(pc))
            {
                continue;
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER) != 0)
            {
                if (pc.IsCustomConditionControl(E_CustomConditionControl.HostileGuild))
                {
                    return pc;
                }
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_ALERT_PLAYER) != 0)
            {
                if (pc.IsCustomConditionControl(E_CustomConditionControl.HostileUser))
                {
                    return pc;
                }
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_MYGUILD_PLAYER) != 0)
            {
                if (pc.IsCustomConditionControl(E_CustomConditionControl.GuildMember))
                {
                    return pc;
                }
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER) != 0)
            {
                if(pc.IsCustomConditionControl(E_CustomConditionControl.AllianceGuild))
                {
                    return pc;
                }
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_PARTY_PLAYER) != 0)
            {
                if (pc.IsCustomConditionControl(E_CustomConditionControl.PartyMember))
                {
                    return pc;
                }
            }

            if ((ZGameOption.Instance.ScanSearchTarget_Type & ZGameOption.ScanSearchTargetType.TARGET_NORMAL_PLAYER) != 0)
            {
                if(!pc.IsCustomConditionControl(E_CustomConditionControl.PartyMember) && !pc.IsCustomConditionControl(E_CustomConditionControl.AllianceGuild) && !pc.IsCustomConditionControl(E_CustomConditionControl.GuildMember) && !pc.IsCustomConditionControl(E_CustomConditionControl.HostileGuild) && !pc.IsCustomConditionControl(E_CustomConditionControl.HostileUser))
                {
                    return pc;
                }
            }

        }

        return null;
    }

    /// <summary>
    /// 모든 채집 오브젝트 검색
    /// </summary>
    private static List<ZObject> SearchGatherObjectList(ZPawn self, uint objectTid = 0, float distance = Mathf.Infinity)
    {
        GatherObjectList.Clear();

        Vector3 diff;
        float sqrDistance = distance * distance;

        foreach(ZPawn pawn in m_dicPawns)
        {
            if(pawn is ZObject gatherObject)
            {
                if(gatherObject.IsGathered)
                {
                    continue;
                }

                if(0 < objectTid && gatherObject.TableId != objectTid)
                {
                    continue;
                }

                diff = gatherObject.transform.position - self.Position;

                if(diff.sqrMagnitude < sqrDistance)
                {
                    GatherObjectList.Add(gatherObject);
                }
            }
        }

        return GatherObjectList;
    }

    /// <summary>
    /// 모든 몬스터 검색
    /// </summary>
    private static List<ZPawnMonster> SearchMonsterList(ZPawn self, Vector3 startPos, float distance = Mathf.Infinity)
    {
        MonsterList.Clear();

        Vector3 diff;
        float sqrDistance = distance * distance;

        foreach(ZPawn pawn in m_dicPawns)
        {
            if(pawn == null)
                continue;

            if (pawn.EntityType != E_UnitType.Monster)
                continue;

            if(pawn is ZPawnMonster monster)
            {
                diff = monster.transform.position - startPos;

                if(!monster.IsDead && diff.sqrMagnitude < sqrDistance)
                {
                    MonsterList.Add(monster);
                }
            }
        }

        return MonsterList;
    }

    /// <summary>
    /// 모든 플레이어 검색(MyPc 제외)
    /// </summary>
    private static List<ZPawnRemotePc> SearchOtherPcList(ZPawn self, Vector3 startPos, float distance = Mathf.Infinity)
    {
        OtherPcList.Clear();

        Vector3 diff;
        float sqrDistance = distance * distance;

        foreach(ZPawn pawn in m_dicPawns)
        {
            if(pawn is ZPawnRemotePc pc)
            {
                //콜로세움은 적이 아니면 제외
                if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
                    if( self.EntityData.IsEnemy( pc.EntityData ) == false ) {
                        continue;
                    }
                }

                diff = pc.transform.position - startPos;
                if (!pc.IsDead && diff.sqrMagnitude < sqrDistance)
                {
                    OtherPcList.Add(pc);
                }
            }
        }

        return OtherPcList;
    }

    /// <summary>
    /// 타겟 리스트를 거리순으로 정렬
    /// </summary>
    private static void SortTargetListByDistance<PAWN_TYPE>(Vector3 startPos, ref List<PAWN_TYPE> targets) where PAWN_TYPE : ZPawn
    {
        targets.Sort((p1, p2) =>
        {
            float dist1 = Vector3.SqrMagnitude(p1.transform.position - startPos);
            float dist2 = Vector3.SqrMagnitude(p2.transform.position - startPos);

            return dist1.CompareTo(dist2);
;       });
    }

}