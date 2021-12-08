using GameDB;

namespace NodeCanvas.Tasks.Conditions
{
	/// <summary> 타겟이 있을 경우 타겟이 시야 범위안에 있는지 체크 </summary>
	public class ZAICondition_IsInSearchRange : ZAIConditionBase
	{
		protected override string OnInit()
		{
			return base.OnInit();
		}

		protected override string info
		{
			get { return agent && agent.GetTarget() ? $"범위 안에 있음 : {OnCheck()}" : "타겟이 있을 경우 시야내 존재 여부 체크"; }
		}

		protected override bool OnCheck()
		{
			var target = agent.GetTarget();

			if (null == target)
				return false;

			float range = DBConfig.SearchTargetRange;
			switch (agent.EntityType)
			{
				case E_UnitType.Monster:
					{
						range = agent.To<ZPawnMonster>().SearchRange;

						if (0 >= range)
							range = DBConfig.SearchTargetRange * 0.5f;
					}
					break;
			}

			var distance = (agent.Position - target.Position).magnitude;

			return distance <= range;
		}
	}
}