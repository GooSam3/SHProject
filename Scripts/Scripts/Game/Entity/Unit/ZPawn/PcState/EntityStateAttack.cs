using GameDB;

public class EntityStateAttack : EntityStateSkillBase
{
    /// <summary> 콤보 인덱스 </summary>
    private int mComboIndex = -1;
    
    protected override void Init()
    {        
        mComboIndex = -1;
    }
    protected override void SetSkill(params object[] args)
    {
        SetNextAttack();
    }
    
    protected override void PostMove()
    {
        //콤보 초기화 
        if(1 != mSkill.Combo)
            mComboIndex = -1;
    }

    protected override void PostUseSkill()
    {
        //다음 공격 준비
        SetNextAttack();
    }

    /// <summary> 다음 공격 준비 </summary>
    private void SetNextAttack()
    {
        mSkill = SkillSystem.GetNormalAttack(Parent.CharacterType, ++mComboIndex);
    }

    protected override bool Check()
    {
        if (0 < InvokeCount)
        {
            //기믹은 한번만 공격하자
            if (null != CurrentTarget && CurrentTarget.EntityType == E_UnitType.Gimmick)
			{
                Parent.ChangeState(E_EntityState.Empty);
                return false;
            }                
        }

        return true;
    }
    protected override E_ConditionControl CheckMezStateType => E_ConditionControl.NotAttack;

    protected override float SkillSpeedRate { get { return Parent.AttackSpeedRate; } }
}