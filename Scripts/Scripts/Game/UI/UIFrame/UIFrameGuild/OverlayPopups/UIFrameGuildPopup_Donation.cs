using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

public class UIFrameGuildPopup_Donation : UIFrameGuildOverlayPopupBase
{
    [Serializable]
    public class SingleDonationObject
    {
        public E_GuildDonationType type;
        public Button btnDonate;
        public Image imgDonationItem;
        public Text txtDonationItemCnt;
        public Image imgReward01;
        public Text txtReward01;
        public Image imgReward02;
        public Text txtReward02;

        [HideInInspector] public uint donationItemTid;
        [HideInInspector] public uint donationItemCnt;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private List<SingleDonationObject> donationObjs;
    [SerializeField] private Text txtRemainedCnt;
    #endregion
    #endregion

    #region System Variables
    private uint availableCntRemained;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildController)
    {
        base.Initialize(guildController);

        donationObjs.ForEach(t => t.btnDonate.onClick.AddListener(() => HandleButtonClicked(t.type)));

        var target = donationObjs.Find(t => t.type == E_GuildDonationType.SMALL);
        target.donationItemTid = DBConfig.Gold_ID;
        target.donationItemCnt = DBConfig.Guild_GoldGive_Count;

        SetDonationInfo(
            target
            , target.donationItemTid
            , target.donationItemCnt
            , DBConfig.Guild_GoldGive_RewardID
            , DBConfig.Guild_GoldGive_RewardCount
            , DBConfig.Exp_Item
            , DBConfig.Guild_GoldGive_Exp);

        target = donationObjs.Find(t => t.type == E_GuildDonationType.MEDIUM);
        target.donationItemTid = DBConfig.Diamond_ID;
        target.donationItemCnt = DBConfig.Guild_DiamondGiveSmall_Count;

        SetDonationInfo(
            target
            , target.donationItemTid
            , target.donationItemCnt
            , DBConfig.Guild_DiamondGiveSmall_RewardID
            , DBConfig.Guild_DiamondGiveSmall_RewardCount
            , DBConfig.Exp_Item
            , DBConfig.Guild_DiamondGiveSmall_Exp);

        target = donationObjs.Find(t => t.type == E_GuildDonationType.LARGE);
        target.donationItemTid = DBConfig.Diamond_ID;
        target.donationItemCnt = DBConfig.Guild_DiamondGiveBig_Count;

        SetDonationInfo(
            target
            , target.donationItemTid
            , target.donationItemCnt
            , DBConfig.Guild_DiamondGiveBig_RewardID
            , DBConfig.Guild_DiamondGiveBig_RewardCount
            , DBConfig.Exp_Item
            , DBConfig.Guild_DiamondGiveBig_Exp);
    }

    public override void Open()
    {
        base.Open();
        UpdateUI();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void UpdateUI()
    {
        if (DBConfig.Guild_Give_Limit >= UIFrameGuildNetCapturer.MyGuildData.MyMemberInfo.donateCnt)
        {
            availableCntRemained = DBConfig.Guild_Give_Limit - UIFrameGuildNetCapturer.MyGuildData.MyMemberInfo.donateCnt;
        }
        else
        {
            availableCntRemained = 0;
        }

        txtRemainedCnt.text = string.Format("{0}회", availableCntRemained);

        donationObjs.ForEach(t => t.btnDonate.interactable = availableCntRemained > 0);
    }

    private void SetDonationInfo(
        SingleDonationObject target
        , uint donationItemID
        , uint donationItemCnt
        , uint rewardId01
        , uint rewardCnt01
        , uint rewardId02
        , uint rewardCnt02)
    {
        target.imgDonationItem.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(donationItemID));
        target.txtDonationItemCnt.text = donationItemCnt.ToString("n0");
        target.imgReward01.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(rewardId01));
        target.imgReward01.SetNativeSize();
        target.txtReward01.text = rewardCnt01.ToString("n0");
        target.imgReward02.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(rewardId02));
        target.imgReward02.SetNativeSize();

        if (rewardCnt02 >= 1000)
        {
            target.txtReward02.text = string.Format("{0}K", (rewardCnt02 / 1000));
        }
        else
        {
            target.txtReward02.text = rewardCnt02.ToString("n0");
        }
    }

    private void HandleButtonClicked(E_GuildDonationType donationType)
    {
        var target = donationObjs.Find(t => t.type == donationType);
        var item = Me.CurCharData.GetInvenItemUsingMaterial(target.donationItemTid);

        if (ZNet.Data.Me.GetCurrency(target.donationItemTid) < target.donationItemCnt)
        {
            UIFrameGuildTabBase.OpenNotiUp("기부 재화가 부족합니다", "알림");
            return;
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open("확인", "정말 기부하시겠습니까?", new string[] { "취소", "확인" }, new Action[] {
                () =>
                {
                    _popup.Close();
                },
                () =>
                {
                    UIFrameGuildNetCapturer.ReqGuildDonation(
                        Me.CurCharData.GuildId
                        , donationType
                        , item != null ? item.item_id : 0
                        , (revPacketRec, resListRec) =>
                        {
                            var obtainedItem = resListRec.GetItem.Value;
                            var obtainedGuildExp = resListRec.GetGuildExp;

                            var displayInfo = new UIFrameGuildTab_DisplayReward.EventParam_DisplayInfo();

                            displayInfo.strTitle = string.Format("길드 기부에 참여하여 공헌도 {0} 및 아이템 보상을 획득하였습니다.", resListRec.GetGuildExp);
                            displayInfo.items = new UIFrameGuildTab_DisplayReward.DisplaySingleItemInfo[]
                            {
                                new UIFrameGuildTab_DisplayReward.DisplaySingleItemInfo() { iconSprite = target.imgReward01.sprite,  strCnt = target.txtReward01.text},
                                new UIFrameGuildTab_DisplayReward.DisplaySingleItemInfo() { iconSprite = target.imgReward02.sprite, strCnt = target.txtReward02.text}
                            };

                            UpdateUI();
                            GuildController.NotifyUpdateEvent(UpdateEventType.DataRefreshed_GuildInfoAndMemberInfo);
                            GuildController.NotifyUpdateEvent(UpdateEventType.ObtainedGuildReward, displayInfo);
                        }, UIFrameGuildTabBase.HandleError);

                    _popup.Close();
                }});
        });
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
