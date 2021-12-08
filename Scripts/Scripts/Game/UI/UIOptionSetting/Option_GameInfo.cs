using NTCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option_GameInfo : OptionSetting
{
	[System.Serializable]
	public class SinglePlatformObj
	{
		public NTCore.PlatformID type;
		public GameObject obj;
	}

    [SerializeField] private Text AccountName, CharacterName, Version;
	[SerializeField] private GameObject CouponObj;

	[SerializeField] private List<SinglePlatformObj> CurrentPlatformIndicators;
	[SerializeField] private List<SinglePlatformObj> PlatformsToLink;
	[SerializeField] private GameObject PlatformsToLinkRoot;

	public override void LoadOption()
	{
		AccountName.text = ZNet.Data.Me.NID;//CommonAPI.SessionID;
		CharacterName.text = ZNet.Data.Me.CurCharData.Nickname;
		Version.text = $"{CommonAPI.GameCode}_{CommonAPI.GameSupportLanguage.LanguageCode}_{CommonAPI.DomainType}_{CommonAPI.SetupData.clientVersion}_{ZGameManager.Instance.RevisionVer}";

		NTCommon.NTClipboard.Copy(AccountName.text);

		UpdateUI_PlatformLogin();

		//[박윤성] IOS에서 쿠폰버튼 안나오게 하기
#if UNITY_IOS
		CouponObj.SetActive(false);
#endif
	}

	#region Platform Login (이윤선 작업)
	private void UpdateUI_PlatformLogin()
	{
		bool amIGuest = AuthAPI.CurrentPlatformID == NTCore.PlatformID.Guest && (AuthAPI.LinkedAccounts != null && AuthAPI.LinkedAccounts.Count == 1);
		bool guestLinked = AuthAPI.IsLinkedAccount(NTCore.PlatformID.Guest);
		bool googleLinked = AuthAPI.IsLinkedAccount(NTCore.PlatformID.GoogleSignIn);
		bool appleLinked = AuthAPI.IsLinkedAccount(NTCore.PlatformID.AppleSignIn);
		bool faceBookLinked = AuthAPI.IsLinkedAccount(NTCore.PlatformID.Facebook);

		if (PlatformsToLinkRoot != null)
		{
			/// 연동 가능 리스트는 내가 게스트일때만 띄움 . 
			PlatformsToLinkRoot.SetActive(amIGuest);
		}

		/// 게스트는 무조건 끔 . 게스트로 연동하는건 없음 .
		SetPlatformLoginLogoActive(PlatformsToLink, NTCore.PlatformID.Guest, false);

		/// 게스트일때만 연동 가능하게 처리 . 
		SetPlatformLoginLogoActive(PlatformsToLink, NTCore.PlatformID.GoogleSignIn, googleLinked == false);
		SetPlatformLoginLogoActive(PlatformsToLink, NTCore.PlatformID.Facebook, faceBookLinked == false);

#if UNITY_ANDROID
		SetPlatformLoginLogoActive(PlatformsToLink, NTCore.PlatformID.AppleSignIn, false);
#elif UNITY_IOS
		SetPlatformLoginLogoActive(PlatformsToLink, NTCore.PlatformID.AppleSignIn, appleLinked == false);
#endif

		if (CurrentPlatformIndicators != null)
		{
			foreach (var t in CurrentPlatformIndicators)
			{
				t.obj.SetActive(t.type == AuthAPI.CurrentPlatformID);
			}
		}
	}

	private void TryLinkPlatformAccount(NTCore.PlatformID type)
	{
		if (Application.isEditor)
		{
			OpenOneBtnCheckPopUp("에디터에서 지원하지 않습니다.");
			return;
		}

		/// 현재 게스트가 아닌 다른 플랫폼 로그인중이다 -> 더이상 연동불가 
		if (AuthAPI.IsLinkedAccount(NTCore.PlatformID.Guest) == false)
		{
			OpenOneBtnCheckPopUp("이미 계정 연동이 되어있습니다.");
			return;
		}

		var targetType = type;
		if (targetType == NTCore.PlatformID.None)
		{
			ZLog.LogError(ZLogChannel.UI, "LinkPlatformOperation Failed");
			return;
		}

		if(AuthAPI.IsLinkedAccount(targetType))
		{
			OpenOneBtnCheckPopUp("이미 연동된 플랫폼입니다.");
			return;
		}

		OpenTwoButtonQueryPopUp("확인", "해당 플랫폼 계정을 연동하시겠습니까?"
			, onConfirmed: () =>
			{
				RequestPlatformAccountLink(targetType);
			});
	}

	private void TryUnlinkPlatformAccount(NTCore.PlatformID type)
	{
		if(Application.isEditor)
		{
			OpenOneBtnCheckPopUp("에디터에서 지원하지 않습니다.");
			return;
		}

		var targetType = type;
		if (targetType == NTCore.PlatformID.None)
		{
			ZLog.LogError(ZLogChannel.UI, "LinkPlatformOperation Failed");
			return;
		}

		int curLinkedCnt = AuthAPI.NonGuestLinkedAccountCount;
		if (curLinkedCnt <= 1)
		{
			OpenOneBtnCheckPopUp("현재 연동된 플랫폼이 2개 이상일 때만 연동 해제가 가능합니다.");
			return;
		}

		OpenTwoButtonQueryPopUp("확인", "해당 플랫폼을 연동 해제하시겠습니까?"
			, onConfirmed: () =>
			{
				RequestPlatformAccountUnlink(targetType);
			});
	}

	private void RequestPlatformAccountLink(NTCore.PlatformID type)
	{
		NTIcarusManager.Instance.RequestLinkAccount(type, OnPlatformAccountLinked, OnPlatformAccountLinkFailed);
	}

	private void RequestPlatformAccountUnlink(NTCore.PlatformID type)
	{
		NTIcarusManager.Instance.RequestUnlinkAccount(type, OnPlatformAccountUnlinked, OnPlatformAccountUnlinkFailed);
	}

	private void OpenOneBtnCheckPopUp(string contentText, string title = "알림", string btnText = "확인", Action onConfirmed = null)
	{
		UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
		{
			/// TODO : LOCALE
			_popup.Open(
				title,
				contentText,
				new string[] { btnText },
				new Action[] { onConfirmed });
		});
	}

	private void OpenTwoButtonQueryPopUp(
		string title, string content, Action onConfirmed, Action onCanceled = null)
	{
		UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
		{
			_popup.Open(title, content, new string[] { "취소", "확인" }, new Action[] {
				() =>
				{
					 onCanceled?.Invoke();
					_popup.Close();
				},
				() =>
				{
				   onConfirmed?.Invoke();
					_popup.Close();
				}});
		});
	}

	private void OnPlatformAccountLinked(NTBaseResult obj)
	{
		UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
		{
			/// TODO : LOCALE
			_popup.Open(
				ZLogLevel.Error.ToString(),
				$"계정 연동에 성공하였습니다.",
				new string[] { "확인" },
				new Action[] { delegate { } });
		});

		UpdateUI_PlatformLogin();
	}

	private void OnPlatformAccountLinkFailed(Exception obj)
	{
		UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
		{
			string msgContent = string.Empty;

			if(obj.Message.Contains("exist linked puser"))
			{
				msgContent = "해당 계정은 이미 연동되어 사용이 불가합니다.";
			}
			else
			{
				msgContent = $"계정 연동을 실패하였습니다. {obj.Message}";
			}

			/// TODO : LOCALE
			_popup.Open(
				ZLogLevel.Error.ToString(),
				msgContent,
				new string[] { "확인" },
				new Action[] { delegate { } });
		});

		UpdateUI_PlatformLogin();
	}

	private void OnPlatformAccountUnlinked(NTBaseResult obj)
	{
		ZLog.Log(ZLogChannel.UI, "PlatformAccount UnLink Success");

		UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
		{
			/// TODO : LOCALE
			_popup.Open(
				ZLogLevel.Error.ToString(),
				$"계정 연동 해제에 성공하였습니다.",
				new string[] { "확인" },
				new Action[] { delegate { } });
		});

		UpdateUI_PlatformLogin();
	}

	private void OnPlatformAccountUnlinkFailed(Exception obj)
	{
		UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
		{
			/// TODO : LOCALE
			_popup.Open(
				ZLogLevel.Error.ToString(),
				$"계정 연동 해제에 실패하였습니다. {obj.Message}",
				new string[] { "확인" },
				new Action[] { delegate { } });
		});

		UpdateUI_PlatformLogin();
	}

	//public void OnClickPlatformLink_Guest()
	//{
	//	TryLinkPlatformAccount(PlatformIDType.Guest);
	//}

	public void OnClickPlatformLink_Google()
	{
		TryLinkPlatformAccount(NTCore.PlatformID.GoogleSignIn);
	}

	public void OnClickPlatformLink_Facebook()
	{
		TryLinkPlatformAccount(NTCore.PlatformID.Facebook);
	}

	public void OnClickPlatformLink_Apple()
	{
		TryLinkPlatformAccount(NTCore.PlatformID.AppleSignIn);
	}

	/// Unlink 기능은 사용하지 않음 . 
	//public void OnClickPlatformUnlink_Guest()
	//{
	//	TryUnlinkPlatformAccount(PlatformIDType.Guest);
	//}

	//public void OnClickPlatformUnlink_Google()
	//{
	//	TryUnlinkPlatformAccount(PlatformIDType.Google);
	//}

	//public void OnClickPlatformUnlink_Facebook()
	//{
	//	TryUnlinkPlatformAccount(PlatformIDType.Facebook);
	//}

	//public void OnClickPlatformUnlink_Apple()
	//{
	//	TryUnlinkPlatformAccount(PlatformIDType.Apple);
	//}

	#endregion

	private void SetPlatformLoginLogoActive(List<SinglePlatformObj> list, NTCore.PlatformID type, bool active)
	{
		if (list == null)
			return;

		var target = list.Find(t => t.type == type);
		if (target != null)
			target.obj.SetActive(active);
	}

	public void ClickAsk()
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 문의하기");

		NTIcarusManager.Instance.ShowCustomerService(ZNet.Data.Me.NID);
	}

    public void ClickCafe()
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 공식카페");
	}

    public void ClickLogout()
    {
        ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 로그아웃");

		UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
		{
			_popup.Open(string.Empty, "계정 로그아웃을 진행하시겠습니까?",
				new string[]
				{
					ZUIString.LOCALE_CANCEL_BUTTON,
					ZUIString.LOCALE_OK_BUTTON
				},
				new Action[]
				{
					delegate { _popup.Close(); },
					delegate 
					{
						NTIcarusManager.Instance.Logout();
						_popup.Close();
						ZGameManager.Instance.Restart();
					}
				});
		});
	}

	public void ClickCharacterSelect()
	{
		ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 캐릭터 선택");

		UICommon.OpenPopup_GoCharacterSelectState();
	}

	public void OnClickCoupon()
	{
		if (null != UIManager.Instance)
			UIManager.Instance.ShowGlobalIndicator(true);

		NTIcarusManager.Instance.ShowCouponPage(ZNet.Data.Me.NID, ()=>
		{
			if (null != UIManager.Instance)
				UIManager.Instance.ShowGlobalIndicator(false);
		});
	}

	public void OnClickNotice()
    {
		ZLog.Log(ZLogChannel.UI, "## OptionSetting ## 공지게시판");
	}

#region  ========:: 탈퇴 처리 ::========

	/// <summary> 게임 탈퇴 </summary>
	public void OnClickAccountLeave()
	{
		UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
		{
			_popup.Open(string.Empty, DBLocale.GetText("Account_Withdrawal_Text"),
				new string[]
				{
					ZUIString.LOCALE_CANCEL_BUTTON,
					ZUIString.LOCALE_OK_BUTTON
				},
				new Action[]
				{
					delegate { _popup.Close(); },
					delegate
					{
						UIMessagePopup.ShowInputPopup(ConfirmAccountLeave);
						_popup.Close();
					}
				});
		});
	}

	/// <summary> 탈퇴시도 마지막 확인 </summary>
	private void ConfirmAccountLeave(UIMessagePopupInput _popup)
	{
		string checkingStr = DBLocale.GetText("Account_Withdrawal_Option_Button");
		_popup.Set(DBLocale.GetText("Account_Withdrawal_Confirm_Title"), DBLocale.GetText("Account_Withdrawal_Confirm"), DBLocale.GetText("Account_Withdrawal_PreInput"),
			(inputStr) => 
			{
				if (inputStr == checkingStr)
				{
					ZWebManager.Instance.WebGame.REQ_AccountLeave(ZNet.Data.Me.NID, (recvPacket, resAccountLeave) =>
					{
						UICommon.OpenSystemPopup_One(string.Empty,
							DBLocale.GetText("Account_Withdrawal_Request_Comp"),
							ZUIString.LOCALE_OK_BUTTON,
							(completionPopup) =>
							{
								UICommon.SetNoticeMessage(DBLocale.GetText("Account_Withdrawal_Request_Comp"), Color.red, 5, UIMessageNoticeEnum.E_MessageType.SubNotice);
								ZGameManager.Instance.QuitApp();
							});
					});

					_popup.Close();
				}
				else
				{
					UICommon.OpenSystemPopup_One(string.Empty,
							DBLocale.GetText("Account_Withdrawal_Error"),
							ZUIString.LOCALE_OK_BUTTON);
				}
			},
			() => 
			{
				_popup.Close();
			}, DBLocale.GetText("Account_Withdrawal_Option_Button").Length);
	}
#endregion
}
