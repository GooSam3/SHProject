using UnityEngine;
using UnityEngine.Events;

public class SHUIFrameLogin : SHUIFrameBase
{
	private const string const_LastLoginKey = "LastLoginServer";
	private const string const_LastLoginTye = "LastLoginType";

	public enum ELastLoginType
	{
		Guest,
		GooglePlay,
		AppleStore,
		OneStore,
	}

	[SerializeField]
	private CButton LoginGooglePlay = null;
	[SerializeField]
	private CButton LoginAppleStore = null;
	[SerializeField]
	private CButton LoginOneStore = null;
	[SerializeField]
	private CButton Login = null;
	[SerializeField]
	private CButton LogOut = null;

	private bool m_bLobbyStart = false;
	private bool m_bLoginSuccess = false;
	private string m_strLoginID;
	private string m_strToken;
	private ELastLoginType m_eLastLogin = ELastLoginType.Guest;
	//------------------------------------------------------
	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		m_bLobbyStart = false;
		m_bLoginSuccess = false;
		LoginGooglePlay.gameObject.SetActive(false);
		Login.gameObject.SetActive(false);
		LogOut.gameObject.SetActive(false);
		PrivLoginLoadLocalProfile();
	}

	
	//------------------------------------------------------
	public void DoLoginResetLoginData()
	{

	}

	//--------------------------------------------------------
	private void PrivLoginLoadLocalProfile()
	{
		string strLoginType = PlayerPrefs.GetString(const_LastLoginTye);
		// 테스트
		LoginGooglePlay.gameObject.SetActive(true);
		LoginGooglePlay.interactable = true;
		return;
		//------------------------------------
		if (strLoginType == string.Empty)
		{
		//	PrivLoginSelect();
		}
		else if (strLoginType == ELastLoginType.GooglePlay.ToString())
		{
		//	PrivLoginAutoGooglePlay();
		}
		else if (strLoginType == ELastLoginType.AppleStore.ToString())
		{
	//		PrivLoginAutoAppleStore();
		}
		else if (strLoginType == ELastLoginType.OneStore.ToString())
		{
	//		PrivLoginAutoOneStore();
		}	
	
	}

	private void PrivLoginAutoStart(ELastLoginType eLoginType)
	{
		if (m_bLobbyStart) return;
		m_bLobbyStart = true;

		CApplication.Instance.GetAppOuth2Login().DoOAuthLoginStart((bool bSucess, string strUseID, string strToken) =>
		{
			if (bSucess)
			{
				m_bLoginSuccess = true;
				m_strLoginID = strUseID;
				m_strToken = strToken;
			}
			else
			{
				//Error!! 
			}
			m_bLobbyStart = false;
			PrivLoginReadyToGame(eLoginType);
			Debug.Log(string.Format("[Login]  {0} / {1} ", bSucess, strUseID));
		});
	}


	private void PrivLoginAutoGooglePlay()
	{
		LoginGooglePlay.gameObject.SetActive(true);
		LoginGooglePlay.interactable = false;
		PrivLoginAutoStart(ELastLoginType.GooglePlay);
	}

	private void PrivLoginAutoAppleStore()
	{
		m_eLastLogin = ELastLoginType.AppleStore;
		PrivLoginAutoStart(ELastLoginType.AppleStore);
	}

	private void PrivLoginAutoOneStore()
	{
		m_eLastLogin = ELastLoginType.OneStore;
		PrivLoginAutoStart(ELastLoginType.OneStore);
	}

	private void PrivLoginSelect()		 
	{
		LoginGooglePlay.gameObject.SetActive(true);
		LoginGooglePlay.interactable = true;
		Login.gameObject.SetActive(false);
		LogOut.gameObject.SetActive(false);
	}

	private void PrivLoginLocalDeveloper()
	{
		string strDeviceID = SystemInfo.deviceUniqueIdentifier; // 게스트 계정으로 DB에 로그인 
		PrivLoginGotoLobby();
	}

	private void PrivLoginGotoLobby()
	{		
		// 사용자 ID로 네트워크 동기화 진행 
		UIManager.Instance.DoUIMgrSceneLoadingStart(EGameSceneType.SHSceneLobby, () =>
		{
			UIManager.Instance.DoUIMgrHide<SHUIFrameLoadingScreen>();
			UIManager.Instance.DoUIMgrShow<SHUIFrameLobby>();
		});
	}

	private void PrivLoginReadyToGame(ELastLoginType eLoginType)
	{
		m_eLastLogin = eLoginType;
		PlayerPrefs.SetString(const_LastLoginTye, m_eLastLogin.ToString());
		if (eLoginType == ELastLoginType.GooglePlay)
		{
			LoginGooglePlay.interactable = false;
		}

		Login.gameObject.SetActive(true);
		LogOut.gameObject.SetActive(true);
	}

	private void PrivLoginFilterForUnityEditor(ELastLoginType eLoginType)
	{
#if (UNITY_EDITOR)
		PrivLoginLocalDeveloper();
#elif (UNITY_ANDROID)
		PrivLoginAutoStart(eLoginType);
#endif
	}

	//------------------------------------------------------
	public void HandleLoginStart()
	{
		PrivLoginGotoLobby();
	}

	public void HandleLogOut()
	{
		m_bLobbyStart = false;
		CApplication.Instance.GetAppOuth2Login().DoOAuthLogOut();
		PlayerPrefs.DeleteKey(const_LastLoginTye);
		PrivLoginSelect();
	}

	public void HandleLoginGooglePlay()
	{
//		PrivLoginFilterForUnityEditor(ELastLoginType.GooglePlay);
		PrivLoginLocalDeveloper();
	}
}
