using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
public class SHUIWindowHeroInfomation : SHUIWindowHeroBase
{
	[System.Serializable]
	public class SStatView
	{
		[SerializeField]
		public ESHStatType StatType = ESHStatType.Attack;
		[SerializeField]
		public CUIWidgetNumberTextChart StatWidget = null;
	}

	[SerializeField]
	private List<SStatView> StatList = null;
	//-----------------------------------------------------------
	protected override void OnUIWindowHeroRefresh(uint hHeroID)
	{
		SPacketStatValue pStatValue = SHManagerGameDB.Instance.GetGameDBHeroStatCache(hHeroID);
		if (pStatValue != null)
		{
			SetMonoActive(true);
			PrivHeroInfoRefresh(hHeroID, pStatValue);
		}
		else
		{
			SetMonoActive(false);
			PrivHeroInfoPurcharse(hHeroID);
		}
	}

	//----------------------------------------------------------
	private void PrivHeroInfoRefresh(uint hHeroID, SPacketStatValue pStatValue)
	{	 	
		PrivHeroInfoStatValue(ESHStatType.Attack,			pStatValue.LevelAttack);
		PrivHeroInfoStatValue(ESHStatType.AttackSkill,		pStatValue.LevelAttackSkill);
		PrivHeroInfoStatValue(ESHStatType.AttackPercent,		pStatValue.LevelAttackPercent);
		PrivHeroInfoStatValue(ESHStatType.Defense,			pStatValue.LevelDefense);
		PrivHeroInfoStatValue(ESHStatType.DefenseSkill,		pStatValue.LevelDefenseSkill);
		PrivHeroInfoStatValue(ESHStatType.DefencePercent,		pStatValue.LevelDefensePercent);
		PrivHeroInfoStatValue(ESHStatType.Stamina,			pStatValue.LevelStamina);
		PrivHeroInfoStatValue(ESHStatType.StaminaPercent,		pStatValue.LevelStaminaPercent);
		PrivHeroInfoStatValue(ESHStatType.Critical,			pStatValue.LevelCritical);
		PrivHeroInfoStatValue(ESHStatType.CriticalAnti,		pStatValue.LevelCriticalAnti);
		PrivHeroInfoStatValue(ESHStatType.CriticalDamage,		pStatValue.LevelCriticalDamage);
		PrivHeroInfoStatValue(ESHStatType.CriticalDamageAnti, pStatValue.LevelCriticalDamageAnti);
		PrivHeroInfoStatValue(ESHStatType.Hit,				pStatValue.LevelHit);
		PrivHeroInfoStatValue(ESHStatType.Dodge,				pStatValue.LevelDodge);
		PrivHeroInfoStatValue(ESHStatType.Block,				pStatValue.LevelBlock);
		PrivHeroInfoStatValue(ESHStatType.BlockAnti,			pStatValue.LevelBlockAnti);
		PrivHeroInfoStatValue(ESHStatType.RecoverPerSecond,	pStatValue.LevelRecoverPerSecond);
		PrivHeroInfoStatValue(ESHStatType.ExtraGold,			pStatValue.LevelExtraGold);
		PrivHeroInfoStatValue(ESHStatType.ExtraEXP,			pStatValue.LevelExtraEXP);
		PrivHeroInfoStatValue(ESHStatType.ExtraItem,			pStatValue.LevelExtraItem);
	} 

	private void PrivHeroInfoPurcharse(uint hHeroID)
	{		
		SHScriptTableDescriptionHero.SDescriptionHero pHeroTable = SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHero(hHeroID);
		PrivHeroInfoStatValue(ESHStatType.Attack, pHeroTable.Attack);
		PrivHeroInfoStatValue(ESHStatType.AttackPercent, pHeroTable.AttackPercent);
		PrivHeroInfoStatValue(ESHStatType.AttackSkill, pHeroTable.AttackSkill);
		PrivHeroInfoStatValue(ESHStatType.Defense, pHeroTable.Defense);
		PrivHeroInfoStatValue(ESHStatType.DefenseSkill, 0);
		PrivHeroInfoStatValue(ESHStatType.Stamina, pHeroTable.Stamina);
		PrivHeroInfoStatValue(ESHStatType.Critical, pHeroTable.Critical);
		PrivHeroInfoStatValue(ESHStatType.CriticalAnti, pHeroTable.CriticalAnti);
		PrivHeroInfoStatValue(ESHStatType.CriticalDamage, pHeroTable.CriticalDamage);
		PrivHeroInfoStatValue(ESHStatType.CriticalDamageAnti, pHeroTable.CriticalDamageAnti);
		PrivHeroInfoStatValue(ESHStatType.Hit, pHeroTable.Hit);
		PrivHeroInfoStatValue(ESHStatType.Dodge, pHeroTable.Dodge);
		PrivHeroInfoStatValue(ESHStatType.Block, pHeroTable.Block);
		PrivHeroInfoStatValue(ESHStatType.BlockAnti, pHeroTable.BlockAnti);
		PrivHeroInfoStatValue(ESHStatType.RecoverPerSecond, pHeroTable.RecoverPerSecond);
		PrivHeroInfoStatValue(ESHStatType.ExtraGold, 0);
		PrivHeroInfoStatValue(ESHStatType.ExtraEXP, 0);
		PrivHeroInfoStatValue(ESHStatType.ExtraItem, 0);
	}

	private void PrivHeroInfoStatValue(ESHStatType eStatType, uint iValue)
	{
		for (int i = 0; i < StatList.Count; i++)
		{
			if (StatList[i].StatType == eStatType)
			{
				StatList[i].StatWidget.DoTextNumber(iValue);
				break;
			}
		}
	}
}
