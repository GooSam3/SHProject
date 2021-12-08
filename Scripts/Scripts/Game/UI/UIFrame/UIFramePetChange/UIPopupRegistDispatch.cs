using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
using UnityEngine.UI;

public class UIPopupRegistDispatch : UIPopupBase
{
    [SerializeField] private UIPetChangeScrollAdapter osaChange;

    [SerializeField] private Text txtFilterNotice;

    [SerializeField] private ZButton btnConfirm;

    private Action<C_PetChangeData> onClickConfirm;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        osaChange.Initilize();
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        UIManager.Instance.Close<UIScreenBlock>();

        btnConfirm.interactable = false;
    }

    public void SetPopup(List<uint>listRegisted, byte grade, E_ChangeQuestType type,  Action<C_PetChangeData> _onClickConfirm)
    {
        var list = Me.CurCharData.GetChangeDataList();
        var mainChange = Me.CurCharData.MainChange;
        var now = TimeManager.NowSec;

        onClickConfirm = _onClickConfirm;

        List<C_PetChangeData> listData = new List<C_PetChangeData>();

        foreach (var iter in list)
        {
            if (DBChange.TryGet(iter.ChangeTid, out var table) == false)
                continue;

            if (DBChangeQuest.CheckChangeDispatchable(iter, grade, type) == false)
                continue;

            if (listRegisted.Contains(iter.ChangeTid))
                continue;

            listData.Add(new C_PetChangeData(iter));
        }

        osaChange.ResetData(listData, OnClickSlot);

        string filterType = string.Empty;

        if (type.HasFlag(E_ChangeQuestType.AttackShort))
            filterType += $"{DBLocale.GetText("Short_Text")}";

        if (type.HasFlag(E_ChangeQuestType.AttackLong))
        {

            filterType += $"{(string.IsNullOrEmpty(filterType)?"":"/")}{DBLocale.GetText("Long_Text")}";
        }

        txtFilterNotice.text = DBLocale.GetText("Change_Dispatch_Filter_Notice", filterType, DBLocale.GetGradeLocale(grade));
    }

    private void OnClickSlot(C_PetChangeData data)
    {
        osaChange.SetFocusItem(data);

        btnConfirm.interactable =  osaChange.selectedData != null;
    }

    public void OnClickConfirm()
    {
        onClickConfirm?.Invoke(osaChange.selectedData);
        OnClickClose();
    }

    public void OnClickClose()
    {
        UIManager.Instance.Close<UIPopupRegistDispatch>(true);
    }

}
