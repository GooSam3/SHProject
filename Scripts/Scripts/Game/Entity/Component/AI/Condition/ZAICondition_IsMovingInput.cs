using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsMovingInput : ZAIConditionBase
	{
		protected override string info
		{
			get { return "클릭 이동 중인지 확인"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsInputMove();
		}
	}
}
