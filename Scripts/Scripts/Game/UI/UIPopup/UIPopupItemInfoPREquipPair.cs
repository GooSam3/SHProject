using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPopupItemInfoPREquipPair : UIPopupBase
{
    [SerializeField] private UIPopupItemInfoPREquip equipPopupUp;// 내 펫이 착용한 정보
    [SerializeField] private UIPopupItemInfoPREquip equipPopupDown;// 비교대상 아이템 정보

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.Close<UIScreenBlock>();

        equipPopupUp.Initialize(this);
        equipPopupDown.Initialize(this);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_data"></param>
    /// <param name="_target"></param>
    /// <param name="onInterect"> 상호작용(장착, 임시장착 등등 후 처리) </param>
    /// <param name="isMyPetEquip"></param>
    /// <param name="tempEquip"></param>
    public void SetPopup(PREquipItemData _data, C_PetChangeData _target, Action _onInterect, UIPopupItemInfoPREquip.E_PREquipPopupType type, Action _onEquip = null, bool tempEquipMode = false)
    {
        if (type == UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Up)
            equipPopupUp.SetPopup(_data, _target, _onInterect,_onEquip, tempEquipMode);
        else if (type == UIPopupItemInfoPREquip.E_PREquipPopupType.Pair_Down) 
            equipPopupDown.SetPopup(_data, _target, _onInterect,_onEquip, tempEquipMode);
    }

    public void SetClose()
    {
        equipPopupUp.OnClickClose();
        equipPopupDown.OnClickClose();
    }

    public void SetEquipTarget(C_PetChangeData target)
    {
        equipPopupUp.SetInteractTarget(target);
        equipPopupDown.SetInteractTarget(target);
    }
}
