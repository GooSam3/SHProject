using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using ZNet.Data;

public class UISubHUDQuickSlot : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] public List<UIQuickScrollAdapter> ScrollAdapter = new List<UIQuickScrollAdapter>();
    [SerializeField] public UIRide UIRide;
    #endregion

    #region System Variable
    public bool IsQuickSlotResetMode = false;
    public bool IsAutoCheck = false;
    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);
        UIRide.DoAddEvent();
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (ZPawnManager.hasInstance)
        {
            ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);

            if (ZPawnManager.Instance.MyEntity != null)
            {
                ZPawnManager.Instance.MyEntity.DoRemoveEventChangeSkillCoolTime(ChangeSkillCoolTimeEvent);
            }
        }

        UIRide.DoRemoveEvent();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIQuickItemSlotPageHolder));
    }

    private void HandleCreateMyEntity()
    {
        ZPawnManager.Instance.MyEntity.DoAddEventChangeSkillCoolTime(ChangeSkillCoolTimeEvent);
    }

    public void Initialize()
    {
        for (int i = 0; i < ScrollAdapter.Count; i++)
            ScrollAdapter[i].SetScrollData(i);

        IsAutoCheck = true;
        CancelInvoke(nameof(CheckAuto));
        InvokeRepeating(nameof(CheckAuto), 0.5f, 1.0f);

        UIRide.DoAddEvent();
    }

    // 추가
    private void UpdateAutoSkill()
    {
        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if (ScrollAdapter[i].Data != null)
            {
                for (int j = 0; j < ScrollAdapter[i].Data.List.Count; j++)
                {
                    for (int k = 0; k < ScrollAdapter[i].Data[j].QuickSlotInfos.Length; k++)
                    {
                        if (ScrollAdapter[i].Data[j].QuickSlotInfos[k] != null)
                        {
                            if (ScrollAdapter[i].Data[j].QuickSlotInfos[k].bAuto && ScrollAdapter[i].Data[j].QuickSlotInfos[k].SlotType == QuickSlotType.TYPE_SKILL)
                               ZPawnManager.Instance.MyEntity.GetAutoSkillController().AddAutoSkill(ScrollAdapter[i].Data[j].QuickSlotInfos[k].TableID);
                            else if(!ScrollAdapter[i].Data[j].QuickSlotInfos[k].bAuto && ScrollAdapter[i].Data[j].QuickSlotInfos[k].SlotType == QuickSlotType.TYPE_SKILL)
                                ZPawnManager.Instance.MyEntity.GetAutoSkillController().RemoveAutoSkill(ScrollAdapter[i].Data[j].QuickSlotInfos[k].TableID);
                        }
                    }
                }
            }
        }
    }

    /// <summary>퀵슬롯 리프레쉬</summary>
    public void RefreshAllSlot()
    {
        if (ScrollAdapter == null)
            return;

        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if(ScrollAdapter[i].IsInitialized)
                ScrollAdapter[i].RefreshData();
        }
    }

    /// <summary>퀵슬롯 리셋</summary>
    public void ReSetAllSlot()
    {
        if (ScrollAdapter == null)
            return;

        for (int i = 0; i < ScrollAdapter.Count; i++)
            ScrollAdapter[i].SetScrollData(i, ScrollAdapter[i].BaseParameters.Snapper._LastSnappedItemIndex);
    }

    /// <summary>퀵 선택 임팩트 활성화 함수</summary>
    /// <param name="_active">퀵 선택 임팩트 활성화 여부</param>
    public void SelectQuickSlotEffect(bool _active)
    {
        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if (ScrollAdapter[i].IsInitialized)
            {
                for (int j = 0; j < ScrollAdapter[i].GetItemsCount(); j++)
                {
                    if (ScrollAdapter[i].GetItemViewsHolder(j) != null)
                    {
                        for (int k = 0; k < ScrollAdapter[i].GetItemViewsHolder(j).SlotArr.Length; k++)
                        {
                            ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].ActiveSelectEffect(_active);
                        }
                    }
                }
            }
        }
    }

    /// <summary>퀵슬롯에 같은 정보를 가지고있는 퀵 데이터를 찾음</summary>
    public List<UIQuickItemSlot> GetQuickListSlot(uint _tableID)
    {
        List<UIQuickItemSlot> List = new List<UIQuickItemSlot>();

        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if (ScrollAdapter[i].Data != null)
            {
                for (int j = 0; j < ScrollAdapter[i].Data.List.Count; j++)
                {
                    if (ScrollAdapter[i].GetItemViewsHolder(j) != null)
                    {
                        for (int k = 0; k < ScrollAdapter[i].GetItemViewsHolder(j).SlotArr.Length; k++)
                        {
                            if (ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].Data.QuickSlotInfos[k] != null && ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].Data.QuickSlotInfos[k].TableID == _tableID)
                            {
                                List.Add(ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k]);
                            }
                        }
                    }
                }
            }
        }

        return List;
    }

    private void ChangeSkillCoolTimeEvent(uint _skillId, float _remainTime)
    {
        List<UIQuickItemSlot> quickSlotList = GetQuickListSlot(_skillId);

        if (quickSlotList != null)
        {
            for (int i = 0; i < quickSlotList.Count; i++)
            {
                quickSlotList[i].UpdateCoolTime(_remainTime);
            }
        }
    }

    public List<UIQuickItemSlot> GetGroupByItem(uint _itemGroupID)
    {
        List<UIQuickItemSlot> result = new List<UIQuickItemSlot>();

        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if (ScrollAdapter[i].Data != null)
            {
                for (int j = 0; j < ScrollAdapter[i].Data.List.Count; j++)
                {
                    if (ScrollAdapter[i].GetItemViewsHolder(j) != null)
                    {
                        for (int k = 0; k < ScrollAdapter[i].GetItemViewsHolder(j).SlotArr.Length; k++)
                        {
                            if (ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].Data.QuickSlotInfos[k] != null&& DBItem.GetItem(ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].Data.QuickSlotInfos[k].TableID) != null && DBItem.GetItem(ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k].Data.QuickSlotInfos[k].TableID).GroupID == _itemGroupID)
                            {
                                result.Add(ScrollAdapter[i].GetItemViewsHolder(j).SlotArr[k]);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    public bool UseItem(QuickSlotInfo _info, Action _callbackAction = null)
    {
        ZItem havItem = null;

        if (DBItem.GetItem(_info.TableID, out var itemTable) == false)
            return false;

        if (DBItem.IsEquipItem(itemTable.ItemType))
            havItem = Me.CurCharData.GetItemData(_info.UniqueID, NetItemType.TYPE_EQUIP);
        else
            havItem = Me.CurCharData.GetItemData(_info.UniqueID, NetItemType.TYPE_STACK);

        if (havItem != null && havItem.cnt > 0)
        {
            if (havItem.UseTime + (ulong)(itemTable.CoolTime * 1000) > TimeManager.NowMs)
            {
                if (_info.bAuto == false)
                {
                    UICommon.SetNoticeMessage("쿨타임입니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                return false;
            }

            if (havItem.IsLock)
            {
                if (false == _info.bAuto)
                {
                    UICommon.SetNoticeMessage("잠금 처리된 아이템은 사용 할 수 없습니다.", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                return false;
            }

            if (itemTable.ItemType == E_ItemType.HPPotion)  // 퀵슬롯에서 hp포션사용 할 경우가 없는데..
            {
                if (ZWebManager.Instance.WebGame.UseItemAction(havItem, false, (useitemId, useitemTid) => {  }))
                {
                }
                return false;
            }

            ZWebManager.Instance.WebGame.UseItemAction(havItem, false, (useitemId, useitemTid) =>
            {
                if (itemTable.CoolTime > 0)
                {
                    if (!UIManager.Instance.Find(out UISubHUDQuickSlot _quickSlotHUD))
                        return;

                    List<UIQuickItemSlot> quickSlotListGlobal = _quickSlotHUD.GetGroupByItem(itemTable.GroupID);
                    if (quickSlotListGlobal != null)
                        for (int j = 0; j < quickSlotListGlobal.Count; j++)
                        {
                            quickSlotListGlobal[j].GlobalCoolTimeUTS = TimeManager.NowMs;
                            quickSlotListGlobal[j].EndCoolTime(1);
                        }

                    List<UIQuickItemSlot> quickSlotList = _quickSlotHUD.GetQuickListSlot(itemTable.ItemID);
                    if (quickSlotList != null)
                        for (int i = 0; i < quickSlotList.Count; i++)
                            quickSlotList[i].EndCoolTime(itemTable.CoolTime);
                }
            });

            _callbackAction?.Invoke();
            return true;
        }

        return false;
    }

    //전체 퀵슬롯의 자동 사용을 체크
    //예외처리 한 루프에 동작 가능한 가챠 오토는 한가지이다
    //이외의 아이템은 즉시 사용 가능하다.
    int AutoGachaIndex;
    //한번의 루프에서 한번만 사용 가능한 제한을 둘 경우 쓴다.
    List<E_ItemType> AutoUseBlockTypes = new List<E_ItemType>();
	private void CheckAuto()
	{
        if (!IsAutoCheck)
            return;

        AutoUseBlockTypes.Clear();

        for (int i = 0; i < ScrollAdapter.Count; i++)
        {
            if (ScrollAdapter[i].Data != null)
            {
                for (int j = 0; j < ScrollAdapter[i].Data.List.Count; j++)
                {
                    for (int k = 0; k < ScrollAdapter[i].Data[j].QuickSlotInfos.Length; k++)
                    {
                        if (ScrollAdapter[i].Data[j].QuickSlotInfos[k] != null && ScrollAdapter[i].Data[j].QuickSlotInfos[k].bAuto)
                        {
                            var useItemType = DoAutoAction(ScrollAdapter[i].Data[j].QuickSlotInfos[k], i, j * ZUIConstant.QUICK_SLOT_PAGE_COUNT + k, AutoUseBlockTypes.ToArray());

                            //사용시 예외처리를 위해 사용된 아이템 타입을 확인한다.
                            switch (useItemType)
                            {
                                case E_ItemType.ItemGacha:
                                case E_ItemType.PetGacha:
                                case E_ItemType.ChangeGacha:
                                case E_ItemType.VehicleGacha:
                                    AutoUseBlockTypes.Add(E_ItemType.ItemGacha);
                                    AutoUseBlockTypes.Add(E_ItemType.PetGacha);
                                    AutoUseBlockTypes.Add(E_ItemType.ChangeGacha);
                                    AutoUseBlockTypes.Add(E_ItemType.VehicleGacha);
                                    AutoGachaIndex = j * ZUIConstant.QUICK_SLOT_PAGE_COUNT + k;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 자동 사용 구문
    /// </summary>
    /// <param name="_slotInfo"></param>
    /// <param name="_continerIndx"></param>
    /// <param name="_slotIndx"></param>
    /// <param name="blockUseTypes"></param>
    /// 이번 루프에서 사용이 제한된 타입
    /// <returns></returns>
	public E_ItemType DoAutoAction(QuickSlotInfo _slotInfo, int _continerIndx, int _slotIndx , params E_ItemType[] blockUseTypes)
    {
        if (DBItem.GetItem(_slotInfo.TableID, out var itemTable) == false)
            return (E_ItemType)0;

        // GetItem 테이블 아이디로 만 찾는거 위험함.
        ZItem testitem = Me.CurCharData.GetItem(_slotInfo.TableID);
        if (testitem == null || testitem.cnt <= 0)
            return (E_ItemType)0;

        if (_slotInfo.bAuto)
        {
            if (blockUseTypes != null)
            {
                for (int i = 0; i < blockUseTypes.Length; i++)
                {
                    if (blockUseTypes[i] == itemTable.ItemType)
                        return (E_ItemType)0;
                }
            }

            switch (itemTable.ItemType)
            {
                case E_ItemType.Change:
                    {
                        //5초전에 미리 쏘자!
                        if(Me.CurCharData.ChangeExpireDt > TimeManager.NowSec + DBConfig.Buff_Pre_Start_Time)
                            return (E_ItemType)0;

                        if (Me.CurCharData.MainChange > 0 && Me.CurCharData.GetChangeDataByTID(Me.CurCharData.MainChange) != null)
                        {
                            ZWebManager.Instance.WebGame.REQ_EquipChange(Me.CurCharData.GetChangeDataByTID(Me.CurCharData.MainChange).ChangeId,
                                Me.CurCharData.GetChangeDataByTID(Me.CurCharData.MainChange).ChangeTid, _slotInfo.UniqueID, (redvPacket, recvMsg) =>
                                {
                                    RefreshAllSlot();
                                    UIManager.Instance.Close<UIFramePetChangeSelect>();
                                });

                            return itemTable.ItemType;
                        }
                        else
                        {
                            // 메인 변신이 없을 경우.
                            AutoOffEvent(_continerIndx, _slotIndx);
                            UICommon.SetNoticeMessage("변신등록 해주세요.(임시 메시지)", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                        }
                    }
                    break;

                case E_ItemType.PetSummon:
                    {
                        //5초전에 미리 쏘자!
                        if (Me.CurCharData.PetExpireDt > TimeManager.NowSec + DBConfig.Buff_Pre_Start_Time)
                            return (E_ItemType)0;

                        if (Me.CurCharData.MainPet > 0 && Me.CurCharData.GetPetData(Me.CurCharData.MainPet) != null)
                        {
                            ZWebManager.Instance.WebGame.REQ_EquipPet(Me.CurCharData.GetPetData(Me.CurCharData.MainPet).PetId,
                                Me.CurCharData.GetPetData(Me.CurCharData.MainPet).PetTid, _slotInfo.UniqueID, (redvPacket, recvMsg) =>
                                {
                                    RefreshAllSlot();
                                    UIManager.Instance.Close<UIFramePetChangeSelect>();
                                });

                            return itemTable.ItemType;
                        }
                        else
                        {
                            // 메인 팻이 없을 경우.
                            AutoOffEvent(_continerIndx, _slotIndx);
                            UICommon.SetNoticeMessage("펫등록 해주세요.(임시 메시지)", new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                        }
                    }
                    break;

                case E_ItemType.ItemGacha:
                case E_ItemType.PetGacha:
                case E_ItemType.ChangeGacha:
                case E_ItemType.VehicleGacha:                    
                    {
                        UseItem(_slotInfo);

                        return itemTable.ItemType;
                    }
                    break;

                case E_ItemType.AttackBuff:
                case E_ItemType.DefenseBuff:
                case E_ItemType.EvasionBuff:
                case E_ItemType.EventBuff:
                case E_ItemType.SpeedBuff:
                    {
                        Item_Table item = DBItem.GetItem(_slotInfo.TableID); // itemTable으로 교체 가능할듯.
                        if (ZPawnManager.Instance.MyEntity != null)
                        {
                            var speedbuffData = ZPawnManager.Instance.MyEntity.FindAbilityAction(item.AbilityActionID_01);
                            if (speedbuffData != null && speedbuffData.EndServerTime > TimeManager.NowSec + DBConfig.Buff_Pre_Start_Time)
                            {
                                return (E_ItemType)0;
                            }

                            UseItem(_slotInfo);

                            return itemTable.ItemType;
                        }
                    }
                    break;

                case E_ItemType.GodTear:
                    {
                        UseItem(_slotInfo);

                        return itemTable.ItemType;
                    }
                    break;
            }
        }

        return (E_ItemType)0;
    }

    private void AutoOffEvent(int ContainerIdx, int SlotIdx)
    {
        QuickSlotInfo info = Me.CurCharData.GetQuickSlotInfo(ContainerIdx, SlotIdx);
        ulong UniqueID = info.SlotType == QuickSlotType.TYPE_ITEM ? info.UniqueID : 0;

        Me.CurCharData.UpdateQuickSlotItem(ContainerIdx, SlotIdx, info.SlotType, UniqueID, info.TableID, false);

        ZWebManager.Instance.WebGame.REQ_SetCharacterOption((ContainerIdx == 0 ?
            WebNet.E_CharacterOptionKey.QuickSlot_Set1 :
            WebNet.E_CharacterOptionKey.QuickSlot_Set2),
            Me.CurCharData.GetQuickSlotValue(ContainerIdx),
            (recvPacket, recvMsgPacket) => {
                RefreshAllSlot();
            });
    }
}