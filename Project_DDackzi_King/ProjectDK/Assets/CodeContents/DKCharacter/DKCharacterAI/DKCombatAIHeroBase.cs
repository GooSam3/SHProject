using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DKCombatAIHeroBase : DKCombatAIBase
{
	[SerializeField]
	protected int TargetFirst = 7000;
	[SerializeField]
	protected int TargetSecond = 9000;
	[SerializeField]
	protected int TargetThird = 10000;

	[SerializeField]
	protected uint AllowAggro = 2;           // 대상의 어그로 2이상까지 공격한다.


	//-----------------------------------------------------------
	protected DKUnitBase ProtCombatAIHeroTarget(EDKStatType eStatType, bool bHigh)
	{
		DKUnitBase pTarget = null;

		List<DKFormationBase.SFomationMember> pFormationMember = new List<DKFormationBase.SFomationMember>();
		ProtCombatAIExtractFormationMember(pFormationMember, eStatType, bHigh);

		if (pFormationMember.Count == 0) return null;

		int iSearchCount = 0;
		int iFormationCount = pFormationMember.Count - 1;

		int iRandom = Random.Range(1, 10001);

		if (iRandom <= TargetFirst)
		{
			iSearchCount = 0;
		}
		else if (iRandom <= TargetSecond)
		{
			iSearchCount = 1;
		}
		else if (iRandom <= TargetThird)
		{
			iSearchCount = 2;
		}

		if (iSearchCount > iFormationCount)
		{
			iSearchCount = iFormationCount;
		}

		if (pFormationMember[iSearchCount].AggroCount >= AllowAggro && iFormationCount > 0) // 어그로 초과일경우 랜덤 타겟
		{
			for (int i = 0; i < iFormationCount; i++)
			{
				int iRandomCount = 0;
				while (true)
				{
					iRandomCount = Random.Range(0, iFormationCount + 1);
					if (iSearchCount != iRandomCount)
					{
						break;
					}
				}
				pTarget = pFormationMember[iRandomCount].pMember;
				if (pFormationMember[iSearchCount].AggroCount < AllowAggro)
				{
					break;
				}
			}
		}
		else
		{
			pTarget = pFormationMember[iSearchCount].pMember;
		}


		for (int i = 0; i < pFormationMember.Count; i++) // 어그로 기록 
		{
			if (pFormationMember[i].pMember == pTarget)
			{
				pFormationMember[i].AggroCount++;
			}
		}

		return pTarget;
	}

	protected DKUnitBase ProtCombatAIHeroTargetRandom()
	{
		DKUnitBase pTarget = null;
		List<DKFormationBase.SFomationMember> pListOutFormation = new List<DKFormationBase.SFomationMember>();
		DKManagerStageSpawner.Instance.ExtractFormationUnit(false, pListOutFormation);

		if (pListOutFormation.Count == 0) return null;

		int iSearchCount = Random.Range(0, pListOutFormation.Count - 1);
		pTarget = pListOutFormation[iSearchCount].pMember;
		pListOutFormation[iSearchCount].AggroCount++;
		return pTarget;
	}


}
 