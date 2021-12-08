using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class CManagerSessionBase : CManagerTemplateBase<CManagerSessionBase>
{
	public enum ESessionState
	{
		None,
		ConnectTry,
		Connect,
	}

	public enum ESessionError
	{
		None,
		ConnectionTry,
		NoNetwork,
		InvalidIP,

	}

	private ESessionState m_eSessionState = ESessionState.None;
	//-----------------------------------------------------------------------
	public void DoSessionConnect(string URL, UnityAction<ESessionError> delFinish)
	{
		if (m_eSessionState != ESessionState.None)
		{
			delFinish?.Invoke(ESessionError.ConnectionTry);
			return;
		}

		m_eSessionState = ESessionState.ConnectTry;
	
		OnMgrSessionConnect(URL, (ESessionError eError) => { 
			if (eError == ESessionError.None)
			{
				m_eSessionState = ESessionState.Connect;
			}
			delFinish?.Invoke(eError);
		});
	}

	public void DoSessionDisconnect(UnityAction<ESessionError> delFinish)
	{
		if (m_eSessionState == ESessionState.None)
		{
			delFinish?.Invoke(ESessionError.None);
			return;
		}

		OnMgrSessionDisConnect((ESessionError eError) => {
			m_eSessionState = ESessionState.None;
			delFinish?.Invoke(eError);
		});
	}


	//-----------------------------------------------------------------------
	protected virtual void OnMgrSessionConnect(string URL, UnityAction<ESessionError> _delFinish) { }
	protected virtual void OnMgrSessionDisConnect(UnityAction<ESessionError> _delFinish) { }
}
