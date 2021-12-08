using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using ZNet.Data;

public class ScrollArtifactManufactureItemListAdapter : OSA<BaseParamsWithPrefab, ScrollArtifactManufactureItemListHolder>
{
    private SimpleDataHelper<ScrollArtifactManufactureItemListModel> Data;

    bool init;

    private Action<ScrollArtifactManufactureItemListModel> onClicked;

    private ulong selectedArtifactID;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        Data = new SimpleDataHelper<ScrollArtifactManufactureItemListModel>(this);
        Init();
    }

    public ScrollArtifactManufactureItemListModel GetFirstDataFound(uint excludeTid)
    {
        return Data.List.Find(t => t.artifactID != excludeTid);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public uint SelectFirstArtifactID()
    {
        uint id = 0;

        if (Data.List.Count > 0)
        {
            id = Data.List.First().artifactID;
        }

        selectedArtifactID = id;
        return id;
    }

    public void SelectArtifactID(ulong id)
    {
        selectedArtifactID = id;
    }

    public bool SnapTo(uint targetArtifactID)
    {
        if (Data == null || Data.List == null || Data.List.Count == 0)
            return false;

        int targetItemIndex = 0;

        for (int i = 0; i < Data.List.Count; i++)
        {
            if (Data.List[i].artifactID == targetArtifactID)
            {
                targetItemIndex = i;
                break;
            }
        }

        if (targetItemIndex == 0)
            return false;

        ScrollTo(targetItemIndex);
        return true;
    }

    public void RefreshData()
    {
        Data.NotifyListChangedExternally();
    }

    public void RefreshData(List<ScrollArtifactManufactureItemListModel> list)
    {
        Data.ResetItems(list);
    }

    public void AddListener_OnClick(Action<ScrollArtifactManufactureItemListModel> callback)
    {
        this.onClicked += callback;
    }

    public void RemoveListener_OnClick(Action<ScrollArtifactManufactureItemListModel> callback)
    {
        this.onClicked -= callback;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OSA Overrides
    protected override ScrollArtifactManufactureItemListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollArtifactManufactureItemListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClicked(() => onClicked?.Invoke(Data[holder.ItemIndex]));
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollArtifactManufactureItemListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];
        bool isSelected = selectedArtifactID == Data[newOrRecycled.ItemIndex].artifactID;
        bool isObtained = t.isObtained;
        bool isEquipped = Me.CurCharData.IsThisArtifactEquipped(t.artifactID);
        var data = DBArtifact.GetArtifactByID(t.artifactID);

        newOrRecycled.Slot.SetData(
            ZManagerUIPreset.Instance.GetSprite(data.Icon)
            , ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(data.Grade))
            , DBLocale.GetText(data.ArtifactName)
            , UIFrameArtifact.GetColorByGrade(data.Grade)
            , isSelected
            , isObtained
            , isEquipped);
    }
    #endregion
}

public class ScrollArtifactManufactureItemListHolder : BaseItemViewsHolder
{
    private ScrollArtifactManufactureItemListSlot slot;
    public ScrollArtifactManufactureItemListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollArtifactManufactureItemListSlot>();
        base.CollectViews();
    }
}

public class ScrollArtifactManufactureItemListModel
{
    public uint artifactID;
    public bool isObtained;
}