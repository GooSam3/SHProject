using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemEnchant : ZUIFrameBase
{
    #region UI Variable
    /// <summary>MatarialSlot : 마부에 필요한 재료, ItemlSlot : 마부 아이템(자료형이 GameObject 지만 추후에 변경 해야할것같음..)</summary>
    [SerializeField] private Image MatarialSlot, ItemlSlot;
    [SerializeField] private Text ItemText;
    [SerializeField] private List<UIEnchantAbilitySlot> BeforeEnchantingSliders = new List<UIEnchantAbilitySlot>();
    [SerializeField] private List<UIEnchantAbilitySlot> AfterEnchantingSliders = new List<UIEnchantAbilitySlot>();
    [SerializeField] private Text EnchantCost;
    [SerializeField] private Text MatarialSlotName, MatarialSlotCost;
    [SerializeField] private GameObject ItemEnchentOption;
    [SerializeField] private Text ResmeltingOptionName, ResmeltingOptionValue, ResmeltingOptionCount;
    [SerializeField] private GameObject BeforeText, BeforeOption, AfterText, AfterOption;
    [SerializeField] private GameObject EnchantButtons, SaveButtons;
    [SerializeField] private ZButton EnchantButton, SaveButton;
    [SerializeField] private Image GradeBoard;
    [SerializeField] private Text GradeText;
    #endregion

    #region System Variable
    [SerializeField] private ZItem Material = null;
    public ZItem Item { get; private set; } = null;
    /// <summary>마법 부여 아이템 옵션 정보를 담는 리스트</summary>
    private List<uint> AfterEnchantingIdList = new List<uint>();
    public override bool IsBackable => true;
    #endregion

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.Open<UIFrameInventory>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
    }

    protected override void OnHide()
    {
        base.OnHide();

        ClearData();

        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.All);
        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.All);
    }

    private void ClearData()
    {
        Item = null;
        Material = null;
        ItemlSlot.gameObject.SetActive(false);
        MatarialSlot.gameObject.SetActive(false);
        BeforeText.SetActive(false);
        BeforeOption.SetActive(false);
        AfterText.SetActive(false);
        AfterOption.SetActive(false);
        GradeBoard.gameObject.SetActive(false);

        ItemlSlot.sprite = null;
        GradeBoard.sprite = null;
        GradeText.text = String.Empty;

        ItemText.text = string.Empty;
        EnchantCost.text = string.Empty;
        MatarialSlotName.text = string.Empty;
        MatarialSlotCost.text = string.Empty;

        ResmeltingOptionName.text = string.Empty;
        ResmeltingOptionValue.text = string.Empty;
        ResmeltingOptionCount.text = string.Empty;

        EnchantButton.interactable = false;
        SaveButton.interactable = false;

        AfterEnchantingIdList.Clear();
    }

    public void Set(ZItem _item)
    {
        if (_item == null)
            return;

        Item = _item;
        ItemlSlot.gameObject.SetActive(Item != null);
        ItemlSlot.sprite = UICommon.GetItemIconSprite(Item.item_tid);
        GradeBoard.sprite = UICommon.GetItemGradeSprite(Item.item_tid);
        GradeBoard.gameObject.SetActive(true);

        bool isEnchanted = DBItem.GetItem(Item.item_tid).Step > 0;
        GradeText.gameObject.SetActive(isEnchanted);
        if (isEnchanted)
            GradeText.text = string.Format("+{0}", DBItem.GetItem(Item.item_tid).Step);

        BeforeText.SetActive(false);
        BeforeOption.SetActive(false);
        AfterText.SetActive(false);
        AfterOption.SetActive(false);

        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Enchant);
        if(UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.Enchant);

        UpdateBeforeEnchantingSlider();
        UpdateAfterEnchantingSlider();
        GetEnchantInfo();
        UpdateButton();
    }

    public void SetMaterial(ZItem _item)
	{
		if (_item == null)
			return;

		Material = _item;
        MatarialSlot.gameObject.SetActive(Material != null);
        MatarialSlot.sprite = UICommon.GetItemIconSprite(Material.item_tid);

        bool isEnchanted = Material == null;

        UpdateCost();
		EnchentOptionPopupSetting();
		UpdateButton();
	}

    public void ClickEquipment()
    {
        ClearData();
        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.EnchantEquip);
        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.EnchantEquip);
    }

    private void UpdateButton()
	{
        EnchantButtons.SetActive(AfterEnchantingIdList.Count == 0);
        SaveButtons.SetActive(AfterEnchantingIdList.Count != 0);
        EnchantButton.interactable = IsResmelting();
        SaveButton.interactable = IsChanged();
    }

    /// <summary>아이템 강화 비용 셋팅</summary>
    public void UpdateCost()
    {
        EnchantCost.text = string.Empty;

        if (Item == null || Material == null)
            return;

        var scroll = DBResmelting.GetResmeltingScroll(Material.item_tid, DBItem.GetItemType(Item.item_tid));

        // 스크롤 필요 개수 셋팅
        for (int i = 0; i < scroll.SmeltItemID.Count; ++i)
        {
            var id = scroll.SmeltItemID[i];
            if (Material.item_tid != id)
                continue;

            var cost = scroll.SmeltItemCnt[i];
            var amount = Me.CurCharData.GetInvenCntUsingMaterial(id);

            MatarialSlotName.text = String.Format("{0}", DBLocale.GetText(DBItem.GetItem(Material.item_tid).ItemTextID));
            MatarialSlotCost.text = String.Format("{0}/{1}", Material.cnt , cost);
        }

        //재화(Gold) 필요 개수 셋팅
        for (int i = 0; i < scroll.SmeltMaterialItemID.Count; ++i)
        {
            var id = scroll.SmeltMaterialItemID[i];
            var cost = scroll.SmeltMaterialItemCnt[i];
            var amount = Me.CurCharData.GetInvenCntUsingMaterial(id);

            EnchantCost.text = String.Format("{0}", cost);
        }
    }

   
    private void UpdateBeforeEnchantingSlider()
    {
        if (Item.item_tid != 0)
        {
            ItemText.text = DBLocale.GetItemLocale(DBItem.GetItem(Item.item_tid));

            for (int i = 0; i < BeforeEnchantingSliders.Count; i++)
            {
                if (Item.Options.Count > i)
                    BeforeEnchantingSliders[i].Set(Item.Options[i]);
                else
                    BeforeEnchantingSliders[i].ResetUI();
            }

            if (Item.ResmeltOptionId_01 == 0 && Item.ResmeltOptionId_02 == 0 && Item.ResmeltOptionId_03 == 0)
            {
                BeforeText.SetActive(true);
                BeforeOption.SetActive(false);
            }
            else
            {
                BeforeText.SetActive(false);
                BeforeOption.SetActive(true);
            }
        }
        else
        {
            ItemText.text = string.Empty;
            foreach (var slot in BeforeEnchantingSliders)
                slot.ResetUI();

            BeforeText.SetActive(true);
            BeforeOption.SetActive(false);
        }
    }


    private void UpdateAfterEnchantingSlider()
    {
        AfterText.SetActive(true);
        AfterOption.SetActive(false);

        for (int i = 0; i < AfterEnchantingSliders.Count; i++)
        {
            if (AfterEnchantingIdList.Count > i)
            {
                AfterEnchantingSliders[i].Set(AfterEnchantingIdList[i]);
                AfterText.SetActive(false);
                AfterOption.SetActive(true);
            }
            else
                AfterEnchantingSliders[i].ResetUI();
        }
    }

    /// <summary> 변경된 재련 정보 저장 </summary>
    private void SetAfterEnchantingIds(List<uint> _reEnchantingIdList)
    {
        AfterEnchantingIdList.Clear();
        if (_reEnchantingIdList != null)
            AfterEnchantingIdList.AddRange(_reEnchantingIdList);
    }

    /// <summary> 재련을 진행했는지 여부 </summary>
    private bool IsChanged()
    {
        for (int i = 0; i < AfterEnchantingIdList.Count; i++)
            return 0 < AfterEnchantingIdList[i];
        return false;
    }

    /// <summary> 재련 가능 여부. 재화 체크 </summary>
    private bool IsResmelting()
    {
        if (Item == null || Material == null)
            return false;

        var scroll = DBResmelting.GetResmeltingScroll(Material.item_tid, DBItem.GetItemType(Item.item_tid));
        if (!Me.CurCharData.CheckCountInvenItemUsingMaterial(DBConfig.Gold_ID, scroll.SmeltMaterialItemCnt[0]))
            return false;

        return true;
    }

    /// <summary>마부 버튼 콜백 함수.</summary>
    public void ClickEnchantItem()
    {
        ZWebManager.Instance.WebGame.REQ_ItemResmeltStart(Item.item_id, Item.item_tid, Material.item_tid, (recvPacket, recvMsgPacket) => {
            GetEnchantInfo();
            //SaveButton.interactable = true;
        });
    }

    public void ClickEnchantISave()
	{
        //if (false == IsChanged())
        //    return;

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("능력치를 저장 하시겠습니까?"),
            new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
            new Action[] {
                delegate { _popup?.Close(); },
                delegate
                {
                    ZWebManager.Instance.WebGame.REQ_ItemResmeltEnd(Item.item_id, Item.item_tid, (recvPacket, recvMsgPacket) => 
                    {
                        if(0 > Item.item_id)
                            return;

                        _popup?.Close();
                        UpdateBeforeEnchantingSlider();
                        SetAfterEnchantingIds(null);
                        UpdateAfterEnchantingSlider();
                        UpdateCost();
                        UpdateButton();
                        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Enchant);
                        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
                            _inventory.ShowInvenSort((int)E_InvenSortType.Enchant);
                    });
                }
            });
        });
    }

    public void OnClickItemEnchentOption()
    {
        if(ItemEnchentOption.activeSelf)
            ItemEnchentOption.SetActive(false);
        else
            ItemEnchentOption.SetActive(true);
    }

    public void OnClickCancelButton()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("Alchemy_Storage_Message"),
            new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
            new Action[] {
                delegate { _popup?.Close(); },
                delegate
                {
                    NotSaveEnchant();
                    _popup?.Close();
                    UpdateBeforeEnchantingSlider();
                    SetAfterEnchantingIds(null);
                    UpdateAfterEnchantingSlider();
                    UpdateCost();
                    UpdateButton();
                    //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Enchant);
                    if (UIManager.Instance.Find(out UIFrameInventory _inventory))
                        _inventory.ShowInvenSort((int)E_InvenSortType.Enchant);
                }
            });
        });
    }

    private void EnchentOptionPopupSetting()
    {
        if (0 >= Item.item_tid || 0 >= Material.item_tid)
        {
            ItemEnchentOption.SetActive(false);
            return;
        }

        var scroll = DBResmelting.GetResmeltingScroll(Material.item_tid, DBItem.GetItemType(Item.item_tid));

        if (null == scroll)
        {
            ItemEnchentOption.SetActive(false);
            return;
        }

        GameDB.E_ItemType smeltScrollType = scroll.SmeltScrollType;
        uint optionGroupId = scroll.SmeltOptionRateGroupID;

        int minOptionCount = 0;
        int maxOptionCount = 0;

        for (int i = 0; i < scroll.AddOptionRate.Count; ++i)
        {
            if (0 >= scroll.AddOptionRate[i])
                continue;

            if (0 == minOptionCount)
                minOptionCount = i + 1;

            maxOptionCount = i + 1;
        }

        if (minOptionCount != maxOptionCount)
            ResmeltingOptionCount.text = $"부여시 무작위로 {minOptionCount}~{maxOptionCount}개의 옵션이 추가 됩니다.";
        else
            ResmeltingOptionCount.text = $"부여시 무작위로 {minOptionCount}개의 옵션이 추가 됩니다.";

        //옵션 확률 테이블을 얻어온다.
        var optionRateTables = DBResmelting.GetVaildResmeltingOptionRateData(optionGroupId);

        string resmeltingInfoName = "";
        string resmeltingInfoValue = "";

        foreach (var table in optionRateTables)
        {
            uint abilityId = 0;
            float minValue = float.MaxValue;
            float maxValue = 0;

            string abilityName = "";

            var options = DBResmelting.GetScrollOptionByGroup(table.SmeltScrollOptionGroupID);

            foreach (var option in options)
            {
                switch (smeltScrollType)
                {
                    case GameDB.E_ItemType.LowSmeltScroll:
                        {
                            if (0 >= option.LowSmeltScrollGetRate)
                                continue;
                        }
                        break;
                    case GameDB.E_ItemType.MidSmeltScroll:
                        {
                            if (0 >= option.MidSmeltScrollGetRate)
                                continue;
                        }
                        break;
                    case GameDB.E_ItemType.HighSmeltScroll:
                        {
                            if (0 >= option.HighSmeltScrollGetRate)
                                continue;
                        }
                        break;
                }

                var abilityActionTable = DBAbility.GetAction(option.AbilityActionID);

                if (string.IsNullOrEmpty(abilityName))
                {
                    abilityName = DBLocale.GetText(DBAbility.GetAbility(abilityActionTable.AbilityID_01).StringName);
                }

                abilityId = (uint)abilityActionTable.AbilityID_01;
                minValue = Mathf.Min(minValue, abilityActionTable.AbilityPoint_01_Min);
                maxValue = Mathf.Max(maxValue, abilityActionTable.AbilityPoint_01_Min);
            }

            string minString = DBAbility.ParseAbilityValue(abilityId, minValue);
            string maxString = DBAbility.ParseAbilityValue(abilityId, maxValue);

            minString = minString.Replace("+", "");
            maxString = maxString.Replace("+", "");

            if (string.IsNullOrEmpty(resmeltingInfoName))
                resmeltingInfoName = $"{abilityName}";
            else
                resmeltingInfoName = $"{resmeltingInfoName}\n{abilityName}";

            if (string.IsNullOrEmpty(resmeltingInfoValue))
            {
                if (false == minString.Equals(maxString))
                    resmeltingInfoValue = $"{minString}~{maxString}";
                else
                    resmeltingInfoValue = $"{minString}";
            }
            else
            {
                if (false == minString.Equals(maxString))
                    resmeltingInfoValue = $"{resmeltingInfoValue}\n{minString}~{maxString}";
                else
                    resmeltingInfoValue = $"{resmeltingInfoValue}\n{minString}";
            }
        }

        ResmeltingOptionName.text = resmeltingInfoName;
        ResmeltingOptionValue.text = resmeltingInfoValue;
    }

    private void GetEnchantInfo()
	{
		ZWebManager.Instance.WebGame.REQ_GetItemResmeltInfo((recvPacket, /*onError*/res) =>
		{
            if (Item.item_id != res.SmeltItemId || 0 >= res.SmeltItemId)
                return;

            List<uint> optionList = new List<uint>();
            for (int i = 0; i < res.OptionsLength; i++)
                optionList.Add(res.Options(i));

            SetAfterEnchantingIds(optionList);
            UpdateAfterEnchantingSlider();
            UpdateCost();
            UpdateButton();
        });
	}

    private void NotSaveEnchant()
	{
        ZWebManager.Instance.WebGame.REQ_ItemResmeltCancel((recvPacket, onError) =>
        {
            SetAfterEnchantingIds(null);
        });
    }

    /// <summary>팝업 close</summary>
    public void Close()
    {
        if (false == IsChanged())
        {
            ItemEnchentOption.SetActive(false);
            UIManager.Instance.Close<UIFrameItemEnchant>();
        }
        else
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("Upgrade_Save_Text"),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] {
                delegate { _popup?.Close(); }
                });
            });
        }
    }
}
