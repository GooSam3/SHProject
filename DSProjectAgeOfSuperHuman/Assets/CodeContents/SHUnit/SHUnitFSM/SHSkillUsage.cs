using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public interface ISHSkillProcessor : ISkillProcessor
{
	public void		ISHSkillRageGain(float fRagePoint);
	public void		ISHSkillAttackTo(SHUnitBase pTarget, NSkill.EDamageType eDamageType, EUnitSocket eUnitSocket, string strHitEffectName, float fPower, List<SHStatModifier> pListBonusStat = null);
	public void		ISHAnimSkinChange(string strAniGroupName, string strSkinName);
	public void		ISHSetTagCoolTime(float fCoolTime);
	public float		ISHGetTagCoolTime();
}

public enum ESkillConditionResult
{
	None,
	CoolTimeGlobal,
	CoolTimeSkill,
	CrowdControll,

	Invalid,
}

public enum ESkillType
{
	None,
	SkillNormal,
	SkillNormalLeft,
	SkillNormalRight,
	SKillSlot,
	SkillReader,
	SkillPassive,
	SkillAdditional,
	SkillCombo,
}

public class SHSkillUsage : CSkillUsage 
{
	public static string g_GlobalCoolTime = "GlobalCoolTime";
}
