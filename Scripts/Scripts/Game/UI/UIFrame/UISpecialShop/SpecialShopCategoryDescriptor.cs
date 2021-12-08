using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
using System.Linq;
using static DBSpecialShop;
using Devcat;
using System;
using WebNet;

public class SpecialShopCategoryDescriptor
{
    #region SerializedField
    #region Preference Variable
    #endregion
    #endregion

    #region System Variables
    private UIFrameSpecialShop FrameSpeicalShop;
    private CategoryDescriptorData Descriptor;

    private Action<E_SpecialShopType> onMainCtgChanged;
    private Action<E_SpecialSubTapType> onSubCtgChanged;
    #endregion

    #region Properties 
    public EnumDictionary<E_SpecialShopType, MainCategoryInfo> MainCategories { get { return Descriptor.MainCtgs; } }
    public int MaxDisplayDataCount
    {
        get
        {
            int cnt = 0;
            foreach (var mainCtg in MainCategories.Values)
            {
                foreach (var subCtg in mainCtg.subCtgInfoList)
                {
                    if (subCtg.displayData.Count > cnt)
                    {
                        cnt = subCtg.displayData.Count;
                    }
                }
            }
            return cnt;
        }
    }
    public int MaxSubCategoryCount
    {
        get
        {
            int cnt = 0;

            foreach (var mainCtg in MainCategories)
            {
                if (mainCtg.Value.subCtgInfoList.Count > cnt)
                {
                    cnt = mainCtg.Value.subCtgInfoList.Count;
                }
            }

            return cnt;
        }
    }
    public int SelectedSubCategoryCount
    {
        get
        {
            int cnt = 0;
            var mainCtg = SelectedMainCategoryInfo;
            if (mainCtg != null)
                cnt = mainCtg.subCtgInfoList.Count;
            return cnt;
        }
	}
	public E_SpecialShopType FirstMainTab { get { return Descriptor.MainCtgs.Count > 0 ? Descriptor.MainCtgs.First().Value.type: E_SpecialShopType.None; } }
	public MainCategoryInfo SelectedMainCategoryInfo { get { return Descriptor.MainCtgs.Count > 0 ? Descriptor.MainCtgs[Descriptor.SelectedMainCtgType] : null; } }
    public SubCategoryInfo SelectedSubCategoryInfo
    {
        get
        {
            var mainCtg = SelectedMainCategoryInfo;
            if (mainCtg == null)
                return null;

            if (mainCtg.subCtgInfoList.Count == 0)
                return null;

            return mainCtg.subCtgInfoList.Find(t => t.type == Descriptor.SelectedSubCtgType);
        }
    }
    public List<SingleDataInfo> SelectedCategoryShopItemList
    {
        get
        {
            var subCtgInfo = SelectedSubCategoryInfo;
            if (subCtgInfo == null)
                return new List<SingleDataInfo>();
            return subCtgInfo.displayData;
        }
    }
    public E_SpecialShopType SelectedMainCategoryType { get { return Descriptor.SelectedMainCtgType; } }
    public E_SpecialSubTapType SelectedSubCategoryType { get { return Descriptor.SelectedSubCtgType; } }
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void Initialize(UIFrameSpecialShop specialShop)
    {
        FrameSpeicalShop = specialShop;

        Descriptor = new CategoryDescriptorData();
        Descriptor.MainCtgs = new EnumDictionary<E_SpecialShopType, MainCategoryInfo>();

        /// 최초 세팅 
        DBSpecialShop.ForeachAllCategories((mainCtgType, subCtgType, tableData) =>
        {
            /// 메인 카테고리의 IsDisplayable 과 
            /// 출력 아이템 자체의 IsDisplayable 체킹 
            bool isDisplayable =
                tableData.SpecialShopType != E_SpecialShopType.Colosseum // 콜로세움 제외 
                && tableData.SpecialShopType != E_SpecialShopType.BlackMarket // 암시장 제외 
                && tableData.SpecialShopType != E_SpecialShopType.CollectEvent // 수집 이벤트 제외 
                && DBSpecialShop.IsMainTabDisplayable(mainCtgType)
                && DBSpecialShop.IsItemDisplayable(tableData.SpecialShopID);

            if (isDisplayable)
            {
                /// MainCtg 가 Display 가능 상태일때만 내부에서 관리함 . 
                if (Descriptor.MainCtgs.ContainsKey(mainCtgType) == false)
                {
                    Descriptor.MainCtgs.Add(mainCtgType, new MainCategoryInfo(mainCtgType));
                }

                var mainCtgInfo = Descriptor.MainCtgs[mainCtgType];

                if (DBSpecialShop.IsItemDisplayable(tableData.SpecialShopID))
                {
                    SubCategoryInfo subCtgInfo = mainCtgInfo.subCtgInfoList.Find(t => t.type == tableData.SpecialSubTapType);

                    if (subCtgInfo == null)
                    {
                        subCtgInfo = new SubCategoryInfo();
                        subCtgInfo.type = subCtgType;
                        mainCtgInfo.subCtgInfoList.Add(subCtgInfo);
                    }

                    var buyLimitState = new BuyLimitStateInfo();
                    UpdateBuyLimitInfo(ref buyLimitState, tableData.SpecialShopID);

                    string dummyString = "";
                    E_SpecialShopDisplayGoodsTarget targetGoodsType = E_SpecialShopDisplayGoodsTarget.None;
                    uint externalGoodsTid = 0;
                    byte grade = 0;
                    E_CashType dummyCashType = E_CashType.None;

                    /// 다른 테이블의 상품을 참조하는 Tid 는 여기서 가져옴 
                    DBSpecialShop.GetGoodsPropsBySwitching(tableData.SpecialShopID, ref dummyCashType, ref targetGoodsType, ref dummyString, ref dummyString, ref grade, ref externalGoodsTid);

                    subCtgInfo.allData.Add(new SingleDataInfo(
                        tableData.SpecialShopID
                        , externalGoodsTid
                        , buyLimitState
                        , false
                        , tableData.PositionNumber
                        , targetGoodsType));
                }
            }
        });

        UpdateData();

        foreach (var mainCtg in Descriptor.MainCtgs.Values)
        {
            foreach (var subCtg in mainCtg.subCtgInfoList)
            {
                SortData(subCtg.allData);
            }
        }

        /// TEST 
        //foreach (var item in MainCategories)
        //{
        //    ZLog.LogError(ZLogChannel.UI, item.Value.type.ToString());

        //    foreach (var item2 in item.Value.subCtgInfoList)
        //    {
        //        foreach (var item3 in item2.displayData)
        //        {
        //            ZLog.LogError(ZLogChannel.UI, "displayData : " + item3.specialShopId + " , " + item3.isForSaleByDate);
        //        }
        //    }
        //}
    }

    public void SetListener_OnMainCtgChanged(Action<E_SpecialShopType> listener)
    {
        onMainCtgChanged = listener;
    }

    public void RemoveListener_OnMainCtgChanged()
    {
        onMainCtgChanged = null;
    }

    public void SetListener_OnSubCtgChanged(Action<E_SpecialSubTapType> listener)
    {
        onSubCtgChanged = listener;
    }

    public void RemoveListener_OnSubCtgChanged()
    {
        onSubCtgChanged = null;
    }

    public void SelectMainCategory(E_SpecialShopType type, bool preventCallBack = false)
    {
        var prevType = Descriptor.SelectedMainCtgType;
        Descriptor.SelectedMainCtgType = type;

        if (preventCallBack == false)
        {
            if (prevType != type)
            {
                onMainCtgChanged?.Invoke(type);
            }
        }
    }

    public void SelectSubCategory(E_SpecialSubTapType type, bool preventCallBack = false)
    {
        var prevType = Descriptor.SelectedSubCtgType;
        Descriptor.SelectedSubCtgType = type;

        if (preventCallBack == false)
        {
            if (prevType != type)
            {
                onSubCtgChanged?.Invoke(type);
            }
        }
    }

    public void SelectDefault()
    {
        SelectFirstMainCategory(true);
        //SelectFirstSubCategory();
    }

    public void SelectFirstMainCategory(bool selectFirstSubTab = true)
    {
        if (Descriptor.MainCtgs.Count == 0)
        {
            return;
        }

        var first = Descriptor.MainCtgs.First().Key;
        SelectMainCategory(first);

        if (selectFirstSubTab)
        {
            SelectFirstSubCategory();
        }
    }

    public void SelectFirstSubCategory()
    {
        if (Descriptor.MainCtgs.ContainsKey(Descriptor.SelectedMainCtgType) == false)
            return;

        var mainCtg = SelectedMainCategoryInfo;

        if (mainCtg == null || mainCtg.subCtgInfoList.Count == 0)
            return;

        SelectSubCategory(mainCtg.subCtgInfoList.First().type);
    }

    public SingleDataInfo FindDisplayData(uint tid)
	{
        var data = DBSpecialShop.Get(tid);
        if (data == null)
            return null;

		if (Descriptor.MainCtgs.ContainsKey(data.SpecialShopType) == false)
			return null;

        var subCtgInfoList = Descriptor.MainCtgs[data.SpecialShopType].subCtgInfoList;

		foreach (var subCtg in subCtgInfoList)
		{
            if(subCtg.type == data.SpecialSubTapType)
			{
                return subCtg.displayData.Find(t => t.specialShopId == tid);
			}
		}

        return null;
	}

	/// <summary>
	/// 데이터 업데이트 
	/// </summary>
	public void UpdateData()
    {
        /// 기존에 이미 있던 데이터들의 상태를 바꿈 . 
        foreach (var mainCtg in Descriptor.MainCtgs.Values)
        {
            UpdateData(mainCtg);
        }
    }

    public bool UpdateSelectedCategoryData()
    {
        var selectedMainCtg = SelectedMainCategoryInfo;

        if (selectedMainCtg == null)
            return false;

        return UpdateData(selectedMainCtg);
    }

    #endregion

    #region Overrides 
    #endregion

    #region Private Methods
    #region Get 
    private MainCategoryInfo GetMainCategoryInfo(E_SpecialShopType type)
    {
        if (MainCategories.ContainsKey(type) == false)
        {
            return null;
        }

        return MainCategories[type];
    }
    #endregion

    #region Update
    /// <summary>
    /// 구매 제한 State 정보 업데이트 
    /// </summary>
    private void UpdateBuyLimitInfo(ref BuyLimitStateInfo info, uint shopID)
    {
        var limitInfo = Me.CurCharData.GetBuyLimitInfo(shopID);
        var shopData = DBSpecialShop.Get(shopID);

        if (shopData.BuyLimitType == E_BuyLimitType.Infinite)
        {
            info.Reset();
        }
        else
        {
            info.Set(shopData.BuyLimitType, shopData.BuyLimitCount, limitInfo == null ? 0 : limitInfo.BuyCnt);
        }
    }

    /// <summary>
    /// TODO : 추후에 데이터그룹 키별로 캐싱을해놓고 LazyLoad 를하자 
    /// </summary>
    private void UpdateDisplayData(List<SingleDataInfo> originalDataList, List<SingleDataInfo> displayDataList)
    {
        int requiredDisplayDataList = originalDataList.Count(t => DBSpecialShop.IsForSaleByDate(t.specialShopId));

        /// 출력할 데이터 리스트의 개수가 출력해야할 데이터 개수와 맞지않는다면 
        /// 개수를 맞춰줌 
        //if (displayDataList.Count != requiredDisplayDataList)
        //{
        //    /// 적다면 필요한 만큼 할당 
        //    if (displayDataList.Count < requiredDisplayDataList)
        //    {
        //        int addCnt = requiredDisplayDataList - displayDataList.Count;

        //        for (int i = 0; i < addCnt; i++)
        //        {
        //            displayDataList.Add(new SingleDataInfo());
        //        }
        //    }
        //    /// 넘치는 만큼 없앰 
        //    else if (displayDataList.Count > requiredDisplayDataList)
        //    {
        //        int removeCnt = displayDataList.Count - requiredDisplayDataList;
        //        displayDataList.RemoveRange(0, removeCnt);
        //    }
        //}

        /// 이거일단 이런식으로 구현
        displayDataList.Clear();

        //  int displayDataSetCnt = 0;

        /// Original Data 를 순회하면서 상점에 보여야하는 데이터만 다시 조립함
        for (int i = 0; i < originalDataList.Count; i++)
        {
            var oriData = originalDataList[i];

            oriData.isForSaleByDate = DBSpecialShop.IsForSaleByDate(oriData.specialShopId);

            /// 상점에 보여야 할 데이터 .
            if (oriData.isForSaleByDate && this.FrameSpeicalShop.ValidateDataByFilter(SelectedMainCategoryType, oriData))
            {
                displayDataList.Add(oriData);
                //    var targetDisplayData = displayDataList[displayDataSetCnt];
                // oriData.CopyTo(targetDisplayData);
                //displayDataSetCnt++;
            }
        }

        // displayDataList.RemoveRange(displayDataSetCnt, displayDataList.Count - displayDataSetCnt);
    }

    /// <summary>
    /// Data Dirty 여부를 리턴함 . 
    /// 현재 (201026) 기준 무조건 True 반환하지만 후에
    /// 캐싱 및 비교 로직을 추가하는 경우 대비 
    /// </summary>
    private bool UpdateData(MainCategoryInfo mainCtgInfo)
    {
        if (mainCtgInfo == null)
            return false;

        foreach (var subCtg in mainCtgInfo.subCtgInfoList)
        {
            foreach (var data in subCtg.allData)
            {
                UpdateBuyLimitInfo(ref data.buyLimitInfo, data.specialShopId);
            }

            /// Display List 의 상태 Update 
            UpdateDisplayData(subCtg.allData, subCtg.displayData);
            //     ZLog.LogError(ZLogChannel.UI, "DataCOsubCtg.displayData.Count.ToString());
        }

        return true;
    }

    private void SortData(List<SingleDataInfo> list)
    {
        list.Sort((t01, t02) =>
        {
            return t01.sortParam.CompareTo(t02.sortParam);
        });
    }
    #endregion
    #endregion

    #region Inspector Events (OnClick)

    #endregion

    #region Define
    class CategoryDescriptorData
    {
        public EnumDictionary<E_SpecialShopType, MainCategoryInfo> MainCtgs;

        public E_SpecialShopType SelectedMainCtgType;
        public E_SpecialSubTapType SelectedSubCtgType;
    }

    public class MainCategoryInfo
    {
        public E_SpecialShopType type;

        public List<SubCategoryInfo> subCtgInfoList = new List<SubCategoryInfo>();

        public MainCategoryInfo(E_SpecialShopType type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// 주의점은 스페셜 상점의 E_SpecialSubTapType 타입의 None 값도 하나의 SubType 으로 취급함 .
    /// 하지만 Visual 로는 보이지않음 . 
    /// </summary>
    public class SubCategoryInfo
    {
        public E_SpecialSubTapType type;
        public List<SingleDataInfo> allData = new List<SingleDataInfo>();
        public List<SingleDataInfo> displayData = new List<SingleDataInfo>();

        //public SubCategoryInfo(E_SpecialSubTapType type, uint tid)
        //{
        //    this.type = type;
        //}
    }

    /// <summary>
    /// 하나의 데이터 . 
    /// </summary>
    public class SingleDataInfo
    {
        public uint specialShopId;
        public uint tidForTargetGoods;

        public uint sortParam;

        public BuyLimitStateInfo buyLimitInfo = new BuyLimitStateInfo();

        /// <summary>
        /// 현재 판매 기간안에 해당 되는 애인지 
        /// </summary>
        public bool isForSaleByDate;

        public E_SpecialShopDisplayGoodsTarget targetGoodsType;

        public SingleDataInfo()
        {
            this.buyLimitInfo = new BuyLimitStateInfo();
        }

        public SingleDataInfo(uint specialShopId, uint tidForTargetGoods, BuyLimitStateInfo buyLimitInfo, bool isForSaleByDate, uint sortParam, E_SpecialShopDisplayGoodsTarget targetGoodsType)
        {
            this.specialShopId = specialShopId;
            this.tidForTargetGoods = tidForTargetGoods;
            this.buyLimitInfo = buyLimitInfo;
            this.isForSaleByDate = isForSaleByDate;
            this.targetGoodsType = targetGoodsType;
            this.sortParam = sortParam;
        }

        public void CopyTo(SingleDataInfo dest)
        {
            dest.specialShopId = this.specialShopId;
            dest.isForSaleByDate = this.isForSaleByDate;
            dest.tidForTargetGoods = this.tidForTargetGoods;
            dest.targetGoodsType = this.targetGoodsType;
            dest.sortParam = this.sortParam;
            this.buyLimitInfo.CopyTo(dest.buyLimitInfo);
        }

        //public void UpdateBuyLimitInfo()
        //{
        //    /// TODO 
        //    // Me.CurCharData.AddBuyLimitInfo
        //}

        public bool IsDisplayable()
        {
            return DBSpecialShop.IsForSaleByDate(specialShopId);
        }
    }
    #endregion
}
