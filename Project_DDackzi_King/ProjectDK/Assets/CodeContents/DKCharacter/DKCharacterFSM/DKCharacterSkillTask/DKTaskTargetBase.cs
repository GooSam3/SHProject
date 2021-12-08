using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
public abstract class DKTaskTargetBase : CTaskTargetBase
{
   
	//--------------------------------------------------------------
}

public class DKTaskTargetRange : DKTaskTargetBase
{
	private bool m_bNearbyMe = false;
	private float m_fRange = 0;
	private int m_iTargetCount = 0;

	//-----------------------------------------------------------
	public void SetTaskTargetRange(bool bNearbyMe, float fRange, int iTargetCount) 
	{
		m_bNearbyMe = bNearbyMe;
		m_fRange = fRange;
		m_iTargetCount = iTargetCount;
	}
}

public class DKTaskTargetHP : DKTaskTargetBase
{
	private ERelationType m_eRelationType = ERelationType.None;
	private bool m_bHPLess = false;
	private int m_iTargetCount = 0;
	//---------------------------------------------------------------
	public void SetTaskTargetHP(ERelationType eRelationType, bool bHPLess, int iTargetCount)
	{
		m_eRelationType = eRelationType;
		m_bHPLess = bHPLess;
		m_iTargetCount = iTargetCount;
	}
}

public class DKTaskTargetRandom : DKTaskTargetBase
{
	private ERelationType m_eRelationType = ERelationType.None;
	private int m_iTargetCount = 0;
	//---------------------------------------------------------------
	public void SetTaskTargetRandom(ERelationType eRelation, int iTargetCount)
	{
		m_eRelationType = eRelation;
		m_iTargetCount = iTargetCount;
	}

	protected override void OnTaskTarget(CSkillUsage pUsage, ISkillProcessor pSkillProcessor, List<CUnitBase> pListTarget)
	{
		CUnitBase.EUnitControlType controllType = pSkillProcessor.IGetUnitControlType();
		List<DKFormationBase.SFomationMember> pListOutFormation = new List<DKFormationBase.SFomationMember>();

		if (m_eRelationType == ERelationType.Relation_Me)
		{
			pListTarget.Add(pSkillProcessor.IGetUnit());
		}
		else if (m_eRelationType == ERelationType.Relation_Target)
		{
			pListTarget.Add(pUsage.UsageTarget.IGetUnit());
		}
		else if (m_eRelationType == ERelationType.Relation_FriendAll)
		{
			if (controllType == CUnitBase.EUnitControlType.PlayerAI)
			{
				DKManagerStageSpawner.Instance.ExtractFormationUnit(true, pListOutFormation);
			}
			else if (controllType == CUnitBase.EUnitControlType.EnemyAI)
			{
				DKManagerStageSpawner.Instance.ExtractFormationUnit(false, pListOutFormation);
			}			

			for (int i = 0; i < pListOutFormation.Count; i++)
			{
				pListTarget.Add(pListOutFormation[i].pMember);
			}
		}
		else if (m_eRelationType == ERelationType.Relation_EnemyAll)
		{
			if (controllType == CUnitBase.EUnitControlType.PlayerAI)
			{
				DKManagerStageSpawner.Instance.ExtractFormationUnit(false, pListOutFormation);
			}
			else if (controllType == CUnitBase.EUnitControlType.EnemyAI)
			{
				DKManagerStageSpawner.Instance.ExtractFormationUnit(true, pListOutFormation);
			}

			for (int i = 0; i < pListOutFormation.Count; i++)
			{
				pListTarget.Add(pListOutFormation[i].pMember);
			}
		}
	}

}

public class DKTaskTargetDefault : DKTaskTargetBase
{
	private ERelationType m_eRelationType = ERelationType.None;
	//-----------------------------------------------------------------
	public void SetTaskTargetDefault(ERelationType eRelation)
	{
		m_eRelationType = eRelation;
	}

	protected override void OnTaskTarget(CSkillUsage pUsage, ISkillProcessor pSkillProcessor, List<CUnitBase> pListTarget) 
	{
		if (m_eRelationType == ERelationType.Relation_Me)
		{
			pListTarget.Add(pSkillProcessor.IGetUnit());
		}
		else 
		{
			pListTarget.Add(pUsage.UsageTarget.IGetUnit());
		}
	}
}
