using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPREquipFilterHolder : ZAdapterHolderBase<PREquipFilterData>
{
    private UIPREquipFilterListItem listItem;

    public override void CollectViews()
    {
        listItem = root.GetComponent<UIPREquipFilterListItem>();
        base.CollectViews();
    }

    public override void SetSlot(PREquipFilterData data)
    {
        listItem.SetSlot(data);
    }

    public void SetAction(Action<PREquipFilterData> _onClick)
    {
        listItem.SetAction(_onClick);
    }
}

/// <summary>
/// 경고 : 동적 생성안하는놈들 주의(링크안됬으니 접근하면 터져버림)
/// 동적생성안하는놈 : 장비슬롯, 등급, 별등급
/// </summary>
public class UIPREquipFilterListItem : MonoBehaviour
{
    [Serializable]
    private class PRUIFilterData
    {
        public ZToggle toggle;

        public E_PREquipFilterType type;
        public GameObject obj;
    }

    [SerializeField, Header("Use For None OSA")] private ZToggle toggle;

    [SerializeField] public PREquipFilterData data;

    [Header("Used In OSA Only"), Space(10)]
    [SerializeField] private Image imgSetType;
    [SerializeField] private Text txtSetType;

    [SerializeField] private Text txtAbility;

    [SerializeField] private List<PRUIFilterData> listFilterObj;

    private PRUIFilterData osaUsedData;

    private Action<PREquipFilterData> onClickSlot;

    private bool isInitialized = false;

    // driven by osa
    public void Initialize()
    {
        foreach (var iter in listFilterObj)
        {
            bool isUseData = data.type == iter.type;

            iter.obj.SetActive(isUseData);

            if (isUseData)
                osaUsedData = iter;
        }
        isInitialized = true;
    }

    // driven by osa
    public void SetSlot(PREquipFilterData _data)
    {
        data = _data;

        //if (isInitialized == false)
            Initialize();

        switch (data.type)
        {
            case E_PREquipFilterType.SetType:
                SetUI(data.runeSetType);
                break;
            case E_PREquipFilterType.AbilityType:
                SetUI(data.abilityType);
                break;
        }

        SetState(data.isOn);
    }

    private void SetUI(E_RuneSetType setType)
    {
        Sprite spr = UICommon.GetRuneSetTypeSprite(setType, true);
        imgSetType.sprite = spr;

        string txt = UICommon.GetRuneSetAbilityText(setType);
        txtSetType.text = txt;
    }

    private void SetUI(E_RuneAbilityViewType runeAbilityType)
    {
        string txt = DBLocale.GetRuneAbilityTypeName(runeAbilityType);
        txtAbility.text = txt;
    }

    public void SetAction(Action<PREquipFilterData> _onClick)
    {
        onClickSlot = _onClick;
    }

    public void SetState(bool state)
    {
        if (osaUsedData != null)
        {
            if (osaUsedData.type == E_PREquipFilterType.AbilityType || osaUsedData.type == E_PREquipFilterType.SetType)
            {
                osaUsedData.toggle.SelectToggleSingle(state, false);
            }
            return;
        }

        toggle.SelectToggleSingle(state, false);
    }

    public void OnClickSlot()
    {
        onClickSlot?.Invoke(data);
    }
}


