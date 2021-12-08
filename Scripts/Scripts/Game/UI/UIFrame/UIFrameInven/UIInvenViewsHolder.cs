using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uTools;
using ZDefine;
using ZNet.Data;

public class UIInvenViewsHolder : CellViewsHolder
{
    #region UI Variable
    private Image Icon = null;
    private RectTransform NewIcon = null;
    private Text GradeTxt = null;
    private Image GradeBoard = null;
    private Text NumTxt = null;
    private RectTransform Equip = null;
    private RectTransform Lock = null;
    private RectTransform CoolTime = null;
    private Text CoolTimeTxt = null;
    private RectTransform BlockRed = null;
    private Image BlockRedClassIcon = null;
    public RectTransform SelectImg = null;
    private RectTransform ItemSlotShareParts = null;
    private RectTransform ItemSlotInvenParts = null;

    private RectTransform SlotGem = null;
    private Text SlotGemCount = null;

    //CoolTime
    private uTweenSlider CoolTimeSlider;
    private uTweenText CoolTimeCount;

    #endregion

    #region System Variable
    public ZItem Item = null;
    #endregion

    /// <summary>아이템 상세보기 버튼 클백 </summary>
    public void ShowItemInfo()
    {
        if (Item == null)
            return;

        if (!UIManager.Instance.Find(out UIFrameInventory _inventory))
            return;

        // 분해 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.Disassemble)
        {
            if(UIManager.Instance.Find(out UIFrameItemDisassemble _break))
            {
                if(_break.ScrollAdapter.Data.List.Count < ZUIConstant.BREAK_ITEM_MAX_COUNT)
                    _break.SetDisassemblePopup(Item);
                else
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
                }
            }
               
            return;
        }

        // 강화 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.Enhance)
        {
            UIManager.Instance.Find<UIFrameItemEnhance>().SetMaterial(Item);
            return;
        }

        // 강화 장비 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.EnhanceEquip)
        {
            UIManager.Instance.Find<UIFrameItemEnhance>().SetEnhanceItem(Item);
            return;
        }

        // 승급 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.Upgrade)
        {
            UIManager.Instance.Find<UIFrameItemUpgrade>().Set(Item);
            return;
        }

        // 제련 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.Enchant)
        {
            UIManager.Instance.Find<UIFrameItemEnchant>().SetMaterial(Item);
            return;
        }

        // 강화 장비 탭에서 선택시
        if (_inventory.CurSortType == E_InvenSortType.EnchantEquip)
        {
            UIManager.Instance.Find<UIFrameItemEnchant>().Set(Item);
            return;
        }

        _inventory.RemoveAllSelectObject();

        var data = _inventory.ScrollAdapter.Data.List.Find(item => (item.Item != null) && (item.Item.item_id == this.Item.item_id));
        if (data == null)
            return;
        data.isSelected = true;
        SelectImg.gameObject.SetActive(data.isSelected);
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (_obj) =>
        {
            _inventory.SetSelectObject(Item);

            UIPopupItemInfo obj = _obj.GetComponent<UIPopupItemInfo>();

            if (obj != null)
            {
                _inventory.SetInfoPopup(obj);
                obj.transform.SetParent(_inventory.gameObject.transform);
                if (Item.netType == NetItemType.TYPE_EQUIP)
                    obj.Initialize(E_ItemPopupType.InventoryEquipment, Item);
                else
                    obj.Initialize(E_ItemPopupType.InventoryStack, Item);
            }
        });
    }

    /// <summary>최초 1번 홀더에서 사용할 컴포넌트를 연결</summary>
    public override void CollectViews()
    {
        base.CollectViews();

        // Component 등록
        views.GetComponentAtPath("ItemSlot_Share_Parts/Item_Icon", out Icon);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Alarm_RedDot", out NewIcon);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Grade/Txt_Grade", out GradeTxt);
        views.GetComponentAtPath("ItemSlot_Share_Parts/Grade_Board", out GradeBoard);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Num/Txt_Num", out NumTxt);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Alarm_Equied", out Equip);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Lock_Icon", out Lock);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/CoolTime", out CoolTime);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/CoolTime/Txt_CoolTime", out CoolTimeTxt);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Block_Red", out BlockRed);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Block_Red/Icon", out BlockRedClassIcon);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Select", out SelectImg);
        
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Gem_Slot", out SlotGem);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/Gem_Slot/BG/Txt_GemNum", out SlotGemCount);

        views.GetComponentAtPath("ItemSlot_Share_Parts", out ItemSlotShareParts);
        views.GetComponentAtPath("ItemSlot_Inven_Parts", out ItemSlotInvenParts);
        // Button Callback 등록
        views.GetComponent<ZButton>().onClick.AddListener(ShowItemInfo);

        //CoolTime
        views.GetComponentAtPath("ItemSlot_Inven_Parts/CoolTime", out CoolTimeSlider);
        views.GetComponentAtPath("ItemSlot_Inven_Parts/CoolTime/Txt_CoolTime", out CoolTimeCount);

        CoolTimeSlider.onFinished.AddListener(EndCoolTime);
    }

    /// <summary>현재 보여주는 홀더 데이터 갱신</summary>
    /// <param name="_holder"></param>
    public void UpdateTitleByItemIndex(ScrollInvenData _holder)
    {
        EndCoolTime();

        if (_holder.type == ScrollInvenData.CellType.Expansion)
            views.gameObject.SetActive(false);

        if (_holder.Item != null && _holder.Item.netType == NetItemType.TYPE_STACK && _holder.Item.cnt == 0)
        {
            _holder.Item = null;
            //UIManager.Instance.Find<UIFrameInventory>().SortList();
        }

        Item = _holder.Item;

        if (Item == null)
        {
            Icon.gameObject.SetActive(false);
            NumTxt.text = string.Empty;
            GradeTxt.text = string.Empty;
            SlotGemCount.text = string.Empty;
            Equip.gameObject.SetActive(false);
            Lock.gameObject.SetActive(false);
            SelectImg.gameObject.SetActive(false);
            NewIcon.gameObject.SetActive(false);
            BlockRed.gameObject.SetActive(false);
            GradeBoard.gameObject.SetActive(false);
            SlotGem.gameObject.SetActive(false);
            return;
        }

        ulong useTime = Item.UseTime;
        if (useTime != 0)
        {
            float remainCooltime = (DBItem.GetItem(Item.item_tid).CoolTime) - ((TimeManager.NowMs - useTime) / 1000);
            if (remainCooltime > 0)
                UpdateCoolTime(DBItem.GetItem(Item.item_tid).CoolTime, remainCooltime);
        }


        SlotGemCount.text = UICommon.GetEquipSocketCount(Item.Sockets).ToString();
        SlotGem.gameObject.SetActive(UICommon.GetEquipSocketCheck(Item.Sockets));

        Icon.gameObject.SetActive(Item.item_tid != 0);
        Icon.sprite = UICommon.GetItemIconSprite(Item.item_tid);
        NumTxt.text = Item.item_tid != 4800 ? Item.cnt.ToString() : string.Empty;
        GradeTxt.text = string.Empty;

        var grade = DBItem.GetItem(Item.item_tid).Grade;
        bool isUseGradeColor = grade > 0;
        GradeBoard.gameObject.SetActive(isUseGradeColor);
        if (isUseGradeColor)
            GradeBoard.sprite = UICommon.GetGradeSprite(grade);

        BlockRed.gameObject.SetActive(!Item.IsLock);

        GradeBoard.gameObject.SetActive(true);
        Equip.gameObject.SetActive(Item.slot_idx != 0);
        Lock.gameObject.SetActive(Item.IsLock);
        SelectImg.gameObject.SetActive(_holder.isSelected);
        NewIcon.gameObject.SetActive(_holder.isNew);
        CoolTimeCount.gameObject.SetActive(false); // 기획팀 요청
        if (Item.netType == NetItemType.TYPE_EQUIP)
        {
            NumTxt.text = string.Empty;
            GradeTxt.text = DBItem.GetItem(_holder.Item.item_tid).Step == 0 ? string.Empty : "+" + DBItem.GetItem(_holder.Item.item_tid).Step.ToString();
            BlockRed.gameObject.SetActive(!ZNet.Data.Me.CurCharData.IsCharacterEquipable(Item.item_tid));

            // to do : 별도의 테이블 정보가 없어서 임시로 처리
            switch (DBItem.GetUseCharacterType(Item.item_tid))
            {
                case GameDB.E_CharacterType.Knight:
                    BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_gladiator_01_s");
                    break;
                case GameDB.E_CharacterType.Assassin:
                    BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_assassin_01_s");
                    break;
                case GameDB.E_CharacterType.Archer:
                    BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_archer_01_s");
                    break;
                case GameDB.E_CharacterType.Wizard:
                    BlockRedClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite("icon_magician_01_s");
                    break;
            }
        }
        else
        {
            BlockRed.gameObject.SetActive(false);
        }
    }

    public void EndCoolTime()
    {
        CoolTimeSlider.gameObject.SetActive(false);
    }

    public void UpdateCoolTime(float _coolTime, float _remainTime = 0)
    {
        if (_coolTime > 0)
        {
            CoolTimeSlider.gameObject.SetActive(true);
            CoolTimeSlider.enabled = true;

            if (_remainTime != 0)
            {
                CoolTimeSlider.duration = _remainTime;
                CoolTimeCount.duration = _remainTime;
                CoolTimeSlider.from = _remainTime / _coolTime;
                CoolTimeCount.from = _remainTime;
            }
            else
            {
                CoolTimeSlider.duration = _coolTime;
                CoolTimeCount.duration = _coolTime;
                CoolTimeCount.from = _coolTime;
            }

            CoolTimeSlider.ResetToBeginning();
            CoolTimeCount.ResetToBeginning();
            CoolTimeSlider.Play(true);
            CoolTimeCount.Play(true);
        }
    }
}