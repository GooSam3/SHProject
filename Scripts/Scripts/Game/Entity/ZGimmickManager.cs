using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_SHADOW_PLAYER
using FlatBuffers;
#endif

/// <summary> 모든 기믹을 관리하는 class </summary>
public class ZGimmickManager : Zero.Singleton<ZGimmickManager>
{
    /// <summary> Id에 의해 관리되는 사당 입구 </summary>
    private Dictionary<uint, ZTempleEntrance> m_dicTempleEntrance = new Dictionary<uint, ZTempleEntrance>();

    /// <summary> Id에 의해 관리되는 기믹 </summary>
    private Dictionary<string, List<ZGimmick>> m_dicGimmick = new Dictionary<string, List<ZGimmick>>();

    /// <summary> 생성된 모든 기믹 </summary>
    private List<ZGimmick> m_listAllGimmick = new List<ZGimmick>();

    /// <summary> 생성된 모든 보물상자 </summary>
    public List<ZGA_Chest> m_listAllChest { get; private set; } = new List<ZGA_Chest>();

    /// <summary> 확산되는 속성을 체크 하고 추가 기능을 발동하게 하기 위한 클래스 </summary>
    private List<ZGimmick_SpreadAttributeChecker> m_listSPreadAttributeChecker = new List<ZGimmick_SpreadAttributeChecker>();

    /// <summary> 확산되는 속성을 체크 하고 추가 기능을 발동하게 하기 위한 클래스 </summary>
    private List<ZGimmick> m_listIceWater = new List<ZGimmick>();

    /// <summary> 화염 속성 확산 체크 및 발동 프리펩 </summary>
    [Header("화염 속성 확산을 체크하고 셋팅된 기믹을 발동")]
    [SerializeField]
    private GameObject FireSpreadAttributeCheckerPrefab = null;

    /// <summary> 화염 속성 확산 체크 및 발동 프리펩 </summary>
    [Header("전기 속성 확산을 체크하고 셋팅된 기믹을 발동")]
    [SerializeField]
    private GameObject ElectricSpreadAttributeCheckerPrefab = null;

    /// <summary> 화염 속성 확산 체크 및 발동 프리펩 </summary>
    [Header("얼음 속성 확산을 체크하고 셋팅된 기믹을 발동")]
    [SerializeField]
    private GameObject IceSpreadAttributeCheckerPrefab = null;

    /// <summary> 화염 속성 확산 체크 및 발동 프리펩 </summary>
    [Header("빛 속성 확산을 체크하고 셋팅된 기믹을 발동")]
    [SerializeField]
    private GameObject LightSpreadAttributeCheckerPrefab = null;

    /// <summary> 화염 속성 확산 체크 및 발동 프리펩 </summary>
    [Header("어둠 속성 확산을 체크하고 셋팅된 기믹을 발동")]
    [SerializeField]
    private GameObject DarkSpreadAttributeCheckerPrefab = null;

    /// <summary> 사당에서 먹은 열쇠 아이템 </summary>
    public List<ZGimmick> GimmickItems_Key = new List<ZGimmick>();

    private const uint TEMPLE_ENTITY_ID = 0x2000000;

    /// <summary> 클라에서 소환할 기본 npc id.  </summary>
    private const uint TEMPLE_NPC_ID = 0x0001000;

    private uint CurrentMonsterEntityId;

    private uint CurrenNpcEntityId;

    protected override void Init()
    {
        base.Init();

        CurrentMonsterEntityId = TEMPLE_ENTITY_ID;
        CurrenNpcEntityId = TEMPLE_NPC_ID;

        //기본 AttributeChecker 등록
        AddDefaultAttributeChecker(FireSpreadAttributeCheckerPrefab);
        AddDefaultAttributeChecker(ElectricSpreadAttributeCheckerPrefab);
        AddDefaultAttributeChecker(IceSpreadAttributeCheckerPrefab);
        AddDefaultAttributeChecker(LightSpreadAttributeCheckerPrefab);
        AddDefaultAttributeChecker(DarkSpreadAttributeCheckerPrefab);
    }

    /// <summary> 기본 AttributeChecker를 등록한다. </summary>
    private void AddDefaultAttributeChecker(GameObject prefab)
    {
        if(null == prefab)
        {
            return;
        }
        var checker = GameObject.Instantiate(prefab).GetComponent<ZGimmick_SpreadAttributeChecker>();

        if (null == checker)
        {
            return;
        }

        checker.transform.parent = transform;
        checker.transform.localPosition = Vector3.zero;
        checker.transform.localRotation = Quaternion.identity;
        checker.IsDefault = true;

        AddSpreadAttributeChecker(checker);
    }

    /// <summary> 기믹 초기화 완료후 호출됨 </summary>
    public void PostInitializeGimmick(ZGimmick gimmick)
    {
        mEventSpawnGimmick?.Invoke(gimmick.GimmickId, gimmick);
    }

    /// <summary> 사당 입구 추가 </summary>
    public void AddTempleEntrance(uint id, ZTempleEntrance temple)
    {
        if(m_dicTempleEntrance.ContainsKey(id))
        {
            ZLog.LogError(ZLogChannel.Temple, $"이미 추가된 사당 입구다. - {id}");
            return;
        }
        m_dicTempleEntrance.Add(id, temple);
    }

    /// <summary> 사당 입구 제거 </summary>
    public void RemoveTempleEntrance(uint id)
    {
        m_dicTempleEntrance.Remove(id);
    }

    /// <summary> 사당 입구 갱신 </summary>
    public void RefreshTempleEntranceAll()
    {
        foreach(var temple in m_dicTempleEntrance)
        {
            temple.Value.Refresh();
        }
    }

    /// <summary> 사당 입구 갱신 </summary>
    public void RefreshTempleEntrance(uint id)
    {
        if(false == m_dicTempleEntrance.TryGetValue(id, out var temple))
        {
            return;
        }

        temple.Refresh();
    }

    /// <summary> 관리하는 기믹을 추가 </summary>
    public void Add(ZGimmick gimmick)
    {
        if(false == m_listAllGimmick.Contains(gimmick))
        {
            m_listAllGimmick.Add(gimmick);
        }
            
        if (string.IsNullOrEmpty(gimmick.GimmickId))
        {
            return;
        }

        if (false == m_dicGimmick.ContainsKey(gimmick.GimmickId))
        {
            m_dicGimmick.Add(gimmick.GimmickId, new List<ZGimmick>());            
        }

        m_dicGimmick[gimmick.GimmickId].Add(gimmick);
    }

    /// <summary> 관리하던 기믹이 제거됨 </summary>
    public void Remove(ZGimmick gimmick)
    {        
        m_listAllGimmick.Remove(gimmick);
        if(m_dicGimmick.TryGetValue(gimmick.GimmickId, out var gimmicks))
        {
            gimmicks.Remove(gimmick);
        }

        mEventDespawnGimmick?.Invoke(gimmick);
    }

    public void InvokeAction(ZGimmick gimmick, E_GimmickActionInvokeType invokeType)
    {
        mEventGimmickActionInvoke?.Invoke(gimmick, invokeType);
    }

    public bool TryGetValue(string id, out List<ZGimmick> gimmick)
    {
        return m_dicGimmick.TryGetValue(id, out gimmick);
    }

    public bool TryGetEntranceValue(uint id, out ZTempleEntrance entrance)
    {
        return m_dicTempleEntrance.TryGetValue(id, out entrance);
    }

    public IList<ZGimmick> AllGimmick()
    {
        return m_listAllGimmick.AsReadOnly();
    }

    public IList<ZGA_Chest> AllChest()
	{
        return m_listAllChest.AsReadOnly();

    }

    public void Clear()
    {
        m_dicGimmick.Clear();
        m_listAllGimmick.Clear();
        m_dicTempleEntrance.Clear();
        m_listAllChest.Clear();
        GimmickItems_Key.Clear();

        mEventSpawnGimmick = null;
        mEventGimmickActionInvoke = null;

        CurrentMonsterEntityId = TEMPLE_ENTITY_ID;
        CurrenNpcEntityId = TEMPLE_NPC_ID;
    }

    /// <summary> 체크할 속성을 추가한다. </summary>
    public void AddSpreadAttributeChecker(ZGimmick_SpreadAttributeChecker checker)
    {
        if(m_listSPreadAttributeChecker.Contains(checker))
        {
            return;
        }

        m_listSPreadAttributeChecker.Add(checker);
    }

    /// <summary> 체크하던 속성을 제거한다. </summary>
    public void RemoveSpreadAttributeChecker(ZGimmick_SpreadAttributeChecker checker)
    {
        m_listSPreadAttributeChecker.Remove(checker);
    }

    /// <summary> 확산되는 속성이 활성화된 기믹을 추가한다. </summary>
    public void AddSpreadAttribute(ZGimmick gimmick, E_UnitAttributeType attributeType)
    {
        foreach(var checker in m_listSPreadAttributeChecker)
        {
            checker.Add(gimmick, attributeType);
        }
    }


    /// <summary> 확산되는 속성이 활성화된 기믹을 제거한다. </summary>
    public void RemoveSpreadAttribute(ZGimmick gimmick)
    {
        foreach (var checker in m_listSPreadAttributeChecker)
        {
            checker.Remove(gimmick);
        }
    }

    /// <summary> 확산 속성 체크 초기화 </summary>
    public void ClearSpreadAttribute()
    {
        foreach (var checker in m_listSPreadAttributeChecker)
        {
            checker.Clear();
        }
    }

    /// <summary> 얼어야 하는 기믹을 추가한다. </summary>
    public void AddIceWater(ZGimmick gimmick)
    {
        if (null == m_listIceWater.Find(d => d == gimmick))
            m_listIceWater.Add(gimmick);
    }

    /// <summary> 얼려야 하는 기믹인지. </summary>
    public bool IsIceWater(ZGimmick gimmick)
	{
        return null != m_listIceWater.Find(d => d == gimmick);
    }

    /// <summary> 얼려야 하는 기믹인지. </summary>
    public void RemoveIceWater(ZGimmick gimmick)
    {
        if (null != m_listIceWater.Find(d => d == gimmick))
            m_listIceWater.Remove(gimmick);
    }

    public List<ZGimmick> GetIceWaterGimmicks()
    {
        return m_listIceWater;
    }

    /// <summary> 몬스터 추가 </summary>
    public uint AddMonster()
    {        
        return CurrentMonsterEntityId++;
    }

    public uint AddNpc()
	{
        // TODO :: npc 싱글용으로 생성하자.
        return CurrenNpcEntityId++;
    }

    /// <summary> 보물상자 추가 </summary>
    public void AddChest(ZGA_Chest chest)
	{
        if (true == m_listAllChest.Contains(chest))
            return;

        m_listAllChest.Add(chest);
    }

    #region ===== :: Event :: =====

    /// <summary> 기믹이 생성되었을 경우 알림 </summary>
    private Action<string, ZGimmick> mEventSpawnGimmick;

    /// <summary> 기믹이 제거되었을 경우 알림 </summary>
    private Action<ZGimmick> mEventDespawnGimmick;

    /// <summary> 기믹 발동 타입이 변경되었을 경우 </summary>
    private Action<ZGimmick, E_GimmickActionInvokeType> mEventGimmickActionInvoke;

    /// <summary> 기믹이 생성되었을 경우 알림 </summary>
    public void DoAddEventSpawnGimmick(Action<string, ZGimmick> action)
    {
        DoRemoveEventSpawnGimmick(action);
        mEventSpawnGimmick += action;
    }

    public void DoRemoveEventSpawnGimmick(Action<string, ZGimmick> action)
    {
        mEventSpawnGimmick -= action;
    }

    /// <summary> 기믹이 생성되었을 경우 알림 </summary>
    public void DoAddEventDespawnGimmick(Action<ZGimmick> action)
    {
        DoRemoveEventDespawnGimmick(action);
        mEventDespawnGimmick += action;
    }

    public void DoRemoveEventDespawnGimmick(Action<ZGimmick> action)
    {
        mEventDespawnGimmick -= action;
    }

    /// <summary> 기믹이 해당 invokeType으로 발동되었을 경우 알림 </summary>
    public void DoAddEventGimmickActionInvoke(Action<ZGimmick, E_GimmickActionInvokeType> action)
    {
        DoRemoveEventGimmickActionInvoke(action);
        mEventGimmickActionInvoke += action;
    }

    public void DoRemoveEventGimmickActionInvoke(Action<ZGimmick, E_GimmickActionInvokeType> action)
    {
        mEventGimmickActionInvoke -= action;
    }


    #endregion

    #region ===== :: Target Search :: =====
    public ZGimmick SearchNearTarget(Transform selfTrans, float distance = Mathf.Infinity)
    {
        ZGimmick foundTarget = null;
        Vector3 diff;
        float curDistance;
        float sqrDistance = distance * distance;

        foreach (ZGimmick target in m_listAllGimmick)
        {
            if (false == target.IsTargetable)
                continue;
            if (false == target.IsRemainInvokeCount())
                continue;

            // 가장 가까운 적 정보 Caching 그리고, 반복
            diff = target.transform.position - selfTrans.position;
            curDistance = diff.sqrMagnitude;
            if (curDistance < sqrDistance)
            {
                foundTarget = target;
                sqrDistance = curDistance;
            }
        }

        return foundTarget;
    }

    public void TryGetTargets(Transform selfTrans, float distance, ref List<ZGimmick> gimmicks)
    {
        Vector3 diff;
        float sqrDistance = distance * distance;

        foreach (ZGimmick target in m_listAllGimmick)
        {
            if (false == target.IsTargetable)
                continue;

            // 가장 가까운 적 정보 Caching 그리고, 반복
            diff = target.transform.position - selfTrans.position;            
            if (diff.sqrMagnitude < sqrDistance)
            {
                gimmicks.Add(target);
            }
        }
    }
    #endregion
}
