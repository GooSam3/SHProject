using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKBuffProcessor : CBuffProcessorBase
{
	protected override CUnitBuffBase.SDamageResult OnBuffProcessDamageCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		DKStatComponent pDKStatMe = pStatMe as DKStatComponent;
		DKStatComponent pDKStatDest = pStatDest as DKStatComponent;
		uint iChance = (uint)(pDKStatMe.GetDKUnitStat(EDKStatType.CriticalRate) - pDKStatMe.GetDKUnitStat(EDKStatType.AntiCriticalRate));
		float fPower = 0;
		if (Random.Range(1, 10000) <= iChance)
		{
			fPower = pDKStatMe.GetDKUnitStat(EDKStatType.AttackPower) * rResult.fDamageRate;
			float fMultiplier = pDKStatMe.GetDKUnitStat(EDKStatType.MultiplierRate) - pDKStatDest.GetDKUnitStat(EDKStatType.AntiMultiplierRate);
			if (fMultiplier < 1) fMultiplier = 1;

			fPower *= fMultiplier;
			rResult.eDamageResultType = (int)EDamageResultType.Critial;
		}
		else
		{
			fPower = pDKStatMe.GetDKUnitStat(EDKStatType.AttackPower);
			rResult.eDamageResultType = (int)EDamageResultType.None;
		}

		rResult.fDamage = fPower - (fPower * pDKStatDest.GetDKUnitStat(EDKStatType.DefenceRate));
		return rResult; 
	}
	protected override CUnitBuffBase.SDamageResult OnBuffProcessHealCalculation(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		rResult = OnBuffProcessDamageCalculation(pStatMe, pStatDest, rResult);		
		return rResult;

	}
	protected override CUnitBuffBase.SDamageResult OnBuffProcessDamageApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{
		DKStatComponent pDKStatMe = pStatMe as DKStatComponent;
		DKStatComponent pDKStatDest = pStatDest as DKStatComponent;
		pDKStatMe.DoStatApplyDamage(rResult);
		return rResult;
	}
	protected override CUnitBuffBase.SDamageResult OnBuffProcessHealApply(CStatComponentBase pStatMe, CStatComponentBase pStatDest, CUnitBuffBase.SDamageResult rResult)
	{	
		return rResult;
	}
}
