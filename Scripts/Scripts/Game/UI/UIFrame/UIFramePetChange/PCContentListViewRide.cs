using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

// 탈것용
public class PCContentListViewRide : PCContentListViewPR
{
    [SerializeField] private UIAbilitySlot RideAbility;

    protected override void InitilaizeList()
    {
        ListContentData.Clear();

        foreach (var data in DBPet.GetAllRideData())
        {
            if (data.ViewType != GameDB.E_ViewType.View)
                continue;

            if (data.PetType != E_PetType.Vehicle)
                continue;

            uint cnt = Me.CurCharData.GetRideData(data.PetID)?.Cnt ?? 0;

            ListContentData.Add(new C_PetChangeData(data, (int)cnt));
        }
    }

    public override void OnFrameShow()
    {
        ZPawnManager.Instance.DoAddEventEquipRideVehicle(OnEquipVehicle);

        base.OnFrameShow();
    }

    public override void OnFrameHide()
    {
        ZPawnManager.Instance.DoRemoveEquipRideVehicle(OnEquipVehicle);

        base.OnFrameHide();
    }

    protected override void SetUI(C_PetChangeData data)
    {
        base.SetUI(data);

        Pet_Table tableData = data.petData;

        SelectPetName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(tableData.PetTextID), tableData.Grade);

        List<UIAbilityData> listAbility = new List<UIAbilityData>();

        DBAbilityAction.GetAbilityTypeList(tableData.RideAbilityActionID, ref listAbility);

        // 일단 한개만..!

        RideAbility.SetSlot(new UIAbilityData(listAbility[0].type, listAbility[0].value));

        hasValue = data.Id > 0;

        isRegisted = Me.CurCharData.MainVehicle == data.Tid;

        SummonButton.interactable = hasValue;

        if (hasValue  == false)
        {
            txtConfirm.text = DBLocale.GetText("Equip_Text");
        }
        else
        {
            txtConfirm.text = isRegisted ? DBLocale.GetText("Unregister_Button") : DBLocale.GetText("Equip_Text");
        }
    }

    private void ReqEquipVehicle()
    {
        if (isRegisted)
            ZMmoManager.Instance.Field.REQ_EquipVehicle(0);
        else
            ZMmoManager.Instance.Field.REQ_EquipVehicle(CurSelectedData.Tid);

        SummonButton.interactable = false;
    }

    public override void OnClickConfirm()
    {
        if (CurSelectedData.type != E_PetChangeViewType.Ride)
        {
            ZLog.LogError(ZLogChannel.Pet, "들어오면 안됨!!! NULL!!");
            return;
        }

        if (CurSelectedData.petData.PetType != E_PetType.Vehicle)
            return;

        ReqEquipVehicle();
        SummonButton.interactable = false;
    }

    private void OnEquipVehicle(uint tID)
    {
        if (Checker.isOn == false)
            return;

        ScrollPetChange.RefreshData();
        SetUI(CurSelectedData);
    }

    protected override S_PCRResourceData GetResourceData()
    {
        Pet_Table data = CurSelectedData.petData;
        return new S_PCRResourceData() { FileName = data.ResourceFile, ViewScale = data.ViewScale, ViewPosY = data.ViewScaleLocY, Grade = data.Grade, ViewRotY = 0f };
    }
}
