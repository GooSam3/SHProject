using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsMovingDir : ZAIConditionBase
	{
		protected override string info
		{
			get { return "조이스틱 이동중인지 여부"; }
		}

		protected override bool OnCheck()
		{
			return agent.IsMovingDir();
		}
	}
}
