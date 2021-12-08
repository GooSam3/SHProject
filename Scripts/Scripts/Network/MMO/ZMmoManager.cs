using Fun;
using System.Collections.Generic;
using UnityEngine;
using Zero;
using static ZNet.MmoSessionBase;

public enum E_MmoType
{
	Field,
}

/// <summary>
/// 
/// </summary>
public class ZMmoManager : Singleton<ZMmoManager>
{
	public ZNet.ZMmoField Field { get; private set; }

    public bool IsConnected { get { return Field.IsConnected; } }

    private Dictionary<E_MmoType, ZNet.MmoSessionBase> mDicClient = new Dictionary<E_MmoType, ZNet.MmoSessionBase>();

    private float mRestTime = 0f;
    public uint GameTick { get; private set; } = 0;
    private float mInterval = 0f;

    protected override void Init()
	{
		base.Init();

		/*
		 * 네트워크 통신기 준비
		 */
		Field = gameObject.GetOrAddComponent<ZNet.ZMmoField>();
		Field.SessionEventCalled += OnSessionEvent;
		Field.TransportEventCalled += OnTransportEvent;
		Field.TransportErrorCalled += OnTransportError;

		mDicClient.Add(E_MmoType.Field, Field);
	}

    public void SetGameTick(uint tick)
    {
        GameTick = tick;
    }

    public void SetGameTick(uint tick, float interval)
    {
        mRestTime = 0f;
        GameTick = tick;
        mInterval = interval;
    }

    private void Update()
    {
        if (mInterval == 0 || GameTick == 0)
            return;

        mRestTime += Time.unscaledDeltaTime;

        //while (mRestTime > mInterval)
        //{
        //    GameTick += 1;
        //    mRestTime -= mInterval;
        //}

        uint time = (uint)(mRestTime * 1000f);
        uint interval = (uint)(mInterval * 1000f);
        uint tick = (uint)(mRestTime / mInterval);

        GameTick += tick;
        mRestTime -= (mInterval * tick);
    }

    public void SetupConnectSetting(ConnectionSetting _newSetting)
	{
		Field.SetupConnectSetting(_newSetting);
	}

	/// <summary> </summary>
	/// <param name="_newSetting">정보가 존재하면 사용하고, 아니라면 미리 설정된 값 사용.</param>
	public void ConnectField(ConnectionSetting? _newSetting)
	{
		if (_newSetting.HasValue)
		{
			this.SetupConnectSetting(_newSetting.Value);
		}

		Field.Connect();
	}

	public bool Disconnect(E_MmoType _mmoType = E_MmoType.Field)
	{
		bool result = false;

		if (mDicClient.TryGetValue(_mmoType, out var netClient))
		{
			netClient.Disconnect();
			result = true;
		}

		return result;
	}

	public void DisconnectAll()
	{
		foreach (var client in mDicClient)
		{
			client.Value.Disconnect();
		}
	}

	/// <summary> 세션의 상태가 변경될 때마다 이벤트 알림 받음</summary>
	private void OnSessionEvent(SessionEventType eventType, string sessionId)
	{
		Debug.Log($"[{nameof(ZMmoManager.OnSessionEvent)}] EventType: {eventType}, SessionID: {sessionId}");

		switch (eventType)
		{
			case SessionEventType.kOpened:
				// 세션이 처음 연결되면 호출됩니다. 같은 세션으로 재연결시에는 호출되지 않습니다.
				break;

			case SessionEventType.kConnected:
				{
					// 모든 Transport의 연결이 완료되면 호출됩니다.
				}
				break;

			case SessionEventType.kStopped:
				{
					// 세션에 연결된 모든 Transport의 연결이 끊기면 호출됩니다.
				}
				break;

			case SessionEventType.kClosed:
				{
					UIManager.Instance.ShowGlobalIndicator(false, true);

					// 세션이 닫히면 호출됩니다. Transport의 연결이 끊겼다고 세션이 닫히는 것은 아닙니다.
					// 세션은 서버에서 명시적으로 Close가 호출되거나 세션 타임아웃이 되었을 때 종료됩니다.
					string content = $"MMO서버와 연결이 끊겼습니다. 게임을 종료하겠습니다.";
					UICommon.OpenSystemPopup((UIPopupSystem _popup)=> {
						_popup.Open(ZUIString.ERROR, content, new string[] { "확인" }, new System.Action[] { delegate { ZGameManager.Instance.QuitApp(); } });
					});
				}
				break;

			case SessionEventType.kRedirectStarted:
			case SessionEventType.kRedirectSucceeded:
			case SessionEventType.kRedirectFailed:
				break;
		}
	}

	/// <summary> Transport의 연결 상태에 따른 이벤트 받음 </summary>
	private void OnTransportEvent(TransportProtocol protocol, TransportEventType eventType)
	{
		Debug.Log($"[{nameof(ZMmoManager.OnTransportEvent)}] Protocol: {protocol}, EventType: {eventType}, LastError: {Field.Session?.GetLastError(protocol)}");

		switch (eventType)
		{
			case TransportEventType.kStarted:
				{
					// 서버와 연결이 완료되면 호출됩니다.
					// 케이스 : 일반 연결 & 재접속 연결시

					UIManager.Instance.ShowGlobalIndicator(false);
				}
				break;
			case TransportEventType.kStopped:
				// 서버와 연결이 종료되거나 연결에 실패하면 호출됩니다.
				//
				// 어떤 이유로 연결이 끊겼던지 Transport 이벤트는 kStopped 하나만 호출됩니다. 
				// 자세한 종료 사유에 대해서는 로그로 확인하거나 FunapiSession.GetLastError 로 확인할 수 있습니다.
				break;
			case TransportEventType.kReconnecting:
				{
					// 재연결 시작시 호출됩니다.
					// 연결이 끊겨서 직접 Connect를 호출할 때에는 발생하지 않습니다.
					// AutoReconnect 옵션으로 인해 재연결을 시도할 때에만 호출됩니다.

					// 재접속 시작시, 성공할때까지 대기&인풋 막기.
					UIManager.Instance.ShowGlobalIndicator(true);
				}
				break;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="error">
	/// [TransportError.Type]
	/// kStartingFailed,      // Transport 초기화에 실패했을 때 호출됩니다.
	/// kConnectionTimeout,   // 연결 제한시간을 초과했을 경우에 호출됩니다.
	/// kEncryptionFailed,    // 메시지 암호화에 실패했을 때 호출됩니다.
	/// kSendingFailed,       // 메시지를 보내는 과정에서 오류가 발생했을 때 호출됩니다.
	/// kReceivingFailed,     // 메시지를 받는 과정에서 오류가 발생했을 때 호출됩니다.
	/// kRequestFailed,       // HTTP 요청에 실패했을 때 호출됩니다.
	/// kWebsocketError,      // Websocket에서 오류가 발생했을 때 호출됩니다.
	/// kDisconnected         // 예기치 않게 서버와의 연결이 끊겼을 때 호출됩니다.
	/// </param>
	private void OnTransportError(TransportProtocol protocol, TransportError error)
	{
		Debug.Log($"[{nameof(ZMmoManager.OnTransportError)}] Protocol: {protocol}, Error: {error.type} | {error.message}");
	}
}
