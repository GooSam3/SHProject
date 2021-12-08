using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	public class ZAIAction_UseQuickSlotAutoSkill : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}

			SkillInfo usableSKill = myEntity.GetAutoSkillController().GetAvailableQuickSlotAutoSkill();

			if (usableSKill == null)
			{
				EndAction(false);
				return;
			}
			else
			{
				myEntity.UseSkillBySkillId(usableSKill.SkillId, true);
				EndAction(true);
			}
		}
	}
}