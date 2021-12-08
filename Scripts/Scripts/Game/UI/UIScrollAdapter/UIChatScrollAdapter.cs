using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ZDefine;
using ZNet.Data;

public class UIChatScrollAdapter : OSA<BaseParamsWithPrefab, ChatDataHolder>
{
    public LazyDataHelper<ScrollChatData> Data { get; private set; }

    private Action<ScrollChatData> onClick;

    private bool isHUDMode = false;

    private bool isAutoFocus = true;
    private Sequence intervalSeq;

    private List<ChatData> listData;

    #region OSA implementation

    protected override void Start() { }

    /// <inheritdoc/>
    protected override ChatDataHolder CreateViewsHolder(int itemIndex)
    {
        var instance = new ChatDataHolder();
        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    public void RefreshData()
    {
        for (int i = 0; i < VisibleItemsCount; i++)
            UpdateViewsHolder(base.GetItemViewsHolder(i));
    }

    /// <inheritdoc/>
    protected override void OnItemHeightChangedPreTwinPass(ChatDataHolder vh)
    {
        base.OnItemHeightChangedPreTwinPass(vh);
        var m = Data.GetOrCreate(vh.ItemIndex);
        m.HasPendingSizeChange = false;
    }

    /// <inheritdoc/>
    protected override void UpdateViewsHolder(ChatDataHolder newOrRecycled)
    {
        // Initialize the views from the associated model
        ScrollChatData model = Data.GetOrCreate(newOrRecycled.ItemIndex);
        newOrRecycled.SetSlotItem(model, onClick);

        if (model.HasPendingSizeChange)
        {
            // Height will be available before the next 'twin' pass, inside OnItemHeightChangedPreTwinPass() callback (see above)
            ScheduleComputeVisibilityTwinPass(true);
        }
    }

    /// <summary>
    /// <para>Because the index is shown in the title, this may lead to a content size change, so mark the ViewsHolder to have its size recalculated</para>
    /// <para>For more info, see <see cref="OSA{TParams, TItemViewsHolder}.OnItemIndexChangedDueInsertOrRemove(TItemViewsHolder, int, bool, int)"/> </para>
    /// </summary>
    protected override void OnItemIndexChangedDueInsertOrRemove(
        ChatDataHolder shiftedViewsHolder,
        int oldIndex,
        bool wasInsert,
        int removeOrInsertIndex)
    {
        base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);

        shiftedViewsHolder.SetSlotItem(Data.GetOrCreate(shiftedViewsHolder.ItemIndex), onClick);
        ScheduleComputeVisibilityTwinPass(true);
    }

    /// <inheritdoc/>
    protected override void RebuildLayoutDueToScrollViewSizeChange()
    {
        // Invalidate the last sizes so that they'll be re-calculated
        foreach (var model in Data.GetEnumerableForExistingItems())
            model.HasPendingSizeChange = true;

        base.RebuildLayoutDueToScrollViewSizeChange();
    }
    #endregion

    public void InitScrollData(Action<ScrollChatData> _onClick, bool _isHUDMode)
    {
        if (Data == null)
            Data = new LazyDataHelper<ScrollChatData>(this, CreateModel);

        isHUDMode = _isHUDMode;

        GameObject itemslot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIChattingListItem));

        Parameters.ItemPrefab = itemslot.GetComponent<RectTransform>();
        Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
        Parameters.ItemPrefab.transform.localScale = Vector2.one;
        Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
        Parameters.ItemPrefab.gameObject.SetActive(false);

        Init();

        onClick = _onClick;

        ResetData(Me.CurCharData.chatFilter);
    }

    // 포커스 수동설정
    public void SetAutoFocus(bool state) => isAutoFocus = state;

    public void AddChatData(ChatData _chatData)
    {
        listData.Add(_chatData);
        Data.InsertItemsAtEnd(1);

        //refresh
        //GetItemViewsHolderIfVisible()


        if (isAutoFocus)
        {
            if (VisibleItemsCount >= listData.Count)
            {
                SetNormalizedPosition(0);
                return;
            }

            SmoothScrollTo(listData.Count - 1, .2f);
        }
    }

    protected override void PostRebuildLayoutDueToScrollViewSizeChange()
    {
        SetNormalizedPosition(0);
    }

    public void OnHide()
    {
        if (intervalSeq != null && intervalSeq.active)
            intervalSeq.Kill(false);
    }

    public void OnStartDrag(BaseEventData eventData)
    {
        if (intervalSeq != null && intervalSeq.active)
            intervalSeq.Kill(false);

        isAutoFocus = false;
    }

    public void OnEndDrag(BaseEventData eventData)
    {
        if (intervalSeq != null && intervalSeq.active)
            intervalSeq.Kill(false);

        intervalSeq = DOTween.Sequence().AppendInterval(DBConfig.Chatting_Auto_SetEnd_Interval).
                                         AppendCallback(() =>
                                         {
                                             isAutoFocus = true;
                                             SmoothScrollTo(listData.Count - 1, .2f);
                                         });
    }

    // 탭이동, 껏다켜짐등 데이터의 동기화가 필요할때 쓰임
    public void ResetData(ChatFilter filter)
    {
        if (intervalSeq != null && intervalSeq.active)
            intervalSeq.Kill(false);

        isAutoFocus = true;

        if(isHUDMode)
        {
            var list = ZWebChatData.GetChatList(filter);
            list.RemoveAll(item => item.type == ChatViewType.TYPE_SYSTEM_GUILD_GREETING);
            listData = list;
        }
        else
            listData = ZWebChatData.GetChatList(filter);

        Data.ResetItems(listData.Count);

        SetNormalizedPosition(1);

        base.StopMovement();

        RefreshData();
    }

    public ScrollChatData CreateModel(int itemIdex)
    {
        return new ScrollChatData(listData[itemIdex], isHUDMode);
    }
}

public class ScrollChatData
{
    public ChatData chatData;

    public bool isHudMode = false;

    public bool HasPendingSizeChange { get; set; }

    public ScrollChatData(ChatData _data, bool _isHudMode)
    {
        chatData = _data;
        isHudMode = _isHudMode;
        HasPendingSizeChange = true;
    }

    public void Reset(ScrollChatData _data)
    {
        chatData = _data.chatData;
        isHudMode = _data.isHudMode;
        HasPendingSizeChange = true;
    }
}
