
using GameDB;
using UnityEngine;

/// <summary> entity Animation 관련 처리 </summary>
public class EntityComponentAnimationBase : EntityComponentBase<EntityBase>
{
    protected float mMoveSpeedRate = 1f;

    protected float mAttackSpeedRate = 1f;

    protected float mDefaultMoveSpeed = 5f;

    /// <summary> 최소 이동 애니메이션 속도 비율 </summary>
    protected const float mMinMoveSpeedRate = 0.5f;

    public float MoveSpeedRate { get; private set; }

    protected override void OnInitializeComponentImpl()
    {
        SetAnimationTableInfo();
    }

    public virtual void Move(bool bMove, bool bForce = false) { }

    public virtual void InitAnim(GameObject modelGo)
    {
        SetAnimationTableInfo();
    }

    /// <summary> 애니메이션 관련 테이블 정보 셋팅 </summary>
    private void SetAnimationTableInfo()
    {
        Resource_Table table = Owner.ResourceTable;

        if (null != table)
        {
            mDefaultMoveSpeed = table.RunSpeed;            
        }
        else if(Owner.EntityType == E_UnitType.Pet)
        {
            //펫 애니메이션 기본 속도 셋팅
            if(DBPet.TryGet(Owner.TableId, out var petTable))
            {
                mDefaultMoveSpeed = petTable.RunSpeed;
            }
        }
    }

    /// <summary> 이동 속도 비율을 변경한다. </summary>    
    public virtual void SetMoveSpeed(float value)
    {
        MoveSpeedRate = value;
        mMoveSpeedRate = value / mDefaultMoveSpeed;
    }

    /// <summary> 공격 속도 비율을 변경한다. </summary>
    public virtual void SetAttackSpeedRate(float value)
    {
        mAttackSpeedRate = value;
    }

    public virtual float GetAnimLength(E_AnimStateName animName)
    {
        return 0;
    }

    public virtual float GetAnimLength(string animName)
    {
        return 0;
    }

    /// <summary> 해당 애니메이션을 변경한다. </summary>
    public virtual void ChangeClip(E_AnimStateName code, AnimationClip clip)
    {
    }

    #region ========== Animator ==========
    public virtual void ResetAnim()
    {

    }

    public virtual void Die(bool bDie, float normalizeTime = 0f)
    {

    }

    /// <summary> 해당 animstate의 normailize time을 변경한다. </summary>
    public virtual void PlayByNormalizeTime(E_AnimStateName state, float normalizeTime)
    {

    }

    /// <summary> 경직 </summary>
    public virtual void Spasiticity( float time, float speed )
    {

    }

    public virtual void SetAnimParameter(E_AnimParameter parameter, bool value)
    {

    }

    public virtual void SetAnimParameter(E_AnimParameter parameter, float value)
    {

    }

    public virtual void SetAnimParameter(E_AnimParameter parameter, int value)
    {

    }

    public virtual void SetAnimParameter(E_AnimParameter parameter)
    {

    }

    public virtual void ChangeController(AnimatorOverrideController controller)
    {
        
    }

    public virtual bool GetBool(E_AnimParameter type)
    {
        return false;
    }

    public virtual int GetInteger(E_AnimParameter type)
    {
        return 0;
    }

    public virtual float GetFloat(E_AnimParameter type)
    {
        return 0f;
    }
    #endregion
}
