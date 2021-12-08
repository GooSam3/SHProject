using BestHTTP.WebSocket;
using System;

namespace ZNet
{
	/// <summary>
	/// 
	/// </summary>
	public class ZWebSocket
	{
		/// <summary> MilliSeconds </summary>
#if UNITY_EDITOR
		public static int PingFrequency = 25000; // Editor환경에서 로딩시 멈춰서 끊기는거 같음.
#else
		public static int PingFrequency = 5000;
#endif

		/// <summary>
		/// 
		/// </summary>
		public string ConnectionUrl { get; protected set; }


		public System.Action<ZWebSocket> OnConnectOpened;
		public System.Action<ZWebSocket> OnConnectClosed;
		public System.Action<ZWebSocket, string> OnRecvMessages;
		public System.Action<ZWebSocket, byte[]> OnRecvBytes;
		public System.Action<ZWebSocket, string> OnErrored;

		protected WebSocket mSocket = null;

		public virtual bool IsOpened
		{
			get
			{
				return mSocket != null && mSocket.IsOpen;
			}
		}

		public virtual bool IsConnecting
		{
			get
			{
				return mSocket != null && mSocket.IsOpen && mSocket.State == WebSocketStates.Connecting;
			}
		}

		public WebSocketStates State
		{
			get
			{
				return null == mSocket ? WebSocketStates.Unknown : mSocket.State;
			}
		}

		public virtual void Connect(string url)
		{
			this.ConnectionUrl = url;
			if (mSocket != null && mSocket.IsOpen)
			{
				mSocket.Close();
				mSocket = null;
			}

			Uri uri = new Uri(url);
			mSocket = new WebSocket(uri);

			mSocket.OnOpen += OnOpen;
			mSocket.OnClosed += OnClosed;
			mSocket.OnBinary += OnMessageReceive;
			mSocket.OnError += OnError;
#if !UNITY_WEBGL
			mSocket.PingFrequency = PingFrequency;
			mSocket.StartPingThread = true;
#endif
			mSocket.Open();
		}

		public virtual void Close()
		{
			ClearEvents();

			if (mSocket != null && mSocket.IsOpen)
			{
				mSocket.Close();
				mSocket = null;
			}
			else
			{
				mSocket = null;				
			}
		}

		private void ClearEvents()
		{
			OnConnectOpened = null;
			OnConnectClosed = null;
			OnRecvBytes = null;
			OnErrored = null;
		}

		protected void OnOpen(WebSocket webSocket)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{ConnectionUrl}] OnOpen");

			OnConnectOpened?.Invoke(this);
		}

		protected void OnClosed(WebSocket webSocket, UInt16 code, string message)
		{
			ZLog.Log(ZLogChannel.WebSocket, $"[{ConnectionUrl}] OnClosed | StatusCode: {(WebSocketStausCodes)code}, message: {message}");

			OnConnectClosed?.Invoke(this);
		}

		protected void OnMessageReceive(WebSocket webSocket, byte[] bytedatas)
		{
			OnRecvBytes?.Invoke(this, bytedatas);
		}

		protected void OnError(WebSocket webSocket, string reason)
		{
			try
			{
				OnErrored?.Invoke(this, reason);
			}
			catch (Exception e)
			{
				ZLog.Log(ZLogChannel.WebSocket, $"OnError : {e.Message}" + e.Message);
			}
		}
		
		/// <summary></summary>
		public virtual void SendByBytes(byte[] _msgBytes)
		{
			mSocket?.Send(_msgBytes);
		}
	}
}