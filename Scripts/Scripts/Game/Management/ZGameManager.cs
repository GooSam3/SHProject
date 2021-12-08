using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WebSocketSharp;
using Zero;
using ZNet;

public enum E_GameEvent
{
	Next,
}

public enum E_GameState
{
	Start,

	Logo,
	Login,
	CharacterSelect,
	InGame
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// ** 클래스명 GameManager로하면, Project상 아이콘이 변해서 GameManager로 안함.
/// </remarks>
public class ZGameManager : Singleton<ZGameManager>
{
	/// <summary> 게임시작에 필요한 기본 정보들 </summary>
	public ZStarterDataBase StarterData => mStarterData;

	[Header("---- 게임 시작시 필수 설정 필요 ----")]
	[SerializeField]
	private ZStarterDataBase mStarterData = null;

	/// <summary> 현재 게임 상태 관리자 </summary>
	public FSM.FSM<E_GameEvent, E_GameState, ZGameManager> FSM { get; private set; }

	/// <summary> SVN Revision </summary>
	public string RevisionVer { get; private set; }

	public float GameStartTime { get; private set; }

	/// <summary> 앱 시작시 디바이스 기본 가로 크기 </summary>
	private float mOriginScreenWidth;
	/// <summary> 앱 시작시 디바이스 기본 세로 크기 </summary>
	private float mOriginScreenHeight;

	/// <summary> 길드던전내 채팅을 위한 RoomNo </summary>
	public ulong GuildDungeonRoomNo;

	/// <summary> 게임 입장중인지 여부 </summary>
	public bool IsEnterGameLoading { get { return (FSM.Current as InGameState)?.IsEnterGameLoading ?? false; } }

	public override void Awake()
	{
		ZLog.BeginProfile($"{nameof(ZGameManager.Awake)}");

		base.Awake();

		FetchRevision();

		GameStartTime = Time.realtimeSinceStartup;

		DefaultSettings();

		ZLog.EndProfile($"{nameof(ZGameManager.Awake)}");
	}

	private void FetchRevision()
	{
		TextAsset revisionTA = Resources.Load("RevisionVer") as TextAsset;
		if (null != revisionTA)
		{
			RevisionVer = revisionTA.text;
		}
	}

	private void DefaultSettings()
	{
		// 멀티 터치 지원
		Input.multiTouchEnabled = true;
		// 화면 안꺼지도록.
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// 게임 기본 옵션 불러오기.
		ZGameOption.Instance.EmptyFunction();

		mOriginScreenWidth = Screen.width;
		mOriginScreenHeight = Screen.height;

		// 화면이 일정수치보다 크다면, 해상도 720p 강제 조절 
		if (mOriginScreenHeight > 720)
		{
			var targetRatio = (float)720 / mOriginScreenHeight;

			// 해상도 최소/최대 범위 정해두기.
			targetRatio = Mathf.Clamp(targetRatio, 0.55f, 1f);

			SetResolution(targetRatio);
		}

        if(GameDBManager.Instance != null)
          DBLocale.DefaultLoad();

		SetupDevelopmentTools();
	}

	private void Start()
	{
		ZLog.BeginProfile($"{nameof(ZGameManager.Start)}");

		if (null == StarterData)
		{
			Debug.Assert(StarterData, $"{typeof(GameStarterDataBase<ZGameManager>)} 데이터는 무조건 존재해야함!");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		else
		{
			StartGame();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.pauseStateChanged += OnEditor_PauseStateChanged;
#endif
		}

		ZLog.EndProfile($"{nameof(ZGameManager.Start)}");
	}

	protected void StartGame()
	{
		FSM = new FSM.FSM<E_GameEvent, E_GameState, ZGameManager>(this);

		// 게임 시작!
		StarterData.DoStart(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.pauseStateChanged -= OnEditor_PauseStateChanged;
#endif
	}

	/// <summary>
	/// TODO : 기능 확장 필요
	/// </summary>
	public void QuitApp()
	{
		Application.Quit();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}

	private Task mGoToCharSelectTask = null;

	/// <summary>
	/// 게임 재시작용 함수 (작업중)
	/// </summary>
	public void Restart()
	{
		//
		// TODO : UI, Network, Scene 등 각종 초기화 채워보시오.
		//
		if (GameDBManager.hasInstance)
			GameDBManager.Instance.Unload();

		if (ZWebManager.hasInstance)
			ZWebManager.Instance.DisconnectAll();

		if (ZMmoManager.hasInstance)
			ZMmoManager.Instance.DisconnectAll();

		if (ZPawnManager.hasInstance)
			ZPawnManager.Instance.DoClear();

		if (CameraManager.hasInstance)
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Empty);

		TimeInvoker.Instance.ClearInvoke();

		if (UIManager.Instance != null)
		{
			UIManager.Instance.Clear();
		}

		if (ZPoolManager.Instance != null)
		{
			ZPoolManager.Instance.ClearCategory(E_PoolType.Character);
            ZPoolManager.Instance.ClearCategory(E_PoolType.Effect);
        }

		if( CoroutineManager.Instance != null ) {
			CoroutineManager.Instance.StopAllActions();
		}

		UnityEngine.SceneManagement.SceneManager.LoadScene("Start");

		FSM.ChangeState(E_GameState.Start);
	}

	/// <summary> 캐릭터 선택화면으로 돌아가기 </summary>
	public void GoCharacterSelectState()
	{
		if (null != mGoToCharSelectTask)
		{
			ZLog.LogError(ZLogChannel.System, $"GoCharacterSelectState | 이미 작동중입니다.");
			return;
		}

		mGoToCharSelectTask = new Task(E_GoCharacterSelectState());
	}

	private IEnumerator E_GoCharacterSelectState()
	{
		if (ZMmoManager.Instance.Field.IsConnected)
			ZMmoManager.Instance.Field.REQ_Logout();

		yield return new WaitForSeconds(0.5f);

		if (TutorialSystem.hasInstance)
			TutorialSystem.Instance.DestroyTutorial();

		if (ZMmoManager.hasInstance)
			ZMmoManager.Instance.DisconnectAll();

		if (ZPawnManager.hasInstance)
			ZPawnManager.Instance.DoClear();

		if (CameraManager.hasInstance)
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Empty);

		if (ZPoolManager.Instance != null)
		{
			ZPoolManager.Instance.ClearCategory(E_PoolType.Character);
			ZPoolManager.Instance.ClearCategory(E_PoolType.Effect);
			ZPoolManager.Instance.ClearCategory(E_PoolType.UI);
		}

		TimeInvoker.Instance.ClearInvoke();

		if (UIManager.Instance != null)
		{
			UIManager.Instance.Clear();
			UIManager.Instance.LoadRequiredUI(() =>
			{
				mGoToCharSelectTask = null;

				ZWebManager.Instance.Disconnect(E_WebSocketType.Chat);
				FSM.ChangeState(E_GameState.CharacterSelect);
			});
		}
	}

#if UNITY_EDITOR
	private void OnGUI()
	{
		FSM?.OnGUI_Dev();
	}

	private void OnEditor_PauseStateChanged(UnityEditor.PauseState _pauseState)
	{
		if (_pauseState == UnityEditor.PauseState.Paused)
		{
			Debug.LogWarning($"에디터 Pause걸림.\n");
		}
	}
#endif

	#region ========:: 인게임용 ::========

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_portalTid"></param>
	/// <param name="_channelId"></param>
	/// <param name="_useItemId"></param>
	/// <param name="_useItemTid"></param>
	/// <returns></returns>
	public bool TryEnterStage( uint _portalTid, bool _bChaosChannel, ulong _useItemId, uint _useItemTid, 
		string _forceServerAddr="", ushort _forceChannelId = 0, long _roomIdx = 0, Action enteredCB = null)
	{
		// 이 메소드까지 태웠다면 미리 떠있던 모달팝업은 닫어도 상황일꺼임 로딩스크린보다 위에뜨는 문제도 있고..
		// 통신장애나 그 다음 어떤 입장제한 조건들에 의해 다시 경고창이 열리는건 ok
		var frameHud = UIManager.Instance.Find<UIFrameHUD>();
		if( frameHud != null ) {
			frameHud.HideAllModalPopup();
		}

		//포탈 쿨타임 처리
		//TODO :: 패킷 쏘는 부분에서 처리되야할 것 같은데...
		if (false == ZWebManager.Instance.WebGame.CheckPortalCoolTime())
			return false;

		ZLog.Log( ZLogChannel.Default,
			$"스테이지입장정보, portalTid:{_portalTid},bChaosChannel:{_bChaosChannel},useItemId:{_useItemId},useItemTid:{_useItemTid},forceServerAddr:{_forceServerAddr},roomIdx:{_roomIdx}" );

		// 대상 스테이지가 현재 게임에서 사용가능한지 체크
		var portalTable = DBPortal.Get(_portalTid);
		if (!DBStage.IsStageUsable(portalTable.StageID))
		{
			ZLog.LogWarn(ZLogChannel.System, $"대상 스테이지[{portalTable.StageID}]는 현재 게임에서는 사용불가능으로 설정된 상태입니다.");
			return false;
		}

		// 입장 가능한 레벨인 체크
		var destStageTable = DBStage.Get(portalTable.StageID);
		if (ZNet.Data.Me.CurCharData.Level != Mathf.Clamp(ZNet.Data.Me.CurCharData.Level, destStageTable.InMinLevel, destStageTable.InMaxLevel))
		{
			ZLog.LogError(ZLogChannel.System, $"대상 스테이지[{destStageTable.StageID}]에 입장할 수 없는 레벨입니다.");
			return false;
		}

		// 돈이 있는지 체크
		if (portalTable.UseItemID>0 && ConditionHelper.CheckCompareCost(portalTable.UseItemID, portalTable.UseItemCount) == false)
			return false;

		// 맵로드를 해야 하는지 체크 
		bool RequireStageLoad = false;
		if (ZGameModeManager.Instance.StageTid != portalTable.StageID)
		{
			RequireStageLoad = true;
		}

		if (null != ZPawnManager.Instance.MyEntity)
		{
			if (ZPawnManager.Instance.MyEntity.IsMezState(GameDB.E_ConditionControl.NotReturn))
			{
				ZLog.Log(ZLogChannel.Entity, "이동 불가능한 상태다!!");
				return false;
			}
			// 케릭터의 모든 행동을 취소하여 디스커넥트된 서버에 패킷이 가지 않도록한다. 
			ZPawnManager.Instance.MyEntity.StopAllAction(true);
		}

        ZWebManager.Instance.WebGame.REQ_EnterPortal(_portalTid, _bChaosChannel, _useItemId, _useItemTid, (recvPacket, resPortalEnter) => 
		{
			ZLog.Log(ZLogChannel.System, $"EnterPortal 성공 | {resPortalEnter.StageTid}, {resPortalEnter.JoinAddr}");

			// MMO에 이동 상태 통보 후 다음 단계 진행 

			ushort channelId = ( _forceChannelId != 0 ) ? _forceChannelId : resPortalEnter.ChannelId;
			ZMmoManager.Instance.Field.REQ_MoveServer(channelId, (FlatBuffers.IFlatbufferObject _resMoveServer) =>
			{
				// 대상 스테이지로 입장하기
				if (FSM.Current is InGameState inGameState)
				{
					string enterMmoAdr = string.IsNullOrEmpty( _forceServerAddr ) == false ? _forceServerAddr : resPortalEnter.JoinAddr;
					inGameState.TryEnterGame(resPortalEnter.StageTid, resPortalEnter.ChannelId, enterMmoAdr, RequireStageLoad, false, false, _roomIdx );

					enteredCB?.Invoke();
				}
				else
				{
					Debug.LogError($"{nameof(TryEnterStage)}() : InGameState가 아닌 상태에서 불리면 잘못 사용한겁니다. Current[{FSM.Current_State}]");
				}
			});

		}, (err_, req_, res_) =>
		{
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
		});

		return true;
	}

	/// <summary>
	/// 보스전 캠프 입장
	/// </summary>
	/// <param name="_portalTid"></param>
	/// <param name="_bChaosChannel"></param>
	/// <param name="_useItemId"></param>
	/// <param name="_useItemTid"></param>
	/// <param name="_forceServerAddr"></param>
	/// <param name="_forceChannelId"></param>
	/// <param name="_roomIdx"></param>
	/// <param name="enteredCB"></param>
	/// <returns></returns>
	public bool TryEnterBossWarCamp(uint _portalTid, uint _useItemTid, Action enteredCB = null)
	{
		// 이 메소드까지 태웠다면 미리 떠있던 모달팝업은 닫어도 상황일꺼임 로딩스크린보다 위에뜨는 문제도 있고..
		// 통신장애나 그 다음 어떤 입장제한 조건들에 의해 다시 경고창이 열리는건 ok
		var frameHud = UIManager.Instance.Find<UIFrameHUD>();
		if (frameHud != null)
		{
			frameHud.HideAllModalPopup();
		}

		//포탈 쿨타임 처리
		//TODO :: 패킷 쏘는 부분에서 처리되야할 것 같은데...
		if (false == ZWebManager.Instance.WebGame.CheckPortalCoolTime())
			return false;

		// 대상 스테이지가 현재 게임에서 사용가능한지 체크
		var portalTable = DBPortal.Get(_portalTid);
		if (!DBStage.IsStageUsable(portalTable.StageID))
		{
			ZLog.LogWarn(ZLogChannel.System, $"대상 스테이지[{portalTable.StageID}]는 현재 게임에서는 사용불가능으로 설정된 상태입니다.");
			return false;
		}

		// 입장 가능한 레벨인 체크
		var destStageTable = DBStage.Get(portalTable.StageID);
		if (ZNet.Data.Me.CurCharData.Level != Mathf.Clamp(ZNet.Data.Me.CurCharData.Level, destStageTable.InMinLevel, destStageTable.InMaxLevel))
		{
			ZLog.LogError(ZLogChannel.System, $"대상 스테이지[{destStageTable.StageID}]에 입장할 수 없는 레벨입니다.");
			return false;
		}

		// 돈이 있는지 체크
		if (portalTable.UseItemID > 0 && ConditionHelper.CheckCompareCost(portalTable.UseItemID, portalTable.UseItemCount) == false)
			return false;

		// 맵로드를 해야 하는지 체크 
		bool RequireStageLoad = false;
		if (ZGameModeManager.Instance.StageTid != portalTable.StageID)
		{
			RequireStageLoad = true;
		}

		if (null != ZPawnManager.Instance.MyEntity)
		{
			if (ZPawnManager.Instance.MyEntity.IsMezState(GameDB.E_ConditionControl.NotReturn))
			{
				ZLog.Log(ZLogChannel.Entity, "이동 불가능한 상태다!!");
				return false;
			}
			// 케릭터의 모든 행동을 취소하여 디스커넥트된 서버에 패킷이 가지 않도록한다. 
			ZPawnManager.Instance.MyEntity.StopAllAction(true);
		}

		ZWebManager.Instance.WebGame.REQ_EnterBossWarCampStage(_portalTid, _useItemTid, (recvPacket, resCampEnter) =>
		{
			ZLog.Log(ZLogChannel.System, $"EnterPortal 성공 | {resCampEnter.StageTid}, {resCampEnter.JoinAddr}");

			// MMO에 이동 상태 통보 후 다음 단계 진행 

			ZMmoManager.Instance.Field.REQ_MoveServer(resCampEnter.ChannelId, (FlatBuffers.IFlatbufferObject _resMoveServer) =>
			{
				// 대상 스테이지로 입장하기
				if (FSM.Current is InGameState inGameState)
				{
					string enterMmoAdr = resCampEnter.JoinAddr;
					inGameState.TryEnterGame(resCampEnter.StageTid, resCampEnter.ChannelId, enterMmoAdr, RequireStageLoad, false, false, 0);

					enteredCB?.Invoke();
				}
				else
				{
					Debug.LogError($"{nameof(TryEnterStage)}() : InGameState가 아닌 상태에서 불리면 잘못 사용한겁니다. Current[{FSM.Current_State}]");
				}
			});

		}, (err_, req_, res_) =>
		{
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
		});

		return true;
	}

	public bool TryEnterBossWarField(uint _portalTid, Action enteredCB = null)
	{
		// 이 메소드까지 태웠다면 미리 떠있던 모달팝업은 닫어도 상황일꺼임 로딩스크린보다 위에뜨는 문제도 있고..
		// 통신장애나 그 다음 어떤 입장제한 조건들에 의해 다시 경고창이 열리는건 ok
		var frameHud = UIManager.Instance.Find<UIFrameHUD>();
		if (frameHud != null)
		{
			frameHud.HideAllModalPopup();
		}

		//포탈 쿨타임 처리
		//TODO :: 패킷 쏘는 부분에서 처리되야할 것 같은데...
		if (false == ZWebManager.Instance.WebGame.CheckPortalCoolTime())
			return false;

		// 대상 스테이지가 현재 게임에서 사용가능한지 체크
		var portalTable = DBPortal.Get(_portalTid);
		if (!DBStage.IsStageUsable(portalTable.StageID))
		{
			ZLog.LogWarn(ZLogChannel.System, $"대상 스테이지[{portalTable.StageID}]는 현재 게임에서는 사용불가능으로 설정된 상태입니다.");
			return false;
		}

		if (null != ZPawnManager.Instance.MyEntity)
		{
			if (ZPawnManager.Instance.MyEntity.IsMezState(GameDB.E_ConditionControl.NotReturn))
			{
				ZLog.Log(ZLogChannel.Entity, "이동 불가능한 상태다!!");
				return false;
			}
			// 케릭터의 모든 행동을 취소하여 디스커넥트된 서버에 패킷이 가지 않도록한다. 
			ZPawnManager.Instance.MyEntity.StopAllAction(true);
		}

		ZWebManager.Instance.WebGame.REQ_EnterBossWarFieldStage(_portalTid, (recvPacket, resCampEnter) =>
		{
			ZLog.Log(ZLogChannel.System, $"EnterBossWarFieldStage 성공 | {resCampEnter.StageTid}, {resCampEnter.JoinAddr}");

			// MMO에 이동 상태 통보 후 다음 단계 진행 
			ZMmoManager.Instance.Field.REQ_MoveServer(resCampEnter.ChannelId, (FlatBuffers.IFlatbufferObject _resMoveServer) =>
			{
				// 대상 스테이지로 입장하기
				if (FSM.Current is InGameState inGameState)
				{
					string enterMmoAdr = resCampEnter.JoinAddr;
					inGameState.TryEnterGame(resCampEnter.StageTid, resCampEnter.ChannelId, enterMmoAdr, false, false, false, 0);

					enteredCB?.Invoke();
				}
				else
				{
					Debug.LogError($"{nameof(TryEnterStage)}() : InGameState가 아닌 상태에서 불리면 잘못 사용한겁니다. Current[{FSM.Current_State}]");
				}
			});

		}, (err_, req_, res_) =>
		{
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
		});

		return true;
	}

	/// <summary>
	/// 길드던전 입장
	/// </summary>
	/// <param name="_portalTid"></param>
	/// <param name="enteredCB"></param>
	/// <returns></returns>
	public bool TryEnterGuildDungeon(uint _portalTid, Action enteredCB = null)
	{
		// 이 메소드까지 태웠다면 미리 떠있던 모달팝업은 닫어도 상황일꺼임 로딩스크린보다 위에뜨는 문제도 있고..
		// 통신장애나 그 다음 어떤 입장제한 조건들에 의해 다시 경고창이 열리는건 ok
		var frameHud = UIManager.Instance.Find<UIFrameHUD>();
		if (frameHud != null)
		{
			frameHud.HideAllModalPopup();
		}

		//포탈 쿨타임 처리
		//TODO :: 패킷 쏘는 부분에서 처리되야할 것 같은데...
		if (false == ZWebManager.Instance.WebGame.CheckPortalCoolTime())
			return false;

		// 대상 스테이지가 현재 게임에서 사용가능한지 체크
		var portalTable = DBPortal.Get(_portalTid);
		if (!DBStage.IsStageUsable(portalTable.StageID))
		{
			ZLog.LogWarn(ZLogChannel.System, $"대상 스테이지[{portalTable.StageID}]는 현재 게임에서는 사용불가능으로 설정된 상태입니다.");
			return false;
		}

		// 입장 가능한 레벨인 체크
		var destStageTable = DBStage.Get(portalTable.StageID);
		if (ZNet.Data.Me.CurCharData.Level != Mathf.Clamp(ZNet.Data.Me.CurCharData.Level, destStageTable.InMinLevel, destStageTable.InMaxLevel))
		{
			ZLog.LogError(ZLogChannel.System, $"대상 스테이지[{destStageTable.StageID}]에 입장할 수 없는 레벨입니다.");
			return false;
		}

		// 맵로드를 해야 하는지 체크 
		bool RequireStageLoad = false;
		if (ZGameModeManager.Instance.StageTid != portalTable.StageID)
		{
			RequireStageLoad = true;
		}

		if (null != ZPawnManager.Instance.MyEntity)
		{
			if (ZPawnManager.Instance.MyEntity.IsMezState(GameDB.E_ConditionControl.NotReturn))
			{
				ZLog.Log(ZLogChannel.Entity, "이동 불가능한 상태다!!");
				return false;
			}
			// 케릭터의 모든 행동을 취소하여 디스커넥트된 서버에 패킷이 가지 않도록한다. 
			ZPawnManager.Instance.MyEntity.StopAllAction(true);
		}

		ZWebManager.Instance.WebGame.REQ_GuildDungeonEnter(_portalTid, (recvPacket, resDungeonEnter) =>
		{
			ZLog.Log(ZLogChannel.System, $"EnterPortal 성공 | {resDungeonEnter.StageTid}, {resDungeonEnter.JoinAddr}");

			// MMO에 이동 상태 통보 후 다음 단계 진행 

			GuildDungeonRoomNo = resDungeonEnter.RoomNo;

			ZMmoManager.Instance.Field.REQ_MoveServer(0, (FlatBuffers.IFlatbufferObject _resMoveServer) =>
			{
				// 대상 스테이지로 입장하기
				if (FSM.Current is InGameState inGameState)
				{
					string enterMmoAdr = resDungeonEnter.JoinAddr;
					inGameState.TryEnterGame(resDungeonEnter.StageTid, 0, enterMmoAdr, RequireStageLoad, false, false, (long)resDungeonEnter.RoomNo);

					enteredCB?.Invoke();
				}
				else
				{
					Debug.LogError($"{nameof(TryEnterStage)}() : InGameState가 아닌 상태에서 불리면 잘못 사용한겁니다. Current[{FSM.Current_State}]");
				}
			});

		}, (err_, req_, res_) =>
		{
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
		});

		return true;
	}

	/// <summary> 강제로 맵이동시킨다 </summary>
	public void DoForceMapMove( uint stageTid, MmoNet.E_ForeceMoveMap_Reason reason )
	{
		UIManager.Instance.Close<UIControlBlock>();

		string reaseonText = string.Empty;

		if( reason == MmoNet.E_ForeceMoveMap_Reason.Close_Stage ) {
			reaseonText = "WPvP_Duel_UnableMatching";
		}
		else {
			ZLog.LogError( ZLogChannel.Default, $"서버에서 킥 이유를 알려줬지만 처리되지 않은 타입! 추가바람, reason:{reason}" );
		}

		// 메세지가 있는경우는 팝업이후에 나가기액션
		if( reaseonText.IsNullOrEmpty() == false ) {

			// 특정 모드의 결과창이 있을때는 나가기 팝업을 띄울 필요가 없다 결과창에서 나가도록 하자
			var colosseumResult = UIManager.Instance.Find<UIPopupColosseumResult>();
			if( colosseumResult != null && colosseumResult.Show ) {
				return;
			}

			UIMessagePopup.ShowPopupOk( DBLocale.GetText(reaseonText), () => {
				DoForceMapMove( stageTid );
			} );
		}
		else {
			DoForceMapMove( stageTid );
		}
	}

	public void DoForceMapMove( uint stageTid )
	{
		var stageTable = DBStage.Get( stageTid );
		var portalTable = DBPortal.Get( stageTable.DefaultPortal );
		if( portalTable == null ) {
			ZLog.LogError( ZLogChannel.Default, $"portalTable이 null이다, DefaultPortal:{stageTable.DefaultPortal}" );
			return;
		}
		TryEnterStage( portalTable.PortalID, false, 0, 0 );
	}

	public void DoChangeChannel(uint _channelID)
	{
		if (ZGameModeManager.Instance.ChannelId == _channelID) return;

		//포탈 쿨타임 처리
		//TODO :: 패킷 쏘는 부분에서 처리되야할 것 같은데...
		if (false == ZWebManager.Instance.WebGame.CheckPortalCoolTime())
		{
			return;
		}

		ZGameModeManager.SChannelData channelData = ZGameModeManager.Instance.GetChannelData(_channelID);
		// 카오스 체널은 입장 불가 (포탈로 이동가능)
		if (channelData == null) return;
		if (channelData.IsBossZone == true || channelData.IsPvPZone == true) return;

		if (null != ZPawnManager.Instance.MyEntity)
		{
			if (ZPawnManager.Instance.MyEntity.IsMezState(GameDB.E_ConditionControl.NotReturn))
			{
				ZLog.Log(ZLogChannel.Entity, "이동 불가능한 상태다!!");
				return;
			}
			// 케릭터의 모든 행동을 취소하여 디스커넥트된 서버에 패킷이 가지 않도록한다. 
			ZPawnManager.Instance.MyEntity.StopAllAction(true);
		}

		ZWebManager.Instance.WebGame.REQ_MMOChangeChannel(ZGameModeManager.Instance.StageTid, (ushort)_channelID, (_stageID, _enterChannelID, _address)=> {
			
			ZMmoManager.Instance.Field.REQ_MoveServer(_enterChannelID, (FlatBuffers.IFlatbufferObject _resMoveServer) =>
			{
				// 대상 스테이지로 입장하기
				if (FSM.Current is InGameState inGameState)
				{
					inGameState.TryEnterGame(_stageID, _enterChannelID, _address, false, false, false, 0);					
				}
				else
				{
					Debug.LogError($"{nameof(TryEnterStage)}() : InGameState가 아닌 상태에서 불리면 잘못 사용한겁니다. Current[{FSM.Current_State}]");
				}
			});
		}, (err_, req_, res_) =>
		{
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			ZWebManager.Instance.ProcessErrorPacket(err_, req_, res_, false);
		});
	}



	#endregion

	/// <summary> 현재 마켓 타입 웹서버용에 맞게 변환 </summary>
	public WebNet.E_MarketType GetMarketType()
	{
		switch (NTCore.CommonAPI.StoreCD)
		{
			case NTCore.StoreCD.GOOGLE_PLAY:		return WebNet.E_MarketType.Google;
			case NTCore.StoreCD.APPLE_APP_STORE:	return WebNet.E_MarketType.Apple;
			case NTCore.StoreCD.ONESTORE:			return WebNet.E_MarketType.OneStore;
			default:
				return WebNet.E_MarketType.None;
		}
	}

	#region ========:: Graphics ::========

	[ReadOnly(), Header("현재 Scene에 적용된 Settings체크용")]
	public ZSceneSettings CurSceneSettings = null;

	private int mStoredQualityLevel = -1;

	/// <summary>
	/// 해당 씬에 필요한 기본 그래픽 설정 셋업
	/// </summary>
	public void SetupSceneGraphics(string sceneName)
	{
		LoadSceneSettings($"SceneSettings_{sceneName}", (settingLoadHandle) => 
		{
			if (settingLoadHandle.Status == AsyncOperationStatus.Succeeded)
			{
				ApplySceneSetting(settingLoadHandle.Result);
			}
			else
			{
				ZLog.Log(ZLogChannel.System, ZLogLevel.Warning, $"현재 씬[{sceneName}]에 해당하는 SceneSettings가 존재하지 않습니다. 'SceneSettings_Common' 공용 셋팅이 적용됩니다.");
				 
				LoadSceneSettings("SceneSettings_Common", (commonLoadHandle) => 
				{
					if (commonLoadHandle.Status == AsyncOperationStatus.Succeeded)
					{
						ApplySceneSetting(commonLoadHandle.Result);
					}
				});
			}
		});		
	}

	public void SetupSceneGraphics()
	{
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		if (null == scene || !scene.isLoaded)
		{
			ZLog.Log(ZLogChannel.System, ZLogLevel.Warning, $"아직 Scene이 준비되지 않아 Scene전용 Graphics셋팅이 불가능합니다.");
			return;
		}
		SetupSceneGraphics(scene.name);
	}

	void LoadSceneSettings(string settingFileName, System.Action<AsyncOperationHandle<ZSceneSettings>> _onLoaded)
	{
		Addressables.LoadAssetAsync<ZSceneSettings>(settingFileName).Completed += _onLoaded;
	}

	/// <summary>
	/// 로드된 셋팅을 게임에 적용.
	/// </summary>
	/// <param name="newSettings"></param>
	void ApplySceneSetting(ZSceneSettings newSettings)
	{
		CurSceneSettings = newSettings;

		ZLog.Log(ZLogChannel.System, $"ZSceneSettings 로드 성공 : {CurSceneSettings.name}");

		if (CurSceneSettings.UseLightSetting)
		{
			Light[] foundLights = GameObject.FindObjectsOfType<Light>();
			foreach (var light in foundLights)
			{
				if (light.type != LightType.Directional ||
					!light.isActiveAndEnabled)
					continue;

				light.color = CurSceneSettings.LightColor;
				light.shadowStrength = CurSceneSettings.ShadowStrength;

				ZLog.Log(ZLogChannel.System, $"ZSceneSettings LIGHT!");
			}
		}

		// TODO : QualitySettings.GetQualityLevel()에 따른 대응이 필요함.
		CurSceneSettings.ApplyShaderSettings();
	}

	/// <summary> 임시로 현재 퀄리티 셋팅 변경하기 </summary>
	/// <remarks> <see cref="RestoreQualityLevel"/> 을 이용해서 기존 설정으로 복귀 가능 </remarks>
	public void ChangeQualityTemporary(E_Quality _quality)
	{
		if (mStoredQualityLevel == -1)
		{
			mStoredQualityLevel = QualitySettings.GetQualityLevel();
		}
		
		QualitySettings.SetQualityLevel((int)_quality);

		ZLog.Log(ZLogChannel.System, $"{nameof(ChangeQualityTemporary)}() | StoredQualityLevel: {mStoredQualityLevel}, SetQualityLevel: {(int)_quality}[{_quality}]");
	}

	public void RestoreQualityLevel()
	{
		QualitySettings.SetQualityLevel((int)mStoredQualityLevel);

		mStoredQualityLevel = -1;

		ZLog.Log(ZLogChannel.System, $"{nameof(RestoreQualityLevel)}() | SetQualityLevel: {QualitySettings.GetQualityLevel()}");
	}

	#endregion

	/// <summary>
	/// 기본 해상도 변경
	/// </summary>
	public void SetResolution(float newScreenRatio, bool bFullscreen = true)
	{
		int newWidth = (int)(mOriginScreenWidth * newScreenRatio);
		int newHeight = (int)(mOriginScreenHeight * newScreenRatio);

		NTCore.ScreenHelper.SetResolution(newWidth, newHeight, bFullscreen);

		ZLog.Log(ZLogChannel.System, $"Changed Resolution: {mOriginScreenWidth}x{mOriginScreenHeight} -> {newWidth}x{newHeight}, newScreenRatio: {newScreenRatio}");
	}

	/// <summary>
	/// 개발 빌드에 일때 디버깅용 컴포넌트들 추가
	/// </summary>
	private void SetupDevelopmentTools()
	{
		if (!Debug.isDebugBuild)
			return;

		if (null != gameObject)
			gameObject.GetOrAddComponent<UniFPSCounter>();

#if !UNITY_EDITOR
		Instantiate(Resources.Load("Debug/Reporter"));
#endif
	}

#if UNITY_EDITOR

	/// <summary>
	/// https://docs.unity3d.com/kr/2019.4/Manual/DomainReloading.html 대응함수
	/// </summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatic()
	{
		Debug.Log("<color=blue>!! ResetStatic !!</color>");

		List<System.Type> derivedTypes = new List<System.Type>();

		// 현재 프로젝트에서 사용중인 모든 Assembly를 돌면서, Type들 수집
		var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			derivedTypes.AddRange(assembly.GetTypes().ToList());
		}

		foreach (System.Type type in derivedTypes)
		{
			if (!type.IsClass || type.BaseType == null)
				continue;

			if (type.BaseType.Name != "Singleton`1")
				continue;

			MethodInfo method = type.GetMethod("ClearStatics", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			if (null != method)
			{
				method.Invoke(null, null);

				//Debug.Log($"{type.Name} | CallInvoke");
			}
			else
			{
				Debug.LogError($"Singleton<>.ClearStatics() 정적함수가 존재하지는지 확인바람.");
			}
		}

		ZNet.Data.Global.ClearMe();
	}

#endif
}
