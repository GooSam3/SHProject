using FlatBuffers;
using System;
using System.Collections.Generic;
using UnityEngine;
using WebNet;
using ZDefine;
using ZNet.Data;

namespace ZNet
{
	public class ZWebGame : ZWebClientBase
	{
		public override bool IsUsable => base.IsUsable;

		public override E_WebSocketType SocketType => E_WebSocketType.Game;

		public override uint SecurityKey { get; protected set; }

		private float LastPortalTime = 0f;

		/// <summary> 포탈 쿨 타임 처리 </summary>
		public bool CheckPortalCoolTime()
		{
			if (LastPortalTime + DBConfig.Portal_Delay_Time > Time.time)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("Channel_CoolTime_Message"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				return false;
			}

			return true;
		}

		public override void Disconnect()
		{
			base.Disconnect();
		}

		protected override void OnSocket_ConnectOpened(ZWebSocket socket)
		{
			if (IsReconnecting)
			{
				if (ZNet.Data.Me.IsValidMe)
				{
					// 재접속 성공시, 캐릭터 번들 요청한번 해줘야 Session 유지됨.
					REQ_GetAllCharInfoBundle(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID, ZNet.ZWebGame.CharacterAllBits, false, true, (recvPacket, resGetAllCharInfoBundle) =>
					{
						base.OnSocket_ConnectOpened(socket);
					} /*TODO : 에러발생시 게임 종료 */);
				}
				else
				{
					// TODO: 게임종료해줘
				}
			}
			else
			{
				base.OnSocket_ConnectOpened(socket);
			}
		}

		protected override void OnSocket_ConnectClosed(ZWebSocket socket)
		{
			base.OnSocket_ConnectClosed(socket);
		}

		protected override void OnSocket_Error(ZWebSocket socket, string reason)
		{
			base.OnSocket_Error(socket, reason);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_reqPacket"></param>
		/// <param name="_onReceive">현재 패킷에 대해 정상 응답 받았을때 호출될 Callback. Error가 발생한다면 해당 콜백은 호출되지 않음.</param>
		/// <param name="_packetErrCB">null이라면 내장 error callback이 호출됩니다. <see cref="ZWebManager.ProcessErrorPacket(ZWebCommunicator.E_ErrorType, ZWebReqPacketBase, ZWebRecvPacket, bool)"/> 작동 흐름 참고 </param>
		public override void SendPacket(ZWebReqPacketBase _reqPacket, ReceiveCBDelegate _onReceive, PacketErrorCBDelegate _packetErrCB)
		{
			if (!IsUsable)
			{
				ZLog.Log(ZLogChannel.MMO, ZLogLevel.Warning, $"[{SocketType}] SendPacket() | 사용불가능 상태에서 호출됨. ID: {_reqPacket.ID}");
				return;
			}

			mCommunicator?.SendPacket(this.SocketType, _reqPacket, _onReceive, _packetErrCB);
		}

		//
		//========================================================================
		//

		// 이카루스에서 삭제됨.
		///// <summary> 대기 없이 로그인 가능한지 체크 요청 </summary>
		///// <param name="_onReceive">[상태, 대기중 유저 수]</param>
		//public void REQ_CheckAccountLogin(System.Action<E_ServerWaitQueueState, uint> _onReceive)
		//{
		//	ReqAccountLoginCheck.StartReqAccountLoginCheck(mBuilder);
		//	var offset = ReqAccountLoginCheck.EndReqAccountLoginCheck(mBuilder);
		//	var reqPacket = ZWebPacket.Create<ReqAccountLoginCheck>(this, Code.GS_ACCOUNT_LOGIN_CHECK, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) => 
		//	{
		//		ResAccountLoginCheck resAccountLoginCheck = recvPacket.Get<ResAccountLoginCheck>();

		//		_onReceive?.Invoke(resAccountLoginCheck.State, resAccountLoginCheck.WaitUserCnt);
		//	});
		//}

		///// <summary> 로그인 대기중인 유저수 요청 </summary>
		//public void REQ_AccountLoginWaitUserCnt(System.Action<ZWebRecvPacket, ResAccountLoginWaitUserCnt> _onReceive)
		//{
		//	ReqAccountLoginWaitUserCnt.StartReqAccountLoginWaitUserCnt(mBuilder);
		//	var offset = ReqAccountLoginWaitUserCnt.EndReqAccountLoginWaitUserCnt(mBuilder);
		//	var reqPacket = ZWebPacket.Create<ReqAccountLoginWaitUserCnt>(this, Code.GS_ACCOUNT_LOGIN_WAIT_USER_CNT, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResAccountLoginWaitUserCnt recvMsgPacket = recvPacket.Get<ResAccountLoginWaitUserCnt>();
		//		_onReceive?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sessionToken">계정 고유 식별자 (구글 UID같은)</param>
		/// <param name="rndKey">Gateway 보안키</param>
		/// <param name="device_id">기기 식별자</param>
		/// <param name="version"></param>
		public void REQ_AccountLogin(string sessionToken, uint rndKey, string device_id, string version, System.Action<ZWebRecvPacket, ResAccountLogin> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqAccountLogin.CreateReqAccountLogin(mBuilder,
				mBuilder.CreateString(device_id),
				mBuilder.CreateString(version),
				rndKey,
				mBuilder.CreateString(sessionToken),
				mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS),
				mBuilder.CreateString(NTCommon.Device.Release),
				mBuilder.CreateString(NTCommon.Device.Model),
				mBuilder.CreateString(NTCommon.Locale.GetLocaleCode()),
				mBuilder.CreateString(NTCore.CommonAPI.GetGameSupportLanguage().langCulture),
				mBuilder.CreateString(NTCore.CommonAPI.SetupData.clientVersion),
				mBuilder.CreateString(NTCore.CommonAPI.SetupData.storeCD.ToString()),
				mBuilder.CreateString(NTCore.AuthAPI.CurrentPlatformID.ToString()),
				NTCore.Security.Root.IsRooted ? true : false,
				false,
				(uint)(Time.realtimeSinceStartup - ZGameManager.Instance.GameStartTime),
				mBuilder.CreateString(NTCore.CommonAPI.GetGameServerID()),
				mBuilder.CreateString(NTIcarusManager.Instance.AdjustGpsADID),
				mBuilder.CreateString(NTIcarusManager.Instance.AdjustIDFA),
				mBuilder.CreateString(NTIcarusManager.Instance.AdjustIDFV),
				mBuilder.CreateString(NTIcarusManager.Instance.AdjustADID));

			var reqPacket = ZWebPacket.Create<ReqAccountLogin>(this, Code.GS_ACCOUNT_LOGIN, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResAccountLogin resAccountLogin = recvPacket.Get<ResAccountLogin>();
				this.SecurityKey = resAccountLogin.Rnd;

				ZNet.Data.Me.MaxCharCnt = resAccountLogin.CharacterMaxCnt;
				if (resAccountLogin.Account.HasValue)
				{
					var accountValue = resAccountLogin.Account.Value;
					ZNet.Data.Me.NID = accountValue.Nid;
					ZNet.Data.Me.gNID = accountValue.Gnid;
				}
				//이거머지 resAccountLogin.Time; 

				//if (recvMsgPacket.Account.Value.State == E_AccountStateType.CheckUserData) //(0: 활성화, 1: 삭제대기, 2: 제제중)
				//{
				//    MessagePopup.OpenSystemErrorPopup("계정이 정지 상태입니다.\n개발사에 문의 하세요.", delegate ()
				//    {
				//        ZCoreHelper.Quit();
				//    }, DBLocale.GetLocaleText("ApplicationQuit") /*"종료"*/);
				//    return;
				//}
				//else if (recvMsgPacket.Account.Value.State == E_AccountStateType.Restriction)
				//{
				//    MessagePopup.OpenSystemErrorPopup("계정이 사용제한 상태입니다.\n(남은시간 : " + TimeHelper.GetRemainTime(new System.DateTime((long)recvMsgPacket.Account.Value.BlockExporeDt)) + ")", delegate ()
				//    {
				//        ZCoreHelper.Quit();
				//    }, DBLocale.GetLocaleText("ApplicationQuit") /*"종료"*/);
				//    return;
				//}
				//else
				//{
				//NetData.Instance.SetCharacterMaxSlot(NetData.UserID, recvMsgPacket.CharacterMaxCnt);

				Me.CurUserData.SetMaxLevel(resAccountLogin.CharLvMax);

				Me.CurUserData.SetStorageMaxCnt(resAccountLogin.StroageMaxCnt);
				Me.CurUserData.SetNormalMsgSendCnt(resAccountLogin.NormalMsgSendCnt);
				Me.CurUserData.SetGuildMsgSendCnt(resAccountLogin.GuildMsgSendCnt);

				//NetData.Instance.SetAttendRewardSeq(NetData.UserID, recvMsgPacket.AttendRewardSeq);
				//NetData.Instance.SetAttendContinuityRewardSeq(NetData.UserID, recvMsgPacket.AttendContinuityRewardSeq);
				//NetData.Instance.SetAttendRewardDt(NetData.UserID, recvMsgPacket.AttendRewardDt);

				//NetData.Instance.SetSpecialSkillMaxCnt(NetData.UserID, recvMsgPacket.SpecialSkillMaxCnt);

				//NetData.Instance.SetHighestCharLv(NetData.UserID, recvMsgPacket.CharLvMax);

				Me.CurUserData.ColosseumRewardCnt = resAccountLogin.ColosseumRewardCnt;
				Me.CurUserData.InfinityDungeonRewardTime = resAccountLogin.InfinityDungeonRewardDt;
				Me.CurUserData.InfinityDungeonScheduleId = resAccountLogin.InfinityDungeonSeq;
				Me.CurUserData.CurrentInfinityDungeonId = resAccountLogin.InfinityDungeonTid;
				Me.CurUserData.LastInfinityDungeonId = resAccountLogin.InfinityDungeonLastTid;
				Me.CurUserData.LastRewardedStageTid = resAccountLogin.InfinityDungeonRewardStageTid;

				//NetData.Instance.SetScenarioDungeonStageTid(NetData.UserID, recvMsgPacket.ScenarioDungeonStageTid);
				//NetData.Instance.SetScenarioDungeonPlayCnt(NetData.UserID, recvMsgPacket.ScenarioDungeonPlayCnt);

				//NetData.Instance.SetInfinityDungeon(NetData.UserID, recvMsgPacket.InfinityDungeonSeq, recvMsgPacket.InfinityDungeonTid);
				//NetData.Instance.SetInfinityDungeonReward(NetData.UserID, recvMsgPacket.InfinityDungeonRewardDt);
				//NetData.Instance.SetInfinityDungeonLastClearTid(NetData.UserID, recvMsgPacket.InfinityDungeonLastTid);

				//강림 파견
				Me.CurUserData.SetChangeQuestLevel(resAccountLogin.ChangeQuestLv);
				Me.CurUserData.SetChangeQuestExp(resAccountLogin.ChangeQuestExp);
				Me.CurUserData.SetChangeQuestIssuedDt(resAccountLogin.ChangeQuestIssuedDt);
				//}

				//전역 장식 드랍 설정(룬)
				Me.CurUserData.AddRuneSetOptionType(resAccountLogin.RuneStageDropBitOption);

				_onReceive?.Invoke(recvPacket, resAccountLogin);
			}, _onError);
		}

		/// <summary> 계정 탈퇴 </summary>
		/// <param name="nid">라인 계정 고유 아이디</param>
		public void REQ_AccountLeave(string nid, System.Action<ZWebRecvPacket, ResAccountLeave> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqAccountLeave.CreateReqAccountLeave(mBuilder, mBuilder.CreateString(nid));
			var reqPacket = ZWebPacket.Create<ReqAccountLeave>(this, Code.GS_ACCOUNT_LEAVE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResAccountLeave recvMsgPacket = recvPacket.Get<ResAccountLeave>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary>
		/// [박윤성] 옵션 - 알림 - 캐릭터명 숨김처리
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_bOn"></param>
		public void REQ_SetAccountOption(E_AccountOptionType _type, bool _bOn, System.Action<ZWebRecvPacket, ResSetAccountOption> _onReceive, PacketErrorCBDelegate _onError = null)
        {
			var offset = ReqSetAccountOption.CreateReqSetAccountOption(mBuilder, _type, _bOn);
			var reqPacket = ZWebPacket.Create<ReqSetAccountOption>(this, Code.GS_SET_ACCOUNT_OPTION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSetAccountOption recvMsgPacket = recvPacket.Get<ResSetAccountOption>();
				
				ZNet.Data.Me.AccountOptionBit = recvMsgPacket.OptionBit;

				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
        }

		#region ========:: Manage Character ::========

		/// <summary> 계정에 존재하는 캐릭터 리스트 요청 </summary>
		public void REQ_CharacterList(System.Action<ZWebRecvPacket, ResCharList> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			ReqCharList.StartReqCharList(mBuilder);
			var offset = ReqCharList.EndReqCharList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqCharList>(this, Code.GS_CHARACTER_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharList resCharList = recvPacket.Get<ResCharList>();

				ZNet.Data.UserData userData = ZNet.Data.Global.GetUser(ZNet.Data.Me.UserID);
				for (int i = 0; i < resCharList.CharactersLength; i++)
				{
					userData.AddChar(resCharList.Characters(i).Value);
				}

				_onReceive?.Invoke(recvPacket, resCharList);
			}, _onError);
		}

		/// <summary> 캐릭터 생성 요청 (생성 성공시, 캐릭터 리스트를 다시 요청해서 정보를 받아야함.) </summary>
		public void REQ_CreateCharacter(uint _charTid, string _nickname, System.Action<ZWebRecvPacket, ResCharCreate> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharCreate.CreateReqCharCreate(mBuilder,
				_charTid,
				mBuilder.CreateString(_nickname));
			var reqPacket = ZWebPacket.Create<ReqCharCreate>(this, Code.GS_CHARACTER_CREATE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				_onReceive?.Invoke(recvPacket, recvPacket.Get<ResCharCreate>());
			}, _onError);
		}

		/// <summary> 닉네임 변경 요청 </summary>
		/// <param name="_itemId">닉네임 변경시 필요한 아이템 (생성시에는 0 입력)</param>
		/// <param name="_itemTid">닉네임 변경시 필요한 아이템 (생성시에는 0 입력)</param>
		public void REQ_ChangeNick(ulong _itemId, uint _itemTid, string _changingNickName, System.Action<ZWebRecvPacket, ResChangeNick> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeNick.CreateReqChangeNick(mBuilder,
				_itemId, _itemTid, mBuilder.CreateString(_changingNickName));
			var reqPacket = ZWebPacket.Create<ReqChangeNick>(this, Code.GS_CHANGE_NICK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeNick resChangeNick = recvPacket.Get<ResChangeNick>();

				//if (resChangeNick.RemainStack != null)
				//	NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.RemainStack.Value);
				ZNet.Data.Me.CurCharData.Nickname = resChangeNick.ChangeNick;

				_onReceive?.Invoke(recvPacket, resChangeNick);
			}, _onError);
		}

		/// <summary>통합 닉네임 변경</summary>
		public void REQ_CombineChangeNick(ulong _charId, string _changingNickName, System.Action<ZWebRecvPacket, ResCombineChangeNick> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCombineChangeNick.CreateReqCombineChangeNick(mBuilder,
				_charId, mBuilder.CreateString(_changingNickName));
			var reqPacket = ZWebPacket.Create<ReqCombineChangeNick>(this, Code.GS_COMBINE_CHANGE_NICK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCombineChangeNick resCombineChangeNick = recvPacket.Get<ResCombineChangeNick>();
				ZNet.Data.Me.CurCharData.Nickname = resCombineChangeNick.ChangeNick;

				_onReceive?.Invoke(recvPacket, resCombineChangeNick);
			}, _onError);
		}

		/// <summary> </summary>
		/// <param name="_isForceDeletion">[개발서버용] 강제 삭제 유무</param>
		public void REQ_CharDelete(ulong _userId, ulong _charId, bool _isForceDeletion, System.Action<ZWebRecvPacket, ResCharDelete> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharDelete.CreateReqCharDelete(mBuilder,
				_userId, _charId, _isForceDeletion);
			var reqPacket = ZWebPacket.Create<ReqCharDelete>(this, Code.GS_CHARACTER_DELETE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharDelete resCharDelete = recvPacket.Get<ResCharDelete>();

				var userData = ZNet.Data.Global.GetUser(_userId);
				if (_isForceDeletion)
				{
					userData.RemoveChar(_charId);
				}
				else
				{
					var charData = userData.GetChar(_charId);
					charData?.UpdateDeleteState(E_CharStateType.DeleteWait, resCharDelete.DeleteDt);
				}

				_onReceive?.Invoke(recvPacket, resCharDelete);
			}, _onError);
		}

		/// <summary> 케릭터 삭제 롤백 요청 </summary>
		public void REQ_CharDeleteRollBack(ulong _userId, ulong _charId, System.Action<ZWebRecvPacket, ResCharDeleteRollBack> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharDeleteRollBack.CreateReqCharDeleteRollBack(mBuilder,
				_userId, _charId);
			var reqPacket = ZWebPacket.Create<ReqCharDeleteRollBack>(this, Code.GS_CHARACTER_DELETE_ROLLBACK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharDeleteRollBack resCharDelRollback = recvPacket.Get<ResCharDeleteRollBack>();

				var charData = ZNet.Data.Global.GetChar(_userId, _charId);
				charData?.UpdateDeleteState(E_CharStateType.Active, 0);

				_onReceive?.Invoke(recvPacket, resCharDelRollback);
			}, _onError);
		}

		/// <summary> 캐릭터의 스텟 포인트 사용 </summary>
		/// <param name="_statDic">[Key: E_AbilityType, Value: Point]</param>
		/// <param name="_useItemId">획득한 스텟포인트 아이템의 ID</param>
		public void REQ_UseStatPoint(Dictionary<uint, uint> _statDic, ulong _useItemId, System.Action<ZWebRecvPacket, ResUseStatPoint> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var stats = new Offset<UseStatPoint>[_statDic.Count];

			int addCnt = 0;
			foreach (uint statType in _statDic.Keys)
			{
				stats[addCnt] = UseStatPoint.CreateUseStatPoint(mBuilder, statType, _statDic[statType]);
				addCnt++;
			}

			var offset = ReqUseStatPoint.CreateReqUseStatPoint(mBuilder,
				ReqUseStatPoint.CreateStatsVector(mBuilder, stats), _useItemId);
			var reqPacket = ZWebPacket.Create<ReqUseStatPoint>(this, Code.GS_USE_STAT_POINT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResUseStatPoint resuseStatPoint = recvPacket.Get<ResUseStatPoint>();

				List<UseStatPoint> statsResult = new List<UseStatPoint>();
				for (int i = 0; i < resuseStatPoint.CurrCharStatsLength; i++)
					statsResult.Add(resuseStatPoint.CurrCharStats(i).Value);

				Me.CurCharData.UpdateBonusStatList(statsResult);

				for (int i = 0; i < resuseStatPoint.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(resuseStatPoint.ResultStackItems(i).Value);

				_onReceive?.Invoke(recvPacket, resuseStatPoint);
			}, _onError);
		}

		/// <summary> 캐릭터의 스탯 초기화 </summary>
		/// <param name="_useItemId">스탯 초기화시 사용될 재화 아이템</param>
		public void REQ_ResetStatPoint(ulong _useItemId, uint _useItemTid, System.Action<ZWebRecvPacket, ResResetStatPoint> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqResetStatPoint.CreateReqResetStatPoint(mBuilder, ZGameManager.Instance.GetMarketType(), _useItemId, _useItemTid);
			var reqPacket = ZWebPacket.Create<ReqResetStatPoint>(this, Code.GS_RESET_STAT_POINT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResResetStatPoint resResetStatPoint = recvPacket.Get<ResResetStatPoint>();

				Me.CurCharData.ClearStatList();

				for (int i = 0; i < resResetStatPoint.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(resResetStatPoint.ResultStackItems(i).Value);

				Me.CurUserData.SetCash(resResetStatPoint.ResultCashCoinBalance);

				_onReceive?.Invoke(recvPacket, resResetStatPoint);
			}, _onError);
		}

		/// <summary> 현재 캐릭터의 클래스를 변경 요청 </summary>
		/// <param name="_newCharTid">변경될 캐릭터TID</param>
		/// <param name="_useItemTid">클레스 체인지용 아이템 아이디</param>
		public void REQ_ChangeClass(uint _newCharTid, uint _useItemTid, System.Action<ZWebRecvPacket, ResClassChange> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqClassChange.CreateReqClassChange(mBuilder,
				_newCharTid, _useItemTid);
			var reqPacket = ZWebPacket.Create<ReqClassChange>(this, Code.GS_CLASS_CHANGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				_onReceive?.Invoke(recvPacket, recvPacket.Get<ResClassChange>());
			}, _onError);
		}

		#endregion//========:: Manage Character ::========

		#region ========:: Manage Stage & Channel ::========

		///// <summary> [mmo기반으로 오면서 이제 필요없을수도 있음] 최초 접속시 or 순단후 접속시 필요한 요청 </summary>
		///// <param name="_isFirstEnter">로그인후 처음 접속하는건지, 순단후 인지.</param>
		///// <param name="_stageTid">순단후 접속이면 서버는 StageTid를 모르기 때문에 필요</param>
		///// <param name="_isPrivateChannel">전용채널인지 여부</param>
		//[System.Obsolete("mmo기반으로 오면서 필요없을수도 있음. 서버팀에서 필요없다고 결정나면 삭제바람.")]
		//public void REQ_CreateSession(bool _isFirstEnter, uint _stageTid, System.Action<ZWebRecvPacket, ResCreateSession> _onReceive, PacketErrorCBDelegate _onError = null)
		//{
		//	var offset = ReqCreateSession.CreateReqCreateSession(mBuilder,
		//		_isFirstEnter,
		//		_stageTid);
		//	var reqPacket = ZWebPacket.Create<ReqCreateSession>(this, Code.GS_CREATE_SESSINON, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResCreateSession resCreateSession = recvPacket.Get<ResCreateSession>();
		//		// 새로운 키로 교체해야함
		//		this.SecurityKey = resCreateSession.Rnd;

		//		_onReceive?.Invoke(recvPacket, resCreateSession);
		//	}, _onError);
		//}

		/// <summary> 로그인후 최초 스테이지 입장시 사용. </summary>
		/// <param name="_channelId">출시버전에서는 0, 개발버전에서는 필요에 의한 채널로 입장</param>
		public void REQ_EnterMMOServer(ushort _channelId, System.Action<ZWebRecvPacket, ResMmoStageEnter> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqMmoStageEnter.CreateReqMmoStageEnter(mBuilder, _channelId);
			var reqPacket = ZWebPacket.Create<ReqMmoStageEnter>(this, Code.GS_MMO_STAGE_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResMmoStageEnter resMmoStageEnter = recvPacket.Get<ResMmoStageEnter>();

				_onReceive?.Invoke(recvPacket, resMmoStageEnter);
			}, _onError);
		}

		/// <summary> 포탈 정보를 이용한 입장 요청 </summary>
		/// <param name="_portalTid">포탈 테이블 아이디</param>
		/// <param name="_useItemId">포탈 사용 소비 아이템 고유 id</param>
		/// <param name="_useItemTid">포탈 사용 소비 아이템 TID</param>
		/// <param name="_onReceive"></param>
		public void REQ_EnterPortal(uint _portalTid, bool _bChaosChannel, ulong _useItemId, uint _useItemTid, System.Action<ZWebRecvPacket, ResPortalEnter> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPortalEnter.CreateReqPortalEnter(mBuilder,
				_portalTid,
				(byte)(_bChaosChannel ? 1 : 0),
				_useItemId,
				_useItemTid);
			var reqPacket = ZWebPacket.Create<ReqPortalEnter>(this, Code.GS_MMO_PORTAL_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPortalEnter resPortalEnter = recvPacket.Get<ResPortalEnter>();
				LastPortalTime = Time.time;

				_onReceive?.Invoke(recvPacket, resPortalEnter);
			}, _onError);
		}

		// 이카루스에서 삭제상태
		///// <summary> 전용 채널 입장 </summary>
		///// <param name="_stageTid">입장할 스테이지 테이블 아이디</param>
		///// <param name="_curPos">현재 캐릭터 위치 좌표</param>
		//public void REQ_EnterChangePrivateChannel(uint _stageTid, Vector3 _curPos, System.Action<ZWebRecvPacket, ResPrivateChannelChangeEnter> _onReceive)
		//{
		//	var offset = ReqPrivateChannelChangeEnter.CreateReqPrivateChannelChangeEnter(mBuilder, 
		//		_stageTid, 
		//		_curPos.x, _curPos.y, _curPos.z);
		//	var reqPacket = ZWebPacket.Create<ReqPrivateChannelChangeEnter>(this, Code.GS_MMO_CHANGE_PRIVATE_CHANNEL_ENTER, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResPrivateChannelChangeEnter resPrivateChannelChangeEnter = recvPacket.Get<ResPrivateChannelChangeEnter>();

		//		// TODO : 서버와 얘기필요
		//		//NetData.CurrentCharacter.LastChannelId = recvMsgPacket.ChannelId;
		//		//NetData.StartPosition.x = recvMsgPacket.PosX;
		//		//NetData.StartPosition.y = recvMsgPacket.PosY;
		//		//NetData.StartPosition.z = recvMsgPacket.PosZ;

		//		_onReceive?.Invoke(recvPacket, resPrivateChannelChangeEnter);
		//	});
		//}

		///// <summary> 전용 채널 퇴장 </summary>
		//public void REQ_ExitPrivateChannel(uint _stageTid, Vector3 _curPos, System.Action<ZWebRecvPacket, ResPrivateChannelOut> _onReceive)
		//{
		//	var offset = ReqPrivateChannelOut.CreateReqPrivateChannelOut(mBuilder, 
		//		_stageTid, 
		//		_curPos.x, _curPos.y, _curPos.z);
		//	var reqPacket = ZWebPacket.Create<ReqPrivateChannelChangeEnter>(this, Code.GS_MMO_PRIVATE_CHANNEL_OUT, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResPrivateChannelOut resPrivateChannelOut = recvPacket.Get<ResPrivateChannelOut>();

		//		// TODO : 서버와 얘기필요
		//		//NetData.CurrentCharacter.LastChannelId = resPrivateChannelOut.ChannelId;
		//		//NetData.StartPosition.x = resPrivateChannelOut.PosX;
		//		//NetData.StartPosition.y = resPrivateChannelOut.PosY;
		//		//NetData.StartPosition.z = resPrivateChannelOut.PosZ;

		//		_onReceive?.Invoke(recvPacket, resPrivateChannelOut);
		//	});
		//}

		///// <summary></summary>
		///// <param name="_isReconnect">같은 지역 재접속시도시 true (mmo->싱글로 가야되는 상황에 사용)</param>
		//public void REQ_EnterChangeChannel(bool _isReconnect, uint _stageTid, ushort _channelId, System.Action<ZWebRecvPacket, ResChannelChangeEnter> _onReceive)
		//{
		//	var offset = ReqChannelChangeEnter.CreateReqChannelChangeEnter(mBuilder,
		//		_isReconnect, 
		//		_stageTid, 
		//		_channelId);
		//	var reqPacket = ZWebPacket.Create<ReqPrivateChannelChangeEnter>(this, Code.GS_MMO_CHANGE_CHANNEL_ENTER, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResChannelChangeEnter resChannelChangeEnter = recvPacket.Get<ResChannelChangeEnter>();

		//		// TODO : 서버와 얘기필요
		//		//NetData.CurrentCharacter.LastChannelId = resChannelChangeEnter.ChannelId;
		//		//NetData.StartPosition.x = resChannelChangeEnter.PosX;
		//		//NetData.StartPosition.y = resChannelChangeEnter.PosY;
		//		//NetData.StartPosition.z = resChannelChangeEnter.PosZ;

		//		_onReceive?.Invoke(recvPacket, resChannelChangeEnter);
		//	});
		//}

		///// <summary> 해당 스테이지에 접속 가능한 서버리스트 요청 </summary>
		public void REQ_MMOChannelList(uint _stageTid, System.Action<ResMmoChannelList> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqMmoChannelList.CreateReqMmoChannelList(mBuilder, _stageTid);
			var reqPacket = ZWebPacket.Create<ReqMmoChannelList>(this, Code.GS_MMO_CHANNEL_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				_onReceive?.Invoke(recvPacket.Get<ResMmoChannelList>());
			}, _onError);
		}

		public void REQ_MMOChangeChannel(uint _stageID, ushort _channelID, System.Action<uint, ushort, string> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChannelChangeEnter.CreateReqChannelChangeEnter(mBuilder, _stageID, _channelID);
			var reqPacket = ZWebPacket.Create<ReqChannelChangeEnter>(this, Code.GS_MMO_CHANGE_CHANNEL_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChannelChangeEnter resChannelChangeEnter = recvPacket.Get<ResChannelChangeEnter>();
				LastPortalTime = Time.time;

				_onReceive?.Invoke(resChannelChangeEnter.StageTid, resChannelChangeEnter.ChannelId, resChannelChangeEnter.JoinAddr);

			}, _onError);
		}

		#endregion

		#region ========:: Time ::========

		//public void REQ_GetServerTime(System.Action<ZWebRecvPacket, ResGetServerTime> _onReceive)
		//{
		//	uint stageTid = 0;
		//	//if (ZGameManager.hasInstance && ZGameManager.instance.CurGameMode)
		//	//{
		//	//	stageTid = ZGameManager.instance.CurGameMode.StageTID;
		//	//}
		//	//else
		//	//{
		//	//	stageTid = NetData.CurrentCharacter != null ? NetData.CurrentCharacter.LastArea : 0;
		//	//}

		//	var offset = ReqGetServerTime.CreateReqGetServerTime(mBuilder,
		//		stageTid, TimeManager.NowMs, (uint)WebNet.Version.Num);
		//	var reqPacket = ZWebPacket.Create<ReqGetServerTime>(this, Code.GS_GET_SERVER_TIME, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		if (recvPacket.ErrCode == ERROR.HEADER_ERROR_BUT_IGNORE_PACKET)
		//		{
		//		}
		//		else if (recvPacket.ErrCode != ERROR.NO_ERROR)
		//		{
		//			//if (!WebSocketManager.instance.IsExceptionError(recvPacket, true))
		//			//	MessagePopup.OpenErrorMessagePopup(recvPacket.MessageTypeNumber, (WebNet.ERROR)recvPacket.BaseError_code);
		//		}
		//		else
		//		{
		//			ResGetServerTime resGetServerTime = recvPacket.Get<ResGetServerTime>();
		//			//NetData.SetServerTime(recvMsgPacket.ServerTsMs);
		//			//TimeInvoker.instance.RequestInvoke(ZNetGame.GetServerTime, TimeManager.PingInterval);

		//			// for TimeManager
		//			{
		//				TimeManager.Instance.SetTime(resGetServerTime.ClientTime, resGetServerTime.ServerTsMs);
		//			}

		//			List<Buff> buffList = new List<Buff>();
		//			for (int i = 0; i < resGetServerTime.ChangeBuffInfosLength; i++)
		//				buffList.Add(resGetServerTime.ChangeBuffInfos(i).Value);

		//			//NetData.Instance.RefreshBuff(NetData.UserID, NetData.CharID, buffList);

		//			_onReceive?.Invoke(recvPacket, resGetServerTime);
		//		}
		//	}, null);
		//}
		#endregion

		#region ========:: Character ::========

		/// <summary> </summary>
		static public readonly byte[] CharacterAllBits = new byte[]
		{
			(byte)E_CharInfoReqBit.CHARACTER,
			(byte)E_CharInfoReqBit.BLOCK,
			(byte)E_CharInfoReqBit.RESTORE_EXP,
			(byte)E_CharInfoReqBit.STORAGE,
			(byte)E_CharInfoReqBit.ACCOUNT_STACK,
			(byte)E_CharInfoReqBit.EQUIP,
			(byte)E_CharInfoReqBit.STACK,
			(byte)E_CharInfoReqBit.MARK,
			(byte)E_CharInfoReqBit.CHANGE,
			(byte)E_CharInfoReqBit.CHANGE_ENCHANT,
			(byte)E_CharInfoReqBit.CHANGE_GACHA_KEEP,
			(byte)E_CharInfoReqBit.PET,
			(byte)E_CharInfoReqBit.PET_ENCHANT,
			(byte)E_CharInfoReqBit.PET_GACHA_KEEP,
			(byte)E_CharInfoReqBit.CHANGE_COLLECT,
			(byte)E_CharInfoReqBit.ITEM_COLLECT,
			(byte)E_CharInfoReqBit.PET_COLLECT,
			(byte)E_CharInfoReqBit.GUILD,
			(byte)E_CharInfoReqBit.GUILD_BUFF,
			(byte)E_CharInfoReqBit.GUILD_ALLIANCE,
			(byte)E_CharInfoReqBit.SKILL_BOOK,
			(byte)E_CharInfoReqBit.PET_ADVENTURE,
			(byte)E_CharInfoReqBit.OPTION,
			(byte)E_CharInfoReqBit.QUEST,
			(byte)E_CharInfoReqBit.DAILY_QUEST,
			(byte)E_CharInfoReqBit.MAKE_LIMIT,
			(byte)E_CharInfoReqBit.FRIEND,
			(byte)E_CharInfoReqBit.REQUEST_FRIEND,
			(byte)E_CharInfoReqBit.RUNE,
			(byte)E_CharInfoReqBit.ITEM_ACQ_HISTORY,
			(byte)E_CharInfoReqBit.EVENT_QUEST,
			(byte)E_CharInfoReqBit.CHANGE_QUEST,

			(byte)E_CharInfoReqBit.ATTRIBUTE,
			(byte)E_CharInfoReqBit.ARTIFACT,
			(byte)E_CharInfoReqBit.COOK_HISTORY,
			(byte)E_CharInfoReqBit.TEMPLE_OPEN_INFO,
			(byte)E_CharInfoReqBit.SKILL_USE_ORDER,
		};

		///// <summary>캐릭터의 일부 정보 받아오기</summary>
		//static public readonly byte[] CharacterBits = new byte[]
		//{
		//	(byte)E_CharInfoReqBit.CHARACTER,
		//	(byte)E_CharInfoReqBit.BUFF,
		//	(byte)E_CharInfoReqBit.GUILD,
		//	(byte)E_CharInfoReqBit.GUILD_BUFF,
		//	(byte)E_CharInfoReqBit.EQUIP,
		//	(byte)E_CharInfoReqBit.ITEM_COLLECT,
		//	(byte)E_CharInfoReqBit.MARK,
		//	(byte)E_CharInfoReqBit.CHANGE,
		//	(byte)E_CharInfoReqBit.CHANGE_ENCHANT,
		//	(byte)E_CharInfoReqBit.CHANGE_COLLECT,
		//	(byte)E_CharInfoReqBit.PET,
		//	(byte)E_CharInfoReqBit.PET_ENCHANT,
		//	(byte)E_CharInfoReqBit.PET_COLLECT,
		//	(byte)E_CharInfoReqBit.SKILL_BOOK,
		//	(byte)E_CharInfoReqBit.RUNE,
		//	(byte)E_CharInfoReqBit.ITEM_ACQ_HISTORY,
		//	(byte)E_CharInfoReqBit.DAILY_QUEST,
		//};

		public static void GetCharacterAllInfo(bool bInitData, ulong _userId, ulong _charId, System.Action onSuccess)
		{
			//ZLog.BeginProfile("IsReceivedOnlyCharInfo");

			//GetAllCharInfoBundle(bInitData, _userId, _charId, REQ_CharacterAllBits, (arg1, arg2) =>
			//{
			//	// 캐릭터 정보만 있으면 일단 캐릭터 생성 요청 가능
			//	ZNetGame.IsReceivedOnlyCharInfo = true;
			//	ZLog.EndProfile("IsReceivedOnlyCharInfo");

			//	ZLog.BeginProfile("IsReceivedCharAllInfo");
			//	//GetCharacterOptionList((msg1, msg2) =>{
			//	CheckParty(NetData.ConnectedServerId, _charId, (partyArg1, partyArg2) =>
			//	{
			//		if (partyArg2.IsParty)
			//			ZNetGame.GetPartyMember(NetData.ConnectedServerId, NetData.CharID, partyArg2.PartyUid, null);

			//		ZNetGame.IsReceivedCharAllInfo = true;
			//		ZLog.EndProfile("IsReceivedCharAllInfo");

			//		if (!PlatformSpecific.IsUnityServer) MiniMapUI.bCheckStart = true;

			//		CheckDailyResetEvent((arg41, arg42) =>
			//		{
			//			ChangeQuestDailyReset((arg43, arg44) =>
			//			{
			//				GetMailList((arg51, arg52) =>
			//				{
			//					GetMessageList((arg61, arg62) =>
			//					{
			//						GetExchangeMessageList((arg71, arg72) =>
			//						{
			//							//GetQuestList((arg81, arg82) => {
			//							//GetDailyQuestList((arg91, arg92) => {
			//							GetBuyLimitList((arg101, arg102) =>
			//							{
			//								//GetMakeLimitList((arg111, arg112) => {
			//								//GetFriendList((x1, y1) => {
			//								//GetRequestFriendList((x2, y2) => {
			//								GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 0, (arg121, arg122) =>
			//								{
			//									GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 1, (arg131, arg132) =>
			//									{
			//										GetColosseumResult((arg141, arg142) =>
			//										{
			//											if (arg142.IsHaveResult == 1 && !PlatformSpecific.IsUnityServer && arg142.ResultAccountStackItemsLength > 0)
			//												UIManager.NoticeMessage(DBLocale.GetLocaleText("WPvP_Duel_RewardNotice"));


			//											GetMailRefreshTime();
			//											CheckRecvEvent();
			//											onSuccess?.Invoke();
			//										});
			//									});
			//								});
			//							});
			//						});
			//						//});
			//						//});
			//						//});
			//						//});
			//						//});
			//					});
			//				});
			//			});
			//		});
			//	});
			//	//});
			//});
		}

		/// <summary> 캐릭터가 가지고 있는 모든 정보를 요청한다. </summary>
		public void REQ_GetAllCharInfoBundle(ulong _userId, ulong _charId, byte[] _charInfoReqBits, bool _bFirstEnter, bool _bOverride, System.Action<ZWebRecvPacket, ResGetAllCharInfoBundle> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var infoBitOffset = ReqGetAllCharInfoBundle.CreateCharInfoReqNumVector(mBuilder, _charInfoReqBits);

			var offset = ReqGetAllCharInfoBundle.CreateReqGetAllCharInfoBundle(mBuilder, _bFirstEnter, infoBitOffset);
			var reqPacket = ZWebPacket.Create<ReqGetAllCharInfoBundle>(this, Code.COMMON_GET_ALL_CHAR_INFO_BUNDLE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetAllCharInfoBundle resGetAllCharInfoBundle = recvPacket.Get<ResGetAllCharInfoBundle>();
				// 패킷용 보안키 새로 적용
				this.SecurityKey = resGetAllCharInfoBundle.Rnd;

				//NetData.Instance.AddAllCharInfoBundle(bInitData, _userId, _charId, ref resGetAllCharInfoBundle);
				var userData = ZNet.Data.Global.GetUser(_userId);
				userData.AddAllCharData(ref resGetAllCharInfoBundle, _bOverride);
				//userData.AddChar(ref resGetAllCharInfoBundle, true);

				_onReceive?.Invoke(recvPacket, resGetAllCharInfoBundle);
			}, _onError);
		}

		public void REQ_GetCashBalance(System.Action<ZWebRecvPacket, ResGetCashCoinBalance> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetCashCoinBalance.CreateReqGetCashCoinBalance(mBuilder, ZGameManager.Instance.GetMarketType());
			var reqPacket = ZWebPacket.Create<ReqGetCashCoinBalance>(this, Code.GS_GET_CASH_COIN_BALANCE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetCashCoinBalance recvMsgPacket = recvPacket.Get<ResGetCashCoinBalance>();

				Me.CurUserData.SetCash(recvMsgPacket.CashCoinBalance);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion

		public void REQ_SKillUseOrderSet(List<SkillOrderData> list, uint CharacterType, System.Action<ZWebRecvPacket, ResSetSkillUseOrder> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var items = new Offset<SkillUseOrder>[list.Count];

			for (int i = 0; i < list.Count; i++)
			{
				items[i] = SkillUseOrder.CreateSkillUseOrder(mBuilder, list[i].Tid, CharacterType, list[i].Order, list[i].CoolTime, Convert.ToByte(list[i].IsUseSkillCycle));
			}

			var vector = ReqSetSkillUseOrder.CreateSkillUseOrdersVector(mBuilder, items);
			var offset = ReqSetSkillUseOrder.CreateReqSetSkillUseOrder(mBuilder, vector);

			var reqPacket = ZWebPacket.Create<ReqSetSkillUseOrder>(this, Code.GS_SET_SKILL_USE_ORDER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSetSkillUseOrder recvMsgPacket = recvPacket.Get<ResSetSkillUseOrder>();

				for (int i = 0; i < recvMsgPacket.SkillUseOrdersLength; i++)
				{
					Me.CurCharData.AddSkillUseOrder(recvMsgPacket.SkillUseOrders(i));
				}

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#region ========:: Item ::========
		public void REQ_ItemUse(ulong UseItemID, uint UseItemTid, uint Cnt, System.Action<ZWebRecvPacket, ResItemUse> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemUse.CreateReqItemUse(mBuilder, UseItemID, UseItemTid, Cnt);

			var reqPacket = ZWebPacket.Create<ReqItemUse>(this, Code.GS_ITEM_USE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemUse recvMsgPacket = recvPacket.Get<ResItemUse>();

				ZNet.Data.CharacterData userData = ZNet.Data.Global.GetChar(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID);

				if (recvMsgPacket.RemainStack != null)
					userData.AddItemList(recvMsgPacket.RemainStack.Value);

				//AlramUI.CheckAlram(Alram.CHECK_SPEED_BUFF | Alram.CHECK_BLESS_BUFF);
				//	 userData. CallItemUpdate( UseItemTid);
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);

			//ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_ITEM_USE);

			//	builder.Clear();

			//	var offset = ReqItemUse.CreateReqItemUse(builder, UseItemID, UseItemTid, Cnt);

			//	builder.Finish(offset.Value);
			//	reqPacket.AddBuilderMsg<ReqItemUse>(builder);

			//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
			//	{
			//		ResItemUse recvMsgPacket = recvPacket.GetResMsg<ResItemUse>();

			//		List<Buff> buffList = new List<Buff>();

			//		for (int i = 0; i < recvMsgPacket.AppliedBuffLength; i++)
			//			buffList.Add(recvMsgPacket.AppliedBuff(i).Value);

			//		NetData.Instance.AddBuff(NetData.UserID, NetData.CharID, buffList);

			//		if (recvMsgPacket.RemainStack != null)
			//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.RemainStack.Value);

			//		AlramUI.CheckAlram(Alram.CHECK_SPEED_BUFF | Alram.CHECK_BLESS_BUFF);

			//		NetData.Instance.CheckCallItemUpdate(NetData.UserID, NetData.CharID, UseItemTid);

			//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			//	});

		}

		#endregion

		#region ========:: PetChangeReOpenGaca ::========
		public void REQ_PetReOpenSelect(ulong KeepId, uint KeepTid, System.Action<ZWebRecvPacket, ResPetTakeGachaKeep> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetTakeGachaKeep.CreateReqPetTakeGachaKeep(mBuilder, KeepId, KeepTid);
			var reqPacket = ZWebPacket.Create<ReqPetTakeGachaKeep>(this, Code.GS_PET_TAKE_GACHA_KEEP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetTakeGachaKeep recvMsgPacket = recvPacket.Get<ResPetTakeGachaKeep>();

				if (DBPet.TryGet(KeepTid, out var table) == false)
				{
					ZLog.Log(ZLogChannel.Pet, $"PET OR RIDE HAS NO VALUE : TID : {KeepTid}");
					return;
				}

				if (table.PetType == GameDB.E_PetType.Pet)
				{
					ZNet.Data.Me.CurCharData.RemovePetKeep(KeepId);
				}
				else if (table.PetType == GameDB.E_PetType.Vehicle)
				{
					ZNet.Data.Me.CurCharData.RemoveRideKeep(KeepId);
				}

				List<Pet> petList = new List<Pet>();
				List<Pet> rideList = new List<Pet>();

				for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPets(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var pTable) == false)
						continue;

					if (pTable.PetType == GameDB.E_PetType.Pet)
					{
						petList.Add(resultData);
					}
					else if (pTable.PetType == GameDB.E_PetType.Vehicle)
					{
						rideList.Add(resultData);
					}
				}
				ZNet.Data.Me.CurCharData.AddPetList(petList);
				ZNet.Data.Me.CurCharData.AddRideList(rideList);


				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		public void REQ_PetReOpen(ulong KeepId, uint KeepTid, System.Action<ZWebRecvPacket, ResPetRetryGachaKeep> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetRetryGachaKeep.CreateReqPetRetryGachaKeep(mBuilder, ZGameManager.Instance.GetMarketType(), KeepId, KeepTid);
			var reqPacket = ZWebPacket.Create<ReqPetRetryGachaKeep>(this, Code.GS_PET_RETRY_GACHA_KEEP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetRetryGachaKeep recvMsgPacket = recvPacket.Get<ResPetRetryGachaKeep>();

				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);

				List<PetGachaKeep> petKeepList = new List<PetGachaKeep>();
				List<PetGachaKeep> rideKeepList = new List<PetGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultPetsGachaKeepsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPetsGachaKeeps(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petKeepList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideKeepList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetKeepList(petKeepList);
				Me.CurCharData.AddRideKeepList(rideKeepList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_ChangeReOpenSelect(ulong KeepId, uint KeepTid, System.Action<ZWebRecvPacket, ResChangeTakeGachaKeep> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeTakeGachaKeep.CreateReqChangeTakeGachaKeep(mBuilder, KeepId, KeepTid);
			var reqPacket = ZWebPacket.Create<ReqPetRetryGachaKeep>(this, Code.GS_CHANGE_TAKE_GACHA_KEEP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeTakeGachaKeep recvMsgPacket = recvPacket.Get<ResChangeTakeGachaKeep>();

				Me.CurCharData.RemoveChangeKeep(KeepId);

				List<Change> changeList = new List<Change>();

				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_ChangeReOpen(ulong KeepId, uint KeepTid, System.Action<ZWebRecvPacket, ResChangeRetryGachaKeep> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeRetryGachaKeep.CreateReqChangeRetryGachaKeep(mBuilder, ZGameManager.Instance.GetMarketType(), KeepId, KeepTid);
			var reqPacket = ZWebPacket.Create<ReqChangeRetryGachaKeep>(this, Code.GS_CHANGE_RETRY_GACHA_KEEP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeRetryGachaKeep recvMsgPacket = recvPacket.Get<ResChangeRetryGachaKeep>();

				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);

				List<ChangeGachaKeep> changeKeepList = new List<ChangeGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultChangesGachaKeepsLength; i++)
					changeKeepList.Add(recvMsgPacket.ResultChangesGachaKeeps(i).Value);

				Me.CurCharData.AddChangeKeepList(changeKeepList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}


		#endregion

		#region ========:: Pet ::========

		public void REQ_EquipPet(ulong petId, uint petTid, ulong useItemId, System.Action<ZWebRecvPacket, ResPetEquip> _onReceive, PacketErrorCBDelegate _onError = null)
		{

			var offset = ReqPetEquip.CreateReqPetEquip(mBuilder, petId, petTid, useItemId);
			var reqPacket = ZWebPacket.Create<ReqPetEquip>(this, Code.GS_PET_EQUIP, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetEquip recvMsgPacket = recvPacket.Get<ResPetEquip>();
				ZLog.Log(ZLogChannel.Pet, $"MainPetTid :{ recvMsgPacket.MainPetTid} MainPetExpireSec:{recvMsgPacket.MainPetExpireSec} MainPetExpireSec: {recvMsgPacket.MainPetExpireSec} ");


				List<WebNet.ItemStack> listItem = new List<WebNet.ItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					listItem.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(listItem);
				Me.CurCharData.UpdateMainPet(recvMsgPacket.MainPetTid, recvMsgPacket.MainPetExpireSec);

				var petType = DBPet.GetPetData(petTid).PetType;
				if (petType == GameDB.E_PetType.Pet)
					Me.CurCharData.SetEquipPetChangeRide(petTid, OptionEquipType.TYPE_PET);
				else if (petType == GameDB.E_PetType.Vehicle)
					Me.CurCharData.SetEquipPetChangeRide(petTid, OptionEquipType.TYPE_RIDE);

				REQ_SetCharacterCurrentPreset(null, null);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);

			//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_EQUIP);
			//
			//builder.Clear();
			//
			//var offset = ReqPetEquip.CreateReqPetEquip(builder, ReqPetEquip.CreatePetIdListVector(builder, petIds.ToArray()), ReqPetEquip.CreateSlotIdxListVector(builder, petTypes.ToArray()));
			//
			//builder.Finish(offset.Value);
			//reqPacket.AddBuilderMsg<ReqPetEquip>(builder);
			//
			//WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
			//{
			//	ResPetEquip recvMsgPacket = recvPacket.GetResMsg<ResPetEquip>();
			//
			//	//펫 장착
			//	NetData.Instance.UpdateEquipPets(NetData.UserID, NetData.CharID, petTids, petTypes);
			//	//메인 펫 데이터만 갱신! 등록된 이벤트 호출 안함.
			//	NetData.Instance.UpdatePet(NetData.UserID, NetData.CharID, recvMsgPacket.MainPetTid, NetData.CurrentCharacter.PetExpireDt, false);
			//
			//	NetData.Instance.RemoveEquipPetCurrentSet(NetData.UserID, NetData.CharID);
			//
			//	if (NetData.Instance.AddEquipPetCurrentSet(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.PetEquip1, NetData.CurrentCharacter.PetEquip2, NetData.CurrentCharacter.PetEquip3))
			//	{
			//		switch (NetData.CurrentCharacter.SelectEquipSetNo)
			//		{
			//			case 1:
			//				ZNetGame.SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET1, NetData.Instance.GetEquipSetValue(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.SelectEquipSetNo), null);
			//				break;
			//			case 2:
			//				ZNetGame.SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET2, NetData.Instance.GetEquipSetValue(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.SelectEquipSetNo), null);
			//				break;
			//		}
			//	}
			//
			//	onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			//});
		}

		#endregion
		#region ========:: Guild(길드) ::========

		///<summary>길드 생성</summary> 
		public void REQ_CreateGuild(
			string guildName
			, string guildIntroduction
			, string guildNoti
			, byte guildMarkTID
			, ulong useItemID
			, bool isQuickJoin
			, Action<ZWebRecvPacket, ResCreateGuild> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqCreateGuild.CreateReqCreateGuild(mBuilder
				, mBuilder.CreateString(guildName)
				, mBuilder.CreateString(guildNoti)
				, mBuilder.CreateString(guildIntroduction)
				, guildMarkTID
				, useItemID
				, isQuickJoin
				, 0
				, 0);
			var reqPacket = ZWebPacket.Create<ReqCreateGuild>(this, Code.GS_CREATE_GUILD, mBuilder, offset.Value);
			
			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResCreateGuild>();
				var charData = Me.CurCharData;
				charData.SetGuildInfo(recvMsgPacket.GuildInfo);

				for (int i = 0; i < recvMsgPacket.GuildMemberInfosLength; i++)
				{
					var t = recvMsgPacket.GuildMemberInfos(i);

					if (Me.CurCharData.ID == t.Value.CharId)
					{
						Me.CurCharData.GuildGrade = t.Value.Grade;
						break;
					}
				}

				Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItem);
				_onReceive?.Invoke(recvPacket, recvMsgPacket);

				ZWebManager.Instance.WebChat.CheckEnterChannel();
			}, _onError);
		}

		///<summary>길드 해산</summary> 
		public void REQ_DismissGuild(
			ulong guildID
			, ulong guildChatID
			, E_GuildAllianceChatGrade guildChatGrade
			, Action<ZWebRecvPacket, ResDismissGuild> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqDismissGuild.CreateReqDismissGuild(mBuilder
				, guildID
				, guildChatID
				, guildChatGrade);

			var reqPacket = ZWebPacket.Create<ReqDismissGuild>(this, Code.GS_DISMISS_GUILD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResDismissGuild>();
				Me.CurCharData.ClearGuildInfo();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드원 임명 </summary> 
		public void REQ_AppointGuildMember(
			ulong guildID
			, ulong memberCharID
			, uint guildGrade // E_GuildMemberGrade
			, Action<ZWebRecvPacket, ResAppointGuildMember> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqAppointGuildMember.CreateReqAppointGuildMember(mBuilder
				, guildID
				, memberCharID
				, guildGrade);

			var reqPacket = ZWebPacket.Create<ReqAppointGuildMember>(this, Code.GS_APPOINT_GUILD_MEMBER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResAppointGuildMember>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 가입 요청 정보(길드 기준)</summary> 
		public void REQ_GuildRequestListForGuild(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildRequestListForGuild> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildRequestListForGuild.CreateReqGuildRequestListForGuild(mBuilder
				, guildID);

			var reqPacket = ZWebPacket.Create<ReqGuildRequestListForGuild>(this, Code.GS_GUILD_REQUEST_LIST_FOR_GUILD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestListForGuild>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 가입 요청 정보(케릭터 기준)</summary> 
		public void REQ_GuildRequestListForChar(
			Action<ZWebRecvPacket, ResGuildRequestListForChar> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			ReqGuildRequestListForChar.StartReqGuildRequestListForChar(mBuilder);
			var offset = ReqGuildRequestListForChar.EndReqGuildRequestListForChar(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGuildRequestListForChar>(this, Code.GS_GUILD_REQUEST_LIST_FOR_CHAR, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestListForChar>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 가입 수락 </summary> 
		public void REQ_GuildRequestAccept(
			ulong guildID
			, ulong acceptCharID
			, Action<ZWebRecvPacket, ResGuildRequestAccept> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildRequestAccept.CreateReqGuildRequestAccept(mBuilder
				, guildID
				, acceptCharID);

			var reqPacket = ZWebPacket.Create<ReqGuildRequestAccept>(this, Code.GS_GUILD_REQUEST_ACCEPT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestAccept>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 가입 거절</summary> 
		public void REQ_GuildRequestReject(
			ulong guildID
			, ulong rejectCharID
			, Action<ZWebRecvPacket, ResGuildRequestReject> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildRequestReject.CreateReqGuildRequestReject(mBuilder, guildID, rejectCharID);

			var reqPacket = ZWebPacket.Create<ReqGuildRequestReject>(this, Code.GS_GUILD_REQUEST_REJECT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestReject>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드원 추방</summary> 
		public void REQ_GuildMemberBan(
			ulong guildID
			, ulong banCharID
			, Action<ZWebRecvPacket, ResGuildMemberBan> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildMemberBan.CreateReqGuildMemberBan(mBuilder, guildID, banCharID);

			var reqPacket = ZWebPacket.Create<ReqGuildMemberBan>(this, Code.GS_GUILD_MEMBER_BAN, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildMemberBan>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드원 탈퇴</summary> 
		public void REQ_GuildMemberLeave(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildMemberLeave> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildMemberLeave.CreateReqGuildMemberLeave(mBuilder, guildID);

			var reqPacket = ZWebPacket.Create<ReqGuildMemberLeave>(this, Code.GS_GUILD_MEMBER_LEAVE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildMemberLeave>();
				Me.CurCharData.ClearGuildInfo();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 정보 가져오기 </summary> 
		public void REQ_GetGuildInfo(
			ulong guildID
			, Action<ZWebRecvPacket, ResGetGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGetGuildInfo.CreateReqGetGuildInfo(mBuilder, guildID);
			var reqPacket = ZWebPacket.Create<ReqGetGuildInfo>(this, Code.GS_GET_GUILD_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGetGuildInfo>();
				//	preCallback?.Invoke(recvMsgPacket);

				// 내 길드 
				if (recvMsgPacket.GuildInfo.Value.GuildId == Me.CurCharData.GuildId)
				{
					Me.CurCharData.SetGuildInfo(recvMsgPacket.GuildInfo.Value);

					// 멤버 정보에서 나를 찾아서 Grade 업데이트 
					for (int i = 0; i < recvMsgPacket.GuildMemberInfosLength; i++)
					{
						var t = recvMsgPacket.GuildMemberInfos(i).Value;

						if (Me.CurCharData.ID == t.CharId)
						{
							Me.CurCharData.GuildGrade = t.Grade;
							break;
						}
					}
				}

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 찾기 </summary> 
		public void REQ_FindGuildInfo(
			string guildName
			, Action<ZWebRecvPacket, ResFindGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqFindGuildInfo.CreateReqFindGuildInfo(mBuilder, mBuilder.CreateString(guildName));
			var reqPacket = ZWebPacket.Create<ReqFindGuildInfo>(this, Code.GS_FIND_GUILD_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResFindGuildInfo>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 출석 보상 </summary> 
		public void REQ_GuildAttendReward(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildAttendReward> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAttendReward.CreateReqGuildAttendReward(mBuilder, guildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAttendReward>(this, Code.GS_GUILD_ATTEND_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAttendReward>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>추천 길드</summary> 
		public void REQ_RecommendGuildInfo(
			Action<ZWebRecvPacket, ResRecommendGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			ReqRecommendGuildInfo.StartReqRecommendGuildInfo(mBuilder);
			var offset = ReqRecommendGuildInfo.EndReqRecommendGuildInfo(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqRecommendGuildInfo>(this, Code.GS_RECOMMEND_GUILD_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResRecommendGuildInfo>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 가입 요청 </summary> 
		public void REQ_GuildRequestJoin(
			ulong guildID
			, string comment
			, Action<ZWebRecvPacket, ResGuildRequestJoin, bool> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildRequestJoin.CreateReqGuildRequestJoin(mBuilder, guildID, mBuilder.CreateString(comment));

			var reqPacket = ZWebPacket.Create<ReqGuildRequestJoin>(this, Code.GS_GUILD_REQUEST_JOIN, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestJoin>();
				bool joined = recvMsgPacket.IsQuickJoin && recvMsgPacket.GuildId != 0;

				if (recvMsgPacket.IsQuickJoin && recvMsgPacket.GuildId != 0)
				{
					Me.CurCharData.SetGuildInfo(recvMsgPacket.GuildId, "", 0);
				}
				_onReceive?.Invoke(recvPacket, recvMsgPacket, joined);
			}, _onError);
		}

		///<summary>길드 가입 신청 취소 </summary> 
		public void REQ_GuildRequestJoinCancel(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildRequestJoinCancel> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildRequestJoinCancel.CreateReqGuildRequestJoinCancel(mBuilder, guildID);

			var reqPacket = ZWebPacket.Create<ReqGuildRequestJoinCancel>(this, Code.GS_GUILD_REQUEST_JOIN_CANCEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildRequestJoinCancel>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 정보 리스트 가져오기 </summary> 
		public void REQ_GetGuildAllianceList(
			ulong guildID, E_GuildAllianceState[] states
			, Action<ZWebRecvPacket, ResGetGuildAllianceList> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGetGuildAllianceList.CreateReqGetGuildAllianceList(
				mBuilder
				, guildID
				, ReqGetGuildAllianceList.CreateStatesVector(mBuilder, states));

			var reqPacket = ZWebPacket.Create<ReqGetGuildAllianceList>(this, Code.GS_GET_GUILD_ALLIANCE_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGetGuildAllianceList>();

				Me.CurCharData.ClearAllianceGuildList();

				for (int i = 0; i < recvMsgPacket.GuildAllianceInfosLength; i++)
				{
					var t = recvMsgPacket.GuildAllianceInfos(i);

					Me.CurCharData.AddAllianceGuild(new GuildSimpleData()
					{
						GuildId = t.Value.GuildId,
						State = t.Value.State
					});
				}

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 추가 요청 </summary> 
		public void REQ_GuildAllianceRequest(
			ulong guildId
			, bool isEnemy
			, string targetGuildName
			, Action<ZWebRecvPacket, ResGuildAllianceRequest> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceRequest.CreateReqGuildAllianceRequest(mBuilder, guildId, isEnemy, mBuilder.CreateString(targetGuildName));

			var reqPacket = ZWebPacket.Create<ReqGuildAllianceRequest>(this, Code.GS_GUILD_ALLIANCE_REQUEST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceRequest>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 추가 요청 승인 </summary> 
		public void REQ_GuildAllianceAccept(
			ulong guildId
			, string guildName
			, ulong targetGuildId
			, Action<ZWebRecvPacket, ResGuildAllianceAccept> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceAccept.CreateReqGuildAllianceAccept(mBuilder, guildId, mBuilder.CreateString(guildName), targetGuildId);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceAccept>(this, Code.GS_GUILD_ALLIANCE_ACCEPT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceAccept>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary>
		/// 길드 연맹 삭제 
		/// </summary>
		public void REQ_GuildAllianceRemove(
			ulong guildId
			, string guildName
			, bool isEnemy
			, ulong targetGuildID
			, string targetGuildName
			, Action<ZWebRecvPacket, ResGuildAllianceRemove> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceRemove.CreateReqGuildAllianceRemove(mBuilder, guildId, mBuilder.CreateString(guildName), isEnemy, targetGuildID, mBuilder.CreateString(targetGuildName));
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceRemove>(this, Code.GS_GUILD_ALLIANCE_REMOVE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceRemove>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 추가 요청 거부 or 취소 </summary> 
		public void REQ_GuildAllianceReject(
			ulong guildId
			, ulong targetGuildId
			, Action<ZWebRecvPacket, ResGuildAllianceReject> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceReject.CreateReqGuildAllianceReject(mBuilder, guildId, targetGuildId);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceReject>(this, Code.GS_GUILD_ALLIANCE_REJECT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceReject>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 생성 </summary> 
		public void REQ_GuildAllianceCreateChat(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildAllianceCreateChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceCreateChat.CreateReqGuildAllianceCreateChat(mBuilder, guildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceCreateChat>(this, Code.GS_GUILD_ALLIANCE_CREATE_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceCreateChat>();
				if (recvMsgPacket.GuildInfo.HasValue && Me.CurCharData.GuildId == recvMsgPacket.GuildInfo.Value.GuildId)
				{
					var guildInfo = recvMsgPacket.GuildInfo.Value;
					Me.CurCharData.SetGuildChatInfo(guildInfo.ChatId, guildInfo.ChatState, guildInfo.ChatGrade);
				}
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 입장 요청 </summary> 
		public void REQ_GuildAllianceRequestChat(
			ulong guildID
			, ulong masterGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceRequestChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceRequestChat.CreateReqGuildAllianceRequestChat(mBuilder, guildID, masterGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceRequestChat>(this, Code.GS_GUILD_ALLIANCE_REQUEST_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceRequestChat>();
				Me.CurCharData.SetGuildChatInfo(recvMsgPacket.GuildInfo.Value.ChatId, recvMsgPacket.GuildInfo.Value.ChatState, recvMsgPacket.GuildInfo.Value.ChatGrade);
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 입장 요청 취소 or 초대 요청 받은것 취소 </summary> 
		public void REQ_GuildAllianceCancelChat(
			ulong guildID
			, Action<ZWebRecvPacket, ResGuildAllianceCancelChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceCancelChat.CreateReqGuildAllianceCancelChat(mBuilder, guildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceCancelChat>(this, Code.GS_GUILD_ALLIANCE_CANCEL_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceCancelChat>();
				Me.CurCharData.SetGuildChatInfo(recvMsgPacket.GuildInfo.Value.ChatId, recvMsgPacket.GuildInfo.Value.ChatState, recvMsgPacket.GuildInfo.Value.ChatGrade);
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 입장 수락 </summary> 
		public void REQ_GuildAllianceAcceptChat(
			ulong guildID
			, ulong acceptGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceAcceptChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceAcceptChat.CreateReqGuildAllianceAcceptChat(mBuilder, guildID, acceptGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceAcceptChat>(this, Code.GS_GUILD_ALLIANCE_ACCEPT_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceAcceptChat>();

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 입장 거절 </summary> 
		public void REQ_GuildAllianceRejectChat(
			ulong guildID
			, ulong rejectGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceRejectChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceRejectChat.CreateReqGuildAllianceRejectChat(mBuilder, guildID, rejectGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceRejectChat>(this, Code.GS_GUILD_ALLIANCE_REJECT_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceRejectChat>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 초대 요청 </summary> 
		public void REQ_GuildAllianceInviteRequestChat(
			ulong guildID
			, ulong inviteGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceInviteRequestChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceInviteRequestChat.CreateReqGuildAllianceInviteRequestChat(mBuilder, guildID, inviteGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceInviteRequestChat>(this, Code.GS_GUILD_ALLIANCE_INVITE_REQUEST_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceInviteRequestChat>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 초대 요청 취소</summary> 
		public void REQ_GuildAllianceInviteCancelChat(
			ulong guildID
			, ulong cancelGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceInviteCancelChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceInviteCancelChat.CreateReqGuildAllianceInviteCancelChat(mBuilder, guildID, cancelGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceInviteCancelChat>(this, Code.GS_GUILD_ALLIANCE_INVITE_CANCEL_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceInviteCancelChat>();

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 초대 수락 </summary> 
		public void REQ_GuildAllianceInviteAcceptChat(
		ulong guildID
		, Action<ZWebRecvPacket, ResGuildAllianceInviteAcceptChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceInviteAcceptChat.CreateReqGuildAllianceInviteAcceptChat(mBuilder, guildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceInviteAcceptChat>(this, Code.GS_GUILD_ALLIANCE_INVITE_ACCEPT_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceInviteAcceptChat>();
				Me.CurCharData.SetGuildChatInfo(recvMsgPacket.GuildInfo.Value.ChatId, recvMsgPacket.GuildInfo.Value.ChatState, recvMsgPacket.GuildInfo.Value.ChatGrade);
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 탈퇴</summary> 
		public void REQ_GuildAllianceLeaveChat(
			ulong guildID
			, ulong chatID
			, E_GuildAllianceChatGrade chatGrade
			, Action<ZWebRecvPacket, ResGuildAllianceLeaveChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceLeaveChat.CreateReqGuildAllianceLeaveChat(mBuilder, guildID, chatID, chatGrade);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceLeaveChat>(this, Code.GS_GUILD_ALLIANCE_LEAVE_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceLeaveChat>();
				if (guildID == Me.CurCharData.GuildId)
				{
					Me.CurCharData.SetGuildChatInfo(0, E_GuildAllianceChatState.None, E_GuildAllianceChatGrade.None);
				}
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 채팅 강퇴</summary> 
		public void REQ_GuildAllianceBanChat(
			ulong guildID
			, ulong targetGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceBanChat> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceBanChat.CreateReqGuildAllianceBanChat(mBuilder, guildID, targetGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceBanChat>(this, Code.GS_GUILD_ALLIANCE_BAN_CHAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceBanChat>();

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 연맹 마스터 변경</summary> 
		public void REQ_GuildAllianceChangeMaster(
			ulong guildID
			, ulong changeGuildID
			, Action<ZWebRecvPacket, ResGuildAllianceChangeMaster> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildAllianceChangeMaster.CreateReqGuildAllianceChangeMaster(mBuilder, guildID, changeGuildID);
			var reqPacket = ZWebPacket.Create<ReqGuildAllianceChangeMaster>(this, Code.GS_GUILD_ALLIANCE_CHANGE_MASTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildAllianceChangeMaster>();

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 랭킹 정보 가져오기 </summary> 
		public void REQ_GetGuildExpRank(Action<ZWebRecvPacket, ResGetGuildExpRank> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			ReqGetGuildExpRank.StartReqGetGuildExpRank(mBuilder);
			var offset = ReqGetGuildExpRank.EndReqGetGuildExpRank(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetGuildExpRank>(this, Code.GS_GET_GUILD_EXP_RANK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGetGuildExpRank>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 정보 세팅</summary> 
		public void REQ_UpdateGuildInfo(
			ulong guildID
			, string intro
			, string notice
			, bool isQuickJoin
			, ushort loginBanStep
			, ushort donateBanStep
			, Action<ZWebRecvPacket, ResUpdateGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqUpdateGuildInfo.CreateReqUpdateGuildInfo(
				mBuilder
				, guildID
				, mBuilder.CreateString(intro)
				, mBuilder.CreateString(notice)
				, isQuickJoin
				, loginBanStep
				, donateBanStep);
			var reqPacket = ZWebPacket.Create<ReqUpdateGuildInfo>(this, Code.GS_UPDATE_GUILD_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResUpdateGuildInfo>();
				Me.CurCharData.GuildId = guildID;
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 문양을 수정</summary> 
		public void REQ_UpdateGuildMark(
			ulong guildID
			, byte markTid
			, Action<ZWebRecvPacket, ResUpdateGuildMark> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqUpdateGuildMark.CreateReqUpdateGuildMark(mBuilder, guildID, markTid);

			var reqPacket = ZWebPacket.Create<ReqUpdateGuildMark>(this, Code.GS_UPDATE_GUILD_MARK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResUpdateGuildMark>();
				Me.CurCharData.GuildId = recvMsgPacket.GuildId;
				Me.CurCharData.GuildMarkTid = recvMsgPacket.MarkTid;
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary> 길드 멤버 남긴말 수정 </summary> 
		public void REQ_UpdateGuildMemberComment(
			ulong guildId
			, string comment
			, Action<ZWebRecvPacket, ResUpdateGuildMemberComment> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqUpdateGuildMemberComment.CreateReqUpdateGuildMemberComment(mBuilder, guildId, mBuilder.CreateString(comment));
			var reqPacket = ZWebPacket.Create<ReqUpdateGuildMemberComment>(this, Code.GS_UPDATE_GUILD_MEMBER_COMMENT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResUpdateGuildMemberComment>();
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		///<summary>길드 기부하기 </summary> 
		public void REQ_GuildDonation(
			ulong guildID
			, E_GuildDonationType donationType
			, ulong useItemID
			, Action<ZWebRecvPacket, ResGuildDonation> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqGuildDonation.CreateReqGuildDonation(mBuilder, ZGameManager.Instance.GetMarketType(), guildID, donationType, useItemID);

			var reqPacket = ZWebPacket.Create<ReqGuildDonation>(this, Code.GS_GUILD_DONATION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGuildDonation>();
				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);
				Me.CurCharData.SetGuildInfo(recvMsgPacket.GuildInfo);

				if (Me.CurCharData.ID == recvMsgPacket.GuildMemberInfo.Value.CharId)
				{
					Me.CurCharData.GuildGrade = recvMsgPacket.GuildMemberInfo.Value.Grade;
				}

				// 아이템 제거 
				List<DelItemInfo> delitems = new List<DelItemInfo>();
				delitems.Add(recvMsgPacket.DelItem.Value);
				Me.CurCharData.RemoveItemList(delitems);

				// 재화 처리 
				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i));

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion
		#region ========:: Temple ::========

		/// <summary>오픈된 유적 정보</summary> -> 번들로 옮김
		//public void REQ_TempleOpenInfo( System.Action<ZWebRecvPacket, ResTempleOpenInfo> _onReceive, PacketErrorCBDelegate _onError = null)
		//{
		//	ReqTempleOpenInfo.StartReqTempleOpenInfo(mBuilder);            

		//	   var offset = ReqTempleOpenInfo.EndReqTempleOpenInfo(mBuilder);
		//	var reqPacket = ZWebPacket.Create<ReqTempleOpenInfo>(this, Code.GS_TEMPLE_OPEN_INFO, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResTempleOpenInfo recvMsgPacket = recvPacket.Get<ResTempleOpenInfo>();
		//		ZNet.Data.CharacterData userData = ZNet.Data.Global.GetChar(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID);

		//		for (int i = 0; i < recvMsgPacket.TempleStagesLength; i++)
		//		{
		//			userData.templeCashData.AddStage(recvMsgPacket.TempleStages(i));
		//		}
		//	//	ZLog.Log(ZLogChannel.UI, $"templeStageLength{templeStageLength} " );

		//		_onReceive?.Invoke(recvPacket, recvMsgPacket);

		//	}, _onError);
		//}
		/// <summary>아이템으로 유적 오픈</summary>
		public void REQ_TempleItemOpen(uint useItemTid, System.Action<ZWebRecvPacket, ResTempleItemOpen> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqTempleItemOpen.CreateReqTempleItemOpen(mBuilder, useItemTid);
			var reqPacket = ZWebPacket.Create<ReqTempleItemOpen>(this, Code.GS_TEMPLE_ITEM_OPEN, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTempleItemOpen recvMsgPacket = recvPacket.Get<ResTempleItemOpen>();
				var templeStage = recvMsgPacket.TempleStage;
				var resultStackItem = recvMsgPacket.ResultStackItem;

				Me.CurCharData.AddItemList(resultStackItem);

				//	ZLog.Log(ZLogChannel.UI, $"templeStage{templeStage.Value} resultStackItem{resultStackItem.Value}");

				ZNet.Data.CharacterData userData = ZNet.Data.Global.GetChar(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID);
				userData.TempleInfo.AddStage(templeStage);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary>기믹으로 유적 오픈</summary>
		public void REQ_TempleGimmickOpen(uint objectTid, uint stageTid, System.Action<ZWebRecvPacket, ResTempleGimmickOpen> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqTempleGimmickOpen.CreateReqTempleGimmickOpen(mBuilder, objectTid, stageTid);
			var reqPacket = ZWebPacket.Create<ReqTempleGimmickOpen>(this, Code.GS_TEMPLE_GIMMICK_OPEN, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTempleGimmickOpen recvMsgPacket = recvPacket.Get<ResTempleGimmickOpen>();
				var templeStage = recvMsgPacket.TempleStage;
				///ZLog.Log(ZLogChannel.UI, $"templeStage{templeStage.Value} ");
				//TODO :: 여기서도 아이템이 소모되야함

				ZNet.Data.CharacterData userData = ZNet.Data.Global.GetChar(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID);
				userData.TempleInfo.AddStage(templeStage);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary>유적 상자 오픈</summary>
		public void REQ_TempleGachaItem(uint templeStageTid, uint gachaArrayIndex, System.Action<ZWebRecvPacket, ResTempleGachaItem> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqTempleGachaItem.CreateReqTempleGachaItem(mBuilder, templeStageTid, gachaArrayIndex);
			var reqPacket = ZWebPacket.Create<ReqTempleGachaItem>(this, Code.GS_TEMPLE_GACHA_ITEM, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTempleGachaItem recvMsgPacket = recvPacket.Get<ResTempleGachaItem>();
				var remainItems = recvMsgPacket.RemainItems;
				var items = recvMsgPacket.GetItems;
				var templeStage = recvMsgPacket.TempleStage;
				//ZLog.Log(ZLogChannel.UI, $"MainPetTid :{ templeStage.Value} items:{items.Value} remainItems: {remainItems.Value} ");

				ZNet.Data.CharacterData userData = ZNet.Data.Global.GetChar(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID);
				userData.TempleInfo.AddStage(recvMsgPacket.TempleStage);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary>유적 클리어</summary>
		public void REQ_TempleClearReward(uint templeStageTid, System.Action<ZWebRecvPacket, ResTempleClearReward> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqTempleClearReward.CreateReqTempleClearReward(mBuilder, templeStageTid);
			var reqPacket = ZWebPacket.Create<ReqTempleClearReward>(this, Code.GS_TEMPLE_CLEAR_REWARD, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTempleClearReward recvMsgPacket = recvPacket.Get<ResTempleClearReward>();
				var remainItems = recvMsgPacket.RemainItems;
				var templeStage = recvMsgPacket.TempleStage;
				var items = recvMsgPacket.GetItems;
				//ZLog.Log(ZLogChannel.UI, $"MainPetTid :{ templeStage.Value} items:{items.Value} remainItems: {remainItems.Value} ");

				_onReceive?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		#endregion

		#region 사망후 경험치 복구 

		/// <summary> 경험치 복구 리스트 가져오기 </summary>
		public void REQ_RestoreExpList(System.Action<ZWebRecvPacket, ResRestoreExpList> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			ReqRestoreExpList.StartReqRestoreExpList(mBuilder);
			var offset = ReqRestoreExpList.EndReqRestoreExpList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqRestoreExpList>(this, Code.GS_RESTORE_EXP_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResRestoreExpList>();

				Me.CurCharData.ClearRestoreExpList();

				for (int i = 0; i < recvMsgPacket.RestoreExpsLength; i++)
				{
					Me.CurCharData.AddRestoreExp(recvMsgPacket.RestoreExps(i));
				}

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 경험치 복구 하기 </summary>
		public void REQ_Restore(ulong restoreId, ulong useItemId, uint useItemTid, System.Action<ZWebRecvPacket, ResRestoreExp> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRestoreExp.CreateReqRestoreExp(mBuilder, ZGameManager.Instance.GetMarketType(),
				restoreId, useItemId, useItemTid);
			var reqPacket = ZWebPacket.Create<ReqRestoreExp>(this, Code.GS_RESTORE_EXP, mBuilder, offset.Value);

			// 경험치 복구 요청 
			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvData = recvPacket.Get<ResRestoreExp>();

				// 재화 업데이트 
				Me.CurUserData.SetCash(recvData.ResultCashCoinBalance);
				for (int i = 0; i < recvData.ResultAccountStackItemsLength; i++)
				{
					Me.CurCharData.AddItemList(recvData.ResultAccountStackItems(i));
				}

				// 남은 무료 회수 , 진행 유료 회수 업데이트 
				Me.CurCharData.UpdateRestoreExpCnt(recvData.ResultRestoreExpCnt);
				Me.CurCharData.UpdateRestoreExpNotFreeCnt(recvData.ResultRestoreExpNotFreeCnt);

				// 경험치 업데이트 
				Me.CurCharData.UpdateExp(recvData.ResultExp, false);

				_onReceive?.Invoke(recvPacket, recvData);
			}, _onError);
		}

		#endregion

		#region 속성 강화 
		/// <summary> 속성 강화하기 </summary>
		public void REQ_EnhanceAttribute(uint attributeID, uint addRateCnt, System.Action<ZWebRecvPacket, ResAttributeEnchant> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqAttributeEnchant.CreateReqAttributeEnchant(mBuilder, attributeID, addRateCnt);
			var reqPacket = ZWebPacket.Create<ReqAttributeEnchant>(this, Code.GS_ATTRIBUTE_ENCHANT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResAttributeEnchant>();

				Me.CurCharData.ClearAttribute();

				for (int i = 0; i < recvMsgPacket.Attribute.Value.AttributeTidsLength; i++)
				{
					uint tid = recvMsgPacket.Attribute.Value.AttributeTids(i);
					Me.CurCharData.AddAttributeTID(DBAttribute.GetTypeByTID(tid), tid);
				}

				//List<DelItemInfo> delitems = new List<DelItemInfo>();

				//for (int i = 0; i < recvMsgPacket.BreakItemsLength; i++)
				//	delitems.Add(recvMsgPacket.BreakItems(i).Value);

				//Me.CurCharData.RemoveItemList(delitems);

				// 재화 업데이트 
				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}


		#endregion


		//#region Common

		//public static void AddRemainItems(ulong UserId, ulong CharId, ItemInfo RemainItems)
		//{
		//	if (RemainItems.AccountStackLength > 0 || RemainItems.StackLength > 0 || RemainItems.EquipLength > 0)
		//	{
		//		List<AccountItemStack> accountItemList = new List<AccountItemStack>();
		//		List<ItemStack> stackList = new List<ItemStack>();
		//		List<ItemEquipment> equipList = new List<ItemEquipment>();

		//		for (int i = 0; i < RemainItems.AccountStackLength; i++)
		//			accountItemList.Add(RemainItems.AccountStack(i).Value);
		//		for (int i = 0; i < RemainItems.StackLength; i++)
		//			stackList.Add(RemainItems.Stack(i).Value);
		//		for (int i = 0; i < RemainItems.EquipLength; i++)
		//			equipList.Add(RemainItems.Equip(i).Value);

		//		NetData.Instance.AddItemList(UserId, CharId, accountItemList, stackList, equipList);
		//	}

		//	if (RemainItems.PetLength > 0)
		//	{
		//		List<Pet> petList = new List<Pet>();

		//		for (int i = 0; i < RemainItems.PetLength; i++)
		//			petList.Add(RemainItems.Pet(i).Value);

		//		NetData.Instance.AddPetList(NetData.UserID, NetData.CharID, petList);
		//	}

		//	if (RemainItems.ChangeLength > 0)
		//	{
		//		List<Change> changeList = new List<Change>();

		//		for (int i = 0; i < RemainItems.ChangeLength; i++)
		//			changeList.Add(RemainItems.Change(i).Value);

		//		NetData.Instance.AddChangeList(NetData.UserID, NetData.CharID, changeList);
		//	}

		//	if (RemainItems.RuneLength > 0)
		//	{
		//		//룬 획득
		//		List<Rune> runeList = new List<Rune>();

		//		for (int i = 0; i < RemainItems.RuneLength; i++)
		//			runeList.Add(RemainItems.Rune(i).Value);

		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);
		//	}
		//}
		//#endregion

		//#region AccountOption
		//public static void SetAccountOption(E_AccountOptionType type, bool bOn, System.Action<ReceiveFBMessage, ResSetAccountOption> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_SET_ACCOUNT_OPTION);

		//	builder.Clear();

		//	var offset = ReqSetAccountOption.CreateReqSetAccountOption(builder, type, bOn);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqSetAccountOption>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResSetAccountOption recvMsgPacket = recvPacket.GetResMsg<ResSetAccountOption>();

		//		NetData.AccountOptionBit = recvMsgPacket.OptionBit;

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void SetPush(E_MarketType marketType, E_PushType type, System.Action<ReceiveFBMessage, ResSetPushOption> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_SET_PUSH_OPTION);

		//	builder.Clear();

		//	var offset = ReqSetPushOption.CreateReqSetPushOption(builder, marketType, type);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqSetPushOption>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResSetPushOption recvMsgPacket = recvPacket.GetResMsg<ResSetPushOption>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void SetSurvey(byte IsAnswer, string AnswerJson, System.Action<ReceiveFBMessage, ResGameSurvey> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GAME_SURVEY);

		//	builder.Clear();

		//	var offset = ReqGameSurvey.CreateReqGameSurvey(builder, IsAnswer, builder.CreateString(AnswerJson));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGameSurvey>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGameSurvey recvMsgPacket = recvPacket.GetResMsg<ResGameSurvey>();

		//		NetData.AccountOptionBit = (long)recvMsgPacket.AccountOptionBit;

		//		if (recvMsgPacket.GetItem != null)
		//		{
		//			List<GainInfo> gainList = new List<GainInfo>();
		//			List<string> addmsg = new List<string>();

		//			gainList.Add(new GainInfo(recvMsgPacket.GetItem.Value));

		//			if (DBItem.IsEquipItem(recvMsgPacket.GetItem.Value.ItemTid))
		//				addmsg.Add(string.Format("{0}을 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetItem.Value.ItemTid)));
		//			else
		//				addmsg.Add(string.Format("{0}({1})을 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetItem.Value.ItemTid), recvMsgPacket.GetItem.Value.ItemCnt));

		//			UIManager.ShowGainEff(gainList);
		//			ZNetChatData.AddSystemMsg(addmsg);
		//		}

		//		if (recvMsgPacket.ResultItemStack != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.ResultItemStack.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		var townPanel = UIManager.instance.GetActiveUI<HudTownPanel>();
		//		if (townPanel != null)
		//			townPanel.GetMenuGroupUI.UpdateUI();
		//	});
		//}
		//#endregion

		//#region Tutorial
		//public static void GetTutorialReward(ulong QuestId, uint QuestTid, System.Action<ReceiveFBMessage, ResTutorialReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TUTORIAL_REWARD);

		//	builder.Clear();

		//	var offset = ReqTutorialReward.CreateReqTutorialReward(builder, QuestId, QuestTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTutorialReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTutorialReward recvMsgPacket = recvPacket.GetResMsg<ResTutorialReward>();

		//		NetData.Instance.NextQuest(NetData.UserID, NetData.CharID, recvMsgPacket.NextQuest.Value, QuestId);

		//		if (recvMsgPacket.GetExp > 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainExp);

		//		if (recvMsgPacket.RemainItems != null)
		//			AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_TUTORIAL, new Dictionary<string, object>()
		//	{
		//		{ "UserID", NetData.UserID },
		//		{ "CharID", NetData.CharID },
		//		{ "QuestTID", QuestTid },
		//	});
		//	});
		//}
		//#endregion

		#region Quest

		public void REQ_QuestReward(uint _questTID, uint _selectItemIndex, System.Action<SRewardItemList> onRecvPacket, bool _delayReward = false, PacketErrorCBDelegate _onError = null)
		{
			QuestData questData = Me.CurCharData.GetQuest(_questTID);
			SRewardItemList itemList = null;
			if (questData == null) return;

			Offset<ReqQuestReward> offset = ReqQuestReward.CreateReqQuestReward(mBuilder, questData.QuestId, _questTID, _selectItemIndex);
			ZWebPacket reqPacket = ZWebPacket.Create<ReqQuestReward>(this, Code.GS_QUEST_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResQuestReward recvMsgPacket = recvPacket.Get<ResQuestReward>();

				// 입수 아이템 UI 출력 
				if (recvMsgPacket.GetItems != null)
				{
					itemList = Me.CurCharData.ExtractItemList(recvMsgPacket.GetItems.Value);
				}

				// 아이템 처리부
				if (recvMsgPacket.RemainItems != null)
					Me.CurCharData.AddRemainItems(recvMsgPacket.RemainItems.Value);

				// 유적 입장 정보 갱신 
				if (recvMsgPacket.TempleStage != null)
				{
					Me.CurCharData.TempleInfo.AddStage(recvMsgPacket.TempleStage, true);
				}

				if (_delayReward)
				{
					UIManager.Instance.Find<UIFrameQuest>().DoUIQuestRewardRandomPlay(itemList, () =>
					{
						ProcessQuestComplete(recvMsgPacket.NextQuest.Value, questData.QuestId);
					});
				}
				else
				{
					ProcessQuestComplete(recvMsgPacket.NextQuest.Value, questData.QuestId);
					onRecvPacket?.Invoke(itemList);
				}
				//Debug.Log(Me.CurCharData.InvenList);
			}, _onError);
		}

		private void ProcessQuestComplete(Quest _quest, ulong _completeQuestSID)
		{
			// 다음 퀘스트 : 마지막 퀘스트일 경우 TID가 같다
			if (_quest.QuestTid != _completeQuestSID)
			{
				Me.CurCharData.CompleteQuest(_completeQuestSID, false);
				Me.CurCharData.AddQuestList(_quest, true);
			}
			else
			{
				Me.CurCharData.CompleteQuest(_completeQuestSID, true);
			}
		}

		public void REQ_QuestAccept(uint _questTID, System.Action<uint> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			Offset<ReqQuestAccept> offset = ReqQuestAccept.CreateReqQuestAccept(mBuilder, _questTID);
			ZWebPacket reqPacket = ZWebPacket.Create<ReqQuestAccept>(this, Code.GS_QUEST_ACCEPT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResQuestAccept recvMsgPacket = recvPacket.Get<ResQuestAccept>();
				Me.CurCharData.AddQuestList(recvMsgPacket.AddQuest, true);
				onRecvPacket?.Invoke(recvMsgPacket.AddQuest.Value.QuestTid);

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
				{
					ZNet.Data.Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i));
				}

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
				{
					ZNet.Data.Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i));
				}

			}, (ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket) =>
			{

				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.ERROR, _recvPacket.ErrCode.ToString(), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
				});
				onRecvPacket?.Invoke(_questTID);
			});
		}

		public void REQ_QuestCancle(uint _questTID, System.Action<uint> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			QuestData questData = Me.CurCharData.GetQuest(_questTID);
			if (questData == null) return;

			Offset<ReqQuestCancel> offset = ReqQuestCancel.CreateReqQuestCancel(mBuilder, questData.QuestId, _questTID);
			ZWebPacket reqPacket = ZWebPacket.Create<ReqQuestCancel>(this, Code.GS_QUEST_CANCEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResQuestCancel recvMsgPacket = recvPacket.Get<ResQuestCancel>();
				Me.CurCharData.DeleteQuestList(recvMsgPacket.QuestId);
				onRecvPacket?.Invoke(_questTID);
			}, (ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket) =>
			{
				onRecvPacket?.Invoke(_questTID);
			});
		}

		#endregion


		//#region Friend
		public void REQ_GetFriendList(System.Action<ZWebRecvPacket, ResGetFriendList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetFriendList.StartReqGetFriendList(mBuilder);
			var offset = ReqGetFriendList.EndReqGetFriendList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetFriendList>(this, Code.GS_GET_FRIEND_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetFriendList recvMsgPacket = recvPacket.Get<ResGetFriendList>();

				List<FriendInfo> friendList = new List<FriendInfo>();
				for (int i = 0; i < recvMsgPacket.FriendsLength; i++)
					friendList.Add(recvMsgPacket.Friends(i).Value);

				Me.CurCharData.ResetFriendList(friendList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_GetRequestFriendList(System.Action<ZWebRecvPacket, ResRequestFriendList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqRequestFriendList.StartReqRequestFriendList(mBuilder);
			var offset = ReqRequestFriendList.EndReqRequestFriendList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqRequestFriendList>(this, Code.GS_REQUEST_FRIEND_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRequestFriendList recvMsgPacket = recvPacket.Get<ResRequestFriendList>();

				Me.CurCharData.ClearRequestFriendList();

				List<FriendRequestInfo> friendList = new List<FriendRequestInfo>();
				for (int i = 0; i < recvMsgPacket.RequestFriendsLength; i++)
					Me.CurCharData.AddRequestFriend(recvMsgPacket.RequestFriends(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		public void REQ_AddFriend(ulong CharId, System.Action<ZWebRecvPacket, ResRequestFriend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRequestFriend.CreateReqRequestFriend(mBuilder, CharId);
			var reqPacket = ZWebPacket.Create<ReqRequestFriend>(this, Code.GS_REQUEST_FRIEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRequestFriend recvMsgPacket = recvPacket.Get<ResRequestFriend>();

				Me.CurCharData.AddRequestFriend(recvMsgPacket.RequestFriend.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_DelFriend(E_FriendState type, ulong CharId, System.Action<ZWebRecvPacket, ResDelFriend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqDelFriend.CreateReqDelFriend(mBuilder, type, CharId);
			var reqPacket = ZWebPacket.Create<ReqDelFriend>(this, Code.GS_DEL_FRIEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResDelFriend recvMsgPacket = recvPacket.Get<ResDelFriend>();

				Me.CurCharData.RemoveFriend(recvMsgPacket.DelCharId, recvMsgPacket.State);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		public void REQ_AcceptReqFriend(ulong CharId, System.Action<ZWebRecvPacket, ResRequestFriendAccept> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRequestFriendAccept.CreateReqRequestFriendAccept(mBuilder, CharId);
			var reqPacket = ZWebPacket.Create<ReqRequestFriendAccept>(this, Code.GS_REQUEST_FRIEND_ACCEPT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRequestFriendAccept recvMsgPacket = recvPacket.Get<ResRequestFriendAccept>();

				Me.CurCharData.RemoveRequestFriend(recvMsgPacket.Friend.Value.CharId);
				Me.CurCharData.AddFriend(recvMsgPacket.Friend.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		public void REQ_CancelReqFriend(ulong CharId, System.Action<ZWebRecvPacket, ResRequestFriendCancel> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRequestFriendCancel.CreateReqRequestFriendCancel(mBuilder, CharId);
			var reqPacket = ZWebPacket.Create<ReqRequestFriendCancel>(this, Code.GS_REQUEST_FRIEND_CANCEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRequestFriendCancel recvMsgPacket = recvPacket.Get<ResRequestFriendCancel>();

				Me.CurCharData.RemoveRequestFriend(recvMsgPacket.CancelCharId);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		public void REQ_AddAlertFriend(ulong CharId, System.Action<ZWebRecvPacket, ResAddAlertFriend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqAddAlertFriend.CreateReqAddAlertFriend(mBuilder, CharId);
			var reqPacket = ZWebPacket.Create<ReqAddAlertFriend>(this, Code.GS_ADD_ALERT_FRIEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResAddAlertFriend recvMsgPacket = recvPacket.Get<ResAddAlertFriend>();

				Me.CurCharData.AddFriend(recvMsgPacket.Friend.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary> 닉네임으로 유저 찾기 </summary>
		public void REQ_FindFriend(string nick, System.Action<ZWebRecvPacket, ResFindFriend> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqFindFriend.CreateReqFindFriend(mBuilder, mBuilder.CreateString(nick));
			var reqPacket = ZWebPacket.Create<ReqFindFriend>(this, Code.GS_FIND_FRIEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResFindFriend res = recvPacket.Get<ResFindFriend>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}
		//#endregion

		///// <param name="myPawnNetId">현재 게임상 존재하는 나의 ZPawn객체의 NetId </param>
		//public static void CreateSession(bool bLoginInit, uint StageTid, bool IsPrivateChannel, uint myPawnNetId, System.Action<ReceiveFBMessage, ResCreateSession> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CREATE_SESSINON);

		//	builder.Clear();

		//	var offset = ReqCreateSession.CreateReqCreateSession(builder, bLoginInit, StageTid, IsPrivateChannel, myPawnNetId, (uint)Version.Num);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqCreateSession>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResCreateSession recvMsgPacket = recvPacket.GetResMsg<ResCreateSession>();
		//		NetData.Server_Time = recvMsgPacket.Time;
		//		NetData.SetRndKey(WebSocketType.GAME_SERVER, recvMsgPacket.Rnd);

		//		if (!recvMsgPacket.IsFirstEnter)
		//		{
		//			bool bRefresh = (ZGameManager.instance.CurGameMode == null || false == ZGameManager.instance.CurGameMode is ScenarioGameMode);

		//			if (bRefresh)
		//				NetData.Instance.RefreshCharacterInfo(NetData.UserID, NetData.CharID, ref recvMsgPacket);

		//			//TimeInvoker.instance.RequestInvoke(RefreshDailyQuestList, TimeHelper.GetRemainSpecificTime(DBConfig.Event_Reset_Time));
		//			AlramUI.CheckAlram(Alram.QUEST | Alram.SPECIAL_SHOP);

		//			//파티 정보도 갱신 필요
		//			CheckParty(NetData.ConnectedServerId, NetData.CharID, (x, y) => {
		//				if (y.IsParty)
		//					ZNetGame.GetPartyMember(NetData.ConnectedServerId, NetData.CharID, y.PartyUid, null);
		//			});
		//		}

		//		GetServerTime();
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_CHAR_CONNECT, new Dictionary<string, object>()
		//	{
		//		{ "UserId", NetData.UserID },
		//		{ "CharId", NetData.CharID }
		//	});
		//	});
		//}

		#region Mark
		//public static void GetMarkList(System.Action<ReceiveFBMessage, ResGetMarkList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_MARK_LIST);

		//	builder.Clear();

		//	ReqGetMarkList.StartReqGetMarkList(builder);
		//	var offset = ReqGetMarkList.EndReqGetMarkList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetMarkList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetMarkList recvMsgPacket = recvPacket.GetResMsg<ResGetMarkList>();

		//		NetData.Instance.ClearMarkList(NetData.UserID, NetData.CharID);

		//		List<Mark> markList = new List<Mark>();

		//		for (int i = 0; i < recvMsgPacket.MarksLength; i++)
		//			markList.Add(recvMsgPacket.Marks(i).Value);

		//		NetData.Instance.AddMarkList(NetData.UserID, NetData.CharID, markList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_EnchantMark(
			uint MarkTid
			, bool bProtect
			, Action<ZWebRecvPacket, ResMarkEnchant> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqMarkEnchant.CreateReqMarkEnchant(mBuilder
				, MarkTid
				, bProtect);
			var reqPacket = ZWebPacket.Create<ReqMarkEnchant>(this, Code.GS_MARK_ENCHANT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResMarkEnchant>();

				if (recvMsgPacket.Mark != null)
				{
					for (int i = 0; i < recvMsgPacket.Mark.Value.MarkTidsLength; i++)
					{
						var tid = recvMsgPacket.Mark.Value.MarkTids(i);
						Me.CurCharData.AddMarkTID(tid);
					}
				}

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Artifact
		public void REQ_MakeArtifactItem(
	uint artifactTid
	, uint[] useItemTid
	, uint protectItemTid
	, List<DBArtifact.MaterialItem> matItems
	, Action<ZWebRecvPacket, ResMakeArtifactItem> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			Offset<MaterialItem>[] vectors = new Offset<MaterialItem>[matItems.Count];
			for (int i = 0; i < vectors.Length; ++i)
			{
				vectors[i] = MaterialItem.CreateMaterialItem(mBuilder, matItems[i].id, matItems[i].tid, matItems[i].cnt);
			}

			var offset = ReqMakeArtifactItem.CreateReqMakeArtifactItem(
				mBuilder
				, artifactTid
				, ReqMakeArtifactItem.CreateUseItemTidVector(mBuilder, useItemTid)
				, protectItemTid
				, ReqMakeArtifactItem.CreateMaterialItemsVector(mBuilder, vectors));

			var reqPacket = ZWebPacket.Create<ReqMakeArtifactItem>(this, Code.GS_MAKE_ARTIFACT_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResMakeArtifactItem>();

				Me.CurCharData.DeleteArtifactID(recvMsgPacket.DeleteArtifactId);

				if (recvMsgPacket.ResultArtifactInfo != null)
					Me.CurCharData.AddArtifactID(recvMsgPacket.ResultArtifactInfo.Value);

				for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
				{
					var v = recvMsgPacket.ResultPets(i).Value;
					/// *** 같은 데이터타입의 Pet 이지만 (펫,탈것) 
					/// 타입따라 다르게 넣어줘야함 .
					var table = DBPet.GetPetData(v.PetTid);
					if (table.PetType == GameDB.E_PetType.Pet)
					{
						Me.CurCharData.AddPetList(v);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						Me.CurCharData.AddRideList(v);
					}
				}

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}


		public void REQ_ArtifactEquip(
			uint artifactType // pet type 
			, ulong artifactID
			, Action<ZWebRecvPacket, ResArtifactEquip> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqArtifactEquip.CreateReqArtifactEquip(mBuilder, artifactType, artifactID);
			var reqPacket = ZWebPacket.Create<ReqArtifactEquip>(this, Code.GS_ARTIFACT_EQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResArtifactEquip>();

				if (artifactType == (uint)GameDB.E_PetType.Pet)
				{
					Me.CurCharData.Artifact_Pet = artifactID;
				}
				else if (artifactType == (uint)GameDB.E_PetType.Vehicle)
				{
					Me.CurCharData.Artifact_Vehicle = artifactID;
				}
				else
				{
					ZLog.LogError(ZLogChannel.Default, " no such type exist");
				}
				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_ArtifactUnEquip(
			uint artifactType // petType 
		  , Action<ZWebRecvPacket, ReqArtifactUnEquip> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();

			var offset = ReqArtifactUnEquip.CreateReqArtifactUnEquip(mBuilder, artifactType);
			var reqPacket = ZWebPacket.Create<ReqArtifactUnEquip>(this, Code.GS_ARTIFACT_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ReqArtifactUnEquip>();

				if (artifactType == (uint)GameDB.E_PetType.Pet)
				{
					Me.CurCharData.Artifact_Pet = 0;
				}
				else if (artifactType == (uint)GameDB.E_PetType.Vehicle)
				{
					Me.CurCharData.Artifact_Vehicle = 0;
				}
				else
				{
					ZLog.LogError(ZLogChannel.Default, " no such type exist");
				}

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion

		//#region Artifact
		//public static void MakeArtifact(uint artifactTid, List<ZItem> materialItems, List<ChangeData> materialChanges01, List<ChangeData> materialChanges02, System.Action<ReceiveFBMessage, ResMakeArtifactItem> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_MAKE_ARTIFACT_ITEM);

		//	builder.Clear();

		//	var items = new Offset<MaterialItem>[materialItems.Count];
		//	for (int i = 0; i < materialItems.Count; i++)
		//	{
		//		items[i] = MaterialItem.CreateMaterialItem(builder, materialItems[i].item_id, materialItems[i].item_tid, (uint)materialItems[i].cnt);
		//	}

		//	var changes01 = new Offset<MaterialChange>[materialChanges01.Count];
		//	for (int i = 0; i < materialChanges01.Count; i++)
		//	{
		//		changes01[i] = MaterialChange.CreateMaterialChange(builder, materialChanges01[i].ChangeId, materialChanges01[i].ChangeTid, materialChanges01[i].Cnt);
		//	}

		//	var changes02 = new Offset<MaterialChange>[materialChanges02.Count];
		//	for (int i = 0; i < materialChanges02.Count; i++)
		//	{
		//		changes02[i] = MaterialChange.CreateMaterialChange(builder, materialChanges02[i].ChangeId, materialChanges02[i].ChangeTid, materialChanges02[i].Cnt);
		//	}

		//	var offset = ReqMakeArtifactItem.CreateReqMakeArtifactItem(builder,
		//		artifactTid,
		//		ReqMakeArtifactItem.CreateMaterialItemsVector(builder, items),
		//		ReqMakeArtifactItem.CreateMaterialChanges1Vector(builder, changes01),
		//		ReqMakeArtifactItem.CreateMaterialChanges1Vector(builder, changes02));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqMakeArtifactItem>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResMakeArtifactItem recvMsgPacket = recvPacket.GetResMsg<ResMakeArtifactItem>();

		//		List<DelItemInfo> delitems = new List<DelItemInfo>();
		//		for (int i = 0; i < recvMsgPacket.DelItemsLength; i++)
		//			delitems.Add(recvMsgPacket.DelItems(i).Value);

		//		NetData.Instance.RemoveItemList(NetData.UserID, NetData.CharID, delitems);

		//		List<AccountItemStack> accountList = new List<AccountItemStack>();
		//		List<ItemEquipment> equipList = new List<ItemEquipment>();
		//		List<ItemStack> stackList = new List<ItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			accountList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			stackList.Add(recvMsgPacket.ResultStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
		//			equipList.Add(recvMsgPacket.ResultEquipItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountList, stackList, equipList);

		//		List<Change> changeList = new List<Change>();

		//		for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
		//			changeList.Add(recvMsgPacket.ResultChanges(i).Value);

		//		NetData.Instance.AddChangeList(NetData.UserID, NetData.CharID, changeList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		//#region Item
		public bool UseItemAction(ZItem useItem, bool bAuto, System.Action<ulong, uint> onActionDone)
		{
			if (!DBItem.GetItem(useItem.item_tid, out var itemTable)) //|| !NetData.bReadyToItemUse)
			{
				//onActionDone(useItem.item_id, useItem.item_tid); // error
				return false;
			}

			if (useItem.UseTime + (ulong)itemTable.CoolTime * 1000 > TimeManager.NowMs)
			{
				UICommon.SetNoticeMessage("쿨타임입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				return false;
			}

			//일단 사용 시각 선처리.
			useItem.UseTime = TimeManager.NowMs;

			switch (itemTable.ItemUseType)
			{
				case GameDB.E_ItemUseType.Goods:
					{
						if (useItem.item_tid == DBConfig.GetStatPoint_ItemID)
						{
							REQ_ItemUse(useItem.item_id, useItem.item_tid, 1, (x, y) =>
							{
								//ItemSlot.DelayGlobalCoolTime(0.3f);
								onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
							});
						}
					}
					break;
				case GameDB.E_ItemUseType.Equip:
					{
						if (useItem.slot_idx == 0)
						{
							byte slotidx = Me.CurCharData.GetCharacterEquipPrioritySlot(useItem.item_tid);
							ZItem equipedItem = Me.CurCharData.GetEquipSlotItem(slotidx);

							if (equipedItem == null)
							{
								//equip
								REQ_EquipItem(useItem.item_id, useItem.item_tid, slotidx, delegate
								{
									//ItemSlot.DelayGlobalCoolTime(0.3f);

									onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
								});
							}
							else
							{
								ulong equipId = useItem.item_id;
								uint equiptId = useItem.item_tid;
								REQ_UnEquipItem(equipedItem.item_id, equipedItem.item_tid, equipedItem.slot_idx, delegate
								{
									REQ_EquipItem(equipId, equiptId, slotidx, delegate
									{
										//ItemSlot.DelayGlobalCoolTime(0.3f);
										onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
									});
								});
							}
						}
						else
						{
							//unequip
							REQ_UnEquipItem(useItem.item_id, useItem.item_tid, useItem.slot_idx, delegate
							{
								//ItemSlot.DelayGlobalCoolTime(0.3f);
								onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
							});
						}
					}
					break;
				case GameDB.E_ItemUseType.UseItem:
					{
						if (useItem.item_tid == DBConfig.Inven_Slot_Plus_ID)
						{
							if (Me.CurCharData.InvenMaxCnt + 1 >= DBConfig.Max_Inventory)
							{
								UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
								{
									_popup.Open(ZUIString.WARRING, "인벤토리가 최대 확장된 상태입니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate { _popup.Close(); } });
								});

								return false;
							}
							BuyInventorySlot(useItem.item_id, useItem.item_tid, (x, y) =>
							{
								onActionDone(useItem.item_id, useItem.item_tid);
							});
						}
						//else if (useItem.item_tid == DBConfig.Storage_Slot_Plus_ID)
						//{
						//	if (NetData.Instance.GetStorageMaxSlot(NetData.UserID) >= DBConfig.Max_Storage_Count)
						//	{
						//		MessagePopup.OpenSimpleMessagePopup("창고가 최대 확장 상태입니다.");
						//		return false;
						//	}
						//	BuyStorageSlot(useItem.item_id, useItem.item_tid, (x, y) => {
						//		onActionDone(useItem.item_id, useItem.item_tid);
						//	});
						//}
						//else if (useItem.item_tid == DBConfig.Ring_Slot_Open_ID)
						//{
						//	if (NetData.CurrentCharacter.AddRingSlot >= DBConfig.Max_Ring_Slot_Count)
						//	{
						//		MessagePopup.OpenSimpleMessagePopup("반지 슬롯 확장이 최대 상태입니다.");
						//		return false;
						//	}
						//	BuyRingSlot(useItem.item_id, (x, y) => {
						//		onActionDone(useItem.item_id, useItem.item_tid);

						//		UIManager.NoticeMessage(DBLocale.GetLocaleText("WItemUse_RingExpend"));
						//	});
						//}
						else if (useItem.item_tid == DBConfig.QuickSlot_Slot_Open_ID)
						{
							if (Me.CurCharData.QuickSlotMaxCnt >= DBConfig.Max_QuickSlot_Count)
							{
								UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
								{
									_popup.Open(ZUIString.WARRING, "퀵 슬롯 확장이 최대 상태입니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate { _popup.Close(); } });
								});
								return false;
							}
							BuyQuickSlotSlot(useItem.item_id, useItem.item_tid, (x, y) =>
							{
								onActionDone(useItem.item_id, useItem.item_tid);

								for (int i = 0; i < UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter.Count; i++)
								{
									for (int j = 0; j < UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[i].DotsBG.Count; j++)
										UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[i].DotsBG[j].SetActive(true);
									UIManager.Instance.Find<UISubHUDQuickSlot>().ScrollAdapter[i].DotRect.anchoredPosition = Vector2.zero;
								}

								UIManager.Instance.Find<UISubHUDQuickSlot>().ReSetAllSlot();
								UICommon.SetNoticeMessage(DBLocale.GetText("WItemUse_QuickslotExpend"), new Color(255, 255, 255), 2.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
							});
						}
						else if (useItem.item_tid == DBConfig.Character_Slot_Open_ID)
						{
							if (Me.MaxCharCnt >= DBConfig.Max_Character_Slot_Count)
							{
								UICommon.SetNoticeMessage("캐릭터 슬롯 확장이 최대 상태입니다.", new Color(255, 255, 255), 2.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
								return false;
							}
							BuyCharacterSlot(useItem.item_id, (x, y) =>
							{
								onActionDone(useItem.item_id, useItem.item_tid);

								UICommon.SetNoticeMessage(DBLocale.GetText("WItemUse_CharacterSlotExpend"), new Color(255, 255, 255), 2.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
							});
						}
						//else if (useItem.item_tid == DBConfig.Expend_SpecialSkillSlot_ItemID)
						//{
						//	if (NetData.Instance.GetSpecialSkillMaxCnt(NetData.UserID) >= DBConfig.Max_SpecialSkill_SlotCount)
						//	{
						//		MessagePopup.OpenSimpleMessagePopup("궁극기 슬롯 확장이 최대 상태입니다.");
						//		return false;
						//	}
						//	BuySpecialSkillSlot(useItem.item_id, useItem.item_tid, (x, y) => {
						//		onActionDone(useItem.item_id, useItem.item_tid);
						//	});
						//}
						//else
						//{
						//	if (DBItem.GetItem(useItem.item_tid).ItemType == GameDB.E_ItemType.SeasonPass)
						//	{
						//		BuySeasonPass(useItem.item_id, useItem.item_tid, (x, y) =>
						//		{
						//			if (y.IsResult == 1)
						//			{
						//				UIManager.NoticeMessage("시즌패스를 사용했습니다.", 1.5f);
						//				UIManager.instance.GetActivePopup("BagPopup")?.CloseClick();
						//				EventIngamePopup popup = UIManager.instance.OpenPopup("EventIngamePopup", EventIngamePopup.E_EventIconType.AttendSeasonPass) as EventIngamePopup;
						//			}
						//			else
						//			{
						//				MessagePopup.OpenSimpleMessagePopup("사용 기간이 지났습니다.");
						//			}
						//		});
						//		break;
						//	}

						//	MessagePopup.OpenSimpleMessagePopup("현재 사용 가능한 컨텐츠가 없습니다.");
						//	return false;
						//}
					}
					break;
				case GameDB.E_ItemUseType.Gacha:
					{
						var useItemTable = DBItem.GetItem(useItem.item_tid);
						if (useItemTable.ItemType == GameDB.E_ItemType.ShopGacha)
						{
							Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);

							UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
								_popup.Open(ZUIString.WARRING, DBLocale.GetText("아직 사용할 수 없습니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
								_popup.Close(); } });
							});

							return false;
						}

						System.Action<GameDB.E_RuneSetType, uint> reqGacha = (runeSet, useCnt) =>
						{
							Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);

							REQ_GachaItem(useItem.item_id, useItem.item_tid, useCnt, 0, (uint)runeSet, (recvPacket, recvMsgPacket) =>
							{
								onActionDone?.Invoke(useItem.item_id, useItem.item_tid);

								// 가챠 아이템
								if (recvMsgPacket.GetItems.HasValue)
								{
									List<string> msgs = new List<string>();
									List<GainInfo> listGainItem = new List<GainInfo>();

									ItemInfo gainItemInfo = recvMsgPacket.GetItems.Value;

									// 장비
									for (int i = 0; i < gainItemInfo.EquipLength; i++)
									{
										var item = gainItemInfo.Equip(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBLocale.GetItemLocale(DBItem.GetItem(item.ItemTid))));
									}

									// 계정스택
									for (int i = 0; i < gainItemInfo.AccountStackLength; i++)
									{
										var item = gainItemInfo.AccountStack(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Stack"), 1));
									}

									// 스택
									for (int i = 0; i < gainItemInfo.StackLength; i++)
									{
										var item = gainItemInfo.Stack(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Stack"), 1));
									}

									// 펫 & 탈것
									for (int i = 0; i < gainItemInfo.PetLength; i++)
									{
										var item = gainItemInfo.Pet(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBPet.GetPetFullName(item.PetTid)));
									}

									// 강림
									for (int i = 0; i < gainItemInfo.ChangeLength; i++)
									{
										var item = gainItemInfo.Change(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBChange.GetChangeFullName(item.ChangeTid)));
									}

									// 룬
									for (int i = 0; i < gainItemInfo.RuneLength; i++)
									{
										var item = gainItemInfo.Rune(i).Value;
										listGainItem.Add(new GainInfo(item));
										msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBLocale.GetItemLocale(DBItem.GetItem(item.ItemTid))));
									}

									// 교체창에 카드 추가됨
									if (gainItemInfo.ChangeGachaKeepLength > 0)
									{
										// 강림
										for (int i = 0; i < gainItemInfo.ChangeGachaKeepLength; i++)
										{
											var item = gainItemInfo.ChangeGachaKeep(i).Value;
											msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBChange.GetChangeFullName(item.ChangeTid)));
											UICommon.SetNoticeMessage(DBLocale.GetText("Card_Replacement_Registration", DBChange.GetChangeFullName(item.ChangeTid)), Color.white, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
										}
									}

									if (gainItemInfo.PetGachaKeepLength > 0)
									{
										// 펫
										for (int i = 0; i < gainItemInfo.PetGachaKeepLength; i++)
										{
											var item = gainItemInfo.PetGachaKeep(i).Value;
											msgs.Add(string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBPet.GetPetFullName(item.PetTid)));
											UICommon.SetNoticeMessage(DBLocale.GetText("Card_Replacement_Registration", DBPet.GetPetFullName(item.PetTid)), Color.white, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
										}
									}
									ZWebChatData.AddSystemMsg(msgs);
								}
							});
						};

						reqGacha.Invoke(GameDB.E_RuneSetType.None, 1);
					}
					break;
				case GameDB.E_ItemUseType.Potion:
					{
						if (useItem.cnt <= 0)
						{
							Debug.LogError("현재 물약 갯수가 모자랍니다.");
							return false;
						}

						List<UIQuickItemSlot> quickSlotListGlobal = UIManager.Instance.Find<UISubHUDQuickSlot>().GetGroupByItem(itemTable.GroupID);
						if (quickSlotListGlobal != null)
							for (int j = 0; j < quickSlotListGlobal.Count; j++)
							{
								quickSlotListGlobal[j].GlobalCoolTimeUTS = TimeManager.NowMs;
								quickSlotListGlobal[j].EndCoolTime(1);
							}

						ZMmoManager.Instance.Field.REQ_UseItem(ZPawnManager.Instance.MyEntityId, useItem.item_tid);
						List<UIQuickItemSlot> quickSlotList = UIManager.Instance.Find<UISubHUDQuickSlot>().GetQuickListSlot(itemTable.ItemID);
						if (quickSlotList != null)
							for (int i = 0; i < quickSlotList.Count; i++)
								quickSlotList[i].EndCoolTime(DBItem.GetItem(useItem.item_tid).CoolTime);
					}
					break;
				case GameDB.E_ItemUseType.Buff:
					{
						var tableData = DBItem.GetItem(useItem.item_tid);

						if (tableData.AbilityActionID_01 != 0)
						{
							var buffData = ZPawnManager.Instance.MyEntity.FindAbilityAction(tableData.AbilityActionID_01);
							if (buffData != null && DBAbility.OverMaxBuffStackTime(tableData.AbilityActionID_01, (uint)(buffData.EndServerTime - TimeManager.NowSec)))
							{
								if (!bAuto)
									Debug.LogError("현재 최대치가 적용 중 입니다.");
								return false;
							}
						}
						if (tableData.AbilityActionID_02 != 0)
						{
							var buffData = ZPawnManager.Instance.MyEntity.FindAbilityAction(tableData.AbilityActionID_02);
							if (buffData != null && DBAbility.OverMaxBuffStackTime(tableData.AbilityActionID_02, (uint)(buffData.EndServerTime - TimeManager.NowSec)))
							{
								if (!bAuto)
									Debug.LogError("현재 최대치가 적용 중 입니다.");
								return false;
							}
						}
						if (tableData.AbilityActionID_03 != 0)
						{
							var buffData = ZPawnManager.Instance.MyEntity.FindAbilityAction(tableData.AbilityActionID_03);
							if (buffData != null && DBAbility.OverMaxBuffStackTime(tableData.AbilityActionID_03, (uint)(buffData.EndServerTime - TimeManager.NowSec)))
							{
								if (!bAuto)
									Debug.LogError("현재 최대치가 적용 중 입니다.");
								return false;
							}
						}

						switch (DBItem.GetItemType(useItem.item_tid))
						{
							case GameDB.E_ItemType.SelfChannel:
							case GameDB.E_ItemType.DespairTower:
								{

									Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);
									REQ_ItemUse(useItem.item_id, useItem.item_tid, 1, (x, y) =>
								   {
									   //ItemSlot.DelayGlobalCoolTime(0.3f);
									   onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
								   });
								}
								break;
							case GameDB.E_ItemType.GodTear:
								{
									if (0 < useItem.cnt && null != ZPawnManager.Instance.MyEntity)
									{
										var blessbuffData = ZPawnManager.Instance.MyEntity.GetGodBuffAbilityAction();
										
										if(blessbuffData == null)
                                        {
											UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
												_popup.Open(ZUIString.WARRING, "이카루스의 빛 설정이 필요합니다.", new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
												_popup.Close(); } });
											});
											return false;
                                        }

										var tabledata = DBAbility.GetAction(blessbuffData.AbilityActionId);
										int stackCount = 0;
										uint maxStackCount = tabledata.MaxBuffStack;
										if (blessbuffData != null && blessbuffData.EndServerTime > TimeManager.NowSec)
										{
											stackCount = Mathf.CeilToInt((float)(blessbuffData.EndServerTime - TimeManager.NowSec) / (float)tabledata.MinSupportTime);
										}

										if (stackCount < maxStackCount)
										{
											Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);
											REQ_ItemUse(useItem.item_id, useItem.item_tid, 1, (x, y) =>
											{
												//ItemSlot.DelayGlobalCoolTime(0.3f);
												onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
											});
										}
									}
								}
								break;
							default:
								{
									Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);
									REQ_ItemUse(useItem.item_id, useItem.item_tid, 1, (x, y) =>
									{
										//ItemSlot.DelayGlobalCoolTime(0.3f);
										onActionDone?.Invoke(useItem.item_id, useItem.item_tid);
									});
								}
								break;
						}
					}
					break;
				case GameDB.E_ItemUseType.Change:
					{
						if (useItem.cnt <= 0)
							return false;

						UIFramePetChangeSelect selectPopup = UIManager.Instance.Find<UIFramePetChangeSelect>();

						void onConfirm()
						{
							bool spawned = Me.CurCharData.MainChange > 0;

							if (spawned)
							{
								var frame = UIManager.Instance.Find<UIFramePetChangeSelect>();
								if (frame != null && frame.isActiveAndEnabled)
								{
									frame.Refresh();
								}
							}
							else
							{
								UIManager.Instance.Close<UIFramePetChangeSelect>();

								UIManager.Instance.Find<UISubHUDMenu>().ShowBag();
							}
						}

						if (selectPopup == null)
						{
							UIManager.Instance.Load<UIFramePetChangeSelect>(nameof(UIFramePetChangeSelect), (loadName, loadFrame) =>
							{
								loadFrame.Init(() =>
								{
									UIManager.Instance.Open<UIFramePetChangeSelect>(delegate
									{
										UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Change, onConfirm);
									});
								});
							});
						}
						else
						{
							if (selectPopup.Show == false)
							{
								UIManager.Instance.Open<UIFramePetChangeSelect>(delegate
								{
									UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Change, onConfirm);
								});
							}
							else
							{
								UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Change, onConfirm);
							}
						}
					}
					break;
				case GameDB.E_ItemUseType.PetSummon:
					{
						if (useItem.cnt <= 0)
							return false;

						UIFramePetChangeSelect selectPopup = UIManager.Instance.Find<UIFramePetChangeSelect>();

						Action onConfirm = () =>
						{
							bool spawned = Me.CurCharData.MainPet > 0;

							if (spawned)
							{
								var frame = UIManager.Instance.Find<UIFramePetChangeSelect>();
								if (frame != null && frame.isActiveAndEnabled)
								{
									frame.Refresh();
								}

							}
							else
							{
								UIManager.Instance.Close<UIFramePetChangeSelect>();
							}
						};

						if (selectPopup == null)
						{
							UIManager.Instance.Load<UIFramePetChangeSelect>(nameof(UIFramePetChangeSelect), (loadName, loadFrame) =>
							{
								loadFrame.Init(() =>
								{
									UIManager.Instance.Open<UIFramePetChangeSelect>(delegate
									{
										UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Pet, onConfirm);
									});
								});
							});
						}
						else
						{
							if (selectPopup.Show == false)
							{
								UIManager.Instance.Open<UIFramePetChangeSelect>(delegate
								{
									UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Pet, onConfirm);
								});
							}
							else
							{
								UIManager.Instance.Find<UIFramePetChangeSelect>().SetViewType(E_PetChangeViewType.Pet, onConfirm);
							}
						}
					}
					break;
				case GameDB.E_ItemUseType.Move:
					{
						if (useItem.cnt <= 0)
							return false;

						// 귀환의 돌인 경우
						if (useItem.item_tid == 4800)
						{
							if (Me.CurCharData.ReturnStoneUseTime + (ulong)itemTable.CoolTime * 1000 > TimeManager.NowMs)
							{
								UICommon.SetNoticeMessage("쿨타임입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
								return false;
							}

							Me.CurCharData.SetReturnStoneUseTime(TimeManager.NowMs);
						}

						var itemTableData = DBItem.GetItem(useItem.item_tid);
						var portalData = DBPortal.Get(itemTableData.MovePortalID);
						var stageData = DBStage.Get(portalData.StageID);

						if (ZGameManager.Instance.TryEnterStage(portalData.PortalID, false, useItem.item_id, useItem.item_tid, "", 0, 0, () => { onActionDone?.Invoke(useItem.item_id, useItem.item_tid); }))
						{
							Me.CurCharData.SetUseItemTime(useItem.item_tid, TimeManager.NowMs);
						}
					}
					break;
				case GameDB.E_ItemUseType.Teleport:
					{
						//if (ZGameManager.instance.CurGameMode == null || ZGameManager.instance.CurGameMode.StageTable == null || ZGameManager.instance.CurGameMode.StageTable.TeleportType == GameDB.E_TeleportType.None)
						//{
						//	ZLog.Log($"현재 텔레포트 사용 불가 상태입니다.");
						//	UIManager.NoticeMessage(DBLocale.GetLocaleText("Teleport_NotUse_Stage"));
						//	return false;
						//}

						//if (false == ZGameModeBase.LocalPawnController?.CanReturnHome())
						//{
						//	ZLog.Log($"현재 텔레포트 사용 불가 상태입니다.");
						//	UIManager.NoticeMessage(DBLocale.GetLocaleText("Teleport_NotUse"));
						//	return false;
						//}

						//NetData.Instance.SetUseItemTime(NetData.UserID, NetData.CharID, useItem.item_tid, TimeManager.Now);
						//ItemUse(useItem.item_id, useItem.item_tid, (uint)1, (x, y) =>
						//{
						//	ItemSlot.DelayGlobalCoolTime(0.3f);
						//	onActionDone?.Invoke(useItem.item_id, useItem.item_tid);

						//	ZGameModeBase.LocalPawnController.CancelAndClearTarget();
						//	ZGameModeBase.LocalPawnController.CmdRandomTeleport();
						//	ZGameModeBase.SaveCharacter(ZGameModeBase.LocalPawnController.MyPawn);
						//});
					}
					break;
				case GameDB.E_ItemUseType.Indulgence:
					{
						if (useItem.cnt <= 0)
						{
							//개수 부족!
							return false;
						}

						if (Me.CurCharData.Tendency >= DBConfig.MaxGoodTendency)
						{
							//꽉참!
							return false;
						}

						REQ_UseTendencyRecover(useItem.item_tid, (x, y) =>
						{
						});

						//if (useItem.cnt <= 0)
						//{
						//	UIManager.WarnMessage("사용 가능한 면죄부가 없습니다.");
						//	return false;
						//}

						//NetData.Instance.SetUseItemTime(NetData.UserID, NetData.CharID, useItem.item_tid, TimeManager.Now);
						//NetData.Instance.RemoveItem(NetData.UserID, NetData.CharID, useItem.item_id, useItem.item_tid, 1);

						//UseTendencyRecover(useItem.item_tid, (x, y) => {
						//	ItemSlot.DelayGlobalCoolTime(0.3f);
						//	float calcedCoolT = 0;
						//	if (null != ZGameModeBase.LocalPawnController)
						//		calcedCoolT = ZGameModeBase.LocalPawnController.GetCalcedPotionCool(useItem.item_tid);
						//	else
						//		calcedCoolT = DBItem.GetItemCoolTime(useItem.item_tid);
						//	//ItemSlot.DelayCoolTime(calcedCoolT, useItem.item_tid);
						//	onActionDone?.Invoke(useItem.item_id, useItem.item_tid);

						//	UIManager.NoticeMessage(DBLocale.GetLocaleText("WItemUse_Indulgence", y.IncTendency));
						//});
					}
					break;
				case GameDB.E_ItemUseType.ChickenCoupon:
					{
						//if (useItem.cnt <= 0)
						//	return false;
						//(UIManager.instance.OpenPopup("ChickenCouponPopup") as ChickenCouponPopup).Set(useItem.item_tid);
					}
					break;
				case GameDB.E_ItemUseType.Ticket:
					{
						//if (useItem.cnt <= 0)
						//	return false;

						//if (DBItem.GetItemType(useItem.item_tid) == GameDB.E_ItemType.NameChange)
						//{
						//	var Popup = (UIManager.instance.OpenPopup("CreateNickPopup") as CreateNickPopup);
						//	Popup.CharId = NetData.CharID;
						//	Popup.SetUseItem(useItem);
						//	Popup.OnChangeSuccess = (changeNick) => {
						//		MessagePopup.OpenSimpleMessagePopup(string.Format(DBLocale.GetLocaleText("PNameChange_Complete"), changeNick), () => {
						//			ZGameManager.ReturnCharacterSelect();
						//		});
						//	};
						//}
						//else if (DBItem.GetItemType(useItem.item_tid) == GameDB.E_ItemType.ClassChange)
						//{
						//	if (null != ZGameManager.instance.CurGameMode && ZGameManager.instance.CurGameMode.StageTable.StageType == GameDB.E_StageType.Town)
						//	{
						//		UIManager.instance.OpenPanel("CharChangePanel");
						//	}
						//	else
						//	{
						//		UIManager.NoticeMessage("마을에서만 사용 가능합니다.");
						//	}
						//}
					}
					break;
				case GameDB.E_ItemUseType.SkillBook:
					{
						if (useItem.cnt <= 0)
						{
							ZLog.LogWarn(ZLogChannel.WebSocket, $"{itemTable.ItemUseType} | [{useItem.item_tid}]아이템 개수가 0개입니다.");
							return false;
						}

						var skillFrame = UIManager.Instance.Find<UIFrameSkill>();
						var skillData = DBSkill.Get(DBItem.GetItem(useItem.item_tid).OpenSkillID);
						var inventoryFrame = UIManager.Instance.Find<UIFrameInventory>();

						if (!Me.CurCharData.HasGainSkill(skillData.SkillID))
						{
							ZWebManager.Instance.WebGame.ReqGainSkill(skillData.SkillID, delegate
							{
								if (skillFrame != null)
									skillFrame.UpdateGainSkill(skillData.SkillID);

								if (inventoryFrame != null)
									inventoryFrame.UpdateGainSkill(skillData);

								onActionDone?.Invoke(0, 0);
							});
						}
						else
							UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
							{
								_popup.Open(ZUIString.WARRING, DBLocale.GetText("이미 습득한 스킬입니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
								_popup.Close(); } });
							});
					}
					break;
				case GameDB.E_ItemUseType.Restoration:
					{
						//if (useItem.cnt <= 0)
						//	return false;

						//System.Action reqRecovery = () => {
						//	var category = DBRestoration.GetCategory(DBItem.GetItem(useItem.item_tid).RestorationGroup);

						//	switch (category)
						//	{
						//		case GameDB.E_CategoryType.Equip:
						//		case GameDB.E_CategoryType.EquipAcc:
						//		case GameDB.E_CategoryType.SkillBook:
						//			UIManager.instance.OpenPanel("RestoreItemPanel", useItem, category);
						//			break;
						//		case GameDB.E_CategoryType.Pet:
						//		case GameDB.E_CategoryType.Change:
						//			UIManager.instance.OpenPanel("RestorePetPanel_R2", useItem, category);
						//			break;
						//	}
						//};

						//if (!UIShortcut.CheckOverWeightInven(!bAuto, () =>
						//{
						//	reqRecovery.Invoke();
						//}))
						//{
						//	reqRecovery.Invoke();
						//}
						//else
						//	return false;
					}
					break;
			}
			return true;
		}

		//public static void GetItemList(System.Action<ReceiveFBMessage, ResGetItemList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_ITEM_LIST);

		//	builder.Clear();

		//	ReqGetItemList.StartReqGetItemList(builder);
		//	var offset = ReqGetItemList.EndReqGetItemList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetItemList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetItemList recvMsgPacket = recvPacket.GetResMsg<ResGetItemList>();

		//		NetData.Instance.ClearInvenList(NetData.UserID, NetData.CharID);
		//		if (recvMsgPacket.ItemList != null)
		//			AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.ItemList.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_EquipItem(ulong item_id, uint item_tid, byte slot_idx, System.Action<ZWebRecvPacket, ResItemEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var equips = new Offset<EquipInfo>[1];
			equips[0] = EquipInfo.CreateEquipInfo(mBuilder, item_id, item_tid, slot_idx);

			var offset = ReqItemEquip.CreateReqItemEquip(mBuilder, ReqItemEquip.CreateEquipInfosVector(mBuilder, equips));

			var reqPacket = ZWebPacket.Create<ReqItemEquip>(this, Code.GS_ITEM_EQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemEquip recvMsgPacket = recvPacket.Get<ResItemEquip>();

				if (recvMsgPacket.EquipResultInfosLength > 0)
				{
					List<EquipResultInfo> equipsinfos = new List<EquipResultInfo>();

					for (int i = 0; i < recvMsgPacket.EquipResultInfosLength; i++)
						equipsinfos.Add(recvMsgPacket.EquipResultInfos(i).Value);

					Me.CurCharData.EquipItems(equipsinfos);

					string resultVal = "";
					switch (Me.CurCharData.SelectEquipSetNo)
					{
						case 1:
							resultVal = Me.CurCharData.AddEquipItemCurrentSet(item_id, slot_idx);
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET1, resultVal, null);
							break;
						case 2:
							resultVal = Me.CurCharData.AddEquipItemCurrentSet(item_id, slot_idx);
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET2, resultVal, null);
							break;
						case 3:
							resultVal = Me.CurCharData.AddEquipItemCurrentSet(item_id, slot_idx);
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET3, resultVal, null);
							break;
					}
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_EquipItems(IList<OptionEquipInfo> equipList, System.Action<ZWebRecvPacket, ResItemEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var equips = new Offset<EquipInfo>[equipList.Count];

			for (int i = 0; i < equipList.Count; i++)
			{
				equips[i] = EquipInfo.CreateEquipInfo(mBuilder, equipList[i].UniqueID, Me.CurCharData.GetItemData(equipList[i].UniqueID, NetItemType.TYPE_EQUIP).item_tid, equipList[i].SlotIdx);
			}

			var offset = ReqItemEquip.CreateReqItemEquip(mBuilder, ReqItemEquip.CreateEquipInfosVector(mBuilder, equips));

			var reqPacket = ZWebPacket.Create<ReqItemEquip>(this, Code.GS_ITEM_EQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemEquip recvMsgPacket = recvPacket.Get<ResItemEquip>();

				if (recvMsgPacket.EquipResultInfosLength > 0)
				{
					List<EquipResultInfo> equipsinfos = new List<EquipResultInfo>();

					for (int i = 0; i < recvMsgPacket.EquipResultInfosLength; i++)
						equipsinfos.Add(recvMsgPacket.EquipResultInfos(i).Value);

					Me.CurCharData.EquipItems(equipsinfos);
				}
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

				Dictionary<ulong, uint> strequipList = new Dictionary<ulong, uint>();
				for (int i = 0; i < equipList.Count; i++)
				{
					strequipList.Add(equipList[i].UniqueID, Me.CurCharData.GetItemData(equipList[i].UniqueID, NetItemType.TYPE_EQUIP).item_tid);
				}
			}, _onError);
		}

		public void REQ_UnEquipItem(ulong item_id, uint item_tid, byte slot, System.Action<ZWebRecvPacket, ResItemUnequip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var equips = new Offset<EquipInfo>[1];
			equips[0] = EquipInfo.CreateEquipInfo(mBuilder, item_id, item_tid, slot);

			var offset = ReqItemUnequip.CreateReqItemUnequip(mBuilder, ReqItemUnequip.CreateUnequipInfosVector(mBuilder, equips));

			var reqPacket = ZWebPacket.Create<ReqItemUnequip>(this, Code.GS_ITEM_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemUnequip recvMsgPacket = recvPacket.Get<ResItemUnequip>();

				if (recvMsgPacket.UnequipResultInfosLength > 0)
				{
					List<ulong> unequipids = new List<ulong>();
					for (int i = 0; i < recvMsgPacket.UnequipResultInfosLength; i++)
						unequipids.Add(recvMsgPacket.UnequipResultInfos(i));
					Me.CurCharData.UnEquipItems(unequipids);

					switch (Me.CurCharData.SelectEquipSetNo)
					{
						case 1:
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET1, Me.CurCharData.RemoveEquipItemCurrentSet(item_id), null);
							break;
						case 2:
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET2, Me.CurCharData.RemoveEquipItemCurrentSet(item_id), null);
							break;
						case 3:
							REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET3, Me.CurCharData.RemoveEquipItemCurrentSet(item_id), null);
							break;
					}
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_UnEquipItems(IList<ZItem> unequipList, System.Action<ZWebRecvPacket, ResItemUnequip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var unequips = new Offset<EquipInfo>[unequipList.Count];

			for (int i = 0; i < unequipList.Count; i++)
				unequips[i] = EquipInfo.CreateEquipInfo(mBuilder, unequipList[i].item_id, unequipList[i].item_tid, unequipList[i].slot_idx);

			var offset = ReqItemUnequip.CreateReqItemUnequip(mBuilder, ReqItemUnequip.CreateUnequipInfosVector(mBuilder, unequips));

			var reqPacket = ZWebPacket.Create<ReqItemUnequip>(this, Code.GS_ITEM_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemUnequip recvMsgPacket = recvPacket.Get<ResItemUnequip>();

				if (recvMsgPacket.UnequipResultInfosLength > 0)
				{
					List<ulong> unequipids = new List<ulong>();
					for (int i = 0; i < recvMsgPacket.UnequipResultInfosLength; i++)
						unequipids.Add(recvMsgPacket.UnequipResultInfos(i));
					Me.CurCharData.UnEquipItems(unequipids);
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 아이템 버리기,삭제 </summary>
		public void REQ_DeleteItem(List<ZItem> deletingItems, System.Action<ZWebRecvPacket, ResItemDelete> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var delitemoffs = new Offset<DelItemInfo>[deletingItems.Count];
			for (int i = 0; i < deletingItems.Count; i++)
			{
				delitemoffs[i] = DelItemInfo.CreateDelItemInfo(mBuilder, deletingItems[i].item_id, deletingItems[i].item_tid, (uint)deletingItems[i].cnt);
			}

			var offset = ReqItemDelete.CreateReqItemDelete(mBuilder, ReqItemDelete.CreateDelItemsVector(mBuilder, delitemoffs));
			var reqPacket = ZWebPacket.Create<ReqItemDelete>(this, Code.GS_ITEM_DELETE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemDelete resItemDelete = recvPacket.Get<ResItemDelete>();

				List<DelItemInfo> delItems = new List<DelItemInfo>();

				for (int i = 0; i < resItemDelete.DelItemsLength; i++)
					delItems.Add(resItemDelete.DelItems(i).Value);

				Me.CurCharData.RemoveItemList(delItems);

				onRecvPacket?.Invoke(recvPacket, resItemDelete);
			}, _onError);
		}

		public void REQ_BreakItem(List<ZItem> BreakItems, System.Action<ZWebRecvPacket, ResItemBreak> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var breaks = new Offset<BreakItemInfo>[BreakItems.Count];
			for (int i = 0; i < BreakItems.Count; i++)
			{
				breaks[i] = BreakItemInfo.CreateBreakItemInfo(mBuilder, BreakItems[i].item_id, BreakItems[i].item_tid, (uint)BreakItems[i].cnt);
			}

			var offset = ReqItemBreak.CreateReqItemBreak(mBuilder, ReqItemBreak.CreateBreakItemsVector(mBuilder, breaks));

			var reqPacket = ZWebPacket.Create<ReqItemBreak>(this, Code.GS_ITEM_BREAK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemBreak recvMsgPacket = recvPacket.Get<ResItemBreak>();

				List<DelItemInfo> delitems = new List<DelItemInfo>();

				for (int i = 0; i < recvMsgPacket.BreakItemsLength; i++)
					delitems.Add(recvMsgPacket.BreakItems(i).Value);

				Me.CurCharData.RemoveItemList(delitems);

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_EnchantItem(ushort EnchantType, IList<ZItem> EnchantItems, ulong Materialid, uint MaterialTid, System.Action<ZWebRecvPacket, ResItemEnchant> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var enchants = new Offset<EnchantItemInfo>[EnchantItems.Count];
			for (int i = 0; i < EnchantItems.Count; i++)
			{
				if (Materialid == 0)
					enchants[i] = EnchantItemInfo.CreateEnchantItemInfo(mBuilder, EnchantItems[i].item_id, EnchantItems[i].item_tid, EnchantItems[i].slot_idx);
				else
					enchants[i] = EnchantItemInfo.CreateEnchantItemInfo(mBuilder, EnchantItems[i].item_id, EnchantItems[i].item_tid, EnchantItems[i].slot_idx, Materialid, MaterialTid);
			}

			ZItem goldItem = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

			var offset = ReqItemEnchant.CreateReqItemEnchant(mBuilder, EnchantType, goldItem.item_id, ReqItemEnchant.CreateEnchantItemsVector(mBuilder, enchants));

			var reqPacket = ZWebPacket.Create<ReqItemEnchant>(this, Code.GS_ITEM_ENCHANT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				if ((ERROR)recvPacket.ErrCode == ERROR.NO_ERROR)
				{
					ResItemEnchant recvMsgPacket = recvPacket.Get<ResItemEnchant>();

					for (int i = 0; i < recvMsgPacket.ResultEnchantLength; i++)
					{
						var finditem = Me.CurCharData.GetItemData(recvMsgPacket.ResultEnchant(i).Value.ItemId, NetItemType.TYPE_EQUIP);
						if (finditem != null && finditem.slot_idx != 0)
						{
							Me.CurCharData.UnEquipItem(finditem.item_id);
						}
					}

					List<DelItemInfo> delitems = new List<DelItemInfo>();
					for (int i = 0; i < recvMsgPacket.DelItemsLength; i++)
						delitems.Add(recvMsgPacket.DelItems(i).Value);

					Me.CurCharData.RemoveItemList(delitems);

					for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
					for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultEquipItems(i).Value);
					for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

					onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
				}
				else
				{
					onRecvPacket?.Invoke(recvPacket, default);
				}
			}, _onError);
		}

		//public static void NotUseItemEnchant(ushort EnchantType, IList<ZItem> EnchantItems, System.Action<ReceiveFBMessage, ResNotUseItemEnchant> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_ITEM_NOT_USE_ENCHANT);

		//	builder.Clear();

		//	var enchants = new Offset<EnchantItemInfo>[EnchantItems.Count];
		//	for (int i = 0; i < EnchantItems.Count; i++)
		//	{
		//		enchants[i] = EnchantItemInfo.CreateEnchantItemInfo(builder, EnchantItems[i].item_id, EnchantItems[i].item_tid, EnchantItems[i].slot_idx, 0, 0);
		//	}

		//	ZItem goldItem = NetData.Instance.GetInvenItem(NetData.UserID, NetData.CharID, DBConfig.Gold_ID);

		//	var offset = ReqNotUseItemEnchant.CreateReqNotUseItemEnchant(builder, EnchantType, goldItem.item_id, ReqNotUseItemEnchant.CreateEnchantItemsVector(builder, enchants));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqNotUseItemEnchant>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		if ((ERROR)recvPacket.GetErrorCode() == ERROR.NO_ERROR)
		//		{
		//			ResNotUseItemEnchant recvMsgPacket = recvPacket.GetResMsg<ResNotUseItemEnchant>();

		//			for (int i = 0; i < recvMsgPacket.ResultEnchantLength; i++)
		//			{
		//				var finditem = NetData.Instance.GetInvenItem(NetData.UserID, NetData.CharID, NetItemType.TYPE_EQUIP, recvMsgPacket.ResultEnchant(i).Value.ItemId);
		//				if (finditem != null && finditem.slot_idx != 0)
		//				{
		//					NetData.Instance.UnEquipItem(NetData.UserID, NetData.CharID, finditem.item_id);
		//				}
		//			}

		//			List<DelItemInfo> delitems = new List<DelItemInfo>();
		//			for (int i = 0; i < recvMsgPacket.DelItemsLength; i++)
		//				delitems.Add(recvMsgPacket.DelItems(i).Value);

		//			NetData.Instance.RemoveItemList(NetData.UserID, NetData.CharID, delitems);

		//			List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//			List<ItemStack> invenStackList = new List<ItemStack>();
		//			List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//			for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//				invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//			for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
		//				invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
		//			for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//				invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//			NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//			onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//			string enchantList = "[";
		//			for (int i = 0; i < recvMsgPacket.ResultEnchantLength; i++)
		//			{
		//				enchantList +=
		//					"{" + string.Format("ItemTid:{0},Result:{1},ResultItemTid:{2}", recvMsgPacket.ResultEnchant(i).Value.FromItemTid, recvMsgPacket.ResultEnchant(i).Value.Result, recvMsgPacket.ResultEnchant(i).Value.ToItemTid) + "},";
		//			}
		//			enchantList += "]";

		//			AnalyticsManager.instance.Event(AnalyticsManager.EVENT_ENCHANT_ITEM, new Dictionary<string, object>()
		//		{
		//			{ "UserID", NetData.UserID },
		//			{ "CharID", NetData.CharID },
		//			{ "EnchantList",enchantList},
		//		});
		//		}
		//		else
		//		{
		//			onRecvPacket?.Invoke(recvPacket, default);
		//		}
		//	});
		//}

		//public static void CheckPeriodItems(List<ZItem> checkItems, System.Action<ReceiveFBMessage, ResGetCheckItem> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_CHECK_ITEM);

		//	builder.Clear();

		//	List<ulong> checkids = new List<ulong>();
		//	foreach (var item in checkItems)
		//		checkids.Add(item.item_id);

		//	var offset = ReqGetCheckItem.CreateReqGetCheckItem(builder, ReqGetCheckItem.CreateGetItemIdsVector(builder, checkids.ToArray()));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetCheckItem>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetCheckItem recvMsgPacket = recvPacket.GetResMsg<ResGetCheckItem>();

		//		List<ItemEquipment> equips = new List<ItemEquipment>();

		//		for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
		//		{
		//			checkids.Remove(recvMsgPacket.ResultEquipItems(i).Value.ItemId);
		//			equips.Add(recvMsgPacket.ResultEquipItems(i).Value);
		//		}

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, null, null, equips);

		//		for (int i = 0; i < checkids.Count; i++)
		//			NetData.Instance.RemoveItem(NetData.UserID, NetData.CharID, NetItemType.TYPE_EQUIP, checkids[i]);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		#region ItemSocket
		public void REQ_EquipGemItem(ulong EquipItemId, uint EquipItemTid, uint GemItemTid, byte SocketIdx, System.Action<ZWebRecvPacket, ResGemItemEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)//socket 1~ 6
		{
			var offset = ReqGemItemEquip.CreateReqGemItemEquip(mBuilder, EquipItemId, EquipItemTid, GemItemTid, SocketIdx);

			var reqPacket = ZWebPacket.Create<ReqGemItemEquip>(this, Code.GS_GEM_ITEM_EQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGemItemEquip recvMsgPacket = recvPacket.Get<ResGemItemEquip>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultEquipItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_UnEquipGemItem(ulong EquipItemId, uint EquipItemTid, List<byte> SocketIdxs, System.Action<ZWebRecvPacket, ResGemItemUnEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)//socket 1~ 6
		{
			var offset = ReqGemItemUnEquip.CreateReqGemItemUnEquip(mBuilder, EquipItemId, EquipItemTid, ReqGemItemUnEquip.CreateSocketIdxsVector(mBuilder, SocketIdxs.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqGemItemUnEquip>(this, Code.GS_GEM_ITEM_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGemItemUnEquip recvMsgPacket = recvPacket.Get<ResGemItemUnEquip>();

				List<AccountItemStack> accountItems = new List<AccountItemStack>();
				List<ItemEquipment> equipItems = new List<ItemEquipment>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultEquipItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Lock
		public void REQ_ToggleLock(GameDB.E_GoodsKindType goodsType, ulong ID, uint Tid, bool IsLock, System.Action<ZWebRecvPacket, ResItemLockToggle> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemLockToggle.CreateReqItemLockToggle(mBuilder, (byte)goodsType, ID, Tid, (byte)(IsLock ? 1 : 0));
			var reqPacket = ZWebPacket.Create<ReqItemLockToggle>(this, Code.GS_ITEM_LOCK_TOGGLE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemLockToggle recvMsgPacket = recvPacket.Get<ResItemLockToggle>();

				AddRemainItems(recvMsgPacket.RemainItems.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);

		}
		#endregion

		//#region Skill
		//public static void LearnSpecialSkill(uint SkillTid, System.Action<ReceiveFBMessage, ResUseSpecialSkillPoint> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_USE_SPECIAL_SKILL_POINT);

		//	builder.Clear();

		//	var offset = ReqUseSpecialSkillPoint.CreateReqUseSpecialSkillPoint(builder, SkillTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqUseSpecialSkillPoint>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResUseSpecialSkillPoint recvMsgPacket = recvPacket.GetResMsg<ResUseSpecialSkillPoint>();

		//		NetData.Instance.AddSpecialSkill(NetData.UserID, NetData.CharID, recvMsgPacket.SpecialSkillTid);

		//		//List<ItemStack> invenStackList = new List<ItemStack>();
		//		//NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, null, invenStackList, null);

		//		AlramUI.CheckAlram(Alram.SPECIAL_SKILL);
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void ResetSpecialSkill(ulong UseItemID, uint UseItemTid, System.Action<ReceiveFBMessage, ResResetSpecialSkillPoint> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RESET_SPECIAL_SKILL_POINT);

		//	builder.Clear();

		//	var offset = ReqResetSpecialSkillPoint.CreateReqResetSpecialSkillPoint(builder, UseItemID, UseItemTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqResetSpecialSkillPoint>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResResetSpecialSkillPoint recvMsgPacket = recvPacket.GetResMsg<ResResetSpecialSkillPoint>();

		//		NetData.Instance.ClearSpecialSkill(NetData.UserID, NetData.CharID);

		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, null);

		//		AlramUI.CheckAlram(Alram.SPECIAL_SKILL);
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void ReqGainSkill(uint SkillTid, System.Action<ZWebRecvPacket, ResSkillItemUse> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqSkillItemUse.CreateReqSkillItemUse(mBuilder, SkillTid);

			var reqPacket = ZWebPacket.Create<ReqSkillItemUse>(this, Code.GS_SKILL_ITEM_USE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSkillItemUse recvMsgPacket = recvPacket.Get<ResSkillItemUse>();

				Me.CurCharData.AddGainSkill(recvMsgPacket.SkillTid);

				List<ItemStack> invenStackList = new List<ItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenStackList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		//#endregion

		//#region SlotBuy
		public void BuyInventorySlot(ulong UseItemID, uint UseItemTid, System.Action<ZWebRecvPacket, ResInvenSlotExtend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInvenSlotExtend.CreateReqInvenSlotExtend(mBuilder, ZGameManager.Instance.GetMarketType(), UseItemID, UseItemTid);

			var reqPacket = ZWebPacket.Create<ReqInvenSlotExtend>(this, Code.GS_INVEN_SLOT_EXTEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInvenSlotExtend recvMsgPacket = recvPacket.Get<ResInvenSlotExtend>();

				Me.CurCharData.UpdateInvenMaxCnt(recvMsgPacket.InvenMaxCnt);

				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);
				if (recvMsgPacket.ResultAccountStackItem != null)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItem.Value);
				if (recvMsgPacket.ResultStackItem != null)
					invenStackList.Add(recvMsgPacket.ResultStackItem.Value);

				if (invenAccountStackList.Count > 0) for (int i = 0; i < invenAccountStackList.Count; i++) Me.CurCharData.AddItemList(invenAccountStackList[i]);
				if (invenStackList.Count > 0) for (int i = 0; i < invenStackList.Count; i++) Me.CurCharData.AddItemList(invenStackList[i]);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_BuyStorageSlot(ulong UseItemID, uint UseItemTid, System.Action<ZWebRecvPacket, ResStorageSlotExtend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqStorageSlotExtend.CreateReqStorageSlotExtend(mBuilder,ZGameManager.Instance.GetMarketType(), UseItemID, UseItemTid);
			var reqPacket = ZWebPacket.Create<ReqStorageSlotExtend>(this, Code.GS_STORAGE_SLOT_EXTEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResStorageSlotExtend recvMsgPacket = recvPacket.Get<ResStorageSlotExtend>();

				Me.CurUserData.SetStorageMaxCnt(recvMsgPacket.StorageMaxCnt);

				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();
				if (recvMsgPacket.ResultAccountStackItem != null)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItem.Value);
				if (recvMsgPacket.ResultStackItem != null)
					invenStackList.Add(recvMsgPacket.ResultStackItem.Value);

				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);

				Me.CurCharData.AddItemList(invenStackList);
				Me.CurCharData.AddItemList(invenAccountStackList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		//public static void BuyRingSlot(ulong UseItemID, System.Action<ReceiveFBMessage, ResRingSlotExtend> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RING_SLOT_EXTEND);

		//	builder.Clear();

		//	var offset = ReqRingSlotExtend.CreateReqRingSlotExtend(builder, UseItemID);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRingSlotExtend>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRingSlotExtend recvMsgPacket = recvPacket.GetResMsg<ResRingSlotExtend>();

		//		NetData.Instance.UpdateRingMaxCnt(NetData.UserID, NetData.CharID, (byte)recvMsgPacket.AddRingSlot);

		//		if (recvMsgPacket.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.ResultStackItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void BuyQuickSlotSlot(ulong UseItemID, uint UseItemTid, System.Action<ZWebRecvPacket, ResQuickSlotExtend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqQuickSlotExtend.CreateReqQuickSlotExtend(mBuilder, UseItemID, UseItemTid);

			var reqPacket = ZWebPacket.Create<ReqQuickSlotExtend>(this, Code.GS_QUICK_SLOT_EXTEND, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResQuickSlotExtend recvMsgPacket = recvPacket.Get<ResQuickSlotExtend>();

				Me.CurCharData.SetQuickSlotCount(recvMsgPacket.QuickSlotMaxCnt);

				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();
				if (recvMsgPacket.ResultAccountStackItem != null)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItem.Value);
				if (recvMsgPacket.ResultStackItem != null)
					invenStackList.Add(recvMsgPacket.ResultStackItem.Value);

				if (invenAccountStackList.Count > 0) for (int i = 0; i < invenAccountStackList.Count; i++) Me.CurCharData.AddItemList(invenAccountStackList[i]);
				if (invenStackList.Count > 0) for (int i = 0; i < invenStackList.Count; i++) Me.CurCharData.AddItemList(invenStackList[i]);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void BuyCharacterSlot(ulong UseItemID, System.Action<ZWebRecvPacket, ResCharacterSlotExtend> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharacterSlotExtend.CreateReqCharacterSlotExtend(mBuilder, UseItemID);

			var reqPacket = ZWebPacket.Create<ReqCharacterSlotExtend>(this, Code.GS_CHARACTER_SLOT_EXTEND, mBuilder, offset.Value);
			Debug.Log(Me.MaxCharCnt);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharacterSlotExtend recvMsgPacket = recvPacket.Get<ResCharacterSlotExtend>();

				Me.MaxCharCnt = recvMsgPacket.CharacterMaxCnt;

				if (recvMsgPacket.ResultStackItem != null)
					Me.CurCharData.AddItem(recvMsgPacket.ResultStackItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		//public static void BuySpecialSkillSlot(ulong UseItemID, uint UseItemTid, System.Action<ReceiveFBMessage, ResSpecialSkillExtend> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_SPECIAL_SKILL_EXTEND);

		//	builder.Clear();

		//	var offset = ReqSpecialSkillExtend.CreateReqSpecialSkillExtend(builder, UseItemID, UseItemTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqSpecialSkillExtend>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResSpecialSkillExtend recvMsgPacket = recvPacket.GetResMsg<ResSpecialSkillExtend>();

		//		NetData.Instance.SetSpecialSkillMaxCnt(NetData.UserID, (byte)recvMsgPacket.SpecialSkillMaxCnt);

		//		if (recvMsgPacket.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.ResultStackItem.Value);

		//		AlramUI.CheckAlram(Alram.SPECIAL_SKILL);
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void BuySeasonPass(ulong UseItemID, uint UseItemTid, System.Action<ReceiveFBMessage, ResUseSeason> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_USE_SEASON);

		//	builder.Clear();

		//	var offset = ReqUseSeason.CreateReqUseSeason(builder, UseItemID, UseItemTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqUseSeason>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResUseSeason recvMsgPacket = recvPacket.GetResMsg<ResUseSeason>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//#endregion

		//#region Potion
		//public void REQ_UsePotion(ulong UseItemID, uint UseItemTid, ulong UseTime, System.Action<ZWebRecvPacket, ResPotionItemUse> onRecvPacket, PacketErrorCBDelegate _onError = null)
		//{
		//	mBuilder.Clear();

		//	var offset = ReqPotionItemUse.CreateReqPotionItemUse(mBuilder, UseItemID, UseItemTid, UseTime);
		//	mBuilder.Finish(offset.Value);

		//	var reqPacket = ZWebPacket.Create<ReqPotionItemUse>(this, Code.GS_POTION_ITEM_USE, mBuilder, offset.Value);

		//	SendPacket(reqPacket, (recvPacket) =>
		//	{
		//		ResPotionItemUse recvMsgPacket = recvPacket.Get<ResPotionItemUse>();

		//		if (recvMsgPacket.RemainStack != null)
		//			Me.CurCharData.AddItem(recvMsgPacket.RemainStack.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	}, _onError);
		//}
		//#endregion

		//#region ItemUse


		public void REQ_UseTendencyRecover(uint useItemTid, System.Action<ZWebRecvPacket, ResItemUseTendency> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemUseTendency.CreateReqItemUseTendency(mBuilder, useItemTid);
			var reqPacket = ZWebPacket.Create<ReqItemUseTendency>(this, Code.GS_ITEM_USE_TENDENCY, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemUseTendency recvMsgPacket = recvPacket.Get<ResItemUseTendency>();

				List<ItemStack> stackList = new List<ItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					stackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(stackList);

				//mmo 쪽에서 갱신됨 굳이 갱신할 필요 없을듯.
				//Me.CurCharData.UpdateTendency(recvMsgPacket.ResultTendency);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		//public static void GetRecoveryList(GameDB.E_CategoryType RecoveryType, System.Action<ReceiveFBMessage, ResRecoveryItemInfo> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RECOVERY_ITEM_INFO);

		//	builder.Clear();

		//	var offset = ReqRecoveryItemInfo.CreateReqRecoveryItemInfo(builder, (byte)RecoveryType);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRecoveryItemInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRecoveryItemInfo recvMsgPacket = recvPacket.GetResMsg<ResRecoveryItemInfo>();
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void TakeRecoveryItem(uint UseItemTid, GameDB.E_CategoryType RecoveryType, uint RecoveryTid, System.Action<ReceiveFBMessage, ResRecoveryItem> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RECOVERY_ITEM);

		//	builder.Clear();

		//	var offset = ReqRecoveryItem.CreateReqRecoveryItem(builder, UseItemTid, (byte)RecoveryType, RecoveryTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRecoveryItem>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRecoveryItem recvMsgPacket = recvPacket.GetResMsg<ResRecoveryItem>();

		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();
		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		List<Change> changeList = new List<Change>();
		//		for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
		//			changeList.Add(recvMsgPacket.ResultChanges(i).Value);
		//		NetData.Instance.AddChangeList(NetData.UserID, NetData.CharID, changeList);


		//		List<ChangeGachaKeep> changeKeepList = new List<ChangeGachaKeep>();

		//		for (int i = 0; i < recvMsgPacket.ResultChangesGachaKeepsLength; i++)
		//			changeKeepList.Add(recvMsgPacket.ResultChangesGachaKeeps(i).Value);

		//		NetData.Instance.AddChangeKeepList(NetData.UserID, NetData.CharID, changeKeepList);

		//		List<Pet> petList = new List<Pet>();
		//		for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
		//			petList.Add(recvMsgPacket.ResultPets(i).Value);
		//		NetData.Instance.AddPetList(NetData.UserID, NetData.CharID, petList);

		//		List<PetGachaKeep> petKeepList = new List<PetGachaKeep>();

		//		for (int i = 0; i < recvMsgPacket.ResultPetsGachaKeepsLength; i++)
		//			petKeepList.Add(recvMsgPacket.ResultPetsGachaKeeps(i).Value);

		//		NetData.Instance.AddPetKeepList(NetData.UserID, NetData.CharID, petKeepList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		#region ItemCollect
		//public void GetItemCollectionList(System.Action<ReceiveFBMessage, ResGetItemCollectList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_ITEM_COLLECT_LIST);

		//	ReqGetItemCollectList.StartReqGetItemCollectList(builder);
		//	var offset = ReqGetItemCollectList.EndReqGetItemCollectList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetItemCollectList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetItemCollectList recvMsgPacket = recvPacket.GetResMsg<ResGetItemCollectList>();

		//		NetData.Instance.ClearItemCollectList(NetData.UserID, NetData.CharID);

		//		List<Collect> collectList = new List<Collect>();

		//		for (int i = 0; i < recvMsgPacket.ItemCollectsLength; i++)
		//			collectList.Add(recvMsgPacket.ItemCollects(i).Value);

		//		NetData.Instance.AddItemCollectList(NetData.UserID, NetData.CharID, collectList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_RegistItemCollection(uint CollectTid, uint slotIndex, ulong matItemId, uint matItemTid, System.Action<ZWebRecvPacket, ResItemCollectMake> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemCollectMake.CreateReqItemCollectMake(mBuilder, CollectTid, MaterialCollect.CreateMaterialCollect(mBuilder, slotIndex, matItemId, matItemTid));

			var reqPacket = ZWebPacket.Create<ReqItemCollectMake>(this, Code.GS_ITEM_COLLECT_MAKE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemCollectMake recvMsgPacket = recvPacket.Get<ResItemCollectMake>();

				Me.CurCharData.RemoveItem(NetItemType.TYPE_EQUIP, matItemId);

				Me.CurCharData.UpdateItemCollect(recvMsgPacket.ItemCollectTid, recvMsgPacket.Material.Value.SlotIdx, recvMsgPacket.Material.Value.MaterialTid);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Item Resmelting
		/// <summary> 장비 재련 정보 </summary>
		public void REQ_GetItemResmeltInfo(System.Action<ZWebRecvPacket, ResGetItemSmeltInfo> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetItemSmeltInfo.StartReqGetItemSmeltInfo(mBuilder);
			var offset = ReqGetItemSmeltInfo.EndReqGetItemSmeltInfo(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetItemSmeltInfo>(this, Code.GS_GET_ITEM_SMELT_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetItemSmeltInfo recvMsgPacket = recvPacket.Get<ResGetItemSmeltInfo>();
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		/// <summary> 장비 재련 시작 </summary>
		public void REQ_ItemResmeltStart(ulong itemId, uint itemTid, uint materialTid, System.Action<ZWebRecvPacket, ResItemSmeltStart> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemSmeltStart.CreateReqItemSmeltStart(mBuilder, itemId, itemTid, materialTid);

			var reqPacket = ZWebPacket.Create<ReqItemSmeltStart>(this, Code.GS_ITEM_SMELT_START, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemSmeltStart recvMsgPacket = recvPacket.Get<ResItemSmeltStart>();

				if (recvMsgPacket.RemainItems != null)
				{
					AddRemainItems(recvMsgPacket.RemainItems.Value);
					//UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Enchant);
					if (UIManager.Instance.Find(out UIFrameInventory _inventory))
						_inventory.ShowInvenSort((int)E_InvenSortType.Enchant);
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 장비 재련 저장 </summary>
		public void REQ_ItemResmeltEnd(ulong itemId, uint itemTid, System.Action<ZWebRecvPacket, ResItemSmeltEnd> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemSmeltEnd.CreateReqItemSmeltEnd(mBuilder, itemId, itemTid);

			var reqPacket = ZWebPacket.Create<ReqItemSmeltEnd>(this, Code.GS_ITEM_SMELT_END, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemSmeltEnd recvMsgPacket = recvPacket.Get<ResItemSmeltEnd>();

				if (recvMsgPacket.RemainItems != null && recvMsgPacket.RemainItems.Value.EquipLength > 0)
				{
					if (recvMsgPacket.RemainItems != null)
						AddRemainItems(recvMsgPacket.RemainItems.Value);
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 장비 재련 취소 </summary>
		public void REQ_ItemResmeltCancel(System.Action<ZWebRecvPacket, ResItemSmeltCancel> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqItemSmeltCancel.StartReqItemSmeltCancel(mBuilder);
			var offset = ReqItemSmeltCancel.EndReqItemSmeltCancel(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqItemSmeltCancel>(this, Code.GS_ITEM_SMELT_CANCEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemSmeltCancel recvMsgPacket = recvPacket.Get<ResItemSmeltCancel>();
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Common
		public void AddRemainItems(ItemInfo RemainItems)
		{
			if (RemainItems.AccountStackLength > 0 || RemainItems.StackLength > 0 || RemainItems.EquipLength > 0)
			{
				for (int i = 0; i < RemainItems.AccountStackLength; i++)
					Me.CurCharData.AddItemList(RemainItems.AccountStack(i).Value);
				for (int i = 0; i < RemainItems.StackLength; i++)
					Me.CurCharData.AddItemList(RemainItems.Stack(i).Value);
				for (int i = 0; i < RemainItems.EquipLength; i++)
					Me.CurCharData.AddItemList(RemainItems.Equip(i).Value);
			}

			if (RemainItems.PetLength > 0)
			{
				List<Pet> petList = new List<Pet>();
				List<Pet> rideList = new List<Pet>();

				for (int i = 0; i < RemainItems.PetLength; i++)
				{
					var item = RemainItems.Pet(i).Value;

					if (DBPet.TryGet(item.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petList.Add(RemainItems.Pet(i).Value);

					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideList.Add(RemainItems.Pet(i).Value);
					}
				}

				Me.CurCharData.AddPetList(petList);
				Me.CurCharData.AddRideList(rideList);
			}

			if (RemainItems.ChangeLength > 0)
			{
				List<Change> changeList = new List<Change>();

				for (int i = 0; i < RemainItems.ChangeLength; i++)
					changeList.Add(RemainItems.Change(i).Value);

				Me.CurCharData.AddChangeList(changeList);
			}

			if (RemainItems.RuneLength > 0)
			{
				//룬 획득
				List<Rune> runeList = new List<Rune>();

				for (int i = 0; i < RemainItems.RuneLength; i++)
					runeList.Add(RemainItems.Rune(i).Value);

				Me.CurCharData.AddRuneList(runeList);
			}
		}
		#endregion

		//#region Make
		//public static void GetMakeLimitList(System.Action<ReceiveFBMessage, ResGetMakeItemLimitList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_MAKE_ITEM_LIMIT_LIST);

		//	builder.Clear();

		//	ReqGetMakeItemLimitList.StartReqGetMakeItemLimitList(builder);
		//	var offset = ReqGetMakeItemLimitList.EndReqGetMakeItemLimitList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetMakeItemLimitList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetMakeItemLimitList recvMsgPacket = recvPacket.GetResMsg<ResGetMakeItemLimitList>();

		//		List<MakeLimitInfo> makeLimitList = new List<MakeLimitInfo>();

		//		for (int i = 0; i < recvMsgPacket.MakeLimitsLength; i++)
		//			makeLimitList.Add(recvMsgPacket.MakeLimits(i).Value);

		//		NetData.Instance.ClearMakeLimitInfos(NetData.UserID);
		//		NetData.Instance.AddMakeLimitInfo(NetData.UserID, makeLimitList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_MakeItem(uint MakeTid, uint MakeCnt, List<List<ZItem>> MatItems, System.Action<ZWebRecvPacket, ResItemMake> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var makes = new Offset<MakeMaterialItem>[MatItems.Count];
			for (int i = 0; i < MatItems.Count; i++)
			{
				var makeitems = new Offset<MaterialItem>[MatItems[i].Count];

				for (int j = 0; j < MatItems[i].Count; j++)
				{
					makeitems[j] = MaterialItem.CreateMaterialItem(mBuilder, MatItems[i][j].item_id, MatItems[i][j].item_tid, (uint)MatItems[i][j].cnt);
				}

				makes[i] = MakeMaterialItem.CreateMakeMaterialItem(mBuilder, MakeMaterialItem.CreateMaterialItemsVector(mBuilder, makeitems));
			}

			var offset = ReqItemMake.CreateReqItemMake(mBuilder, MakeTid, MakeCnt, ReqItemMake.CreateMakeMaterialItemsVector(mBuilder, makes));

			var reqPacket = ZWebPacket.Create<ReqItemMake>(this, Code.GS_ITEM_MAKE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				if (recvPacket.ErrCode == 0)
				{
					ResItemMake recvMsgPacket = recvPacket.Get<ResItemMake>();

					List<DelItemInfo> delItems = new List<DelItemInfo>();

					for (int i = 0; i < recvMsgPacket.DelItemsLength; i++)
						delItems.Add(recvMsgPacket.DelItems(i).Value);

					Me.CurCharData.RemoveItemList(delItems);

					for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
					for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultEquipItems(i).Value);
					for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
						Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

					if (recvMsgPacket.MakeLimit != null)
						Me.CurCharData.AddMakeLimitInfo(recvMsgPacket.MakeLimit.Value);

					onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

					int succCnt = 0, failCnt = 0;
					for (int i = 0; i < recvMsgPacket.ResultMakeLength; i++)
					{
						if (recvMsgPacket.ResultMake(i).Value.Result)
							succCnt++;
						else
							failCnt++;
					}
				}
				else
				{
					onRecvPacket?.Invoke(recvPacket, default);
				}
			}, _onError);
		}

		//public static void MakeHonorCoin(uint MakeCnt, System.Action<ReceiveFBMessage, ResExpHonorCoinTrade> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_EXP_HONOR_COIN_TRADE);

		//	builder.Clear();

		//	var offset = ReqExpHonorCoinTrade.CreateReqExpHonorCoinTrade(builder, MakeCnt);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqExpHonorCoinTrade>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResExpHonorCoinTrade recvMsgPacket = recvPacket.GetResMsg<ResExpHonorCoinTrade>();

		//		NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.ResultExp);

		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, null, null);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_MAKE_ITEM, new Dictionary<string, object>()
		//	{
		//		{ "UserID", NetData.UserID },
		//		{ "CharID", NetData.CharID },
		//		{ "MakeCnt",MakeCnt},
		//	});
		//	});
		//}
		//#endregion

		#region Gacha

		public void REQ_GachaItem(ulong GachaItemId, uint GachaItemTid, uint GachaCount, uint SelectGroupIdx, uint SelectRuneType, System.Action<ZWebRecvPacket, ResItemGacha> onReceive, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemGacha.CreateReqItemGacha(mBuilder, GachaItemId, GachaItemTid, GachaCount, SelectGroupIdx, SelectRuneType);
			var reqPacket = ZWebPacket.Create<ReqItemGacha>(this, Code.GS_ITEM_GACHA, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemGacha resGacha = recvPacket.Get<ResItemGacha>();

				if (resGacha.RemainItems != null)
				{
					ItemInfo remainItems = resGacha.RemainItems.Value;

					Me.CurCharData.AddRemainItems(remainItems);
				}

				onReceive?.Invoke(recvPacket, resGacha);
			}, null);
		}

		public void REQ_GachaShopItem(uint itemTid, System.Action<ZWebRecvPacket, ResItemShopGacha> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqItemShopGacha.CreateReqItemShopGacha(mBuilder, itemTid);
			var reqPacket = ZWebPacket.Create<ReqItemShopGacha>(this, Code.GS_ITEM_SHOP_GACHA, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemShopGacha recvMsgPacket = recvPacket.Get<ResItemShopGacha>();

				if (recvMsgPacket.RemainItems != null)
					Me.CurCharData.AddRemainItems(recvMsgPacket.RemainItems.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, null);
		}

		#endregion

		#region Shop

		public void REQ_SellItem(List<ZItem> sellItems, Action<ZWebRecvPacket, ResItemSell> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var oSellItems = new Offset<SellItem>[sellItems.Count];

			for (int i = 0; i < sellItems.Count; i++)
			{
				oSellItems[i] = SellItem.CreateSellItem(mBuilder, sellItems[i].item_id, sellItems[i].item_tid, (uint)sellItems[i].cnt);
			}

			var offset = ReqItemSell.CreateReqItemSell(mBuilder, ReqItemSell.CreateSellItemInfosVector(mBuilder, oSellItems));

			var reqPacket = ZWebPacket.Create<ReqItemSell>(this, Code.GS_ITEM_SELL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemSell recvMsgPacket = recvPacket.Get<ResItemSell>();

				List<DelItemInfo> delList = new List<DelItemInfo>();
				for (int i = 0; i < recvMsgPacket.SellItemInfosLength; i++)
					delList.Add(recvMsgPacket.SellItemInfos(i).Value);

				Me.CurCharData.RemoveItemList(delList);

				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();
				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);

				List<ItemStack> invenStackList = new List<ItemStack>();
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenStackList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_BuyItem(bool isAutoBuyPotion, uint shopTid, uint buyCnt, ulong useItemID, uint useItemTid, Action<ZWebRecvPacket, ResNormalShopBuy> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var buyItem = new Offset<BuyItem>[1];

			buyItem[0] = BuyItem.CreateBuyItem(mBuilder, shopTid, buyCnt, useItemID, useItemTid);

			var offset = ReqNormalShopBuy.CreateReqNormalShopBuy(mBuilder, isAutoBuyPotion, ReqNormalShopBuy.CreateBuyItemsVector(mBuilder, buyItem));

			var reqPacket = ZWebPacket.Create<BuyItem>(this, Code.GS_NORMAL_SHOP_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResNormalShopBuy recvMsgPacket = recvPacket.Get<ResNormalShopBuy>();

				List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurCharData.AddItemList(invenStackList);
				Me.CurCharData.AddItem(invenEquipList);

				List<BuyLimitInfo> buyLimitList = new List<BuyLimitInfo>();

				for (int i = 0; i < recvMsgPacket.ResultBuyLimitsLength; i++)
					buyLimitList.Add(recvMsgPacket.ResultBuyLimits(i).Value);

				Me.CurCharData.AddBuyLimitInfo(buyLimitList);

				////룬 획득
				//List<Rune> runeList = new List<Rune>();

				//for (int i = 0; i < recvMsgPacket.ResultRuneItemsLength; i++)
				//	runeList.Add(recvMsgPacket.ResultRuneItems(i).Value);

				//NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_BuyItems(bool isAutoBuyPotion, ScrollShopItemData[] buyList, Action<ZWebRecvPacket, ResNormalShopBuy> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var arrBuyItem = new Offset<BuyItem>[buyList.Length];

			for (int i = 0; i < arrBuyItem.Length; i++)
			{
				arrBuyItem[i] = BuyItem.CreateBuyItem(mBuilder, buyList[i].shopItemTid, (uint)buyList[i].Count,
													  Me.CurCharData.GetInvenItemUsingMaterial(buyList[i].tableData.BuyItemID).item_id,
													  buyList[i].tableData.BuyItemID);
			}
			var offset = ReqNormalShopBuy.CreateReqNormalShopBuy(mBuilder, isAutoBuyPotion, ReqNormalShopBuy.CreateBuyItemsVector(mBuilder, arrBuyItem));

			var reqPacket = ZWebPacket.Create<BuyItem>(this, Code.GS_NORMAL_SHOP_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResNormalShopBuy recvMsgPacket = recvPacket.Get<ResNormalShopBuy>();

				List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurCharData.AddItemList(invenStackList);
				Me.CurCharData.AddItem(invenEquipList);

				List<BuyLimitInfo> buyLimitList = new List<BuyLimitInfo>();

				for (int i = 0; i < recvMsgPacket.ResultBuyLimitsLength; i++)
					buyLimitList.Add(recvMsgPacket.ResultBuyLimits(i).Value);

				Me.CurCharData.AddBuyLimitInfo(buyLimitList);

				////룬 획득
				//List<Rune> runeList = new List<Rune>();

				//for (int i = 0; i < recvMsgPacket.ResultRuneItemsLength; i++)
				//	runeList.Add(recvMsgPacket.ResultRuneItems(i).Value);

				//NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_GetBuyLimitList(Action<ZWebRecvPacket, ResGetItemBuyLimitList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			ReqGetItemBuyLimitList.StartReqGetItemBuyLimitList(mBuilder);
			var offset = ReqGetItemBuyLimitList.EndReqGetItemBuyLimitList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetItemBuyLimitList>(this, Code.GS_GET_ITEM_BUY_LIMIT_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetItemBuyLimitList recvMsgPacket = recvPacket.Get<ResGetItemBuyLimitList>();

				List<BuyLimitInfo> buyLimitList = new List<BuyLimitInfo>();

				for (int i = 0; i < recvMsgPacket.BuyLimitsLength; i++)
					buyLimitList.Add(recvMsgPacket.BuyLimits(i).Value);

				Me.CurCharData.ClearBuyLimitInfo();
				Me.CurCharData.AddBuyLimitInfo(buyLimitList);

				//AlramUI.CheckAlram(Alram.SPECIAL_SHOP);
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_MileageShopBuy(
			List<DBMileageShop.ItemBuyInfo> buyList
			, Action<ZWebRecvPacket, ResMileageShopBuy> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			Offset<BuyItem>[] vectors = new Offset<BuyItem>[buyList.Count];
			for (int i = 0; i < vectors.Length; ++i)
			{
				vectors[i] = BuyItem.CreateBuyItem(mBuilder, buyList[i].shopTid, buyList[i].buyCnt, buyList[i].useItemId, buyList[i].useItemTid, buyList[i].selectShopListTid, buyList[i].selectListGroupTid);
			}

			var offset = ReqMileageShopBuy.CreateReqMileageShopBuy(mBuilder, ReqMileageShopBuy.CreateBuyItemsVector(mBuilder, vectors));

			var reqPacket = ZWebPacket.Create<ReqMileageShopBuy>(this, Code.GS_MILEAGE_SHOP_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResMileageShopBuy>();

				for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
				{
					var v = recvMsgPacket.ResultPets(i).Value;
					var table = DBPet.GetPetData(v.PetTid);
					if (table.PetType == GameDB.E_PetType.Pet)
					{
						Me.CurCharData.AddPetList(v);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						Me.CurCharData.AddRideList(v);
					}
				}

				List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurCharData.AddItemList(invenStackList);
				Me.CurCharData.AddItem(invenEquipList);

				List<Change> changeList = new List<Change>();
				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				List<BuyLimitInfo> buyLimitList = new List<BuyLimitInfo>();

				for (int i = 0; i < recvMsgPacket.ResultBuyLimitsLength; i++)
					buyLimitList.Add(recvMsgPacket.ResultBuyLimits(i).Value);

				Me.CurCharData.AddBuyLimitInfo(buyLimitList);

				List<Rune> runeList = new List<Rune>();

				for (int i = 0; i < recvMsgPacket.ResultRuneItemsLength; i++)
					runeList.Add(recvMsgPacket.ResultRuneItems(i).Value);

				Me.CurCharData.AddRuneList(runeList);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_SpecialShopBuy(
			List<DBSpecialShop.ItemBuyInfo> buyList
			, Action<ZWebRecvPacket, ResSpecialShopBuy> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			Offset<BuyItem>[] vectors = new Offset<BuyItem>[buyList.Count];
			for (int i = 0; i < vectors.Length; ++i)
			{
				vectors[i] = BuyItem.CreateBuyItem(mBuilder, buyList[i].shopTid, buyList[i].buyCnt, buyList[i].useItemId, buyList[i].useItemTid, buyList[i].selectItemTid);
			}

			var offset = ReqSpecialShopBuy.CreateReqSpecialShopBuy(mBuilder, ZGameManager.Instance.GetMarketType(), ReqSpecialShopBuy.CreateBuyItemsVector(mBuilder, vectors));

			var reqPacket = ZWebPacket.Create<ReqSpecialShopBuy>(this, Code.GS_SPECIAL_SHOP_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResSpecialShopBuy>();

				Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);

				List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
				List<ItemStack> invenStackList = new List<ItemStack>();
				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);
				Me.CurCharData.AddItemList(invenStackList);
				Me.CurCharData.AddItem(invenEquipList);

				List<Change> changeList = new List<Change>();
				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				List<BuyLimitInfo> buyLimitList = new List<BuyLimitInfo>();

				for (int i = 0; i < recvMsgPacket.ResultBuyLimitsLength; i++)
					buyLimitList.Add(recvMsgPacket.ResultBuyLimits(i).Value);

				Me.CurCharData.AddBuyLimitInfo(buyLimitList);

				/// 펫 + 탈것 + 펫 keep + 탈것 keep
				List<Pet> petList = new List<Pet>();
				List<Pet> rideList = new List<Pet>();
				for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPets(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetList(petList);
				Me.CurCharData.AddRideList(rideList);

				List<PetGachaKeep> petKeepList = new List<PetGachaKeep>();
				List<PetGachaKeep> rideKeepList = new List<PetGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultPetsGachaKeepsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPetsGachaKeeps(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petKeepList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideKeepList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetKeepList(petKeepList);
				Me.CurCharData.AddRideKeepList(rideKeepList);

				/// 강림 가차 
				List<ChangeGachaKeep> changeKeepList = new List<ChangeGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultChangesGachaKeepsLength; i++)
					changeKeepList.Add(recvMsgPacket.ResultChangesGachaKeeps(i).Value);

				Me.CurCharData.AddChangeKeepList(changeKeepList);

				/// 룬 
				List<Rune> runeList = new List<Rune>();

				for (int i = 0; i < recvMsgPacket.ResultRuneItemsLength; i++)
					runeList.Add(recvMsgPacket.ResultRuneItems(i).Value);

				Me.CurCharData.AddRuneList(runeList);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion

		//#region MiniShop
		//public static void BuyMiniShopItem(uint ShopTid, uint BuyCnt, ulong UseItemID, uint UseItemTid, System.Action<ReceiveFBMessage, ResMiniShopBuy> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_MINI_SHOP_BUY);

		//	builder.Clear();

		//	var offset = ReqMiniShopBuy.CreateReqMiniShopBuy(builder, BuyItem.CreateBuyItem(builder, ShopTid, BuyCnt, UseItemID, UseItemTid));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqMiniShopBuy>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResMiniShopBuy recvMsgPacket = recvPacket.GetResMsg<ResMiniShopBuy>();

		//		if (recvMsgPacket.ResultAccountStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.ResultAccountStackItem.Value);

		//		if (recvMsgPacket.ResultBuyLimit != null)
		//			NetData.Instance.AddBuyLimitInfo(NetData.UserID, recvMsgPacket.ResultBuyLimit.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		Dictionary<uint, ulong> buyDic = new Dictionary<uint, ulong>();
		//		buyDic.Add(ShopTid, BuyCnt);
		//	});
		//}
		//#endregion


		#region Upgrade
		public void REQ_UpgradeItem(ulong ItemId, uint ItemTid, byte slot, ulong MatItemId, uint MatTid, uint SelectTid, System.Action<ZWebRecvPacket, ResItemUpgrade> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ZItem goldItem = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

			var offset = ReqItemUpgrade.CreateReqItemUpgrade(mBuilder, ItemId, ItemTid, slot, MatItemId, MatTid, goldItem.item_id, SelectTid);

			var reqPacket = ZWebPacket.Create<ReqItemUpgrade>(this, Code.GS_ITEM_UPGRADE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResItemUpgrade recvMsgPacket = recvPacket.Get<ResItemUpgrade>();

				var findItem = Me.CurCharData.GetItemData(ItemId, NetItemType.TYPE_EQUIP);
				if (findItem != null && findItem.slot_idx != 0)
					Me.CurCharData.UnEquipItem(ItemId);

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultEquipItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Mail
		public void GetMailRefreshTime()
		{
			if (Me.CurCharData != null && Me.CurCharData.ID != 0)
				REQ_GetMailRefreshTime((recvPacket, Msg) => { });
			else
				TimeInvoker.Instance.RequestInvoke(GetMailRefreshTime, 60f);
		}

		/// <summary>여러 곳에서 호출하면 안 됨! (메일 갱신이 필요한 경우 GetMailList를 호출할 것)</summary>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		private void REQ_GetMailRefreshTime(System.Action<ZWebRecvPacket, ResGetMailRefreshTime> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetMailRefreshTime.StartReqGetMailRefreshTime(mBuilder);
			var offset = ReqGetMailRefreshTime.EndReqGetMailRefreshTime(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetMailRefreshTime>(this, Code.GS_GET_MAIL_REFRESH_TIME, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				if (recvPacket.ErrCode == ERROR.HEADER_ERROR_BUT_IGNORE_PACKET)
				{
				}
				else if (recvPacket.ErrCode != ERROR.NO_ERROR)
				{

				}
				else
				{
					ResGetMailRefreshTime recvMsgPacket = recvPacket.Get<ResGetMailRefreshTime>();

					bool bRefreshMailList = false, bRefreshMessageList = false;//, bRefreshExchangeMessageList = false;, bRefreshCashMailList = false;

					if (Me.CurCharData.GetMailRefreshTime() != recvMsgPacket.MailRefreshDt)
						bRefreshMailList = true;
					if (Me.CurCharData.GetMessageRefreshTime() != recvMsgPacket.MessageRefreshDt)
						bRefreshMessageList = true;
					//if (Me.CurCharData.GetExchangeMessageRefreshTime() != recvMsgPacket.ExchangeMessageRefreshDt)
					//	bRefreshExchangeMessageList = true;
					//if (Me.CurCharData.GetCashMailRefreshTime() != recvMsgPacket.CashMailRefreshDt)
					//    bRefreshCashMailList = true;

					Me.CurCharData.SetMailRefreshTime(recvMsgPacket.MailRefreshDt);
					Me.CurCharData.SetMessageRefreshTime(recvMsgPacket.MessageRefreshDt);
					//Me.CurCharData.SetExchangeMessageRefreshTime(recvMsgPacket.ExchangeMessageRefreshDt);
					//Me.CurCharData.SetCashMailRefreshTime(recvMsgPacket.CashMailRefreshDt);

					TimeInvoker.Instance.RequestInvoke(GetMailRefreshTime, 60f);

					if (bRefreshMailList || bRefreshMessageList)
						if (UIManager.Instance.Find(out UIFrameMailbox _mailBox))
							_mailBox.RefreshMailList(true);

					//if (bRefreshMessageList)
					//    REQ_GetMessageList(null);
					//if (bRefreshExchangeMessageList)
					//	REQ_GetExchangeMessageList(null);
					//if (bRefreshCashMailList)
					//    GetCashMailList(null);

					if (UIManager.Instance.Find(out UISubHUDMenu _menu))
						_menu.ActiveRedDot(E_HUDMenu.Mailbox, Me.CurCharData.MailList.Count > 0 || Me.CurCharData.GetNotReadMessage());
					
					onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
				}
			}, _onError);
		}

		public void REQ_GetMailList(System.Action<ZWebRecvPacket, ResGetMailList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetMailList.StartReqGetMailList(mBuilder);
			var offset = ReqGetMailList.EndReqGetMailList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetMailList>(this, Code.GS_GET_MAIL_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetMailList reqGetMailList = recvPacket.Get<ResGetMailList>();

				Me.CurCharData.ClearMailList();

				List<MailInfo> mailInfoList = new List<MailInfo>();
				for (int i = 0; i < reqGetMailList.MailInfoListLength; i++)
					mailInfoList.Add(reqGetMailList.MailInfoList(i).Value);

				List<MailInfo> accountmailInfoList = new List<MailInfo>();
				for (int i = 0; i < reqGetMailList.AccountMailInfoListLength; i++)
					accountmailInfoList.Add(reqGetMailList.AccountMailInfoList(i).Value);

				Me.CurCharData.AddMailList(mailInfoList, accountmailInfoList);
				Me.CurCharData.SetMailRefreshTime(reqGetMailList.MailRefreshDt);

				if (UIManager.Instance.Find(out UISubHUDMenu _menu))
					_menu.ActiveRedDot(E_HUDMenu.Mailbox, Me.CurCharData.MailList.Count > 0 || Me.CurCharData.GetNotReadMessage());

				onRecvPacket?.Invoke(recvPacket, reqGetMailList);
			}, _onError);
		}

		public void REQ_TakeMailItem(GameDB.E_MailReceiver type, ulong MailIdx, uint ItemTid, ulong ItemCnt, System.Action<ZWebRecvPacket, ResTakeMailItem> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var mails = new Offset<TakeMailInfo>[1];
			mails[0] = TakeMailInfo.CreateTakeMailInfo(mBuilder, (uint)type, MailIdx, ItemTid, (uint)ItemCnt);

			var offset = ReqTakeMailItem.CreateReqTakeMailItem(mBuilder, ZGameManager.Instance.GetMarketType(), (uint)type, ReqTakeMailItem.CreateTakeMailInfosVector(mBuilder, mails));

			var reqPacket = ZWebPacket.Create<ReqTakeMailItem>(this, Code.GS_TAKE_MAIL_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTakeMailItem recvMsgPacket = recvPacket.Get<ResTakeMailItem>();

                Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinGainCnt);
				if (recvMsgPacket.RemainItems != null)
					Me.CurCharData.AddRemainItems(recvMsgPacket.RemainItems.Value);

				List<TakeMailInfo> removeMails = new List<TakeMailInfo>();
				for (int i = 0; i < recvMsgPacket.TakeMailInfosLength; i++)
					removeMails.Add(recvMsgPacket.TakeMailInfos(i).Value);

				Me.CurCharData.RemoveMailList(removeMails);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

				if (UIManager.Instance.Find(out UIFrameInventory _inventory)) _inventory.ScrollAdapter.SetData();

			}, _onError);
		}

		public void REQ_TakeMailItems(GameDB.E_MailReceiver type, List<MailData> maildatas, System.Action<ZWebRecvPacket, ResTakeMailItem> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var mails = new Offset<TakeMailInfo>[maildatas.Count];

			for (int i = 0; i < maildatas.Count; i++)
				mails[i] = TakeMailInfo.CreateTakeMailInfo(mBuilder, (uint)type, maildatas[i].MailIdx, maildatas[i].ItemTid, maildatas[i].Cnt);

			var offset = ReqTakeMailItem.CreateReqTakeMailItem(mBuilder, ZGameManager.Instance.GetMarketType(), (uint)type, ReqTakeMailItem.CreateTakeMailInfosVector(mBuilder, mails));

			var reqPacket = ZWebPacket.Create<ReqTakeMailItem>(this, Code.GS_TAKE_MAIL_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTakeMailItem recvMsgPacket = recvPacket.Get<ResTakeMailItem>();

				if (recvMsgPacket.RemainItems != null)
					Me.CurCharData.AddRemainItems(recvMsgPacket.RemainItems.Value);

				List<TakeMailInfo> removeMails = new List<TakeMailInfo>();
				for (int i = 0; i < recvMsgPacket.TakeMailInfosLength; i++)
					removeMails.Add(recvMsgPacket.TakeMailInfos(i).Value);

				Me.CurCharData.RemoveMailList(removeMails);

				if (UIManager.Instance.Find(out UIFrameInventory _inventory)) _inventory.ScrollAdapter.SetData();

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_GetMessageList(System.Action<ZWebRecvPacket, ResGetMessageList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetMessageList.StartReqGetMessageList(mBuilder);
			var offset = ReqGetMessageList.EndReqGetMessageList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetMessageList>(this, Code.GS_GET_MESSAGE_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetMessageList recvMsgPacket = recvPacket.Get<ResGetMessageList>();

				Me.CurCharData.ClearMessageList();

				List<MessageInfo> messageinfos = new List<MessageInfo>();

				for (int i = 0; i < recvMsgPacket.MessageInfoListLength; i++)
					messageinfos.Add(recvMsgPacket.MessageInfoList(i).Value);

				Me.CurCharData.AddMessageList(messageinfos);
				Me.CurCharData.SetMessageRefreshTime(recvMsgPacket.MessageRefreshDt);

				if (UIManager.Instance.Find(out UISubHUDMenu _menu))
					_menu.ActiveRedDot(E_HUDMenu.Mailbox, Me.CurCharData.MailList.Count > 0 || Me.CurCharData.GetNotReadMessage());

				//AlramUI.CheckAlram(Alram.NEW_MESSAGE);
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_ReadMessage(ulong messageIdx, System.Action<ZWebRecvPacket, ResReadMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqReadMessage.CreateReqReadMessage(mBuilder, messageIdx);

			var reqPacket = ZWebPacket.Create<ReqReadMessage>(this, Code.GS_READ_MESSAGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResReadMessage recvMsgPacket = recvPacket.Get<ResReadMessage>();
				Me.CurCharData.ReadMessage(recvMsgPacket.ReadMessageIdx);

				//AlramUI.CheckAlram(Alram.NEW_MESSAGE);
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_DeleteMessage(ulong messageIdx, System.Action<ZWebRecvPacket, ResDelMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			List<ulong> datas = new List<ulong>();

			datas.Add(messageIdx);

			var offset = ReqDelMessage.CreateReqDelMessage(mBuilder, ReqDelMessage.CreateDelMessageIdxVector(mBuilder, datas.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqDelMessage>(this, Code.GS_DEL_MESSAGE, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResDelMessage recvMsgPacket = recvPacket.Get<ResDelMessage>();

				List<ulong> delmessageidxs = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.DelMessageIdxLength; i++)
					delmessageidxs.Add(recvMsgPacket.DelMessageIdx(i));

				Me.CurCharData.RemoveMessageList(delmessageidxs);

				//AlramUI.CheckAlram(Alram.NEW_MESSAGE);
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_DeleteMessages(List<MessageData> messages, System.Action<ZWebRecvPacket, ResDelMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			List<ulong> datas = new List<ulong>();

			for (int i = 0; i < messages.Count; i++)
				datas.Add(messages[i].MessageIdx);

			var offset = ReqDelMessage.CreateReqDelMessage(mBuilder, ReqDelMessage.CreateDelMessageIdxVector(mBuilder, datas.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqDelMessage>(this, Code.GS_DEL_MESSAGE, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResDelMessage recvMsgPacket = recvPacket.Get<ResDelMessage>();

				List<ulong> delmessageidxs = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.DelMessageIdxLength; i++)
					delmessageidxs.Add(recvMsgPacket.DelMessageIdx(i));

				Me.CurCharData.RemoveMessageList(delmessageidxs);

				//AlramUI.CheckAlram(Alram.NEW_MESSAGE);
				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_SendMailMessage(ushort Type, string Nick, string Title, string Message, System.Action<ZWebRecvPacket, ResSendMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqSendMessage.CreateReqSendMessage(mBuilder, Type, mBuilder.CreateString(Nick), mBuilder.CreateString(Title), mBuilder.CreateString(Message));

			var reqPacket = ZWebPacket.Create<ReqSendMessage>(this, Code.GS_SEND_MESSAGE, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSendMessage recvMsgPacket = recvPacket.Get<ResSendMessage>();

				Me.CurUserData.SetNormalMsgSendCnt(recvMsgPacket.NormalMsgSendCnt);
				Me.CurUserData.SetGuildMsgSendCnt(recvMsgPacket.GuildMsgSendCnt);

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i));

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region CashMail
		public void REQ_GetCashMailList(
			Action<ZWebRecvPacket, ResGetCashMailList> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			ReqGetCashMailList.StartReqGetCashMailList(mBuilder);
			var offset = ReqGetCashMailList.EndReqGetCashMailList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetCashMailList>(this, Code.GS_GET_CASH_MAIL_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResGetCashMailList>();

				Me.CurCharData.ClearCashMailList();

				List<CashMailInfo> mailInfoList = new List<CashMailInfo>();
				for (int i = 0; i < recvMsgPacket.MailInfoListLength; i++)
					mailInfoList.Add(recvMsgPacket.MailInfoList(i).Value);

				Me.CurCharData.AddCashMailList(mailInfoList);

				Me.CurCharData.SetCashMailRefreshDt(recvMsgPacket.MailRefreshDt);

				//AlramUI.CheckAlram(Alram.SPECIAL_SHOP_MAIL);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_TakeCashMailItems(List<CashMailData> mailItemList, Action<ZWebRecvPacket, ResTakeCashMailItem> _onReceive, PacketErrorCBDelegate _onError = null)
		{
			mBuilder.Clear();
			Offset<TakeMailInfo>[] vectors = new Offset<TakeMailInfo>[mailItemList.Count];
			for (int i = 0; i < vectors.Length; ++i)
			{
				vectors[i] = TakeMailInfo.CreateTakeMailInfo(mBuilder, 0, mailItemList[i].MailIdx, mailItemList[i].ShopTid, 0);
			}

			var offset = ReqTakeCashMailItem.CreateReqTakeCashMailItem(mBuilder, ReqTakeCashMailItem.CreateTakeMailInfosVector(mBuilder, vectors), ZGameManager.Instance.GetMarketType());

			var reqPacket = ZWebPacket.Create<ReqTakeCashMailItem>(this, Code.GS_TAKE_CASH_MAIL_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvMsgPacket = recvPacket.Get<ResTakeCashMailItem>();

                Me.CurUserData.SetCash(recvMsgPacket.ResultCashCoinBalance);

                if (recvMsgPacket.RemainItems != null)
					AddRemainItems(recvMsgPacket.RemainItems.Value);

				List<ulong> removeMailIdxs = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.TakeMailIdxsLength; i++)
					removeMailIdxs.Add(recvMsgPacket.TakeMailIdxs(i));

				Me.CurCharData.RemoveCashMailList(removeMailIdxs);

				//TimeInvoker.Instance.RequestInvoke(GetMailRefreshTime, 1.5f);

				REQ_GetMailList(null);

				_onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		#region Change

		public void REQ_EquipChange(ulong changeId, uint changeTid, ulong useItemId, System.Action<ZWebRecvPacket, ResChangeEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeEquip.CreateReqChangeEquip(mBuilder, changeId, changeTid, useItemId);
			var reqPacket = ZWebPacket.Create<ReqChangeEquip>(this, Code.GS_CHANGE_EQUIP, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeEquip resChange = recvPacket.Get<ResChangeEquip>();

				ZLog.Log(ZLogChannel.Pet, $"ChangeTid :{ resChange.ChangeTid}");

				List<WebNet.ItemStack> listItem = new List<WebNet.ItemStack>();

				for (int i = 0; i < resChange.ResultStackItemsLength; i++)
					listItem.Add(resChange.ResultStackItems(i).Value);

				Me.CurCharData.AddItemList(listItem);
				Me.CurCharData.UpdateMainChange(resChange.ChangeTid, resChange.ChangeExpireDt);

				Me.CurCharData.SetEquipPetChangeRide(changeTid, OptionEquipType.TYPE_CHANGE);

				REQ_SetCharacterCurrentPreset(null, null);

				if (UIManager.Instance.Find(out UIFrameInventory _inventory))
					if (_inventory.Show)
						_inventory.ScrollAdapter.RefreshData();

				onRecvPacket?.Invoke(recvPacket, resChange);
			}, _onError);
		}

		//public static void GetChangeList(System.Action<ReceiveFBMessage, ResGetChangeList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_CHANGE_LIST);

		//	builder.Clear();

		//	ReqGetChangeList.StartReqGetChangeList(builder);
		//	var offset = ReqGetChangeList.EndReqGetChangeList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetChangeList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetChangeList recvMsgPacket = recvPacket.GetResMsg<ResGetChangeList>();

		//		NetData.Instance.ClearChangeList(NetData.UserID, NetData.CharID);

		//		List<Change> changeList = new List<Change>();
		//		for (int i = 0; i < recvMsgPacket.ChangesLength; i++)
		//			changeList.Add(recvMsgPacket.Changes(i).Value);
		//		NetData.Instance.AddChangeList(NetData.UserID, NetData.CharID, changeList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}


		public void REQ_UnEquipChange(System.Action<ZWebRecvPacket, ResChangeUnequip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqChangeUnequip.StartReqChangeUnequip(mBuilder);
			var offset = ReqChangeUnequip.EndReqChangeUnequip(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqChangeUnequip>(this, Code.GS_CHANGE_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeUnequip recvMsgPacket = recvPacket.Get<ResChangeUnequip>();

				Me.CurCharData.UpdateMainChange(0, 0);

				Me.CurCharData.SetEquipPetChangeRide(0, OptionEquipType.TYPE_CHANGE);
				REQ_SetCharacterCurrentPreset(null, null);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		//public static void EquipAbilityChange(ulong changeId, uint changeTid, System.Action<ReceiveFBMessage, ResChangeAbilEquip> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHANGE_ABIL_EQUIP);

		//	builder.Clear();

		//	var offset = ReqChangeAbilEquip.CreateReqChangeAbilEquip(builder, changeId, changeTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChangeAbilEquip>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChangeAbilEquip recvMsgPacket = recvPacket.GetResMsg<ResChangeAbilEquip>();

		//		NetData.Instance.UpdateAbilChange(NetData.UserID, NetData.CharID, recvMsgPacket.ChangeTid);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void UnEquipAbilityChange(System.Action<ReceiveFBMessage, ResChangeAbilUnEquip> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHANGE_ABIL_UNEQUIP);

		//	builder.Clear();

		//	ReqChangeAbilUnEquip.StartReqChangeAbilUnEquip(builder);
		//	var offset = ReqChangeAbilUnEquip.EndReqChangeAbilUnEquip(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChangeAbilUnEquip>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChangeAbilUnEquip recvMsgPacket = recvPacket.GetResMsg<ResChangeAbilUnEquip>();

		//		NetData.Instance.UpdateAbilChange(NetData.UserID, NetData.CharID, 0);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_ComposeChange(uint ComposeTid, uint Cnt, List<C_PetChangeData> datas, System.Action<ZWebRecvPacket, ResChangeCompose> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ZItem goldItem = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

			var changes = new Offset<ChangeComposeMaterial>[datas.Count];
			for (int i = 0; i < datas.Count; i++)
				changes[i] = ChangeComposeMaterial.CreateChangeComposeMaterial(mBuilder, datas[i].Id, datas[i].Tid, (uint)datas[i].ViewCount);

			var offset = ReqChangeCompose.CreateReqChangeCompose(mBuilder, ComposeTid, Cnt, goldItem.item_id, ReqChangeCompose.CreateChangeMaterialsVector(mBuilder, changes));
			var reqPacket = ZWebPacket.Create<ReqChangeCompose>(this, Code.GS_CHANGE_COMPOSE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeCompose recvMsgPacket = recvPacket.Get<ResChangeCompose>();

				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);

				List<Change> changeList = new List<Change>();
				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				List<ChangeGachaKeep> changeKeepList = new List<ChangeGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultChangesGachaKeepsLength; i++)
					changeKeepList.Add(recvMsgPacket.ResultChangesGachaKeeps(i).Value);

				Me.CurCharData.AddChangeKeepList(changeKeepList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		//public static void EnchantChange(ulong ChangeID, uint ChangeTid, System.Action<ReceiveFBMessage, ResChangeEnchant> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHANGE_ENCHANT);

		//	builder.Clear();

		//	var offset = ReqChangeEnchant.CreateReqChangeEnchant(builder, ChangeID, ChangeTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChangeEnchant>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChangeEnchant recvMsgPacket = recvPacket.GetResMsg<ResChangeEnchant>();

		//		if (recvMsgPacket.ResultChange != null)
		//			NetData.Instance.AddChange(NetData.UserID, NetData.CharID, recvMsgPacket.ResultChange.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		////변신 추출 => 아이템화
		//public static void ExtractionChange(ulong ChangeID, uint ChangeTid, System.Action<ReceiveFBMessage, ResChangeExtraction> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHANGE_EXTRACTION);

		//	builder.Clear();

		//	var offset = ReqChangeExtraction.CreateReqChangeExtraction(builder, ChangeID, ChangeTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChangeExtraction>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChangeExtraction recvMsgPacket = recvPacket.GetResMsg<ResChangeExtraction>();

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		#endregion

		//#region ChangeCollect
		//public static void GetChangeCollectionList(System.Action<ReceiveFBMessage, ResGetChangeCollectList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_CHANGE_COLLECT_LIST);

		//	builder.Clear();

		//	ReqGetChangeCollectList.StartReqGetChangeCollectList(builder);
		//	var offset = ReqGetChangeCollectList.EndReqGetChangeCollectList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetChangeCollectList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetChangeCollectList recvMsgPacket = recvPacket.GetResMsg<ResGetChangeCollectList>();

		//		NetData.Instance.ClearChangeCollectList(NetData.UserID, NetData.CharID);

		//		List<Collect> collectList = new List<Collect>();

		//		for (int i = 0; i < recvMsgPacket.ChangeCollectsLength; i++)
		//			collectList.Add(recvMsgPacket.ChangeCollects(i).Value);

		//		NetData.Instance.AddChangeCollectList(NetData.UserID, NetData.CharID, collectList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_RegistChangeCollection(IList<SendCollectData> sendCollectList, ulong matItemId, uint matItemTid, System.Action<ZWebRecvPacket, ResChangeCollectMake> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var collects = new Offset<CollectMake>[sendCollectList.Count];
			for (int i = 0; i < sendCollectList.Count; i++)
			{
				collects[i] = CollectMake.CreateCollectMake(mBuilder, sendCollectList[i].CollectTid, sendCollectList[i].Slot);
			}

			var offset = ReqChangeCollectMake.CreateReqChangeCollectMake(mBuilder, ReqChangeCollectMake.CreateChangeCollectMakesVector(mBuilder, collects), matItemId, matItemTid);
			var reqPacket = ZWebPacket.Create<ReqChangeCollectMake>(this, Code.GS_CHANGE_COLLECT_MAKE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeCollectMake recvMsgPacket = recvPacket.Get<ResChangeCollectMake>();

				for (int i = 0; i < recvMsgPacket.ChangeCollectMakesLength; i++)
					Me.CurCharData.UpdateChangeCollect(recvMsgPacket.ChangeCollectMakes(i).Value.CollectTid, recvMsgPacket.ChangeCollectMakes(i).Value.SlotIdx, recvMsgPacket.ChangeTid);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		//#endregion

		#region ChangeQuest
		///// <summary> 강링 파견 리셋 </summary>
		///origin : ChangeQuestDailyReset
		public void REQ_ResetChangeDailyQuest(System.Action<ZWebRecvPacket, ResChangeQuestDailyReset> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqChangeQuestDailyReset.StartReqChangeQuestDailyReset(mBuilder);
			var offset = ReqChangeQuestDailyReset.EndReqChangeQuestDailyReset(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqChangeQuestDailyReset>(this, Code.GS_CHANGE_QUEST_DAILY_RESET, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeQuestDailyReset recvMsgPacket = recvPacket.Get<ResChangeQuestDailyReset>();

				Me.CurUserData.SetChangeQuestIssuedDt(recvMsgPacket.ChangeQuestIssuedDt);

				List<ChangeQuest> questList = new List<ChangeQuest>();

				for (int i = 0; i < recvMsgPacket.ChangeQuestsLength; i++)
					questList.Add(recvMsgPacket.ChangeQuests(i).Value);

				Me.CurCharData.AddChangeQuestList(questList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 강링 파견 등록 변경 </summary>
		public void REQ_ChangeQuestRegistChange(uint questTid, List<uint> changeTids, System.Action<ZWebRecvPacket, ResChangeQuestRegistChange> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeQuestRegistChange.CreateReqChangeQuestRegistChange(mBuilder, ReqChangeQuestRegistChange.CreateChangeTidsVector(mBuilder, changeTids.ToArray()), questTid);

			var reqPacket = ZWebPacket.Create<ReqChangeQuestRegistChange>(this, Code.GS_CHANGE_QUEST_REGIST_CHANGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeQuestRegistChange recvMsgPacket = recvPacket.Get<ResChangeQuestRegistChange>();

				if (recvMsgPacket.ResultQuest.HasValue)
					Me.CurCharData.AddChangeQuest(recvMsgPacket.ResultQuest.Value);

				List<Change> changeList = new List<Change>();

				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary> 강링 파견 보상 받기</summary>
		public void REQ_ChangeQuestReward(uint questTid, List<uint> changeTids, System.Action<ZWebRecvPacket, ResChangeQuestReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqChangeQuestReward.CreateReqChangeQuestReward(mBuilder, ReqChangeQuestReward.CreateChangeTidsVector(mBuilder, changeTids.ToArray()), questTid);
			var reqPacket = ZWebPacket.Create<ReqChangeQuestReward>(this, Code.GS_CHANGE_QUEST_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeQuestReward recvMsgPacket = recvPacket.Get<ResChangeQuestReward>();

				Me.CurUserData.SetChangeQuestLevel(recvMsgPacket.ChangeQuestLv);
				Me.CurUserData.SetChangeQuestExp(recvMsgPacket.ChangeQuestExp);

				if (recvMsgPacket.ResultQuest.HasValue)
					Me.CurCharData.AddChangeQuest(recvMsgPacket.ResultQuest.Value);

				AddRemainItems(recvMsgPacket.RemainItems.Value);

				if (recvMsgPacket.GetItems != null)
				{
					//획득 연출 -> 개별처리
					//if (recvMsgPacket.GetItems.HasValue)
					//	UIManager.ShowGainEff(recvMsgPacket.GetItems.Value);
				}

				if (recvMsgPacket.RemainItems != null)
				{
					if (recvMsgPacket.RemainItems.HasValue)
						AddRemainItems(recvMsgPacket.RemainItems.Value);
				}

				//갱신되어야함.
				List<Change> changeList = new List<Change>();

				for (int i = 0; i < recvMsgPacket.ResultChangesLength; i++)
					changeList.Add(recvMsgPacket.ResultChanges(i).Value);

				Me.CurCharData.AddChangeList(changeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 강림 퀘스트 파견대 레벨업 </summary>

		public void REQ_ChangeQuestLevelUp(System.Action<ZWebRecvPacket, ResChangeQuestLevelUp> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqChangeQuestLevelUp.StartReqChangeQuestLevelUp(mBuilder);
			var offset = ReqChangeQuestLevelUp.EndReqChangeQuestLevelUp(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqChangeQuestLevelUp>(this, Code.GS_CHANGE_QUEST_LEVEL_UP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResChangeQuestLevelUp recvMsgPacket = recvPacket.Get<ResChangeQuestLevelUp>();

				Me.CurUserData.SetChangeQuestLevel(recvMsgPacket.ChangeQuestLv);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion

		//#region Pet
		//public static void GetPetList(System.Action<ReceiveFBMessage, ResGetPetList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_PET_LIST);

		//	builder.Clear();

		//	ReqGetPetList.StartReqGetPetList(builder);
		//	var offset = ReqGetPetList.EndReqGetPetList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetPetList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetPetList recvMsgPacket = recvPacket.GetResMsg<ResGetPetList>();

		//		NetData.Instance.ClearPetList(NetData.UserID, NetData.CharID);

		//		List<Pet> petList = new List<Pet>();
		//		for (int i = 0; i < recvMsgPacket.PetsLength; i++)
		//			petList.Add(recvMsgPacket.Pets(i).Value);
		//		NetData.Instance.AddPetList(NetData.UserID, NetData.CharID, petList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void LeaderPetSet(ulong petId, uint petTid, System.Action<ReceiveFBMessage, ResPetReaderSet> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_SET_READER);
		//	builder.Clear();

		//	var offset = ReqPetReaderSet.CreateReqPetReaderSet(builder, petId, petTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetReaderSet>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetReaderSet recvMsgPacket = recvPacket.GetResMsg<ResPetReaderSet>();
		//		//메인 펫 데이터만 갱신! 등록된 이벤트 호출 안함.
		//		NetData.Instance.UpdatePet(NetData.UserID, NetData.CharID, recvMsgPacket.MainPetTid, NetData.CurrentCharacter.PetExpireDt, false);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}		




		//public static void EquipPet(ulong petId, uint petTid, byte petType, System.Action<ReceiveFBMessage, ResPetEquip> onRecvPacket)
		//{
		//	EquipPets(new List<ulong>() { petId }, new List<uint>() { petTid }, new List<byte>() { petType }, onRecvPacket);
		//}

		//public static void EquipPets(uint attackPetTid, uint defensePetTid, uint utilPetTid, System.Action<ReceiveFBMessage, ResPetEquip> onRecvPacket)
		//{
		//	List<uint> petTids = new List<uint>();
		//	List<ulong> petIds = new List<ulong>();
		//	List<byte> petTypes = new List<byte>();

		//	if (0 < attackPetTid)
		//	{
		//		petTids.Add(attackPetTid);
		//		petIds.Add(NetData.Instance.GetPet(NetData.UserID, NetData.CharID, attackPetTid)?.PetId ?? 0);
		//		petTypes.Add((byte)GameDB.E_PetType_02.Attack);
		//	}

		//	if (0 < defensePetTid)
		//	{
		//		petTids.Add(defensePetTid);
		//		petIds.Add(NetData.Instance.GetPet(NetData.UserID, NetData.CharID, defensePetTid)?.PetId ?? 0);
		//		petTypes.Add((byte)GameDB.E_PetType_02.Defense);
		//	}

		//	if (0 < utilPetTid)
		//	{
		//		petTids.Add(utilPetTid);
		//		petIds.Add(NetData.Instance.GetPet(NetData.UserID, NetData.CharID, utilPetTid)?.PetId ?? 0);
		//		petTypes.Add((byte)GameDB.E_PetType_02.Util);
		//	}

		//	EquipPets(petIds, petTids, petTypes, onRecvPacket);
		//}

		////public static void EquipPets(List<ulong> petIds, System.Action<ReceiveFBMessage, ResPetEquip> onRecvPacket)
		////{
		////    List<uint> petTids = new List<uint>();        
		////    List<byte> petTypes = new List<byte>();

		////    foreach(ulong id in petIds)
		////    {
		////        PetData data =  NetData.Instance.GetPet(NetData.UserID, NetData.CharID, id);

		////        if (null == data)
		////            continue;

		////        var table = DBPet.GetPetData(data.PetTid);

		////        petTids.Add(data.PetTid);
		////        petTypes.Add((byte)table.PetType_02);
		////    }

		////    EquipPets(petIds, petTids, petTypes, onRecvPacket);
		////}

		//public static void EquipPets(List<ulong> petIds, List<uint> petTids, List<byte> petTypes, System.Action<ReceiveFBMessage, ResPetEquip> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_EQUIP);

		//	builder.Clear();

		//	var offset = ReqPetEquip.CreateReqPetEquip(builder, ReqPetEquip.CreatePetIdListVector(builder, petIds.ToArray()), ReqPetEquip.CreateSlotIdxListVector(builder, petTypes.ToArray()));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetEquip>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetEquip recvMsgPacket = recvPacket.GetResMsg<ResPetEquip>();

		//		//펫 장착
		//		NetData.Instance.UpdateEquipPets(NetData.UserID, NetData.CharID, petTids, petTypes);
		//		//메인 펫 데이터만 갱신! 등록된 이벤트 호출 안함.
		//		NetData.Instance.UpdatePet(NetData.UserID, NetData.CharID, recvMsgPacket.MainPetTid, NetData.CurrentCharacter.PetExpireDt, false);

		//		NetData.Instance.RemoveEquipPetCurrentSet(NetData.UserID, NetData.CharID);

		//		if (NetData.Instance.AddEquipPetCurrentSet(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.PetEquip1, NetData.CurrentCharacter.PetEquip2, NetData.CurrentCharacter.PetEquip3))
		//		{
		//			switch (NetData.CurrentCharacter.SelectEquipSetNo)
		//			{
		//				case 1:
		//					ZNetGame.SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET1, NetData.Instance.GetEquipSetValue(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.SelectEquipSetNo), null);
		//					break;
		//				case 2:
		//					ZNetGame.SetCharacterOption(WebNet.E_CharacterOptionKey.EQUIP_SET2, NetData.Instance.GetEquipSetValue(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.SelectEquipSetNo), null);
		//					break;
		//			}
		//		}

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		//AnalyticsManager.instance.Event(AnalyticsManager.EVENT_EQUIP_PET, new Dictionary<string, object>()
		//		//{
		//		//    { "UserID", NetData.UserID },
		//		//    { "CharID", NetData.CharID },
		//		//    { "PetTid",petTid},
		//		//});
		//	});
		//}


		public void REQ_UnEquipPet(System.Action<ZWebRecvPacket, ResPetUnequip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqPetUnequip.StartReqPetUnequip(mBuilder);
			var offset = ReqPetUnequip.EndReqPetUnequip(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqPetUnequip>(this, Code.GS_PET_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetUnequip recvMsgPacket = recvPacket.Get<ResPetUnequip>();

				///Me.CurCharData.RemoveEquipPet(Me.CurCharData.MainPet);

				//장착중인 펫을 해제한다.
				Me.CurCharData.UpdateMainPet(0, 0);

				Me.CurCharData.SetEquipPetChangeRide(0, OptionEquipType.TYPE_PET);
				REQ_SetCharacterCurrentPreset(null, null);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		//public static void PetSummonField(ulong useItemId, System.Action<ReceiveFBMessage, ResPetSummonField> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_SUMMON_FIELD);

		//	builder.Clear();

		//	var offset = ReqPetSummonField.CreateReqPetSummonField(builder, useItemId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetSummonField>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetSummonField recvMsgPacket = recvPacket.GetResMsg<ResPetSummonField>();

		//		List<ItemStack> invenStackList = new List<ItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, null, invenStackList, null);

		//		NetData.Instance.UpdatePet(NetData.UserID, NetData.CharID, recvMsgPacket.PetTid, recvMsgPacket.PetExpireDt);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void PetSummonRelease(System.Action<ReceiveFBMessage, ResPetSummonRelease> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_SUMMON_RELEASE);

		//	builder.Clear();

		//	ReqPetSummonRelease.StartReqPetSummonRelease(builder);
		//	var offset = ReqPetSummonRelease.EndReqPetSummonRelease(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetSummonRelease>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetSummonRelease recvMsgPacket = recvPacket.GetResMsg<ResPetSummonRelease>();

		//		NetData.Instance.UpdatePet(NetData.UserID, NetData.CharID, NetData.CurrentCharacter.MainPet, recvMsgPacket.PetExpireDt);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_ComposePet(uint ComposeTid, uint Cnt, List<C_PetChangeData> datas, System.Action<ZWebRecvPacket, ResPetCompose> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ZItem goldItem = Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK);

			var pets = new Offset<PetComposeMaterial>[datas.Count];
			for (int i = 0; i < datas.Count; i++)
				pets[i] = PetComposeMaterial.CreatePetComposeMaterial(mBuilder, datas[i].Id, datas[i].Tid, (uint)datas[i].ViewCount);

			var offset = ReqPetCompose.CreateReqPetCompose(mBuilder, ComposeTid, Cnt, goldItem.item_id, ReqPetCompose.CreatePetMaterialsVector(mBuilder, pets));
			var reqPacket = ZWebPacket.Create<ReqPetCompose>(this, Code.GS_PET_COMPOSE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetCompose recvMsgPacket = recvPacket.Get<ResPetCompose>();

				List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

				Me.CurCharData.AddItemList(invenAccountStackList);

				List<Pet> petList = new List<Pet>();
				List<Pet> rideList = new List<Pet>();
				for (int i = 0; i < recvMsgPacket.ResultPetsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPets(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetList(petList);
				Me.CurCharData.AddRideList(rideList);

				List<PetGachaKeep> petKeepList = new List<PetGachaKeep>();
				List<PetGachaKeep> rideKeepList = new List<PetGachaKeep>();

				for (int i = 0; i < recvMsgPacket.ResultPetsGachaKeepsLength; i++)
				{
					var resultData = recvMsgPacket.ResultPetsGachaKeeps(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petKeepList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideKeepList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetKeepList(petKeepList);
				Me.CurCharData.AddRideKeepList(rideKeepList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, null);
		}

		//public static void EnchantPet(ulong PetId, uint PetTid, System.Action<ReceiveFBMessage, ResPetEnchant> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_ENCHANT);

		//	builder.Clear();

		//	var offset = ReqPetEnchant.CreateReqPetEnchant(builder, PetId, PetTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetEnchant>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetEnchant recvMsgPacket = recvPacket.GetResMsg<ResPetEnchant>();

		//		if (recvMsgPacket.ResultPet != null)
		//			NetData.Instance.AddPet(NetData.UserID, NetData.CharID, recvMsgPacket.ResultPet.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_GrowthPet(ulong PetId, uint PetTid, System.Action<ZWebRecvPacket, ResPetGrowth> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetGrowth.CreateReqPetGrowth(mBuilder, PetId, PetTid);
			var reqPacket = ZWebPacket.Create<ReqPetGrowth>(this, Code.GS_PET_GROWTH, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetGrowth recvMsgPacket = recvPacket.Get<ResPetGrowth>();

				var table = DBPet.GetPetData(recvMsgPacket.PetInfo.Value.PetTid);
				if (table.PetType == GameDB.E_PetType.Pet)
				{
					Me.CurCharData.AddPetList(recvMsgPacket.PetInfo.Value);
				}
				else if (table.PetType == GameDB.E_PetType.Vehicle)
				{
					Me.CurCharData.AddRideList(recvMsgPacket.PetInfo.Value);
				}


				List<AccountItemStack> accountstackList = new List<AccountItemStack>();
				for (int i = 0; i < recvMsgPacket.RemainAccountStacksLength; i++)
					accountstackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);

				Me.CurCharData.AddItemList(accountstackList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		////펫 추출 => 아이템화
		//public static void ExtractionPet(ulong PetID, uint PetTid, System.Action<ReceiveFBMessage, ResPetExtraction> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_PET_EXTRACTION);

		//	builder.Clear();

		//	var offset = ReqPetExtraction.CreateReqPetExtraction(builder, PetID, PetTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPetExtraction>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPetExtraction recvMsgPacket = recvPacket.GetResMsg<ResPetExtraction>();

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		#region Pet Rune
		/// <summary> 룬 리스트를 얻어온다. </summary>    
		//public static void GetRuneList(System.Action<ReceiveFBMessage, ResRuneList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RUNE_LIST);
		//
		//	builder.Clear();
		//
		//	ReqRuneList.StartReqRuneList(builder);
		//	var offset = ReqRuneList.EndReqRuneList(builder);
		//
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRuneList>(builder);
		//
		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRuneList recvMsgPacket = recvPacket.GetResMsg<ResRuneList>();
		//
		//		NetData.Instance.ClearRuneList(NetData.UserID, NetData.CharID);
		//
		//		List<Rune> runeList = new List<Rune>();
		//		for (int i = 0; i < recvMsgPacket.RuneLength; i++)
		//			runeList.Add(recvMsgPacket.Rune(i).Value);
		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);
		//
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		/// <summary> 룬을 장착한다. </summary>
		public void REQ_RuneEquip(uint petTid, ulong runeId, System.Action<ZWebRecvPacket, ResRuneEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			REQ_RuneEquip(petTid, new List<ulong>() { runeId }, onRecvPacket, _onError);
		}

		/// <summary> 룬을 장착한다. </summary>
		public void REQ_RuneEquip(uint petTid, List<ulong> runes, System.Action<ZWebRecvPacket, ResRuneEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRuneEquip.CreateReqRuneEquip(mBuilder, petTid, ReqRuneEquip.CreateRuneItemIdListVector(mBuilder, runes.ToArray()));
			var reqPacket = ZWebPacket.Create<ReqRuneEquip>(this, Code.GS_RUNE_EQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRuneEquip recvMsgPacket = recvPacket.Get<ResRuneEquip>();

				List<Rune> runeList = new List<Rune>();

				for (int i = 0; i < recvMsgPacket.EquipRuneListLength; i++)
					runeList.Add(recvMsgPacket.EquipRuneList(i).Value);

				Me.CurCharData.AddRuneList(runeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 룬을 삭제 한다. </summary>
		public void REQ_RuneDelete(ulong runeId, System.Action<ZWebRecvPacket, ResRuneDelete> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			REQ_RuneDelete(new List<ulong>() { runeId }, onRecvPacket);
		}

		/// <summary> 룬을 삭제 한다. </summary>
		public void REQ_RuneDelete(List<ulong> runes, System.Action<ZWebRecvPacket, ResRuneDelete> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRuneDelete.CreateReqRuneDelete(mBuilder, ReqRuneDelete.CreateRuneItemIdListVector(mBuilder, runes.ToArray()));
			var reqPacket = ZWebPacket.Create<ReqRuneDelete>(this, Code.GS_RUNE_DELETE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRuneDelete recvMsgPacket = recvPacket.Get<ResRuneDelete>();

				//기존 장착된 룬 제거
				List<ulong> removeList = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.RuneItemIdListLength; i++)
					removeList.Add(recvMsgPacket.RuneItemIdList(i));

				Me.CurCharData.RemoveRuneList(removeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 룬을 장착 해제 한다. </summary>
		public void REQ_RuneUnequip(uint petTid, ulong runeId, System.Action<ZWebRecvPacket, ResRuneUnEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			REQ_RuneUnequip(petTid, new List<ulong>() { runeId }, onRecvPacket);
		}

		/// <summary> 룬을 장착 해제 한다. </summary>
		public void REQ_RuneUnequip(uint petTid, List<ulong> runes, System.Action<ZWebRecvPacket, ResRuneUnEquip> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			if (null == runes || 0 >= runes.Count)
			{
				onRecvPacket?.Invoke(default, default);
				return;
			}

			var offset = ReqRuneUnEquip.CreateReqRuneUnEquip(mBuilder, petTid, ReqRuneUnEquip.CreateRuneItemIdListVector(mBuilder, runes.ToArray()));
			var reqPacket = ZWebPacket.Create<ReqRuneUnEquip>(this, Code.GS_RUNE_UNEQUIP, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRuneUnEquip recvMsgPacket = recvPacket.Get<ResRuneUnEquip>();

				List<AccountItemStack> accountstackList = new List<AccountItemStack>();

				if (recvMsgPacket.RemainAccountStack != null)
					accountstackList.Add(recvMsgPacket.RemainAccountStack.Value);

				Me.CurCharData.AddItemList(accountstackList);

				//룬 갱신
				List<Rune> runeList = new List<Rune>();
				for (int i = 0; i < recvMsgPacket.UnequipRuneListLength; i++)
					runeList.Add(recvMsgPacket.UnequipRuneList(i).Value);

				Me.CurCharData.AddRuneList(runeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}

		/// <summary> 룬을 판매한다. </summary>
		public void REQ_RuneSell(ulong runeId, System.Action<ZWebRecvPacket, ResRuneSell> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			REQ_RuneSell(new List<ulong>() { runeId }, onRecvPacket);
		}

		/// <summary> 룬을 판매한다. </summary>
		public void REQ_RuneSell(List<ulong> runes, System.Action<ZWebRecvPacket, ResRuneSell> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRuneSell.CreateReqRuneSell(mBuilder, ReqRuneSell.CreateRuneItemIdListVector(mBuilder, runes.ToArray()));
			var reqPacket = ZWebPacket.Create<ReqRuneSell>(this, Code.GS_RUNE_SELL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRuneSell recvMsgPacket = recvPacket.Get<ResRuneSell>();

				List<AccountItemStack> accountstackList = new List<AccountItemStack>();

				if (recvMsgPacket.RemainAccountStack != null)
					accountstackList.Add(recvMsgPacket.RemainAccountStack.Value);

				Me.CurCharData.AddItemList(accountstackList);

				List<ulong> removeList = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.DeleteRuneItemIdListLength; i++)
					removeList.Add(recvMsgPacket.DeleteRuneItemIdList(i));
				Me.CurCharData.RemoveRuneList(removeList);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 룬을 강환한다. </summary>
		public void REQ_RuneEnchant(ulong runeId, System.Action<ZWebRecvPacket, ResRuneEnchant> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqRuneEnchant.CreateReqRuneEnchant(mBuilder, runeId);
			var reqPacket = ZWebPacket.Create<ReqRuneEnchant>(this, Code.GS_RUNE_ENCHANT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResRuneEnchant recvMsgPacket = recvPacket.Get<ResRuneEnchant>();

				List<AccountItemStack> accountstackList = new List<AccountItemStack>();

				if (recvMsgPacket.RemainAccountStack != null)
					accountstackList.Add(recvMsgPacket.RemainAccountStack.Value);

				Me.CurCharData.AddItemList(accountstackList);

				if (recvMsgPacket.EnchantRune != null)
					Me.CurCharData.EnchantRune(recvMsgPacket.EnchantRune.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		/// <summary> 룬 드랍 셋팅을 선택 </summary>
		public void REQ_RuneSetOption(ulong _runeSetOption, System.Action onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqSetRuneOption.CreateReqSetRuneOption(mBuilder, _runeSetOption, (ushort)ZGameOption.Instance.Auto_Sell_PetEquipmentGrade, (ushort)ZGameOption.Instance.Auto_Sell_PetEquipmentGradeType);
			var reqPacket = ZWebPacket.Create<ReqSetRuneOption>(this, Code.GS_SET_RUNE_OPTION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSetRuneOption recvMsgPacket = recvPacket.Get<ResSetRuneOption>();

				Me.CurUserData.AddRuneSetOptionType(recvMsgPacket.RuneStageDropBitOption);
				onRecvPacket?.Invoke();
			}, _onError);
		}

		#endregion

		//#region PetCollect
		//public static void GetPetCollectionList(System.Action<ReceiveFBMessage, ResGetPetCollectList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_PET_COLLECT_LIST);

		//	builder.Clear();

		//	ReqGetPetCollectList.StartReqGetPetCollectList(builder);
		//	var offset = ReqGetPetCollectList.EndReqGetPetCollectList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetPetCollectList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetPetCollectList recvMsgPacket = recvPacket.GetResMsg<ResGetPetCollectList>();

		//		NetData.Instance.ClearPetCollectList(NetData.UserID, NetData.CharID);

		//		List<Collect> collectList = new List<Collect>();

		//		for (int i = 0; i < recvMsgPacket.PetCollectsLength; i++)
		//			collectList.Add(recvMsgPacket.PetCollects(i).Value);

		//		NetData.Instance.AddPetCollectList(NetData.UserID, NetData.CharID, collectList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		public void REQ_RegistPetCollection(IList<SendCollectData> sendCollectList, ulong matItemId, uint matItemTid, System.Action<ZWebRecvPacket, ResPetCollectMake> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var collects = new Offset<CollectMake>[sendCollectList.Count];
			for (int i = 0; i < sendCollectList.Count; i++)
			{
				collects[i] = CollectMake.CreateCollectMake(mBuilder, sendCollectList[i].CollectTid, sendCollectList[i].Slot);
			}

			var offset = ReqPetCollectMake.CreateReqPetCollectMake(mBuilder, ReqPetCollectMake.CreatePetCollectMakesVector(mBuilder, collects), matItemId, matItemTid);
			var reqPacket = ZWebPacket.Create<ReqPetCollectMake>(this, Code.GS_PET_COLLECT_MAKE, mBuilder, offset.Value);

			if (DBPetCollect.GetPetRideCollection(matItemTid, out var tab))
			{
				print($"MAKE_____{tab.PetType}");
			}
			else
			{
				print($"MAKE_____{matItemTid}");
			}

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetCollectMake recvMsgPacket = recvPacket.Get<ResPetCollectMake>();

				for (int i = 0; i < recvMsgPacket.PetCollectMakesLength; i++)
				{
					var data = recvMsgPacket.PetCollectMakes(i).Value;

					if (DBPetCollect.GetPetRideCollection(data.CollectTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						Me.CurCharData.UpdatePetCollect(data.CollectTid, data.SlotIdx, recvMsgPacket.PetTid);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						Me.CurCharData.UpdateRideCollect(data.CollectTid, data.SlotIdx, recvMsgPacket.PetTid);
					}
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		//#endregion



		#region PetAdventure
		public void REQ_GetPetAdventureInfo(System.Action<ZWebRecvPacket, ResTakePetAdvInfo> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqTakePetAdvInfo.StartReqTakePetAdvInfo(mBuilder);
			var offset = ReqTakePetAdvInfo.EndReqTakePetAdvInfo(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqTakePetAdvInfo>(this, Code.GS_TAKE_PET_ADV_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResTakePetAdvInfo recvMsgPacket = recvPacket.Get<ResTakePetAdvInfo>();

				List<PetAdv> petInfos = new List<PetAdv>();
				for (int i = 0; i < recvMsgPacket.AdvInfosLength; i++)
					petInfos.Add(recvMsgPacket.AdvInfos(i).Value);

				Me.CurCharData.ClearPetAdventureList();
				Me.CurCharData.AddPetAdventureList(petInfos);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_StartPetAdventure(ulong AdventureId, List<ulong> petList, System.Action<ZWebRecvPacket, ResPetAdvStart> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetAdvStart.CreateReqPetAdvStart(mBuilder, AdventureId, ReqPetAdvStart.CreatePetIdListVectorBlock(mBuilder, petList.ToArray()));
			var reqPacket = ZWebPacket.Create<ReqPetAdvStart>(this, Code.GS_PET_ADV_START, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetAdvStart recvMsgPacket = recvPacket.Get<ResPetAdvStart>();

				List<Pet> petinfoList = new List<Pet>();
				for (int i = 0; i < recvMsgPacket.PetListLength; i++)
				{
					petinfoList.Add(recvMsgPacket.PetList(i).Value);
				}

				Me.CurCharData.AddPetList(petinfoList);

				Me.CurCharData.AddPetAdventure(recvMsgPacket.AdvInfo.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_CancelPetAdventure(ulong AdventureId, System.Action<ZWebRecvPacket, ResPetAdvCancel> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetAdvCancel.CreateReqPetAdvCancel(mBuilder, AdventureId);
			var reqPacket = ZWebPacket.Create<ReqPetAdvCancel>(this, Code.GS_PET_ADV_CANCEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetAdvCancel recvMsgPacket = recvPacket.Get<ResPetAdvCancel>();

				List<Pet> petinfoList = new List<Pet>();
				for (int i = 0; i < recvMsgPacket.PetListLength; i++)
					petinfoList.Add(recvMsgPacket.PetList(i).Value);

				Me.CurCharData.AddPetList(petinfoList);

				Me.CurCharData.AddPetAdventure(recvMsgPacket.AdvInfo);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_RewardPetAdventure(ulong AdventureId, System.Action<ZWebRecvPacket, ResPetAdvReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetAdvReward.CreateReqPetAdvReward(mBuilder, AdventureId);
			var reqPacket = ZWebPacket.Create<ReqPetAdvReward>(this, Code.GS_PET_ADV_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetAdvReward recvMsgPacket = recvPacket.Get<ResPetAdvReward>();

				if (recvMsgPacket.RemainItems.HasValue)
					AddRemainItems(recvMsgPacket.RemainItems.Value);

				List<Pet> petinfoList = new List<Pet>();
				for (int i = 0; i < recvMsgPacket.PetListLength; i++)
					petinfoList.Add(recvMsgPacket.PetList(i).Value);

				Me.CurCharData.AddPetList(petinfoList);

				//탐험 삭제
				Me.CurCharData.AddPetAdventure(recvMsgPacket.AdvInfo);

				//AlramUI.CheckAlram(UIDefine.Alram2.PET_ADVENTURE_COMPLETE);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_ResetPetAdventure(ulong adventureTid, uint matItemTid, System.Action<ZWebRecvPacket, ResPetAdvReset> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPetAdvReset.CreateReqPetAdvReset(mBuilder, adventureTid, matItemTid);
			var reqPacket = ZWebPacket.Create<ReqPetAdvReset>(this, Code.GS_PET_ADV_RESET, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPetAdvReset recvMsgPacket = recvPacket.Get<ResPetAdvReset>();

				if (recvMsgPacket.ResultAccountStackItem.HasValue)
				{
					Me.CurCharData.AddItem(recvMsgPacket.ResultAccountStackItem.Value);
				}

				Me.CurCharData.AddPetAdventure(recvMsgPacket.AdvInfo);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		//#region Block
		public void REQ_GetBlockList(System.Action<ZWebRecvPacket, ResCharBlockList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqCharBlockList.StartReqCharBlockList(mBuilder);
			var offset = ReqCharBlockList.EndReqCharBlockList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqCharBlockList>(this, Code.GS_CHARACTER_BLOCK_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharBlockList recvMsgPacket = recvPacket.Get<ResCharBlockList>();

				Me.CurCharData.ClearBlockCharacterList();

				for (int i = 0; i < recvMsgPacket.BlockListLength; i++)
					Me.CurCharData.AddBlockCharacter(recvMsgPacket.BlockList(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_AddBlockCharacter(string Nick, System.Action<ZWebRecvPacket, ResCharBlockSet> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharBlockSet.CreateReqCharBlockSet(mBuilder, mBuilder.CreateString(Nick));

			var reqPacket = ZWebPacket.Create<ReqCharBlockSet>(this, Code.GS_CHARACTER_BLOCK_SET, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharBlockSet recvMsgPacket = recvPacket.Get<ResCharBlockSet>();

				Me.CurCharData.AddBlockCharacter(recvMsgPacket.BlockChar.Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_RemoveBlockCharacter(ulong CharID, System.Action<ZWebRecvPacket, ResCharBlockDel> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCharBlockDel.CreateReqCharBlockDel(mBuilder, CharID);

			var reqPacket = ZWebPacket.Create<ReqCharBlockDel>(this, Code.GS_CHARACTER_BLOCK_DEL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCharBlockDel recvMsgPacket = recvPacket.Get<ResCharBlockDel>();

				Me.CurCharData.RemoveBlockCharacter(recvMsgPacket.BlockCharId);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		//#endregion

		#region CheckRefresh
		public void REQ_CheckDailyResetEvent()
		{

			REQ_CheckDailyResetEvent((recvPacket, recvMsgPacket) =>
			{
				REQ_ResetChangeDailyQuest(null);

				// 이벤트 목록 갱신
				REQ_EventDataAll(delegate
				{
					// 유료 출석 업데이트
					REQ_RefreshCashAttendEvent(delegate
					{
						// hud 업데이트
						if (UIManager.Instance.Find<UISubHUDEvent>(out var frame))
						{
							if (frame.Show)
							{
								frame.RefreshEventList(false);
							}
						}
					});
				});

				//GetRepeatQuestList(null);
			});
		}

		public void REQ_CheckDailyResetEvent(Action<ZWebRecvPacket, ResCheckDailyResetEvent> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqCheckDailyResetEvent.StartReqCheckDailyResetEvent(mBuilder);
			var offset = ReqCheckDailyResetEvent.EndReqCheckDailyResetEvent(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqCheckDailyResetEvent>(this, Code.GS_CHECK_DAILY_RESET_EVENT, mBuilder, offset.Value);
			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCheckDailyResetEvent recvMsgPacket = recvPacket.Get<ResCheckDailyResetEvent>();

				Me.CurCharData.UpdateCheckDailyTime(recvMsgPacket.DailyResetDt);

				long time = Me.CurCharData.GetRemainCheckDailyNextTime();
				TimeInvoker.Instance.RequestInvoke(REQ_CheckDailyResetEvent, time <= 0 ? 60 : time);

				Me.CurUserData.SetNormalMsgSendCnt(recvMsgPacket.NormalMsgSendCnt);
				Me.CurUserData.SetGuildMsgSendCnt(recvMsgPacket.GuildMsgSendCnt);
				Me.CurUserData.ColosseumRewardCnt = recvMsgPacket.ColosseumRewardCnt;

				//Me.CurCharData.ColosseumScore = recvMsgPacket.ColosseumScore;
				//Me.CurCharData.InstanceDungeonClearCnt = recvMsgPacket.InstanceDungeonClearCnt;

				// TODO :: 요기 작업해야함
				//NetData.Instance.SetScenarioDungeonPlayCnt(NetData.UserID, recvMsgPacket.ScenarioDungeonCnt);

				var guild = UIManager.Instance.Find<UIFrameGuild>();
				if (guild != null)
					guild.NotifyUpdateEvent(UpdateEventType.RequestedDataRefreshed);

				//                  System.Action<System.Action> reloadChar = (callback) =>
				//{
				//	if (recvMsgPacket.IsUpdateCharBundle)
				//		GetCharacterInfos(false, NetData.UserID, NetData.CharID, (x, y) => { callback.Invoke(); });
				//	else
				//		callback.Invoke();
				//};

				//System.Action<System.Action> reloadItems = (callback) =>
				//{

				//	if (recvMsgPacket.IsUpdateItemBundle)
				//		GetCharacterItemInfos(false, NetData.UserID, NetData.CharID, (x, y) => { callback.Invoke(); });
				//	else
				//		callback.Invoke();
				//};

				//System.Action<System.Action> reloadCollections = (callback) =>
				//{

				//	if (recvMsgPacket.IsUpdateCollectBundle)
				//		GetCharacterCollectInfos(false, NetData.UserID, NetData.CharID, (x, y) => { callback.Invoke(); });
				//	else
				//		callback.Invoke();
				//};

				//System.Action<System.Action> reloadRaidEtc = (callback) =>
				//{
				//	if (recvMsgPacket.IsUpdateRaidBundle)
				//		GetRaidEtcInfos(false, NetData.UserID, NetData.CharID, (x, y) => { callback.Invoke(); });
				//	else
				//		callback.Invoke();
				//};

				//reloadChar(() =>
				//{
				//	reloadItems(() =>
				//	{
				//		reloadCollections(() =>
				//		{
				//			reloadRaidEtc(() =>
				//			{

				//			});
				//		});
				//	});
				//});

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

			}, _onError);
		}
		#endregion

		#region Event

		// 이벤트관련 묶어서 요청, 캐릭터 정보 받아올시 사용함
		public void REQ_EventDataAll(Action onRecvAll)
		{
			Me.CurCharData.ServerEventContainer.REQ_GetServerEventList(delegate
			{
				REQ_GetAttendEventInfoList(delegate
				{
					onRecvAll?.Invoke();
				});
			});
		}

		/// <summary>
		/// 로그인시 발생되는 이벤트 체크
		/// </summary>
		public void REQ_CheckLoginEvent(System.Action<ZWebRecvPacket, ResCheckLoginEvent> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqCheckLoginEvent.StartReqCheckLoginEvent(mBuilder);
			var offset = ReqCheckLoginEvent.EndReqCheckLoginEvent(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqCheckLoginEvent>(this, Code.GS_CHECK_LOGIN_EVENT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCheckLoginEvent recvMsgPacket = recvPacket.Get<ResCheckLoginEvent>();

				List<uint> rewardItem = new List<uint>();

				for(int i =0;i<recvMsgPacket.RewardItemsLength;i++)
				{
					rewardItem.Add(recvMsgPacket.RewardItems(i).Value.ItemTid);
				}

				LoginEventData data = new LoginEventData()
				{
					isHaveMail = recvMsgPacket.IsHaveMail,
					loginEventDt = recvMsgPacket.LoginEventDt,
					bgHash = recvMsgPacket.BgHash,
					bgUrl = recvMsgPacket.BgUrl,
					reward_Item = rewardItem,
					title = recvMsgPacket.PushTitle
				};

				Me.CurUserData.SetLoginEvent(data);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			},_onError);
		}

		public void REQ_GetAttendEventInfoList(System.Action<ZWebRecvPacket, ResGetAttendEventInfoList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetAttendEventInfoList.StartReqGetAttendEventInfoList(mBuilder);
			var offset = ReqGetAttendEventInfoList.EndReqGetAttendEventInfoList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetAttendEventInfoList>(this, Code.GS_GET_ACCOUNT_ATTEND_INFO_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetAttendEventInfoList recvMsgPacket = recvPacket.Get<ResGetAttendEventInfoList>();

				for (int i = 0; i < recvMsgPacket.InfoLength; i++)
				{
					Me.CurUserData.AddAttendData(recvMsgPacket.Info(i).Value);
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary>
		/// 서버로 "출석" 요청, 유료출석 일일 갱신용
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_RefreshCashAttendEvent(System.Action onRecvPacket)
		{
			var refreshAttendData = Me.CurUserData.GetAttendRefreshEventTarget();

			if (refreshAttendData != null)
			{
				REQ_GetAttendEventReward(E_ATTEND_TYPE.PAID_ATTEND, refreshAttendData.groupId, false, delegate { onRecvPacket?.Invoke(); }, delegate { });
			}
			else
			{
				onRecvPacket?.Invoke();
			}
		}

		/// <summary>
		/// 서버로 출석보상 요청
		/// </summary>
		/// <param name="isGetReward"> 보상받을건지 여부, false일시 이벤트 출석정보 갱신함, 유료출석이외에 사용하지않음, </param>
		public void REQ_GetAttendEventReward(E_ATTEND_TYPE mainType, uint groupId, bool isGetReward, System.Action<ZWebRecvPacket, ResGetAttendReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetAttendReward.CreateReqGetAttendReward(mBuilder, mainType, groupId, isGetReward);
			var reqPacket = ZWebPacket.Create<ReqGetAttendReward>(this, Code.GS_GET_ACCOUNT_ATTEND_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetAttendReward recvMsgPacket = recvPacket.Get<ResGetAttendReward>();

				Me.CurUserData.AddAttendData(recvMsgPacket.Info.Value);

				if (recvMsgPacket.IsSendReward)
				{
					UICommon.SetNoticeMessage(DBLocale.GetText("Event_Notice_GetRewardMail"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
					REQ_GetMailList(null);
					//REQ_GetMailRefreshTime(null);
				}

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_GetEventQuestReward(uint questEventTid, System.Action<ZWebRecvPacket, ResQuestEventReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqQuestEventReward.CreateReqQuestEventReward(mBuilder, questEventTid);
			var reqPacket = ZWebPacket.Create<ReqQuestEventReward>(this, Code.GS_QUEST_EVENT_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResQuestEventReward recvMsgPacket = recvPacket.Get<ResQuestEventReward>();

				Me.CurCharData.AddEventQuestData(recvMsgPacket.ClearQuest.Value);

				UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) => frame.AddItem(recvMsgPacket.GetItems));

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion Event

		#region Storage

		public void REQ_StorageList(Action<ZWebRecvPacket, ResStorageItemList> onReceive, PacketErrorCBDelegate _onError = null)
		{
			ReqStorageItemList.StartReqStorageItemList(mBuilder);
			var offset = ReqStorageItemList.EndReqStorageItemList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqStorageItemList>(this, Code.GS_STORAGE_ITEM_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResStorageItemList recvMsgPacket = recvPacket.Get<ResStorageItemList>();

				List<ItemEquipment> storageEquipList = new List<ItemEquipment>();
				List<ItemStack> storageStackList = new List<ItemStack>();

				Me.CurCharData.ClearStorageList();

				for (int i = 0; i < recvMsgPacket.EquipItemsLength; i++)
					storageEquipList.Add(recvMsgPacket.EquipItems(i).Value);

				Me.CurCharData.AddStorage(storageEquipList);

				for (int i = 0; i < recvMsgPacket.StackItemsLength; i++)
					storageStackList.Add(recvMsgPacket.StackItems(i).Value);

				Me.CurCharData.AddStorage(storageStackList);

				onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_StoragePut(List<ZItem> items, Action<ZWebRecvPacket, ResStorageItemPut> onReceive, PacketErrorCBDelegate _onError = null)
		{
			List<ZItem> listEquip = new List<ZItem>();
			List<ZItem> listStack = new List<ZItem>();

			foreach (ZItem item in items)
			{
				if (item.netType == NetItemType.TYPE_EQUIP)
					listEquip.Add(item);
				else if (item.netType == NetItemType.TYPE_STACK)
					listStack.Add(item);
			}

			var offsetEquip = new Offset<ItemEquipment>[listEquip.Count];
			for (int i = 0; i < listEquip.Count; i++)
			{
				ZItem item = listEquip[i];

				offsetEquip[i] = ItemEquipment.CreateItemEquipment(mBuilder, item.item_id, item.item_tid,
																   ItemEquipment.CreateOptionVector(mBuilder, item.Options.ToArray()),
																   ItemEquipment.CreateSocketsVector(mBuilder, item.Sockets.ToArray()),
																   item.slot_idx, item.IsLock ? (byte)1 : (byte)0, item.expire_dt);
			}

			var offsetStack = new Offset<ItemStack>[listStack.Count];
			for (int i = 0; i < listStack.Count; i++)
			{
				ZItem item = listStack[i];

				offsetStack[i] = ItemStack.CreateItemStack(mBuilder, item.item_id, item.item_tid, (uint)item.cnt);
			}

			var offset = ReqStorageItemPut.CreateReqStorageItemPut(mBuilder,
																   ReqStorageItemPut.CreatePutEquipItemsVector(mBuilder, offsetEquip),
																   ReqStorageItemPut.CreatePutStackItemsVector(mBuilder, offsetStack));

			var reqPacket = ZWebPacket.Create<ReqStorageItemPut>(this, Code.GS_STORAGE_ITEM_PUT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResStorageItemPut recvMsgPacket = recvPacket.Get<ResStorageItemPut>();

				// 맡긴장비 넷데이터에서 삭제 및 창고에 추가
				List<ItemEquipment> storedEquipList = new List<ItemEquipment>();
				for (int i = 0; i < recvMsgPacket.EntrustEquipsLength; i++)
					storedEquipList.Add(recvMsgPacket.EntrustEquips(i).Value);

				Me.CurCharData.RemoveItemList(storedEquipList);
				Me.CurCharData.AddStorage(storedEquipList);

				// 맡긴 스택 갱신
				List<ItemStack> storedStackList = new List<ItemStack>();
				for (int i = 0; i < recvMsgPacket.RemainStorageStacksLength; i++)
					storedStackList.Add(recvMsgPacket.RemainStorageStacks(i).Value);

				Me.CurCharData.AddStorage(storedStackList);

				// 인벤 스택 갱신
				List<ItemStack> remainInvenStackList = new List<ItemStack>();

				for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
					remainInvenStackList.Add(recvMsgPacket.RemainStacks(i).Value);

				Me.CurCharData.AddItemList(remainInvenStackList);

				onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_StorageGet(List<ZItem> items, Action<ZWebRecvPacket, ResStorageItemGet> onReceive, PacketErrorCBDelegate _onError = null)
		{
			List<ZItem> listEquip = new List<ZItem>();
			List<ZItem> listStack = new List<ZItem>();

			foreach (ZItem item in items)
			{
				if (item.netType == NetItemType.TYPE_EQUIP)
					listEquip.Add(item);
				else if (item.netType == NetItemType.TYPE_STACK)
					listStack.Add(item);
			}

			var offsetEquip = new Offset<ItemEquipment>[listEquip.Count];
			for (int i = 0; i < listEquip.Count; i++)
			{
				ZItem item = listEquip[i];

				offsetEquip[i] = ItemEquipment.CreateItemEquipment(mBuilder, item.item_id, item.item_tid,
																   ItemEquipment.CreateOptionVector(mBuilder, item.Options.ToArray()),
																   ItemEquipment.CreateSocketsVector(mBuilder, item.Sockets.ToArray()),
																   item.slot_idx, item.IsLock ? (byte)1 : (byte)0, item.expire_dt);
			}

			var offsetStack = new Offset<ItemStack>[listStack.Count];
			for (int i = 0; i < listStack.Count; i++)
			{
				ZItem item = listStack[i];

				offsetStack[i] = ItemStack.CreateItemStack(mBuilder, item.item_id, item.item_tid, (uint)item.cnt);
			}

			var offset = ReqStorageItemGet.CreateReqStorageItemGet(mBuilder,
																   ReqStorageItemGet.CreateGetEquipItemsVector(mBuilder, offsetEquip),
																   ReqStorageItemGet.CreateGetStackItemsVector(mBuilder, offsetStack));

			var reqPacket = ZWebPacket.Create<ReqStorageItemGet>(this, Code.GS_STORAGE_ITEM_GET, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResStorageItemGet recvMsgPacket = recvPacket.Get<ResStorageItemGet>();

				// 창고에서 제거된 장비
				List<ItemEquipment> removedEquipLIst = new List<ItemEquipment>();
				for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
					removedEquipLIst.Add(recvMsgPacket.GetEquips(i).Value);

				Me.CurCharData.RemoveStorage(removedEquipLIst);

				// 창고에 남은 스택 갱신
				List<ItemStack> remainStorageStackList = new List<ItemStack>();
				for (int i = 0; i < recvMsgPacket.RemainStorageStacksLength; i++)
					remainStorageStackList.Add(recvMsgPacket.RemainStorageStacks(i).Value);

				Me.CurCharData.AddStorage(remainStorageStackList);

				// 인벤토리에 추가된 장비
				List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
				for (int i = 0; i < recvMsgPacket.NewEquipsLength; i++)
					invenEquipList.Add(recvMsgPacket.NewEquips(i).Value);

				Me.CurCharData.AddItem(invenEquipList);

				// 스택 갱신
				List<ItemStack> remainStackList = new List<ItemStack>();
				for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
					remainStackList.Add(recvMsgPacket.RemainStacks(i).Value);

				Me.CurCharData.AddItem(remainStackList);

				List<AccountItemStack> remainAccountStackList = new List<AccountItemStack>();
				for(int i =0;i<recvMsgPacket.RemainAccountStacksLength;i++)
				{
					remainAccountStackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);
				}
				Me.CurCharData.AddItemList(remainAccountStackList);

				onReceive?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		#endregion

		//#region Time
		//public static void GetServerTime()
		//{
		//	ZNetGame.GetServerTime(delegate (ReceiveFBMessage recvPacket, ResGetServerTime recvMsgPacket) {
		//	});

		//	if (null != ZGameModeBase.LocalPawnController)
		//	{
		//		ZGameModeBase.SaveCharacter(ZGameModeBase.LocalPawnController.MyPawn);
		//	}
		//}

		//public static void GetServerTime(System.Action<ReceiveFBMessage, ResGetServerTime> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_SERVER_TIME);

		//	uint stageTid = 0;
		//	if (ZGameManager.hasInstance && ZGameManager.instance.CurGameMode)
		//	{
		//		stageTid = ZGameManager.instance.CurGameMode.StageTID;
		//	}
		//	else
		//	{
		//		stageTid = NetData.CurrentCharacter != null ? NetData.CurrentCharacter.LastArea : 0;
		//	}

		//	builder.Clear();
		//	var offset = ReqGetServerTime.CreateReqGetServerTime(
		//		builder,
		//		stageTid,
		//		TimeManager.Now,
		//		(uint)Version.Num);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetServerTime>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		if (recvPacket.error_code == ERROR.HEADER_ERROR_BUT_IGNORE_PACKET)
		//		{
		//		}
		//		else if (recvPacket.error_code != ERROR.NO_ERROR)
		//		{
		//			if (!WebSocketManager.instance.IsExceptionError(recvPacket, true))
		//				MessagePopup.OpenErrorMessagePopup(recvPacket.MessageTypeNumber, (WebNet.ERROR)recvPacket.BaseError_code);
		//		}
		//		else
		//		{
		//			ResGetServerTime recvMsgPacket = recvPacket.GetResMsg<ResGetServerTime>();
		//			NetData.SetServerTime(recvMsgPacket.ServerTsMs);
		//			ZLog.Log($"Next GetServerTime Second : {TimeManager.PingInterval}");
		//			TimeInvoker.instance.RequestInvoke(ZNetGame.GetServerTime, TimeManager.PingInterval);

		//			// for TimeManager
		//			{
		//				TimeManager.instance.SetTime(recvMsgPacket.ClientTime, recvMsgPacket.ServerTsMs);
		//			}

		//			List<Buff> buffList = new List<Buff>();
		//			for (int i = 0; i < recvMsgPacket.ChangeBuffInfosLength; i++)
		//				buffList.Add(recvMsgPacket.ChangeBuffInfos(i).Value);

		//			NetData.Instance.RefreshBuff(NetData.UserID, NetData.CharID, buffList);

		//			onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//		}
		//	});
		//}
		//#endregion

		//#region Stage & Battle

		///// <summary> 주어진 스테이지 입장완료했다고 WebServer에 알림 </summary>
		///// <param name="_channelId"></param>
		///// <param name="_adress">서버에서 받은 주소와 일치해야함!</param>
		///// <param name="myPawnNetId">현재 게임상 존재하는 나의 ZPawn객체의 NetId </param>
		//public static void ReqStageEntered(uint _stageTid, string _adress, Vector3 pos, uint myPawnNetId, System.Action<ReceiveFBMessage, ResEnteredStage> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_ENTERED_STAGE);

		//	builder.Clear();
		//	var offset = ReqEnteredStage.CreateReqEnteredStage(builder,
		//		_stageTid,
		//		builder.CreateString(_adress),
		//		pos.x, pos.y, pos.z,
		//		myPawnNetId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqEnteredStage>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		if (recvPacket.BaseError_code != 0)
		//			MessagePopup.OpenErrorMessagePopup(recvPacket.MessageTypeNumber, (ERROR)recvPacket.BaseError_code);

		//		ResEnteredStage recvMsgPacket = recvPacket.GetResMsg<ResEnteredStage>();

		//		List<AccountItemStack> accountstackList = new List<AccountItemStack>();
		//		List<ItemStack> stackList = new List<ItemStack>();

		//		if (recvMsgPacket.RemainAccountStack != null)
		//			accountstackList.Add(recvMsgPacket.RemainAccountStack.Value);
		//		if (recvMsgPacket.RemainStack != null)
		//			stackList.Add(recvMsgPacket.RemainStack.Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountstackList, stackList, null);

		//		NetData.CurrentCharacter.LastArea = recvMsgPacket.StageTid;
		//		NetData.CurrentCharacter.LastPortalTid = recvMsgPacket.PortalTid;
		//		NetData.CurrentCharacter.LastChannelId = recvMsgPacket.ChannelId;
		//		NetData.Instance.SetEnteredStageTID(NetData.UserID, NetData.CharID, recvMsgPacket.StageTid);
		//		//obsolete
		//		//NetData.Instance.UpdateBossPoint(NetData.UserID, NetData.CharID, recvMsgPacket.RemainStagePoint);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//		// for LocalHost
		//		PawnManager.CanPK = recvMsgPacket.IsPkZone;

		//		UIManager.instance.GetActiveUI<HudBasePanel>()?.EnteredStage();

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_STAGE_ENTER, new Dictionary<string, object>()
		//	{
		//		{ "UserId", NetData.UserID },
		//		{ "CharId", NetData.CharID },
		//		{ "StageId", _stageTid },
		//		{ "ChanneldId", NetData.CurrentCharacter.LastChannelId },
		//	});
		//	});
		//}

		//public static void ReqStageLeave(bool bGameOut, System.Action<ReceiveFBMessage, ResLeaveStage> onRecvPacket, WebSocketManager.SendFailedCallback sendFailureCallback = null)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_LEAVE_STAGE);

		//	NetData.Instance.SetEnteredStageTID(NetData.UserID, NetData.CharID, 0);

		//	NetData.bReadyToItemUse = false;
		//	WebSocketManager.instance.RemoveSendList(WebSocketType.GAME_SERVER, Code.GS_POTION_ITEM_USE, Code.GS_ITEM_GACHA, Code.GS_GUILD_BUY_BUFF, Code.GS_CHANGE_EQUIP, Code.GS_PET_EQUIP);

		//	builder.Clear();
		//	var offset = ReqLeaveStage.CreateReqLeaveStage(builder, bGameOut);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqLeaveStage>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResLeaveStage recvMsgPacket = recvPacket.GetResMsg<ResLeaveStage>();
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	}, sendFailureCallback);
		//}

		//public static void ReqBossStageCampEnter(uint PortalTid, uint UseItemTid, System.Action<ReceiveFBMessage, ResInterStageCampEnter> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INTER_STAGE_CAMP_ENTER);

		//	builder.Clear();

		//	var offset = ReqInterStageCampEnter.CreateReqInterStageCampEnter(builder, PortalTid, UseItemTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInterStageCampEnter>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInterStageCampEnter recvMsgPacket = recvPacket.GetResMsg<ResInterStageCampEnter>();

		//		NetData.StartPosition.x = recvMsgPacket.PosX;
		//		NetData.StartPosition.y = recvMsgPacket.PosY;
		//		NetData.StartPosition.z = recvMsgPacket.PosZ;
		//		NetData.CurrentCharacter.LastArea = recvMsgPacket.StageTid;
		//		NetData.CurrentCharacter.LastPortalTid = recvMsgPacket.PortalTid;
		//		NetData.Instance.SetEnteredStageTID(NetData.UserID, NetData.CharID, recvMsgPacket.StageTid);
		//		NetData.ConnectedInterServerAddr = recvMsgPacket.JoinAddr;


		//		//[wisreal][2020.02.04] - 일단 빠진다(카오스 모두다 서버별 매칭 서버 처리)
		//		/*if (!ZNetGlobalMatch.IsGlobalMatchServerOpened)
		//		{
		//			ZNetGlobalMatch.InitGlobalMatchServer();
		//		}*/

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_BOSS_DUNGEON_ENTER, new Dictionary<string, object>()
		//	{
		//		{ "UserId", NetData.UserID },
		//		{ "CharId", NetData.CharID },
		//		{ "StageId", DBPortal.GetPortalData(PortalTid).StageID },
		//		{ "Addr", recvMsgPacket.JoinAddr },
		//	});
		//	});
		//}

		///// <summary>인터보스맵안에서 포탈 사용</summary>
		//public static void ReqBossStageFieldEnter(uint PortalTid, System.Action<ReceiveFBMessage, ResInterStageFieldEnter> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INTER_STAGE_FIELD_ENTER);

		//	builder.Clear();

		//	var offset = ReqInterStageFieldEnter.CreateReqInterStageFieldEnter(builder, PortalTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInterStageFieldEnter>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInterStageFieldEnter recvMsgPacket = recvPacket.GetResMsg<ResInterStageFieldEnter>();
		//		NetData.CurrentCharacter.LastArea = recvMsgPacket.StageTid;
		//		NetData.CurrentCharacter.LastPortalTid = recvMsgPacket.PortalTid;

		//		//[wisreal][2020.02.04] - 일단 빠진다(카오스 모두다 서버별 매칭 서버 처리)
		//		/*
		//		if (!ZNetGlobalMatch.IsGlobalMatchServerOpened)
		//		{
		//			ZNetGlobalMatch.InitGlobalMatchServer();
		//		}*/

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		AnalyticsManager.instance.Event(AnalyticsManager.EVENT_BOSS_DUNGEON_FIELD_ENTER, new Dictionary<string, object>()
		//	{
		//		{ "UserId", NetData.UserID },
		//		{ "CharId", NetData.CharID },
		//		{ "StageId", DBPortal.GetPortalData(PortalTid).StageID },
		//		{ "Addr", recvMsgPacket.JoinAddr },
		//	});
		//	});
		//}

		//public static void ReqStageState(System.Action<ReceiveFBMessage, ResOverrideStageInfo> onRecvPacket, WebSocketManager.SendFailedCallback sendFailureCallback = null)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_OVERRIDE_STAGE_INFO);

		//	builder.Clear();

		//	ReqOverrideStageInfo.StartReqOverrideStageInfo(builder);
		//	var offset = ReqOverrideStageInfo.EndReqOverrideStageInfo(builder);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqOverrideStageInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResOverrideStageInfo recvMsgPacket = recvPacket.GetResMsg<ResOverrideStageInfo>();
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	}, sendFailureCallback);
		//}

		///// <summary>
		///// 
		///// </summary>
		///// <param name="StageIdx"></param>
		///// <param name="MonsterId">TID</param>
		///// <param name="partyCharIds">파티 상태에서 킬했다면, 파티원 캐릭터ID도 함께</param>
		///// <param name="bFullInven">Client-Side 인벤 꽉 찼는지 여부</param>
		//public static void KillMonster(uint MyUnetId, uint StageIdx, ushort channelid, uint MonsterId, uint PartyId, List<PartyInfo> partyList, bool bFullInven, bool bFullRuneInven, Vector3 KillPosition,
		//	List<uint> unetIds, List<ulong> guildList, Dictionary<ulong, AggroManagement.AggroInfo> aggroDic, System.Action<ResMonsterKill> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_MONSTER_KILL);

		//	builder.Clear();

		//	var partyOffset = new Offset<PartyMember>[partyList.Count];
		//	if (partyList != null && partyList.Count > 0)
		//	{
		//		List<ulong> partyIds = new List<ulong>();

		//		for (int i = 0; i < partyList.Count; i++)
		//		{
		//			partyOffset[i] = PartyMember.CreatePartyMember(builder, partyList[i].CharId, partyList[i].netId, partyList[i].ServerIdx);
		//		}
		//	}

		//	// 공격자들 정보
		//	Offset<DamageList>[] dmgListOffset;
		//	if (null != aggroDic)
		//	{
		//		dmgListOffset = new Offset<DamageList>[aggroDic.Count];
		//		int index = 0;
		//		foreach (var info in aggroDic.Values)
		//		{
		//			dmgListOffset[index] = DamageList.CreateDamageList(builder, info.ServerIdx, info.CharID, (uint)info.TotalDamage);
		//			++index;
		//		}
		//	}
		//	else
		//	{
		//		dmgListOffset = default(Offset<DamageList>[]);
		//	}

		//	var offset = ReqMonsterKill.CreateReqMonsterKill(builder, StageIdx, channelid, MonsterId, PartyId,
		//		ReqMonsterKill.CreatePartyMemberVector(builder, partyOffset)
		//		, bFullInven
		//		, bFullRuneInven
		//		, (byte)ZGameOption.Instance.Auto_Break_Belong_Item
		//		, (byte)ZGameOption.Instance.Auto_Sell_RuneGrade
		//		, (byte)ZGameOption.Instance.Auto_Sell_RuneGradeType
		//		, ReqMonsterKill.CreateRuneDropSelectTypeVector(builder, NetData.Instance.GetRuneDropSelectList(NetData.UserID, NetData.CharID))
		//		, builder.CreateString(KNetManager.instance.GetConnectedAddrForWebServer())
		//		, ReqMonsterKill.CreateUnetIdsVector(builder, unetIds.ToArray())
		//		, MyUnetId
		//		, KillPosition.x
		//		, KillPosition.y
		//		, KillPosition.z
		//		, ReqMonsterKill.CreateGuildMemberCharIdsVector(builder, guildList.ToArray())
		//		, ReqMonsterKill.CreateDamageListVector(builder, dmgListOffset));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqMonsterKill>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResMonsterKill recvMsgPacket = recvPacket.GetResMsg<ResMonsterKill>();

		//		NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);
		//		NetData.Instance.UpdateTendencyPoint(NetData.UserID, NetData.CharID, recvMsgPacket.TendencyPoint);
		//		NetData.CurrentCharacter.InstanceDungeonStageTID = recvMsgPacket.InstanceDungeonStageTid;
		//		NetData.CurrentCharacter.InstanceDungeonClearCnt = recvMsgPacket.InstanceDungeonCnt;

		//		//obsolete
		//		//NetData.Instance.UpdateBossPoint(NetData.UserID, NetData.CharID, recvMsgPacket.RemainBossPoint);

		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.RemainAccountStacksLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);
		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.GetEquips(i).Value);
		//		for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
		//			invenStackList.Add(recvMsgPacket.RemainStacks(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		//룬 획득
		//		List<Rune> runeList = new List<Rune>();

		//		for (int i = 0; i < recvMsgPacket.GetRunesLength; i++)
		//			runeList.Add(recvMsgPacket.GetRunes(i).Value);

		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

		//		onRecvPacket?.Invoke(recvMsgPacket);

		//		ZNetChatData.OnKillMonster(recvMsgPacket);

		//		string strHighItems = "{";
		//		bool bHighItem = false;

		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//		{
		//			var itemTid = recvMsgPacket.GetEquips(i).Value.ItemTid;
		//			if (DBItem.GetItemGrade(itemTid) >= 5)
		//			{
		//				strHighItems += itemTid.ToString() + ",";
		//				bHighItem = true;
		//			}
		//		}
		//		strHighItems += "}";

		//		if (bHighItem)
		//		{
		//			AnalyticsManager.instance.Event(AnalyticsManager.EVENT_HIGH_TIER_ITEM, new Dictionary<string, object>()
		//		{
		//			{ "UserID", NetData.UserID },
		//			{ "CharID", NetData.CharID },
		//			{ "ItemTids",strHighItems},
		//			{ "RecvType","KillMonster"},
		//			{ "RecvValue",MonsterId},
		//		});
		//		}
		//	});
		//}

		///// <summary></summary>
		///// <param name="deadCharId">죽은 유저의 WebNet  CharID</param>
		///// <param name="isCounterAttack">보라돌이 상태로 반격하다가 죽었는지 여부</param>
		//public static void KillPc(uint deadCharServerId, ulong deadCharId, bool isCounterAttack, uint StageTid, uint deadCharTid, string deadCharName, uint deadCharLv, int deadCharTendency, System.Action<ReceiveFBMessage, ResCharacterKill> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.COMMON_CHARACTER_KILL);

		//	builder.Clear();
		//	var offset = ReqCharacterKill.CreateReqCharacterKill(builder, isCounterAttack, StageTid, deadCharServerId, deadCharId, deadCharTid, builder.CreateString(deadCharName), deadCharLv, deadCharTendency);
		//	builder.Finish(offset.Value);

		//	reqPacket.AddBuilderMsg<ReqCharacterKill>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResCharacterKill resCharKill = recvPacket.GetResMsg<ResCharacterKill>();

		//		NetData.Instance.UpdateTendencyPoint(NetData.UserID, NetData.CharID, resCharKill.ResultTendency);
		//		NetData.CurrentCharacter.PkCnt = resCharKill.ResultPkCnt;

		//		// TODO : resCharKill.PkScore.Value.Score 사용하는 곳에 처리해주삼~
		//		// TODO : resCharKill.DecTendency 필요없으면 지우도록 합시다~

		//		onRecvPacket?.Invoke(recvPacket, resCharKill);
		//	});
		//}

		///// <summary> 현재 플레이중인 로컬플레이어가 죽었을때 호출 </summary>
		//public static void DieMyPc(uint StageTid, uint KillerServerId, ulong KillerCharId, ulong KillerUserId, string KillerName, ulong KillerGuildId, string KillerGuildName, byte KillerGuildMarkTid, System.Action<ReceiveFBMessage, ResCharacterDie> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.COMMON_CHARACTER_DIE);
		//	builder.Clear();

		//	var offset = ReqCharacterDie.CreateReqCharacterDie(builder, StageTid, KillerServerId, KillerUserId, KillerCharId, builder.CreateString(KillerName), KillerGuildId, builder.CreateString(KillerGuildName), KillerGuildMarkTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqCharacterDie>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResCharacterDie resCharDie = recvPacket.GetResMsg<ResCharacterDie>();

		//		if (NetData.Instance.GetUser(NetData.UserID, out var netUserData))
		//		{
		//			netUserData.UpdateCharacterExp(NetData.CharID, resCharDie.Exp);
		//			var netCharData = netUserData.GetCharacter(NetData.CharID);
		//			if (null != netCharData && netCharData.Lv != resCharDie.Level)
		//			{
		//				netUserData.UpdateLevel(NetData.CharID, resCharDie.Level);
		//			}

		//			//if (resCharDie.Buff.HasValue)
		//			//	netUserData.AddBuff(NetData.CharID, resCharDie.Buff.Value);

		//			var delActionTids = resCharDie.GetDelAbilityAcidArray();
		//			foreach (uint delAbilityAcid in delActionTids)
		//				netUserData.RemoveBuff(NetData.CharID, delAbilityAcid);
		//		}

		//		if (resCharDie.RestoreExp != null)
		//			NetData.Instance.AddRestoreExp(NetData.UserID, NetData.CharID, resCharDie.RestoreExp.Value);


		//		onRecvPacket?.Invoke(recvPacket, resCharDie);
		//	});
		//}

		//public static void GetMonsterKillReward(bool bFullInven, bool bFullRuneInven, byte IsInterReward, System.Action<ReceiveFBMessage, ResMonsterKillReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_MONSTER_KILL_REWARD);

		//	builder.Clear();

		//	var offset = ReqMonsterKillReward.CreateReqMonsterKillReward(builder, bFullInven, bFullRuneInven
		//		, (byte)ZGameOption.Instance.Auto_Break_Belong_Item
		//		, (byte)ZGameOption.Instance.Auto_Sell_RuneGrade
		//		, (byte)ZGameOption.Instance.Auto_Sell_RuneGradeType, IsInterReward);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqMonsterKillReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResMonsterKillReward recvMsgPacket = recvPacket.GetResMsg<ResMonsterKillReward>();

		//		if (recvMsgPacket.RemainCharExp != 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);

		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.RemainAccountStacksLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);
		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.GetEquips(i).Value);
		//		for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
		//			invenStackList.Add(recvMsgPacket.RemainStacks(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		if (recvMsgPacket.IsNextData)
		//		{
		//			GetMonsterKillReward(bFullInven, bFullRuneInven, recvMsgPacket.IsInterReward, null);
		//		}

		//		//룬 획득
		//		List<Rune> runeList = new List<Rune>();

		//		for (int i = 0; i < recvMsgPacket.RemainRunesLength; i++)
		//			runeList.Add(recvMsgPacket.RemainRunes(i).Value);

		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		ZNetChatData.OnKillMonsterReward(recvMsgPacket);

		//		//룬이 가득 찼을 경우 노티
		//		if (bFullInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Item_Inven_Full")); // 아이템 가방이 가득 찼습니다.
		//		else if (bFullRuneInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Rune_Inven_Full")); // 룬 가방이 가득 찼습니다. 

		//		//List<GainInfo> gainList = new List<GainInfo>();

		//		//for (int i = 0; i < recvMsgPacket.GetAccountStacksLength; ++i)
		//		//{
		//		//    if (recvMsgPacket.GetAccountStacks(i).Value.ItemTid == DBConfig.Gold_ID && ZGameOption.Instance.bShowGoldGainEffect)
		//		//        gainList.Add(new GainInfo(recvMsgPacket.GetAccountStacks(i).Value));
		//		//}

		//		//if(ZGameOption.Instance.bShowExpGainEffect && recvMsgPacket.GetCharExp > 0)
		//		//    gainList.Add(new GainInfo(GainType.TYPE_EXP,0, recvMsgPacket.GetCharExp));

		//		//if(gainList.Count > 0)
		//		//    UIManager.ShowGainInfo(gainList);

		//		string strHighItems = "{";
		//		bool bHighItem = false;

		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//		{
		//			var itemTid = recvMsgPacket.GetEquips(i).Value.ItemTid;
		//			if (DBItem.GetItemGrade(itemTid) >= 5)
		//			{
		//				strHighItems += itemTid.ToString() + ",";
		//				bHighItem = true;
		//			}
		//		}
		//		strHighItems += "}";

		//		if (bHighItem)
		//		{
		//			AnalyticsManager.instance.Event(AnalyticsManager.EVENT_HIGH_TIER_ITEM, new Dictionary<string, object>()
		//		{
		//			{ "UserID", NetData.UserID },
		//			{ "CharID", NetData.CharID },
		//			{ "ItemTids",strHighItems},
		//			{ "RecvType","KillMonsterReward"},
		//			{ "RecvValue","0"},
		//		});
		//		}
		//	});
		//}

		////obsolete
		/////// <summary> 보스소환 시도! </summary>
		////public static void SummonBoss(uint _stageTid, System.Action<ReceiveFBMessage, ResSummonBoss> onRecvCallback, WebSocketManager.SendFailedCallback sendFailureCallback = null)
		////{
		////	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_SUMMON_BOSS);

		////	builder.Clear();
		////	var offset = ReqSummonBoss.CreateReqSummonBoss(builder, _stageTid);
		////	builder.Finish(offset.Value);
		////	reqPacket.AddBuilderMsg<ReqSummonBoss>(builder);

		////	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) => 
		////	{
		////		ResSummonBoss resSummonBoss = recvPacket.GetResMsg<ResSummonBoss>();
		////		if ((ERROR)recvPacket.BaseError_code == ERROR.NO_ERROR)
		////		{

		////			//NetData.Instance.UpdateBossPoint(NetData.UserID, NetData.CharID, resSummonBoss.RemainStagePoint);
		////			// 재화 갱신
		////			if (resSummonBoss.RemainAccountStack.HasValue)
		////			{
		////				NetData.Instance.AddItem(NetData.UserID, NetData.CharID, resSummonBoss.RemainAccountStack.Value);
		////			}
		////		}

		////		onRecvCallback?.Invoke(recvPacket, resSummonBoss);
		////	}, sendFailureCallback);
		////}

		//public static void GetLimitDropList(uint _stageTid, System.Action<ReceiveFBMessage, ResGetConsumedLimitItemList> onRecvCallback)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_CONSUMED_LIMIT_ITEM_LIST);

		//	builder.Clear();

		//	var offset = ReqGetConsumedLimitItemList.CreateReqGetConsumedLimitItemList(builder, _stageTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetConsumedLimitItemList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetConsumedLimitItemList resSummonBoss = recvPacket.GetResMsg<ResGetConsumedLimitItemList>();

		//		onRecvCallback?.Invoke(recvPacket, resSummonBoss);
		//	});
		//}

		//public static void GetStageClearGuildList(System.Action<ReceiveFBMessage, ResFieldBossKillerGuildAll> onRecvCallback, WebSocketManager.SendFailedCallback sendFailureCallback = null)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_FIELD_BOSS_KILLER_GUILD_ALL);

		//	builder.Clear();

		//	ReqFieldBossKillerGuildAll.StartReqFieldBossKillerGuildAll(builder);
		//	var offset = ReqFieldBossKillerGuildAll.EndReqFieldBossKillerGuildAll(builder);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqFieldBossKillerGuildAll>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResFieldBossKillerGuildAll resClearList = recvPacket.GetResMsg<ResFieldBossKillerGuildAll>();

		//		onRecvCallback?.Invoke(recvPacket, resClearList);
		//	}, sendFailureCallback);
		//}

		//public static void RestoreDieBuff(ulong UseItemId, System.Action<ReceiveFBMessage, ResRestoreDieBuff> onRecvCallback)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RESTORE_DIE_BUFF);

		//	builder.Clear();

		//	var offset = ReqRestoreDieBuff.CreateReqRestoreDieBuff(builder, UseItemId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRestoreDieBuff>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRestoreDieBuff recvPacketMsg = recvPacket.GetResMsg<ResRestoreDieBuff>();

		//		NetData.Instance.RemoveBuff(NetData.UserID, NetData.CharID, recvPacketMsg.DelBuffId);

		//		NetData.CurrentCharacter.RestoreDieBuffCnt = (byte)recvPacketMsg.ResultRestoreDieBuffCnt;

		//		List<AccountItemStack> accountList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvPacketMsg.ResultAccountStackItemsLength; i++)
		//			accountList.Add(recvPacketMsg.ResultAccountStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountList, null, null);

		//		onRecvCallback?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void RestoreDieExp(ulong RestoreId, ulong UseItemId, System.Action<ReceiveFBMessage, ResRestoreExp> onRecvCallback)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RESTORE_EXP);

		//	builder.Clear();

		//	var offset = ReqRestoreExp.CreateReqRestoreExp(builder, RestoreId, UseItemId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRestoreExp>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRestoreExp recvPacketMsg = recvPacket.GetResMsg<ResRestoreExp>();

		//		NetData.Instance.RemoveRestoreExp(NetData.UserID, NetData.CharID, RestoreId);

		//		NetData.CurrentCharacter.RestoreExpCnt = (byte)recvPacketMsg.ResultRestoreExpCnt;
		//		NetData.CurrentCharacter.RestoreExpNotFreeCnt = recvPacketMsg.ResultRestoreExpNotFreeCnt;

		//		NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvPacketMsg.ResultExp);

		//		List<AccountItemStack> accountList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvPacketMsg.ResultAccountStackItemsLength; i++)
		//			accountList.Add(recvPacketMsg.ResultAccountStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountList, null, null);

		//		onRecvCallback?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}
		//#endregion


		//#region InfinityDungeon
		//public static void InfinityDungeonInfo(System.Action<ReceiveFBMessage, ResInfinityDungeonGetOpenInfo> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INFINITY_DUNGEON_GET_OPEN_INFO);

		//	builder.Clear();

		//	ReqInfinityDungeonGetOpenInfo.StartReqInfinityDungeonGetOpenInfo(builder);
		//	var offset = ReqInfinityDungeonGetOpenInfo.EndReqInfinityDungeonGetOpenInfo(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInfinityDungeonGetOpenInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInfinityDungeonGetOpenInfo recvMsgPacket = recvPacket.GetResMsg<ResInfinityDungeonGetOpenInfo>();

		//		var oldSeq = NetData.Instance.GetInfinityDungeonSeq(NetData.UserID);

		//		NetData.Instance.SetInfinityDungeon(NetData.UserID, recvMsgPacket.InfinityDungeonSeq, oldSeq != recvMsgPacket.InfinityDungeonSeq ? 0 : NetData.Instance.GetInfinityDungeonTid(NetData.UserID));
		//		if (oldSeq != recvMsgPacket.InfinityDungeonSeq)//클리어 정보도 갱신
		//			NetData.Instance.SetInfinityDungeonLastClearTid(NetData.UserID, 0);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonClear(uint DungeonTid, System.Action<ReceiveFBMessage, ResInfinityDungeonClearReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INFINITY_DUNGEON_CLEAR_REWARD);

		//	builder.Clear();

		//	var offset = ReqInfinityDungeonClearReward.CreateReqInfinityDungeonClearReward(builder, DungeonTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInfinityDungeonClearReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInfinityDungeonClearReward recvMsgPacket = recvPacket.GetResMsg<ResInfinityDungeonClearReward>();

		//		NetData.Instance.SetInfinityDungeon(NetData.UserID, recvMsgPacket.InfinityDungeonSeq, recvMsgPacket.InfinityDungeonTid);

		//		NetData.Instance.SetInfinityDungeonLastClearTid(NetData.UserID, recvMsgPacket.InfinityDungeonLastTid);

		//		if (recvMsgPacket.GetCharExp != 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonDailyReward(System.Action<ReceiveFBMessage, ResInfinityDungeonDailyReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INFINITY_DUNGEON_DAILY_REWARD);

		//	builder.Clear();

		//	ReqInfinityDungeonDailyReward.StartReqInfinityDungeonDailyReward(builder);
		//	var offset = ReqInfinityDungeonDailyReward.EndReqInfinityDungeonDailyReward(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInfinityDungeonDailyReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInfinityDungeonDailyReward recvMsgPacket = recvPacket.GetResMsg<ResInfinityDungeonDailyReward>();

		//		NetData.Instance.SetInfinityDungeonReward(NetData.UserID, recvMsgPacket.InfinityDungeonRewardDt);

		//		if (recvMsgPacket.GetCharExp != 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void GetOldExpRankList(uint ClassTid, System.Action<ReceiveFBMessage, ResGetOldExpRankList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_OLD_EXP_RANK_LIST);

		//	builder.Clear();

		//	var offset = ReqGetOldExpRankList.CreateReqGetOldExpRankList(builder, ClassTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetOldExpRankList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetOldExpRankList recvMsgPacket = recvPacket.GetResMsg<ResGetOldExpRankList>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonReset(System.Action<ReceiveFBMessage, ResInfinityDungeonReset> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INFINITY_DUNGEON_RESET);

		//	builder.Clear();

		//	ReqInfinityDungeonReset.StartReqInfinityDungeonReset(builder);
		//	var offset = ReqInfinityDungeonReset.EndReqInfinityDungeonReset(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInfinityDungeonReset>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInfinityDungeonReset recvMsgPacket = recvPacket.GetResMsg<ResInfinityDungeonReset>();

		//		List<AccountItemStack> accountList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			accountList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);

		//		NetData.Instance.SetInfinityDungeon(NetData.UserID, NetData.Instance.GetInfinityDungeonSeq(NetData.UserID), 0);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountList, null, null);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonBuffList(System.Action<ReceiveFBMessage, ResGetInfinityDungeonBuffList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_INFINITY_DUNGEON_BUFF_LIST);

		//	builder.Clear();

		//	ReqGetInfinityDungeonBuffList.StartReqGetInfinityDungeonBuffList(builder);
		//	var offset = ReqGetInfinityDungeonBuffList.EndReqGetInfinityDungeonBuffList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetInfinityDungeonBuffList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetInfinityDungeonBuffList recvMsgPacket = recvPacket.GetResMsg<ResGetInfinityDungeonBuffList>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonRandomBuffList(System.Action<ReceiveFBMessage, ResGetInfinityDungeonSelectBuffList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_INFINITY_DUNGEON_SELECT_BUFF_LIST);

		//	builder.Clear();

		//	ReqGetInfinityDungeonSelectBuffList.StartReqGetInfinityDungeonSelectBuffList(builder);
		//	var offset = ReqGetInfinityDungeonSelectBuffList.EndReqGetInfinityDungeonSelectBuffList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetInfinityDungeonSelectBuffList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetInfinityDungeonSelectBuffList recvMsgPacket = recvPacket.GetResMsg<ResGetInfinityDungeonSelectBuffList>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void InfinityDungeonRandomBuffSelect(uint selectBuff, System.Action<ReceiveFBMessage, ResInfinityDungeonSelectBuff> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_INFINITY_DUNGEON_SELECT_BUFF);

		//	builder.Clear();

		//	var offset = ReqInfinityDungeonSelectBuff.CreateReqInfinityDungeonSelectBuff(builder, selectBuff);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqInfinityDungeonSelectBuff>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResInfinityDungeonSelectBuff recvMsgPacket = recvPacket.GetResMsg<ResInfinityDungeonSelectBuff>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		#region ===== :: Party :: =====
		/// <summary> 파티 여부 체크 (ZPartyManager에서 호출하자) </summary>
		public void Req_CheckParty(uint serverIdx, ulong charId, System.Action<ZWebRecvPacket, ResPartyCheck> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyCheck.CreateReqPartyCheck(mBuilder, serverIdx, charId);
			var reqPacket = ZWebPacket.Create<ReqPartyCheck>(this, Code.COMMON_PARTY_CHECK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyCheck res = recvPacket.Get<ResPartyCheck>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 맴버 정보를 얻어온다. </summary>
		public void Req_PartyMemberInfo(uint serverIdx, ulong charId, uint partyUid, System.Action<ZWebRecvPacket, ResPartyMemberInfo> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyMemberInfo.CreateReqPartyMemberInfo(mBuilder, serverIdx, charId, partyUid);
			var reqPacket = ZWebPacket.Create<ReqPartyMemberInfo>(this, Code.COMMON_PARTY_MEMBER_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyMemberInfo res = recvPacket.Get<ResPartyMemberInfo>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 생성 </summary>
		public void Req_PartyCreate(uint serverIdx, ulong charid, uint charTid, string charNick, System.Action<ZWebRecvPacket, ResPartyCreate> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyCreate.CreateReqPartyCreate(mBuilder, charid, charTid, mBuilder.CreateString(charNick), serverIdx);
			var reqPacket = ZWebPacket.Create<ReqPartyCreate>(this, Code.COMMON_PARTY_CREATE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyCreate res = recvPacket.Get<ResPartyCreate>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 초대 </summary>
		public void Req_InviteParty(uint serverIdx, ulong senderCharacterId, uint senderCharacterTid, string senderNick, uint inviteeServerIdx, ulong inviteeCharId, string inviteeNick, System.Action<ZWebRecvPacket, ResPartyInvite> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyInvite.CreateReqPartyInvite(mBuilder, senderCharacterId, senderCharacterTid, mBuilder.CreateString(senderNick), serverIdx, inviteeCharId, mBuilder.CreateString(inviteeNick), inviteeServerIdx);
			var reqPacket = ZWebPacket.Create<ReqPartyInvite>(this, Code.COMMON_PARTY_INVITE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyInvite res = recvPacket.Get<ResPartyInvite>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 가입 </summary>
		public void Req_PartyJoin(uint serverIdx, ulong charID, uint charTid, string charNick, uint partyUid, System.Action<ZWebRecvPacket, ResPartyJoin> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyJoin.CreateReqPartyJoin(mBuilder, charID, charTid, mBuilder.CreateString(charNick), serverIdx, partyUid);
			var reqPacket = ZWebPacket.Create<ReqPartyJoin>(this, Code.COMMON_PARTY_JOIN, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyJoin res = recvPacket.Get<ResPartyJoin>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 초대 거절 </summary>
		public void Req_PartyRefuse(uint serverIdx, ulong charId, string charNick, uint partyUid, uint materServerIdx, ulong materCharId, System.Action<ZWebRecvPacket, ResPartyRefuse> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyRefuse.CreateReqPartyRefuse(mBuilder, charId, mBuilder.CreateString(charNick), serverIdx, partyUid, materCharId, materServerIdx);
			var reqPacket = ZWebPacket.Create<ReqPartyRefuse>(this, Code.COMMON_PARTY_REFUSE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyRefuse res = recvPacket.Get<ResPartyRefuse>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 나가기 </summary>
		public void Req_PartyOut(uint serverIdx, ulong charId, uint charTid, string charNick, uint partyUid, System.Action<ZWebRecvPacket, ResPartyOut> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyOut.CreateReqPartyOut(mBuilder, charId, charTid, mBuilder.CreateString(charNick), serverIdx, partyUid);
			var reqPacket = ZWebPacket.Create<ReqPartyOut>(this, Code.COMMON_PARTY_OUT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyOut res = recvPacket.Get<ResPartyOut>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티 강퇴 </summary>
		public void Req_PartyKickOut(uint serverIdx, ulong charId, uint kickuserServerIdx, ulong kickuserCharId, uint kickuserCharTid, string kickuserNick, System.Action<ZWebRecvPacket, ResPartyKickOut> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyKickOut.CreateReqPartyKickOut(mBuilder, kickuserCharId, kickuserCharTid, mBuilder.CreateString(kickuserNick), kickuserServerIdx, charId, serverIdx);
			var reqPacket = ZWebPacket.Create<ReqPartyKickOut>(this, Code.COMMON_PARTY_KICK_OUT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyKickOut res = recvPacket.Get<ResPartyKickOut>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		/// <summary> 파티장 변경 </summary>
		public void Req_PartyChangeMaster(uint serverIdx, ulong userId, ulong charId, uint newMatserServerIdx, ulong newMatserCharId, uint newMasterCharTid, string newMasterNick, System.Action<ZWebRecvPacket, ResPartyChangeMaster> onRecvPacket, PacketErrorCBDelegate onError = null)
		{
			var offset = ReqPartyChangeMaster.CreateReqPartyChangeMaster(mBuilder, userId, charId, serverIdx, newMatserCharId, newMasterCharTid, mBuilder.CreateString(newMasterNick), newMatserServerIdx);
			var reqPacket = ZWebPacket.Create<ReqPartyChangeMaster>(this, Code.COMMON_PARTY_CHANGE_MASTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPartyChangeMaster res = recvPacket.Get<ResPartyChangeMaster>();
				onRecvPacket?.Invoke(recvPacket, res);
			}, onError);
		}

		///// <summary> 나의 버프 정보를 공유하도록 한다. </summary>
		//public static void SendPartyBuffSync(uint serverIdx, ulong charId, byte[] messages, System.Action<ReceiveFBMessage, ResPartyBuffSync> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.COMMON_PARTY_BUFF_SYNC);

		//	builder.Clear();

		//	VectorOffset byteMsgOffset;
		//	if (null == messages || messages.Length == 0)
		//	{
		//		byteMsgOffset = default(VectorOffset);
		//	}
		//	else
		//	{
		//		byteMsgOffset = ReqPartyBuffSync.CreateByteMessageVector(builder, messages);
		//	}

		//	var offset = ReqPartyBuffSync.CreateReqPartyBuffSync(builder, charId, serverIdx, byteMsgOffset);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqPartyBuffSync>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResPartyBuffSync recvMsgPacket = recvPacket.GetResMsg<ResPartyBuffSync>();
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		#endregion

		#region @Colosseum =============================================
		public void REQ_AddColosseumQueue(ulong userId, ulong charId, bool isShowNick, uint serverIdx, uint stageTid, Action<ZWebRecvPacket, ResAddColosseumQueue> onRecvPacket = null, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqAddColosseumQueue.CreateReqAddColosseumQueue(mBuilder, userId, charId, isShowNick ? (byte)1 : (byte)0, serverIdx, stageTid);
			var reqPacket = ZWebPacket.Create<ReqAddColosseumQueue>(this, Code.GS_ADD_COLOSSEUM_QUEUE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResAddColosseumQueue>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_LeaveColosseumQueue(ulong userId, ulong charId, uint serverIdx, uint stageTid, Action<ZWebRecvPacket, ResLeaveColosseumQueue> onRecvPacket = null, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqLeaveColosseumQueue.CreateReqLeaveColosseumQueue(mBuilder, userId, charId, serverIdx);
			var reqPacket = ZWebPacket.Create<ReqLeaveColosseumQueue>(this, Code.GS_LEAVE_COLOSSEUM_QUEUE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResLeaveColosseumQueue>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_RewardColosseum(System.Action<ZWebRecvPacket, ResRewardColosseum> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqRewardColosseum.StartReqRewardColosseum(mBuilder);
			var offset = ReqRewardColosseum.EndReqRewardColosseum(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqRewardColosseum>(this, Code.GS_REWARD_COLOSSEUM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResRewardColosseum>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetColosseumRankList(System.Action<ZWebRecvPacket, ResGetColosseumRankList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetColosseumRankList.StartReqGetColosseumRankList(mBuilder);
			var offset = ReqGetColosseumRankList.EndReqGetColosseumRankList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetColosseumRankList>(this, Code.GS_GET_COLOSSEUM_RANK, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetColosseumRankList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}
		#endregion

		#region @GodLand =============================================
		public void REQ_GetSelfGodLandInfo(Action<ZWebRecvPacket, ResGetSelfGodLandInfo> onRecvPacket = null, PacketErrorCBDelegate _onError = null)
		{
			ReqGetSelfGodLandInfo.StartReqGetSelfGodLandInfo(mBuilder);
			var offset = ReqGetSelfGodLandInfo.EndReqGetSelfGodLandInfo(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetSelfGodLandInfo>(this, Code.GS_GOD_LAND_SELF_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetSelfGodLandInfo>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetGodLandInfo(uint slotGroupId, Action<ZWebRecvPacket, ResGetGodLandInfo> onRecvPacket = null, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetGodLandInfo.CreateReqGetGodLandInfo(mBuilder, slotGroupId);
			var reqPacket = ZWebPacket.Create<ReqGetGodLandInfo>(this, Code.GS_GOD_LAND_GET_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetGodLandInfo>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetMatchGodLandSpot(uint godLandTid, Action<ZWebRecvPacket, ResGetMatchGodLandSpot> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetMatchGodLandSpot.CreateReqGetMatchGodLandSpot(mBuilder, godLandTid);
			var reqPacket = ZWebPacket.Create<ReqGetMatchGodLandSpot>(this, Code.GS_GOD_LAND_FIGHT_MATCH, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetMatchGodLandSpot>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetGodLandSpotGatheringItem(uint godLandTid, Action<ZWebRecvPacket, ResGetGodLandSpotGatheringItem> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetGodLandSpotGatheringItem.CreateReqGetGodLandSpotGatheringItem(mBuilder, godLandTid);
			var reqPacket = ZWebPacket.Create<ReqGetGodLandSpotGatheringItem>(this, Code.GS_GOD_LAND_GATHERING_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetGodLandSpotGatheringItem>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GodLandDiscard(uint godLandTid, uint slotGroupId, Action<ZWebRecvPacket, ResGodLandDiscard> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGodLandDiscard.CreateReqGodLandDiscard(mBuilder, godLandTid, slotGroupId);
			var reqPacket = ZWebPacket.Create<ReqGodLandDiscard>(this, Code.GS_GOD_LAND_DISCARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) => {
				var recvPacketMsg = recvPacket.Get<ResGodLandDiscard>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetGodLandFightRecord(Action<ZWebRecvPacket, ResGetGodLandFightRecode> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetGodLandFightRecode.StartReqGetGodLandFightRecode(mBuilder);
			var offset = ReqGetGodLandFightRecode.EndReqGetGodLandFightRecode(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetGodLandFightRecode>(this, Code.GS_GOD_LAND_FIGHT_RECODE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetGodLandFightRecode>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}
		#endregion

		//#region Raid
		///// <summary> 매칭 큐 등록 (이지 모드일 경우 - 자동 매칭) </summary>
		//public static void RaidAddQueue(uint stageTid, System.Action<ReceiveFBMessage, ResRaidAddQueue> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ADD_QUEUE);

		//	builder.Clear();

		//	var offset = ReqRaidAddQueue.CreateReqRaidAddQueue(builder, NetData.UserID, NetData.CharID, NetData.ConnectedServerId, stageTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidAddQueue>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidAddQueue recvMsgPacket = recvPacket.GetResMsg<ResRaidAddQueue>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 매칭 취소 (이지 모드일 경우 - 자동 매칭) </summary>
		//public static void RaidLeaveQueue(uint stageTid, System.Action<ReceiveFBMessage, ResRaidLeaveQueue> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_LEAVE_QUEUE);

		//	builder.Clear();

		//	var offset = ReqRaidLeaveQueue.CreateReqRaidLeaveQueue(builder, NetData.UserID, NetData.CharID, NetData.ConnectedServerId, stageTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidLeaveQueue>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidLeaveQueue recvMsgPacket = recvPacket.GetResMsg<ResRaidLeaveQueue>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 보상 요청. Auto Match 보상 </summary>
		//public static void RaidReward(System.Action<ReceiveFBMessage, ResRaidReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_REWARD);

		//	builder.Clear();

		//	bool bFullInven = NetData.Instance.IsFullInven(NetData.UserID, NetData.CharID);
		//	bool bFullRuneInven = NetData.Instance.IsFullRuneInven(NetData.UserID, NetData.CharID);

		//	var offset = ReqRaidReward.CreateReqRaidReward(builder
		//	   , bFullInven
		//	   , bFullRuneInven
		//	   , (byte)ZGameOption.Instance.Auto_Break_Belong_Item
		//	   , (byte)ZGameOption.Instance.Auto_Sell_RuneGrade
		//	   , (byte)ZGameOption.Instance.Auto_Sell_RuneGradeType);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidReward recvMsgPacket = recvPacket.GetResMsg<ResRaidReward>();

		//		if (recvMsgPacket.RemainCharExp != 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);

		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.RemainAccountStacksLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);
		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.GetEquips(i).Value);
		//		for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
		//			invenStackList.Add(recvMsgPacket.RemainStacks(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		if (recvMsgPacket.IsNextData)
		//		{
		//			RaidReward(null);
		//		}

		//		//룬 획득
		//		List<Rune> runeList = new List<Rune>();

		//		for (int i = 0; i < recvMsgPacket.RemainRunesLength; i++)
		//			runeList.Add(recvMsgPacket.RemainRunes(i).Value);

		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		ZNetChatData.OnKillMonsterReward(recvMsgPacket);

		//		//룬이 가득 찼을 경우 노티
		//		if (bFullInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Item_Inven_Full")); // 아이템 가방이 가득 찼습니다.
		//		else if (bFullRuneInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Rune_Inven_Full")); // 룬 가방이 가득 찼습니다. 
		//	});
		//}

		///// <summary> 보상 요청. Manual Match 보상 </summary>
		//public static void RaidRoomReward(System.Action<ReceiveFBMessage, ResRaidRoomReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_REWARD);

		//	builder.Clear();

		//	bool bFullInven = NetData.Instance.IsFullInven(NetData.UserID, NetData.CharID);
		//	bool bFullRuneInven = NetData.Instance.IsFullRuneInven(NetData.UserID, NetData.CharID);

		//	var offset = ReqRaidRoomReward.CreateReqRaidRoomReward(builder
		//	   , bFullInven
		//	   , bFullRuneInven
		//	   , (byte)ZGameOption.Instance.Auto_Break_Belong_Item
		//	   , (byte)ZGameOption.Instance.Auto_Sell_RuneGrade
		//	   , (byte)ZGameOption.Instance.Auto_Sell_RuneGradeType);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomReward recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomReward>();

		//		if (recvMsgPacket.RemainCharExp != 0)
		//			NetData.Instance.UpdateCharacterExp(NetData.UserID, NetData.CharID, recvMsgPacket.RemainCharExp);

		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.RemainAccountStacksLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.RemainAccountStacks(i).Value);
		//		for (int i = 0; i < recvMsgPacket.GetEquipsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.GetEquips(i).Value);
		//		for (int i = 0; i < recvMsgPacket.RemainStacksLength; i++)
		//			invenStackList.Add(recvMsgPacket.RemainStacks(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		if (recvMsgPacket.IsNextData)
		//		{
		//			RaidRoomReward(null);
		//		}

		//		//룬 획득
		//		List<Rune> runeList = new List<Rune>();

		//		for (int i = 0; i < recvMsgPacket.RemainRunesLength; i++)
		//			runeList.Add(recvMsgPacket.RemainRunes(i).Value);

		//		NetData.Instance.AddRuneList(NetData.UserID, NetData.CharID, runeList, true);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);

		//		ZNetChatData.OnKillMonsterReward(recvMsgPacket);

		//		//룬이 가득 찼을 경우 노티
		//		if (bFullInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Item_Inven_Full")); // 아이템 가방이 가득 찼습니다.
		//		else if (bFullRuneInven)
		//			UIManager.WarnMessage(DBLocale.GetLocaleText("Rune_Inven_Full")); // 룬 가방이 가득 찼습니다. 
		//	});
		//}

		///// <summary> 레이드 룸 참가 여부 확인 </summary>
		//public static void RaidRoomCheck(System.Action<ReceiveFBMessage, ResRaidRoomCheck> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_CHECK);

		//	builder.Clear();

		//	var offset = ReqRaidRoomCheck.CreateReqRaidRoomCheck(builder, NetData.ConnectedServerId, NetData.UserID, NetData.CharID);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomCheck>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomCheck recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomCheck>();

		//		//recvMsgPacket.IsEntered
		//		//recvMsgPacket.RaidRoomIdx

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 참가 멤버 정보 요청 </summary>
		//public static void RaidRoomMemberInfo(ulong roomIdx, System.Action<ReceiveFBMessage, ResRaidRoomMemberInfo> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_MEMBER_INFO);

		//	builder.Clear();

		//	var offset = ReqRaidRoomMemberInfo.CreateReqRaidRoomMemberInfo(builder, NetData.ConnectedServerId, NetData.UserID, NetData.CharID, roomIdx);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomMemberInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomMemberInfo recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomMemberInfo>();

		//		//recvMsgPacket.MemberInfoLength
		//		//recvMsgPacket.MemberInfo

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//private static List<uint> GetMyRaidStat()
		//{
		//	//내 공방 수치. (공격력, 방어력, 마법 방어력)
		//	List<uint> stats = new List<uint>();
		//	{
		//		switch (DBCharacter.GetClassTypeByTid(NetData.CurrentCharacter.CharTid))
		//		{
		//			case GameDB.E_CharacterType.Knight:
		//				{
		//					string value = DBAbility.ParseAbilityValue((uint)eFinalStat.MAX_SHORT_ATTACK, UIManager.GetMyPawn.Stat[eFinalStat.MAX_SHORT_ATTACK]);
		//					stats.Add((uint)float.Parse(value));
		//				}
		//				break;
		//			case GameDB.E_CharacterType.Archer:
		//				{
		//					string value = DBAbility.ParseAbilityValue((uint)eFinalStat.MAX_LONG_ATTACK, UIManager.GetMyPawn.Stat[eFinalStat.MAX_LONG_ATTACK]);
		//					stats.Add((uint)float.Parse(value));
		//				}
		//				break;
		//			case GameDB.E_CharacterType.Wizard:
		//				{
		//					float fOriginValue = CharacterInfoPopup.GetStatViewValue(UIManager.GetMyPawn.Stat, eFinalStat.MAX_MAGIC_ATTACK);
		//					string value = DBAbility.ParseAbilityValue((uint)eFinalStat.MAX_MAGIC_ATTACK, fOriginValue);
		//					stats.Add((uint)float.Parse(value));
		//				}
		//				break;
		//		}

		//		string defence = DBAbility.ParseAbilityValue((uint)eFinalStat.MELEE_DEFENCE, UIManager.GetMyPawn.Stat[eFinalStat.MELEE_DEFENCE]);
		//		string mDefence = DBAbility.ParseAbilityValue((uint)eFinalStat.MAGIC_DEFENCE, UIManager.GetMyPawn.Stat[eFinalStat.MAGIC_DEFENCE]);

		//		stats.Add((uint)float.Parse(defence));
		//		stats.Add((uint)float.Parse(mDefence));
		//	}

		//	return stats;
		//}
		//public static Offset<RaidMemberDetail> GetMyRaidMemberDetail(ref FlatBufferBuilder builder, bool bMaster)
		//{
		//	return RaidMemberDetail.CreateRaidMemberDetail(builder,
		//	   NetData.UserID,
		//	   NetData.CharID,
		//	   NetData.CurrentCharacter.CharTid,
		//	   builder.CreateString(NetData.CurrentCharacter.Nick),
		//	   NetData.ConnectedServerId,
		//	   RaidMemberDetail.CreatePowerListVector(builder, GetMyRaidStat().ToArray()),
		//	   NetData.CurrentCharacter.Lv,
		//	   bMaster);
		//}
		///// <summary> 레이드 룸 생성 </summary>
		//public static void RaidRoomCreate(uint stageTid, string roomName, string passwd, System.Action<ReceiveFBMessage, ResRaidRoomCreate> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_CREATE);

		//	builder.Clear();

		//	var offset = ReqRaidRoomCreate.CreateReqRaidRoomCreate(builder, GetMyRaidMemberDetail(ref builder, true), stageTid, builder.CreateString(roomName), builder.CreateString(passwd));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomCreate>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomCreate recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomCreate>();

		//		//recvMsgPacket.RaidRoomIdx
		//		//recvMsgPacket.RaidRoomMember

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 초대 </summary>
		//public static void RaidRoomInvite(ulong targetCharId, string targetNick, System.Action<ReceiveFBMessage, ResRaidRoomInvite> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_INVITE);

		//	builder.Clear();

		//	var offset = ReqRaidRoomInvite.CreateReqRaidRoomInvite(builder,
		//		NetData.UserID,
		//		NetData.CharID,
		//		NetData.CurrentCharacter.CharTid,
		//		builder.CreateString(NetData.CurrentCharacter.Nick),
		//		NetData.ConnectedServerId,
		//		targetCharId,
		//		builder.CreateString(targetNick),
		//		NetData.ConnectedServerId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomInvite>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomInvite recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomInvite>();

		//		//recvMsgPacket.MemberInfoLength
		//		//recvMsgPacket.MemberInfo

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 가입 </summary>
		//public static void RaidRoomJoin(ulong raidRoomIdx, System.Action<ReceiveFBMessage, ResRaidRoomInviteJoin> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_INVITE_JOIN);

		//	builder.Clear();

		//	var offset = ReqRaidRoomInviteJoin.CreateReqRaidRoomInviteJoin(builder,
		//		GetMyRaidMemberDetail(ref builder, false),
		//		raidRoomIdx);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomInviteJoin>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomInviteJoin recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomInviteJoin>();

		//		//recvMsgPacket.RaidRoomIdx
		//		//recvMsgPacket.IsJoin
		//		//recvMsgPacket.RaidRoomMembersLength
		//		//recvMsgPacket.RaidRoomMembers

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 거절 </summary>
		//public static void RaidRoomRefuse(ulong raidRoomIdx, ulong senderUserId, ulong senderCharId, uint senderServerId, bool bError, System.Action<ReceiveFBMessage, ResRaidRoomRefuse> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_REFUSE);

		//	builder.Clear();

		//	var offset = ReqRaidRoomRefuse.CreateReqRaidRoomRefuse(builder,
		//		NetData.UserID,
		//		NetData.CharID,
		//		builder.CreateString(NetData.CurrentCharacter.Nick),
		//		NetData.ConnectedServerId,
		//		raidRoomIdx,
		//		senderUserId,
		//		senderCharId,
		//		senderServerId
		//		, bError);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomRefuse>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomRefuse recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomRefuse>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 탈퇴 </summary>
		//public static void RaidRoomOut(ulong raidRoomIdx, System.Action<ReceiveFBMessage, ResRaidRoomOut> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_OUT);

		//	builder.Clear();

		//	var offset = ReqRaidRoomOut.CreateReqRaidRoomOut(builder,
		//		NetData.UserID,
		//		NetData.CharID,
		//		NetData.CurrentCharacter.CharTid,
		//		builder.CreateString(NetData.CurrentCharacter.Nick),
		//		NetData.ConnectedServerId,
		//		raidRoomIdx);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomOut>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomOut recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomOut>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 강퇴 </summary>
		//public static void RaidRoomKickOut(ulong kickUserId, ulong kickCharId, uint kickCharTid, string kickNickName, uint kickServerId, System.Action<ReceiveFBMessage, ResRaidRoomKickOut> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_KICK_OUT);

		//	builder.Clear();

		//	var offset = ReqRaidRoomKickOut.CreateReqRaidRoomKickOut(builder,
		//		kickUserId,
		//		kickCharId,
		//		kickCharTid,
		//		builder.CreateString(kickNickName),
		//		kickServerId,
		//		NetData.UserID,
		//		NetData.CharID,
		//		NetData.ConnectedServerId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomKickOut>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomKickOut recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomKickOut>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 위임 </summary>
		//public static void RaidRoomChangeMaster(ulong targetUserId, ulong targetCharId, uint targetCharTid, string targetNickName, uint targetServerId, System.Action<ReceiveFBMessage, ResRaidRoomChangeMaster> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_CHANGE_MASTER);

		//	builder.Clear();

		//	var offset = ReqRaidRoomChangeMaster.CreateReqRaidRoomChangeMaster(builder,
		//		NetData.UserID,
		//		NetData.CharID,
		//		NetData.ConnectedServerId,
		//		targetUserId,
		//		targetCharId,
		//		targetCharTid,
		//		builder.CreateString(targetNickName),
		//		targetServerId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomChangeMaster>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomChangeMaster recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomChangeMaster>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 리스트 요청 </summary>
		//public static void RaidRoomList(uint stageTid, string seearchitle, E_RaidRoomSortType type, uint page, System.Action<ReceiveFBMessage, ResRaidRoomList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_LIST);

		//	builder.Clear();

		//	var offset = ReqRaidRoomList.CreateReqRaidRoomList(builder,
		//		stageTid,
		//		builder.CreateString(seearchitle),
		//		type,
		//		page);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomList recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomList>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 시작 카운트 방인원에게 알림 </summary>
		//public static void RaidRoomStartNotify(ulong roomIdx, System.Action<ReceiveFBMessage, ResRaidRoomStartNotify> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_START_NOTIFY);

		//	builder.Clear();

		//	var offset = ReqRaidRoomStartNotify.CreateReqRaidRoomStartNotify(builder, NetData.ConnectedServerId, NetData.UserID, NetData.CharID, roomIdx);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomStartNotify>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomStartNotify recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomStartNotify>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 시작 </summary>
		//public static void RaidRoomStart(ulong roomIdx, System.Action<ReceiveFBMessage, ResRaidRoomStart> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_START);

		//	builder.Clear();

		//	var offset = ReqRaidRoomStart.CreateReqRaidRoomStart(builder, NetData.ConnectedServerId, NetData.UserID, NetData.CharID, roomIdx);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomStart>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomStart recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomStart>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 레이드 룸 입장 </summary>
		//public static void RaidRoomEnter(ulong roomIdx, string passwd, System.Action<ReceiveFBMessage, ResRaidRoomEnter> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_RAID_ROOM_ENTER);

		//	builder.Clear();

		//	var offset = ReqRaidRoomEnter.CreateReqRaidRoomEnter(builder, GetMyRaidMemberDetail(ref builder, false), roomIdx, builder.CreateString(passwd));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRaidRoomEnter>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRaidRoomEnter recvMsgPacket = recvPacket.GetResMsg<ResRaidRoomEnter>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//#endregion

		#region EventList
		public void REQ_GetServerEventList(Action<ZWebRecvPacket, ResGetServerEventList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetServerEventList.StartReqGetServerEventList(mBuilder);
			var offset = ReqGetServerEventList.EndReqGetServerEventList(mBuilder);
			var reqPacket = ZWebPacket.Create<ReqGetServerEventList>(this, Code.GS_GET_SERVER_EVENT_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				var recvPacketMsg = recvPacket.Get<ResGetServerEventList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		#endregion

		//#region Scenario
		///// <summary> 시나리오 던전 클리어 </summary>
		//public static void ScenarioDungeonReward(uint stageTid, System.Action<ReceiveFBMessage, ResScenarioDungeonReward> onRecvPacket = null)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_SCENARIO_DUNGEON_REWARD);

		//	builder.Clear();
		//	var offset = ReqScenarioDungeonReward.CreateReqScenarioDungeonReward(builder, stageTid);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqScenarioDungeonReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResScenarioDungeonReward recvMsgPacket = recvPacket.GetResMsg<ResScenarioDungeonReward>();

		//		NetData.Instance.SetScenarioDungeonStageTid(NetData.UserID, recvMsgPacket.ScenarioDungeonStageTid);
		//		NetData.Instance.SetScenarioDungeonPlayCnt(NetData.UserID, recvMsgPacket.ScenarioDungeonPlayCnt);

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		if (recvMsgPacket.GetItems != null)
		//		{
		//			List<GainInfo> gainList = new List<GainInfo>();
		//			List<string> addmsg = new List<string>();

		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.EquipLength; i++)
		//			{
		//				var item = recvMsgPacket.GetItems.Value.Equip(i).Value;
		//				gainList.Add(new GainInfo(item));
		//				addmsg.Add(string.Format("{0}을 획득하였습니다.", DBItem.GetItemFullName(item.ItemTid)));
		//			}
		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.AccountStackLength; i++)
		//			{
		//				var item = recvMsgPacket.GetItems.Value.AccountStack(i).Value;
		//				gainList.Add(new GainInfo(item));
		//				addmsg.Add(string.Format("{0}({1})을 획득하였습니다.", DBItem.GetItemFullName(item.ItemTid), item.Cnt));
		//			}
		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.StackLength; i++)
		//			{
		//				var item = recvMsgPacket.GetItems.Value.Stack(i).Value;
		//				gainList.Add(new GainInfo(item));
		//				addmsg.Add(string.Format("{0}({1})을 획득하였습니다.", DBItem.GetItemFullName(item.ItemTid), item.Cnt));
		//			}


		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.PetLength; i++)
		//			{
		//				var pet = recvMsgPacket.GetItems.Value.Pet(i).Value;
		//				gainList.Add(new GainInfo(pet));
		//				addmsg.Add(string.Format("{0}을 획득하였습니다.", DBPet.GetPetFullName(pet.PetTid)));
		//			}

		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.ChangeLength; i++)
		//			{
		//				var change = recvMsgPacket.GetItems.Value.Change(i).Value;
		//				gainList.Add(new GainInfo(change));
		//				addmsg.Add(string.Format("{0}을 획득하였습니다.", DBChange.GetChangeFullName(change.ChangeTid)));
		//			}

		//			for (int i = 0; i < recvMsgPacket.GetItems.Value.RuneLength; i++)
		//			{
		//				var Rune = recvMsgPacket.GetItems.Value.Rune(i).Value;
		//				gainList.Add(new GainInfo(Rune));
		//				addmsg.Add(string.Format("{0}을 획득하였습니다.", DBItem.GetItemFullName(Rune.ItemTid)));
		//			}

		//			UIManager.ShowGainEff(gainList);
		//			ZNetChatData.AddSystemMsg(addmsg);
		//		}

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		//#region Event
		//public static void CheckRecvEvent()
		//{
		//	if (NetData.CharID != 0)
		//		CheckRecvEvent(null);
		//}

		//public static void CheckRecvEvent(System.Action<ReceiveFBMessage, ResCheckLoginEvent> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHECK_LOGIN_EVENT);

		//	builder.Clear();

		//	ReqCheckLoginEvent.StartReqCheckLoginEvent(builder);
		//	var offset = ReqCheckLoginEvent.EndReqCheckLoginEvent(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqCheckLoginEvent>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResCheckLoginEvent recvMsgPacket = recvPacket.GetResMsg<ResCheckLoginEvent>();

		//		if (recvMsgPacket.IsHaveMail)
		//			GetMailRefreshTime();

		//		TimeInvoker.instance.RequestInvoke(CheckRecvEvent, TimeHelper.MinuteSecond * 10);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void AttendEvent(System.Action<ReceiveFBMessage, ResDailyAttendEvent> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_DAILY_ATTEND_EVENT);

		//	builder.Clear();

		//	ReqDailyAttendEvent.StartReqDailyAttendEvent(builder);
		//	var offset = ReqDailyAttendEvent.EndReqDailyAttendEvent(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqDailyAttendEvent>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResDailyAttendEvent recvMsgPacket = recvPacket.GetResMsg<ResDailyAttendEvent>();

		//		NetData.Instance.SetAttendRewardSeq(NetData.UserID, recvMsgPacket.AttendRewardSeq);
		//		NetData.Instance.SetAttendContinuityRewardSeq(NetData.UserID, recvMsgPacket.AttendContinuityRewardSeq);
		//		NetData.Instance.SetAttendRewardDt(NetData.UserID, recvMsgPacket.AttendRewardDt);

		//		List<AccountItemStack> accountList = new List<AccountItemStack>();
		//		List<ItemStack> stackList = new List<ItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			accountList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			stackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, accountList, stackList, null);

		//		AlramUI.CheckAlram(Alram.ATTEND);
		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static void ChickenCoupon(uint UseItemTid, string senderPhoneNumber, string PhoneNumber, string Msg, System.Action<ReceiveFBMessage, ResChickenCouponMake> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHICKEN_COUPON_MAKE);

		//	builder.Clear();

		//	var offset = ReqChickenCouponMake.CreateReqChickenCouponMake(builder, UseItemTid, builder.CreateString(senderPhoneNumber), builder.CreateString(PhoneNumber), builder.CreateString(Msg));

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChickenCouponMake>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChickenCouponMake recvMsgPacket = recvPacket.GetResMsg<ResChickenCouponMake>();

		//		if (recvMsgPacket.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvMsgPacket.ResultStackItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//public static float LastEventRewardRecvTime;
		//static ReceiveFBMessage LastEventRewardRecv;
		//public static ResGetServerEventRewardInfo LastEventRewardRecvMsg;
		//public static void CheckEventReward(bool bForce, System.Action<ReceiveFBMessage, ResGetServerEventRewardInfo> onRecvPacket)
		//{
		//	if (!bForce && (Time.realtimeSinceStartup - LastEventRewardRecvTime) < 10f)
		//	{
		//		onRecvPacket?.Invoke(LastEventRewardRecv, LastEventRewardRecvMsg);
		//		return;
		//	}

		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_SERVER_EVENT_REWARD_INFO);

		//	builder.Clear();

		//	ReqGetServerEventRewardInfo.StartReqGetServerEventRewardInfo(builder);
		//	var offset = ReqGetServerEventRewardInfo.EndReqGetServerEventRewardInfo(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetServerEventRewardInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		LastEventRewardRecv = recvPacket;
		//		LastEventRewardRecvMsg = recvPacket.GetResMsg<ResGetServerEventRewardInfo>();
		//		LastEventRewardRecvTime = Time.realtimeSinceStartup;

		//		AlramUI.CheckAlram(Alram2.REWARD_EVENT);

		//		onRecvPacket?.Invoke(LastEventRewardRecv, LastEventRewardRecvMsg);
		//	});
		//}

		//public static void GetEventRewarwd(ulong eventId, System.Action<ReceiveFBMessage, ResTakeServerEventReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TAKE_SERVER_EVENT_REWARD);

		//	builder.Clear();

		//	var offset = ReqTakeServerEventReward.CreateReqTakeServerEventReward(builder, eventId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTakeServerEventReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTakeServerEventReward recvMsgPacket = recvPacket.GetResMsg<ResTakeServerEventReward>();

		//		AddRemainItems(NetData.UserID, NetData.CharID, recvMsgPacket.RemainItems.Value);

		//		LastEventRewardRecvTime = 0;//갱신용 위해 리프레시 타임 초기화

		//		AlramUI.CheckAlram(Alram2.REWARD_EVENT);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		//#region SpecialEvent
		/////새롭게 추가되는 이벤트패널

		////이벤트리스트 요청, 첫 로그인시 or 날 바뀔때 받아옴
		//public static void GetEventIngameList(System.Action<ReceiveFBMessage, ResEventIngameList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_EVENT_INGAME_LIST);

		//	builder.Clear();

		//	ReqEventIngameList.StartReqEventIngameList(builder);
		//	var offset = ReqEventIngameList.EndReqEventIngameList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqEventIngameList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResEventIngameList recvMsgPacket = recvPacket.GetResMsg<ResEventIngameList>();

		//		NetData.Instance.SetSpecialEventData(NetData.UserID, recvMsgPacket);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		////접속이벤트 보상요청
		//public static void GetIngameAttendanceReward(ulong eventId, E_AttendEventType attendType, byte attendDay, System.Action<ReceiveFBMessage, ResEventIngameAttendanceReward> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_EVENT_INGAME_ATTENDANCE_REWARD);

		//	builder.Clear();

		//	var offset = ReqEventIngameAttendanceReward.CreateReqEventIngameAttendanceReward(builder, eventId, attendType, attendDay);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqEventIngameAttendanceReward>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResEventIngameAttendanceReward recvMsgPacket = recvPacket.GetResMsg<ResEventIngameAttendanceReward>();

		//		ZNetGame.GetMailRefreshTime();

		//		if (recvMsgPacket.IsHaveMail)
		//		{
		//			UIManager.NoticeMessage("메일로 발송됨", 1f);
		//		}

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//#endregion SpecialEvent

		//#region Boss
		//public static void GetBossSpawnInfo(System.Action<ReceiveFBMessage, ResGetServerBossInfo> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.COMMON_GET_SERVER_BOSS_INFO);

		//	builder.Clear();

		//	var offset = ReqGetServerBossInfo.CreateReqGetServerBossInfo(builder, is_req_spawn: true, is_req_boss_info: true);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetServerBossInfo>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetServerBossInfo recvMsgPacket = recvPacket.GetResMsg<ResGetServerBossInfo>();

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}
		//#endregion

		//#region Option
		//public static void GetCharacterOptionList(System.Action<ReceiveFBMessage, ResGetCharacterOptionList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_CHARACTER_OPTION_LIST);

		//	builder.Clear();

		//	ReqGetCharacterOptionList.StartReqGetCharacterOptionList(builder);
		//	var offset = ReqGetCharacterOptionList.EndReqGetCharacterOptionList(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetCharacterOptionList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetCharacterOptionList recvMsgPacket = recvPacket.GetResMsg<ResGetCharacterOptionList>();

		//		NetData.Instance.ClearOptionList(NetData.UserID, NetData.CharID);

		//		List<CharacterOption> charoptionList = new List<CharacterOption>();
		//		for (int i = 0; i < recvMsgPacket.OptionListLength; i++)
		//			charoptionList.Add(recvMsgPacket.OptionList(i).Value);

		//		NetData.Instance.AddOptionInfos(NetData.UserID, NetData.CharID, charoptionList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		/// <param name="optionKeys"> <see cref="E_CharacterOptionKey"/>값들 </param>
		public void GetCharacterOption(List<uint> optionKeys, System.Action<ZWebRecvPacket, ResGetCharacterOption> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetCharacterOption.CreateReqGetCharacterOption(mBuilder, Me.UserID, Me.CharID, Me.SelectedServerID, ReqGetCharacterOption.CreateKeyVector(mBuilder, optionKeys.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqGetCharacterOption>(this, Code.COMMON_GET_CHARACTER_OPTION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetCharacterOption recvMsgPacket = recvPacket.Get<ResGetCharacterOption>();

				List<CharacterOption> charoptionList = new List<CharacterOption>();
				for (int i = 0; i < recvMsgPacket.OptionLength; i++)
					charoptionList.Add(recvMsgPacket.Option(i).Value);

				for (int i = 0; i < charoptionList.Count; i++)
					Me.CurCharData.AddOptionInfo(charoptionList[i]);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_SetCharacterOption(E_CharacterOptionKey optionKey, string optionValue, System.Action<ZWebRecvPacket, ResSetCharacterOption> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var options = new Offset<CharacterOption>[1];
			options[0] = CharacterOption.CreateCharacterOption(mBuilder, (uint)optionKey, mBuilder.CreateString(optionValue));

			var offset = ReqSetCharacterOption.CreateReqSetCharacterOption(mBuilder, Me.UserID, Me.CurCharData.ID, Me.SelectedServerID, ReqSetCharacterOption.CreateOptionVector(mBuilder, options));

			var reqPacket = ZWebPacket.Create<ReqSetCharacterOption>(this, Code.COMMON_SET_CHARACTER_OPTION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSetCharacterOption recvMsgPacket = recvPacket.Get<ResSetCharacterOption>();

				for (int i = 0; i < recvMsgPacket.OptionsLength; i++)
					Me.CurCharData.AddOptionInfo(recvMsgPacket.Options(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		public void REQ_SetCharacterCurrentPreset(System.Action<ZWebRecvPacket, ResSetCharacterOption> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var presetNum = Me.CurCharData.SelectEquipSetNo;
			var presetValue = Me.CurCharData.GetEquipSetValue(presetNum);

			switch (presetNum)
			{
				case 1:
					REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET1, presetValue, onRecvPacket, _onError);
					break;
				case 2:
					REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET2, presetValue, onRecvPacket, _onError);
					break;
				case 3:
					REQ_SetCharacterOption(E_CharacterOptionKey.EQUIP_SET3, presetValue, onRecvPacket, _onError);
					break;
			}
		}
		//#endregion

		//#region MergeFunction
		///// <summary> 캐릭터 정보만 받았다면 </summary>
		//public static bool IsReceivedOnlyCharInfo
		//{
		//	get; set;
		//}

		///// <summary> 캐릭터의 모든 정보 받았다면, true  </summary>
		//public static bool IsReceivedCharAllInfo
		//{
		//	get { return isReceviedCharAllInfo; }
		//	set
		//	{
		//		ZLog.LogWarning($"IsReceivedCharAllInfo : {isReceviedCharAllInfo} = {value}");
		//		isReceviedCharAllInfo = value;
		//	}
		//}
		//private static bool isReceviedCharAllInfo;

		///// <summary> </summary>
		//static public readonly byte[] REQ_CharacterAllBits = new byte[]
		//{
		//(byte)E_CharInfoReqBit.CHARACTER,
		//(byte)E_CharInfoReqBit.BLOCK,
		//(byte)E_CharInfoReqBit.BUFF,
		//(byte)E_CharInfoReqBit.RESTORE_EXP,
		//(byte)E_CharInfoReqBit.STORAGE,
		//(byte)E_CharInfoReqBit.ACCOUNT_STACK,
		//(byte)E_CharInfoReqBit.EQUIP,
		//(byte)E_CharInfoReqBit.STACK,
		//(byte)E_CharInfoReqBit.MARK,
		//(byte)E_CharInfoReqBit.CHANGE,
		//(byte)E_CharInfoReqBit.CHANGE_ENCHANT,
		//(byte)E_CharInfoReqBit.CHANGE_GACHA_KEEP,
		//(byte)E_CharInfoReqBit.PET,
		//(byte)E_CharInfoReqBit.PET_ENCHANT,
		//(byte)E_CharInfoReqBit.PET_GACHA_KEEP,
		//(byte)E_CharInfoReqBit.CHANGE_COLLECT,
		//(byte)E_CharInfoReqBit.ITEM_COLLECT,
		//(byte)E_CharInfoReqBit.PET_COLLECT,
		//(byte)E_CharInfoReqBit.GUILD,
		//(byte)E_CharInfoReqBit.GUILD_BUFF,
		//(byte)E_CharInfoReqBit.GUILD_ALLIANCE,
		//(byte)E_CharInfoReqBit.SKILL_BOOK,
		//(byte)E_CharInfoReqBit.PET_ADVENTURE,
		//(byte)E_CharInfoReqBit.OPTION,
		//(byte)E_CharInfoReqBit.QUEST,
		//(byte)E_CharInfoReqBit.DAILY_QUEST,
		//(byte)E_CharInfoReqBit.FRIEND,
		//(byte)E_CharInfoReqBit.REQUEST_FRIEND,
		//(byte)E_CharInfoReqBit.RUNE,
		//(byte)E_CharInfoReqBit.ITEM_ACQ_HISTORY,
		//(byte)E_CharInfoReqBit.EVENT_QUEST,
		//(byte)E_CharInfoReqBit.CHANGE_QUEST,
		//(byte)E_CharInfoReqBit.RAID_ETC,
		//(byte)E_CharInfoReqBit.REPEAT_QUEST,
		//};

		//public static void GetCharacterAllInfo(bool bInitData, ulong _userId, ulong _charId, System.Action onSuccess)
		//{
		//	ZNetGame.IsReceivedCharAllInfo = false;
		//	ZNetGame.IsReceivedOnlyCharInfo = false;

		//	ZLog.BeginProfile("IsReceivedOnlyCharInfo");

		//	GetAllCharInfoBundle(bInitData, _userId, _charId, REQ_CharacterAllBits, (arg1, arg2) => {
		//		// 캐릭터 정보만 있으면 일단 캐릭터 생성 요청 가능
		//		ZNetGame.IsReceivedOnlyCharInfo = true;
		//		ZLog.EndProfile("IsReceivedOnlyCharInfo");

		//		ZLog.BeginProfile("IsReceivedCharAllInfo");
		//		//GetCharacterOptionList((msg1, msg2) =>{
		//		CheckParty(NetData.ConnectedServerId, _charId, (partyArg1, partyArg2) =>
		//		{
		//			if (partyArg2.IsParty)
		//				ZNetGame.GetPartyMember(NetData.ConnectedServerId, NetData.CharID, partyArg2.PartyUid, null);

		//			ZNetGame.IsReceivedCharAllInfo = true;
		//			ZLog.EndProfile("IsReceivedCharAllInfo");

		//			if (!PlatformSpecific.IsUnityServer) MiniMapUI.bCheckStart = true;

		//			CheckDailyResetEvent((arg41, arg42) => {
		//				ChangeQuestDailyReset((arg43, arg44) => {
		//					GetMailList((arg51, arg52) => {
		//						GetMessageList((arg61, arg62) => {
		//							GetExchangeMessageList((arg71, arg72) => {
		//								//GetQuestList((arg81, arg82) => {
		//								//GetDailyQuestList((arg91, arg92) => {
		//								GetBuyLimitList((arg101, arg102) => {
		//									//GetMakeLimitList((arg111, arg112) => {
		//									//GetFriendList((x1, y1) => {
		//									//GetRequestFriendList((x2, y2) => {
		//									GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 0, (arg121, arg122) => {
		//										GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 1, (arg131, arg132) => {
		//											GetColosseumResult((arg141, arg142) => {
		//												if (arg142.IsHaveResult == 1 && !PlatformSpecific.IsUnityServer && arg142.ResultAccountStackItemsLength > 0)
		//													UIManager.NoticeMessage(DBLocale.GetLocaleText("WPvP_Duel_RewardNotice"));


		//												GetMailRefreshTime();
		//												CheckRecvEvent();
		//												onSuccess?.Invoke();
		//											});
		//										});
		//									});
		//								});
		//							});
		//							//});
		//							//});
		//							//});
		//							//});
		//							//});
		//						});
		//					});
		//				});
		//			});
		//		});
		//		//});
		//	});
		//}

		///// <summary> 콜로세움 용 정보 받아오기용 플래그값 </summary>
		//static readonly byte[] reqColosseumCharacterAllbit = new byte[]
		//{
		//(byte)E_CharInfoReqBit.CHARACTER,
		//(byte)E_CharInfoReqBit.BUFF,
		//(byte)E_CharInfoReqBit.GUILD_BUFF,
		//(byte)E_CharInfoReqBit.ITEM_COLLECT,
		//(byte)E_CharInfoReqBit.CHANGE,
		//(byte)E_CharInfoReqBit.CHANGE_ENCHANT,
		//(byte)E_CharInfoReqBit.CHANGE_COLLECT,
		//(byte)E_CharInfoReqBit.PET,
		//(byte)E_CharInfoReqBit.PET_ENCHANT,
		//(byte)E_CharInfoReqBit.PET_COLLECT,
		//(byte)E_CharInfoReqBit.PET_ADVENTURE,
		//(byte)E_CharInfoReqBit.RUNE,
		//(byte)E_CharInfoReqBit.PET_COLLECT,
		//(byte)E_CharInfoReqBit.ITEM_ACQ_HISTORY,
		//};

		//public static void GetCharacterAllInfo_Colosseum(bool bInitData, ulong _userId, ulong _charId, System.Action onSuccess)
		//{
		//	NetData.Instance.ClearInvenList(_userId, _charId);
		//	ZNetGame.IsReceivedCharAllInfo = false;
		//	ZNetGame.IsReceivedOnlyCharInfo = false;
		//	GetAllCharInfoBundle(bInitData, _userId, _charId, reqColosseumCharacterAllbit, (arg1, arg2) => {
		//		GetCharacterEquipItemInfos(_userId, _charId, (arg11, arg21) =>
		//		{
		//			// 캐릭터 정보만 있으면 일단 캐릭터 생성 요청 가능
		//			ZNetGame.IsReceivedOnlyCharInfo = true;

		//			GetCharacterOption(NetData.ConnectedServerId, _userId, _charId, new List<uint>() { (uint)E_CharacterOptionKey.Colosseum_HP_Auto_Per }, (msg3, msg4) =>
		//			{
		//				//GetCharacterOptionList((msg1, msg2) => {
		//				ZNetGame.IsReceivedCharAllInfo = true;

		//				if (!PlatformSpecific.IsUnityServer) MiniMapUI.bCheckStart = true;

		//				CheckDailyResetEvent((arg41, arg42) =>
		//				{
		//					ChangeQuestDailyReset((arg43, arg44) =>
		//					{
		//						GetMailList((arg51, arg52) =>
		//						{
		//							GetMessageList((arg61, arg62) =>
		//							{
		//								GetExchangeMessageList((arg71, arg72) =>
		//								{
		//									GetQuestList((arg81, arg82) =>
		//									{
		//										//GetDailyQuestList((arg91, arg92) =>
		//										//{
		//										GetBuyLimitList((arg101, arg102) =>
		//										{
		//											GetMakeLimitList((arg111, arg112) =>
		//											{
		//												GetFriendList((x1, y1) =>
		//												{
		//													GetRequestFriendList((x2, y2) =>
		//													{
		//														GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 0, (arg121, arg122) =>
		//														{
		//															GetMonsterKillReward(NetData.Instance.IsFullInven(_userId, _charId), NetData.Instance.IsFullRuneInven(_userId, _charId), 1, (arg131, arg132) =>
		//															{
		//																GetColosseumResult((arg141, arg142) =>
		//																{

		//																	if (arg142.IsHaveResult == 1 && !PlatformSpecific.IsUnityServer && arg142.ResultAccountStackItemsLength > 0)
		//																		UIManager.NoticeMessage(DBLocale.GetLocaleText("WPvP_Duel_RewardNotice"));

		//																	GetMailRefreshTime();
		//																	CheckRecvEvent();
		//																	onSuccess?.Invoke();
		//																});
		//															});
		//														});
		//													});
		//												});
		//											});
		//										});
		//										//});
		//									});
		//								});
		//							});
		//						});
		//					});
		//				});
		//			});
		//			//GetCharacterItemInfos(bReconnect, _userId, _charId, (arg31, arg32) => {                
		//		});
		//	});
		//}

		///// <summary>
		///// New버전
		///// </summary>
		//public static void GetAllCharInfoBundle(bool bInitData, ulong _userId, ulong _charId, byte[] charInfoReqBit, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.COMMON_GET_ALL_CHAR_INFO_BUNDLE);
		//	builder.Clear();

		//	var infoBitOffset = ReqGetAllCharInfoBundle.CreateCharInfoReqNumVector(builder, charInfoReqBit);

		//	var offset = ReqGetAllCharInfoBundle.CreateReqGetAllCharInfoBundle(builder,
		//		_userId,
		//		_charId,
		//		NetData.ConnectedServerId,
		//		infoBitOffset);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetAllCharInfoBundle>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetAllCharInfoBundle recvMsgPacket = recvPacket.GetResMsg<ResGetAllCharInfoBundle>();

		//		NetData.Instance.AddAllCharInfoBundle(bInitData, _userId, _charId, ref recvMsgPacket);

		//		//TimeInvoker.instance.RequestInvoke(RefreshDailyQuestList, TimeHelper.GetRemainSpecificTime(DBConfig.Event_Reset_Time));

		//		AlramUI.CheckAlram(Alram.QUEST | Alram.SPECIAL_SHOP);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary> 캐릭터 정보 받아오기용 플래그값 </summary>
		//static readonly byte[] CharacterInfoBits = new byte[]
		//{
		//(byte)E_CharInfoReqBit.CHARACTER,
		//(byte)E_CharInfoReqBit.BLOCK,
		//(byte)E_CharInfoReqBit.BUFF,
		//(byte)E_CharInfoReqBit.RESTORE_EXP,
		//(byte)E_CharInfoReqBit.GUILD,
		//(byte)E_CharInfoReqBit.GUILD_BUFF,
		//(byte)E_CharInfoReqBit.GUILD_ALLIANCE,
		//(byte)E_CharInfoReqBit.REPEAT_QUEST,
		//};

		///// <summary> 캐릭터 아이템 정보 받아오기용 플래그값 </summary>
		//static readonly byte[] CharacterItemInfoBits = new byte[]
		//{
		//(byte)E_CharInfoReqBit.STORAGE,
		//(byte)E_CharInfoReqBit.ACCOUNT_STACK,
		//(byte)E_CharInfoReqBit.EQUIP,
		//(byte)E_CharInfoReqBit.STACK,
		//(byte)E_CharInfoReqBit.MARK,
		//(byte)E_CharInfoReqBit.CHANGE,
		//(byte)E_CharInfoReqBit.CHANGE_ENCHANT,
		//(byte)E_CharInfoReqBit.CHANGE_GACHA_KEEP,
		//(byte)E_CharInfoReqBit.PET,
		//(byte)E_CharInfoReqBit.PET_ENCHANT,
		//(byte)E_CharInfoReqBit.PET_GACHA_KEEP,
		//(byte)E_CharInfoReqBit.SKILL_BOOK,
		//(byte)E_CharInfoReqBit.PET_ADVENTURE,
		//(byte)E_CharInfoReqBit.RUNE,
		//};

		///// <summary> 캐릭터 컬렉션 정보 받아오기용 플래그값 </summary>
		//static readonly byte[] CharacterCollectInfoBits = new byte[]
		//{
		//(byte)E_CharInfoReqBit.CHANGE_COLLECT,
		//(byte)E_CharInfoReqBit.ITEM_COLLECT,
		//(byte)E_CharInfoReqBit.PET_COLLECT,
		//};

		//public static void GetCharacterInfos(bool bInitData, ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	GetAllCharInfoBundle(bInitData, UserId, CharId, CharacterInfoBits, onRecvPacket);
		//}

		//public static void GetCharacterItemInfos(bool bInitData, ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	GetAllCharInfoBundle(bInitData, UserId, CharId, CharacterItemInfoBits, onRecvPacket);
		//}

		//public static void GetCharacterCollectInfos(bool bInitData, ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	GetAllCharInfoBundle(bInitData, UserId, CharId, CharacterCollectInfoBits, onRecvPacket);
		//}

		//public static void GetRaidEtcInfos(bool bInitData, ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	GetAllCharInfoBundle(bInitData, UserId, CharId, new byte[] { (byte)E_CharInfoReqBit.RAID_ETC }, onRecvPacket);
		//}

		///// <summary> 시나리오용 캐릭터 정보 요청 </summary>
		//public static void GetCharacterScenarioInfos(bool bInitData, ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetAllCharInfoBundle> onRecvPacket)
		//{
		//	GetAllCharInfoBundle(bInitData, UserId, CharId, new byte[] { (byte)E_CharInfoReqBit.CHARACTER }, onRecvPacket);
		//}

		//public static void GetCharacterEquipItemInfos(ulong UserId, ulong CharId, System.Action<ReceiveFBMessage, ResGetEquipItemSlotInfoBundle> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, WebNet.Code.COMMON_GET_EQUIP_ITEM_SLOT_INFO_BUNDLE);

		//	builder.Clear();

		//	var offset = ReqGetEquipItemSlotInfoBundle.CreateReqGetEquipItemSlotInfoBundle(builder, UserId, CharId, NetData.ConnectedServerId);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetEquipItemSlotInfoBundle>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetEquipItemSlotInfoBundle recvMsgPacket = recvPacket.GetResMsg<ResGetEquipItemSlotInfoBundle>();

		//		List<ItemEquipment> equipList = new List<ItemEquipment>();

		//		for (int i = 0; i < recvMsgPacket.EquipItemLength; i++)
		//			equipList.Add(recvMsgPacket.EquipItem(i).Value);

		//		NetData.Instance.AddItemList(UserId, CharId, null, null, equipList);

		//		List<uint> gainSkillList = new List<uint>();

		//		for (int i = 0; i < recvMsgPacket.SkillBookLength; i++)
		//			gainSkillList.Add(recvMsgPacket.SkillBook(i).Value.SkillTid);

		//		NetData.Instance.SetGainSkills(UserId, CharId, gainSkillList);

		//		List<Mark> markList = new List<Mark>();

		//		for (int i = 0; i < recvMsgPacket.MarksLength; i++)
		//			markList.Add(recvMsgPacket.Marks(i).Value);

		//		NetData.Instance.AddMarkList(UserId, CharId, markList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//#endregion

		//#region GuildShop
		//public static void BuyGuildShop(ulong GuildId, uint BuffTid, bool bAutoBuy, System.Action<ReceiveFBMessage, ResGuildBuyBuff> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_BUY_BUFF);

		//	builder.Clear();
		//	var offset = ReqGuildBuyBuff.CreateReqGuildBuyBuff(builder, GuildId, BuffTid, bAutoBuy);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildBuyBuff>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		if ((ERROR)recvPacket.BaseError_code == ERROR.NO_ERROR)
		//		{
		//			ResGuildBuyBuff recvPacketMsg = recvPacket.GetResMsg<ResGuildBuyBuff>();

		//			NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvPacketMsg.GuildInfo.Value);
		//			NetData.Instance.AddGuildBuff(NetData.UserID, NetData.CharID, recvPacketMsg.GuildBuff.Value);
		//			onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//		}
		//		else
		//			onRecvPacket?.Invoke(recvPacket, default);
		//	});
		//}

		//public static void ToggleAutoBuffBuy(ulong GuildId, uint AbilityTid, bool bAutoBuy, System.Action<ReceiveFBMessage, ResGuildAutoBuyBuffToggle> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_AUTO_BUY_BUFF_TOGGLE);

		//	builder.Clear();

		//	var offset = ReqGuildAutoBuyBuffToggle.CreateReqGuildAutoBuyBuffToggle(builder, GuildId, AbilityTid, bAutoBuy);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAutoBuyBuffToggle>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAutoBuyBuffToggle recvPacketMsg = recvPacket.GetResMsg<ResGuildAutoBuyBuffToggle>();

		//		NetData.Instance.AddGuildBuff(NetData.UserID, NetData.CharID, recvPacketMsg.GuildBuff.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}
		//#endregion

		//#region AllianceChat
		//public static void CreateAllianceChat(ulong GuildId, System.Action<ReceiveFBMessage, ResGuildAllianceCreateChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_CREATE_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceCreateChat.CreateReqGuildAllianceCreateChat(builder, GuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceCreateChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceCreateChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceCreateChat>();

		//		NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvPacketMsg.GuildInfo.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void InviteAllianceChat(ulong GuildId, ulong InviteGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceInviteRequestChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_INVITE_REQUEST_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceInviteRequestChat.CreateReqGuildAllianceInviteRequestChat(builder, GuildId, InviteGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceInviteRequestChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceInviteRequestChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceInviteRequestChat>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void AcceptInviteAllianceChat(ulong GuildId, System.Action<ReceiveFBMessage, ResGuildAllianceInviteAcceptChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_INVITE_ACCEPT_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceInviteAcceptChat.CreateReqGuildAllianceInviteAcceptChat(builder, GuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceInviteAcceptChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceInviteAcceptChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceInviteAcceptChat>();

		//		NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvPacketMsg.GuildInfo.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void OutAllianceChat(ulong GuildId, ulong ChatId, E_GuildAllianceChatGrade ChatGrade, System.Action<ReceiveFBMessage, ResGuildAllianceLeaveChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_LEAVE_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceLeaveChat.CreateReqGuildAllianceLeaveChat(builder, GuildId, ChatId, ChatGrade);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceLeaveChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceLeaveChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceLeaveChat>();

		//		NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvPacketMsg.GuildInfo.Value);
		//		NetData.Instance.ClearAllianceChatData(NetData.UserID, NetData.CharID);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void ChangeMasterAllianceChat(ulong GuildId, ulong ChangeGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceChangeMaster> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_CHANGE_MASTER);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceChangeMaster.CreateReqGuildAllianceChangeMaster(builder, GuildId, ChangeGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceChangeMaster>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceChangeMaster recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceChangeMaster>();

		//		NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvPacketMsg.GuildInfo.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void CancelRequestAllianceChat(ulong GuildId, ulong CancelGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceInviteCancelChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_INVITE_CANCEL_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceInviteCancelChat.CreateReqGuildAllianceInviteCancelChat(builder, GuildId, CancelGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceChangeMaster>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceInviteCancelChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceInviteCancelChat>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void RejectRequestAllianceChat(ulong GuildId, ulong CancelGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceRejectChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_REJECT_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceRejectChat.CreateReqGuildAllianceRejectChat(builder, GuildId, CancelGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceRejectChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceRejectChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceRejectChat>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void AcceptRequestAllianceChat(ulong GuildId, ulong AcceptGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceAcceptChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_ACCEPT_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceAcceptChat.CreateReqGuildAllianceAcceptChat(builder, GuildId, AcceptGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceAcceptChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceAcceptChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceAcceptChat>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void KickAllianceChat(ulong GuildId, ulong KickGuildId, System.Action<ReceiveFBMessage, ResGuildAllianceBanChat> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GUILD_ALLIANCE_BAN_CHAT);

		//	builder.Clear();

		//	var offset = ReqGuildAllianceBanChat.CreateReqGuildAllianceBanChat(builder, GuildId, KickGuildId);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGuildAllianceBanChat>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGuildAllianceBanChat recvPacketMsg = recvPacket.GetResMsg<ResGuildAllianceBanChat>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}
		//#endregion

		#region Rank
		public void REQ_GetExpRankList(uint ClassType, System.Action<ZWebRecvPacket, ResGetExpRankList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetExpRankList.CreateReqGetExpRankList(mBuilder, ClassType);

			var reqPacket = ZWebPacket.Create<ReqGetExpRankList>(this, Code.GS_GET_EXP_RANK_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetExpRankList recvPacketMsg = recvPacket.Get<ResGetExpRankList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_GetPKRankList(System.Action<ZWebRecvPacket, ResGetPkRankList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetPkRankList.StartReqGetPkRankList(mBuilder);
			var offset = ReqGetPkRankList.EndReqGetPkRankList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetPkRankList>(this, Code.GS_GET_PK_RANK_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetPkRankList recvPacketMsg = recvPacket.Get<ResGetPkRankList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}
		#endregion

		#region PVPHistory
		public void REQ_GetPVPHistory(System.Action<ZWebRecvPacket, ResGetPkLogList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetPkLogList.StartReqGetPkLogList(mBuilder);
			var offset = ReqGetPkLogList.EndReqGetPkLogList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetPkLogList>(this, Code.GS_GET_PK_LOG_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetPkLogList recvPacketMsg = recvPacket.Get<ResGetPkLogList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		public void REQ_SendPvpSneer(ulong PkID, ulong UseItemId, System.Action<ZWebRecvPacket, ResPkSneer> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqPkSneer.CreateReqPkSneer(mBuilder, PkID, UseItemId);

			var reqPacket = ZWebPacket.Create<ReqPkSneer>(this, Code.GS_PK_SNEER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResPkSneer recvPacketMsg = recvPacket.Get<ResPkSneer>();

				if (recvPacketMsg.ResultAccountStackItem != null)
					Me.CurCharData.AddItem(recvPacketMsg.ResultAccountStackItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}
		#endregion

		//#region Coupon
		//public static void RegistCoupon(string CouponCode, System.Action<ReceiveFBMessage, ResRegistCoupon> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_REGIST_COUPON);

		//	builder.Clear();

		//	var offset = ReqRegistCoupon.CreateReqRegistCoupon(builder, builder.CreateString(CouponCode));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqRegistCoupon>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResRegistCoupon recvPacketMsg = recvPacket.GetResMsg<ResRegistCoupon>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}
		//#endregion

		#region Trade
		/// <summary> 거래소 시세 확인 </summary>
		/// <param name="tabType"></param>
		/// <param name="subTabType"></param>
		/// <param name="charType"></param>
		/// <param name="PageIdx"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_SearchExchangePrice(GameDB.E_TradeTapType tabType, GameDB.E_TradeSubTapType subTabType, GameDB.E_CharacterType charType, uint PageIdx, uint ItemGroupId, System.Action<ZWebRecvPacket, ResExchangeSearchPrice> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
            var offset = ReqExchangeSearchPrice.CreateReqExchangeSearchPrice(mBuilder, (uint)tabType, (uint)subTabType, (uint)charType, PageIdx, ItemGroupId);

            var reqPacket = ZWebPacket.Create<ReqExchangeSearchPrice>(this, Code.GS_EXCHANGE_SEARCH_PRICE, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResExchangeSearchPrice recvPacketMsg = recvPacket.Get<ResExchangeSearchPrice>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

		/// <summary>거래소 시세 확인</summary>
		/// <param name="GroupIdx"></param>
		/// <param name="PageIdx"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_SearchExchangePrice(uint GroupIdx, uint PageIdx, System.Action<ZWebRecvPacket, ResExchangeSearchPrice> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeSearchPrice.CreateReqExchangeSearchPrice(mBuilder, 0, 0, 0, PageIdx, GroupIdx);

			var reqPacket = ZWebPacket.Create<ReqExchangeSearchPrice>(this, Code.GS_EXCHANGE_SEARCH_PRICE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeSearchPrice recvPacketMsg = recvPacket.Get<ResExchangeSearchPrice>();

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 아이템 검색 </summary>
		/// <param name="ItemGroupId"></param>
		/// <param name="PageIdx"></param>
		/// <param name="sortType"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_SearchExchangeList(uint[] ItemTid, uint PageIdx, E_ExchangeSortType sortType, uint OptionCnt, System.Action<ZWebRecvPacket, ResExchangeSearchItem> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var stepid = ReqExchangeSearchItem.CreateItemTidsVector(mBuilder, ItemTid);
			var offset = ReqExchangeSearchItem.CreateReqExchangeSearchItem(mBuilder, PageIdx, stepid, sortType, OptionCnt);

			var reqPacket = ZWebPacket.Create<ReqExchangeSearchItem>(this, Code.GS_EXCHANGE_SEARCH_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeSearchItem recvPacketMsg = recvPacket.Get<ResExchangeSearchItem>();

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 아이템 구매 </summary>
		/// <param name="exhcangeData"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_BuyExchangeItem(ExchangeItemData exhcangeData, System.Action<ZWebRecvPacket, ResExchangeBuy> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeBuy.CreateReqExchangeBuy(mBuilder, ZGameManager.Instance.GetMarketType(), exhcangeData.ExchangeID, exhcangeData.ItemID, exhcangeData.ItemTId);

			var reqPacket = ZWebPacket.Create<ReqExchangeBuy>(this, Code.GS_EXCHANGE_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				if ((ERROR)recvPacket.ErrCode != ERROR.NO_ERROR)
				{
					onRecvPacket?.Invoke(recvPacket, default);
					return;
				}

				ResExchangeBuy recvPacketMsg = recvPacket.Get<ResExchangeBuy>();

				// Cash는 따로 저장
				Me.CurUserData.SetCash(recvPacketMsg.ResultCashCoinBalance);
				if (recvPacketMsg.ResultStackItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultStackItem.Value);
				if (recvPacketMsg.ResultEquipItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultEquipItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 아이템 구매</summary>
		/// <param name="exhcangeData"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_BuyExchangeItem(E_MarketType marketType, ExchangeItem exhcangeData, System.Action<ZWebRecvPacket, ResExchangeBuy> onRecvPacket, PacketErrorCBDelegate _onError = null)

		{
			var offset = ReqExchangeBuy.CreateReqExchangeBuy(mBuilder, marketType, exhcangeData.ExchangeId, exhcangeData.ItemId, exhcangeData.ItemTid);

			var reqPacket = ZWebPacket.Create<ReqExchangeBuy>(this, Code.GS_EXCHANGE_BUY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				if ((ERROR)recvPacket.ErrCode != ERROR.NO_ERROR)
				{
					onRecvPacket?.Invoke(recvPacket, default);
					return;
				}

				ResExchangeBuy recvPacketMsg = recvPacket.Get<ResExchangeBuy>();

				Me.CurUserData.SetCash(recvPacketMsg.ResultCashCoinBalance);
				if (recvPacketMsg.ResultStackItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultStackItem.Value);
				if (recvPacketMsg.ResultEquipItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultEquipItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 쪽지 리스트 요청 </summary>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_GetExchangeMessageList(System.Action<ZWebRecvPacket, ResGetExchangeMessageList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetExchangeMessageList.StartReqGetExchangeMessageList(mBuilder);
			var offset = ReqGetExchangeMessageList.EndReqGetExchangeMessageList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetExchangeMessageList>(this, Code.GS_GET_EXCHANGE_MESSAGE_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetExchangeMessageList recvPacketMsg = recvPacket.Get<ResGetExchangeMessageList>();

				Me.CurCharData.ClearExchangeMessageList();

				List<MessageInfo> messageinfos = new List<MessageInfo>();

				for (int i = 0; i < recvPacketMsg.MessageInfoListLength; i++)
					messageinfos.Add(recvPacketMsg.MessageInfoList(i).Value);

				Me.CurCharData.AddExchangeMessageList(messageinfos);
				Me.CurCharData.SetExchangeMessageRefreshTime(recvPacketMsg.MessageRefreshDt);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 쪽지 읽기</summary>
		/// <param name="messageIdx"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_ReadExchangeMessage(ulong messageIdx, System.Action<ZWebRecvPacket, ResReadExchangeMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqReadExchangeMessage.CreateReqReadExchangeMessage(mBuilder, messageIdx);

			var reqPacket = ZWebPacket.Create<ReqReadExchangeMessage>(this, Code.GS_READ_EXCHANGE_MESSAGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResReadExchangeMessage recvMsgPacket = recvPacket.Get<ResReadExchangeMessage>();
				Me.CurCharData.ReadExchangeMessage(recvMsgPacket.ReadMessageIdx);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 거래소 쪽지 삭제</summary>
		/// <param name="messageIdx"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_DeleteExchangeMessage(ulong messageIdx, System.Action<ZWebRecvPacket, ResDelExchangeMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			List<ulong> datas = new List<ulong>();

			datas.Add(messageIdx);

			var offset = ReqDelExchangeMessage.CreateReqDelExchangeMessage(mBuilder, ReqDelExchangeMessage.CreateDelMessageIdxVector(mBuilder, datas.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqDelExchangeMessage>(this, Code.GS_DEL_EXCHANGE_MESSAGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResDelExchangeMessage recvMsgPacket = recvPacket.Get<ResDelExchangeMessage>();

				List<ulong> delmessageidxs = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.DelMessageIdxLength; i++)
					delmessageidxs.Add(recvMsgPacket.DelMessageIdx(i));

				Me.CurCharData.RemoveExchangeMessageList(delmessageidxs);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary>거래소 쪽지 삭제 </summary>
		/// <param name="messages"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_DeleteExchangeMessages(List<MessageData> messages, System.Action<ZWebRecvPacket, ResDelExchangeMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			List<ulong> datas = new List<ulong>();

			for (int i = 0; i < messages.Count; i++)
				datas.Add(messages[i].MessageIdx);

			var offset = ReqDelExchangeMessage.CreateReqDelExchangeMessage(mBuilder, ReqDelExchangeMessage.CreateDelMessageIdxVector(mBuilder, datas.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqDelExchangeMessage>(this, Code.GS_DEL_EXCHANGE_MESSAGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResDelExchangeMessage recvMsgPacket = recvPacket.Get<ResDelExchangeMessage>();

				List<ulong> delmessageidxs = new List<ulong>();
				for (int i = 0; i < recvMsgPacket.DelMessageIdxLength; i++)
					delmessageidxs.Add(recvMsgPacket.DelMessageIdx(i));

				Me.CurCharData.RemoveExchangeMessageList(delmessageidxs);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 거래소 쪽지 보내기 </summary>
		/// <param name="UserId"></param>
		/// <param name="Title"></param>
		/// <param name="Msg"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_SendExchangeMessage(ulong UserId, string Title, string Msg, System.Action<ZWebRecvPacket, ResSendExchangeMessage> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqSendExchangeMessage.CreateReqSendExchangeMessage(mBuilder, UserId, mBuilder.CreateString(Title), mBuilder.CreateString(Msg));

			var reqPacket = ZWebPacket.Create<ReqSendExchangeMessage>(this, Code.GS_SEND_EXCHANGE_MESSAGE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSendExchangeMessage recvPacketMsg = recvPacket.Get<ResSendExchangeMessage>();

				Me.CurUserData.SetNormalMsgSendCnt(recvPacketMsg.NormalMsgSendCnt);

				for (int i = 0; i < recvPacketMsg.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultAccountStackItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 판매완료 리스트 </summary>
		/// <param name="state"></param>
		/// <param name="PageIdx"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void GetExchangeSoldOutList(E_ExchangeTransactionState state, uint PageIdx, System.Action<ZWebRecvPacket, ResExchangeSoldOutList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeSoldOutList.CreateReqExchangeSoldOutList(mBuilder, state, PageIdx);

			var reqPacket = ZWebPacket.Create<ReqExchangeSoldOutList>(this, Code.GS_EXCHANGE_SOLD_OUT_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeSoldOutList recvPacketMsg = recvPacket.Get<ResExchangeSoldOutList>();

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary>거래소 정산 금액 받기</summary>
		/// <param name="takeIdList"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_ExchangeTakeMoney(List<ulong> takeIdList, System.Action<ZWebRecvPacket, ResExchangeTakeMoney> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeTakeMoney.CreateReqExchangeTakeMoney(mBuilder, ZGameManager.Instance.GetMarketType(), ReqExchangeTakeMoney.CreateTransactionIdsVector(mBuilder, takeIdList.ToArray()));

			var reqPacket = ZWebPacket.Create<ReqExchangeTakeMoney>(this, Code.GS_EXCHANGE_TAKE_MONEY, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeTakeMoney recvPacketMsg = recvPacket.Get<ResExchangeTakeMoney>();

				Me.CurUserData.SetCash(recvPacketMsg.ResultCashCoinBalance);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 판매중인 리스트</summary>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_GetExchangeSellList(System.Action<ZWebRecvPacket, ResExchangeSellList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqExchangeSellList.StartReqExchangeSellList(mBuilder);
			var offset = ReqExchangeSellList.EndReqExchangeSellList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqExchangeSellList>(this, Code.GS_EXCHANGE_SELL_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeSellList recvPacketMsg = recvPacket.Get<ResExchangeSellList>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary> 거래소 아이템 상세 정보 </summary>
		/// <param name="ItemTid"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_GetExchangePriceInfo(uint ItemTid, System.Action<ZWebRecvPacket, ResExchangeItemDetailInfo> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var tabledata = DBItem.GetItem(ItemTid);
			if (tabledata == null)
			{
				return;
			}

			var offset = ReqExchangeItemDetailInfo.CreateReqExchangeItemDetailInfo(mBuilder, (uint)tabledata.TradeTapType, (uint)tabledata.TradeSubTapType, (uint)tabledata.UseCharacterType, tabledata.GroupID, ItemTid);

			var reqPacket = ZWebPacket.Create<ReqExchangeItemDetailInfo>(this, Code.GS_EXCHANGE_ITEM_DETAIL_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeItemDetailInfo recvPacketMsg = recvPacket.Get<ResExchangeItemDetailInfo>();
				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary>거래소 아이템 판매</summary>
		/// <param name="SellItemId"></param>
		/// <param name="SellItemTid"></param>
		/// <param name="SellItemCnt"></param>
		/// <param name="SellPrice"></param>
		/// <param name="TaxItemId"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_ExchangeSellRegist(ulong SellItemId, uint SellItemTid, uint SellItemCnt, uint SellPrice, ulong TaxItemId, System.Action<ZWebRecvPacket, ResExchangeSell> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeSell.CreateReqExchangeSell(mBuilder, SellItemId, SellItemTid, SellItemCnt, SellPrice, TaxItemId);

			var reqPacket = ZWebPacket.Create<ReqExchangeSell>(this, Code.GS_EXCHANGE_SELL, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeSell recvPacketMsg = recvPacket.Get<ResExchangeSell>();

				if (DBItem.IsEquipItem(SellItemTid))
				{
					Me.CurCharData.RemoveItem(NetItemType.TYPE_EQUIP, recvPacketMsg.SellItemId);
				}
				else
				{
					Me.CurCharData.RemoveItem(NetItemType.TYPE_ACCOUNT_STACK, recvPacketMsg.SellItemId);
					Me.CurCharData.RemoveItem(NetItemType.TYPE_STACK, recvPacketMsg.SellItemId);
				}

				if (recvPacketMsg.ResultAccountStackItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultAccountStackItem.Value);
				if (recvPacketMsg.ResultStackItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultStackItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}

		/// <summary>거래소 판매 안된 아이템 회수</summary>
		/// <param name="ExchangeId"></param>
		/// <param name="SellItemId"></param>
		/// <param name="SellItemTid"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_ExchangeSellCancel(ulong ExchangeId, ulong SellItemId, uint SellItemTid, System.Action<ZWebRecvPacket, ResExchangeWithDraw> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqExchangeWithDraw.CreateReqExchangeWithDraw(mBuilder, ExchangeId, SellItemId, SellItemTid);

			var reqPacket = ZWebPacket.Create<ReqExchangeWithDraw>(this, Code.GS_EXCHANGE_WITHDRAW, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResExchangeWithDraw recvPacketMsg = recvPacket.Get<ResExchangeWithDraw>();

				if (recvPacketMsg.ResultStackItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultStackItem.Value);
				if (recvPacketMsg.ResultEquipItem != null)
					Me.CurCharData.AddItemList(recvPacketMsg.ResultEquipItem.Value);

				onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
			}, _onError);
		}
		#endregion

		#region 요리
		/// <summary> 요리 조합(레시피X)</summary>
		/// <param name="MatItems"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_MakeCookItemNoRecipe(List<List<ZItem>> MatItems, System.Action<ZWebRecvPacket, ResMakeCookItemNoRecipe> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var makeslist = new Offset<MaterialItem>[MatItems.Count];
			for (int i = 0; i < MatItems.Count; i++)
			{
				for (int j = 0; j < MatItems[i].Count; j++)
					makeslist[i] = MaterialItem.CreateMaterialItem(mBuilder, MatItems[i][j].item_id, MatItems[i][j].item_tid, (uint)MatItems[i][j].cnt);
			}

			var offset = ReqMakeCookItemNoRecipe.CreateReqMakeCookItemNoRecipe(mBuilder, ReqMakeCookItemNoRecipe.CreateMakeMaterialItemsVector(mBuilder, makeslist));

			var reqPacket = ZWebPacket.Create<ReqMakeCookItemNoRecipe>(this, Code.GS_MAKE_COOK_ITEM_NO_RECIPE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResMakeCookItemNoRecipe recvMsgPacket = recvPacket.Get<ResMakeCookItemNoRecipe>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}

		/// <summary> 요리 조합(레시피O)</summary>
		/// <param name="MatItems"></param>
		/// <param name="onRecvPacket"></param>
		/// <param name="_onError"></param>
		public void REQ_MakeCookItem(uint CookTid, uint UseItemTid, uint Cnt, List<List<ZItem>> MatItems, System.Action<ZWebRecvPacket, ResMakeCookItem> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var makeslist = new Offset<MaterialItem>[MatItems.Count];
			for (int i = 0; i < MatItems.Count; i++)
			{
				for (int j = 0; j < MatItems[i].Count; j++)
					makeslist[i] = MaterialItem.CreateMaterialItem(mBuilder, MatItems[i][j].item_id, MatItems[i][j].item_tid, (uint)MatItems[i][j].cnt);
			}

			var offset = ReqMakeCookItem.CreateReqMakeCookItem(mBuilder, CookTid, UseItemTid, Cnt, ReqMakeCookItem.CreateMakeMaterialItemsVector(mBuilder, makeslist));

			var reqPacket = ZWebPacket.Create<ReqMakeCookItem>(this, Code.GS_MAKE_COOK_ITEM, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResMakeCookItem recvMsgPacket = recvPacket.Get<ResMakeCookItem>();

				for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultAccountStackItems(i).Value);
				for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
					Me.CurCharData.AddItemList(recvMsgPacket.ResultStackItems(i).Value);

				if (recvMsgPacket.CookHistory != null)
					Me.CurCharData.AddCookRecipeData(recvMsgPacket.CookHistory);

				onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
			}, _onError);
		}
		#endregion

		//#region PrivateTrade
		//public static void PrivateTradeSell(ulong SellItemId, uint SellItemTid,uint SellItemCnt,uint SellItemTotalPrice,string Password,string BuyCharNick, System.Action<ReceiveFBMessage, ResTradeSell> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_SELL);

		//	builder.Clear();

		//	var offset = ReqTradeSell.CreateReqTradeSell(builder, SellItemId, SellItemTid, SellItemCnt, SellItemTotalPrice, builder.CreateString(Password),builder.CreateString(BuyCharNick));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeSell>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeSell recvPacketMsg = recvPacket.GetResMsg<ResTradeSell>();

		//		if (DBItem.IsEquipItem(recvPacketMsg.SellItemTid))
		//		{
		//			NetData.Instance.RemoveItem(NetData.UserID, NetData.CharID, NetItemType.TYPE_EQUIP, recvPacketMsg.SellItemId);
		//		}
		//		else
		//		{
		//			NetData.Instance.RemoveItem(NetData.UserID, NetData.CharID, NetItemType.TYPE_ACCOUNT_STACK, recvPacketMsg.SellItemId);
		//			NetData.Instance.RemoveItem(NetData.UserID, NetData.CharID, NetItemType.TYPE_STACK, recvPacketMsg.SellItemId);
		//		}

		//		if (recvPacketMsg.ResultAccountStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultAccountStackItem.Value);
		//		if(recvPacketMsg.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultStackItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void PrivateTradeBuy(ulong TradeId, ulong BuyItemId, uint BuyItemTid, string Password, System.Action<ReceiveFBMessage, ResTradeBuy> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_BUY);

		//	builder.Clear();

		//	var offset = ReqTradeBuy.CreateReqTradeBuy(builder, TradeId, BuyItemId, BuyItemTid, builder.CreateString(Password));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeBuy>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeBuy recvPacketMsg = recvPacket.GetResMsg<ResTradeBuy>();

		//		if (recvPacketMsg.ResultAccountStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultAccountStackItem.Value);
		//		if (recvPacketMsg.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultStackItem.Value);
		//		if (recvPacketMsg.ResultEquipItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultEquipItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void PrivateTradeWithDraw(ulong TradeId, ulong ItemId, uint ItemTid, System.Action<ReceiveFBMessage, ResTradeWithDraw> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_WITHDRAW);

		//	builder.Clear();

		//	var offset = ReqTradeWithDraw.CreateReqTradeWithDraw(builder, TradeId, ItemId, ItemTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeWithDraw>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeWithDraw recvPacketMsg = recvPacket.GetResMsg<ResTradeWithDraw>();

		//		if (recvPacketMsg.ResultEquipItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultEquipItem.Value);
		//		if (recvPacketMsg.ResultStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultStackItem.Value);
		//		if (recvPacketMsg.ResultEquipItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultEquipItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void PrivateTradeSellList(E_TradeState state, uint PageIndex, System.Action<ReceiveFBMessage, ResTradeSellList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_SELL_LIST);

		//	builder.Clear();

		//	var offset = ReqTradeSellList.CreateReqTradeSellList(builder, PageIndex, state);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeSellList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeSellList recvPacketMsg = recvPacket.GetResMsg<ResTradeSellList>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void PrivateTradeBuyList(System.Action<ReceiveFBMessage, ResTradeBuyList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_BUY_LIST);

		//	builder.Clear();

		//	ReqTradeBuyList.StartReqTradeBuyList(builder);
		//	var offset = ReqTradeBuyList.EndReqTradeBuyList(builder);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeBuyList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeBuyList recvPacketMsg = recvPacket.GetResMsg<ResTradeBuyList>();

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}

		//public static void PrivateTradeTakeMoney(List<ulong> TradeIds, System.Action<ReceiveFBMessage, ResTradeTakeMoney> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TRADE_TAKE_MONEY);

		//	builder.Clear();

		//	var offset = ReqTradeTakeMoney.CreateReqTradeTakeMoney(builder, ReqTradeTakeMoney.CreateTradeIdsVector(builder, TradeIds.ToArray()));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTradeTakeMoney>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResTradeTakeMoney recvPacketMsg = recvPacket.GetResMsg<ResTradeTakeMoney>();

		//		if (recvPacketMsg.ResultAccountStackItem != null)
		//			NetData.Instance.AddItem(NetData.UserID, NetData.CharID, recvPacketMsg.ResultAccountStackItem.Value);

		//		onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
		//	});
		//}
		//#endregion

		//#region Bot

		///// <summary>클라용 PC봇 리스트 요청한다.</summary>
		//public static void GetBotList(uint _stageTid, uint botCount, System.Action<ReceiveFBMessage, ResGetBotList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_GET_BOT_LIST);

		//	builder.Clear();

		//	var offset = ReqGetBotList.CreateReqGetBotList(builder, _stageTid, botCount);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqGetBotList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResGetBotList recvMsgPacket = recvPacket.GetResMsg<ResGetBotList>();

		//		int count = recvMsgPacket.BotInfosLength;
		//		for (int i = 0; i < count; ++i)
		//		{
		//			if (null == recvMsgPacket.BotInfos(i))
		//				continue;

		//			var botInfo = recvMsgPacket.BotInfos(i).Value;

		//			AddNetDataOfBot(ref botInfo);
		//		}

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary>체인지할 봇 정보 보내고, 새로운 봇으로 받도록 한다.</summary>
		//public static void GetChangeBotList(uint _stageTid, ulong[] botCharIds, System.Action<ReceiveFBMessage, ResChangeBotList> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CHANGE_BOT_LIST);

		//	builder.Clear();

		//	var offset = ReqChangeBotList.CreateReqChangeBotList(builder, _stageTid, ReqChangeBotList.CreateChangeBotIdsVector(builder, botCharIds));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqChangeBotList>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResChangeBotList recvMsgPacket = recvPacket.GetResMsg<ResChangeBotList>();

		//		int count = recvMsgPacket.BotInfosLength;
		//		for (int i = 0; i < count; ++i)
		//		{
		//			if (null == recvMsgPacket.BotInfos(i))
		//				continue;

		//			var botInfo = recvMsgPacket.BotInfos(i).Value;

		//			AddNetDataOfBot(ref botInfo);
		//		}

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		///// <summary>NetData에 봇 정보 저장</summary>
		//public static void AddNetDataOfBot(ref BotInfo botInfo)
		//{
		//	ulong userId, charId;
		//	NetUserData userData = null;
		//	if (botInfo.Character.HasValue)
		//	{
		//		userId = botInfo.Character.Value.UserId;
		//		charId = botInfo.Character.Value.CharId;

		//		if (!NetData.Instance.GetUser(userId, out userData))
		//		{
		//			userData = NetData.Instance.AddUserData(userId);
		//		}

		//		userData.RemoveChar(charId);
		//		if (null == NetData.Instance.GetCharacter(userId, charId))
		//		{
		//			NetData.Instance.AddChar(userId, botInfo.Character.Value);
		//		}
		//	}
		//	else
		//	{
		//		return;
		//	}

		//	//==== 버프
		//	List<Buff> buffList = new List<Buff>();
		//	for (int buffIdx = 0; buffIdx < botInfo.BuffInfoLength; buffIdx++)
		//		buffList.Add(botInfo.BuffInfo(buffIdx).Value);

		//	userData.ClearBuffList(charId);
		//	userData.AddBuff(charId, buffList);

		//	//==== 장비
		//	List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//	for (int itemIdx = 0; itemIdx < botInfo.EquipItemLength; itemIdx++)
		//		invenEquipList.Add(botInfo.EquipItem(itemIdx).Value);
		//	userData.ClearInvenList(charId);
		//	userData.AddItemList(charId, null, null, invenEquipList);

		//	//==== 변신
		//	List<Change> changeList = new List<Change>();

		//	for (int changeIdx = 0; changeIdx < botInfo.ChangeItemLength; changeIdx++)
		//		changeList.Add(botInfo.ChangeItem(changeIdx).Value);

		//	userData.ClearChangeList(charId);
		//	userData.AddChangeList(charId, changeList);

		//	//==== 펫
		//	List<Pet> petList = new List<Pet>();

		//	for (int petIdx = 0; petIdx < botInfo.PetItemLength; petIdx++)
		//		petList.Add(botInfo.PetItem(petIdx).Value);

		//	userData.ClearPetList(charId);
		//	userData.AddPetList(charId, petList);

		//	////==== 문양
		//	//List<Mark> markList = new List<Mark>();
		//	//for (int markIdx = 0; markIdx < botInfo.MarkItemLength; markIdx++)
		//	//	markList.Add(botInfo.MarkItem(markIdx).Value);

		//	//userData.ClearMarkList(charId);
		//	//userData.AddMarkList(charId, markList);

		//	////==== 컬렉션들
		//	//List<Collect> itemCollectList = new List<Collect>();

		//	//for (int colIdx1 = 0; colIdx1 < botInfo.ItemCollectLength; colIdx1++)
		//	//	itemCollectList.Add(botInfo.ItemCollect(colIdx1).Value);

		//	//userData.ClearItemCollectList(charId);
		//	//userData.AddItemCollectList(charId, itemCollectList);

		//	//List<Collect> changeCollectList = new List<Collect>();
		//	////change
		//	//for (int colIdx2 = 0; colIdx2 < botInfo.ChangeCollectLength; colIdx2++)
		//	//	changeCollectList.Add(botInfo.ChangeCollect(colIdx2).Value);

		//	//userData.ClearChangeCollectList(charId);
		//	//userData.AddChangeCollectList(charId, changeCollectList);

		//	//List<Collect> petCollectList = new List<Collect>();
		//	////pet
		//	//for (int colIdx3 = 0; colIdx3 < botInfo.PetCollectLength; colIdx3++)
		//	//	petCollectList.Add(botInfo.PetCollect(colIdx3).Value);

		//	//userData.ClearPetCollectList(charId);
		//	//userData.AddPetCollectList(charId, petCollectList);
		//}

		//public static void GetGachaBotItems(uint ItemTid, uint Cnt, System.Action<ReceiveFBMessage, ResBotGetItem> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_BOT_GET_ITEM);

		//	builder.Clear();

		//	var offset = ReqBotGetItem.CreateReqBotGetItem(builder, ItemTid, Cnt);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqBotGetItem>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ResBotGetItem recvMsgPacket = recvPacket.GetResMsg<ResBotGetItem>();

		//		List<ItemEquipment> invenEquipList = new List<ItemEquipment>();
		//		List<ItemStack> invenStackList = new List<ItemStack>();
		//		List<AccountItemStack> invenAccountStackList = new List<AccountItemStack>();

		//		for (int i = 0; i < recvMsgPacket.ResultAccountStackItemsLength; i++)
		//			invenAccountStackList.Add(recvMsgPacket.ResultAccountStackItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultEquipItemsLength; i++)
		//			invenEquipList.Add(recvMsgPacket.ResultEquipItems(i).Value);
		//		for (int i = 0; i < recvMsgPacket.ResultStackItemsLength; i++)
		//			invenStackList.Add(recvMsgPacket.ResultStackItems(i).Value);

		//		NetData.Instance.AddItemList(NetData.UserID, NetData.CharID, invenAccountStackList, invenStackList, invenEquipList);

		//		onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
		//	});
		//}

		//#endregion

		//#region Log
		//public static void SendTutorialLog(uint tutorialTid, System.Action<ReceiveFBMessage, ResTutorialLog> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TUTORIAL_LOG);

		//	builder.Clear();

		//	var offset = ReqTutorialLog.CreateReqTutorialLog(builder, tutorialTid);
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTutorialLog>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		ZLog.Log("Recv TutorialLog >>>>>> Step : " + tutorialTid);
		//		onRecvPacket?.Invoke(recvPacket, recvPacket.GetResMsg<ResTutorialLog>());
		//	});
		//}

		///// <summary> 아이템 카운트 체크용 </summary>
		//[System.Diagnostics.Conditional("UNITY_ANDROID")]
		//public static void CheckItemCount(ZItem itemInfo)
		//{
		//	switch (itemInfo.netType)
		//	{
		//		case NetItemType.TYPE_ACCOUNT_STACK:
		//			if (itemInfo.cnt > 100000000000)
		//				SendLog($"UserID: {NetData.UserID}, <<AccountStackHeavy>> itemid : {itemInfo.item_id} , itemtid : {itemInfo.item_tid} , cnt : {itemInfo.cnt} , Date : {System.DateTime.Now}");
		//			break;
		//		case NetItemType.TYPE_STACK:
		//			if (itemInfo.cnt > 100000000)
		//				SendLog($"UserID: {NetData.UserID}, <<StackHeavy>> itemid : {itemInfo.item_id} , itemtid : {itemInfo.item_tid} , cnt : {itemInfo.cnt} , Date : {System.DateTime.Now}");
		//			break;
		//	}

		//}

		///// <summary> 서버에 메시지 남기고 싶을때 로깅용 패킷 </summary>
		//[System.Diagnostics.Conditional("UNITY_ANDROID")]
		//public static void SendLog(string logMsg)
		//{
		//	if (PlatformSpecific.IsUnityServer)
		//		return;

		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_CLIENT_CUSTOM_LOG);

		//	builder.Clear();

		//	var offset = ReqClientCustomLog.CreateReqClientCustomLog(builder, builder.CreateString(logMsg)); ;
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqClientCustomLog>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, null);
		//}

		//#endregion

		//#region :: Hack Detection ::

		///// <summary>핵 의심유저 정보 보내기</summary>
		//public static void ReportHackUser(E_HACK_CATEGORY hackType, string payload, System.Action<ReceiveFBMessage, ResHackUser> onRecvPacket)
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_HACK_USER);

		//	builder.Clear();

		//	if (string.IsNullOrEmpty(payload))
		//		payload = string.Empty;

		//	var offset = ReqHackUser.CreateReqHackUser(builder, hackType, builder.CreateString(payload));
		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqHackUser>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//		onRecvPacket?.Invoke(recvPacket, recvPacket.GetResMsg<ResHackUser>());
		//	});
		//}

		//#endregion

		//#region Test
		///// <summary>
		/////  치트
		/////  
		///// cmd : 1001 data : "{level:50}"   -레벨
		///// 
		///// cmd : 1000 data = "[{item_tid:111,cnt:10},]" -우편요청
		///// </summary>
		///// <param name="cmd"></param>
		///// <param name="value"></param>
		///// <param name="onRecvPacket"></param>
		//[System.Diagnostics.Conditional("SHOW_LOG")]
		public void REQ_CheatSend(uint cmd, string value, System.Action<ZWebRecvPacket, ResCheatSendMail> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqCheatSendMail.CreateReqCheatSendMail(mBuilder, cmd, mBuilder.CreateString(value));

			var reqPacket = ZWebPacket.Create<ReqCheatSendMail>(this, Code.GS_CHEAT, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResCheatSendMail resCheatSendMail = recvPacket.Get<ResCheatSendMail>();

				onRecvPacket?.Invoke(recvPacket, resCheatSendMail);
			}, _onError);
		}

		public void REQ_InfinityDungeonClearReward(uint DungeonId, Action<ZWebRecvPacket, ResInfinityDungeonClearReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInfinityDungeonClearReward.CreateReqInfinityDungeonClearReward(mBuilder, DungeonId);

			var reqPacket = ZWebPacket.Create<ReqInfinityDungeonClearReward>(this, Code.GS_INFINITY_DUNGEON_CLEAR_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInfinityDungeonClearReward resClear = recvPacket.Get<ResInfinityDungeonClearReward>();

				Me.CurUserData.InfinityDungeonScheduleId = resClear.InfinityDungeonSeq;
				Me.CurUserData.CurrentInfinityDungeonId = resClear.InfinityDungeonTid;
				Me.CurUserData.LastInfinityDungeonId = resClear.InfinityDungeonLastTid;

				onRecvPacket?.Invoke(recvPacket, resClear);
			}, _onError);
		}

		public void REQ_InfinityDungeonDailyReward(Action<ZWebRecvPacket, ResInfinityDungeonDailyReward> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqInfinityDungeonDailyReward.StartReqInfinityDungeonDailyReward(mBuilder);
			var offset = ReqInfinityDungeonDailyReward.EndReqInfinityDungeonDailyReward(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqInfinityDungeonDailyReward>(this, Code.GS_INFINITY_DUNGEON_DAILY_REWARD, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInfinityDungeonDailyReward resReward = recvPacket.Get<ResInfinityDungeonDailyReward>();

				Me.CurUserData.InfinityDungeonRewardTime = resReward.InfinityDungeonRewardDt;
				Me.CurUserData.LastRewardedStageTid = resReward.InfinityDungeonRewardStageTid;

				if (resReward.RemainItems.HasValue)
				{
					Me.CurCharData.AddRemainItems(resReward.RemainItems.Value);
				}

				onRecvPacket?.Invoke(recvPacket, resReward);
			}, _onError);
		}

		public void REQ_InfinityDungeonReset(Action<ZWebRecvPacket, ResInfinityDungeonReset> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInfinityDungeonReset.CreateReqInfinityDungeonReset(mBuilder, ZGameManager.Instance.GetMarketType());

			var reqPacket = ZWebPacket.Create<ReqInfinityDungeonReset>(this, Code.GS_INFINITY_DUNGEON_RESET, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInfinityDungeonReset resReset = recvPacket.Get<ResInfinityDungeonReset>();
				Me.CurUserData.SetCash(resReset.ResultCashCoinBalance);

				onRecvPacket?.Invoke(recvPacket, resReset);
			}, _onError);
		}

		public void REQ_GetInfinityDungeonSelectBuffList(Action<ZWebRecvPacket, ResGetInfinityDungeonSelectBuffList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetInfinityDungeonSelectBuffList.StartReqGetInfinityDungeonSelectBuffList(mBuilder);
			var offset = ReqGetInfinityDungeonSelectBuffList.EndReqGetInfinityDungeonSelectBuffList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetInfinityDungeonSelectBuffList>(this, Code.GS_GET_INFINITY_DUNGEON_SELECT_BUFF_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetInfinityDungeonSelectBuffList resBuffList = recvPacket.Get<ResGetInfinityDungeonSelectBuffList>();

				onRecvPacket?.Invoke(recvPacket, resBuffList);
			}, _onError);
		}

		public void REQ_InfinityDungeonSelectBuff(uint BuffId, Action<ZWebRecvPacket, ResInfinityDungeonSelectBuff> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInfinityDungeonSelectBuff.CreateReqInfinityDungeonSelectBuff(mBuilder, BuffId);

			var reqPacket = ZWebPacket.Create<ReqInfinityDungeonSelectBuff>(this, Code.GS_INFINITY_DUNGEON_SELECT_BUFF, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInfinityDungeonSelectBuff resBuff = recvPacket.Get<ResInfinityDungeonSelectBuff>();

				if (GameDBManager.Container.InfiBuff_Table_data.TryGetValue(resBuff.InfinityBuffTid, out GameDB.InfiBuff_Table table))
				{
					Me.CurCharData.InfinityTowerContainer.InfinityBuffList.Add(table);
				}

				onRecvPacket?.Invoke(recvPacket, resBuff);
			}, _onError);
		}

		public void REQ_GetInfinityDungeonBuffList(Action<ZWebRecvPacket, ResGetInfinityDungeonBuffList> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			ReqGetInfinityDungeonBuffList.StartReqGetInfinityDungeonBuffList(mBuilder);
			var offset = ReqGetInfinityDungeonBuffList.EndReqGetInfinityDungeonBuffList(mBuilder);

			var reqPacket = ZWebPacket.Create<ReqGetInfinityDungeonBuffList>(this, Code.GS_GET_INFINITY_DUNGEON_BUFF_LIST, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				Me.CurCharData.InfinityTowerContainer.InfinityBuffList.Clear();
				ResGetInfinityDungeonBuffList resBuffList = recvPacket.Get<ResGetInfinityDungeonBuffList>();

				for (int i = 0; i < resBuffList.InfinityBuffTidsLength; i++)
				{
					if (GameDBManager.Container.InfiBuff_Table_data.TryGetValue(resBuffList.InfinityBuffTids(i), out GameDB.InfiBuff_Table table))
					{
						Me.CurCharData.InfinityTowerContainer.InfinityBuffList.Add(table);
					}
				}

				onRecvPacket?.Invoke(recvPacket, resBuffList);
			}, _onError);
		}

		/// <summary> 아이템 자동 분해 옵션 설정 </summary>
		public void REQ_SetItemOption(Action<ZWebRecvPacket, ResSetItemOption> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqSetItemOption.CreateReqSetItemOption(mBuilder, (ushort)ZGameOption.Instance.Auto_Break_Belong_Item);

			var reqPacket = ZWebPacket.Create<ReqSetItemOption>(this, Code.GS_SET_ITEM_OPTION, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResSetItemOption resOption = recvPacket.Get<ResSetItemOption>();

				onRecvPacket?.Invoke(recvPacket, resOption);
			}, _onError);
		}

		/// <summary> 보스전 정보 </summary>
		public void REQ_GetServerBossInfo(uint stageTid, Action<ZWebRecvPacket, ResGetServerBossInfo> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGetServerBossInfo.CreateReqGetServerBossInfo(mBuilder, stageTid);

			var reqPacket = ZWebPacket.Create<ReqGetServerBossInfo>(this, Code.GS_GET_SERVER_BOSS_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGetServerBossInfo resBossInfo = recvPacket.Get<ResGetServerBossInfo>();

				if (resBossInfo.OpenBoss.HasValue)
				{
					//resBossInfo.OpenBoss.Value.EnterAbleEndTsSec		// 입장 가능한 시간
					//resBossInfo.OpenBoss.Value.EnterAbleStartTsSec	// 입장 가능한 시간
					//resBossInfo.OpenBoss.Value.IsEnterAble			// 입장 했던 유저인지 여부
					//resBossInfo.OpenBoss.Value.IsKill					// 보스 킬 여부 (true면 입장 불가)
					//resBossInfo.OpenBoss.Value.KillAbleEndTsSec		// 킬 가능 시간
					//resBossInfo.OpenBoss.Value.KillAbleStartTsSec		// 킬 가능 시간
					//resBossInfo.OpenBoss.Value.RoomExpireTsSec		// 서버 보스 룸 만료 시간
					//resBossInfo.OpenBoss.Value.SpawnTsSec				// 필드 입장 후 보스 스폰 남은 시간
					//resBossInfo.OpenBoss.Value.StageTid
					//for(int i = 0; i < resBossInfo.OpenBoss.Value.TimeSecListLength; i++)
					//{
					//	resBossInfo.OpenBoss.Value.TimeSecList(i);
					//}
				}

				onRecvPacket?.Invoke(recvPacket, resBossInfo);
			}, _onError);
		}

		/// <summary> 보스전 캠프(안전지대) 진입 </summary>
		public void REQ_EnterBossWarCampStage(uint portalTid, uint useItemTid, Action<ZWebRecvPacket, ResInterStageCampEnter> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInterStageCampEnter.CreateReqInterStageCampEnter(mBuilder, portalTid, useItemTid);

			var reqPacket = ZWebPacket.Create<ReqInterStageCampEnter>(this, Code.GS_INTER_STAGE_CAMP_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInterStageCampEnter resCampEnter = recvPacket.Get<ResInterStageCampEnter>();
				//resCampEnter.JoinAddr
				//resCampEnter.StageTid

				onRecvPacket?.Invoke(recvPacket, resCampEnter);
			}, _onError);
		}

		/// <summary> 보스전 필드 진입 </summary>
		public void REQ_EnterBossWarFieldStage(uint portalTid, Action<ZWebRecvPacket, ResInterStageFieldEnter> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqInterStageFieldEnter.CreateReqInterStageFieldEnter(mBuilder, portalTid);

			var reqPacket = ZWebPacket.Create<ReqInterStageFieldEnter>(this, Code.GS_INTER_STAGE_FIELD_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResInterStageFieldEnter resFieldEnter = recvPacket.Get<ResInterStageFieldEnter>();

				onRecvPacket?.Invoke(recvPacket, resFieldEnter);
			}, _onError);
		}

		public void REQ_GuildDungeonInfo(ulong guildId, Action<ZWebRecvPacket, ResGuildDungeonInfo> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGuildDungeonInfo.CreateReqGuildDungeonInfo(mBuilder, guildId);

			var reqPacket = ZWebPacket.Create<ReqGuildDungeonInfo>(this, Code.GS_GUILD_DUNGEON_INFO, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGuildDungeonInfo info = recvPacket.Get<ResGuildDungeonInfo>();

				for(int i = 0; i < info.InfoLength; i++)
				{
					GuildDungeonInfo? dungeonInfo = info.Info(i);
					if (dungeonInfo.HasValue)
					{
						//dungeonInfo.Value.BossKillTsSec 보스 잡은 시간 (안잡았으면 0)
						//dungeonInfo.Value.OpenGuildId 오픈한 길드 ID
						//dungeonInfo.Value.OpenTsSec 오픈한 시간
						//dungeonInfo.Value.RoomNo MMO 고유방 번호
						//dungeonInfo.Value.IsClose 닫힘 여부 (오픈 했다가 닫은 상태, 다시 열수 없음)
						//dungeonInfo.Value.StageTid
						//dungeonInfo.Value.Addr 할당 받은 MMO 주소
						
					}
				}

				onRecvPacket?.Invoke(recvPacket, info);
			}, _onError);
		}

		public void REQ_GuildDungeonOpen(ulong guildId, uint stageId, Action<ZWebRecvPacket, ResGuildDungeonOpen> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGuildDungeonOpen.CreateReqGuildDungeonOpen(mBuilder, guildId, stageId);

			var reqPacket = ZWebPacket.Create<ReqGuildDungeonOpen>(this, Code.GS_GUILD_DUNGEON_OPEN, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGuildDungeonOpen resOpen = recvPacket.Get<ResGuildDungeonOpen>();

				onRecvPacket?.Invoke(recvPacket, resOpen);
			}, _onError);
		}

		public void REQ_GuildDungeonEnter(uint portalId, Action<ZWebRecvPacket, ResGuildDungeonEnter> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGuildDungeonEnter.CreateReqGuildDungeonEnter(mBuilder, portalId);

			var reqPacket = ZWebPacket.Create<ReqGuildDungeonEnter>(this, Code.GS_GUILD_DUNGEON_ENTER, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGuildDungeonEnter resEnter = recvPacket.Get<ResGuildDungeonEnter>();

				onRecvPacket?.Invoke(recvPacket, resEnter);
			}, _onError);
		}

		public void REQ_GuildDungeonClose(uint stageId, Action<ZWebRecvPacket, ResGuildDungeonClose> onRecvPacket, PacketErrorCBDelegate _onError = null)
		{
			var offset = ReqGuildDungeonClose.CreateReqGuildDungeonClose(mBuilder, stageId);

			var reqPacket = ZWebPacket.Create<ReqGuildDungeonClose>(this, Code.GS_GUILD_DUNGEON_CLOSE, mBuilder, offset.Value);

			SendPacket(reqPacket, (recvPacket) =>
			{
				ResGuildDungeonClose resClose = recvPacket.Get<ResGuildDungeonClose>();

				onRecvPacket?.Invoke(recvPacket, resClose);
			}, _onError);
		}

		//[System.Diagnostics.Conditional("SHOW_LOG")]
		//public static void TestSend()
		//{
		//	ZReqPacket reqPacket = new ZReqPacket(WebSocketType.GAME_SERVER, Code.GS_TEST);

		//	builder.Clear();

		//	ReqTest.StartReqTest(builder);
		//	var offset = ReqTest.EndReqTest(builder);

		//	builder.Finish(offset.Value);
		//	reqPacket.AddBuilderMsg<ReqTest>(builder);

		//	WebSocketManager.instance.SendMsg(WebSocketType.GAME_SERVER, reqPacket, (recvPacket) =>
		//	{
		//	});
		//}
		//#endregion

		//		#region ========:: Purchase ::========

		//		/// <summary>
		//		/// 구매전 결제 초기화
		//		/// </summary>
		//		/// <param name="_ShopTid"></param>
		//		/// <param name="_Price"></param>
		//		/// <param name="_CurrencyCode"></param>
		//		public void REQ_PaymentInit(uint _ShopTid, float _Price, string _CurrencyCode, Action<ZWebRecvPacket, ResPaymentInit> onRecvPacket, PacketErrorCBDelegate _onError = null)
		//		{
		//			var offset = ReqPaymentInit.CreateReqPaymentInit(mBuilder,
		//				_ShopTid,
		//				ZGameManager.Instance.GetMarketType(),
		//				mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS),
		//				_Price,
		//				mBuilder.CreateString(_CurrencyCode),
		//				mBuilder.CreateString(Application.version));

		//			var reqPacket = ZWebPacket.Create<ReqPaymentInit>(this, Code.BI_PAYMENT_INIT, mBuilder, offset.Value);

		//			SendPacket(reqPacket, (recvPacket) =>
		//			{
		//				ResPaymentInit resPaymentInit = recvPacket.Get<ResPaymentInit>();
		//				onRecvPacket?.Invoke(recvPacket, resPaymentInit);
		//			}, _onError);
		//		}

		//		/// <summary>
		//		/// 상품 구매 요청
		//		/// </summary>
		//		/// <param name="_ProductInfo"></param>
		//		/// <param name="_Signature"></param>
		//		/// <param name="_GpsAdid"></param>
		//		public void REQ_PaymentPurchase(NTCore.PurchaseProductInfo _ProductInfo, string _Signature, string _GpsAdid, System.Action<ZWebRecvPacket, ResPaymentPurchase> onRecvPacket, PacketErrorCBDelegate _onError = null)
		//		{
		//			var offset = ReqPaymentPurchase.CreateReqPaymentPurchase(mBuilder,
		//				ZGameManager.Instance.GetMarketType(),
		//				mBuilder.CreateString(_Signature),
		//				mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS),
		//				mBuilder.CreateString(NTCommon.Device.Release),
		//				mBuilder.CreateString(_ProductInfo.currenyCode),
		//				mBuilder.CreateString(NTIcarusManager.Instance.GetAndroidId()),
		//				mBuilder.CreateString(com.adjust.sdk.Adjust.getIdfa()),
		//#if UNITY_IOS && !UNITY_EDITOR
		//				mBuilder.CreateString(NTCore.CommonAPI.getIDFV()),
		//#else
		//				mBuilder.CreateString(""),
		//#endif
		//				mBuilder.CreateString(_GpsAdid),
		//				mBuilder.CreateString(NTCore.CommonAPI.GetGameServerID()),
		//				mBuilder.CreateString(NTCommon.Locale.GetLocaleCode()),
		//				mBuilder.CreateString(NTCore.CommonAPI.GetGameSupportLanguage().langCulture),
		//				_ProductInfo.price.ToFloat);

		//			var reqPacket = ZWebPacket.Create<ReqPaymentPurchase>(this, Code.BI_PAYMENT_PURCHASE, mBuilder, offset.Value);

		//			SendPacket(reqPacket, (recvPacket) =>
		//			{
		//				ResPaymentPurchase resPaymentPurchase = recvPacket.Get<ResPaymentPurchase>();
		//				onRecvPacket?.Invoke(recvPacket, resPaymentPurchase);
		//			}, _onError);
		//		}
		//		#endregion

	}//ZWebGame
}