using System.Collections.Generic;
using TinyJSON;
using UnityEngine;

public class ZGameOption
{
    static ZGameOption _Instance;
    public static ZGameOption Instance
    {
        get {
            if (_Instance == null)
            {
                /*string loadData = DeviceSaveDatas.LoadData("GameOption", "");
                //ZLog.Log("load data : " + loadData);

                if (!string.IsNullOrEmpty(loadData))
                {
                    try
                    {
                        _Instance = JSON.Load(loadData).Make<ZGameOption>();

                        Application.targetFrameRate = (int)_Instance.FrameRate;
                    }
                    catch
                    {
                        _Instance = null;
                    }
                }*/

                if (_Instance == null)
                {
                    _Instance = new ZGameOption();
                    _Instance.DefaultSet();
                }
            }

            return _Instance;
        }
    }

	/// <summary>
	/// KPRO : Reload Domain 이슈
	/// </summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void ClearStatic()
	{
		_Instance = null;
	}

	[TinyJSON.Exclude]
    public System.Action<OptionKey> OnOptionChanged;

    public void LoadCharacterOption()
    {
        //auto
        NormalHPPotionPer = DeviceSaveDatas.LoadCurCharData(nameof(NormalHPPotionPer), 0.8f);
        BigHPPotionPer = DeviceSaveDatas.LoadCurCharData(nameof(BigHPPotionPer), 0.8f);

        NormalPotionAutoBuy = DeviceSaveDatas.LoadCurCharData(nameof(NormalPotionAutoBuy), false);
        BigPotionAutoBuy = DeviceSaveDatas.LoadCurCharData(nameof(BigPotionAutoBuy), false);

        HP_PotionUsePriority = (HPPotionUsePriority)DeviceSaveDatas.LoadCurCharData(nameof(HP_PotionUsePriority), (int)HPPotionUsePriority.NORMAL);

        RemainMPPer = DeviceSaveDatas.LoadCurCharData(nameof(RemainMPPer), 0f);

        GodTearStackCnt = (uint)DeviceSaveDatas.LoadCurCharData(nameof(GodTearStackCnt), 0);

        Buff_UseType = (BuffUseType)DeviceSaveDatas.LoadCurCharData(nameof(Buff_UseType), (int)BuffUseType.CheckMove);

        ScanSearchTarget_Priority = (ScanSearchTargetPriority)DeviceSaveDatas.LoadCurCharData(nameof(ScanSearchTarget_Priority), (int)(ScanSearchTargetPriority.TARGET_MONSTER | ScanSearchTargetPriority.TARGET_PLAYER));

        ScanSearchTarget_Type = (ScanSearchTargetType)DeviceSaveDatas.LoadCurCharData(nameof(ScanSearchTarget_Type), (int)(ScanSearchTargetType.TARGET_ALERT_PLAYER | ScanSearchTargetType.TARGET_ALLIANCEGUILD_PLAYER | ScanSearchTargetType.TARGET_ENEMYGUILD_PLAYER | ScanSearchTargetType.TARGET_HOSTILE_MONSTER | ScanSearchTargetType.TARGET_MYGUILD_PLAYER | ScanSearchTargetType.TARGET_NORMAL_MONSTER | ScanSearchTargetType.TARGET_NORMAL_PLAYER | ScanSearchTargetType.TARGET_PARTY_PLAYER | ScanSearchTargetType.TARGET_QUEST_MONSTER));

        AutoBattle_Target = (AutoBattleTargetPriority)DeviceSaveDatas.LoadCurCharData(nameof(AutoBattle_Target), (int)(AutoBattleTargetPriority.TARGET_HOSTILE_MONSTER | AutoBattleTargetPriority.TARGET_QUEST_MONSTER));

        bMonsterPriority = DeviceSaveDatas.LoadCurCharData(nameof(bMonsterPriority), true);

        bAutoSummon_Boss = DeviceSaveDatas.LoadCurCharData(nameof(bAutoSummon_Boss), false);
        bManner_Search = DeviceSaveDatas.LoadCurCharData(nameof(bManner_Search), false);
        Auto_Screen_Saver_Time = DeviceSaveDatas.LoadCurCharData(nameof(Auto_Screen_Saver_Time), (float)(5 * TimeHelper.MinuteSecond));
        bAuto_Wakeup_ScreenSaver = DeviceSaveDatas.LoadCurCharData(nameof(bAuto_Wakeup_ScreenSaver), false);

        Auto_Break_Belong_Item = (AutoBreakType)DeviceSaveDatas.LoadCurCharData(nameof(Auto_Break_Belong_Item), (int)AutoBreakType.OFF);

        Auto_Sell_PetEquipmentGrade = (AutoSellPetEquipmentType)DeviceSaveDatas.LoadCurCharData(nameof(Auto_Sell_PetEquipmentGrade), (int)AutoSellPetEquipmentType.OFF);
        Auto_Sell_PetEquipmentGradeType = (GameDB.E_RuneGradeType)DeviceSaveDatas.LoadCurCharData(nameof(Auto_Sell_PetEquipmentGradeType), (int)GameDB.E_RuneGradeType.Normal);
        Auto_Return_Town_Weight_Per = DeviceSaveDatas.LoadCurCharData(nameof(Auto_Return_Town_Weight_Per), 0f);

        //control
        Search_Target_Priority = (SearchTargetPriority)DeviceSaveDatas.LoadCurCharData(nameof(Search_Target_Priority), (int)(SearchTargetPriority.TARGET_BE_ATTACK_PLAYER | SearchTargetPriority.TARGET_ENEMY_GUILD | SearchTargetPriority.TARGET_NEAR_CHARACTER));

        bUseVirtualPad = DeviceSaveDatas.LoadCurCharData(nameof(bUseVirtualPad), true);
        bUseTargetCancel = DeviceSaveDatas.LoadCurCharData(nameof(bUseTargetCancel), true);

        bAllowPartyInvite = DeviceSaveDatas.LoadCurCharData(nameof(bAllowPartyInvite), true);
        bAllowPartyJoin = DeviceSaveDatas.LoadCurCharData(nameof(bAllowPartyJoin), true);

        bVibration = DeviceSaveDatas.LoadCurCharData(nameof(bVibration), true);

    }

    public void EmptyFunction() { }

    public void DefaultSet()
    {
        BGMSound = DeviceSaveDatas.LoadData(nameof(BGMSound), 0.7f);
        SFXSound = DeviceSaveDatas.LoadData(nameof(SFXSound), 0.7f);
		Quality = (E_Quality)DeviceSaveDatas.LoadData(nameof(Quality), (int)E_Quality.High);
		if (QualitySettings.GetQualityLevel() != (int)Quality)
			ChangeQuality(Quality);


        bShowDefaultCharacterText = DeviceSaveDatas.LoadData(nameof(bShowDefaultCharacterText), true);
        bShowCharacterName = DeviceSaveDatas.LoadData(nameof(bShowCharacterName), true);
        bShowTargetUIText = DeviceSaveDatas.LoadData(nameof(bShowTargetUIText), true);
        bShowExpGainEffect = DeviceSaveDatas.LoadData(nameof(bShowExpGainEffect), true);
        bShowGoldGainEffect = DeviceSaveDatas.LoadData(nameof(bShowGoldGainEffect), true);
        bShowDamageEffect = DeviceSaveDatas.LoadData(nameof(bShowDamageEffect), true);
        bShowDodgeEffect = DeviceSaveDatas.LoadData(nameof(bShowDodgeEffect), true);

        //alram
        //bPushEnable = NetData.pushType == WebNet.E_PushType.AllDay || NetData.pushType == WebNet.E_PushType.NotNight;
        //bNightPushEnable = NetData.pushType == WebNet.E_PushType.AllDay;
        bPushEnable = DeviceSaveDatas.LoadData(nameof(bPushEnable), false);
        bNightPushEnable = DeviceSaveDatas.LoadData(nameof(bNightPushEnable), false);

        bAlram_Friend_Connect = DeviceSaveDatas.LoadData(nameof(bAlram_Friend_Connect), true);
        bAlram_Enemy_Connect = DeviceSaveDatas.LoadData(nameof(bAlram_Enemy_Connect), true);
        bAlram_Guild_Member_Connect = DeviceSaveDatas.LoadData(nameof(bAlram_Guild_Member_Connect), true);

        Alram_Party_GetItem = (GetItemAlram)DeviceSaveDatas.LoadData(nameof(Alram_Party_GetItem), (int)GetItemAlram.ALL);

        AlramHPPotion = (uint)DeviceSaveDatas.LoadData(nameof(AlramHPPotion), 0);
        AlramHPPer = DeviceSaveDatas.LoadData(nameof(AlramHPPer), 0f);
        AlramWeight = DeviceSaveDatas.LoadData(nameof(AlramWeight), .5f);
        bAlramBeAttacked_PC = DeviceSaveDatas.LoadData(nameof(bAlramBeAttacked_PC), true);
        AlramSound = DeviceSaveDatas.LoadData(nameof(AlramSound), 70f);
    }

	/// <summary>
	/// 게임 성능&그래픽 관련 설정 변경
	/// </summary>
	private void ChangeQuality(E_Quality newQuality)
	{
		switch (Quality)
		{
			case E_Quality.Low:
				Application.targetFrameRate = 26;
				break;
			case E_Quality.Midium:
				Application.targetFrameRate = 28;
				break;
			case E_Quality.High:
				Application.targetFrameRate = 30;
				break;
			case E_Quality.VeryHigh:
				Application.targetFrameRate = 30;
				break;
		}

        QualitySettings.SetQualityLevel((int)Quality);        


        ZLog.Log(ZLogChannel.System, $"ZGameOption.ChangeQuality | Quality: {Quality}, FrameRate: {Application.targetFrameRate}");
	}

    public enum OptionKey
    {
        Option_None,

        Option_HP_Potion_Priority,
        Option_Normal_PotionPer,
        Option_Big_PotionPer,

        Option_Normal_PotionAutoBuy,
        Option_Big_PotionAutoBuy,

        Option_RemainMpPer,
        Option_GodTearStackCnt,

        Option_BuffUseType,

        Option_AutoBattle_Target,
        Option_BossSummon,
        Option_MannerSearch,
        Option_AutoScreenSaverTime,
        Option_WakeUp_ScreenSaver,

        Option_AutoBreakBelongItem,
        Option_AutoReturnTown_Weight,

        Option_SearchTarget_Priority,
        Option_Use_VirtualPad,
        Option_TargetCancel,
        Option_AllowPartyInvite,
        Option_AllowPartyJoin,

        Option_Bgm,
        Option_SfxSound,
		Option_Quality,

        Option_ShowCharacterName,
        Option_ShowDefaultCharacterText,
        Option_TargetUiText,
        Option_ExpGainEffect,
        Option_GoldGainEffect,
        Option_DamageEffect,
        Option_DodgeEffect,

        Option_Alram_HpPotion,
        Option_Alram_HpPer,
        Option_Alram_Weight,
        Option_Alram_BeAttacked,

        Option_AlramSound,

        Option_Push,
        Option_NightPush,
        Option_AlramFriendConnect,
        Option_AlramEnemyConnect,
        Option_AlramGuildMemeberConnect,

        Option_AlramPartyGetItem,
        Option_HideGetitemNickName,

        Option_Vibration,

        Option_AutoSellRuneGrade,
        Option_AutoSellRuneGradeType,

        Option_Max,

        Option_AutoHpPotionUse,

        Option_ScanSearchTargetType,
        Option_ScanSearchTargetPriority,
        Option_Monster_Priority
    }

    public class OptionItem
    {
        public OptionKey key;
        public object Value;
    }

    public void SetOptionList(List<OptionItem> optionList)
    {
        foreach (var optionitem in optionList)
        {
            SetOption(optionitem.key, optionitem.Value, false);
        }

        OnOptionChanged?.Invoke(OptionKey.Option_Max);
        DeviceSaveDatas.SaveData("GameOption", JSON.Dump(_Instance, EncodeOptions.IncludePublicProperties));
    }

    public void SetOption(OptionKey key, object Value, bool bCallDelegate = true)
    {
        switch (key)
        {
            //auto
            case OptionKey.Option_HP_Potion_Priority:
                HP_PotionUsePriority = (HPPotionUsePriority)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(HP_PotionUsePriority), (int)HP_PotionUsePriority);
                break;

            case OptionKey.Option_Normal_PotionPer:
                NormalHPPotionPer = (float)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(NormalHPPotionPer), NormalHPPotionPer);
                break;
            case OptionKey.Option_Big_PotionPer:
                BigHPPotionPer = (float)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(BigHPPotionPer), BigHPPotionPer);
                break;

            case OptionKey.Option_Normal_PotionAutoBuy:
                NormalPotionAutoBuy = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(NormalPotionAutoBuy), NormalPotionAutoBuy);
                break;
            case OptionKey.Option_Big_PotionAutoBuy:
                BigPotionAutoBuy = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(BigPotionAutoBuy), BigPotionAutoBuy);
                break;

            case OptionKey.Option_RemainMpPer:
                RemainMPPer = (float)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(RemainMPPer), RemainMPPer);
                break;

            case OptionKey.Option_GodTearStackCnt:
                GodTearStackCnt = (uint)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(GodTearStackCnt), (int)GodTearStackCnt);
                break;

            case OptionKey.Option_BuffUseType:
                Buff_UseType = (BuffUseType)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Buff_UseType), (int)Buff_UseType);
                break;

            case OptionKey.Option_Monster_Priority:
                bMonsterPriority = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bMonsterPriority), bMonsterPriority);
                break;

            case OptionKey.Option_ScanSearchTargetPriority:
                ScanSearchTarget_Priority = (ScanSearchTargetPriority)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(ScanSearchTarget_Priority), (int)ScanSearchTarget_Priority);
                break;

            case OptionKey.Option_ScanSearchTargetType:
                ScanSearchTarget_Type = (ScanSearchTargetType)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(ScanSearchTarget_Type), (int)ScanSearchTarget_Type);
                break;

            case OptionKey.Option_AutoBattle_Target:
                AutoBattle_Target = (AutoBattleTargetPriority)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(AutoBattle_Target), (int)AutoBattle_Target);
                break;
            case OptionKey.Option_BossSummon:
                bAutoSummon_Boss = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bAutoSummon_Boss), bAutoSummon_Boss);
                break;
            case OptionKey.Option_MannerSearch:
                bManner_Search = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bManner_Search), bManner_Search);
                break;
            case OptionKey.Option_AutoScreenSaverTime:
                Auto_Screen_Saver_Time = (float)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Auto_Screen_Saver_Time), Auto_Screen_Saver_Time);
                break;
            case OptionKey.Option_WakeUp_ScreenSaver:
                bAuto_Wakeup_ScreenSaver = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bAuto_Wakeup_ScreenSaver), bAuto_Wakeup_ScreenSaver);
                break;

            case OptionKey.Option_AutoBreakBelongItem:
                Auto_Break_Belong_Item = (AutoBreakType)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Auto_Break_Belong_Item), (int)Auto_Break_Belong_Item);
                break;
            case OptionKey.Option_AutoReturnTown_Weight:
                Auto_Return_Town_Weight_Per = (float)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Auto_Return_Town_Weight_Per), Auto_Return_Town_Weight_Per);
                break;

            case OptionKey.Option_SearchTarget_Priority:
                Search_Target_Priority = (SearchTargetPriority)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Search_Target_Priority), (int)Search_Target_Priority);
                break;
            case OptionKey.Option_Use_VirtualPad:
                bUseVirtualPad = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bUseVirtualPad), bUseVirtualPad);
                break;
            case OptionKey.Option_TargetCancel:
                bUseTargetCancel = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bUseTargetCancel), bUseTargetCancel);
                break;
            case OptionKey.Option_AllowPartyInvite:
                bAllowPartyInvite = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bAllowPartyInvite), bAllowPartyInvite);
                break;
            case OptionKey.Option_AllowPartyJoin:
                bAllowPartyJoin = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bAllowPartyJoin), bAllowPartyJoin);
                break;

            //
            case OptionKey.Option_Bgm:
                BGMSound = (float)Value;
                DeviceSaveDatas.SaveData(nameof(BGMSound), BGMSound);
                break;
            case OptionKey.Option_SfxSound:
                SFXSound = (float)Value;
                DeviceSaveDatas.SaveData(nameof(SFXSound), SFXSound);
                break;
			case OptionKey.Option_Quality:
				{
					var prevQuality = Quality;
					Quality = (E_Quality)Value;
					DeviceSaveDatas.SaveData(nameof(Quality), (int)Quality);

					if (prevQuality != Quality)
						ChangeQuality(Quality);
				}
				break;          
            case OptionKey.Option_ShowCharacterName:
                bShowCharacterName = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowCharacterName), bShowCharacterName);
                break;
            case OptionKey.Option_ShowDefaultCharacterText:
                bShowDefaultCharacterText = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowDefaultCharacterText), bShowDefaultCharacterText);
                break;
            case OptionKey.Option_TargetUiText:
                bShowTargetUIText = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowTargetUIText), bShowTargetUIText);
                break;
            case OptionKey.Option_ExpGainEffect:
                bShowExpGainEffect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowExpGainEffect), bShowExpGainEffect);
                break;
            case OptionKey.Option_GoldGainEffect:
                bShowGoldGainEffect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowGoldGainEffect), bShowGoldGainEffect);
                break;
            case OptionKey.Option_DamageEffect:
                bShowDamageEffect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowDamageEffect), bShowDamageEffect);
                break;
            case OptionKey.Option_DodgeEffect:
                bShowDodgeEffect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bShowDodgeEffect), bShowDodgeEffect);
                break;

            case OptionKey.Option_Alram_HpPotion:
                AlramHPPotion = (uint)Value;
                DeviceSaveDatas.SaveData(nameof(AlramHPPotion), (int)AlramHPPotion);
                break;
            case OptionKey.Option_Alram_HpPer:
                AlramHPPer = (float)Value;
                DeviceSaveDatas.SaveData(nameof(AlramHPPer), AlramHPPer);
                break;
            case OptionKey.Option_Alram_Weight:
                AlramWeight = (float)Value;
                DeviceSaveDatas.SaveData(nameof(AlramWeight), AlramWeight);
                break;
            case OptionKey.Option_Alram_BeAttacked:
                bAlramBeAttacked_PC = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bAlramBeAttacked_PC), bAlramBeAttacked_PC);
                break;

            case OptionKey.Option_AlramSound:
                AlramSound = (float)Value;
                DeviceSaveDatas.SaveData(nameof(AlramSound), AlramSound);
                break;

            case OptionKey.Option_Push:
                bPushEnable = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bPushEnable), bPushEnable);
                break;
            case OptionKey.Option_NightPush:
                bNightPushEnable = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bNightPushEnable), bNightPushEnable);
                break;
            case OptionKey.Option_AlramFriendConnect:
                bAlram_Friend_Connect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bAlram_Friend_Connect), bAlram_Friend_Connect);
                break;
            case OptionKey.Option_AlramEnemyConnect:
                bAlram_Enemy_Connect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bAlram_Enemy_Connect), bAlram_Enemy_Connect);
                break;
            case OptionKey.Option_AlramGuildMemeberConnect:
                bAlram_Guild_Member_Connect = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bAlram_Guild_Member_Connect), bAlram_Guild_Member_Connect);
                break;

            case OptionKey.Option_AlramPartyGetItem:
                Alram_Party_GetItem = (GetItemAlram)Value;
                DeviceSaveDatas.SaveData(nameof(Alram_Party_GetItem), (int)Alram_Party_GetItem);
                break;

            case OptionKey.Option_HideGetitemNickName:
                bHideGetitemNickName = (bool)Value;
                DeviceSaveDatas.SaveData(nameof(bHideGetitemNickName), bHideGetitemNickName);
                break;
            case OptionKey.Option_Vibration:
                bVibration = (bool)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(bVibration), bVibration);
                break;
            case OptionKey.Option_AutoSellRuneGrade:
                Auto_Sell_PetEquipmentGrade = (AutoSellPetEquipmentType)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Auto_Sell_PetEquipmentGrade), (int)Auto_Sell_PetEquipmentGrade);
                break;
            case OptionKey.Option_AutoSellRuneGradeType:
                Auto_Sell_PetEquipmentGradeType = (GameDB.E_RuneGradeType)Value;
                DeviceSaveDatas.SaveCurCharData(nameof(Auto_Sell_PetEquipmentGradeType), (int)Auto_Sell_PetEquipmentGradeType);
                break;
        }

        if (bCallDelegate)
        {
            OnOptionChanged?.Invoke(key);
            //ZLog.Log("save data : "+ JSON.Dump(_Instance));
            //DeviceSaveDatas.SaveData("GameOption", JSON.Dump(_Instance));
        }
    }

    #region 시나리오용 옵션 셋팅
    private float CachedNormalHpPotionPer;
    private float CachedBigHpPotionPer;
    private bool CachedBigPotionAutoBuy;
    private bool CachedNormalHPPotionBuy;
    private HPPotionUsePriority CachedPriority;

    private bool IsSetScenarioOption;

    public void SetScenarioPotion(uint itemTid, float potionPer)
    {
        if (IsSetScenarioOption)
            return;
        //static uint NORMAL_HP_POTION_TID = 6000;
        //static uint BIG_HP_POTION_TID = 6100;

        CachedPriority = HP_PotionUsePriority;
        if (6000 == itemTid)
        {
            HP_PotionUsePriority = HPPotionUsePriority.NORMAL;
        }
        else
        {
            HP_PotionUsePriority = HPPotionUsePriority.BIG;
        }

        IsSetScenarioOption = true;

        CachedBigHpPotionPer = ZGameOption.Instance.BigHPPotionPer;
        CachedNormalHpPotionPer = ZGameOption.Instance.NormalHPPotionPer;

        BigHPPotionPer = potionPer;
        NormalHPPotionPer = potionPer;

        CachedBigPotionAutoBuy = BigPotionAutoBuy;
        CachedNormalHPPotionBuy = NormalPotionAutoBuy;

        BigPotionAutoBuy = false;
        NormalPotionAutoBuy = false;

        OnOptionChanged?.Invoke(OptionKey.Option_Big_PotionPer);
        OnOptionChanged?.Invoke(OptionKey.Option_Normal_PotionPer);
        OnOptionChanged?.Invoke(OptionKey.Option_HP_Potion_Priority);
    }

    public void ResetScenarioPotion()
    {
        if (false == IsSetScenarioOption)
            return;

        IsSetScenarioOption = false;

        BigHPPotionPer = CachedBigHpPotionPer;
        NormalHPPotionPer = CachedNormalHpPotionPer;

        BigPotionAutoBuy = CachedBigPotionAutoBuy;
        NormalPotionAutoBuy = CachedNormalHPPotionBuy;

        HP_PotionUsePriority = CachedPriority;
    }
    #endregion

    #region Use
    public enum HPPotionUsePriority
    {
        NORMAL,
        BIG,
    }
    [TinyJSON.Include]
    public HPPotionUsePriority HP_PotionUsePriority { private set; get; } //물약자동사용 우선순위설정
    [TinyJSON.Include]
    public float NormalHPPotionPer { private set; get; }                 //일반HP물약사용체력설정%
    [TinyJSON.Include]
    public bool NormalPotionAutoBuy { private set; get; }                  //일반물약자동구매

    [TinyJSON.Include]
    public float BigHPPotionPer { private set; get; }                    //고급HP물약사용체력설정%
    [TinyJSON.Include]
    public bool BigPotionAutoBuy { private set; get; }                //고급물약자동구매

    [TinyJSON.Include]
    public float RemainMPPer { private set; get; }                      //자동 사냥시 최소 잔여 MP설정

    [TinyJSON.Include]
    public uint GodTearStackCnt { private set; get; }                   //신의축복 버프 스택이 특정 개수이하가 되면 해당개수가 될때까지 신의 눈물 자동 사용

    public enum BuffUseType
    {
        EveryWhere, //어디서든 언제든 조건에 맞으면 사용
        OnlyField,  //마을에서는 제외
        CheckMove,  //수동 조작시 이동을 감지하여 사용
    }
    [TinyJSON.Include]
    public BuffUseType Buff_UseType { private set; get; }        //퀵슬롯에 등록된 버프 사용에 대한 설정
    #endregion

    #region Battle
    [System.Flags]
    public enum AutoBattleTargetPriority
    {
        TARGET_HOSTILE_MONSTER = 1,//선공 몬스터 1순위 체크
        TARGET_QUEST_MONSTER = 2,//퀘스트 몬스터 2순위 체크
        TARGET_NEAR_MONSTER = 4, //가까운 몬스터 3순위 체크 
    }
    [TinyJSON.Include]
    public AutoBattleTargetPriority AutoBattle_Target { private set; get; }    //자동사냥에서 타겟 설정시 ,  

    [TinyJSON.Include]
    public bool bAutoSummon_Boss { private set; get; }           // 보스 자동 소환 (로컬)

    [TinyJSON.Include]
    public bool bManner_Search { private set; get; }             // 매너 모드 - 타 플레이어가 공격중인 몬스터는 자동사냥 탐색에서 제외한다.
    [TinyJSON.Include]
    public float Auto_Screen_Saver_Time { private set; get; }    // 자동 절전 모드 - 설정 시간
    [TinyJSON.Include]
    public bool bAuto_Wakeup_ScreenSaver { private set; get; }               // 타 플레이어에게 피격시 절전모드 자동해제

    [System.Flags]
    public enum ScanSearchTargetType
    {
        TARGET_QUEST_MONSTER = 1,
        TARGET_NORMAL_MONSTER = 2,
        TARGET_HOSTILE_MONSTER = 4,
        TARGET_ENEMYGUILD_PLAYER = 8,
        TARGET_ALERT_PLAYER = 16,
        TARGET_NORMAL_PLAYER = 32,
        TARGET_MYGUILD_PLAYER = 64,
        TARGET_ALLIANCEGUILD_PLAYER = 128,
        TARGET_PARTY_PLAYER = 256,
    }

    public ScanSearchTargetType ScanSearchTarget_Type { private set; get; }

    public bool bMonsterPriority { private set; get; }

    public enum ScanSearchTargetPriority
    {
        TARGET_MONSTER = 1,
        TARGET_PLAYER = 2
    }

    public ScanSearchTargetPriority ScanSearchTarget_Priority { private set; get; }
    #endregion

    #region Get
    [System.Flags]
    public enum AutoBreakType
    {
        OFF = 0,
        Tier_1 = 1,
        Tier_2 = 2,
    }
    [TinyJSON.Include]
    public AutoBreakType Auto_Break_Belong_Item { private set; get; }    // 귀속 아이템 자동 분해 설정(중복 선택 가능)
    [TinyJSON.Include]
    public float Auto_Return_Town_Weight_Per { private set; get; }       // 자동 마을 귀환(무게 %에 따라 귀환)

    [System.Flags]
    public enum AutoSellPetEquipmentType
    {
        OFF = 0,
        Grade_01 = 1,
        Grade_02 = 2,
        Grade_03 = 3,
    }

    [TinyJSON.Include]
    public AutoSellPetEquipmentType Auto_Sell_PetEquipmentGrade { private set; get; }             //룬 자동 판매 등급
    [TinyJSON.Include]
    public GameDB.E_RuneGradeType Auto_Sell_PetEquipmentGradeType { private set; get;}    //자동 판매 룬등급
#endregion

#region Control
    [System.Flags]
    public enum SearchTargetPriority
    {
        TARGET_BE_ATTACK_PLAYER = 1, // 1순위 나를 공격중인 캐릭터
        TARGET_ENEMY_GUILD = 2,//2순위 적대 길드
        TARGET_NEAR_CHARACTER = 4,//3순위 가까운 캐릭터
    }
    [TinyJSON.Include]
    public SearchTargetPriority Search_Target_Priority { private set; get; }  //타겟 선택 시 우선순위(중복 선택 가능) 

    [TinyJSON.Include]
    public bool bUseVirtualPad { private set; get; }                     //버츄얼 패드 사용
    [TinyJSON.Include]
    public bool bUseTargetCancel{ private set; get; }                     //타겟 캔슬 버튼 사용
    [TinyJSON.Include]
    public bool bAllowPartyInvite{ private set; get; }                    //파티 초대 수신
    [TinyJSON.Include]
    public bool bAllowPartyJoin { private set; get; }                    //파티 신청 수신
    [TinyJSON.Include]
    public bool bVibration { private set; get; }                    //진동 on/off
    #endregion

    #region Enviroment
    [TinyJSON.Include]
    public float BGMSound { private set; get; }                    //배경음 크기
    [TinyJSON.Include]
    public float SFXSound { private set; get; }                    //효과음 크기
	/// <summary> 게임 퀄리티 설정 값 <see cref="E_Quality"/>참고 </summary>
	[TinyJSON.Include]
	public E_Quality Quality { private set; get; }

    [TinyJSON.Include]
    public bool bShowDefaultCharacterText { private set; get; }      //캐릭터 정보 출력 여부(hud의 좌측 상단 텍스트)
    [TinyJSON.Include]
    public bool bShowCharacterName { private set; get; }
    [TinyJSON.Include]
    public bool bShowTargetUIText { private set; get; }              //캐릭터 이름 출력 여부(TargetUI의 내 이름 표시)
    [TinyJSON.Include]
    public bool bShowExpGainEffect { private set; get; }             //경험치 획득 연출
    [TinyJSON.Include]
    public bool bShowGoldGainEffect { private set; get; }            //골드 획득 연출
    [TinyJSON.Include]
    public bool bShowDamageEffect { private set; get; }              //데미지 이펙트 연출
    [TinyJSON.Include]
    public bool bShowDodgeEffect { private set; get; }               //회피 이펙트 연출
#endregion

#region Alram
    [TinyJSON.Include]
    public uint AlramHPPotion { private set; get; }                //체력회복약 일정 갯수 이하일때 진동 알림(0~100)
    [TinyJSON.Include]
    public float AlramHPPer { private set; get; }                  //체력이하일때 진동 알림(0~100)
    [TinyJSON.Include]
    public float AlramWeight { private set; get; }                 //무게 일정 이상일때 진동 알림(0~100)
    [TinyJSON.Include]
    public bool bAlramBeAttacked_PC { private set; get; }            //자동 사냥중 다른 플레이어에게 피격시 알림
    [TinyJSON.Include]
    public float AlramSound { private set; get; }                    //경고음 크기

    [TinyJSON.Include]
    public bool bPushEnable { private set; get; }                    //푸시 수신
    [TinyJSON.Include]
    public bool bNightPushEnable { private set; get; }               //야간 푸시 수신
    [TinyJSON.Include]
    public bool bAlram_Friend_Connect { private set; get; }          //친구 접속 알림
    [TinyJSON.Include]
    public bool bAlram_Enemy_Connect { private set; get; }           //경계대상 접속 알림
    [TinyJSON.Include]
    public bool bAlram_Guild_Member_Connect { private set; get; }    //길드원 접속 알림

    public enum GetItemAlram
    {
        ALL,//전체
        HIGH,//고급
        RARE,//희귀
        OFF,
    }
    [TinyJSON.Include]
    public GetItemAlram Alram_Party_GetItem { private set; get; }    //파티원 아이템 획득 알림

	public bool bHideGetitemNickName { private set; get; }

#endregion

#region Function
//	public static void CallNotification(OptionKey key, string Msg)
//	{
//		UIManager.WarnMessage(Msg);
//#if UNITY_IPHONE || UNITY_ANDROID
//        Handheld.Vibrate();
//#endif
//        if (SoundManager.instance)
//            SoundManager.instance.PlaySFX(40101);
//	}
#endregion
}
