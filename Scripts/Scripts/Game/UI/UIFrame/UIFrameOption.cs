using UnityEngine;
using UnityEngine.UI;

public class UIFrameOption : ZUIFrameBase
{
    //[Header("Artifact Slots"), Space(10)]
    //[SerializeField] private UIInvenArtifactEquipSlot ArtifactSlot_Pet;
    //[SerializeField] private UIInvenArtifactEquipSlot ArtifactSlot_Vehicle;
    enum OptionType
    {
        Battle,
        Control,
        Environment,
        Alram,
        Information
    }

    enum BattleTab
    {
        Use,
        Hunt,
        Gain,
        Scan
    }

    #region UI Locale Text
    [Header("Use Sub Menu"), Space(5)]
    [Header("Battle"), Space(2)]
    [SerializeField] private Text AutoUsePotionTitle;
    [SerializeField] private Text AutoUsePotionDesc;
    [SerializeField] private Text BigPotionPriorityText;
    [SerializeField] private Text RemainMpTitle;
    [SerializeField] private Text RemainMpDesc;
    [SerializeField] private Text GodTearItemTitle;
    [SerializeField] private Text GodTearItemDesc;
    [SerializeField] private Text AutoBuyPotionOpenDesc;

    [Header("Hunt Sub Menu"), Space(5)]
    [SerializeField] private Text AutoBattlePriorityTitle;
    [SerializeField] private Text AutoBattlePriorityDesc;
    [SerializeField] private Text AutoBattle1st;
    [SerializeField] private Text AutoBattle2nd;
    [SerializeField] private Text AutoBattle3rd;
    [SerializeField] private Text AutoBattleHostile;
    [SerializeField] private Text AutoBattleQuest;
    [SerializeField] private Text AutoBattleNear;
    [SerializeField] private Text MannerModeTitle;
    [SerializeField] private Text MannerModeDesc;
    [SerializeField] private Text ScreenSaverTitle;
    [SerializeField] private Text ScreenSaverOff;

    [Header("Gain Sub Menu"), Space(5)]
    [SerializeField] private Text AutoBreakPetEquipmentOpenDesc;
    [SerializeField] private Text PetEquipmentAutoDisassembleTitle;
    [SerializeField] private Text PetEquipmentAutoDisassembleDesc;
    [SerializeField] private Text PetEquipmentNormal;
    [SerializeField] private Text PetEquipmentHigh;
    [SerializeField] private Text PetEquipmentRare;
    [SerializeField] private Text PetEquipmentLegend;
    [SerializeField] private Text PetEquipmentMyth;
    [SerializeField] private Text AutoReturnTitle;
    [SerializeField] private Text AutoReturnDesc;

    [Header("Scan Sub Menu"), Space(5)]
    [SerializeField] private Text ScanTarget;
    [SerializeField] private Text ScanMonster;
    [SerializeField] private Text ScanPlayer;
    [SerializeField] private Text ScanMonsterPriority;
    [SerializeField] private Text ScanPlayerPriority;
    [SerializeField] private Text ScanMonsterQuest;
    [SerializeField] private Text ScanMonsterNormal;
    [SerializeField] private Text ScanMonsterHostle;
    [SerializeField] private Text ScanPlayerEnemyGuild;
    [SerializeField] private Text ScanPlayerAlert;
    [SerializeField] private Text ScanPlayerNormal;
    [SerializeField] private Text ScanPlayerMyGuild;
    [SerializeField] private Text ScanPlayerAllianceGuild;
    [SerializeField] private Text ScanPlayerParty;

    [Header("Control"), Space(10)]
    [SerializeField] private Text ControlTitle;
    [SerializeField] private Text VirtualPad;
    [SerializeField] private Text ScreenVibration;
    [SerializeField] private Text QuickSlotReset;
    [SerializeField] private Text QuickSlotAllReset;
    [SerializeField] private Text QuickSlotResetButtonText;
    [SerializeField] private Text QuickSlotAllResetButtonText;
    [SerializeField] private Text PartyInvite;
    [SerializeField] private Text PartyJoin;
    [SerializeField] private Text TargetTitle;
    [SerializeField] private Text TargetPriorityTitle;
    [SerializeField] private Text TargetTitleDesc;
    [SerializeField] private Text Target1st;
    [SerializeField] private Text Target2nd;
    [SerializeField] private Text Target3rd;
    [SerializeField] private Text TargetAttack;
    [SerializeField] private Text TargetEnemyGuild;
    [SerializeField] private Text TargetNear;
    [SerializeField] private Text TargetCancel;

    [Header("Environment"), Space(10)]
    [SerializeField] private Text EnvironmentTitle;
    [SerializeField] private Text ShowCharacterInfo;
    [SerializeField] private Text ShowCharacterName;
    [SerializeField] private Text ExpEffect;
    [SerializeField] private Text GoldEffect;
    [SerializeField] private Text DamageEffect;
    [SerializeField] private Text DodgeEffect;
    [SerializeField] private Text SoundTitle;
    [SerializeField] private Text Bgm;
    [SerializeField] private Text Sfx;
    [SerializeField] private Text FrameTitle;
    [SerializeField] private Text FrameVeryHigh;
    [SerializeField] private Text FrameHigh;
    [SerializeField] private Text FrameNormal;
    [SerializeField] private Text FrameLow;

    [Header("Alarm"), Space(10)]
    [SerializeField] private Text PushTitle;
    [SerializeField] private Text PushReceive;
    [SerializeField] private Text NightPushReceive;
    [SerializeField] private Text ConnectTitle;
    [SerializeField] private Text FriendConnect;
    [SerializeField] private Text AlertConnect;
    [SerializeField] private Text GuildConnect;
    [SerializeField] private Text EtcTitle;
    [SerializeField] private Text EtcDesc;
    [SerializeField] private Text BeAttackedTitle;
    [SerializeField] private Text BeAttackedDesc;
    [SerializeField] private Text BeAttackedSound;
    [SerializeField] private Text HpAlarmTitle;
    [SerializeField] private Text HpPotionAlarm;
    [SerializeField] private Text HpPotionCount;
    [SerializeField] private Text HpPerAlarm;
    [SerializeField] private Text HpPer;
    [SerializeField] private Text WeightAlarmTitle;
    [SerializeField] private Text WeightAlarmDesc;
    [SerializeField] private Text WeightPer;

    [Header("Information"), Space(10)]
    [SerializeField] private Text AccountName;
    [SerializeField] private Text CharacterName;
    [SerializeField] private Text Version;
    [SerializeField] private Text CouponNumber;
    [SerializeField] private Text Ask;
    [SerializeField] private Text Caffe;
    [SerializeField] private Text Logout;
    [SerializeField] private Text CharacterSelect;
    #endregion

    [SerializeField] private GameObject BattleGroup;
    [SerializeField] private GameObject ControlGroup;
    [SerializeField] private GameObject EnvironmentGroup;
    [SerializeField] private GameObject AlramGroup;
    [SerializeField] private GameObject InformationGroup;
    [SerializeField] private GameObject UseTab;
    [SerializeField] private GameObject HuntTab;
    [SerializeField] private GameObject GainTab;
    [SerializeField] private GameObject ScanTab;
    [SerializeField] private Button HidePanel;

    [SerializeField] private ZToggle BattleTabToggle;
    [SerializeField] private ZToggle UseTabToggle;
    public override bool IsBackable => true;
    public void Init()
    {
        
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame(E_UIStyle.FullScreen);

        HidePanel.gameObject.SetActive(false);

        //[박윤성] 모든 Locale들을 ZText의 Localizing TID로 뺌.
        //SetLocaleText();

        BattleTabToggle.SelectToggle();
        UseTabToggle.SelectToggle();
        SetOptionGroupActive(OptionType.Battle);
        SetBattleTabActive(BattleTab.Use);
    }

    protected override void OnHide()
    {
        base.OnHide();

        UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
    }

    public void Close()
    {
        UIManager.Instance.Close<UIFrameOption>();
    }

    private void SetLocaleText()
    {
        #region Battle
        AutoUsePotionTitle.text = DBLocale.GetText("Option_HpPotion_Setting");
        AutoUsePotionDesc.text = DBLocale.GetText("Option_HpPotion_Setting_Des");

        RemainMpTitle.text = DBLocale.GetText("Option_MP_Setting");
        RemainMpDesc.text = DBLocale.GetText("Option_MP_Setting_Des");
        GodTearItemTitle.text = DBLocale.GetText("God_Tear_AutoUse");
        GodTearItemDesc.text = DBLocale.GetText("God_Tear_AutoUseDesc");
        AutoBattlePriorityTitle.text = DBLocale.GetText("Option_AutoPlay_AttackOrder");
        AutoBattlePriorityDesc.text = DBLocale.GetText("Option_AutoPlay_AttackOrder_Desc");
        AutoBuyPotionOpenDesc.text = string.Format(DBLocale.GetText("Auto_Buy_Potion_Available"), DBLocale.GetText(DBPetCollect.GetCollectionName(DBConfig.PetCollection_Auto_HpPotion)));

        MannerModeTitle.text = DBLocale.GetText("Option_AutoPlay_Manner");
        MannerModeDesc.text = DBLocale.GetText("Option_AutoPlay_Manner_Desc");
        ScreenSaverTitle.text = DBLocale.GetText("Option_AutoPlay_PowerSavingMode");
        ScreenSaverOff.text = DBLocale.GetText("Option_AutoPlay_Manner_Cancel");
        PetEquipmentAutoDisassembleTitle.text = DBLocale.GetText("RUNE_AUTO_BREAK");
        PetEquipmentAutoDisassembleDesc.text = DBLocale.GetText("RUNE_AUTO_BREAK_Des");
        AutoBreakPetEquipmentOpenDesc.text = string.Format(DBLocale.GetText("Auto_Break_Available"), DBLocale.GetText(DBPetCollect.GetCollectionName(DBConfig.PetCollection_Auto_Break)));

        AutoReturnTitle.text = DBLocale.GetText("Option_AutoPlay_ReCall");
        AutoReturnDesc.text = DBLocale.GetText("Option_AutoPlay_ReCall_Desc");

        #endregion

        #region Control
        ControlTitle.text = DBLocale.GetText("Option_Control_Convenience");
        VirtualPad.text = DBLocale.GetText("Option_Control_VirtualPad");
        ScreenVibration.text = DBLocale.GetText("Option_Control_ScreenShake");
        QuickSlotAllReset.text = DBLocale.GetText("Option_Control_QuickSlotReset_All");
        QuickSlotReset.text = DBLocale.GetText("Option_Control_QuickSlotReset_Select");
        QuickSlotAllResetButtonText.text = DBLocale.GetText("Option_Control_QuickSlotReset_AllBtn");
        QuickSlotResetButtonText.text = DBLocale.GetText("Option_Control_QuickSlotReset_SelectBtn");
        PartyInvite.text = DBLocale.GetText("Option_Control_PartyAccept");
        PartyJoin.text = DBLocale.GetText("Option_Control_PartyApply");
        TargetTitle.text = DBLocale.GetText("Option_Control_Target");
        TargetPriorityTitle.text = DBLocale.GetText("Option_Control_Target_Desc1");
        TargetTitleDesc.text = DBLocale.GetText("Option_Control_Target_Desc2");
        //Target1st.text = DBLocale.GetText("Option_Order_First");
        //Target2nd.text = DBLocale.GetText("Option_Order_Second");
        //Target3rd.text = DBLocale.GetText("Option_Order_Third");
        //TargetAttack.text = DBLocale.GetText("Option_Target_AttackOrder_First_Text");
        //TargetEnemyGuild.text = DBLocale.GetText("Option_Target_AttackOrder_Second_Text");
        //TargetNear.text = DBLocale.GetText("Option_Target_AttackOrder_Third_Text");
        TargetCancel.text = DBLocale.GetText("Option_Target_Cancel");
        #endregion

        #region Alarm
        EnvironmentTitle.text = DBLocale.GetText("Option_Environment_Display");
        ShowCharacterInfo.text = DBLocale.GetText("WGameSetting_Environment_CharacterInfo");
        ShowCharacterName.text = DBLocale.GetText("WGameSetting_Environment_CharacterName");
        ExpEffect.text = DBLocale.GetText("WGameSetting_Environment_ExpEffect");
        GoldEffect.text = DBLocale.GetText("WGameSetting_Environment_GoldEffect");
        DamageEffect.text = DBLocale.GetText("WGameSetting_Environment_DamageEffect");
        DodgeEffect.text = DBLocale.GetText("WGameSetting_Environment_EvasionEffect");
        SoundTitle.text = DBLocale.GetText("Option_Environment_Sound");
        Bgm.text = DBLocale.GetText("WGameSetting_Environment_BGM");
        Sfx.text = DBLocale.GetText("WGameSetting_Environment_EffectSound");
        //FrameTitle.text = DBLocale.GetText("WGameSetting_Environment_GameFrame");
        //FrameVeryHigh.text = DBLocale.GetText("Option_GameFrame_VeryHigh");
        //FrameHigh.text = DBLocale.GetText("Option_GameFrame_High");
        //FrameNormal.text = DBLocale.GetText("Option_GameFrame_Normal");
        //FrameLow.text = DBLocale.GetText("Option_GameFrame_Low");
        PushTitle.text = DBLocale.GetText("Option_Alert_Push");
        PushReceive.text = DBLocale.GetText("Option_Alert_Push_Agreement");
        NightPushReceive.text = DBLocale.GetText("Option_Alert_Push_Agreement_Night");
        ConnectTitle.text = DBLocale.GetText("Option_Alert_Connect");
        FriendConnect.text = DBLocale.GetText("Option_Alert_Connect_Friend");
        AlertConnect.text = DBLocale.GetText("Option_Alert_Connect_Enemy");
        GuildConnect.text = DBLocale.GetText("Option_Alert_Connect_GuildCrew");
        EtcTitle.text = DBLocale.GetText("Option_Alert_ETC");
        EtcDesc.text = DBLocale.GetText("Option_Alert_GetValuable_HideName");
        BeAttackedTitle.text = DBLocale.GetText("Option_Alert_PK");
        BeAttackedDesc.text = DBLocale.GetText("Option_Alert_PK_Desc");
        BeAttackedSound.text = DBLocale.GetText("Option_Alert_PK_Sound");
        HpAlarmTitle.text = DBLocale.GetText("Option_Alert_HP");
        HpPotionAlarm.text = DBLocale.GetText("Option_Alert_HP_Desc1");
        HpPerAlarm.text = DBLocale.GetText("Option_Alert_HP_Desc2");
        WeightAlarmTitle.text = DBLocale.GetText("Option_Alert_Weight");
        WeightAlarmDesc.text = DBLocale.GetText("Option_Alert_Weight_Desc");
        #endregion

        #region Information
        AccountName.text = DBLocale.GetText("Option_Information_Account");
        CharacterName.text = DBLocale.GetText("Option_Information_CharName");
        Version.text = DBLocale.GetText("Option_Information_Version");
        CouponNumber.text = DBLocale.GetText("Option_Information_Coupon");
        Ask.text = DBLocale.GetText("WGameSetting_Notice_Button");
        Caffe.text = DBLocale.GetText("WGameSetting_Caffe_Button");
        Logout.text = DBLocale.GetText("WGameSetting_Logout_Button");
        CharacterSelect.text = DBLocale.GetText("WGameSetting_CharacterChange_Button");
        #endregion

        //RemainMpTitle.text= DBLocale.GetText("Option_")
    }

    public void SelectOptionGroup(int _optionIndex)
    {
        
        OptionType type = (OptionType)_optionIndex;

        SetOptionGroupActive(type);

        switch (type)
        {
            case OptionType.Battle:
                break;
            case OptionType.Control:
                break;
            case OptionType.Environment:
                break;
            case OptionType.Alram:
                break;
            case OptionType.Information:
                break;
        }
    }

    public void SelectBattleGroupTab(int _tabIndex)
    {
        BattleTab type = (BattleTab)_tabIndex;

        SetBattleTabActive(type);

        switch (type)
        {
            case BattleTab.Use:
                break;
            case BattleTab.Hunt:
                break;
            case BattleTab.Gain:
                break;
            case BattleTab.Scan:
                break;
        }
    }

    private void SetOptionGroupActive(OptionType _type)
    {
        BattleGroup.SetActive(_type == OptionType.Battle);
        ControlGroup.SetActive(_type == OptionType.Control);
        EnvironmentGroup.SetActive(_type == OptionType.Environment);
        AlramGroup.SetActive(_type == OptionType.Alram);
        InformationGroup.SetActive(_type == OptionType.Information);
    }

    private void SetBattleTabActive(BattleTab _type)
    {
        UseTab.SetActive(_type == BattleTab.Use);
        HuntTab.SetActive(_type == BattleTab.Hunt);
        GainTab.SetActive(_type == BattleTab.Gain);
        ScanTab.SetActive(_type == BattleTab.Scan);
    }

    public void OpenQuickSlotResetMode()
    {
        HidePanel.gameObject.SetActive(true);

        if(UIManager.Instance.Find(out UISubHUDQuickSlot quick))
        {
            quick.IsQuickSlotResetMode = true;
            quick.UIRide.gameObject.SetActive(false);
            UIManager.Instance.Open<UISubHUDQuickSlot>();
        }
    }

    public void CloseQuickSlotResetMode()
    {
        HidePanel.gameObject.SetActive(false);

        if (UIManager.Instance.Find(out UISubHUDQuickSlot quick))
        {
            quick.IsQuickSlotResetMode = false;
            quick.UIRide.gameObject.SetActive(true);
            UIManager.Instance.Close<UISubHUDQuickSlot>();
        }
    }
}
