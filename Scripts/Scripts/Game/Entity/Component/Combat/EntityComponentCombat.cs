using GameDB;
using UnityEngine;

/// <summary> 전투 관련 컴포넌트 </summary>
public class EntityComponentCombat : EntityComponentBase<ZPawn>
{
    /// <summary> 스킬 연출 </summary>
    private SkillAction mSkillAction = null;

    /// <summary> 스킬 연출중인지 여부 </summary>
    public bool IsSkillAction { get { return null != mSkillAction; } }

    /// <summary> Skill 사용 연출 </summary>
    public void UseSkill(Vector3 pos, uint targetEntityId, uint skillId, float attackSpeed, float dir, uint entTick)
    {
        //바라보기
        Owner.LookAt(dir);

        if (false == ZPawnManager.Instance.TryGetEntity(targetEntityId, out var targetEntity))
        {
            return;
        }

        targetEntity.SetAttackerEntityId(Owner.EntityId);

        UseSkill(pos, targetEntity, skillId, attackSpeed, entTick);
    }

    public void UseSkillForGimmick(EntityBase targetEntity, uint skillId, float attackSpeed, uint entTick)
    {
        Owner.LookAt(targetEntity.transform);

        UseSkill(Owner.Position, targetEntity, skillId, attackSpeed, entTick);
    }

    /// <summary> Skill 사용 연출 </summary>
    private void UseSkill(Vector3 pos, EntityBase targetEntity, uint skillId, float attackSpeed, uint entTick)
    {
        SkillCancel();

        mSkillAction = SkillAction.UseSkill(this, pos, targetEntity, skillId, attackSpeed, entTick);
    }

    /// <summary> 스킬 취소 처리 </summary>
    public void SkillCancel()
    {
        if(null != mSkillAction)
        {
            mSkillAction.Stop();
        }
    }

    public void SkillStop(SkillAction skillAction)
    {
        if (mSkillAction != skillAction)
            return;

        GameObject.Destroy(mSkillAction);

        mSkillAction = null;
    }

    /// <summary> 유적에서 강제이동 명령이 왔을 때 처리 </summary>
    public void ForceMove(Vector3 position, float duration, E_PosMoveType moveType)
    {
        if (null == mSkillAction)
            return;

        mSkillAction?.ForceMove(position, duration, moveType);
    }
}
