using GameDB;
using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> 스킬을 사용한다. </summary>
	public class ZAIAction_UseSkill : ZAIActionBase
	{
		protected override void OnExecute()
		{
			var target = blackboard.GetVariable<EntityBase>(ZBlackbloardKey.Target);
			var skillTid = blackboard.GetVariable<uint>(ZBlackbloardKey.SkillTid);
			if (null == target || null == skillTid)
			{
				EndAction(false);
				return;
			}

			var skill = agent.SkillSystem.GetSkillInfoById(skillTid.value);

			float angleY = agent.Rotation.eulerAngles.y;

			float skillSpeedRate = skill.SkillTable.SkillType == E_SkillType.Normal ? agent.AttackSpeedRate : agent.SkillSpeedRate;

			//ZMmoManager.Instance.Field.REQ_Attack(agent.EntityId, agent.Position, angleY, skill.SkillId, target.value.EntityId, skill.Combo, skillSpeedRate);
			//패킷 안보내고 걍 연출
			agent.UseSkill(agent.Position, target.value.EntityId, skill.SkillId, skillSpeedRate, angleY, 0);
			agent.StopMove(agent.Position);
			//쿨타임 추가!! 
			//skill.SetCoolTime(skill.SkillTable.CoolTime);
			ZNet.Data.Me.CurCharData.SetSkillCoolTime(skillTid.value, (ulong)skill.SkillTable.CoolTime);
			
			EndAction(true);
		}
	}
}