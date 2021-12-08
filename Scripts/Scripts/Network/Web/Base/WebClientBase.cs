using FlatBuffers;
using System;
using System.Collections;
using UnityEngine;

namespace ZNet
{
	/// <summary> </summary>
	public delegate void ReceiveCBDelegate(ZWebRecvPacket recvPacket);
	/// <summary> 패킷 전송관련 에러 발생시 콜백용 delegate </summary>
	public delegate void PacketErrorCBDelegate(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket);

	/// <summary>
	/// <see cref="FlatBuffers"/>를 프로토콜로 사용하는 웹 통신 클래스
	/// </summary>
	/// <typeparam name="REQ_PACKET_TYPE"></typeparam>
	/// <typeparam name="RECV_PACKET_TYPE"></typeparam>
	public abstract class WebClientBase<REQ_PACKET_TYPE, RECV_PACKET_TYPE> : MonoBehaviour
	{
		/// <summary>
		/// 통신 사용가능한지 여부
		/// </summary>
		public abstract bool IsUsable { get; }

		/// <summary> 통신 보안용 키 </summary>
		public abstract uint SecurityKey { get; protected set; }

		/// <summary> 정보 얻기 위한 서버 주소 (설정 필수) </summary>
		public string ServerUrl
		{
			set { SetServerURL(value); }
			get { return mServerUrl; }
		}

		protected FlatBufferBuilder mBuilder = new FlatBufferBuilder(16);

		private string mServerUrl;

		/// <summary> 연결용 서버 주소 설정 </summary>
		protected virtual void SetServerURL(string _serverUrl)
		{
			// Http, Https 모두 유효한 것으로 간주
			bool result = Uri.TryCreate(_serverUrl, UriKind.RelativeOrAbsolute, out var uriResult) 
				&& (uriResult.Scheme == "ws" || uriResult.Scheme == "wss" || uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			if (result)
			{
				this.mServerUrl = _serverUrl;
			}
			else
			{
				Debug.LogError($"유효하지 않은 Url({_serverUrl})을 설정하려고 했습니다.");
			}

			ZLog.Log(ZLogChannel.WebSocket, $"[{GetType().Name}] SetServerURL : {_serverUrl}");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class ZWebClientBase : WebClientBase<ZWebReqPacketBase, ZWebRecvPacket>
	{
		public override bool IsUsable => (null != mWebSocket && mWebSocket.IsOpened) && null != mCommunicator;

		/// <summary>
		/// 재접속 시도중인지 여부 
		/// </summary>
		public bool IsReconnecting => mReconnectStartTime > Mathf.Epsilon && CurReconnectNumber != 0;

		public abstract E_WebSocketType SocketType { get; }

		///// <summary> 사용중인 WebSocket </summary>
		public ZWebSocket Socket => mWebSocket;

		/// <summary> 자동 재접속 작동 여부 </summary>
		public virtual bool AutoReconnect { get; set; } = false;
		/// <summary> 재접속 최대 대기 시간 (대기 시간끝날때까지 계속 재접속 시도함) </summary>
		public virtual float MaxReconnectTimeout { get; protected set; } = 20f;

		public System.Action<ZWebClientBase> ConnectOpened;
		public System.Action<ZWebClientBase> ConnectClosed;
		public System.Action<ZWebClientBase, byte[]> BytesReceived;
		public System.Action<ZWebClientBase, string> ErrorOccurred;
		/// <summary>재접속 처리까지 실패했을때 발생하는 이벤트</summary>
		public System.Action<ZWebClientBase> ErrorReconnected;

		protected ZWebSocket mWebSocket;
		protected ZWebCommunicator mCommunicator;

		/// <summary> 마지막으로 사용한 패킷 번호 *주의* 0번은 응답 못받음. </summary>
		private ulong mLatestPacketNumber = 1;

		private float mReconnectStartTime = 0;
		public uint CurReconnectNumber { get; private set; } = 0;
		private Coroutine mReconnectRoutine = null;

		protected virtual void Awake()
		{
			mWebSocket = new ZWebSocket();
		}

		protected virtual void OnDestroy()
		{
			if (null != mCommunicator)
			{
				mCommunicator?.RemoveClient(this);
			}
		}

		public virtual void Connect(string _serverUrl)
		{
			if  (mWebSocket.IsOpened)
			{
				ZLog.LogWarn(ZLogChannel.WebSocket, $"[{SocketType}] Aleary IsOpened: { mWebSocket.IsOpened}, IsConnecting: { mWebSocket.IsConnecting}, State: { mWebSocket.State}");
				return;
			}

			if (mWebSocket.IsConnecting)
			{
				ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Warning, $"IsOpened: {mWebSocket.IsOpened}, IsConnecting: {mWebSocket.IsConnecting}, State: {mWebSocket.State}");
				Disconnect();
			}

			SetServerURL(_serverUrl);

			mCommunicator = ZWebCommunicator.Instance;
			if (null != mCommunicator)
			{
				mCommunicator.AddClient(this);
			}

			mWebSocket.OnConnectOpened = OnSocket_ConnectOpened;
			mWebSocket.OnConnectClosed = OnSocket_ConnectClosed;
			mWebSocket.OnRecvBytes = OnSocket_RecvBytes;
			mWebSocket.OnErrored = OnSocket_Error;
			mWebSocket.Connect(_serverUrl);

			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] Connect : {_serverUrl}");
		}

		public virtual void Disconnect()
		{
			mWebSocket?.Close();
			mCommunicator?.ClearDatas(SocketType);

			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] Try Disconnect : {ServerUrl}");
		}

		/// <summary> 패킷 번호 생성 </summary>
		public ulong NextPacketNumber()
		{
			return mLatestPacketNumber++;
		}

		protected virtual void OnSocket_ConnectOpened(ZWebSocket socket)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] OnSocket_ConnectOpened");

			ClearReconnectInfo();

			ConnectOpened?.Invoke(this);
		}

		protected virtual void OnSocket_ConnectClosed(ZWebSocket socket)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] OnSocket_ConnectClosed");

			ConnectClosed?.Invoke(this);
		}

		protected void OnSocket_RecvBytes(ZWebSocket socket, byte[] recvBytes)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}].{nameof(OnSocket_RecvBytes)}() RecvBytes: {recvBytes.Length}");

			BytesReceived?.Invoke(this, recvBytes);
		}

		protected virtual void OnSocket_Error(ZWebSocket socket, string reason)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] OnSocket_Error | Reason: {reason}, State: {socket.State}");

			ErrorOccurred?.Invoke(this, reason);

			// 재접속 시도 처리
			if (AutoReconnect && !string.IsNullOrEmpty(ServerUrl))
			{
				if (null != mReconnectRoutine)
					StopCoroutine(mReconnectRoutine);
				mReconnectRoutine = StartCoroutine(E_Reconnect());
			}
		}

		private IEnumerator E_Reconnect()
		{
			// 최대 대기 시간 넘어가면 Fatal에러 발생
			if (CurReconnectNumber != 0 &&
				Time.realtimeSinceStartup - mReconnectStartTime > MaxReconnectTimeout)
			{
				ErrorReconnected?.Invoke(this);
				ClearReconnectInfo();

				ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] 재접속 제한 시간 초과! | Timeout: {MaxReconnectTimeout}, CurReconnectNumber: {CurReconnectNumber}");
			}
			else
			{				
				if (0 == CurReconnectNumber)
					mReconnectStartTime = Time.realtimeSinceStartup;
				else
				{
					// 재접속 루틴을 시작했다면, 최대 1초마다 재시도 하도록 하자.
					yield return new WaitForSeconds(3f);
				}
				
				this.Connect(ServerUrl);
				this.CurReconnectNumber++;

				ZLog.Log(ZLogChannel.WebSocket, $"[{SocketType}] 재접속 시도 | Url: {ServerUrl}, CurReconnectNumber: {CurReconnectNumber}");
			}
		}

		private void ClearReconnectInfo()
		{
			mReconnectStartTime = 0;
			CurReconnectNumber = 0;
		}

		/// <summary> </summary>
		public abstract void SendPacket(ZWebReqPacketBase _reqPacket, ReceiveCBDelegate _onReceive, PacketErrorCBDelegate _packetErrCB);
	}
}
