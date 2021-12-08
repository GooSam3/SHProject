using GameDB;
using UnityEngine;
using ZNet.Data;
using System;
/// <summary> Character </summary>
public abstract class ZPawnCharacter : ZPawn
{
    public override E_UnitType EntityType { get { return E_UnitType.Character; } }

    public E_CharacterType CharacterType { get; private set; }

    public E_CharacterType OriginalCharacterType { get; private set; }

    public E_UnitAttributeType OriginalAttributeType { get; private set; }
    
    public ZPet mMyPet { get; private set; }
    public ZVehicle MyVehicle { get; private set; }

    public ZPawnDataCharacter CharacterData { get { return EntityData.To<ZPawnDataCharacter>(); } }

    public override bool IsRiding { get { return null != MyVehicle; } }

    protected override void OnInitializeEntityData(EntityDataBase data)
    {
        base.OnInitializeEntityData(data);
        SetCharacterInfo();
        SetChangePet(CharacterData.PetTid);
        SetChangeVehicle(CharacterData.VehicleTid);
    }

	protected override void OnLoadedModelImpl()
	{
        base.OnLoadedModelImpl();
        // 강림 적용시, 이펙트 연출
        if (false ==IsDead && CharacterData.ChangeTid != 0)
		{
			ZEffectManager.Instance.SpawnEffect(DBResource.Fx_Summon_Change, this.Position, this.Rotation, 0f, 1f, null);
		}
	}

    /// <summary> 캐릭터 정보 셋팅 </summary>
    private void SetCharacterInfo()
    {
        CharacterType = E_CharacterType.None;
        
        uint changeTid = CharacterData.ChangeTid;
        if (0 < changeTid)
        {
            if (DBChange.TryGet(changeTid, out var changetable))
            {
                //캐릭터 타입 셋팅
                CharacterType = changetable.UseAttackType;
                //속성 레벨 셋팅
                AttributeLevel = DBConfig.GetAttributeLevel(changetable.Grade);
            }
        }

        if (DBCharacter.TryGet(TableId, out var characterTable))
        {
            if (CharacterType == E_CharacterType.None)
            {
                //캐릭터 타입 셋팅
                CharacterType = characterTable.CharacterType;
                //속성 레벨 셋팅
                AttributeLevel = E_AttributeLevel.Level_1;
            }

            OriginalCharacterType = characterTable.CharacterType;            
        }
    }

	protected override void SetAttributeType()
    {
        UnitAttributeType = E_UnitAttributeType.None;

        uint changeTid = CharacterData.ChangeTid;
        if (0 < changeTid)
        {
            if (DBChange.TryGet(changeTid, out var changeTable))
            {
                UnitAttributeType = changeTable.AttributeType;                
            }
        }

        if (DBCharacter.TryGet(TableId, out var characterTable))
        {
            if (UnitAttributeType == E_UnitAttributeType.None)
                UnitAttributeType = characterTable.AttributeType;

            OriginalAttributeType = characterTable.AttributeType;
        }
    }

    #region ===== :: 애니메이션 :: =====
    /// <summary> 애니메이션의 이동 속도 비율을 변경한다. </summary>    
    public override void SetAnimMoveSpeed(float value)
    {
        base.SetAnimMoveSpeed(value);
        MyVehicle?.SetAnimMoveSpeed(value);
    }
    #endregion

    #region ===== :: 강림 :: =====       
    public virtual bool SetChangeChange(uint changeTid)
    {
        bool ret = false;
        if (CharacterData.ChangeTid != changeTid)
        {
            CharacterData.DoChangeClassTid(changeTid);
            ret = true;
        }
        SetModel();
        SetAttributeType();
        SetCharacterInfo();

        return ret;
    }
    #endregion

    #region ===== :: 펫 :: =====
    public virtual void SetChangePet(uint petTid)
    {
        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if (stageTable.StageType == E_StageType.Temple) return;

            CharacterData.DoChangePetTid(petTid);            
            CreatePet();
        }
    }    

    protected void CreatePet()
    {
        uint petTid = CharacterData.PetTid;

        DestroyPet();

        if (0 >= petTid)
        {
            return;
        }
        
        ZLog.Log(ZLogChannel.Pet, $"{CharacterData.PetTid} 펫이 생성되었습니다.");        
        mMyPet = ZPetBase.CreatePet<ZPet>(this, petTid);
    }

    protected void DestroyPet()
    {
        if (mMyPet != null)
        {
            GameObject.Destroy(mMyPet.gameObject);
            mMyPet = null;
        }
    }
    #endregion

    #region ===== :: 탈 것 :: =====
    private Action<ZVehicle> mEventChangeVehicle;

    public void DoAddEventChangeVehicle(Action<ZVehicle> action)
    {
        DoRemoveEventChangeVehicle(action);
        mEventChangeVehicle += action;

        if(null != MyVehicle && MyVehicle.IsLoadedModel)
        {
            action?.Invoke(MyVehicle);
        }
    }

    public void DoRemoveEventChangeVehicle(Action<ZVehicle> action)
    {
        mEventChangeVehicle -= action;
    }
    public virtual void SetChangeVehicle(uint vehicleTid)
    {
        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if (stageTable.RidingType == E_RidingType.NotRiding) return;

            CharacterData.DoChangeVehicleTid(vehicleTid);            
            CreateVehicle();
        }
    }

    protected void CreateVehicle()
    {
        uint vehicleTid = CharacterData.VehicleTid;

        DestroyVehicle();

        if (0 >= vehicleTid)
        {
            return;
        }

        ZLog.Log(ZLogChannel.Pet, $"{CharacterData.VehicleTid} 탈것이 생성되었습니다.");
        MyVehicle = ZPetBase.CreatePet<ZVehicle>(this, vehicleTid);
        MyVehicle.DoAddEventLoadedModel(LoadVehicleModel);
    }

    protected void DestroyVehicle()
    {
        if (MyVehicle != null)
        {
            MyVehicle.DoRemoveEventLoadedModel(LoadVehicleModel);
        
            GameObject.Destroy(MyVehicle.gameObject);
            MyVehicle = null;            
        }

        mEventChangeVehicle?.Invoke(null);
    }

    private void LoadVehicleModel()
    {
        mEventChangeVehicle?.Invoke(MyVehicle);
    }

    #endregion

    #region ===== :: 길드 :: =====
    /// <summary> 길드 정보 업데이트 </summary>
    public void UpdateGuildInfo(uint guildMarkTid, ulong guildId)
    {
        CharacterData.GuildMarkId = guildMarkTid;
        CharacterData.GuildId = guildId;

        bool bMyGuild = 0 >= Me.CurCharData.GuildId ? Me.CurCharData.GuildId == guildId : false;
        bool bAllience = Me.CurCharData.IsAllienceGuild(guildId);
        bool bHostile = Me.CurCharData.IsEnemyGuild(guildId);

        SetCustomConditionControl(E_CustomConditionControl.GuildMember, bMyGuild);
        SetCustomConditionControl(E_CustomConditionControl.AllianceGuild, bAllience);
        SetCustomConditionControl(E_CustomConditionControl.HostileGuild, bHostile);
    }
    #endregion

    /// <summary> 해당 클래스 인지 여부 (현재 Character만 사용한다.) </summary>
    public override bool CheckCharacterType(E_CharacterType characterType)
    {
        return characterType == E_CharacterType.None || characterType == E_CharacterType.All || characterType == CharacterType;
    }
}
