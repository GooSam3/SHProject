using GameDB;
using System;
using ZNet.Data;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class ZGameModeColosseum : ZGameModeBase
{
	private enum E_DirectorState
	{
		Nothing,
		Ready,
		Start,
		Playing,
		Finish,
	}

	private class DirectorCameraInfo
	{
		public readonly ZPawn Entity;
		public readonly uint EntityId;
		public readonly Vector3 StartPos;
		public readonly bool IsEnemy;

		public bool IsFinished;
		public float HideDelayTime;
		public float ShowDelayTime;
		public bool IsKillFinisher;
		public bool IsLastestDeath;

		public DirectorCameraInfo(ZPawn _entity, float _showDelayTime)
		{
			Entity = _entity;
			EntityId = _entity.EntityId;
			StartPos = _entity.Position;
			ShowDelayTime = _showDelayTime;

			var myEntity = ZPawnManager.Instance.MyEntity;
			if (myEntity == null) {
				ZLog.LogError(ZLogChannel.Default, $"내가 스폰이 아직 없다니.. 스폰중인entity:{_entity}");
				return;
			}

			IsEnemy = myEntity.EntityData.IsEnemy(Entity.EntityData);
		}
	}

	public override E_GameModeType GameModeType { get { return E_GameModeType.Colosseum; } }
	protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;
	public bool IsFinishedMmo { get; set; }
	public bool IsFinishedWeb { get; set; }
	public Action UserListCallback { private get; set; }
	public Action CountDownCallback { private get; set; }
	public Action RemainTimeCallback { private get; set; }

	private const float TARGET_CAMERA_ENTER_SHOW_DELAY = 0.5f;
	private const float TARGET_CAMERA_ENTER_HIDE_DELAY = 2.0f;
	private const float TARGET_CAMERA_FINISH_DELAY = 2.0f;

	private List<DirectorCameraInfo> directorCamInfoList = new List<DirectorCameraInfo>();
	private E_DirectorState directorState = E_DirectorState.Nothing;
	private uint myKillCount = 0;
	private bool isWin;

	protected override void ExitGameMode()
	{
		UIManager.Instance.Close<UIPopupColosseumResult>();
		UIManager.Instance.Close<UIControlBlock>();
		UIManager.Instance.Close<UIKillMessage>();
		CoroutineManager.Instance.StopAllActions();

		ResetData();
	}

	protected override void StartGameMode()
	{
		ResetData();

		UIManager.Instance.Open<UIControlBlock>();
	}

	private void ResetData()
	{
		IsFinishedMmo = false;
		IsFinishedWeb = false;
		directorState = E_DirectorState.Nothing;
		myKillCount = 0;
		isWin = false;
		Me.CurCharData.ColosseumContainer.Clear();
		directorCamInfoList.Clear();
	}

	#region 이벤트 //////////////////////////////////////////////////////////

	public void SetUserListCallback(Action callback)
	{
		if (Me.CurCharData.ColosseumContainer.RoomUserList.Count > 0) {
			callback?.Invoke();
			return;
		}
		UserListCallback = callback;
	}

	public void SetCountDownCallback(Action callback)
	{
		if (Me.CurCharData.ColosseumContainer.CountDownEndTime > 0) {
			callback?.Invoke();
			return;
		}
		CountDownCallback = callback;
	}

	public void SetRemainTimeCallback(Action callback)
	{
		if (Me.CurCharData.ColosseumContainer.RemainTimeTargetTime > 0) {
			callback?.Invoke();
			return;
		}
		RemainTimeCallback = callback;
	}

	#endregion

	#region 연출관련 //////////////////////////////////////////////////////////

	public override void CreateEntity(bool isMyPc, uint entityId, ZPawn entity)
	{
		// 모든 enttiy 중앙보기
		entity.LookAt(Vector3.zero);

		if (isMyPc) {
			ZPawn myEntity = ZPawnManager.Instance.MyEntity;
			CameraManager.Instance.DoSetTarget(myEntity.transform);
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Top, Cinemachine.CinemachineBlendDefinition.Style.Cut, 0, true);
		}

		// 카메라정보에 추가
		directorCamInfoList.Add(new DirectorCameraInfo(entity, TARGET_CAMERA_ENTER_SHOW_DELAY));
		SetDirectorState(E_DirectorState.Ready);
	}

	public override void RemoveEntity(bool isMyPc, uint entityId)
	{
		if (isMyPc) {
			CameraManager.Instance.DoSetTarget(null);
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Empty);
		}
	}

	public override void DieEntity(uint attackerEntityId, bool isMyPcDead, ZPawn deadPawn)
	{
		if (isMyPcDead) {
			BlockMyControl();
		}

		if (attackerEntityId == 0 || deadPawn == null) {
			return;
		}

		var attacker = ZPawnManager.Instance.GetEntityData(attackerEntityId);
		if (attacker == null) {
			return;
		}

		var myEntity = ZPawnManager.Instance.MyEntity;
		if (myEntity == null) {
			return;
		}

		for (int i = 0; i < directorCamInfoList.Count; ++i) {
			var info = directorCamInfoList[i];
			info.IsKillFinisher = info.EntityId == attackerEntityId;
			info.IsLastestDeath = info.EntityId == deadPawn.EntityData.EntityId;
		}

		bool isEnemyDead = myEntity.EntityData.IsEnemy(deadPawn.EntityData);
		if (isEnemyDead) {

			// 내가 적을 죽였을 경우 킬콤보 이펙트
			if (attackerEntityId == myEntity.EntityId) {
				myKillCount += 1;
				var hudColosseum = UIManager.Instance.Find<UISubHUDColosseum>();
				if (hudColosseum != null) {
					hudColosseum.ShowKillCount(myKillCount);
				}
			}

			// 킬메세지
			string killMsg = DBLocale.GetText("WPvP_Pk_Notice_Enemy");
			killMsg = string.Format(killMsg, attacker.Name, deadPawn.EntityData.Name);
			UICommon.SetKillMessage(killMsg, E_KillMessage.EnemyTeamDead);

		}
		else {
			// 킬메세지
			string killMsg = DBLocale.GetText("WPvP_Pk_Notice_Ally");
			killMsg = string.Format(killMsg, attacker.Name, deadPawn.EntityData.Name);
			UICommon.SetKillMessage(killMsg, E_KillMessage.OurTeamDead);
		}
	}

	private void SetDirectorState(E_DirectorState _directorState)
	{
		if (directorState == E_DirectorState.Nothing || directorState < _directorState) {
			directorState = _directorState;
			ZLog.Log(ZLogChannel.Default, $"@SetDirectorState 연출상태변경, {_directorState}");
		}
	}

	private void Update()
	{
		switch (directorState) {
			case E_DirectorState.Nothing: {
				break;
			}
			case E_DirectorState.Ready: {

				// 돌아가며 타겟켐 비추기
				if (directorCamInfoList.Count == 0) {
					return;
				}

				var info = directorCamInfoList.Find(v => v.Entity != null && v.IsFinished == false);
				if (info == null) {
					return;
				}

				if (info.HideDelayTime > 0) {
					info.HideDelayTime -= Time.deltaTime;
					if (info.HideDelayTime <= 0) {
						info.IsFinished = true;
					}
					return;
				}

				if (info.ShowDelayTime > 0f) {
					info.ShowDelayTime -= Time.deltaTime;
				}
				else {
					SetTargetCamera(info.Entity);
					info.HideDelayTime = TARGET_CAMERA_ENTER_HIDE_DELAY;

					// 등장메세지
					if (info.Entity != null) {
						string userName = info.Entity.EntityData.Name;
						string enterMsg = string.Format(DBLocale.GetText("Wguild_Connection_Notice"), userName);

						var myEntity = ZPawnManager.Instance.MyEntity;
						if (myEntity.EntityData.IsEnemy(info.Entity.EntityData)) {
							UICommon.SetNoticeMessage(
								enterMsg, ParadoxNotion.ColorUtils.HexToColor("FF7777"), 0.8f, UIMessageNoticeEnum.E_MessageType.SubNotice);
						}
						else {
							UICommon.SetNoticeMessage(
								enterMsg, ParadoxNotion.ColorUtils.HexToColor("64daff"), 0.8f, UIMessageNoticeEnum.E_MessageType.SubNotice);
						}
					}
				}
				break;
			}
			case E_DirectorState.Start: {

				// 저장된 캠으로 바꾸기
				ZPawn myEntity = ZPawnManager.Instance.MyEntity;
				CameraManager.Instance.DoSetTarget(myEntity.transform);
				E_CameraMotorType lastMotor = CameraManager.Instance.LoadCameraMotorType();
				CameraManager.Instance.DoChangeCameraMotor(lastMotor);

				SetDirectorState(E_DirectorState.Playing);
				break;
			}
			case E_DirectorState.Playing: {
				break;
			}
			case E_DirectorState.Finish: {

				BlockMyControl();

				// 유저들 시작시 제자리로
				for (int i = 0; i < directorCamInfoList.Count; ++i) {
					var info = directorCamInfoList[i];
					if (info.Entity == null) {
						continue;
					}
					info.Entity.LookAt(Vector3.zero);
					info.Entity.StopMove();
					info.Entity.Warp(info.StartPos);
				}

				var myEntity = ZPawnManager.Instance.MyEntity;

				if (isWin) {
					if (myEntity != null && myEntity.IsDead == false) {
						// 내가 살아있음 나를 마지막 타겟캠
						SetTargetCamera(myEntity);
					}
					else {
						// 내가 죽었다면 마지막 생존한 킬피시셔로..
						var info = directorCamInfoList.Find(v => v.IsEnemy == false && v.Entity != null && v.Entity.IsDead == false && v.IsKillFinisher);
						if (info != null) {
							SetTargetCamera(info.Entity);
						}
						else {
							// 생존중인 아군 타겟캠
							info = directorCamInfoList.Find(v => v.IsEnemy == false && v.Entity != null && v.Entity.IsDead == false);
							if (info != null) {
								SetTargetCamera(info.Entity);
							}
						}
					}
				}
				else {
					if (myEntity != null && myEntity.IsDead == false) {
						// 내가 살아있음 나를 마지막 타겟캠
						SetTargetCamera(myEntity);
					}
					else {
						// 아직 살이 있는 아군 타겟캠
						var info = directorCamInfoList.Find(v => v.IsEnemy == false && v.Entity != null && v.Entity.IsDead == false);
						if (info != null) {
							SetTargetCamera(info.Entity);
						}
						else {
							// 마지막에 죽은 아군 타겟캠
							info = directorCamInfoList.Find(v => v.IsEnemy == false && v.IsLastestDeath);
							if (info != null) {
								SetTargetCamera(info.Entity);
							}
						}
					}
				}

				SetDirectorState(E_DirectorState.Nothing);
				break;
			}
		}
	}

	private void SetTargetCamera(ZPawn entity)
	{
		if (entity == null) {
			return;
		}

		CameraManager.Instance.DoSetTarget(entity.transform);
		if (CameraManager.Instance.CurrentMotorType != E_CameraMotorType.ModeDirector) {
			CameraManager.Instance.DoChangeCameraMotor(
				E_CameraMotorType.ModeDirector, Cinemachine.CinemachineBlendDefinition.Style.EaseOut, 1f, true);
		}

		var motor = CameraManager.Instance.CurrentMotor as CameraMotorModeDirector;
		motor.DoChangeLookTarget(entity.transform);
		motor.ZoomIn();
	}

	private void BlockMyControl()
	{
		ZPawnManager.Instance.MyEntity?.StopAllAction(true);

		var frameHud = UIManager.Instance.Find<UIFrameHUD>();
		if (frameHud != null) {
			frameHud.RemoveAllInfoPopup();
			frameHud.HideAllContentFrame();
		}

		UIManager.Instance.Close<UISubHUDMenu>();
		UIManager.Instance.Close<UISubHUDCharacterState>();
		UIManager.Instance.Close<UISubHUDQuickSlot>();
		UIManager.Instance.Close<UISubHUDCharacterAction>();
		UIManager.Instance.Close<UISubHUDJoyStick>();
	}

	#endregion

	#region 입장 //////////////////////////////////////////////////////////

	/// <summary> 매칭등록 </summary>
	public static void REQ_AddColosseumQueue(uint stageTid)
	{
		// 무기 착용중인지
		var data = Me.CurCharData.GetEquippedItem(E_EquipSlotType.Weapon);
		if (null == data) {
			UIMessagePopup.ShowPopupOk(DBLocale.GetText(DBLocale.GetText("Unmount_Weapon_Message")));
			return;
		}

		// 클래스에 맞는 무기 착용중인지
		bool hasClassWeapon = Me.CurCharData.IsCharacterEquipable(data.ItemTid);
		if (hasClassWeapon == false) {
			UIMessagePopup.ShowPopupOk(DBLocale.GetText(DBLocale.GetText("Job_Match_Weapon_Message")));
			return;
		}

		var stageTable = DBStage.Get(stageTid);
		if (Me.CurCharData.LastLevel < stageTable.InMinLevel) {
			string msg = string.Format(DBLocale.GetText("WPvP_Duel_Level"), DBConfig.WPvP_Duel_Level);
			UIMessagePopup.ShowPopupOk(msg);
			return;
		}

		ZWebManager.Instance.WebGame.REQ_AddColosseumQueue(
			Me.UserID, Me.CharID, true, Me.SelectedServerID, stageTid, (res, msg) => {
				Me.CurCharData.ColosseumContainer.IsMachingNow = true;
				var frameColoseeum = UIManager.Instance.Find<UIFrameColosseum>();
				if (frameColoseeum != null) {
					frameColoseeum.RefreshAll();
				}
			});
	}

	/// <summary> 매칭취소 </summary>
	public static void REQ_LeaveColosseumQueue(uint stageTid)
	{
		ZWebManager.Instance.WebGame.REQ_LeaveColosseumQueue(
			Me.UserID, Me.CharID, Me.SelectedServerID, stageTid, (res, msg) => {
				Me.CurCharData.ColosseumContainer.IsMachingNow = false;
				var frameColoseeum = UIManager.Instance.Find<UIFrameColosseum>();
				if (frameColoseeum != null) {
					frameColoseeum.RefreshAll();
				}
			});
	}

	/// <summary> 콜로세움 강제로 큐 퇴장을 알려준다. </summary>
	public static void RECV_LeaveColosseumQueue(ZNet.ZWebRecvPacket recv)
	{
		var msg = recv.Get<WebNet.BroadcastLeaveColosseumQueue>();

		if (msg.Reason == 0) {
		}

		Me.CurCharData.ColosseumContainer.IsMachingNow = false;
		var frameColosseum = UIManager.Instance.Find<UIFrameColosseum>();
		if (frameColosseum != null) {
			frameColosseum.RefreshAll();
		}
	}

	/// <summary>  콜로세움 매칭이 되었으니 게임을 플레이를 알려준다. </summary>
	public static void RECV_JoinColosseum(ZNet.ZWebRecvPacket recv)
	{
		var msg = recv.Get<WebNet.BroadcastJoinColosseum>();

		if (Me.CurCharData.ColosseumContainer.IsMachingNow == false) {
			ZLog.LogError(ZLogChannel.Default, $"매칭중이 아닌데 입장이 되었다 체크!,stageTid:{msg.StageTid}");
		}

		var stageTable = DBStage.Get(msg.StageTid);

		var portalTable = DBPortal.Get(stageTable.DefaultPortal);
		if (portalTable == null) {
			ZLog.LogError(ZLogChannel.Default, $"portalTable이 null이다, DefaultPortal:{stageTable.DefaultPortal}");
			return;
		}

		ulong ivenItemId = 0;
		var invenItem = Me.CurCharData.GetInvenItemUsingMaterial(portalTable.UseItemID);
		if (invenItem != null) {
			ivenItemId = invenItem.item_id;
		}

		Me.CurCharData.ColosseumContainer.IsMachingNow = false;

		AudioManager.Instance.PlaySFX(30004); //입장사운드
		ZGameManager.Instance.TryEnterStage(
			portalTable.PortalID, false, ivenItemId, portalTable.UseItemID, msg.ServerAddr, (ushort)msg.ChannelId, msg.RoomNo);

		UIManager.Instance.Close<UIFrameColosseum>();
	}
	#endregion

	#region 대기 //////////////////////////////////////////////////////////

	/// <summary> 입장직후 유저정보들 </summary>
	public override void RECV_RoomInfo(MmoNet.S2C_RoomInfo info)
	{
		for (int i = 0; i < info.RoomUsersLength; ++i) {
			var roomUser = info.RoomUsers(i).Value;

			Me.CurCharData.ColosseumContainer.RoomUserList.Add(new ColosseumRoomUserConverted(roomUser));
		}
		Me.CurCharData.ColosseumContainer.SortRoomUserList();

		UserListCallback?.Invoke();
		UserListCallback = null;
	}

	/// <summary> 0-ready,1-play, play time (플레이 상태일때만 유효한 값)  </summary>
	public override void RECV_StageState(MmoNet.S2C_StageState info)
	{
		if (info.State == 0) {
		}
		else if (info.State == 1) {
			UIManager.Instance.Close<UIControlBlock>();

			var container = Me.CurCharData.ColosseumContainer;
			container.RemainTimeTargetTime = info.RemainSec + TimeManager.NowSec;
			RemainTimeCallback?.Invoke();
			RemainTimeCallback = null;

			SetDirectorState(E_DirectorState.Start);
		}
	}

	/// <summary> Play전까지 카운트 다운 </summary>
	public override void RECV_GameStart(MmoNet.S2C_GameStart info)
	{
		var container = Me.CurCharData.ColosseumContainer;
		container.CountDownEndTime = info.Countdown + TimeManager.NowSec;

		CountDownCallback?.Invoke();
		CountDownCallback = null;

		CoroutineManager.Instance.StartTimer(3f, () => {
			SetDirectorState(E_DirectorState.Start);
		});
	}

	#endregion

	#region 진행 //////////////////////////////////////////////////////////

	/// <summary> 0-이면 게임진행중, 1-게임종료 </summary>
	public override void RECV_GameScore(MmoNet.S2C_GameScore info)
	{
		var hudColosseum = UIManager.Instance.Find<UISubHUDColosseum>();
		if (hudColosseum != null) {
			hudColosseum.SetScoreUpdate(info.Score(0), info.Score(1));
		}
		else {
			ZLog.LogError(ZLogChannel.Default, "UISubHUDColosseum 가 null이다");
		}

		if (info.GameEnd == 0) {
		}
		else if (info.GameEnd == 1) {
			BlockMyControl();

			// mmo와 web이 종료를 각각 다르게 준다 구조적인 문제
			IsFinishedMmo = true;
			if (IsFinishedMmo && IsFinishedWeb) {
				REQ_RewardColosseum();
			}
		}
	}

	#endregion

	#region 종료 //////////////////////////////////////////////////////////

	/// <summary> 콜로세움 매칭이 끝났으니 플레이 보상을 지급한다. </summary>
	public static void RECV_RewardColosseum(ZNet.ZWebRecvPacket recv)
	{
		var modeColosseum = ZGameModeManager.Instance.CurrentGameMode<ZGameModeColosseum>();
		if (modeColosseum != null) {
			modeColosseum.BlockMyControl();

			// mmo와 web이 종료를 각각 다르게 준다 구조적인 문제
			modeColosseum.IsFinishedWeb = true;
			if (modeColosseum.IsFinishedMmo && modeColosseum.IsFinishedWeb) {
				modeColosseum.REQ_RewardColosseum();
			}
		}
	}

	/// <summary> 결과요청 </summary>
	private void REQ_RewardColosseum()
	{
		ZWebManager.Instance.WebGame.REQ_RewardColosseum((res, msg) => {

			isWin = (msg.IsWin == 1);
			SetDirectorState(E_DirectorState.Finish);

			CoroutineManager.Instance.StartTimer(TARGET_CAMERA_FINISH_DELAY, () => {
				UIManager.Instance.Open<UIPopupColosseumResult>((str, frame) => {
					var score = msg.Score;
					var colosseumTable = DBColosseum.FindByColosseumPoint(score);

					var resultInfo = new UIPopupColosseumResult.ResultInfo() {
						ColosseumTable = colosseumTable,
						IsWin = msg.IsWin,
						OldScore = msg.OldScore,
						OldRank = msg.OldRank,
						Score = score,
						Rank = msg.Rank,
					};

					frame.SetResult(resultInfo);

					var hudColosseum = UIManager.Instance.Find<UISubHUDColosseum>();
					if (hudColosseum != null) {
						hudColosseum.StopRamainTime();
					}
					else {
						ZLog.LogError(ZLogChannel.Default, "UISubHUDColosseum 가 null이다");
					}
				});
			});
		});
	}

	/// <summary> 콜로세움 강제로 퇴장을 알려준다. </summary>
	public static void RECV_LeaveColosseum(ZNet.ZWebRecvPacket recv)
	{
		ZGameManager.Instance.DoForceMapMove(DBConfig.Town_Stage_ID);
	}

	#endregion

}

