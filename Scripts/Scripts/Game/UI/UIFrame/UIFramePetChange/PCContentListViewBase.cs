using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

using DG.Tweening;
using UnityEngine.EventSystems;

public enum E_PetChangeSortType
{
    All = 0,    // 모두
    Tier_1 = 1, // 일반
    Tier_2 = 2, // 마법
    Tier_3 = 3, // 희귀
    Tier_4 = 4, // 영웅
    Tier_5 = 5, // 전설
    Tier_6 = 6, // 고대
}

public enum E_ViewModelPosition
{
    ListView,
    Collection,
}

public struct S_PCRResourceData
{
    public byte Grade;
    public string FileName;
    public uint ViewScale;
    public float ViewPosY;
    public float ViewRotY;
}

// PC : Pet & Change
// 강림펫 첫번째 컨텐츠(명칭이 애매함.. 강림?펫?리스트?)
// 이하 리스트뷰라 칭합니다
public abstract class PCContentListViewBase : PCRContentBase
{
    protected const string FORMAT_ATTRIBUTE_ICON = "icon_element_{0}_s";
    protected const string FORMAT_ATTRIBUTE_LEVEL = "Lv.{0}";
    protected const string FORMAT_CHANGE_TYPE = "{0},{1}";

    // # NEED LOCALE
    protected const string LOCALE_ELEMENT_FIRE = "Attribute_Fire";
    protected const string LOCALE_ELEMENT_WATER = "Attribute_Water";
    protected const string LOCALE_ELEMENT_ELECTRIC = "Attribute_Electric";
    protected const string LOCALE_ELEMENT_LIGHT = "Attribute_Light";
    protected const string LOCALE_ELEMENT_DARK = "Attribute_Dark";

    // # 펫 | 강림 리스트 정렬버튼
    [SerializeField] private ZToggle toggleFirstTab;

    //[SerializeField] protected ZUIScrollPetChangeList PetChangeScroll;

    [SerializeField] protected UIPetChangeScrollAdapter ScrollPetChange;

    [SerializeField] protected UIAbilityListAdapter ScrollAbility;

    [SerializeField] private ZToggle toggleLock;

    [SerializeField] protected Text txtConfirm;

    protected bool isRegisted = false;
    protected bool hasValue = false;

    // 강림 | 펫 리스트 
    protected List<C_PetChangeData> ListContentData = new List<C_PetChangeData>();

    protected C_PetChangeData CurSelectedData = null;

    protected E_PetChangeViewType ViewType;

    // 모델변경 <resId, viewSacale>
    protected Action<UIFramePetChangeBase.E_PetChangeContentType, S_PCRResourceData, bool> OnSelectDataChanged;

    protected UIFramePetChangeBase.C_ContentUseObject Checker;

    public virtual void Initialize(E_PetChangeViewType target, Action<UIFramePetChangeBase.E_PetChangeContentType, S_PCRResourceData, bool> _onDataChanged, UIFramePetChangeBase.C_ContentUseObject checker)
    {
        //!!
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAbilitySlot), (objAbil)=>
        {
            ScrollAbility.Initialize();
            ZPoolManager.Instance.Return(objAbil);
        });

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPetChangeListItem), (obj)=>
        {
            ScrollPetChange.Initilize();
            ZPoolManager.Instance.Return(obj);
        });

        ViewType = target;

        OnSelectDataChanged = _onDataChanged;

        Checker = checker;
    }
    //--프레임 이벤트 받아옴--
    public virtual void OnFrameShow() { }

    public virtual void OnFrameHide() { }
    //-------------------------

    // base.onshow
    public void ReloadListData()
    {
        InitilaizeList();
        toggleFirstTab.SelectToggle(false);
    }

    // 전체 데이터 리스트생성
    protected abstract void InitilaizeList();
    // ui설정
    protected abstract void SetUI(C_PetChangeData data);
    // 어빌리티액션 리스트받아옴
    protected abstract List<UIAbilityData> GetAbilityList(C_PetChangeData data);
    // 강림 소환 등등
    public abstract void OnClickConfirm();
    // 모델변경용 데이터세팅
    protected abstract S_PCRResourceData GetResourceData();

    // 정렬
    private int SortComparison(C_PetChangeData left, C_PetChangeData right)
    {
        bool haveLeft = left.IsOwn;
        bool haveRight = right.IsOwn;

        if (left.isEquiped)
            return -1;
        if (right.isEquiped)
            return 1;

        if ((haveLeft && haveRight))
        {
            if (left.Order < right.Order)
                return -1;

            if (left.Order > right.Order)
                return 1;

            return 0;
        }

        if (!haveLeft && !haveRight)
        {
            if (left.Order < right.Order)
                return -1;

            if (left.Order > right.Order)
                return 1;

            return 0;
        }

        //둘중하나만 있음

        if (haveLeft)
            return -1;

        if (haveRight)
            return 1;

        //들어올일없지만..
        return 0;
    }

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

        SetSortedContent((E_PetChangeSortType)type);

    }

    public virtual void SetSortedContent(E_PetChangeSortType sortType)
    {
        List<C_PetChangeData> listTarget = new List<C_PetChangeData>();

        if (sortType.Equals(E_PetChangeSortType.All))
        {
            listTarget = ListContentData;
        }
        else
        {
            listTarget = ListContentData.FindAll((data) => data.Grade == (byte)sortType);
        }

        listTarget.Sort(SortComparison);

        ScrollPetChange.ResetData(listTarget, OnClickSlot);

        if (ScrollPetChange.Data.Count > 0)
            ScrollPetChange.SetFocusItem(ScrollPetChange.Data[0]);
    }

    protected virtual void OnClickSlot(C_PetChangeData data)
    {
        if (data == null)
            return;

        CurSelectedData = data;

        SetUI(data);

        SetAbilityUI(GetAbilityList(data));

        S_PCRResourceData resData = GetResourceData();

        OnSelectDataChanged.Invoke(Checker.ContentType, resData, true);

        toggleLock.SelectToggleSingle(data.isLock, false);
        toggleLock.interactable = !(CurSelectedData.Id == 0 || CurSelectedData.Tid == 0);
    }

    protected void SetAbilityUI(List<UIAbilityData> ability)
    {
        ScrollAbility.RefreshListData(ability);
    }

    public void OnClickLock(bool isLock)
    {
        if (CurSelectedData.Id == 0 || CurSelectedData.Tid == 0)
        {
            UIMessagePopup.ShowPopupOk("PCR_Notice_NoCard");
            return;
        }

        E_GoodsKindType type = E_GoodsKindType.None;


        if (ViewType == E_PetChangeViewType.Change)
            type = E_GoodsKindType.Change;
        else if (ViewType == E_PetChangeViewType.Pet || ViewType == E_PetChangeViewType.Ride)
            type = E_GoodsKindType.Pet;

        ZWebManager.Instance.WebGame.REQ_ToggleLock(type, CurSelectedData.Id, CurSelectedData.Tid, isLock, (recvPacket, recvMsgPacket) =>
        {
            CurSelectedData.isLock = isLock;
            ScrollPetChange.RefreshData();
            toggleLock.interactable = true;
        }, null);

        toggleLock.interactable = false;
    }

    /// <summary> 튜토리얼에서 adapter 필요해서 훔침! </summary>
    public UIPetChangeScrollAdapter GetScrollPetChangeAdapter()
    {
        return ScrollPetChange;
    }
}

// 강림 / 펫 
public abstract class PCContentListViewPR : PCContentListViewBase
{
    [Header("ChangeStatus"), Space(5)]
    [SerializeField] protected Text SelectPetName;
    [SerializeField] protected ZButton SummonButton;

    // 레벨업 버튼(모드변경)
    [SerializeField] protected ZButton BtnLevelUPMode;
    [SerializeField] protected Text TxtLevelUp;

    // 실 레벨업 버튼
    [SerializeField] protected ZButton BtnLevelUp;

    [SerializeField] protected Image ImgLvUpMaterialFirst;
    [SerializeField] protected Text TxtLvUpCostFirst;

    [SerializeField] protected Image ImgLvUpMaterialSecond;
    [SerializeField] protected Text TxtLvUpCostSecond;

    [SerializeField] protected Text TxtPetLevel;

    [SerializeField] protected PREquipmentInventory EquipInven;

    // 소유하지 않은 펫 선택시 장비창 안보이게/비활성화
    [SerializeField] private PREquipInfo EquipInfo;

    [SerializeField] private ZButton BtnEquip;

    [SerializeField] private ZButton BtnInventory;

    [SerializeField] private Image ImgInventoryFillAmount;
    [SerializeField] private Text TxtInventoryFillAmount;

    [SerializeField] private CanvasGroup cgSetInfoToolTip;
    [SerializeField] private Text txtTooltipTitle;
    [SerializeField] private Text txtTooltipDesc;

    private Action onLevelup;

    protected bool isLevelUpMode = false;

    public void Initialize(E_PetChangeViewType target, Action<UIFramePetChangeBase.E_PetChangeContentType, S_PCRResourceData, bool> _onDataChanged, UIFramePetChangeBase.C_ContentUseObject checker, Action _onLevelup)
    {
        onLevelup = _onLevelup;
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPREquipListItem), objPR=>
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPREquipFilterListItem), objFilter=>
            {
                EquipInven.Init(OnInterectEquipment);

                ZPoolManager.Instance.Return(objPR);
                ZPoolManager.Instance.Return(objFilter);
            });
        });

        EquipInfo.Initialize(OnClickEquipSlot, OnClickSetInfo);

        base.Initialize(target, _onDataChanged, checker);
    }

    public override void ShowContent()
    {
        if (EquipInven.IsActive)
            EquipInven.SetState(false);

        cgSetInfoToolTip.alpha = 0;

        base.ShowContent();
    }

    public override void HideContent()
    {
        if (EquipInven.IsActive)
            EquipInven.SetState(false);

        CancelInvoke();

        base.HideContent();
    }

    private PetData GetPetData(uint tid)
    {
        if (ViewType == E_PetChangeViewType.Pet)
            return Me.CurCharData.GetPetData(tid);
        else if (ViewType == E_PetChangeViewType.Ride)
            return Me.CurCharData.GetRideData(tid);

        return null;
    }

    protected override List<UIAbilityData> GetAbilityList(C_PetChangeData data)
    {
        Pet_Table tableData = data.petData;

        var listAbility = UIStatHelper.GetPetStat(data.petData);

        if (isLevelUpMode)
        {
            var petData = GetPetData(tableData.PetID);
            var petLevel = DBPetLevel.GetLevel(tableData.PetExpGroup, petData.Exp);

            UIStatHelper.SetPetCompareStat(ref listAbility, tableData.PetExpGroup, (byte)(petLevel + 1));
        }
        return listAbility;
    }

    protected override void SetUI(C_PetChangeData data)
    {
        PetData myData = null;
        if (data.petData.PetType == E_PetType.Pet)
        {
            myData = Me.CurCharData.GetPetData(data.Tid);
        }
        else if (data.petData.PetType == E_PetType.Vehicle)
        {
            myData = Me.CurCharData.GetRideData(data.Tid);
        }

        bool isOwn = myData!=null &&  myData.PetId > 0 && myData.AdvId <= 0;
        // 소유하지 않은 펫 선택시 장비창 안보이게/비활성화
        EquipInfo.SetState(isOwn);

        if (isOwn)
        {
            EquipInfo.SetEquipSlot(data.Tid);
        }

        BtnEquip.interactable = isOwn;
        BtnInventory.interactable = isOwn;


        float amount = (float)Me.CurCharData.GetRuneCountAll() / (float)DBConfig.Rune_Inventory_Max_Count;

        TxtInventoryFillAmount.text = $"{(amount*100f).ToString("F2")}%";
        ImgInventoryFillAmount.fillAmount = amount;

        if (isOwn == false && EquipInven.IsActive)
            EquipInven.SetState(false);
    }

    protected override void OnClickSlot(C_PetChangeData data)
    {
        RefreshLevelUpMode(data.petData);
        base.OnClickSlot(data);

        RefreshPetLevel();

        EquipInven.ResetEquipTarget(data);
        OnClickSetInfo(E_RuneSetType.None);
    }

    private void RefreshLevelUpMode(Pet_Table table)
    {
        var petData = GetPetData(table.PetID);

        SetLevelUpMode(false);

        if (petData == null)
        {
            BtnLevelUPMode.interactable = false;
            return;
        }

        var petLevel = DBPetLevel.GetLevel(table.PetExpGroup, petData.Exp);


        bool isExistNextLevel = DBPetLevel.IsExistLevelUp(table.PetExpGroup, petLevel);

        BtnLevelUPMode.interactable = isExistNextLevel && GetPetData(table.PetID).AdvId==0;
        TxtLevelUp.text = DBLocale.GetText(isExistNextLevel ? "PR_LevelUp" : "Attribute_Enhance_MaxButtont");
    }

    private void SetLevelUpMode(bool state)
    {
        isLevelUpMode = state;
        BtnLevelUp.gameObject.SetActive(state);
    }

    private void RefreshCost()
    {
        var petData = GetPetData(CurSelectedData.petData.PetID);
        var table = CurSelectedData.petData;
        if (petData == null || table == null)
            return;

        if (DBPetGrowth.GetData(table.PetGrowthGroup, DBPetLevel.GetLevel(table.PetExpGroup, petData.Exp), out var growthTable) == false)
            return;

        bool firstOn = growthTable.PetGrowthItem_01 > 0;
        bool secondOn = growthTable.PetGrowthItem_02 > 0;

        TxtLvUpCostFirst.gameObject.SetActive(firstOn);
        TxtLvUpCostSecond.gameObject.SetActive(secondOn);

        if (firstOn)
        {
            ImgLvUpMaterialFirst.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(growthTable.PetGrowthItem_01));
            TxtLvUpCostFirst.text = growthTable.PetGrowthItemCnt_01.ToString("N0");
            TxtLvUpCostFirst.color = (Me.CurCharData.GetItem(growthTable.PetGrowthItem_01)?.cnt ?? 0) < growthTable.PetGrowthItemCnt_01 ? Color.red : Color.white;
        }
        if (secondOn)
        {
            ImgLvUpMaterialSecond.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(growthTable.PetGrowthItem_02));
            TxtLvUpCostSecond.text = growthTable.PetGrowthItemCnt_02.ToString("N0");
            TxtLvUpCostSecond.color = (Me.CurCharData.GetItem(growthTable.PetGrowthItem_02)?.cnt ?? 0) < growthTable.PetGrowthItemCnt_02 ? Color.red : Color.white;
        }
    }

    public void RefreshPetLevel()
    {
        var petData = GetPetData(CurSelectedData.petData.PetID);
        TxtPetLevel.gameObject.SetActive(petData != null);

        if (petData == null)
            return;

        TxtPetLevel.text = DBLocale.GetText("Attribute_Level", DBPetLevel.GetLevel(CurSelectedData.petData.PetExpGroup, petData.Exp));
    }

    public void OnClickLevelUpMode()
    {
        SetLevelUpMode(true);

        SetAbilityUI(GetAbilityList(CurSelectedData));
        RefreshCost();
        BtnLevelUp.Select();
    }

    public void OnDeselectLevelUpMode(BaseEventData eventData)
	{
        RefreshLevelUpMode(CurSelectedData.petData);
        SetAbilityUI(GetAbilityList(CurSelectedData));
    }

    public void OnClickLevelUp()
    {
        var petData = GetPetData(CurSelectedData.petData.PetID);

        if (DBPetGrowth.GetData(CurSelectedData.petData.PetGrowthGroup, DBPetLevel.GetLevel(CurSelectedData.petData.PetExpGroup, petData.Exp), out var growthTable) == false)
        {
            ZLog.LogWarn(ZLogChannel.Pet, "성장테이블 없음~~");
            return;
        }

        if (petData == null)
        {
            SetLevelUpMode(false);
            ZLog.LogWarn(ZLogChannel.Pet, "비정상적인 경로!!!! 소유한 펫 없는데 레벨업호출됨~~");
            return;
        }

        if (growthTable.PetGrowthItem_01 > 0)
        {
            if (ConditionHelper.CheckCompareCost(growthTable.PetGrowthItem_01, growthTable.PetGrowthItemCnt_01) == false)
                return;
        }

        if (growthTable.PetGrowthItem_02 > 0)
        {
            if (ConditionHelper.CheckCompareCost(growthTable.PetGrowthItem_02, growthTable.PetGrowthItemCnt_02) == false)
                return;
        }

        ZWebManager.Instance.WebGame.REQ_GrowthPet(petData.PetId, petData.PetTid, (recvPacket, recvMsgPacket) =>
        {
            if (recvMsgPacket.PetInfo.HasValue == false)
                return;

            var recvPetData = recvMsgPacket.PetInfo.Value;

            if (DBPet.TryGet(recvPetData.PetTid, out var table) == false)
                return;

            bool isExist = DBPetLevel.IsExistLevelUp(table.PetExpGroup, DBPetLevel.GetLevel(table.PetExpGroup, recvPetData.Exp));

            if (!isExist)
                RefreshLevelUpMode(table);

            onLevelup?.Invoke();

            CurSelectedData.Reset(table, (int)recvPetData.Cnt);

            ScrollPetChange.RefreshData();
            SetAbilityUI(GetAbilityList(CurSelectedData));
            RefreshPetLevel();

            if (isExist)
                RefreshCost();
        });
    }

    public void OnClickInvenPR()
    {
        EquipInven.SetState(true, CurSelectedData);
    }

    public void OnClickEquipManage()
    {
        EquipInven.SetState(PREquipmentInventory.E_PRInvenType.Equip);
    }

    // 빈슬롯
    public void OnClickEquipSlot()
    {
        if (EquipInven.IsActive)
            return;

        EquipInven.SetState(true, CurSelectedData);
    }

    // 착용 아이템 클릭
    private void OnClickEquipSlot(PREquipItemData slot)
    {
        OnClickEquipSlot();

        var popup = UIManager.Instance.Find<UIPopupItemInfoPREquipPair>();
        if (popup != null)
        {//정보팝업 열려있음
            popup.SetPopup(slot, CurSelectedData, OnInterectEquipment, UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up);
        }
        else
        {//정보팝업 닫혀있음
            UIManager.Instance.Open<UIPopupItemInfoPREquipPair>((name, popupFrame) =>
            {
                popupFrame.SetPopup(slot, CurSelectedData, OnInterectEquipment, UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up);
            });
        }
    }

    // 장착등 상호작용 후 처리
    // 새로고침하면됨
    private void OnInterectEquipment()
    {
        SetAbilityUI(GetAbilityList(CurSelectedData));
        EquipInfo.SetEquipSlot(CurSelectedData.Tid);
        EquipInven.Refresh();
    }

    private void OnClickSetInfo(E_RuneSetType type)
    {
        CancelInvoke(nameof(FadeOutSetToolTip));
        cgSetInfoToolTip.DOKill(false);

        if(type ==  E_RuneSetType.None)
        {
            cgSetInfoToolTip.DOFade(0, .2f);
            return;
        }

        var info = UICommon.GetRuneSetAbilityTextArray(type);

        txtTooltipTitle.text = info[0];
        txtTooltipDesc.text = info[1];

        cgSetInfoToolTip.DOFade(1, .2f);
        Invoke(nameof(FadeOutSetToolTip),2f);
    }

    private void FadeOutSetToolTip()
    {
        cgSetInfoToolTip.DOFade(0, .2f);
    }
}

[Serializable]
public class PREquipInfo
{
    [Serializable]//적용중인 세트 정보
    public class PREquipSetInfo
    {
        public Image imgSetIcon;
        public Button btnSetInfo;
        [HideInInspector]
        public E_RuneSetType setType;
    }

    [Serializable]
    public class PREquipSlot
    {
        public E_EquipSlotType type;
        public UIPREquipListItem slot;
    }

    [SerializeField] private GameObject objEquipInfo;

    [SerializeField] private GameObject objSetInfo;

    [SerializeField] private List<PREquipSetInfo> listSetInfo;

    [SerializeField] private List<PREquipSlot> listEquipSlot;

    private uint targetPetTid;

    // 사용하는 타입 순회용
    private readonly E_EquipSlotType[] ARR_EQUIP_SLOT_INDEX = { E_EquipSlotType.Rune_01, E_EquipSlotType.Rune_02, E_EquipSlotType.Rune_03,
                                                                E_EquipSlotType.Rune_04, E_EquipSlotType.Rune_05, E_EquipSlotType.Rune_06 };

    // equipslot 딕셔너리로 캐싱
    private Dictionary<E_EquipSlotType, PREquipSlot> dicEquipSlot = new Dictionary<E_EquipSlotType, PREquipSlot>();

    private Action<E_RuneSetType> onClickSetType;

    public void Initialize(Action<PREquipItemData> _onClickEquipSlot, Action<E_RuneSetType> _onClickSetType)
    {
        onClickSetType = _onClickSetType;
        foreach (var iter in listEquipSlot)
        {
            if (dicEquipSlot.ContainsKey(iter.type) == false)
            {
                dicEquipSlot.Add(iter.type, iter);
                iter.slot.SetAction(_onClickEquipSlot);
            }
            iter.slot.gameObject.SetActive(false);
        }
    }

    public void SetState(bool state)
    {
        objEquipInfo.SetActive(state);
    }

    // 1개만 넘어옴
    public void SetEquipSlot(PetRuneData equipData, bool tempEquip = false)
    {
        dicEquipSlot[equipData.SlotType].slot.gameObject.SetActive(equipData != null);

        if (equipData == null)
            return;

        dicEquipSlot[equipData.SlotType].slot.SetSlot(equipData, tempEquip);

        if (tempEquip == false)
            RefreshSetInfo();
        else
            RefreshSetInfoTemp();
    }

    // 2개이상 넘어옴 -> 6칸 전부순회
    public void SetEquipSlot(uint petTid)
    {
        targetPetTid = petTid;
        Dictionary<E_EquipSlotType, PetRuneData> dicEquipData = Me.CurCharData.GetEquipRuneDic(targetPetTid);

        foreach (var iter in ARR_EQUIP_SLOT_INDEX)
        {
            PetRuneData equipData = null;

            dicEquipData.TryGetValue(iter, out equipData);

            if (equipData == null)
            {
                dicEquipSlot[iter].slot.gameObject.SetActive(false);
                continue;
            }

            SetEquipSlot(equipData);
        }
        RefreshSetInfo();
    }

    public void SetEquipSlotTemp(Dictionary<E_EquipSlotType, PetRuneData> dicTempEquip)
    {
        foreach (var iter in ARR_EQUIP_SLOT_INDEX)
        {
            PetRuneData equipData = null;

            dicTempEquip.TryGetValue(iter, out equipData);

            if (equipData == null)
            {
                dicEquipSlot[iter].slot.gameObject.SetActive(false);
                continue;
            }

            SetEquipSlot(equipData, true);
        }

        RefreshSetInfoTemp();
    }

    private void RefreshSetInfo()
    {
        var setTable = DBRune.GetAppliedSetOptionList(targetPetTid);

        int setTableCount = (setTable?.Count ?? 0);
        objSetInfo.SetActive(setTableCount > 0);

        if (setTableCount <= 0)
            return;

        List<RuneSet_Table> listTable = new List<RuneSet_Table>();

        foreach (var iter in setTable)
        {
            for (int i = 0; i < iter.Value; i++)
                listTable.Add(iter.Key);
        }

        for (int i = 0; i < listSetInfo.Count; i++)
        {
            if (listTable.Count <= i)
            {
                listSetInfo[i].imgSetIcon.gameObject.SetActive(false);
                continue;
            }
            listSetInfo[i].imgSetIcon.gameObject.SetActive(true);
            listSetInfo[i].imgSetIcon.sprite = UICommon.GetRuneSetTypeSprite(listTable[i].RuneSetType, true);
            listSetInfo[i].setType = listTable[i].RuneSetType;
            var capt = i;
            listSetInfo[i].btnSetInfo.onClick.RemoveAllListeners();
            listSetInfo[i].btnSetInfo.onClick.AddListener(() => OnClickSetInfo(capt));
        }
    }

    private void OnClickSetInfo(int i)
    {
        onClickSetType.Invoke(listSetInfo[i].setType);
    }

    private void RefreshSetInfoTemp()
    {
        Dictionary<E_RuneSetType, int> dicSetType = new Dictionary<E_RuneSetType, int>();

        foreach (var iter in dicEquipSlot)
        {
            if (iter.Value.slot.gameObject.activeSelf == false)
                continue;

            if (DBItem.GetItem(iter.Value.slot.data.data.RuneTid, out var table) == false)
                continue;

            if (dicSetType.ContainsKey(table.RuneSetType) == false)
                dicSetType.Add(table.RuneSetType, 0);

            dicSetType[table.RuneSetType]++;
        }

        List<RuneSet_Table> listTable = new List<RuneSet_Table>();

        foreach (var iter in dicSetType)
        {
            var table = DBRune.GetSetTable(iter.Key);

            int cnt = iter.Value / table.SetCompleteCount;

            for (int i = 0; i < cnt; i++)
            {
                listTable.Add(table);
            }
        }
        objSetInfo.SetActive(listTable.Count > 0);

        if (listTable.Count <= 0)
            return;

        listTable.Sort((x, y) => x.RuneSetID.CompareTo(y.RuneSetID));

        for (int i = 0; i < listSetInfo.Count; i++)
        {
            if (listTable.Count <= i)
            {
                listSetInfo[i].imgSetIcon.gameObject.SetActive(false);
                continue;
            }
            listSetInfo[i].imgSetIcon.gameObject.SetActive(true);
            listSetInfo[i].imgSetIcon.sprite = UICommon.GetRuneSetTypeSprite(listTable[i].RuneSetType, true);

            listSetInfo[i].setType = listTable[i].RuneSetType;
            var capt = i;
            listSetInfo[i].btnSetInfo.onClick.RemoveAllListeners();
            listSetInfo[i].btnSetInfo.onClick.AddListener(() => OnClickSetInfo(capt));
        }
    }
}