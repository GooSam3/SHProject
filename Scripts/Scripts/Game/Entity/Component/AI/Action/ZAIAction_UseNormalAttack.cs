using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
	public class ZAIAction_UseNormalAttack : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}

			myEntity.UseNormalAttack();
			EndAction(false);
		}
	}
}
