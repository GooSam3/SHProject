using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntityComponentTestUseSkill))]
public class EntityComponentTestUseSkillInspector : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EntityComponentTestUseSkill comp = (EntityComponentTestUseSkill)target;
		ZPawnMyPc pc = comp.Owner.To<ZPawnMyPc>();
		SkillSystem skillSystem = pc.SkillSystem;
		IList<SkillInfo> skills = skillSystem.GetSkills();

		if (null != skillSystem && comp.IsShowSkillList) 
		{
			comp.SearchText = EditorGUILayout.TextField("검색키워드 : ", comp.SearchText);

			foreach (SkillInfo skill in skills) {
				var buttonName = $"[{skill.SkillTable.SkillType}] Skill ({skill.SkillId}) - {DBLocale.GetText(skill.SkillTable.SkillTextID)}";

				if (string.IsNullOrEmpty(comp.SearchText) == false &&
					buttonName.ToLowerInvariant().Contains(comp.SearchText.ToLowerInvariant()) == false) {
					continue;
				}

				if (GUILayout.Button(buttonName)) {
					pc.UseSkillBySkillId(skill.SkillId);
				}
			}
		}
	}
}