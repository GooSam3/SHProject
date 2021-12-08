using GameDB;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> 사용가능한 스킬을 랜덤하게 셋팅한다. </summary>
	public class ZAIAction_SetRandomSkill : ZAIActionBase
	{
		protected override void OnExecute()
		{
			var target = agent.GetTarget();
			if (null == target)
			{
				EndAction(false);
				return;
			}

			//사용가능한 스킬 얻기
			agent.SkillSystem.TryGetUseableSkillInfos(out var skills);

			//셔플!!
			skills.Shuffle();

			//스킬 셋팅하기
			foreach (var skill in skills)
			{
				EntityBase skillTarget = null;
				switch (skill.TargetType)
				{
					case E_TargetType.Enemmy:
						{
							skillTarget = target;
						}
						break;
					case E_TargetType.Self:
						{
							skillTarget = agent;
						}
						break;
					default:    //일단 다른건 구현할 필요없을듯.
						continue;
				}

				blackboard.AddVariable(ZBlackbloardKey.Target, skillTarget);
				blackboard.AddVariable(ZBlackbloardKey.SkillTid, skill.SkillId);

				EndAction(true);
				return;
			}

			EndAction(false);
		}
	}
}