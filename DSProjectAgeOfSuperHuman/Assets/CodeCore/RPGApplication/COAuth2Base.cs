using UnityEngine.Events;
using UnityEngine;
public abstract class COAuth2Base
{
	
	private bool m_bLoginStart = false;
	private bool m_bLogin = false; public bool GetOAuthLogin() { return m_bLogin; }
	private UnityAction<bool, string, string> m_delFinish = null;
	protected MonoBehaviour m_pCoroutine = null;
	//---------------------------------------------------------
	public void DoOAuthInitialize()
	{
		OnOAuthInitialize();
	}

	public void DoOAuthLoginStart(UnityAction<bool, string, string> delFinish)
	{
		if (m_bLoginStart)
		{
			delFinish?.Invoke(false, "[Login] Already Login start", "");
			return;
		}
		m_bLoginStart = true;
		m_delFinish = delFinish;
		OnOAuthLoginStart(HandleOAuthLoginResult);
	}

	public void DoOAuthLogOut()
	{
		m_bLoginStart = false;
		m_bLogin = false;
		OnOAuthLogOut();
	}

	//--------------------------------------------------------------
	private void HandleOAuthLoginResult(bool bResult, string strUserID, string strToken)
	{
		m_bLoginStart = false;
		if (bResult)
		{
			m_bLogin = true;
		}
		m_delFinish?.Invoke(bResult, strUserID, strToken);
	}

	//-------------------------------------------------------------
	protected virtual void OnOAuthInitialize() { }
	protected virtual void OnOAuthLoginStart(UnityAction<bool, string, string> delFinish) { }
	protected virtual void OnOAuthLogOut() { }
}
