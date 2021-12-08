using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;


public class UIPetChangeScrollAdapter : GridAdapter<GridParams, UIPetChangeViewHolder>
{
    public SimpleDataHelper<C_PetChangeData> Data { get; private set; }

    private Action<C_PetChangeData> onClickSlot;

    // 포커스 후 클릭시
    private Action<C_PetChangeData> onDoubleClickSlot;

    public C_PetChangeData selectedData { get; private set; } = null;

    public int Count { get; private set; }

    protected override void Start() { }

    protected override void UpdateCellViewsHolder(UIPetChangeViewHolder viewsHolder)
    {
        if (viewsHolder == null)
            return;

        C_PetChangeData data = Data[viewsHolder.ItemIndex];
        viewsHolder.UpdateItemSlot(data, SetFocusItem);
    }

    public void RefreshData()
    {
        for (int i = 0; i < base.CellsCount; i++)
            UpdateCellViewsHolder(base.GetCellViewsHolder(i));
    }

    public void Initilize()
    {
        if (Data == null)
        {
            Data = new SimpleDataHelper<C_PetChangeData>(this);
        }

        GameObject slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIPetChangeListItem));

        Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        Parameters.Grid.CellPrefab.SetParent(GetComponent<Transform>());
        Parameters.Grid.CellPrefab.transform.localScale = Vector2.one;
        Parameters.Grid.CellPrefab.transform.localPosition = new Vector3(0, 0, 0);
        Parameters.Grid.CellPrefab.gameObject.SetActive(false);

        Init();
    }

    public void SetFocusItem(C_PetChangeData data)
    {
        if (selectedData != null)
        {
            if (data != null && selectedData.Id == data.Id&& selectedData.Id != 0)
            {
                onDoubleClickSlot?.Invoke(data);
                return;
            }

            selectedData.isSelected = false;
        }

        selectedData = data;

        if (selectedData != null)
        {
            selectedData.isSelected = true;
            onClickSlot.Invoke(data);
        }

        RefreshData();
    }

    public void ResetData(List<C_PetChangeData> dataList, Action<C_PetChangeData> _onClickSlot, Action<C_PetChangeData> _onDoubleClickSlot = null)
    {
        if (selectedData != null)
        {
            selectedData.isSelected = false;
            selectedData = null;
        }

        onClickSlot = _onClickSlot;
        onDoubleClickSlot = _onDoubleClickSlot;
        
        Data.ResetItems(dataList);
        SetNormalizedPosition(1);
    }
}

// 보기 설정
public enum E_PetChangeViewType
{
    Change = 0,
    Pet = 1,
    Ride = 2,
}

public class C_PetChangeData
{
    public E_PetChangeViewType type;

    public Change_Table changeData = null;
    public Pet_Table petData = null;

    public uint Tid;
    public ulong Id;
    public uint Grade;
    public bool IsOwn;

    public uint Order;

    public bool isSelected = false;

    public bool isLock = false;

    public bool isEquiped = false;

    // 일반적인상황에서 할당하지 않음, 
    // 슬롯은 모든 데이터를 갱신 후 해당값으로 재갱신
    public E_PCR_PostSetting postSetting = E_PCR_PostSetting.None;

    // UI상 노출될 갯수(실제갯수와 상이할수있음)
    public int ViewCount;

    public UIPetChangeListItem ItemInstance;

    public C_PetChangeData() { }

    public C_PetChangeData(C_PetChangeData data, int count = 0)
    {
        Reset(data);
    }

    public C_PetChangeData(Change_Table data, int count = 0)
    {
        Reset(data, count);
    }

    public C_PetChangeData(ChangeData data, int count = 0)
    {
        Reset(DBChange.Get(data.ChangeTid), count);
    }

    public C_PetChangeData(Pet_Table data, int count = 0)
    {
        Reset(data, count);
    }

    public C_PetChangeData(PetData data, int count = 0)
    {
        Reset(DBPet.GetPetData(data.PetTid), count);
    }

    public void Reset(C_PetChangeData data)
    {
        changeData = data.changeData;
        petData = data.petData;
        type = data.type;
        Tid = data.Tid;
        Grade = data.Grade;
        IsOwn = data.IsOwn;
        Id = data.Id;
        isSelected = data.isSelected;
        ViewCount = data.ViewCount;
        Order = data.Order;
        isLock = data.isLock;
        isEquiped = data.isEquiped;
    }

    public void Reset(Change_Table data, int count = 0)
    {
        changeData = data;
        type = E_PetChangeViewType.Change;
        Tid = changeData.ChangeID;
        Grade = changeData.Grade;

        ChangeData myData = Me.CurCharData.GetChangeDataByTID(Tid);
        IsOwn = myData != null;
        Id = IsOwn ? myData.ChangeId : 0;

        Order = data.Sort;
        ViewCount = count;

        isLock = myData?.IsLock ?? false;
        isEquiped = Me.CurCharData.MainChange == Tid;
    }

    public void Reset(Pet_Table data, int count = 0)
    {
        petData = data;
        
        type = data.PetType == E_PetType.Pet ? E_PetChangeViewType.Pet : E_PetChangeViewType.Ride;
        Tid = petData.PetID;
        Grade = petData.Grade;

        PetData myData = data.PetType == E_PetType.Pet ? Me.CurCharData.GetPetData(Tid) : Me.CurCharData.GetRideData(Tid);
        IsOwn = myData != null;
        Id = IsOwn ? myData.PetId : 0;

        Order = data.Sort;
        ViewCount = count;

        isLock = myData?.IsLock ?? false;
        isEquiped = data.PetType == E_PetType.Pet? Me.CurCharData.MainPet == Tid : Me.CurCharData.MainVehicle == Tid;

    }
}
