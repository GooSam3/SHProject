using GameDB;
using System;
using ZNet.Data;
using UnityEngine;
using System.Collections.Generic;

/// <summary> 성지 거점 정보 </summary>
public class GodLandSpotInfoConverted
{
	public readonly uint GodLandTid;
	public readonly uint SlotGroupId;
	public readonly GodLand_Table GodLandTable;
	public readonly E_UnitType UnitType;
	public readonly E_TargetType TargetType;
	public readonly uint MonsterTid;
	public readonly uint CharTid;
	public readonly ulong CharId;
	public readonly uint ChangeTid;
	public readonly string Nick;
	public readonly uint Lv;
	public readonly uint ProductionCnt;
	public readonly ulong TimeCnt;
	public readonly uint Atk;
	public readonly uint Def;
	public readonly uint Mdef;

	public bool IsSelected { get; set; }

	/// <summary> 강탈시 내 서버정보가 없어도 내 슬롯 정보를 보여주는 용도 </summary>
	public GodLandSpotInfoConverted(uint atk, uint def, uint mdef)
	{
		GodLandTid = 0;
		SlotGroupId = 0;
		GodLandTable = null;
		UnitType = E_UnitType.Character;
		TargetType = E_TargetType.Self;
		MonsterTid = 0;
		CharTid = Me.CurCharData.TID;
		CharId = Me.CurCharData.ID;
		ChangeTid = Me.CurCharData.CurrentMainChange;
		Nick = Me.CurCharData.Nickname;
		Lv = Me.CurCharData.Level;
		Atk = atk;
		Def = def;
		Mdef = mdef;
	}

	/// <summary> 패킷으로 생성하지만 스텟정보는 안줄때 </summary>
	public GodLandSpotInfoConverted(WebNet.GodLandSpotInfo info, uint atk, uint def, uint mdef)
	{
		GodLandTid = info.GodLandTid;
		SlotGroupId = info.SlotGroupId;
		GodLandTable = DBGodLand.Get(info.GodLandTid);
		UnitType = (E_UnitType)info.UnitType;
		TargetType = (E_TargetType)info.TargetType;
		MonsterTid = info.Data.Value.MonsterTid;
		CharTid = info.Data.Value.CharTid;
		CharId = info.Data.Value.CharId;
		ChangeTid = info.Data.Value.ChangeTid;
		Nick = info.Data.Value.Nick;
		Lv = info.Data.Value.Lv;
		ProductionCnt = info.Data.Value.ProductionCnt;
		TimeCnt = info.Data.Value.TimeCnt;
		Atk = atk;
		Def = def;
		Mdef = mdef;
	}

	/// <summary> 패킷받기 전에 임시용도 빠른 맵전환을 위해 기본정보만 생성 </summary>
	public GodLandSpotInfoConverted(GodLand_Table table)
	{
		GodLandTid = table.GodLandID;
		GodLandTable = table;
	}

	/// <summary> 지역정보 요청 패킷으로 생성 </summary>
	public GodLandSpotInfoConverted(WebNet.GodLandSpotInfo info)
	{
		GodLandTid = info.GodLandTid;
		SlotGroupId = info.SlotGroupId;
		GodLandTable = DBGodLand.Get(info.GodLandTid);
		UnitType = (E_UnitType)info.UnitType;
		TargetType = (E_TargetType)info.TargetType;
		MonsterTid = info.Data.Value.MonsterTid;
		CharTid = info.Data.Value.CharTid;
		CharId = info.Data.Value.CharId;
		ChangeTid = info.Data.Value.ChangeTid;
		Nick = info.Data.Value.Nick;
		Lv = info.Data.Value.Lv;
		ProductionCnt = info.Data.Value.ProductionCnt;
		TimeCnt = info.Data.Value.TimeCnt;
		Atk = info.Data.Value.Atk;
		Def = info.Data.Value.Def;
		Mdef = info.Data.Value.Mdef;
	}
}

/// <summary> 성지 기록 정보 </summary>
public class GodLandBattleRecordConverted
{
	public readonly uint GodLandTid;
	public readonly WebNet.E_BATTLE_ROLE Role;
	public readonly bool Status;
	public readonly ushort ServerIdx;
	public readonly uint CharTid;
	public readonly string Nick;
	public readonly uint Lv;
	public readonly string GuildName;
	public readonly byte MarkTid;
	public readonly uint ChangeTid;
	public readonly ulong CreateDt;

	public GodLandBattleRecordConverted(WebNet.GodLandFightRecode info)
	{
		GodLandTid = info.GodLandTid;
		Role = info.Role;
		Status = info.Status;
		ServerIdx = info.ServerIdx;
		CharTid = info.CharTid;
		Nick = info.Nick;
		Lv = info.Lv;
		GuildName = info.GuildName;
		MarkTid = (byte)info.MarkTid;
		ChangeTid = info.ChangeTid;
		CreateDt = info.CreateDt;
	}
}

public class GodLandStatInfoConverted
{
	public readonly uint ObjectID;
	public readonly uint Level;
	public readonly uint Attack;
	public readonly uint MeleeDefence;
	public readonly uint MagicDefence;

	public GodLandStatInfoConverted(MmoNet.S2C_GodLandStatInfo info)
	{
		ObjectID = info.ObjectID;
		Level = info.Level;
		Attack = (uint)info.Attack;
		MeleeDefence = (uint)info.MeleeDefence;
		MagicDefence = (uint)info.MagicDefence;
	}
}

public class GodLandContainer : ContainerBase
{
	public List<GodLandSpotInfoConverted> SpotInfoList = new List<GodLandSpotInfoConverted>();

	public List<GodLandStatInfoConverted> UserInfoList = new List<GodLandStatInfoConverted>();

	private List<GodLandBattleRecordConverted> BattleRecordList = new List<GodLandBattleRecordConverted>();

	private GodLandSpotInfoConverted mySpotInfo;

	public ulong RemainTimeTargetTime { get; set; }

	public static bool IsRedDotOn { get; private set; }

	public override void Clear()
	{
		SpotInfoList.Clear();

		UserInfoList.Clear();

		BattleRecordList.Clear();

		mySpotInfo = null;

		RemainTimeTargetTime = 0;
	}

	// 레드닷 표시를 위한 로직들
	
	public bool IsMyGatheringFulled(GodLandSpotInfoConverted myData)
	{
		if (myData != null &&
			myData.GodLandTable != null &&
			myData.TargetType == E_TargetType.Self &&
			myData.ProductionCnt == myData.GodLandTable.ProductionItemCountMax) {
			return true;
		}
		return false;
	}

	public bool IsBattleRecordChanged(List<GodLandBattleRecordConverted> checkList)
	{
		if (checkList.Count == 0) {
			return false;
		}

		var oldList = DeviceSaveDatas.LoadData(DeviceSaveDatas.KEY_GODLAND_BATTLERECORD_LIST);
		var list = GetConvertedBattleRecordList(checkList);

		if (oldList.Count == list.Count) {
			for (int i = 0; i < oldList.Count; ++i) {
				if (oldList[i] != list[i]) {
					return true;
				}
			}
			return false;
		}
		else {
			return true;
		}
	}

	private List<int> GetConvertedBattleRecordList(List<GodLandBattleRecordConverted> checkList)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < checkList.Count; ++i) {
			var value1 = (int)checkList[i].Role * 10; //10의 자리로 변환
			var value2 = checkList[i].Status ? 1 : 0; //1의 자리
			list.Add(value1 + value2);
		}
		return list;
	}

	public void SaveBattleRecord(List<GodLandBattleRecordConverted> checkList)
	{
		var list = GetConvertedBattleRecordList(checkList);
		DeviceSaveDatas.SaveData(DeviceSaveDatas.KEY_GODLAND_BATTLERECORD_LIST, list);
	}

	#region 필드 통신 //////////////////////////////////////////////////////////

	/// <summary> 내가 점거중인 정보를 요청한다 없으면 null</summary>
	public void REQ_GetSelfGodLandInfo(Action<bool, GodLandSpotInfoConverted> callback)
	{
		ZWebManager.Instance.WebGame.REQ_GetSelfGodLandInfo((res, msg) => {
			var myEntity = ZPawnManager.Instance.MyEntity;
			if (myEntity == null)
			{
				return;
			}

			uint atk;
			switch (myEntity.CharacterType) {
				case E_CharacterType.Knight:
				case E_CharacterType.Assassin: atk = (uint)myEntity.GetAbility(E_AbilityType.FINAL_MAX_SHORT_ATTACK); break;
				case E_CharacterType.Archer: atk = (uint)myEntity.GetAbility(E_AbilityType.FINAL_MAX_LONG_ATTACK); break;
				case E_CharacterType.Wizard: atk = (uint)myEntity.GetAbility(E_AbilityType.FINAL_MAX_MAGIC_ATTACK); break;
				default: atk = 0; break;
			}
			uint def = (uint)(myEntity.GetAbility(E_AbilityType.FINAL_MELEE_DEFENCE) +
				myEntity.GetAbility(E_AbilityType.FINAL_PET_MELEE_DEFENCE));
			uint mdef = (uint)(myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_DEFENCE) +
				myEntity.GetAbility(E_AbilityType.FINAL_PET_MAGIC_DEFENCE));

			if (msg.IsHave) {
				mySpotInfo = new GodLandSpotInfoConverted(msg.SpotInfo.Value, atk, def, mdef);
				callback?.Invoke(true, mySpotInfo);
			}
			else {
				mySpotInfo = null;
				callback?.Invoke(false, new GodLandSpotInfoConverted(atk, def, mdef));
			}
		});
	}

	/// <summary> 점유지 목록을 요청한다 </summary>
	public void REQ_GetGodLandInfo(uint slotGroupId, bool forceRquest, Action<List<GodLandSpotInfoConverted>> callback)
	{
		// 이미 최근에 받은 데이타가 있을경우 요청없이 리턴
		if (forceRquest == false && SpotInfoList.Count > 0 && SpotInfoList.Exists(v => v.SlotGroupId == slotGroupId)) {
			callback?.Invoke(SpotInfoList);
			return;
		}

		ZWebManager.Instance.WebGame.REQ_GetGodLandInfo(slotGroupId, (res, msg) => {
			SpotInfoList.Clear();

			for (int i = 0; i < msg.SpotInfoLength; ++i) {
				var data = msg.SpotInfo(i).Value;

				if ((E_UnitType)data.UnitType == E_UnitType.Monster) {
					var godLandTable = DBGodLand.Get(data.GodLandTid);
					var monsterTable = DBMonster.Get(godLandTable.DefaultMonsterID);
					var myEntity = ZPawnManager.Instance.MyEntity;

					var skillTable = DBSkill.Get(monsterTable.BaseAttackID_01);
					uint atk;
					switch (skillTable.AttackType) {
						case E_AttackType.Magic: atk = monsterTable.MagicAttack; break;
						case E_AttackType.Long: atk = monsterTable.LongAttack + monsterTable.WeaponAttack; break;
						default: atk = monsterTable.ShortAttack + monsterTable.WeaponAttack; break;
					}

					uint def = monsterTable.MeleeDefense;
					uint mdef = monsterTable.MagicDefense;

					SpotInfoList.Add(new GodLandSpotInfoConverted(data, atk, def, mdef));
				}
				else {
					SpotInfoList.Add(new GodLandSpotInfoConverted(data));
				}
			}
			callback?.Invoke(SpotInfoList);
		});
	}

	/// <summary> 강탈(점유)하러 입장하기 </summary>
	public void REQ_GetMatchGodLandSpot(uint godLandTid, Action callback, Action discardCallback)
	{
		//// 소유지가 이미 있는지
		//if( mySpotInfo != null) {
		//	UIMessagePopup.ShowPopupOk(DBLocale.GetText(DBLocale.GetText("GOD_LAND_SLOT_IS_FULL")));
		//	return;
		//}

		// 무기 착용중인지
		var data = Me.CurCharData.GetEquippedItem(E_EquipSlotType.Weapon);
		if (null == data) {
			UIMessagePopup.ShowPopupOk(DBLocale.GetText(DBLocale.GetText("Unmount_Weapon_Message")));
			return;
		}

		// 클래스에 맞는 무기 착용중인지
		bool hasClassWeapon = Me.CurCharData.IsCharacterEquipable(data.ItemTid);
		if (hasClassWeapon == false) {
			UIMessagePopup.ShowPopupOk(DBLocale.GetText(DBLocale.GetText("Job_Match_Weapon_Message")));
			return;
		}

		// 최종 확인 팝업
		var title = DBLocale.GetText("GodLand_GatherStart_Title_Popup");
		var desc = DBLocale.GetText("GodLand_GatherStart_Desc_Popup");
		UIMessagePopup.ShowPopupOkCancel(title, desc, () => {
			ZWebManager.Instance.WebGame.REQ_GetMatchGodLandSpot(godLandTid, (res, msg) => {
				callback?.Invoke();
			},
			(error, req, res) => {
				if (res.ErrCode == WebNet.ERROR.GOD_LAND_SLOT_IS_FULL) {
					if (mySpotInfo != null) {
						REQ_GodLandDiscard(mySpotInfo.GodLandTid, mySpotInfo.SlotGroupId, discardCallback);
					}
					else {
						ZLog.LogError(ZLogChannel.Default, "클라정보에 내 점유지가 업는데 서버에는 있다 확인해보자");
					}
				}
			});
		});
	}

	/// <summary> 나의 점유지 재화를 획득한다 </summary>
	public void REQ_GetGodLandSpotGatheringItem(uint godLandTid, Action<uint, ulong, uint> callback)
	{
		ZWebManager.Instance.WebGame.REQ_GetGodLandSpotGatheringItem(godLandTid, (res, msg) => {
			// 재화 업데이트 
			for (int i = 0; i < msg.ResultAccountStackItemsLength; i++) {
				var item = msg.ResultAccountStackItems(i).Value;
				Me.CurCharData.AddItemList(msg.ResultAccountStackItems(i).Value);

				if ( item.ItemTid == DBConfig.Godland_Production_ID ) {
					callback?.Invoke(item.ItemTid, item.Cnt, msg.TimeCnt);
				}
			}

			for (int i = 0; i < msg.ResultStackItemsLength; i++) {
				var item = msg.ResultAccountStackItems(i).Value;
				Me.CurCharData.AddItemList(msg.ResultStackItems(i).Value);

				if (item.ItemTid == DBConfig.Godland_Production_ID) {
					callback?.Invoke(item.ItemTid, item.Cnt, msg.TimeCnt);
				}
			}
		});
	}

	/// <summary> 나의 점유지 포기 </summary>
	private void REQ_GodLandDiscard(uint godLandTid, uint slotGroupId, Action discardCallback)
	{
		var title = DBLocale.GetText("GOD_LAND_SLOT_IS_FULL");
		var desc = DBLocale.GetText("GodLand_Popup_Exchange_Confirm");

		UIMessagePopup.ShowPopupOkCancel(title, desc, () => {
			ZWebManager.Instance.WebGame.REQ_GodLandDiscard(godLandTid, slotGroupId, (res, msg) => {
				discardCallback?.Invoke();
			});
		});
	}

	/// <summary>  나의 전투기록을 요청한다  </summary>
	/// <param name="forceRequest">true:항상보내기, false:전에꺼 있으면 리턴</param>
	public void REQ_GetGodLandFightRecord(bool forceRequest, Action<List<GodLandBattleRecordConverted>> callback )
	{
		if (forceRequest == false && BattleRecordList.Count > 0) {
			callback?.Invoke(BattleRecordList);
			return;
		}

		List<GodLandBattleRecordConverted> recordList = new List<GodLandBattleRecordConverted>();
		ZWebManager.Instance.WebGame.REQ_GetGodLandFightRecord((res, msg) => {
			for (int i = 0; i < msg.RecodeLength; ++i) {
				recordList.Add(new GodLandBattleRecordConverted(msg.Recode(i).Value));
			}

			callback?.Invoke(recordList);

		},
		(error, req, res) => {
			callback?.Invoke(recordList);
		});
	}

	#endregion
}