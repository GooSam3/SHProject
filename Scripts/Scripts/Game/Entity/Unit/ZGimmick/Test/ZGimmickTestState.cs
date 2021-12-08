using FlatBuffers;
using MmoNet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// </summary>
public class ZGimmickTestState : FSM.BaseState<ZGameManager>
{
    public ZPawnMyPc MyPc;

    public override void OnInitialize(ZGameManager _parent)
    {
        base.OnInitialize(_parent);
    }

    public override void OnEnter(Action callback, params object[] args)
    {
        base.OnEnter(callback, args);

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string sceneName = scene.name;

        var split = sceneName.Split('_');
        sceneName = sceneName.Replace(split[split.Length - 1], "");
        sceneName = sceneName + "Main";

        ZGameManager.Instance.SetupSceneGraphics(sceneName);

        PrepareTestGimmick();
    }

    private void PrepareTestGimmick()
    {
        // Addressable Object
        AddressableManagerLoader.Instance.Initialize();
        // Patch UI
        UIManagerRoot.Instance.Initialize();

        // Audio 
        AudioManager.Instance.Initialize();

        // Resource Set
        ResourceSetManager.Instance.Initialize();

        CameraManager.Instance.DoSetVisible(true);


        NTIcarusManager.Instance.InitializeSDK();
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

                // AuthURL 교체!
                //Parent.StarterData.ForceChangeAuthURL(serverAddr);
                // AuthURL로 우리게임서버로부터 정보 받아오기
                //mAuthManagement.StartAuth(Parent.StarterData.AuthUrl, NTCore.CommonAPI.SetupData.clientVersion);

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
    }

    private void HandlePatchInitialize()
    {
        StartPatchProcess();
    }

    private void StartPatchProcess()
    {
        ZManagerPatchDownloader.Instance.DoPatcherStartAssetBundle();
    }

    private void HandlePatchProgress(string _AddressableName, long _downloadedByte, long _totalByte, float _Progress, uint _loadCurrent, uint _loadMax)
    {
        ZLog.Log(ZLogChannel.Temple, $"{_AddressableName}, _{_Progress}");
    }

    private void HandlePatchDownloadSize(string _AddressableName, long _Size)
    {

    }

    private void HandlePatchEnd(string _AddressableName, byte[] _Null)
    {
        ZManagerPatchDownloader.Instance.DoPatcherStartTableFile(HandlePatchTableProgress, HandlePatchTableEnd, HandlePatchError, HandlePatchTableMessage);
    }

    private void HandlePatchError(CPatcherBase.E_PatchError _errorType, string _message = null)
    {
        string[] btntxt = new string[2] { "재시도", "취소" };
        Action[] callback = new Action[2] { delegate { }, delegate { Application.Quit(); } };

        ZLog.LogError(ZLogChannel.Temple, $"{_errorType}");
    }

    private void HandlePatchTableProgress(string _AddressableName, int _CurrentCount, int _TotalCount, float _Progress)
    {
        string message = $"{_CurrentCount}/{_TotalCount} - {_AddressableName} | {(float)_CurrentCount / (float)_TotalCount}";

        float Percent = (float)_CurrentCount / (float)_TotalCount;
        float ItemPercent = 1f / (float)_TotalCount;
        float CurrentPercent = Percent + (ItemPercent * _Progress);

        ZLog.Log(ZLogChannel.Temple, $"{message} - {CurrentPercent}%");
    }

    private void HandlePatchTableEnd()
    {
        List<string> cachePath = new List<string>();
        Caching.GetAllCachePaths(cachePath);
        if (cachePath.Count > 0)
        {
            Debug.LogWarning($"CachePath : {cachePath[0]}");
        }

        GameDBManager.Instance.ProgressUpdated = OnLoadGameDBProgress;
        GameDBManager.Instance.AllLoaded = OnLoadedGameDB;
        GameDBManager.Instance.Load($"{ZManagerPatchDownloader.Instance.GetPatcherTableFilePath()}", (tableName) =>
        {
            Debug.LogError("테이블 다운로드 다 받았는지 확인 필요!");
        });
    }

    private void OnLoadGameDBProgress(GameDBManager.TableLoadProgress _progress)
    {
        float Percent = (float)_progress.CurrentNo / (float)_progress.TotalCount;        
        ZLog.Log(ZLogChannel.Temple, $"Loading | {_progress.CurrentNo}/{_progress.TotalCount} | {_progress.Name}");
    }

    private void HandlePatchTableMessage(string _Message)
    {
        Debug.Log($"HandlePatchTableMessage : {_Message}");
    }

    private void OnLoadedGameDB()
    {
        CameraManager.Instance.DoSetVisible(true);

        var starterData = Parent.StarterData as ZGimmickStarterData;

        ZGameModeManager.Instance.SetStage(starterData.StageTid, 0);
        ZPawnManager.Instance.MyEntityId = 1;
        ZPawnManager.Instance.DoAddEventCreateEntity(OnAddedPawn);

        ZPawnManager.Instance.DoAdd(GetCharacterInfo(starterData.CharacterTableId, starterData.ChangeTableId));
    }

    private void OnAddedPawn(uint entityId, ZPawn pawn)
    {
        if (false == pawn.IsMyPc)
            return;

        MyPc = pawn.To<ZPawnMyPc>();
        MyPc.Warp(Parent.transform.position);        
        MyPc.DoAddEventLoadedModel(OnLoadedModel);        
    }

    private void OnLoadedModel()
    {
        MyPc.ChangeAnimatorForTemple();
        MyPc.DoRemoveEventLoadedModel(OnLoadedModel);
        MyPc.AbilityNotify(GameDB.E_AbilityType.FINAL_MAX_HP, 100);
        MyPc.AbilityNotify(GameDB.E_AbilityType.FINAL_CURR_HP, 100);
        MyPc.AbilityNotify(GameDB.E_AbilityType.FINAL_MOVE_SPEED, 5f);
        //MyPc.SetHp(100, 100);
    }

    /// <summary> flatbuffer 데이터 생성 </summary>
    private S2C_AddCharInfo GetCharacterInfo(uint characterId, uint changeId)
    {
        FlatBufferBuilder builder = new FlatBufferBuilder(1);
        var bb = S2C_AddCharInfo.CreateS2C_AddCharInfo(builder
            , ZPawnManager.Instance.MyEntityId
            , builder.CreateString("")
            , characterId
            , ServerPos3.CreateServerPos3(builder, 0, 0, 0)
            , 0
            , 0
            , ServerPos3.CreateServerPos3(builder, 0, 0, 0)
            , changeId
            , 0
            , 0
            , 0
            , 0);

        builder.Finish(bb.Value);
        return S2C_AddCharInfo.GetRootAsS2C_AddCharInfo(builder.DataBuffer);
    }

    public override void OnExit(Action callback)
    {
        base.OnExit(callback);
    }

    private void LoadManagerAudio()
    {
        if (AudioManager.hasInstance)
            return;

        Instantiate(Resources.Load("Defaults/AudioManager"));
    }

#if UNITY_EDITOR
    private bool bKeyMove = false;

    private void Update()
    {
        if (null == MyPc)
            return;

        Vector2 dir = Vector3.zero;
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.y = Input.GetAxisRaw("Vertical");

        if (Vector2.zero != dir) // 조이스틱 값 변화시
        {
            bKeyMove = true;
            float angle = VectorHelper.Axis2Angle(dir, true);

            if (CameraManager.Instance.Main != null)
            {

                Quaternion rot = Quaternion.Euler(
                    0,
                    CameraManager.Instance.Main.transform.rotation.eulerAngles.y + angle,
                    0);

                // 이동할 방향 구하기
                Vector3 newDir = rot * Vector3.forward;
                MyPc.MoveToDirection(MyPc.transform.position, newDir, MyPc.MoveSpeed, dir);
            }
        }
        else
        {
            if(true == bKeyMove)
			{
                bKeyMove = false;                
                MyPc.StopMove(MyPc.transform.position);
            }   
        }
    }
#endif
}
