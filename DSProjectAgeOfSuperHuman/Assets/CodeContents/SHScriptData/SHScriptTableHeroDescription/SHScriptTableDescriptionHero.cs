using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class SHScriptTableDescriptionHero : CScriptDataTableBase
{
	public class SDescriptionHero
	{
		public uint		UnitID = 0;
		public string		UnitName;
		public string		UIFaceName;
		public string		IconName;
		public string		UnitPrefabName;
		public uint		UnitGrade;
		public uint		StoryID;
		public uint		SkillLeft;
		public uint		SkillRight;
		public uint		SkillReader;

		public uint SkillSlot1;
		public uint SkillSlot2;
		public uint SkillSlot3;
		public uint SkillRageCombo;

		public uint SkillPassive1;
		public uint SkillPassive2;
		public uint SkillPassive3;

		public List<string> PreLoadAsset = new List<string>();

		public uint Attack;
		public uint AttackSkill;
		public uint AttackPercent;
		public uint Defense;
		public uint DefenseSkill;
		public uint DefensePercent;
		public uint Critical;
		public uint CriticalAnti;
		public uint CriticalDamage;
		public uint CriticalDamageAnti;
		public uint Hit;
		public uint Dodge;
		public uint Block;
		public uint BlockAnti;
		public uint RecoverPerSecond;
		public uint Stamina;
		public uint StaminaPercent;
		public uint ExtraGold;
		public uint ExtraEXP;
		public uint ExtraItem;
	}

	private Dictionary<uint, SDescriptionHero> m_mapUnitHero = new Dictionary<uint, SDescriptionHero>();

	//--------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}

	//-----------------------------------------------------------------
	public SHStatGroupBasic ExtractHeroBasicStat(uint hHeroID)
	{
		SHStatGroupBasic pStatBasic = new SHStatGroupBasic();
		if (m_mapUnitHero.ContainsKey(hHeroID))
		{
			SDescriptionHero pTableHero = m_mapUnitHero[hHeroID];
			pStatBasic.Attack = pTableHero.Attack;
			pStatBasic.Defence = pTableHero.Defense;
			pStatBasic.Critical = pTableHero.Critical;
			pStatBasic.CriticalAnti = pTableHero.CriticalAnti;
			pStatBasic.CriticalDamage = pTableHero.CriticalDamage;
			pStatBasic.CriticalDamageAnti = pTableHero.CriticalDamageAnti;
			pStatBasic.Hit = pTableHero.Hit;
			pStatBasic.Dodge = pTableHero.Dodge;
			pStatBasic.RecoverPerSecond = pTableHero.RecoverPerSecond;
			pStatBasic.Stamina = pTableHero.Stamina;
		}
		return pStatBasic;
	}

	public void DoTableLoadHero(uint hHeroID, UnityAction<SHUnitHero, uint> delFinish)
	{
		if (m_mapUnitHero.ContainsKey(hHeroID) == false) delFinish?.Invoke(null, 0);

		SDescriptionHero pTableHero = m_mapUnitHero[hHeroID];
		PrivScriptTableLoadPreAsset(pTableHero, () => {
			SHManagerPrefabPool.Instance.LoadComponent(EPoolType.Unit, pTableHero.UnitPrefabName, (SHUnitHero pHero) => {
				delFinish?.Invoke(pHero, hHeroID);
			});
		});
	}

	public SDescriptionHero GetTableDescriptionHero(uint hHeroID)
	{
		SDescriptionHero pFindHero = null;
		if (m_mapUnitHero.ContainsKey(hHeroID))
		{
			pFindHero = m_mapUnitHero[hHeroID];
		}

		return pFindHero;
	}

	public List<SDescriptionHero> GetTableDescriptionHeroList()
	{
		return m_mapUnitHero.Values.ToList();
	}

	//---------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SDescriptionHero> pListTableLoad = ProtDataTableRead<SDescriptionHero>();
		for (int i = 0; i < pListTableLoad.Count; i++)
		{
			SDescriptionHero pTableEnemy = pListTableLoad[i];
			m_mapUnitHero[pTableEnemy.UnitID] = pTableEnemy;
		}
	}

	private void PrivScriptTableLoadPreAsset(SDescriptionHero pTableHero, UnityAction delFinish)
	{
		if (pTableHero.PreLoadAsset.Count == 0)
		{
			delFinish?.Invoke();
		}
		else
		{
			for (int i = 0; i < pTableHero.PreLoadAsset.Count; i++)
			{
				if (i == pTableHero.PreLoadAsset.Count - 1)
				{
					SHManagerPrefabPool.Instance.LoadInstance(EPoolType.Effect, pTableHero.PreLoadAsset[i], delFinish);
				}
				else
				{
					SHManagerPrefabPool.Instance.LoadInstance(EPoolType.Effect, pTableHero.PreLoadAsset[i], null);
				}
			}
		}
	}
}
