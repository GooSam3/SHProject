using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using ZNet;
using ZNet.Data;
using WebNet;
using GameDB;

public class SpecialShopArchiveItemSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtName;
    [SerializeField] private GameObject diamondRoot;
    [SerializeField] private Text txtDiamondCnt;
    #endregion
    #endregion

    #region System Variables
    private Action _onClickedReceive;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetUI(SpecialShopArchiveListItemModel data)
    {
        var specialShopData = DBSpecialShop.Get(data.specialShopTid);

        if (specialShopData == null)
        {
            ZLog.LogError(ZLogChannel.UI, "Target Special Shop Table Data from CashMail ShopTid From server Not Exist , Tid : " + data.specialShopTid);
            return;
        }

        uint dummyTargetTid = 0;
        E_CashType dummyCashType = E_CashType.None;
        DBSpecialShop.E_SpecialShopDisplayGoodsTarget dummyTargetGoodsType = DBSpecialShop.E_SpecialShopDisplayGoodsTarget.None;
        string spriteKey = string.Empty;
        string nameTextKey = string.Empty;
        uint diamondCnt = 0;
        byte grade = 0;

        DBSpecialShop.GetGoodsPropsBySwitching(data.specialShopTid, ref dummyCashType, ref dummyTargetGoodsType, ref nameTextKey, ref spriteKey, ref grade, ref dummyTargetTid);

        /// Set UI 
        this.imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(spriteKey);
        this.txtName.text = DBLocale.GetText(nameTextKey);

        bool diamondActive = diamondCnt > 0;
        this.diamondRoot.SetActive(diamondActive);
        if (diamondActive)
        {
            this.txtDiamondCnt.text = diamondCnt.ToString("n0");
        }
    }

    public void AddListener_OnClicked(Action callback)
    {
        _onClickedReceive += callback;
    }

    public void RemoveListener_OnClicked(Action callback)
    {
        _onClickedReceive -= callback;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClickReceive()
    {
        _onClickedReceive?.Invoke();
    }
    #endregion
}