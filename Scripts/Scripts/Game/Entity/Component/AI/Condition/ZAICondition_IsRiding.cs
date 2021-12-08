using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsRiding : ZAIConditionBase
	{
		protected override string info
		{
			get { return "탈것 탑승중인지 확인"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsRiding;
		}
	}
}
