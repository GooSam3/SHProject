using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameItemDisassemble : ZUIFrameBase
{
    #region UI Enum
    public enum WeaponType_Index
    {
        Weapon,
        Armor,
        //Accessory
    }

    public enum Tier_Index
    {
        Normal,
        HighClass,
        Rare,
    }
    #endregion

    #region UI Variable
    /// <summary>아이템 분해 스크롤 어댑터</summary>
    public UIItemDisassembleScrollAdapter ScrollAdapter = null;
    /// <summary>아이템 분해 소모 재화 이미지</summary>
    [SerializeField] private Image CostImage = null;
    /// <summary>아이템 분해 소모 재화 비용 텍스트</summary>
    [SerializeField] private Text CostCntText = null;
    /// <summary>분해 아이템 갯수 텍스트</summary>
    [SerializeField] private Text DisassembleItemCountText = null;
    /// <summary>아이템 분해시 얻는 재화 이미지 배열</summary>
    [SerializeField] private Image[] GainImage = new Image[ZUIConstant.DISASSEMBLE_GAIN_ITEM_COUNT];
    /// <summary>아이템 분해시 얻는 재화 갯수 배열</summary>
    [SerializeField] private Text[] GainCntText = new Text[ZUIConstant.DISASSEMBLE_GAIN_ITEM_COUNT];
    /// <summary>아이템 분해 Parts 토글 배열</summary>
    [SerializeField] private GameObject[] PartsToggle = new GameObject[ZUIConstant.DISASSEMBLE_ITEM_PARTS_COUNT];
    /// <summary>아이템 분해 Tier 토글 배열</summary>
    [SerializeField] private GameObject[] TierToggle = new GameObject[ZUIConstant.DISASSEMBLE_ITEM_TIER_COUNT];
    #endregion

    #region System Variable
    public override bool IsBackable => true;
    #endregion

    protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
        UIManager.Instance.Open<UIFrameInventory>();
    }

	protected override void OnRemove()
    {
        base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIItemDisassembleHolder));
    }

	protected override void OnHide()
	{
		base.OnHide();

        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.All);
        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.All);

        ScrollAdapter.ClearData();
        RefreshText();
    }

    public void SetDisassemblePopup(ZItem _item)
    {
        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Disassemble);
        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.Disassemble);

        ScrollAdapter.AddData(_item, delegate { RefreshText(); });
        UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.RemoveData(_item);
    }

    public void SetDisassemblePopup(List<ZItem> _listItem)
	{
        //UIManager.Instance.Find<UIFrameInventory>().ShowInvenSort((int)E_InvenSortType.Disassemble);
        if (UIManager.Instance.Find(out UIFrameInventory _inventory))
            _inventory.ShowInvenSort((int)E_InvenSortType.Disassemble);

        if(ScrollAdapter.Data.List.Count > ZUIConstant.BREAK_ITEM_MAX_COUNT)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("분해는 한 번에 최대 " + ZUIConstant.BREAK_ITEM_MAX_COUNT.ToString() + "개까지 가능합니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate
                    {
                        _popup.Close();
                    }
                });
            });
            return;
        }

        int index = ScrollAdapter.Data.List.Count + _listItem.Count;
        List<ZItem> breakItemList = new List<ZItem>();

        if (index > ZUIConstant.BREAK_ITEM_MAX_COUNT)
        {
            for (int i = 0; i < ZUIConstant.BREAK_ITEM_MAX_COUNT - ScrollAdapter.Data.List.Count; i++)
                breakItemList.Add(_listItem[i]);
        }
        else
            breakItemList = _listItem;

        ScrollAdapter.AddData(breakItemList, delegate { RefreshText(); });
        UIManager.Instance.Find<UIFrameInventory>().ScrollAdapter.RemoveData(_listItem);
    }

    public void RefreshText()
    {
        DisassembleItemCountText.text = string.Format("({0}/{1})", ScrollAdapter.Data.List.Count, ZUIConstant.BREAK_ITEM_MAX_COUNT);

        uint cost = 0;
        uint[] gainId = new uint[ZUIConstant.DISASSEMBLE_GAIN_ITEM_COUNT];
        uint[] gainCnt = new uint[ZUIConstant.DISASSEMBLE_GAIN_ITEM_COUNT];

        for(int i = 0; i < ScrollAdapter.Data.List.Count; i++)
        {
            var table = DBItem.GetItem(ScrollAdapter.Data.List[i].Item.item_tid);
            cost += table.BreakUseCount;

            for(int j = 0; j < table.BreakItemID.Count; j++)
            {
                gainId[j] = table.BreakItemID[j];
                gainCnt[j] += table.BreakItemCount[j];
            }  
        }

        /*if(cost != 0) */CostCntText.text = cost.ToString();

        for (int i = 0; i < GainCntText.Length; i++)
        {
            GainCntText[i].gameObject.SetActive(gainCnt[i] != 0);
            GainImage[i].gameObject.SetActive(gainCnt[i] != 0);

            if (gainCnt[i] != 0)
            {
                GainImage[i].sprite = UICommon.GetItemIconSprite(gainId[i]);
                GainCntText[i].text = gainCnt[i].ToString();
            }  
        }
    }

    /// <summary> 분해 버튼 콜백. </summary>
    public void ClickDisassemble()
    {
        if(ScrollAdapter.Data.List.Count == 0)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("등록된 분해를 진행할 아이템이 없습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate
                    { 
                        _popup.Close();
                    }
                });
            });
            return;
        }

        if(ScrollAdapter.GetTotalCost() > Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK).cnt)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("골드가 부족합니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate
                    {
                        _popup.Close(); 
                    }
                });
            });
            return;
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.WARRING, "아이템을 분해하시겠습니까?",
                new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
                new Action[] {  delegate { _popup.Close(); }, delegate {
                    List<ZItem> items = new List<ZItem>();

                    for (int i = 0; i < ScrollAdapter.Data.List.Count; i++)
                    items.Add(ScrollAdapter.Data[i].Item);

                    ZWebManager.Instance.WebGame.REQ_BreakItem(items, (recvPacket, onError) => {
                    UIManager.Instance.Close<UIFrameItemDisassemble>();

                        // 아이템 획득 팝업.

                        List<GainInfo> gainList = new List<GainInfo>();
                        for(int i = 0; i<onError.GetItemsLength; i++)
                            gainList.Add(new GainInfo(onError.GetItems(i).Value));

                        UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame)=>
                        {
                             frame.AddItem(gainList);
                        });

                        if(UIManager.Instance.Find(out UIFrameInventory _inventory))
                            _inventory.RefreshInvenVolume();

                        //if (UIManager.Instance.Find(out UIFrameItemRewardShot _rewardShot))
                        //{
                        //     _rewardShot.AddItem(gainList);
                        //}

                        //ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIFrameItemRewardShot), (_obj) =>
                        //{
                        //    UIFrameItemRewardShot obj = _obj.GetComponent<UIFrameItemRewardShot>();

                        //    if (obj != null)
                        //    {
                        //        obj.transform.SetParent(UIManager.Instance.transform);
                        //        List<GainInfo> gainList = new List<GainInfo>();

                        //        for(int i = 0; i<onError.GetItemsLength; i++)
                        //            gainList.Add(new GainInfo(onError.GetItems(i).Value));

                        //        obj.AddItem(gainList);
                        //    }
                        //});

                    _popup.Close();
        }); } });});
    }

    /// <summary> 일괄등록 버튼 콜백. </summary> 
    public void ClickSelecAll()
    {
        ScrollAdapter.Data.RemoveItemsFromEnd(ScrollAdapter.Data.Count);    // 기존장비 제거.

        ClickSelectAllDisable();

        if (TierToggle[(int)Tier_Index.Normal].activeSelf/* || Tier1Toggle.activeSelf*/)
            AddDisassembleItemList(GameDB.E_RuneGradeType.Normal);
        if (TierToggle[(int)Tier_Index.HighClass].activeSelf/* Tier2Toggle.activeSelf*/)
            AddDisassembleItemList(GameDB.E_RuneGradeType.HighClass);
        if (TierToggle[(int)Tier_Index.Rare].activeSelf/* Tier3Toggle.activeSelf*/)
            AddDisassembleItemList(GameDB.E_RuneGradeType.Rare);
    }

    private void AddDisassembleItemList(GameDB.E_RuneGradeType _grade)
	{
        List<ZItem> itemList = new List<ZItem>();

        for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
        {
            var item = Me.CurCharData.InvenList[i];
            var itemBreakData = DBItem.GetItem(item.item_tid);
            if (null == item || item.netType != NetItemType.TYPE_EQUIP || item.slot_idx != 0 || item.IsLock || !itemBreakData.LimitType.HasFlag(GameDB.E_LimitType.Break))
            {
                // zlog
                continue;
            }

            var itemTable = DBItem.GetItem(item.item_tid);
            if (itemTable.Grade != (byte)_grade)
                continue;

            bool existDissableItems = false;
            if (PartsToggle[(int)WeaponType_Index.Weapon].activeSelf/* || WeaponToggle.activeSelf*/)
            {
                if (itemTable.EquipSlotType == GameDB.E_EquipSlotType.Weapon ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.SideWeapon)
                {
                    existDissableItems = true;
                }
            }

            if (PartsToggle[(int)WeaponType_Index.Armor].activeSelf/* || ArmorToggle.activeSelf*/)
            {
                if (itemTable.EquipSlotType == GameDB.E_EquipSlotType.Helmet ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.Armor ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.Pants ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.Shoes ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.Gloves ||
                    itemTable.EquipSlotType == GameDB.E_EquipSlotType.Cape)
                {
                    existDissableItems = true;
                }
            }

            if (existDissableItems)
            {
                itemList.Add(item);
            }
        }

        SetDisassemblePopup(itemList);
    }

    /// <summary>
    /// 일괄등록 해제버튼 콜백. 
    /// </summary>
    public void ClickSelectAllDisable()
    {
        if (!UIManager.Instance.Find(out UIFrameInventory _inventory))
            return;

        for (int i = 0; i < ScrollAdapter.Data.Count; i++)
        {
            var data = _inventory.ScrollAdapter.Data.List.Find(item => item.Item == null);
            if (data != null)
                data.Reset(new ScrollInvenData() { Item = ScrollAdapter.Data[i].Item });
        }

        ScrollAdapter.Data.RemoveItemsFromEnd(ScrollAdapter.Data.Count);
        RefreshText();
        _inventory.ScrollAdapter.SetData();
        _inventory.RefreshInvenVolume();
    }

    /// <summary>팝업 close </summary>
    public void Close()
    {
        UIManager.Instance.Close<UIFrameItemDisassemble>();
    }
}