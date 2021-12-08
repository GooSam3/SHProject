using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPacketData
{
	[System.Serializable]
	public struct SIntPair
	{
		[SerializeField]
		public uint Value1;
		[SerializeField]
		public uint Value2;

		public SIntPair(uint iValue1, uint iValue2) { Value1 = iValue1; Value2 = iValue2; }
	}

	[System.Serializable]
	public class SPacketStageClear : CObjectInstanceBase
	{
		[SerializeField]
		public uint WorldIndex = 0;
		[SerializeField]
		public uint StageIndex = 0;
	}

	[System.Serializable]
	public class SPacketHero : CObjectInstanceBase
	{
		[SerializeField]
		public uint HeroID = 0;
		[SerializeField]
		public ulong HeroSID = 0;
		[SerializeField]
		public uint EquipMain = 0;
		[SerializeField]
		public uint EquipSub1 = 0;
		[SerializeField]
		public uint EquipSub2 = 0;

		[SerializeField]
		public List<uint> HeroStoryClear = new List<uint>(); // 하나의 초인이 여러게의 스토리를 가질수 있다 (시즌별)
	}

	[System.Serializable]
	public class SPacketHeroSkill : CObjectInstanceBase
	{
		[SerializeField]
		public uint SkillID = 0;
		[SerializeField]
		public uint SkillLevel = 0;
	}

	[System.Serializable]
	public class SPackatStatCache : CObjectInstanceBase
	{
		[SerializeField]
		public uint HeroID = 0;

		[SerializeField]
		public SPacketStatValue StatValue = new SPacketStatValue();
	}

	[System.Serializable]
	public class SPacketStatValue : CObjectInstanceBase  // 나중에 암호화 할것
	{
		[SerializeField]
		public uint LevelAttack = 0;
		[SerializeField]
		public uint LevelDefense = 0;
		[SerializeField]
		public uint LevelStamina = 0;

		[SerializeField]
		public uint LevelAttackPercent = 0;
		[SerializeField]
		public uint LevelDefensePercent = 0;
		[SerializeField]
		public uint LevelStaminaPercent = 0;

		[SerializeField]
		public uint LevelAttackSkill = 0;		
		[SerializeField]
		public uint LevelDefenseSkill = 0;
		[SerializeField]
		public uint LevelCritical = 0;
		[SerializeField]
		public uint LevelCriticalAnti = 0;
		[SerializeField]
		public uint LevelCriticalDamage = 0;
		[SerializeField]
		public uint LevelCriticalDamageAnti = 0;
		[SerializeField]
		public uint LevelHit = 0;
		[SerializeField]
		public uint LevelDodge = 0;
		[SerializeField]
		public uint LevelBlock = 0;
		[SerializeField]
		public uint LevelBlockAnti = 0;
		[SerializeField]
		public uint LevelRecoverPerSecond = 0;
		
		[SerializeField]
		public uint LevelExtraGold = 0;
		[SerializeField]
		public uint LevelExtraEXP = 0;
		[SerializeField]
		public uint LevelExtraItem = 0;
	}

	[System.Serializable]
	public class SPacketHeroStatUpgrade : CObjectInstanceBase
	{
		[SerializeField]
		public uint HeroID = 0;
		[SerializeField]
		public uint EXPCurrent = 0;
		[SerializeField]
		public int HeroLevel = 0;
		[SerializeField]
		public int RemainStat = 0;
		[SerializeField]
		public List<SIntPair> UpgradePoint = new List<SIntPair>(); // value1 = UpgradeID / value2 = Point
	}

	[System.Serializable]
	public class SPacketHeroDeck : CObjectInstanceBase
	{
		[System.Serializable]
		public class SDeckInfo : CObjectInstanceBase
		{
			[SerializeField]
			public uint ReaderID = 0;
			[SerializeField]
			public List<uint> DeckMember = new List<uint>();
		}
	
		[SerializeField]
		public int SelectedDeck = 0;
		[SerializeField]
		public List<SDeckInfo> DeckList = new List<SDeckInfo>();
	}

	[System.Serializable]
	public class SPacketItem : CObjectInstanceBase
	{
		[SerializeField]
		public uint ItemTID = 0;
		[SerializeField]
		public ulong ItemSID = 0;       // 서버측 식별자 
		[SerializeField]
		public int ItemLevel = 0;
		[SerializeField]
		public int ItemCount = 0;
		[SerializeField]
		public List<uint>		ItemOptionID;
		[SerializeField]
		public List<float>	ItemOptionValue;
	}

	[System.Serializable]
	public class SPacketPlayerAccount : CObjectInstanceBase
	{
		[SerializeField]
		public uint SessionID = 0;
		[SerializeField]
		public string PlayerName = "테스트 계정";
	}

	[System.Serializable]
	public class SPacketCurrency : CObjectInstanceBase
	{
		[SerializeField]
		public uint Gold = 0;
		[SerializeField]
		public uint Diamond = 0;
		[SerializeField]
		public uint Soul = 0;
	}
	//--------------------------------------------------빙고 관련---------------------------------------------------------
	[System.Serializable]
	public class SPacketPotentialBingoInfo : CObjectInstanceBase
	{
		[SerializeField]
		public uint HeroID = 0;
		[SerializeField]
		public List<SPacketPotentialBingoPage> BingoPage = new List<SPacketPotentialBingoPage>();
	}

	[System.Serializable]
	public class SPacketPotentialBingoPage : CObjectInstanceBase
	{
		[SerializeField]
		public uint HeroID = 0;
		[SerializeField]
		public uint PageIndex = 0;
		[SerializeField]
		public List<SPacketPotentialBingoSlot> BingoBoard = new List<SPacketPotentialBingoSlot>(); // 순서대로 1,2,3.....빙고판의 현재 상태를 묘사
	}

	[System.Serializable]
	public class SPacketPotentialBingoSlot : CObjectInstanceBase
	{
		public uint HeroID = 0;
		[SerializeField]
		public uint PageIndex = 0;
		[SerializeField]
		public uint SlotIndex = 0;
		[SerializeField]
		public List<SPacketPotentialBingoOption> BingoOption = new List<SPacketPotentialBingoOption>();
	}

	[System.Serializable]
	public class SPacketPotentialBingoOption : CObjectInstanceBase
	{
		[SerializeField]
		public uint StatID = 0;		// ESHStatType
		[SerializeField]
		public uint OptionType = 0;	// EPotentialBingoOptionType
		[SerializeField]
		public uint StatValue = 0;
	}
}


public class SHGameDBData{}
