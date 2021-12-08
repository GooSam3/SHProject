using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

// 안씀. 언젠가 쓸 날이 올지도..

public class UIPopupItemInfoPREquipSingle : UIPopupBase
{
    [SerializeField] private UIPopupItemInfoPREquip equipPopup;

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        equipPopup.Initialize();
    }

   // public void SetPopup(PetRuneData _data)
   // {
   //     equipPopup.SetPopup(_data, UIPopupItemInfoPREquip.E_PREquipPopupType.Single);
   // }

   // public void SetPopup(PREquipItemData _data, C_PetChangeData _target, UIPopupItemInfoPREquip.E_PREquipPopupType _type)
   // {
   //     equipPopup.SetPopup(_data, _target, _type);
   // }

   // public void SetPopup(uint runeTid)
   // {
   //     equipPopup.SetPopup(runeTid);
   // }

    public void SetEquipTarget(C_PetChangeData target)
    {
        equipPopup.SetInteractTarget(target);
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIPopupItemInfoPREquipSingle>(true);
    }
}
