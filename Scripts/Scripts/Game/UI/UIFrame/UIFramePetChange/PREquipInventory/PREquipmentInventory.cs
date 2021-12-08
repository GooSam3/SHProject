using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

// 기본 인벤토리 기능
public class PREquipmentInventory : MonoBehaviour
{
    public enum E_PRInvenType
    {
        Normal = 1, // 걍 인벤토리
        Sell = 2, // 판매
        Equip = 3, // 장착
        Sort = 4 // 관리
    }

    [Serializable]
    public class C_PRInvenContent
    {
        public E_PRInvenType type;
        public Transform invenRoot;
        public PREquipContentBase objContent;
    }

    [SerializeField] private UIPREquipScrollAdapter scrollInven;

    [SerializeField] private RectTransform scrollRectTransform;

    [SerializeField] private List<C_PRInvenContent> invenContent;

    [SerializeField] private GameObject imgBackGround;
    [SerializeField] private GameObject objBackBtn;

    [SerializeField] private ZToggle toggleSort;
    [SerializeField] private ZToggle toggleSell;

    [SerializeField] private Text txtEquipmentCapacity;

    [SerializeField] private GameObject objClose;

    private C_PRInvenContent curContent = null;

    private List<PREquipItemData> listEquipData = new List<PREquipItemData>();
    public List<PREquipItemData> ListEquipData => listEquipData;

    // 선택된 펫
    private C_PetChangeData equipTarget;
    public C_PetChangeData EquipTarget => equipTarget;

    public bool IsActive => this.gameObject.activeSelf;

    private Action onInterect;
    public Action OnInterect => onInterect;

    private bool isSortByStep = true;

    public void Init(Action _onInterect)
    {
        onInterect = _onInterect;

        scrollInven.Initialize(OnClickInvenSlot);

        foreach (var iter in invenContent)
        {
            if (iter.type == E_PRInvenType.Normal)
                continue;

            iter.objContent.Init(this);
        }

        isSortByStep = true;
    }

    // 인벤토리와 인터렉션할 대상 지정
    public void ResetEquipTarget(C_PetChangeData data)
    {
        equipTarget = data;

        UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
        UIManager.Instance.Close<UIPopupItemInfoPREquipPair>(true);

        ResetInvenData();
        RefreshInven();
    }

    // 인벤토리만 켜줌
    public void SetState(bool state, C_PetChangeData targetData = null)
    {
        if (state == false)
        {
            UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
            UIManager.Instance.Close<UIPopupItemInfoPREquipPair>(true);
        }

        if (curContent != null && curContent.type != E_PRInvenType.Normal)
        {
            curContent.objContent.Close(false);
        }
        curContent = invenContent[0];

        if (state)
        {
            ResetInvenData();
            AttachAdapter(curContent.invenRoot);
        }

        RefreshUI();

        if(targetData!=null)
            ResetEquipTarget(targetData);

        objClose.SetActive(true);

        this.gameObject.SetActive(state);
    }

    public void SetSortType(bool _isSortByStep)
    {
        isSortByStep = _isSortByStep;
        RefreshInven();
    }    

    // 꺼져있으면 해당타입으로 켜줌, 켜진상태면 상태이동
    public void SetState(E_PRInvenType type)
    {
        UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
        UIManager.Instance.Close<UIPopupItemInfoPREquipPair>(true);

        isSortByStep = false;

        objClose.gameObject.SetActive(type == E_PRInvenType.Normal);

        // 기본인벤은 equipcontent 없음 
        if (type == E_PRInvenType.Normal)
        {
            SetState(true);

            return;
        }

        foreach (var iter in invenContent)
        {
            if (type == iter.type)
            {
                iter.objContent.Open();
                curContent = iter;
            }
            else
            {
                if (iter.type != E_PRInvenType.Normal)
                    iter.objContent.Close(false);
            }
        }
        ResetInvenData();

        AttachAdapter(curContent.invenRoot);

        RefreshUI();

        RefreshInven();
        this.gameObject.SetActive(true);
    }

    private void ResetInvenData()
    {
        listEquipData.Clear();

        foreach (var iter in Me.CurCharData.RuneDic.Values)
        {
            var data = new PREquipItemData(iter);
            listEquipData.Add(data);
        }

        int leftCount = 0;

        if (listEquipData.Count > ZUIConstant.PR_EQUIP_INVEN_COUNT_MIN)
        {
            var left = listEquipData.Count % ZUIConstant.PR_EQUIP_INVEN_WIDTH_COUNT;
            if (left > 0)
                leftCount = ZUIConstant.PR_EQUIP_INVEN_WIDTH_COUNT - left;//% ZUIConstant.PR_EQUIP_INVEN_WIDTH_COUNT;
        }
        else
            leftCount = ZUIConstant.PR_EQUIP_INVEN_COUNT_MIN - listEquipData.Count;

        for (int i = 0; i < leftCount; i++)
        {
            listEquipData.Add(new PREquipItemData());
        }

        scrollInven.ResetListData(listEquipData);
    }

    public void RefreshInven()
    {
        if (curContent == null)
            return;

        foreach (var iter in scrollInven.Data.List)
        {
            // 빈칸
            if (iter.data == null)
            {
                continue;
            }

            if (iter.isMoved)
            {
                iter.isVisible = false;
                continue;
            }

            if (curContent.type != E_PRInvenType.Normal)
            {
                bool filterState = curContent.objContent.Filter(iter.data);
                iter.isVisible = filterState;

                if (filterState == false)
                    continue;
            }
            else
            {
                iter.isVisible = true;
            }

            if (iter.isVisible == false)
                continue;

            if(curContent.type == E_PRInvenType.Equip && iter.data.OwnerPetTid>0)
            {
                iter.SortFirst = false;
                iter.isDisable = true;
                continue;
            }

            iter.SortFirst = (iter.data.OwnerPetTid == (equipTarget?.Tid ?? iter.data.OwnerPetTid + 1));
            iter.isDisable = (iter.data.OwnerPetTid > 0 && iter.data.OwnerPetTid != equipTarget?.Tid);
        }

        if(isSortByStep)
        {
            scrollInven.Data.List.Sort(InvenSortComparisonStep);
        }
        else
        {
            scrollInven.Data.List.Sort(InvenSortComparisonGrade);
        }


        scrollInven.RefreshData();// (listEquipData);

        txtEquipmentCapacity.text = UICommon.GetProgressText(Me.CurCharData.GetRuneCountAll(), (int)DBConfig.Rune_Inventory_Max_Count);
    }

    private int InvenSortComparisonStep(PREquipItemData left, PREquipItemData right)
    {
        // 안보이는놈이면 맨뒤로
        if (left.isVisible == false)
            return 1;
        if (right.isVisible == false)
            return -1;

        if (left.SortFirst != right.SortFirst)
        {
            if (left.SortFirst)
                return -1;
            if (right.SortFirst)
                return 1;
        }

        if (left.isDisable != right.isDisable)
        {
            if (left.isDisable)//다른놈이 장착중
                return 1;
            if (right.isDisable)//다른놈이 장착중
                return -1;
        }

        if (left.enchantTable.EnchantStep > right.enchantTable.EnchantStep)
            return -1;

        if (left.enchantTable.EnchantStep < right.enchantTable.EnchantStep)
            return 1;

        if (left.itemTable.Grade > right.itemTable.Grade)
            return -1;

        if (left.itemTable.Grade < right.itemTable.Grade)
            return 1;

        if (left.itemTable.RuneGradeType > right.itemTable.RuneGradeType)
            return -1;

        if (left.itemTable.RuneGradeType < right.itemTable.RuneGradeType)
            return 1;

        if (left.itemTable.ItemID > right.itemTable.ItemID)
            return -1;

        if (left.itemTable.ItemID < right.itemTable.ItemID)
            return 1;

        //최종으론 획득순
        if (left.data.RuneId < right.data.RuneId)
            return -1;
        else
            return 1;
    }

    private int InvenSortComparisonGrade(PREquipItemData left, PREquipItemData right)
    {       
        // 안보이는놈이면 맨뒤로
        if (left.isVisible == false)
            return 1;
        if (right.isVisible == false)
            return -1;

        if (left.SortFirst != right.SortFirst)
        {
            if (left.SortFirst)
                return -1;
            if (right.SortFirst)
                return 1;
        }

        if (left.isDisable != right.isDisable)
        {
            if (left.isDisable)//다른놈이 장착중
                return 1;
            if (right.isDisable)//다른놈이 장착중
                return -1;
        }
        if (left.itemTable.Grade > right.itemTable.Grade)
            return -1;

        if (left.itemTable.Grade < right.itemTable.Grade)
            return 1;

        if (left.itemTable.RuneGradeType > right.itemTable.RuneGradeType)
            return -1;

        if (left.itemTable.RuneGradeType < right.itemTable.RuneGradeType)
            return 1;

        if (left.enchantTable.EnchantStep > right.enchantTable.EnchantStep)
            return -1;

        if (left.enchantTable.EnchantStep < right.enchantTable.EnchantStep)
            return 1;

        if (left.itemTable.ItemID > right.itemTable.ItemID)
            return -1;

        if (left.itemTable.ItemID < right.itemTable.ItemID)
            return 1;


        //최종으론 획득순
        if (left.data.RuneId < right.data.RuneId)
            return -1;
        else
            return 1;

    }

    public void AttachAdapter(Transform rootTR)
    {
        if (rootTR.gameObject.activeSelf == false)
            rootTR.gameObject.SetActive(true);

        scrollRectTransform.transform.SetParent(rootTR);
        scrollRectTransform.localScale = Vector3.one;
        scrollRectTransform.SetAnchor(AnchorPresets.VertStretchRight);
        scrollRectTransform.anchoredPosition = Vector2.zero;
        scrollRectTransform.sizeDelta = new Vector2(ZUIConstant.PR_EQUIP_INVEN_WIDTH, 0);
    }

    private bool IsUsePairPopup()
    {
        return true;
    }

    private void OnClickInvenSlot(PREquipItemData data)
    {
        if (data.isVisible == false)
            return;

        if (curContent.type != E_PRInvenType.Normal)
        {
            curContent.objContent.OnInvenClick(data);
            return;
        }

        bool usePairPopup = IsUsePairPopup();

        if (usePairPopup)
        {
            // 장착중인놈은 무조건 위로 출력한다.
            var popup = UIManager.Instance.Find<UIPopupItemInfoPREquipPair>();
            if (popup != null)
            {//정보팝업 열려있음
                popup.SetPopup(data, equipTarget, onInterect, data.data.OwnerPetTid > 0 ? UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up : UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down );
            }
            else
            {//정보팝업 닫혀있음
                UIManager.Instance.Open<UIPopupItemInfoPREquipPair>((name, popupFrame) =>
                {
                    popupFrame.SetPopup(data, equipTarget, onInterect, data.data.OwnerPetTid > 0 ? UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up : UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down);
                });
            }
        }
        else
        {

        }


    }

    // 필요정보만 갱신
    public void Refresh()
    {
        //AttachAdapter(curContent.invenRoot);
        RefreshUI();

        ResetInvenData();
        RefreshInven();

        if (curContent.type != E_PRInvenType.Normal)
        {
            curContent.objContent.Refresh();
        }
    }

    public void RefreshUI()
    {
        bool isNormalContent = curContent.type == E_PRInvenType.Normal;
        imgBackGround.SetActive(!isNormalContent);
        objBackBtn.SetActive(!isNormalContent);

        toggleSort.SelectToggleSingle(curContent.type == E_PRInvenType.Sort, false);
        toggleSell.SelectToggleSingle(curContent.type == E_PRInvenType.Sell, false);
    }

    public void OnClickSort()
    {
        if(curContent.type == E_PRInvenType.Sort)
		{
            OnClickClose();
            return;
		}

        SetState(E_PRInvenType.Sort);
    }

    public void OnClickSell()
    {
        if (curContent.type == E_PRInvenType.Sell)
        {
            OnClickClose();
            return;
        }

        SetState(E_PRInvenType.Sell);
    }

    public void OnClickClose()
    {
        SetState(false);
    }
    #region EDITOR
#if UNITY_EDITOR
    [ContextMenu("CLOSE_POPUP")]
    public void CLOSE_POPUP()
    {
        SetState(false);
    }

    public void SET_STATE(E_PRInvenType type)
    {
        foreach (var iter in invenContent)
        {
            if (type == iter.type)
            {
                if (iter.type != E_PRInvenType.Normal)
                {
                    iter.objContent.Open();
                }
                AttachAdapter(iter.invenRoot);
            }
            else if (iter.type != E_PRInvenType.Normal)
                iter.objContent.Close(false);
        }

        imgBackGround.SetActive(type != E_PRInvenType.Normal);

        this.gameObject.SetActive(true);
    }

    [ContextMenu("OPEN_NORMAL")]
    public void OPEN_TYPE_1()
    {
        SET_STATE(E_PRInvenType.Normal);
    }
    [ContextMenu("OPEN_SELL")]
    public void OPEN_TYPE_2()
    {
        SET_STATE(E_PRInvenType.Sell);
    }
    [ContextMenu("OPEN_EQUIP")]
    public void OPEN_TYPE_3()
    {
        SET_STATE(E_PRInvenType.Equip);
    }
    [ContextMenu("OPEN_SORT")]
    public void OPEN_TYPE_4()
    {
        SET_STATE(E_PRInvenType.Sort);
    }
#endif
    #endregion EDITOR
}

public abstract class PREquipContentBase : MonoBehaviour
{
    [SerializeField] private List<UIPREquipFilterListItem> listFilter;

    protected PREquipmentInventory owner;

    private Dictionary<(E_PREquipFilterType, uint), UIPREquipFilterListItem> dicNoneOSAFilter = new Dictionary<(E_PREquipFilterType, uint), UIPREquipFilterListItem>();

    public virtual void Init(PREquipmentInventory _owner)
    {
        owner = _owner;

        foreach (var iter in listFilter)
        {
            if (iter.data == null)
                continue;

            iter.SetAction(OnClickFilterSlot);

            iter.data.CompressData();
            iter.data.isNotUsedInOSA = true;

            var key = (iter.data.type, iter.data.intData);

            if (dicNoneOSAFilter.ContainsKey(key) == false)
            {
                dicNoneOSAFilter.Add(key, iter);
            }
        }

        Close(false);
    }

    public virtual void Close(bool invokeAction)
    {
        UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
        UIManager.Instance.Close<UIPopupItemInfoPREquipPair>(true);

        this.gameObject.SetActive(false);
    }
    public virtual void Open()
    {
        this.gameObject.SetActive(true);
        RefreshFilter(E_PREquipFilterType.None);
    }

    public virtual void Refresh()
    {

    }

    public void OnClickResetFilter(int i)
    {
        RefreshFilter((E_PREquipFilterType)i);
    }

    protected virtual void RefreshFilter(E_PREquipFilterType filter)
    {
        foreach (var iter in dicNoneOSAFilter.Values)
        {
            if (filter == E_PREquipFilterType.None || filter == iter.data.type)
            {
                iter.data.isOn = true;
                iter.SetState(iter.data.isOn);
            }
        }

        owner.RefreshInven();
    }

    protected virtual void RefreshFilterAll()
    {
        foreach (var iter in dicNoneOSAFilter.Values)
        {
            iter.data.isOn = true;
            iter.SetState(iter.data.isOn);
        }
    }


    public virtual bool Filter(PetRuneData data)
    {
        if (DBItem.GetItem(data.RuneTid, out var table) == false)
            return false;

        if (DBRune.GetRuneEnchantTable(data.BaseEnchantTid, out var enchatTable) == false)
            return false;


        foreach (var iter in dicNoneOSAFilter)
        {
            if (iter.Value.data.isOn)
                continue;

            switch (iter.Key.Item1)
            {
                case E_PREquipFilterType.EquipSlot:
                    if ((E_EquipSlotType)iter.Key.Item2 == table.EquipSlotType)
                        return false;
                    break;
                case E_PREquipFilterType.Grade:
                    if (iter.Key.Item2 == (uint)table.RuneGradeType)
                        return false;
                    break;
                case E_PREquipFilterType.StarGrade:
                    if (iter.Key.Item2 == table.Grade)
                        return false;
                    break;
            }
        }

        return true;
    }

    protected virtual void OnClickFilterSlot(PREquipFilterData data)
    {
        if (data.isNotUsedInOSA == false)
            return;

        data.isOn = !data.isOn;

        bool state = data.isOn;

        dicNoneOSAFilter[(data.type, data.intData)].SetState(state);

        owner.RefreshInven();
    }


    public abstract void OnInvenClick(PREquipItemData data);
}