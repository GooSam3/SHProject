using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class TempUISkillListItem : BaseItemViewsHolder
{
    public ScrollSkillData SkillData;
    public Transform GainImage;
    public Image SelectImage;
    private Image SkillIcon;
    private Text SkillName;
    private Text SkillState;
    private Button SkillSelect;
    private Button OrderUp;
    private Button OrderDown;
    
    private void OrderChange(bool _isUp)
    {
        SkillData.OrderChange(_isUp);
    }

    private void ClickSkillSelect()
    {
        UIManager.Instance.Find<UIFrameSkill>().SelectSkill(SkillData.SkillData.SkillID);

        SkillData.SkillSelect();
        
        //for(int i = 0; i < UIManager.Instance.Find<UIFrameSkill>().UISkillListItem.Count; i++)
        //{
        //    UIManager.Instance.Find<UIFrameSkill>().UISkillListItem[i].SelectImage.gameObject.SetActive(false);
        //}

        SelectImage.gameObject.SetActive(true);
    }

    public override void CollectViews()
    {
        base.CollectViews();

        root.GetComponentAtPath("SkillList_Slot/Txt_SkillName", out SkillName);
        root.GetComponentAtPath("SkillList_Slot/Txt_State", out SkillState);
        root.GetComponentAtPath("SkillList_Slot/Bt", out SkillSelect);
        root.GetComponentAtPath("SkillList_Slot/Button_Up", out OrderUp);
        root.GetComponentAtPath("SkillList_Slot/Button_Down", out OrderDown);
        root.GetComponentAtPath("SkillList_Slot/Skill_Slot/Icon_Skill", out SkillIcon);
        root.GetComponentAtPath("SkillList_Slot/Img_Select", out SelectImage);
        root.GetComponentAtPath("SkillList_Slot/Skill_Slot/Study_Alram", out GainImage);

        SkillSelect.onClick.AddListener(ClickSkillSelect);
        OrderUp.onClick.AddListener(() => { OrderChange(true); });
        OrderDown.onClick.AddListener(() => { OrderChange(false); });

        //UIManager.Instance.Find<UIFrameSkill>().UISkillListItem.Add(this);
    }

    public void UpdateTitleByItemIndex(ScrollSkillData model, bool _isOrderSettingMode)
    {
        SkillData = model;
        
        SkillName.text = DBLocale.GetSkillLocale(SkillData.SkillData);
        SkillIcon.sprite = ZManagerUIPreset.Instance.GetSprite(SkillData.SkillData.IconID);
        SkillState.text = SkillData.SkillData.SkillType.ToString();

        SkillIcon.color = ZNet.Data.Me.CurCharData.HasGainSkill(SkillData.SkillData.SkillID) ? Color.white : Color.gray;
        GainImage.gameObject.SetActive(!ZNet.Data.Me.CurCharData.HasGainSkill(SkillData.SkillData.SkillID));
        OrderUp.gameObject.SetActive(_isOrderSettingMode);
        OrderDown.gameObject.SetActive(_isOrderSettingMode);

        SelectImageCheck();
    }

    public void UpdateGainSkill()
	{
        GainImage.gameObject.SetActive(!ZNet.Data.Me.CurCharData.HasGainSkill(SkillData.SkillData.SkillID));
    }

    private void SelectImageCheck()
    {
        SelectImage.gameObject.SetActive(SkillData.IsSelected);
    }
}
