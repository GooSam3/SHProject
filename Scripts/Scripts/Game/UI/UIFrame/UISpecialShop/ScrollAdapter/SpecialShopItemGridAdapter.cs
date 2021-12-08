using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using ZNet.Data;
using UnityEngine.UI;
using UnityEngine;
using static DBSpecialShop;
using static SpecialShopCategoryDescriptor;
using Com.TheFallenGames.OSA.Util;

public class SpecialShopItemGridAdapter : GridAdapter<SpecialShopAdapterGridParam, SpecialShopItemScrollAdapterHolder>
{
    private SimpleDataHelper<SingleDataInfo> Data;

    /// <summary> 튜토리얼에서 사용 </summary>
    public SimpleDataHelper<SingleDataInfo> SingleData { get { return Data; } }

    public Action<SingleDataInfo> onClickedSlot;
    
    public Vector2 bigItemCellSize;
    Vector2 smallItemCellSize;

    #region Public Methods
    protected override void Start()
    {
        //    base.Start();
    }

    public void Initialize(int maximumDataCountOfAll)
    {
        if (bigItemCellSize.x == 0 ||
            bigItemCellSize.y == 0)
        {
            ZLog.LogError(ZLogChannel.UI, "Please Set big size item size");
        }

        Data = new SimpleDataHelper<SingleDataInfo>(this);
        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(SpecialShopItemSlot));
        Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        var pf = Parameters.Grid.CellPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);
        smallItemCellSize = new Vector2(pf.rect.width, pf.rect.height);

        /// TODO : -1 로 설정해놓고 하면은 , 최소한의 CellGroup 이 생성되지만 
        /// Slot 들이 Horizontal 방향으로 Grow 가 됨 . 몇가지 테스트하였지만 우선 이렇게 적용함
        /// 후에 -1 로 설정해놓고 할 수있는 방법을 PakcedGridLayoutGroup 을 분석해서 수정하던지 해야할듯함. 
        Parameters.Grid.MaxCellsPerGroup = maximumDataCountOfAll;
        Init();
    }

    public void RefreshData(List<SingleDataInfo> list)
    {
        Data.ResetItems(list);
    }

    public void OnSlotClicked(SpecialShopItemScrollAdapterHolder holder)
    {
        if (holder.ItemIndex == -1)
            return;

        onClickedSlot?.Invoke(Data[holder.ItemIndex]);
    }

    protected override CellGroupViewsHolder<SpecialShopItemScrollAdapterHolder> GetNewCellGroupViewsHolder()
    {
        // Create cell group holders of our custom type (which stores the CSF component)
        return new SpecialShopItemAdapterCellGroupHolder();
    }

    /// <param name="viewsHolder"></param>
    //public void SetSelectedMarkID(byte tid, bool notifyDataUpdate)
    //{
    //    selectedMarkTID = tid;

    //    if (notifyDataUpdate)
    //        Data.NotifyListChangedExternally();
    //}
    #endregion

    #region Private Methods
    //private void OnClickedSlot(ScrollGuildCreateGuildGridHolder holder)
    //{
    //    Data[holder.ItemIndex].hasBeenSelected = true;
    //    onClickedSlot?.Invoke(holder);
    //}
    #endregion


    #region OSA Overrides
    protected override void UpdateViewsHolder(CellGroupViewsHolder<SpecialShopItemScrollAdapterHolder> newOrRecycled)
    {
        base.UpdateViewsHolder(newOrRecycled);

        // Constantly triggering a twin pass after the current normal pass, so the CSFs will be updated
        ScheduleComputeVisibilityTwinPass();
    }

    protected override void UpdateCellViewsHolder(SpecialShopItemScrollAdapterHolder viewsHolder)
    {
        var t = Data[viewsHolder.ItemIndex];
        var data = DBSpecialShop.Get(t.specialShopId);

        if (data.SizeType == GameDB.E_SizeType.Big) 
        {
            viewsHolder.rootLayoutElement.preferredWidth = bigItemCellSize.x;
            viewsHolder.rootLayoutElement.preferredHeight = bigItemCellSize.y;
        }
        else
        {
            viewsHolder.rootLayoutElement.preferredWidth = smallItemCellSize.x;
            viewsHolder.rootLayoutElement.preferredHeight = smallItemCellSize.y;
        }

        viewsHolder.UpdateSlot(t, OnSlotClicked);
    }
    #endregion
}

/// <inheritdoc/>
[Serializable]
public class SpecialShopAdapterGridParam : GridParams
{
    public bool biggerChildrenFirst = true;

    protected override LayoutGroup AddLayoutGroupToCellGroupPrefab(GameObject cellGroupGameObject)
    {
        return cellGroupGameObject.AddComponent<PackedGridLayoutGroup>();
    }

    protected override void InitOrReinitCellGroupPrefabLayoutGroup(LayoutGroup cellGroupGameObject)
    {
        base.InitOrReinitCellGroupPrefabLayoutGroup(cellGroupGameObject);

        var packedGridLG = cellGroupGameObject as PackedGridLayoutGroup;
        packedGridLG.ForcedSpacing = Grid.SpacingInGroup;
        packedGridLG.ChildrenControlSize = PackedGridLayoutGroup.AxisOrNone.Horizontal;
        packedGridLG.BiggerChildrenFirst = biggerChildrenFirst;
    }
}

public class SpecialShopItemAdapterCellGroupHolder : CellGroupViewsHolder<SpecialShopItemScrollAdapterHolder>
{
    public UnityEngine.UI.ContentSizeFitter contentSizeFitterComponent;

    public override void CollectViews()
    {
        base.CollectViews();

        contentSizeFitterComponent = root.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        contentSizeFitterComponent.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitterComponent.enabled = true;
    }
}

public class SpecialShopItemScrollAdapterHolder : CellViewsHolder
{
    private SpecialShopItemSlot slot;
    public SpecialShopItemSlot Slot { get { return slot; } }

    Action<SpecialShopItemScrollAdapterHolder> onClicked;

    public void UpdateSlot(SingleDataInfo data, Action<SpecialShopItemScrollAdapterHolder> onClickedSlot)
    {
        slot.SetUI(data);
        onClicked = onClickedSlot;
        
        /// TODO : 활성화되지않은 Slot 들이 가려버리는 이슈가있어서 , 보일애들은 LastSibling 세팅 
        slot.transform.SetAsLastSibling();
    }

    public override void CollectViews()
    {
        slot = root.GetComponent<SpecialShopItemSlot>();
        slot.SetOnClickHandler(() => onClicked?.Invoke(this));
        base.CollectViews();

        rootLayoutElement.preferredWidth = 0f;
    }
}

//public class SpecialShopItemModel
//{
//    public uint specialShopTid;
//    public uint externalTargetTableID;
//    public E_SpecialShopDisplayGoodsTarget dataType;

//    public SpecialShopItemModel(uint specialShopTid, uint externalTargetTableID, E_SpecialShopDisplayGoodsTarget dataType)
//    {
//        this.specialShopTid = specialShopTid;
//        this.externalTargetTableID = externalTargetTableID;
//        this.dataType = dataType;
//    }
//}
