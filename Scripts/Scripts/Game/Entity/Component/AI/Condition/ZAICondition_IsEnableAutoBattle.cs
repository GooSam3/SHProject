
namespace NodeCanvas.Tasks.Conditions
{
	public class ZAICondition_IsEnableAutoBattle : ZAIConditionBase
	{
		protected override string info
		{
			get { return "오토사냥 가능 여부"; }
		}
		protected override bool OnCheck()
		{
			if (!(agent is ZPawnMyPc myEntity))
				return false;

			if (myEntity.IsMoving())
			{
				return false;
			}

			if (myEntity.IsSkillAction)
			{
				return false;
			}

			if (!myEntity.IsAutoPlay && !myEntity.isSecondTargetAttack())
			{
				return false;
			}

			return true;
		}
	}

}