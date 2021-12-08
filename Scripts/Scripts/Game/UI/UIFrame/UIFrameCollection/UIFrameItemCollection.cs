using DG.Tweening;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemCollection : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private UIItemCollectionScrollAdapter ScrollAdapter;
    [SerializeField] public UIPopupItemInfo ItemInfo;

    [SerializeField] private Text CollectionAbilityNameText;
    [SerializeField] private Text CollectionAbilityValueText;

    [SerializeField] private Text CollectionPageText;

    [SerializeField] private ZButton CollectionButton = null;
    //collection info
    [SerializeField] private Text ItemCurrntCountText;
    [SerializeField] private Text ItemMaxCountText;
    [SerializeField] private Text ItemRateText;
    [SerializeField] private Slider ItemRateImage;
    [SerializeField] private Text CollectionCurrntCountText;
    [SerializeField] private Text CollectionMaxCountText;
    [SerializeField] private Text CollectionRateText;
    [SerializeField] private Slider CollectionRateImage;

    //toggle
    [SerializeField] private ZToggle FirstTab;
    [SerializeField] private ZToggle FirstTierTab;
    [SerializeField] private ZButton FilterButton;

    [SerializeField] private GameObject ObjFilterOn;


    //search
    [SerializeField] private InputField SearchInput;
    #endregion

    #region System Variable
    public uint CollectionTid { get; private set; } = 0;
	public uint SelectCollectionSlotIdx { get; private set; } = 0;
    public ZItem Material { get; private set; } = null;

    private CollectState ItemCollectionSortType = CollectState.STATE_NONE;
    private E_TapType ItemCollectionTierSortType = E_TapType.All;

    private uint TotalItemCount = 0;
    private uint CurrentItemCount = 0;
    private uint TotalCollectionCount = 0;
    private uint CurrentCollectionCount = 0;

    private int CollectionPageCount = 1;

    private E_AbilityType? FilterAilityType = null;
    public override bool IsBackable => true;
    private bool IsInit = false;
    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        ItemCollectionSortType = CollectState.STATE_NONE;
        ItemCollectionTierSortType = E_TapType.All;

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemCollectionViewsHolder), delegate
        {
            ScrollAdapter.Initialize();
            FirstTab.SelectToggleAction(delegate {
                OnClickItemCollectionSortType((int)CollectState.STATE_NONE);
            });

            FirstTierTab.SelectToggleAction(delegate {
                OnClickItemCollectionTierSortType((int)E_TapType.All);
            });

            UpdateTab();
            UpdateCurrentApply();

            IsInit = true;
        });
    }

    protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        if (!IsInit)
            return;
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);

        FirstTab.SelectToggleAction(delegate {
            OnClickItemCollectionSortType((int)CollectState.STATE_NONE);
        });

        FirstTierTab.SelectToggleAction(delegate {
            OnClickItemCollectionTierSortType((int)E_TapType.All);
        });
    }

    protected override void OnHide()
    {
        base.OnHide();

        SearchInput.text = string.Empty;
        FilterAilityType = null;

        CloseItemInfoPopup();

        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame();
    }

    public void UpdateTab()
    {
        CloseItemInfoPopup();

        IList<ItemCollection_Table> collectionList = DBItemCollect.GetItemCollections();
        List<ItemCollection_Table> dataCollectStateList = new List<ItemCollection_Table>();
        dataCollectStateList.Clear();
        for (int i = 0; i < collectionList.Count; i++)
        {
            switch (ItemCollectionSortType)
            {
                case CollectState.STATE_NONE:
                    {
                        dataCollectStateList.Add(collectionList[i]);
                    }
                    break;

                case CollectState.STATE_PROGRESS:
                    {
                        if (Me.CurCharData.GetCollectData(collectionList[i].ItemCollectionID, CollectionType.TYPE_ITEM) == null ||
                            Me.CurCharData.GetCollectData(collectionList[i].ItemCollectionID, CollectionType.TYPE_ITEM).curState == CollectState.STATE_PROGRESS)
                        {
                            dataCollectStateList.Add(collectionList[i]);
                        }
                    }
                    break;

                case CollectState.STATE_COMPLETE:
                    {
                        if (Me.CurCharData.GetCollectData(collectionList[i].ItemCollectionID, CollectionType.TYPE_ITEM) != null &&
                            Me.CurCharData.GetCollectData(collectionList[i].ItemCollectionID, CollectionType.TYPE_ITEM).curState == CollectState.STATE_COMPLETE)
                        {
                            dataCollectStateList.Add(collectionList[i]);
                        }
                    }
                    break;
            }
        }

        if(null != ScrollAdapter.Data && 0 < ScrollAdapter.Data.Count)
            ScrollAdapter.ScrollTo(0);

        dataCollectStateList.RemoveAll((item) =>
        {
            //타입 필터
            if(ItemCollectionTierSortType != E_TapType.All)
            {
                if (item.TapType != ItemCollectionTierSortType)
                    return true;
            }

            if (true == FilterAilityType.HasValue)
            {
                if (false == DBItemCollect.ContainsAbility(item.ItemCollectionID, FilterAilityType.Value))
                    return true;
            }

            if(false == string.IsNullOrEmpty(SearchInput.text))
            {
                if (false == IsSearchCollection(SearchInput.text, item))
                    return true;
            }

            return false;
        });

        CollectionPageCount = 1;

        ScrollAdapter.SetScrollData(dataCollectStateList);
        UpdatePageCountText();

        ObjFilterOn.SetActive(FilterAilityType.HasValue);
    }

    private bool IsSearchCollection(string value, ItemCollection_Table data)
    {
        string[] strSearchDatas = value.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);

        List<string> findstrs = new List<string>();
        CollectData collectData = null;
        List<E_AbilityType> abilTypes = new List<E_AbilityType>();
        abilTypes.Clear();
        findstrs.Clear();
        foreach (E_AbilityType abilType in DBItemCollect.GetAllAbility(data.ItemCollectionID))
        {
            if (!DBAbility.IsParseAbility(abilType))
                continue;

            if (!abilTypes.Contains(abilType))
            {
                abilTypes.Add(abilType);
                findstrs.Add(DBLocale.GetText(DBAbility.GetAbilityName(abilType)));
            }
        }

        collectData = Me.CurCharData.GetCollectData(data.ItemCollectionID, CollectionType.TYPE_ITEM);
        foreach (uint itemtid in DBItemCollect.GetAllCollectItems(data.ItemCollectionID, collectData != null ? collectData.MaterialTids.ToArray() : null))
        {
            if (!findstrs.Contains(DBItem.GetItemName(itemtid)))
                findstrs.Add(DBItem.GetItemName(itemtid));
        }

        string dataName = DBLocale.GetText(data.ItemCollectionTextID);
        for (int i = 0; i < strSearchDatas.Length; i++)
        {
            if ((dataName.Contains(strSearchDatas[i]) || findstrs.Find(item => item.Contains(strSearchDatas[i])) != null))
            {
                return true;
            }
        }

        return false;
    }

    public void OnClickItemCollectionSortType(int _changedStateType)
    {
        if (ItemCollectionSortType == (CollectState)_changedStateType)
            return;

        ItemCollectionSortType = (CollectState)_changedStateType;

        CollectionPageCount = 1;
        UpdateTab();
    }

    public void OnClickItemCollectionTierSortType(int _changedTabType)
    {
        if (ItemCollectionTierSortType == (E_TapType)_changedTabType)
            return;

        ItemCollectionTierSortType = (E_TapType)_changedTabType;
        
        CollectionPageCount = 1;
        UpdateTab();
    }

    public void OpenItemInfoPopup(uint _itemTid, uint _slotIdx, uint _collectTid, bool _check)
    {
        CollectionButton.interactable = false;
        for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
        {
            if (_itemTid == Me.CurCharData.InvenList[i].item_tid && _check)
                CollectionButton.interactable = true;
        }

        SelectCollectionSlotIdx = _slotIdx;
        CollectionTid = _collectTid;
        Material = Me.CurCharData.GetItem(_itemTid, NetItemType.TYPE_EQUIP);

        ItemInfo.gameObject.SetActive(true);
        ItemInfo.Initialize(E_ItemPopupType.Collection, _itemTid);
    }

    public void CloseItemInfoPopup()
    {
        if (ItemInfo != null && ItemInfo.gameObject.activeSelf)
        {
            ItemInfo.gameObject.SetActive(false);
        }
    }

    private int PageCountCalculation(int _cnt)
    {
        if (_cnt % 30 == 0)
            return _cnt / 30;
        else
            return (_cnt / 30) + 1;
    }

    public void OnClickCollectionPageCount(int _cnt)
    {
        if (CollectionPageCount + _cnt < 1 || CollectionPageCount + _cnt > PageCountCalculation(ScrollAdapter.HoleDataList.Count))
            return;

        CollectionPageCount += _cnt;
        ScrollAdapter.ScrollTo(0);
        ScrollAdapter.DataPageControll(CollectionPageCount);

        UpdatePageCountText();
    }

    public void UpdateCurrentApply()
    {
        UpdatePageCountText();
        CollectionButton.interactable = false;

        CollectionAbilityNameText.text = string.Empty;
        CollectionAbilityValueText.text = string.Empty;

        string applyStr = "";
        Dictionary<E_AbilityType, System.ValueTuple<float, float>> applyDic = new Dictionary<E_AbilityType, System.ValueTuple<float, float>>();

        if (Me.CurCharData.GetCompleteCollectItems(CollectionType.TYPE_ITEM) == null)
        {
            ItemMaxCountText.text = $"/{TotalItemCount}";
            ItemCurrntCountText.text = $"{(int)0}";
            ItemRateText.text = "0";
            ItemRateImage.value = 0;
            CollectionMaxCountText.text = $"/{TotalCollectionCount}";
            CollectionCurrntCountText.text = $"{(int)0}";
            CollectionRateText.text = "0";
            CollectionRateImage.value = 0;
            return;
        }

        foreach (CollectData collectdata in Me.CurCharData.GetCompleteCollectItems(CollectionType.TYPE_ITEM))
        {
            if (DBItemCollect.GetCollection(collectdata.CollectTid, out var tableData))
            {
                if (tableData.AbilityActionID_01 != 0)
                {
                    var actionData = DBAbility.GetAction(tableData.AbilityActionID_01);
                    if (actionData.AbilityViewType == E_AbilityViewType.ToolTip)
                    {
                        if (string.IsNullOrEmpty(applyStr))
                            applyStr = DBLocale.GetText(actionData.ToolTip);
                        else
                            applyStr = string.Format("{0}\n{1}", applyStr, DBLocale.GetText(actionData.ToolTip));
                    }
                    else
                    {
                        var enumer = DBAbility.GetAllAbilityData(tableData.AbilityActionID_01).GetEnumerator();
                        while (enumer.MoveNext())
                        {
                            if (applyDic.ContainsKey(enumer.Current.Key))
                                applyDic[enumer.Current.Key] = (applyDic[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, applyDic[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
                            else
                                applyDic.Add(enumer.Current.Key, enumer.Current.Value);
                        }
                    }
                }
                if (tableData.AbilityActionID_02 != 0)
                {
                    var actionData = DBAbility.GetAction(tableData.AbilityActionID_02);
                    if (actionData.AbilityViewType == E_AbilityViewType.ToolTip)
                    {
                        if (string.IsNullOrEmpty(applyStr))
                            applyStr = DBLocale.GetText(actionData.ToolTip);
                        else
                            applyStr = string.Format("{0}\n{1}", applyStr, DBLocale.GetText(actionData.ToolTip));
                    }
                    else
                    {
                        var enumer = DBAbility.GetAllAbilityData(tableData.AbilityActionID_02).GetEnumerator();
                        while (enumer.MoveNext())
                        {
                            if (applyDic.ContainsKey(enumer.Current.Key))
                                applyDic[enumer.Current.Key] = (applyDic[enumer.Current.Key].Item1 + enumer.Current.Value.Item1, applyDic[enumer.Current.Key].Item2 + enumer.Current.Value.Item2);
                            else
                                applyDic.Add(enumer.Current.Key, enumer.Current.Value);
                        }
                    }
                }
            }
        }

        string valueText = "";
        foreach (E_AbilityType type in applyDic.Keys)
        {
            if (!DBAbility.IsParseAbility(type))
                continue;

            string value = DBAbility.ParseAbilityValue(type, applyDic[type].Item1, applyDic[type].Item2, " ");

            if (string.IsNullOrEmpty(applyStr))
                applyStr = string.Format("{0}", DBLocale.GetText(DBAbility.GetAbilityName(type)));
            else
                applyStr = string.Format("{0}\n{1}", applyStr, DBLocale.GetText(DBAbility.GetAbilityName(type)));
            
            if (string.IsNullOrEmpty(valueText))
                valueText = value;
            else
                valueText = string.Format("{0}\n{1}", valueText, value);
        }

        CollectionAbilityNameText.text = applyStr;
        CollectionAbilityValueText.text = valueText;


        TotalItemCount = 0;
        CurrentItemCount = 0;
        TotalCollectionCount = 0;
        CurrentCollectionCount = 0;

        foreach (ItemCollection_Table tableData in DBItemCollect.GetItemCollections())
        {
            TotalItemCount += (uint)tableData.CollectionItemCount;
            ++TotalCollectionCount;

            CollectData collectData = Me.CurCharData.GetCollectData(tableData.ItemCollectionID, CollectionType.TYPE_ITEM);
            if (collectData != null)
            {
                if (collectData.curState == CollectState.STATE_COMPLETE)
                {
                    CurrentItemCount += (uint)tableData.CollectionItemCount;
                    ++CurrentCollectionCount;
                }
                else
                {
                    for (int i = 0; i < collectData.MaterialTids.Count; i++)
                        if (collectData.MaterialTids[i] != 0)
                            ++CurrentItemCount;
                }
            }
        }

        DOTween.To(() => 0, value => UpdateCollectionTextInfo(value), 1f, 1f).SetEase(Ease.InQuad).onComplete += () =>
        {
            UpdateCollectionTextInfo(1f);
        };
    }

    private void UpdateCollectionTextInfo(float value)
    {
        float currentValue = (int)(CurrentItemCount * value);

        ItemMaxCountText.text = $"/{TotalItemCount}";
        ItemCurrntCountText.text = $"{(int)currentValue}";
        ItemRateText.text = $"{((currentValue / TotalItemCount) * 100f).ToString("F2")}";
        ItemRateImage.value = currentValue / TotalItemCount;

        currentValue = (int)(CurrentCollectionCount * value);

        CollectionMaxCountText.text = $"/{TotalCollectionCount}";
        CollectionCurrntCountText.text = $"{(int)currentValue}";
        CollectionRateText.text = $"{((currentValue / TotalCollectionCount) * 100f).ToString("F2")}";
        CollectionRateImage.value = currentValue / TotalCollectionCount;
    }

    private void UpdatePageCountText()
    {
        int maxPageCount = PageCountCalculation(ScrollAdapter.HoleDataList.Count);
        CollectionPageCount = Math.Min(CollectionPageCount, maxPageCount);        

        CollectionPageText.text = string.Format("{0}/{1}", CollectionPageCount, maxPageCount);
    }

    // 필터
    public void OnClickFilter()
    {
        UIManager.Instance.Open<UIPopupCollectionFilterSelect>((str, popup) =>
        {
            popup.SetListener(OnSetFilter);
        });
    }

    public void SearchAction()
    {
        UpdateTab();
    }

    private void OnSetFilter(E_AbilityType _type)
    {
        FilterAilityType = _type;
        UpdateTab();
    }

    public void OnClickResetFilter()
    {
        SearchInput.text = string.Empty;
        FilterAilityType = null;
        UpdateTab();
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameItemCollection>(true);
    }
}
