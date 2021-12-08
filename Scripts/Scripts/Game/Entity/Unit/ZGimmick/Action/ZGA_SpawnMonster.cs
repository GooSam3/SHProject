using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 몬스터 소환 및 Enable/Disable Gimmick </summary>
public class ZGA_SpawnMonster : ZGimmickActionBase
{
    [Header("소환할 몬스터들. (ZGimmickComp_SpawnMonster를 자식으로 추가하면 자동으로 셋팅됨)")]
    [ReadOnly]
    [SerializeField]
    private List<ZGimmickComp_SpawnMonster> m_listMonster = new List<ZGimmickComp_SpawnMonster>();

    [Header("몬스터 소환시 활성화 할 Gimmick 처리")]
    [SerializeField]
    private List<string> m_listBeforeEnableId = new List<string>();

    [Header("몬스터 소환시 비활성화 할 Gimmick 처리")]
    [SerializeField]
    private List<string> m_listBeforeDisableId = new List<string>();

    [Header("소환 지연 시간")]
    [SerializeField]
    private float SpawnDelayTime = 0f;

    [Header("모든 몬스터 제거 후 활성화 할 Gimmick 처리")]
    [SerializeField]
    private List<string> m_listDieEnableId = new List<string>();

    [Header("모든 몬스터 제거 후 비활성화 할 Gimmick 처리")]
    [SerializeField]
    private List<string> m_listDieDisableId = new List<string>();

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    private void Awake()
    {
        m_listMonster.AddRange(GetComponentsInChildren<ZGimmickComp_SpawnMonster>());
    }

    protected override void InvokeImpl()
    {
        SetEnableGimmick(ref m_listBeforeEnableId, true);
        SetEnableGimmick(ref m_listBeforeDisableId, false);

        Invoke("SpawnMonster", SpawnDelayTime);
    }

    protected override void CancelImpl()
    {
    }

    /// <summary> 몬스터 소환 </summary>
    private void SpawnMonster()
    {
        foreach(var spawner in m_listMonster)
        {
            spawner.Spawn(this);
        }
    }

    /// <summary> 몬스터 사망 </summary>
    public void DieMonster()
    {
        foreach(var spawner in m_listMonster)
        {
            if (spawner.IsDead)
                continue;

            return;
        }

        //클리어!!
        SetEnableGimmick(ref m_listDieEnableId, true);
        SetEnableGimmick(ref m_listDieDisableId, false);
    }

    private void SetEnableGimmick(ref List<string> list, bool bEnable)
    {
        foreach(var id in list)
        {
            ZTempleHelper.EnableGimmicks(id, bEnable, AttributeLevel, false);
        }
    }
}