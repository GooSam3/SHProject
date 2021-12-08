using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPetChangeReplaceViewHolder : ZAdapterHolderBase<GachaKeepData>
{
    private UIPetChangeReplaceListItem listItem;

    public override void CollectViews()
    {
        listItem = root.GetComponent<UIPetChangeReplaceListItem>();
        listItem.Initialize();
        base.CollectViews();
    }

    public override void SetSlot(GachaKeepData data)
    {
        listItem.SetSlot(data);
    }

    public void RefreshRemainTime()
    {
        listItem.RefreshRemainTime();
    }

    public void SetEvent(Action<GachaKeepData> _onClickReplace, Action<GachaKeepData> _onClickConfirm, Action<GachaKeepData> _onClickDetail)
    {
        listItem.SetEvent(_onClickReplace, _onClickConfirm, _onClickDetail);
    }
}

public class UIPetChangeReplaceListItem : MonoBehaviour
{
    [SerializeField] private Image classIcon;
    [SerializeField] private Image elementIcon;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image gradeImage;

    [SerializeField] private Text classTitle;
    [SerializeField] private Text mainTitle;
    [SerializeField] private Text txtLeftDay;

    [SerializeField] private UIAbilityListAdapter scrollAbility;

    [SerializeField] private Text txtCost;
    [SerializeField] private Image goodsIcon;

    [SerializeField] private Text txtLeftReplaceCount;

    [SerializeField] private ZButton btnReplace;

    private GachaKeepData keepData;

    private Action<GachaKeepData> onClickReplace;
    private Action<GachaKeepData> onClickConfirm;
    private Action<GachaKeepData> onClickDetail;
    public void Initialize()
    {
        scrollAbility.Initialize();
    }

    // 특정 타입에서만 꺼지거나 켜지는 녀석들 꺼줌
    public void SetUnsharedObject(bool state)
    {
        classIcon.gameObject.SetActive(state);
        elementIcon.gameObject.SetActive(state);
        classTitle.gameObject.SetActive(state);
        btnReplace.interactable = true;
    }

    public void SetSlot(GachaKeepData data)
    {
        keepData = data;
        SetUnsharedObject(data.KeepType == E_GachaKeepType.Change);


        goodsIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBItem.GetItemIconName(DBConfig.Diamond_ID));
        txtCost.text = DBConfig.CardChange_Diamond.ToString();

        int leftCnt = (int)DBConfig.CardChange_Change_Count - (int)data.ReOpenCnt;

        if (leftCnt <= 0)
        {
            leftCnt = 0;
            btnReplace.interactable = false;
        }

        txtLeftReplaceCount.text = DBLocale.GetText("PCR_Replace_ReplaceCount", leftCnt);

        RefreshRemainTime();

        switch (data.KeepType)
        {
            case E_GachaKeepType.Pet:
            case E_GachaKeepType.Ride:
                if (DBPet.TryGet(data.Tid, out var pTable))
                {
                    SetSlot(pTable);
                }
                break;

            case E_GachaKeepType.Change:
                if (DBChange.TryGet(data.Tid, out var cTable))
                {
                    SetSlot(cTable);
                }
                break;
        }
    }

    public void RefreshRemainTime()
    {
        if (keepData == null)
            return;

        var remainTime = ((long)keepData.CreateDt + (long)DBConfig.CardChange_ChangeTime - (long)TimeManager.NowSec);

        if (remainTime < 0)
        {
            txtLeftDay.text = DBLocale.GetText("PCR_Replace_Timeout");
            btnReplace.interactable = false;
        }
        else
            txtLeftDay.text = TimeHelper.GetRemainTime((ulong)remainTime);
    }

    public void SetEvent(Action<GachaKeepData> _onClickReplace, Action<GachaKeepData> _onClickConfirm,Action<GachaKeepData> _onClickDetail)
    {
        onClickReplace = _onClickReplace;
        onClickConfirm = _onClickConfirm;
        onClickDetail = _onClickDetail;
    }

    public void SetSlot(Change_Table table)
    {
        classIcon.sprite = UICommon.GetClassIconSprite(table.ClassIcon, UICommon.E_SIZE_OPTION.Midium);
        elementIcon.sprite = UICommon.GetAttributeSprite(table.AttributeType, UICommon.E_SIZE_OPTION.Midium);
        mainImage.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
        gradeImage.sprite = UICommon.GetGradeSprite(table.Grade);

        classTitle.text = table.ChangeQuestType == E_ChangeQuestType.AttackShort ? DBLocale.GetText("Short_Text") : DBLocale.GetText("Long_Text");
        mainTitle.text = DBLocale.GetText(table.ChangeTextID);

        //List<UIAbilityData> listAbilityPair = new List<UIAbilityData>();

        //foreach (var iter in table.AbilityActionIDs)
        //{
        //    DBAbilityAction.GetAbilityTypeList(iter, ref listAbilityPair);
        //}

        scrollAbility.RefreshListData(UIStatHelper.GetChangeStat(table));
    }

    public void SetSlot(Pet_Table table)
    {
        mainImage.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
        gradeImage.sprite = UICommon.GetGradeSprite(table.Grade);

        mainTitle.text = DBLocale.GetText(table.PetTextID);

        List<UIAbilityData> listAbilityPair = UIStatHelper.GetPetStat(table);

        if (table.PetType == E_PetType.Vehicle)
            DBAbilityAction.GetAbilityTypeList(table.RideAbilityActionID, ref listAbilityPair);

        scrollAbility.RefreshListData(listAbilityPair);
    }

    public void OnClickReplace()
    {
        onClickReplace?.Invoke(keepData);
    }

    public void OnClickConfirm()
    {
        onClickConfirm?.Invoke(keepData);
    }

    public void OnClickDetail()
	{
        onClickDetail?.Invoke(keepData);

    }
}
