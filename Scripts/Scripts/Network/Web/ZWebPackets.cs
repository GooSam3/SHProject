using FlatBuffers;
using System;
using System.Collections.Generic;
using WebNet; // protocol

namespace ZNet
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="ID_TYPE">패킷 고유 식별자</typeparam>
	/// <typeparam name="NUMBER_TYPE">패킷 순서(넘버링)</typeparam>
	public interface IWebPacket<ID_TYPE, NUMBER_TYPE>
	{
		ID_TYPE ID { get; }
		NUMBER_TYPE Number { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class ZWebReqPacketBase : IWebPacket<WebNet.Code, ulong>
	{
		public WebNet.Code ID { get; protected set; }
		public ulong Number { get; set; }

		protected List<byte> mReqBytes = new List<byte>();
		protected List<byte> mBodyBytes = new List<byte>();

		/// <summary> 약속된 Header 재구성 </summary>
		protected abstract void FillHeader();

		public virtual byte[] GetBytes()
		{
			FillHeader();
			if (mBodyBytes.Count == 0)
			{
				ZLog.Log(ZLogChannel.WebSocket, ZLogLevel.Warning, $"Body가 비어있는데 보내려고합니다.");
			}
			mReqBytes.AddRange(mBodyBytes);

			return mReqBytes.ToArray();
		}

		/// <summary>
		/// <see cref="FlatBufferBuilder.Finish(int)"/> 처리 및 ByteBuffer저장
		/// </summary>
		public void FinishBuilder(FlatBufferBuilder builder, int offsetValue)
		{
			builder.Finish(offsetValue);
			AddBodyMsg(builder);
		}

		public void AddBodyMsg(FlatBufferBuilder builder)
		{
			mBodyBytes.AddRange(builder.SizedByteArray());
		}

		[System.Diagnostics.Conditional("ZLOG")]
		public void PrintLog<REQ_TYPE>(FlatBufferBuilder _builder) where REQ_TYPE : IFlatbufferObject
		{
			ZLog.Log(ZLogChannel.WebSocket, $"ID[{ID}({(ushort)ID})], No[{Number}]," +
					$" UserID[{ZNet.Data.Me.UserID}], CharID[{ZNet.Data.Me.CharID}] | " +
					$"<color=green>[REQ] {typeof(REQ_TYPE).Name}: {ZNetHelper.GetPropertyStrings(ZNetHelper.ConvertFBObject<REQ_TYPE>(_builder.DataBuffer))}</color>");
		}
	}

	/// <summary>
	/// 게이트웨이 전용
	/// </summary>
	public class ZWWWPacket : ZWebReqPacketBase
	{
		public ZWWWPacket(WebNet.Code _code)
		{
			this.ID = _code;
		}

		protected override void FillHeader()
		{
			mReqBytes.Clear();

			mReqBytes.AddRange(BitConverter.GetBytes(this.Number));
			mReqBytes.AddRange(BitConverter.GetBytes((ushort)this.ID));
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.UserID));//UserID
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.CharID));//CharID
			mReqBytes.AddRange(BitConverter.GetBytes(0));//RandKey
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.SelectedServerID));//ServerID
		}

		/// <summary>
		/// 플랫버퍼용 패킷 완성해서 리턴해준다.
		/// </summary>
		/// <param name="_filledFB">*주의* 사용후 클리어됨</param>
		/// <param name="_offsetValue">버퍼 최종 offset값</param>
		/// <returns></returns>
		public static ZWWWPacket Create<REQ_TYPE>(WebNet.Code _code, FlatBufferBuilder _filledFB, int _offsetValue) where REQ_TYPE : IFlatbufferObject
		{
			ZWWWPacket reqPacket = new ZWWWPacket(_code);

			reqPacket.FinishBuilder(_filledFB, _offsetValue);
			reqPacket.PrintLog<REQ_TYPE>(_filledFB);
			_filledFB.Clear();

			return reqPacket;
		}
	}

	public class ZWebPacket : ZWebReqPacketBase
	{
		private uint mSecurityKey = 0;

		public ZWebPacket(WebNet.Code _code, uint _rndKey)
		{
			this.ID = _code;
			this.mSecurityKey = _rndKey;
		}

		protected override void FillHeader()
		{
			mReqBytes.Clear();

			mReqBytes.AddRange(BitConverter.GetBytes(this.Number));
			mReqBytes.AddRange(BitConverter.GetBytes((ushort)this.ID));
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.UserID));//UserID
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.CharID));//CharID
			mReqBytes.AddRange(BitConverter.GetBytes(this.mSecurityKey));//RandKey
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.SelectedServerID));//ServerID
		}

		/// <summary>
		/// 플랫버퍼용 패킷 완성해서 리턴해준다.
		/// </summary>
		/// <param name="_filledFB">*주의* 사용후 클리어됨</param>
		/// <param name="_offsetValue">버퍼 최종 offset값</param>
		/// <returns></returns>
		public static ZWebPacket Create<REQ_TYPE>(ZWebClientBase _webClient, WebNet.Code _code, FlatBufferBuilder _filledFB, int _offsetValue) where REQ_TYPE : IFlatbufferObject
		{
			ZWebPacket reqPacket = new ZWebPacket(_code, _webClient.SecurityKey);

			reqPacket.Number = _webClient.NextPacketNumber();
			reqPacket.FinishBuilder(_filledFB, _offsetValue);
			reqPacket.PrintLog<REQ_TYPE>(_filledFB);
			_filledFB.Clear();

			return reqPacket;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ZWebRecvPacket : IWebPacket<WebNet.Code, ulong>
	{
		public WebNet.Code ID { get; protected set; }
		public ulong Number { get; protected set; }
		public ERROR ErrCode { get; protected set; }

		public List<byte> mRecvBytes = new List<byte>();

		public ZWebRecvPacket(byte[] _recvBytes)
		{
			Recv(_recvBytes);
		}

		public void Recv(byte[] _recvBytes)
		{
			mRecvBytes.Clear();
			mRecvBytes.AddRange(_recvBytes);

			//
			// 예약된 기본 Header만 파싱하고, BodyData만 남겨두도록 한다.
			//
			Number = BitConverter.ToUInt64(mRecvBytes.ToArray(), 0);
			ID = (WebNet.Code)BitConverter.ToUInt16(mRecvBytes.ToArray(), 8);
			ErrCode = (ERROR)BitConverter.ToUInt16(mRecvBytes.ToArray(), 10);
			// 8 + 2 + 2
			mRecvBytes.RemoveRange(0, 12);
		}

		/// <summary>
		/// <see cref="IFlatbufferObject"/> 기반 프로토콜 타입 객체로 변환
		/// </summary>
		/// <typeparam name="FBOBJECT_TYPE"><see cref="IFlatbufferObject"/> 기반 타입 </typeparam>
		public FBOBJECT_TYPE Get<FBOBJECT_TYPE>() where FBOBJECT_TYPE : IFlatbufferObject
		{
			var fbObject = ZNetHelper.ConvertFBObject<FBOBJECT_TYPE>(mRecvBytes);

			ZLog.Log(ZLogChannel.WebSocket, $"ID[{this.ID}({(ushort)ID})], No[{this.Number}], Error[{this.ErrCode}({(uint)this.ErrCode})] | " +
				$"<color=orange>[RECV] {typeof(FBOBJECT_TYPE).Name} : {ZNetHelper.GetPropertyStrings(fbObject)}</color>");

			return fbObject;
		}

	}

	//채팅 전용 패킷
	public class ZChatPacket : ZWebReqPacketBase
    {
		private uint mSecurityKey = 0;

		public ZChatPacket(WebNet.Code _code, uint _rndKey)
        {
			this.ID = _code;
			this.mSecurityKey = _rndKey;
        }

        protected override void FillHeader()
        {
			mReqBytes.Clear();

			mReqBytes.AddRange(BitConverter.GetBytes(this.Number));
			mReqBytes.AddRange(BitConverter.GetBytes((ushort)this.ID));
			mReqBytes.AddRange(BitConverter.GetBytes(ZNet.Data.Me.SelectedServerID));//ServerID
		}

		/// <summary>
		/// 플랫버퍼용 패킷 완성해서 리턴해준다.
		/// </summary>
		/// <param name="_filledFB">*주의* 사용후 클리어됨</param>
		/// <param name="_offsetValue">버퍼 최종 offset값</param>
		/// <returns></returns>
		public static ZChatPacket Create<REQ_TYPE>(ZWebClientBase _webClient, WebNet.Code _code, FlatBufferBuilder _filledFB, int _offsetValue) where REQ_TYPE : IFlatbufferObject
		{
			ZChatPacket reqPacket = new ZChatPacket(_code, _webClient.SecurityKey);

			reqPacket.Number = _webClient.NextPacketNumber();
			reqPacket.FinishBuilder(_filledFB, _offsetValue);
			reqPacket.PrintLog<REQ_TYPE>(_filledFB);
			_filledFB.Clear();

			return reqPacket;
		}
	}
}