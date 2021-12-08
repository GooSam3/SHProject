namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsMmoConnected : ZAIConditionBase
	{
		protected override string info
		{
			get { return ZMmoManager.hasInstance ? $"Connected : {ZMmoManager.Instance.IsConnected}" : "Mmo Server 연결 상태 체크"; }
		}

		protected override bool OnCheck()
		{
			return ZMmoManager.Instance.IsConnected;
		}
	}
}