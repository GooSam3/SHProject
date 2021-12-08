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
			PrivStageEnter(m_pWaitStage, 0); // 싱글모드로 스테이지를 실행시킬때
			m_pWaitStage = null;
		}
	}

	//---------------------------------------------------------------
	internal void ImportStageRegist(CStageBase pStage, bool bRegist)
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
					PrivStageEnter(pStage, 0);
				}
				else
				{
					m_pWaitStage = pStage;
				}
			}
			else
			{
				m_pCurrentStage = pStage;
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
			pStage.ImportStageExit();
			m_pCurrentStage = null;
		}
	}

	private void PrivStageEnter(CStageBase pStage, uint hStageID)
	{
		if (m_pCurrentStage)
		{
			m_pCurrentStage.ImportStageExit();
		}
		pStage.ImportStageEnter(hStageID);
		m_pCurrentStage = pStage;		
	}

	//-------------------------------------------------------------------
	protected CStageBase GetStageCurrent()
	{
		CStageBase pCurrent = m_pCurrentStage;
		if (pCurrent == null)
		{
			pCurrent = m_pWaitStage;
		}

		return pCurrent;
	}

	protected void ProtStageEnter(uint hStageID)
	{
		PrivStageEnter(m_pCurrentStage, hStageID);
	}

	protected void ProtStageEnter(string strStageName, uint hStageID)
	{
		if (m_mapStageInstance.ContainsKey(strStageName))
		{
			PrivStageEnter(m_mapStageInstance[strStageName], hStageID);
		}		
	}
}
