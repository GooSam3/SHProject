using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions
{
    public class ZAICondition_IsQuestStage : ZAIConditionBase
	{
		protected override string info
		{
			get { return "현재 퀘스트의 스테이지 인지 확인"; }
		}

		protected override bool OnCheck()
		{			
			return ZGameModeManager.Instance.StageTid == blackboard.GetVariableValue<uint>(ZBlackbloardKey.StageTid);
		}
	}
}
