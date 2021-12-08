using GameDB;
using UnityEngine;

/// <summary> 펫 </summary>
public abstract class ZPetBase : EntityBase
{
    /// <summary> Pet의 소유자 </summary>
    [SerializeField]
    [ReadOnly]
    protected ZPawnCharacter mOwnerCharacter;
        
    public ZPawnCharacter OwnerCharacter { get { return mOwnerCharacter; } }

    public override E_UnitType EntityType => E_UnitType.Pet;

    /// <summary> 펫(탈것) 생성 </summary>
    public static T CreatePet<T>(ZPawnCharacter owner, uint tid) where T : ZPetBase
    {
        var loadObj = Resources.Load<GameObject>($"Pawn/{typeof(T).Name}");
        if (null == loadObj)
        {
            ZLog.LogError(ZLogChannel.Entity, $"{typeof(T).Name} 을 찾을 수 없다.");
            return null;
        }

        GameObject go = GameObject.Instantiate(loadObj);

#if UNITY_EDITOR
        go.name = ($"{typeof(T).Name}_{owner.EntityId}");
#endif

        go.layer = UnityConstants.Layers.Entity;

        var petBase = go.AddComponent<T>();
        var petData = new ZPawnDataPet();
        petData.DoInitialize(tid);

        petBase.mOwnerCharacter = owner;
        petBase.DoInitialize(petData);
        return petBase;
    }

    protected override void OnInitializeEntityData(EntityDataBase data)
    {        
    }

    protected override void OnInitializeImpl()
    {
    }

    protected override void OnPostInitializeImpl()
    {        
    }

    protected override void OnInitializeTableDataImpl()
    {
    }

    protected override void OnSetDefaultComponentImpl()
    {
    }


    protected override void OnDestroyImpl()
    {
    }

    protected override void OnLoadedModelImpl()
    {
    }

    /// <summary> 스탯 컴포넌트 셋팅 </summary>
    protected override EntityComponentStatBase OnSetStatComponent()
    {
        return null;
    }

    protected override void CreateTargetUI()
    {

    }
}
