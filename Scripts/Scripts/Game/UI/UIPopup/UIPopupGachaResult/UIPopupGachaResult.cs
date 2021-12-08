using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIPopupGachaResult : UIPopupBase
{
    private const int MAX_VIEW_SLOT = 10;


    [SerializeField] private ZButton prevButton;
    [SerializeField] private ZButton nextButton;
    [SerializeField] private ZButton confirmButton;
    [SerializeField] private Text pageText;
    [SerializeField] private GameObject pageGroup;

    [SerializeField] private GameObject objBG;

    [SerializeField] private Transform objTimelineRoot;

    [SerializeField] private ZToggle btnSkip;

    [SerializeField] private Button btnNextModel;

    private int maxShowedPage = 0;
    private int curPage = 0;

    private int maxPage = 0;
    private int lastPageFaction = 0;

    private int loadCnt = 0;

    private int LOAD_COUNT_MAX = 0;

    private UIGachaCardLinker loadedLinker = null;

    private PlayableDirector loadedTimeline = null;

    private GachaViewController loadedScene = null;

    private List<uint> listResultTid = new List<uint>();

    private UIGachaEnum.E_GachaStyle curStyle;

    private bool skipState = false;

    private int curTurnCardCnt = 0; // 현재 돌아간 카드
    private int curPageCardCnt = 0; // 현재 페이지 카드 수

    private bool isLoadSubScene = false;

    private int systemKey = -1;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        objBG.SetActive(true);
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.ShowGlobalIndicator(true, true);
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.IncludeSubScene);

        UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.Gacha);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);


        if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
            gainSystem.SetPlayState(false);

    }

    protected override void OnHide()
    {
        base.OnHide();

        UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.None);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, false);
        UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, false);

        if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
            gainSystem.SetPlayState(true);
    }

    public void SetCombineResult(UIGachaEnum.E_GachaStyle style, UIGachaEnum.E_TimeLineType timeline, List<uint> listTid)
    {
        loadCnt = 0;
        listResultTid.Clear();
        listResultTid.AddRange(listTid);
        curStyle = style;

        bool isSceneDirectable = style == UIGachaEnum.E_GachaStyle.Class;
        isLoadSubScene = isSceneDirectable;
        btnSkip.gameObject.SetActive(isSceneDirectable);

        if(isSceneDirectable)
        {
            skipState = System.Convert.ToBoolean(PlayerPrefs.GetInt(string.Format(ZUIConstant.PREFS_KEY_COMBINE_SKIP_FORMAT, style.ToString()), 0));
            if( skipState)
                btnSkip.SelectToggleSingle(true, false);

            isLoadSubScene = !skipState;
        }

        if (isLoadSubScene)
        {
            LOAD_COUNT_MAX = 3;
        }
        else
        {
            LOAD_COUNT_MAX = 1;
        }

        ZResourceManager.Instance.Load(UIGachaData.GetTimeLineName(style, timeline), (string str, GameObject loaded) =>
        {
            var obj = Instantiate(loaded);

            loadedTimeline = obj.GetComponent<PlayableDirector>();
            loadedLinker = obj.GetComponent<UIGachaCardLinker>();

            systemKey = UIManager.Instance.SetSystemObject(this, obj, objTimelineRoot.gameObject);


            //obj.transform.SetParent(objTimelineRoot, false);
            loadCnt++;
        });

        if (isLoadSubScene)
        {

            ZSceneManager.Instance.OpenAdditive(ZUIConstant.SUB_SCENE_GACHA_VIEW, null, delegate
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(ZUIConstant.SUB_SCENE_GACHA_VIEW);

                if (scene.isLoaded == false)
                {
                    UICommon.OpenSystemPopup_One(ZUIString.ERROR, "씬 로드 실패.", ZUIString.LOCALE_OK_BUTTON);
                    return;
                }

                foreach (var iter in scene.GetRootGameObjects())
                {
                    if (iter.name.Equals("Root"))
                    {
                        loadedScene = iter.GetComponent<GachaViewController>();

                        loadedScene.Initialize(new List<uint>(),null);
                        loadCnt++;
                        break;
                    }
                }

                if (loadedScene == null)
                {
                    UICommon.OpenSystemPopup_One(ZUIString.ERROR, "씬 로드 실패.", ZUIString.LOCALE_OK_BUTTON);
                    return;
                }

                loadCnt++;
            });
        }

        StartCoroutine(CoLoadEnd());

        curPage = 0;
        maxPage = listResultTid.Count / MAX_VIEW_SLOT;
        lastPageFaction = listResultTid.Count % MAX_VIEW_SLOT;

        if (lastPageFaction == 0)
        {
            maxPage -= 1;
            lastPageFaction = MAX_VIEW_SLOT;
        }

        maxShowedPage = -1;

        RefreshUI();
    }

    private void OnLoadEnd()
    {

        PlayDirection();
       
    }

    private void OnTimelineEnd()
    {
        confirmButton.gameObject.SetActive(true);
    }

    public void OnCardTurn()
    {
        curTurnCardCnt++;
        if(curTurnCardCnt>=curPageCardCnt)
        {
            confirmButton.interactable = false;

            prevButton.interactable =  0 < curPage;;
            nextButton.interactable = curPage < maxPage; ;
        }
    }

    public void OnToggleChanged()
    {
        skipState = btnSkip.isOn;
        PlayerPrefs.SetInt(string.Format(ZUIConstant.PREFS_KEY_COMBINE_SKIP_FORMAT, curStyle.ToString()), skipState ? 1 : 0);
    }

    /// <summary>
    /// ui갱신
    /// </summary>
    /// <param name="isResultView"> 결과인지, false일시 모델뷰모드로 전환 </param>
    public void RefreshUI(bool isResultView = true)
    {
        objBG.SetActive(isResultView);
        btnNextModel.gameObject.SetActive(!isResultView);

        confirmButton.interactable = isResultView;
        confirmButton.gameObject.SetActive(!isResultView);

        pageGroup.SetActive(isResultView && maxPage > 0);

        prevButton.interactable = 0 < curPage;
        nextButton.interactable = curPage<maxPage;

        pageText.text = UICommon.GetProgressText(curPage + 1, maxPage + 1);
    }

    public void OnClickPage(bool isNext)
    {
        curPage += isNext ? 1 : -1;

        curPageCardCnt = curPage == maxPage ? lastPageFaction : MAX_VIEW_SLOT;
        curTurnCardCnt = 0;
        
        RefreshUI();

        PlayDirection();
    }

    private List<uint> GetCurResultList()
    {
        var startIdx = curPage * MAX_VIEW_SLOT;

        var endIdx = startIdx;

        return listResultTid.GetRange(startIdx, curPage==maxPage?lastPageFaction:MAX_VIEW_SLOT);
    }

    private void PlayDirection(bool forceSkip = false)
    {
        var listNext = GetCurResultList();

        loadedLinker.Clear();

        bool turnState = forceSkip;

        if (maxShowedPage < curPage)
        {
            maxShowedPage = curPage;
        }
        else
            turnState = true;

        // 스킵이거나, 씬로드 안했음(스킵상태에서 실행시)
        if (skipState || isLoadSubScene == false || turnState == true)
        {

            loadedLinker.Initialize(curStyle, listNext, OnTimelineEnd, OnCardTurn, isAllTurned: turnState);
            loadedTimeline.gameObject.SetActive(true);
            loadedTimeline.initialTime = turnState ? loadedTimeline.duration : 0;
            loadedTimeline.time = turnState ? loadedTimeline.duration : 0;
            loadedTimeline.Play();
        }
        else
        {
            loadedTimeline.gameObject.SetActive(false);
            loadedScene.Initialize(listNext, () =>
            {
                RefreshUI(false);
                loadedScene.SetNext();
            });
        }
    }

    public void OnClickNextModel()
    {
        if(loadedScene.SetNext())
        {
            RefreshUI(true);
            PlayDirection(true);
            return;
        }
    }

    private void Release()
    {

        if (loadedLinker != null)
        {
            loadedLinker.Clear();

            if (systemKey > 0)
            {
                if (UIManager.Instance.GetOutSystemObject(this, systemKey, objTimelineRoot.gameObject))
                    systemKey = -1;
            }

            Destroy(loadedLinker.gameObject);

            loadedLinker = null;
            loadedTimeline = null;
        }

        if (loadedScene != null)
        {
            loadedScene.Clear();
            ZSceneManager.Instance.CloseAdditive(ZUIConstant.SUB_SCENE_GACHA_VIEW, null);
            loadedScene = null;
        }

    }

    public void OnClickClose()
    {
        Release();
        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();

        UIManager.Instance.Close<UIPopupGachaResult>(true);
    }

    public void OnClickConfirm()
    {
        loadedLinker.TurnAll();
        confirmButton.interactable = false;
        prevButton.interactable = false;
        nextButton.interactable = false;
    }

    IEnumerator CoLoadEnd()
    {
        yield return new WaitUntil(() => loadCnt >= LOAD_COUNT_MAX);
        UIManager.Instance.ShowGlobalIndicator(false, true);

        OnLoadEnd();
    }
}
