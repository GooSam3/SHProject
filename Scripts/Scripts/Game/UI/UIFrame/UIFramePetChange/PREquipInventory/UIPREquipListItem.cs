using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPREquipListHolder : ZGridAdapterHolderBase<PREquipItemData>
{
    private UIPREquipListItem listItem;

    public override void CollectViews()
    {
        base.CollectViews();

        listItem = root.GetComponent<UIPREquipListItem>();
    }

    public override void SetSlot(PREquipItemData data)
    {
        listItem.SetSlot(data);
    }

    public void SetAction(Action<PREquipItemData> onClick)
    {
        listItem.SetAction(onClick);
    }
}

public class UIPREquipListItem : MonoBehaviour
{
    [SerializeField] private GameObject objVisible;
    [SerializeField] private GameObject objInvisible;

    [SerializeField] private GameObject objEquip;
    [SerializeField] private GameObject objSelect;

    [SerializeField] private Text txtEnchant;

    [SerializeField] private Image imgGradeStar;
    [SerializeField] private Image imgSetType;
    [SerializeField] private Image imgGrade;
    [SerializeField] private Image imgIcon;

    [SerializeField] private GameObject objDisable;

    public PREquipItemData data { get; private set; }

    private Action<PREquipItemData> onClick;

    public void SetSlot(PREquipItemData _data)
    {
        data = _data;

        objVisible.SetActive(data.isVisible);
        objInvisible.SetActive(!data.isVisible);

        if (data.isVisible)
        {
            //SetSlot(data.data);

            RefreshUI();
            objDisable.SetActive(data.isDisable);
        }
    }

    public void SetSlot(PetRuneData _data, bool tempEquiped = false)
    {
        if (data == null)
            data = new PREquipItemData(_data);
        else
            data.Reset(_data);

        if (data.data.OwnerPetTid <= 0 && tempEquiped)
            data.isTempEquiped = true;

        RefreshUI();
    }

    private void RefreshUI()
    {
        var table = DBItem.GetItem(data.data.RuneTid);

        imgGrade.sprite = UICommon.GetGradeSprite((byte)(table.RuneGradeType+1));
        imgGradeStar.sprite = UICommon.GetRuneGradeStarSprite(table.Grade);
        imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);
        imgSetType.sprite = UICommon.GetRuneSetTypeSprite(table.RuneSetType);

        txtEnchant.text = UICommon.GetEnchantText(DBRune.GetRuneEnchantTable(data.data.BaseEnchantTid).EnchantStep);

        objEquip.SetActive(data.data.OwnerPetTid > 0 || data.isTempEquiped);
        objDisable.SetActive(false);
    }

    public void SetAction(Action<PREquipItemData> _onClick)
    {
        onClick = _onClick;
    }

    public void OnClick()
    {
        onClick?.Invoke(data);
    }
}
