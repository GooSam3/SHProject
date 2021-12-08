using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CManagerScriptDataBase : CManagerTemplateBase<CManagerScriptDataBase>
{
	private Dictionary<string, CScriptDataBase> m_mapScriptData = new Dictionary<string, CScriptDataBase>();
	//-----------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		PrivScriptDataInitilize();
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		GlobalManagerScriptLoaded();
	}

	//-----------------------------------------------------------------
	private void PrivScriptDataInitilize()
	{
		List<CScriptDataBase> listScriptData = new List<CScriptDataBase>();
		GetComponentsInChildren(true, listScriptData);

		for (int i = 0; i < listScriptData.Count; i++)
		{
			m_mapScriptData[listScriptData[i].GetType().Name] = listScriptData[i];
			listScriptData[i].ImportScriptDataInitialize();
			OnScriptDataInitialize(listScriptData[i]);
		}
	}

	
	//-------------------------------------------------------------------
	protected virtual void OnScriptDataInitialize(CScriptDataBase pScriptData) { }
}
