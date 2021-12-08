using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DBArtifact;
using static UIFrameArtifactLink;

public class ScrollArtifactLinkListAdapter : OSA<BaseParamsWithPrefab, ScrollLinkListHolder>
{
    private SimpleDataHelper<LinkDataExtended> Data;

    bool init;

    private Action<int, LinkDataExtended> onClicked;
    private Action<uint> _onClickedLeftMaterial;
    private Action<uint> _onClickedRightMaterial;

    List<ArtifactLink> myObtainedLinkIDs;
    List<uint> myArtifacts;

    uint selectedTid;

    #region Public Methods
    public void Initialize()
    {
        if (init)
            return;

        init = true;

        myObtainedLinkIDs = new List<ArtifactLink>();
        myArtifacts = new List<uint>();
        Data = new SimpleDataHelper<LinkDataExtended>(this);
        Init();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetSelectedData_Tid(uint tid)
    {
        selectedTid = tid;
    }

    public void SetSelectedData_Index(int index)
    {
        if (index >= Data.Count)
        {
            ZLog.LogError(ZLogChannel.UI, "index cannot be larger than the whole count , check the data error");
            return;
        }

        selectedTid = Data[index].baseData.tid;
    }

    public void Refresh(List<LinkDataExtended> list)
    {
        //myArtifacts.Clear();
        //myObtainedLinkIDs.Clear();

        ///// 내 아티팩트 ++ 
        //foreach (var artifact in Me.CurCharData.ArtifactItemList)
        //{
        //    myArtifacts.Add(artifact.Value.ArtifactTid);
        //}

        //DBArtifact.GetLinkIDsByArtifactIDs(false, myArtifacts, ref myObtainedLinkIDs);

        Data.ResetItems(list);
    }

    public void AddListener_OnClick(Action<int, LinkDataExtended> callback)
    {
        this.onClicked += callback;
    }

    public void RemoveListener_OnClick(Action<int, LinkDataExtended> callback)
    {
        this.onClicked -= callback;
    }

    public void AddListener_OnClickedLeftMat(Action<uint> callback)
    {
        _onClickedLeftMaterial += callback;
    }

    public void RemoveListener_OnClickedLeftMat(Action<uint> callback)
    {
        _onClickedLeftMaterial -= callback;
    }

    public void AddListener_OnClickedRightMat(Action<uint> callback)
    {
        _onClickedRightMaterial += callback;
    }

    public void RemoveListener_OnClickedRightMat(Action<uint> callback)
    {
        _onClickedRightMaterial -= callback;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OSA Overrides
    protected override ScrollLinkListHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new ScrollLinkListHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        holder.Slot.AddListener_OnClicked(() => onClicked?.Invoke(itemIndex, Data[holder.ItemIndex]));
        holder.Slot.AddListener_OnClickedLeftMat((artifactID) => _onClickedLeftMaterial?.Invoke(artifactID));
        holder.Slot.AddListener_OnClickedRightMat((artifactID) => _onClickedRightMaterial?.Invoke(artifactID));
        return holder;
    }

    protected override void UpdateViewsHolder(ScrollLinkListHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];

        if (t.material01.isDataExistOnTable == false ||
            t.material02.isDataExistOnTable == false)
        {
            newOrRecycled.Slot.SetUI(0, 0, null, null, null, null, Color.white, "DataNotFound", "(0 / 0)", false, false, false, false);
            return;
        }

        int obtainedMatCnt = 0;

        if (t.material01.isObtained)
            obtainedMatCnt++;
        if (t.material02.isObtained)
            obtainedMatCnt++;

        string linkTitle = DBLocale.GetText(t.titleKey);
        string cntStr = string.Empty;

        /// link 소유중이 아니라면 카운트까지 표기
        if (t.isObtained == false)
        {
            cntStr = string.Format("({0} / {1})", obtainedMatCnt, 2);
        }

        newOrRecycled.Slot.SetUI(
            t.material01.tid_forDisplay
            , t.material02.tid_forDisplay
           , GetSprite(t.material01.artifactIcon)
            , t.material01.gradeSprite_forDisplay
            , GetSprite(t.material02.artifactIcon)
            , t.material02.gradeSprite_forDisplay
            , UIFrameArtifact.GetColorByGrade((byte)t.grade)
            , linkTitle
            , cntStr
            , t.isObtained
            , t.material01.isObtained
            , t.material02.isObtained
            , t.isSelected);
    }

    Sprite GetSprite(string name)
    {
        return ZManagerUIPreset.Instance.GetSprite(name);
    }

    /// <summary>
    ///  ex) ff0fab
    /// </summary>
    Color ParseColor(string str)
    {
        Color result = new Color(0, 0, 0, 0);

        if (ColorUtility.TryParseHtmlString(str, out result) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Color parsing error");
        }

        return result;
    }
    #endregion
}

public class ScrollLinkListHolder : BaseItemViewsHolder
{
    private ScrollArtifactLinkListSlot slot;
    public ScrollArtifactLinkListSlot Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<ScrollArtifactLinkListSlot>();
        base.CollectViews();
    }
}