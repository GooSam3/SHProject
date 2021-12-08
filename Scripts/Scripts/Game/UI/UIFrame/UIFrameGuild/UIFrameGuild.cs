using Devcat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet;
using ZNet.Data;

public class UIFrameGuild : ZUIFrameBase
{
    [Serializable]
    public class TabGroupDefine
    {
        public List<FrameGuildTabType> tabs;
    }

    [Serializable]
    public class TabSingleDefine
    {
        public FrameGuildTabType tab;
        public UIFrameGuildTabBase target;
    }

    //#region TEST 
    //public FrameGuildTabType TestTabTypeToOpen;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Z))
    //    {
    //        OpenTab(TestTabTypeToOpen);
    //    }
    //}
    //#endregion


    #region SerializedField
    #region Preference Variable
    [Tooltip("길드 UI 가 켜질때 처음에 보일 탭 들입니다. ")]
    [SerializeField] private FrameGuildTabType[] firstTabs;
    #endregion

    #region UI Variables
    [SerializeField] private List<TabSingleDefine> states;
    [SerializeField] private List<TabGroupDefine> tabGroupDefined;
    [SerializeField] private List<GuildOverlayPopup> overlayPopups;
    [SerializeField] private GameObject BackgroundObject;
    private Camera subSceneCamera;
    [SerializeField] private Transform rayTarget;
    #endregion
    #endregion

    #region Public Variables
    #endregion

    #region System Variables
    private EnumDictionary<UpdateEventType, Action<UpdateEventType, GuildDataUpdateEventParamBase>> dataUpdateEventHandlers;

    Coroutine checkPrefabLoadedCo;
    private bool tabStatesInit;
    private bool isOnShowProcessDone;
    private bool firstOpen;

    /// <summary> 길드던전 </summary>
    private const string SUBSCENE_MODELVIEW = "UIModelView";
    private UIPopupItemInfo InfoPopup;
    private Transform viewRoot;
    private Action EventLoadComplete;
    private GameObject loaded3DModel;
    private bool IsLoadScene = false;
    private bool IsLoadCompleteBossModel = true;
    #endregion

    #region Properties 
    public bool IsTabStateInit { get => tabStatesInit; }
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    public override bool IsBackable => true;
	protected override void OnInitialize()
    {
        base.OnInitialize();

#if _GTEST_
        UIFrameGuildNetCapturer.SetupTestData();
#endif

        var updateDataTypes = (UpdateEventType[])Enum.GetValues(typeof(UpdateEventType));

        dataUpdateEventHandlers = new EnumDictionary<UpdateEventType, Action<UpdateEventType, GuildDataUpdateEventParamBase>>();

        foreach (var type in updateDataTypes)
        {
            dataUpdateEventHandlers.Add(type, null);
        }
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        firstOpen = true;

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);

        ZLog.Log(ZLogChannel.UI, "Guild OnShow");

        isOnShowProcessDone = false;
        UIManager.Instance.ShowGlobalIndicator(true);

        // 비동기 방식 로드방식 때문에 코루틴으로 프리팹 로드 여부 체크함 . 
        // 코루틴쓰기때문에 OnShow() 에서 함 
        InitTabStatesIfNeeded(() =>
        {
            if(!IsLoadScene)
			{
                EventLoadComplete = LoadGuildInfo;
                LoadSubScene();
			}
            else
			{
                LoadGuildInfo();
			}
        });
    }

    private void LoadGuildInfo()
	{
        // 길드 있는 경우 , 없는 경우 필요한 데이터가 다르기때문에 분기해서 
        // 필요한 정보 업데이트후에 진행함. 
        StartCoroutine(RefreshAllData_Server(
            onEarlyGetGuildInfoFinished: () =>
            {
                OpenTabSequence(firstTabs);
            },
            onFinished: () =>
            {
                for (int i = 0; i < firstTabs.Length; i++)
                {
                    if (IsTabOpen(firstTabs[i]) == false)
                    {
                        OpenTab(firstTabs[i]);
                    }
                }

                UIManager.Instance.ShowGlobalIndicator(false);
                isOnShowProcessDone = true;
            }
            , onError: (err, req, res) =>
            {
                UIFrameGuildTabBase.HandleError(err, req, res);
                UIManager.Instance.Close<UIFrameGuild>();
            }));

        CloseOverlayPopup();
    }

    private void OnDisable()
    {
    }

    protected override void OnHide()
    {
        base.OnHide();

		if (isOnShowProcessDone == false)
			UIManager.Instance.ShowGlobalIndicator(false);

		RemoveInfoPopup();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
        CloseAllTabs();

        UnloadSubScene();

		if (UIManager.Instance.Find(out UISubHUDMenu _menu))
		{
			if (Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == WebNet.E_GuildMemberGrade.SubMaster)
			{
                _menu.ActiveRedDot(E_HUDMenu.Guild, UIFrameGuildNetCapturer.IsRedDotStatusDirty());
            }
			else
			{
                _menu.ActiveRedDot(E_HUDMenu.Guild, false);
            }
		}
	}
    #endregion

    #region Public Methods
    public UIFrameGuildTabBase OpenTab(FrameGuildTabType tabType)
    {
        UIFrameGuildTabBase target = null;

        // 일단 해당 그룹 가져옴 
        var group = FindGroup(tabType);

        if (group == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Failed to find the group : " + tabType.ToString());
            return null;
        }

        // 그룹을 돌면서 찾는 state 는 enter , 켜져있는 다른 애는 exit 함 
        foreach (var tab in group.tabs)
        {
            var state = FindTab(tab);

            if (state != null)
            {
                // 내가 키려는 탭 발견  
                if (state.tab.Equals(tabType))
                {
                    target = state.target;

                    // 근데 이미 켜져있다면 아무것도 하지않음 
                    if (state.target.IsOpen == false)
                    {
                        if(tabType == FrameGuildTabType.Dungeon)
						{
                            BackgroundObject.SetActive(false);
						}
                        else
						{
                            BackgroundObject.SetActive(true);
						}

                        state.target.OnOpen();
                    }
                }
                // 내가 키려는 탭이 아닌 같은 그룹내의 다른 탭이라면 꺼준다 
                // 물론 켜져있을 때만  
                else if (state.target.IsOpen)
                {
                    state.target.OnClose();
                }
            }
        }

        return target;
    }

    public UIFrameGuildTabBase CloseTab(FrameGuildTabType tabType)
    {
        UIFrameGuildTabBase target = null;

        // 일단 해당 그룹 가져옴 
        var group = FindGroup(tabType);

        if (group == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Failed to find the group : " + tabType.ToString());
            return null;
        }

        foreach (var tab in group.tabs)
        {
            var state = FindTab(tab);

            if (state != null)
            {
                if (state.tab.Equals(tabType) && state.target.IsOpen)
                {
                    state.target.OnClose();
                    target = state.target;
                    break;
                }
            }
        }

        return target;
    }

    public void OpenOverlayPopup(OverlayWindowPopUP type)
    {
        foreach (var popup in overlayPopups)
        {
            if (popup.Type.Equals(type))
            {
                popup.Obj.Open();
            }
            else if (popup.Obj.IsOpen)
            {
                popup.Obj.Close();
            }
        }
    }

    public void CloseOverlayPopup(OverlayWindowPopUP type)
    {
        var target = overlayPopups.Find(t => t.Type.Equals(type));

        if (target != null)
        {
            target.Obj.Close();
        }
    }

    public void CloseOverlayPopup()
    {
        foreach (var popup in overlayPopups)
        {
            if (popup.Obj.IsOpen || popup.Obj.gameObject.activeSelf)
            {
                popup.Obj.Close();
            }
        }
    }

    public T RetrieveTabComponent<T>()
        where T : UIFrameGuildTabBase
    {
        foreach (var state in states)
        {
            if (state.target is T)
            {
                return state.target as T;
            }
        }

        return null;
    }

    public bool IsTabOpen(FrameGuildTabType tab)
    {
        foreach (var t in states)
        {
            if (t.tab == tab)
            {
                return t.target.IsOpen;
            }
        }

        return false;
    }

    public void OnAllServerDataRefreshRequested(Action onFinished, bool skipGetInfo = false)
    {
        StartCoroutine(RefreshAllData_Server(
            onFinished: () =>
            {
                onFinished?.Invoke();
            },
            onError: (err, req, res) =>
            {
                UIFrameGuildTabBase.HandleError(err, req, res);
                UIManager.Instance.Close<UIFrameGuild>();
            }
            , skipGetGuildInfo: skipGetInfo));
    }

    public void NotifyUpdateEvent(UpdateEventType type, GuildDataUpdateEventParamBase param = null)
    {
        foreach (var state in states)
        {
            if (state.target.IsOpen)
            {
                state.target.OnUpdateEventRise(type, param);
            }
        }
        //        dataUpdateEventHandlers[type]?.Invoke(type, param);
    }

	public void RequestAllianceInfoUpdate()
	{
		NotifyUpdateEvent(UpdateEventType.RequestAllianceInfo
			, new EventParam_ReqAllianceState(
				new WebNet.E_GuildAllianceState[]
				{
					WebNet.E_GuildAllianceState.Alliance
					, WebNet.E_GuildAllianceState.Enemy
					, WebNet.E_GuildAllianceState.RequestAlliance
					, WebNet.E_GuildAllianceState.ReceiveAlliance
				})
			);
	}

    public static bool RequestRefreshAllianceInfo_ViaBroadcast(ulong executorCharID)
	{
        /// 내가 발생시킨 브로드캐스트 메시지는 무시한다. (중복 패킷 발생함.)
        if (executorCharID != Me.CurCharData.ID)
        {
            if (Me.CurCharData.GuildId != 0)
            {
                if (UIManager.Instance.Find<UIFrameGuild>(out var guildFrame))
                {
                    if (guildFrame.Show)
                    {
                        guildFrame.RequestAllianceInfoUpdate();
                        return true;
                    }
                }
            }
        }

        return false; 
    }

    // 데이터 업데이트 이벤트 함수 추가 
    //public void AddListener_UpdateDataEvent(UpdateEventType dataEventType, Action<UpdateEventType, GuildDataUpdateEventParamBase> listener)
    //{
    //    dataUpdateEventHandlers[dataEventType] += listener;
    //}

    // 데이터 업데이트 이벤트 함수 제거
    //public void RemoveListener_UpdateDataEvent(UpdateEventType dataEventType, Action<UpdateEventType, GuildDataUpdateEventParamBase> listener)
    //{
    //    dataUpdateEventHandlers[dataEventType] -= listener;
    //}

    #endregion

    #region Private Methods
    private IEnumerator CheckPrefabLoaded(Action onLoadedFinished)
    {
        List<string> preloadPrefabs = new List<string>();
        List<string> loadedPrefabs = new List<string>();

        // 필요한 프리팹들 미리 로딩다해놓고 들어가야함 . 
        preloadPrefabs.Add(nameof(UIGuildRecommendListSlot));
        preloadPrefabs.Add(nameof(UIGuildRequestForCharListSlot));
        preloadPrefabs.Add(nameof(UIGuildBuffListSlot));
        preloadPrefabs.Add(nameof(UIGuildCreateGuildSlot));
        preloadPrefabs.Add(nameof(GuildInfoTabScrollSlot));
        preloadPrefabs.Add(nameof(ScrollGuildMemberListSlot));
        preloadPrefabs.Add(nameof(ScrollGuildRankingListSlot));
        preloadPrefabs.Add(nameof(ScrollGuildJoinListSlot));
        preloadPrefabs.Add(nameof(ScrollGuildSettingGridSlot));
        preloadPrefabs.Add(nameof(ScrollGuildRequestAllyListSlot));
        preloadPrefabs.Add(nameof(UIGuildDungeonListItem));
        preloadPrefabs.Add(nameof(UIRewardableListItem));

        foreach (var prefabName in preloadPrefabs)
        {
            //           if (ZPoolManager.Instance.FindClone(E_PoolType.UI, prefabName) == null)
            //        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, prefabName, delegate
            {
                ZLog.Log(ZLogChannel.UI, "GuildPrefab Loaded : " + prefabName);
                loadedPrefabs.Add(prefabName);
            }, bActiveSelf: false);
            //       }
            //else
            //{
            //    loadedPrefabs.Add(prefabName);
            //}
        }

        bool ready = false;

        while (ready == false)
        {
            yield return null;

            if (preloadPrefabs.Count == loadedPrefabs.Count)
            {
                ready = true;
            }
        }

        onLoadedFinished?.Invoke();
    }

    private IEnumerator RefreshAllData_Server(
        Action onFinished = null
        , Action onEarlyGetGuildInfoFinished = null
        , Action<ZWebCommunicator.E_ErrorType, ZWebReqPacketBase, ZWebRecvPacket> onError = null
        , bool skipGetGuildInfo = false
        , bool notifyUpdateChain = true)
    {
        float operationElapsedTime = 0;

        if (Me.CurCharData.GuildId != 0)
        {
            bool loaded = false;
            bool failed = false;

            if (skipGetGuildInfo == false)
            {
                // GET 하면서 capturer 에서 데이터 my guild data 업데이트함 . 
                UIFrameGuildNetCapturer.ReqGetGuildInfo(Me.CurCharData.GuildId,
                    (revPacketRec, resListRec) =>
                    {
                        if (firstOpen)
                        {
                            firstOpen = false;
                            onEarlyGetGuildInfoFinished?.Invoke();
                        }

                        loaded = true;
                    }
                    , (err, req, res) =>
                     {
                         onError?.Invoke(err, req, res);
                         failed = true;
                     });

                while (loaded == false)
                {
                    if (failed)
                    {
                        yield break;
                    }

                    operationElapsedTime += Time.deltaTime;
                    if (operationElapsedTime >= 8)
                        UIManager.Instance.Close<UIFrameGuild>();
                    yield return null;
                }
            }

            failed = false;
            loaded = false;

            // 연맹 정보 리스트 요청함.  
            UIFrameGuildNetCapturer.ReqGetGuildAllianceList(
                Me.CurCharData.GuildId
                , new WebNet.E_GuildAllianceState[] {
                    WebNet.E_GuildAllianceState.Alliance
                    , WebNet.E_GuildAllianceState.Enemy
                    , WebNet.E_GuildAllianceState.RequestAlliance
                    , WebNet.E_GuildAllianceState.ReceiveAlliance}
                , (revPacketRec, resListRec) =>
                {
                    loaded = true;
                }, (err, req, res) =>
                {
                    onError?.Invoke(err, req, res);
                    failed = true;
                });

            while (loaded == false)
            {
                if (failed)
                {
                    yield break;
                }

                operationElapsedTime += Time.deltaTime;
                if (operationElapsedTime >= 8)
                    UIManager.Instance.Close<UIFrameGuild>();
                yield return null;
            }

            loaded = false;
            failed = false;

            // 길드 랭킹 리스트 요청함 . 
            UIFrameGuildNetCapturer.ReqGetGuildExpRank(
                (revPacketRec, resListRec) =>
                {
                    loaded = true;
                }, (err, req, res) =>
                {
                    onError?.Invoke(err, req, res);
                    failed = true;
                });

            while (loaded == false)
            {
                if (failed)
                {
                    yield break;
                }

                operationElapsedTime += Time.deltaTime;
                if (operationElapsedTime >= 8)
                    UIManager.Instance.Close<UIFrameGuild>();
                yield return null;
            }

            loaded = false;
            failed = false;

            // 길드입장서 가입 요청 요청함. 
            UIFrameGuildNetCapturer.ReqGuildRequestListForGuild(
                Me.CurCharData.GuildId
                , (revPacketRec_03, resListRec_03) =>
                {
                    loaded = true;
                }, (err, req, res) =>
                {
                    onError?.Invoke(err, req, res);
                    failed = true;
                });

            while (loaded == false)
            {
                if (failed)
                {
                    yield break;
                }

                operationElapsedTime += Time.deltaTime;
                if (operationElapsedTime >= 8)
                    UIManager.Instance.Close<UIFrameGuild>();
                yield return null;
            }
        }
        else
        {
            bool loaded = false;
            bool failed = false;

            // 길드가 없는 경우 
            UIFrameGuildNetCapturer.ReqRecommendGuildInfo(
                (revPacketRec, resListRec) =>
                {
                    loaded = true;
                }, (err, req, res) =>
                {
                    onError?.Invoke(err, req, res);
                    failed = true;
                });

            if (loaded == false)
            {
                if (failed)
                {
                    yield break;
                }

                operationElapsedTime += Time.deltaTime;
                if (operationElapsedTime >= 8)
                    UIManager.Instance.Close<UIFrameGuild>();
                yield return null;
            }

            loaded = false;
            failed = false;

            UIFrameGuildNetCapturer.ReqGuildRequestListForChar(
                (revPacketReq, resListReq) =>
                {
                    loaded = true;
                }, (err, req, res) =>
                {
                    onError?.Invoke(err, req, res);
                    failed = true;
                });

            while (loaded == false)
            {
                if (failed)
                {
                    yield break;
                }

                operationElapsedTime += Time.deltaTime;
                if (operationElapsedTime >= 8)
                    UIManager.Instance.Close<UIFrameGuild>();
                yield return null;
            }
        }

        if (notifyUpdateChain)
            NotifyUpdateEvent(UpdateEventType.DataAllRefreshed);
        onFinished?.Invoke();
    }

    private void InitTabStatesIfNeeded(Action onInitFinished)
    {
        if (tabStatesInit)
        {
            onInitFinished?.Invoke();
            return;
        }

        if (checkPrefabLoadedCo != null)
        {
            StopCoroutine(checkPrefabLoadedCo);
            checkPrefabLoadedCo = null;
        }

        // 필요 prefab 들 로딩된 후에 Init 진행함 . 
        checkPrefabLoadedCo = StartCoroutine(CheckPrefabLoaded(() =>
        {
            foreach (var state in states)
            {
                state.target.Initialize(this, state.tab);
            }

            foreach (var popup in overlayPopups)
            {
                popup.Obj.Initialize(this);
            }

            tabStatesInit = true;
            checkPrefabLoadedCo = null;

            onInitFinished?.Invoke();
        }));
    }

    private void OpenTab(UIFrameGuildTabBase tab)
    {
        tab.OnOpen();
    }

    private void CloseTab(UIFrameGuildTabBase tab)
    {
        tab.OnClose();
    }

    private void OpenTabSequence(params FrameGuildTabType[] tabs)
    {
        if (tabs != null)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                OpenTab(tabs[i]);
            }
        }
    }

    private void CloseAllTabs()
    {
        foreach (var state in states)
        {
            var t = state.target;

            if (t.IsOpen)
            {
                t.OnClose();
            }
        }
    }

    private void ForeachByType(Action<UIFrameGuildTabBase, FrameGuildTabType> action, FrameGuildTabType type)
    {
        var typeGroup = FindGroup(type);

        if (typeGroup == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Please define a group for the type");
            return;
        }

        foreach (var t in typeGroup.tabs)
        {
            var tab = FindTab(t);
            action(tab.target, t);
        }
    }

    private TabGroupDefine FindGroup(FrameGuildTabType type)
    {
        foreach (var group in tabGroupDefined)
        {
            foreach (var tabType in group.tabs)
            {
                if (tabType.Equals(type))
                {
                    return group;
                }
            }
        }

        return null;
    }

    private TabSingleDefine FindTab(FrameGuildTabType type)
    {
        return states.Find(t => t.tab.Equals(type));
    }

    public void RemoveInfoPopup()
    {
        if (InfoPopup != null)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }
    }

    public void SetInfoPopup(UIPopupItemInfo popup)
    {
        if (InfoPopup)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }

        InfoPopup = popup;
    }
#if UNITY_EDITOR

    public bool bAutoUpdateViewRoot = true;

    private void LateUpdate()
    {
        if (bAutoUpdateViewRoot == false || IsLoadScene == false) return;
        
        SetViewRootPosition();
    }

#endif
    private void LoadSubScene()
    {
        ZSceneManager.Instance.OpenAdditive(SUBSCENE_MODELVIEW, (float _progress) =>
        {
            ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
        }, (temp) =>
        {
            IsLoadScene = true;

            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(SUBSCENE_MODELVIEW);

            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                if (obj.name == "Root")
                {
                    Vector3 originPosition = obj.transform.position;
                    Vector3 targetPostiion = UIManager.Instance.transform.position;
                    targetPostiion.z = 0;
                    obj.transform.position = targetPostiion + originPosition;
                }

                viewRoot = obj.transform.FindTransform("ViewRoot");
                subSceneCamera = obj.transform.FindTransform("SubSceneCamera")?.GetComponent<Camera>();

                if(subSceneCamera!=null)
				{
                    subSceneCamera.transform.localPosition = new Vector3(0, 1.24f, -3.3f);
                }
            }

            OnSubSceneLoaded();
        });
    }

    private void OnSubSceneLoaded()
    {
        // 로드완료했는데 frame이 꺼져있으면 다시 언로드

        if(subSceneCamera!=null)
            UIManager.Instance.DoSubCameraStack(subSceneCamera, true);

        if (!Show)
        {
            UnloadSubScene();
            return;
        }

        CoroutineManager.Instance.NextFrame(() =>
        {
            SetViewRootPosition();
        });
        
        EventLoadComplete?.Invoke();
        EventLoadComplete = null;
    }

    private void SetViewRootPosition()
    {
        if (subSceneCamera == null)
            return;

        var ray = subSceneCamera.ScreenPointToRay(subSceneCamera.WorldToScreenPoint(rayTarget.position));

        if (Physics.Raycast(ray, out var hitInfo, 100f))
        {
            var vec = viewRoot.position;
            vec.x = hitInfo.point.x;
            viewRoot.position = vec;
        }
    }

    public void UnloadSubScene()
    {
        if (IsLoadScene)
        {
            IsLoadScene = false;

            if (subSceneCamera != null)
            {
                UIManager.Instance.DoSubCameraStack(subSceneCamera, false);
                subSceneCamera = null;
            }

            ZSceneManager.Instance.CloseAdditive(SUBSCENE_MODELVIEW, null);
        }
    }

    public void ShowBossModel(GameDB.Stage_Table stageTable)
    {
        IsLoadCompleteBossModel = false;

        var monsterTable = DBMonster.Get(stageTable.SummonBossID);
        if (monsterTable == null)
        {
            ZLog.LogError(ZLogChannel.Default, $"DBMonster가 null이다, SummonBossID:{stageTable.SummonBossID}");
            return;
        }

        var resourceTable = DBResource.Get(monsterTable.ResourceID);
        if (resourceTable == null)
        {
            ZLog.LogError(ZLogChannel.Default, $"DBResource가 null이다, ResourceID:{monsterTable.ResourceID}");
            return;
        }

        ShowModel(resourceTable.ResourceFile, monsterTable.ViewScale, stageTable.BossViewPosY);
    }

    private void ShowModel(string resFile, uint viewScale, float viewPosY)
    {
        if (string.IsNullOrEmpty(resFile) == false)
        {
			UnityEngine.AddressableAssets.Addressables.InstantiateAsync(resFile).Completed += (obj) => {
                Action prepareAction = () => {
                    if (loaded3DModel != null)
                    {
                        UnityEngine.AddressableAssets.Addressables.ReleaseInstance(loaded3DModel);
                        loaded3DModel = null;
                    }

                    loaded3DModel = obj.Result;
                    loaded3DModel.SetLayersRecursively("UIModel");

                    LODGroup lodGroup = loaded3DModel.GetComponent<LODGroup>();
                    if (null != lodGroup)
                    {
                        lodGroup.ForceLOD(0);
                    }

                    loaded3DModel.transform.SetParent(viewRoot);

                    Vector3 pos = Vector3.zero;
                    pos.y = viewPosY;
                    loaded3DModel.transform.localPosition = pos;
                    loaded3DModel.transform.localScale = Vector3.one * viewScale * .01f;
                    loaded3DModel.transform.localRotation = Quaternion.Euler(0, -30, 0);

                    //소켓 세팅
                    foreach (GameDB.E_ModelSocket socket in Enum.GetValues(typeof(GameDB.E_ModelSocket)))
                    {
                        Transform socketTrans = loaded3DModel.transform.FindTransform($"Socket_{socket}");

                        if (null == socketTrans)
                            continue;
                    }

                    IsLoadCompleteBossModel = true;
                };

                prepareAction.Invoke();
            };
        }
    }

    #endregion

    #region OnClick Event (인스펙터 연결)

    #endregion
}
