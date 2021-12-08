using FlatBuffers;
using Fun;
using GameDB;
using MmoNet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZNet
{
	public class ZMmoField : MmoSessionBase
	{
		protected override MmoReceiverBase Receiver => mReceiver;

		private ZMmoFieldReceiver mReceiver;

		protected override void Initialize()
		{
			base.Initialize();
            
            mReceiver = new ZMmoFieldReceiver(this);
        }

		protected override void EditTransportOption(ref TransportOption _refOption)
		{
			base.EditTransportOption(ref _refOption);

			if (_refOption is TcpTransportOption tcpOption)
			{
				tcpOption.SetPing(10, 30, true);
			}
		}

		/// <summary> Log출력 포함 SendMessage </summary>
		public void SendMessage<FBOBJECT_TYPE>(MmoNet.MSG msgType, FlatBufferBuilder _builder)
		{
			ZLog.Log(ZLogChannel.MMO, $"<color=green>[SEND] {typeof(FBOBJECT_TYPE).Name} : {ZNetHelper.GetPropertyStrings(ZNetHelper.ConvertFBObject<FBOBJECT_TYPE>(_builder.DataBuffer))}</color>");

			base.SendMessage(msgType, _builder);
		}

		/// <summary> <see cref="InMapMsgUnion"/> 필드 관련 패킷 전송에 사용 </summary>
		private void SendMessage_InMapMsg<FBOBJECT_TYPE>(InMapMsgUnion _mapMsgType, int _offset)
		{
#if ZLOG
			mFBuilder.Finish(_offset);
			ZLog.Log(ZLogChannel.MMO, $"<color=green>[SEND] {typeof(FBOBJECT_TYPE).Name} : {ZNetHelper.GetPropertyStrings(ZNetHelper.ConvertFBObject<FBOBJECT_TYPE>(mFBuilder.DataBuffer))}</color>");
#endif
			var baseMsgOffset = InMapMsgBase.CreateInMapMsgBase(mFBuilder, _mapMsgType, _offset);
			var msgOffset = InMapMsg.CreateInMapMsg(mFBuilder, ZMmoManager.Instance.GameTick, baseMsgOffset);
			mFBuilder.Finish(msgOffset.Value);

			// not using SendMessage<TYPE>()!
			base.SendMessage(MSG.InMapMsg, mFBuilder);
		}

		//
		//======================================================================================
		//

		public void REQ_ServerList()
		{
			//var offset = ReqServerList.CreateReqServerList(mFBuilder, MmoNet.ClientType.test, 1);
			//mFBuilder.Finish(offset.Value);

			//SendMessage<ReqServerList>(MSG.ReqServerList, mFBuilder);
		}

		public void REQ_LoginID(string _accountName = "kpro1")
		{
			//var offset = ReqLoginID.CreateReqLoginID(mFBuilder,
			//	mFBuilder.CreateString(_accountName), 0,
			//	mFBuilder.CreateString("tt"));
			//mFBuilder.Finish(offset.Value);

			//SendMessage<ReqLoginID>(MSG.ReqLoginID, mFBuilder);
		}

		/// <summary>
		/// 캐릭터 선택화면 같은곳으로 갈때, 종료전 요청해야함.
		/// </summary>
		/// <param name="_reason">아직 서버에서 정해진 값은 없음</param>
		public void REQ_Logout(byte _reason = 0)
		{
			var offset = LogOutReq.CreateLogOutReq(mFBuilder, _reason);
			mFBuilder.Finish(offset.Value);

			SendMessage<LogOutReq>(MSG.LogOut, mFBuilder);
		}

		public void REQ_JoinField(ulong _userId, ulong _charId, string _charName, uint _serverId, long _roomIdx )
		{
			var offset = JoinFieldReq.CreateJoinFieldReq(mFBuilder,
				_charId, _userId, mFBuilder.CreateString(_charName), 
				1111 /* 현재는 서버에서 아무거나 보내도된다고 해서... */,
                _serverId,
                _roomIdx);
			mFBuilder.Finish(offset.Value);

			ZPawnManager.Instance.MyEntityId = 0;

			SendMessage<JoinFieldReq>(MSG.JoinField, mFBuilder);
        }

        public void REQ_ServerTime()
        {
            var offset = GetServerTimeReq.CreateGetServerTimeReq(mFBuilder,
                TimeManager.NowMs, (uint)Version.Num);
            mFBuilder.Finish(offset.Value);

            SendMessage<GetServerTimeReq>(MSG.GetServerTime, mFBuilder);
        }

		public Dictionary<MSG, UnityAction<IFlatbufferObject>> MmoFieldCallBack = new Dictionary<MSG, UnityAction<IFlatbufferObject>>();
		public void REQ_MoveServer(ushort _channelId, UnityAction<IFlatbufferObject> _moveServerCallBack)
		{
			MmoFieldCallBack[MSG.MoveServer] = _moveServerCallBack;
			var offset = MoveServerReq.CreateMoveServerReq(mFBuilder, _channelId);
			mFBuilder.Finish(offset.Value);
			SendMessage<MoveServerReq>(MSG.MoveServer, mFBuilder);		
		}

        #region ========:: for InMapMsg ::========

        /// <summary> 클라이언트단 모든 준비가 끝나서 캐릭터 배치 가능하다고 알린다. </summary>
        public void REQ_LoadMapOK()
		{
			var offset = ReqLoadMapOK.CreateReqLoadMapOK(mFBuilder, 0);

			SendMessage_InMapMsg<ReqLoadMapOK>(InMapMsgUnion.ReqLoadMapOK, offset.Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 개발모드에서는 Cheat기능 존재함. 서버 저장소에 있는 문서 참고
		/// </remarks>
		public void REQ_MapChat(uint _objId, string _chatMsg)
		{
			var offset = CS_MapChat.CreateCS_MapChat(mFBuilder, _objId, mFBuilder.CreateString(_chatMsg));

			SendMessage_InMapMsg<CS_MapChat>(InMapMsgUnion.CS_MapChat, offset.Value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_objId"></param>
		/// <param name="_curPos"></param>
		/// <param name="_dirDeg">360단위</param>
		/// <param name="_speed"></param>
		/// <param name="_lookDirDeg"></param>
		public void REQ_MoveToDir(uint _objId, Vector3 _curPos, float _dirDeg, float _speed, float _lookDirDeg)
		{
			var offset = CS_MoveToDir.CreateCS_MoveToDir(mFBuilder, 
				_objId,
				CreateServerPos(ref _curPos),
				_dirDeg,
				_speed,
				_lookDirDeg);

			SendMessage_InMapMsg<CS_MoveToDir>(InMapMsgUnion.CS_MoveToDir, offset.Value);
		}

        public void REQ_MoveToDest(uint _objId, Vector3 _curPos, Vector3 _destPos, float _speed)
        {
            REQ_MoveToDest(_objId, _curPos, new List<Vector3>() { _destPos } , _speed);
        }

        public void REQ_MoveToDest(uint _objId, Vector3 _curPos, List<Vector3> _destPath, float _speed)
		{
            Offset<ServerPos3>[] vectors = new Offset<ServerPos3>[_destPath.Count];

            for(int i = 0; i < vectors.Length; ++i)
            {
                Vector3 dest = _destPath[i];
                vectors[i] = CreateServerPos(ref dest);
            }

            var destOffset = CS_MoveToDest.CreateDestVector(mFBuilder, vectors);
			var offset = CS_MoveToDest.CreateCS_MoveToDest(mFBuilder,
				_objId,
				ServerPos3.CreateServerPos3(mFBuilder, _curPos.x, _curPos.y, _curPos.z),
				_speed,
                destOffset);

			SendMessage_InMapMsg<CS_MoveToDest>(InMapMsgUnion.CS_MoveToDest, offset.Value);
		}

		public void REQ_MoveToCollision(uint _objId, Vector3 _curPos, Vector3 _destPos, float _speed, float _lookDirDeg)
		{
            var offset = CS_MoveToCollision.CreateCS_MoveToCollision(mFBuilder,
				_objId,
				CreateServerPos(ref _curPos),
				_speed,
				_lookDirDeg,
				CreateServerPos(ref _destPos));

			SendMessage_InMapMsg<CS_MoveToCollision>(InMapMsgUnion.CS_MoveToCollision, offset.Value);
		}

        public void REQ_MoveStop(uint _objId, Vector3 _curPos)
        {
            var offset = CS_MoveStop.CreateCS_MoveStop(mFBuilder,
                _objId,
                CreateServerPos(ref _curPos));

            SendMessage_InMapMsg<CS_MoveStop>(InMapMsgUnion.CS_MoveStop, offset.Value);
        }

		public void REQ_Attack(uint _objId, Vector3 _curPos, float _dirDeg, uint _skillNo, uint _targetObjId, byte _combo, float _attackSpeed)
		{
			var offset = CS_Attack.CreateCS_Attack(mFBuilder, _objId,
				CreateServerPos(ref _curPos),
				_dirDeg,
				_skillNo,
				_targetObjId,
				_combo,
                _attackSpeed);

			SendMessage_InMapMsg<CS_Attack>(InMapMsgUnion.CS_Attack, offset.Value);
		}

        /// <summary> 스킬을 취소한다. </summary>
        public void REQ_SkillCancel(uint _objId)
        {
            var offset = CS_SkillCancle.CreateCS_SkillCancle(mFBuilder, _objId);

            SendMessage_InMapMsg<CS_SkillCancle>(InMapMsgUnion.CS_SkillCancle, offset.Value);
        }

        /// <summary> 스킬에 탈 것 등록 </summary>
        public void REQ_EquipVehicle(uint _vehicleTid)
        {
            var offset = CS_EquipVehicle.CreateCS_EquipVehicle(mFBuilder, _vehicleTid);

            SendMessage_InMapMsg<CS_EquipVehicle>(InMapMsgUnion.CS_EquipVehicle, offset.Value);
        }

        /// <summary> 탈것 탑승여부 </summary>
        public void REQ_RideVehicle(uint _objId, uint _vehicleTid)
        {
            var offset = CS_RideVehicle.CreateCS_RideVehicle(mFBuilder, _objId, _vehicleTid);

            SendMessage_InMapMsg<CS_RideVehicle>(InMapMsgUnion.CS_RideVehicle, offset.Value);
        }

        /// <summary> 아이템 사용 </summary>
        public void REQ_UseItem(uint _objId, uint _itemTid)
        {
            var offset = C2S_UseItem.CreateC2S_UseItem(mFBuilder, _objId, _itemTid);

            SendMessage_InMapMsg<C2S_UseItem>(InMapMsgUnion.C2S_UseItem, offset.Value);
        }

        /// <summary> 채집 요청 </summary>
        public void REQ_Gather(uint _objId, uint _targetId)
        {
            var offset = C2S_Gather.CreateC2S_Gather(mFBuilder, _objId, _targetId);

            SendMessage_InMapMsg<C2S_Gather>(InMapMsgUnion.C2S_Gather, offset.Value);
        }

        /// <summary> 파티 타겟 변경 </summary>
        public void REQ_SetPartyTarget(uint _objId)
        {
            var offset = C2S_SetPartyTarget.CreateC2S_SetPartyTarget(mFBuilder, _objId);

            SendMessage_InMapMsg<C2S_SetPartyTarget>(InMapMsgUnion.C2S_SetPartyTarget, offset.Value);
        }

        #region ===== :: 스탯 갱신 프리뷰 :: =====
        public void REQ_StatPreview(Dictionary<E_AbilityType, float> statValues)
        {
            Offset<Ability>[] vectors = new Offset<Ability>[statValues.Count];

            int index = 0;
            foreach(var stat in statValues)
            {
                vectors[index] = Ability.CreateAbility(mFBuilder, (ushort)stat.Key, stat.Value);
                ++index;
            }

            var offset = CS_StatPreview.CreateCS_StatPreview(mFBuilder, CS_StatPreview.CreateAbilsVector(mFBuilder, vectors));

            SendMessage_InMapMsg<CS_StatPreview>(InMapMsgUnion.CS_StatPreview, offset.Value);
        }
        #endregion

        #region ===== :: 싱글 전투용 :: =====
        /// <summary> 몬스터 소환 요청 </summary>
        public void REQ_MonsterSpawnReq(uint _monsterTid, Vector3 _spawnPos, Quaternion _spawnRot)
        {
            var offset = C2S_MonsterSpawnReq.CreateC2S_MonsterSpawnReq(mFBuilder, _monsterTid, CreateServerPos(ref _spawnPos), CreateServerRot(ref _spawnRot));

            SendMessage_InMapMsg<C2S_MonsterSpawnReq>(InMapMsgUnion.C2S_MonsterSpawnReq, offset.Value);
        }

        /// <summary> 싱글 전투에서 데미지 계산을 요청하는 패킷 (invoke Timing에 발동) </summary>
        public void REQ_DamageReq(uint _attackerEntityId, uint _targetEntityId, uint _skillTid, uint _damage = 0)
        {
            var offset = C2S_DamageReq.CreateC2S_DamageReq(mFBuilder, _attackerEntityId, _targetEntityId, _skillTid, _damage);

            SendMessage_InMapMsg<C2S_DamageReq>(InMapMsgUnion.C2S_DamageReq, offset.Value);
        }
        #endregion



        /// <summary> </summary>
        private Offset<ServerPos3> CreateServerPos(ref Vector3 _inPos)
		{
			return ServerPos3.CreateServerPos3(mFBuilder, _inPos.x, _inPos.y, _inPos.z);
		}

        /// <summary> </summary>
        private float CreateServerRot(ref Quaternion _inRot)
        {
            return _inRot.eulerAngles.y;
        }

        #endregion //========:: for InMapMsg ::========
    }
}
