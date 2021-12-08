using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIPetChangeToolTip : MonoBehaviour
{
    #region UIVariables

    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    [SerializeField] private GameObject activeState;
    [SerializeField] private GameObject lockState;
    [SerializeField] private ZButton confirmBtn;
    [SerializeField] private Text confirmText;

    [SerializeField] private GameObject btnUnEquip;

    #endregion UIVariables

    #region SystemVariables

    [SerializeField] private UIAbilityListAdapter scrollList;

    private Action onClickConfirm;

    private C_PetChangeData curData;

    #endregion SystemVariables

    public void Initialize(Action _onClickConfirm)
    {
        scrollList.Initialize();
        onClickConfirm = _onClickConfirm;
    }

    public void SetDefualt()
    {
        icon.gameObject.SetActive(false);
        title.gameObject.SetActive(false);
        activeState.SetActive(false);
        lockState.SetActive(false);

        confirmBtn.interactable = true;
    }

    public void Refresh()
    {
        if (this.gameObject.activeSelf == false)
            return;

        confirmBtn.interactable = true;
        SetToolTipData(curData);
    }

    public void SetToolTipData(C_PetChangeData data)
    {
        curData = data;

        SetDefualt();

        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        switch (data.type)
        {
            case E_PetChangeViewType.Change:
                SetToolTipData(data.changeData, ref listAbility);
                break;

            case E_PetChangeViewType.Pet:
                SetToolTipData(data.petData, ref listAbility);
                break;

            case E_PetChangeViewType.Ride:
                SetToolTipData(data.petData, ref listAbility);
                break;
        }

        scrollList.RefreshListData(listAbility);

        SetActive(true);
    }

    public void SetToolTipData(Change_Table _changeData, ref List<UIAbilityData> listAbility)
    {
        //foreach (var abilId in _changeData.AbilityActionIDs) 
        //{
        //    if (DBAbilityAction.TryGet(abilId, out var abilTable))
        //        DBAbilityAction.GetAbilityTypeList(abilTable, ref listAbility);
        //}

        var myChangeData = Me.CurCharData.GetChangeDataByTID(_changeData.ChangeID);

        listAbility = UIStatHelper.GetChangeStat(_changeData);

        icon.gameObject.SetActive(true);
        title.gameObject.SetActive(true);

        activeState.SetActive(Me.CurCharData.MainChange == _changeData.ChangeID);
        lockState.SetActive(myChangeData.IsLock);

        icon.sprite = UICommon.GetClassIconSprite(_changeData.ClassIcon, UICommon.E_SIZE_OPTION.Midium);
        title.text = DBLocale.GetText(_changeData.ChangeTextID);

        //## 기획사항 : 변신 및 해제 둘중하나만 출력

        bool isSummon = _changeData.ChangeID == Me.CurCharData.MainChange;

        bool isSpawnable = !isSummon;
        if (isSummon)// 남은시간 없다면 등록해제로처리
            isSpawnable = Me.CurCharData.ChangeExpireDt < TimeManager.NowSec;


        confirmText.text = DBLocale.GetText("Confirm_Change");

        confirmBtn.gameObject.SetActive(isSpawnable);
        btnUnEquip.SetActive(!isSpawnable);
    }

    public void SetToolTipData(Pet_Table _petData, ref List<UIAbilityData> listAbility)
    {
        listAbility = UIStatHelper.GetPetStat(_petData);

        var myPetData = Me.CurCharData.GetPetData(_petData.PetID);

        confirmBtn.interactable = myPetData.AdvId <= 0;

        title.gameObject.SetActive(true);

        lockState.SetActive(Me.CurCharData.GetPetData(_petData.PetID)?.IsLock ?? false);

        title.text = DBLocale.GetText(_petData.PetTextID);

        //## 기획사항 : 소환 및 해제 둘중하나만 출력

        if (_petData.PetType == E_PetType.Pet)
        {
            bool isSummon = _petData.PetID == Me.CurCharData.MainPet;
            confirmText.text =DBLocale.GetText("Pet_Directly_Text");

            activeState.SetActive(Me.CurCharData.MainPet == _petData.PetID);


            bool isSpawnable = !isSummon;
            if (isSummon)// 남은시간 없다면 등록해제로처리
                isSpawnable = Me.CurCharData.PetExpireDt < TimeManager.NowSec;


            confirmBtn.gameObject.SetActive(isSpawnable);
            btnUnEquip.SetActive(!isSpawnable);
        }
        else if (_petData.PetType == E_PetType.Vehicle)
        {
            DBAbilityAction.GetAbilityTypeList(_petData.RideAbilityActionID, ref listAbility);

            bool isSummon = _petData.PetID == Me.CurCharData.MainVehicle;
            confirmText.text =  DBLocale.GetText("Equip_Text");

            activeState.SetActive(Me.CurCharData.MainVehicle == _petData.PetID);
            confirmBtn.gameObject.SetActive(!isSummon);
            btnUnEquip.SetActive(isSummon);
        }
    }


    // 탈것용 테이블 추가시 SetToolTipData(T rideData)함수 추가필요

    public void SetActive(bool state)
    {
        this.gameObject.SetActive(state);
    }

    public void OnClickConfirm()
    {
        if (curData == null) return;

        ReqEquip();
    }

    public void OnClickUnEquip()
    {
        if (curData == null) return;

        ReqUnEquip();

        onClickConfirm?.Invoke();
    }

    private void ReqEquip()
    {

        switch (curData.type)
        {
            case E_PetChangeViewType.Change:
                {

                    //강림 소모아이템 비교
                    ZItem matItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Change_Use_Item);
                    if (matItem == null || matItem.cnt <= 0)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_CHANGE_USE_ITEM"));
                        return;
                    }

                    ZWebManager.Instance.WebGame.REQ_EquipChange(curData.Id, curData.Tid, matItem.item_id, (redvPacket, recvMsg) =>
                    {
                        UIManager.Instance.Close<UIFramePetChangeSelect>();
                        onClickConfirm?.Invoke();
                    });
                }

                break;
            case E_PetChangeViewType.Pet:
                {
                    //강림 소모아이템 비교
                    ZItem matItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Pet_Summon_Item);
                    if (matItem == null || matItem.cnt <= 0)
                    {
                        UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_PET_USE_ITEM"));
                        return;
                    }


                    ZWebManager.Instance.WebGame.REQ_EquipPet(curData.Id, curData.Tid, matItem.item_id, (redvPacket, recvMsg) =>
                    {
                        UIManager.Instance.Close<UIFramePetChangeSelect>();
                        onClickConfirm?.Invoke();
                    });

                }
                break;
            case E_PetChangeViewType.Ride:
                ZMmoManager.Instance.Field.REQ_EquipVehicle(curData.Tid);
                break;
        }
    }

    private void ReqUnEquip()
    {
        switch (curData.type)
        {
            case E_PetChangeViewType.Change:
                ZWebManager.Instance.WebGame.REQ_UnEquipChange((recvPacket, recvMsgPacket) =>
                {
                    onClickConfirm?.Invoke();
                    confirmBtn.interactable = true;
                });

                break;
            case E_PetChangeViewType.Pet:
                ZWebManager.Instance.WebGame.REQ_UnEquipPet((recvPacket, recvMsgPacket) =>
                {
                    onClickConfirm?.Invoke();
                    confirmBtn.interactable = true;
                });
                break;
            case E_PetChangeViewType.Ride:
                ZMmoManager.Instance.Field.REQ_EquipVehicle(0);
                onClickConfirm?.Invoke();
                confirmBtn.interactable = true;
                break;
        }
        confirmBtn.interactable = false;
    }

    public void OnClickClose()
    {
        SetActive(false);
    }
}
