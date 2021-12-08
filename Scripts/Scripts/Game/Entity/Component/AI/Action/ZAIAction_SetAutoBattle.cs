using System;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{	
    /// <summary> 자동 전투를 킨다. </summary>
    public class ZAIAction_SetAutoBattle : ZAIActionBase
	{
		public bool IsAutoPlay = false;

		protected override void OnExecute()
		{
			if((agent is ZPawnMyPc pc))
            {
				pc.IsAutoPlay = IsAutoPlay;

				EndAction(true);
				return;
			}

			EndAction(false);
		}
	}
}