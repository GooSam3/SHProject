

namespace NodeCanvas.Tasks.Actions
{
	/// <summary> 내 pc를 타겟팅한다. </summary>
	public class ZAIAction_SetTargetMyPc : ZAIActionBase
	{
		protected override void OnExecute()
		{
			if (null == ZPawnManager.Instance.MyEntity)
			{
				EndAction(false);
				return;
			}

			agent.SetTarget(ZPawnManager.Instance.MyEntity);
			EndAction(true);
		}
	}
}