using Devcat;
using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static DBMileageShop;
using static UIFrameMileage;

/*
 * Main/Sub 카테고리 자체의 정보 관리 클래스.
 * 카테고리의 선택 및 visible 등 정보는 여기서 가져옴 
 * (카테고리에서 보여지는 Data 는 MileageDisplayDataHandler 에서 관리됌)
 * */
public class MileageCategoryDescriptor
{
    private CategoryDescripterData _CategoryDesc;

    private UIFrameMileage FrameMileage;

    public MileageCategoryDescriptor() { }

    #region Properties
    public E_MileageShopType SelectedMainCategoryType { get { return _CategoryDesc.SelectedMainCategory; } }
    public MileageDataEvaluateTargetDataType SelectedDataType
    {
        get
        {
            var subCtg = SelectedSubCategory;
            if (subCtg == null)
                return MileageDataEvaluateTargetDataType.None;
            return subCtg.TargetDataType;
        }
    }
    public MainCategoryInfo SelectedMainCategoryInfo { get { return GetMainCategoryInfo(SelectedMainCategoryType); } }
    public int SelectedSubCategoryIndex
    {
        get
        {
            return _CategoryDesc.SelectedSubCategoryIndex;
        }
    }
    public SubCategoryInfo SelectedSubCategory
    {
        get
        {
            var subCtgs = _CategoryDesc.MainCategories[_CategoryDesc.SelectedMainCategory].subCategories;
            return subCtgs.Find(t => t.CtgData.ShopID == _CategoryDesc.SelectedSubCategoryShopID);
        }
    }
    public List<SubCategoryInfo> SelectedSubCategoryList
    {
        get
        {
            return _CategoryDesc.MainCategories[_CategoryDesc.SelectedMainCategory].subCategories;
        }
    }
    public List<MileageBaseDataIdentifier> CurrentDisplayData
    {
        get
        {
            var subCtg = SelectedSubCategory;
            if (subCtg == null)
                return new List<MileageBaseDataIdentifier>();
            return subCtg.DisplayData;
        }
    }
    public uint SelectedSubCategoryShopID
    {
        get
        {
            var target = SelectedSubCategory;
            if (target == null)
                return 0;
            return target.CtgData.ShopID;
        }
    }
    public uint SelectedSubCategoryShopListID
	{
		get
		{
            var target = SelectedSubCategory;
            if (target == null)
                return 0;
            return target.CtgData.ShopListID;
		}
	}

    public int MaxSubCategoryCount
    {
        get
        {
            int maxCnt = 0;
            foreach (var mainCtg in _CategoryDesc.MainCategories)
            {
                if (mainCtg.Value.subCategories.Count > maxCnt)
                    maxCnt = mainCtg.Value.subCategories.Count;
            }
            return maxCnt;
        }
    }
    public int SelectedSubCategoryCount
    {
        get
        {
            return SelectedSubCategoryList.Count;
        }
    }
    public int SelectedMainCategoryDisplayableSubCategoryCount
    {
        get
        {
            var mainCtg = SelectedMainCategoryInfo;
            if (mainCtg == null)
                return 0;

            int cnt = 0;

            for (int i = 0; i < mainCtg.subCategories.Count; i++)
            {
                var subCtg = mainCtg.subCategories[i];

                if (subCtg.IsDisplayable())
                {
                    cnt++;
                }
            }

            return cnt;
        }
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void Initialize(UIFrameMileage frameMileage, List<E_MileageShopType> mainCategories)
    {
        _CategoryDesc = new CategoryDescripterData();
        FrameMileage = frameMileage;

        #region Set Category Base Info 
        /// 파라미터 mainCateogory 순서따라 세팅함 
        for (int i = 0; i < mainCategories.Count; i++)
        {
            _CategoryDesc.MainCategories.Add(mainCategories[i], new MainCategoryInfo() { type = mainCategories[i], subCategories = new List<SubCategoryInfo>() });
            var source = _CategoryDesc.MainCategories.Last().Value;
            InitializeCategoryInfo(mainCategories[i], source);
        }
        #endregion

        #region Set Display Data 
        #endregion
    }

    public bool SetDisplayData(SubCategoryInfo subCtg, List<RequestEvaluateParam> reqParam)
    {
        if (subCtg == null)
            return false;

        /// 의미없는 중복 세팅 방지 
        if (subCtg.CurItemEvaluatorType.Count > 0)
        {
            if (IsDataRequestParamEqual(reqParam, subCtg.CurItemEvaluatorType))
            {
                return false;
            }
        }

        subCtg.DisplayData = FrameMileage.GetData(subCtg.CtgData.ShopListID, subCtg.TargetDataType, reqParam);
        subCtg.CurItemEvaluatorType = reqParam;

        return true;
    }

    public bool UpdateSelectedSubCategoryData(List<RequestEvaluateParam> reqParam)
    {
        var subCtg = SelectedSubCategory;

        if (subCtg == null)
            return false;

        // subCtg.DisplayData = FrameMileage.GetData(subCtg.CtgData.ShopListID, subCtg.TargetDataType, reqParam);
        return SetDisplayData(subCtg, reqParam);
    }

    public void SetDefault()
    {
        SelectFirstMainCategory();
        SelectFirstSubCateogry();
    }

    public void ForeachDisplayableSubCategories(Action<SubCategoryInfo> loopCallBack)
    {
        var mainCtg = SelectedMainCategoryInfo;
        if (mainCtg == null)
            return;

        for (int i = 0; i < mainCtg.subCategories.Count; i++)
        {
            var subCtg = mainCtg.subCategories[i];

            if (subCtg.IsDisplayable())
            {
                loopCallBack.Invoke(subCtg);
            }
        }
    }


    public MainCategoryInfo GetFirstVisibleCategory()
    {
        foreach (var mainCtg in _CategoryDesc.MainCategories.Values)
        {
            if (mainCtg.IsDisplayable())
            {
                return mainCtg;
            }
        }

        return null;
    }

    public int GetSubCateogryIndex(uint mileageShopID)
    {
        foreach (var mainCtg in _CategoryDesc.MainCategories)
        {
            for (int i = 0; i < mainCtg.Value.subCategories.Count; i++)
            {
                var subCtg = mainCtg.Value.subCategories[i];

                if (subCtg.CtgData.ShopID == mileageShopID)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public SubCategoryInfo GetSubCategoryInfo(uint mileageShopID)
    {
        foreach (var mainCtg in _CategoryDesc.MainCategories)
        {
            for (int i = 0; i < mainCtg.Value.subCategories.Count; i++)
            {
                var subCtg = mainCtg.Value.subCategories[i];

                if (subCtg.CtgData.ShopID == mileageShopID)
                {
                    return subCtg;
                }
            }
        }

        return null;
    }

    //public List<RequestEvaluateParam> GetSubCategoryCurrentReqParam(uint mileageShopID)
    //{
    //    var target = GetSubCategoryInfo(mileageShopID);
    //    if (target == null)
    //        return new List<RequestEvaluateParam>();
    //    return target.CurItemEvaluatorType;
    //}

    public bool IsMainCategoryDisplayable(E_MileageShopType type)
    {
        var target = GetMainCategoryInfo(type);

        if (target == null)
            return false;

        return target.IsDisplayable();
    }

    public bool IsSubCategoryDisplayable(uint mileageShopID)
    {
        var target = GetSubCategoryInfo(mileageShopID);

        if (target == null)
            return false;

        return target.IsDisplayable();
    }

    public void SelectMainCategory(E_MileageShopType type)
    {
        _CategoryDesc.SelectedMainCategory = type;
    }

    public bool SelectSubCategory(uint mileageShopID)
    {
        /// 지금 선택된 MainCategory 에 Select 하려는 SubCategory 가 없으면 에러가 날것임 . 
        if (MainCategoryHasSubCategory(SelectedMainCategoryType, mileageShopID) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "No Target MileageShopID Found From Current MainCategory , MileageShopID : " + mileageShopID);
            return false;
        }

        _CategoryDesc.SelectedSubCategoryShopID = mileageShopID;
        _CategoryDesc.SelectedSubCategoryIndex = SelectedMainCategoryInfo.subCategories.FindIndex(t => t.CtgData.ShopID == mileageShopID);

        return true;
    }

    public void SelectFirstMainCategory()
    {
        if (_CategoryDesc.MainCategories.Count == 0)
            return;

        var t = GetFirstVisibleCategory();

        SelectMainCategory(t == null ? E_MileageShopType.None : t.type);
    }

    public void SelectFirstSubCateogry()
    {
        var selectedMainCtgInfo = SelectedMainCategoryInfo;

        if (selectedMainCtgInfo == null
            || selectedMainCtgInfo.subCategories.Count == 0)
            return;

        var firstSubCtg = selectedMainCtgInfo.subCategories.Find(t => t.IsDisplayable());
        uint targetShopID = firstSubCtg.CtgData.ShopID;
        SelectSubCategory(targetShopID);
    }
    #endregion

    #region Private Methods
    private void InitializeCategoryInfo(E_MileageShopType mainCategory, MainCategoryInfo mainCtg)
    {
        mainCtg.type = mainCategory;
        mainCtg.subCategories = new List<SubCategoryInfo>();

        var subCtgList = DBMileageShop.GetTableDataByShopType(mainCategory);
        var dataReqList = new List<RequestEvaluateParam>();

        if (subCtgList != null)
        {
            foreach (var t in subCtgList)
            {
                var subCtg = new SubCategoryInfo();
                var subCtgTableData = new SubCategoryData(t.MileageShopID, t.ShopTextID, t.ShopListID, t.UnusedType);
                var dataType = GetDataTypeFromMileageShopID(t.MileageShopID);
                subCtg.CtgData = subCtgTableData;
                subCtg.TargetDataType = dataType;
                
                var getDataReqParams = new List<RequestEvaluateParam>();

                /// init 이므로 Default 값으로 필터링하여 데이터 세팅함 
                foreach (var key in FrameMileage.GetDefaultEvaluatorKeys(dataType))
                {
                    getDataReqParams.Add(new RequestEvaluateParam(key, MileageAndOrOperation.And));
                }

                #region Set Data
                SetDisplayData(subCtg, getDataReqParams);
                #endregion

                subCtg.display = subCtg.CtgData.UnusedType == E_UnusedType.Use && subCtg.DisplayData != null && subCtg.DisplayData.Count > 0; ;

                mainCtg.subCategories.Add(subCtg);
            }
        }
        else
        {
            subCtgList = new List<MileageShop_Table>();
            ZLog.LogWarn(ZLogChannel.UI, "TargetMileageShopType TableData not exist , " + mainCategory.ToString());
        }

        /// 메인 카테고리의 Display 세팅 
        mainCtg.display = mainCtg.subCategories.Count > 0 && mainCtg.subCategories.Exists(t => t.IsDisplayable());
    }

    //private void SetDisplayData(SubCategoryInfo subCtg, List<MileageBaseDataIdentifier> dataList, List<RequestEvaluateParam> reqParam)
    //{
    //    subCtg.DisplayData = dataList;
    //    subCtg.CurItemEvaluatorType = reqParam;
    //}

    /// <summary>
    /// Sub Category 의 실제 데이터 종류를 가져옴 (아이템, 강림, 펫 , 룬 ETC..)
    /// </summary>
    private MileageDataEvaluateTargetDataType GetDataTypeFromMileageShopID(uint mileageShopID)
    {
        var singleMileageShopData = DBMileageShop.GetDataByTID(mileageShopID);

        if (singleMileageShopData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "TableDataNotFound (MileageShop_table) , TID : " + mileageShopID);
            return MileageDataEvaluateTargetDataType.None;
        }

        var shopItemListData = DBShopList.GetDataByTID(singleMileageShopData.ShopListID);

        if (shopItemListData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "TableDataNotFound (ShopList_table)");
            return MileageDataEvaluateTargetDataType.None;
        }

		/// 0 보다 크다는 것은 현재 ShopList 데이터가 
		/// 해당 아이템 테이블의 Group ID 를 의미하는 것임 .
		//if (shopItemListData.GoodsItemID > 0)
		//{
		//	return MileageDataEvaluateTargetDataType.Item;
		//}
		//else if (shopItemListData.GoodsChangeID > 0)
		//{
		//	return MileageDataEvaluateTargetDataType.Change;
		//}
		//else if (shopItemListData.GoodsPetID > 0)
		//{
		//	return MileageDataEvaluateTargetDataType.Pet;
		//}
		//else if (shopItemListData.GoodsRuneID > 0)
		//{
		//	return MileageDataEvaluateTargetDataType.Rune;
		//}

		if (DBListGroup.IsItemByListGroupID(shopItemListData.ListGroupID))
		{
			return MileageDataEvaluateTargetDataType.Item;
		}
		else if (DBListGroup.IsChangeByListGroupID(shopItemListData.ListGroupID))
		{
			return MileageDataEvaluateTargetDataType.Change;
		}
		else if (DBListGroup.IsPetByListGroupID(shopItemListData.ListGroupID))
		{
			return MileageDataEvaluateTargetDataType.Pet;
		}
		else if (DBListGroup.IsRuneByListGroupID(shopItemListData.ListGroupID))
		{
			return MileageDataEvaluateTargetDataType.Rune;
		}

		return MileageDataEvaluateTargetDataType.None;
    }

    private bool MainCategoryHasSubCategory(E_MileageShopType type, uint mileageShopID)
    {
        var mainCtg = GetMainCategoryInfo(type);

        if (mainCtg == null)
            return false;

        return mainCtg.subCategories.Exists(t => t.CtgData.ShopID == mileageShopID);
    }

    private MainCategoryInfo GetMainCategoryInfo(E_MileageShopType type)
    {
        foreach (var mainCtg in _CategoryDesc.MainCategories.Values)
        {
            if (mainCtg.type == type)
            {
                return mainCtg;
            }
        }

        return null;
    }

    private bool IsDataRequestParamEqual(
        List<RequestEvaluateParam> sour
         , List<RequestEvaluateParam> dest)
    {
        if (dest == null)
            return false;

        if (sour.Count != dest.Count)
            return false;

        for (int i = 0; i < sour.Count; i++)
        {
            var tSour = sour[i];

            if (dest.Exists(t => t.conditionalType == tSour.conditionalType && t.key == tSour.key) == false)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Define
    /// <summary>
    ///  Data Capturerd
    /// </summary>
    class CategoryDescripterData
    {
        public EnumDictionary<E_MileageShopType, MainCategoryInfo> MainCategories = new EnumDictionary<E_MileageShopType, MainCategoryInfo>();
        public E_MileageShopType SelectedMainCategory = E_MileageShopType.None;
        public uint SelectedSubCategoryShopID = 0;
        public int SelectedSubCategoryIndex;
    }

    public class MainCategoryInfo : MileageCategoryDisplayable
    {
        public E_MileageShopType type;
        public List<SubCategoryInfo> subCategories = new List<SubCategoryInfo>();
        /// <summary>
        /// 메인 카테고리는 최초 한번에 Default 필터링 값으로 데이터 체킹후 
        /// SubCategory 개수가 있다면 출력해줌 
        /// </summary>
        public bool display;

        public MainCategoryInfo() { }
        public override bool IsDisplayable()
        {
            return display;
        }
    }

    public class SubCategoryInfo : MileageCategoryDisplayable
    {
        public SubCategoryData CtgData;

        /// <summary>
        /// 실제 참조해야할 데이터 종류 
        /// </summary>
        public MileageDataEvaluateTargetDataType TargetDataType;
        /// <summary>
        /// 현재 띄어줘야하거나 띄어주고 있는 데이터 
        /// </summary>
        public List<MileageBaseDataIdentifier> DisplayData;
        /// <summary>
        /// 현재 설정된 데이터 Evaluator 타입들 
        /// 중복 Get 방지 체킹 가능 
        /// </summary>
        public List<RequestEvaluateParam> CurItemEvaluatorType = new List<RequestEvaluateParam>();

        public bool display;

        public override bool IsDisplayable()
        {
            return display;
        }

        public void ClearCurItemEvaluatorTypeList()
        {
            CurItemEvaluatorType.Clear();
        }
    }
    #endregion
}
