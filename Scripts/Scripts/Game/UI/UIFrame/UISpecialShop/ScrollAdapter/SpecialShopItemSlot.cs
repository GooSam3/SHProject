using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameDB;
using static DBSpecialShop;
using static SpecialShopCategoryDescriptor;
using ZNet.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpecialShopItemSlot : MonoBehaviour
{
    #region Public Fields
    Action OnClickedListener;
    #endregion

    #region Private Fields

    private bool init;
    #endregion

    #region UI 
    // [SerializeField] private List<UIGroupBase> uiGroupIntegrated;

    [SerializeField] private RawImage rawImgBigSizeIcon;
    [SerializeField] private Image imgBigSizeMainIcon;

    [SerializeField] private Image imgSmallSizeMainIcon;

    [SerializeField] private Text txtName;
    [SerializeField] private GameObject mileageRoot;
    [SerializeField] private GameObject buyLimitCountRoot;
    [SerializeField] private Text txtLimitCount;
    [SerializeField] private List<GameObject> activeOnSaleRemainedTime;
    [SerializeField] private GameObject activeOnNoRemainedTime;
    [SerializeField] private Text saleRemainedTime;
    [SerializeField] private Text txtCostNum;
    [SerializeField] private Image imgCostIcon;
    [SerializeField] private Image imgItemGradeBG;
    [SerializeField] private GameObject lockRoot;

    //[SerializeField] private UIGroup_RateItem uiGroup_rateItem;
    //[SerializeField] private UIGroup_Class uiGroup_class;
    //[SerializeField] private UIGroup_Item uiGroup_item;
    //[SerializeField] private UIGroup_Pet uiGroup_pet;
    #endregion

    #region Preference
    #endregion

    public bool IsLocked { get; private set; }

    #region Public Methods
    public void SetUI(SingleDataInfo info)
    {
        if (init == false)
            Initialize();

        var specialShopData = DBSpecialShop.Get(info.specialShopId);
        E_CashType cashType = E_CashType.None;
        E_SpecialShopDisplayGoodsTarget targetGoods = E_SpecialShopDisplayGoodsTarget.None;
        string nameTxtKey = string.Empty;
        string iconName = string.Empty;
        string costSpriteName = string.Empty;
        byte grade = 0;
        uint targetGoodsTid = 0;

        DBSpecialShop.GetGoodsPropsBySwitching(info.specialShopId, ref cashType, ref targetGoods, ref nameTxtKey, ref iconName, ref grade, ref targetGoodsTid);

        /// 기본 UI 세팅 
        SetIcon(specialShopData, iconName);
        txtName.text = DBLocale.GetText(nameTxtKey);
        txtCostNum.text = specialShopData.BuyItemCount.ToString("n0");
        var hasCostItemCount = ZNet.Data.Me.GetCurrency(specialShopData.BuyItemID);
        txtCostNum.color = cashType == E_CashType.Cash ? Color.white : (hasCostItemCount >= specialShopData.BuyItemCount ? Color.white : Color.red);
        costSpriteName = cashType == E_CashType.Cash ? "icon_cash_kr" : DBItem.GetItemIconName(specialShopData.BuyItemID);
        imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(costSpriteName);
        imgItemGradeBG.sprite = specialShopData.SpecialShopType != E_SpecialShopType.Essence ? ZManagerUIPreset.Instance.GetSprite("img_slotgrade_gray") : GetGradeSprite(grade);

        //switch (info.targetGoodsType)
        //{
        //    case E_SpecialShopDisplayGoodsTarget.None:
        //        break;
        //    case E_SpecialShopDisplayGoodsTarget.RateItem:
        //        {
        //            SetUI_RateItem(specialShopData);
        //        }
        //        break;
        //    case E_SpecialShopDisplayGoodsTarget.Item:
        //        {
        //            SetUI_Item(specialShopData, DBItem.GetItem(info.tidForTargetGoods));
        //        }
        //        break;
        //    case E_SpecialShopDisplayGoodsTarget.Change:
        //        {
        //            /// 2020 10 23 기준 강림 판매할 계획 X , 구현 X - 기획 컨펌 사항 
        //            ZLog.LogError(ZLogChannel.UI, "Not implemented");
        //        }
        //        break;
        //    case E_SpecialShopDisplayGoodsTarget.Pet:
        //        {
        //            /// 2020 10 23 기준 펫 판매할 계획 X , 구현 X - 기획 컨펌 사항 
        //            ZLog.LogError(ZLogChannel.UI, "Not implemented");
        //        }
        //        break;
        //    default:
        //        {
        //            ZLog.LogError(ZLogChannel.UI, "type not handled");
        //        }
        //        break;
        //}

        //this.imgBigSizeMainIcon.gameObject.SetActive(specialShopData.SizeType == E_SizeType.Big);
        //this.rawImgBigSizeIcon.gameObject.SetActive(specialShopData.SizeType == E_SizeType.Big);
        //this.imgSmallSizeMainIcon.gameObject.SetActive(specialShopData.SizeType == E_SizeType.Small);

        /// 마일리지  
        bool mileageActive = specialShopData.ShopListID.Count > 0 && DBShopList.IsMileageExist(specialShopData.ShopListID);
        mileageRoot.SetActive(mileageActive);

        /// 구매 제한  
        bool buyLimitActive = info.buyLimitInfo.buyLimitType != E_BuyLimitType.Infinite;

        if (buyLimitActive)
        {
            string buyLimitText = "구매제한";

			switch (specialShopData.BuyLimitType)
			{
				case E_BuyLimitType.OneShot:
                    buyLimitText = "BuyLimitType_OneShot";
                    break;
				case E_BuyLimitType.Day:
                    buyLimitText = "BuyLimitType_Day";
                    break;
				case E_BuyLimitType.Week:
                    buyLimitText = "BuyLimitType_Week";
                    break;
				case E_BuyLimitType.Monthly:
                    buyLimitText = "BuyLimitType_Monthly";
                    break;
				default:
                    ZLog.LogError(ZLogChannel.UI, "WHY HERE? NO NEED TO SHOW LIMIT INFO !");
					break;
			}

            buyLimitText = DBLocale.GetText(buyLimitText);
			txtLimitCount.text = string.Format("{0} <color=#ffffff>( {1} / {2} )</color>", buyLimitText, info.buyLimitInfo.curBuyCnt, info.buyLimitInfo.limitBuyCnt);
		}

        buyLimitCountRoot.SetActive(buyLimitActive);

        /// 남은 시간
        bool saleRemainedTimeActive = specialShopData.BuyFinishTime != 0;

        if (saleRemainedTimeActive)
        {
            this.saleRemainedTime.text = TimeHelper.GetRemainFullTimeMin((specialShopData.BuyFinishTime - TimeManager.NowMs) / 1000);
        }

        this.activeOnSaleRemainedTime.ForEach(t => t.SetActive(saleRemainedTimeActive));
        activeOnNoRemainedTime.SetActive(saleRemainedTimeActive == false);

        /// 잠금
        IsLocked = DBSpecialShop.IsLocked(info.specialShopId, Me.CurCharData.ItemGainHistory);
        lockRoot.SetActive(IsLocked);

        /// 타입에 맞게 Active 
        //foreach (var groupInte in uiGroupIntegrated)
        //{
        //    groupInte.enableList.ForEach(t => t.gameObject.SetActive(t.active));
        //}
    }

    public void SetOnClickHandler(Action onClicked)
    {
        OnClickedListener = onClicked;
    }

    public void OnClicked()
    {
        if (IsLocked)
        {
            return;
        }

        OnClickedListener?.Invoke();
    }
    #endregion

    #region Private Methods
    private void Initialize()
    {
        if (init)
            return;

        /// Group 이 새로 생기면 Add 해주는 처리가 필요함 
        //uiGroupIntegrated = new List<UIGroupBase>();
        //uiGroupIntegrated.Add(uiGroup_class);
        //uiGroupIntegrated.Add(uiGroup_item);
        //uiGroupIntegrated.Add(uiGroup_pet);

        init = true;
    }

    public void Release()
    {
        if (rawImgBigSizeIcon.texture != null)
        {
            Addressables.Release<Texture>(rawImgBigSizeIcon.texture);
            rawImgBigSizeIcon.texture = null;
        }
    }

    /*
     *         
     *  public Text txtName;
        public GameObject buyLimitRoot;
        public Text txtBuyLimit;
        public Image imgRewardMileageIcon;
        public Text txtRewardMileageCount;
        public Image imgCostIcon;
        public Text txtCostCount;
     * */

    //private void SetUI_CashItem(SpecialShop_Table data)
    //{
    //    GetMainIconBySize(data.SizeType).sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(DBConfig.Diamond_ID));
    //    txtName.text = DBLocale.GetText(data.ShopTextID);
    //    txtCostNum.text = data.BuyItemCount.ToString("n0");
    //    imgCostIcon.sprite = null;

    //    ZLog.LogError(ZLogChannel.UI, "Please Set cost sprite");
    //}

    //private void SetUI_RateItem(SpecialShop_Table data)
    //{
    //    //      uiGroup_rateItem.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
    //    // TODO uiGroup_rateItem.buyLimitRoot
    //    //        uiGroup_rateItem.txtName.text = DBLocale.GetText(data.ShopTextID);

    //    SetIcon(data.SizeType).sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
    //    txtName.text = DBLocale.GetText(data.ShopTextID);
    //    txtCostNum.text = data.BuyItemCount.ToString("n0");
    //    imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(data.BuyItemID));

    //    // uiGroup_rateItem.buyLimitRoot
    //}

    //private void SetUI_Change(Change_Table data)
    //{
    //    uiGroup_class.imgGrade.sprite = GetGradeSprite(data.Grade);
    //    uiGroup_class.imgElement.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetAttributeSpriteName(data.AttributeType));
    //    uiGroup_class.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
    //    uiGroup_class.imgClass.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetCharacterTypeSprite(data.UseAttackType));
    //    uiGroup_class.txtName.text = DBLocale.GetText(data.ChangeTextID);
    //    uiGroup_class.txtName.color = UIFrameMileage.GetColorByGrade(data.Grade);
    //    uiGroup_class.txtProperty.text = string.Format("{0} - {1}", DBLocale.GetText(UIFrameMileage.GetAttributeName(data.AttributeType)), DBLocale.GetCharacterTypeName(data.UseAttackType));
    //}

    /// <summary>
    /// CAUTION : 다이아몬드를 현금 결제로 사는 경우 (SpecialShopTable 에서 GoodsID__ 전부 0 에 GoodsType 이 Item 에 DiamondCount 가 0 초과인 경우에도
    /// 이 로직을 실행함 . DBSpecialShop 에서 CashType 을 체킹하여 Cash 인 경우 하드코딩으로 중간에 Item 정보를 다이아몬드 정보로 넣음 
    /// </summary>
    //private void SetUI_Item(SpecialShop_Table specialShopData, Item_Table data)
    //{
    //    GetTargetIcon(specialShopData.SizeType).sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
    //    txtCostNum.text = specialShopData.BuyItemCount.ToString("n0");

    //    /// 현금 결제인 경우는 DBItem 에서 Icon Sprite 를 가져올 수없으므로 별도 처리 
    //    if (specialShopData.CashType == E_CashType.Cash)
    //    {
    //        txtName.text = DBLocale.GetText(specialShopData.ShopTextID);
    //        imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_cash_kr");
    //    }
    //    else
    //    {
    //        txtName.text = DBLocale.GetText(data.ItemTextID);
    //        imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(specialShopData.BuyItemID));
    //    }

    //    //uiGroup_item.imgMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
    //    //uiGroup_item.imgClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(UIFrameMileage.GetCharacterTypeSprite(data.UseCharacterType));
    //    //uiGroup_item.imgGrade.sprite = GetGradeSprite(data.Grade);
    //    //uiGroup_item.txtProperty.text = string.Format("{0} - {1}", DBLocale.GetText(DBLocale.GetCharacterTypeName(data.UseCharacterType)), DBLocale.GetItemTypeText(data.ItemType));
    //    //uiGroup_item.txtName.text = DBLocale.GetText(data.ItemTextID);
    //    //uiGroup_item.txtName.color = UIFrameMileage.GetColorByGrade(data.Grade);
    //    //uiGroup_item.txtCost.text = specialShopData.BuyItemCount.ToString("n0");
    //    //uiGroup_item.imgCostIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(specialShopData.BuyItemID));
    //}

    //private void SetUI_Pet(Pet_Table data)
    //{
    //    uiGroup_pet.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.Icon);
    //    uiGroup_pet.imgGrade.sprite = GetGradeSprite(data.Grade);

    //    string petTypeSpriteName = "";

    //    if (data.PetType == E_PetType.Pet)
    //    {
    //        petTypeSpriteName = "icon_equip_ride";
    //    }
    //    else if (data.PetType == E_PetType.Vehicle)
    //    {
    //        petTypeSpriteName = "icon_equip_pet";
    //    }

    //    uiGroup_pet.imgPetType.sprite = ZManagerUIPreset.Instance.GetSprite(petTypeSpriteName);
    //}

    #region Util
    private Sprite GetGradeSprite(byte grade)
    {
        return ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(grade));
    }

    private void SetIcon(SpecialShop_Table data, string iconName)
    {
        rawImgBigSizeIcon.gameObject.SetActive(false);
        imgSmallSizeMainIcon.gameObject.SetActive(false);
        imgBigSizeMainIcon.gameObject.SetActive(false);

        if (data.SizeType == E_SizeType.Big) {
            //rawImgBigSizeIcon.texture = ZManagerUIPreset.Instance.GetSprite(;
            rawImgBigSizeIcon.gameObject.SetActive(true);

            if (rawImgBigSizeIcon.texture == null || rawImgBigSizeIcon.texture.name != iconName) {
                if (rawImgBigSizeIcon.texture != null) {
                    Addressables.Release<Texture>(rawImgBigSizeIcon.texture);
                    rawImgBigSizeIcon.texture = null;
                }

                Addressables.LoadAssetAsync<Texture>(iconName).Completed += (AsyncOperationHandle<Texture> _Result) => {
                    if (_Result.Status == AsyncOperationStatus.Succeeded) {
                        rawImgBigSizeIcon.texture = _Result.Result;
                    }
                };
            }
            else {
                rawImgBigSizeIcon.rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        else {
            if (data.SpecialShopType == E_SpecialShopType.Essence||
                data.SpecialShopType == E_SpecialShopType.Colosseum) 
            {
                imgSmallSizeMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(iconName);
                imgSmallSizeMainIcon.gameObject.SetActive(true);
            }
            else
            {
                imgBigSizeMainIcon.sprite = ZManagerUIPreset.Instance.GetSprite(iconName);
                imgBigSizeMainIcon.gameObject.SetActive(true);
            }
        }
    }

    //private string GetCharacterTypeName(E_CharacterType type)
    //{
    //    switch (type)
    //    {
    //        case E_CharacterType.Knight:
    //            return "Character_Knight";
    //        case E_CharacterType.Archer:
    //            return "Character_Archer";
    //        case E_CharacterType.Wizard:
    //            return "Character_Wizard";
    //        case E_CharacterType.Assassin:
    //            return "Character_Assassin";
    //        case E_CharacterType.All:
    //            ZLog.LogError(ZLogChannel.UI, "cannot be all here");
    //            return "";
    //        default:
    //            break;
    //    }

    //    return "";
    //}

    #endregion

    #endregion

    #region Define
    /// <summary>
    /// UIGroup Base 클래스 
    /// </summary>
    [System.Serializable]
    public class UIGroupBase
    {
        [Serializable]
        public class SetEnableGameObject
        {
            public GameObject gameObject;
            public bool active;
        }

        public E_SpecialShopDisplayGoodsTarget dataType;
        public List<SetEnableGameObject> enableList;
    }

    //[System.Serializable]
    //public class UIGroup_RateItem
    //{
    //    public Image imgMainIcon;
    //    public Text txtName;
    //    public GameObject buyLimitRoot;
    //    public Text txtBuyLimit;
    //    public Image imgRewardMileageIcon;
    //    public Text txtRewardMileageCount;
    //    public Image imgCostIcon;
    //    public Text txtCostCount;
    //}

    /// <summary>
    /// TODO : 2020 10 23 구현 기준 강림은 일단 스페셜 상점에서 구현할 필요 없으므로 일단 스킵 . 추후작업 
    /// </summary>
    //[Serializable]
    //public class UIGroup_Class : UIGroupBase
    //{
    //    public Image imgMainIcon;
    //    public Image imgGrade;
    //    public Image imgClass;
    //    public Image imgElement;
    //    public Text txtProperty;
    //    public Text txtName; /// Grade Color 설정 필요 
    //}

    [Serializable]
    public class UIGroup_Item : UIGroupBase
    {
        public Image imgMainIcon;
        public Image imgClassIcon;
        public Image imgGrade;
        public Image imgCostIcon;
        public Text txtProperty;
        public Text txtName;
        public Text txtCost;
    }

    /// TODO : 2020 10 23 구현 기준 펫은 일단 스페셜 상점에서 구현할 필요 없으므로 일단 스킵 . 추후작업 
    //[Serializable]
    //public class UIGroup_Pet : UIGroupBase
    //{
    //    public Image imgIcon;
    //    public Image imgGrade;
    //    public Image imgPetType;
    //}

    #endregion
}
