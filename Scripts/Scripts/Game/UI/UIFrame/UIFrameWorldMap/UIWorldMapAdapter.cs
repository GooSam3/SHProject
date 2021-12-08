using GameDB;
using System;
using System.Collections.Generic;
using static UIFrameWorldMap;

public class UIWorldMapAdapter : ZScrollAdapterBase<C_WorldMapData, UIWorldMapViewHolder>
{
    protected override string AddressableKey => nameof(UIWorldMapListItem);

    private Action<C_WorldMapData> onClickSlot;

    public C_WorldMapData selectedData { get; private set; } = null;

    protected override void OnCreateHolder(UIWorldMapViewHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetEvent(onClickSlot);
    }

    public void SetEvent(Action<C_WorldMapData> _onClickSlot)
    {
        onClickSlot = _onClickSlot;
    }

    // 리스트 클릭 및 지도내 아이콘 클릭
    public void SetSelectItem(C_WorldMapData data)
    {
        if (selectedData != null)
            selectedData.isSelected = false;

        if(data!=null)
        {
            selectedData = data;
            selectedData.isSelected = true;
        }
        gameObject.SetActive(true);
        RefreshData();
    }

    protected override void OnResetData(List<C_WorldMapData> _listData)
    {
        foreach (var iter in _listData)
        {
            iter.isSelected = false;
           // iter.dataType = C_WorldMapData.E_WorldMapDataType.None;
        }

        base.OnResetData(_listData);
    }
}


// 월드와 로컬 같은홀더사용
public class C_WorldMapData
{
    public enum E_WorldMapDataType
    {
        None = 0,
        Local = 1,
        World = 2,
    }

    public E_WorldMapDataType dataType;

    public int index;
    public bool isSelected;
    public bool isCanEnter;

    public SLocalStageInfo worldInfo;
    public Portal_Table localInfo;

    public C_WorldMapData()
    {
        SetDefault();
    }

    public C_WorldMapData(Portal_Table table)
    {
        Reset(table);
    }

    public C_WorldMapData(SLocalStageInfo info)
    {
        Reset(info);
    }

    public void Reset(C_WorldMapData data)
    {
        dataType = data.dataType;
        index = data.index;
        isSelected = data.isSelected;
        isCanEnter = data.isCanEnter;
        worldInfo = data.worldInfo;
        localInfo = data.localInfo;
    }

    public void SetDefault()
    {
        dataType = E_WorldMapDataType.None;
        index = 0;
        isSelected = false;
        isCanEnter = true;
        worldInfo = null;
        localInfo = null;
    }

    public void Reset(Portal_Table table)
    {
        SetDefault();
        dataType = E_WorldMapDataType.Local;
        localInfo = table;
    }

    public void Reset(SLocalStageInfo table)
    {
        SetDefault();
        dataType = E_WorldMapDataType.World;
        worldInfo = table;
    }
}


