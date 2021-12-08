using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIPopupItemInfo : MonoBehaviour
{
    [System.Serializable]
    public class ItemDescInfo
    {
        //public E_ItemInfoType Type;
        public string strTitle;
        public string strDesc;
        public string Value;
        public float cellSize;
    }

    #region UI Variable
    [SerializeField] private ItemInfoButton[] PopupInfoButtonList = new ItemInfoButton[ZUIConstant.ITEM_INFO_POPUP_BUTTON_COUNT];
    [SerializeField] private Image ItemIcon = null;
    [SerializeField] private GameObject Lock = null;
    [SerializeField] private Text EquipTxt = null;
    [SerializeField] private Text ItemName = null;
    [SerializeField] private Image CharClassImage = null;
    [SerializeField] private Text CharClass = null;
    [SerializeField] private GameObject ItemOptionGameObject = null;
    [SerializeField] private Text ItemOption = null;
    [SerializeField] private GameObject ItemToolTipGameObject = null;
    [SerializeField] private Text ItemToolTip = null;
    [SerializeField] private Image AbilityIcon = null;
    [SerializeField] private Text AbilityTitle = null;
    [SerializeField] private Text AbilityValue = null;
    [SerializeField] private GameObject Ability = null;
    [SerializeField] private GameObject Option = null;
    [SerializeField] private Text Belong = null;
	[SerializeField] private Image BelongLine = null;
    [SerializeField] private GameObject BelongIcon = null;

    [SerializeField] private GameObject TotalPay = null;
    [SerializeField] private Text ExchangePrice = null;
    // 리팩중
    //[SerializeField] private ZUIScrollItemEnhanceList ScrollList;
    [SerializeField] private List<UIInfoTextSlot> UIItemInfoTextSlotList = new List<UIInfoTextSlot>();
    [SerializeField] private List<UIInfoTextSlot> UIItemInfoDetailTextSlotList = new List<UIInfoTextSlot>();
    [SerializeField] private List<UIInfoTextSlot> UIItemEffectTextSlotList = new List<UIInfoTextSlot>();
    [SerializeField] private List<UIInfoTextSlot> UIItemOptionTextSlotList = new List<UIInfoTextSlot>();
    [SerializeField] private UIInfoTextSlot[] UIItemWeightTextSlot = null;
    // 리팩중

    [SerializeField] private GameObject AbilityLine = null;
    [SerializeField] private GameObject OptionLine = null;
    [SerializeField] private Text UseCharacterTypeTextPage1 = null;
    [SerializeField] private Text UseCharacterTypeTextPage2 = null;
    [SerializeField] private GameObject UseCharacterTypeTextMain = null;

    // ------- ljh : 창고 추가변수 --------
    // "요약보기(더보기x)"시 나올 툴팁
    // 0 : 등급 1 : 종류 2 : 설명
    [SerializeField] private List<UIInfoTextSlot> txtToolTip = new List<UIInfoTextSlot>();
    [SerializeField] private GameObject objSummery;
    // 더보기 모드에서 작동될 오브젝트들
    [SerializeField] private List<GameObject> listDetailObject = new List<GameObject>();

    // 팝업창 사이즈 변경용 recttransform
    [SerializeField] private RectTransform PopupRT;
    // "더보기" 버튼
    [SerializeField] private GameObject detailButton;
    // "더보기" 모드시 인풋 막는용도
    [SerializeField] private GameObject blockBackGround;

    [SerializeField] private ZImage GradeHeaderBG;
    #endregion

    #region System Variable
    [SerializeField] private List<ItemDescInfo> ItemDetailInfoList = new List<ItemDescInfo>();
    public ZItem Item = null;
    [SerializeField] private ExchangeItemData ExchangeItem = null;
    [SerializeField] private E_ItemPopupType Type;

    // ------- ljh : 창고 추가변수 ------
    // "더보기" 상태인가
    private bool isDetailMode = false;

    private Action onStorageClose;

    private const int SIZE_DEFAULT = 430;
    private const int SIZE_DETAIL = 800;

    private Action closeAction;
    #endregion

    #region ui 상세보기 분리 작업
    private Camera Camera;
    private Vector3 MousePosition;
    private Vector3 MouseDragPosition;
    private float distance;
    private bool dragCheck = false;

    private bool ItemDecTransCheck = false;
    private bool itemTypeCheck = false;

    [SerializeField] private GameObject ItemDescTypeA;
    [SerializeField] private ZImage IndicaterTypeA;
    [SerializeField] private GameObject ItemDescTypeB;
    [SerializeField] private ZImage IndicaterTypeB;
    [SerializeField] private List<CanvasGroup> fadeoutGrop = new List<CanvasGroup>();

    // 정보 그룹, 간략보기에서 끄기용
    [SerializeField] private GameObject infoGroup;
    // 중간 버튼 그룹
    [SerializeField] private GameObject MidButtonGroup;
    // 아래 버튼 그룹
    [SerializeField] private GameObject BotButtonGroup;

    private bool itemDescTypeCheck = false;

    private E_ItemPopupFlag midGroupFlag = E_ItemPopupFlag.Reforging | E_ItemPopupFlag.Delete | E_ItemPopupFlag.Info | E_ItemPopupFlag.Disassamble | E_ItemPopupFlag.Lock | E_ItemPopupFlag.Unlock;
    private E_ItemPopupFlag bottomGroupFlag = E_ItemPopupFlag.DownGrade | E_ItemPopupFlag.Reinforce | E_ItemPopupFlag.Upgrade | E_ItemPopupFlag.Use |
                                              E_ItemPopupFlag.Equip | E_ItemPopupFlag.Disarm | E_ItemPopupFlag.Collection | E_ItemPopupFlag.Sign | E_ItemPopupFlag.Exchange | E_ItemPopupFlag.GemEquip | E_ItemPopupFlag.GemUnEquip | E_ItemPopupFlag.ExchangeByMileage;
    #endregion

    /// <summary>
    /// 임시로 zitem만들어서 출력
    /// 단순 출력용이라 쓸데없는 데이터 초기화 안함
    /// </summary>
    /// <param name="_closeAction">닫을때 호출됨</param>
    public void Initialize(E_ItemPopupType _type, uint _itemTid, Action _closeAction=null, ulong _cnt = 1)
    {
        ZItem tempItem = new ZItem()
        {
            item_tid = _itemTid,
            cnt = _cnt,
            IsLock = false,
        };

        Initialize(_type, tempItem, _closeAction);

        RefreshButtonGroup();
    }

    public void Initialize(ExchangeItemData _exchangeItemData)
    {
        if (_exchangeItemData == null)
            return;

        ExchangeItem = _exchangeItemData;
		ExchangePrice.text = ExchangeItem.ItemTotalPrice.ToString();

        this.transform.localPosition = Vector3.zero;

        ZItem item = new ZItem()
        {
            item_id = _exchangeItemData.ItemID,
            item_tid = _exchangeItemData.ItemTId,
            cnt = _exchangeItemData.ItemCnt,
            expire_dt = _exchangeItemData.ExpireDt
        };

        switch (DBItem.GetItem(_exchangeItemData.ItemTId).ItemStackType)
        {
            case E_ItemStackType.Not:
                item.netType = NetItemType.TYPE_EQUIP;
                break;
            case E_ItemStackType.Stack:
                item.netType = NetItemType.TYPE_STACK;
                break;
            case E_ItemStackType.AccountStack:
                item.netType = NetItemType.TYPE_ACCOUNT_STACK;
                break;
        }

        Type = E_ItemPopupType.Exchange;
        Initialize(Type, item);
    }

    public void Initialize(E_ItemPopupType _type, ZDefine.ZItem _item, Action _closeAction = null)
    {
        transform.localScale = Vector2.one;

        Item = _item;
        Type = _type;
        closeAction = _closeAction;

        DBItem.GetItem(Item.item_tid, out Item_Table table);

        Ability.SetActive(table.AbilityActionID_01 != 0);
        if (table.AbilityActionID_01 != 0)
        {
            AbilityAction_Table abilityAction = DBAbilityAction.Get(table.AbilityActionID_01);

            if (abilityAction != null)
            {
                Ability_Table ability = DBAbility.GetAbility(abilityAction.AbilityID_01);

                if (ability != null && ability.AbilityIcon != null)
                {
                    AbilityIcon.sprite = ZManagerUIPreset.Instance.GetSprite(ability.AbilityIcon);
                    AbilityTitle.text = DBLocale.GetText(ability.StringName);
                    AbilityValue.text = DBAbilityAction.Get(table.AbilityActionID_01).AbilityPoint_01_Min.ToString();
                }
                else
                    Ability.SetActive(false);
            }
        }

        if (!Ability.activeSelf)
        {
            RectTransform rt = Option.GetComponent<RectTransform>();
            rt.offsetMax = new Vector2(rt.offsetMax.x, 0);
        }

        ItemIcon.sprite = UICommon.GetItemIconSprite(Item.item_tid);

        Lock.SetActive(Item.IsLock);
        EquipTxt.text = string.Empty;
        EquipTxt.gameObject.SetActive(Item.slot_idx != 0);
        ItemName.text = DBLocale.GetItemLocale(table);

        GradeHeaderBG.sprite = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(E_UIType.Header, table.Grade));

        Belong.text = table.BelongType == E_BelongType.Belong ? "귀속" : "비귀속";
        BelongIcon.SetActive(table.BelongType != E_BelongType.Belong);

        CharClassImage.sprite = UICommon.GetClassIconSprite(DBItem.GetItem(Item.item_tid).UseCharacterType, UICommon.E_SIZE_OPTION.Small);
        CharClass.text = table.UseCharacterType.ToString();

        SetItemDesc(); // 수정함수..
        SetitemText();
        SetPopup(_type);
        SetPosition(_type);

        TotalPay.SetActive(Type == E_ItemPopupType.Exchange);

        PopupInfoButtonList[(int)E_ItemPopupButton.Use].Button.interactable = Me.CurCharData.IsCharacterUsable(Item.item_tid);

        if (_type != E_ItemPopupType.Mailbox && 
            _type != E_ItemPopupType.Storage &&
            _type != E_ItemPopupType.Exchange &&
            _type != E_ItemPopupType.Reward &&
            _type != E_ItemPopupType.MileageShop &&
            _type != E_ItemPopupType.InfinityTower)
        {
            PopupInfoButtonList[(int)E_ItemPopupButton.Use].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_STACK && Type != E_ItemPopupType.Collection && Type != E_ItemPopupType.GemUnEquip && table.ItemUseType != E_ItemUseType.Ingredients && table.ItemUseType != E_ItemUseType.Material && table.ItemUseType != E_ItemUseType.Temple && table.ItemUseType != E_ItemUseType.Ticket);
            PopupInfoButtonList[(int)E_ItemPopupButton.Equip].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && Item.slot_idx == 0 && Type != E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.UnEquip].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && Item.slot_idx != 0 && Type != E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.Delete].Button.gameObject.SetActive(Item.netType != NetItemType.TYPE_ACCOUNT_STACK && Type != E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.Disassamble].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && Item.slot_idx == 0 && !Item.IsLock && Type != E_ItemPopupType.Collection && DBItem.GetItem(Item.item_tid).LimitType.HasFlag(E_LimitType.Break));
            PopupInfoButtonList[(int)E_ItemPopupButton.Enhance].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && Type != E_ItemPopupType.Collection && (table.EnchantUseType.HasFlag(E_EnchantUseType.BlessEnchant) || table.EnchantUseType.HasFlag(E_EnchantUseType.CurseEnchant) || table.EnchantUseType.HasFlag(E_EnchantUseType.NormalEnchant)) && table.Step < 9);
            PopupInfoButtonList[(int)E_ItemPopupButton.Upgrade].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && table.EnchantUseType.HasFlag(E_EnchantUseType.Upgrade) && Type != E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.Enchant].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && table.SmeltScrollUseType == E_SmeltScrollUseType.SmeltScroll && Type != E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.Collection].Button.gameObject.SetActive(Type == E_ItemPopupType.Collection);
            PopupInfoButtonList[(int)E_ItemPopupButton.GemEquip].Button.gameObject.SetActive(Type == E_ItemPopupType.GemEquip);
            PopupInfoButtonList[(int)E_ItemPopupButton.GemUnEquip].Button.gameObject.SetActive(Type == E_ItemPopupType.GemUnEquip);
            PopupInfoButtonList[(int)E_ItemPopupButton.Lock].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && !Item.IsLock);
            PopupInfoButtonList[(int)E_ItemPopupButton.UnLock].Button.gameObject.SetActive(Item.netType == NetItemType.TYPE_EQUIP && Item.IsLock);
        }

        PopupInfoButtonList[(int)E_ItemPopupButton.Exchange].Button.gameObject.SetActive(Type == E_ItemPopupType.Exchange);

        switch ( _type ) {
            case E_ItemPopupType.Mailbox:

                break;

            case E_ItemPopupType.InventoryEquipment:
            case E_ItemPopupType.CharacterStateEquip:
                if( Item.slot_idx != 0 )
                    EquipTxt.text = "장착중";

                break;

            case E_ItemPopupType.InventoryStack:

                break;

            case E_ItemPopupType.Storage:
                if( Item.slot_idx != 0 )
                    EquipTxt.text = "장착중";

                SetDetailMode( false );
                break;

            case E_ItemPopupType.Exchange:
               
                PopupInfoButtonList[ ( int )E_ItemPopupButton.Exchange ].Button.gameObject.SetActive( Type == E_ItemPopupType.Exchange);
                break;
            case E_ItemPopupType.MileageShop:
                {
                    PopupInfoButtonList[(int)E_ItemPopupButton.ExchangeByMileage].Button.gameObject.SetActive(Type == E_ItemPopupType.MileageShop);
                }
                break;

        }

        RefreshButtonGroup();
    }

    private void RefreshButtonGroup()
    {
        bool midState = false;
        bool botState = false;

        for(int i = 0;i<PopupInfoButtonList.Length;i++)
        {
            bool activeState = PopupInfoButtonList[i].Button.gameObject.activeSelf;
            E_ItemPopupFlag flag = PopupInfoButtonList[i].flag;

            if (midState == false && activeState == true)
                midState = midGroupFlag.HasFlag(flag);

            if (botState == false && activeState == true)
                botState = bottomGroupFlag.HasFlag(flag);
        }

        MidButtonGroup.SetActive(midState);
        BotButtonGroup.SetActive(botState);
    }

    public void Refresh()
    {
        Initialize(Type, Item);
    }

    public void Use()
    {
        if (DBItem.GetItem(Item.item_tid).ItemUseType == E_ItemUseType.Enchant)
        {
            UIFrameItemEnhance enhance = UIManager.Instance.Find<UIFrameItemEnhance>();

            if (enhance == null)
            {
                UIManager.Instance.Load(nameof(UIFrameItemEnhance), (_loadName, _loadFrame) => {
                    UIManager.Instance.Open<UIFrameItemEnhance>((_name, _frame) => {
                        _frame.SetMaterial(Item);
                    });
                });
            }
            else
            {
                UIManager.Instance.Open<UIFrameItemEnhance>();
                UIManager.Instance.Find<UIFrameItemEnhance>().SetMaterial(Item);
            }

            UIManager.Instance.Find<UIFrameInventory>().SetSelectObject(null);
            Destroy(gameObject);
            return;
        }

        if (DBItem.GetItem(Item.item_tid).ItemUseType == E_ItemUseType.SmeltScroll)
        {
            UIFrameItemEnchant enchant = UIManager.Instance.Find<UIFrameItemEnchant>();

            if (enchant == null)
            {
                UIManager.Instance.Load(nameof(UIFrameItemEnchant), (_loadName, _loadFrame) => {
                    UIManager.Instance.Open<UIFrameItemEnchant>((_name, _frame) => {
                        _frame.ClickEquipment();
                    });
                });
            }
            else
            {
                UIManager.Instance.Open<UIFrameItemEnchant>();
                UIManager.Instance.Find<UIFrameItemEnchant>().ClickEquipment();
            }

            UIManager.Instance.Find<UIFrameInventory>().SetSelectObject(null);
            Destroy(gameObject);
            return;
        }

        if (Item.netType == NetItemType.TYPE_STACK)
        {
            if (ZNet.Data.Me.CurCharData.IsCharacterUsable(Item.item_tid))
            {
                ZWebManager.Instance.WebGame.UseItemAction(Item, false, delegate
                {
                    if (UIManager.Instance.Find(out UIFrameInventory _inventory))
                        _inventory.RemoveInfoPopup();
                });
            }
        }
    }

    public void Equip()
    {
        ZWebManager.Instance.WebGame.UseItemAction(Item, false, delegate
        {
            if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            {
                _inventory.SetSelectObject(null);
                _inventory.RemoveInfoPopup();
            }
        });
    }

    public void UnEquip()
    {
        ZWebManager.Instance.WebGame.UseItemAction(Item, false, delegate
        {
            if(UIManager.Instance.Find(out UIFrameInventory _inventory))
			{
                _inventory.SetSelectObject(null);
                _inventory.RemoveInfoPopup();
            }
        });
    }

    public void Delete()
    {
        if (Item.cnt > 1)
        {
            // to do : 삭제할 카운트를 처리할 수 있는 별도 팝업을 추후 구현해야함
            DeleteItem();
        }
        else
            DeleteItem();
    }

    private void DeleteItem()
    {
        if (Item.netType == NetItemType.TYPE_EQUIP && Item.slot_idx != 0)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, "장착한 아이템은 삭제할 수 없습니다.",
                    new string[] { ZUIString.LOCALE_OK_BUTTON },
                    new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.WARRING, "아이템을 삭제하시겠습니까?",
                new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
                new Action[] {  delegate { _popup.Close(); }, delegate { ZWebManager.Instance.WebGame.REQ_DeleteItem(new List<ZItem>() { Item }, (recvPacket, recvMsgPacket) => {
                    _popup.Close();

                    if (UIManager.Instance.Find(out UIFrameInventory _inventory))
					{
                        _inventory.SetSelectObject(null);
                        _inventory.RemoveInfoPopup();
                    }
        });} });
        });
    }

    public void Close()
    {
        switch (Type)
        {
            case E_ItemPopupType.Mailbox:
                UIFrameMailbox mailbox = UIManager.Instance.Find<UIFrameMailbox>();
                mailbox.RemoveInfoPopup();
                break;
            case E_ItemPopupType.Storage:
                blockBackGround.SetActive(false);
                gameObject.SetActive(false);
                return;
            case E_ItemPopupType.Collection:
                gameObject.SetActive(false);
                break;
            case E_ItemPopupType.CharacterStateEquip:
                gameObject.SetActive(false);
                break;
            case E_ItemPopupType.Exchange:
                if (UIManager.Instance.Find(out UIFrameTrade _trade))
                {
                    _trade.RemoveInfoPopup();
                }
                break;
            case E_ItemPopupType.Reward: {
                    gameObject.SetActive(false);
                    break;
            }
            case E_ItemPopupType.GemEquip:
                if (UIManager.Instance.Find(out UIFrameItemGem _gemEquip))
				{
                    _gemEquip.OnSelectInvenGem(0);
                    _gemEquip.RemoveInfoPopup();
                }
                break;
            case E_ItemPopupType.GemUnEquip:
                if (UIManager.Instance.Find(out UIFrameItemGem _gemUnEquip))
                {
                    _gemUnEquip.RemoveInfoPopup();
                }
                break;
            case E_ItemPopupType.MileageShop:
                {
                    if(UIManager.Instance.Find(out UIFrameMileage _mileage))
                    {
                        _mileage.CloseItemInfoPopup();
                    }
                }
                break;
            case E_ItemPopupType.InfinityTower:
                {
                    if(UIManager.Instance.Find(out UIFrameDungeon _dungeon))
                    {
                        _dungeon.CloseItemInfoPopup();
                    }
                }
                break;
            default:
                if (UIManager.Instance.Find(out UIFrameInventory _inventory))
				{
                    _inventory.SetSelectObject(null);
                    _inventory.RemoveInfoPopup();
                    _inventory.RemoveAllSelectObject();
                }  
                break;
        }

        closeAction?.Invoke();
    }

    //[SerializeField] private UIItemInfoScrollAdapter ScrollAdapter = null;
    
    private void SetitemText()
    {
        itemTypeCheck = false;
        AbilityLine.SetActive(false);
        OptionLine.SetActive(false);

        TotalPay.SetActive(false);
        // 리팩 중
        for (int i = 0; i < UIItemInfoTextSlotList.Count; i++)
        {
            UIItemInfoTextSlotList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < UIItemInfoDetailTextSlotList.Count; i++)
        {
            UIItemInfoDetailTextSlotList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < UIItemEffectTextSlotList.Count; i++)
        {
            UIItemEffectTextSlotList[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < UIItemOptionTextSlotList.Count; i++)
        {
            UIItemOptionTextSlotList[i].gameObject.SetActive(false);
        }

        if (ItemDetailInfoList.Count > 0)
        {
            for (int i = 0; i < ItemDetailInfoList.Count; i++)
            {
                if (ItemDetailInfoList[i].Value != null)
                {
                    UIItemInfoTextSlotList[i].Initialize("", ItemDetailInfoList[i].Value, ItemDetailInfoList[i].strDesc);
                }
                else
                {
                    UIItemEffectTextSlotList[i].Initialize("", "", ItemDetailInfoList[i].strDesc);
                }
            }
        }
        for (int i = 0; i < UIItemInfoTextSlotList.Count; i++)
        {
            if (UIItemInfoTextSlotList[i].GetSlotActiveCheck())
            {
                UIItemInfoTextSlotList[i].gameObject.SetActive(true);
                itemTypeCheck = true;
                AbilityLine.SetActive(true);
            }
            UIItemInfoTextSlotList[i].SetSlotActiveCheck();
        }
        for (int i = 0; i < UIItemEffectTextSlotList.Count; i++)
        {
            if (UIItemEffectTextSlotList[i].GetSlotActiveCheck())
            {
                UIItemEffectTextSlotList[i].gameObject.SetActive(true);
                itemTypeCheck = true;
                OptionLine.SetActive(true);
            }
            UIItemEffectTextSlotList[i].SetSlotActiveCheck();
        }
        //
        for (int i = 0; i < Item.Options.Count; i++)
        {
            if (false == DBResmelting.GetResmeltScrollOption(Item.Options[i], out var table))
            {
                continue;
            }

            var keyPair = DBResmelting.GetResmeltingOptionGroupOrder(table);
            var abilityActionTable = DBAbility.GetAction(table.AbilityActionID);

            string abilityName = DBAbility.GetAbilityName(abilityActionTable.AbilityID_01);
            uint abilityId = (uint)abilityActionTable.AbilityID_01;
            float abilityValue = abilityActionTable.AbilityPoint_01_Min;

            UIItemOptionTextSlotList[i].Initialize("", DBAbility.ParseAbilityValue(abilityId, abilityValue).ToString(), abilityName);
        }

        for (int i = 0; i < UIItemOptionTextSlotList.Count; i++)
        {
            if (UIItemOptionTextSlotList[i].GetSlotActiveCheck())
            {
                UIItemOptionTextSlotList[i].gameObject.SetActive(true);
                itemTypeCheck = true;
                OptionLine.SetActive(true);
            }
            UIItemOptionTextSlotList[i].SetSlotActiveCheck();
        }

        for(int i = 0; i < UIItemWeightTextSlot.Length; i++)
            UIItemWeightTextSlot[i].Initialize("", (DBItem.GetItem(Item.item_tid).Weight).ToString(), DBLocale.GetText("ItemInfo_Group_Weight"));

        // to do : 기존 구조의 문제로 일단 이렇게 추가함. 나중에 구조 바꿔야함.
        UIItemInfoDetailTextSlotList[0].Initialize("", "", "획득처");
        UIItemInfoDetailTextSlotList[1].Initialize("", "", "-");

        UIItemInfoDetailTextSlotList[0].gameObject.SetActive(itemTypeCheck);
        UIItemInfoDetailTextSlotList[1].gameObject.SetActive(itemTypeCheck);

        UIItemWeightTextSlot[0].gameObject.SetActive(itemTypeCheck);
        UIItemWeightTextSlot[1].gameObject.SetActive(!itemTypeCheck);

        // 리팩 중
        UseCharacterTypeTextPage1.text = DBLocale.GetItemUseCharacterTypeName(Item.item_tid);
        UseCharacterTypeTextPage2.text = DBLocale.GetItemUseCharacterTypeName(Item.item_tid);

        if (DBItem.GetItem(Item.item_tid).TooltipID != null)
            ItemToolTip.text = DBLocale.GetText(DBItem.GetItem(Item.item_tid).TooltipID);
        else
            ItemToolTipGameObject.gameObject.SetActive(false);

        if (itemTypeCheck == false)
        {
            UseCharacterTypeTextMain.SetActive(true);

            ItemDescTypeA.SetActive(false);
            ItemDescTypeB.SetActive(true);
            IndicaterTypeA.gameObject.SetActive(false);
            IndicaterTypeB.gameObject.SetActive(false);
        }
        else
        {
            UseCharacterTypeTextMain.SetActive(false);

            ItemDescTypeA.SetActive(true);
            ItemDescTypeB.SetActive(false);
            IndicaterTypeA.gameObject.SetActive(true);
            IndicaterTypeB.gameObject.SetActive(true);
            IndicaterTypeA.color = Color.white;
            IndicaterTypeB.color = Color.gray;
        }
    }

    private void SetPosition(E_ItemPopupType _type)
    {
        switch (_type)
        {
            case E_ItemPopupType.InventoryEquipment:
            case E_ItemPopupType.InventoryStack:
                transform.localPosition = new Vector3(-800, -25, 0);
                break;

            case E_ItemPopupType.Mailbox:
                transform.localPosition = Vector3.zero;
                break;

            case E_ItemPopupType.CharacterStateEquip:
                transform.localPosition = new Vector3(870, -535, 0);
                break;
            case E_ItemPopupType.InfinityTower:
            case E_ItemPopupType.Reward:
                transform.localPosition = Vector3.zero;
                break;

            case E_ItemPopupType.GemEquip:
                transform.localPosition = new Vector3(330, -110, 0);
                break;

            case E_ItemPopupType.GemUnEquip:
                transform.localPosition = Vector3.zero;
                break;

            case E_ItemPopupType.MileageShop:
                transform.localPosition = Vector3.zero;
                break;

                //ljh : 창고에서 쓰이는 팝업은 프리팹에 부착됨, 따로 설정할필요 없음
        }
    }

    private void SetPopup(E_ItemPopupType _type)
    {
        if (_type != E_ItemPopupType.InventoryEquipment && _type != E_ItemPopupType.InventoryStack)
        {
            UIFrameInventory inventory = UIManager.Instance.Find<UIFrameInventory>();
            if (inventory != null)
                inventory.RemoveInfoPopup();
        }

        if (_type != E_ItemPopupType.Mailbox)
        {
            UIFrameMailbox mailbox = UIManager.Instance.Find<UIFrameMailbox>();
            if (mailbox != null)
                mailbox.RemoveInfoPopup();
        }

        if (_type != E_ItemPopupType.CharacterStateEquip)
        {
            UISubHUDCharacterState characterState = UIManager.Instance.Find<UISubHUDCharacterState>();
            if (characterState != null)
                characterState.RemoveInfoPopup();
        }

        //ljh : 창고는 내부프리팹에 부착됨, 따로 설정할필요 없음
    }

    private void SetItemDesc()
    {
        ItemDetailInfoList.Clear();                     // 아이템정보 리스트 지우기.
        //ItemOption.text = string.Empty;                 // 아이템 상세 정보 텍스트 지우기.
        var tableData = DBItem.GetItem(Item.item_tid);  // 해당 아이템의 Item_table 정보를 가져옴.

        List<uint> listAbilityActionIds = new List<uint>(); // abilityAction(최대 3개)를 저장할 리스트.
        Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();   //abilityAction 안에(최대 9개) 있는 abilityID를 담을 딕션.

        // 리스트에 abilityActionId 담기.
        if (tableData.AbilityActionID_01 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_01);
        if (tableData.AbilityActionID_02 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_02);
        if (tableData.AbilityActionID_03 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_03);

        foreach (var abilityActionId in listAbilityActionIds)   // listAbilityActionIds 리스트에 담긴 abilityActionId을 분석해보자.
        {
            var abilityActionData = DBAbility.GetAction(abilityActionId);   // abilityAction_table 데이터..
            switch (abilityActionData.AbilityViewType)  // AbilityViewType을 체크하자
            {
                case GameDB.E_AbilityViewType.ToolTip:  // E_AbilityViewType 타입이 ToolTip일 경우(ex. 특수(스킬)관련 일경우).
                    var itemDescInfo = new ItemDescInfo();
                    itemDescInfo.strDesc = string.Format("{0}{1}", "", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
                    itemDescInfo.Value = null;
                    ItemDetailInfoList.Add(itemDescInfo);   // 해당 내용을 ItemDetailInfoList에 담자.
                    break;
                case GameDB.E_AbilityViewType.Not:  // 보통의 경우 이쪽으로.
                default:
                    var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();  // abilityActionId 에 있는 ability를 추출하자.
                    while (enumer.MoveNext())
                    {
                        if (!abilitys.ContainsKey(enumer.Current.Key))
                        {
                            abilitys.Add(enumer.Current.Key, enumer.Current.Value); // ability를 추출해서 abilitys 딕션에 담자.
                        }
                    }
                    break;
            }
        }

        foreach (var ability in abilitys)   // 담은 abilitys 딕션을 분석해보자.
        {
            if (!DBAbility.IsParseAbility(ability.Key)) // 정확한 값이 아니면 넘김..
                continue;

            float abilityminValue = (uint)abilitys[ability.Key].Item1;  // 최소 대미지
            float abilitymaxValue = (uint)abilitys[ability.Key].Item2;  // 최대 대미지   보통 최소 대미지 만 있다.

            var itemDescInfo = new ItemDescInfo();

            itemDescInfo.strDesc = DBLocale.GetText(DBAbility.GetAbilityName(ability.Key));
            var newValue = DBAbility.ParseAbilityValue(ability.Key, abilityminValue, abilitymaxValue);
            itemDescInfo.Value = string.Format("{0}", newValue);

            ItemDetailInfoList.Add(itemDescInfo);   // 해당 내용을 ItemDetailInfoList에 담자.
        }
    }

    // ljh : 창고 추가작업 ----------------------------------------

    public void SetStorageOnClose(Action act)
    {
        onStorageClose = act;
    }

    private void SetDetailMode(bool state)
    {
        isDetailMode = state;

        Vector2 targetSize = PopupRT.sizeDelta;
        targetSize.y = isDetailMode ? SIZE_DETAIL : SIZE_DEFAULT;
        PopupRT.sizeDelta = targetSize;

        detailButton.SetActive(!isDetailMode);
        blockBackGround.SetActive(isDetailMode);
        Option.SetActive(isDetailMode);

        objSummery.SetActive(!isDetailMode);
        infoGroup.SetActive(isDetailMode);

        if (!isDetailMode)
        {
            Item_Table itemTable = DBItem.GetItem(Item.item_tid);

            //등급
            txtToolTip[0].gameObject.SetActive(itemTable.Grade > 0);
            txtToolTip[0].Initialize("", DBLocale.GetText(DBUIResouce.GetTierText(itemTable.Grade)), "등급");

            // 종류
            txtToolTip[1].Initialize("", DBLocale.GetItemTypeText(itemTable.ItemType), "종류");

            // 설명
            txtToolTip[2].gameObject.SetActive(!string.IsNullOrEmpty(itemTable.TooltipID));
            txtToolTip[2].Initialize("", DBLocale.GetText(DBItem.GetItem(Item.item_tid).TooltipID), "정보");
        }

        listDetailObject.ForEach(item => item.SetActive(isDetailMode));

        if (isDetailMode)
        {
            ItemDescTypeA.SetActive(true);
            ItemDescTypeB.SetActive(false);
            IndicaterTypeA.color = Color.white;
            IndicaterTypeB.color = Color.gray;
            itemDescTypeCheck = false;
        }
    }

    public void CloseStorageInfo()
    {
        if (isDetailMode)
        {// 더보기 모드시 닫기 -> 화면 작게

            SetDetailMode(false);
        }
        else
        {// 더보기 모드가 아닐시 닫기 -> 팝업닫기
            Close();
            onStorageClose?.Invoke();
        }
    }

    public void ClickStorageInfoDetail()
    {
        SetDetailMode(true);
    }

    #region ui 상세보기 분리 작업 함수
    private void ItemDescCheck()
    {
        for (int i = 0; i < fadeoutGrop.Count; i++)
        {
            fadeoutGrop[i].alpha = 1;
        }

        if (itemDescTypeCheck)
        {
            ItemDescTypeA.SetActive(true);
            ItemDescTypeB.SetActive(false);
            IndicaterTypeA.color = Color.white;
            IndicaterTypeB.color = Color.gray;
            itemDescTypeCheck = false;
        }
        else
        {
            ItemDescTypeA.SetActive(false);
            ItemDescTypeB.SetActive(true);
            IndicaterTypeA.color = Color.gray;
            IndicaterTypeB.color = Color.white;
            itemDescTypeCheck = true;
        }
    }

    public void MousePointerBeginDrag()
    {
        if (!itemTypeCheck)
            return;
        dragCheck = true;
        Camera = GameObject.Find("UICamera").GetComponent<Camera>();
        MousePosition = Input.mousePosition;
        MousePosition.y = Camera.transform.position.y;
        MousePosition.z = Camera.transform.position.z;
        MousePosition = Camera.ScreenToWorldPoint(MousePosition);
    }

    public void MousePointerEndDrag()
    {
    }
    public void OnDrag(BaseEventData eventData)
    {
    }

    public void MousePointerDrag()
    {
    }

    private void Update()
    {
        if (dragCheck && Input.GetMouseButtonUp(0))
        {
            if (ItemDecTransCheck)
            {
                ItemDecTransCheck = false;
                ItemDescCheck();
            }

            MousePosition = Vector3.zero;
            MouseDragPosition = Vector3.zero;
            distance = 0.0f;
            dragCheck = false;
            for (int i = 0; i < fadeoutGrop.Count; i++)
            {
                fadeoutGrop[i].alpha = 1;
            }
        }

        if (dragCheck)
        {
            Camera = GameObject.Find("UICamera").GetComponent<Camera>();
            MouseDragPosition = Input.mousePosition;
            MouseDragPosition.y = Camera.transform.position.y;
            MouseDragPosition.z = Camera.transform.position.z;
            MouseDragPosition = Camera.ScreenToWorldPoint(MouseDragPosition);

            distance = Vector3.Distance(MousePosition, MouseDragPosition);
            for (int i = 0; i < fadeoutGrop.Count; i++)
            {
                fadeoutGrop[i].alpha = 1 - distance;
            }
            if (distance > 1.0f)
            {
                ItemDecTransCheck = true;
            }
            else if (distance < 1.0f)
            {
                ItemDecTransCheck = false;
            }
        }
    }
    #endregion

    /// <summary>아이템 정보창에서 분해 버튼 클릭시 </summary>
    public void ClickDisassemble()
    {
        UIFrameItemDisassemble disassemble = UIManager.Instance.Find<UIFrameItemDisassemble>();

        if (disassemble == null)
        {
            UIManager.Instance.Load(nameof(UIFrameItemDisassemble), (_loadName, _loadFrame) => {
                UIManager.Instance.Open<UIFrameItemDisassemble>((_name, _frame) => {
                    ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIItemDisassembleHolder), delegate
                    {
                        _frame.ScrollAdapter.Initialize();
                        _frame.SetDisassemblePopup(Item);
                        UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.RemoveData(Item);
                        //UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.ScrollTo(0);
                    });
                });
            });
        }
        else
        {
            UIManager.Instance.Open<UIFrameItemDisassemble>();
            UIManager.Instance.Find<UIFrameItemDisassemble>().SetDisassemblePopup(Item);
            UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.RemoveData(Item);
            //UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.ScrollTo(0);
        }

        UIManager.Instance.Find<UIFrameInventory>().SetSelectObject(null);
        Destroy(gameObject);
    }

    /// <summary> 아이템 정보창에서 승급 버튼 클릭 </summary>
    public void ClickTierUp()
    {
        UIFrameItemUpgrade upgrade = UIManager.Instance.Find<UIFrameItemUpgrade>();

        if (upgrade == null)
        {
            UIManager.Instance.Load(nameof(UIFrameItemUpgrade), (_loadName, _loadFrame) => {
                UIManager.Instance.Open<UIFrameItemUpgrade>((_name, _frame) => {
                    _frame.Set(Item);
                });
            });
        }
        else
        {
            //UIManager.Instance.Open<UIFrameItemUpgrade>();
            UIManager.Instance.Open<UIFrameItemUpgrade>((_name, _frame) => {
                _frame.Set(Item);
            });
        }

        if(UIManager.Instance.Find(out UIFrameInventory _inventory))
		{
            _inventory.SetSelectObject(null);
            _inventory.RemoveInfoPopup();
        }
    }

    /// <summary>아이템 정보창에서 강화 버튼 클릭 </summary>
    public void ClickEnhance()
    {
        UIFrameItemEnhance enhance = UIManager.Instance.Find<UIFrameItemEnhance>();

        if (enhance == null)
        {
            UIManager.Instance.Load(nameof(UIFrameItemEnhance), (_loadName, _loadFrame) => {
                UIManager.Instance.Open<UIFrameItemEnhance>((_name, _frame) => {
                    _frame.SetEnhanceItem(Item);
                });
            });
        }
        else
        {
            UIManager.Instance.Open<UIFrameItemEnhance>();
            UIManager.Instance.Find<UIFrameItemEnhance>().SetEnhanceItem(Item);
        }

        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
        {
            _inventory.SetSelectObject(null);
            _inventory.RemoveInfoPopup();
        }
    }

    /// <summary> 제련 버튼 클릭시 </summary>
    public void ClickEnchent()
    {
        UIFrameItemEnchant enchant = UIManager.Instance.Find<UIFrameItemEnchant>();

        if (enchant == null)
        {
            UIManager.Instance.Load(nameof(UIFrameItemEnchant), (_loadName, _loadFrame) => {
                UIManager.Instance.Open<UIFrameItemEnchant>((_name, _frame) => {
                    if (DBItem.IsEquipItem(Item.item_tid))
                    {
                        _frame.Set(Item);
                    }
                });
            });
        }
        else
        {
            UIManager.Instance.Open<UIFrameItemEnchant>();
            UIManager.Instance.Find<UIFrameItemEnchant>().Set(Item);
        }

        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
        {
            _inventory.SetSelectObject(null);
            _inventory.RemoveInfoPopup();
        }
    }

    /// <summary>콜렉션 버튼 클릭시</summary>
    public void ClickCollection()
	{
        var collection = UIManager.Instance.Find<UIFrameItemCollection>();

        if (collection == null)
            return;

        UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
            _popup.Open("컬렉션 등록", string.Format(DBLocale.GetText("Register_Item_Collection"), DBLocale.GetText(DBItem.GetItem(Item.item_tid).ItemTextID)), new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate{ _popup.Close(); }, delegate {
                 ZWebManager.Instance.WebGame.REQ_RegistItemCollection(collection.CollectionTid, collection.SelectCollectionSlotIdx, collection.Material.item_id, collection.Material.item_tid, (recvPacket, onError)=> {
                    _popup.Close();
                    collection.UpdateTab();
                    collection.UpdateCurrentApply();
                    collection.ItemInfo.gameObject.SetActive(false);
                });
            } });
        });
	}

    /// <summary>교환 버튼 클릭시</summary>
    public void ClickExchange()
    {
        if (UIManager.Instance.Find(out UIFrameTrade _trade))
		{
            _trade.OnBuyItem(ExchangeItem);
            _trade.RemoveInfoPopup();
        }
    }

    public void ClickGemEquip()
    {
        if (UIManager.Instance.Find(out UIFrameItemGem _gem))
		{
            _gem.EquipGem(Item);
            _gem.RemoveInfoPopup();
        }
    }

    public void ClickGemUnEquip()
    {
        if (UIManager.Instance.Find(out UIFrameItemGem _gem))
        {
            _gem.UnEquipGem(Item);
            _gem.RemoveInfoPopup();
        }
    }

    /// <summary>
    /// 마일리기 보상받기 버튼 클릭시 
    /// </summary>
    public void ClickExchangeMileage()
    {
        if(UIManager.Instance.Find(out UIFrameMileage _mileage))
        {
            _mileage.ExchangeItem_CurrentContextBuyInfo();
        }
    }

    public void ClickItemLock()
	{
        ZWebManager.Instance.WebGame.REQ_ToggleLock(E_GoodsKindType.Item, Item.item_id, Item.item_tid, !Item.IsLock, (recvPacket, msg) => {
            
            if(recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
			{
                ZItem item = new ZItem(msg.RemainItems.Value.Equip(0).Value);

                if (item.IsLock != false)
                    UICommon.OpenSystemPopup_One(ZUIString.ERROR,
                    DBLocale.GetText("Unlock_Message"), ZUIString.LOCALE_OK_BUTTON, delegate { Initialize(Type, item); });
                else
                    UICommon.OpenSystemPopup_One(ZUIString.ERROR,
                    DBLocale.GetText("Lock_Message"), ZUIString.LOCALE_OK_BUTTON, delegate { Initialize(Type, item); });
            }
        });
	}

    [SerializeField] private List<GameObject> objBtnGroup = new List<GameObject>();

    // 빌드용 급조코드, 버튼다끔
    public void SetOFFButtonGroup()
	{
        objBtnGroup.ForEach(item => item.SetActive(false));
	}
}

[Serializable]
public class ItemInfoButton
{
    public ZButton Button = null;
    public E_ItemPopupFlag flag = E_ItemPopupFlag.None;
}