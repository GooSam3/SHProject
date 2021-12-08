using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableItem : CScriptDataTableBase
{
	[System.Serializable]
	public class SItemTable : CObjectInstanceBase
	{
		[SerializeField]
		public uint				ItemID = 0;
		[SerializeField]
		public string				ItemName;
		[SerializeField]
		public string				ItemDescription;
		[SerializeField]
		public EItemType			EItemType = EItemType.None;
		[SerializeField]
		public EItemGrade			EItemGrade = EItemGrade.None;
		[SerializeField]
		public EItemGradeUI		EItemGradeUI = EItemGradeUI.Common;
		[SerializeField]
		public EItemConsumeType	EItemConsumeType = EItemConsumeType.None;
		[SerializeField]
		public int				ConsumeValue = 0;
		[SerializeField]
		public string				IconName;
		[SerializeField]
		public uint				MaxStack;
		[SerializeField]
		public EPriceType			EPriceType = EPriceType.None;
		[SerializeField]
		public uint				PriceBuy = 0;
		[SerializeField]
		public uint				PriceSell = 0;
	}

	private Dictionary<uint, SItemTable> m_mapItem = new Dictionary<uint, SItemTable>();
	//-------------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		List<SItemTable> pListTableLoad = ProtDataTableRead<SItemTable>();
		for (int i = 0; i < pListTableLoad.Count; i++)
		{
			SItemTable pTableItem = pListTableLoad[i];
			m_mapItem[pTableItem.ItemID] = pTableItem;
		}
	}
	//-------------------------------------------------------------------------
	public SItemTable GetTableItem(uint hItemID)
	{
		SItemTable pFindItem = null;
		if (m_mapItem.ContainsKey(hItemID))
		{
			pFindItem = m_mapItem[hItemID].CopyInstance<SItemTable>();
		}
		return pFindItem;
	}

}
