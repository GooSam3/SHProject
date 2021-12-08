using GameDB;
using System;
using System.Collections.Generic;


/// <summary> 게임 모드 </summary>
public abstract class ZGameModeBase : FSM.BaseState<ZGameModeManager>
{
	public abstract E_GameModeType GameModeType { get; }

	/// <summary> 체크할 게임 모드 </summary>
	protected virtual E_GameModePrepareStateType CheckPrepareStateType { get; } = E_GameModePrepareStateType.None;

	/// <summary> 현재 게임모드의 준비 상태 </summary>
	protected E_GameModePrepareStateType CurrentPrepareStateType = E_GameModePrepareStateType.None;

	/// <summary> 추후 처리해볼까 </summary>
	//protected virtual List<string> UINames => new List<string>();

	public bool IsStartGameMode { get; private set; }

	public Stage_Table Table { get { return ZGameModeManager.Instance.Table; } }

	public sealed override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);

		IsStartGameMode = false;
		CurrentPrepareStateType = E_GameModePrepareStateType.None;
		EnterGameMode();
	}

	public sealed override void OnExit(Action callback)
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity(CreateMyEntity);

		IsStartGameMode = false;
		CurrentPrepareStateType = E_GameModePrepareStateType.None;
		ExitGameMode();
		base.OnExit(callback);
	}

	/// <summary> 게임 모드 변경 후 바로 호출됨. 아직 모든 데이터가 정상적으로 준비되진 않음. </summary>
	protected virtual void EnterGameMode()
	{

	}

	/// <summary> 게임 모드 종료시 호출됨 </summary>
	protected virtual void ExitGameMode()
	{

	}

	public virtual void CreateEntity(bool isMyPc, uint entityId, ZPawn entity)
	{
		if (isMyPc) {
			ZPawn myEntity = ZPawnManager.Instance.MyEntity;
			CameraManager.Instance.DoSetTarget(myEntity.transform);
			E_CameraMotorType lastMotor = CameraManager.Instance.LoadCameraMotorType();
			CameraManager.Instance.DoChangeCameraMotor(lastMotor, Cinemachine.CinemachineBlendDefinition.Style.Cut);
		}
	}

	public virtual void RemoveEntity(bool isMyPc, uint entityId)
	{
		if (isMyPc) {
			CameraManager.Instance.DoSetTarget(null);
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Empty);
		}
	}

	public virtual void DieEntity(uint attackerEntityId, bool isMyPcDead, ZPawn deadPawn)
	{
	}

	/// <summary> mmo joinfield </summary>
	public void JoinField()
	{
		OnJoinField();
		SetModePrepareState(E_GameModePrepareStateType.MMO_JoinField);
	}

	/// <summary> join field 완료 후 각 게임모드 구현부 </summary>
	protected virtual void OnJoinField()
	{

	}

	/// <summary> mmo loadmapod </summary>
	public void LoadMapOK()
	{
		OnLoadMapOK();

		SetModePrepareState(E_GameModePrepareStateType.MMO_LoadMapOk);

		//LoadMapOK 이후에 내 캐릭터 체크한다.
		ZPawnManager.Instance.DoAddEventCreateMyEntity(CreateMyEntity);
	}

	/// <summary> load map ok 완료 후 각 게임모드 구현부 </summary>
	protected virtual void OnLoadMapOK()
	{

	}

	public virtual void RECV_StageState(MmoNet.S2C_StageState info)
	{
	}
	public virtual void RECV_GameStart(MmoNet.S2C_GameStart info)
	{
	}

	public virtual void RECV_GameScore(MmoNet.S2C_GameScore info)
	{
	}

	public virtual void RECV_RoomInfo(MmoNet.S2C_RoomInfo info)
	{
	}

	public virtual void RECV_InstanceFinish(MmoNet.S2C_InstanceFinish info)
	{
	}

	/// <summary> 내 캐릭터 생성됨 </summary>
	private void CreateMyEntity()
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity(CreateMyEntity);

		OnCreateMyEntity();
		SetModePrepareState(E_GameModePrepareStateType.CreateMyPc);
	}

	/// <summary> 내 캐릭터 생성 후 각 게임모드 구현부</summary>
	protected virtual void OnCreateMyEntity()
	{

	}

	/// <summary> 씬로드가 완료됨 </summary>
	public void SceneLoadComplete()
	{
		OnSceneLoadComplete();
		SetModePrepareState(E_GameModePrepareStateType.SceneLoadComplete);
	}

	/// <summary> 씬로드 완료시 각 게임모드 구현부 </summary>
	protected virtual void OnSceneLoadComplete()
	{

	}

	/// <summary> 맵 데이터가 로드됨 </summary>
	public void MapDataLoadComplete()
	{
		OnMapDataLoadComplete();
		SetModePrepareState(E_GameModePrepareStateType.MapDataLoadComplete);
	}

	/// <summary> 맵 데이터 로드 완료시 각 게임모드 구현부 </summary>
	protected virtual void OnMapDataLoadComplete()
	{

	}

	/// <summary> 현재 게임모드의 준비 상태를 업데이트 한다. </summary>
	protected void SetModePrepareState(E_GameModePrepareStateType flag)
	{
		CurrentPrepareStateType |= flag;	
		if(false == IsStartGameMode && CheckModePrepareState())
		{
			IsStartGameMode = true;
			PreStartGameMode();
			UICommon.FadeInOut(() =>
			{
				StartGameMode();
			}, E_UIFadeType.FadeOut);
			
		}
	}

	/// <summary> 현재 게임모드의 준비 상태 체크. </summary>
	private bool CheckModePrepareState()
	{		
		return (CheckPrepareStateType) == (E_GameModePrepareStateType)((int)CheckPrepareStateType & (int)CurrentPrepareStateType);
	}
	
	/// <summary> 모든 준비가 끝났다. 게임을 시작하자 </summary>
	protected abstract void StartGameMode();

	/// <summary> Fade 이전에 셋팅할 부분 처리 </summary>
	protected virtual void PreStartGameMode()
	{

	}
}

