using NodeCanvas.Framework;

namespace NodeCanvas.Tasks.Conditions
{
    public class ZAICondition_IsExistGatherObject : ZAIConditionBase
	{
		public bool IsQuest = false;
		protected override string info
		{
			get { return IsQuest ? "퀘스트 채집물이 있는지 확인" : "채집물이 있는지 확인"; }
		}

		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}

			uint searchTid = IsQuest ? blackboard.GetVariableValue<uint>(ZBlackbloardKey.TargetTid) : 0;

			ZObject gatherObject = ZPawnTargetHelper.ScanGatherObject(myEntity, searchTid);

			return gatherObject != null;
		}
	}
}
