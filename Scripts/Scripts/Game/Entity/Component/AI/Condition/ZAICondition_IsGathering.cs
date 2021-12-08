namespace NodeCanvas.Tasks.Conditions
{
    public class ZAICondition_IsGathering : ZAIConditionBase
	{
		protected override string info
		{
			get { return "채집 중인지 확인"; }
		}

		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
			{
				return false;
			}
			
			return myEntity.IsGathering || myEntity.CurrentState == E_EntityState.Gathering;
		}
	}
}
