using FlatBuffers;
using Fun;
using funapi.network.fun_message;
using icarus_mmo_messages;
using UnityEngine;

namespace ZNet
{
	public abstract class MmoSessionBase : MonoBehaviour
	{
		// TODO : 필요하다면 ScriptableObject 로 만들어서 가져오기하던지 하자.
		[System.Serializable]
		public struct ConnectionSetting
		{
			public string Address;
			public ushort Port;
			public TransportProtocol Protocol;
			public FunEncoding Encoding;

			public ConnectionSetting(string _addr, ushort _port)
			{
				this.Address = _addr;
				this.Port = _port;
				this.Protocol = TransportProtocol.kTcp;
				this.Encoding = FunEncoding.kProtobuf;
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(Address) && Port != 0;
			}
		}

		/// <summary> [Session] 최상위 메시지 전송 예약어 </summary>
		public const string SessionMessageTypeName = "OnToFlatBuffer";

		/// <summary> </summary>
		public string Address => mConnSetting.Address;
		/// <summary> </summary>
		public ushort Port => mConnSetting.Port;
		/// <summary> 클라-서버 통신 프로토콜 tcp, udp, http </summary>
		public TransportProtocol Protocol => mConnSetting.Protocol;
		/// <summary> 클라-서버 통신에 사용될 메시지 포맷 </summary>
		public FunEncoding Encoding => mConnSetting.Encoding;

		/// <summary>
		/// 
		/// </summary>
		public FunapiSession Session { get; private set; }

		/// <summary> 메시지 처리기 </summary>
		protected abstract MmoReceiverBase Receiver { get; }

		/// <summary>
		/// 실 메시지 처리 이벤트
		/// </summary>
		public event System.Action<FlatMsg> FBMessageReceived;

		public event System.Action<SessionEventType, string> SessionEventCalled;
		public event System.Action<TransportProtocol, TransportEventType> TransportEventCalled;
		public event System.Action<TransportProtocol, TransportError> TransportErrorCalled;

		public string SessionID
		{
			get { return null != Session ? Session.GetSessionId() : string.Empty; }
		}

		/// <summary> 연결돼있고 통신 가능한지 여부 </summary>
		public virtual bool IsConnected
		{
			get { return null != Session ? Session.Connected : false; }
		}

		public SessionOption SessOption => mSessionOption;
		public TransportOption TransOption => mTransportOption;

		/// <summary> 재사용 용도 </summary>
		protected FlatBufferBuilder mFBuilder = new FlatBufferBuilder(16);

		[SerializeField]
		private ConnectionSetting mConnSetting = new ConnectionSetting(string.Empty, 0);
		private SessionOption mSessionOption = new SessionOption();
		private TransportOption mTransportOption = new TcpTransportOption();

		private void Awake()
		{
			Initialize();
		}

		protected virtual void Initialize()
		{
			mFBuilder.Clear();
			EditSessionOption(ref mSessionOption);
			EditTransportOption(ref mTransportOption);
		}

		/// <summary> 내부 <see cref="Session"/> 생성에 필요한 옵션값 할당 작업 </summary>
		protected virtual void EditSessionOption(ref SessionOption _refOption)
		{
			// 메시지 순서를 보장합니다. 재연결시에도 이전 메시지와의 순서를 동기화합니다.
			_refOption.sessionReliability = true;
			// 기본적으로 모든 메시지에 session id 값이 포함되며 이 옵션을 true 로 변경할 경우
			// 처음 연결시에만 session id 를 보내고 그 이후 메시지에는 session id 를 포함하지 않습니다.
			_refOption.sendSessionIdOnlyOnce = false;
			// sessionReliability 옵션을 사용할 경우 서버와 ack 메시지를 주고 받는데
			// 이 옵션을 사용할 경우 piggybacking, delayed sending 등을 통해 네트워크 트래픽을 줄여줍니다.
			// 옵션 값은 ack를 보내는 간격에 대한 시간 값이며 초단위 값을 사용합니다.
			// 시간 값이 0f 보다 클 경우 piggybacking 은 자동으로 이루어집니다.
			_refOption.delayedAckInterval = 0;
		}

		/// <summary> 전송 단계에 필요한 옵션값 할당 작업 </summary>
		protected virtual void EditTransportOption(ref TransportOption _refOption)
		{
			var tcpOption = new TcpTransportOption();

			// 서버와 연결할 때 Timeout 될 시간을 지정합니다.
			// 기본 값은 10초이며, 0을 입력할 경우 Timeout 처리를 하지 않고 계속 연결을 시도합니다.
			// 이 경우, 서버로부터 응답이 오지 않으면 무한히 대기하는 상황이 발생할 수 있기 때문에
			// 디버깅 목적으로만 사용하기를 권장합니다.
			// Timeout에 설정한 시간을 초과하면 연결 시도를 중단하고, kStopped 이벤트를 발생시킵니다.
			tcpOption.ConnectionTimeout = 20f;
			// 이 값이 true일 경우 연결에 실패하거나 연결이 끊겼을 경우 재연결을 시도합니다.
			// 재시도 Timeout을 정하고 싶다면 ConnectionTimeout 값을 입력하면 됩니다.
			// ConnectionTimeout 시간을 초과하면 재시도를 중단하고 kStopped 이벤트가 호출됩니다.
			tcpOption.AutoReconnect = true;

			_refOption = tcpOption;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newSetting"></param>
		public virtual void SetupConnectSetting(ConnectionSetting newSetting)
		{
			this.mConnSetting = newSetting;
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Connect()
		{
			if (!mConnSetting.IsValid())
			{
				Debug.LogError($"접속 정보 설정이 필요합니다! call {nameof(SetupConnectSetting)}()");
				return;
			}

			if (null == Session)
			{
				Session = FunapiSession.Create(this.Address, SessOption);
			}

			if (Session.Started || Session.Connected)
			{
				Debug.LogError($"이미 연결 시도중이거나 연결된 상태! Started: {Session.Started}, Connected: {Session.Connected}");
				return;
			}

			UnRegisterInternalCallbacks();
			RegisterInternalCallbacks();

			Session.Connect(this.Protocol, this.Encoding, this.Port, this.TransOption);

			ZLog.Log(ZLogChannel.System, $"{GetType().Name}.Connect() | {this.Address}:{this.Port}");
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Disconnect()
		{
			Session?.Stop();
			Session = null;
		}

		public virtual void DestroySession()
		{
			FunapiSession.Destroy(this.Session);
		}

		private void RegisterInternalCallbacks()
		{
			if (null == Session)
				return;

			Session.SessionEventCallback += OnSessionEvent;
			Session.TransportEventCallback += OnTransportEvent;
			Session.TransportErrorCallback += OnTransportError;
			Session.ReceivedMessageCallback += OnReceivedMessage;
			Session.DroppedMessageCallback += OnDroppedMessage;
		}

		private void UnRegisterInternalCallbacks()
		{
			if (null == Session)
				return;

			Session.SessionEventCallback -= OnSessionEvent;
			Session.TransportEventCallback -= OnTransportEvent;
			Session.TransportErrorCallback -= OnTransportError;
			Session.ReceivedMessageCallback -= OnReceivedMessage;
			Session.DroppedMessageCallback -= OnDroppedMessage;
		}

		/// <summary> 세션의 상태가 변경될 때마다 이벤트 알림 받음</summary>
		private void OnSessionEvent(SessionEventType eventType, string sessionId)
		{
			//Debug.Log($"[{nameof(OnSessionEvent)}] EventType: {eventType}, SessionID: {sessionId}");

			//switch (eventType)
			//{
			//	case SessionEventType.kOpened:
			//		// 세션이 처음 연결되면 호출됩니다. 같은 세션으로 재연결시에는 호출되지 않습니다.
			//		break;

			//	case SessionEventType.kConnected:
			//		// 모든 Transport의 연결이 완료되면 호출됩니다.
			//		break;

			//	case SessionEventType.kStopped:
			//		// 세션에 연결된 모든 Transport의 연결이 끊기면 호출됩니다.
			//		break;

			//	case SessionEventType.kClosed:
			//		// 세션이 닫히면 호출됩니다. Transport의 연결이 끊겼다고 세션이 닫히는 것은 아닙니다.
			//		// 세션은 서버에서 명시적으로 Close가 호출되거나 세션 타임아웃이 되었을 때 종료됩니다.
			//		break;

			//	case SessionEventType.kRedirectStarted:
			//	case SessionEventType.kRedirectSucceeded:
			//	case SessionEventType.kRedirectFailed:
			//		break;
			//}

			SessionEventCalled.Invoke(eventType, sessionId);
		}

		/// <summary> Transport의 연결 상태에 따른 이벤트 받음 </summary>
		private void OnTransportEvent(TransportProtocol protocol, TransportEventType eventType)
		{
			//Debug.Log($"[{nameof(OnTransportEvent)}] Protocol: {protocol}, EventType: {eventType}");

			//switch (eventType)
			//{
			//	case TransportEventType.kStarted:
			//		// 서버와 연결이 완료되면 호출됩니다.
			//		break;
			//	case TransportEventType.kStopped:
			//		// 서버와 연결이 종료되거나 연결에 실패하면 호출됩니다.
			//		//
			//		// 어떤 이유로 연결이 끊겼던지 Transport 이벤트는 kStopped 하나만 호출됩니다. 
			//		// 자세한 종료 사유에 대해서는 로그로 확인하거나 FunapiSession.GetLastError 로 확인할 수 있습니다.
			//		break;
			//	case TransportEventType.kReconnecting:
			//		// 재연결 시작시 호출됩니다.
			//		// 연결이 끊겨서 직접 Connect를 호출할 때에는 발생하지 않습니다.
			//		// AutoReconnect 옵션으로 인해 재연결을 시도할 때에만 호출됩니다.
			//		break;
			//}

			TransportEventCalled.Invoke(protocol, eventType);
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
			//Debug.Log($"[{nameof(OnTransportError)}] Protocol: {protocol}, Error: {error.type} | {error.message}");

			TransportErrorCalled.Invoke(protocol, error);
		}

		private void OnReceivedMessage(string messageType, object message)
		{
			//Debug.Log($"[{nameof(OnReceivedMessage)}] MessageType: {messageType}, Message: {message}");

			if (messageType == SessionMessageTypeName)
			{
				var funMessage = message as FunMessage;
				var flatbufferPacket = FunapiMessage.GetMessage<ToFlatBuffer>(funMessage, MessageType.TOFLATBUFFER);

				int msgCount = flatbufferPacket.msglist.Count;
				for (int i = 0; i < msgCount; ++i)
				{
					FlatMsg flatMsg = flatbufferPacket.msglist[i];

					OnReceivedFBMessage(flatMsg);
				}
			}
			else
			{
				FunDebug.LogError("not registered MessageType: {0}, Message: {1}", messageType, message);
			}
		}

		/// <summary></summary>
		/// <remarks> 
		/// 메시지를 보낼 수 없는 상태에서 메시지 전송 시 해당 메시지는 전송되지 않고 버려집니다. 
		/// 이 때 DroppedMessageCallback 이 등록되어 있다면 이 콜백으로 버려진 메시지를 전달
		/// </remarks>
		private void OnDroppedMessage(string messageType, object message)
		{
		}

		protected void OnReceivedFBMessage(FlatMsg _flatMsg)
		{
			//Debug.Log($"OnReceivedFBMessage() : MmoNet.MSG.{(MmoNet.MSG)_flatMsg.msgtype}");

			if (null != Receiver)
			{
				FBMessageReceived?.Invoke(_flatMsg);
			}
			else
			{
				FunDebug.LogError("not exist Receiver | MmoNet.MSG: {0}", (MmoNet.MSG)_flatMsg.msgtype);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_builder">사용후 내부에서 Clear() 호출함.</param>
		public void SendMessage(MmoNet.MSG msgType, FlatBufferBuilder _builder)
		{
			SendMessage((int)msgType, _builder.SizedByteArray());
			// 사용후 Clear()
			_builder.Clear();
		}

		private void SendMessage(int _messageId, byte[] _sendBytes)
		{
			FlatMsg flatMsg = new FlatMsg()
			{
				msgtype = (int)_messageId,
				array = _sendBytes
			};

			ToFlatBuffer packet = new ToFlatBuffer();
			packet.tick = (uint)Time.frameCount;
			packet.msglist.Add(flatMsg);

			FunMessage message = FunapiMessage.CreateFunMessage(packet, MessageType.TOFLATBUFFER);

			// 실서버로 전송!
			this.Session?.SendMessage(SessionMessageTypeName, message);
		}
	}
}