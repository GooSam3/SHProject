using GameDB;
using System;
using UnityEngine;

public abstract class EntityStateSkillBase : EntityStateBase
{
    /// <summary> 다음 이동 딜레이 타임 </summary>
    private const float MOVE_DELAY_TIME = 0.2f;
    /// <summary> 상태이상 메시지 딜레이 타임 </summary>
    private const float MEZ_STATE_MESSAGE_DELAY_TIME = 1f;

    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableUpdate => false;

    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableLateUpdate => true;
    
    /// <summary> 사용할 스킬 </summary>
    protected SkillInfo mSkill;

    /// <summary> 스킬 사용 시간 </summary>
    private float mSkillUseTime = 0f;

    /// <summary> 마지막 이동 시간 </summary>
    private float mLastMoveTime = 0f;
    
    /// <summary> 발동 횟수 </summary>
    protected int InvokeCount { get; private set; }

    /// <summary> 바닥 클릭 이동 입력 </summary>
    private Vector3? InputPosition = null;

    /// <summary> 이동 속도 </summary>
    private float InputPositionMoveSpeed = 5f;

    /// <summary> 목적지 </summary>
    private Vector3? GoalPosition = null;

    /// <summary> 내 포지션 캐싱 </summary>
    private Vector3? CachedMyPosition = null;

    /// <summary> 현재 타겟 </summary>
    protected EntityBase CurrentTarget { get; private set; }

    /// <summary> JoyStick 이동 무시 </summary>
    private bool IsIgnoreDirMove = false;

    /// <summary> 스킬 사용 패킷 응답 대기 </summary>
    private bool IsWaitReceiveAttack = false;

    private float mLastShowMezStateMessageTime = 0f;

    public override void OnEnter(Action callback, params object[] args)
    {
        base.OnEnter(callback, args);

        IsIgnoreDirMove = true;
        IsWaitReceiveAttack = false;

        InvokeCount = 0;
        InputPosition = null;
        GoalPosition = null;
        CachedMyPosition = null;
        mLastMoveTime = 0f;
        mLastShowMezStateMessageTime = 0f;
        Init();
        SetSkill(args);
        SetTarget();

        ZPawnManager.Instance.DoAddEventReceiveAttack(HandleReceiveAttack);
    }

    public override void OnExit(Action callback)
    {
        //Parent.IsSkillAction = false;
        //Parent.IsSkipHitMotion = false;
        CurrentTarget = null;
        ZPawnManager.Instance.DoRemoveEventReceiveAttack(HandleReceiveAttack);
        base.OnExit(callback);        
    }

    protected override void DestroyImpl()
    {
        if(ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventReceiveAttack(HandleReceiveAttack);
        }
    }

    protected override void LateUpdateStateImpl()
    {
        if(Parent.IsDead)
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }
        if (IsWaitReceiveAttack)
        {
            return;
        }
            
        //스킬 셋팅 체크
        if (null == mSkill)
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        if (Parent.IsSkillAction)
        {
            return;
        }

        if(false == Check())
        {
            Parent.StopMove(Parent.transform.position);
            //Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        //방향키로 이동
        if (IsIgnoreDirMove == false && Parent.IsMovingDir())
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        //최초 시작시 JoyStick 이동중일경우 무시하기 위해
        IsIgnoreDirMove = false;

        if (null != InputPosition && 0 < InputPositionMoveSpeed)
        {
            Parent.MoveTo(InputPosition.Value, InputPositionMoveSpeed);
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        EntityBase target = CurrentTarget;

        if(null == target || target.IsDead)
        {
            //이동을 시작했다면 정지
            if(0 < mLastMoveTime)
            {
                Parent.StopMove(Parent.transform.position);
            }
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        //사거리 체크 후 이동 처리
        float skillDistance = mSkill.Distance;
        float distance = (Parent.transform.position - target.transform.position).magnitude;
        float targetRadius = target.Radius;

        if (skillDistance < (distance - targetRadius))
        {
            //마지막 이동시간에서 딜레이 시간 처리
            if(mLastMoveTime + MOVE_DELAY_TIME > Time.time)
            {
                return;
            }

            if (null != GoalPosition && GoalPosition == target.transform.position && null != CachedMyPosition && CachedMyPosition != Parent.transform.position)
            {
                return;
            }

            if(Parent.IsMezState(E_ConditionControl.NotMove))
            {
                ShowMezNotify(E_ConditionControl.NotMove);
                return;
            }

            CachedMyPosition = Parent.transform.position;
            GoalPosition = target.transform.position;
            //이동
            Parent.MoveTo(target.transform.position, Parent.MoveSpeed);
            mLastMoveTime = Time.time;
            PostMove();
            return;
        }

        GoalPosition = null;
        CachedMyPosition = null;

        //이동 정지
        Parent.StopMove(Parent.transform.position);

        if (false == CheckMezState())
        {
            ShowMezNotify(CheckMezStateType);
            return;
        }

        if(false == CheckUseSkill())
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }
        //공격 처리
        UseAttack();
    }

    private bool CheckUseSkill()
    {
        E_SkillSystemError error = SkillSystem.CheckUseSkill(mSkill.SkillId);

        if(error != E_SkillSystemError.None)
        {
            Parent.ShowSkillErrorMessage(error);
        }

        return error == E_SkillSystemError.None;
    }

    private void UseAttack()
    {
        mSkillUseTime = Time.time;

        float angleY = Parent.transform.rotation.eulerAngles.y;
        EntityBase target = CurrentTarget;

        //TODO :: 임시 처리 GameTick 관련 처리 필요
        uint endCastingTick = ZMmoManager.Instance.GameTick + (uint)(mSkill.SkillTable.CastingTime * 1000);// TimeManager.NowMs + (ulong)(mSkill.SkillTable.CastingTime * TimeHelper.Unit_SecToMs);
        if (CurrentTarget.EntityType != E_UnitType.Gimmick)
        {
            if (target != Parent)
            {
                angleY = VectorHelper.Axis3Angle((target.transform.position - Parent.transform.position));
            }

            //기믹이 아닐경우 처리            
            //Parent.UseSkill(Parent.transform.position, target.EntityId, mSkill.SkillId, SkillSpeedRate, angleY, endCastingTime);
            //일단 패킷 전송으로 변경
            IsWaitReceiveAttack = true;

            //패킷 타임아웃 예외처리
            Invoke(nameof(InvokeTimeout), 1f);

            ZMmoManager.Instance.Field.REQ_Attack(Parent.EntityId, Parent.transform.position, angleY, mSkill.SkillId, target.EntityId, mSkill.Combo, SkillSpeedRate);
        }
        else
        {
            //기믹일 경우처리
            Parent.UseSkillForGimmick(CurrentTarget, mSkill.SkillId, SkillSpeedRate, endCastingTick);
        }
        
        //Parent.IsSkillAction = true;        

        PostUseSkill();

        ++InvokeCount;
    }

    private void HandleReceiveAttack()
    {
        InvokeTimeout();

        //탑승중일 경우 내리기
        if (Parent.IsRiding)
        {
            Parent.SetChangeVehicle(0);
        }
    }

    private void InvokeTimeout()
    {
        CancelInvoke(nameof(InvokeTimeout));
        IsWaitReceiveAttack = false;
    }

    /// <summary> 타겟을 설정한다. </summary>
    protected virtual void SetTarget()
    {
        //스킬 및 옵션에 따라 타겟 셋팅되야함.

        switch(mSkill.TargetType)
        {
            case E_TargetType.Enemmy:
                {
                    bool isTargetChanged = false;

                    CurrentTarget = Parent.GetTarget();

                    if (null != CurrentTarget)
                    {
                        if (CurrentTarget == Parent || (CurrentTarget.EntityType != E_UnitType.Character && CurrentTarget.EntityType != E_UnitType.Monster && CurrentTarget.EntityType != E_UnitType.Gimmick))
                        {
                            CurrentTarget = null;
                            isTargetChanged = true;
                            //TODO :: pk,party등에 따라 체크해야함
                        }
                    }

                    if(null == CurrentTarget)
                    {
                        EntityBase target = null;
                        // 콜로세움은 적만 공격해야할듯?
                        if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
                            target = ZPawnTargetHelper.SearchTargetEnemyPC( ZPawnManager.Instance.MyEntity );
                        }
                        else {
                            target = ZPawnTargetHelper.SearchNearTarget( Parent, E_UnitType.Monster, Parent.Position, DBConfig.SearchTargetRange );
                        }

                        if (null == target)
                        {
                            if(mSkill.SkillTable.SkillType == E_SkillType.Normal)
                            {
                                //TODO :: 근처의 기믹을 찾음.
                                target = ZGimmickManager.Instance.SearchNearTarget(Parent.transform, DBConfig.SearchTargetRange);
                            }
                        }

                        isTargetChanged = true;
                        CurrentTarget = target;
                    }

                    //pvp 채널이 아닐때 캐릭터를 공격한다면 취소해야함.
                    if(null != CurrentTarget && CurrentTarget.EntityType == E_UnitType.Character)
                    {
                        if(false == ZGameModeManager.Instance.IsPvPChannel())
                        {
                            CurrentTarget = null;
                            isTargetChanged = true;
                        }
                    }
                    
                    if (isTargetChanged)
                    {
                        Parent.SetTarget(CurrentTarget);
                    }
                }
                break;
            case E_TargetType.Self:
                {
                    CurrentTarget = Parent;                    
                }
                return;
            case E_TargetType.Party:
                {
                    //TODO :: 파티 구현 후 처리 필요
                    CurrentTarget = Parent;
                }
                break;
        }
    }

    /// <summary> 중간에 input이 들어온경우 처리 </summary>
    public void SetInputPosition(Vector3? pos, float speed)
    {
        InputPosition = pos;
        InputPositionMoveSpeed = speed;
    }

    /// <summary> Mez 상태 체크 </summary>
    protected bool CheckMezState()
    {
        return false == Parent.IsMezState(CheckMezStateType);
    }

    /// <summary> Mez 상태시 알림 메시지 </summary>
    protected void ShowMezNotify(E_ConditionControl type)
    {
        if (mLastShowMezStateMessageTime + MEZ_STATE_MESSAGE_DELAY_TIME <= Time.time)
        {
            UICommon.SetNoticeMessage(type.ToString(), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            mLastShowMezStateMessageTime = Time.time;
        }
    }

    protected virtual void Init() { }    

    protected virtual void PostMove() { }
    protected virtual void PostUseSkill() { }

    protected abstract void SetSkill(params object[] args);
    protected abstract bool Check();

    protected abstract E_ConditionControl CheckMezStateType { get; }

    protected abstract float SkillSpeedRate { get; }
}