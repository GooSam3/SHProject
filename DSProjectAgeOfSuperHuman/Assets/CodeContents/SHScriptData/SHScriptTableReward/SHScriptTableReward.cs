using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableReward : CScriptDataTableBase
{
	private class STableRewardPackage
	{
		public uint PackageID = 0;
		public uint ItemID1 = 0;
		public uint Probability1 = 0;
		public uint CountMin1 = 0;
		public uint CountMax1 = 0;

		public uint ItemID2 = 0;
		public uint Probability2 = 0;
		public uint CountMin2 = 0;
		public uint CountMax2 = 0;

		public uint ItemID3 = 0;
		public uint Probability3 = 0;
		public uint CountMin3 = 0;
		public uint CountMax3 = 0;

		public uint ItemID4 = 0;
		public uint Probability4 = 0;
		public uint CountMin4 = 0;
		public uint CountMax4 = 0;

		public uint ItemID5 = 0;
		public uint Probability5 = 0;
		public uint CountMin5 = 0;
		public uint CountMax5 = 0;
	}

	public class SRewardContents
	{
		public uint ItemID = 0;
		public uint Probability = 0;
		public uint CountMin = 0;
		public uint CountMax = 0;
	}

	public class SRewardPackage
	{
		public uint PackageID = 0;
		public List<SRewardContents> PackageContents = new List<SRewardContents>();
	}

	private Dictionary<uint, SRewardPackage> m_mapRewardPackage = new Dictionary<uint, SRewardPackage>();
	//-------------------------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptTableLoad();
	}
	//------------------------------------------------------------------------------------
	public SRewardPackage DoRewardPackage(uint hPackageID)
	{
		SRewardPackage pPackage = null;
		if (m_mapRewardPackage.ContainsKey(hPackageID))
		{
			pPackage = m_mapRewardPackage[hPackageID];
		}

		return pPackage;
	}

	//------------------------------------------------------------------------------------
	private void PrivScriptTableLoad()
	{
		List<STableRewardPackage> pListContents = ProtDataTableRead<STableRewardPackage>();
		for (int i = 0; i < pListContents.Count; i++)
		{
			STableRewardPackage pTablePackage = pListContents[i];
			SRewardPackage pRewardPackage = null;
			if (m_mapRewardPackage.ContainsKey(pTablePackage.PackageID))
			{
				pRewardPackage = m_mapRewardPackage[pTablePackage.PackageID];
			}
			else
			{
				pRewardPackage = new SRewardPackage();
				m_mapRewardPackage[pTablePackage.PackageID] = pRewardPackage;
			}

			pRewardPackage.PackageID = pTablePackage.PackageID;

			if (pTablePackage.ItemID1 != 0)
			{
				SRewardContents pRewardContns = new SRewardContents();
				pRewardContns.ItemID = pTablePackage.ItemID1;
				pRewardContns.Probability = pTablePackage.Probability1;
				pRewardContns.CountMin = pTablePackage.CountMin1;
				pRewardContns.CountMax = pTablePackage.CountMax1;
				pRewardPackage.PackageContents.Add(pRewardContns);
			}

			if (pTablePackage.ItemID2 != 0)
			{
				SRewardContents pRewardContns = new SRewardContents();
				pRewardContns.ItemID = pTablePackage.ItemID2;
				pRewardContns.Probability = pTablePackage.Probability2;
				pRewardContns.CountMin = pTablePackage.CountMin2;
				pRewardContns.CountMax = pTablePackage.CountMax2;
				pRewardPackage.PackageContents.Add(pRewardContns);
			}

			if (pTablePackage.ItemID3 != 0)
			{
				SRewardContents pRewardContns = new SRewardContents();
				pRewardContns.ItemID = pTablePackage.ItemID3;
				pRewardContns.Probability = pTablePackage.Probability3;
				pRewardContns.CountMin = pTablePackage.CountMin3;
				pRewardContns.CountMax = pTablePackage.CountMax3;
				pRewardPackage.PackageContents.Add(pRewardContns);
			}

			if (pTablePackage.ItemID4 != 0)
			{
				SRewardContents pRewardContns = new SRewardContents();
				pRewardContns.ItemID = pTablePackage.ItemID4;
				pRewardContns.Probability = pTablePackage.Probability4;
				pRewardContns.CountMin = pTablePackage.CountMin4;
				pRewardContns.CountMax = pTablePackage.CountMax4;
				pRewardPackage.PackageContents.Add(pRewardContns);
			}

			if (pTablePackage.ItemID5 != 0)
			{
				SRewardContents pRewardContns = new SRewardContents();
				pRewardContns.ItemID = pTablePackage.ItemID5;
				pRewardContns.Probability = pTablePackage.Probability5;
				pRewardContns.CountMin = pTablePackage.CountMin5;
				pRewardContns.CountMax = pTablePackage.CountMax5;
				pRewardPackage.PackageContents.Add(pRewardContns);
			}
		}
	}
}
