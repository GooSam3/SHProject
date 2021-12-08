using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

// 강림용
public class PCContentListViewChange : PCContentListViewBase
{
    // # 선택된 강림 정보
    [Header("ChangeStatus"), Space(5)]
    [SerializeField] private Text SelectChangeType;
    [SerializeField] private Text SelectChangeName;
    [SerializeField] private Image SelectChangeClassIcon;
    [SerializeField] private Image SelectChangeAttributeIcon;
    [SerializeField] private Text SelectChangeAttributeLevel;

    [SerializeField] private ZButton ChangeButton;

    [SerializeField] private GameObject UnEquipButton;

    [SerializeField] private EnhanceElementTooltip elementTooltip;

    protected override void InitilaizeList()
    {
        ListContentData.Clear();

        foreach (var data in DBChange.GetAllChangeDatas())
        {
            if (data.ViewType != GameDB.E_ViewType.View)
                continue;

            uint cnt = Me.CurCharData.GetChangeDataByTID(data.ChangeID)?.Cnt ?? 0;

            ListContentData.Add(new C_PetChangeData(data, (int)cnt));
        }

        elementTooltip.HideTooltip();
    }

    protected override List<UIAbilityData> GetAbilityList(C_PetChangeData data)
    {
        //Change_Table tableData = data.changeData;

        

        //List<UIAbilityData> listAbility = new List<UIAbilityData>();

        //foreach (var abilId in tableData.AbilityActionIDs)
        //{
        //    if (DBAbilityAction.TryGet(abilId, out var abilTable))
        //        DBAbilityAction.GetAbilityTypeList(abilTable, ref listAbility);
        //}

        return UIStatHelper.GetChangeStat(data.changeData);
    }

    protected override void SetUI(C_PetChangeData data)
    {
        Change_Table tableData = data.changeData;

        string attackType = tableData.ChangeQuestType == E_ChangeQuestType.AttackShort ? DBLocale.GetText("Short_Text") : DBLocale.GetText("Long_Text");
        string attributeType = string.Empty;

        switch (tableData.AttributeType)
        {
            case E_UnitAttributeType.Fire:
                attributeType = DBLocale.GetText(LOCALE_ELEMENT_FIRE);
                break;
            case E_UnitAttributeType.Water:
                attributeType = DBLocale.GetText(LOCALE_ELEMENT_WATER);
                break;
            case E_UnitAttributeType.Electric:
                attributeType = DBLocale.GetText(LOCALE_ELEMENT_ELECTRIC);
                break;
            case E_UnitAttributeType.Light:
                attributeType = DBLocale.GetText(LOCALE_ELEMENT_LIGHT);
                break;
            case E_UnitAttributeType.Dark:
                attributeType = DBLocale.GetText(LOCALE_ELEMENT_DARK);
                break;
        }

        SelectChangeType.text = string.Format(FORMAT_CHANGE_TYPE, attackType, attributeType);
        SelectChangeName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(tableData.ChangeTextID), tableData.Grade);

        SelectChangeAttributeLevel.text = string.Format(FORMAT_ATTRIBUTE_LEVEL, Me.CurCharData.GetAttributeLevelByType(tableData.AttributeType));

        SelectChangeClassIcon.sprite = UICommon.GetClassIconSprite(tableData.ClassIcon, UICommon.E_SIZE_OPTION.Midium);

        SelectChangeAttributeIcon.sprite = UICommon.GetAttributeSprite(tableData.AttributeType, UICommon.E_SIZE_OPTION.Midium);

        // 미보유

        hasValue = data.Id > 0;
        isRegisted = data.Tid == Me.CurCharData.MainChange;
        ChangeButton.interactable = true;

        if (hasValue)
        {

            bool isSpawnable = !isRegisted;
            if (isRegisted)// 남은시간 없다면 등록해제로처리
                isSpawnable = Me.CurCharData.ChangeExpireDt < TimeManager.NowSec;

            ChangeButton.gameObject.SetActive(isSpawnable);
            UnEquipButton.gameObject.SetActive(!isSpawnable);
        }
        else
		{
            ChangeButton.gameObject.SetActive(false);
            UnEquipButton.gameObject.SetActive(false);
        }

        // 기획사항 : 변신 및 해제 둘중하나만 출력, 아래코드는 혹시모르니 놔둠

        //ChangeButton.interactable = hasValue;
        //
        //UnEquipButton.gameObject.SetActive(hasValue && isRegisted);
        //
        //if (Me.CurCharData.GetChangeDataByTID(tableData.ChangeID) == null|| (isRegisted&&Me.CurCharData.ChangeExpireDt>TimeManager.NowSec))
        //    ChangeButton.interactable = false;
        //else
        //    ChangeButton.interactable = true;
        //
        //UnEquipButton.gameObject.SetActive(hasValue && isRegisted);

        elementTooltip.HideTooltip();
    }

    public void OnClickUnEquip()
    {
        if (!isRegisted)
        {
            ChangeButton.gameObject.SetActive(true);
            UnEquipButton.gameObject.SetActive(false);
            return;
        }

        ZWebManager.Instance.WebGame.REQ_UnEquipChange((recvPacket, recvMsgPacket) =>
        {
            ScrollPetChange.RefreshData();

            SetUI(CurSelectedData);
        });
    }

    public override void OnClickConfirm()
    {
        if (CurSelectedData.type != E_PetChangeViewType.Change)
        {
            ZLog.LogError(ZLogChannel.Pet, "들어오면 안됨!!! NULL!!");
            return;
        }

        //강림 있는지 한번더확인
        ChangeData changeData = Me.CurCharData.GetChangeDataByTID(CurSelectedData.Tid);
        if (changeData == null)
        {
            // 만약 없다면 버튼꺼줌
            ChangeButton.interactable = false;
            return;
        }

        //강림 소모아이템 비교
        ZItem matItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Change_Use_Item);
        if (hasValue == false || (matItem == null || matItem.cnt <= 0))
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_CHANGE_USE_ITEM"));
            return;
        }

        //강림
        ZWebManager.Instance.WebGame.REQ_EquipChange(changeData.ChangeId,
                                                     changeData.ChangeTid,
                                                     matItem.item_id,
                                                     (recvPacket, recvMsgPacket) =>
                                                     {
                                                         ChangeButton.interactable = true;
                                                             // 강림패널꺼줌
                                                             UIManager.Instance.Find<UIFrameChange>()?.OnClickClose();
                                                     },
                                                     null);
        ChangeButton.interactable = false;
    }

    protected override S_PCRResourceData GetResourceData()
    {
        Change_Table data = CurSelectedData.changeData;
        return new S_PCRResourceData() { FileName = DBResource.GetResourceFileName(data.ResourceID), ViewScale = data.ViewScale, ViewPosY = data.ViewScaleLocY, Grade = data.Grade , ViewRotY = 0f};
    }

    public void OnClickAttribute()
    {
        Change_Table data = CurSelectedData.changeData;

        string attributename = "";
        switch (data.AttributeType)
        {
            case E_UnitAttributeType.Fire:
                attributename = DBLocale.GetText(LOCALE_ELEMENT_FIRE);
                break;
            case E_UnitAttributeType.Water:
                attributename = DBLocale.GetText(LOCALE_ELEMENT_WATER);
                break;
            case E_UnitAttributeType.Electric:
                attributename = DBLocale.GetText(LOCALE_ELEMENT_ELECTRIC);
                break;
            case E_UnitAttributeType.Light:
                attributename = DBLocale.GetText(LOCALE_ELEMENT_LIGHT);
                break;
            case E_UnitAttributeType.Dark:
                attributename = DBLocale.GetText(LOCALE_ELEMENT_DARK);
                break;
        }

        elementTooltip.ShowTooltip(
            data.AttributeType,
            attributename,
            string.Format(FORMAT_ATTRIBUTE_LEVEL, Me.CurCharData.GetAttributeLevelByType(data.AttributeType)),
            UICommon.GetAttributeSprite(data.AttributeType, UICommon.E_SIZE_OPTION.Midium));
    }
}
