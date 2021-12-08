using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class UIBlockUserScrollAdapter : OSA<BaseParamsWithPrefab, BlockUserHolder>
{
    // 스크롤 데이터 리스트
    public SimpleDataHelper<ScrollBlockUserData> Data { get; private set; }

    private Action<BlockCharacterData> onClickUnblock;
    private Action<BlockCharacterData> onClickSlot;

    private ScrollBlockUserData selectedData;

    /// <summary>
    /// 홀더를 생성 (Content 사이즈에 맞춰서 최대 수량을 계산해서 생성함)
    /// Init(오브젝트 본체, 스크롤뷰에 담을(Content)위치, itemIndex(오브젝트 인덱스)).
    /// </summary>
    /// <param name="itemIndex">생성하려는 Holder Index</param>
    protected override BlockUserHolder CreateViewsHolder(int itemIndex)
    {
        var instance = new BlockUserHolder();

        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    /// <summary>리스트 홀더를 갱신해준다.(스크롤이 Active 또는 Cell Group이 이동될 때(Drag))</summary>
    /// <param name="_holder">홀더 오브젝트</param>
    protected override void UpdateViewsHolder(BlockUserHolder _holder)
    {
        ScrollBlockUserData data = Data[_holder.ItemIndex];
        _holder.SetSlotItem(data,SetFocusItem, onClickUnblock);
    }

    /// <summary>스크롤 데이터 리스트에 아이템 추가 및 삭제 (자동으로 갱신)</summary>
    protected override void OnItemIndexChangedDueInsertOrRemove(BlockUserHolder shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
    {
        base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
        shiftedViewsHolder.SetSlotItem(Data[shiftedViewsHolder.ItemIndex],SetFocusItem, onClickUnblock);
    }

    private void Initialize()
    {
        if (Data == null)
            Data = new SimpleDataHelper<ScrollBlockUserData>(this);

        GameObject itemslot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIBlockUserListItem));

        Parameters.ItemPrefab = itemslot.GetComponent<RectTransform>();
        Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
        Parameters.ItemPrefab.transform.localScale = Vector2.one;
        Parameters.ItemPrefab.transform.localPosition =Vector3.zero;
        Parameters.ItemPrefab.gameObject.SetActive(false);

    }

    private void SetFocusItem(ScrollBlockUserData data)
    {
        if (selectedData != null)
        {
            if(selectedData.blockCharData.CharID == data.blockCharData.CharID)
            {
                selectedData.isSelected = !selectedData.isSelected;
                RefreshData();
                return;
            }
            selectedData.isSelected = false;
        }

        selectedData = data;

        selectedData.isSelected = true;
        RefreshData();
    }

    public void SetScrollData(Action<BlockCharacterData> _onClick)
    {
        Initialize();
        Init();

        onClickUnblock = _onClick;

        ResetData();
    }

    public void RefreshData()
    {
        for (int i = 0; i < VisibleItemsCount; i++)
            UpdateViewsHolder(base.GetItemViewsHolder(i));
    }

    public void ResetData()
    {
        selectedData = null;
        #region 사용자 변경 로직
        List<BlockCharacterData> list = Me.CurCharData.GetBlockCharacter();

        if (list == null)
            return;

        List<ScrollBlockUserData> scrollList = new List<ScrollBlockUserData>();

        for (int i = 0; i < list.Count; i++)
        {
            scrollList.Add(new ScrollBlockUserData() { blockCharData = list[i], isSelected = false });
        }

        Data.ResetItems(scrollList);

        #endregion
    }
}

///<summary>Scroll Item Define (Scroll 전용 사용자 정의 자료구조 선언)</summary>
public class ScrollBlockUserData
{
    public BlockCharacterData blockCharData;
    public bool isSelected = false;

    public void Reset(ScrollBlockUserData _data)
    {
        blockCharData = _data.blockCharData;
        isSelected = _data.isSelected;
    }
}