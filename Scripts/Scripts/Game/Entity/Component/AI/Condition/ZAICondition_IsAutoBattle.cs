namespace NodeCanvas.Tasks.Conditions
{
    public class ZAICondition_IsAutoBattle : ZAIConditionBase
	{
		protected override string info
		{
			get { return agent && (agent is ZPawnMyPc myEntity) ? $"IsAutoPlay : {myEntity.IsAutoPlay}" : "오토 사냥중인지 체크"; }
		}
		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}

			return myEntity.IsAutoPlay;
		}
	}
}
