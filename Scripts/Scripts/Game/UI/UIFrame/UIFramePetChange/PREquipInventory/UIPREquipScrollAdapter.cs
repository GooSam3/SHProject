using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;

public class UIPREquipScrollAdapter : ZGridScrollAdapter<PREquipItemData, UIPREquipListHolder>
{
    public override string AddressableKey => nameof(UIPREquipListItem);

    private Action<PREquipItemData> onClick;

    public void SetAction(Action<PREquipItemData> _onClick)
    {
        onClick = _onClick;
    }

    protected override void OnUpdateViewHolder(UIPREquipListHolder holder)
    {
        base.OnUpdateViewHolder(holder);
        holder.SetAction(onClick);
    }

    public void Initialize(Action<PREquipItemData> _onClick)
    {
        SetAction(_onClick);

        Initialize();
    }
}

public class PREquipItemData
{
    public PetRuneData data;

    public bool SortFirst = false;
    public bool isDisable = false;

    public bool isUseTempEquip = false;
    public bool isTempEquiped = false;

    public bool isMoved = false;// 인벤토리에서 잠깐 사라지는녀석(판매)

    public bool isVisible = true;

    public Item_Table itemTable;
    public RuneEnchant_Table enchantTable;

    public PREquipItemData()
    {
        data = null;
        isVisible = false;
    }

    public PREquipItemData(PetRuneData _data)
    {
        data = _data;
        isVisible = true;

        itemTable = DBItem.GetItem(_data.RuneTid);
        enchantTable = DBRune.GetRuneEnchantTable(_data.BaseEnchantTid);
    }

    public void Reset(PetRuneData _data)
    {
        data = _data;
        isVisible = true;

        isMoved = false;

        SortFirst = false;
        isDisable = false;

        isUseTempEquip = false;
        isTempEquiped = false;

        itemTable = DBItem.GetItem(_data.RuneTid);
        enchantTable = DBRune.GetRuneEnchantTable(_data.BaseEnchantTid);
    }

    public void Reset(PREquipItemData _data)
    {
        Reset(_data.data);
    }
}
