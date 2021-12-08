using GameDB;
using System;
using System.Collections;
using UnityEngine;
using ZNet.Data;
using static ZNet.MmoSessionBase;

public class InGameState : IcarusStateBase
{
	/// <summary> 입장 처리중인지 여부 </summary>
	public bool AlreadyTryingEnter { get; private set; }
	/// <summary> 입장 처리중인 스테이지 테이블 (로딩 완료시 null) </summary>
	private Stage_Table CurTryingStageTable { get; set; }

	//  UI 로딩이 끝났는지 
	private bool UILoadingFinish = false;

	/// <summary> 게임 입장중인지 여부 </summary>
	public bool IsEnterGameLoading { get; private set; }

	public override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);
		UILoadingFinish = false;

		// 캐릭터 데이터 로드와 메인맵 로딩이 끝났을 때 실행될 콜백
		ZMultiCallback.AddMultiCallback(new MultiCallbackData(nameof(InGameState), 3, delegate
		{
			// 데이터 로드와 맵 로딩이 끝나는 시점이 다르므로 둘 다 끝나는 시점에서 실행할 함수는 이 곳에 등록
			if (UIManager.Instance.Find(out UISubHUDCharacterState _charstate)) _charstate.Init();
			if (UIManager.Instance.Find(out UISubHUDCharacterAction _charaction)) _charaction.Init();
			if (UIManager.Instance.Find(out UIGainSystem _gainSystem)) _gainSystem.SetEvent();
			if (UIManager.Instance.Find(out UISubHUDMenu _menu))
			{
				_menu.CheckSlotWeight();
				_menu.ActiveRedDot(E_HUDMenu.Mailbox, Me.CurCharData.MailList.Count > 0 || Me.CurCharData.GetNotReadMessage());
				_menu.ActiveRedDot(E_HUDMenu.Friend, Me.CurCharData.GetReceiveRequestFriend().Count > 0);
			}

			//ZWebManager.Instance.WebGame.GetMailRefreshTime();
			TimeInvoker.Instance.RequestInvoke(ZWebManager.Instance.WebGame.GetMailRefreshTime, 60f);

			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIQuickItemSlotPageHolder), obj=>
			{
				if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick)) _quick.Initialize();

				ZPoolManager.Instance.Return(obj);
			});

			if (_menu != null)
			{
				_menu.CheckGuildRedDot();

				_menu.CheckGodLandRedDot(true);
			}
		}));

		UIManager.Instance.Open<UIFrameLoadingScreen>();
		UIManager.Instance.Open<UIFrameMessageCharacter>();
		UIManager.Instance.Open<UIFrameHUD>(delegate
		{
			UIManager.Instance.Find<UIFrameHUD>().Init(delegate
			{
				UILoadingFinish = true;
				ZMultiCallback.CheckMultiCallback(nameof(InGameState));
			});

			ushort mmoChannerId = 0;
#if UNITY_EDITOR
			mmoChannerId = (ushort)PlayerPrefs.GetInt("SelectMMOChannelId", 0);
#endif
			// TODO : 패킷 하나하나당 예외처리 필요
			ZWebManager.Instance.WebGame.REQ_EnterMMOServer(mmoChannerId, (recvPacket, resMmoStageEnter) =>
			{
				TryEnterGame(resMmoStageEnter.StageTid, resMmoStageEnter.ChannelId, resMmoStageEnter.JoinAddr, true, true, true, 0);
			});
		});
	}

	public override void OnExit(Action callback)
	{
		UIManager.Instance.Close<UIFrameHUD>(true);

		//Game Mode 초기화
		ZGameModeManager.Instance.SetStage(0, 0);
		base.OnExit(callback);
	}

	/// <summary>
	/// 
	/// </summary>
	public bool TryEnterGame(uint _enterStageTid, ushort _enterChannelId, string _enterMmoAddr, bool _RequireStageLoad, bool _forceReconntect, bool _bFirstEnter = false, long _roomIdx = 0)
	{
		// UI업데이트 중지 
		UIManager.Instance.SetUIManagerUpdate(false);

		if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
			_quick.IsAutoCheck = false;

		if (_RequireStageLoad == false)
			UIManager.Instance.ShowGlobalIndicator(true);

		if (AlreadyTryingEnter)
		{
			// 일단 에러나도 Pass
			ZLog.Log(ZLogChannel.Loading, ZLogLevel.Error, $"{nameof(InGameState.TryEnterGame)} | 이미 시도중인데, 다시 시도하는건 머지? Info : newStageTid : {_enterStageTid}, newMmoAddr : {_enterMmoAddr}");
			return false;
		}

		if (!DBStage.TryGet(_enterStageTid, out var stageTable))
		{
			ZLog.Log(ZLogChannel.Loading, ZLogLevel.Error, $"{nameof(InGameState.TryEnterGame)} | StageTID[{_enterStageTid}] 테이블 정보가 존재하지 않습니다.");
			return false;
		}

		AlreadyTryingEnter = true;
		CurTryingStageTable = stageTable;

		StartCoroutine(EnterGameRoutine(stageTable, _enterChannelId, _enterMmoAddr, _RequireStageLoad, _forceReconntect, _bFirstEnter, _roomIdx));

		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	private IEnumerator EnterGameRoutine(Stage_Table _stageTable, ushort _enterChannelId, string _enterMmoAddr, bool _RequireStageLoad, bool _forceReconnect, bool _bFirstEnter, long _roomIdx)
	{
		// 서버에 접속할지 여부 (주소가 없다면 싱글모드로 간주)
		bool doConnectToServer = false;
		System.Uri createdUrl;
		if (System.Uri.TryCreate(string.Format("http://{0}", _enterMmoAddr), System.UriKind.Absolute, out createdUrl))
		{
			doConnectToServer = true;
		}

		if (doConnectToServer)
		{
			// 주소와 포트를 비교 후 커넥션을 다시 설정할지 결정한다.
			string CurrentAddress = ZMmoManager.Instance.Field.Address;
			ushort CurrentPort = ZMmoManager.Instance.Field.Port;
			bool bReconnect = (CurrentAddress != createdUrl.Host || CurrentPort != (ushort)createdUrl.Port || _forceReconnect);

			yield return StartCoroutine(EnterGameLoading(_stageTable, _enterChannelId, createdUrl.Host, (ushort)createdUrl.Port, _RequireStageLoad, bReconnect, _bFirstEnter, _roomIdx));
		}

		ZLog.Log(ZLogChannel.Loading, $"{nameof(InGameState.EnterGameRoutine)} | EnterStageTID: {_stageTable.StageID}, EnterMmoAddr: {_enterMmoAddr} 로드 성공!");

		AlreadyTryingEnter = false;
		CurTryingStageTable = null;
	}

	private IEnumerator EnterGameLoading(Stage_Table _stageTable, ushort _enterChannelId, string _ipAddr, ushort _port, bool _RequireStageLoad, bool _bReconnect, bool _bFirstEnter, long _roomIdx)
	{
		IsEnterGameLoading = true;
		if (_bReconnect)
		{
			// 연결 끊길때 까지 기다려보자.
			ZMmoManager.Instance.Disconnect(E_MmoType.Field);
			while (ZMmoManager.Instance.Field.IsConnected)
				yield return null;
		}

		//** MmoGame 연결시도
		if (_bReconnect)
		{
			ConnectionSetting connSetting = new ConnectionSetting(_ipAddr, _port);
			ZMmoManager.Instance.ConnectField(connSetting);
		}

		bool bReceivedAllCharInfo = false;

		//** 캐릭터의 모든 정보 요청
		ZWebManager.Instance.WebGame.REQ_GetAllCharInfoBundle(Me.UserID, Me.CharID,
			ZNet.ZWebGame.CharacterAllBits, _bFirstEnter, true, delegate
			{
                // 유료재화 정보 요청
                Me.CurUserData.RefreshCash((long _cash) =>
                {
                    //일일 이벤트 정보 요청
                    ZWebManager.Instance.WebGame.REQ_CheckDailyResetEvent((recvPacket, recvMsgPacket) =>
                    {
                        // 인게임 이벤트 정보 요청
                        ZWebManager.Instance.WebGame.REQ_EventDataAll(delegate
                        {
                            // 강림 파견 정보 요청
                            ZWebManager.Instance.WebGame.REQ_ResetChangeDailyQuest(delegate
                            {
								// 유료출석 갱신 요청
								ZWebManager.Instance.WebGame.REQ_RefreshCashAttendEvent(delegate
								{
									// 출석 보상 요청( 점검등의 이유로 보상받을 리스트)
									ZWebManager.Instance.WebGame.REQ_CheckLoginEvent(delegate
									{
										// 리셋이후에 일부 패킷 재요청해야 한다..
										//ZWebManager.Instance.WebGame.REQ_GetAllCharInfoBundle(Me.UserID, Me.CharID,
										//ZNet.ZWebGame.CharacterAllBits, false, true, delegate
										//{
										ZMultiCallback.CheckMultiCallback(nameof(InGameState));

										// 케릭터 정보를 기반으로 퀘스트를 최초 갱신
										UIManager.Instance.Find<UIFrameQuest>().DoUIQuestInitialize();

										bReceivedAllCharInfo = true;
										//});
									});
								});
                            });
                        });
                    });
                });

				if (UIManager.Instance.Find(out UISubHUDMenu _menu))
					_menu.ActiveNewAlarm(UISubHUDMenu.E_TopMenuButton.Bag, false);

				Me.CurCharData.ClearNewGainItemList();
			});

		yield return new WaitUntil(() => bReceivedAllCharInfo);

		//** Scene 로드시도
		string sceneLoadingMessage = "";
		bool loadingFinish = false;

		if (_bReconnect || _RequireStageLoad)
		{
			CameraManager.Instance.DoChangeCameraMotor(E_CameraMotorType.Empty);

			ZPawnManager.Instance.DoClear();
			ZGimmickManager.Instance.Clear();

			ZMultiCallback.CheckMultiCallback(nameof(InGameState));
			UIManager.Instance.Find<UIFrameNameTag>().DoUINameTagClearAll();
			if (UIManager.Instance.Find(out UISubHUDMiniMap _minimap)) _minimap.DoMinimapMapMarkerClearAll();

			if (ZPoolManager.Instance != null)
			{
				ZPoolManager.Instance.ClearCategory(E_PoolType.Character);
				ZPoolManager.Instance.ClearCategory(E_PoolType.Effect);
			}
		}

		if (_RequireStageLoad)
		{
			//로딩 페이지 나오기전에 페이드 연출
			yield return Co_Fade(E_UIFadeType.FadeIn, 0.5f);

			ZSceneManager.Instance.OpenMain(_stageTable.ResourceFileName, (float _Progress) =>
			{
				sceneLoadingMessage = _Progress.ToString();
			},
			(string _AddressableName) =>
			{
				ZGameManager.Instance.SetupSceneGraphics();
				loadingFinish = true;
				sceneLoadingMessage = string.Format(" SceneLoading Finish.");
				UIManager.Instance.Open<UIFrameNameTag>();

			});

			//로딩 페이지 나온후 페이드 연출
			yield return Co_Fade(E_UIFadeType.FadeOut, 0.5f);
		}
		else
		{
			loadingFinish = true;

			//스테이지 로딩 필요 없을 경우 바로 fade in
			yield return Co_Fade(E_UIFadeType.FadeIn, 0.5f);
		}

		// mmo연결 기다림
		if (_bReconnect)
		{
			while (!ZMmoManager.Instance.Field.IsConnected)
			{
				ZLog.Log(ZLogChannel.Loading, $"MmoField연결중..");
				yield return null;
			}
		}

		//이전 스테이지
		var prevStageTid = ZGameModeManager.Instance.StageTid;
		Stage_Table prevStageTable = null;
		if (prevStageTid != 0)
			prevStageTable = DBStage.Get(prevStageTid);

		//게임 모드 변경
		ZGameModeManager.Instance.SetStage(_stageTable.StageID, _enterChannelId, _roomIdx);

		// 서버 시간 동기화
		ZMmoManager.Instance.Field.REQ_ServerTime();

		// scene로드되는 도중에 필드 연결하기.
		// TODO :: Room 정보 입력해야함.
		ZMmoManager.Instance.Field.REQ_JoinField(ZNet.Data.Me.UserID, ZNet.Data.Me.CharID, ZNet.Data.Me.CurCharData.Nickname, Me.SelectedServerID, _roomIdx);

		// scene로드 기다림. 예외처리 필요.
		while (!loadingFinish || null == ZPawnManager.Instance.MyCharInfo || 0 == ZPawnManager.Instance.MyEntityId)
		{
			ZLog.Log(ZLogChannel.Loading, $"SceneLoading : {sceneLoadingMessage} / Mmo캐릭터 정보[{null != ZPawnManager.Instance.MyCharInfo}] 대기중..");
			yield return new WaitForSeconds(0.2f);
		}

		//스테이지 로딩했을 경우 fade in
		if (true == _RequireStageLoad)
		{
			yield return Co_Fade(E_UIFadeType.FadeIn, 0.5f);
		}

		if (ZWebManager.Instance.WebChat.IsUsable && ZWebManager.Instance.WebChat.IsUserConnected)
		{
			ZWebManager.Instance.WebChat.ExitChannelAll(ZWebManager.Instance.WebChat.CheckEnterChannel);
		}
		else if(ZWebManager.Instance.WebChat.IsUserConnected == false)
		{
			ZWebManager.Instance.WebChat.ConnectInitialize(Me.SelectedServerID, Me.UserID, Me.CharID, () =>
			{
				ZWebManager.Instance.WebChat.CheckEnterChannel();
			});
		}
		else
		{
			ZWebManager.Instance.ConnectWebChat(ZNet.Data.Me.ChatServerUrl);
		}

		//카메라 준비
		CameraManager.Instance.DoResolveMainCamera();

		// TODO : 옮겨야겠죠?
		AudioManager.Instance.Play(_stageTable.BGMID);

		// REQ_JoinField 후,
		// Scene로드 완료 & 객체 생성 준비 완료시, 서버에 객체정보 동기화 요청한다.
		ZMmoManager.Instance.Field.REQ_LoadMapOK();

		yield return new WaitUntil(() => null != ZPawnManager.Instance.MyEntity);

		while (!UILoadingFinish)
		{
			yield return null;
		}

		if (_RequireStageLoad == false)
			UIManager.Instance.ShowGlobalIndicator(false);

		UIManager.Instance.Close<UIFrameLoadingScreen>();

		UIManager.Instance.Find<UIFrameHUD>().RefreshSubHud(_stageTable.StageType, prevStageTable != null ? prevStageTable.StageType : E_StageType.None);
		UIManager.Instance.Find<UIFrameQuest>().QuestChecker.EventStageMove(ZGameModeManager.Instance.StageTid);
		UIManager.Instance.SetUIManagerUpdate(true);
		UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);

		if (_stageTable.StageType == E_StageType.Temple)
			CameraManager.Instance.SetMainCameraRender(E_RenderType.IngameShrine);
		else
			CameraManager.Instance.SetMainCameraRender(E_RenderType.InGameDefault);

		if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
			_quick.IsAutoCheck = true;

		//TODO :: 임시 씬로딩 완료시 이벤트 처리 -> GameMode 관련 스크립트 추가할듯?
		ZPawnManager.Instance.SceneLoadedComplete();
		ZGameModeManager.Instance.SceneLoadComplete();

		IsEnterGameLoading = false;
	}

	private IEnumerator Co_Fade(E_UIFadeType fadeType, float duration)
	{
		bool bWaitFade = true;
		UICommon.FadeInOut(() =>
		{
			bWaitFade = false;
		}, fadeType, duration);

		while (true == bWaitFade)
		{
			yield return null;
		}
	}
}