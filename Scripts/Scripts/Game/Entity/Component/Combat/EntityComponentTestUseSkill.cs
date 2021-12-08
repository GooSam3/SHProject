using UnityEngine;
/// <summary> 전투 관련 컴포넌트 </summary>
public class EntityComponentTestUseSkill : EntityComponentBase<EntityBase>
{
	public bool IsShowSkillList = false;
	public string SearchText { get; set; }
}
