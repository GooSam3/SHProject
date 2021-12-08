using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsAvailableAutoSkill : ZAIConditionBase
	{
		protected override string info
		{
			get { return "현재 사용가능 한 스킬이 있는지 확인"; }
		}
		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}

			SkillInfo usableSkill = myEntity.GetAutoSkillController().GetAvailableQuickSlotAutoSkill();

			if (usableSkill != null)
			{
				blackboard.AddVariable(ZBlackbloardKey.SkillTid, usableSkill.SkillId);
			}

			return usableSkill != null;
		}
	}
}