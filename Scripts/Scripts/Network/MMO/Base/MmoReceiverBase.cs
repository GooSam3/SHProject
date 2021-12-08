using FlatBuffers;
using icarus_mmo_messages;
using System.Collections.Generic;
using UnityEngine;

namespace ZNet
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class MmoReceiverBase
	{
		protected MmoSessionBase Owner { get; private set; }

		/// <summary>
		/// Key: int (MmoNet.MSG) | <see cref="IFlatbufferObject"/> 응답 메시지 핸들러
		/// </summary>
		protected Dictionary<int, System.Action<byte[]>> mReceiveHandler = new Dictionary<int, System.Action<byte[]>>();

		public MmoReceiverBase(MmoSessionBase _owner)
		{
			Initialize(_owner);
		}

		~MmoReceiverBase()
		{
			if (null != this.Owner)
			{
				this.Owner.FBMessageReceived -= OnFBMessageReceived;
				Debug.Log("sadfsdafsdfsd");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected void Initialize(MmoSessionBase _owner)
		{
			this.Owner = _owner;
			this.Owner.FBMessageReceived += OnFBMessageReceived;
			this.RegisterCallbacks();
		}

		/// <summary>
		/// 메시지 응답에 맞는 처리를 위한 핸들러들 등록하도록 하자.
		/// </summary>
		protected abstract void RegisterCallbacks();

		/// <summary>
		/// 에러가 존재하는 응답 메시지를 받았을때 호출된다.
		/// </summary>
		protected abstract void OnErrorMessage(FlatMsg _flatMsg);

		/// <summary>
		/// 메시지 타입에 맞는 핸들러 찾아서 호출 처리
		/// </summary>
		private void OnFBMessageReceived(FlatMsg _flatMsg)
		{
			if (mReceiveHandler.TryGetValue(_flatMsg.msgtype, out var recvFunc))
			{
				if (_flatMsg.err_codeSpecified)
				{
					OnErrorMessage(_flatMsg);
				}
				else
				{
					recvFunc.Invoke(_flatMsg.array);
				}
			}
			else
			{
				Fun.FunDebug.LogWarning("not exist ReceiveFBMessageHandler | MmoNet.MSG: {0}", (MmoNet.MSG)_flatMsg.msgtype);
			}
		}

		/// <summary>
		/// <see cref="IFlatbufferObject"/> 기반 프로토콜 타입 객체로 변환
		/// </summary>
		/// <typeparam name="FBOBJECT_TYPE"><see cref="IFlatbufferObject"/> 기반 타입 </typeparam>
		public FBOBJECT_TYPE ConvertFBObject<FBOBJECT_TYPE>(byte[] _recvBytes) where FBOBJECT_TYPE : IFlatbufferObject
		{
			var fbObject = ZNetHelper.ConvertFBObject<FBOBJECT_TYPE>(_recvBytes);

			ZLog.Log(ZLogChannel.MMO, $"<color=orange>[RECV] {typeof(FBOBJECT_TYPE).Name} : {ZNetHelper.GetPropertyStrings(fbObject)}</color>");

			return fbObject;
		}

        public FBOBJECT_TYPE ConvertFBObjectNoLog<FBOBJECT_TYPE>(byte[] _recvBytes) where FBOBJECT_TYPE : IFlatbufferObject
        {
            var fbObject = ZNetHelper.ConvertFBObject<FBOBJECT_TYPE>(_recvBytes);
            return fbObject;
        }
    }

}