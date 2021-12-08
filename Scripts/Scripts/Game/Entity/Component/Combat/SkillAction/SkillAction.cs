using GameDB;
using System.Collections;
using UnityEngine;

/// <summary> 스킬 연출 </summary>
public class SkillAction : MonoBehaviour
{
    private EntityComponentCombat mCombatComp = null;
    private ZPawn mOwner = null;
    private EntityBase mTarget = null;
    private Skill_Table mTable = null;
    private float mAttackSpeed = 0f;
    private Vector3 mStartPosition = Vector3.zero;
         
    private ulong mCastingEndTick = 0;
    private float mSkillInvokeTime = 0f;
    private float mSkillLength = 0f;

    private uint mCastingEffectTid = 0;
    private uint mLoopEffectTid = 0;
    private uint mActionEffectTid = 0;

    private uint mCastingSoundTid = 0;
    private uint mLoopSoundTid = 0;
    private uint mActionSoundTid = 0;

    private E_AnimParameter mSkillAnimParameterType = E_AnimParameter.Attack_001;

    private void Init(EntityComponentCombat combatComp, Vector3 pos, EntityBase targetEntity, Skill_Table skillTable, float attackSpeed, uint entTick)
    {
        mCombatComp = combatComp;
        mOwner = mCombatComp.Owner;
        mTarget = targetEntity;
        mTable = skillTable;
        mAttackSpeed = attackSpeed;
        mCastingEndTick = entTick;
        mStartPosition = pos;

        //스킬 사용중 히트 관련 처리
        mOwner.IsSkillAction = true;

        if (mOwner.IsMyPc)
        {
            mOwner.IsSkipHitMotion = true;
        }
        else
        {
            //TODO :: 추후 세분화 필요            
            mOwner.IsSkipHitMotion = UnityEngine.Random.value > 0.5f;
        }

        SetSkillInfo(mTable.SkillAniType);
        StartCoroutine(Co_Action());
    }

    private IEnumerator Co_Action()
    {
        //일단 무조건 멈춘다
        //if (mTable.PosMoveType == E_PosMoveType.None)
        {
            mOwner.StopMove(mStartPosition);
        }

        if(0 < mCastingEndTick)
            yield return Co_Casting();

        switch(mTable.PosMoveType)
        {
            case E_PosMoveType.CasterBackJump:                
            case E_PosMoveType.CasterLeap:                
            case E_PosMoveType.CasterRandomMove:
            case E_PosMoveType.CasterRush:
                {
                    yield return Co_Move(E_PosMoveType.CasterRush != mTable.PosMoveType);
                }
                break;
        }

        yield return Co_Skill();

        Stop();
    }

    /// <summary> 캐스팅 </summary>
    private IEnumerator Co_Casting()
    {
        float duration = 0f;

        if(mCastingEndTick > ZMmoManager.Instance.GameTick)
        {
            duration = ((mCastingEndTick - ZMmoManager.Instance.GameTick) * TimeHelper.Unit_MsToSec);
        }
        
        SpawnEffect(mCastingEffectTid);

        SpawnEffect(mLoopEffectTid, duration);

        mOwner.SetAnimParameter(E_AnimParameter.SkillCasting_001, true);
        mOwner.SetAnimParameter(E_AnimParameter.SkillCast_Start_001);
        yield return new WaitForSeconds(duration);
    }

    /// <summary> 이동 타입일 경우 이동 연출 </summary>
    private IEnumerator Co_Move(bool fixedDuration)
    {
        float distance = mTable.PosMoveDistance;
        float speed = mTable.PosMoveSpeed;

        if (0 >= distance || 0 >= speed)
            yield break;

        float duration = distance / speed;

        if(false == fixedDuration)
        {
            if(null != mTarget)
            {
                duration = (mTarget.Position - mOwner.Position).magnitude / speed;
            }
        }

        switch(mTable.SkillAniType)
        {
            case E_SkillAniType.Leap:
                {
                    mOwner.SetAnimParameter(E_AnimParameter.SkillLeap_Start_001);
                    mOwner.SetAnimParameter(E_AnimParameter.SkillLeap_001, true);
                }
                break;
            case E_SkillAniType.Rush:
                {
                    mOwner.SetAnimParameter(E_AnimParameter.SkillRush_Start_001);
                    mOwner.SetAnimParameter(E_AnimParameter.SkillRush_001, true);
                }
                break;
        }

        SpawnEffect(mCastingEffectTid);
        SpawnEffect(mLoopEffectTid, duration);

        yield return new WaitForSeconds(duration);

        ResetAnim();
    }

    /// <summary> 스킬 액션 연출 시작 </summary>
    private IEnumerator Co_Skill()
    {
        //탑승중이면 내리고 다음프레임에 애니가 들어가야 실행된다. IsRiding은 이미 false임
        if (mOwner.IsMyPc && mOwner.GetBool(E_AnimParameter.Riding_001)) {
			yield return null;
		}

		float invokeTime = mSkillInvokeTime / mAttackSpeed;

        mOwner.SetAttackSpeedRate(mAttackSpeed);

        //애니메이션 출력
        mOwner.SetAnimParameter(mSkillAnimParameterType);

        //이펙트 출력
        SpawnEffect(mActionEffectTid, 0f, mAttackSpeed);
        SpawnEffect(mTable.SkillEffectID, 0f, mAttackSpeed);

        yield return new WaitForSeconds(invokeTime);

        SkillInvoke();

        float skillLength = mSkillLength / mAttackSpeed;

        yield return new WaitForSeconds(skillLength - invokeTime);
    }

    /// <summary> 스킬 발동 </summary>
    private void SkillInvoke()
    {
        if (false == DBSkill.TryGet(mTable.SkillID, out Skill_Table skillTable))
            return;

        //범위 이펙트 출력
        if (skillTable.RangeEffectID > 0)
            mOwner.SpawnEffect(skillTable.RangeEffectID, 0f, mAttackSpeed);

        if (null != mTarget)
        {
            //발사체 처리DO 
            if (E_MissileType.Not != skillTable.MissileType)
            {
                ProjectileLauncher.Fire(mOwner, mTarget, mTable.SkillID, mOwner.transform.forward);
            }
            else
            {
                if (mTarget.EntityType == E_UnitType.Gimmick)
                {
                    ZGimmick gimmcik = mTarget.To<ZGimmick>();
                    gimmcik?.TakeAttribute(mOwner.UnitAttributeType, mOwner.AttributeLevel);
                }
                else if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Temple)
                {
                    //사당에선 따로 데미지요청 보냄.
                    //TODO :: 범위 스킬 따로해야하나?
                    //ZMmoManager.Instance.Field.REQ_DamageReq(mOwner.EntityId, mTarget.EntityId, mTable.SkillID);
                    ZTempleHelper.TempleEntityAttack(mOwner, mTarget, mTable.SkillID);
                }
            }
        }
    }

    /// <summary> 스킬 종료 </summary>
    public void Stop()
    {
        StopAllCoroutines();

        mOwner.IsSkillAction = false;
        mOwner.IsSkipHitMotion = false;

        ResetAnim();
        mCombatComp.SkillStop(this);
    }

    /// <summary> 유적에서 강제이동 명령이 왔을 때 처리 </summary>
    public void ForceMove(Vector3 position, float duration, E_PosMoveType moveType)
    {
        if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Temple)
            return;

        switch(moveType)
        {
            case E_PosMoveType.TargetMoveCaster:
                break;
            case E_PosMoveType.TargetKnockBack:
                break;
            case E_PosMoveType.CasterBackJump:
                break;
            case E_PosMoveType.CasterLeap:
                break;
            case E_PosMoveType.CasterRandomMove:
                break;
            case E_PosMoveType.CasterRush:
                break;
            default:
                {
                    mOwner.Warp(position);
                }
                break;
        }
    }

    /// <summary> 이펙트 스폰 </summary>
    private void SpawnEffect(uint effectTid, float duration = 0f, float speed = 1f)
    {
        if (0 >= effectTid)
            return;

        mOwner.SpawnEffect(effectTid, duration, speed, (effect) => 
        {            
        });
    }

    /// <summary> 애니메이션 리셋 </summary>
    private void ResetAnim()
    {
        mOwner.SetAnimParameter(E_AnimParameter.SkillCasting_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.SkillRush_001, false);
        mOwner.SetAnimParameter(E_AnimParameter.SkillLeap_001, false);
    }

    #region ===== :: 스킬 연출 관련 데이터 셋팅 :: =====
    /// <summary> 스킬 연출에 필요한 정보 셋팅 </summary>
    private void SetSkillInfo(E_SkillAniType aniType)
    {
        if (null == mOwner.ResourceTable)
            return;

        string animName = string.Empty;

        Resource_Table table = mOwner.ResourceTable;

        switch (aniType)
        {
            case E_SkillAniType.Attack_01:
                {
                    mSkillAnimParameterType = E_AnimParameter.Attack_001;

                    animName = table.AttackAni_01;
                    mActionEffectTid = table.AttackEffectID_01;
                    mActionSoundTid = table.AttackSoundID;
                    
                    //캐릭터의 기본 애니매이션 길이는 1초다. (40frame 으로 만들어진다. 복귀 동작때문에)
                    if (mOwner.IsMyPc)
                        mSkillLength = 1f;
                }
                break;
            case E_SkillAniType.Attack_02:
                {
                    mSkillAnimParameterType = E_AnimParameter.Attack_002;

                    animName = table.AttackAni_02;
                    mActionEffectTid = table.AttackEffectID_02;
                    mActionSoundTid = table.AttackSoundID;
                    
                    //캐릭터의 기본 애니매이션 길이는 1초다. (40frame 으로 만들어진다. 복귀 동작때문에)
                    if (mOwner.IsMyPc)
                        mSkillLength = 1f;
                }
                break;
            case E_SkillAniType.Attack_03:
                {
                    mSkillAnimParameterType = E_AnimParameter.Attack_003;

                    animName = table.AttackAni_03;
                    mActionEffectTid = table.AttackEffectID_03;
                    mActionSoundTid = table.AttackSoundID;

                    //캐릭터의 기본 애니매이션 길이는 1초다. (40frame 으로 만들어진다. 복귀 동작때문에)
                    if (mOwner.IsMyPc)
                        mSkillLength = 1f;
                }
                break;
            case E_SkillAniType.Skill_01:
                {
                    mSkillAnimParameterType = E_AnimParameter.Skill_001;

                    animName = table.SkillAni_01;                    
                    mActionEffectTid = table.SkillEffectID_01;
                    mActionSoundTid = table.SkillSoundID;
                }
                break;
            case E_SkillAniType.Skill_02:
                {
                    mSkillAnimParameterType = E_AnimParameter.Skill_002;

                    animName = table.SkillAni_02;
                    mActionEffectTid = table.SkillEffectID_02;
                    mActionSoundTid = table.SkillSoundID;
                }
                break;
            case E_SkillAniType.Buff:
                {
                    mSkillAnimParameterType = E_AnimParameter.Buff_001;

                    animName = table.BuffAni_01;                    
                    mActionEffectTid = table.BuffEffectID_01;
                    mActionSoundTid = table.BuffSoundID;
                }
                break;
            case E_SkillAniType.Casting:
                {
                    mSkillAnimParameterType = E_AnimParameter.SkillCast_End_001;

                    animName = table.CastingAni;
                    mCastingEffectTid = table.Casting_S_EffectID;
                    mLoopEffectTid = table.Casting_I_EffectID;
                    mActionEffectTid = table.Casting_A_EffectID;

                    mCastingSoundTid = table.Casting_S_SoundID;
                    mLoopSoundTid = table.Casting_I_SoundID;
                    mActionSoundTid = table.Casting_A_SoundID;
                }
                break;
            case E_SkillAniType.Leap:
                {
                    mSkillAnimParameterType = E_AnimParameter.SkillLeap_End_001;

                    animName = table.LeapAni;
                    
                    mCastingEffectTid = table.Leap_S_EffectID;
                    mLoopEffectTid = table.Leap_I_EffectID;
                    mActionEffectTid = table.Leap_A_EffectID;

                    mCastingSoundTid = table.Leap_S_SoundID;
                    mLoopSoundTid = table.Leap_I_SoundID;
                    mActionSoundTid = table.Leap_A_SoundID;
                }
                break;
            case E_SkillAniType.Pull:
                {
                    mSkillAnimParameterType = E_AnimParameter.SkillPull_001;

                    animName = table.PullAni;

                    mActionEffectTid = table.PullEffectID;

                    mActionSoundTid = table.PullSoundID;
                }
                break;
            case E_SkillAniType.Rush:
                {
                    mSkillAnimParameterType = E_AnimParameter.SkillRush_End_001;

                    animName = table.RushAni;

                    mCastingEffectTid = table.Rush_S_EffectID;
                    mLoopEffectTid = table.Rush_I_EffectID;
                    mActionEffectTid = table.Rush_A_EffectID;

                    mCastingSoundTid = table.Rush_S_SoundID;
                    mLoopSoundTid = table.Rush_I_SoundID;
                    mActionSoundTid = table.Rush_A_SoundID;
                }
                break;
        }

        //애니메이션 길이 및 발동 타이밍 셋팅
        if(DBAnimation.TryGet(animName, out var aniTable))
        {
            mSkillInvokeTime = aniTable.InvokeTiming_01;

            if(0f >= mSkillLength)
            {
                mSkillLength = aniTable.AnimationLength;
            }
        }
    }
    #endregion

    /// <summary> 스킬 액션 생성 </summary>
    public static SkillAction UseSkill(EntityComponentCombat combatComp, Vector3 pos, EntityBase targetEntity, uint skillId, float attackSpeed, uint entTick)
    {
        if (false == DBSkill.TryGet(skillId, out Skill_Table skillTable))
            return null;

        SkillAction action = combatComp.gameObject.AddComponent<SkillAction>();
        action.Init(combatComp, pos, targetEntity, skillTable, attackSpeed, entTick);
        return action;
    }
}
