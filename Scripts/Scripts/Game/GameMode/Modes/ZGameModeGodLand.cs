using System;
using ZNet.Data;
using UnityEngine;
using System.Collections.Generic;

public sealed class ZGameModeGodLand : ZGameModeBase
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

		public bool IsFinished;
		public float HideDelayTime;
		public float ShowDelayTime;

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
		}
	}

	public override E_GameModeType GameModeType { get { return E_GameModeType.GodLand; } }

	protected override E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.DefaultCheckState | E_GameModePrepareStateType.CreateMyPc;

	private const float TARGET_CAMERA_ENTER_SHOW_DELAY = 0.5f;
	private const float TARGET_CAMERA_ENTER_HIDE_DELAY = 2.0f;
	private const float TARGET_CAMERA_FINISH_DELAY = 2.0f;

	private Action<List<GodLandStatInfoConverted>> UserListCallback;
	public Action RemainTimeCallback { private get; set; }

	private List<DirectorCameraInfo> directorCamInfoList = new List<DirectorCameraInfo>();
	private E_DirectorState directorState = E_DirectorState.Nothing;
	
	protected override void ExitGameMode()
	{
		UIManager.Instance.Close<UISubHUDGodLand>();
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
		directorState = E_DirectorState.Nothing;
		Me.CurCharData.GodLandContainer.Clear();
		directorCamInfoList.Clear();
	}

	#region 이벤트 /////////////////////////////////////////////////////////

	/// <summary> 이벤트 </summary>
	public void SetUserListCallback(Action<List<GodLandStatInfoConverted>> callback)
	{
		if (Me.CurCharData.GodLandContainer.UserInfoList.Count == 2) {
			callback?.Invoke(Me.CurCharData.GodLandContainer.UserInfoList);
			return;
		}
		UserListCallback = callback;
	}

	public void SetRemainTimeCallback(Action callback)
	{
		if (Me.CurCharData.GodLandContainer.RemainTimeTargetTime > 0) {
			callback?.Invoke();
			return;
		}
		RemainTimeCallback = callback;
	}

	#endregion

	#region 연출관련 ///////////////////////////////////////////////////////

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

						//모두 카메라 보여준뒤 상태전환
						if (directorCamInfoList.Exists(v => v.IsFinished == false) == false) {
							SetDirectorState(E_DirectorState.Start);
						}
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

						if (info.Entity.IsMyPc) {
							UICommon.SetNoticeMessage(
								enterMsg, ParadoxNotion.ColorUtils.HexToColor("64daff"), 0.8f, UIMessageNoticeEnum.E_MessageType.SubNotice);
						}
						else {
							UICommon.SetNoticeMessage(
								enterMsg, ParadoxNotion.ColorUtils.HexToColor("FF7777"), 0.8f, UIMessageNoticeEnum.E_MessageType.SubNotice);
						}
					}
				}
				break;
			}
			case E_DirectorState.Start: {
				UIManager.Instance.Close<UIControlBlock>();

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
				SetTargetCamera(myEntity);

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
	}

	#endregion

	#region 대기 //////////////////////////////////////////////////////////

	/// <summary> 입장직후 시뮬레이션 유저 정보, 내꺼 상대꺼 따로옴 </summary>
	public void RECV_GodLandStatInfo(MmoNet.S2C_GodLandStatInfo info)
	{
		Me.CurCharData.GodLandContainer.UserInfoList.Add(new GodLandStatInfoConverted(info));
		if (Me.CurCharData.GodLandContainer.UserInfoList.Count == 2) {
			UserListCallback?.Invoke(Me.CurCharData.GodLandContainer.UserInfoList);
			UserListCallback = null;
		}
	}

	public override void RECV_StageState(MmoNet.S2C_StageState info)
	{
		if (info.State == 0) {
		}
		else if (info.State == 1) {
			UIManager.Instance.Close<UIControlBlock>();

			var container = Me.CurCharData.GodLandContainer;
			container.RemainTimeTargetTime = info.RemainSec + TimeManager.NowSec;
			RemainTimeCallback?.Invoke();
			RemainTimeCallback = null;
		}
	}

	#endregion

	#region 종료 //////////////////////////////////////////////////////////

	/// <summary> 성지 시뮬레이션 결과 </summary>
	public void RECV_GodLandFinishInfo(MmoNet.S2C_GodLandFinishInfo info)
	{
		SetDirectorState(E_DirectorState.Finish);

		CoroutineManager.Instance.StartTimer(TARGET_CAMERA_FINISH_DELAY, () => {
			var hudGodLand = UIManager.Instance.Find<UISubHUDGodLand>();
			if (hudGodLand != null) {
				hudGodLand.SetResult(info.GodLandTid, info.IsWin);
			}
		});
	}

	#endregion
}
