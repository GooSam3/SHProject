using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> entity ability Action 관리 </summary>
public class EntityComponentAbilityAction : EntityComponentBase<ZPawn>
{
    private Dictionary<uint, EntityAbilityAction> m_dicAbilityAction = new Dictionary<uint, EntityAbilityAction>();

    // ljh : 어빌리티액션 변경 이벤트, ui출력용
    public event Action<Dictionary<uint, EntityAbilityAction>> OnAblityActionChanged = delegate { };

    /// <summary> 상태이상 어빌리티가 걸린 카운트 </summary>
    private int MezAbilityCount = 0;
    private ulong BuffEndTime = 0;
    private uint AbilityActionId = 0;

    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();
        m_dicAbilityAction.Clear();
        MezAbilityCount = 0;

        //생성시 어빌리티 액션 추가
        foreach( var abilityAction in Owner.PawnData.dicAbilityAction ) {
            AddAbilityAction( abilityAction.Key, abilityAction.Value.Key, abilityAction.Value.Value);
        }

        Owner.PawnData.dicAbilityAction.Clear();
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();
        RemoveAll();
    }

    private void SetBuffEndTime()
    {
        List<SkillInfo> skillInfoList = ZPawnManager.Instance.MyEntity.SkillSystem.GetSkills();

        SkillInfo skillInfo = skillInfoList.Find(skill => skill.SkillTable.AbilityActionID_01 == AbilityActionId);

        if(skillInfo != null)
        {
            ZNet.Data.Me.CurCharData.SetSkillBuffEndTime(skillInfo.SkillId, BuffEndTime);
            //skillInfo.SetBuffEndTime(BuffEndTime);
        }
    }

    /// <summary> 어빌리티 액션을 추가한다. </summary>
    public void AddAbilityAction(uint abilityActionId, ulong endTime, bool bNotConsume)
    {
        AbilityActionId = abilityActionId;
        BuffEndTime = endTime;

        ZPawnManager.Instance.DoAddEventCreateMyEntity(SetBuffEndTime);

        if (false == m_dicAbilityAction.TryGetValue(abilityActionId, out EntityAbilityAction action))
        {
            action = CreateAbilityAction(abilityActionId, endTime, bNotConsume);
            if(null != action)
            {
                m_dicAbilityAction.Add(abilityActionId, action);
            }   
        }
        else
        {
            action.StartAction(endTime, bNotConsume);
        }

        OnAblityActionChanged?.Invoke(m_dicAbilityAction);
    }

    /// <summary> 어빌리티 액션을 제거한다. </summary>
    public void RemoveAbilityAction(uint abilityActionId)
    {
        if (m_dicAbilityAction.TryGetValue(abilityActionId, out EntityAbilityAction action))
        {
            action.EndAction();
            m_dicAbilityAction.Remove(abilityActionId);
        }

        OnAblityActionChanged?.Invoke(m_dicAbilityAction);
    }

    private void RemoveAll()
    {
        foreach(var action in m_dicAbilityAction)
        {
            action.Value.EndAction();
        }

        m_dicAbilityAction.Clear();

        OnAblityActionChanged?.Invoke(m_dicAbilityAction);
    }

    private EntityAbilityAction CreateAbilityAction(uint abilityActionId, ulong endTime, bool bNotConsume)
    {
        return new EntityAbilityAction(this, abilityActionId, endTime, bNotConsume);
    }

    public void AddMezAbility()
    {
        if(0 >= MezAbilityCount)
        {
            MezAbilityCount = 0;
            Owner.SetAnimParameter(E_AnimParameter.Stun_001, true);
        }

        ++MezAbilityCount;
    }

    public void RemoveMezAbility()
    {
        MezAbilityCount = Mathf.Max(MezAbilityCount - 1, 0);

        if(0 >= MezAbilityCount)
        {
            Owner.SetAnimParameter(E_AnimParameter.Stun_001, false);
        }
    }

    public EntityAbilityAction FindAbilityAction( uint abilityActionId )
    {
        var it = m_dicAbilityAction.GetEnumerator();
        while( it.MoveNext() ) {
            if( it.Current.Key == abilityActionId ) {
                return it.Current.Value;
            }
        }
        return null;
    }
}
