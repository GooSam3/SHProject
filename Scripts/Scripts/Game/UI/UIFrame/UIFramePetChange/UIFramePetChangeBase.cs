using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCRContentBase : MonoBehaviour
{
    public virtual void ShowContent() { }
    public virtual void HideContent() { }
}

// 씬, 모델, 메뉴
public class UIFramePetChangeBase : ZUIFrameBase
{
    [Serializable]
    public class C_ContentUseObject
    {
        // 컨텐츠 에 사용되는 오브젝트, 켜고 끌때 사용됨
        public List<GameObject> UseObject;

        public E_PetChangeContentType ContentType;

        public Transform RayTarget;

        public bool Use3DModel = false;

        public bool isOn { get; private set; } = false;

        public PCRContentBase Content;

        public void SetState(bool state)
        {
            isOn = state;
        }
    }

    public enum E_PetChangeContentType
    {
        None = -1,     // 없음,(예약안됨)
        Content_1 = 0, // 강림
        Content_2 = 1, // 합성
        Content_3 = 2, // 파견
        Content_4 = 3, // 교체
        Content_5 = 4, // 컬렉션
    }

    private struct QueuedResFile
    {
        public S_PCRResourceData resFile;
        public bool isReady;

        public void Reset(S_PCRResourceData _resFile)
        {
            resFile = _resFile;
            isReady = !string.IsNullOrEmpty(resFile.FileName);
        }
    }

    private const string SUBSCENE_MODELVIEW = "UIModelView";

    #region UI Variable

    [Header("SET TYPE!!!!")]
    [SerializeField] protected E_PetChangeViewType ViewType;

    [SerializeField] private GameObject BackGround;// 씬 로드될때까지 켜줌

    [SerializeField] protected List<C_ContentUseObject> ListContentObject;
    [SerializeField] private ZToggleGroup toggleGroupMenuTab;

    #endregion

    #region System Variable


    //-----설계미스---- 컨텐츠 모두 구현 후 수정
    // 목록은 모든 소환수들에 적용될듯하니 베이스에 배치
    [SerializeField] protected PCContentListViewBase ContentListView;

    // 합성두
    [SerializeField] protected PCContentCombineBase ContentCombine;

    [SerializeField] protected PCContentReplaceBase ContentReplace;

    [SerializeField] protected PCContentCollectionBase ContentCollection;

    // 디테일 정보 팝업
    // 씬을 사용하는 팝업이라 내부에 위치, 씬의 해제는 본 클래스에서만 함
    [SerializeField] private MileageChangePetPopup DetailPopup;
    // # 씬
    private Transform ViewRoot;
    private Camera SubSceneCamera;
    protected UIModelViewController SubSceneController;
    // # 모델
    private GameObject Loaded3DModel;
    private Dictionary<E_ModelSocket, Transform> DicModelSocket = new Dictionary<E_ModelSocket, Transform>();

    private QueuedResFile waitLoadModel = new QueuedResFile();

    // # 비동기 대응
    private bool IsInitialized = false;
    protected bool IsLoadScene = false;

    private bool isReserveUnloadScene = false;
    
    private E_PetChangeContentType reservedContent;

    private C_ContentUseObject contentChecker = null;

    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Initialize();
    }

    public virtual void Initialize()
    {
        IsInitialized = true;
        reservedContent = E_PetChangeContentType.None;
    }

#if UNITY_EDITOR

    public bool bAutoUpdateViewRoot = true;

    private void LateUpdate()
    {
        if (bAutoUpdateViewRoot == false || IsLoadScene == false) return;

        if ((contentChecker == null || contentChecker.Use3DModel == false)) return;

        SetViewRootPosition(contentChecker.RayTarget);
    }

#endif
    private void SetViewRootPosition(Transform rayTarget)
    {
        if (SubSceneCamera == null) return;
        if (IsLoadScene == false) return;
        var a = SubSceneCamera.ScreenPointToRay(SubSceneCamera.WorldToScreenPoint(rayTarget.position));

        if (Physics.Raycast(a, out var hitInfo, 100f))
        {
            var vec = ViewRoot.position;

            vec.x = hitInfo.point.x;
            ViewRoot.position = vec;
        }
    }

    // 임시 :  프레임이 다시 보여질때 열릴 컨텐츠를 예약한다.(ex- 합성컨텐츠 -> 합성결과팝업(fullscreen) -> 합성컨텐츠)
    // 현재 클래스/펠로우/탈것 닫힐시 destory 안됨, 만약 destroy로 바뀔시 로직 수정해야함
    public void SetReserveContent(E_PetChangeContentType type)
    {
        reservedContent = type;
    }

    protected override void OnShow(int _LayerOrder)
    {
        if (!IsInitialized)
        {
            OnClickClose();
            return;
        }

        ContentListView.ReloadListData();

        waitLoadModel.isReady = false;

        if (reservedContent != E_PetChangeContentType.None)
        {
            toggleGroupMenuTab.GetToggle((int)reservedContent).SelectToggle(false);
            SetMainContent(reservedContent);
            
            reservedContent = E_PetChangeContentType.None;
        }
        else
        {
            toggleGroupMenuTab.GetToggle(0).SelectToggle(false);
            SetMainContent(E_PetChangeContentType.Content_1);
        }

        IsLoadScene = false;
        isReserveUnloadScene = false;

        //씬로드, 컨텐츠는 로드완료후 해준다
        LoadSubScene();

        CUIFrameBase frame = UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
        if (frame) frame.ImportInputEnable(false);
        frame = UIManager.Instance.TopMost<UISubHUDCurrency>(true);
        if (frame) frame.ImportInputEnable(false);

        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        base.OnHide();
        waitLoadModel.isReady = false;
        isReserveUnloadScene = true;
        CUIFrameBase frame = UIManager.Instance.TopMost<UISubHUDCharacterState>(false);
        if (frame) frame.ImportInputEnable(true);
        frame = UIManager.Instance.TopMost<UISubHUDCurrency>(false);
        if (frame) frame.ImportInputEnable(true);

        SetContentOFF();

        DetailPopup.Release();

        UnloadSubScene();
    }

    public void OnMenuToggleValueChanged(int type)
    {
        SetMainContent((E_PetChangeContentType)type);
    }

    private void SetContentOFF()
    {
        if (DetailPopup.gameObject.activeSelf)
        {
            DetailPopup.OnClose();
            DetailPopup.gameObject.SetActive(false);
        }

        for (int i = 0; i < ListContentObject.Count; i++)
        {
            C_ContentUseObject contentObj = ListContentObject[i];

            for (int j = 0; j < contentObj.UseObject.Count; j++)
                contentObj.UseObject[j].SetActive(false);

            contentObj.SetState(false);

            if (contentObj.UseObject.Count > 0 || contentObj.Content != null)
                contentObj.Content.HideContent();
        }

        if (Loaded3DModel != null)
        {
            Destroy(Loaded3DModel);
            SubSceneController?.ResetFx();
            Loaded3DModel = null;
        }
    }

    private void SetContentObjectState(E_PetChangeContentType type)
    {
        // 일단 다 꺼줌
        SetContentOFF();

        ListContentObject[(int)type].SetState(true);
        // 해당하는것만 켜줌
        ListContentObject[(int)type].UseObject.ForEach(item => item.SetActive(true));

        contentChecker = ListContentObject[(int)type];

        if (contentChecker == null || contentChecker.Use3DModel == false)
            return;

        SetViewRootPosition(contentChecker.RayTarget);
        SubSceneController?.SetFxRootType(ViewType,type);
    }

    //abs
    protected virtual void SetMainContent(E_PetChangeContentType type)
    {
        SetContentObjectState(type);
    }

    private void LoadSubScene()
    {
        if (!IsLoadScene)
        {
            ZSceneManager.Instance.OpenAdditive(SUBSCENE_MODELVIEW,
                (float _progress) =>
                {
                    ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
                },
                (UnityEngine.Events.UnityAction<string>)((temp) =>
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

                            SubSceneController = obj.GetComponent<UIModelViewController>();
                            SubSceneController.SetFxRootType(ViewType, contentChecker.ContentType);
                        }

                        Transform[] children = obj.GetComponentsInChildren<Transform>();

                        foreach (Transform child in children)
                        {
                            if (child.name == "ViewRoot")
                            {
                                ViewRoot = child.transform;
                                continue;
                            }
                            if (child.name == "SubSceneCamera")
                            {
                                SubSceneCamera = child.GetComponent<Camera>();
                                continue;
                            }
                        }
                    }


                    OnSubSceneLoaded();
                }));
        }

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.IncludeSubScene);
    }

    public void OnSubSceneLoaded()
    {
        if (SubSceneCamera != null)
        {
            UIManager.Instance.DoSubCameraStack(SubSceneCamera, true);
        }

        if (isReserveUnloadScene)
        {
            UnloadSubScene();
            return;
        }
        IsLoadScene = true;


        BackGround.SetActive(false);

        Invoke(nameof(LoadModelFrameSkip), Time.deltaTime);

        //StartCoroutine(LoadModelFrameSkip());
        

        //SetMainContent(E_PetChangeContentType.Content_1);

    }

    private void LoadModelFrameSkip()
    {
        if (waitLoadModel.isReady)
        {
            SetChangeModel(E_PetChangeContentType.Content_1, waitLoadModel.resFile);
            waitLoadModel.isReady = false;
        }
    }

    private void UnloadSubScene()
    {
        if (!IsLoadScene) return;
        UIManager.Instance.DoSubCameraStack(SubSceneCamera, false);

        SubSceneCamera = null;
        ZSceneManager.Instance.CloseAdditive(SUBSCENE_MODELVIEW, null);
        SubSceneController = null;

        if (Loaded3DModel != null)
        {
            Destroy(Loaded3DModel);
            Loaded3DModel = null;
        }

        // 이전에 썼던 hud로!!
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();

        IsLoadScene = false;
        BackGround.SetActive(true);

    }

    protected void SetChangeModel(E_PetChangeContentType callType, S_PCRResourceData resData, bool loadLOBBYModel = true)
    {
        if (IsLoadScene == false)
        {
            waitLoadModel.Reset(resData);
            return;
        }

        if (ViewRoot == null)
            return;

        if (contentChecker == null || contentChecker.Use3DModel == false)
            return;


        if (false == string.IsNullOrEmpty(resData.FileName))
        {
            if (loadLOBBYModel)
                resData.FileName = $"{resData.FileName}_LOBBY";
            ZResourceManager.Instance.Instantiate(resData.FileName, (str, obj) =>
            {                // 로드된 모델이 있는데 마지막 으로 요청된놈과 다름
                if (Loaded3DModel != null)
                {
                    ZLog.LogWarn(ZLogChannel.Pet, $"로드된 모델 해제 : {Loaded3DModel.name}"); 
                    Destroy(Loaded3DModel);
                    SubSceneController?.ResetFx();
                    Loaded3DModel = null;
                }

                //컨텐츠 체커가 없거나, 모델 사용하지않는 컨텐츠임
                if (contentChecker == null || contentChecker.Use3DModel == false)
                {
                    ZLog.LogWarn(ZLogChannel.Pet, $"컨텐츠 체커가 없거나, 모델 사용하지않는 컨텐츠임");

                    return;
                }

                if (obj==null)
                {
                    if (loadLOBBYModel == false)
                    {
                        ZLog.LogWarn(ZLogChannel.Pet, "LOBBY없어서 일반모델 불러오는데 그마저도없음~~");

                        return;
                    }
                    else
                        ZLog.LogWarn(ZLogChannel.Pet, "LOBBY 불러오기 실패~~ 일반모델로 출력요청됨~~");

                    resData.FileName = resData.FileName.Replace("_LOBBY", "");
                    SetChangeModel(callType, resData, false);
                    return;
                }

                ZLog.Log(ZLogChannel.Pet, $"불러온 모델 이름 : {obj.name}");

                this.Loaded3DModel = obj;

                // 로드된놈의 컨텐츠가 호출된 컨텐츠와 다름
                if (contentChecker.ContentType != callType && Loaded3DModel != null)
                {
                    ZLog.Log(ZLogChannel.Pet, $"로드된놈의 컨텐츠가 호출된 컨텐츠와 다름");
                    Destroy(Loaded3DModel);
                    SubSceneController?.ResetFx();
                    Loaded3DModel = null;
                }

                if (this.Loaded3DModel == null)
				{
                    ZLog.Log(ZLogChannel.Pet, $"로드된 모델이 없음");
                    return;
                }
                    

                if (contentChecker != null && contentChecker.Use3DModel)
                    SetViewRootPosition(contentChecker.RayTarget);

                this.Loaded3DModel.SetLayersRecursively("UIModel");

                LODGroup lodGroup = this.Loaded3DModel.GetComponent<LODGroup>();
                if (null != lodGroup)
                    lodGroup.ForceLOD(0);

                this.Loaded3DModel.transform.SetParent(ViewRoot);
                SubSceneController?.SetGradeFx(resData.Grade);

                Vector3 pos = Vector3.zero;
                pos.y = resData.ViewPosY;

                Quaternion rot = Quaternion.Euler(0f, resData.ViewRotY, 0f);

                this.Loaded3DModel.transform.SetLocalTRS(pos, rot, Vector3.one * resData.ViewScale * .01f);

                Loaded3DModel.SetActive(true);
                //소켓 세팅
                foreach (E_ModelSocket socket in Enum.GetValues(typeof(E_ModelSocket)))
                {
                    Transform socketTrans = Loaded3DModel.transform.FindTransform($"Socket_{socket}");

                    if (null == socketTrans)
                        continue;

                    DicModelSocket[socket] = socketTrans;
                }
            });
        }
        else
        {
            if (Loaded3DModel != null)
            {
                Destroy(Loaded3DModel);
                SubSceneController?.ResetFx();
                Loaded3DModel = null;
            }
        }
    }

    public void OnDrag(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UICommon.RotateObjectDrag(eventData, Loaded3DModel);
    }

    public virtual void OnClickClose()
    {
    }


    /// <summary> 튜토리얼에서 adapter 필요해서 훔침! </summary>
    public UIPetChangeScrollAdapter GetScrollPetChangeAdapter()
    {
        return ContentListView.GetScrollPetChangeAdapter();
    }

#if UNITY_EDITOR
    // 프리팹 작업때문에..

    [ContextMenu("OFF_ALLCONTENT")]
    public void EDITOR_OffAllContent()
    {
        SetContentOFF();
    }

    public void EDITOR_OnContent(int i)
    {
        SetContentOFF();
        ListContentObject[i].UseObject.ForEach(item => item.SetActive(true));
    }

    [ContextMenu("ON_CONTENT_1")]
    public void ON_1() => EDITOR_OnContent(0);
    [ContextMenu("ON_CONTENT_2")]
    public void ON_2() => EDITOR_OnContent(1);
    [ContextMenu("ON_CONTENT_3")]
    public void ON_3() => EDITOR_OnContent(2);
    [ContextMenu("ON_CONTENT_4")]
    public void ON_4() => EDITOR_OnContent(3);
    [ContextMenu("ON_CONTENT_5")]
    public void ON_5() => EDITOR_OnContent(4);
#endif
}