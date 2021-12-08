using GameDB;

public class EntityStateSkill : EntityStateSkillBase
{
    protected override void Init()
    {
    }

    protected override void SetSkill(params object[] args)
    {
        if(args.Length <= 0)
        {
            Parent.ChangeState(E_EntityState.Empty);
            return;
        }

        uint skillId = (uint)args[0];

        SkillSystem.TryGetSkillInfo(skillId, out mSkill);
    }

    protected override void PostMove()
    {
    }

    protected override void PostUseSkill()
    {
    }

    /// <summary> 사용전 체크 </summary>
    protected override bool Check()
    {
        var target = Parent.GetTarget();

        //스킬은 한번만 발동된다.
        if (0 < InvokeCount)
        {
            //이 전 스테이트가 Attack이거나 buff skill이 아니라면 공격!            
            if(null != target)
            {
                if (Parent.PreviousState == E_EntityState.Attack)
                {
                    Parent.ChangeState(E_EntityState.Attack);
                }
                else if (mSkill.SkillTable.SkillType == E_SkillType.BuffSkill)
                {
                    Parent.ChangeState(E_EntityState.Empty);
                }
                else
                {
                    Parent.ChangeState(E_EntityState.Attack);
                }
            }
            else
            {
                Parent.ChangeState(E_EntityState.Empty);
            }

            return false;
        }   
        else if(null != target && target.EntityType == E_UnitType.Gimmick)
		{
            //기믹은 스킬로 공격하지 말자
            Parent.ChangeState(E_EntityState.Empty);
            return false;
		}

        return true;
    }

    protected override E_ConditionControl CheckMezStateType => E_ConditionControl.NotSkill;

    protected override float SkillSpeedRate { get { return Parent.SkillSpeedRate; } }
}