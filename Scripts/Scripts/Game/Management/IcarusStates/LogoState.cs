using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LogoState : IcarusStateBase
{
	private enum E_LogoState
	{
		Auth,
		AssetBundle,
		TableFile,
		Error,
	}

	private E_LogoState mCurrentLogoState = E_LogoState.Auth;
	private string mScreenMessage;
	private float mProgressBar = 0;
	private float mGameDBLoadStartTime = 0;
	private UIFramePatchProcess mUIFramePatcher = null;

	public override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);
		UIManager.Instance.Open<UIFrameLogo>(delegate
		{
			StartCoroutine(ShowLogoRoutine());
		});
	}

	public override void OnExit(Action callback)
	{
		base.OnExit(callback);

		UIManager.Instance.DoSubCameraStack(CameraManager.Instance.PP, true);
	}

	//--------------------------------------------------------------------
	private IEnumerator ShowLogoRoutine()
	{
		mCurrentLogoState = E_LogoState.Auth;
		
		UIFrameLogo FrontLogo = UIManager.Instance.Find<UIFrameLogo>();
		yield return new WaitUntil(() => !FrontLogo.IsPlaying);

		//
		// 기본적인 준비가 끝나면,
		// Auth관리도구로 부터 정보 받아오기 시도
		//

		NTIcarusManager.Instance.StartSDK((result, exception) =>
		{
			if (result.success && null == exception)
			{
				// NTSDK관련 통합툴에 설정한 새로운 AuthURL주소 가져오기
				string serverAddr = NTCommon.NTJson.GetPropAsString(result.packetData, "server_addr");
				if (string.IsNullOrEmpty(serverAddr))
				{
					Debug.LogError("Does not have 'server_addr' key.");
					return;
				}

				string patchAddr = NTCommon.NTJson.GetPropAsString(result.packetData, "patch_addr");

				Auth.ServerAddr = serverAddr;
				Auth.PatchAddr = patchAddr;
				Auth.AssetUrlShortcut.Setup(patchAddr);

				OnAuthSettingComplete();
			}
			else
			{
				UICommon.OpenConsolePopup((UIPopupConsole _popup) => 
				{
					_popup.Open(
						ZLogLevel.Error.ToString(), 
						$"NTSDK초기화 및 약관설정 관련 실패!", 
						new string[] { "종료" }, 
						new Action[] { delegate { ZGameManager.Instance.QuitApp(); } });
				});
			}
		});
	}

	private void OnAuthSettingComplete()
	{
		ZLog.Log(ZLogChannel.Default, ZLog.ReplaceColorLog($"Auth설정 성공!\t {ZNetHelper.GetFieldStrings(NTCore.CommonAPI.SetupData)}", UnityEngine.Color.cyan));
		
		CPatcherBase.SPatchEvent EventHandler = ZManagerPatchDownloader.Instance.InitializePatcherURL(Auth.AssetUrlShortcut.BaseUrl, true);
		EventHandler.EventPatchInitComplete += HandlePatchInitialize;
		EventHandler.EventPatchDownloadSize += HandlePatchDownloadSize;
		EventHandler.EventPatchError += HandlePatchError;
		EventHandler.EventPatchProgress += HandlePatchProgress;
		EventHandler.EventPatchEnd += HandlePatchEnd;
		EventHandler.EventPatchLabelStart += HandlePatchLabelStart;
	}

	private void StartPatchProcess()
	{
		UIManager.Instance.Open<UIFramePatchProcess>((frameName, opendFrame) =>
		{
			// 대체할 UI가 열리고 나고, 기존UI닫기
			UIManager.Instance.Close<UIFrameLogo>(true);
			ZLog.BeginProfile("Total Patch Time");
			mUIFramePatcher = opendFrame;
			ZManagerPatchDownloader.Instance.DoPatcherStartAssetBundle();
		});

		NTIcarusManager.Instance.AdjustTrackEvent(NTIcarusManager.TOKEN_02_ResourceStart);
	}

	private void LoadAddressableUI()
	{
		ZManagerUIPreset.Instance.DoUIPresetLoadAtlas();
		UIManager.Instance.LoadRequiredUI(HandleUIFrameLoadFinish);		
	}

	//--------------------------------------------------------------------
	private void HandleUIFrameLoadFinish()
	{
		ZSceneManager.Instance.ImportWorldDescription(() =>
		{
			Debug.Log($"GameDB 로드 끝! Elapsed : {Time.realtimeSinceStartup - mGameDBLoadStartTime}");
			GoNextState();
		});
	}


	//--------------------------------------------------------------------
	private void HandlePatchInitialize()
	{
		mCurrentLogoState = E_LogoState.AssetBundle;
		ZManagerPatchDownloader.Instance.DoPatcherDownLoadSize((_eventFinish) => {
			// 다운로드 시작전에 전체 다운로드 규모를 알수 있다.  패치를 할건지 말건지 (와이파이확인) 메시지 박스 출력 가능 
			StartPatchProcess();
		});
	}

	private void HandlePatchDownloadSize(string _AddressableName, long _Size) {}

	private void HandlePatchProgress(string _AddressableName, long _downloadedByte, long _totalByte, float _Progress, uint _loadCurrent, uint _loadMax)
	{
		float Percent = _Progress * 100f;
		mProgressBar = _Progress;
		mScreenMessage = string.Format("{0:#.#}", Percent);

		mUIFramePatcher.UpdatePatchAssetBundle(_AddressableName, _Progress, _downloadedByte, _totalByte, _loadCurrent, _loadMax) ;
	}

	private void HandlePatchError(CPatcherBase.E_PatchError _errorType, string _message)
	{
		mCurrentLogoState = E_LogoState.Error;
		mScreenMessage = _errorType.ToString();
		string localeMessage = ZManagerPatchDownloader.Instance.ExtractLocalText(_errorType, _message);
		UICommon.OpenConsolePopup((UIPopupConsole _popup) => {
			_popup.Open(E_LogoState.Error.ToString(), localeMessage, new string[] { DBLocale.GetText("Patch_Retry"), DBLocale.GetText("Patch_End") }, new Action[] { delegate { StartPatchProcess(); }, delegate { ZGameManager.Instance.QuitApp(); } });
		});
	}

	private void HandlePatchEnd(string _AddressableName, byte[] _Null)
	{
		ZLog.EndProfile("Total Patch Time");
		mUIFramePatcher.SetPatchInfo("Patch_Download_Table", 0);
		ZManagerPatchDownloader.Instance.DoPatcherStartTableFile(HandlePatchTableProgress, HandlePatchTableEnd, HandlePatchError, HandlePatchTableMessage);
	}

	private void HandlePatchLabelStart(string _labelName)
	{
		if (_labelName == "Video") // 안드로이드 비디오 플레이어는 압축되면 오류가 발생한다
		{
			Caching.compressionEnabled = false;
		}
		else
		{
			Caching.compressionEnabled = true;
		}
	}

	//---------------------------------------------------------------------------------
	private void HandlePatchTableProgress(string _AddressableName, int _currentCount, int _totalCount, float _progress)
	{
		mProgressBar = _progress;
		mScreenMessage = $"{_currentCount}/{_totalCount} - {_AddressableName} | {(float)_currentCount / (float)_totalCount}";

		mUIFramePatcher.UpdatePatchTableDownload(_currentCount, _totalCount, _progress);
	}

	private void HandlePatchTableEnd()
	{
		List<string> cachePath = new List<string>();
		Caching.GetAllCachePaths(cachePath);
		if (cachePath.Count > 0)
		{
			Debug.LogWarning($"CachePath : {cachePath[0]}");
		}

		mGameDBLoadStartTime = Time.realtimeSinceStartup;
		mUIFramePatcher.SetPatchInfo("Patch_Read_Table", 0);
		GameDBManager.Instance.ProgressUpdated = OnLoadGameDBProgress;
		GameDBManager.Instance.AllLoaded = OnLoadedGameDB;
		GameDBManager.Instance.Load($"{ZManagerPatchDownloader.Instance.GetPatcherTableFilePath()}", (tableName) =>
		{
			UICommon.OpenConsolePopup((UIPopupConsole _popup) =>
			{
				_popup.Open(
					ZLogLevel.Error.ToString(),
					$"다운받은 테이블({tableName}) 데이터가 클라이언트 Table Schema와 맞지 않습니다.",
					new string[] { "종료" },
					new Action[] { delegate { ZGameManager.Instance.QuitApp(); } });
			});
		});

		NTIcarusManager.Instance.AdjustTrackEvent(NTIcarusManager.TOKEN_04_Resource_Complete);
	}

	private void HandlePatchTableMessage(string _Message)
	{
		Debug.Log($"HandlePatchTableMessage : {_Message}");
	}

#region ========:: for GameDB ::========

	private void OnLoadGameDBProgress(GameDBManager.TableLoadProgress _progress)
	{
		mUIFramePatcher.UpdatePatchTableRead(_progress.CurrentNo, _progress.TotalCount);
	}

	private void OnLoadedGameDB()
	{
		LoadAddressableUI();		
	}

#endregion
}