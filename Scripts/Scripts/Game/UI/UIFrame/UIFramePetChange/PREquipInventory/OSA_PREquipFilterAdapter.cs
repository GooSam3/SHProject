using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// !!!! 주의 !!!!!
/// 해당하는 슬롯마다 사이즈 체크 필!!!!
/// == sortmode == 
/// set : 350,120
/// status : 310,68
/// equipdetail-textfiler : 210 ,100
/// </summary>
public class OSA_PREquipFilterAdapter : ZScrollAdapterBase<PREquipFilterData, UIPREquipFilterHolder>
{
    protected override string AddressableKey => nameof(UIPREquipFilterListItem);

    private Action<PREquipFilterData> onClick;

    protected override void OnCreateHolder(UIPREquipFilterHolder holder)
    {
        base.OnCreateHolder(holder);
        holder.SetAction(onClick);
    }

    public void Initialize(Action<PREquipFilterData> _onClick)
	{
        Initialize();
        SetAction(_onClick);
	}

    public void SetAction(Action<PREquipFilterData> _onClick)
    {
        onClick = _onClick;
    }
}

public enum E_PREquipFilterType
{
    None = 0,
    EquipSlot = 1,
    Grade = 2,
    StarGrade = 3,
    SetType = 4,
    AbilityType = 5,
}

[Serializable]
public class PREquipFilterData
{
    public bool isNotUsedInOSA = true;

    [HideInInspector]
    public bool isOn = false;

    // 프리팹 동작 타입
    public E_PREquipFilterType type;

    [SerializeField]
    private E_EquipSlotType slotData; // 동적생성 안하는 놈에서 쓰일 단편적 슬롯 정보(슬롯)
    public uint intData;// 동적생성 안하는놈에서 쓰일 단편적 uint 데이터(등급, 별등급)

    // 세트
    public E_RuneSetType runeSetType;

    // 어빌리티
    public E_RuneAbilityViewType abilityType;

    public PREquipFilterData(E_RuneSetType _setType)
    {
        type = E_PREquipFilterType.SetType;
        runeSetType = _setType;
        isNotUsedInOSA = false;
        isOn = true;
    }

    public PREquipFilterData(E_RuneAbilityViewType _ability)
    {
        type = E_PREquipFilterType.AbilityType;
        abilityType = _ability;
        isNotUsedInOSA = false;
        isOn = true;
    }
    
    // ui세팅용 데이터 하나로묶음(slotdata => intData)
    public void CompressData()
    {
        if (slotData == E_EquipSlotType.None)
            return;

        intData = (uint)slotData;
    }
}