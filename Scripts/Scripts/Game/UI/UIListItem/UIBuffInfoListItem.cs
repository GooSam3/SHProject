using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UIFrameBuffList;

public class UIBuffInfoListItem : BaseItemViewsHolder
{
    [SerializeField] private Image buffIcon;
    [SerializeField] private Text buffName;
    [SerializeField] private Text buffToolTip;
    [SerializeField] private Text buffRemainTime;
    [SerializeField] private Transform objSelected;

    private Action<C_CustomAbilityAction> onClickSlot;

    private C_CustomAbilityAction buffData;

    //tooltip, nametext

    public void SetSlot(C_CustomAbilityAction _data, Action<C_CustomAbilityAction> _onClickSlot, uint selectedID)
    {
        onClickSlot = _onClickSlot;
        buffData = _data;
        buffData.instance = this;

        objSelected.gameObject.SetActive(selectedID == buffData.uniqueTable.AbilityActionID);

        buffIcon.sprite = ZManagerUIPreset.Instance.GetSprite(buffData.uniqueTable.BuffIconString);
        buffName.text = DBLocale.GetText(buffData.uniqueTable.NameText);
        buffToolTip.text = DBLocale.GetText(buffData.uniqueTable.ToolTip);

        RefreshTime();
    }

    public void RefreshTime()
    {
        buffRemainTime.text = string.Empty;

        if (buffData.uniqueTable.AbilityActionType == GameDB.E_AbilityActionType.Passive)
            return;

        if (buffData.endTime > float.Epsilon)
        {
            long remainTime = buffData.IsNotConsume ? (long)buffData.addedRemainTime : ((long)buffData.endTime - (long)TimeManager.NowSec);

            if (remainTime < 0)
                remainTime = 0;

            buffRemainTime.text = TimeHelper.GetRemainTime((ulong)remainTime);
        }
    }

    public void OnClickSlot()
    {
        onClickSlot?.Invoke(buffData);
    }

    public void SetFocusState(bool state)
    {
        objSelected.gameObject.SetActive(state);
    }

    public override void CollectViews()
    {
        base.CollectViews();

        root.GetComponentAtPath("BuffInfo_Slot/Icon_Buff", out buffIcon);
        root.GetComponentAtPath("BuffInfo_Slot/Txt_Name", out buffName);
        root.GetComponentAtPath("BuffInfo_Slot/Txt_Script", out buffToolTip);
        root.GetComponentAtPath("BuffInfo_Slot/Txt_Time", out buffRemainTime);
        root.GetComponentAtPath("BuffInfo_Slot/Img_SelectLine", out objSelected);

        root.GetComponentAtPath("BuffInfo_Slot", out Button btn);
        btn.onClick.AddListener(OnClickSlot);
    }
}
