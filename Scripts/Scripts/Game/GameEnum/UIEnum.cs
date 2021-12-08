public enum E_UIFrameType
{
    None = 0,
    UIFrameLogo,
    UIFramePatchProcess,
    UIFrameLogin,
    UIFrameLoginN,
    UIFrameLoadingScreen,
    UIFrameCharacterSelect,
    UIFrameCashShop,
    
    UIFramePopupItemInfo,
    UIPopupSystem,

    UIFrameHUD,
    UIScreenBlock,
    UIFrameConsolePopup,
    UISubHUDBottom,
    UISubHUDCharacterAction,
    UISubHUDCharacterState,
    UISubHUDLeftMenu,
    UISubHUDMenu,
    UISubHUDMiniMap,
    UISubHUDQuest,
    UISubHUDQuickSlot,
    UISubHUDCurrency,
    UISubHUDPartyMenu,
    UISubHUDJoyStick,
    UIPopupServerSelect,
    UIFrameMailbox,
    UIFrameInventory,
    UIFrameChange,
    UIFramePet,
    UIFramePetChangeSelect,
    UIFrameTemple
}

public enum E_UILocalizing
{
    Korean = 0,  
    English,
    Chinese,
}

public enum E_UIThemaType
{
    Default = 0,
}

public enum E_UIItemGrade
{
    Gray_Lv1,
    Green_Lv2,
    Blue_Lv3,
}

public enum E_UIconBorderType
{
    Border_1,
    Border_2,
}

public enum E_UIStyle
{
    Normal = 0,
    FullScreen = 1,
    IncludeSubScene = 2, // Addictive Scene을 사용하는 UI
}

public enum E_MailboxWindow
{
    Mail = 0,
    Message = 1,
    MessageInside = 2
}

public enum E_HUDMenu
{
    Mailbox = 0,
    Change = 1,
    Pet = 2,
    Ride = 3,
    Friend = 4,
    Guild = 5,
    GodLand = 6,
}

public enum E_InvenSortType
{
    All = 0,
    Equipment = 1,
    ETC = 2,
    Disassemble = 3,
    Enhance = 4,
    Upgrade = 5,
    Enchant = 6,
    EnhanceEquip = 7,
    EnchantEquip = 8,
}

public enum E_EnchantResultType
{
    None,       /*없음*/
    Success,    /*성공*/
    Fail,       /*실패*/
    BigSuccess, /*대성공*/
    DownSuccess /*저주강화성공*/
}

public enum E_ItemPopupButton
{
    Use = 0,
    Equip = 1,
    UnEquip = 2,
    Delete = 3,
    Disassamble = 4,
    Enhance = 5,
    Upgrade = 6,
    Enchant = 7,
    Collection = 8,
    Exchange = 9,
    GemEquip = 10,
    GemUnEquip = 11,
    Lock = 12,
    UnLock = 13,
    ExchangeByMileage = 14,
}

[System.Flags]
public enum E_ItemPopupFlag
{
    None =          0,
    Use =           1<<0,
    Equip =         1<<1,
    UnEquip =       1<<2,
    Delete =        1<<3,
    Disassamble =   1<<4,
    Enhance =       1<<5,
    Upgrade =       1<<6,
    Enchant =       1<<7,
    Info =          1<<8,
    DownGrade =     1<<9,
    Reinforce =     1<<10,
    Disarm =        1<<11,
    Collection =    1<<12,
    Sign =          1<<13,
    Reforging =     1<<14,
    Exchange =      1<<15,
    GemEquip =      1<<16,
    GemUnEquip =    1<<17,
    Lock       =    1<<18,
    Unlock     =    1<<19,
    ExchangeByMileage = 1 << 20,
}

public enum E_ItemPopupType
{
    Mailbox = 0,
    InventoryEquipment = 1,
    InventoryStack = 2,
    CharacterStateEquip = 3,
    Storage = 4,
    Collection = 5,
    Exchange = 6,
    Reward = 7,
    GemEquip = 8,
    GemUnEquip = 9,
    MileageShop = 10,
    InfinityTower = 11,
    None = 12,
}

public enum E_CharacterSelectState
{
    Select = 0,
    Create = 1
}

public enum E_CharacterState
{
    Equip = 0,
    Status = 1
}

public enum E_CharacterStateEquip
{
    Equipment = 0,
    Accessory = 1,
    PetRide = 2
}

public enum E_CharacterStateEquipSlot
{
    Helmet = 0,
    Armor = 1,
    Gloves = 2,
    Shoes = 3,
    MainWeapon = 4,
    Cape = 5,
    Belt = 6,
    Necklace = 7,
    SubWeapon = 8,
    Artifact = 9,
    Earring1 = 10,
    Bracelet1 = 11,
    Ring1 = 12,
    Ring2 = 13,
    Earring2 = 14,
    Bracelet2 = 15,
    Ring3 = 16,
    Ring4 = 17
}

public enum E_SkillClassType
{
    Knight = 0,
    Assassin = 1,
    Archer = 2,
    Wizard = 3,
    All = 4
}

public enum E_MessageType
{
    MainNotice = 0,
    SimpleNotice = 1
}

public enum E_NoticeOption
{
    TypeA = 0,
    TypeB = 1
}

public enum E_UIFadeType
{
    FadeIn = 0,
    FadeOut = 1
}

public enum E_UIToggleState
{
    Normal = 0,
    On = 1,
}

public enum E_Stat
{
    STR = 0,
    DEX = 1,
    INT = 2,
    WIS = 3,
    VIT = 4
}

public enum E_CloseHudState
{
    None = 0,
    UISubScene = 1,
    FullFrame = 2,
}

public enum E_TradeSearchMenu
{
    Main = 0,
    All = 1,
    Weapon = 2,
    Armor = 3,
    Accessory = 4,
    SkillBook = 5,
    Consumable = 6,
    ETC = 7,
}

public enum E_TradeMenu
{
    Search = 0,
    Sell = 1,
    Settlement = 2,
    SettlementLog = 3,
}

public enum E_KillMessage
{
    OurTeamDead = 0,
    EnemyTeamDead = 1
}