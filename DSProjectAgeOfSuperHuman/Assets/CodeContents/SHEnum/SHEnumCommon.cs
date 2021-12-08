using UnityEngine;
using NSkill;
using NPacketData;

public enum EDamageResult
{
	None,
	Normal,
	Block,
	Dodge,
	Critical,
	Absorb,
}

public enum EHeroType
{
	None,
	ImGang = 101,
	CodeNameB = 102,
	SaintPearl = 103,
	OBangOne = 104,
	Widro = 105,
	Dion = 106,
	JainSonata = 107,
	MaximBasilov = 108,
	Cyrilo = 109,
	Nangi = 110,
	DanYang = 111,
}

public enum EItemType
{
	None,
	Equipment,
	Consumable,
	Event,
}

public enum EItemConsumeType
{
	None,
	Gold,
	Exp,
	Gacha,
	Cash,
}

public enum EPriceType
{
	None,
	Gold,
	Cash,
}

public enum EItemGrade
{
	None = 0,
	Common_1,
	Common_2,
	Common_3,
	Common_4,

	Uncommon_5,
	Uncommon_6,
	Uncommon_7,
	Uncommon_8,

	Rare_9,
	Rare_10,
	Rare_11,
	Rare_12,

	Epic_13,
	Epic_14,
	Epic_15,
	Epic_16,

	Legend_17,
	Legend_18,
	Legend_19,
	Legend_20,

	Artifact_21,
	Artifact_22,
	Artifact_23,
	Artifact_24,
}

public enum EItemGradeUI
{
	Common,
	Uncommon,
	Rare,
	Epic,
	Legend,
	Artifact,
}

public enum ECurrencyType
{
	None,
	Gold,
	Diamond,
	Soul,
}

public enum EOpenContentsType
{
	None,
}

public enum ESkillDescriptionType
{
	None,
	Normal,
	Active,
	Passive,
	AutoCasting,
}

public enum EEquipSlot
{
	None,
	Main,
	Sub1,
	Sub2,
}

public enum EUnitType
{
	None,
	Hero,
	Enemy,
}

public enum EUnitTagPosition
{
	None,
	Center,
	Left,
	Right,
}

public enum EPotentialBingoOptionType
{
	None,
	SlotPanel,		// 일반 슬롯에서 나오는 옵션
	BingoPanel,		// 빙고 슬롯에서 나오는 옵션 
	BingoColor,	    // 빙고 컬러에서 나오는 옵션 
	BingoComplete,	// 빙고 전체 완성시 나오는 옵션 
}

//--------------------------------------------------------------------------
public class SDamageResult
{
	public EDamageType	eDamageType = EDamageType.None;
	public float			fTotalValue = 0;
	public EDamageResult	eDamageResult = EDamageResult.None;
	public SHUnitBase	    pAttacker = null;
	public SHUnitBase		pDefender = null;
}

public class SItemData
{
	public uint			ItemID = 0;
	public bool			ItemEquip = false;
	public SPacketItem	ItemIDB = null;					
	public SHScriptTableItem.SItemTable ItemTable = null;	
}

public abstract class SHEnumCommon {}
