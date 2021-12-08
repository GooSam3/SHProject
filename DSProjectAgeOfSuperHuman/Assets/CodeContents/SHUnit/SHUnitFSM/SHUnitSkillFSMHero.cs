using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUnitSkillFSMHero : SHUnitSkillFSMBase
{
	[SerializeField]
	private SUnitSkillInfo NormalLeft = new SUnitSkillInfo();
	[SerializeField]
	private SUnitSkillInfo NormalRight = new SUnitSkillInfo();
	[SerializeField]
	private SUnitSkillInfo ReaderSkill = new SUnitSkillInfo();
	[SerializeField]
	private SUnitSkillInfo ComboSkill = new SUnitSkillInfo();

	//----------------------------------------------------------------------
	protected override void OnFSMSkillInitialize(ISkillProcessor pSkillOnwer)
	{
		base.OnFSMSkillInitialize(pSkillOnwer);
		PrivUnitSkillHeroLoadNormalSkill();
	}

	//-----------------------------------------------------------------------
	public int DoUnitSkillNormal(bool bLeft, SHUnitBase pTarget)
	{
		SHSkillUsage pUsage = new SHSkillUsage();
		if (bLeft)
		{
			pUsage.UsageSkillID = NormalLeft.SkillID;
		}
		else
		{
			pUsage.UsageSkillID = NormalRight.SkillID;
		}

		pUsage.UsageTarget = pTarget;
		return ProtFSMSkillUseTry(pUsage);
	}

	public void SetUnitHeroSkill(uint hSkillLeft, uint hSkillRight, uint hSkillReader, List<uint> pListSkillActive, List<uint> pListSkillPassive)
	{

	}

	public int DoUnitHeroSkillCombo(SHUnitBase pTarget)
	{
		SHSkillUsage pUsage = new SHSkillUsage();
		pUsage.UsageTarget = pTarget;
		pUsage.UsageSkillID = ComboSkill.SkillID;
		return ProtFSMSkillUseTry(pUsage);
	}

	//----------------------------------------------------------------------
	private void PrivUnitSkillHeroLoadNormalSkill()
	{
		SHSkillDataActive pSkillInstance = null;
		if (NormalLeft.SkillID != 0)
		{
			pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(NormalLeft.SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillNormalLeft, pSkillInstance);
		}

		if (NormalRight.SkillID != 0)
		{
			pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(NormalRight.SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillNormalRight, pSkillInstance);
		}

		if (ReaderSkill.SkillID != 0)
		{
			SHSkillDataPassive pSkillPassive = SHManagerScriptData.Instance.DoLoadSkillPassive(ReaderSkill.SkillID);
			ProtFSMSkillDataLoad(pSkillPassive);
			ProtUnitSkillExportInfo(ESkillType.SkillReader, pSkillInstance);
		}

		if (ComboSkill.SkillID != 0)
		{
			pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(ComboSkill.SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillCombo, pSkillInstance);
		}

	}
}
