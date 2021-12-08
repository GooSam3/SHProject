using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolTipBuffList : MonoBehaviour
{
    [SerializeField] private GameObject objToolTip;

    [SerializeField] private Image buffIcon;
    [SerializeField] private Text buffName;

    [SerializeField] UIAbilityListAdapter abilityScroller;

    private List<UIAbilityData> listDetail = new List<UIAbilityData>();
    private Action onClickClose;

    public void Initialize(Action _onClickClose)
    {
        abilityScroller.Initialize();

        onClickClose = _onClickClose;

        SetActiveState(false);
    }

    public void OnClickClose()
    {
        onClickClose?.Invoke();
    }

    public void SetActiveState(bool b)
    {
        objToolTip.gameObject.SetActive(b);
    }

    public void SetToolTipData(UIFrameBuffList.C_CustomAbilityAction toolTipData)
    {
        AbilityAction_Table table = toolTipData.uniqueTable;

        buffIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.BuffIconString);
        buffName.text = DBLocale.GetText(table.NameText);

        listDetail.Clear();

        listDetail.Add(new UIAbilityData() { viewType = E_UIAbilityViewType.Text, textLeft = "능력", textRight = table.ToolTip });
        listDetail.Add(new UIAbilityData(E_UIAbilityViewType.Blank));

        if (table.AbilityViewType != E_AbilityViewType.ToolTip)
        {
            foreach (var iter in toolTipData.dicAbiliyActionValue)
            {
                string key = iter.Key.ToString();
                listDetail.Add(new UIAbilityData(iter.Key, iter.Value));
            }
        }

        float supportTime = toolTipData.uniqueTable.MinSupportTime;
        string duration = string.Empty;
        if (supportTime <= 0) // 적용시간 없음
        {
            if (table.AbilityActionType == E_AbilityActionType.Passive)
                duration = DBLocale.GetText("PassiveSkill");
        }
        else
        {
            
               duration = TimeHelper.GetRemainTime((ulong)toolTipData.uniqueTable.MinSupportTime);

            if((int)toolTipData.uniqueTable.MaxSupportTime>0)
            {
                duration += $" ~ {TimeHelper.GetRemainTime((ulong)toolTipData.uniqueTable.MaxSupportTime)}";
            }
        }

        if (string.IsNullOrEmpty(duration) == false)
        {
            listDetail.Add(new UIAbilityData(E_UIAbilityViewType.Blank));
            listDetail.Add(new UIAbilityData() { viewType = E_UIAbilityViewType.Text, textLeft = DBLocale.GetText("Buff_Duration"), textRight = duration });
        }

        abilityScroller.RefreshListData(listDetail);
    }
}
