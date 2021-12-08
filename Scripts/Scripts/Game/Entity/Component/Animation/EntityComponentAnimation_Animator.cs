using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> entity Animation (Animator)관련 처리 </summary>
public class EntityComponentAnimation_Animator : EntityComponentAnimationBase
{
    /// <summary> Normal Update 등록 </summary>
    protected override bool EnableUpdate => true;

    public Animator Anim { get; private set; }

    public AnimatorOverrideController Controller { get; private set; }

    public int LastTriggerId = 0;

    /// <summary> 해당 파라미터 값이 변경되었다. </summary>
    public Action<E_AnimParameter> OnChangeAnimParameter;

    public Action<bool> OnChangeMove;

    /// <summary> 사망 연출 코루틴 </summary>
    private Coroutine mPlayByNormalizeCoroutine = null;

    private Coroutine spasiticityCoroutine = null;

    #region Dirty
    /// <summary> 이동속도 변경 관련 dirty </summary>
    private ZDirty MoveSpeedDirty = new ZDirty(0.35f);
	#endregion

	/// <summary>Animator에 실제 존재하는 Params</summary>
	public Dictionary<int, AnimatorControllerParameter> DicAnimParams { get; } = new Dictionary<int, AnimatorControllerParameter>();

    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();
    }

    protected override void OnDestroyImpl()
    {
        Anim = null;
    }

    protected override void OnUpdateImpl()
    {
        if (null == Anim)
            return;

        //이동 속도 변경
        if (MoveSpeedDirty.Update())
        {
            SetAnimParameter(E_AnimParameter.MoveSpeedRate, MoveSpeedDirty.CurrentValue);
        }
    }

    /// <summary> Animator 및 AnimatorOverrideController를 셋팅한다. </summary>
    public override void InitAnim(GameObject modelGo)
    {
        base.InitAnim(modelGo);
        if (null == Owner)
        {
            ZLog.Log(ZLogChannel.Entity, ZLogLevel.Error, "ZAnimationController::SetAnimator - Owner가 셋팅되지 않았다.");
            return;
        }

        if(null != modelGo)
        {
            Anim = modelGo.GetComponentInChildren<Animator>();
        }
        else
        {
            Anim = Owner.GetComponentInChildren<Animator>();
        }

        if (null == Anim)
        {
            return;
        }

        Controller = Anim.runtimeAnimatorController as AnimatorOverrideController;// new AnimatorOverrideController(Anim.runtimeAnimatorController);

        Anim.enabled = true;

        ResetAnimParameterValues();
        //Anim.runtimeAnimatorController = Controller;        

        return;
    }

    public override void ResetAnim()
    {
        ResetAnimParameterValues();
    }

    private void ResetAnimParameterValues()
    {
		DicAnimParams.Clear();

        if (null == Anim || null == Anim.runtimeAnimatorController)
            return;

		foreach (var param in Anim.parameters)
		{
			DicAnimParams.Add(param.nameHash, param);
		}

        if(0 < LastTriggerId)
        {
            Anim.ResetTrigger(LastTriggerId);
        }

		EntityAnimatorParameter.ResetDefaultParametersValue(this);
    }

    /// <summary> 이동 여부. </summary>    
    public override void Move(bool bMove, bool bForce = false)
    {
        float moveSpeed = bMove ? mMoveSpeedRate : 0f;

        if (false == bForce && moveSpeed == MoveSpeedDirty.GoalValue)
            return;

        var moveSpeedRate = Mathf.Max(mMinMoveSpeedRate, mMoveSpeedRate);

        MoveSpeedDirty.GoalValue = bMove ? moveSpeedRate : 0f;
        MoveSpeedDirty.IsDirty = true;

        OnChangeMove?.Invoke(bMove);
    }

    /// <summary> 이동 속도 비율을 변경한다. </summary>    
    public override void SetMoveSpeed(float value)
    {
        var moveSpeedRate = value / mDefaultMoveSpeed;

        if (mMoveSpeedRate == moveSpeedRate)
            return;

        mMoveSpeedRate = moveSpeedRate;

        //이동속도가 변경됐을 때 애니메이션 속도도 변경한다.
        MoveSpeedDirty.GoalValue = 0 < MoveSpeedDirty.GoalValue ? mMoveSpeedRate : 0f;
        MoveSpeedDirty.IsDirty = true;
    }
        
    /// <summary> Animator을 교체한다. </summary>
    public override void ChangeController(AnimatorOverrideController controller)
    {
        Anim.runtimeAnimatorController = controller;
        Controller = Anim.runtimeAnimatorController as AnimatorOverrideController;// new AnimatorOverrideController(Anim.runtimeAnimatorController);
        Anim.enabled = true;

        ResetAnim();
    }


    /// <summary> 해당 애니메이션을 변경한다. </summary>
    public override void ChangeClip(E_AnimStateName code, AnimationClip clip)
    {
        if (null == Controller)
            return;

        Controller[code.ToString()] = clip;
    }

    /// <summary> 애니메이션 길이를 얻어온다. </summary>
    public override float GetAnimLength(E_AnimStateName animName)
    {
        return Controller?[animName.ToString()]?.length ?? 1f;
    }

    /// <summary> 애니메이션 길이를 얻어온다. </summary>
    public override float GetAnimLength(string animName)
    {        
        return Controller?[animName]?.length ?? 1f;
    }

    /// <summary> 애니메이터의 현재 스테이트 speed를 얻어온다. </summary>
    public float GetCurAnimatorStateSpeed()
    {
        var stateInfo = Anim.GetCurrentAnimatorStateInfo(0);        
        return stateInfo.speed * stateInfo.speedMultiplier;
    }

    /// <summary> 사망 연출 처리 </summary>
    public override void Die(bool bDie, float normalizeTime = 0f)
    {
        StartCoroutine(Co_Die(bDie, normalizeTime));
    }

    private IEnumerator Co_Die(bool bDie, float normalizeTime = 0f)
    {
        //탈것탑승중에는 다음 프레임에 애니를 실행해야 한다
        if (Owner.IsMyPc && (Owner as ZPawn).IsRiding) {
            yield return null;
        }

        SetAnimParameter(E_AnimParameter.Die_001, bDie);
        if (0f < normalizeTime && true == bDie) {
            PlayByNormalizeTime(E_AnimStateName.Die_001, normalizeTime);
        }
    }

    public override void PlayByNormalizeTime(E_AnimStateName state, float normalizeTime)
    {
        if (null != mPlayByNormalizeCoroutine)
        {
            StopCoroutine(mPlayByNormalizeCoroutine);
        }

        mPlayByNormalizeCoroutine = StartCoroutine(Co_PlayByNormalize(state, normalizeTime));
    }

    private IEnumerator Co_PlayByNormalize(E_AnimStateName state, float normalizeTime)
    {
        string stateName = $"{state}";        

        while (true)
        {
            var stateInfo = Anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(stateName))
            {
                Anim.Play(stateName, 0, normalizeTime);
                break;
            }

            yield return null;
        }

        mPlayByNormalizeCoroutine = null;
    }

    public override void Spasiticity( float time, float speed )
    {
        if( null != spasiticityCoroutine ) {
            CoroutineManager.Instance.StopAction( spasiticityCoroutine );
        }

        // 이동중 경직으로 애니가 멈추면 어색한 미끄러짐 현상이 나올수 있기에 패스
        if( Anim != null ) {
            var stateInfo = Anim.GetCurrentAnimatorStateInfo( 0 );
            if( stateInfo.IsName( $"{E_AnimStateName.Run_001}" ) ||
                stateInfo.IsName( $"{E_AnimStateName.Walk_001}" ) ||
                stateInfo.IsName( $"{E_AnimStateName.Jump_001}" ) ||
                stateInfo.IsName( $"{E_AnimStateName.Jump_Start_001}" ) ) {
                return;
            }
            Anim.speed = speed;
        }

        spasiticityCoroutine = CoroutineManager.Instance.StartTimer( time, () => {
            if( Anim != null ) {
                Anim.speed = 1;
            }
            spasiticityCoroutine = null;
        } );
    }

    /// <summary> 공격 속도를 변경한다. </summary>
    public override void SetAttackSpeedRate(float value)
    {
        base.SetAttackSpeedRate(value);

        SetAnimParameter(E_AnimParameter.AttackSpeedRate, value);
    }


    public override void SetAnimParameter(E_AnimParameter parameter, bool value)
    {
        EntityAnimatorParameter.SetParameter(this, parameter, value);

        if (parameter != E_AnimParameter.MoveSpeedRate)
            OnChangeAnimParameter?.Invoke(parameter);
    }

    public override void SetAnimParameter(E_AnimParameter parameter, float value)
    {
        EntityAnimatorParameter.SetParameter(this, parameter, value);

        if (parameter != E_AnimParameter.MoveSpeedRate)
            OnChangeAnimParameter?.Invoke(parameter);
    }

    public override void SetAnimParameter(E_AnimParameter parameter, int value)
    {
        EntityAnimatorParameter.SetParameter(this, parameter, value);

        if (parameter != E_AnimParameter.MoveSpeedRate)
            OnChangeAnimParameter?.Invoke(parameter);
    }

    public override void SetAnimParameter(E_AnimParameter parameter)
    {
        EntityAnimatorParameter.SetParameter(this, parameter);

        if (parameter != E_AnimParameter.MoveSpeedRate)
            OnChangeAnimParameter?.Invoke(parameter);
    }

    public override bool GetBool(E_AnimParameter type)
    {
        return EntityAnimatorParameter.GetBool(Anim, type);
    }

    public override int GetInteger(E_AnimParameter type)
    {
        return EntityAnimatorParameter.GetInteger(Anim, type);
    }

    public override float GetFloat(E_AnimParameter type)
    {
        return EntityAnimatorParameter.GetFloat(Anim, type);
    }
}
