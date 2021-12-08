using FlatBuffers;
using GameDB;
using MmoNet;
using UnityEngine;

/// <summary> Character </summary>
public class ZPawnLobbyPc : ZPawnCharacter
{
    public override E_UnitType EntityType { get { return E_UnitType.Character; } }
    
    public static ZPawnLobbyPc CreateLobbyPc(uint characterTid, uint changeTid)
    {
        GameObject go = new GameObject("LobbyCharacter");
        go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        ZPawnLobbyPc pc = go.AddComponent<ZPawnLobbyPc>();
        pc.SetLobbyCharacter(characterTid, changeTid);

		return pc;
    }

	protected override EntityComponentModelBase OnSetModelComponent()
	{
		// 로비용 모델로드를 위해 컴포넌트 교체.
		return GetOrAddComponent<EntityComponentModelLobby>();
	}

	/// <summary> 캐릭터 셋팅 </summary>
	public void SetLobbyCharacter(uint characterTid, uint changeTid)
    {
        SetLobbyCharacter(GetCharacterInfo(characterTid, changeTid));
    }

    private void SetLobbyCharacter(S2C_AddCharInfo charInfo)
    {
        if (null == EntityData)
            EntityData = new ZPawnDataCharacter();
		else
		{
			if (CharacterData.TableId == charInfo.CharTid && CharacterData.ChangeTid == charInfo.ChangeId)
				return;
		}

        CharacterData.DoInitialize(charInfo);
        DoInitialize(EntityData);
    }

    /// <summary> flatbuffer 데이터 생성 </summary>
    private S2C_AddCharInfo GetCharacterInfo(uint characterId, uint changeId)
    {
        FlatBufferBuilder builder = new FlatBufferBuilder(1);
        var bb = S2C_AddCharInfo.CreateS2C_AddCharInfo(builder
            , 0
            , builder.CreateString("")
            , characterId
            , ServerPos3.CreateServerPos3(builder, 0, 0, 0)
            , 0
            , 0
            , ServerPos3.CreateServerPos3(builder, 0, 0, 0)
            , changeId
            , 0
            , 0
            , 0
            , 0);

        builder.Finish(bb.Value);
        return S2C_AddCharInfo.GetRootAsS2C_AddCharInfo(builder.DataBuffer);
    }

    /// <summary> 이동 컴포넌트 셋팅 </summary>
    protected override EntityComponentMovementBase OnSetMovementComponent()
    {
        return null;
    }

    /// <summary> 스탯 컴포넌트 셋팅 </summary>
    protected override EntityComponentStatBase OnSetStatComponent()
    {
        return null;
    }

    /// <summary> 전투 컴포넌트 셋팅 </summary>
    protected override EntityComponentCombat OnSetCombatComponent()
    {
        return null;
    }
    /// <summary> 어빌리티 액션 관련 컴포넌트 </summary>
    protected override EntityComponentAbilityAction OnSetAbilityActionComponent()
    {
        return null;
    }

    protected override void CreateTargetUI()
    {
        
    }

	GameObject lobbyEffect;
	string lobbyEffName = "Fx_PC_Kt_Lobby";

	protected override void OnLoadedModelImpl()
	{
		//base.OnLoadedModelImpl();

		// 이전 이펙트 반환
		if (null != lobbyEffect)
		{
			ZPoolManager.Instance.Return(lobbyEffect);
			lobbyEffect = null;
		}

		// 등작 연출 애니
		AnimComponent?.SetAnimParameter(E_AnimParameter.Lobby_001);
		// 강제 업데이트한번
		(AnimComponent as EntityComponentAnimation_Animator).Anim.Update(0f);

		if (DBResource.TryGetEffect(ResourceTable.LobbyEffectTID, out var effTable))
		{
			float animLength = AnimComponent.GetAnimLength(E_AnimStateName.Lobby_001);
			lobbyEffName = effTable.EffectFile;

			ZPoolManager.Instance.Spawn(E_PoolType.Effect, lobbyEffName, (effGO) =>
			{
				if (null == effGO)
					return;

				lobbyEffect = effGO;
				lobbyEffect.transform.SetParent(this.transform, false);
				lobbyEffect.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 180, 0));

				ZEffectComponent comp = effGO.GetOrAddComponent<ZEffectComponent>();				
				comp.StartTimer(animLength);
			});
		}
	}
}
