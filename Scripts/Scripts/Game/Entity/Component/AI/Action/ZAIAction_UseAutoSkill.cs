using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using GameDB;

namespace NodeCanvas.Tasks.Actions
{
	public class ZAIAction_UseAutoSkill : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}

			var skillTid = blackboard.GetVariable<uint>(ZBlackbloardKey.SkillTid);

			Skill_Table skillTable = DBSkill.Get(skillTid.value);
			if ((myEntity.CurrentMp - skillTable.UseMPCount) / myEntity.MaxMp < ZGameOption.Instance.RemainMPPer)
			{
				EndAction(false);
				return;
			}

			myEntity.UseSkillBySkillId(skillTid.value, true);
			EndAction(true);
		}
	}
}
