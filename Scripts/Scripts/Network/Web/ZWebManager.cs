using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Zero;
using ZNet;
using System.Collections;

public enum E_WebSocketType
{
	GateWay,
	Game,
	Chat,
	Billing,
}

/// <summary>
/// 
/// </summary>
public class ZWebManager : Singleton<ZWebManager>
{
	public ZNet.ZGateway Gateway { get; private set; }
	public ZNet.ZWebGame WebGame { get; private set; }
	public ZNet.ZWebChat WebChat { get; private set; }
	public ZNet.ZWebBilling Billing { get; private set; }

	private Dictionary<E_WebSocketType, ZNet.ZWebClientBase> mDicWebClient = new Dictionary<E_WebSocketType, ZNet.ZWebClientBase>();

	protected override void Init()
	{
		base.Init();

		/*
		 * 네트워크 통신기 준비
		 */
		Gateway = gameObject.GetOrAddComponent<ZNet.ZGateway>();
		WebGame = gameObject.GetOrAddComponent<ZNet.ZWebGame>();
		WebChat = gameObject.GetOrAddComponent<ZNet.ZWebChat>();
		Billing = gameObject.GetOrAddComponent<ZNet.ZWebBilling>();

		mDicWebClient.Add(Gateway.SocketType, Gateway);
		mDicWebClient.Add(WebGame.SocketType, WebGame);
		mDicWebClient.Add(WebChat.SocketType, WebChat);
		mDicWebClient.Add(Billing.SocketType, Billing);

		WebGame.ErrorReconnected += OnErrorReconnected;

		ZWebCommunicator.Instance.PacketErrorOccured += ProcessErrorPacket;
		ZWebCommunicator.Instance.IndicatorStatusEvent += OnIndicatorStatus;
	}

	public bool IsUsable(E_WebSocketType socketType)
	{
		return mDicWebClient.TryGetValue(socketType, out var client) && client.IsUsable;
	}

	public ZNet.ZWebGame ConnectWebGame(string _serverUrl)
	{
		WebGame.Connect(_serverUrl);

		return WebGame;
	}

	public ZNet.ZWebChat ConnectWebChat(string _serverUrl)
    {
		WebChat.Connect(_serverUrl);

		return WebChat;
    }

	public bool Disconnect(E_WebSocketType socketType)
	{
		bool result = false;

		if (mDicWebClient.TryGetValue(socketType, out var webClient))
		{
			webClient.Disconnect();
			result = true;
		}

		return result;
	}

	/// <summary> 초기화용 </summary>
	public void DisconnectAll()
	{
		foreach (var webClient in mDicWebClient.Values)
		{
			if (null != webClient &&
				(webClient.Socket.IsOpened || webClient.Socket.IsConnecting))
				webClient.Disconnect();
		}
	}

	/// <summary> 재접속 처리까지 실패시 발생되는 이벤트 </summary>
	private void OnErrorReconnected(ZWebClientBase _webClient)
	{
		UICommon.OpenConsolePopup((UIPopupConsole _popup) => {
			_popup.Open("재접속 처리 실패", $"ReconnectTimeout:{_webClient.MaxReconnectTimeout}초, TryedCount: {_webClient.CurReconnectNumber}\n게임을 종료합니다.", new string[] { "확인" }, new System.Action[] { () =>
			{
				ZGameManager.Instance.QuitApp();
			}});
		});
	}

    /// <summary>
    /// 프로토콜 통신 에러 났을때 기본적으로 처리될 함수.
    /// </summary>
    /// <param name="alreayHasErrorCB">이전에 이미 에러처리 콜백이 돼었는지 여부</param>
    public void ProcessErrorPacket(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket, bool alreayHasErrorCB)
	{
		if (!alreayHasErrorCB && _errorType == ZWebCommunicator.E_ErrorType.Receive)
		{
			ShowErrorPopup(_recvPacket.ErrCode);
		}
	}

	/// <summary> 크리터컬 에러 여부 </summary>
	public static bool IsCriticalError(WebNet.ERROR errorCode)
	{
		return (uint)errorCode < 10000 && errorCode > 0;
	}

	/// <summary> <see cref="WebNet.ERROR"/> 에러 코드 대응 팝업 띄우기 </summary>
	public static void ShowErrorPopup(WebNet.ERROR _errorCode, UnityAction<UIPopupBase> _btnCB = null)
	{
		if (_errorCode == WebNet.ERROR.NO_ERROR)
			return;

		UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
		{
			/*
			 * 중요 : 에러코드 10000미만은 크리티컬로 앱 종료필요
			 */
			bool isCriticalError = IsCriticalError(_errorCode); ;

			string content = isCriticalError
			? DBLocale.GetText(_errorCode.ToString()) + (UnityEngine.Debug.isDebugBuild ? $"\nErrorCode : {_errorCode}" : string.Empty)
			: DBLocale.GetText(_errorCode.ToString());

			_popup.Open(
				DBLocale.GetText(ZUIString.CRITICAL_ERROR),
				content,
				new string[]
				{
						ZUIString.LOCALE_OK_BUTTON
				},
				new Action[]
				{
						delegate
						{
							if (isCriticalError)
							{
								ZGameManager.Instance.QuitApp();
							}
							_btnCB?.Invoke(_popup);
							_popup.Close();
						}
				});
		});
	}

	private void OnIndicatorStatus(ZWebCommunicator.E_IndicatorStatus status)
	{
		if (null == UIManager.Instance)
		{
			ZLog.LogWarn(ZLogChannel.UI, $"UIManager instance가 존재하지 않아서 표시불가 IndicatorStatus: {status}");
			return;
		}

		switch (status)
		{
			case ZWebCommunicator.E_IndicatorStatus.SHOW:
				UIManager.Instance.ShowGlobalIndicator(true);
				break;
			case ZWebCommunicator.E_IndicatorStatus.HIDE:
				UIManager.Instance.ShowGlobalIndicator(false);
				break;
			case ZWebCommunicator.E_IndicatorStatus.RESET:
				UIManager.Instance.ShowGlobalIndicator(false, true);
				break;
		}

	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();

		DisconnectAll();
	}

    #region FileDownload

    //단순 파일 다운로드 용
    public void DownloadFile(string url, Action<UnityEngine.Networking.DownloadHandler> onLoadEnd)
    {
        StartCoroutine(CoDownloadFile(url, onLoadEnd));
    }

    private IEnumerator CoDownloadFile(string url, Action<UnityEngine.Networking.DownloadHandler> onLoadEnd)
    {
        int rnd = UnityEngine.Random.Range(0, 1000000);

#if UNITY_EDITOR
        url = url.Replace("file:///", "file://");
#endif
        ZLog.Log(ZLogChannel.Default, $"파일 다운로드 시작 : {url}");

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url + "?p=" + rnd))
        {
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError || www.error != null)
            {
                ZLog.Log(ZLogChannel.Default, $"파일 다운로드 에러남 error : {www.error}");
            }
            else
            {
                onLoadEnd?.Invoke(www.downloadHandler);
            }
        }
    }

    #endregion
}
