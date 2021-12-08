using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using WebNet; // protocol

namespace ZNet
{
	/// <summary>
	/// <see cref="UnityWebRequest"/>기반 통신 도구
	/// </summary>
	public abstract class ZWebRequestClientBase : ZWebClientBase
	{
		public enum NetErrorType
		{
			/// <summary> HTTP 요청 관련 에러 </summary>
			WebRequest,
			/// <summary> Gateway WebServer에서 패킷 처리 에러 발생 했을때 </summary>
			Packet,
		}

		public delegate void WebRequestErrorDelegate(NetErrorType errorType, UnityWebRequest request, ZWebRecvPacket recvPacket);

		#region :: Signle Requester ::

		public Coroutine RequestWebData(string _url, ZWWWPacket _reqPacket, ReceiveCBDelegate _onRecvSuccess, WebRequestErrorDelegate _onNetError)
		{
			return StartCoroutine(RequestWeb(_url, _reqPacket, _onRecvSuccess, _onNetError));
		}

		IEnumerator RequestWeb(string _url, ZWWWPacket _reqPacket, ReceiveCBDelegate _onRecvSuccess, WebRequestErrorDelegate _onNetError)
		{
			using (UnityWebRequest www = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST))
			{
				UploadHandlerRaw handler = new UploadHandlerRaw(_reqPacket.GetBytes());
				handler.contentType = "application/octet-stream";
				www.uploadHandler = handler;
				www.downloadHandler = new DownloadHandlerBuffer();

				yield return www.SendWebRequest();

				if (www.isHttpError || www.isNetworkError)
				{
					ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"[{SocketType}] RecvErrorType[{NetErrorType.WebRequest}] | {_url} ({www.error})");

					_onNetError?.Invoke(NetErrorType.WebRequest, www, null);
				}
				else if (www.isDone)
				{
					// 기본 Parsing 처리
					ZWebRecvPacket recvPacket = new ZWebRecvPacket(www.downloadHandler.data);
					if (recvPacket.ErrCode == ERROR.NO_ERROR)
					{
						_onRecvSuccess?.Invoke(recvPacket);
					}
					else
					{
						ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"[{SocketType}] RecvErrorType[{NetErrorType.Packet}] | ID: {recvPacket.ID}, ErrorCode: {recvPacket.ErrCode}");

						_onNetError?.Invoke(NetErrorType.Packet, www, recvPacket);
					}
				}
			}
		}

		public override void SendPacket(ZWebReqPacketBase _reqPacket, ReceiveCBDelegate _onReceive, PacketErrorCBDelegate _packetErrCB)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}