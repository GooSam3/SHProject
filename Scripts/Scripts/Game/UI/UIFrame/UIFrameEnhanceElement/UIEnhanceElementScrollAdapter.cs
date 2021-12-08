using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

[Serializable]
public class EnhanceElementScrollGridParamEx : GridParams
{
    public void SetDragEnabled(bool enabled)
    {
        base.DragEnabled = enabled;
    }
}

public class UIEnhanceElementScrollAdapter : GridAdapter<EnhanceElementScrollGridParamEx, UIScrollEnhanceElementSlot>
{
    // 스크롤 데이터 리스트
    public SimpleDataHelper<ScrollEnhanceElementData> Data { get; private set; }
    private bool initCheck = false;

    Action<ScrollEnhanceElementData, UIEnhanceElementSlot> onClick;
    UIEnhanceElementSettingProvider SettingProvider;

    public event Action<double> onScroll;

    /// <summary>리스트 홀더를 갱신해준다.(스크롤이 Active 또는 Cell Group이 이동될 때(Drag))</summary>
    /// <param name="_holder">홀더 오브젝트</param>
    protected override void UpdateCellViewsHolder(UIScrollEnhanceElementSlot _holder)
    {
        if (_holder == null)
            return;

        _holder.UpdateSlot(SettingProvider, Data[_holder.ItemIndex], onClick);
    }

    #region Public Methods
    public bool GetWorldPos(uint tid, out Vector2 pos)
    {
        pos = Vector2.zero;

        for (int i = 0; i < CellsCount; i++)
        {
            var cell = GetCellViewsHolder(i);
            if (cell != null)
            {
                if (cell.Target.tid == tid)
                {
                    pos = new Vector2(cell.Target.transform.position.x, cell.Target.transform.position.y);
                    return true;
                }
            }
        }

        return false;
    }

    public void SetDragEnable(bool enable)
    {
        Parameters.SetDragEnabled(enable);
    }

    public void ScrollToSpecificAttribute(uint attributeID, float offsetFromLeft, bool smooth = false)
    {
        for (int i = 0; i < this.Data.List.Count; i++)
        {
            if (Data.List[i].TableData.AttributeID == attributeID)
            {
                if (smooth)
                {
                    SmoothScrollTo(i, 0.3f, offsetFromLeft);
                }
                else
                {
                    ScrollTo(i, offsetFromLeft);

                }
                return;
            }
        }
    }

    public void Initialize(UIEnhanceElementSettingProvider settingProvider, Action<ScrollEnhanceElementData, UIEnhanceElementSlot> _onClickSlot)
    {
        if (Data == null)
            Data = new SimpleDataHelper<ScrollEnhanceElementData>(this);

        if (_onClickSlot != null)
            onClick = _onClickSlot;

        // 스크롤 prefab 연결 및 기본 세팅 
        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIEnhanceElementSlot));
        Parameters.Grid.CellPrefab = slot.GetComponent<RectTransform>();
        Parameters.Grid.CellPrefab.SetParent(transform);
        Parameters.Grid.CellPrefab.localScale = Vector2.one;
        Parameters.Grid.CellPrefab.localPosition = Vector3.zero;
        Parameters.Grid.CellPrefab.gameObject.SetActive(false);

        SettingProvider = settingProvider;

        if (!initCheck)
        {
            gameObject.SetActive(true);
            Init();
            RefreshAll();
            Data.NotifyListChangedExternally();
            initCheck = true;
        }
    }

    // 홀더 데이터 리스트 전체 갱신
    public void RefreshAll()
    {
        bool isReset = false;
        List<ScrollEnhanceElementData> list = null;

        if (Data.List.Count == 0)
        {
            isReset = true;
            list = new List<ScrollEnhanceElementData>();
        }
        else
        {
            isReset = false;
        }

        // OSA 스크롤 horizontal 의 데이터 순서와 맞게끔 데이터 세팅을 한다 . 
        DBAttribute.ForeachAllTypes_ByEachLevel(
            (type, data) =>
        {
            bool isObtainedOrNextAvailableLevel = false;
            bool isObtainedLevel = false;
            bool colorActive = false;

            if (data != null)
            {
                isObtainedLevel = Me.CurCharData.IsThisAttributeObtained_ByID(data.AttributeID);
                isObtainedOrNextAvailableLevel = Me.CurCharData.IsThisAttributeObtainedOrNextAvailableLevel_ByID(data.AttributeID);
            }

            colorActive = isObtainedLevel || isObtainedOrNextAvailableLevel;

            var targetData = isReset ? new ScrollEnhanceElementData() : Data.List.Find(t => t.TableData.AttributeID == data.AttributeID);

            targetData.Set(data, isObtainedLevel, isObtainedOrNextAvailableLevel,
                colorActive ? SettingProvider.GetColorActive_BG(type) : SettingProvider.GetColorInactive_BG()
                , colorActive ? SettingProvider.GetColorActive_IconLineAndEnhance(type) : SettingProvider.GetColorInactive_IconLineAndEnhance());

            if (isReset)
            {
                list.Add(targetData);
            }
        });

        if (isReset)
        {
            ResetData(list);
        }
        else
        {
            Data.NotifyListChangedExternally();
        }
    }

    public void ResetData(List<ScrollEnhanceElementData> dataList)
    {
        Data.ResetItems(dataList);
    }

    public void ResetVisibleSlots()
    {
        for (int i = 0; i < base.CellsCount; i++)
        {
            var holder = base.GetCellViewsHolderIfVisible(i);
            if (holder != null)
            {
                UpdateCellViewsHolder(holder);
            }
        }
    }

    // 첫번째 슬롯의 x 값 월드 위치
    public float GetContentLeftXPos()
    {
        if (Content == null)
            return 0;
        Vector3[] v = new Vector3[4];
        Content.GetWorldCorners(v);
        return v[0].x;
    }

    public float GetSlotGroupGapDistance()
    {
        return _Params.DefaultItemSize + _Params.ContentSpacing;
    }

    public float GetItemSize()
    {
        return _Params.DefaultItemSize;
    }

    // 얼마나 스크롤됐는지 x 값 오프셋 구함 
    public double GetCurrentScrollOffset(double normPos)
    {
        return (GetContentSize() - Viewport.rect.width) * normPos;
    }

    public void SetActiveElementEdge(uint tid, bool active)
	{
        if (Data == null || Data.List == null || Data.List.Count == 0)
            return;

		int itemCnt = GetItemsCount();
		for (int i = 0; i < itemCnt; i++)
		{
			var holder = GetCellViewsHolder(i);
			if (Data.List[i].TableData.AttributeID == tid)
			{
                if(holder != null)
				{
                    holder.Target.SetSelectedActive(active);
				}

                break;
			}
		}
	}
	#endregion

	#region Test
	protected override void OnScrollPositionChanged(double normPos)
    {
        base.OnScrollPositionChanged(normPos);
        onScroll?.Invoke(normPos);
    }
    #endregion
}

public class ScrollEnhanceElementData
{
    public Attribute_Table TableData { get; private set; }
    public bool IsObtainedLevel;
    public bool IsObtainedOrNextAvailableLevel;
    public Color BgColor;
    public Color LineAndEnhanceColor;

    public void Set(Attribute_Table data, bool isObtainedLevel, bool isObtainedOrNextAvailableLevel, Color bgColor, Color lineAndEnhanceCol)
    {
        TableData = data;
        IsObtainedLevel = isObtainedLevel;
        IsObtainedOrNextAvailableLevel = isObtainedOrNextAvailableLevel;
        BgColor = bgColor;
        LineAndEnhanceColor = lineAndEnhanceCol;
    }

    public virtual void Reset(ScrollEnhanceElementData data)
    {
    }

}