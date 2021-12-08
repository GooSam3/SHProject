using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableRewardGroup : CScriptDataTableBase
{
	public class STableRewardGroup
	{
		public uint PackageGroupID = 0;
		public uint PackageID1 = 0;
		public uint Probability1 = 0;
		public uint Grade1 = 0;

		public uint PackageID2 = 0;
		public uint Probability2 = 0;
		public uint Grade2 = 0;

		public uint PackageID3 = 0;
		public uint Probability3 = 0;
		public uint Grade3 = 0;

		public uint PackageID4 = 0;
		public uint Probability4 = 0;
		public uint Grade4 = 0;

		public uint PackageID5 = 0;
		public uint Probability5 = 0;
		public uint Grade5 = 0;
	}

	public class SRewardGroupContents
	{
		public uint PackageID = 0;
		public uint Probability = 0;
		public uint Grade = 0;
	}

	public class SRewardGroup
	{
		public uint PackageGroupID = 0;
		public List<SRewardGroupContents> PackageContents = new List<SRewardGroupContents>();
	}

	private Dictionary<uint, SRewardGroup> m_mapRewardGroup = new Dictionary<uint, SRewardGroup>();
	//------------------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivTableRewardGroupLoad();
	}

	//-----------------------------------------------------------------------------
	private void PrivTableRewardGroupLoad()
	{
		List<STableRewardGroup> pListTable = ProtDataTableRead<STableRewardGroup>();
		for (int i = 0; i < pListTable.Count; i++)
		{
			STableRewardGroup pTableReward = pListTable[i];
			SRewardGroup pRewardGroup = null;
			if (m_mapRewardGroup.ContainsKey(pTableReward.PackageGroupID))
			{
				pRewardGroup = m_mapRewardGroup[pTableReward.PackageGroupID];
			}
			else
			{
				pRewardGroup = new SRewardGroup();
				m_mapRewardGroup[pTableReward.PackageGroupID] = pRewardGroup;
			}

			if (pTableReward.PackageID1 != 0)
			{
				SRewardGroupContents pRewardContents = new SRewardGroupContents();
				pRewardContents.PackageID = pTableReward.PackageID1;
				pRewardContents.Probability = pTableReward.Probability1;
				pRewardContents.Grade = pTableReward.Grade1;
				pRewardGroup.PackageContents.Add(pRewardContents);
			}

			if (pTableReward.PackageID2 != 0)
			{
				SRewardGroupContents pRewardContents = new SRewardGroupContents();
				pRewardContents.PackageID = pTableReward.PackageID2;
				pRewardContents.Probability = pTableReward.Probability2;
				pRewardContents.Grade = pTableReward.Grade2;
				pRewardGroup.PackageContents.Add(pRewardContents);
			}

			if (pTableReward.PackageID3 != 0)
			{
				SRewardGroupContents pRewardContents = new SRewardGroupContents();
				pRewardContents.PackageID = pTableReward.PackageID3;
				pRewardContents.Probability = pTableReward.Probability3;
				pRewardContents.Grade = pTableReward.Grade3;
				pRewardGroup.PackageContents.Add(pRewardContents);
			}

			if (pTableReward.PackageID4 != 0)
			{
				SRewardGroupContents pRewardContents = new SRewardGroupContents();
				pRewardContents.PackageID = pTableReward.PackageID4;
				pRewardContents.Probability = pTableReward.Probability4;
				pRewardContents.Grade = pTableReward.Grade4;
				pRewardGroup.PackageContents.Add(pRewardContents);
			}

			if (pTableReward.PackageID5 != 0)
			{
				SRewardGroupContents pRewardContents = new SRewardGroupContents();
				pRewardContents.PackageID = pTableReward.PackageID5;
				pRewardContents.Probability = pTableReward.Probability5;
				pRewardContents.Grade = pTableReward.Grade5;
				pRewardGroup.PackageContents.Add(pRewardContents);
			}
		}
	}
}
