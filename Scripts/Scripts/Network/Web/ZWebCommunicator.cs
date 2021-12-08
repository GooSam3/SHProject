using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebNet;
using Zero;

namespace ZNet
{
	/// <summary>
	/// <see cref="WebClientBase{REQ_PACKET_TYPE, RECV_PACKET_TYPE}"/>기반 송/수신 처리 관리
	/// </summary>
	public class ZWebCommunicator : Singleton<ZWebCommunicator>
	{
		public enum E_ErrorType
		{
			/// <summary> 어떤 이유로 인해 전송 실패가 발생했을 경우 </summary>
			SendFailure,
			/// <summary> 이미 동일 패킷이 전송된 상태이다. 처리되기전에 다시 전송했을때 발생 </summary>
			SendDuplication,
			Receive,
		}

		public enum E_IndicatorStatus
		{
			SHOW,
			HIDE,
			RESET,
		}

		/// <summary> 현재 사용중인 통신 클라이언트들 모음 </summary>
		protected Dictionary<E_WebSocketType, ZWebClientBase> DicWebClient = new Dictionary<E_WebSocketType, ZWebClientBase>();

		/// <summary> [SENDER] 소켓별 보내야할 패킷들 </summary>
		Dictionary<E_WebSocketType, List<ZWebReqPacketBase>> DicPendingPacket = new Dictionary<E_WebSocketType, List<ZWebReqPacketBase>>();
		/// <summary> [SENDER] 소켓별 전송한 패킷들 (응답을 받아야 제거)</summary>
		Dictionary<E_WebSocketType, List<ZWebReqPacketBase>> DicSendedPacket = new Dictionary<E_WebSocketType, List<ZWebReqPacketBase>>();
		/// <summary> [SENDER] 패킷별 마지막 전송 시간 기록 </summary>
		Dictionary<WebNet.Code, float> DicLastSendTimeRecord = new Dictionary<WebNet.Code, float>();

		/// <summary> [RECEIVER] Key: { SocketType }, Value : { Key: Number, Value: Callback }</summary>
		Dictionary<E_WebSocketType, Dictionary<ulong, ValueTuple<ReceiveCBDelegate, PacketErrorCBDelegate>>> DicReceiveCallback = new Dictionary<E_WebSocketType, Dictionary<ulong, ValueTuple<ReceiveCBDelegate, PacketErrorCBDelegate>>>();
		/// <summary> [RECEIVER] 서버로부터 받은 패킷들  </summary>
		Devcat.EnumDictionary<E_WebSocketType, Queue<ZWebRecvPacket>> DicReceivedPacket = new Devcat.EnumDictionary<E_WebSocketType, Queue<ZWebRecvPacket>>();

		/// <summary>
		/// 패킷 전송/응답시 에러 발생시 호출될 이벤트. TODO : 아직 제대로 처리안된 상태.
		/// </summary>		
		public Action<E_ErrorType, ZWebReqPacketBase, ZWebRecvPacket, bool> PacketErrorOccured;
		/// <summary> 패킷 전송/응답시 UI표시용 이벤트</summary>
		public Action<E_IndicatorStatus> IndicatorStatusEvent;

		public void AddClient(ZWebClientBase _newClient)
		{
			if (!DicWebClient.ContainsKey(_newClient.SocketType))
			{
				DicWebClient.Add(_newClient.SocketType, _newClient);

				// 이벤트 등록.
				_newClient.ConnectOpened += OnClient_ConnectOpened;
				_newClient.ConnectClosed += OnClient_ConnectClosed;
				_newClient.BytesReceived += OnClient_BytesReceived;
				_newClient.ErrorOccurred += OnClient_ErrorOccurred;
			}
		}

		public bool RemoveClient(ZWebClientBase _removingClient)
		{
			bool result = false;
			if (DicWebClient.TryGetValue(_removingClient.SocketType, out var targetClient))
			{
				// 이벤트 삭제
				targetClient.ConnectOpened -= OnClient_ConnectOpened;
				targetClient.ConnectClosed -= OnClient_ConnectClosed;
				targetClient.BytesReceived -= OnClient_BytesReceived;
				targetClient.ErrorOccurred -= OnClient_ErrorOccurred;

				// 삭제 처리
				result = DicWebClient.Remove(_removingClient.SocketType);
				// 메시지 처리 등등.
			}

			return result;
		}

		/// <summary> Send관련 데이터 클리어 </summary>
		private void ClearSendRelatedData(E_WebSocketType socketType)
		{
			if (DicPendingPacket.TryGetValue(socketType, out var pendingList))
			{
				// 누적된 인디게이터 지우기
				pendingList.ForEach((packet) => 
				{
					if (NeedIndicator(packet.ID))
						IndicatorStatusEvent?.Invoke(E_IndicatorStatus.HIDE);
				});

				pendingList.Clear();
			}
			if (DicSendedPacket.TryGetValue(socketType, out var sendedList))
			{
				// 누적된 인디게이터 지우기
				pendingList.ForEach((packet) => 
				{
					if (NeedIndicator(packet.ID))
						IndicatorStatusEvent?.Invoke(E_IndicatorStatus.HIDE);
				});

				sendedList.Clear();
			}

			DicPendingPacket.Clear();
			DicSendedPacket.Clear();
			DicLastSendTimeRecord.Clear();
		}

		/// <summary> Receive 관련 데이터 클리어 </summary>
		/// <param name="socketType"></param>
		private void ClearReceiveRelatedData(E_WebSocketType socketType)
		{
			if (DicReceivedPacket.TryGetValue(socketType, out var receiveQueue))
			{
				if (receiveQueue.Count > 0)
					ConsumeReceivedPackets();

				receiveQueue.Clear();
			}

			if (DicReceiveCallback.TryGetValue(socketType, out var dicCallback))
			{
				dicCallback.Clear();
			}

			DicReceivedPacket.Clear();
			DicReceiveCallback.Clear();
		}

		private void OnClient_ConnectOpened(ZWebClientBase _webClient)
		{
			//연결 사이에 대기중인 메시지가 있다면 전송
			SendPendingPacket(_webClient.SocketType);
		}

		private void OnClient_ConnectClosed(ZWebClientBase _webClient)
		{
			ClearSendRelatedData(_webClient.SocketType);
		}

		/// <summary> WebClient 소켓 에러 발생시 호출되는 이벤트 함수 </summary>
		private void OnClient_ErrorOccurred(ZWebClientBase _webClient, string reason)
		{
			// 소켓이 망가졌을거라 다 초기화 필요

			var socketType = _webClient.SocketType;
			/*
			 * Send, Receive 관련 데이터 클리어
			 */
			ClearDatas(socketType);
		}

		/// <summary> Send, Receive 관련 데이터 클리어 </summary>
		public void ClearDatas(E_WebSocketType socketType)
		{
			ClearSendRelatedData(socketType);
			ClearReceiveRelatedData(socketType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_socketType"></param>
		/// <param name="_reqPacket"></param>
		/// <param name="_onReceive">현재 패킷에 대해 정상 응답 받았을때 호출될 Callback. Error가 발생한다면 해당 콜백은 호출되지 않음.</param>
		/// <param name="_packetErrCB">현재 패킷에 대해 서버로부터 에러를 받았을때 호출될 Callback</param>
		public void SendPacket(E_WebSocketType _socketType, ZWebReqPacketBase _reqPacket, ReceiveCBDelegate _onReceive, PacketErrorCBDelegate _packetErrCB = null)
		{
			// Receive처리를 위한 요청 패킷 등록해두기.
			RegisterReceiveCallback(_socketType, _reqPacket.Number, _onReceive, _packetErrCB);

			if (IsDirectPacket(_reqPacket.ID))
			{
				if (TrySendPacket(_socketType, _reqPacket))
				{
					AddSendedQueue(_socketType, _reqPacket);

					if (NeedIndicator(_reqPacket.ID))
						IndicatorStatusEvent?.Invoke(E_IndicatorStatus.SHOW);
				}
				else
				{
					// error : 연결이 끊어졌거나, 소켓 미존재
					OnErrorPacket(_socketType, E_ErrorType.SendFailure, _reqPacket, null);
				}
			}
			else
			{
				if (AddPendingPacket(_socketType, _reqPacket))
				{
					if (NeedIndicator(_reqPacket.ID))
						IndicatorStatusEvent?.Invoke(E_IndicatorStatus.SHOW);
				}
				else
				{
					// 전송 대기열 실패
					OnErrorPacket(_socketType, E_ErrorType.SendFailure, _reqPacket, null);
				}
			}
		}

		/// <summary> Communicator단계에서 에러발생시 호출되는 이벤트 </summary>
		/// <param name="_errorType"></param>
		/// <param name="_webSocketType"></param>
		/// <param name="_reqPacket"></param>
		/// <param name="_recvPacket"></param>
		private void OnErrorPacket(E_WebSocketType _socketType, ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
		{
			// 패킷에 대한 에러 콜백을 사용자가 할당(처리)해놨는지 여부
			bool hasErrorCallback = false;

			switch (_errorType)
			{
				case ZWebCommunicator.E_ErrorType.SendFailure:
					{
						if (DicReceiveCallback.TryGetValue(_socketType, out var dicNumberCallback) &&
							dicNumberCallback.TryGetValue(_reqPacket.Number, out var registeredCallback))
						{
							registeredCallback.Item2?.Invoke(E_ErrorType.SendFailure, _reqPacket, _recvPacket);
							hasErrorCallback = null != registeredCallback.Item2;

							// 에러가 났으니 등록되어 있던 콜백 삭제
							dicNumberCallback.Remove(_reqPacket.Number);
						}

						ZLog.Log(ZLogChannel.WebSocket, $"[{_socketType}] 전송 실패 | ErrorType: {_errorType}, ID: {_reqPacket.ID}, Number: {_reqPacket.Number}");
					}
					break;

				case ZWebCommunicator.E_ErrorType.SendDuplication:
					{
						if (DicReceiveCallback.TryGetValue(_socketType, out var dicNumberCallback) &&
								dicNumberCallback.TryGetValue(_reqPacket.Number, out var registeredCallback))
						{
							registeredCallback.Item2?.Invoke(E_ErrorType.SendDuplication, _reqPacket, _recvPacket);
							hasErrorCallback = null != registeredCallback.Item2;

							// 에러가 났으니 등록되어 있던 콜백 삭제
							dicNumberCallback.Remove(_reqPacket.Number);
						}

						ZLog.Log(ZLogChannel.WebSocket, $"[{_socketType}] 이미 동일패킷이 전송중이거나 전송될 예정입니다. 응답대기 상태 (동일 패킷 1개만 보내도록 설정해놨는데, 중복으로 보냈는지 체크바람) | ErrorType: {_errorType}, ID: {_reqPacket.ID}, Number: {_reqPacket.Number}");
					}
					break;

				case ZWebCommunicator.E_ErrorType.Receive:
					{
						if (DicReceiveCallback.TryGetValue(_socketType, out var dicNumberCallback) &&
							dicNumberCallback.TryGetValue(_recvPacket.Number, out var registeredCallback))
						{
							registeredCallback.Item2?.Invoke(E_ErrorType.Receive, _reqPacket, _recvPacket);
							hasErrorCallback = null != registeredCallback.Item2;

							// 에러가 났으니 등록되어 있던 콜백 삭제
							dicNumberCallback.Remove(_recvPacket.Number);
						}

						ZLog.LogWarn(ZLogChannel.WebSocket, $"[{_socketType}] 응답은 받았으나 에러난 패킷 | ErrorType: {_errorType}, ID: {_recvPacket.ID}, <color=red>ErrorCode: {_recvPacket.ErrCode}</color>");
					}
					break;
			}

			PacketErrorOccured?.Invoke(_errorType, _reqPacket, _recvPacket, hasErrorCallback);
		}

		/// <summary> 
		/// 실제 연결된 서버로 해당 패킷 전송! 
		/// </summary>
		private bool TrySendPacket(E_WebSocketType _socketType, ZWebReqPacketBase _reqPacket)
		{
			if (!DicWebClient.TryGetValue(_socketType, out var webClient))
			{
				ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"{_socketType} 에 해당하는 WebClient가 존재하지 않습니다. PacketID: {_reqPacket.ID}");
				return false;
			}

			if (!webClient.IsUsable)
			{
				ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"{_socketType} 사용 불가능 상태. 연결 상태 확인바람. PacketID: {_reqPacket.ID}");
				return false;
			}

			if (!DicLastSendTimeRecord.ContainsKey(_reqPacket.ID))
				DicLastSendTimeRecord.Add(_reqPacket.ID, 0);

			float lastSendedTime = DicLastSendTimeRecord[_reqPacket.ID];
			float sendInterval = GetSendInterval(_reqPacket.ID);

			if ((Time.realtimeSinceStartup - lastSendedTime) < sendInterval)
			{
				StartCoroutine(SendDelayPacket_Routine(_socketType, _reqPacket, sendInterval - (Time.realtimeSinceStartup - lastSendedTime)));
				return true;
			}

			// 실제 연결된 서버로 Binary데이터 전송
			webClient.Socket.SendByBytes(_reqPacket.GetBytes());

			DicLastSendTimeRecord[_reqPacket.ID] = Time.realtimeSinceStartup;

			//ZLog.Log(ZLogChannel.Network, $"[{_socketType}] Actual Send Packet | ID: {_reqPacket.ID}, Number: {_reqPacket.Number}");

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		private IEnumerator SendDelayPacket_Routine(E_WebSocketType _socketType, ZWebReqPacketBase _reqPacket, float _dealyTime)
		{
			yield return new WaitForSeconds(_dealyTime);
			TrySendPacket(_socketType, _reqPacket);
		}

		/// <summary> 전송 대기 메시지에 추가한다 </summary>
		private bool AddPendingPacket(E_WebSocketType _socketType, ZWebReqPacketBase _reqPacket)
		{
			if (!DicPendingPacket.TryGetValue(_socketType, out var pendingList))
			{
				pendingList = new List<ZWebReqPacketBase>();
				DicPendingPacket.Add(_socketType, pendingList);
			}

			if (IsUniquePacket(_reqPacket.ID))
			{
				for (int i = 0; i < pendingList.Count; i++)
				{
					if (pendingList[i].ID == _reqPacket.ID)
					{
						OnErrorPacket(_socketType, E_ErrorType.SendDuplication, _reqPacket, null);
						ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"[{_socketType}] 이미 Packet[{_reqPacket.ID}] 응답 대기중. 중복 허용하려면, {nameof(IsUniquePacket)}()에 패킷ID 추가바람. | PacketID: {_reqPacket.ID}");
						return false;
					}
				}

				if (DicSendedPacket.TryGetValue(_socketType, out var sendedList))
				{
					for (int i = 0; i < sendedList.Count; i++)
					{
						if (sendedList[i].ID == _reqPacket.ID)
						{
							OnErrorPacket(_socketType, E_ErrorType.SendDuplication, _reqPacket, null);
							ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"[{_socketType}] 이미 Packet[{_reqPacket.ID}] 응답 대기중. 중복 허용하려면, {nameof(IsUniquePacket)}()에 패킷ID 추가바람. | PacketID: {_reqPacket.ID}");
							return false;
						}
					}
				}
			}

			pendingList.Add(_reqPacket);

			// 보낸메시지가 하나도 없다면, 대기중 메시지 보내기
			SendPendingPacket(_socketType);

			return true;
		}

		/// <summary> 대기중인 패킷이 존재하는지 체크해서 1개 전송한다. </summary>
		private void SendPendingPacket(E_WebSocketType _socketType)
		{
			// 보낸 메시지가 아직 존재하면 대기중인 메시지는 보내지 않는다.
			if (DicSendedPacket.TryGetValue(_socketType, out var sendedList) && sendedList.Count > 0)
			{
				return;
			}

			if (!DicPendingPacket.TryGetValue(_socketType, out var pendingList) || pendingList.Count == 0)
			{
				return;
			}

			if (TrySendPacket(_socketType, pendingList[0]))
			{
				bool isWaitReceive = AddSendedQueue(_socketType, pendingList[0]);
				pendingList.RemoveAt(0);

				// Receive를 기다려야하는 패킷이 아니라면 다음꺼 바로 시도.
				if (!isWaitReceive)
				{
					SendPendingPacket(_socketType);
				}
			}
			else
			{
				OnErrorPacket(_socketType, E_ErrorType.SendFailure, pendingList[0], null);
			}
		}

		/// <summary> 보낸 리스트에 저장 </summary>
		public bool AddSendedQueue(E_WebSocketType _socketType, ZWebReqPacketBase _reqPacket)
		{
			if (!ExistReceivePacket(_reqPacket.ID))
				return false;

			if (!DicSendedPacket.ContainsKey(_socketType))
				DicSendedPacket.Add(_socketType, new List<ZWebReqPacketBase>());
			DicSendedPacket[_socketType].Add(_reqPacket);
			return true;
		}

		/// <summary> 보낸 리스트에서 삭제 (추가/삭제 쌍이 맞아야함) </summary>
		void RemoveSendedMessage(E_WebSocketType _socketType, ZWebRecvPacket _recvPacket)
		{
			if (DicSendedPacket.TryGetValue(_socketType, out var sendedList))
			{
				for (int i = 0; i < sendedList.Count; i++)
				{
					if (sendedList[i].Number == _recvPacket.Number && sendedList[i].ID == _recvPacket.ID)
					{
						sendedList.RemoveAt(i);
						break;
					}
				}
			}
		}

		/// <summary>
		/// 응답 받을때 처리할 콜백 등록
		/// </summary>
		/// <param name="_packetErrCB">전송 or 응답시 오류 발생시 호출될 콜백</param>
		public void RegisterReceiveCallback(E_WebSocketType _socketType, ulong packetNo, ReceiveCBDelegate _receiveCB, PacketErrorCBDelegate _packetErrCB = null)
		{
			if (!DicReceiveCallback.TryGetValue(_socketType, out var dicCallback))
			{
				dicCallback = new Dictionary<ulong, ValueTuple<ReceiveCBDelegate, PacketErrorCBDelegate>>();
				DicReceiveCallback.Add(_socketType, dicCallback);
			}

			if (!dicCallback.ContainsKey(packetNo))
				dicCallback.Add(packetNo, (_receiveCB, _packetErrCB));
			else
			{
				ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Error, $"[{_socketType}] already registered CallBack | PacketNo: {packetNo}");
				dicCallback[packetNo] = (_receiveCB, _packetErrCB);
			}
		}

		/// <summary>
		/// 응답받은 패킷에 맞는 콜백을 호출하고 캐시에서 제거한다.
		/// </summary>
		public void ConsumeReceiveCallback(E_WebSocketType _socketType, ZWebRecvPacket _recvPacket)
		{
			if (DicReceiveCallback.TryGetValue(_socketType, out var dicNumberCallback) &&
				dicNumberCallback.TryGetValue(_recvPacket.Number, out var registeredCallback))
			{
				if (_recvPacket.ErrCode == ERROR.NO_ERROR ||
					CanIgnoreErrorCode(_recvPacket.ErrCode))
				{
					registeredCallback.Item1?.Invoke(_recvPacket);
				}
				else
				{
					// 정상 응답은 받았지만, 패킷 결과 에러일때					
					OnErrorPacket(_socketType, E_ErrorType.Receive, null, _recvPacket);
				}

				// OnErrorPacket때문에 현 위치에서 삭제해야함.
				dicNumberCallback.Remove(_recvPacket.Number);
			}
			else
			{
				// Broadcast 메시지일때 처리
				ushort id = (ushort)_recvPacket.ID;
				if (id >= 60000 && id < 61000)
				{
					ZWebBroadcast.RecvBroadCastMessage(_recvPacket);
				}
			}
		}

		//각 메세지 종류에 따른 요청 딜레이 시간이 있을시 추가 처리를 해주면 된다.
		/// <summary>
		/// 패킷별 딜레이 시간 체크
		/// </summary>
		/// <param name="_packetId"></param>
		/// <returns></returns>
		public float GetSendInterval(Code _packetId)
		{
			return ZNetPacketConfig.GetPacketCoolTime(_packetId);
		}

		#region ========:: Receiver ::========

		float mLastConsumeTIme = 0f;
		const float mConsumeProcessInterval = 0.1f;

		/// <summary>
		/// 실서버로부터 수신받는 이벤트 함수
		/// </summary>
		private void OnClient_BytesReceived(ZWebClientBase _webClient, byte[] _recvBytes)
		{
			if (!DicReceivedPacket.TryGetValue(_webClient.SocketType, out var packetQueue))
			{
				packetQueue = new Queue<ZWebRecvPacket>();
				DicReceivedPacket.Add(_webClient.SocketType, packetQueue);
			}

			packetQueue.Enqueue(new ZWebRecvPacket(_recvBytes));

			//ZLog.Log(ZLogChannel.WebSocket, $"[{_webClient.SocketType}] OnClient_BytesReceived | QueueCount: {packetQueue.Count}");
		}

		private void Update()
		{
			// 일정시간마다 Receive 받은 패킷들 처리하도록 한다.
			if (Time.time - mLastConsumeTIme > mConsumeProcessInterval)
			{
				ConsumeReceivedPackets();

				mLastConsumeTIme = Time.time;
			}
		}

		/// <summary>
		/// 호출시 마다 <see cref="DicReceivedPacket"/>에 있는 패킷들 모두 처리한다.
		/// </summary>
		private void ConsumeReceivedPackets()
		{
			//try
			//{
				foreach (var socketType in DicReceivedPacket.Keys)
				{
					var recvQueue = DicReceivedPacket[socketType];

					while (recvQueue.Count > 0)
					{
						var recvPacket = recvQueue.Dequeue();

						// 인디게이터 지우기
						if (NeedIndicator(recvPacket.ID))
							IndicatorStatusEvent?.Invoke(E_IndicatorStatus.HIDE);

						//ZLog.Log(ZLogChannel.WebSocket, $"[{socketType}][{nameof(ZWebCommunicator.ConsumeReceivedPackets)}] Receive PacketID: {recvPacket.ID}, No: {recvPacket.Number}, Error: {recvPacket.ErrCode}");

						RemoveSendedMessage(socketType, recvPacket);

						// 응답받아서 처리해야하는 콜백 호출
						ConsumeReceiveCallback(socketType, recvPacket);
					}

					SendPendingPacket(socketType);
				}
			//}
			//catch(System.Exception e)
			//{
			//	ZLog.Log(ZLogChannel.Network, ZLogLevel.Error, $"[{nameof(ZWebCommunicator.ConsumeReceivedPackets)}] 수신 받은 패킷 처리 상태에서 예외발생! ( | Message: {e.Message},\nStackTrace: {e.StackTrace}");
			//}
		}

		#endregion

		#region ====:: 조건 체크 함수들 ::====

		/// <summary>즉시 전송해야할 패킷인지 검사</summary>
		/// <remarks>
		/// 응답 대기안하고, 여러개 보내야하는 패킷은 이곳에서 예외처리 해주도록 하자.
		/// </remarks>
		private bool IsDirectPacket(Code _packetId)
		{
			switch (_packetId)
			{
				default:
					return false;
			}
		}

		/// <summary> 응답 받아야하는 패킷인지 여부 </summary>
		private bool ExistReceivePacket(Code _packetId)
		{
			return true;
		}

		/// <summary> 동일 ID한번에 하나만 보내야하는 패킷인지 여부 </summary>
		/// <remarks>
		/// 기본적으로 동일 ID는 Request보낸다음 응답받고 다시 보낼 수 있도록 하자.
		/// </remarks>
		private bool IsUniquePacket(Code _packetId)
		{
            return ZNetPacketConfig.IsUniquePacket(_packetId);

            /*
			switch (_packetId)
			{
				//case Code.GS_GET_MAIL_LIST:
				case Code.GS_PET_COLLECT_MAKE:    // 여러개의 컬렉션 갱신
				case Code.GS_CHANGE_COLLECT_MAKE: // 여러개의 컬렉션 갱신
				case Code.GS_ITEM_GACHA:          // 아이템 뽑기 (뽑을 수 있는 아이템 종류가 많음)
				case Code.COMMON_SET_CHARACTER_OPTION:  // 캐릭터 옵션 설정 (퀵슬롯 등의 유저 설정) 
				case Code.GS_PET_ADV_REWARD:		// 펫 모험 일괄 보상받기
					return false;

			}

			return true;*/
		}

		private bool NeedIndicator(Code _packetId)
		{
            return ZNetPacketConfig.IsIndicatorPacket(_packetId);
            /*
            switch (_packetId)
			{
				case Code.GS_GET_MAIL_LIST:
				case Code.GS_ITEM_GACHA:
				case Code.GS_ITEM_USE:
				case Code.GS_ITEM_DELETE:
				case Code.GS_CHANGE_COLLECT_MAKE:
				case Code.GS_PET_COLLECT_MAKE:
				//case Code.GS_ITEM_GACHA:

					return false;
			}

			return true;*/
		}

		/// <summary> Receive시 정상 데이터와 에러코드둘다 왔을때, 특정 에러코드 무시하고 정상 처리할지 여부 </summary>
		/// <param name="_ignoreError"></param>
		/// <returns></returns>
		private bool CanIgnoreErrorCode(ERROR _ignoreError)
		{
			switch (_ignoreError)
			{
				case ERROR.NOT_HAVE_DAILY_RESET_EVENT:
				case ERROR.HEADER_ERROR_BUT_IGNORE_PACKET:
				case ERROR.DO_NOT_FIND_EVENT_TYPE:
					return true;
			}

			return false;
		}

		#endregion
	}
}