using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskReduceCoolTime : SHSkillTaskBase
{
	private string m_strCoolTimeName;
	private float m_fCoolTime;

	//--------------------------------------------------------------------
	public void SetTaskReduceCoolTime(string strCoolTimeName, float fCoolTime)
	{
		m_strCoolTimeName = strCoolTimeName;
		m_fCoolTime = fCoolTime;
	}
	//--------------------------------------------------------------------
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillOwner.IGetUnit().IsAlive == false) return;
		pSkillOwner.ISetSkillCoolTimeReduce(m_strCoolTimeName, m_fCoolTime);
	}
}

public class SHSkillTaskReduceCoolTimeRandom : SHSkillTaskBase
{
	private int	 m_iCoolTimeMin = 0;
	private int	 m_iCoolTimeMax = 0;

	public void SetTaskReduceCoolTimeRandom(int iCoolTimeMin, int iCoolTimeMax)
	{
		m_iCoolTimeMin = iCoolTimeMin;
		m_iCoolTimeMax = iCoolTimeMax;
	}

	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillUsage.UsageTarget.IGetUnit().IsAlive == false) return;
	
		int iCoolTimeValue = Random.Range(m_iCoolTimeMin, m_iCoolTimeMax + 1);

		SHUnitBase pUnit = pSkillOwner.IGetUnit() as SHUnitBase;
		List<SHUnitSkillFSMBase.SSkillExportInfo> pListCoolTimeEnable = new List<SHUnitSkillFSMBase.SSkillExportInfo>();
		List<SHUnitSkillFSMBase.SSkillExportInfo> pListSkillInfo = pUnit.ExportUnitSkillInfo();
		for (int i = 0; i < pListSkillInfo.Count; i++)
		{
			if (pListSkillInfo[i].SkillType == ESkillType.SKillSlot)
			{
				float fValue = pUnit.IGetSkillCoolTime(pListSkillInfo[i].CoolTimeName);
				if (fValue > 0)
				{
					pListCoolTimeEnable.Add(pListSkillInfo[i]);
				}
			}
		}

		if (pListCoolTimeEnable.Count > 0)
		{
			int iChoice = Random.Range(0, pListCoolTimeEnable.Count);
			pUnit.ISetSkillCoolTimeReduce(pListCoolTimeEnable[iChoice].CoolTimeName, iCoolTimeValue);

			if(pUnit.GetUnitRelationForPlayer() == CUnitBase.EUnitRelationType.Hero)
			{
				UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatHeroFocusSkillSlot(pListCoolTimeEnable[iChoice].SkillID);
			}
		}
	}

}