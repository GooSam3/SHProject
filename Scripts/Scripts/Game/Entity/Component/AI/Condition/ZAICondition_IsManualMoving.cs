using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsManualMoving : ZAIConditionBase
	{
		protected override string info
		{
			get { return "수동 이동중인지 확인"; }
		}

		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}

			return myEntity.IsMovingDir() || myEntity.IsInputMove();
		}
	}
}
