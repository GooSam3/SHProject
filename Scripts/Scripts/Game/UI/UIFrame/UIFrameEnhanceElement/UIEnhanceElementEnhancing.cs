using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;
using static DBAttribute;

public class UIEnhanceElementEnhancing : MonoBehaviour
{
    #region Readonly
    private readonly uint PerRateUnit = 10000;
    #endregion

    #region Serialized Field
    [SerializeField] private Text txtRate;
    [SerializeField] private RectTransform costItemParent;
    [SerializeField] private ZButton btnEnhance;
    [SerializeField] private Button btnDecrease;
    [SerializeField] private Button btnIncrease;
    [SerializeField] private Button btnMax;

    [SerializeField] private UIEnhanceElementCostItem costItemSourceObj;
    #endregion

    #region System Variables
    private UIEnhanceElement EnhanceController;
    private List<UIEnhanceElementCostItem> costItems;

    // private event Action<bool> OnEnhanceTried;

    // 테이블 데이터 직접 참조 X 

    private uint selectedAttributeID;
    private uint selectedAttributeLevel;
    //    private uint selectedAttributeMaxLevel;
    private E_UnitAttributeType selectedType;

    private uint curEnhanceRawRate;
    private uint curRateConverted;

    // 레벨업 가능한 레벨인지 
    private E_LevelUpType levelUpType;

    private uint curMinRawRate;
    private uint curMinRateConverted;

    // private AttributeEnhanceSimpleRateInfo curEnhanceCostInfo;
    private List<uint> costInfo_itemID;
    private List<uint> costInfo_itemCnt;

    private uint enhanceLimitRawRate;

    /// <summary>
    /// 현재 강화 속성 단계의 단계 조절 단위 ( 50000 이면 5% 단위로 조정 )
    /// </summary>
    private uint adjustRateUnit_increase;
    private uint adjustRateUnit_decrease;

    private bool disableFeature;
    private bool reachedCurLimitLevel;

    private bool operatingEnhance;
    private bool isInitialized;
    #endregion

    #region Properties 
    #endregion

    #region Public Methods

    public void Initialize(UIEnhanceElement enhanceController)
    {
        if (isInitialized)
            return;

        EnhanceController = enhanceController;
        ///adjustRateRawAmount = DBAttribute.ConvertEnhanceActualRateToRawRate(adjustRateAmount);
        costItems = new List<UIEnhanceElementCostItem>();
        costInfo_itemID = new List<uint>();
        costInfo_itemCnt = new List<uint>();

        isInitialized = true;
    }

    public void Release()
    {
        operatingEnhance = false;
    }

    public void SetEnhanceAttributeID(uint attributeID, bool resetRate, E_UnitAttributeType type)
    {
        selectedAttributeID = attributeID;
        selectedType = type;

        Attribute_Table data;

        // 해당 data 가 없으면 그냥 비활성화함 
        if (DBAttribute.GetAttributeByID(attributeID, out data) == false)
        {
            disableFeature = true;
            // ZLog.LogError(ZLogChannel.UI, " Attribute ID : " + attributeID + " , No imple : 해당 속성 레벨 0 일때는 cost 정보가 없음. 어떻게해야할지 기획컨펌필요함 ");
            UpdateUI();
            return;
        }

        disableFeature = false;

        enhanceLimitRawRate = DBAttribute.GetMaxAttributeEnhanceableRawRate(attributeID);
        selectedAttributeLevel = data.AttributeLevel;

        levelUpType = data.LevelUpType;

        reachedCurLimitLevel = selectedAttributeLevel == DBAttribute.GetAttributeReachableMaxLevelAtCurrentChainLevel(Me.CurCharData.GetAttributeChainEffectLevel());

        uint newRawRate = curEnhanceRawRate;

        // 리셋하면 현재 확률 기본 확률로 리셋함 
        if (resetRate)
        {
            newRawRate = data.LevelUpRate;
        }
        else // 리셋이 아닌 확률 보존 
        {
            // 기존 확률이 새로 바뀐 attribute 의 한계 확률을 넘어가면 
            // 한계치로 설정 
            if (curEnhanceRawRate >= enhanceLimitRawRate)
            {
                newRawRate = enhanceLimitRawRate;
            }
            // 기존 확률이 새로 바뀐 attribute 의 최소 확률보다 낮으면 
            // 기본 확률로 설정 
            else if (curEnhanceRawRate < enhanceLimitRawRate)
            {
                newRawRate = data.LevelUpRate;
            }
        }

        SetRate(newRawRate);
    }

    public void RefreshCurrentData()
    {
        var curAttributeID = Me.CurCharData.GetAttributeIDByType(selectedType);
        SetEnhanceAttributeID(curAttributeID, true, selectedType);
    }
    #endregion

    #region Private Methods


    private void UpdateUI()
    {
        UpdateUI_Btns();
        UpdateUI_Text();
        UpdateUI_CostItems();
    }

    private void UpdateUI_Btns()
    {
        // reachedCurLimitLevel == false 는 현재 도달가능한 최대 레벨에 도달했는지 체킹 코드 

        bool isActiveFeature = levelUpType == E_LevelUpType.Up && disableFeature == false;
        bool reachedMaxRate = curEnhanceRawRate >= enhanceLimitRawRate ||
            curEnhanceRawRate >= DBAttribute.ConvertEnhanceActualRateToRawRate(100);

        btnDecrease.interactable = isActiveFeature && curEnhanceRawRate > curMinRawRate && operatingEnhance == false;
        btnIncrease.interactable = isActiveFeature && reachedMaxRate == false && operatingEnhance == false;
        btnMax.interactable = isActiveFeature && reachedMaxRate == false && operatingEnhance == false;
       // btnEnhance.interactable = isActiveFeature;
    }

    private void UpdateUI_Text()
    {
        string rate = disableFeature ? "0" : curRateConverted.ToString();
        txtRate.text = rate;
    }

    private void UpdateUI_CostItems()
    {
        if (disableFeature)
        {
            if (costItems != null)
                costItems.ForEach(t => t.gameObject?.SetActive(false));
        }
        else
        {
            uint requiredCostItemCnt = (uint)costInfo_itemID.Count;

            // 나중에 요구 아이템 개수가 달라질수있으니 체킹후 필요한 만큼 생성함. 
            if (costItems.Count < requiredCostItemCnt)
            {
                int costItemCntNeeded = (int)requiredCostItemCnt - costItems.Count;

                for (int i = 0; i < costItemCntNeeded; i++)
                {
                    AddCostItem();
                }

                costItems.ForEach(t => t.gameObject.SetActive(false));
            }

            if (costInfo_itemID.Count ==
                costInfo_itemCnt.Count)
            {
                for (int i = 0; i < costInfo_itemID.Count; i++)
                {
                    Item_Table itemData;

                    if (DBItem.GetItem(costInfo_itemID[i], out itemData))
                    {
                        costItems[i].Set(ZManagerUIPreset.Instance.GetSprite(itemData.IconID), costInfo_itemCnt[i]);
                        costItems[i].gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                ZLog.LogError(ZLogChannel.UI, "테이블 데이터 설정 잘못돼있음 . 필요 아이템 종류와 개수가 서로 일치하지않음 ");
            }
        }
    }

    private UIEnhanceElementCostItem AddCostItem()
    {
        var obj = Instantiate(costItemSourceObj, costItemParent);
        obj.gameObject.SetActive(false);
        costItems.Add(obj);
        return obj;
    }

    // 확률을 세팅합니다 . 
    private void SetRate(uint rawRate)
    {
        if (rawRate > enhanceLimitRawRate)
        {
            rawRate = enhanceLimitRawRate;
        }

        curEnhanceRawRate = rawRate;
        curRateConverted = DBAttribute.ConvertEnhanceRawRateToActualRate(rawRate);
        curMinRawRate = DBAttribute.GetAttributeMinRawRate(selectedAttributeID);
        curMinRateConverted = DBAttribute.ConvertEnhanceRawRateToActualRate(curMinRawRate);

        // DBAttribute.GetAttributeEnhanceInfoAtSpecificRate(selectedAttributeID, curEnhanceRawRate, ref curEnhanceCostInfo);
        DBAttribute.GetSequentialDesiredRawRateInfoFromMinRate(selectedAttributeID, rawRate, costInfo_itemID, costInfo_itemCnt); //  DBAttribute.GetAttributeEnhanceCost(selectedAttributeID, rawRate);
        //curEnhanceRawRate = curEnhanceCostInfo.LevelUpMaxRawRate;

        AttributeEnhanceSimpleRateInfo dummy = default(AttributeEnhanceSimpleRateInfo);
        DBAttribute.GetAttributeEnhanceInfoAtSpecificRate(selectedAttributeID, rawRate + 1, ref dummy);
        adjustRateUnit_increase = dummy.AdditionalRateUnit;
        DBAttribute.GetAttributeEnhanceInfoAtSpecificRate(selectedAttributeID, rawRate > 0 ? rawRate - 1 : 0, ref dummy);
        adjustRateUnit_decrease = dummy.AdditionalRateUnit;

        UpdateUI();
    }

    //private void AdjustEnhanceRate(int rawAmount)
    //{
    //    int virtualRate = (int)curEnhanceRawRate;

    //    if (virtualRate + rawAmount < 0)
    //    {
    //        virtualRate = 0;
    //    }
    //    else if (virtualRate + rawAmount > rawMaxRate)
    //    {
    //        virtualRate = (int)rawMaxRate;
    //    }
    //    else
    //    {
    //        virtualRate += rawAmount;
    //    }

    //    UpdateInfo((uint)virtualRate);
    //}

    //private bool CheckCurrencyEnough(AttributeEnhanceSimpleRateInfo info)
    //{
    //    if (info.LevelUpItem.Count == 0 ||
    //        info.LevelUpItemCnt.Count == 0)
    //        return false;

    //    bool result = true;

    //    for (int i = 0; i < info.LevelUpItem.Count; i++)
    //    {
    //        ulong cnt = ZNet.Data.Me.GetCurrency(info.LevelUpItem[i]);

    //        if (cnt < info.LevelUpItemCnt[i])
    //        {
    //            result = false;
    //            break;
    //        }
    //    }

    //    return result;
    //}

    private uint ClampUint(uint current, int t, uint min, uint max)
    {
        uint result = 0;

        if (current + t <= min)
            result = min;
        else if (current + t >= max)
            result = max;
        else result = current + (uint)t;

        return result;
    }
    #endregion

    #region OnClick Event
    public void OnClickRateDecreaseBtn()
    {
        uint virtualRate = ClampUint(curEnhanceRawRate, (int)adjustRateUnit_decrease * -1, curMinRawRate, enhanceLimitRawRate);
        SetRate(virtualRate);
    }

    public void OnClickRateIncreaseBtn()
    {
        uint virtualRate = ClampUint(curEnhanceRawRate, (int)adjustRateUnit_increase, curMinRawRate, enhanceLimitRawRate);
        SetRate(virtualRate);
    }

    public void OnClickMaxBtn()
    {
        SetRate(enhanceLimitRawRate);
    }

    public void OnClickEnhanceBtn()
    {
        //if (UIEnhanceElement.ignore)
        //{
        //    return;
        //}

        if (operatingEnhance)
        {
            ZLog.Log(ZLogChannel.UI, "Enhancing..");
            return;
        }

        /// 재화체킹 
        for (int i = 0; i < costInfo_itemID.Count; i++)
        {
            if (ConditionHelper.CheckCompareCost(costInfo_itemID[i], costInfo_itemCnt[i]) == false)
            {
                return;
            }
        }

        /// 모든 속성이 끝까지 도달했는지 체킹 
        if (Me.CurCharData.IsAttributeAllMaxLevel())
        {
            OpenErrorPopUp(ERROR.ATTRIBUTE_NO_MORE_ENCHANT);
            return;
        }
        /// 해당 속성은 이미 끝 레벨에 도달하였음 
        else if (levelUpType == E_LevelUpType.End)
        {
            OpenErrorPopUp(ERROR.ATTRIBUTE_NO_MORE_ENCHANT);
            return;
        }
        /// 현재 체인에서 도달 가능한 레벨에 도달, 다른 레벨들을 올려야함 . 
        else if (Me.CurCharData.CanEnhanceAttributeByChainCondition(selectedType) == false)
        {
            OpenNotiUp(DBLocale.GetText("Attribute_Chain_Alert_Text"));
            return;
        }

        operatingEnhance = true;
        UpdateUI_Btns();

        // uint preChainLevel = Me.CurCharData.GetAttributeChainEffectLevel();

        // UIEnhanceElement.ignore = true;
        uint nextTargetTid = 0;
        DBAttribute.GetNextAttributeID(selectedAttributeID, out nextTargetTid);

        EnhanceController.AdvanceTryEnhance(
            selectedType
            , nextTargetTid
            , DBAttribute.GetSequentialDesiredRawRateInfoFromMinRate(selectedAttributeID, curEnhanceRawRate)
            , OnEnhanceProcessFinished);

        //ZWebManager.Instance.WebGame.REQ_EnhanceAttribute(
        //    selectedAttributeID
        //    /// ** 해당 Rate 를 몇번 Add 해야 현재 CurEnhanceRawRate 가 되는건지를 계산을 해서 ㅡㅡ 보내야함 ** 
        //    , DBAttribute.GetSequentialDesiredRawRateInfoFromMinRate(selectedAttributeID, curEnhanceRawRate) // curRateConverted - curMinRateConverted
        //    , _onReceive:
        //    (revPacket, resList) =>
        //    {
        //        // string comment = string.Format("강화에 {0}하였습니다.", resList.IsSuccess ? "성공" : "실패");
        //        //OpenNotiPopup(comment);
        //        StartCoroutine(ShowResult(EnhanceController.GetCurrentSelectedElementPos(), resList.IsSuccess, preChainLevel, selectedAttributeID));
        //        operatingEnhance = false;
        //    },
        //    (err, req, res) =>
        //    {
        //        operatingEnhance = false;
        //        ZWebManager.Instance.ProcessErrorPacket(err, req, res, false);
        //    });
    }

    void OnEnhanceProcessFinished()
    {
        operatingEnhance = false;
        UpdateUI();
    }

    void OpenErrorPopUp(ERROR errorCode)
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(
                "알림"
                , DBLocale.GetText(errorCode.ToString())
                , new string[] { ZUIString.LOCALE_OK_BUTTON }
                , new Action[] { delegate { _popup.Close(); } });
        });
    }

    public void OpenNotiUp(string content, string title = "확인", Action onConfirmed = null)
    {
        DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);

        if (table != null)
        {
            title = DBLocale.GetText(table.Text);
        }

        //if (string.IsNullOrEmpty(title))
        //{
        //	title = ZUIString.ERROR;
        //}

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { title }, new Action[] { () =>
                {
                    onConfirmed?.Invoke();
                    _popup.Close();
                }});
        });
    }
}
#endregion