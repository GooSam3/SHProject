using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CApplication : CManagerTemplateBase<CApplication>
{
	public enum EAppLanguageType
	{
		English,
		Korean,
		China,
		Japan,		
	}

	public enum EOAuth2Type
	{
		Andorid,
		IOS,
	}

	[SerializeField]
	private Vector2 ScreenSize = new Vector2(2160, 1080);  public Vector2 GetAppBaseResolution() { return ScreenSize; }

	[SerializeField]
	private EAppLanguageType BuildLanguage = EAppLanguageType.English; public EAppLanguageType GetAppLanguage() { return BuildLanguage; }

	[SerializeField]
	private EOAuth2Type OAuthType = EOAuth2Type.Andorid;		public EOAuth2Type GetMarketType() { return OAuthType; }

	private COAuth2Base m_pOAuth2Login = null;				public COAuth2Base GetAppOuth2Login() { return m_pOAuth2Login; }
	//------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		Application.targetFrameRate = 60;
//		Screen.SetResolution((int)ScreenSize.x, (int)ScreenSize.y, false);

		if (OAuthType == EOAuth2Type.Andorid)
		{
			m_pOAuth2Login = new COAuth2GooglePlay();
		}
	}

	protected override void OnUnityStart()
	{
		m_pOAuth2Login.DoOAuthInitialize();
	}
}
