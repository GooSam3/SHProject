using WebNet; // protocol

namespace ZNet
{
	public class ZGateway : ZWebRequestClientBase
	{
		public override bool IsUsable
		{
			get { return !string.IsNullOrEmpty(this.ServerUrl) && this.enabled; }
		}

		public override E_WebSocketType SocketType => E_WebSocketType.GateWay;

		/// <summary> 서버에서 받은 통신키 (보안키로 사용) </summary>
		public override uint SecurityKey { get => mRndKey; protected set => mRndKey = value; }

		private uint mRndKey;

		/// <summary>
		/// 
		/// </summary>
		public void REQ_ServerList(System.Action<ZWebRecvPacket, ResServerList> _onRecvSuccess, WebRequestErrorDelegate _onNetError = null)
		{
			// FlatBuffer 구성
			WebNet.ReqServerList.StartReqServerList(mBuilder);
			var offset = WebNet.ReqServerList.EndReqServerList(mBuilder);
			// FlatBuffer 기반으로 패킷 생성
			var reqPacket = ZWWWPacket.Create<ReqServerList>(Code.GW_SERVER_LIST, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResServerList resServerList = recvPacket.Get<ResServerList>();

				// 서버 시간 최초 저장
				TimeManager.Instance.SetTime(resServerList.ServerTsMs, resServerList.ServerTsMs);
				// 서버 리스트 정보 저장
				{
					ZNet.Data.Global.DicServer.Clear();

					int serverCount = resServerList.ServerListLength;
					for (int i = 0; i < serverCount; i++)
					{
						var serverInfo = resServerList.ServerList(i).Value;
						ZNet.Data.Global.DicServer.Add(serverInfo.Id, serverInfo);
					}
				}

				_onRecvSuccess?.Invoke(recvPacket, resServerList);
			},
			_onNetError);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_sessionToken">계정 고유 식별자 (구글 UID, sessio_token같은)</param>
		public void REQ_GetAccount(string _sessionToken, System.Action<ZWebRecvPacket, ResGetAccount> _onRecvSuccess, WebRequestErrorDelegate _onNetError = null)
		{
			ZLog.LogWarn(ZLogChannel.System, $"{nameof(REQ_GetAccount)} | _accountUID(session_token): {_sessionToken}");

			var offset = WebNet.ReqGetAccount.CreateReqGetAccount(mBuilder, mBuilder.CreateString(_sessionToken));
			var reqPacket = ZWWWPacket.Create<ReqGetAccount>(Code.GW_GET_ACCOUNT, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResGetAccount resGetAccount = recvPacket.Get<ResGetAccount>();
				ZNet.Data.Me.UserID = resGetAccount.UserId;
				ZNet.Data.Me.NID = resGetAccount.Nid;
				ZNet.Data.Me.gNID = resGetAccount.Gnid;
				ZNet.Data.Me.LastestLoginServerID = resGetAccount.LastLoginServerIdx;
				ZNet.Data.Global.AddUser(resGetAccount.UserId);

				_onRecvSuccess?.Invoke(recvPacket, resGetAccount);
			},
			_onNetError);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_sessionToken">계정 고유 식별자 (구글 UID, sessio_token같은)</param>
		/// <param name="_email"></param>
		/// <param name="_onRecvSuccess"></param>
		public void REQ_CreateAccount(string _sessionToken, string _email, System.Action<ZWebRecvPacket, ResCreateAccount> _onRecvSuccess, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = WebNet.ReqCreateAccount.CreateReqCreateAccount(mBuilder,
				mBuilder.CreateString(_sessionToken), /*!string.IsNullOrEmpty(_email) ? mBuilder.CreateString(_email) : default */
				!string.IsNullOrEmpty(NTCore.CommonAPI.RuntimeOS) ? mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS) : default,
				!string.IsNullOrEmpty(NTCommon.Device.Release) ? mBuilder.CreateString(NTCommon.Device.Release) : default,
				!string.IsNullOrEmpty(NTCommon.Device.Model) ? mBuilder.CreateString(NTCommon.Device.Model) : default,
				!string.IsNullOrEmpty(NTCommon.Locale.GetLocaleCode()) ? mBuilder.CreateString(NTCommon.Locale.GetLocaleCode()) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.GetGameSupportLanguage().langCulture) ? mBuilder.CreateString(NTCore.CommonAPI.GetGameSupportLanguage().langCulture) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.SetupData.clientVersion) ? mBuilder.CreateString(NTCore.CommonAPI.SetupData.clientVersion) : default,
				!string.IsNullOrEmpty(NTIcarusManager.Instance.ReferrerUrl) ? mBuilder.CreateString(NTIcarusManager.Instance.ReferrerUrl) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.SetupData.storeCD.ToString()) ? mBuilder.CreateString(NTCore.CommonAPI.SetupData.storeCD.ToString()) : default,
				!string.IsNullOrEmpty(NTCore.AuthAPI.CurrentPlatformID.ToString()) ? mBuilder.CreateString(NTCore.AuthAPI.CurrentPlatformID.ToString()) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.GetGameServerID()) ? mBuilder.CreateString(NTCore.CommonAPI.GetGameServerID()) : default);
			var reqPacket = ZWWWPacket.Create<ReqCreateAccount>(Code.GW_CREATE_ACCOUNT, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResCreateAccount resCreateAccount = recvPacket.Get<ResCreateAccount>();
				ZNet.Data.Me.UserID = resCreateAccount.UserId;
				ZNet.Data.Me.NID = resCreateAccount.Nid;
				ZNet.Data.Me.gNID = resCreateAccount.Gnid;
				//ZNet.Data.Me.LastestLoginServerID = resCreateAccount.LastLoginServerIdx;
				ZNet.Data.Global.AddUser(resCreateAccount.UserId);
				_onRecvSuccess?.Invoke(recvPacket, resCreateAccount);

				NTIcarusManager.Instance.AdjustTrackEvent(NTIcarusManager.TOKEN_05_FirstLogin);
			},
			_onNetError);
		}

		public void REQ_LoginAccount(uint _serverId, string _sessionToken, System.Action<ZWebRecvPacket, ResLoginAccount> _onRecvSuccess, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = WebNet.ReqLoginAccount.CreateReqLoginAccount(mBuilder,
				(byte)_serverId,
				(uint)WebNet.Version.Num,
				mBuilder.CreateString(_sessionToken),
				!string.IsNullOrEmpty(NTCore.CommonAPI.RuntimeOS) ? mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS) : default,
				!string.IsNullOrEmpty(NTCommon.Device.Release) ? mBuilder.CreateString(NTCommon.Device.Release) : default,
				!string.IsNullOrEmpty(NTCommon.Device.Model) ? mBuilder.CreateString(NTCommon.Device.Model) : default,
				!string.IsNullOrEmpty(NTCommon.Locale.GetLocaleCode()) ? mBuilder.CreateString(NTCommon.Locale.GetLocaleCode()) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.GetGameSupportLanguage().langCulture) ? mBuilder.CreateString(NTCore.CommonAPI.GetGameSupportLanguage().langCulture) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.SetupData.clientVersion) ? mBuilder.CreateString(NTCore.CommonAPI.SetupData.clientVersion) : default,
				!string.IsNullOrEmpty(NTCore.CommonAPI.SetupData.storeCD.ToString()) ? mBuilder.CreateString(NTCore.CommonAPI.SetupData.storeCD.ToString()) : default,
				!string.IsNullOrEmpty(NTCore.AuthAPI.CurrentPlatformID.ToString()) ? mBuilder.CreateString(NTCore.AuthAPI.CurrentPlatformID.ToString()) : default,
				false, // TODO: 넣어줘
				false, // TODO: 넣어줘
				(uint)(UnityEngine.Time.realtimeSinceStartup - ZGameManager.Instance.GameStartTime));

			var reqPacket = ZWWWPacket.Create<ReqLoginAccount>(Code.GW_LOGIN_ACCOUNT, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResLoginAccount resLoginAccount = recvPacket.Get<ResLoginAccount>();

				var webChat = ZWebManager.Instance.ConnectWebChat(ZNet.Data.Me.ChatServerUrl);

				webChat.ConnectOpened = (webClient) =>
				{
					REQ_LoginWaitUsers(_serverId, (recvWaitPacket, resMsg) =>
					{
						if (resMsg.IsContinue)
						{
							ZNet.Data.Me.SessionToken = _sessionToken;
							ZNet.Data.Me.UserID = resLoginAccount.UserId;
							ZNet.Data.Me.AccountOptionBit = resLoginAccount.OptionBit;
							ZNet.Data.Me.LastestLoginServerID = resLoginAccount.LastLoginServerIdx;
							this.SecurityKey = resMsg.Rndkey;

							_onRecvSuccess?.Invoke(recvPacket, resLoginAccount);

							NTIcarusManager.Instance.AdjustTrackEvent(NTIcarusManager.TOKEN_06_FirstLogin_Device);
							NTIcarusManager.Instance.AdjustTrackEvent(NTIcarusManager.TOKEN_07_Login);
						}
						else
						{
							if (UIManager.Instance.Find(out UIFrameLogin _login))
								_login.PopupServerWait.BindReconnectCallback(resLoginAccount.WaitQueueCnt);
						}

						ZWebManager.Instance.WebChat.ConnectOpened = null;
						ZWebManager.Instance.WebChat.ErrorOccurred = null;
					});
				};

				webChat.ErrorOccurred = (webClient, err) =>
				{
					if(webChat.IsUsable == false)
					{
						UIMessagePopup.ShowPopupOk($"{DBLocale.GetText("CHATTING_SERVER_SHUTOUT")}", () => 
						{
							ZGameManager.Instance.QuitApp();
						});

						//어플리케이션 종료로 인해 뜨지 않게
						if (ZGameManager.hasInstance) {
							ZLog.LogError(ZLogChannel.ChatServer, $"에러, {webClient.Socket.State}, {err}, connecting : {webClient.Socket.IsConnecting}");
						}
					}
				};
			},
			_onNetError);
		}

		public void REQ_LoginWaitUsers(uint _serverId, System.Action<ZWebRecvPacket, ResLoginWaitUsers> _onRecvPacket, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = WebNet.ReqLoginWaitUsers.CreateReqLoginWaitUsers(mBuilder,
				(byte)_serverId,
				ZNet.Data.Me.UserID);

			var reqPacket = ZWWWPacket.Create<ReqLoginWaitUsers>(Code.GW_LOGIN_WAIT_USERS, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResLoginWaitUsers resLoginWaitUsers = recvPacket.Get<ResLoginWaitUsers>();

				_onRecvPacket?.Invoke(recvPacket, resLoginWaitUsers);
			},
			_onNetError);
		}

		/// <summary>
		/// 계정에 존재하는 캐릭터 정보 요청
		/// </summary>
		public void REQ_ServerCharList(string _sessionToken, System.Action<ZWebRecvPacket, ResServerCharList> _onRecvSuccess, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = ReqServerCharList.CreateReqServerCharList(mBuilder, mBuilder.CreateString(_sessionToken));
			var reqPacket = ZWWWPacket.Create<ReqServerCharList>(Code.GW_SERVER_CHAR_LIST, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResServerCharList resServerCharList = recvPacket.Get<ResServerCharList>();
				_onRecvSuccess?.Invoke(recvPacket, resServerCharList);
			},
			_onNetError);
		}

		/// <summary> 계정 탈퇴 취소 </summary>
		/// <param name="_sessionToken">계정 고유 식별자 (구글 UID같은)</param>
		public void REQ_AccountLeaveCancel(string _sessionToken, System.Action<ZWebRecvPacket, ResAccountLeaveCancel> _onReceive, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = ReqAccountLeaveCancel.CreateReqAccountLeaveCancel(mBuilder, mBuilder.CreateString(_sessionToken));
			var reqPacket = ZWWWPacket.Create<ReqAccountLeaveCancel>(Code.GW_ACCOUNT_LEAVE_CANCEL, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResAccountLeaveCancel recvMsgPacket = recvPacket.Get<ResAccountLeaveCancel>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onNetError);
		}
	}
}
