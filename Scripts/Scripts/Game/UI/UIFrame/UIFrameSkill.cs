using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameSkill : ZUIFrameBase
{
    public enum E_SkillPopupMode
    {
        None,
        SkillOrderSettingMode,
        SkillCoolTimeSettingMode,
        SkillInfoMode
    }

    public class SkillDescInfo
    {
        public string strTitle;
        public string strDesc;
        public string Value;
        public float cellSize;
    }

    #region UI Variable
    [SerializeField] private GameObject InfoPopup = null;
    [SerializeField] private Image SkillIcon = null;
    [SerializeField] private ZImage SkillNameHeaderBG = null;
    [SerializeField] private Text SkillName = null;
    [SerializeField] private Text SkillGrade = null;
    [SerializeField] private Text SkillDesc = null;
    [SerializeField] private Text SkillConsume = null;
    [SerializeField] private Text SkillEffect = null;
    [SerializeField] private Text SkillCoolTime = null;
    [SerializeField] private Text SkillState = null;
    [SerializeField] private GameObject GainButtonObj = null;
    [SerializeField] private Image[] SkillClassIcon = new Image[ZUIConstant.SKILL_CLASS_COUNT];
    [SerializeField] private List<UIInfoTextSlot> UISkillInfoTextSlotList = new List<UIInfoTextSlot>();
    [SerializeField] private UISkillListScrollAdapter ScrollAdapter;
    [SerializeField] private Transform DescContent;
    [SerializeField] private ScrollRect SkillListScroll = null;
    [SerializeField] private Button GainButton = null;
    [SerializeField] private ZToggle KnightButton = null;
    [SerializeField] private ZToggle AssassinButton = null;
    [SerializeField] private ZToggle ArcherButton = null;
    [SerializeField] private ZToggle WizardButton = null;
    [SerializeField] private ZToggle CommonButton = null;
    [SerializeField] private GameObject OrderSettingEffect = null;
    [SerializeField] private Animation GainSkillAnimation = null;
    [SerializeField] private Image AnimSkillIcon = null;
    [SerializeField] private Text AnimSkillName = null;
    [SerializeField] private GameObject MpUseObject = null;
    [SerializeField] private GameObject SkillListItem = null;
    #endregion

    #region System Variable
    private object SelectObject = null;
    public uint SelectSkillId = 0;
    [SerializeField] private UIPopupSkillCalculrator SkillCalculratorPopup = null;
    //public List<TempUISkillListItem> UISkillListItem = new List<TempUISkillListItem>();
    private List<SkillDescInfo> SkillDetailInfoList = new List<SkillDescInfo>();
    public E_SkillPopupMode PopupMode = E_SkillPopupMode.None;
    private E_CharacterType CharacterType = E_CharacterType.None;
    private E_WeaponType WeaponType = E_WeaponType.None;
    private Queue<uint> SkillAnimationQueue = new Queue<uint>();
    private Coroutine SkillAnimationCoroutine = null;
    public override bool IsBackable => true;
    #endregion

    public void Init(Action open = null)
    {
        
    }

    private void Initialize()
    {
        
    }
    private bool bInitSkillFrame;
    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (ScrollAdapter.Parameters.ItemPrefab == null)
        {
            ScrollAdapter.Parameters.ItemPrefab = SkillListItem.GetComponent<RectTransform>();
            ScrollAdapter.Parameters.ItemPrefab.SetParent(ScrollAdapter.GetComponent<Transform>());
            ScrollAdapter.Parameters.ItemPrefab.transform.localScale = Vector2.one;
            ScrollAdapter.Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
            ScrollAdapter.Parameters.ItemPrefab.gameObject.SetActive(false);
        }

        ScrollAdapter.Init();

        bInitSkillFrame = true;
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        StopAllCoroutines();
        StartCoroutine(Co_WaitStart());
    }

    private IEnumerator Co_WaitStart()
    {
        while(false == bInitSkillFrame)
        {
            yield return null;
        }

        UIManager.Instance.Find<UIFrameHUD>().RemoveAllInfoPopup();

        // 스킬 팝업을 열 때 자신의 캐릭터타입에 맞는 탭부터 보여준다.
        switch (ZPawnManager.Instance.MyEntity.CharacterType)
        {
            case E_CharacterType.Knight:
                KnightButton.SelectToggle();
                break;
            case E_CharacterType.Archer:
                ArcherButton.SelectToggle();
                break;
            case E_CharacterType.Assassin:
                AssassinButton.SelectToggle();
                break;
            case E_CharacterType.Wizard:
                WizardButton.SelectToggle();
                break;
            case E_CharacterType.All:
                CommonButton.SelectToggle();
                break;
        }

        ScrollAdapter.UpdateGainSkill();
        OrderSettingEffect.SetActive(PopupMode == E_SkillPopupMode.SkillOrderSettingMode);
    }

    protected override void OnHide()
    {
        base.OnHide();

        if (SkillAnimationCoroutine != null)
        {
            StopCoroutine(nameof(ShowGainSkillAnimation));
            SkillAnimationCoroutine = null;
        }

        if (GainSkillAnimation.gameObject.activeSelf)
            GainSkillAnimation.gameObject.SetActive(false);

        if (false == bInitSkillFrame)
            return;

        if(null != ScrollAdapter.Data)
            ScrollAdapter.RemoveEvent();

        if (PopupMode == E_SkillPopupMode.SkillCoolTimeSettingMode)
            CloseSkillCalculratorPopup();

        if (PopupMode == E_SkillPopupMode.SkillOrderSettingMode)
            CloseSkillOrderSetting();

        SelectSkillId = 0;

        ActivePopup(false);

        SetSelectObject(null);
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameSkill>();
    }

    public void SelectSkillTab(int _tabIdx)
    {
        if(PopupMode == E_SkillPopupMode.SkillOrderSettingMode && (E_CharacterType)_tabIdx == E_CharacterType.All)
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("All_Skil_Not_Order_Tip"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);

            switch (CharacterType)
            {
                case E_CharacterType.Knight:
                    KnightButton.SelectToggle();
                    break;
                case E_CharacterType.Archer:
                    ArcherButton.SelectToggle();
                    break;
                case E_CharacterType.Assassin:
                    AssassinButton.SelectToggle();
                    break;
                case E_CharacterType.Wizard:
                    WizardButton.SelectToggle();
                    break;
            }

            return;
        }

        CharacterType = (E_CharacterType)_tabIdx;

        switch (CharacterType)
        {
            case E_CharacterType.Knight:
                WeaponType = E_WeaponType.Sword;
                break;
            case E_CharacterType.Assassin:
                WeaponType = E_WeaponType.TwoSwords;
                break;
            case E_CharacterType.Archer:
                WeaponType = E_WeaponType.Bow;
                break;
            case E_CharacterType.Wizard:
                WeaponType = E_WeaponType.Wand;
                break;
            case E_CharacterType.All:
                WeaponType = E_WeaponType.None;
                break;
        }

        switch (PopupMode)
        {
            case E_SkillPopupMode.SkillCoolTimeSettingMode:
                CloseSkillCalculratorPopup();
                break;
            case E_SkillPopupMode.SkillInfoMode:
                if (InfoPopup.activeSelf)
                    InfoPopup.SetActive(false);
                break;
        }

        SelectSkillId = 0;

        ScrollAdapter.SetScrollData(CharacterType, WeaponType, PopupMode == E_SkillPopupMode.SkillOrderSettingMode);
    }

    public void SelectSkill(uint _skillId)
    {
        for (int i = 0; i < ScrollAdapter.GetItemsCount(); i++)
        {
            if (ScrollAdapter.GetItemViewsHolder(i) != null && ScrollAdapter.GetItemViewsHolder(i).SkillData.SkillData.SkillID != _skillId)
                ScrollAdapter.GetItemViewsHolder(i).SelectImage.gameObject.SetActive(false);
        }

        if (PopupMode == E_SkillPopupMode.SkillCoolTimeSettingMode)
        {
            if (!ZNet.Data.Me.CurCharData.HasGainSkill(_skillId))
            {
                CloseSkillCalculratorPopup();
                SelectSkillId = _skillId;
                UICommon.SetNoticeMessage(DBLocale.GetText("Do_Not_Have_Skill_Alert"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                return;
            }

            SaveSkillCycle();

            SelectSkillId = _skillId;

            SkillCalculratorPopup.Initialized(SelectSkillId);

            return;
        }

        if (PopupMode != E_SkillPopupMode.SkillOrderSettingMode)
            PopupMode = E_SkillPopupMode.SkillInfoMode;

        SelectSkillId = _skillId;

        ActivePopup(true);

        if ((Me.CurCharData.HasGainSkill(_skillId) && (DBSkill.Get(_skillId).SkillType == E_SkillType.ActiveSkill || DBSkill.Get(_skillId).SkillType == E_SkillType.BuffSkill)))
        {
            SetSelectObject(_skillId);
        }
        else
        {
            SetSelectObject(null);
        }
    }

    private void SetSelectObject(object _obj)
    {
        if (_obj == null)
            UIManager.Instance.Find<UISubHUDQuickSlot>().SelectQuickSlotEffect(false);
        else
            UIManager.Instance.Find<UISubHUDQuickSlot>().SelectQuickSlotEffect(true);

        SelectObject = _obj;
    }

    public object GetSelectObject()
    {
        return SelectObject;
    }

    public void UpdateGainSkill(uint _skillId)
    {
        if (this.gameObject.activeSelf)
        {
            for (int i = 0; i < ScrollAdapter.GetItemsCount(); i++)
            {
                if (ScrollAdapter.GetItemViewsHolder(i) != null && ScrollAdapter.GetItemViewsHolder(i).SkillData.SkillData.SkillID == _skillId)
                    ScrollAdapter.GetItemViewsHolder(i).GainImage.gameObject.SetActive(false);
            }

            SetPopup();
        }
    }

    public void ActivePopup(bool _active)
    {
        InfoPopup.SetActive(_active);

        if(_active)
            SetPopup();
    }

    public void SetPopup()
    {
        var data = DBSkill.Get(SelectSkillId);
        string grade = DBLocale.GetText("Tier" + data.Grade.ToString() + "_Text");

        SkillIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.IconID);
        SkillName.text = DBLocale.GetSkillLocale(data);
        SkillNameHeaderBG.sprite = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(E_UIType.Header, data.Grade));
        SkillGrade.text = DBUIResouce.GetSkillGradeFormat(grade, data.Grade);

        if(data.UseMPCount <= 0)
        {
            MpUseObject.SetActive(false);
        }
        else
        {
            MpUseObject.SetActive(true);
            SkillConsume.text = data.UseMPCount.ToString();
        }

        SkillDesc.text = DBLocale.GetText(data.ToolTipID);
        
        //
        //SkillEffect.text = ParseDescription(data.Skill.SkillID);
        SetSkillDesc(data.SkillID);   // 추가 함수.
        SetSkillText();
        //

        var item = Me.CurCharData.GetItem(DBSkill.Get(SelectSkillId).OpenItemID, NetItemType.TYPE_STACK);

        SkillCoolTime.text = data.CoolTime.ToString();
        SkillState.text = data.SkillType.ToString();
        GainButtonObj.SetActive(!Me.CurCharData.HasGainSkill(SelectSkillId));
        // 스킬북 미소지시 [습득]버튼 비활성화
        GainButton.interactable = item != null;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)DescContent);
    }

    public void GainSkill()
    {
        SkillAnimationQueue.Enqueue(SelectSkillId);
        var skillData = DBSkill.Get(SelectSkillId);
        var item = Me.CurCharData.GetItem(skillData.OpenItemID, NetItemType.TYPE_STACK);

        if (item == null)
        {
            ActivePopup(false);
            UICommon.SetNoticeMessage(DBLocale.GetText("Do_Not_Have_SkillBook_Alert"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        ZWebManager.Instance.WebGame.UseItemAction(item, false, delegate {
            // TODO : 습득성공시 알림띄우기
            if (SkillAnimationCoroutine == null)
                SkillAnimationCoroutine = StartCoroutine(nameof(ShowGainSkillAnimation));
        });
    }

    IEnumerator ShowGainSkillAnimation()
    {
        while(SkillAnimationQueue.Count > 0)
        {
            uint skillId = SkillAnimationQueue.Dequeue();
            Skill_Table skillTable = DBSkill.Get(skillId);

            AnimSkillIcon.sprite = ZManagerUIPreset.Instance.GetSprite(skillTable.IconID);
            AnimSkillName.text = DBLocale.GetText(skillTable.SkillTextID);

            GainSkillAnimation.gameObject.SetActive(true);
            yield return new WaitUntil(() => !GainSkillAnimation.isPlaying);
            
            GainSkillAnimation.gameObject.SetActive(false);
        }
        
        SkillAnimationCoroutine = null;
        yield break;
    }

    // 스킬 상세정보 부분..
    private void SetSkillText()
    {
        for (int i = 0; i < UISkillInfoTextSlotList.Count; i++)
        {
            UISkillInfoTextSlotList[i].gameObject.SetActive(false);
        }
        if (SkillDetailInfoList.Count > 0)
        {
            for (int i = 0; i < SkillDetailInfoList.Count; i++)
            {
                if (i == 0)
                {
                    UISkillInfoTextSlotList[i].Initialize(SkillDetailInfoList[i].strDesc, SkillDetailInfoList[i].Value, "효 과");
                }
                else
                {
                    UISkillInfoTextSlotList[i].Initialize(SkillDetailInfoList[i].strDesc, SkillDetailInfoList[i].Value);
                }
            }
        }
        for (int i = 0; i < UISkillInfoTextSlotList.Count; i++)
        {
            if (UISkillInfoTextSlotList[i].GetSlotActiveCheck())
            {
                UISkillInfoTextSlotList[i].gameObject.SetActive(true);
            }
            UISkillInfoTextSlotList[i].SetSlotActiveCheck();
        }
    }

    private void SetSkillDesc(uint skill_ID)
    {
        SkillDetailInfoList.Clear();
        var tableData = DBSkill.Get(skill_ID);

        List<uint> listAbilityActionIds = new List<uint>();
        Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

        if (tableData.AbilityActionID_01 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_01);
        if (tableData.AbilityActionID_02 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_02);
        if (tableData.AbilityActionID_03 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_03);

        foreach (var abilityActionId in listAbilityActionIds) 
        {
            var abilityActionData = DBAbility.GetAction(abilityActionId);
            switch (abilityActionData.AbilityViewType)
            {
                case GameDB.E_AbilityViewType.ToolTip:
                    var itemDescInfo = new SkillDescInfo();
                    itemDescInfo.strDesc = string.Format("{0}{1}", "", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
                    itemDescInfo.Value = null;

                    SkillDetailInfoList.Add(itemDescInfo);
                    break;
                case GameDB.E_AbilityViewType.Not:
                default:
                    var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        if (!abilitys.ContainsKey(enumer.Current.Key))
                        {
                            abilitys.Add(enumer.Current.Key, enumer.Current.Value);
                        }
                    }
                    break;
            }
        }

        foreach (var ability in abilitys) 
        {
            if (!DBAbility.IsParseAbility(ability.Key))
                continue;

            float abilityminValue = (int)abilitys[ability.Key].Item1;
            float abilitymaxValue = (int)abilitys[ability.Key].Item2;

            var itemDescInfo = new SkillDescInfo();

            itemDescInfo.strDesc = DBLocale.GetText(DBAbility.GetAbilityName(ability.Key));
            var newValue = DBAbility.ParseAbilityValue(ability.Key, abilityminValue, abilitymaxValue);
            itemDescInfo.Value = string.Format("{0}", newValue);

            SkillDetailInfoList.Add(itemDescInfo);
        }
    }

    public void ShowSkillCalculratorPopup()
    {
        if (InfoPopup.activeSelf)
        {
            InfoPopup.SetActive(false);
        }

        if (PopupMode == E_SkillPopupMode.SkillOrderSettingMode)
        {
            CloseSkillOrderSetting();
            SelectSkillId = 0;
            return;
        }

        if (SelectSkillId <= 0)
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("SkillCooltime_Setup_NotChoice"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        if (!ZNet.Data.Me.CurCharData.HasGainSkill(SelectSkillId))
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("Do_Not_Have_Skill_Alert"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        if (PopupMode == E_SkillPopupMode.SkillCoolTimeSettingMode)
        {
            CloseSkillCalculratorPopup();
            return;
        }

        PopupMode = E_SkillPopupMode.SkillCoolTimeSettingMode;

        SkillCalculratorPopup.gameObject.SetActive(true);
        SkillCalculratorPopup.Initialized(SelectSkillId);
    }

    public void CloseSkillCalculratorPopup()
    {
        SaveSkillCycle();

        PopupMode = E_SkillPopupMode.None;

        if (SkillCalculratorPopup.gameObject.activeSelf)
        {
            SkillCalculratorPopup.gameObject.SetActive(false);
        }
    }

    private void SaveSkillCycle()
    {
        SkillOrderData skill = ZNet.Data.Me.CurCharData.SkillUseOrder.Find(a => a.Tid == SelectSkillId);
        List<SkillOrderData> list = new List<SkillOrderData>();

        uint Order = ZPawnManager.Instance.MyEntity.SkillSystem.GetSkills().Find(a => a.SkillId == SelectSkillId).Order;
        
        if (skill == null)
            ZNet.Data.Me.CurCharData.SkillUseOrder.Add(skill = new SkillOrderData(SelectSkillId, Order, (uint)SkillCalculratorPopup.GetSkillCycle(), SkillCalculratorPopup.IsUseSkillCycle));

        else
        {
            skill.CoolTime = (uint)SkillCalculratorPopup.GetSkillCycle();
            skill.IsUseSkillCycle = SkillCalculratorPopup.IsUseSkillCycle;
        }

        list.Add(skill);

        ZWebManager.Instance.WebGame.REQ_SKillUseOrderSet(list, (uint)CharacterType, null);
    }

    public void ClickSkillOrderSetting()
    {
        if(CharacterType == E_CharacterType.All)
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("All_Skil_Not_Order_Tip"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);

            return;
        }

        switch(PopupMode)
        {
            case E_SkillPopupMode.None:
                break;
            case E_SkillPopupMode.SkillCoolTimeSettingMode:
                if (SkillCalculratorPopup.gameObject.activeSelf)
                    CloseSkillCalculratorPopup();
                break;
            case E_SkillPopupMode.SkillInfoMode:
                if (InfoPopup.activeSelf)
                    InfoPopup.SetActive(false);
                break;
            case E_SkillPopupMode.SkillOrderSettingMode:
                CloseSkillOrderSetting();
                return;
        }

        OrderSettingEffect.SetActive(true);
        PopupMode = E_SkillPopupMode.SkillOrderSettingMode;
        ScrollAdapter.SetScrollData(CharacterType, WeaponType, true);
    }

    private void CloseSkillOrderSetting()
    {
        SendSetSkillUseOrderCommand();

        OrderSettingEffect.SetActive(false);
        PopupMode = E_SkillPopupMode.None;

        if (this.gameObject.activeSelf)
            ScrollAdapter.SetScrollData(CharacterType, WeaponType, false);
    }

    private void SendSetSkillUseOrderCommand()
    {
        List<SkillOrderData> orderList = new List<SkillOrderData>();

        for (int i = 0; i < Me.CurCharData.SkillUseOrder.Count; i++)
        {
            if (Me.CurCharData.SkillUseOrder[i].IsChanged)
            {
                orderList.Add(Me.CurCharData.SkillUseOrder[i]);
                Me.CurCharData.SkillUseOrder[i].IsChanged = false;
            }
        }

        if (orderList.Count > 0)
        {
            ZWebManager.Instance.WebGame.REQ_SKillUseOrderSet(orderList, (uint)CharacterType, (a, b) => ZPawnManager.Instance.MyEntity.GetAutoSkillController().SortSkillByOrderNum());
        }
    }
}