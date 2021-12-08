using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UIFrameBuffList;

public class UIBuffListScrollerAdapter : OSA<BaseParamsWithPrefab, UIBuffInfoListItem>
{
    public SimpleDataHelper<C_CustomAbilityAction> Data
    {
        get; private set;
    }
    protected override void Start()
    {
        //자동 이니셜라이즈 해제용으로 오버라이드만함
    }

    private Action<C_CustomAbilityAction> onClick = null;

    private uint selectedID = 0;

    protected override UIBuffInfoListItem CreateViewsHolder(int itemIndex)
    {
        var instance = new UIBuffInfoListItem();

        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    protected override void UpdateViewsHolder(UIBuffInfoListItem newOrRecycled)
    {
        C_CustomAbilityAction model = Data[newOrRecycled.ItemIndex];
        newOrRecycled.SetSlot(model, onClick, selectedID);
    }

    /// <summary>스크롤 데이터 리스트에 아이템 추가 및 삭제 (자동으로 갱신)</summary>
    protected override void OnItemIndexChangedDueInsertOrRemove(UIBuffInfoListItem shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
    {
        base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
        shiftedViewsHolder.SetSlot(Data[shiftedViewsHolder.ItemIndex], onClick, selectedID);
    }


    public void Initialize(Action<C_CustomAbilityAction> _onClickSlot)
    {
        Data = new SimpleDataHelper<C_CustomAbilityAction>(this);

        onClick = _onClickSlot;

        GameObject obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIBuffInfoListItem));
        Parameters.ItemPrefab = obj.GetComponent<RectTransform>();

        Parameters.ItemPrefab.SetParent(transform);
        Parameters.ItemPrefab.transform.localScale = Vector2.one;
        Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
        Parameters.ItemPrefab.gameObject.SetActive(false);

        Init();
    }

    public void RefreshListData(List<C_CustomAbilityAction> listData)
    {
        if (IsInitialized)
        {
            Data.ResetItems(listData);
        }
        else
            Initialized += () => RefreshListData(listData);
    }

    public void UpdateSelectedID(uint id) => selectedID = id;
}
