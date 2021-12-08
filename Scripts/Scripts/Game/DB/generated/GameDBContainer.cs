//***Auto Generation Code****

using System;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace GameDB
{
	public class GameDBContainer
	{
		public Dictionary<uint, Ability_Table> Ability_Table_data = new Dictionary<uint, Ability_Table>();
		public Dictionary<uint, AbilityAction_Table> AbilityAction_Table_data = new Dictionary<uint, AbilityAction_Table>();
		public Dictionary<string, Animation_Table> Animation_Table_data = new Dictionary<string, Animation_Table>();
		public Dictionary<uint, Artifact_Table> Artifact_Table_data = new Dictionary<uint, Artifact_Table>();
		public Dictionary<uint, ArtifactLink_Table> ArtifactLink_Table_data = new Dictionary<uint, ArtifactLink_Table>();
		public Dictionary<uint, AttendEvent_Table> AttendEvent_Table_data = new Dictionary<uint, AttendEvent_Table>();
		public Dictionary<uint, Attribute_Table> Attribute_Table_data = new Dictionary<uint, Attribute_Table>();
		public Dictionary<uint, AttributeChain_Table> AttributeChain_Table_data = new Dictionary<uint, AttributeChain_Table>();
		public Dictionary<uint, AttributeDamage_Table> AttributeDamage_Table_data = new Dictionary<uint, AttributeDamage_Table>();
		public Dictionary<uint, BossWar_Table> BossWar_Table_data = new Dictionary<uint, BossWar_Table>();
		public Dictionary<uint, Broadcast_Table> Broadcast_Table_data = new Dictionary<uint, Broadcast_Table>();
		public Dictionary<uint, Change_Table> Change_Table_data = new Dictionary<uint, Change_Table>();
		public Dictionary<uint, ChangeCollection_Table> ChangeCollection_Table_data = new Dictionary<uint, ChangeCollection_Table>();
		public Dictionary<uint, ChangeCompose_Table> ChangeCompose_Table_data = new Dictionary<uint, ChangeCompose_Table>();
		public Dictionary<uint, ChangeList_Table> ChangeList_Table_data = new Dictionary<uint, ChangeList_Table>();
		public Dictionary<uint, ChangeQuest_Table> ChangeQuest_Table_data = new Dictionary<uint, ChangeQuest_Table>();
		public Dictionary<uint, ChangeQuestLevel_Table> ChangeQuestLevel_Table_data = new Dictionary<uint, ChangeQuestLevel_Table>();
		public Dictionary<uint, Character_Table> Character_Table_data = new Dictionary<uint, Character_Table>();
		public Dictionary<uint, ClassChange_Table> ClassChange_Table_data = new Dictionary<uint, ClassChange_Table>();
		public Dictionary<uint, ColoSeasonReward_Table> ColoSeasonReward_Table_data = new Dictionary<uint, ColoSeasonReward_Table>();
		public Dictionary<byte, Colosseum_Table> Colosseum_Table_data = new Dictionary<byte, Colosseum_Table>();
		public Dictionary<string, Config_Table> Config_Table_data = new Dictionary<string, Config_Table>();
		public Dictionary<uint, Cooking_Table> Cooking_Table_data = new Dictionary<uint, Cooking_Table>();
		public Dictionary<uint, CouponGroup_Table> CouponGroup_Table_data = new Dictionary<uint, CouponGroup_Table>();
		public Dictionary<uint, DailyQuest_Table> DailyQuest_Table_data = new Dictionary<uint, DailyQuest_Table>();
		public Dictionary<uint, DailyQuestList_Table> DailyQuestList_Table_data = new Dictionary<uint, DailyQuestList_Table>();
		public Dictionary<uint, DeathPenalty_Table> DeathPenalty_Table_data = new Dictionary<uint, DeathPenalty_Table>();
		public Dictionary<uint, Dialogue_Table> Dialogue_Table_data = new Dictionary<uint, Dialogue_Table>();
		public Dictionary<uint, DropGroup_Table> DropGroup_Table_data = new Dictionary<uint, DropGroup_Table>();
		public Dictionary<uint, Effect_Table> Effect_Table_data = new Dictionary<uint, Effect_Table>();
		public Dictionary<uint, Emoticon_Table> Emoticon_Table_data = new Dictionary<uint, Emoticon_Table>();
		public Dictionary<uint, EventList_Table> EventList_Table_data = new Dictionary<uint, EventList_Table>();
		public Dictionary<uint, EventReward_Table> EventReward_Table_data = new Dictionary<uint, EventReward_Table>();
		public Dictionary<uint, Gacha_Table> Gacha_Table_data = new Dictionary<uint, Gacha_Table>();
		public Dictionary<uint, GodBuff_Table> GodBuff_Table_data = new Dictionary<uint, GodBuff_Table>();
		public Dictionary<uint, GodLand_Table> GodLand_Table_data = new Dictionary<uint, GodLand_Table>();
		public Dictionary<uint, Guild_Table> Guild_Table_data = new Dictionary<uint, Guild_Table>();
		public Dictionary<uint, GuildBuff_Table> GuildBuff_Table_data = new Dictionary<uint, GuildBuff_Table>();
		public Dictionary<byte, GuildMark_Table> GuildMark_Table_data = new Dictionary<byte, GuildMark_Table>();
		public Dictionary<uint, InfiBuff_Table> InfiBuff_Table_data = new Dictionary<uint, InfiBuff_Table>();
		public Dictionary<uint, InfinityDungeon_Table> InfinityDungeon_Table_data = new Dictionary<uint, InfinityDungeon_Table>();
		public Dictionary<uint, InfinitySchedule_Table> InfinitySchedule_Table_data = new Dictionary<uint, InfinitySchedule_Table>();
		public Dictionary<uint, Infor_Table> Infor_Table_data = new Dictionary<uint, Infor_Table>();
		public Dictionary<uint, Item_Table> Item_Table_data = new Dictionary<uint, Item_Table>();
		public Dictionary<uint, ItemCollection_Table> ItemCollection_Table_data = new Dictionary<uint, ItemCollection_Table>();
		public Dictionary<uint, ItemEnchant_Table> ItemEnchant_Table_data = new Dictionary<uint, ItemEnchant_Table>();
		public Dictionary<uint, Level_Table> Level_Table_data = new Dictionary<uint, Level_Table>();
		public Dictionary<uint, LineLog_Table> LineLog_Table_data = new Dictionary<uint, LineLog_Table>();
		public Dictionary<uint, ListGroup_Table> ListGroup_Table_data = new Dictionary<uint, ListGroup_Table>();
		public Dictionary<uint, Loading_Table> Loading_Table_data = new Dictionary<uint, Loading_Table>();
		public Dictionary<uint, LoadingTip_Table> LoadingTip_Table_data = new Dictionary<uint, LoadingTip_Table>();
		public Dictionary<string, Locale_Table> Locale_Table_data = new Dictionary<string, Locale_Table>();
		public Dictionary<uint, Mail_Table> Mail_Table_data = new Dictionary<uint, Mail_Table>();
		public Dictionary<uint, Make_Table> Make_Table_data = new Dictionary<uint, Make_Table>();
		public Dictionary<uint, Mark_Table> Mark_Table_data = new Dictionary<uint, Mark_Table>();
		public Dictionary<uint, MarkEnchant_Table> MarkEnchant_Table_data = new Dictionary<uint, MarkEnchant_Table>();
		public Dictionary<uint, MezRate_Table> MezRate_Table_data = new Dictionary<uint, MezRate_Table>();
		public Dictionary<uint, MileageShop_Table> MileageShop_Table_data = new Dictionary<uint, MileageShop_Table>();
		public Dictionary<uint, Monster_Table> Monster_Table_data = new Dictionary<uint, Monster_Table>();
		public Dictionary<uint, MonsterDrop_Table> MonsterDrop_Table_data = new Dictionary<uint, MonsterDrop_Table>();
		public Dictionary<uint, NormalShop_Table> NormalShop_Table_data = new Dictionary<uint, NormalShop_Table>();
		public Dictionary<uint, NPC_Table> NPC_Table_data = new Dictionary<uint, NPC_Table>();
		public Dictionary<uint, Object_Table> Object_Table_data = new Dictionary<uint, Object_Table>();
		public Dictionary<uint, Pet_Table> Pet_Table_data = new Dictionary<uint, Pet_Table>();
		public Dictionary<uint, PetAdventure_Table> PetAdventure_Table_data = new Dictionary<uint, PetAdventure_Table>();
		public Dictionary<uint, PetCollection_Table> PetCollection_Table_data = new Dictionary<uint, PetCollection_Table>();
		public Dictionary<uint, PetCompose_Table> PetCompose_Table_data = new Dictionary<uint, PetCompose_Table>();
		public Dictionary<uint, PetGrowth_Table> PetGrowth_Table_data = new Dictionary<uint, PetGrowth_Table>();
		public Dictionary<uint, PetLevel_Table> PetLevel_Table_data = new Dictionary<uint, PetLevel_Table>();
		public Dictionary<uint, PetList_Table> PetList_Table_data = new Dictionary<uint, PetList_Table>();
		public Dictionary<uint, PKBuff_Table> PKBuff_Table_data = new Dictionary<uint, PKBuff_Table>();
		public Dictionary<uint, Portal_Table> Portal_Table_data = new Dictionary<uint, Portal_Table>();
		public Dictionary<uint, Price_Table> Price_Table_data = new Dictionary<uint, Price_Table>();
		public Dictionary<uint, Quest_Table> Quest_Table_data = new Dictionary<uint, Quest_Table>();
		public Dictionary<uint, QuestEvent_Table> QuestEvent_Table_data = new Dictionary<uint, QuestEvent_Table>();
		public Dictionary<uint, RankBuff_Table> RankBuff_Table_data = new Dictionary<uint, RankBuff_Table>();
		public Dictionary<uint, Resource_Table> Resource_Table_data = new Dictionary<uint, Resource_Table>();
		public Dictionary<uint, Restoration_Table> Restoration_Table_data = new Dictionary<uint, Restoration_Table>();
		public Dictionary<uint, RuneComponent_Table> RuneComponent_Table_data = new Dictionary<uint, RuneComponent_Table>();
		public Dictionary<uint, RuneEnchant_Table> RuneEnchant_Table_data = new Dictionary<uint, RuneEnchant_Table>();
		public Dictionary<uint, RuneOption_Table> RuneOption_Table_data = new Dictionary<uint, RuneOption_Table>();
		public Dictionary<uint, RuneSet_Table> RuneSet_Table_data = new Dictionary<uint, RuneSet_Table>();
		public Dictionary<uint, ScenarioAbility_Table> ScenarioAbility_Table_data = new Dictionary<uint, ScenarioAbility_Table>();
		public Dictionary<uint, ScenarioDirection_Table> ScenarioDirection_Table_data = new Dictionary<uint, ScenarioDirection_Table>();
		public Dictionary<uint, ScenarioMission_Table> ScenarioMission_Table_data = new Dictionary<uint, ScenarioMission_Table>();
		public Dictionary<uint, ShopList_Table> ShopList_Table_data = new Dictionary<uint, ShopList_Table>();
		public Dictionary<uint, Skill_Table> Skill_Table_data = new Dictionary<uint, Skill_Table>();
		public Dictionary<uint, SmeltOptionRate_Table> SmeltOptionRate_Table_data = new Dictionary<uint, SmeltOptionRate_Table>();
		public Dictionary<uint, SmeltScroll_Table> SmeltScroll_Table_data = new Dictionary<uint, SmeltScroll_Table>();
		public Dictionary<uint, SmeltScrollOption_Table> SmeltScrollOption_Table_data = new Dictionary<uint, SmeltScrollOption_Table>();
		public Dictionary<uint, Sound_Table> Sound_Table_data = new Dictionary<uint, Sound_Table>();
		public Dictionary<uint, SpecialShop_Table> SpecialShop_Table_data = new Dictionary<uint, SpecialShop_Table>();
		public Dictionary<uint, Stage_Table> Stage_Table_data = new Dictionary<uint, Stage_Table>();
		public Dictionary<uint, StageDrop_Table> StageDrop_Table_data = new Dictionary<uint, StageDrop_Table>();
		public Dictionary<uint, StageDropList_Table> StageDropList_Table_data = new Dictionary<uint, StageDropList_Table>();
		public Dictionary<uint, StartingItem_Table> StartingItem_Table_data = new Dictionary<uint, StartingItem_Table>();
		public Dictionary<uint, Summon_Table> Summon_Table_data = new Dictionary<uint, Summon_Table>();
		public Dictionary<uint, Survery_Table> Survery_Table_data = new Dictionary<uint, Survery_Table>();
		public Dictionary<uint, Temple_Table> Temple_Table_data = new Dictionary<uint, Temple_Table>();
		public Dictionary<uint, TempleObject_Table> TempleObject_Table_data = new Dictionary<uint, TempleObject_Table>();
		public Dictionary<uint, Tutorial_Table> Tutorial_Table_data = new Dictionary<uint, Tutorial_Table>();
		public Dictionary<byte, UIResource_Table> UIResource_Table_data = new Dictionary<byte, UIResource_Table>();
		public Dictionary<uint, UpgradeList_Table> UpgradeList_Table_data = new Dictionary<uint, UpgradeList_Table>();
		public Dictionary<uint, WeightPenalty_Table> WeightPenalty_Table_data = new Dictionary<uint, WeightPenalty_Table>();
	}

	[MessagePackObject]
	public class Ability_Table
	{
		[Key(0)]
		public uint AbilityID;
		[Key(1)]
		public string StringName;
		[Key(2)]
		public E_ServerCheckType ServerCheckType;
		[Key(3)]
		public E_AttributeType AttributeType;
		[Key(4)]
		public E_ApplyType ApplyType;
		[Key(5)]
		public E_ConditionControl ConditionControl;
		[Key(6)]
		public E_ImmuneControl ImmuneControl;
		[Key(7)]
		public E_LocaleType LocaleType;
		[Key(8)]
		public float Unit;
		[Key(9)]
		public E_MarkType MarkType;
		[Key(10)]
		public E_PlusOutput PlusOutput;
		[Key(11)]
		public string AbilityIcon;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Ability_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Ability_Table> dicTables = new Dictionary<uint, Ability_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Ability_Table>(ref reader);

				dicTables.Add(table.AbilityID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class AbilityAction_Table
	{
		[Key(0)]
		public uint AbilityActionID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public byte BuffPriority;
		[Key(3)]
		public uint UIGroupID;
		[Key(4)]
		public E_UnitType UnitType;
		[Key(5)]
		public E_ApplicationType ApplicationType;
		[Key(6)]
		public E_AbilityActionType AbilityActionType;
		[Key(7)]
		public E_BuffType BuffType;
		[Key(8)]
		public E_InvokeTimingType InvokeTimingType;
		[Key(9)]
		public List<uint> UseSkillID = new List<uint>();
		[Key(10)]
		public uint LinkAbilityActionID;
		[Key(11)]
		public E_TargetType TargetType;
		[Key(12)]
		public float AbilityRate;
		[Key(13)]
		public float MinSupportTime;
		[Key(14)]
		public float MaxSupportTime;
		[Key(15)]
		public float PeriodTime;
		[Key(16)]
		public float AuraRange;
		[Key(17)]
		public uint MaxBuffStack;
		[Key(18)]
		public E_BuffSupportType BuffSupportType;
		[Key(19)]
		public E_BuffStackType BuffStackType;
		[Key(20)]
		public E_ReconnectionType ReconnectionType;
		[Key(21)]
		public E_DeathSupportType DeathSupportType;
		[Key(22)]
		public E_ChangeSupportType ChangeSupportType;
		[Key(23)]
		public E_MagicSignType MagicSignType;
		[Key(24)]
		public float CoolTime_Min;
		[Key(25)]
		public float CoolTime_Max;
		[Key(26)]
		public uint BuffNumber;
		[Key(27)]
		public string BuffIconString;
		[Key(28)]
		public string NameText;
		[Key(29)]
		public string ToolTip;
		[Key(30)]
		public uint EffectID;
		[Key(31)]
		public uint DotEffectID;
		[Key(32)]
		public E_HudBuffSignType HudBuffSignType;
		[Key(33)]
		public byte PartyBuffSignNo;
		[Key(34)]
		public E_AbilityViewType AbilityViewType;
		[Key(35)]
		public E_AbilityTargetType AbilityTargetType;
		[Key(36)]
		public uint TargetSkillID;
		[Key(37)]
		public uint ChangeArtifactCheck;
		[Key(38)]
		public E_AbilityType AbilityID_01;
		[Key(39)]
		public float AbilityPoint_01_Min;
		[Key(40)]
		public float AbilityPoint_01_Max;
		[Key(41)]
		public E_AbilityType AbilityID_02;
		[Key(42)]
		public float AbilityPoint_02;
		[Key(43)]
		public E_AbilityType AbilityID_03;
		[Key(44)]
		public float AbilityPoint_03;
		[Key(45)]
		public E_AbilityType AbilityID_04;
		[Key(46)]
		public float AbilityPoint_04;
		[Key(47)]
		public E_AbilityType AbilityID_05;
		[Key(48)]
		public float AbilityPoint_05;
		[Key(49)]
		public E_AbilityType AbilityID_06;
		[Key(50)]
		public float AbilityPoint_06;
		[Key(51)]
		public E_AbilityType AbilityID_07;
		[Key(52)]
		public float AbilityPoint_07;
		[Key(53)]
		public E_AbilityType AbilityID_08;
		[Key(54)]
		public float AbilityPoint_08;
		[Key(55)]
		public E_AbilityType AbilityID_09;
		[Key(56)]
		public float AbilityPoint_09;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, AbilityAction_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, AbilityAction_Table> dicTables = new Dictionary<uint, AbilityAction_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<AbilityAction_Table>(ref reader);

				dicTables.Add(table.AbilityActionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Animation_Table
	{
		[Key(0)]
		public string AnimationID;
		[Key(1)]
		public float AnimationLength;
		[Key(2)]
		public byte InvokeCount;
		[Key(3)]
		public float InvokeTiming_01;
		[Key(4)]
		public float InvokeTiming_02;
		[Key(5)]
		public float InvokeTiming_03;
		[Key(6)]
		public float InvokeTiming_04;
		[Key(7)]
		public float InvokeTiming_05;
		[Key(8)]
		public List<uint> EffectTiming = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<string, Animation_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<string, Animation_Table> dicTables = new Dictionary<string, Animation_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Animation_Table>(ref reader);

				dicTables.Add(table.AnimationID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Artifact_Table
	{
		[Key(0)]
		public uint ArtifactID;
		[Key(1)]
		public uint ArtifactGroupID;
		[Key(2)]
		public string ArtifactName;
		[Key(3)]
		public string ArtifactToolTip;
		[Key(4)]
		public string Icon;
		[Key(5)]
		public byte Step;
		[Key(6)]
		public byte Grade;
		[Key(7)]
		public E_PetType ArtifactType;
		[Key(8)]
		public uint SuccessRate;
		[Key(9)]
		public byte MaterialCount;
		[Key(10)]
		public E_ArtifactMaterialType MaterialType;
		[Key(11)]
		public byte Material_1_Grade;
		[Key(12)]
		public byte Material_1_Count;
		[Key(13)]
		public byte Material_2_Grade;
		[Key(14)]
		public byte Material_2_Count;
		[Key(15)]
		public byte Material_3_Grade;
		[Key(16)]
		public byte Material_3_Count;
		[Key(17)]
		public List<uint> CostItemID = new List<uint>();
		[Key(18)]
		public List<uint> CostItemCount = new List<uint>();
		[Key(19)]
		public uint ProtectItemID;
		[Key(20)]
		public uint ProtectItemCount;
		[Key(21)]
		public List<uint> AbilityActionID = new List<uint>();
		[Key(22)]
		public List<uint> CheckPetID = new List<uint>();
		[Key(23)]
		public List<uint> CheckPetAbilityActionID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Artifact_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Artifact_Table> dicTables = new Dictionary<uint, Artifact_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Artifact_Table>(ref reader);

				dicTables.Add(table.ArtifactID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ArtifactLink_Table
	{
		[Key(0)]
		public uint LinkID;
		[Key(1)]
		public uint LinkGroup;
		[Key(2)]
		public string LinkTitle;
		[Key(3)]
		public string LinkDes;
		[Key(4)]
		public string LinkImg_1;
		[Key(5)]
		public string LinkImg_2;
		[Key(6)]
		public byte LinkGrade;
		[Key(7)]
		public uint MaterialArtifactID_1;
		[Key(8)]
		public uint MaterialArtifactID_2;
		[Key(9)]
		public List<uint> AbilityActionID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ArtifactLink_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ArtifactLink_Table> dicTables = new Dictionary<uint, ArtifactLink_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ArtifactLink_Table>(ref reader);

				dicTables.Add(table.LinkID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class AttendEvent_Table
	{
		[Key(0)]
		public uint AttendEventID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public E_AttendBoardType AttendBoardType;
		[Key(3)]
		public E_ServerEventSubCategory AttendEventType;
		[Key(4)]
		public E_AttendEventOpenType AttendEventOpenType;
		[Key(5)]
		public uint AttendEventNumber;
		[Key(6)]
		public uint PurposeDay;
		[Key(7)]
		public uint RewardItemID;
		[Key(8)]
		public uint RewardItemCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, AttendEvent_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, AttendEvent_Table> dicTables = new Dictionary<uint, AttendEvent_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<AttendEvent_Table>(ref reader);

				dicTables.Add(table.AttendEventID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Attribute_Table
	{
		[Key(0)]
		public uint AttributeID;
		[Key(1)]
		public E_UnitAttributeType AttributeType;
		[Key(2)]
		public byte AttributeLevel;
		[Key(3)]
		public string AttributeTitle;
		[Key(4)]
		public E_LevelUpType LevelUpType;
		[Key(5)]
		public List<uint> LevelUpItem = new List<uint>();
		[Key(6)]
		public List<uint> UpItemCnt = new List<uint>();
		[Key(7)]
		public uint LevelUpRate;
		[Key(8)]
		public uint AdditionalRateUnit_1;
		[Key(9)]
		public uint AdditionalRateMaxRate_1;
		[Key(10)]
		public List<uint> AdditionalItem_1 = new List<uint>();
		[Key(11)]
		public List<uint> AdditionalItemCnt_1 = new List<uint>();
		[Key(12)]
		public uint AdditionalRateUnit_2;
		[Key(13)]
		public uint AdditionalRateMaxRate_2;
		[Key(14)]
		public List<uint> AdditionalItem_2 = new List<uint>();
		[Key(15)]
		public List<uint> AdditionalItemCnt_2 = new List<uint>();
		[Key(16)]
		public uint AdditionalRateUnit_3;
		[Key(17)]
		public uint AdditionalRateMaxRate_3;
		[Key(18)]
		public List<uint> AdditionalItem_3 = new List<uint>();
		[Key(19)]
		public List<uint> AdditionalItemCnt_3 = new List<uint>();
		[Key(20)]
		public uint AdditionalRateUnit_4;
		[Key(21)]
		public uint AdditionalRateMaxRate_4;
		[Key(22)]
		public List<uint> AdditionalItem_4 = new List<uint>();
		[Key(23)]
		public List<uint> AdditionalItemCnt_4 = new List<uint>();
		[Key(24)]
		public uint AdditionalRateUnit_5;
		[Key(25)]
		public uint AdditionalRateMaxRate_5;
		[Key(26)]
		public List<uint> AdditionalItem_5 = new List<uint>();
		[Key(27)]
		public List<uint> AdditionalItemCnt_5 = new List<uint>();
		[Key(28)]
		public uint AdditionalRateUnit_6;
		[Key(29)]
		public uint AdditionalRateMaxRate_6;
		[Key(30)]
		public List<uint> AdditionalItem_6 = new List<uint>();
		[Key(31)]
		public List<uint> AdditionalItemCnt_6 = new List<uint>();
		[Key(32)]
		public uint AdditionalRateUnit_7;
		[Key(33)]
		public uint AdditionalRateMaxRate_7;
		[Key(34)]
		public List<uint> AdditionalItem_7 = new List<uint>();
		[Key(35)]
		public List<uint> AdditionalItemCnt_7 = new List<uint>();
		[Key(36)]
		public uint AdditionalRateUnit_8;
		[Key(37)]
		public uint AdditionalRateMaxRate_8;
		[Key(38)]
		public List<uint> AdditionalItem_8 = new List<uint>();
		[Key(39)]
		public List<uint> AdditionalItemCnt_8 = new List<uint>();
		[Key(40)]
		public uint AdditionalRateUnit_9;
		[Key(41)]
		public uint AdditionalRateMaxRate_9;
		[Key(42)]
		public List<uint> AdditionalItem_9 = new List<uint>();
		[Key(43)]
		public List<uint> AdditionalItemCnt_9 = new List<uint>();
		[Key(44)]
		public uint AdditionalRateUnit_10;
		[Key(45)]
		public uint AdditionalRateMaxRate_10;
		[Key(46)]
		public List<uint> AdditionalItem_10 = new List<uint>();
		[Key(47)]
		public List<uint> AdditionalItemCnt_10 = new List<uint>();
		[Key(48)]
		public string AttributeIconID;
		[Key(49)]
		public string IconID;
		[Key(50)]
		public uint AbilityActionID_01;
		[Key(51)]
		public uint AbilityActionID_02;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Attribute_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Attribute_Table> dicTables = new Dictionary<uint, Attribute_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Attribute_Table>(ref reader);

				dicTables.Add(table.AttributeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class AttributeChain_Table
	{
		[Key(0)]
		public uint AttributeChainID;
		[Key(1)]
		public byte ChainLevel;
		[Key(2)]
		public string ChainTitleID;
		[Key(3)]
		public E_LevelUpType LevelUpType;
		[Key(4)]
		public byte FireLevelReq;
		[Key(5)]
		public byte WaterLevelReq;
		[Key(6)]
		public byte ElectricLevelReq;
		[Key(7)]
		public byte LightLevelReq;
		[Key(8)]
		public byte DarkLevelReq;
		[Key(9)]
		public string IconID;
		[Key(10)]
		public uint AbilityActionID_01;
		[Key(11)]
		public uint AbilityActionID_02;
		[Key(12)]
		public uint AbilityActionID_03;
		[Key(13)]
		public uint AbilityActionID_04;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, AttributeChain_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, AttributeChain_Table> dicTables = new Dictionary<uint, AttributeChain_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<AttributeChain_Table>(ref reader);

				dicTables.Add(table.AttributeChainID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class AttributeDamage_Table
	{
		[Key(0)]
		public uint AttributeDamageID;
		[Key(1)]
		public E_UnitAttributeType AttackAttribute;
		[Key(2)]
		public E_UnitAttributeType DefenseAttribute;
		[Key(3)]
		public int MinGapLevel;
		[Key(4)]
		public int MaxGapLevel;
		[Key(5)]
		public float DamageRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, AttributeDamage_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, AttributeDamage_Table> dicTables = new Dictionary<uint, AttributeDamage_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<AttributeDamage_Table>(ref reader);

				dicTables.Add(table.AttributeDamageID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class BossWar_Table
	{
		[Key(0)]
		public uint BossWarID;
		[Key(1)]
		public uint StageID;
		[Key(2)]
		public byte Grade;
		[Key(3)]
		public ulong MinDamage;
		[Key(4)]
		public ulong MaxDamage;
		[Key(5)]
		public uint NextID;
		[Key(6)]
		public List<uint> RewardID = new List<uint>();
		[Key(7)]
		public List<uint> RewardCount = new List<uint>();
		[Key(8)]
		public string IconID;
		[Key(9)]
		public string TooltipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, BossWar_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, BossWar_Table> dicTables = new Dictionary<uint, BossWar_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<BossWar_Table>(ref reader);

				dicTables.Add(table.BossWarID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Broadcast_Table
	{
		[Key(0)]
		public uint BroadcastID;
		[Key(1)]
		public E_BroadcastType BroadcastType;
		[Key(2)]
		public uint IDbyType;
		[Key(3)]
		public uint StackCheckCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Broadcast_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Broadcast_Table> dicTables = new Dictionary<uint, Broadcast_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Broadcast_Table>(ref reader);

				dicTables.Add(table.BroadcastID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Change_Table
	{
		[Key(0)]
		public uint ChangeID;
		[Key(1)]
		public E_ViewType ViewType;
		[Key(2)]
		public uint Sort;
		[Key(3)]
		public E_ExtractionType ExtractionType;
		[Key(4)]
		public List<uint> ExtractionMaterialItemID = new List<uint>();
		[Key(5)]
		public List<uint> ExtractionMaterialItemCnt = new List<uint>();
		[Key(6)]
		public uint ExtractionGetItemID;
		[Key(7)]
		public string ChangeTextID;
		[Key(8)]
		public byte Grade;
		[Key(9)]
		public E_CharacterType UseAttackType;
		[Key(10)]
		public E_UnitAttributeType AttributeType;
		[Key(11)]
		public E_UniqueType UniqueType;
		[Key(12)]
		public E_MoveType MoveType;
		[Key(13)]
		public uint UseItemCount;
		[Key(14)]
		public uint ChangeTime;
		[Key(15)]
		public E_AbilityRetain AbilityRetain;
		[Key(16)]
		public List<uint> AbilityActionIDs = new List<uint>();
		[Key(17)]
		public List<uint> EnchantAbilityActionID = new List<uint>();
		[Key(18)]
		public uint ResourceID;
		[Key(19)]
		public string Icon;
		[Key(20)]
		public uint Scale;
		[Key(21)]
		public uint ViewScale;
		[Key(22)]
		public float ViewScaleLocY;
		[Key(23)]
		public uint SeletScale;
		[Key(24)]
		public E_ChangeQuestType ChangeQuestType;
		[Key(25)]
		public string ClassIcon;
		[Key(26)]
		public string ToolTipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Change_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Change_Table> dicTables = new Dictionary<uint, Change_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Change_Table>(ref reader);

				dicTables.Add(table.ChangeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ChangeCollection_Table
	{
		[Key(0)]
		public uint ChangeCollectionID;
		[Key(1)]
		public E_ViewType ViewType;
		[Key(2)]
		public string ChangeCollectionTextID;
		[Key(3)]
		public E_TapType TapType;
		[Key(4)]
		public E_CollectionType CollectionType;
		[Key(5)]
		public uint Sort;
		[Key(6)]
		public byte CollectionChangeCount;
		[Key(7)]
		public uint CollectionChangeID_01;
		[Key(8)]
		public uint CollectionChangeID_02;
		[Key(9)]
		public uint CollectionChangeID_03;
		[Key(10)]
		public uint CollectionChangeID_04;
		[Key(11)]
		public uint CollectionChangeID_05;
		[Key(12)]
		public uint CollectionChangeID_06;
		[Key(13)]
		public uint CollectionChangeID_07;
		[Key(14)]
		public uint CollectionChangeID_08;
		[Key(15)]
		public uint AbilityActionID_01;
		[Key(16)]
		public uint AbilityActionID_02;
		[Key(17)]
		public string ToolTipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ChangeCollection_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ChangeCollection_Table> dicTables = new Dictionary<uint, ChangeCollection_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ChangeCollection_Table>(ref reader);

				dicTables.Add(table.ChangeCollectionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ChangeCompose_Table
	{
		[Key(0)]
		public uint ChangeComposeID;
		[Key(1)]
		public byte ChangeMaterialTier;
		[Key(2)]
		public byte ChangeMaterialCount;
		[Key(3)]
		public uint ChangeItemCount;
		[Key(4)]
		public uint HighTierRate;
		[Key(5)]
		public uint SameTierGroupID;
		[Key(6)]
		public uint HighTierGroupID;
		[Key(7)]
		public uint FailGetItemID;
		[Key(8)]
		public uint FailGetItemCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ChangeCompose_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ChangeCompose_Table> dicTables = new Dictionary<uint, ChangeCompose_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ChangeCompose_Table>(ref reader);

				dicTables.Add(table.ChangeComposeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ChangeList_Table
	{
		[Key(0)]
		public uint ChangeListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint GetChangeID;
		[Key(3)]
		public uint GetChangeRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ChangeList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ChangeList_Table> dicTables = new Dictionary<uint, ChangeList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ChangeList_Table>(ref reader);

				dicTables.Add(table.ChangeListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ChangeQuest_Table
	{
		[Key(0)]
		public uint ChangeQuestID;
		[Key(1)]
		public uint QuestGrade;
		[Key(2)]
		public string QuestTitle;
		[Key(3)]
		public List<uint> RewardItem = new List<uint>();
		[Key(4)]
		public List<uint> RewardItemCount = new List<uint>();
		[Key(5)]
		public byte QuestSlotCount;
		[Key(6)]
		public byte ChangeGrade1;
		[Key(7)]
		public E_ChangeQuestType ChangeType1;
		[Key(8)]
		public byte ChangeCount1;
		[Key(9)]
		public byte ChangeGrade2;
		[Key(10)]
		public E_ChangeQuestType ChangeType2;
		[Key(11)]
		public byte ChangeCount2;
		[Key(12)]
		public byte ChangeGrade3;
		[Key(13)]
		public E_ChangeQuestType ChangeType3;
		[Key(14)]
		public byte ChangeCount3;
		[Key(15)]
		public uint CostTime;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ChangeQuest_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ChangeQuest_Table> dicTables = new Dictionary<uint, ChangeQuest_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ChangeQuest_Table>(ref reader);

				dicTables.Add(table.ChangeQuestID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ChangeQuestLevel_Table
	{
		[Key(0)]
		public uint ChangeQuestLevelID;
		[Key(1)]
		public byte ChangeQuestLevel;
		[Key(2)]
		public E_LevelUpType LevelUpType;
		[Key(3)]
		public string LevelInfo;
		[Key(4)]
		public byte QuestCount;
		[Key(5)]
		public byte LevelUpGrade;
		[Key(6)]
		public uint LevelUpCount;
		[Key(7)]
		public uint GradeRate1;
		[Key(8)]
		public uint GradeRate2;
		[Key(9)]
		public uint GradeRate3;
		[Key(10)]
		public uint GradeRate4;
		[Key(11)]
		public uint GradeRate5;
		[Key(12)]
		public uint GradeRate6;
		[Key(13)]
		public uint MaxRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ChangeQuestLevel_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ChangeQuestLevel_Table> dicTables = new Dictionary<uint, ChangeQuestLevel_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ChangeQuestLevel_Table>(ref reader);

				dicTables.Add(table.ChangeQuestLevelID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Character_Table
	{
		[Key(0)]
		public uint CharacterID;
		[Key(1)]
		public string CharacterTextID;
		[Key(2)]
		public E_CharacterSelect CharacterSelect;
		[Key(3)]
		public E_CharacterType CharacterType;
		[Key(4)]
		public E_UnitAttributeType AttributeType;
		[Key(5)]
		public byte MaxLevel;
		[Key(6)]
		public uint Strength;
		[Key(7)]
		public uint Dexterity;
		[Key(8)]
		public uint Intellect;
		[Key(9)]
		public uint Wisdom;
		[Key(10)]
		public uint Vitality;
		[Key(11)]
		public uint MaxHP;
		[Key(12)]
		public uint MaxMP;
		[Key(13)]
		public uint ShortAttack;
		[Key(14)]
		public uint LongAttack;
		[Key(15)]
		public uint MagicAttack;
		[Key(16)]
		public uint ShortAccuracy;
		[Key(17)]
		public uint LongAccuracy;
		[Key(18)]
		public uint MagicAccuracy;
		[Key(19)]
		public float ShortCritical;
		[Key(20)]
		public float LongCritical;
		[Key(21)]
		public float MagicCritical;
		[Key(22)]
		public uint ShortCriticalDmg;
		[Key(23)]
		public uint LongCriticalDmg;
		[Key(24)]
		public uint MagicCriticalDmg;
		[Key(25)]
		public uint MeleeDefense;
		[Key(26)]
		public uint MagicDefense;
		[Key(27)]
		public float WalkSpeed;
		[Key(28)]
		public float RunSpeed;
		[Key(29)]
		public float AttackSpeed;
		[Key(30)]
		public uint Reduction;
		[Key(31)]
		public uint ReductionIgnore;
		[Key(32)]
		public uint ShortEvasion;
		[Key(33)]
		public uint LongEvasion;
		[Key(34)]
		public uint MagicEvasion;
		[Key(35)]
		public uint ShortEvasionIgnore;
		[Key(36)]
		public uint LongEvasionIgnore;
		[Key(37)]
		public uint MagicEvasionIgnore;
		[Key(38)]
		public uint PotionRecoveryPoint;
		[Key(39)]
		public float PotionRecoveryRate;
		[Key(40)]
		public uint MaxWeight;
		[Key(41)]
		public float HPRecovery;
		[Key(42)]
		public float MPRecovery;
		[Key(43)]
		public float HPRecoveryTime;
		[Key(44)]
		public float MPRecoveryTime;
		[Key(45)]
		public uint ResourceID;
		[Key(46)]
		public uint Scale;
		[Key(47)]
		public float CollisionRadius;
		[Key(48)]
		public string Icon;
		[Key(49)]
		public string AttackIcon;
		[Key(50)]
		public string ToolTipID;
		[Key(51)]
		public uint ScenarioWeapon;
		[Key(52)]
		public string CreationDirector;
		[Key(53)]
		public string CharacterIcon;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Character_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Character_Table> dicTables = new Dictionary<uint, Character_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Character_Table>(ref reader);

				dicTables.Add(table.CharacterID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ClassChange_Table
	{
		[Key(0)]
		public uint ClassChangeID;
		[Key(1)]
		public E_ChangeType ChangeType;
		[Key(2)]
		public E_CharacterType ChangeClassType;
		[Key(3)]
		public List<uint> NowCharacterID = new List<uint>();
		[Key(4)]
		public uint ChangeCharacterID;
		[Key(5)]
		public List<uint> NowSkillID = new List<uint>();
		[Key(6)]
		public uint ChangeSkillID;
		[Key(7)]
		public List<uint> NowItemID = new List<uint>();
		[Key(8)]
		public uint ChangeItemID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ClassChange_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ClassChange_Table> dicTables = new Dictionary<uint, ClassChange_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ClassChange_Table>(ref reader);

				dicTables.Add(table.ClassChangeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ColoSeasonReward_Table
	{
		[Key(0)]
		public uint SeasonID;
		[Key(1)]
		public ulong SeasonStart;
		[Key(2)]
		public ulong SeasonEnd;
		[Key(3)]
		public byte Grade;
		[Key(4)]
		public List<uint> SeasonEndRewardItem = new List<uint>();
		[Key(5)]
		public List<uint> SeasonEndRewardCnt = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ColoSeasonReward_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ColoSeasonReward_Table> dicTables = new Dictionary<uint, ColoSeasonReward_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ColoSeasonReward_Table>(ref reader);

				dicTables.Add(table.SeasonID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Colosseum_Table
	{
		[Key(0)]
		public byte Grade;
		[Key(1)]
		public string GradeIcon;
		[Key(2)]
		public string GradeEffect;
		[Key(3)]
		public string GradeName;
		[Key(4)]
		public uint ColosseumPoint;
		[Key(5)]
		public uint Rank;
		[Key(6)]
		public uint WinPoint;
		[Key(7)]
		public int LosePoint;
		[Key(8)]
		public List<uint> WinRewardItem = new List<uint>();
		[Key(9)]
		public List<uint> WinRewardCnt = new List<uint>();
		[Key(10)]
		public List<uint> LoseRewardItem = new List<uint>();
		[Key(11)]
		public List<uint> LoseRewardCnt = new List<uint>();
		[Key(12)]
		public uint MaxRewardCnt;
		[Key(13)]
		public List<uint> WinGuildItem = new List<uint>();
		[Key(14)]
		public List<uint> WinGuildCnt = new List<uint>();
		[Key(15)]
		public List<uint> LoseGuildItem = new List<uint>();
		[Key(16)]
		public List<uint> LoseGuildCnt = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<byte, Colosseum_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<byte, Colosseum_Table> dicTables = new Dictionary<byte, Colosseum_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Colosseum_Table>(ref reader);

				dicTables.Add(table.Grade, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Config_Table
	{
		[Key(0)]
		public string StringName;
		[Key(1)]
		public string Type;
		[Key(2)]
		public string Value;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<string, Config_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<string, Config_Table> dicTables = new Dictionary<string, Config_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Config_Table>(ref reader);

				dicTables.Add(table.StringName, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Cooking_Table
	{
		[Key(0)]
		public uint CookingID;
		[Key(1)]
		public E_UnusedType UnusedType;
		[Key(2)]
		public string CookingRecipeTitle;
		[Key(3)]
		public uint PositionNumber;
		[Key(4)]
		public byte CookingCharLevel;
		[Key(5)]
		public byte CookingTime;
		[Key(6)]
		public byte CookingMaterialCount;
		[Key(7)]
		public uint MaterialItemID_1;
		[Key(8)]
		public uint MaterialItemID_2;
		[Key(9)]
		public uint MaterialItemID_3;
		[Key(10)]
		public uint MaterialItemID_4;
		[Key(11)]
		public uint MaterialItemID_5;
		[Key(12)]
		public uint MaterialItemID_6;
		[Key(13)]
		public uint GoldCount;
		[Key(14)]
		public uint GreatSuccessRate;
		[Key(15)]
		public uint SuccessGetItemID;
		[Key(16)]
		public uint SuccessGetItemCount;
		[Key(17)]
		public uint GSuccessGetItemID;
		[Key(18)]
		public uint GSuccessGetItemCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Cooking_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Cooking_Table> dicTables = new Dictionary<uint, Cooking_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Cooking_Table>(ref reader);

				dicTables.Add(table.CookingID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class CouponGroup_Table
	{
		[Key(0)]
		public uint CouponID;
		[Key(1)]
		public uint CouponGroup;
		[Key(2)]
		public uint ItemID;
		[Key(3)]
		public uint ItemCnt;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, CouponGroup_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, CouponGroup_Table> dicTables = new Dictionary<uint, CouponGroup_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<CouponGroup_Table>(ref reader);

				dicTables.Add(table.CouponID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class DailyQuest_Table
	{
		[Key(0)]
		public uint DailyQuestID;
		[Key(1)]
		public uint OpenLevel;
		[Key(2)]
		public E_QuestGradeType QuestGradeType;
		[Key(3)]
		public uint QuestAcquireRate;
		[Key(4)]
		public uint GroupID;
		[Key(5)]
		public uint ExpCount;
		[Key(6)]
		public uint GoldCount;
		[Key(7)]
		public uint RewardID;
		[Key(8)]
		public uint RewardCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, DailyQuest_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, DailyQuest_Table> dicTables = new Dictionary<uint, DailyQuest_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<DailyQuest_Table>(ref reader);

				dicTables.Add(table.DailyQuestID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class DailyQuestList_Table
	{
		[Key(0)]
		public uint DailyQuestListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public string TextID;
		[Key(3)]
		public E_DailyQuestType DailyQuestType;
		[Key(4)]
		public uint QuestAcquireRate;
		[Key(5)]
		public uint MissionConditionID;
		[Key(6)]
		public uint MissionMinCount;
		[Key(7)]
		public uint MissionMaxCount;
		[Key(8)]
		public uint CountUnit;
		[Key(9)]
		public uint ShortCutID;
		[Key(10)]
		public E_UIShortCut UIShortCut;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, DailyQuestList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, DailyQuestList_Table> dicTables = new Dictionary<uint, DailyQuestList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<DailyQuestList_Table>(ref reader);

				dicTables.Add(table.DailyQuestListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class DeathPenalty_Table
	{
		[Key(0)]
		public uint DeathPenaltyID;
		[Key(1)]
		public int MinTendencyCount;
		[Key(2)]
		public int MaxTendencyCount;
		[Key(3)]
		public uint ExpRestoreCount;
		[Key(4)]
		public string TendencyTextID;
		[Key(5)]
		public string TendencyIcon;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, DeathPenalty_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, DeathPenalty_Table> dicTables = new Dictionary<uint, DeathPenalty_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<DeathPenalty_Table>(ref reader);

				dicTables.Add(table.DeathPenaltyID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Dialogue_Table
	{
		[Key(0)]
		public uint DialogueID;
		[Key(1)]
		public E_DialogueType DialogueType;
		[Key(2)]
		public string DialogueNPCName;
		[Key(3)]
		public string DialogueText;
		[Key(4)]
		public uint DialogueSelectGroup;
		[Key(5)]
		public uint SelectSequence;
		[Key(6)]
		public string DialogueSelectText;
		[Key(7)]
		public uint DialogueNextID;
		[Key(8)]
		public E_SelectRewardOrderType SelectRewardOrderType;
		[Key(9)]
		public uint SelectRewardOrder;
		[Key(10)]
		public E_DialogueAutoNextType DialogueAutoNextType;
		[Key(11)]
		public uint DialogueAutoNextTime;
		[Key(12)]
		public E_DialogueResourceType DialogueResourceType;
		[Key(13)]
		public string DialogueResource;
		[Key(14)]
		public E_DialogueSkipType DialogueSkipType;
		[Key(15)]
		public E_DialogueBGType DialogueBGType;
		[Key(16)]
		public string GuideImage;
		[Key(17)]
		public string GuideText;
		[Key(18)]
		public uint VoiceSoundID;
		[Key(19)]
		public E_GuideType GuideType;
		[Key(20)]
		public List<string> GuideParams = new List<string>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Dialogue_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Dialogue_Table> dicTables = new Dictionary<uint, Dialogue_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Dialogue_Table>(ref reader);

				dicTables.Add(table.DialogueID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class DropGroup_Table
	{
		[Key(0)]
		public uint DropID;
		[Key(1)]
		public uint DropGroupID;
		[Key(2)]
		public E_DropType DropType;
		[Key(3)]
		public E_RuneSetType RuneSetType;
		[Key(4)]
		public uint DropItemID;
		[Key(5)]
		public uint DropItemCnt;
		[Key(6)]
		public uint DropRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, DropGroup_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, DropGroup_Table> dicTables = new Dictionary<uint, DropGroup_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<DropGroup_Table>(ref reader);

				dicTables.Add(table.DropID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Effect_Table
	{
		[Key(0)]
		public uint EffectID;
		[Key(1)]
		public string EffectFile;
		[Key(2)]
		public uint EffectSize;
		[Key(3)]
		public E_EffectType EffectType;
		[Key(4)]
		public E_PlayType PlayType;
		[Key(5)]
		public E_ModelSocket ModelSocket;
		[Key(6)]
		public uint EffectSoundID;
		[Key(7)]
		public E_EffectOffsetType EffectOffsetType;
		[Key(8)]
		public float EffectDelayTime;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Effect_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Effect_Table> dicTables = new Dictionary<uint, Effect_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Effect_Table>(ref reader);

				dicTables.Add(table.EffectID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Emoticon_Table
	{
		[Key(0)]
		public uint EmoticonID;
		[Key(1)]
		public string EmoticonFile;
		[Key(2)]
		public string EmoticonTextID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Emoticon_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Emoticon_Table> dicTables = new Dictionary<uint, Emoticon_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Emoticon_Table>(ref reader);

				dicTables.Add(table.EmoticonID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class EventList_Table
	{
		[Key(0)]
		public uint EventID;
		[Key(1)]
		public uint EventGroupID;
		[Key(2)]
		public string EventKey;
		[Key(3)]
		public string EventIcon;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, EventList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, EventList_Table> dicTables = new Dictionary<uint, EventList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<EventList_Table>(ref reader);

				dicTables.Add(table.EventID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class EventReward_Table
	{
		[Key(0)]
		public uint RewardID;
		[Key(1)]
		public uint RewardGroupID;
		[Key(2)]
		public E_ServerEventSubCategory EventType;
		[Key(3)]
		public E_PayAttendEventStep Step;
		[Key(4)]
		public uint TypeCount;
		[Key(5)]
		public uint No_Pass_ItemID_1;
		[Key(6)]
		public uint No_Pass_ItemCount_1;
		[Key(7)]
		public uint No_Pass_ItemID_2;
		[Key(8)]
		public uint No_Pass_ItemCount_2;
		[Key(9)]
		public uint One_Pass_ItemID_1;
		[Key(10)]
		public uint One_Pass_ItemCount_1;
		[Key(11)]
		public uint One_Pass_ItemID_2;
		[Key(12)]
		public uint One_Pass_ItemCount_2;
		[Key(13)]
		public uint Two_Pass_ItemID_1;
		[Key(14)]
		public uint Two_Pass_ItemCount_1;
		[Key(15)]
		public uint Two_Pass_ItemID_2;
		[Key(16)]
		public uint Two_Pass_ItemCount_2;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, EventReward_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, EventReward_Table> dicTables = new Dictionary<uint, EventReward_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<EventReward_Table>(ref reader);

				dicTables.Add(table.RewardID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Gacha_Table
	{
		[Key(0)]
		public uint GachaListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint ListGroupID;
		[Key(3)]
		public E_RuneType RuneType;
		[Key(4)]
		public uint GetRate;
		[Key(5)]
		public byte UITipNo;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Gacha_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Gacha_Table> dicTables = new Dictionary<uint, Gacha_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Gacha_Table>(ref reader);

				dicTables.Add(table.GachaListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class GodBuff_Table
	{
		[Key(0)]
		public uint GodbuffID;
		[Key(1)]
		public E_GodBuffType GodBuffType;
		[Key(2)]
		public uint Stack;
		[Key(3)]
		public uint AbilityActionID_01;
		[Key(4)]
		public uint AbilityActionID_02;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, GodBuff_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, GodBuff_Table> dicTables = new Dictionary<uint, GodBuff_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<GodBuff_Table>(ref reader);

				dicTables.Add(table.GodbuffID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class GodLand_Table
	{
		[Key(0)]
		public uint GodLandID;
		[Key(1)]
		public uint SlotGroupID;
		[Key(2)]
		public uint LevelLimit;
		[Key(3)]
		public byte MapNumber;
		[Key(4)]
		public string MapFileName;
		[Key(5)]
		public string GodLandUpperTextID;
		[Key(6)]
		public string GodLandTextID;
		[Key(7)]
		public uint DefaultMonsterID;
		[Key(8)]
		public string Icon;
		[Key(9)]
		public uint ProductionItemID;
		[Key(10)]
		public uint ProductionItemCount;
		[Key(11)]
		public uint ProductionItemCountMax;
		[Key(12)]
		public uint ProductionTime;
		[Key(13)]
		public List<int> LocalMapPosition = new List<int>();
		[Key(14)]
		public List<int> WorldMapPosition = new List<int>();
		[Key(15)]
		public uint UseAbilityActionID_01;
		[Key(16)]
		public uint UseAbilityActionID_02;
		[Key(17)]
		public uint UseAbilityActionID_03;
		[Key(18)]
		public uint UseAbilityActionID_04;
		[Key(19)]
		public uint UseAbilityActionID_05;
		[Key(20)]
		public uint UseAbilityActionID_06;
		[Key(21)]
		public uint UseAbilityActionID_07;
		[Key(22)]
		public uint UseAbilityActionID_08;
		[Key(23)]
		public uint UseAbilityActionID_09;
		[Key(24)]
		public uint UseAbilityActionID_10;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, GodLand_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, GodLand_Table> dicTables = new Dictionary<uint, GodLand_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<GodLand_Table>(ref reader);

				dicTables.Add(table.GodLandID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Guild_Table
	{
		[Key(0)]
		public uint GuildID;
		[Key(1)]
		public uint GuildLevel;
		[Key(2)]
		public E_LevelUpType LevelUpType;
		[Key(3)]
		public ulong LevelUpExp;
		[Key(4)]
		public string ContentsTextID;
		[Key(5)]
		public string DungeonTextID;
		[Key(6)]
		public byte DungeonOpenCnt;
		[Key(7)]
		public string ShopTextID;
		[Key(8)]
		public string BuffTextID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Guild_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Guild_Table> dicTables = new Dictionary<uint, Guild_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Guild_Table>(ref reader);

				dicTables.Add(table.GuildID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class GuildBuff_Table
	{
		[Key(0)]
		public uint GuildBuffID;
		[Key(1)]
		public string GuildBuffTextID;
		[Key(2)]
		public E_GuildBuffType GuildBuffType;
		[Key(3)]
		public string IconID;
		[Key(4)]
		public uint OpenLevel;
		[Key(5)]
		public uint AbilityActionID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, GuildBuff_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, GuildBuff_Table> dicTables = new Dictionary<uint, GuildBuff_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<GuildBuff_Table>(ref reader);

				dicTables.Add(table.GuildBuffID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class GuildMark_Table
	{
		[Key(0)]
		public byte GuildMarkID;
		[Key(1)]
		public string GuildMarkFile;
		[Key(2)]
		public string GuildMarkSmallFile;
		[Key(3)]
		public uint GuildLevel;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<byte, GuildMark_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<byte, GuildMark_Table> dicTables = new Dictionary<byte, GuildMark_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<GuildMark_Table>(ref reader);

				dicTables.Add(table.GuildMarkID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class InfiBuff_Table
	{
		[Key(0)]
		public uint InfiBuffID;
		[Key(1)]
		public uint InfiBuffGroupID;
		[Key(2)]
		public uint InfiBuffRate;
		[Key(3)]
		public uint AbilityActionID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, InfiBuff_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, InfiBuff_Table> dicTables = new Dictionary<uint, InfiBuff_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<InfiBuff_Table>(ref reader);

				dicTables.Add(table.InfiBuffID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class InfinityDungeon_Table
	{
		[Key(0)]
		public uint DungeonID;
		[Key(1)]
		public uint StageID;
		[Key(2)]
		public uint InfinityDungeonGroupID;
		[Key(3)]
		public E_DungeonType DungeonType;
		[Key(4)]
		public uint StageLevel;
		[Key(5)]
		public string StageLevelName;
		[Key(6)]
		public string StageName;
		[Key(7)]
		public E_StageClearType StageClearType;
		[Key(8)]
		public List<string> NormalMonster = new List<string>();
		[Key(9)]
		public string UseRanker;
		[Key(10)]
		public string UseRankerName;
		[Key(11)]
		public uint ClearRewardExp;
		[Key(12)]
		public uint DayRewardExp;
		[Key(13)]
		public List<uint> ClearRewardItemID = new List<uint>();
		[Key(14)]
		public List<uint> ClearRewardItemIDCnt = new List<uint>();
		[Key(15)]
		public List<uint> DayRewardItemID = new List<uint>();
		[Key(16)]
		public List<uint> DayRewardItemIDCnt = new List<uint>();
		[Key(17)]
		public uint InfiBuffGroupID;
		[Key(18)]
		public uint InfiBuffCnt;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, InfinityDungeon_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, InfinityDungeon_Table> dicTables = new Dictionary<uint, InfinityDungeon_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<InfinityDungeon_Table>(ref reader);

				dicTables.Add(table.DungeonID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class InfinitySchedule_Table
	{
		[Key(0)]
		public uint InfinityScheduleID;
		[Key(1)]
		public uint InfinityDungeonGroupID;
		[Key(2)]
		public ulong Start;
		[Key(3)]
		public ulong End;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, InfinitySchedule_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, InfinitySchedule_Table> dicTables = new Dictionary<uint, InfinitySchedule_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<InfinitySchedule_Table>(ref reader);

				dicTables.Add(table.InfinityScheduleID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Infor_Table
	{
		[Key(0)]
		public uint InforID;
		[Key(1)]
		public string InforType;
		[Key(2)]
		public E_UnusedType UnusedType;
		[Key(3)]
		public string InforTextID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Infor_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Infor_Table> dicTables = new Dictionary<uint, Infor_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Infor_Table>(ref reader);

				dicTables.Add(table.InforID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Item_Table
	{
		[Key(0)]
		public uint ItemID;
		[Key(1)]
		public string ItemTextID;
		[Key(2)]
		public uint GroupID;
		[Key(3)]
		public uint ViewGroupID;
		[Key(4)]
		public E_ItemUseType ItemUseType;
		[Key(5)]
		public E_ItemType ItemType;
		[Key(6)]
		public E_ItemSubType ItemSubType;
		[Key(7)]
		public E_TradeTapType TradeTapType;
		[Key(8)]
		public E_TradeSubTapType TradeSubTapType;
		[Key(9)]
		public E_UniqueType UniqueType;
		[Key(10)]
		public E_ItemStackType ItemStackType;
		[Key(11)]
		public E_CharacterType UseCharacterType;
		[Key(12)]
		public byte Grade;
		[Key(13)]
		public byte Step;
		[Key(14)]
		public E_RuneGradeType RuneGradeType;
		[Key(15)]
		public byte LimitLevel;
		[Key(16)]
		public List<uint> SocketData = new List<uint>();
		[Key(17)]
		public E_SmeltScrollUseType SmeltScrollUseType;
		[Key(18)]
		public E_EnchantUseType EnchantUseType;
		[Key(19)]
		public uint ItemEnchantID;
		[Key(20)]
		public uint StepUpID;
		[Key(21)]
		public E_InvenUseType InvenUseType;
		[Key(22)]
		public E_QuickSlotType QuickSlotType;
		[Key(23)]
		public E_QuickSlotAutoType QuickSlotAutoType;
		[Key(24)]
		public float Weight;
		[Key(25)]
		public E_EquipSlotType EquipSlotType;
		[Key(26)]
		public string IconID;
		[Key(27)]
		public E_BelongType BelongType;
		[Key(28)]
		public E_LimitType LimitType;
		[Key(29)]
		public uint SellItemCount;
		[Key(30)]
		public uint BreakUseCount;
		[Key(31)]
		public List<uint> BreakItemID = new List<uint>();
		[Key(32)]
		public List<uint> BreakItemCount = new List<uint>();
		[Key(33)]
		public uint RuneliftItemCount;
		[Key(34)]
		public uint StorageItemCount;
		[Key(35)]
		public string TooltipID;
		[Key(36)]
		public byte AbilityTipOff;
		[Key(37)]
		public E_GachaType GachaType;
		[Key(38)]
		public List<uint> GachaGroupID = new List<uint>();
		[Key(39)]
		public List<uint> ShopListGroupID = new List<uint>();
		[Key(40)]
		public uint RestorationGroup;
		[Key(41)]
		public uint MovePortalID;
		[Key(42)]
		public uint OpenSkillID;
		[Key(43)]
		public float CoolTime;
		[Key(44)]
		public uint AbilityActionID_01;
		[Key(45)]
		public uint AbilityActionID_02;
		[Key(46)]
		public uint AbilityActionID_03;
		[Key(47)]
		public uint RuneBaseOptionID;
		[Key(48)]
		public uint RuneFirstOptionRate;
		[Key(49)]
		public uint RuneFirstOptionID;
		[Key(50)]
		public byte RuneSubOptionCount;
		[Key(51)]
		public uint RuneSubOptionID;
		[Key(52)]
		public E_RuneSetType RuneSetType;
		[Key(53)]
		public E_HaveSupportType HaveSupportType;
		[Key(54)]
		public uint HaveSupportTime;
		[Key(55)]
		public ulong HaveEndTime;
		[Key(56)]
		public E_DropModelType DropModelType;
		[Key(57)]
		public byte DropEffectGrade;
		[Key(58)]
		public uint SoundID;
		[Key(59)]
		public string DropTip;
		[Key(60)]
		public uint TempleStageID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Item_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Item_Table> dicTables = new Dictionary<uint, Item_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Item_Table>(ref reader);

				dicTables.Add(table.ItemID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ItemCollection_Table
	{
		[Key(0)]
		public uint ItemCollectionID;
		[Key(1)]
		public string ItemCollectionTextID;
		[Key(2)]
		public E_TapType TapType;
		[Key(3)]
		public E_CollectionType CollectionType;
		[Key(4)]
		public uint Sort;
		[Key(5)]
		public byte CollectionItemCount;
		[Key(6)]
		public uint CollectionItemID_01;
		[Key(7)]
		public uint CollectionItemID_02;
		[Key(8)]
		public uint CollectionItemID_03;
		[Key(9)]
		public uint CollectionItemID_04;
		[Key(10)]
		public uint CollectionItemID_05;
		[Key(11)]
		public uint CollectionItemID_06;
		[Key(12)]
		public uint CollectionItemID_07;
		[Key(13)]
		public uint CollectionItemID_08;
		[Key(14)]
		public uint AbilityActionID_01;
		[Key(15)]
		public uint AbilityActionID_02;
		[Key(16)]
		public string ToolTipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ItemCollection_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ItemCollection_Table> dicTables = new Dictionary<uint, ItemCollection_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ItemCollection_Table>(ref reader);

				dicTables.Add(table.ItemCollectionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ItemEnchant_Table
	{
		[Key(0)]
		public uint ItemEnchantID;
		[Key(1)]
		public E_EnchantType EnchantType;
		[Key(2)]
		public List<uint> NormalUseItemID = new List<uint>();
		[Key(3)]
		public uint NormalUseItemCount;
		[Key(4)]
		public uint NormalUseGoldCount;
		[Key(5)]
		public uint NormalEnchantRate;
		[Key(6)]
		public List<uint> BlessUseItemID = new List<uint>();
		[Key(7)]
		public uint BlessUseItemCount;
		[Key(8)]
		public uint BlessUseGoldCount;
		[Key(9)]
		public uint BlessNormalEnchantRate;
		[Key(10)]
		public uint BlessEnchantRate;
		[Key(11)]
		public List<uint> CurseUseItemID = new List<uint>();
		[Key(12)]
		public uint CurseUseItemCount;
		[Key(13)]
		public uint CurseUseGoldCount;
		[Key(14)]
		public uint CurseEnchantRate;
		[Key(15)]
		public List<uint> NonDestroyUseItemID = new List<uint>();
		[Key(16)]
		public uint NonDestroyUseItemCount;
		[Key(17)]
		public uint NonDestroyUseGoldCount;
		[Key(18)]
		public uint NonDestroyEnchantRate;
		[Key(19)]
		public List<uint> UpgradeUseItemID = new List<uint>();
		[Key(20)]
		public uint UpgradeUseItemCount;
		[Key(21)]
		public uint UpgradeUseGoldCount;
		[Key(22)]
		public uint UpgradeGroupID;
		[Key(23)]
		public E_DestroyType DestroyType;
		[Key(24)]
		public List<uint> FailItemID = new List<uint>();
		[Key(25)]
		public List<uint> FailItemCount = new List<uint>();
		[Key(26)]
		public uint MileageID;
		[Key(27)]
		public uint MileageCount;
		[Key(28)]
		public List<uint> MileageShopID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ItemEnchant_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ItemEnchant_Table> dicTables = new Dictionary<uint, ItemEnchant_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ItemEnchant_Table>(ref reader);

				dicTables.Add(table.ItemEnchantID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Level_Table
	{
		[Key(0)]
		public uint LevelID;
		[Key(1)]
		public E_CharacterType CharacterType;
		[Key(2)]
		public uint CharacterLevel;
		[Key(3)]
		public E_LevelUpType LevelUpType;
		[Key(4)]
		public ulong LevelUpExp;
		[Key(5)]
		public byte GetStatPoint;
		[Key(6)]
		public uint MaxHP;
		[Key(7)]
		public uint MaxMP;
		[Key(8)]
		public uint ShortAttack;
		[Key(9)]
		public uint LongAttack;
		[Key(10)]
		public uint MagicAttack;
		[Key(11)]
		public uint ShortAccuracy;
		[Key(12)]
		public uint LongAccuracy;
		[Key(13)]
		public uint MagicAccuracy;
		[Key(14)]
		public uint ShortCritical;
		[Key(15)]
		public uint LongCritical;
		[Key(16)]
		public uint MagicCritical;
		[Key(17)]
		public uint MeleeDefense;
		[Key(18)]
		public uint MagicDefense;
		[Key(19)]
		public uint ShortEvasion;
		[Key(20)]
		public uint LongEvasion;
		[Key(21)]
		public uint MagicEvasion;
		[Key(22)]
		public uint HPRecovery;
		[Key(23)]
		public uint MPRecovery;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Level_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Level_Table> dicTables = new Dictionary<uint, Level_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Level_Table>(ref reader);

				dicTables.Add(table.LevelID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class LineLog_Table
	{
		[Key(0)]
		public uint LogID;
		[Key(1)]
		public string CollectionName;
		[Key(2)]
		public string Key;
		[Key(3)]
		public string Type;
		[Key(4)]
		public uint MaxLength;
		[Key(5)]
		public string Value;
		[Key(6)]
		public string Comment;
		[Key(7)]
		public string Etc;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, LineLog_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, LineLog_Table> dicTables = new Dictionary<uint, LineLog_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<LineLog_Table>(ref reader);

				dicTables.Add(table.LogID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ListGroup_Table
	{
		[Key(0)]
		public uint ListID;
		[Key(1)]
		public uint ListGroupID;
		[Key(2)]
		public uint ItemID;
		[Key(3)]
		public uint ItemCount;
		[Key(4)]
		public uint ChangeID;
		[Key(5)]
		public uint PetID;
		[Key(6)]
		public uint RuneID;
		[Key(7)]
		public uint GetRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ListGroup_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ListGroup_Table> dicTables = new Dictionary<uint, ListGroup_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ListGroup_Table>(ref reader);

				dicTables.Add(table.ListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Loading_Table
	{
		[Key(0)]
		public uint LoadingID;
		[Key(1)]
		public E_LoadingType LoadingType;
		[Key(2)]
		public byte MinLevel;
		[Key(3)]
		public byte MaxLevel;
		[Key(4)]
		public string FileName;
		[Key(5)]
		public uint GroupID;
		[Key(6)]
		public uint EventOutputRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Loading_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Loading_Table> dicTables = new Dictionary<uint, Loading_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Loading_Table>(ref reader);

				dicTables.Add(table.LoadingID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class LoadingTip_Table
	{
		[Key(0)]
		public uint LoadingTipID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public string TipText;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, LoadingTip_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, LoadingTip_Table> dicTables = new Dictionary<uint, LoadingTip_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<LoadingTip_Table>(ref reader);

				dicTables.Add(table.LoadingTipID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Locale_Table
	{
		[Key(0)]
		public string StringName;
		[Key(1)]
		public E_TextType TextType;
		[Key(2)]
		public string Text;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<string, Locale_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<string, Locale_Table> dicTables = new Dictionary<string, Locale_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Locale_Table>(ref reader);

				dicTables.Add(table.StringName, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Mail_Table
	{
		[Key(0)]
		public uint MailID;
		[Key(1)]
		public E_MailType MailType;
		[Key(2)]
		public E_MailReceiver MailReceiver;
		[Key(3)]
		public string TitleTextID;
		[Key(4)]
		public uint KeepTime;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Mail_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Mail_Table> dicTables = new Dictionary<uint, Mail_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Mail_Table>(ref reader);

				dicTables.Add(table.MailID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Make_Table
	{
		[Key(0)]
		public uint MakeID;
		[Key(1)]
		public E_UnusedType UnusedType;
		[Key(2)]
		public E_MakeType MakeType;
		[Key(3)]
		public E_MakeTapType MakeTapType;
		[Key(4)]
		public uint PositionNumber;
		[Key(5)]
		public byte MakeCharLevel;
		[Key(6)]
		public byte MakeMaterialCount;
		[Key(7)]
		public E_MaterialType MaterialType;
		[Key(8)]
		public uint MaterialIncrement;
		[Key(9)]
		public List<uint> MaterialItemID_01 = new List<uint>();
		[Key(10)]
		public List<uint> MaterialItemCount_01 = new List<uint>();
		[Key(11)]
		public List<uint> MaterialItemID_02 = new List<uint>();
		[Key(12)]
		public List<uint> MaterialItemCount_02 = new List<uint>();
		[Key(13)]
		public List<uint> MaterialItemID_03 = new List<uint>();
		[Key(14)]
		public List<uint> MaterialItemCount_03 = new List<uint>();
		[Key(15)]
		public List<uint> MaterialItemID_04 = new List<uint>();
		[Key(16)]
		public List<uint> MaterialItemCount_04 = new List<uint>();
		[Key(17)]
		public List<uint> MaterialItemID_05 = new List<uint>();
		[Key(18)]
		public List<uint> MaterialItemCount_05 = new List<uint>();
		[Key(19)]
		public uint MakeSuccessRate;
		[Key(20)]
		public uint SuccessGetItemID;
		[Key(21)]
		public uint SuccessGetItemCount;
		[Key(22)]
		public List<uint> FailGetItemID = new List<uint>();
		[Key(23)]
		public List<uint> FailGetItemCount = new List<uint>();
		[Key(24)]
		public uint MileageShopID;
		[Key(25)]
		public E_ServerType ServerType;
		[Key(26)]
		public E_MakeLimitType MakeLimitType;
		[Key(27)]
		public uint MakeLimitCount;
		[Key(28)]
		public uint AccountLimitCount;
		[Key(29)]
		public uint ServerLimitCount;
		[Key(30)]
		public ulong EventOpenDay;
		[Key(31)]
		public ulong EventEndDay;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Make_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Make_Table> dicTables = new Dictionary<uint, Make_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Make_Table>(ref reader);

				dicTables.Add(table.MakeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Mark_Table
	{
		[Key(0)]
		public uint MarkID;
		[Key(1)]
		public string MarkTextID;
		[Key(2)]
		public E_MarkAbleType MarkAbleType;
		[Key(3)]
		public E_MarkUniqueType MarkUniqueType;
		[Key(4)]
		public E_DiceUseType DiceUseType;
		[Key(5)]
		public byte Step;
		[Key(6)]
		public byte MaxStep;
		[Key(7)]
		public E_MarkEnchantType MarkEnchantType;
		[Key(8)]
		public uint EssenceCount;
		[Key(9)]
		public uint GoldCount;
		[Key(10)]
		public uint UpRate;
		[Key(11)]
		public byte SuccesStep;
		[Key(12)]
		public byte FailStep;
		[Key(13)]
		public uint AbilityActionID_01;
		[Key(14)]
		public uint AbilityActionID_02;
		[Key(15)]
		public uint UniqAbilityOpenItemCount;
		[Key(16)]
		public uint UniqAbilityOpenRate;
		[Key(17)]
		public uint UniqAbilityActionID;
		[Key(18)]
		public string Icon;
		[Key(19)]
		public string ToolTipID;
		[Key(20)]
		public List<E_AbilityType> HighlightAbility = new List<E_AbilityType>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Mark_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Mark_Table> dicTables = new Dictionary<uint, Mark_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Mark_Table>(ref reader);

				dicTables.Add(table.MarkID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class MarkEnchant_Table
	{
		[Key(0)]
		public uint MarkEnchantID;
		[Key(1)]
		public string TextID;
		[Key(2)]
		public string IconID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, MarkEnchant_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, MarkEnchant_Table> dicTables = new Dictionary<uint, MarkEnchant_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<MarkEnchant_Table>(ref reader);

				dicTables.Add(table.MarkEnchantID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class MezRate_Table
	{
		[Key(0)]
		public uint MezRateID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public byte Section;
		[Key(3)]
		public byte Down50;
		[Key(4)]
		public byte Down49;
		[Key(5)]
		public byte Down48;
		[Key(6)]
		public byte Down47;
		[Key(7)]
		public byte Down46;
		[Key(8)]
		public byte Down45;
		[Key(9)]
		public byte Down44;
		[Key(10)]
		public byte Down43;
		[Key(11)]
		public byte Down42;
		[Key(12)]
		public byte Down41;
		[Key(13)]
		public byte Down40;
		[Key(14)]
		public byte Down39;
		[Key(15)]
		public byte Down38;
		[Key(16)]
		public byte Down37;
		[Key(17)]
		public byte Down36;
		[Key(18)]
		public byte Down35;
		[Key(19)]
		public byte Down34;
		[Key(20)]
		public byte Down33;
		[Key(21)]
		public byte Down32;
		[Key(22)]
		public byte Down31;
		[Key(23)]
		public byte Down30;
		[Key(24)]
		public byte Down29;
		[Key(25)]
		public byte Down28;
		[Key(26)]
		public byte Down27;
		[Key(27)]
		public byte Down26;
		[Key(28)]
		public byte Down25;
		[Key(29)]
		public byte Down24;
		[Key(30)]
		public byte Down23;
		[Key(31)]
		public byte Down22;
		[Key(32)]
		public byte Down21;
		[Key(33)]
		public byte Down20;
		[Key(34)]
		public byte Down19;
		[Key(35)]
		public byte Down18;
		[Key(36)]
		public byte Down17;
		[Key(37)]
		public byte Down16;
		[Key(38)]
		public byte Down15;
		[Key(39)]
		public byte Down14;
		[Key(40)]
		public byte Down13;
		[Key(41)]
		public byte Down12;
		[Key(42)]
		public byte Down11;
		[Key(43)]
		public byte Down10;
		[Key(44)]
		public byte Down9;
		[Key(45)]
		public byte Down8;
		[Key(46)]
		public byte Down7;
		[Key(47)]
		public byte Down6;
		[Key(48)]
		public byte Down5;
		[Key(49)]
		public byte Down4;
		[Key(50)]
		public byte Down3;
		[Key(51)]
		public byte Down2;
		[Key(52)]
		public byte Down1;
		[Key(53)]
		public byte Normal;
		[Key(54)]
		public byte up1;
		[Key(55)]
		public byte up2;
		[Key(56)]
		public byte up3;
		[Key(57)]
		public byte up4;
		[Key(58)]
		public byte up5;
		[Key(59)]
		public byte up6;
		[Key(60)]
		public byte up7;
		[Key(61)]
		public byte up8;
		[Key(62)]
		public byte up9;
		[Key(63)]
		public byte up10;
		[Key(64)]
		public byte up11;
		[Key(65)]
		public byte up12;
		[Key(66)]
		public byte up13;
		[Key(67)]
		public byte up14;
		[Key(68)]
		public byte up15;
		[Key(69)]
		public byte up16;
		[Key(70)]
		public byte up17;
		[Key(71)]
		public byte up18;
		[Key(72)]
		public byte up19;
		[Key(73)]
		public byte up20;
		[Key(74)]
		public byte up21;
		[Key(75)]
		public byte up22;
		[Key(76)]
		public byte up23;
		[Key(77)]
		public byte up24;
		[Key(78)]
		public byte up25;
		[Key(79)]
		public byte up26;
		[Key(80)]
		public byte up27;
		[Key(81)]
		public byte up28;
		[Key(82)]
		public byte up29;
		[Key(83)]
		public byte up30;
		[Key(84)]
		public byte up31;
		[Key(85)]
		public byte up32;
		[Key(86)]
		public byte up33;
		[Key(87)]
		public byte up34;
		[Key(88)]
		public byte up35;
		[Key(89)]
		public byte up36;
		[Key(90)]
		public byte up37;
		[Key(91)]
		public byte up38;
		[Key(92)]
		public byte up39;
		[Key(93)]
		public byte up40;
		[Key(94)]
		public byte up41;
		[Key(95)]
		public byte up42;
		[Key(96)]
		public byte up43;
		[Key(97)]
		public byte up44;
		[Key(98)]
		public byte up45;
		[Key(99)]
		public byte up46;
		[Key(100)]
		public byte up47;
		[Key(101)]
		public byte up48;
		[Key(102)]
		public byte up49;
		[Key(103)]
		public byte up50;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, MezRate_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, MezRate_Table> dicTables = new Dictionary<uint, MezRate_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<MezRate_Table>(ref reader);

				dicTables.Add(table.MezRateID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class MileageShop_Table
	{
		[Key(0)]
		public uint MileageShopID;
		[Key(1)]
		public E_UnusedType UnusedType;
		[Key(2)]
		public string ShopTextID;
		[Key(3)]
		public string IconID;
		[Key(4)]
		public byte Grade;
		[Key(5)]
		public E_MileageShopType MileageShopType;
		[Key(6)]
		public E_SubTapType SubTapType;
		[Key(7)]
		public uint PositionNumber;
		[Key(8)]
		public E_BuyBonusType BuyBonusType;
		[Key(9)]
		public uint ShopListID;
		[Key(10)]
		public E_GoodsKindType GoodsKindType;
		[Key(11)]
		public uint BuyItemID;
		[Key(12)]
		public uint BuyItemCount;
		[Key(13)]
		public byte BuyCharLevel;
		[Key(14)]
		public E_BuyLimitType BuyLimitType;
		[Key(15)]
		public uint BuyLimitCount;
		[Key(16)]
		public uint BuyLimitSlotCount;
		[Key(17)]
		public ulong BuyStartTime;
		[Key(18)]
		public ulong BuyFinishTime;
		[Key(19)]
		public string TooltipImageCode;
		[Key(20)]
		public string TooltipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, MileageShop_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, MileageShop_Table> dicTables = new Dictionary<uint, MileageShop_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<MileageShop_Table>(ref reader);

				dicTables.Add(table.MileageShopID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Monster_Table
	{
		[Key(0)]
		public uint MonsterID;
		[Key(1)]
		public string MonsterTextID;
		[Key(2)]
		public E_MonsterType MonsterType;
		[Key(3)]
		public E_UnitAttributeType AttributeType;
		[Key(4)]
		public int AttributeLevel;
		[Key(5)]
		public E_SkillArouseType SkillArouseType;
		[Key(6)]
		public uint PlaceStageID;
		[Key(7)]
		public uint Level;
		[Key(8)]
		public uint MaxHP;
		[Key(9)]
		public uint MaxMP;
		[Key(10)]
		public uint ShortAttack;
		[Key(11)]
		public uint LongAttack;
		[Key(12)]
		public uint WeaponAttack;
		[Key(13)]
		public uint MagicAttack;
		[Key(14)]
		public uint ShortAccuracy;
		[Key(15)]
		public uint LongAccuracy;
		[Key(16)]
		public uint MagicAccuracy;
		[Key(17)]
		public uint ShortCritical;
		[Key(18)]
		public uint LongCritical;
		[Key(19)]
		public uint MagicCritical;
		[Key(20)]
		public uint ShortCriticalMinus;
		[Key(21)]
		public uint LongCriticalMinus;
		[Key(22)]
		public uint MagicCriticalMinus;
		[Key(23)]
		public uint ShortCriticalDmg;
		[Key(24)]
		public uint LongCriticalDmg;
		[Key(25)]
		public uint MagicCriticalDmg;
		[Key(26)]
		public uint ShortCriticalDmgMinus;
		[Key(27)]
		public uint LongCriticalDmgMinus;
		[Key(28)]
		public uint MagicCriticalDmgMinus;
		[Key(29)]
		public uint MeleeDefense;
		[Key(30)]
		public uint MagicDefense;
		[Key(31)]
		public float WalkSpeed;
		[Key(32)]
		public float RunSpeed;
		[Key(33)]
		public float AttackSpeed;
		[Key(34)]
		public uint Reduction;
		[Key(35)]
		public uint ReductionIgnore;
		[Key(36)]
		public int ShortEvasion;
		[Key(37)]
		public int LongEvasion;
		[Key(38)]
		public int MagicEvasion;
		[Key(39)]
		public uint ShortEvasionIgnore;
		[Key(40)]
		public uint LongEvasionIgnore;
		[Key(41)]
		public uint MagicEvasionIgnore;
		[Key(42)]
		public uint HPRecovery;
		[Key(43)]
		public uint MPRecovery;
		[Key(44)]
		public float HPRecoveryTime;
		[Key(45)]
		public float MPRecoveryTime;
		[Key(46)]
		public uint GetCritical;
		[Key(47)]
		public int GetMezRate;
		[Key(48)]
		public byte BaseAttackCount;
		[Key(49)]
		public uint BaseAttackID_01;
		[Key(50)]
		public uint BaseAttackID_02;
		[Key(51)]
		public uint BaseAttackID_03;
		[Key(52)]
		public byte ActiveSkillCount;
		[Key(53)]
		public uint ActiveSkillID_01;
		[Key(54)]
		public uint ActiveSkillID_02;
		[Key(55)]
		public uint ActiveSkillID_03;
		[Key(56)]
		public uint ActiveSkillID_04;
		[Key(57)]
		public uint ActiveSkillID_05;
		[Key(58)]
		public uint ActiveSkillID_06;
		[Key(59)]
		public uint ActiveSkillID_07;
		[Key(60)]
		public uint ActiveSkillID_08;
		[Key(61)]
		public uint ActiveSkillID_09;
		[Key(62)]
		public uint PassiveSkillID;
		[Key(63)]
		public uint ResourceID;
		[Key(64)]
		public uint Scale;
		[Key(65)]
		public uint ViewScale;
		[Key(66)]
		public E_CollisionType CollisionType;
		[Key(67)]
		public float CollisionRadius;
		[Key(68)]
		public E_HitPossibleType HitPossibleType;
		[Key(69)]
		public E_RotationType RotationType;
		[Key(70)]
		public E_MoveType MoveType;
		[Key(71)]
		public E_BattleType BattleType;
		[Key(72)]
		public float SearchRange;
		[Key(73)]
		public float CorpseRetentionTime;
		[Key(74)]
		public float RoamingRange;
		[Key(75)]
		public byte MaxRoamingTime;
		[Key(76)]
		public E_SpawnType SpawnType;
		[Key(77)]
		public float SpawnTime;
		[Key(78)]
		public byte SpawnCnt;
		[Key(79)]
		public E_ReturnType ReturnType;
		[Key(80)]
		public float ReturnRange;
		[Key(81)]
		public float TendencyCount;
		[Key(82)]
		public uint DropGroupID;
		[Key(83)]
		public uint ResultDropGroupID;
		[Key(84)]
		public uint GuildDropItemID;
		[Key(85)]
		public uint Auction_DropGroupID;
		[Key(86)]
		public uint ExpCount;
		[Key(87)]
		public string Icon;
		[Key(88)]
		public string ToolTipID;
		[Key(89)]
		public uint ReplaceMonsterID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Monster_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Monster_Table> dicTables = new Dictionary<uint, Monster_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Monster_Table>(ref reader);

				dicTables.Add(table.MonsterID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class MonsterDrop_Table
	{
		[Key(0)]
		public uint MonsterDropID;
		[Key(1)]
		public uint DropGroupID;
		[Key(2)]
		public E_CharacterType DropCharacterType;
		[Key(3)]
		public E_DropConditionType DropConditionType;
		[Key(4)]
		public E_DropItemType DropItemType;
		[Key(5)]
		public uint DropItemID;
		[Key(6)]
		public uint DropItemGroupID;
		[Key(7)]
		public uint NormalDropRate;
		[Key(8)]
		public uint ProdDropRate;
		[Key(9)]
		public uint BlessDropRate;
		[Key(10)]
		public uint PowerDropRate;
		[Key(11)]
		public uint Ch1_NormalDropRate;
		[Key(12)]
		public uint Ch1_ProdDropRate;
		[Key(13)]
		public uint Ch1_BlessDropRate;
		[Key(14)]
		public uint Ch1_PowerDropRate;
		[Key(15)]
		public uint DropItemMinCount;
		[Key(16)]
		public uint DropItemMaxCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, MonsterDrop_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, MonsterDrop_Table> dicTables = new Dictionary<uint, MonsterDrop_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<MonsterDrop_Table>(ref reader);

				dicTables.Add(table.MonsterDropID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class NormalShop_Table
	{
		[Key(0)]
		public uint NormalShopID;
		[Key(1)]
		public E_UnusedType UnusedType;
		[Key(2)]
		public string ShopTextID;
		[Key(3)]
		public E_ShopType ShopType;
		[Key(4)]
		public E_NormalShopType NormalShopType;
		[Key(5)]
		public uint PositionNumber;
		[Key(6)]
		public byte ColosseumGrade;
		[Key(7)]
		public E_StateOutputType StateType;
		[Key(8)]
		public uint GoodsItemID;
		[Key(9)]
		public uint GoodsCount;
		[Key(10)]
		public E_PriceType PriceType;
		[Key(11)]
		public uint BuyItemID;
		[Key(12)]
		public uint BuyItemCount;
		[Key(13)]
		public byte BuyCharLevel;
		[Key(14)]
		public E_BuyLimitType BuyLimitType;
		[Key(15)]
		public uint BuyLimitCount;
		[Key(16)]
		public uint BuyLimitSlotCount;
		[Key(17)]
		public uint AutoBuyCount;
		[Key(18)]
		public ulong BuyStartTime;
		[Key(19)]
		public ulong BuyFinishTime;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, NormalShop_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, NormalShop_Table> dicTables = new Dictionary<uint, NormalShop_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<NormalShop_Table>(ref reader);

				dicTables.Add(table.NormalShopID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class NPC_Table
	{
		[Key(0)]
		public uint NPCID;
		[Key(1)]
		public string NPCTextID;
		[Key(2)]
		public E_NPCType NPCType;
		[Key(3)]
		public E_JobType JobType;
		[Key(4)]
		public float WalkSpeed;
		[Key(5)]
		public float RunSpeed;
		[Key(6)]
		public uint ResourceID;
		[Key(7)]
		public string QuestResource;
		[Key(8)]
		public uint Scale;
		[Key(9)]
		public E_CollisionType CollisionType;
		[Key(10)]
		public float CollisionRadius;
		[Key(11)]
		public float CollisionVoiceRadius;
		[Key(12)]
		public E_TargetUseType TargetUseType;
		[Key(13)]
		public E_RotationType RotationType;
		[Key(14)]
		public E_MoveType MoveType;
		[Key(15)]
		public float SearchRange;
		[Key(16)]
		public float TouchRange;
		[Key(17)]
		public float RoamingRange;
		[Key(18)]
		public E_NPCSpawnType NPCSpawnType;
		[Key(19)]
		public string Icon;
		[Key(20)]
		public string MinimapIcon;
		[Key(21)]
		public E_ToolTipType ToolTipType;
		[Key(22)]
		public string ToolTipID;
		[Key(23)]
		public List<uint> ActionStartSoundID = new List<uint>();
		[Key(24)]
		public List<uint> ActionEndSoundID = new List<uint>();
		[Key(25)]
		public List<uint> NearbySoundID = new List<uint>();
		[Key(26)]
		public List<uint> RecorverySoundID = new List<uint>();
		[Key(27)]
		public uint IdleEffectID;
		[Key(28)]
		public string MetaData;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, NPC_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, NPC_Table> dicTables = new Dictionary<uint, NPC_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<NPC_Table>(ref reader);

				dicTables.Add(table.NPCID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Object_Table
	{
		[Key(0)]
		public uint ObjectID;
		[Key(1)]
		public string ObjectTextID;
		[Key(2)]
		public E_ObjectType ObjectType;
		[Key(3)]
		public uint GreatSuccessRate;
		[Key(4)]
		public uint GreatSuccessItemID;
		[Key(5)]
		public uint GreatSuccessItemCntMin;
		[Key(6)]
		public uint GreatSuccessItemCntMax;
		[Key(7)]
		public uint ItemID;
		[Key(8)]
		public uint ItemCntMin;
		[Key(9)]
		public uint ItemCntMax;
		[Key(10)]
		public uint CastTime;
		[Key(11)]
		public uint StageID;
		[Key(12)]
		public uint MaxSpawnCount;
		[Key(13)]
		public uint RespawnTime;
		[Key(14)]
		public uint RespawnLimitCount;
		[Key(15)]
		public E_ObjectActionType ActionType;
		[Key(16)]
		public E_CollisionType CollisionType;
		[Key(17)]
		public float CollisionRadius;
		[Key(18)]
		public E_ObjectSpawnType ObjectSpawnType;
		[Key(19)]
		public string ResourceName;
		[Key(20)]
		public uint IdleEffect;
		[Key(21)]
		public uint SpawnEffect;
		[Key(22)]
		public uint ActionEffect;
		[Key(23)]
		public uint DisapearEffect;
		[Key(24)]
		public uint ActionStartSoundID;
		[Key(25)]
		public uint ActionContinueSoundID;
		[Key(26)]
		public uint ActionEndSoundID;
		[Key(27)]
		public uint Scale;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Object_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Object_Table> dicTables = new Dictionary<uint, Object_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Object_Table>(ref reader);

				dicTables.Add(table.ObjectID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Pet_Table
	{
		[Key(0)]
		public uint PetID;
		[Key(1)]
		public E_ViewType ViewType;
		[Key(2)]
		public uint Sort;
		[Key(3)]
		public E_ExtractionType ExtractionType;
		[Key(4)]
		public List<uint> ExtractionMaterialItemID = new List<uint>();
		[Key(5)]
		public List<uint> ExtractionMaterialItemCnt = new List<uint>();
		[Key(6)]
		public uint ExtractionGetItemID;
		[Key(7)]
		public string PetTextID;
		[Key(8)]
		public byte Grade;
		[Key(9)]
		public E_PetType PetType;
		[Key(10)]
		public uint CoolTime;
		[Key(11)]
		public uint PetExpGroup;
		[Key(12)]
		public uint PetGrowthGroup;
		[Key(13)]
		public E_UniqueType UniqueType;
		[Key(14)]
		public E_MoveType MoveType;
		[Key(15)]
		public uint MoveSpeed;
		[Key(16)]
		public uint UseItemCount;
		[Key(17)]
		public uint SupportTime;
		[Key(18)]
		public uint RideAbilityActionID;
		[Key(19)]
		public uint GrowthAbility;
		[Key(20)]
		public List<uint> AbilityActionID = new List<uint>();
		[Key(21)]
		public List<uint> EnchantAbilityActionID = new List<uint>();
		[Key(22)]
		public string ResourceFile;
		[Key(23)]
		public float RunSpeed;
		[Key(24)]
		public uint Scale;
		[Key(25)]
		public uint ViewScale;
		[Key(26)]
		public float ViewScaleLocY;
		[Key(27)]
		public string Icon;
		[Key(28)]
		public uint ActionEffectID;
		[Key(29)]
		public string ToolTipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Pet_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Pet_Table> dicTables = new Dictionary<uint, Pet_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Pet_Table>(ref reader);

				dicTables.Add(table.PetID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetAdventure_Table
	{
		[Key(0)]
		public uint AdventureID;
		[Key(1)]
		public uint AdventureGroupID;
		[Key(2)]
		public E_AdventureTab AdventureTab;
		[Key(3)]
		public string AdventureScene;
		[Key(4)]
		public string AdventureIcon;
		[Key(5)]
		public string AdventureNameText;
		[Key(6)]
		public uint ResetCoolTime;
		[Key(7)]
		public uint ResetItemID;
		[Key(8)]
		public uint ResetItemCnt;
		[Key(9)]
		public byte AdventureMinSlotCnt;
		[Key(10)]
		public byte AdventureMaxSlotCnt;
		[Key(11)]
		public uint NeedPetPower;
		[Key(12)]
		public uint OpenRate;
		[Key(13)]
		public uint Time;
		[Key(14)]
		public uint DefaultDropGroupID;
		[Key(15)]
		public uint AdvancedPetPower_1;
		[Key(16)]
		public uint AdvancedDropGroupID_1;
		[Key(17)]
		public uint AdvancedPetPower_2;
		[Key(18)]
		public uint AdvancedDropGroupID_2;
		[Key(19)]
		public uint AdvancedPetPower_3;
		[Key(20)]
		public uint AdvancedDropGroupID_3;
		[Key(21)]
		public uint BattleMonsterResourceID_1;
		[Key(22)]
		public uint MonsterScale_1;
		[Key(23)]
		public uint BattleMonsterResourceID_2;
		[Key(24)]
		public uint MonsterScale_2;
		[Key(25)]
		public uint BattleMonsterResourceID_3;
		[Key(26)]
		public uint MonsterScale_3;
		[Key(27)]
		public uint BattleMonsterResourceID_4;
		[Key(28)]
		public uint MonsterScale_4;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetAdventure_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetAdventure_Table> dicTables = new Dictionary<uint, PetAdventure_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetAdventure_Table>(ref reader);

				dicTables.Add(table.AdventureID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetCollection_Table
	{
		[Key(0)]
		public uint PetCollectionID;
		[Key(1)]
		public E_ViewType ViewType;
		[Key(2)]
		public E_PetType PetType;
		[Key(3)]
		public string PetCollectionTextID;
		[Key(4)]
		public E_TapType TapType;
		[Key(5)]
		public E_CollectionType CollectionType;
		[Key(6)]
		public uint Sort;
		[Key(7)]
		public byte CollectionPetCount;
		[Key(8)]
		public uint CollectionPetID_01;
		[Key(9)]
		public uint CollectionPetID_02;
		[Key(10)]
		public uint CollectionPetID_03;
		[Key(11)]
		public uint CollectionPetID_04;
		[Key(12)]
		public uint CollectionPetID_05;
		[Key(13)]
		public uint CollectionPetID_06;
		[Key(14)]
		public uint CollectionPetID_07;
		[Key(15)]
		public uint CollectionPetID_08;
		[Key(16)]
		public uint AbilityActionID_01;
		[Key(17)]
		public uint AbilityActionID_02;
		[Key(18)]
		public string ToolTipID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetCollection_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetCollection_Table> dicTables = new Dictionary<uint, PetCollection_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetCollection_Table>(ref reader);

				dicTables.Add(table.PetCollectionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetCompose_Table
	{
		[Key(0)]
		public uint PetComposeID;
		[Key(1)]
		public E_PetType PetType;
		[Key(2)]
		public byte PetMaterialTier;
		[Key(3)]
		public byte PetMaterialCount;
		[Key(4)]
		public uint PetItemCount;
		[Key(5)]
		public uint HighTierRate;
		[Key(6)]
		public uint SameTierGroupID;
		[Key(7)]
		public uint HighTierGroupID;
		[Key(8)]
		public uint FailGetItemID;
		[Key(9)]
		public uint FailGetItemCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetCompose_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetCompose_Table> dicTables = new Dictionary<uint, PetCompose_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetCompose_Table>(ref reader);

				dicTables.Add(table.PetComposeID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetGrowth_Table
	{
		[Key(0)]
		public uint PetGrowthID;
		[Key(1)]
		public uint PetGrowthGroup;
		[Key(2)]
		public uint PetLevel;
		[Key(3)]
		public uint PetGrowthItem_01;
		[Key(4)]
		public uint PetGrowthItemCnt_01;
		[Key(5)]
		public uint PetGrowthItem_02;
		[Key(6)]
		public uint PetGrowthItemCnt_02;
		[Key(7)]
		public uint PetGrowthSeccesRate;
		[Key(8)]
		public uint PetExpCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetGrowth_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetGrowth_Table> dicTables = new Dictionary<uint, PetGrowth_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetGrowth_Table>(ref reader);

				dicTables.Add(table.PetGrowthID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetLevel_Table
	{
		[Key(0)]
		public uint PetExpID;
		[Key(1)]
		public uint PetExpGroup;
		[Key(2)]
		public byte PetLevel;
		[Key(3)]
		public E_PetLevelUpType PetLevelUpType;
		[Key(4)]
		public uint PetExp;
		[Key(5)]
		public uint AdventureSeccesRate;
		[Key(6)]
		public E_RoundingType RoundingType;
		[Key(7)]
		public float AbilityUpPer;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetLevel_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetLevel_Table> dicTables = new Dictionary<uint, PetLevel_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetLevel_Table>(ref reader);

				dicTables.Add(table.PetExpID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PetList_Table
	{
		[Key(0)]
		public uint PetListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint GetPetID;
		[Key(3)]
		public uint GetPetRate;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PetList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PetList_Table> dicTables = new Dictionary<uint, PetList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PetList_Table>(ref reader);

				dicTables.Add(table.PetListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class PKBuff_Table
	{
		[Key(0)]
		public uint PKPenaltyID;
		[Key(1)]
		public int MinTendencyCount;
		[Key(2)]
		public int MaxTendencyCount;
		[Key(3)]
		public uint CharacterLevel;
		[Key(4)]
		public uint AbilityActionID_01;
		[Key(5)]
		public uint AbilityActionID_02;
		[Key(6)]
		public E_TendencyIconType TendencyIconType;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, PKBuff_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, PKBuff_Table> dicTables = new Dictionary<uint, PKBuff_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<PKBuff_Table>(ref reader);

				dicTables.Add(table.PKPenaltyID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Portal_Table
	{
		[Key(0)]
		public uint PortalID;
		[Key(1)]
		public uint StageID;
		[Key(2)]
		public E_PortalType PortalType;
		[Key(3)]
		public E_PKAreaChangeType PKAreaChangeType;
		[Key(4)]
		public byte MapNumber;
		[Key(5)]
		public string PortalMiniName;
		[Key(6)]
		public string ItemTextID;
		[Key(7)]
		public uint ChaosChannelUseItemID;
		[Key(8)]
		public uint ChaosChannelUseItemCnt;
		[Key(9)]
		public uint UseItemID;
		[Key(10)]
		public uint UseItemCount;
		[Key(11)]
		public string Position;
		[Key(12)]
		public float Radius;
		[Key(13)]
		public List<uint> LocalMapPosition = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Portal_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Portal_Table> dicTables = new Dictionary<uint, Portal_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Portal_Table>(ref reader);

				dicTables.Add(table.PortalID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Price_Table
	{
		[Key(0)]
		public uint PriceID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint MinBuyCount;
		[Key(3)]
		public uint MaxBuyCount;
		[Key(4)]
		public uint PriceCount;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Price_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Price_Table> dicTables = new Dictionary<uint, Price_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Price_Table>(ref reader);

				dicTables.Add(table.PriceID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Quest_Table
	{
		[Key(0)]
		public uint QuestID;
		[Key(1)]
		public string QuestTextID;
		[Key(2)]
		public E_QuestType QuestType;
		[Key(3)]
		public uint UIQuestConfirmTarget;
		[Key(4)]
		public E_UIQuestType UIQuestType;
		[Key(5)]
		public uint QuestSequence;
		[Key(6)]
		public uint QuestGroupID;
		[Key(7)]
		public E_QuestGroupRewardOn QuestGroupRewardOn;
		[Key(8)]
		public uint QuestGroupOrder;
		[Key(9)]
		public byte ConditionLevel;
		[Key(10)]
		public List<uint> ConditionQuestID1 = new List<uint>();
		[Key(11)]
		public List<uint> ConditionQuestID2 = new List<uint>();
		[Key(12)]
		public List<uint> ConditionQuestID3 = new List<uint>();
		[Key(13)]
		public uint NextQuestID;
		[Key(14)]
		public E_CharacterType QuestCharacterType;
		[Key(15)]
		public E_OpenDay OpenDay;
		[Key(16)]
		public E_QuestOpenType QuestOpenType;
		[Key(17)]
		public uint QuestOpenNPCID;
		[Key(18)]
		public uint QuestOpenNPCMap;
		[Key(19)]
		public uint QuestOpenItemID;
		[Key(20)]
		public E_TaskQuestType TaskQuestType;
		[Key(21)]
		public E_CompleteCheck CompleteCheck;
		[Key(22)]
		public uint TermsMonsterID;
		[Key(23)]
		public uint ItemGetMonsterID;
		[Key(24)]
		public uint ItemGetRate;
		[Key(25)]
		public E_DeliveryItemType DeliveryItemType;
		[Key(26)]
		public uint DeliveryItemID;
		[Key(27)]
		public byte TargetLevel;
		[Key(28)]
		public uint TargetTempleID;
		[Key(29)]
		public uint ItemGetObjectID;
		[Key(30)]
		public uint TalkNPCID;
		[Key(31)]
		public uint TalkNPCMap;
		[Key(32)]
		public uint MapMoveID;
		[Key(33)]
		public List<uint> MapPos = new List<uint>();
		[Key(34)]
		public uint MapPosRange;
		[Key(35)]
		public ulong TargetCount;
		[Key(36)]
		public uint DialogueBeforeStartID;
		[Key(37)]
		public uint DialogueAfterStartID;
		[Key(38)]
		public uint DialogueTalkID;
		[Key(39)]
		public uint DialogueGuideID;
		[Key(40)]
		public uint DialogueEndID;
		[Key(41)]
		public uint UIQuestNext;
		[Key(42)]
		public uint UIChapter;
		[Key(43)]
		public E_UIChapterType UIChapterType;
		[Key(44)]
		public string ChapterName;
		[Key(45)]
		public string ChapterIcon;
		[Key(46)]
		public uint QuestWarpPortalID;
		[Key(47)]
		public E_AutoProgressType AutoProgressType;
		[Key(48)]
		public E_AutoNoneInfoType AutoNoneInfoType;
		[Key(49)]
		public string InfoMsg;
		[Key(50)]
		public E_AutoCompleteType AutoCompleteType;
		[Key(51)]
		public E_UICompleteHideType UICompleteHideType;
		[Key(52)]
		public uint QuestCompleteHideNpcID;
		[Key(53)]
		public uint QuestCompleteHideStageID;
		[Key(54)]
		public string QuestTermsText;
		[Key(55)]
		public string QuestSimpleText;
		[Key(56)]
		public E_VideoType ChapterStartVideoType;
		[Key(57)]
		public string ChapterStartVideo;
		[Key(58)]
		public E_VideoType ChapterEndVideoType;
		[Key(59)]
		public string ChapterEndVideo;
		[Key(60)]
		public E_VideoType DialogueStartVideoType;
		[Key(61)]
		public string DialogueStartVideo;
		[Key(62)]
		public E_VideoType QuestCompletableVideoType;
		[Key(63)]
		public string QuestCompletableVideo;
		[Key(64)]
		public E_VideoType DialogueEndVideoType;
		[Key(65)]
		public string DialogueEndVideo;
		[Key(66)]
		public E_RewardExpType RewardExpType;
		[Key(67)]
		public uint RewardExp;
		[Key(68)]
		public uint RewardGoldCount;
		[Key(69)]
		public List<uint> RewardItemID = new List<uint>();
		[Key(70)]
		public List<uint> RewardItemCount = new List<uint>();
		[Key(71)]
		public uint RewardChangeID;
		[Key(72)]
		public uint RewardPetID;
		[Key(73)]
		public uint RewardAbilityID;
		[Key(74)]
		public uint RewardAbilityCount;
		[Key(75)]
		public uint TempleStageID;
		[Key(76)]
		public uint RandomRewardDropGroup;
		[Key(77)]
		public List<uint> SelectRewardItemID = new List<uint>();
		[Key(78)]
		public List<uint> SelectRewardItemCount = new List<uint>();
		[Key(79)]
		public E_UIShortCut UIShortCut;
		[Key(80)]
		public List<uint> RewardCharacterID = new List<uint>();
		[Key(81)]
		public List<uint> RewardCharacterItemID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Quest_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Quest_Table> dicTables = new Dictionary<uint, Quest_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Quest_Table>(ref reader);

				dicTables.Add(table.QuestID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class QuestEvent_Table
	{
		[Key(0)]
		public uint EventQuestID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public string EventQuestTextID;
		[Key(3)]
		public E_EventOpenDay EventOpenDay;
		[Key(4)]
		public E_EventCompleteCheck CompleteCheck;
		[Key(5)]
		public uint TermsMonsterID;
		[Key(6)]
		public byte TargetLevel;
		[Key(7)]
		public uint TargetTempleID;
		[Key(8)]
		public uint ItemGetObjectID;
		[Key(9)]
		public uint MapMoveID;
		[Key(10)]
		public uint TargetItemID;
		[Key(11)]
		public E_MakeType TargetMakeType;
		[Key(12)]
		public ulong TargetCount;
		[Key(13)]
		public uint RewardGoldCount;
		[Key(14)]
		public List<uint> RewardItemID = new List<uint>();
		[Key(15)]
		public List<uint> RewardItemCount = new List<uint>();
		[Key(16)]
		public List<uint> QuestMissionRewardCount = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, QuestEvent_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, QuestEvent_Table> dicTables = new Dictionary<uint, QuestEvent_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<QuestEvent_Table>(ref reader);

				dicTables.Add(table.EventQuestID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class RankBuff_Table
	{
		[Key(0)]
		public uint RankBuffID;
		[Key(1)]
		public E_RankBuffType RankBuffType;
		[Key(2)]
		public E_CharacterType CharacterType;
		[Key(3)]
		public uint MinRanking;
		[Key(4)]
		public uint MaxRanking;
		[Key(5)]
		public uint AbilityActionID_01;
		[Key(6)]
		public uint AbilityActionID_02;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, RankBuff_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, RankBuff_Table> dicTables = new Dictionary<uint, RankBuff_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<RankBuff_Table>(ref reader);

				dicTables.Add(table.RankBuffID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Resource_Table
	{
		[Key(0)]
		public uint ResourceID;
		[Key(1)]
		public string ResourceFile;
		[Key(2)]
		public string LobbyFile;
		[Key(3)]
		public float WalkSpeed;
		[Key(4)]
		public float RunSpeed;
		[Key(5)]
		public float Diameter;
		[Key(6)]
		public float SizeX;
		[Key(7)]
		public float SizeY;
		[Key(8)]
		public float SizeZ;
		[Key(9)]
		public uint DieEffectID;
		[Key(10)]
		public string AttackAni_01;
		[Key(11)]
		public uint AttackEffectID_01;
		[Key(12)]
		public string AttackAni_02;
		[Key(13)]
		public uint AttackEffectID_02;
		[Key(14)]
		public string AttackAni_03;
		[Key(15)]
		public uint AttackEffectID_03;
		[Key(16)]
		public uint CriticalEffectID;
		[Key(17)]
		public string BuffAni_01;
		[Key(18)]
		public uint BuffEffectID_01;
		[Key(19)]
		public string SkillAni_01;
		[Key(20)]
		public uint SkillEffectID_01;
		[Key(21)]
		public string SkillAni_02;
		[Key(22)]
		public uint SkillEffectID_02;
		[Key(23)]
		public string CastingAni;
		[Key(24)]
		public uint Casting_S_EffectID;
		[Key(25)]
		public uint Casting_I_EffectID;
		[Key(26)]
		public uint Casting_A_EffectID;
		[Key(27)]
		public string RushAni;
		[Key(28)]
		public uint Rush_S_EffectID;
		[Key(29)]
		public uint Rush_I_EffectID;
		[Key(30)]
		public uint Rush_A_EffectID;
		[Key(31)]
		public string LeapAni;
		[Key(32)]
		public uint Leap_S_EffectID;
		[Key(33)]
		public uint Leap_I_EffectID;
		[Key(34)]
		public uint Leap_A_EffectID;
		[Key(35)]
		public string PullAni;
		[Key(36)]
		public uint PullEffectID;
		[Key(37)]
		public uint MissileEffect_Base;
		[Key(38)]
		public uint MissileEffect_01;
		[Key(39)]
		public uint MissileEffect_02;
		[Key(40)]
		public uint MissileEffect_03;
		[Key(41)]
		public uint MissileEffect_04;
		[Key(42)]
		public uint HitEffect;
		[Key(43)]
		public uint LobbyEffectTID;
		[Key(44)]
		public uint AttackSoundID;
		[Key(45)]
		public uint CriticalSoundID;
		[Key(46)]
		public uint SkillSoundID;
		[Key(47)]
		public uint BuffSoundID;
		[Key(48)]
		public uint HitSoundID;
		[Key(49)]
		public uint DieSoundID;
		[Key(50)]
		public uint Casting_S_SoundID;
		[Key(51)]
		public uint Casting_I_SoundID;
		[Key(52)]
		public uint Casting_A_SoundID;
		[Key(53)]
		public uint Rush_S_SoundID;
		[Key(54)]
		public uint Rush_I_SoundID;
		[Key(55)]
		public uint Rush_A_SoundID;
		[Key(56)]
		public uint Leap_S_SoundID;
		[Key(57)]
		public uint Leap_I_SoundID;
		[Key(58)]
		public uint Leap_A_SoundID;
		[Key(59)]
		public uint PullSoundID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Resource_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Resource_Table> dicTables = new Dictionary<uint, Resource_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Resource_Table>(ref reader);

				dicTables.Add(table.ResourceID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Restoration_Table
	{
		[Key(0)]
		public uint RestorationID;
		[Key(1)]
		public uint RestorationGroup;
		[Key(2)]
		public uint RestorationCheck;
		[Key(3)]
		public E_CategoryType CategoryType;
		[Key(4)]
		public uint HighTierRate;
		[Key(5)]
		public uint HighTierItemReward;
		[Key(6)]
		public uint ChangeHighTierGroupID;
		[Key(7)]
		public uint ChangeSameTierGroupID;
		[Key(8)]
		public uint PetHighTierGroupID;
		[Key(9)]
		public uint PetSameTierGroupID;
		[Key(10)]
		public uint FailReward;
		[Key(11)]
		public uint FailRewardCnt;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Restoration_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Restoration_Table> dicTables = new Dictionary<uint, Restoration_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Restoration_Table>(ref reader);

				dicTables.Add(table.RestorationID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class RuneComponent_Table
	{
		[Key(0)]
		public uint RuneComponentID;
		[Key(1)]
		public uint ItemID;
		[Key(2)]
		public uint MainOptionID;
		[Key(3)]
		public uint FirstOptionID;
		[Key(4)]
		public List<uint> SubOptionID_01 = new List<uint>();
		[Key(5)]
		public List<uint> SubOptionID_02 = new List<uint>();
		[Key(6)]
		public List<uint> SubOptionID_03 = new List<uint>();
		[Key(7)]
		public List<uint> SubOptionID_04 = new List<uint>();
		[Key(8)]
		public byte RuneEnchant;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, RuneComponent_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, RuneComponent_Table> dicTables = new Dictionary<uint, RuneComponent_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<RuneComponent_Table>(ref reader);

				dicTables.Add(table.RuneComponentID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class RuneEnchant_Table
	{
		[Key(0)]
		public uint RuneEnchantID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public byte EnchantStep;
		[Key(3)]
		public uint EnchantRate;
		[Key(4)]
		public uint EnchantItemCount;
		[Key(5)]
		public uint AbilityActionID;
		[Key(6)]
		public E_GetSupOptionType GetSupOptionType;
		[Key(7)]
		public byte SupOptionCount;
		[Key(8)]
		public E_RuneGradeType CheckGradeType;
		[Key(9)]
		public E_RuneGradeType UpGradeType;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, RuneEnchant_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, RuneEnchant_Table> dicTables = new Dictionary<uint, RuneEnchant_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<RuneEnchant_Table>(ref reader);

				dicTables.Add(table.RuneEnchantID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class RuneOption_Table
	{
		[Key(0)]
		public uint RuneOptionID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public E_RuneOptionType RuneOptionType;
		[Key(3)]
		public uint GetRate;
		[Key(4)]
		public E_AbilityType RuneAbilityType;
		[Key(5)]
		public uint AbilityActionID;
		[Key(6)]
		public uint RuneEnchantID;
		[Key(7)]
		public string TextID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, RuneOption_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, RuneOption_Table> dicTables = new Dictionary<uint, RuneOption_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<RuneOption_Table>(ref reader);

				dicTables.Add(table.RuneOptionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class RuneSet_Table
	{
		[Key(0)]
		public uint RuneSetID;
		[Key(1)]
		public E_RuneSetType RuneSetType;
		[Key(2)]
		public byte SetCompleteCount;
		[Key(3)]
		public uint AbilityActionID;
		[Key(4)]
		public uint RuneItemId;
		[Key(5)]
		public string SetTextID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, RuneSet_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, RuneSet_Table> dicTables = new Dictionary<uint, RuneSet_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<RuneSet_Table>(ref reader);

				dicTables.Add(table.RuneSetID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ScenarioAbility_Table
	{
		[Key(0)]
		public uint ScenarioAbilityID;
		[Key(1)]
		public uint AbilityActionID;
		[Key(2)]
		public uint QuickAbilityTime;
		[Key(3)]
		public uint QuickAbilityCount;
		[Key(4)]
		public string QuickAbilityIcon;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ScenarioAbility_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ScenarioAbility_Table> dicTables = new Dictionary<uint, ScenarioAbility_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ScenarioAbility_Table>(ref reader);

				dicTables.Add(table.ScenarioAbilityID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ScenarioDirection_Table
	{
		[Key(0)]
		public uint ScenarioDirectionID;
		[Key(1)]
		public uint DirectionGroup;
		[Key(2)]
		public byte Step;
		[Key(3)]
		public E_ScenarioDirectionType ScenarioDirectionType;
		[Key(4)]
		public string DialogueNPCName;
		[Key(5)]
		public string DialogueText;
		[Key(6)]
		public E_ScenarioDialogueType ScenarioDialogueType;
		[Key(7)]
		public string DialogueResource;
		[Key(8)]
		public List<string> DialogueAddResources = new List<string>();
		[Key(9)]
		public uint DialogueAddSound;
		[Key(10)]
		public string DialogueBG;
		[Key(11)]
		public List<string> DialogueParams = new List<string>();
		[Key(12)]
		public string GuideImage;
		[Key(13)]
		public string GuideText;
		[Key(14)]
		public string GuideFailText;
		[Key(15)]
		public E_ScenarioGuideType ScenarioGuideType;
		[Key(16)]
		public uint GuideDuration;
		[Key(17)]
		public List<uint> SoundID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ScenarioDirection_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ScenarioDirection_Table> dicTables = new Dictionary<uint, ScenarioDirection_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ScenarioDirection_Table>(ref reader);

				dicTables.Add(table.ScenarioDirectionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ScenarioMission_Table
	{
		[Key(0)]
		public uint ScenarioMissionID;
		[Key(1)]
		public uint StageID;
		[Key(2)]
		public uint PortalID;
		[Key(3)]
		public string Prefab;
		[Key(4)]
		public uint CharacterID;
		[Key(5)]
		public E_SpecialButtonType SpecialButtonType;
		[Key(6)]
		public uint SpecialButtonTime;
		[Key(7)]
		public string SpecialButtonIcon;
		[Key(8)]
		public string SpecialButtonAni;
		[Key(9)]
		public uint SpecialButtonEffect;
		[Key(10)]
		public byte MissionNumber;
		[Key(11)]
		public string MissionTitle;
		[Key(12)]
		public string MissionDesc;
		[Key(13)]
		public string MissionStory;
		[Key(14)]
		public string MissionButtonText;
		[Key(15)]
		public uint StartDialogueID;
		[Key(16)]
		public uint EndDialogueID;
		[Key(17)]
		public uint FailDialogueID;
		[Key(18)]
		public uint PotionItem;
		[Key(19)]
		public uint PotionCount;
		[Key(20)]
		public uint PotionPer;
		[Key(21)]
		public uint QuickSkill1;
		[Key(22)]
		public uint QuickSkill2;
		[Key(23)]
		public uint QuickSkill3;
		[Key(24)]
		public uint QuickSkill4;
		[Key(25)]
		public uint QuickSkill5;
		[Key(26)]
		public uint QuickSkill6;
		[Key(27)]
		public uint QuickSkill7;
		[Key(28)]
		public uint QuickSkill8;
		[Key(29)]
		public List<uint> RewardItem = new List<uint>();
		[Key(30)]
		public List<uint> RewardItemCount = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ScenarioMission_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ScenarioMission_Table> dicTables = new Dictionary<uint, ScenarioMission_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ScenarioMission_Table>(ref reader);

				dicTables.Add(table.ScenarioMissionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class ShopList_Table
	{
		[Key(0)]
		public uint ShopListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint ListGroupID;
		[Key(3)]
		public uint GetRate;
		[Key(4)]
		public uint MileageItemID;
		[Key(5)]
		public uint MileageCount;
		[Key(6)]
		public string SpecialShopTextID;
		[Key(7)]
		public byte UITipNo;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, ShopList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, ShopList_Table> dicTables = new Dictionary<uint, ShopList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<ShopList_Table>(ref reader);

				dicTables.Add(table.ShopListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Skill_Table
	{
		[Key(0)]
		public uint SkillID;
		[Key(1)]
		public string SkillTextID;
		[Key(2)]
		public uint GroupID;
		[Key(3)]
		public E_UnitType UnitType;
		[Key(4)]
		public E_CharacterType CharacterType;
		[Key(5)]
		public E_WeaponType WeaponType;
		[Key(6)]
		public byte SkillSort;
		[Key(7)]
		public byte OpenLevel;
		[Key(8)]
		public uint OpenItemID;
		[Key(9)]
		public byte OpenGrade;
		[Key(10)]
		public byte OpenGradeCount;
		[Key(11)]
		public uint SubSkillID;
		[Key(12)]
		public uint ChangeSkillID;
		[Key(13)]
		public byte Grade;
		[Key(14)]
		public E_QuickSlotType QuickSlotType;
		[Key(15)]
		public E_QuickSlotAutoType QuickSlotAutoType;
		[Key(16)]
		public E_PreconditionType PreconditionType;
		[Key(17)]
		public float PreconditionCheck;
		[Key(18)]
		public E_SkillType SkillType;
		[Key(19)]
		public E_SkillAniType SkillAniType;
		[Key(20)]
		public E_SkillEventType SkillEventType;
		[Key(21)]
		public bool TargetRenew;
		[Key(22)]
		public E_TargetPosType TargetPosType;
		[Key(23)]
		public E_TargetType TargetType;
		[Key(24)]
		public float Distance;
		[Key(25)]
		public E_CastingType CastingType;
		[Key(26)]
		public float CastingTime;
		[Key(27)]
		public E_CastingDeleteType CastingDeleteType;
		[Key(28)]
		public E_MissileType MissileType;
		[Key(29)]
		public byte MissileCnt;
		[Key(30)]
		public float MissileDelayTime;
		[Key(31)]
		public float MissileSpeed;
		[Key(32)]
		public float MissileDistance;
		[Key(33)]
		public float MissileRange;
		[Key(34)]
		public E_RangeType RangeType;
		[Key(35)]
		public uint RangeAngle;
		[Key(36)]
		public float Range;
		[Key(37)]
		public byte TargetCnt;
		[Key(38)]
		public E_DamageType DamageType;
		[Key(39)]
		public E_AttackType AttackType;
		[Key(40)]
		public float DamageRate;
		[Key(41)]
		public int AddDamage;
		[Key(42)]
		public byte AttackCount;
		[Key(43)]
		public int AttributeDamage;
		[Key(44)]
		public E_UnitAttributeType DefenAttributeType;
		[Key(45)]
		public byte DefenAttributeLv;
		[Key(46)]
		public float HealRate;
		[Key(47)]
		public int AddHeal;
		[Key(48)]
		public E_PosMoveType PosMoveType;
		[Key(49)]
		public float PosMoveDistance;
		[Key(50)]
		public float PosMoveSpeed;
		[Key(51)]
		public uint SummonID;
		[Key(52)]
		public E_CallType CallType;
		[Key(53)]
		public float SummonDistance;
		[Key(54)]
		public float CoolTime;
		[Key(55)]
		public float GlobalCoolTime;
		[Key(56)]
		public byte SkillUseCount;
		[Key(57)]
		public uint UseMPCount;
		[Key(58)]
		public E_MonAI_Type MonAI_Type;
		[Key(59)]
		public uint MonAI_Condition;
		[Key(60)]
		public uint MonAI_DelayTime;
		[Key(61)]
		public uint MonAI_CoolTime;
		[Key(62)]
		public byte MonAI_Order;
		[Key(63)]
		public E_HitAniType HitAniType;
		[Key(64)]
		public uint SkillEffectID;
		[Key(65)]
		public uint RangeEffectID;
		[Key(66)]
		public E_MissileEffectType MissileEffectType;
		[Key(67)]
		public uint ActuationEffectID;
		[Key(68)]
		public uint HitEffectID;
		[Key(69)]
		public E_HitPositionType HitPositionType;
		[Key(70)]
		public string IconID;
		[Key(71)]
		public string ToolTipID;
		[Key(72)]
		public string DetailTipID;
		[Key(73)]
		public uint AbilityActionID_01;
		[Key(74)]
		public uint AbilityActionID_02;
		[Key(75)]
		public uint AbilityActionID_03;
		[Key(76)]
		public E_AutoSlot AutoSlot;
		[Key(77)]
		public uint AutoSort;
		[Key(78)]
		public E_TownUseType TownUseType;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Skill_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Skill_Table> dicTables = new Dictionary<uint, Skill_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Skill_Table>(ref reader);

				dicTables.Add(table.SkillID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class SmeltOptionRate_Table
	{
		[Key(0)]
		public uint SmeltOptionRateID;
		[Key(1)]
		public uint SmeltOptionRateGroupID;
		[Key(2)]
		public uint Rate;
		[Key(3)]
		public uint SmeltScrollOptionGroupID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, SmeltOptionRate_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, SmeltOptionRate_Table> dicTables = new Dictionary<uint, SmeltOptionRate_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<SmeltOptionRate_Table>(ref reader);

				dicTables.Add(table.SmeltOptionRateID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class SmeltScroll_Table
	{
		[Key(0)]
		public uint SmeltScrollID;
		[Key(1)]
		public E_ItemType SmeltScrollType;
		[Key(2)]
		public E_ItemType ItemType;
		[Key(3)]
		public List<uint> SmeltItemID = new List<uint>();
		[Key(4)]
		public List<uint> SmeltItemCnt = new List<uint>();
		[Key(5)]
		public List<uint> SmeltMaterialItemID = new List<uint>();
		[Key(6)]
		public List<uint> SmeltMaterialItemCnt = new List<uint>();
		[Key(7)]
		public List<uint> AddOptionRate = new List<uint>();
		[Key(8)]
		public uint SmeltOptionRateGroupID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, SmeltScroll_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, SmeltScroll_Table> dicTables = new Dictionary<uint, SmeltScroll_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<SmeltScroll_Table>(ref reader);

				dicTables.Add(table.SmeltScrollID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class SmeltScrollOption_Table
	{
		[Key(0)]
		public uint SmeltScrollOptionID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint LowSmeltScrollGetRate;
		[Key(3)]
		public uint MidSmeltScrollGetRate;
		[Key(4)]
		public uint HighSmeltScrollGetRate;
		[Key(5)]
		public uint AbilityActionID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, SmeltScrollOption_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, SmeltScrollOption_Table> dicTables = new Dictionary<uint, SmeltScrollOption_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<SmeltScrollOption_Table>(ref reader);

				dicTables.Add(table.SmeltScrollOptionID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Sound_Table
	{
		[Key(0)]
		public uint SoundID;
		[Key(1)]
		public string SoundFile;
		[Key(2)]
		public E_SoundType SoundType;
		[Key(3)]
		public float SoundVolume;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Sound_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Sound_Table> dicTables = new Dictionary<uint, Sound_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Sound_Table>(ref reader);

				dicTables.Add(table.SoundID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class SpecialShop_Table
	{
		[Key(0)]
		public uint SpecialShopID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public E_UnusedType UnusedType;
		[Key(3)]
		public E_BuyOpenType BuyOpenType;
		[Key(4)]
		public E_ViewType ViewType;
		[Key(5)]
		public E_MiniGoodsType MiniGoodsType;
		[Key(6)]
		public string ShopTextID;
		[Key(7)]
		public string IconID;
		[Key(8)]
		public E_SizeType SizeType;
		[Key(9)]
		public E_CashType CashType;
		[Key(10)]
		public E_SpecialShopType SpecialShopType;
		[Key(11)]
		public E_SpecialSubTapType SpecialSubTapType;
		[Key(12)]
		public uint PositionNumber;
		[Key(13)]
		public byte RecommendGoods;
		[Key(14)]
		public uint SpecialShopCondition;
		[Key(15)]
		public uint ChangeCollectionCondition;
		[Key(16)]
		public uint PetCollectionCondition;
		[Key(17)]
		public byte WPvPGrade;
		[Key(18)]
		public E_BuyKindType BuyKindType;
		[Key(19)]
		public E_GoodsListGetType GoodsListGetType;
		[Key(20)]
		public List<uint> ShopListID = new List<uint>();
		[Key(21)]
		public List<uint> ShopListCount = new List<uint>();
		[Key(22)]
		public List<E_BonusType> BonusType = new List<E_BonusType>();
		[Key(23)]
		public List<uint> BonusItemID = new List<uint>();
		[Key(24)]
		public List<uint> BonusItemCnt = new List<uint>();
		[Key(25)]
		public List<byte> BonusBuyCnt = new List<byte>();
		[Key(26)]
		public E_StateType StateType;
		[Key(27)]
		public E_GoodsKindType GoodsKindType;
		[Key(28)]
		public uint GoodsItemID;
		[Key(29)]
		public uint GoodsChangeID;
		[Key(30)]
		public uint GoodsPetID;
		[Key(31)]
		public uint GoodsCount;
		[Key(32)]
		public uint FlatProductListID;
		[Key(33)]
		public E_FlatProductType FlatProductType;
		[Key(34)]
		public uint FlatProductPeriod;
		[Key(35)]
		public uint LastGiveListID;
		[Key(36)]
		public E_PriceType PriceType;
		[Key(37)]
		public uint BuyItemID;
		[Key(38)]
		public uint BuyItemCount;
		[Key(39)]
		public uint OriganalBuyCount;
		[Key(40)]
		public uint PriceGroupID;
		[Key(41)]
		public uint RubyCount;
		[Key(42)]
		public byte BuyCharLevel;
		[Key(43)]
		public E_ServerType ServerType;
		[Key(44)]
		public E_BuyLimitType BuyLimitType;
		[Key(45)]
		public uint BuyLimitCount;
		[Key(46)]
		public ulong BuyStartTime;
		[Key(47)]
		public ulong BuyFinishTime;
		[Key(48)]
		public uint BuyMaxCount;
		[Key(49)]
		public uint DiamondCount;
		[Key(50)]
		public string GoogleID;
		[Key(51)]
		public string IOSStoreID;
		[Key(52)]
		public string OneStoreID;
		[Key(53)]
		public string TooltipImageCode;
		[Key(54)]
		public string TooltipID;
		[Key(55)]
		public string DescriptionID;
		[Key(56)]
		public byte RedNotice;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, SpecialShop_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, SpecialShop_Table> dicTables = new Dictionary<uint, SpecialShop_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<SpecialShop_Table>(ref reader);

				dicTables.Add(table.SpecialShopID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Stage_Table
	{
		[Key(0)]
		public uint StageID;
		[Key(1)]
		public E_UnusedType UnusedType;
		[Key(2)]
		public string StageTextID;
		[Key(3)]
		public string StageDescID;
		[Key(4)]
		public string ResourceFileName;
		[Key(5)]
		public string MinmapFileName;
		[Key(6)]
		public string MapFileName;
		[Key(7)]
		public string IconFileName;
		[Key(8)]
		public E_StageType StageType;
		[Key(9)]
		public byte StageLevel;
		[Key(10)]
		public E_StageOpenType StageOpenType;
		[Key(11)]
		public byte OpenGuildLevel;
		[Key(12)]
		public E_ChannelPrivate ChannelPrivate;
		[Key(13)]
		public E_ChannelChange ChannelChange;
		[Key(14)]
		public byte InMinLevel;
		[Key(15)]
		public byte InMaxLevel;
		[Key(16)]
		public uint ClearQuest;
		[Key(17)]
		public uint OpenUseItemID;
		[Key(18)]
		public uint OpenUseItemCount;
		[Key(19)]
		public uint OpenClearGimmickID;
		[Key(20)]
		public byte StageGuildOpen;
		[Key(21)]
		public E_StageEnterType StageEnterType;
		[Key(22)]
		public List<uint> EnterUseItemID = new List<uint>();
		[Key(23)]
		public List<uint> EnterUseItemCount = new List<uint>();
		[Key(24)]
		public uint LinkTempleID;
		[Key(25)]
		public uint InBuffID;
		[Key(26)]
		public uint InCharacterCount;
		[Key(27)]
		public List<uint> NotConsumeBuffIDs = new List<uint>();
		[Key(28)]
		public List<uint> QuickSlot = new List<uint>();
		[Key(29)]
		public List<uint> GiveItemID = new List<uint>();
		[Key(30)]
		public List<uint> GiveItemCount = new List<uint>();
		[Key(31)]
		public uint StageDropGroupID;
		[Key(32)]
		public uint Ch_Exp_Penalt;
		[Key(33)]
		public uint Ch_Gold_Penalt;
		[Key(34)]
		public E_StageClearType StageClearType;
		[Key(35)]
		public uint FirstClearReward;
		[Key(36)]
		public uint ClearLimitTime;
		[Key(37)]
		public List<uint> ClearRewardID = new List<uint>();
		[Key(38)]
		public List<uint> ClearRewardCount = new List<uint>();
		[Key(39)]
		public List<uint> FailRewardID = new List<uint>();
		[Key(40)]
		public List<uint> FailRewardCount = new List<uint>();
		[Key(41)]
		public E_SummonBossType SummonBossType;
		[Key(42)]
		public uint SummonBossID;
		[Key(43)]
		public uint SummonCount;
		[Key(44)]
		public uint SummonItemCount;
		[Key(45)]
		public E_AutoUseType AutoUseType;
		[Key(46)]
		public E_TeleportType TeleportType;
		[Key(47)]
		public E_PKUseType PKUseType;
		[Key(48)]
		public List<byte> PKChannelNum = new List<byte>();
		[Key(49)]
		public List<byte> FieldBossSpawnChannelNum = new List<byte>();
		[Key(50)]
		public uint BossSpawnRate;
		[Key(51)]
		public uint BossSpawnTime;
		[Key(52)]
		public List<byte> LuckyMonSpawnChannelNum = new List<byte>();
		[Key(53)]
		public uint LuckyMonSpawnRate;
		[Key(54)]
		public uint LuckyMonSpawnTime;
		[Key(55)]
		public E_TendencyDownType TendencyDownType;
		[Key(56)]
		public E_DeathPenaltyType DeathPenaltyType;
		[Key(57)]
		public E_StageSaveType StageSaveType;
		[Key(58)]
		public E_RoamingType RoamingType;
		[Key(59)]
		public uint DeathPortal;
		[Key(60)]
		public uint BGMID;
		[Key(61)]
		public List<int> WorldMapPosition = new List<int>();
		[Key(62)]
		public uint StageSizeX;
		[Key(63)]
		public uint StageSizeY;
		[Key(64)]
		public uint DefaultPortal;
		[Key(65)]
		public string BoundsMin;
		[Key(66)]
		public string BoundsMax;
		[Key(67)]
		public uint SightGridDistance;
		[Key(68)]
		public E_CollisionType CollisionType;
		[Key(69)]
		public E_RidingType RidingType;
		[Key(70)]
		public float BossViewPosY;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Stage_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Stage_Table> dicTables = new Dictionary<uint, Stage_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Stage_Table>(ref reader);

				dicTables.Add(table.StageID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class StageDrop_Table
	{
		[Key(0)]
		public uint StageDropID;
		[Key(1)]
		public uint DropGroupID;
		[Key(2)]
		public uint NormalBlessRate;
		[Key(3)]
		public uint NormalPowerRate;
		[Key(4)]
		public uint BossBlessRate;
		[Key(5)]
		public uint BossPowerRate;
		[Key(6)]
		public uint DropListGroupID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, StageDrop_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, StageDrop_Table> dicTables = new Dictionary<uint, StageDrop_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<StageDrop_Table>(ref reader);

				dicTables.Add(table.StageDropID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class StageDropList_Table
	{
		[Key(0)]
		public uint StageDropListID;
		[Key(1)]
		public uint ListGroupID;
		[Key(2)]
		public uint DropRate;
		[Key(3)]
		public uint BossDropRate;
		[Key(4)]
		public uint DropItemID;
		[Key(5)]
		public uint DropItemCount;
		[Key(6)]
		public uint BossDropItemCount;
		[Key(7)]
		public uint Down50Per;
		[Key(8)]
		public uint Down40Per;
		[Key(9)]
		public uint Down30Per;
		[Key(10)]
		public uint Down20Per;
		[Key(11)]
		public uint Down10Per;
		[Key(12)]
		public uint DropSignRate;
		[Key(13)]
		public uint DropLimitCount;
		[Key(14)]
		public E_AutoStorageType ViewCheck;
		[Key(15)]
		public uint ViewNo;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, StageDropList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, StageDropList_Table> dicTables = new Dictionary<uint, StageDropList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<StageDropList_Table>(ref reader);

				dicTables.Add(table.StageDropListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class StartingItem_Table
	{
		[Key(0)]
		public uint StartingItemID;
		[Key(1)]
		public uint CharacterID;
		[Key(2)]
		public List<uint> ItemID = new List<uint>();
		[Key(3)]
		public List<uint> ItemCnt = new List<uint>();
		[Key(4)]
		public List<uint> ChangeID = new List<uint>();
		[Key(5)]
		public List<uint> PetID = new List<uint>();
		[Key(6)]
		public List<uint> SkillID = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, StartingItem_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, StartingItem_Table> dicTables = new Dictionary<uint, StartingItem_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<StartingItem_Table>(ref reader);

				dicTables.Add(table.StartingItemID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Summon_Table
	{
		[Key(0)]
		public uint SummonID;
		[Key(1)]
		public E_SummonType SummonType;
		[Key(2)]
		public E_ActiveType ActiveType;
		[Key(3)]
		public uint SummonTime;
		[Key(4)]
		public byte SummonCnt;
		[Key(5)]
		public byte OverlapCnt;
		[Key(6)]
		public float StartTime;
		[Key(7)]
		public float PeriodTime;
		[Key(8)]
		public E_DeathType DeathType;
		[Key(9)]
		public float MaxHPRate;
		[Key(10)]
		public float ShortAttackRate;
		[Key(11)]
		public float LongAttackRate;
		[Key(12)]
		public float MagicAttackRate;
		[Key(13)]
		public float DefenseRate;
		[Key(14)]
		public float MagicDefenseRate;
		[Key(15)]
		public float RunSpeedRate;
		[Key(16)]
		public float HPRecoveryRate;
		[Key(17)]
		public uint HPRecoveryTime;
		[Key(18)]
		public List<uint> MonsterID = new List<uint>();
		[Key(19)]
		public List<uint> MonsterCount = new List<uint>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Summon_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Summon_Table> dicTables = new Dictionary<uint, Summon_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Summon_Table>(ref reader);

				dicTables.Add(table.SummonID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Survery_Table
	{
		[Key(0)]
		public uint SurveryID;
		[Key(1)]
		public string QText;
		[Key(2)]
		public List<string> QAText = new List<string>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Survery_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Survery_Table> dicTables = new Dictionary<uint, Survery_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Survery_Table>(ref reader);

				dicTables.Add(table.SurveryID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Temple_Table
	{
		[Key(0)]
		public uint TempleID;
		[Key(1)]
		public E_TempleType TempleType;
		[Key(2)]
		public string TempleAreaName;
		[Key(3)]
		public uint TempleOrderUI;
		[Key(4)]
		public E_HiddenUI HiddenUI;
		[Key(5)]
		public E_RegistrationUI RegistrationUI;
		[Key(6)]
		public E_Replay Replay;
		[Key(7)]
		public string ResourcePrefab;
		[Key(8)]
		public uint ResourcePrefabFX1;
		[Key(9)]
		public uint ResourcePrefabFX2;
		[Key(10)]
		public uint ResourcePrefabFX3;
		[Key(11)]
		public uint ResourcePrefabFX4;
		[Key(12)]
		public uint Scale;
		[Key(13)]
		public E_CollisionType CollisionType;
		[Key(14)]
		public float CollisionRadius;
		[Key(15)]
		public E_NameViewType NameViewType;
		[Key(16)]
		public float NameViewRadius;
		[Key(17)]
		public uint NameViewFontSize;
		[Key(18)]
		public uint NameViewTime;
		[Key(19)]
		public string Icon;
		[Key(20)]
		public string QuestRewardIcon;
		[Key(21)]
		public string ToolTipID;
		[Key(22)]
		public uint EntrancePortalID;
		[Key(23)]
		public uint ExitPortalID;
		[Key(24)]
		public List<uint> GachaGroupID = new List<uint>();
		[Key(25)]
		public string GuideLocale;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Temple_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Temple_Table> dicTables = new Dictionary<uint, Temple_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Temple_Table>(ref reader);

				dicTables.Add(table.TempleID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class TempleObject_Table
	{
		[Key(0)]
		public uint TempleObjectID;
		[Key(1)]
		public string TempleObjectTextID;
		[Key(2)]
		public string ResourcePrefab;
		[Key(3)]
		public string FieldGimmickId;
		[Key(4)]
		public uint TempleStageID;
		[Key(5)]
		public uint NeedItemID;
		[Key(6)]
		public string ItemPrefab;
		[Key(7)]
		public uint ItemScale;
		[Key(8)]
		public uint Scale;
		[Key(9)]
		public E_NameViewType NameViewType;
		[Key(10)]
		public float NameViewRadius;
		[Key(11)]
		public byte HideOnOpen;
		[Key(12)]
		public string ErrorLocale;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, TempleObject_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, TempleObject_Table> dicTables = new Dictionary<uint, TempleObject_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<TempleObject_Table>(ref reader);

				dicTables.Add(table.TempleObjectID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class Tutorial_Table
	{
		[Key(0)]
		public uint TutorialID;
		[Key(1)]
		public uint QuestID;
		[Key(2)]
		public byte Step;
		[Key(3)]
		public E_UnitAttributeType AttributeType;
		[Key(4)]
		public E_TutorialType TutorialType;
		[Key(5)]
		public string NpcNameText;
		[Key(6)]
		public string GuideResource;
		[Key(7)]
		public string DialogueResource;
		[Key(8)]
		public string Text;
		[Key(9)]
		public uint SoundID;
		[Key(10)]
		public uint SkipSoundID;
		[Key(11)]
		public string SkipText;
		[Key(12)]
		public E_GuideType GuideType;
		[Key(13)]
		public List<string> GuideParams = new List<string>();

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, Tutorial_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, Tutorial_Table> dicTables = new Dictionary<uint, Tutorial_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<Tutorial_Table>(ref reader);

				dicTables.Add(table.TutorialID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class UIResource_Table
	{
		[Key(0)]
		public byte ID;
		[Key(1)]
		public E_UIType UIType;
		[Key(2)]
		public string TextColor;
		[Key(3)]
		public byte Tier;
		[Key(4)]
		public string TierText;
		[Key(5)]
		public string BgImg;
		[Key(6)]
		public string BgEffect;
		[Key(7)]
		public string MakeMileageGaugeBar;
		[Key(8)]
		public string MileageIconEffect;
		[Key(9)]
		public string MileageBarEffect;
		[Key(10)]
		public string GachaCardEffect;
		[Key(11)]
		public string DropEffect;
		[Key(12)]
		public string EffEnchantFront;
		[Key(13)]
		public string EffEnchantBG;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<byte, UIResource_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<byte, UIResource_Table> dicTables = new Dictionary<byte, UIResource_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<UIResource_Table>(ref reader);

				dicTables.Add(table.ID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class UpgradeList_Table
	{
		[Key(0)]
		public uint UpgradeListID;
		[Key(1)]
		public uint GroupID;
		[Key(2)]
		public uint UpgradeItemID;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, UpgradeList_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, UpgradeList_Table> dicTables = new Dictionary<uint, UpgradeList_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<UpgradeList_Table>(ref reader);

				dicTables.Add(table.UpgradeListID, table);
			}
			return dicTables;
		}
	}

	[MessagePackObject]
	public class WeightPenalty_Table
	{
		[Key(0)]
		public uint WeightPenaltyID;
		[Key(1)]
		public uint CharacterWeighRate;
		[Key(2)]
		public string WeightPenaltyColor;
		[Key(3)]
		public byte SlotMaxCheck;
		[Key(4)]
		public uint NotPenaltyRanking;
		[Key(5)]
		public uint AbilityActionID_01;
		[Key(6)]
		public uint AbilityActionID_02;
		[Key(7)]
		public string SimplePenaltyTip;

		[UnityEngine.Scripting.Preserve]
		public static Dictionary<uint, WeightPenalty_Table> Deserialize(ref byte[] _readBytes)
		{
			Dictionary<uint, WeightPenalty_Table> dicTables = new Dictionary<uint, WeightPenalty_Table>();

			MessagePackReader reader = new MessagePackReader(new System.ReadOnlyMemory<byte>(_readBytes));

			int tableCount = MessagePackSerializer.Deserialize<int>(ref reader);

			for (int i = 0; i < tableCount; i++)
			{
				var table = MessagePackSerializer.Deserialize<WeightPenalty_Table>(ref reader);

				dicTables.Add(table.WeightPenaltyID, table);
			}
			return dicTables;
		}
	}

}
