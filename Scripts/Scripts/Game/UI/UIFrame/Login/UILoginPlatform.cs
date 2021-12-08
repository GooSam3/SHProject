using System.Collections;
using UnityEngine;

public class UILoginPlatform : UIFrameLogin
{
	#region Variable
	private UIFrameLogin Frame = null;

	private bool IsLogoutInProcess;
	#endregion

	protected override void Initialize(ZUIFrameBase _frame)
	{
		base.Initialize(_frame);

		Frame = _frame as UIFrameLogin;

		InitPlatformLoginGroup();
	}

	private void InitPlatformLoginGroup()
	{
		Frame.PlatformLoginGroupObject.SetActive(false);

		Frame.AppleBtn.SetListener(OnClickAppleLogin);
		Frame.FacebookBtn.SetListener(OnClickFacebookLogin);
		Frame.GoogleBtn.SetListener(OnClickGoogleLogin);
		Frame.GuestBtn.SetListener(OnClickGuestLogin);
		Frame.LogoutBtn.SetListener(OnClickChangeAccount);
	}

	public void CheckPlatformLoginGroup()
	{
		bool facebook = NTIcarusManager.Instance.IsSupportPlatform_ConfigChecked(NTCore.PlatformID.Facebook);
		bool google = NTIcarusManager.Instance.IsSupportPlatform_ConfigChecked(NTCore.PlatformID.GoogleSignIn);
		bool apple = NTIcarusManager.Instance.IsSupportPlatform_ConfigChecked(NTCore.PlatformID.AppleSignIn);
		bool guest = NTIcarusManager.Instance.IsSupportPlatform_ConfigChecked(NTCore.PlatformID.Guest);

		Frame.FacebookBtn.gameObject.SetActive(!NTCore.AuthAPI.Connected && facebook);
		Frame.GoogleBtn.gameObject.SetActive(!NTCore.AuthAPI.Connected && google);
		Frame.GuestBtn.gameObject.SetActive(!NTCore.AuthAPI.Connected && guest);
		Frame.AppleBtn.gameObject.SetActive(!NTCore.AuthAPI.Connected && apple);

		if (facebook == false
			&& google == false
			&& apple == false
			&& guest == false)
		{
			ZLog.LogError(ZLogChannel.UI, "No Platform Supported. Solve this. ");

			if (Frame.NoPlatformEnabledText != null)
				Frame.NoPlatformEnabledText.gameObject.SetActive(true);
		}
		else
		{
			if (Frame.NoPlatformEnabledText != null)
				Frame.NoPlatformEnabledText.gameObject.SetActive(false);
		}

		Frame.LogoutBtn.gameObject.SetActive(NTCore.AuthAPI.Connected);

		Frame.GuestIDField.gameObject.SetActive(true);
		Frame.PlatformLoginGroupObject.SetActive(true);
	}

	public void OnClickAppleLogin()
	{
		Frame.PlatformLoginGroupObject.SetActive(false);

		TryLogin_NTSDK(NTCore.PlatformID.AppleSignIn);
	}

	public void OnClickFacebookLogin()
	{
		Frame.PlatformLoginGroupObject.SetActive(false);

		TryLogin_NTSDK(NTCore.PlatformID.Facebook);
		//UICommon.OpenSystemPopup_One(ZUIString.WARRING,	"아직 미지원 플랫폼입니다.", ZUIString.LOCALE_OK_BUTTON);
	}

	public void OnClickGoogleLogin()
	{
		Frame.PlatformLoginGroupObject.SetActive(false);

		TryLogin_NTSDK(NTCore.PlatformID.GoogleSignIn);
	}

	public void OnClickGuestLogin()
	{
		Frame.PlatformLoginGroupObject.SetActive(false);

		ZLog.Log(ZLogChannel.System, $"ClickGuestLogin: {Frame.CustomAccountID}");

		TryLogin_NTSDK(NTCore.PlatformID.Guest, Frame.CustomAccountID);
	}

	private void TryLogin_NTSDK(NTCore.PlatformID platformID, params object[] @params)
	{
		NTIcarusManager.Instance.TryLogin(platformID, (result) =>
		{
			Frame.Network.CheckAccount();

			/// 플랫폼 로그인 성공시 우측 상단 현재 플랫폼 출력
			if (Frame.Platform != null)
				Frame.Platform.ShowLogoutGroup(platformID);
		}, (error) =>
		{

			string localeID = "Login_Error_Message";
			if (error is NTCore.NTException ntError)
			{
				switch (ntError.Result)
				{
					case NTRESULT.NETWORK_ERROR:
						localeID = "Internet_Error_Message";
						break;
				}
			}
//#if UNITY_ANDROID || UNITY_IOS //GoogleSignInLoginFailedException <<내부 코드도 이렇게 감싸져 있어서..
//			else if (error is NTSDK.Module.GooglePlayService.GoogleSignInLoginFailedException signError)
//			{
//				localeID = $"StatusCode: {signError.StatusCode}, Message: {signError.Message}";
//			}
//#endif

			UICommon.OpenSystemPopup_One(ZUIString.ERROR,
				DBLocale.GetText(localeID), ZUIString.LOCALE_OK_BUTTON);

			//실패시 다시 플랫폼 로그인창 띄워주기
			CheckPlatformLoginGroup();
		}, @params);
	}

	/// <summary> 클릭시, 계정변경(로그아웃과 같은) 용도 </summary>
	public void OnClickChangeAccount()
	{
		ChangeAccountProcess();
	}

	public void ChangeAccountProcess()
	{
		if (IsLogoutInProcess)
			return; 

		E_ChangePlatformAccount();

		// 팝업출력 제거 
		//UICommon.OpenSystemPopup_One(ZUIString.ERROR,
		//	DBLocale.GetText("Logout_Account_Message"),
		//	ZUIString.LOCALE_OK_BUTTON,
		//	//ok
		//	delegate
		//	{
		//		if (null != Frame.ChangePlatformRoutine)
		//			return;

		//		Frame.SetPlatformRoutine(StartCoroutine(E_ChangePlatformAccount()));
		//	});
	}

	private void E_ChangePlatformAccount()
	{
		ShowLogoutGroup(NTCore.PlatformID.None);
		
		/// 모든 플랫폼 로그아웃 
		NTIcarusManager.Instance.Logout(
			_onAllPlatformLogoutCB: (result) =>
			{
				CheckPlatformLoginGroup();
				IsLogoutInProcess = false; 
			});
	}

	public void ShowLogoutGroup(NTCore.PlatformID ID)
    {
		if (Frame.LogoutPromptObject == null || Frame.LogoutPromptStatus == null)
		{
			ZLog.LogWarn(ZLogChannel.UI, "UI NULL");
			return;
		}

		if (ID == NTCore.PlatformID.None)
        {
			Frame.SetLogoutPromptStatusText(string.Empty);
			Frame.LogoutPromptObject.SetActive(false);
			return;
		}

		string platformName = "Logout";

		switch (ID)
		{
			case NTCore.PlatformID.Facebook:
				platformName = "Facebook";
				break;
			case NTCore.PlatformID.GooglePlay:
			case NTCore.PlatformID.GoogleSignIn:
				platformName = "Google";
				break;
			case NTCore.PlatformID.AppleGameCenter:
			case NTCore.PlatformID.AppleSignIn:
				platformName = "GameCenter";
				break;
			case NTCore.PlatformID.Guest:
				platformName = "Guest";
				break;
			default:
				ZLog.LogError(ZLogChannel.UI, "Please Add");
				break;
		}

		Frame.SetLogoutPromptStatusText(platformName);

		Frame.LogoutPromptObject.SetActive(true);
	}
}