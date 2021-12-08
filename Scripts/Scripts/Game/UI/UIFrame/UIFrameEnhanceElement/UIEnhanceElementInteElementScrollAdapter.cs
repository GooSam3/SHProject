using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.DataHelpers;
using Com.TheFallenGames.OSA.CustomParams;
using System;
using static UIEnhanceElement;
using ZNet.Data;
using UnityEngine;
using UnityEngine.UI;
using GameDB;

public class UIEnhanceElementInteElementScrollAdapter : OSA<BaseParamsWithPrefab, EnhanceElementIntegratedElementHolder>
{
    private SimpleDataHelper<ScrollEnhanceElementIntegratedData> Data;
    private UIEnhanceElementTitleValuePair txtPairSourceObj;
    bool enableHighlight;
    E_UnitAttributeType type;
    Color activatedTitleColor;
    Color activatedValueColor;
    Color inactivatedColor;
    Color elementColor;

    bool init;

    bool willSnap;
    int frameRemainedTilSnap;
    int snapTargetItemIndex;
    Action onSnapFinished;

    protected override void Update()
    {
        base.Update();

        if (frameRemainedTilSnap > 0)
            frameRemainedTilSnap--;

        if (willSnap && frameRemainedTilSnap == 0)
        {
            willSnap = false;
            if ((Data == null || Data.List == null || snapTargetItemIndex >= Data.List.Count) == false)
            {
                ScrollTo(snapTargetItemIndex);
                onSnapFinished?.Invoke();
            }
        }
    }

    #region Public Methods
    public void Initialize(
        bool useHighlight
        , Color titleActiveColor
        , Color valueActiveColor
        , Color inactiveColor
        , UIEnhanceElementTitleValuePair pairSourceObj)
    {
        if (init)
            return;

        Data = new SimpleDataHelper<ScrollEnhanceElementIntegratedData>(this);
        txtPairSourceObj = pairSourceObj;
        this.enableHighlight = useHighlight;
        activatedTitleColor = titleActiveColor;
        activatedValueColor = valueActiveColor;
        inactivatedColor = inactiveColor;
        init = true;

        Init();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshData(E_UnitAttributeType type, Color elementColor, List<ScrollEnhanceElementIntegratedData> list)
    {
        this.type = type;
        this.elementColor = elementColor;
        Data.ResetItems(list);
    }

    public void NotifyDataChanged()
    {
        Data.NotifyListChangedExternally();
    }

    /// <summary>
    ///  지금 이 함수 정상작동하지않음 . OSA 의 ScrollTo 함수에 문제가 있어보임 . 
    ///  업데이트가 먼저되고 이런 이슈는 아님 . 10 프레임 뒤에 실행해도 결과는 간헐적으로 계속 이상하게 나옴 . 
    ///  무슨 이슈인지 당장 알수없음 . 
    ///  특이사항으로는 OSA 가 각각 그룹의 크기를 어떻게 알고 아이템 인덱스만 가지고 그걸 맞춰줄까 하는 생각임 . 
    ///  근데 또 부분적으로 되기는함 ㅡ.
    /// </summary>
    public bool SnapToMyNextLevel(int frameJumpCount, Action onFinished)
    {
        if (Data == null || Data.List == null | Data.List.Count == 0)
            return false;

        uint myCurLevel = Me.CurCharData.GetAttributeLevelByType(this.type);
        var snapDesiredData = DBAttribute.GetAttributeByLevel(this.type, myCurLevel + 1);
        uint targetLevel = myCurLevel + 1;

        if (snapDesiredData == null)
            targetLevel = myCurLevel;

        int itemIndex = 0;

        for (int i = 0; i < Data.List.Count; i++)
        {
            /// 애로 snap 해야함 
            if (Data.List[i].level == targetLevel)
            {
                itemIndex = i;
                break;
            }
        }

        if (frameJumpCount > 0)
        {
            willSnap = true;
            frameRemainedTilSnap = frameJumpCount;
            snapTargetItemIndex = itemIndex;
            onSnapFinished = onFinished;

        }
        else
        {
            ScrollTo(itemIndex);
            onFinished?.Invoke();
        }

        return true;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OSA Overrides
    protected override EnhanceElementIntegratedElementHolder CreateViewsHolder(int itemIndex)
    {
        var holder = new EnhanceElementIntegratedElementHolder();
        holder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
        return holder;
    }

    protected override void UpdateViewsHolder(EnhanceElementIntegratedElementHolder newOrRecycled)
    {
        var t = Data[newOrRecycled.ItemIndex];

        newOrRecycled.Slot.gameObject.name = Data[newOrRecycled.ItemIndex].level.ToString();
        uint curMyLevel = Me.CurCharData.GetAttributeLevelByType(this.type);
        bool isObtained = Me.CurCharData.IsThisAttributeObtained_ByID(t.attributeID);
        bool isHighlight = false;
        bool isMyNextLevel = false;
        Color highlightColor = Color.black;

        if (enableHighlight)
        {
            if (DBAttribute.GetAttributeByID(t.attributeID, out var attributeData))
            {
                if (curMyLevel + 1 == attributeData.AttributeLevel)
                {
                    isMyNextLevel = true;
                }
            }

            if (isMyNextLevel)
            {
                isHighlight = true;
                Color col = elementColor;
                col.a = 0.3f;
                highlightColor = col;
            }
        }

        /// 보유중 또는 하이라이트(다음 레벨) 이면 active color 아니면 ianctive . 
        Color levelColor = isObtained || isHighlight ? this.activatedTitleColor : this.inactivatedColor;
        Color titleColor = isObtained || isHighlight ? this.activatedTitleColor : this.inactivatedColor;
        Color valueColor = isObtained || isHighlight ? this.activatedValueColor : this.inactivatedColor;

        newOrRecycled.Slot.SetUI(t.level, t.txtData, txtPairSourceObj, isHighlight, highlightColor, levelColor, titleColor, valueColor);

        ScheduleComputeVisibilityTwinPass(true);
    }
    #endregion
}

public class EnhanceElementIntegratedElementHolder : BaseItemViewsHolder
{
    private UIEnhanceElementInteElementScrollGroup slot;
    public UIEnhanceElementInteElementScrollGroup Slot { get { return slot; } }

    public override void CollectViews()
    {
        slot = root.GetComponent<UIEnhanceElementInteElementScrollGroup>();
        base.CollectViews();
    }
}

public class ScrollEnhanceElementIntegratedData
{
    public uint attributeID;
    public uint level;
    public List<AbilityActionTitleValuePair> txtData;
}