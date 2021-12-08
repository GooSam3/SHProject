using FlatBuffers;
using icarus_mmo_messages;
using MmoNet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZNet
{
	public class ZMmoFieldReceiver : MmoReceiverBase
	{
		protected delegate void InMapMsgRecvDelegate(InMapMsgBase? inMapMsgBase, uint tick);

		protected Dictionary<int, InMapMsgRecvDelegate> mMapReceiveHandler = new Dictionary<int, InMapMsgRecvDelegate>();

		public ZMmoFieldReceiver(MmoSessionBase _owner) : base(_owner)
		{
		}

		protected override void RegisterCallbacks()
		{
			mReceiveHandler[(int)MSG.LogOut] = OnRECV_LogOut;
			mReceiveHandler[(int)MSG.JoinField] = OnRECV_JoinField;
            mReceiveHandler[(int)MSG.GetServerTime] = OnRECV_ServerTime;
            mReceiveHandler[(int)MSG.MoveServer] = OnRECV_MoveServer;

            // 필드 입장시부터 받는 대표 메시지.
            mReceiveHandler[(int)MSG.InMapMsg] = OnRECV_InMapMsg;

			mMapReceiveHandler[(int)InMapMsgUnion.ResLoadMapOK] = OnMAP_LoadMapOK;
			mMapReceiveHandler[(int)InMapMsgUnion.CS_MapChat] = OnMAP_OnMapChat;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_AddCharInfo] = OnMAP_AddCharInfo;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_AddMonsterInfo] = OnMAP_AddMonsterInfo;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_AddNPCInfo] = OnMap_AddNpcInfo;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_AddGatherObj] = OnMap_AddObjectInfo;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_DelObject] = OnMAP_OnDelObject;
			mMapReceiveHandler[(int)InMapMsgUnion.CS_MoveToDir] = OnMAP_OnMoveToDir;
            mMapReceiveHandler[(int)InMapMsgUnion.CS_MoveStop] = OnMAP_OnMoveStop;
            mMapReceiveHandler[(int)InMapMsgUnion.CS_MoveToDest] = OnMAP_MoveToDest;
			mMapReceiveHandler[(int)InMapMsgUnion.CS_MoveToCollision] = OnMAP_OnMoveToCollision;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_MoveMap] = OnMAP_OnMoveMap;
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_MoveMap ] = OnMAP_OnMoveMap;

            //데미지
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_Damage] = OnMAP_OnDamage;
            //도트 데미지
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_DotDamage] = OnMAP_OnDotDamage;            

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ForceMove] = OnMAP_OnForceMove;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangeMezState] = OnMAP_ChangeMezState;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_GameTick] = OnMAP_GameTick;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangeClass] = OnMAP_OnChangeClass;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangePet] = OnMAP_OnChangePet;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangeChange] = OnMap_OnChangeChange;

            //스킬 관련
            mMapReceiveHandler[(int)InMapMsgUnion.CS_Attack] = OnMAP_OnAttack;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_Cooltime] = OnMAP_OnCoolTime;
            mMapReceiveHandler[(int)InMapMsgUnion.CS_SkillCancle] = OnMAP_OnSkillCancle;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_SkillAbility] = OnMAP_OnSkillAbility;
            mMapReceiveHandler[(int)InMapMsgUnion.CS_AttackToPos] = OnMAP_OnAttackToPos;


            //어빌리티 관련
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_AbilityNfy] = OnMAP_OnAbilityNotify;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_AddAbilAction] = OnMAP_OnAddAbilityAction;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_DelAbilAction] = OnMAP_OnDelAbilityAction;			

            //파티 관련
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_UpdatePartyInfo] = OnMAP_OnUpdatePartyInfo;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_DisplayEffect] = OnMAP_OnDisplayEffect;

            mMapReceiveHandler[(int)InMapMsgUnion.S2C_DropItemInfos] = OnMAP_OnDropItemInfos;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_RemainItemInfos] = OnMAP_OnRemainItemInfos;
			mMapReceiveHandler[(int)InMapMsgUnion.S2C_LvExpTendencyUp] = OnMAP_OnLvExpTendencyUp;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_LvExpDown] = OnMAP_OnLvExpDown;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_TendencyDown] = OnMAP_OnTendencyDown;

            //탑승물 관련
            mMapReceiveHandler[(int)InMapMsgUnion.CS_EquipVehicle] = OnMAP_OnEquipVehicle;
            mMapReceiveHandler[(int)InMapMsgUnion.CS_RideVehicle] = OnMAP_RideVehicle;

            //성향 수치 갱신 (모든 유저)
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangeTendency] = OnMAP_OnChangeTendency;

            //길드 정보 변경
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_ChangeGuildInfo] = OnMAP_OnChangeGuildInfo;

            //채집 관련
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_Casting] = OnMAP_OnCasting;

            //스탯 프리뷰
            mMapReceiveHandler[(int)InMapMsgUnion.CS_StatPreview] = OnMAP_OnStatPreview;

            //게임모드
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_StageState ] = OnMAP_StageState;
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_GameStart ] = OnMAP_GameStart;
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_GameScore ] = OnMAP_GameScore;
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_RoomInfo ] = OnMAP_RoomInfo;
            mMapReceiveHandler[ ( int )InMapMsgUnion.S2C_InstanceFinish ] = OnMAP_InstanceFinish;

            //성지
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_GodLandStatInfo] = OnMAP_GodLandStatInfo;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_GodLandFinishInfo] = OnMAP_GodLandFinishInfo;

            //보스전
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_BossPointRankingList] = OnMAP_BossWarDamageRanking;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_BossPointDeathPenalty] = OnMAP_BossWarDeathPenalty;
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_UserBossPoint] = OnMAP_BossWarMyPlayerPoint;

            //에러 처리
            mMapReceiveHandler[(int)InMapMsgUnion.S2C_Error] = OnMAP_OnError;
        }

        /// <summary> <see cref="MSG.InMapMsg"/> 관련 응답에 대한 종합 처리 </summary>
        private void OnRECV_InMapMsg(byte[] _recvDatas)
		{
			var inMapMsg = ConvertFBObjectNoLog<InMapMsg>(_recvDatas);
			var msgtype = inMapMsg.Msgbase.Value.MsgType;

            if (mMapReceiveHandler.TryGetValue((int)msgtype, out var recvAction))
			{
                ZMmoManager.Instance.SetGameTick(inMapMsg.Tick);

                recvAction(inMapMsg.Msgbase, inMapMsg.Tick);
			}
			else
			{
				Fun.FunDebug.LogWarning("not exist MapReceiveHandler | MmoNet.InMapMsgUnion: {0}, Tick: {1}", msgtype, inMapMsg.Tick);
			}
		}

		//
		//======================================================================================
		//

		protected override void OnErrorMessage(FlatMsg _flatMsg)
		{			
			ZLog.Log(ZLogChannel.MMO, ZLogLevel.Error, $"OnErrorMessage | MSG: {(MSG)_flatMsg.msgtype}, ErrCode: {(WebNet.ERROR)_flatMsg.err_code}");

			WebNet.ERROR errorCode = (WebNet.ERROR)_flatMsg.err_code;

			// TODO : 통합 처리를 위한 개선 필요.
			if (errorCode == WebNet.ERROR.PROTOCOL_VERSION)
			{
				string content = $"MMO서버 프로토콜이 맞지않습니다.\nCurrent Protocol Version: {(uint)MmoNet.Version.Num}";
				if (Application.isEditor)
					content += $"\n\n[Menu] 'ZGame/Protocols/[MMO] Sync ProtocolFiles' 로 프로토콜 갱신이 필요합니다.";

#if UNITY_EDITOR
				if (UnityEditor.EditorUtility.DisplayDialog(
					"MMO 통신 에러 발생",
					content, "확인"))
				{
					ZGameManager.Instance.QuitApp();
				}
#endif
				UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
					_popup.Open(ZUIString.ERROR, content, new string[] { "확인" }, new Action[] { delegate { ZGameManager.Instance.QuitApp(); } });
				});

				ZLog.LogWarn(ZLogChannel.System, $"{content}");
			}
		}

		private void OnRECV_LogOut(byte[] _recvDatas)
		{
			var resLogOut = ConvertFBObject<LogOutRes>(_recvDatas);
			ZLog.Log(ZLogChannel.MMO, $"Logout 완료");
		}

		private void OnRECV_JoinField(byte[] _recvDatas)
		{
			var resJoinField = ConvertFBObject<JoinFieldRes>(_recvDatas);            
            ZPawnManager.Instance.DoJoinField(resJoinField);

            ZGameModeManager.Instance.GameMode.JoinField();
        }

        private void OnRECV_ServerTime(byte[] _recvDatas)
        {
            var res = ConvertFBObject<GetServerTimeRes>(_recvDatas);
            TimeManager.Instance.SetTime(res.ClientTime, res.ServerTsMs);
        }

        private void OnRECV_MoveServer(byte[] _recvDatas)
		{
            var res = ConvertFBObject<MoveServerRes>(_recvDatas);
            ZMmoManager.Instance.Field.MmoFieldCallBack[MSG.MoveServer].Invoke(res);
        }

        #region ========:: for InMapMsg ::========

        private void OnMAP_LoadMapOK(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var resLoadMapOK = GetInMapMsg<ResLoadMapOK>(ref _inMapMsgBase);

            ZGameModeManager.Instance.GameMode.LoadMapOK();
        }

		private void OnMAP_OnMapChat(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			GetInMapMsg<CS_MapChat>(ref _inMapMsgBase);
		}

		private void OnMAP_AddCharInfo(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var addCharInfo = GetInMapMsg<S2C_AddCharInfo>(ref _inMapMsgBase).Value;
			
			ZPawnManager.Instance.DoAdd(addCharInfo);
		}

		private void OnMAP_AddMonsterInfo(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var addMonsterInfo = GetInMapMsg<S2C_AddMonsterInfo>(ref _inMapMsgBase).Value;

            ZPawnManager.Instance.DoAdd(addMonsterInfo);
        }

        private void OnMap_AddNpcInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var addNpcInfo = GetInMapMsg<S2C_AddNPCInfo>(ref _inMapMsgBase).Value;

            ZPawnManager.Instance.DoAdd(addNpcInfo);
        }

        private void OnMap_AddObjectInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var addObjectInfo = GetInMapMsg<S2C_AddGatherObj>(ref _inMapMsgBase).Value;

            ZPawnManager.Instance.DoAdd(addObjectInfo);
        }

        private void OnMAP_OnDelObject(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var delObject = GetInMapMsg<S2C_DelObject>(ref _inMapMsgBase).Value;

            for (int i = 0; i < delObject.ObjectidLength; ++i)
			{
                uint targetObjId = delObject.Objectid(i);
                ZPawnManager.Instance.DoRemove(targetObjId);
			}
		}

		private void OnMAP_OnMoveToDir(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var moveToDir = GetInMapMsg<CS_MoveToDir>(ref _inMapMsgBase).Value;

            ZPawnManager.Instance.DoMoveDir(moveToDir);
		}

        private void OnMAP_OnMoveStop(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var moveStop = GetInMapMsg<CS_MoveStop>(ref _inMapMsgBase).Value;

            ZPawnManager.Instance.DoMoveStop(moveStop);
        }

		private void OnMAP_MoveToDest(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var moveToDest = GetInMapMsg<CS_MoveToDest>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoMoveToDest(moveToDest);
        }

		private void OnMAP_OnMoveToCollision(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var moveToCol = GetInMapMsg<CS_MoveToCollision>(ref _inMapMsgBase).Value;
		}

        private void OnMAP_OnMoveMap(InMapMsgBase? _inMapMsgBase, uint tick) {
            var moveMap = GetInMapMsg<S2C_MoveMap>(ref _inMapMsgBase).Value;

            ZGameManager.Instance.DoForceMapMove( moveMap.Stagetid, moveMap.Reason );
        }

		private void OnMAP_OnAttack(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var attack = GetInMapMsg<CS_Attack>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoAttack(attack);
            //ZLog.LogError(ZLogChannel.Skill, $"tick : {tick}, endtick : {attack.Endtime} = {attack.Endtime - tick}");
        }

        private void OnMAP_OnAttackToPos(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var attack = GetInMapMsg<CS_AttackToPos>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoAttackToPos(attack);
        }        

        private void OnMAP_OnCoolTime(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var skill = GetInMapMsg<S2C_Cooltime>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoSkillCoolTime(skill);
        }

        private void OnMAP_OnSkillCancle(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var skill = GetInMapMsg<CS_SkillCancle>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoSkillCancel(skill);
        }

        private void OnMAP_OnSkillAbility(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var skill = GetInMapMsg<S2C_SkillAbility>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoSkillAbilityNotify(skill);
        }        

        private void OnMAP_OnDamage(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var dmgInfo = GetInMapMsg<S2C_Damage>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoTakeDamage(dmgInfo);
        }

        private void OnMAP_OnDotDamage(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var dmgInfo = GetInMapMsg<S2C_DotDamage>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoTakeDotDamage(dmgInfo);
        }

        private void OnMAP_OnAbilityNotify(InMapMsgBase? _inMapMsgBase, uint tick)
		{
            //스탯 등이 변경되었을 경우 호출됨
			var abilInfo = GetInMapMsg<S2C_AbilityNfy>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoAbilityNotify(abilInfo);
        }

        private void OnMAP_OnAddAbilityAction(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var abilInfo = GetInMapMsg<S2C_AddAbilAction>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoAddAbilityAction(abilInfo);
        }

        private void OnMAP_OnDelAbilityAction(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var abilInfo = GetInMapMsg<S2C_DelAbilAction>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoDelAbilityAction(abilInfo);
        }

        private void OnMAP_OnUpdatePartyInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_UpdatePartyInfo>(ref _inMapMsgBase).Value;
            ZPartyManager.Instance.RecvBroadcastMessageByMMo(info);
        }

        /// <summary> 서버상 Entity들 연출에 필요한 이벤트 처리 </summary>
        private void OnMAP_OnDisplayEffect(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			/*
			 * 현재 서버와 약속된 이펙트 연출은 텔레포트에 대한 연출 밖에 없음.
			 */
			var effInfo = GetInMapMsg<S2C_DisplayEffect>(ref _inMapMsgBase).Value;

            var objectId = effInfo.Objectid;
            var effectTid = effInfo.EffectTid;

            if (ZPawnManager.Instance.TryGetEntity(objectId, out var entity))
			{
                entity.SpawnEffect(effectTid);
			}
            else if(ZPawnManager.Instance.TryGetEntityData(objectId, out var entityData))
			{
                //entityData.Position;
                if(entityData.Position.HasValue)
                    ZEffectManager.Instance.SpawnEffect(effectTid, entityData.Position.Value, Quaternion.identity);
            }
		}

		private void OnMAP_GameTick(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var msgInfo = GetInMapMsg<S2C_GameTick>(ref _inMapMsgBase).Value;
            ZMmoManager.Instance.SetGameTick(msgInfo.Tick, msgInfo.Inteval);
		}

		private void OnMAP_OnForceMove(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var forceMove = GetInMapMsg<S2C_ForceMove>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoForceMove(forceMove);
        }

        private void OnMAP_ChangeMezState(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var mezInfo = GetInMapMsg<S2C_ChangeMezState>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoChangeMezState(mezInfo);
        }

        private void OnMAP_OnChangeClass(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var changeClass = GetInMapMsg<S2C_ChangeClass>(ref _inMapMsgBase).Value;
			ZPawnManager.Instance.DoChangeClass(changeClass);

		}

		private void OnMAP_OnChangePet(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var addCharInfo = GetInMapMsg<S2C_ChangePet>(ref _inMapMsgBase).Value;
			ZPawnManager.Instance.DoChangePet(addCharInfo);	
		}

		private void OnMap_OnChangeChange(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var addcharInfo = GetInMapMsg<S2C_ChangeChange>(ref _inMapMsgBase).Value;
			ZPawnManager.Instance.DoChangeChange(addcharInfo);
        }

		/// <summary> 바닥에 드랍되는 아이템 연출을 위해서 처리를 위한 함수 </summary>
		private void OnMAP_OnDropItemInfos(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var dropItemInfos = GetInMapMsg<S2C_DropItemInfos>(ref _inMapMsgBase).Value;
            
            DropItemSpawner.DropItem(ref dropItemInfos);

            ZWebChatData.OnRemainItemInfos(dropItemInfos);
        }

        /// <summary> 획득 아이템에 대한 처리를 위한 함수 </summary>
        private void OnMAP_OnRemainItemInfos(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var remainItemInfo = GetInMapMsg<S2C_RemainItemInfos>(ref _inMapMsgBase).Value;

            var charData = ZNet.Data.Me.CurCharData;
			if (null != charData)
			{
				int accountStackCount = remainItemInfo.AccountStackLength;
				for (int i = 0; i < accountStackCount; ++i)
					charData.AddItemList(remainItemInfo.AccountStack(i));

				int equipCount = remainItemInfo.EquipLength;
				for (int i = 0; i < equipCount; ++i)
					charData.AddItemList(remainItemInfo.Equip(i));

				int stackCount = remainItemInfo.StackLength;
				for (int i = 0; i < stackCount; ++i)
					charData.AddItemList(remainItemInfo.Stack(i));

                int runeCount = remainItemInfo.RuneLength;
                for (int i = 0; i < runeCount; ++i)
                    charData.AddRune(remainItemInfo.Rune(i));
			}
		}

		/// <summary> 내 캐릭터의 레벨업, 경험치, 성향치 갱신시 처리 </summary>
		private void OnMAP_OnLvExpTendencyUp(InMapMsgBase? _inMapMsgBase, uint tick)
		{
			var info = GetInMapMsg<S2C_LvExpTendencyUp>(ref _inMapMsgBase).Value;

			var charData = ZNet.Data.Me.CurCharData;
			if (null != charData)
			{
				charData.UpdateLevel(info.Lv);
				charData.UpdateExp(info.Exp, info.IsMonsterKill);
				charData.UpdateTendency(info.Tendency);
			}
		}

        /// <summary> 내 캐릭터의  레벨업, 경험치 갱신시 처리 </summary>
		private void OnMAP_OnLvExpDown(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_LvExpDown>(ref _inMapMsgBase).Value;

            var charData = ZNet.Data.Me.CurCharData;
            if (null != charData)
            {
                charData.UpdateLevel(info.Lv);
                charData.UpdateExp(info.Exp, false);
            }
        }

        /// <summary> 내 캐릭터의 성향치 갱신시 처리 </summary>
		private void OnMAP_OnTendencyDown(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_TendencyDown>(ref _inMapMsgBase).Value;

            var charData = ZNet.Data.Me.CurCharData;
            if (null != charData)
            {
                charData.UpdateTendency(info.Tendency);
            }
        }

        /// <summary> 모든 유저의 성향치가 갱신(단계별로갱신)될 때 처리 </summary>
        private void OnMAP_OnChangeTendency(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_ChangeTendency>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoChangeTendency(info);
        }

        /// <summary> 길드 정보 갱신 </summary>
        private void OnMAP_OnChangeGuildInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_ChangeGuildInfo>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoChangeGuildInfo(info);
        }        

        /// <summary> 탑승물 관련 </summary>
        private void OnMAP_OnEquipVehicle(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<CS_EquipVehicle>(ref _inMapMsgBase);
            ZPawnManager.Instance.DoEquipVehicle(info.Value);
        }

        private void OnMAP_RideVehicle(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<CS_RideVehicle>(ref _inMapMsgBase);
            ZPawnManager.Instance.DoRideVehicle(info.Value);
        }

        /// <summary> 채집 캐스팅 </summary>
        private void OnMAP_OnCasting(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_Casting>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoObjectCasting(info);
        }

        private void OnMAP_OnStatPreview(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<CS_StatPreview>(ref _inMapMsgBase).Value;
            ZPawnManager.Instance.DoUpdateStatPreview(info);
        }

        /// <summary> 게임모드 </summary>
        private void OnMAP_StageState( InMapMsgBase? _inMapMsgBase, uint tick )
        {
            var info = GetInMapMsg<S2C_StageState>( ref _inMapMsgBase ).Value;
            ZGameModeManager.Instance.GameMode.RECV_StageState( info );
        }

        private void OnMAP_GameStart( InMapMsgBase? _inMapMsgBase, uint tick )
        {
            var info = GetInMapMsg<S2C_GameStart>( ref _inMapMsgBase ).Value;
            ZGameModeManager.Instance.GameMode.RECV_GameStart( info );
        }

        private void OnMAP_GameScore( InMapMsgBase? _inMapMsgBase, uint tick )
        {
            var info = GetInMapMsg<S2C_GameScore>( ref _inMapMsgBase ).Value;
            ZGameModeManager.Instance.GameMode.RECV_GameScore( info );
        }

        private void OnMAP_RoomInfo( InMapMsgBase? _inMapMsgBase, uint tick )
        {
            var info = GetInMapMsg<S2C_RoomInfo>( ref _inMapMsgBase ).Value;
            ZGameModeManager.Instance.GameMode.RECV_RoomInfo( info );
        }
        
        private void OnMAP_InstanceFinish( InMapMsgBase? _inMapMsgBase, uint tick )
        {
            var info = GetInMapMsg<S2C_InstanceFinish>( ref _inMapMsgBase ).Value;
            ZGameModeManager.Instance.GameMode.RECV_InstanceFinish( info );
        }

        /// <summary> 성지 </summary>
        private void OnMAP_GodLandStatInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_GodLandStatInfo>(ref _inMapMsgBase).Value;

            var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeGodLand>();
            gameMode.RECV_GodLandStatInfo(info);
        }

        private void OnMAP_GodLandFinishInfo(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var info = GetInMapMsg<S2C_GodLandFinishInfo>(ref _inMapMsgBase).Value;

            var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeGodLand>();
            gameMode.RECV_GodLandFinishInfo(info);
        }

        /// <summary> 보스전 </summary>
        private void OnMAP_BossWarDamageRanking(InMapMsgBase? _inMapMsgBase, uint tick)
		{
            var info = GetInMapMsg<S2C_BossPointRankingList>(ref _inMapMsgBase).Value;

            var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeBossWar>();
            if(gameMode != null)
                gameMode.RECV_BossWarPointRankingList(info);
		}

        private void OnMAP_BossWarDeathPenalty(InMapMsgBase? _inMapMsgBase, uint tick)
		{
            var info = GetInMapMsg<S2C_BossPointDeathPenalty>(ref _inMapMsgBase).Value;

            var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeBossWar>();
            gameMode.RECV_BossWarDeathPenalty(info);
        }

        private void OnMAP_BossWarMyPlayerPoint(InMapMsgBase? _inMapMsgBase, uint tick)
		{
            var info = GetInMapMsg<S2C_UserBossPoint>(ref _inMapMsgBase).Value;

            var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeBossWar>();
            gameMode.RECV_BossWarMyPlayerPoint(info);
		}

        /// <summary> 에러 처리 </summary>
        private void OnMAP_OnError(InMapMsgBase? _inMapMsgBase, uint tick)
        {
            var error = GetInMapMsg<S2C_Error>(ref _inMapMsgBase).Value;

            var errorCode = (WebNet.ERROR)error.ErrorCode;

            UICommon.SetNoticeMessage(DBLocale.GetText(errorCode.ToString()), Color.red, 1, UIMessageNoticeEnum.E_MessageType.BackNotice);

            switch(errorCode)
            {
                case WebNet.ERROR.NOT_USE_IN_TOWN:
                    {
                        var myPc = ZPawnManager.Instance.MyEntity;

                        if(null != myPc)
                        {
                            myPc.ChangeState(E_EntityState.Empty);
                            //TODO :: 추후 수정하자
                            if(myPc.IsAutoPlay)
                            {
                                var actionUI = UIManager.Instance.Find<UISubHUDCharacterAction>();

                                if (null != actionUI)
                                    actionUI.SelectAuto();
                            }
                        }
                    }
                    break;
                case WebNet.ERROR.UNUSABLE_MEZSTATE:
                case WebNet.ERROR.USER_NOT_FIND:
                    {
                        //맵 이동 실패시 내 캐릭터 이동 활성화
                        if(error.MsgType == (uint)MSG.MoveServer)
						{
                            EnableMoveForMyPc();
                        }
                    }
                    break;
            }

            ZLog.LogError(ZLogChannel.MMO, $"MsgType - {(InMapMsgUnion)error.MsgType}, ErrorCode - ({(WebNet.ERROR)error.ErrorCode})");
		}

        private void EnableMoveForMyPc()
		{
            if (true == ZPawnManager.hasInstance && null != ZPawnManager.Instance.MyEntity)
                ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
        }

		#endregion//========:: for InMapMsg ::========

		private FBO_TYPE? GetInMapMsg<FBO_TYPE>(ref InMapMsgBase? _inMapMsgBase) where FBO_TYPE : struct, IFlatbufferObject
		{
			var parsedMsg = _inMapMsgBase.Value.Msg<FBO_TYPE>();

			ZLog.Log(ZLogChannel.MMO, $"<color=orange>[RECV] {typeof(FBO_TYPE).Name} : {ZNetHelper.GetPropertyStrings(parsedMsg)}</color>");

			return parsedMsg;
		}

		public static Vector3 ToVector3(ServerPos3? serverPos)
		{
			return new Vector3(serverPos.Value.X, serverPos.Value.Y, serverPos.Value.Z);
		}
	}
}