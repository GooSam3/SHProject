using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class C_PCCombineList
{
    [SerializeField]// 합성 슬롯들
    private List<UIPetChangeListItem> listPetChangeSlot = new List<UIPetChangeListItem>();

    // 원본 참조 딕셔너리
    private Dictionary<uint, C_PetChangeData> dicOriginData = new Dictionary<uint, C_PetChangeData>();

    public List<C_PetChangeData> listCombineData { get; private set; } = new List<C_PetChangeData>();

    public int TotalCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < listCombineData.Count; i++)
            {
                count += listCombineData[i].ViewCount;
            }
            return count;
        }
    }

    public int GetViewCount(uint tId)
    {
        var data = listCombineData.Find(item => item.Tid == tId);

        return data?.ViewCount ?? 0;
    }

    public int ListCount => listCombineData.Count;

    public void Refresh()
    {
        for (int i = 0; i < listPetChangeSlot.Count; i++)
        {
            bool hasValue = listCombineData.Count > i;

            listPetChangeSlot[i].gameObject.SetActive(hasValue);

            if (hasValue)
            {
                listPetChangeSlot[i].SetSlot(listCombineData[i], null);
                listPetChangeSlot[i].SetSelectState(false);
            }
        }
    }

    public void Clear()
    {
        listCombineData.Clear();
        dicOriginData.Clear();
        Refresh();
    }

    //원본리스트에ㅓㅅ
    public void Add(C_PetChangeData data)
    {
        if (dicOriginData.ContainsKey(data.Tid) == false)
        {
            // 등록카드갯수 넘칠시 out
            if (listCombineData.Count >= ZUIConstant.PCR_COMBINE_REGIST_MAX)
                return;

            dicOriginData.Add(data.Tid, data);
            listCombineData.Add(new C_PetChangeData(data) { ViewCount = 0 });
        }

        if (data.ViewCount <= 0)
            return;

        data.ViewCount--;

        var cData = listCombineData.Find(item => item.Tid == data.Tid);
        cData.ViewCount++;

        Refresh();
    }

    public void Add(List<C_PetChangeData> data, int combineCount)
    {
        foreach (var iter in data)
        {
            // 연산최소화 캐싱, 합성리스트 최대갯수
            int pastMaxCount = TotalCount;

            if (pastMaxCount >= combineCount)
                break;

            if (dicOriginData.ContainsKey(iter.Tid) == false)
            {
                if (listCombineData.Count >= ZUIConstant.PCR_COMBINE_REGIST_MAX)
                    break;

                dicOriginData.Add(iter.Tid, iter);
                listCombineData.Add(new C_PetChangeData(iter) { ViewCount = 0 });
            }

            // 등록 가능한 갯수
            int count = iter.ViewCount;

            // 등록된 + 등록 할 갯수가 넘치면 
            if (pastMaxCount + count > combineCount)
            {
                // 넣을수 있는 최대 갯수만큼만 넣어준다.
                count = combineCount - pastMaxCount;
            }

            iter.ViewCount -= count;

            var cData = listCombineData.Find(item => item.Tid == iter.Tid);
            cData.ViewCount += count;
        }

        Refresh();
    }

    // 합성리스트에서, 액션동작x
    public void Remove(int i)
    {
        if (listPetChangeSlot[i].gameObject.activeSelf == false)
            return;

        var data = listPetChangeSlot[i].SlotData;

        data.ViewCount--;
        dicOriginData[data.Tid].ViewCount++;

        if (data.ViewCount <= 0)
        {
            dicOriginData.Remove(data.Tid);
            listCombineData.Remove(data);
        }

        Refresh();
    }
}

public abstract class PCContentCombineBase : PCRContentBase
{
    [SerializeField] private ZToggle toggleFirstTab;

    // 목록에서 쓰이는 어댑터와 공유
    [SerializeField] protected UIPetChangeScrollAdapter ScrollPetChange;

    // 전설이상, 비어있을때
    [SerializeField] protected Text TextNotice;

    // 합성슬롯 등록갯수 (-/400)
    [SerializeField] private Text TextAmount;

    // 동일한 등급으 ㅣ카드를 --장 등록해주세용 같은 가이드메세지
    [SerializeField] private Text TextCombineGuide;

    // 합성용 리스트
    [SerializeField] protected C_PCCombineList CombineList;

    [SerializeField] private ZButton BtnCombine;

    [SerializeField] private Text TextCost;

    // 사용리스트 캐싱
    protected List<C_PetChangeData> ListContentData = new List<C_PetChangeData>();

    // 사용 리스트중 현재 보여지는 리스트
    protected List<C_PetChangeData> ListSortData = new List<C_PetChangeData>();

    protected C_PetChangeData CurSelectedData = null;

    protected E_PetChangeViewType ViewType;

    private UIFramePetChangeBase.C_ContentUseObject Checker;

    // 등록된 합성물의 등급
    // 0일시 등록안된상태
    protected uint RegistedGrade = 0;

    private int CombineCount = 0;

    public void Initialize(E_PetChangeViewType target, UIFramePetChangeBase.C_ContentUseObject checker)
    {
        // 스크롤 어댑터는 1번컨텐츠인 목록에서 해줌, 

        ViewType = target;

        Checker = checker;
    }

    public void ReloadListData()
    {
        InitilaizeList();
    }

    // 전체 데이터 리스트생성
    protected abstract void InitilaizeList();

    // 총 가격
    protected abstract long GetCombineCost(int multi);

    // 충족갯수
    protected abstract int GetCombineCount();

    public abstract void OnClickCombine();

    public override void ShowContent()
    {
        InitilaizeList();

        toggleFirstTab.SelectToggle(false);
        OnSortToggleValueChanged((int)E_PetChangeSortType.All);
    }

    // 정렬방식 바뀔때, type : (int)E_PetChangeSortType
    public void OnSortToggleValueChanged(int type)
    {
        if (Checker.isOn == false)
            return;

        RegistedGrade = 0;
        CombineCount = 0;

        CombineList.Clear();

        RefreshUI();

        SetSortedContent((E_PetChangeSortType)type);
    }

    public virtual void SetSortedContent(E_PetChangeSortType sortType)
    {
        ListSortData.Clear();

        foreach (var iter in ListContentData)
        {
            if (sortType.Equals(E_PetChangeSortType.All) || (uint)sortType == iter.Grade)
                ListSortData.Add(new C_PetChangeData(iter) { postSetting = E_PCR_PostSetting.RegistStateOff });
        }

        ListSortData.Sort(SortComparison);

        ScrollPetChange.ResetData(ListSortData, OnClickSlot, OnDoubleClickSlot);
    }

    // 정렬, 등급만 비교
    private int SortComparison(C_PetChangeData left, C_PetChangeData right)
    {
        if (left.Order < right.Order)
            return 1;

        if (left.Order > right.Order)
            return -1;

        return 0;
    }

    protected void OnClickSlot(C_PetChangeData data)
    {
        if (data == null)
            return;

        CurSelectedData = data;
    }

    private void RefreshUI()
    {
        int count = CombineList.TotalCount;

        TextAmount.text = DBLocale.GetText("UI_Common_Amount_Simple", count, DBConfig.PCR_Combine_Max);

        BtnCombine.interactable = false;

        if (count >= DBConfig.PCR_Combine_Max)
        {
            TextCombineGuide.text = DBLocale.GetText("Compose_Max_Count");
            BtnCombine.interactable = true;
        }
        else
        {
            TextCombineGuide.text = DBLocale.GetText("PCR_Combine_Default");

            if (count > 0)
            {
                int needCount = CombineCount - count % CombineCount;

                if (needCount == CombineCount)//합성조건 완료
                {
                    BtnCombine.interactable = true;
                }
                else
                {
                    TextCombineGuide.text = string.Format(DBLocale.GetText("PCR_Combine_AddMoreCard"), needCount);
                }
            }
        }

        if (count > 0)
        {
            int costMultiply = (int)count / (int)CombineCount;
            TextCost.text = GetCombineCost(costMultiply).ToString();
        }
        else
        {
            TextCost.text = 0.ToString();
        }
    }

    protected void OnDoubleClickSlot(C_PetChangeData data)
    {
        if (data == null)
            return;

        if (CurSelectedData == null)
        {
            ScrollPetChange.RefreshData();
            return;
        }

        if (RegistedGrade == 0)
        {
            SetCombineInfo(data.Grade);
            // 처음등록시엔 본 메서드의 끝까지 도달하기에 osa갱신 안해줌(아래서해줄것임)
        }

        if (RegistedGrade != data.Grade)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("PCR_Combine_Error_NotEqualGrade"));
            return;
        }

        // 갯수 업승ㅁ
        if (data.ViewCount <= 0)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("PCR_Combine_Error_Empty"));
            return;
        }

        // 최대 등록 갯수 검사
        if (CombineList.TotalCount >= DBConfig.PCR_Combine_Max)
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("Compose_Max_Count"));
            return;
        }

        CombineList.Add(data);

        ScrollPetChange.Refresh();
        RefreshUI();
    }

    public void OnClickCombineListSlot(int i)
    {
        CombineList.Remove(i);

        if (CombineList.ListCount <= 0)
            SetCombineInfo(0);

        ScrollPetChange.Refresh();
        RefreshUI();
    }

    public void SetCombineInfo(uint grade)
    {
        RegistedGrade = grade;
        CombineCount = GetCombineCount();

        foreach (var iter in ListSortData)
        {
            iter.postSetting = E_PCR_PostSetting.RegistStateOff;

            if (RegistedGrade > 0 && iter.Grade != RegistedGrade)
            {
                iter.postSetting |= E_PCR_PostSetting.GainStateOn | E_PCR_PostSetting.BlockInput;
            }
        }
    }

    public void OnClickAutoRegist()
    {
        //우선순위 : 최대한 많이넣을수있는 등급

        // 차라리 linq로..?

        // 자동클릭시 합성탭 최상단으로이동
        ShowContent();

        // grade : count
        Dictionary<uint, int> dicSortList = new Dictionary<uint, int>();

        foreach (var iter in ListSortData)
        {
            if (dicSortList.ContainsKey(iter.Grade) == false)
                dicSortList.Add(iter.Grade, 0);

            dicSortList[iter.Grade] += iter.ViewCount;
        }

        uint targetGrade = 0;

        for (uint i = 0; i < ZUIConstant.PCR_TIER_MAX; i++)
        {
            // GetcombineCount는 RegistedGrade를 참조하여 값 가져옴
            // 여기선 임시로 세팅후 값만가져오는용도르쓰임
            RegistedGrade = i + 1;

            if (dicSortList.ContainsKey(RegistedGrade) && dicSortList[RegistedGrade] >= GetCombineCount())
            {
                targetGrade = RegistedGrade;
                RegistedGrade = 0;

                break;
            }
            RegistedGrade = 0;

        }

        // 가장많은놈이 조합가능수량 충족을 못한다..
        if (targetGrade <= 0)
        {
            UIMessagePopup.ShowPopupOk("WChangePet_Compose_NotAuto1");
            return;
        }

        SetCombineInfo(targetGrade);

        // 리스트로
        var targetList = ListSortData.FindAll(item => item.Grade == targetGrade);

        // 리스트 내 탑8 선정
        targetList.Sort((x, y) =>
        {
            if (x.ViewCount > y.ViewCount) return -1;
            else return 1;
        });

        int listCount = targetList.Count > ZUIConstant.PCR_COMBINE_REGIST_MAX ? ZUIConstant.PCR_COMBINE_REGIST_MAX : targetList.Count;

        targetList = targetList.GetRange(0, listCount);

        // 등록할수 있는 갯수
        int regiCount = 0;
        targetList.ForEach(item => regiCount += item.ViewCount);


        if (regiCount > DBConfig.PCR_Combine_Max)
            regiCount = (int)DBConfig.PCR_Combine_Max;
        else
        {
            regiCount = regiCount - (regiCount % CombineCount);
        }

        CombineList.Add(targetList, regiCount);
        ScrollPetChange.Refresh();
        RefreshUI();
    }
}
