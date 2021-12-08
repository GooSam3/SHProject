using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;


// UIManager로 호출시 SetType 하셔야합니다!!
// ex) UIManager.Instance.DoUIManagerShowFrame<UIFramePetChangeSelect>(E_UIFrameType.UIFramePetChangeSelect).SetViewType(E_PetChangeViewType.Change);
public class UIFramePetChangeSelect : ZUIFrameBase
{
    public enum E_SortType
    {
        All = 0,        // 전체
        Grade = 1,      // 등급      // 강림, 펫
        Class = 2,      // 클래스    // 강림
        Attribute = 3,  // 속성      // 강림
    }

    [Serializable]
    private class C_ToggleActiveObject
    {
        public List<GameObject> listObj;
    }

    [Serializable]
    private class C_CustomDropDown
    {
        public GameObject contentObj;
    }

    #region UIVariables

    [SerializeField] Text title;

    #endregion UIVariables

    #region SystemVariables

    // 타입별로 켜지는 오브젝트들
    [Header("[E_PetChangeViewType] 0 : change | 1 : pet | 2 : ride")]
    [SerializeField] private List<C_ToggleActiveObject> listToggleObject;

    [SerializeField] private UIPetChangeScrollAdapter scrollPetChange;

    [SerializeField] private UIPetChangeToolTip toolTip;

    /// 드롭다운들 <seealso cref="E_SortType"/>
    [SerializeField] private List<C_CustomDropDown> listDropDown;

    //소환 강림 등등, 인터렉션 후
    private Action OnConfirm;

    private Action OnClose;

    // 현재 뷰타입
    private E_PetChangeViewType viewType;
    public override bool IsBackable => true;
    #endregion SystemVariables

    public void Init(Action onEndInit)
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIAbilitySlot), (objAbil)=>
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPetChangeListItem), (objListItem)=>
            {
                Initialize();
                onEndInit.Invoke();

                ZPoolManager.Instance.Return(objAbil);
                ZPoolManager.Instance.Return(objListItem);
            });
        });
    }

    private void Initialize()
    {
        toolTip.Initialize(OnClickConfirm);
        scrollPetChange.Initilize();
        SetDefault();
    }

    public void Refresh()
    {
        scrollPetChange.RefreshData();

        toolTip.Refresh();
    }

    public void SetOnCloseAction(Action _OnClose)
    {
        OnClose = _OnClose;
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        OnClose = null;
    }

    protected override void OnHide()
    {
        base.OnHide();

        SetDefault();

        OnClose?.Invoke();
    }

    public void SetDefault()
    {
        foreach (var toggle in listToggleObject)
        {
            foreach (var obj in toggle.listObj)
            {
                obj.SetActive(false);
            }
        }

        toolTip.SetActive(false);
        listDropDown.ForEach((elem) => elem.contentObj.SetActive(false));
    }

    public void ActiveViewTypeObject(E_PetChangeViewType type)
    {
        if (listToggleObject.Count <= (int)type)
            return;

        foreach (var obj in listToggleObject[(int)type].listObj)
        {
            obj.SetActive(true);
        }
    }

    /// <summary>
    /// 타입세팅 및 스크롤세팅
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="_onConfirm">TID,ID</param>
    public void SetViewType(E_PetChangeViewType type, Action _onConfirm = null)
    {
        SetDefault();

        viewType = type;

        OnConfirm = _onConfirm;

        title.text = DBLocale.GetText($"Title_{type.ToString()}_Select");

        ActiveViewTypeObject(viewType);

        SetScrollList();
    }

    // 스크롤 세팅, 기본은 모두보여주는걸로
    private void SetScrollList()
    {
        SetScrollList(GetDataList());
    }

    private void SetScrollList(List<C_PetChangeData> listData)
    {
        listData.Sort(SortComparison);
        scrollPetChange.ResetData(listData, OnClickSlot);
    }

    private List<C_PetChangeData> GetDataList()
    {
        List<C_PetChangeData> dataList = new List<C_PetChangeData>();

        switch (viewType)
        {
            case E_PetChangeViewType.Change:
                {
                    var originList = Me.CurCharData.GetChangeDataList();

                    foreach (var data in originList)
                    {
                        if (DBChange.TryGet(data.ChangeTid, out Change_Table table) == false)
                            continue;

                        dataList.Add(new C_PetChangeData(data));
                    }
                }
                break;
            case E_PetChangeViewType.Pet:
                {
                    var originList = Me.CurCharData.GetPetDataList();

                    foreach (var data in originList)
                    {
                        if (DBPet.TryGet(data.PetTid, out Pet_Table table) == false)
                            continue;
                        if (table.PetType != E_PetType.Pet)
                            continue;

                        dataList.Add(new C_PetChangeData(data));
                    }
                }
                break;
            case E_PetChangeViewType.Ride:
                {
                    var originList = Me.CurCharData.GetRideDataList();

                    foreach (var data in originList)
                    {
                        if (DBPet.TryGet(data.PetTid, out Pet_Table table) == false)
                            continue;
                        if (table.PetType != E_PetType.Vehicle)
                            continue;

                        dataList.Add(new C_PetChangeData(data));
                    }
                }
                break;
        }

        return dataList;
    }

    private int SortComparison(C_PetChangeData left, C_PetChangeData right)
    {
        if (left.Grade < right.Grade)
            return 1;

        if (left.Grade > right.Grade)
            return -1;

        return 0;
    }


    public void OnClickClose()
    {
        UIManager.Instance.Close<UIFramePetChangeSelect>();
    }

    public void OnClickToolTipClose()
    {
        scrollPetChange.SetFocusItem(null);

        toolTip.OnClickClose();
    }

    public void OnClickSlot(C_PetChangeData data)
    {
        toolTip.SetToolTipData(data);

        listDropDown.ForEach(item => item.contentObj.SetActive(false));
    }

    public void OnClickConfirm()
    {
        Refresh();
        OnConfirm?.Invoke();
    }

    /// 버튼 클릭했을때, <seealso cref="E_SortType"/>
    public void OnClickSortButton(int index)
    {
        if (index >= listDropDown.Count)
            return;

        if (index == 0)//all
            SetScrollList();

        for (int i = 1; i < listDropDown.Count; i++)
        {
            if (i == index)
                listDropDown[i].contentObj.SetActive(!listDropDown[i].contentObj.activeSelf);
            else
                listDropDown[i].contentObj.SetActive(false);
        }
    }

    public void SortByGrade(int index)
    {
        var list = GetDataList();

        if (listDropDown.Count >= (int)E_SortType.Grade)
            listDropDown[(int)E_SortType.Grade].contentObj.SetActive(false);

        if (index == 0)
            SetScrollList(list);
        else
            SetScrollList(list.FindAll((data) => data.Grade == index));
    }


    /// 캐릭터 타입 <seealso cref="E_CharacterType">
    public void SortByClass(int index)
    {
        if (viewType != E_PetChangeViewType.Change) return;

        var list = GetDataList();

        if (listDropDown.Count >= (int)E_SortType.Class)
            listDropDown[(int)E_SortType.Class].contentObj.SetActive(false);

        SetScrollList(list.FindAll((data) => data.changeData.UseAttackType.HasFlag((E_CharacterType)index)));
    }

    /// 속성 타입 <seealso cref="E_UnitAttributeType">
    public void SortByAttribute(int index)
    {
        if (viewType != E_PetChangeViewType.Change) return;

        var list = GetDataList();

        if (listDropDown.Count >= (int)E_SortType.Attribute)
            listDropDown[(int)E_SortType.Attribute].contentObj.SetActive(false);

        SetScrollList(list.FindAll((data) => data.changeData.AttributeType == (E_UnitAttributeType)index));
    }
}
