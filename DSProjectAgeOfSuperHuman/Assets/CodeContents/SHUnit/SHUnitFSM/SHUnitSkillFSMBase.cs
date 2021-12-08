using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public abstract class SHUnitSkillFSMBase : CFiniteStateMachineSkillBase
{
	public class SSkillExportInfo
	{
		public uint SkillID = 0;
		public string CoolTimeName;
		public string SkillName;
		public ESkillType SkillType;
	}

	[System.Serializable]
	protected class SUnitSkillInfo
	{
		public uint SkillID = 0;
		public float StartCoolTime = 0;
	}

	[System.Serializable]
	protected class SUnitSkillList
	{
		public List<SUnitSkillInfo> SkillSlot = new List<SUnitSkillInfo>();
		public List<SUnitSkillInfo> SkillPassive = new List<SUnitSkillInfo>();
		public List<SUnitSkillInfo> SkillAdditional = new List<SUnitSkillInfo>();
	}

	[SerializeField]
	private SUnitSkillList SkillList = new SUnitSkillList();

	protected ISHSkillProcessor m_pSHSkillOwner = null;
	protected List<SSkillExportInfo> m_listSkillExport = new List<SSkillExportInfo>();
	//--------------------------------------------------------------------
	protected override void OnFSMSkillInitialize(ISkillProcessor pSkillOnwer)
	{
		base.OnFSMSkillInitialize(pSkillOnwer);
		m_pSHSkillOwner = pSkillOnwer as ISHSkillProcessor;
		PrivUnitSkillLoad();
	}

	//---------------------------------------------------------------------	
	public int DoUnitSkillActive(int iSlotIndex, SHUnitBase pTarget)
	{
		if (iSlotIndex >= SkillList.SkillSlot.Count)
		{
			return (int)ESkillConditionResult.Invalid;
		}

		SHSkillUsage pUsage = new SHSkillUsage();
		pUsage.UsageTarget = pTarget;
		pUsage.UsageSkillID = SkillList.SkillSlot[iSlotIndex].SkillID;
		return ProtFSMSkillUseTry(pUsage);
	}

	public int DoUnitSkillActive(SHSkillUsage pUsage)
	{
		return ProtFSMSkillUseTry(pUsage);
	}


	public void DoUnitSkillResetCoolTime()
	{
		for (int i = 0; i < SkillList.SkillSlot.Count; i++)
		{
			if (SkillList.SkillSlot[i].StartCoolTime != 0)
			{
				string strCoolTime = ExtractUnitSkillCoolTimeName(SkillList.SkillSlot[i].SkillID);
				m_pSHSkillOwner.ISetSkillCoolTime(strCoolTime, SkillList.SkillSlot[i].StartCoolTime);
			}
		}
	}

	public void DoUnitSkillReset()
	{
		ProtStatClearAll();
	}
	
	public List<SSkillExportInfo> ExportSkillInfo() { return m_listSkillExport; }
	//----------------------------------------------------------------------
	private void PrivUnitSkillLoad()
	{		
		for (int i = 0; i < SkillList.SkillSlot.Count; i++)
		{
			SHSkillDataActive pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(SkillList.SkillSlot[i].SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SKillSlot, pSkillInstance);
		}

		for (int i = 0; i < SkillList.SkillAdditional.Count; i++)
		{
			SHSkillDataActive pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillActive(SkillList.SkillAdditional[i].SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillAdditional, pSkillInstance);
		}

		for (int i = 0; i < SkillList.SkillPassive.Count; i++)
		{
			SHSkillDataPassive pSkillInstance = SHManagerScriptData.Instance.DoLoadSkillPassive(SkillList.SkillPassive[i].SkillID);
			ProtFSMSkillDataLoad(pSkillInstance);
			ProtUnitSkillExportInfo(ESkillType.SkillPassive, pSkillInstance);
		}
	}

	private string ExtractUnitSkillCoolTimeName(uint hSkillID)
	{
		string strCoolTimeName ="";
		CSkillDataActive pSkillDataActive = FindSkillData(hSkillID) as CSkillDataActive;
		
		if (pSkillDataActive != null)
		{
			for (int i = 0; i < pSkillDataActive.listCondition.Count; i++)
			{
				SHSkillConditionCoolTime pCoolTime = pSkillDataActive.listCondition[i] as SHSkillConditionCoolTime;
				if (pCoolTime != null)
				{
					strCoolTimeName = pCoolTime.CoolTimeName;
				}
			}
		}

		return strCoolTimeName;
	}

	//---------------------------------------------------------------------
	protected void ProtUnitSkillExportInfo(ESkillType eSkillType, CSkillDataBase pSkillData)
	{
		SSkillExportInfo pExportInfo = new SSkillExportInfo();
		pExportInfo.SkillID = pSkillData.hSkillID;
		pExportInfo.SkillName = pSkillData.SkillName;
		pExportInfo.SkillType = eSkillType;
		pExportInfo.CoolTimeName = ExtractUnitSkillCoolTimeName(pSkillData.hSkillID);
		m_listSkillExport.Add(pExportInfo);
	}
	//----------------------------------------------------------------------
}
