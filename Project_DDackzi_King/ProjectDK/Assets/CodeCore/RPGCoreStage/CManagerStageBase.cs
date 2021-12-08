using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CManagerStageBase : CManagerTemplateBase<CManagerStageBase>
{
	private Dictionary<string, CStageBase> m_mapStageInstance = new Dictionary<string, CStageBase>();
	private CStageBase m_pCurrentStage = null;
	private CStageBase m_pWaitStage = null;
	private bool m_bInitialize = false;
	//--------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		CStageBase pStage = FindObjectOfType<CStageBase>();
		if (pStage)
		{
			PrivStageRegist(pStage);
		}
	}

	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();
		m_bInitialize = true;

		if (m_pWaitStage != null)
		{
			PrivStageEnter(m_pWaitStage);
			m_pWaitStage = null;
		}
	}



	//---------------------------------------------------------------
	internal void ImportStage(CStageBase pStage, bool bRegist)
	{
		if (bRegist)
		{
			PrivStageRegist(pStage);
		}
		else
		{
			PrivStageUnRegist(pStage);
		}
	}

	//---------------------------------------------------------------
	private void PrivStageRegist(CStageBase pStage)
	{
		string strStageName = pStage.GetType().Name;
		if (m_mapStageInstance.ContainsKey(strStageName) == false)
		{
			m_mapStageInstance[strStageName] = pStage;
			pStage.ImportStageInitialize();

			if (pStage.IsDefaultStage)
			{
				if (m_bInitialize)
				{
					PrivStageEnter(pStage);
				}
				else
				{
					m_pWaitStage = pStage;
				}
			}
		}
	}

	private void PrivStageUnRegist(CStageBase pStage)
	{
		string strStageName = pStage.GetType().Name;
		if (m_mapStageInstance.ContainsKey(strStageName))
		{
			m_mapStageInstance.Remove(strStageName);
		}

		if (m_pCurrentStage == pStage)
		{
			pStage.ImportStageOut();
			m_pCurrentStage = null;
		}
	}

	private void PrivStageEnter(CStageBase pStage)
	{
		if (m_pCurrentStage)
		{
			m_pCurrentStage.ImportStageOut();
		}
		pStage.ImportStageEnter();
		m_pCurrentStage = pStage;		
	}
}
