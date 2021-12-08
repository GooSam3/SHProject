using GameDB;
using MmoNet;
using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_SHADOW_PLAYER
using FlatBuffers;
#endif

/// <summary> 모든 Entity를 관리하는 class </summary>
public class ZPawnManager : Zero.Singleton<ZPawnManager>
{
    private ZPawnContainer Container = new ZPawnContainer();

    public const uint SHADOW_PLAYER_ENTITY_ID = 0;

    public uint MyEntityId { get; set; }
    public ZPawnMyPc MyEntity { get; private set; }

    public ZMmoCharacterData MyCharInfo { get; internal set; } = null;

    /// <summary> 유적 고정 변신 </summary>
    public uint TempleFixedChangeTid = 0;

    #region ===== :: 사당 체크 포지션 (추후 사당용 게임 모드나 클래스가 생긴다면 그쪽으로 이동) :: ======
    public Vector3 TempleCheckPosition = Vector3.zero;
    public Quaternion TempleCheckRotation = Quaternion.identity;
    #endregion

    protected override void Init()
    {
        base.Init();

        Container.DoAddEventCreateEntity(OnCreateEntity);
        Container.DoAddEventRemoveEntity(OnRemoveEntity);
    }

    private void OnCreateEntity(uint entityId, ZPawn entity)
    {
        bool isMyEntity = MyEntityId == entityId;
        if ( isMyEntity ) {
            MyEntity = entity.To<ZPawnMyPc>();
        }

        ZGameModeManager.Instance.GameMode.CreateEntity( isMyEntity, entityId, entity );

        if( isMyEntity ) {
            mEventCreateMyEntity?.Invoke();  //내캐릭 생성시 알림
        }

        mEventCreateEntity?.Invoke(entityId, entity);
    }

    private void OnRemoveEntity(uint entityId)
    {
        bool isMyEntity = MyEntityId == entityId;
        if ( isMyEntity ) {
            if(null != MyEntity)
            {
               MyEntity.DestroyEntity();
            }
            MyEntity = null;
        }

        ZGameModeManager.Instance.GameMode.RemoveEntity( isMyEntity, entityId );

        mEventRemoveEntity?.Invoke(entityId); 
    }

    public void DoJoinField(JoinFieldRes info)
    {
        ZGameModeManager.Instance.PKType = (E_PKAreaChangeType)info.PkType;

        MyEntityId = info.Objectid;
        if (null == MyCharInfo)
            MyCharInfo = new ZMmoCharacterData(info.CharInfo.Value);
        else
            MyCharInfo.Reset(info.CharInfo.Value);
    }
    
    public void DoAdd(S2C_AddCharInfo info)
    {        
        Container.DoAdd(info);

#if ENABLE_SHADOW_PLAYER
        //쉐도우 플레이어 생성
        if (info.Objectid == MyEntityId)
        {
            DoAdd(GetCharacterInfo(SHADOW_PLAYER_ENTITY_ID, info));
        }
#endif
    }

    public void DoAdd(S2C_AddMonsterInfo info)
    {
        Container.DoAdd(info);
    }

    public void DoAdd(S2C_AddNPCInfo info)
    {
        Container.DoAdd(info);
    }

    public void DoAdd(S2C_AddGatherObj info)
    {
        Container.DoAdd(info);
    }

    public void DoRemove(uint entityId)
    {
        Container.DoRemove(entityId);
    }

    public void DoClear()
    {
        Container.DoClear();
    }

    private void Update()
    {
        Container.DoUpdate();

		UpdateVisibility();
    }

	private void UpdateVisibility()
	{
		foreach (var entity in Container.DicEntity)
		{

		}
	}

    public Dictionary<uint, ZPawn>.ValueCollection  GetEntitys()
    {
        return Container.DicEntity;
    }

    #region ===== :: Move 관련 처리 :: =====
    /// <summary>  Dir로 이동한다. </summary>
    public void DoMoveDir(CS_MoveToDir info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
        {
            SetMoveData(info);
            return;
        }

        Quaternion rot = Quaternion.Euler(0, info.Dir, 0);
        ServerPos3 pos = info.Pos.Value;
        Vector3 curPos = new Vector3(pos.X, pos.Y, pos.Z);

        if (entity.IsRemoteMove == false) 
        {
            //내가 움직이는 경우 Shadow Entity를 움직이도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.MoveToDirection(curPos, rot * Vector3.forward, info.Speed);
            }
#endif
        }
        else
        {
                entity.MoveToDirection(curPos, rot * Vector3.forward, info.Speed);
        }
    }

    /// <summary> Path로 이동한다. </summary>
    public void DoMoveToDest(CS_MoveToDest info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
        {
            SetMoveData(info);
            return;
        }

        List<Vector3> path = new List<Vector3>();
        for(int i = 0; i < info.DestLength; ++i)
        {
            ServerPos3 dest = info.Dest(i).Value;
            path.Add(new Vector3(dest.X, dest.Y, dest.Z));
        }

        if (entity.IsRemoteMove) 
        {
            entity.MoveTo(path, info.Speed);
        }
        else
        {
            //내가 움직이는 경우 Shadow Entity를 움직이도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.MoveTo(path, info.Speed);
            }
#endif
        }
    }

    /// <summary> 강제로 이동시킨다 </summary>
    public void DoForceMove(S2C_ForceMove info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
        {
            SetMoveData(info);
            return;
        }   

        ServerPos3 pos = info.Pos.Value;
        Vector3 dest = new Vector3(pos.X, pos.Y, pos.Z);
        entity.ForceMove(dest, info.Duration, (E_PosMoveType)info.Movetype);

        if (entity.IsRemoteMove) { }
        else 
        {
            //내가 움직이는 경우 Shadow Entity를 움직이도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.ForceMove(dest, info.Duration, (E_PosMoveType)info.Movetype);
            }
#endif
        }
    }

    /// <summary> 이동을 중지한다. </summary>
    public void DoMoveStop(CS_MoveStop info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
        {
            SetMoveData(info);
            return;
        }   

        ServerPos3 pos = info.Pos.Value;
        Vector3 curPos = new Vector3(pos.X, pos.Y, pos.Z);

        if (entity.IsRemoteMove)
        {
            entity.StopMove(curPos);
        }
        else
        {
            //내가 움직이는 경우 Shadow Entity를 움직이도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.StopMove(curPos);
            }
#endif
        }
    }

    /// <summary> 생성되기 전 이동 패킷 처리 </summary>
    private void SetMoveData(CS_MoveToDir info)
    {
        if (false == Container.TryGetEntityData(info.Objectid, out var data))
            return;

        if (data is ZPawnDataBase pawnData)
        {
            pawnData.SetMoveData(info);
            return;
        }
    }

    /// <summary> 생성되기 전 이동 패킷 처리 </summary>
    private void SetMoveData(CS_MoveToDest info)
    {
        if (false == Container.TryGetEntityData(info.Objectid, out var data))
            return;

        if (data is ZPawnDataBase pawnData)
        {
            pawnData.SetMoveData(info);
            return;
        }
    }

    /// <summary> 생성되기 전 이동 패킷 처리 </summary>
    private void SetMoveData(S2C_ForceMove info)
    {
        if (false == Container.TryGetEntityData(info.Objectid, out var data))
            return;

        if (data is ZPawnDataBase pawnData)
        {
            pawnData.SetMoveData(info);
            return;
        }
    }

    /// <summary> 생성되기 전 이동 패킷 처리 </summary>
    private void SetMoveData(CS_MoveStop info)
    {
        if (false == Container.TryGetEntityData(info.Objectid, out var data))
            return;

        if (data is ZPawnDataBase pawnData)
        {
            pawnData.SetMoveData(info);
            return;
        }
    }
#endregion

    /// <summary> TODO :: 내 캐릭터 공격만 델리게이트 등록해서 처리하면 될듯. (추후 필요하다면 공통으로 빼야함) </summary>
    private Action mEventReceiveAttack;

    public void DoAddEventReceiveAttack(Action action)
    {
        DoRemoveEventReceiveAttack(action);
        mEventReceiveAttack += action;
    }

    public void DoRemoveEventReceiveAttack(Action action)
    {
        mEventReceiveAttack -= action;
    }

    public void DoAttack(CS_Attack info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        ServerPos3 pos = info.Pos.Value;
        Vector3 curPos = new Vector3(pos.X, pos.Y, pos.Z);
        
        if (entity.EntityId == MyEntityId)
        {
            mEventReceiveAttack?.Invoke();

            //내가 공격하는 경우 Shadow Entity를 공격하도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.UseSkill(curPos, info.Targetid, info.Skillno, info.Attackspeed, info.Dir, info.Endtime);
            }
#endif
        }

        if(TryGetEntity(info.Targetid, out var target))
		{
            if(false == target.IsDead)
			{
                entity.UseSkill(curPos, info.Targetid, info.Skillno, info.Attackspeed, info.Dir, info.Endtime);
            }
        }
    }

    /// <summary> TODO :: 추후 필요하면 작업 </summary>
    public void DoAttackToPos(CS_AttackToPos info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        ServerPos3 pos = info.Pos.Value;
        Vector3 curPos = new Vector3(pos.X, pos.Y, pos.Z);

        if (entity.EntityId == MyEntityId)
        {
            //내가 공격하는 경우 Shadow Entity를 공격하도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                //shadowEntity.UseSkill(curPos, info.Targetid, info.Skillno, info.Attackspeed, info.Dir, (ulong)info.Invoketime);
            }
#endif
        }
        //entity.UseSkill(curPos, info.Targetid, info.Skillno, info.Attackspeed, info.Dir, (ulong)info.Invoketime);
    }

    /// <summary> 현재 사용중인 스킬 취소 </summary>
    public void DoSkillCancel(CS_SkillCancle info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        entity.SkillCancel();

        if (entity.EntityId == MyEntityId)
        {
            //내가 공격하는 경우 Shadow Entity를 공격하도록 한다.
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.SkillCancel();
            }
#endif
        }
    }

    /// <summary> 스킬 어빌리티 셋팅 </summary>
    public void DoSkillAbilityNotify(S2C_SkillAbility info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
		{
            if (info.Objectid == MyEntityId)
            {
                MyCharInfo.SkillAbilityNotiofy(info);
            }
            return;
        }
            
        for (int i = 0; i < info.SkillabilsLength; ++i)
        {
            SkillAbility skill = info.Skillabils(i).Value;
            uint skillTid = skill.Skilltid;
            for (int j = 0; j < skill.AbilsLength; ++j)
            {
                Ability ability = skill.Abils(j).Value;
                E_AbilityType abilityType = (E_AbilityType)ability.Type;

                entity.SkillAbilityNotify(skillTid, abilityType, ability.Value);
            }
        }
    }

    /// <summary> 스킬 쿨타임 변경 </summary>
    public void DoSkillCoolTime(S2C_Cooltime info)
    {     
        if(null != MyEntity )
        {
            MyEntity.SetSkillCoolTime(info.Skilltid, info.Cooltime);
        }
    }

    public void DoTakeDamage(S2C_Damage info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        entity.TakeDamage(info.Atkobjid, info.Skillno, info.Damagetype, info.Dmg, info.Currhp);

        if (entity.EntityId == MyEntityId)
        {
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.TakeDamage(info.Atkobjid, info.Skillno, info.Damagetype, info.Dmg, info.Currhp);
            }
#endif
        }

    }

    public void DoTakeDotDamage(S2C_DotDamage info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        entity.TakeDotDamage(info.Atkobjid, info.Abilactid, info.Dmg, info.Currhp);

        if (entity.EntityId == MyEntityId)
        {
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.TakeDotDamage(info.Atkobjid, info.Abilactid, info.Dmg, info.Currhp);
            }
#endif
        }

    }

    public void DoAbilityNotify(S2C_AbilityNfy info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
		{
            if (info.Objectid == MyEntityId)
            {
                MyCharInfo.AbilityNotify(info);
            }
            return;
        }
            

        for (int i = 0; i < info.AbilsLength; ++i)
        {
            Ability ability = info.Abils(i).Value;
            E_AbilityType abilityType = (E_AbilityType)ability.Type;
            //ZLog.Log(ZLogChannel.Entity, $"{abilityType} - 변경된 값 : {ability.Value}");

            entity.AbilityNotify(abilityType, ability.Value);
        }
    }

    public void DoAddAbilityAction(S2C_AddAbilAction info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
		{
            if(true == Container.TryGetEntityData(info.Objectid, out var data))
			{
                var myData = data.To<ZPawnDataBase>();
                if(null != myData)
				{
                    myData.AddAbilityAction(info.Abilactiontid, info.Restsec, info.NotConsume);
                }
			}
            return;
        }            

        entity.AddAbilityAction(info.Abilactiontid, info.Restsec, info.NotConsume);

        if (entity.EntityId == MyEntityId)
        {
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.AddAbilityAction(info.Abilactiontid, info.Restsec, info.NotConsume);
            }
#endif
        }
    }

    public void DoDelAbilityAction(S2C_DelAbilAction info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
        {            
            if (true == Container.TryGetEntityData(info.Objectid, out var data))
            {
                var myData = data.To<ZPawnDataBase>();
                if (null != myData)
                {
                    myData.RemoveAbilityAction(info.Abilactiontid);
                }
            }
            return;
        }

        entity.RemoveAbilityAction(info.Abilactiontid);

#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.RemoveAbilityAction(info.Abilactiontid);
            }
#endif
    }

    public void DoChangeMezState(S2C_ChangeMezState info)
    {
        var entity = Container.GetEntity(info.Objectid);

        if (null == entity)
            return;

        entity.ChangeMezState(info.Mezstate);

#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.ChangeMezState(info.Mezstate);
            }
#endif
    }

    public void DoChangePet(S2C_ChangePet info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;

        entity.To<ZPawnCharacter>().SetChangePet(info.PetTid);

#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.To<ZPawnCharacter>().SetChangePet(info.PetTid);
            }
#endif
    }

    public void DoChangeChange(S2C_ChangeChange info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;

        entity.To<ZPawnCharacter>().SetChangeChange(info.ChangeTid);

#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.To<ZPawnCharacter>().SetChangeChange(info.ChangeTid);
            }
#endif
    }

    /// <summary> 탈것은 mmo에서 장착한다. (추후 Web에서 장착된다면 수정해야함.) </summary>
    public void DoEquipVehicle(CS_EquipVehicle info)
    {
        //내꺼만 날라와야함!!!
        var charData = ZNet.Data.Me.CurCharData;
        charData.MainVehicle = info.Vehicletid;
        mEventEquipRideVehicle.Invoke(info.Vehicletid);
    }

    public void DoRideVehicle(CS_RideVehicle info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;

        uint vehicleTid = info.Vehicletid;

        //내 캐릭터일 경우 처리
        if(info.Objectid == MyEntityId)
        {
            if (0 >= vehicleTid)
            {
                //쿨타임 등록
                var charData = ZNet.Data.Me.CurCharData;
                ulong coolTime = 10;

                if (DBPet.TryGet(charData.MainVehicle, out var petData))
                {
                    coolTime = petData.CoolTime;
                }

                charData.VehicleEndCoolTime = TimeManager.NowSec + coolTime;
            }

            mEventUpdateRideVehicle?.Invoke(vehicleTid > 0);
#if ENABLE_SHADOW_PLAYER
            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

            if (shadowEntity)
            {
                shadowEntity.To<ZPawnCharacter>().SetChangeVehicle(vehicleTid);
            }
#endif
        }

        entity.To<ZPawnCharacter>().SetChangeVehicle(vehicleTid);
    }

    public void DoChangeClass(S2C_ChangeClass info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;
    
        //entity.To<ZPawnCharacter>().DoSetChangeClass(info.);

//#if ENABLE_SHADOW_PLAYER
//            var shadowEntity = Container.GetEntity(SHADOW_PLAYER_ENTITY_ID);

//            if (shadowEntity)
//            {
//                shadowEntity.To<ZPawnCharacter>().DoSetChangeClass(info.ChangeTid);
//            }
//#endif
    }

    /// <summary> 성향 변경시 처리 </summary>
    public void DoChangeTendency(S2C_ChangeTendency info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;

        entity.SetTendency(info.Tendency);
    }

    public void DoChangeGuildInfo(S2C_ChangeGuildInfo info)
    {
        var entity = Container.GetEntity(info.Objectid);
        if (null == entity)
            return;

        var character = entity.To<ZPawnCharacter>();

        if (null == character)
            return;

        character.UpdateGuildInfo(info.GuildmarkTid, info.GuildId);
    }

    public void DoObjectCasting(S2C_Casting info)
    {
        var entity = Container.GetEntity(info.Objectid);
        
        if (null == entity)
        {
            return;
        }

        ZObject gatherObject = Container.GetEntity(info.Target) as ZObject;

        if(gatherObject != null)
        {
            gatherObject.IsGathered = info.Duration == 0 ? false : true;
        }

        if (entity.EntityId == MyEntityId)
        {
            MyEntity.ShowGatherInterface(info.Duration, gatherObject);
        }
    }

    /// <summary> 스탯 프리뷰 </summary>
    public void DoUpdateStatPreview(CS_StatPreview info)
    {
        Dictionary<E_AbilityType, float> stats = new Dictionary<E_AbilityType, float>();

        for(int i = 0; i < info.AbilsLength; ++i)
        {
            var abil = info.Abils(i).Value;
            var type = (E_AbilityType)abil.Type;
            var value = abil.Value;

            if(type == E_AbilityType.FINAL_MAX_MAGIC_ATTACK)
            {
                value /= DBConfig.MagicAttackViewValue;
            }

            stats.Add(type, value);
        }

        mEventStatPreviewUpdated?.Invoke(stats);
    }

    // ==========================================================================================================

    /// <summary> 해당 EntityId 의 Entity 데이터를 얻어온다. </summary>
    public EntityDataBase GetEntityData(uint entityId)
    {
        return Container.GetEntityData(entityId);
    }

    /// <summary> 해당 EntityId 의 Entity 데이터를 얻어온다. </summary>
    public bool TryGetEntityData(uint entityId, out EntityDataBase data)
    {
        return Container.TryGetEntityData(entityId, out data);
    }

    /// <summary> 해당 EntityId 의 Entity Object를 얻어온다. </summary>
    public ZPawn GetEntity(uint entityId)
    {
        return Container.GetEntity(entityId);
    }

    /// <summary> 해당 EntityId 의 Entity Object를 얻어온다. </summary>
    public bool TryGetEntity(uint entityId, out ZPawn entity)
    {
        return Container.TryGetEntity(entityId, out entity);
    }

    /// <summary> charID로 EntityDataBase 를 얻는다, EntityObject를 얻고싶다면 EntityDataBase 의 EntityId로 다시 찾을것(안정성&성능)  </summary>
    public EntityDataBase FindEntityDataByCharID( ulong charId )
    {
        return Container.FindEntityDataByCharID( charId );
    }

    /// <summary> charID로 EntityBase 를 얻는다, </summary>
    public EntityBase FindEntityByCharID(ulong charId)
    {
        return Container.FindEntityByCharID(charId);
    }

    /// <summary> charID로 EntityBase 를 얻는다, </summary>
    public EntityBase FindEntityByTid(uint tid, E_UnitType entityType)
    {
        return Container.FindEntityByTid(tid, entityType);
    }

    /// <summary> 모든 캐릭터 가지고 오기 </summary>
    public List<ZPawn> GetAllPawn(E_UnitType type = E_UnitType.None, uint findTid = 0)
	{
        return Container.GetAllPawn(type);
    }

    /// <summary> 씬 로딩 완료후 호출됨 </summary>
    public void SceneLoadedComplete()
    {
        mEventSceneLoaded?.Invoke();
    }

    #region ::======== Event ========::    
    private Action<bool> mEventUpdateRideVehicle;
    private Action<uint> mEventEquipRideVehicle;
    private Action mEventCreateMyEntity;
    private Action<uint, ZPawn> mEventCreateEntity;
    private Action<uint> mEventRemoveEntity;
    private Action<uint, ZPawn> mEventDieEntity;
    private Action mEventSceneLoaded;
    /// <summary> 무게가 변경되었을 경우 알림 </summary>
    private Action<EntityBase, float, float> mEventChangeWeight;
    private Action<Dictionary<E_AbilityType, float>> mEventStatPreviewUpdated;

    /// <summary> 내 캐릭터 생성시 호출됨. (이미 있다면 바로 알림) </summary>    
    public void DoAddEventCreateMyEntity(Action action)
    {        
        DoRemoveEventCreateMyEntity(action);
        mEventCreateMyEntity += action;

        if (null != MyEntity)
        {
            action?.Invoke();
        }
    }

    public void DoRemoveEventCreateMyEntity(Action action)
    {
        mEventCreateMyEntity -= action;
    }

    public void DoAddEventCreateEntity(Action<uint, ZPawn> action)
    {
        DoRemoveEventCreateEntity(action);
        mEventCreateEntity += action;
    }

    public void DoRemoveEventCreateEntity(Action<uint, ZPawn> action)
    {
        mEventCreateEntity -= action;
    }

    public void DoAddEventRemoveEntity(Action<uint> action)
    {
        DoRemoveEventRemoveEntity(action);
        mEventRemoveEntity += action;
    }

    public void DoRemoveEventRemoveEntity(Action<uint> action)
    {
        mEventRemoveEntity -= action;
    }

    public void DoAddEventDieEntity(Action<uint, ZPawn> action)
    {
        DoRemoveEventDieEntity(action);
        mEventDieEntity += action;
    }

    public void DoRemoveEventDieEntity(Action<uint, ZPawn> action)
    {
        mEventDieEntity -= action;
    }

    /// <summary> 씬 로딩 완료 이벤트 등록 </summary>
    public void DoAddEventSceneLoaded(Action action)
    {
        DoRemoveEventSceneLoaded(action);
        mEventSceneLoaded += action;
    }

    /// <summary> 씬 로딩 완료 이벤트 해제 </summary>
    public void DoRemoveEventSceneLoaded(Action action)
    {
        mEventSceneLoaded -= action;
    }

    public void DoAddEventUpdateRideVehicle(Action<bool> action)
    {
        DoRemoveUpdateRideVehicle(action);
        mEventUpdateRideVehicle += action;
    }

    public void DoRemoveUpdateRideVehicle(Action<bool> action)
    {
        mEventUpdateRideVehicle -= action;
    }

    public void DoAddEventEquipRideVehicle(Action<uint> action)
    {
        DoRemoveEquipRideVehicle(action);
        mEventEquipRideVehicle += action;
    }

    public void DoRemoveEquipRideVehicle(Action<uint> action)
    {
        mEventEquipRideVehicle -= action;
    }

    /// <summary> 기믹이나 pc의 무게가 변경되었을 경우 처리 </summary>
    public void DoAddEventChangeWeight(Action<EntityBase, float, float> action)
    {
        DoRemoveEventChangeWeight(action);
        mEventChangeWeight += action;
    }

    public void DoRemoveEventChangeWeight(Action<EntityBase, float, float> action)
    {
        mEventChangeWeight -= action;
    }

    /// <summary> 스탯 갱신 관련 처리 </summary>
    public void DoAddEventStatPreviewUpdated(Action<Dictionary<E_AbilityType, float>> action)
    {
        DoRemoveEventStatPreviewUpdated(action);
        mEventStatPreviewUpdated += action;
    }

    public void DoRemoveEventStatPreviewUpdated(Action<Dictionary<E_AbilityType, float>> action)
    {
        mEventStatPreviewUpdated -= action;
    }
    #endregion

    /// <summary> entity 사망시 호출 </summary>
    public void DieEntity(ZPawn pawn)
    {
        if (null == pawn)
        {
            return;
        }

		// TODO : 타겟 이펙트 해제를 위해서. (전역으로 이벤트 받아서 처리할 곳이 여기 밖에 없어서...)
		if (null != MyEntity)
		{
			MyEntity.CheckTargetSelectFX();
		}

		mEventDieEntity?.Invoke(pawn.EntityId, pawn);
    }

    /// <summary> 무게 변경시 호출해주자 </summary>
    public void ChangeWeight(EntityBase entity, float preWeight, float newWeight)
    {
        mEventChangeWeight?.Invoke(entity, preWeight, newWeight);
    }

#if ENABLE_SHADOW_PLAYER
    /// <summary> 해당 EntityID의 캐릭터 정보를 생성 (현재 ShadowPlayer용) </summary>
    private S2C_AddCharInfo GetCharacterInfo(uint entityId, S2C_AddCharInfo info)
    {
        FlatBufferBuilder builder = new FlatBufferBuilder(1);
        var bb = S2C_AddCharInfo.CreateS2C_AddCharInfo(builder
            , entityId
            , builder.CreateString($"Dummy_{MyEntityId}")
            , info.CharTid
            , ServerPos3.CreateServerPos3(builder, info.Pos.Value.X, info.Pos.Value.Y, info.Pos.Value.Z)
            , info.Dir
            , info.Movespeed
            , ServerPos3.CreateServerPos3(builder, info.DestPos.Value.X, info.DestPos.Value.Y, info.DestPos.Value.Z)
            , info.ChangeId
            , info.CurHp
            , info.MaxHp
            , info.CurMp
            , info.MaxMp);

        builder.Finish(bb.Value);
        return S2C_AddCharInfo.GetRootAsS2C_AddCharInfo(builder.DataBuffer);
    }
#endif
}
