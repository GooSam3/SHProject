using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKScriptCharacter : CScriptDataTableBase
{
	public class SDKCharacterTable
	{
		public string CharName;
		public string Image;
		public List<int> Data = new List<int>();
	}

	private Dictionary<uint, SDKCharacterTable> m_mapCharacterTable = new Dictionary<uint, SDKCharacterTable>();
	//-------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);
		PrivScriptDataLoadCharacterTable();
	}

	//-------------------------------------------------------------------
	private void PrivScriptDataLoadCharacterTable()
	{
		int totalLine = GetLineCount();
		for (int i = 0; i < totalLine; i++)
		{
			SDataTableLine pLine = GetLine(i);
			SDKCharacterTable pCharTable = PrivScriptDataReadLine(pLine);
			
		}		
	}

	private SDKCharacterTable PrivScriptDataReadLine(SDataTableLine pDataTable)
	{
		SDKCharacterTable pNewInstance = new SDKCharacterTable();
		int totalHeader = GetHeaderCount();
		
		for (int i = 0; i < totalHeader; i++)
		{
			string strValueName = GetHeader(i);
			string strValue = pDataTable.contentsList[i];
			GlobalScriptDataReadField(pNewInstance, strValueName, strValue);
		}


		return pNewInstance;
	}
}
