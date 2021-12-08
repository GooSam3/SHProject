using NTCore;
using System;
using UnityEngine;
using UnityEngine.Networking;
using WebNet;
using ZNet;
using ZNet.Data;

public class UILoginNetwork : UIFrameLogin
{
    #region Variable
    private UIFrameLogin Frame = null;
	#endregion

	protected override void Initialize(ZUIFrameBase _frame)
	{
		base.Initialize(_frame);

        Frame = _frame as UIFrameLogin;
    }

	/// <summary>게이트웨이 Server URL 체크</summary>
	private void CheckGatewayURL()
    {
        if (!ZWebManager.Instance.Gateway.IsUsable && string.IsNullOrEmpty(Auth.ServerAddr))
        {
            UICommon.OpenSystemPopup_One(ZUIString.ERROR,
                $"Auth.SelectedAuthInfo.ServerUrl이 설정되어야 사용 가능",
                ZUIString.LOCALE_OK_BUTTON);
        }
        else
        {
            ZWebManager.Instance.Gateway.ServerUrl = Auth.ServerAddr;
        }
    }

    /// <summary> 로그인 서버 연결 시작 </summary>
	public void OnConnectServer()
    {
        Frame.StartLoginBtn.interactable = false;

        CheckGatewayURL();

        NTIcarusManager.Instance.TryLastLogin((result) =>
        {
            GetServerList();

            /// 플랫폼 로그인 성공시 우측 상단 현재 플랫폼 출력
            if (Frame.Platform != null)
                Frame.Platform.ShowLogoutGroup(AuthAPI.CurrentPlatformID);
        }, (error) =>
        {
            ZLog.Log(ZLogChannel.System, $"{error.Message}");
            // 현재 머신에서 최초 로그인이라면 로그인 가능한 버튼 표시해주기
            Frame.Platform.CheckPlatformLoginGroup();
        }, Frame.CustomAccountID);
    }

    /// <summary> 서버 정보를 수신하고 리스트로 출력 </summary>
	private void GetServerList()
    {
        Frame.PopupServerList.ClearServerList();

        ZWebManager.Instance.Gateway.REQ_ServerList((recvPacket, resServerList) =>
        {
            CheckGatewayURL();

            // 계정 정보 요청
            ZWebManager.Instance.Gateway.REQ_GetAccount(ZNet.Data.Me.SessionToken, (recvPacketGetAnt, resGetAccount) =>
            {
				if (resGetAccount.State != E_AccountStateType.Active)
				{
					// 비정상 상태에 대한 처리를 했다면 이외의 처리는 Skip
					if (ProcessUnusualAccountState(ref resGetAccount))
						return;
				}

                //if (Frame.Platform != null)
                //    Frame.Platform.ShowLogoutGroup(AuthAPI.CurrentPlatformID);

                if (resGetAccount.LastLoginServerIdx != 0)
                {
                    Me.LastestLoginServerID = resGetAccount.LastLoginServerIdx;
                    if (Global.DicServer.TryGetValue(resGetAccount.LastLoginServerIdx, out WebNet.ServerInfo info))
                        Frame.SelectServerName.text = info.Name;
                }
                else
                {
                    Frame.SelectServerName.text = string.Empty;
                    Frame.PopupServerList.SwitchServerList(true);
                }

                foreach (var pair in Global.DicServer)
                {
                    void callBack()
                    {
                        Frame.SelectServerName.text = pair.Value.Name;
                        Me.SelectedServerID = pair.Key;
                        Me.SetConnectServerInfo(pair.Key);
                        Frame.PopupServerList.SwitchServerList(false);
                    }

                    UILoginServerListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UILoginServerListItem)).GetComponent<UILoginServerListItem>();

                    if (obj != null)
                    {
                        obj.transform.SetParent(Frame.ServerListScroll.content, false);
                        obj.Initialize(pair.Value.Name, pair.Value.TrafficStatus, pair.Value.IsNew, callBack);
                        Frame.ServerList.Add(obj);
                    }

                    if (pair.Value.Id == resGetAccount.LastLoginServerIdx)
                    {
                        callBack();
                    }
                }

                Frame.PopupServerList.SortServerList();
                Frame.StartBoardBtn.interactable = true;
                Frame.StartLoginBtn.gameObject.SetActive(false);
            }, OnGatewayNetError);
        }, OnGatewayNetError);
    }

	/// <summary>
	/// 계정 상태 처리
	/// </summary>
	/// <param name="resGetAccount"></param>
	private bool ProcessUnusualAccountState(ref ResGetAccount resGetAccount)
	{
		switch (resGetAccount.State)
		{
			case E_AccountStateType.LeaveWait:
				{
					Frame.PopupServerList.SwitchServerList(false);

					// yyyy-MM-dd-HH-mm-ss
					System.DateTime expireDateTime = TimeHelper.Time2DateTimeSec(resGetAccount.LeaveEndDt);
					UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
					{
						//TimeHelper.DateTime2TimeMs()
						_popup.Open(string.Empty, DBLocale.GetText("Account_Withdrawal_Cancel", expireDateTime.ToString()),
							new string[]
							{
								ZUIString.LOCALE_CANCEL_BUTTON,
								ZUIString.LOCALE_OK_BUTTON
							},
							new System.Action[]
							{
								delegate 
								{
									// TODO : 현재 로그인 플랫폼 변경하던지 해야될듯.
									//CheckAccount();
									Frame.Platform.CheckPlatformLoginGroup();
									_popup.Close();
								},
								delegate
								{
									ZWebManager.Instance.Gateway.REQ_AccountLeaveCancel(ZNet.Data.Me.SessionToken, (recvPacket, resAccountLeaveCancel) =>
									{
										CheckAccount();
									}, OnGatewayNetError);
									_popup.Close();
								}
							});
					});
				}
				return true;

			default:
				return false;
		}
	}

	/// <summary>계정을 체크하고 선택한 서버로 로그인 요청</summary>
	public void CheckAccount()
    {
        if (string.IsNullOrEmpty(Frame.SelectServerName.text))
        {
            Frame.PopupServerList.SwitchServerList(true);
            GetServerList();
            return;
        }

        //[wisreal][2020.11.20] - 불필요하게 호출 되는 케이스라 주석 처리
        //ZWebManager.Instance.Gateway.REQ_GetAccount(Me.SessionToken, (recvPacketGetAnt, resGetAccount) =>
        //{
            ZWebManager.Instance.Gateway.REQ_LoginAccount(Me.SelectedServerID, Me.SessionToken, (recvPacketLoginAnt, resServerList) =>
            {
                ZWebManager.Instance.WebGame.AutoReconnect = true;
                ZWebManager.Instance.WebChat.AutoReconnect = true;
                if (ZWebManager.Instance.WebGame.IsUsable)
                {
                    TryAccountLogin();
                }
                else
                {
                    ZWebManager.Instance.ConnectWebGame(Me.GameServerUrl).ConnectOpened = (webClient) =>
                    {
						ZWebManager.Instance.Billing.ServerUrl = Me.BillingServerUrl;

                        TryAccountLogin();
                    };
                }
            }, OnGatewayNetError);
        //}, OnGatewayNetError);
    }

    /// <summary>로그인 성공했을 경우 다음 씬으로 진행</summary>
	private bool TryAccountLogin()
    {
        if (!ZWebManager.Instance.WebGame.IsUsable)
        {
            ZLog.LogWarn(ZLogChannel.System, $"TryAccountLogin() | ZWebManager.Instance.WebGame.IsUsable: {ZWebManager.Instance.WebGame.IsUsable}");
            return false;
        }

        PlayerPrefs.SetString($"{Application.dataPath}_AccountId", Frame.CustomAccountID);
        PlayerPrefs.Save();

        ZWebManager.Instance.WebGame.ConnectOpened = null;
        ZWebManager.Instance.WebGame.REQ_AccountLogin(Me.SessionToken, ZWebManager.Instance.Gateway.SecurityKey, SystemInfo.deviceUniqueIdentifier, NTCore.CommonAPI.SetupData.clientVersion, (recvPacketAntLogin, resAccountLogin) =>
        {
            ZGameManager.Instance.FSM.ChangeState(E_GameState.CharacterSelect);
        });

        return true;
    }

    /// <summary>로그인 관련 서버 에러 수신 후 팝업 표시 </summary>
    private void OnGatewayNetError(ZGateway.NetErrorType errorType, UnityWebRequest request, ZWebRecvPacket recvPacket)
    {
        if (errorType == ZGateway.NetErrorType.Packet)
        {
            switch (recvPacket.ID)
            {
                case Code.GW_GET_ACCOUNT:

                    // 계정이 없는 경우
                    if (recvPacket.ErrCode == ERROR.NOT_HAVE_ACCOUNT)
                    {
                        DoCreateAccount();
                    }
                    else
                    {
                        ZWebManager.ShowErrorPopup(recvPacket.ErrCode);
                    }
                    break;

                case Code.GW_LOGIN_ACCOUNT:

                    if (recvPacket.ErrCode == ERROR.NOT_HAVE_ACCOUNT)
					{
                        DoCreateAccount();
                    }
                    else if (recvPacket.ErrCode == ERROR.PROTOCOL_VERSION)
                    {
                        string content = $"서버 프로토콜이 맞지않습니다.\nCurrent Protocol Version: {(uint)WebNet.Version.Num}";
                        if (Application.isEditor)
                            content += $"\n\n[Menu] 'ZGame/Protocols/[WEB] Sync ProtocolFiles' 로 프로토콜 갱신이 필요합니다.";

                        UICommon.OpenSystemPopup_One(ZUIString.ERROR, content, ZUIString.LOCALE_OK_BUTTON);
                    }
                    break;

                default:
					ZWebManager.ShowErrorPopup(recvPacket.ErrCode);
					//UICommon.OpenSystemPopup_One(ZUIString.ERROR, "문제가 발생하였습니다. 다시 시작해주세요.", ZUIString.LOCALE_OK_BUTTON);
                    break;
            }
        }
        else
        {
			string content = DBLocale.GetText("Internet_Error_Message");
			// DEV부터 QA까지는 추가 메시지 붙여주기.
			if (NTCore.CommonAPI.DomainType < NTCore.DomainType.Real)
			{
				content += $"\n{request.error}";
			}

			UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
			{
				_popup.Open(
					ZUIString.ERROR,
					content,
					new string[] { ZUIString.LOCALE_OK_BUTTON },
					new Action[] { delegate { ZGameManager.Instance.QuitApp(); } });
			});
        }
    }

    /// <summary>계정 생성후 원래 Flow대로 수행</summary>
    private void DoCreateAccount()
	{
        ZWebManager.Instance.Gateway.REQ_CreateAccount(ZNet.Data.Me.SessionToken, string.Empty, (recvPacketCreAnt, resServerList) =>
        {
            // 다시 계정 체크 시도
            CheckAccount();
        }, OnGatewayNetError);
    }
}
