using DG.Tweening;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PCContentCollectionBase : PCRContentBase
{
    private const int SEARCH_STRING_LIMIT = 10;

    public enum E_CollectionSortType
    {
        All = 0,
        Completed = 1,
        InProgress = 2,
    }

    private const float TWEEN_DURATION = .5f;
    protected const string FORMAT_CHANGE_TYPE = "{0},{1}";

    [SerializeField] private UIPCRCollectionScrollAdapter ScrollCollection;
    [SerializeField] protected UIAbilityListAdapter ScrollAbility;

    [SerializeField] private ZButton btnPrev;
    [SerializeField] private ZButton btnNext;
    [SerializeField] private Text textPage;

    [SerializeField] private Text txtCollectionProgress;
    [SerializeField] private Text txtCollectionAmount;
    [SerializeField] private Image imgCollectionAmount;

    [SerializeField] private Text txtPCRProgress;
    [SerializeField] private Text txtPCRAmount;
    [SerializeField] private Image imgPCRAmount;

    [SerializeField] protected Text selectPCRName;
    [SerializeField] protected Text selectPCRInfo;

    [SerializeField] private GameObject objBtnFocusOff;// 선택된 강림(모델) 끄기 버튼
    [SerializeField] private GameObject objAbilityList;// 컬렉션효과

    [SerializeField] private GameObject objNotice;

    [SerializeField] private ZToggle toggleFirstTab;

    [SerializeField] private InputField searchInput;

    //Dictionary<E_CollectionSortType, List<ScrollPCRCollectionData>> DicContentData = new Dictionary<E_CollectionSortType, List<ScrollPCRCollectionData>>();

    protected List<ScrollPCRCollectionData> BaseContentData = new List<ScrollPCRCollectionData>();
    protected List<ScrollPCRCollectionData> CurContentData = new List<ScrollPCRCollectionData>();

    protected ScrollPCRCollectionData CurSelectedData = null;

    protected E_PetChangeViewType ViewType;

    protected Action<UIFramePetChangeBase.E_PetChangeContentType,S_PCRResourceData,bool> OnSelectDataChanged;

    protected UIFramePetChangeBase.C_ContentUseObject Checker;

    private List<E_AbilityType> pastFilterList = new List<E_AbilityType>();// 전에 선택한 필터 타입

    protected int MaxCollection => BaseContentData.Count;
    protected abstract int CurCollection { get; }
    protected abstract int MaxPCR { get; }
    protected abstract int CurPCR { get; }

    private int TweenValueCollection = 0;
    private float TweenValueCollectionAmount = 0f;
    private int TweenValuePCR = 0;
    private float TweenValuePCRAmount = 0f;

    protected int CurrentPage = 0;
    protected int MaxPage = 0;

    private Sequence CurSequence = null;

    public void Initialize(E_PetChangeViewType target, Action<UIFramePetChangeBase.E_PetChangeContentType, S_PCRResourceData,bool> _onDataChanged, UIFramePetChangeBase.C_ContentUseObject checker)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPCRCollectionListItem), obj=>
        {
            ScrollCollection.Initialize();
            ScrollCollection.SetEvent(OnClickSlotToggle, OnClickSlotPCR);
            ScrollAbility.Initialize();

            ZPoolManager.Instance.Return(obj);
        });

        InitilaizeList();

        RefreshPageData(BaseContentData.Count);

        ViewType = target;

        OnSelectDataChanged = _onDataChanged;

        Checker = checker;
    }

    // 전체 데이터 리스트생성(기준 데이ㅓㅌ)
    protected abstract void InitilaizeList();

    protected abstract List<ScrollPCRCollectionData> GetSortedData(E_CollectionSortType type);
    protected abstract List<ScrollPCRCollectionData> GetSortedData(List<E_AbilityType> type);
    protected abstract List<ScrollPCRCollectionData> GetSortedData(string str);

    // 컬렉션 어빌리티 리스트 받아옴
    protected abstract List<UIAbilityData> GetCollectionAbilityList();

    protected abstract S_PCRResourceData GetResourceData(C_PetChangeData data);

    protected abstract void SetSelectInfo(C_PetChangeData data);
    //--프레임 이벤트 받아옴--
    public virtual void OnFrameShow()
    {
        CurSequence.Clear();
    }

    public virtual void OnFrameHide()
    {
        CurSequence.Clear();
        SetFocusedObject(false);
    }
    //-------------------------

    public void OnClickSort(int idx)
    {
        pastFilterList.Clear();

        searchInput.SetTextWithoutNotify(string.Empty);

        OnClickFocusOff();
        E_CollectionSortType type = (E_CollectionSortType)idx;

        SetCurrentData(GetSortedData(type));

        SetPage(0);
    }

    private void RefreshPageData(int listCount)
    {
        CurrentPage = 0;

        int maxPage = (listCount / (int)DBConfig.PCR_Collection_Page_ViewCount);

        if (maxPage > 0 && listCount % (int)DBConfig.PCR_Collection_Page_ViewCount == 0)
        {
            maxPage -= 1;
        }

        MaxPage = maxPage;
    }

    private void OnClickSlotToggle(ScrollPCRCollectionData data)
    {
        data.IsOpened = !data.IsOpened;

        ScrollCollection.RefreshData(true);
        ScrollCollection.SmoothScrollTo(data);
    }

    private void SetFocusedObject(bool state)
    {
        objBtnFocusOff.SetActive(state);
        objAbilityList.SetActive(!state);

        selectPCRInfo.gameObject.SetActive(state);
        selectPCRName.gameObject.SetActive(state);

        if (state == false)
            ScrollCollection.SetSelectedPCR(null, null);
    }

    private void OnClickSlotPCR(ScrollPCRCollectionData data, C_PetChangeData pcrData)
    {
        if(data.IsSelected && data.SelectedTid == pcrData.Tid)
		{
            OnClickFocusOff();
            return;
		}

        // 모델데이터 추출
        ScrollCollection.SetSelectedPCR(data, pcrData);

        OnSelectDataChanged?.Invoke(Checker.ContentType, GetResourceData(pcrData),true);

        SetSelectInfo(pcrData);

        SetFocusedObject(true);

        // 클래스 컬렉션이 아니라면 속성관련 텍스트 출력안함
        if (data.ViewType != E_PetChangeViewType.Change)
            selectPCRInfo.gameObject.SetActive(false);
    }

    public void OnClickFocusOff()
    {
        SetFocusedObject(false);

        OnSelectDataChanged?.Invoke(Checker.ContentType, new S_PCRResourceData(),true);
    }

    ///0~
    private void SetPage(int page)
    {
        btnPrev.interactable = CurrentPage > 0;
        btnNext.interactable = CurrentPage <= MaxPage;

        textPage.text = UICommon.GetProgressText(CurrentPage + 1, MaxPage + 1,false);

        if (CurrentPage == 0 && CurrentPage == MaxPage)
            return;

        int startIdx = (page) * (int)DBConfig.PCR_Collection_Page_ViewCount;
        int count = (int)DBConfig.PCR_Collection_Page_ViewCount;

        if (startIdx + count >= CurContentData.Count)
        {
            count = CurContentData.Count - startIdx;
        }

        ScrollCollection.ResetListData(CurContentData.GetRange(startIdx, count));
    }

    public override void ShowContent()
    {
        searchInput.SetTextWithoutNotify(string.Empty);
        ScrollAbility.RefreshListData(GetCollectionAbilityList());

        SetTweenUIInfo();
        toggleFirstTab.SelectToggle(false);
        OnClickSort(0);
    }

    public void OnClickFilter()
    {
        UIManager.Instance.Open<UIPopupCollectionFilterSelect>((str, popup) =>
        {
            popup.SetPopup(pastFilterList, OnSetFilter);
        });
    }

    private void SetCurrentData(List<ScrollPCRCollectionData> data)
    {
        CurContentData = data;

        objNotice.SetActive(CurContentData.Count <= 0);

        ScrollCollection.ResetListData(CurContentData);

        RefreshPageData(ScrollCollection.Data.Count);

    }

    private void OnSetFilter(List<E_AbilityType> listType)
    {
        pastFilterList.Clear();
        pastFilterList.AddRange(listType);

        OnClickFocusOff();

        toggleFirstTab.SelectToggle(false);

        string searchStr = string.Empty;    

        for (int i =0;i<listType.Count;i++)
		{
            searchStr += $"{DBLocale.GetText(listType[i].ToString())},";
            
            if (i > 3)
                break;
		}

        if (listType.Count > 0)
            searchStr = searchStr.Substring(0, searchStr.Length - 1);

        if (searchStr.Length > 7)
		{
            searchStr = searchStr.Substring(0, searchStr.Length - 1);
            searchStr = searchStr.Substring(0, 7);
            searchStr += "...";
        }

        searchInput.SetTextWithoutNotify(searchStr);

        SetCurrentData(GetSortedData(listType));

        SetPage(0);
    }

    public void OnSearchInputValueChanged(string msg)
    {
        if (string.IsNullOrEmpty(msg.Trim()))
            return;

        if (msg.Length > SEARCH_STRING_LIMIT)
        {
            msg = msg.Substring(0, SEARCH_STRING_LIMIT);
        }

#if UNITY_EDITOR1
        // 수정이 끝난상태(포커스 변경)는 2가지로 나뉨
        // 유저가 정상적으로 입력을 완료했다 or 입력중 다른곳을 클릭했다
        // 에디터는 포커스 변경시에 그냥 엔터라 판단하고 검색
        // 스마트폰 빌드시엔 터치스크린키보드로 판단한다.
#elif UNITY_ANDROID || UNITY_IOS// 정상완료 검사
        if (searchInput.touchScreenKeyboard.status != TouchScreenKeyboard.Status.Done)
            return;
#endif
        OnClickFocusOff();

        toggleFirstTab.SelectToggle(false);

        SetCurrentData(GetSortedData(searchInput.text));

        SetPage(0);
    }

    public void OnClickResetSearch()
    {
        OnClickSort(0);
    }

    private void SetTweenUIInfo()
    {
        CurSequence.Clear();

        int maxCol = MaxCollection;
        int curCol = CurCollection;
        int maxPcr = MaxPCR;
        int curPcr = CurPCR;

        float amountCol = (float)curCol / (float)maxCol;
        float amountPcr = (float)curPcr / (float)maxPcr;

        TweenValueCollection = 0;
        TweenValueCollectionAmount = 0f;
        TweenValuePCR = 0;
        TweenValuePCRAmount = 0f;

        txtCollectionProgress.text = UICommon.GetProgressText(0, maxCol);
        txtCollectionAmount.text = 0f.ToString("F2");
        imgCollectionAmount.fillAmount = 0f;

        txtPCRProgress.text = UICommon.GetProgressText(0, maxPcr);
        txtPCRAmount.text = 0f.ToString("F2");
        imgPCRAmount.fillAmount = 0f;

        CurSequence = DOTween.Sequence().
            Join(DOTween.To(
                () => TweenValueCollection, (val) => TweenValueCollection = val, curCol, TWEEN_DURATION).
                OnUpdate(() => txtCollectionProgress.text = UICommon.GetProgressText(TweenValueCollection, maxCol))).
            Join(DOTween.To(
                () => TweenValuePCR, (val) => TweenValuePCR = val, curPcr, TWEEN_DURATION).
                OnUpdate(() => txtPCRProgress.text = UICommon.GetProgressText(TweenValuePCR, maxPcr))).
            Join(DOTween.To(
                () => TweenValueCollectionAmount, (val) => TweenValueCollectionAmount = val, amountCol, TWEEN_DURATION).
                OnUpdate(() =>
                {
                    txtCollectionAmount.text = (TweenValueCollectionAmount * 100f).ToString("F2");
                    imgCollectionAmount.fillAmount = TweenValueCollectionAmount;
                })).
            Join(DOTween.To(
                () => TweenValuePCRAmount, (val) => TweenValuePCRAmount = val, amountPcr, TWEEN_DURATION).
                OnUpdate(() =>
                {
                    txtPCRAmount.text = (TweenValuePCRAmount * 100f).ToString("F2");
                    imgPCRAmount.fillAmount = TweenValuePCRAmount;
                })).Play();
    }

    public void OnClickPageButton(bool isNext)
    {
        int desPage = CurrentPage;

        if (isNext)
            desPage++;
        else
            desPage--;

        desPage = Mathf.Clamp(desPage, 0, MaxPage);

        if (desPage == CurrentPage)//변한게없음
            return;

        CurrentPage = desPage;

        SetPage(CurrentPage);
    }
}
