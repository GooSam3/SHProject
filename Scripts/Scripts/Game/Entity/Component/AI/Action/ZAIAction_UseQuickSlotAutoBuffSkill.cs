using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	public class ZAIAction_UseQuickSlotAutoBuffSkill : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}

			SkillInfo usableBuffSkill = myEntity.GetAutoSkillController().GetAvailableQuickSlotAutoBuffSkill();

			if (usableBuffSkill == null)
			{
				EndAction(false);
				return;
			}
			else
			{
				myEntity.UseSkillBySkillId(usableBuffSkill.SkillId, true);
				EndAction(true);
			}
		}
	}
}