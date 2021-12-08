using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHScriptTableDescriptionEnemy : CScriptDataTableBase
{
	public class SDescriptionEnemy : object
	{
		public uint UnitID = 0;
		public string UnitName;
		public string UIPrefabFaceName;
		public string IconName;
		public List<string> PreLoadAsset = new List<string>();
		public string UnitPrefabName;
		public uint UnitStatID;
		public uint UnitLevel;
		public uint SkillIDNormal;
		public uint SkillIDSlot1;
		public uint SkillIDSlot2;
		public uint SkillIDSlot3;
		public uint SkillIDSlot4;
		public uint SkillIDPassive1;
		public uint SkillIDPassive2;
		public uint SkillIDPassive3;
	}

	private Dictionary<uint, SDescriptionEnemy> m_mapUnitEnemy = new Dictionary<uint, SDescriptionEnemy>();

	//--------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}

	public uint GetDescriptionEnemyRewardID(uint hEnemyID)
	{
		return 0;
	}

	public void DoTableLoadEnemy(uint hEnemyID, int iLevel, uint iHPGaugeScale, UnityAction<SHUnitEnemy, uint> delFinish)
	{
		if (m_mapUnitEnemy.ContainsKey(hEnemyID) == false)
		{
			Debug.LogWarning("[EnemyTable] Invalid EnemyID : " + hEnemyID + m_mapUnitEnemy.Count);
			return;
		}

		SDescriptionEnemy pTableEnemy = m_mapUnitEnemy[hEnemyID];

		PrivScriptTableEnemyPreLoadAsset(pTableEnemy, () => {
			SHManagerPrefabPool.Instance.LoadComponent(EPoolType.Unit, pTableEnemy.UnitPrefabName, (SHUnitEnemy pEnemy) =>
			{
				pEnemy.DoUnitIniailize();
				pEnemy.SetUnitHPGaugeScale(iHPGaugeScale);
				pEnemy.SetUnitInfo(hEnemyID, pTableEnemy.UnitName, iLevel);
				delFinish?.Invoke(pEnemy, hEnemyID);
			});
		});
	}

	//---------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<SDescriptionEnemy> pListTableLoad = ProtDataTableRead<SDescriptionEnemy>();
		for (int i = 0; i < pListTableLoad.Count; i++)
		{
			SDescriptionEnemy pTableEnemy = pListTableLoad[i];
			m_mapUnitEnemy[pTableEnemy.UnitID] = pTableEnemy;
		}
	}

	private void PrivScriptTableEnemyPreLoadAsset(SDescriptionEnemy pTableEnemy, UnityAction delFinish)
	{
		if (pTableEnemy.PreLoadAsset.Count == 0)
		{
			delFinish?.Invoke();
		}
		else
		{
			for (int i = 0; i < pTableEnemy.PreLoadAsset.Count; i++)
			{
				if (i == pTableEnemy.PreLoadAsset.Count - 1)
				{
					SHManagerPrefabPool.Instance.LoadGameObject(EPoolType.Effect, pTableEnemy.PreLoadAsset[i], (GameObject pLoadObject)=>
					{
						delFinish?.Invoke();
					});
				}
				else
				{
					SHManagerPrefabPool.Instance.LoadGameObject(EPoolType.Effect, pTableEnemy.PreLoadAsset[i], null);
				}
			}
		}
	}
}
