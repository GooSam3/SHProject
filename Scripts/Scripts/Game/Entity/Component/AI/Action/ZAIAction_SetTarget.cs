using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	public class ZAIAction_SetTarget : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}

			var target = myEntity.GetTarget();
						
			if (target == null)
			{
				target = ZPawnTargetHelper.SearchAutoBattleTarget(myEntity, myEntity.Position, DBConfig.Auto_Search_Range);
				myEntity.SetTarget(target);
			}

			EndAction(true);
		}
	}
}
