using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UIFrameArtifactMaterialManagementBase;

public class ScrollArtifactMaterialGridAdapter : GridAdapter<GridParams, ScrollArtifactMaterialGridHolder>
{
    private SimpleDataHelper<MaterialDataOnSlot> Data;
    public Action<int, MaterialDataOnSlot> onClickedSlot;
    bool init;
    bool showCount;
    bool disableOnCountZero;

    #region Public Methods
    public void Initialize(bool showCount, bool disableOnCountZero)
    {
        if (init)
            return;

        init = true;
        this.showCount = showCount;
        this.disableOnCountZero = disableOnCountZero;
        Data = new SimpleDataHelper<MaterialDataOnSlot>(this);
        Init();
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddOnClicked(Action<int, MaterialDataOnSlot> callback)
    {
        onClickedSlot += callback;
    }

    public void ApplyData()
    {
        Data.NotifyListChangedExternally();
    }
    public void RefreshData(List<MaterialDataOnSlot> data)
    {
        Data.ResetItems(data);
    }
    #endregion

    #region Private Methods
    private void OnClickedSlot(ScrollArtifactMaterialGridHolder holder)
    {
        onClickedSlot?.Invoke(holder.ItemIndex, Data[holder.ItemIndex]);
    }
    #endregion

    #region OSA Overrides
    protected override void UpdateCellViewsHolder(ScrollArtifactMaterialGridHolder viewsHolder)
    {
        var t = Data[viewsHolder.ItemIndex];
        viewsHolder.SetListenerOnClick(OnClickedSlot);

        if (viewsHolder.Slot.gameObject.activeSelf == false)
            viewsHolder.Slot.gameObject.SetActive(true);

        Sprite gradeBG = ZManagerUIPreset.Instance.GetSprite(t.gradeSpriteName);

        if (t.cntByContext == 0 && disableOnCountZero)
        {
            viewsHolder.Slot.ClearSlot(gradeBG);
        }
        else
        {
            viewsHolder.Slot.SetUI(viewsHolder.ItemIndex, showCount, gradeBG, t);
        }
    }
    #endregion
}

public class ScrollArtifactMaterialGridHolder : CellViewsHolder
{
    private UIFrameArtifactResourceSlot slot;
    public UIFrameArtifactResourceSlot Slot { get { return slot; } }

    Action<ScrollArtifactMaterialGridHolder> onClicked;

    public void SetListenerOnClick(Action<ScrollArtifactMaterialGridHolder> callback)
    {
        onClicked = callback;
    }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIFrameArtifactResourceSlot>();
        slot.AddListener_OnClicked(() => onClicked?.Invoke(this));
        base.CollectViews();
    }
}