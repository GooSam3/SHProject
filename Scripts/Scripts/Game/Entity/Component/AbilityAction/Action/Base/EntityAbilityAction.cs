using GameDB;
using System.Collections.Generic;

/// <summary> entity ability Action 관련 처리 </summary>
public class EntityAbilityAction
{
    private EntityComponentAbilityAction OwnerComp;
    private ZPawn Owner { get { return OwnerComp.Owner; } }
    private uint mAbilityActionId;
    private ulong mAddedServerTime;//ljh : 어빌리티 액션 등록된 시간(서버시간), 버프, 디버프 정렬시 사용
    private ulong mAddedRemainTime; //버프 시작시 남은 시간
    private ulong mEndServerTime = 0;// ljh : 버프 종료시간(서버시간)
    private ulong mStartOffsetTime = 0; //버프 시작시 종료시간과 현재 시간의 차이
    private AbilityAction_Table mTable = null;
    private List<E_AbilityType> m_listAbilityType = new List<E_AbilityType>();

    private ZEffectComponent mEffectComp = null;

    //==외부노출용 프로퍼티
    public AbilityAction_Table Table => mTable;
    public ulong AddedServerTime => mAddedServerTime;

    public ulong AddedRemainTime = 0;
    /// <summary> 서버상 종료시간, IsNotConsume 이라면 현재 시간에서 최초 받은 데이터의 남은 버프시간을 추가하여 넘겨준다. </summary>
    public ulong EndServerTime => false == IsNotCunsume ? mEndServerTime : TimeManager.NowSec + mAddedRemainTime;
    public uint AbilityActionId => mAbilityActionId;

    public bool IsNotCunsume { get; private set; }
    public EntityAbilityAction(EntityComponentAbilityAction comp, uint abilityActionId, ulong endTime, bool bNotConsume)
    {
        OwnerComp = comp;
        mAbilityActionId = abilityActionId;
        if (DBAbilityAction.TryGet(mAbilityActionId, out mTable))
        {
            StartAction(endTime, bNotConsume);
            DBAbilityAction.GetAbilityTypeList(mTable, ref m_listAbilityType);

            //어빌리티 연출한다.
            ApplyAbility();
        }
    }

    public void StartAction(ulong endTime, bool bNotConsume)
    {        
        mEndServerTime = endTime;
        mAddedServerTime = TimeManager.NowSec;
        IsNotCunsume = bNotConsume;
        mAddedRemainTime = mEndServerTime >= mAddedServerTime ? mEndServerTime - mAddedServerTime : 0;
        bool removeImmediatelyPawnDie = mTable.AbilityActionType == E_AbilityActionType.Mez;

        if (0 < mEndServerTime)
		{
            Owner.SpawnEffect(mTable.EffectID, -1, 1f, (comp) => {
                if (comp != null) {
                    mEffectComp?.Despawn();
                    mEffectComp = comp;
                    comp.RemoveImmediatelyPawnDie = removeImmediatelyPawnDie;
                }
            });
        }
        else {
            //Owner.SpawnEffect(mTable.EffectID);
            Owner.SpawnEffect(mTable.EffectID, 0, 1f, (comp) => {
                if (comp != null) {
                    comp.RemoveImmediatelyPawnDie = removeImmediatelyPawnDie;
                }
            });
        }

        //ZLog.Log(ZLogChannel.Skill, $"어빌리티 추가 - 어빌리티 ID : {mAbilityActionId}, 버프 끝나는 시각 {mEndServerTime}");
    }

    public void EndAction()
    {
        //Mez 상태를 해제한다.
        UnapplyAbility();
        DespawnEffect();
        //ZLog.Log(ZLogChannel.Skill, $"어빌리티 제거 - 어빌리티 ID : {mAbilityActionId}");
    }

    private void DespawnEffect()
    {
        mEffectComp?.Despawn();
        mEffectComp = null;
    }

    private void ApplyAbility()
    {
        UpdateAbility(true);
    }

    private void UnapplyAbility()
    {
        UpdateAbility(false);
    }

    private void UpdateAbility(bool bApply)
    {
        foreach (var abilityType in m_listAbilityType)
        {
            switch (abilityType)
            {
                case E_AbilityType.STATE_STUN:
                case E_AbilityType.STATE_FEAR:
                case E_AbilityType.STATE_KNOCKBACK:
                case E_AbilityType.STATE_PULL:
                    {
                        if(bApply)
                        {
                            OwnerComp.AddMezAbility();
                        }
                        else
                        {
                            OwnerComp.RemoveMezAbility();
                        }
                    }
                    break;
                case E_AbilityType.PK_CONDITION:
                    {
                        //중복되지 않는다고 한다.
                        Owner.SetCustomConditionControl(E_CustomConditionControl.Pk, bApply);
                    }
                    break;
            }
        }
    }
}
