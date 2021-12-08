using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableLevel : CScriptDataTableBase
{
	[System.Serializable]
	public class STableLevel : CObjectInstanceBase
	{
		[SerializeField]
		public uint LevelCap = 0;
		[SerializeField]
		public uint EXPIncrease = 0;
		[SerializeField]
		public EOpenContentsType EOpenContentsType = EOpenContentsType.None;
		[SerializeField]
		public uint RewardID = 0;
	}

	private List<STableLevel> m_listHeroLevel = null;
	//----------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);

		m_listHeroLevel = ProtDataTableRead<STableLevel>();
	}

	//------------------------------------------------------------------
	public uint GetTableLevelEXP(int iLevel)
	{
		uint iExpResult = 0;
		uint iPrevSection = 0;
		for (int i = 0; i < m_listHeroLevel.Count; i++)
		{
			STableLevel pTableLevel = m_listHeroLevel[i];

			if (iLevel <= pTableLevel.LevelCap)
			{
				uint iLevelSection = (uint)iLevel - iPrevSection;
				iExpResult += (iLevelSection * pTableLevel.EXPIncrease);
				break;
			}
			else
			{
				uint iLevelSection = pTableLevel.LevelCap - iPrevSection;
				iPrevSection = pTableLevel.LevelCap;
				iExpResult += (iLevelSection * pTableLevel.EXPIncrease);
			}
		}

		return iExpResult;
	}

	public List<STableLevel> GetTableLevelCopyList()
	{
		List<STableLevel> pListCopy = new List<STableLevel>();

		for (int i = 0; i < m_listHeroLevel.Count; i++)
		{
			pListCopy.Add(m_listHeroLevel[i].CopyInstance<STableLevel>());
		}

		return pListCopy;
	}


}
