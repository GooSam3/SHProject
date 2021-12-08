using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScriptTableGameConfig : CScriptDataTableBase
{
    public class SGameConfig : object
	{
		public string		ConfigKeyName;
		public string		Value;
	}

	private Dictionary<string, string> m_mapGameConfig = new Dictionary<string, string>();
	//------------------------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		base.OnScriptDataInitialize(strTextData);

		List<SGameConfig> pListTableLoad = ProtDataTableRead<SGameConfig>();
		for (int i = 0; i < pListTableLoad.Count; i++)
		{
			m_mapGameConfig[pListTableLoad[i].ConfigKeyName] = pListTableLoad[i].Value;
		}
	}

	//-------------------------------------------------------------------------
	public float ExtractValueFloat(string strKey)
	{
		float fValue = 0;
		if (m_mapGameConfig.ContainsKey(strKey))
		{
			float.TryParse(m_mapGameConfig[strKey], out fValue);
		}
		return fValue;
	}

	public int ExtractValueInteger(string strKey)
	{
		int fValue = 0;
		if (m_mapGameConfig.ContainsKey(strKey))
		{
			int.TryParse(m_mapGameConfig[strKey], out fValue);
		}
		return fValue;
	}

	public string ExtractValueString(string strKey)
	{
		string strValue;
		m_mapGameConfig.TryGetValue(strKey, out strValue);
		return strValue;
	}
	//------------------------------------------------------------------------
}
