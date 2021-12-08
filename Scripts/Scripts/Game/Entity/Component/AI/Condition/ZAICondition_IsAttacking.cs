using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsAttacking : ZAIConditionBase
	{
		protected override string info
		{
			get { return agent && (agent is ZPawnMyPc myEntity) ? $"Is Attacking : {myEntity.IsAttacking}" : "내가 공격중인지 체크"; }
		}

		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}

			return myEntity.IsAttacking;
		}
	}
}
