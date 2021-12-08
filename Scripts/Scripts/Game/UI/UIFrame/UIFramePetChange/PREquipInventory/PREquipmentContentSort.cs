using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

public class PREquipmentContentSort : PREquipContentBase
{
    [SerializeField] private OSA_PREquipFilterAdapter adapterSetType;
    [SerializeField] private OSA_PREquipFilterAdapter adapterAbilityType;


    private List<PREquipFilterData> listSetType = new List<PREquipFilterData>();
    private List<PREquipFilterData> listAbilityType = new List<PREquipFilterData>();

    public override void Init(PREquipmentInventory owner)
    {
        base.Init(owner);

        foreach (var iter in EnumHelper.Values<E_RuneSetType>())
        {
            if (iter == E_RuneSetType.None)
                continue;

            if (iter == E_RuneSetType.Max)
                continue;

            listSetType.Add(new PREquipFilterData(iter) { isNotUsedInOSA = false, isOn = true });
        }

        foreach (var iter in EnumHelper.Values<E_RuneAbilityViewType>())
        {
            if (iter == E_RuneAbilityViewType.None)
                continue;

            listAbilityType.Add(new PREquipFilterData(iter) { isNotUsedInOSA = false, isOn = true });

        }

        adapterSetType.Initialize();
        adapterSetType.SetAction(OnClickFilterSlot);
        adapterSetType.ResetListData(listSetType);

        adapterAbilityType.Initialize();
        adapterAbilityType.SetAction(OnClickFilterSlot);
        adapterAbilityType.ResetListData(listAbilityType);
    }

    public override void Open()
    {
        base.Open();
    }


    protected override void RefreshFilter(E_PREquipFilterType filter)
    {
        if (filter == E_PREquipFilterType.None || filter == E_PREquipFilterType.SetType)
        {
            foreach (var iter in listSetType)
            {
                iter.isOn = true;
            }
            adapterSetType.RefreshData();
        }

        if (filter == E_PREquipFilterType.None || filter == E_PREquipFilterType.AbilityType)
        {
            foreach (var iter in listAbilityType)
            {
                iter.isOn = true;
            }
            adapterAbilityType.RefreshData();
        }

        base.RefreshFilter(filter);
    }

    public override void OnInvenClick(PREquipItemData data)
    {
        var popup = UIManager.Instance.Find<UIPopupItemInfoPREquipPair>();
        if (popup != null)
        {//정보팝업 열려있음
            popup.SetPopup(data, owner.EquipTarget, ()=>owner.RefreshInven(), UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down);
        }
        else
        {//정보팝업 닫혀있음
            UIManager.Instance.Open<UIPopupItemInfoPREquipPair>((name, popupFrame) =>
            {
                popupFrame.SetPopup(data, owner.EquipTarget, () => owner.RefreshInven(), UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down);
            });
        }
    }

    public override bool Filter(PetRuneData data)
    {
        if (DBItem.GetItem(data.RuneTid, out var table) == false)
            return false;

        if (DBRune.GetRuneEnchantTable(data.BaseEnchantTid, out var enchatTable) == false)
            return false;

        foreach (var iter in listSetType)
        {
            if (iter.isOn)
                continue;

            if (iter.runeSetType == table.RuneSetType)
                return false;
        }

        foreach (var iter in listAbilityType)
        {
            if (iter.isOn)
                continue;

            if (iter.abilityType.ToString().Equals(DBAbility.GetAction(enchatTable.AbilityActionID).AbilityID_01.ToString()))
                return false;
        }

        return base.Filter(data);
    }

    protected override void OnClickFilterSlot(PREquipFilterData data)
    {
        base.OnClickFilterSlot(data);

        if (data.isNotUsedInOSA == true)
            return;
        data.isOn = !data.isOn;

        bool state = data.isOn;

        if (data.type == E_PREquipFilterType.AbilityType)
        {
            adapterAbilityType.RefreshData();
        }
        else
        {
            adapterSetType.RefreshData();
        }
        owner.RefreshInven();
    }
}
