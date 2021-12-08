using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WebNet;
using ZDefine;

namespace ZNet.Data
{
	/// <summary>
	/// 현재 기기에서 플레이하는 주인용 데이터
	/// </summary>
	public static class Me
	{
		#region :: 연결될 서버별 주소들 ::
		public static string GlobalMatchServerUrl = "";
		public static string GameServerUrl = "";
		public static string MatchServerUrl = "";
		public static string ChatServerUrl = "";
		public static string BillingServerUrl = "";
		#endregion

		/// <summary> 
		/// NTSDK 사용시, pfSessionToken(플랫폼세션토큰)
		/// NTSDK 미사용시, 계정 고유 식별자 (구글 UID같은..)
		/// </summary>
		public static string SessionToken;
		/// <summary>
		/// 이용자ID (NID : 해당 게임서버의 유저 ID로 사용됩니다.소스상으로는 nid로 표기하며 19자리의 숫자 포맷입니다.(bigint)
		/// 게임에서는 '해당 서버에서 이용자의 유니크한 아이디로 사용'을 합니다.
		/// </summary>
		public static string NID;
		/// <summary>
		/// 이용자ID(GNID): N개의 게임서버에 발급된 N개의 NID를 대표하는 1개의 유저ID입니다.소스상으로는 gnid로 표기되며 bigint타입의 19자리 숫자입니다.
		/// </summary>
		public static string gNID;

		/// <summary> 선택된 ServerID </summary>
		public static uint SelectedServerID;
		/// <summary> 가장 최근에 접속한 서버 </summary>
		public static uint LastestLoginServerID = 1;
		public static ulong UserID;
		public static ulong CharID;
		/// <summary> 계정 전용 옵션값들 (Bitmask) <see cref="WebNet.E_AccountOptionType"/> </summary>
		public static long AccountOptionBit;
		/// <summary> 생성할 수 있는 캐릭터 최대 수 </summary>
		public static uint MaxCharCnt;

		public static List<uint> ItemGainHistory = new List<uint>();
		public static List<uint> CompleteArtifactDestinys = new List<uint>();

		public static event Action<uint> UpdateArtifactDestiny;

		/// <summary> 현재 플레이어 기본 정보가 유효한지 여부 </summary>
		public static bool IsValidMe
		{
			get { return UserID != 0 && CharID != 0; }// && null != CurUserData && null != CurCharData; }
		}

		/// <summary> 루틴상 유저 데이타가 null 이 나오면 안되고 반드시 존재해야 할 경우 이용(일반적인) </summary>
		public static UserData CurUserData
		{
			get { return Global.GetUser(UserID); }
		}

		/// <summary>  유저 데이타가 null 이 가능한 상황이거나 방어적으로 코드를 작성할 경우 이용 </summary>
		public static UserData FindCurUserData
		{
			get { return Global.FindUser(UserID); }
		}

		/// <summary> 루틴상 캐릭터 데이타가 null 이 나오면 안되고 반드시 존재해야 할 경우 이용(일반적인) </summary>
		public static CharacterData CurCharData
		{
			get { return CurUserData.GetChar(CharID); }
		}

		/// <summary>  캐릭터가 데이타가 null 이 가능한 상황이거나 방어적으로 코드를 작성할 경우 이용 </summary>
		public static CharacterData FindCurCharData
		{
			get
			{
				if (FindCurUserData != null)
				{
					return FindCurUserData.FindChar(CharID);
				}
				return null;
			}
		}

		/// <summary>
		/// 접속할 서버의 [기능별 서버] 주소를 설정한다.
		/// </summary>
		public static bool SetConnectServerInfo(uint _serverId)
		{
			SelectedServerID = _serverId;
			if (Global.DicServer.TryGetValue(_serverId, out var serverInfo))
			{
				for (int i = 0; i < serverInfo.ConnectInfoLength; i++)
				{
					var connectInfo = serverInfo.ConnectInfo(i).Value;

					if (connectInfo.Name == "global_match_server")//둘중 하나만 붙는다 , match
						GlobalMatchServerUrl = serverInfo.ConnectInfo(i).Value.Url;
					else if (connectInfo.Name == "match_server")//둘중 하나만 붙는다 , match
						MatchServerUrl = connectInfo.Url;
					else if (connectInfo.Name == "chat_server")
						ChatServerUrl = connectInfo.Url;
					else if (connectInfo.Name == "game_server")
						GameServerUrl = connectInfo.Url;
					else if (connectInfo.Name == "billing_server")
						BillingServerUrl = connectInfo.Url;
				}

				TimeHelper.SecondOffset = serverInfo.TsOffset;

				ZLog.Log(ZLogChannel.System, $"접속할 서버 선택 ServerID : {_serverId} | " +
					$"\n{nameof(GlobalMatchServerUrl)}: {GlobalMatchServerUrl}" +
					$"\n{nameof(GameServerUrl)}: {GameServerUrl}" +
					$"\n{nameof(MatchServerUrl)}: {MatchServerUrl}" +
					$"\n{nameof(ChatServerUrl)}: {ChatServerUrl}" +
					$"\n{nameof(BillingServerUrl)}: {BillingServerUrl}");

				return true;
			}
			else
			{
				ZLog.Log(ZLogChannel.System, ZLogLevel.Error, "Can't Find ServerID : " + _serverId);
				return false;
			}
		}

		public static void AddGainItem(uint addGroupId)
		{
			if (!ItemGainHistory.Contains(addGroupId))
			{
				ItemGainHistory.Add(addGroupId);
				CheckUpdateCompleteArtifactDestiny(true);
			}
		}

		public static bool IsGainedItem(uint itemGroupId)
		{
			return ItemGainHistory.Contains(itemGroupId);
		}

		static void CheckUpdateCompleteArtifactDestiny(bool bCheckNewAlram = false)
		{
			//갱신되면 전체를 다시 체크 해야함...
			bool bAddNew = false;
			foreach (var tableData in DBArtifact.GetAllLink())
			{
				if (CompleteArtifactDestinys.Contains(tableData.LinkID))
					continue;

				bool bComplete = true;

				//foreach (var tid in tableData.MaterialArtifactID)
				//{
				//	if (!IsGainedItem(DBItem.GetGroupId(tid)))
				//		bComplete = false;

				//	if (!bComplete)
				//		break;
				//}

				if (bComplete)
				{
					CompleteArtifactDestinys.Add(tableData.LinkID);

					if (bCheckNewAlram)
					{
						bAddNew = true;
						DeviceSaveDatas.AddCharacterKey(CharID, "COMPLETE_ARTIFACT_DESTINY", tableData.LinkID);
					}

					UpdateArtifactDestiny?.Invoke(tableData.LinkID);
				}
			}
		}

		/// <summary>
		/// 재화 정보
		/// </summary>
		/// <param name = "_itemTid"> 아이템 Tid </param>
		public static ulong GetCurrency(uint _itemTid)
		{
			// Diamond는 AccountItemStack에서 독자적 Cash로 변환됨
			if (DBConfig.Diamond_ID == _itemTid)
			{
				// TODO : 음수도 가능한 값이라 수정 필요함.
				return (ulong)CurUserData.Cash;
			}

			var invenList = CurCharData.InvenList;
			int invenCnt = invenList.Count;
			for (int i = 0; i < invenCnt; i++)
			{
				var item = invenList[i];
				if (null != item && item.item_tid == _itemTid)
					return item.cnt;
			}
			return 0;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static class Global
	{
		#region ====:: ServerInfo Management ::====

		/// <summary> Gateway로 부터 받은 서버정보 모음 </summary>
		public static Dictionary<uint, WebNet.ServerInfo> DicServer = new Dictionary<uint, WebNet.ServerInfo>();

		#endregion//ServerInfo Management

		public static Dictionary<ulong, UserData> DicUserData = new Dictionary<ulong, UserData>();

		public static UserData AddUser(ulong _newUserId)
		{
			if (!DicUserData.ContainsKey(_newUserId))
				DicUserData.Add(_newUserId, new UserData(_newUserId));

			return DicUserData[_newUserId];
		}

		public static bool RemoveUser(ulong _userId)
		{
			return DicUserData.Remove(_userId);
		}

		public static void ClearMe()
		{
			Me.SessionToken = string.Empty;
			Me.NID = string.Empty;
			Me.gNID = string.Empty;
			Me.SelectedServerID = 0;
			Me.LastestLoginServerID = 1;
			Me.UserID = 0;
			Me.CharID = 0;
			Me.MaxCharCnt = 0;
			Me.ItemGainHistory.Clear();
			Me.CompleteArtifactDestinys.Clear();
		}

		public static void ClearAllUsers()
		{
			DicUserData.Clear();
		}

		public static UserData GetUser(ulong _userID)
		{
			return DicUserData[_userID];
		}

		public static UserData FindUser(ulong _userID)
		{
			if (DicUserData.ContainsKey(_userID))
			{
				return DicUserData[_userID];
			}
			return null;
		}

		public static CharacterData GetChar(ulong _userID, ulong _charId)
		{
			return DicUserData[_userID].GetChar(_charId);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class UserData
	{
		public ulong UserID { get; private set; }

		/// <summary> Key : CharacterID </summary>
		public Dictionary<ulong, CharacterData> DicChar { get; private set; } = new Dictionary<ulong, CharacterData>();

		uint NormalMsgSendCnt = 0;  // 오늘 보낸 일반 쪽지 갯수
		uint GuildMsgSendCnt = 0;   // 오늘 보낸 길드 쪽지 갯수
		uint StorageMaxCnt = 0;     // 총 창고 슬롯 갯수

		public uint ColosseumRewardCnt = 0;

		public uint InfinityDungeonScheduleId = 0;
		public uint CurrentInfinityDungeonId = 0;
		public uint LastInfinityDungeonId = 0;
		public ulong InfinityDungeonRewardTime = 0;
		public uint LastRewardedStageTid = 0;

		/// <summary> 다이아몬드(캐시) </summary>
		/// <remarks> 캐시는 <see cref="AccountItemStack"/>으로 관리되지 않음.</remarks>
		public long Cash { get; private set; }

		/// <summary>유저가 달성한 최대 레벨 (갱신은 클라에서 알아서 진행)</summary>
		public uint MaxLevel { get; private set; }

		// ChangeDispatch
		public ulong ChangeQuestIssuedDt { get; private set; }
		public uint ChangeQuestLv { get; private set; }
		public uint ChangeQuestExp { get; private set; }

		private LoginEventData loginEventData = null;

		// 전역 룬드랍 셋트 (공용)
		public SortedList<GameDB.E_RuneSetType, bool> ListDropRuneSetType = new SortedList<GameDB.E_RuneSetType, bool>();
		public ulong DropRuneSet = 0;

		// -- 이벤트 --

		// 출석 정보 {groupId : Data}
		private Dictionary<uint, AttendEventData> dicAttendEventData = new Dictionary<uint, AttendEventData>();

		// -----------

		public UserData(ulong _userId)
		{
			this.UserID = _userId;
		}

		public void RefreshCash(UnityAction<long> _callback = null)
		{
			ZWebManager.Instance.WebGame.REQ_GetCashBalance((receivePacket, msg) =>
			{
				_callback?.Invoke(msg.CashCoinBalance);
			});
		}

		public void SetCash(long _cash)
		{
			this.Cash = _cash;

            if (UIManager.Instance.Find(out UIFrameHUD _hud)) _hud.RefreshCurrency(DBConfig.Diamond_ID);
        }

		/// <summary> 새로운 캐릭터 추가 or 이미 존재한다면 리셋 </summary>
		public CharacterData AddChar(Character _newCharacter)
		{
			if (!DicChar.ContainsKey(_newCharacter.CharId))
				DicChar.Add(_newCharacter.CharId, new CharacterData(ref _newCharacter));
			else
				DicChar[_newCharacter.CharId].Reset(ref _newCharacter);

			return DicChar[_newCharacter.CharId];
		}

		public void RemoveChar(ulong _charId)
		{
			if (DicChar.ContainsKey(_charId))
				DicChar.Remove(_charId);
		}

		public CharacterData GetChar(ulong _charId)
		{
			return DicChar[_charId];
		}

		public CharacterData FindChar(ulong CharId)
		{
			if (DicChar.ContainsKey(CharId))
				return DicChar[CharId];

			return null;
		}

		public CharacterData GetCharacter(string Nick)
		{
			foreach (CharacterData charData in DicChar.Values)
			{
				if (charData.Nickname == Nick)
					return charData;
			}

			return null;
		}

		public void AddAllCharData(ref ResGetAllCharInfoBundle _resAllCharInfoBundle, bool _bOverwrite = true)
		{
			// 기본 캐릭터 생성.
			var charData = AddChar(_resAllCharInfoBundle.Character.Value);

			if (_bOverwrite)
				charData.ClearData();

			// 차단한 캐릭터 정보
			for (int i = 0; i < _resAllCharInfoBundle.BlockInfoLength; ++i)
				charData.AddBlockCharacter(_resAllCharInfoBundle.BlockInfo(i));

			// 회복 경험치
			for (int i = 0; i < _resAllCharInfoBundle.RestoreExpLength; ++i)
				charData.AddRestoreExp(_resAllCharInfoBundle.RestoreExp(i));

			// 보관함
			for (int i = 0; i < _resAllCharInfoBundle.StorageEquipLength; ++i)
				charData.AddStorage(_resAllCharInfoBundle.StorageEquip(i));

			for (int i = 0; i < _resAllCharInfoBundle.StorageStackLength; ++i)
				charData.AddStorage(_resAllCharInfoBundle.StorageStack(i));

			// 인벤토리
			for (int i = 0; i < _resAllCharInfoBundle.AccountStackLength; ++i)
				charData.AddItemList(_resAllCharInfoBundle.AccountStack(i));

			for (int i = 0; i < _resAllCharInfoBundle.EquipItemLength; ++i)
				charData.AddItemList(_resAllCharInfoBundle.EquipItem(i));

			for (int i = 0; i < _resAllCharInfoBundle.StackItemLength; ++i)
				charData.AddItemList(_resAllCharInfoBundle.StackItem(i));

			// Mark
			for (int i = 0; i < _resAllCharInfoBundle.MarkItem.Value.MarkTidsLength; ++i)
				charData.AddMarkTID(_resAllCharInfoBundle.MarkItem.Value.MarkTids(i));

			// Collect
			for (int i = 0; i < _resAllCharInfoBundle.ItemCollectLength; ++i)
				charData.AddCollection(_resAllCharInfoBundle.ItemCollect(i), CollectionType.TYPE_ITEM);

			for (int i = 0; i < _resAllCharInfoBundle.ChangeCollectLength; ++i)
				charData.AddCollection(_resAllCharInfoBundle.ChangeCollect(i), CollectionType.TYPE_CHANGE);

			for (int i = 0; i < _resAllCharInfoBundle.PetCollectLength; ++i)
			{
				var petData = _resAllCharInfoBundle.PetCollect(i).Value;

				if (DBPetCollect.GetPetRideCollection(petData.CollectTid, out var table) == false)
					continue;

				if (table.PetType == GameDB.E_PetType.Pet)
					charData.AddCollection(petData, CollectionType.TYPE_PET);
				else if (table.PetType == GameDB.E_PetType.Vehicle)
					charData.AddCollection(petData, CollectionType.TYPE_RIDE);
			}

			// Change
			for (int i = 0; i < _resAllCharInfoBundle.ChangeItemLength; i++)
				charData.AddChangeList(_resAllCharInfoBundle.ChangeItem(i).Value);

			// Pet & Ride
			List<Pet> petList = new List<Pet>();
			List<Pet> rideList = new List<Pet>();

			for (int i = 0; i < _resAllCharInfoBundle.PetItemLength; i++)
			{
				var resultData = _resAllCharInfoBundle.PetItem(i).Value;
				if (DBPet.TryGet(resultData.PetTid, out var table) == false)
					continue;

				if (table.PetType == GameDB.E_PetType.Pet)
				{
					petList.Add(resultData);
				}
				else if (table.PetType == GameDB.E_PetType.Vehicle)
				{
					rideList.Add(resultData);
				}
			}
			charData.AddPetList(petList);
			charData.AddRideList(rideList);


			// Pet & Ride Keep
			for (int i = 0; i < _resAllCharInfoBundle.PetGachaKeepsLength; i++)
			{
				var resultData = _resAllCharInfoBundle.PetGachaKeeps(i).Value;
				if (DBPet.TryGet(resultData.PetTid, out var table) == false)
					continue;

				if (table.PetType == GameDB.E_PetType.Pet)
				{
					charData.AddPetKeepList(resultData);
				}
				else if (table.PetType == GameDB.E_PetType.Vehicle)
				{
					charData.AddRideKeepList(resultData);
				}
			}


			// Change Keep
			for (int i = 0; i < _resAllCharInfoBundle.ChangeGachaKeepsLength; i++)
				charData.AddChangeKeepList(_resAllCharInfoBundle.ChangeGachaKeeps(i).Value);

			// Guild Buff
			for (int i = 0; i < _resAllCharInfoBundle.GuildBuffInfoLength; i++)
				charData.AddGuildBuff(_resAllCharInfoBundle.GuildBuffInfo(i).Value);

			// Guild Alliance Simple Info
			for (int i = 0; i < _resAllCharInfoBundle.GuildAllianceInfoLength; i++)
				charData.AddAllianceGuild(_resAllCharInfoBundle.GuildAllianceInfo(i).Value);

			// Gain Skill
			for (int i = 0; i < _resAllCharInfoBundle.SkillBookLength; i++)
				charData.AddGainSkill(_resAllCharInfoBundle.SkillBook(i).Value.SkillTid);

			// Pet Adventure
			for (int i = 0; i < _resAllCharInfoBundle.PetAdventureLength; i++)
				charData.AddPetAdventure(_resAllCharInfoBundle.PetAdventure(i).Value);

			// Character Option
			for (int i = 0; i < _resAllCharInfoBundle.OptionsLength; i++)
				charData.AddOptionInfo(_resAllCharInfoBundle.Options(i).Value);

			// Quest
			for (int i = 0; i < _resAllCharInfoBundle.QuestsLength; i++)
				charData.AddQuestList(_resAllCharInfoBundle.Quests(i).Value, false);

			// Quest Event
			for (int i = 0; i < _resAllCharInfoBundle.QuestEventLength; i++)
				charData.AddEventQuestData(_resAllCharInfoBundle.QuestEvent(i).Value);

			// Friend
			for (int i = 0; i < _resAllCharInfoBundle.FriendsLength; i++)
				charData.AddFriend(_resAllCharInfoBundle.Friends(i).Value);

			for (int i = 0; i < _resAllCharInfoBundle.RequestFriendsLength; i++)
				charData.AddRequestFriend(_resAllCharInfoBundle.RequestFriends(i).Value);

			// Rune
			var listRune = new List<Rune>();
			for (int i = 0; i < _resAllCharInfoBundle.RuneItemLength; ++i)
			{
				if (_resAllCharInfoBundle.RuneItem(i).HasValue)
					listRune.Add(_resAllCharInfoBundle.RuneItem(i).Value);
			}
			charData.AddRuneList(listRune);

			// Gain Item
			for (int i = 0; i < _resAllCharInfoBundle.ItemAcqHistoryLength; ++i)
				charData.AddGainItem(_resAllCharInfoBundle.ItemAcqHistory(i));

			// Artifact
			for (int i = 0; i < _resAllCharInfoBundle.ArtifactLength; ++i)
				charData.AddArtifactID(_resAllCharInfoBundle.Artifact(i).Value);

			// 강림 파견
			charData.ClearChangeQuestList();
			for (int i = 0; i < _resAllCharInfoBundle.ChangeQuestLength; ++i)
				charData.AddChangeQuest(_resAllCharInfoBundle.ChangeQuest(i).Value);

			// 사당
			for (int i = 0; i < _resAllCharInfoBundle.TempleStagesLength; i++)
				charData.TempleInfo.AddStage(_resAllCharInfoBundle.TempleStages(i));

			// 속성 
			if (_resAllCharInfoBundle.Attribute.HasValue)
			{
				var value = _resAllCharInfoBundle.Attribute.Value;

				for (int i = 0; i < value.AttributeTidsLength; i++)
				{
					uint tid = value.AttributeTids(i);
					var type = DBAttribute.GetTypeByTID(tid);
					charData.AddAttributeTID(type, tid);
				}
			}

			// 스킬 사용 우선 순위
			for (int i = 0; i < _resAllCharInfoBundle.SkillUseOrdersLength; i++)
				charData.AddSkillUseOrder(_resAllCharInfoBundle.SkillUseOrders(i));

			// 요리
			for (int i = 0; i < _resAllCharInfoBundle.CookHistorysLength; i++)
				charData.AddCookRecipeData(_resAllCharInfoBundle.CookHistorys(i));
		}

		public void SetStorageMaxCnt(uint _maxCnt)
		{
			StorageMaxCnt = _maxCnt;
		}

		public void SetMaxLevel(uint _maxLevel)
		{
			MaxLevel = _maxLevel;
		}

		public uint GetStorageMaxCnt()
		{
			return StorageMaxCnt;
		}

		public void SetNormalMsgSendCnt(uint _NormalMsgSendCnt)
		{
			NormalMsgSendCnt = _NormalMsgSendCnt;
		}

		public uint GetNormalMsgSendCnt()
		{
			return NormalMsgSendCnt;
		}

		public void SetGuildMsgSendCnt(uint _GuildMsgSendCnt)
		{
			GuildMsgSendCnt = _GuildMsgSendCnt;
		}

		public uint GetGuildMsgSendCnt()
		{
			return GuildMsgSendCnt;
		}

		public void SetChangeQuestIssuedDt(ulong issuedDt)
		{
			ChangeQuestIssuedDt = issuedDt;
		}

		public void SetChangeQuestLevel(uint lv)
		{
			ChangeQuestLv = lv;
		}

		public void SetChangeQuestExp(uint exp)
		{
			ChangeQuestExp = exp;
		}

		/// <summary> 전역 룬 드랍 설정 </summary>		
		public void AddRuneSetOptionType(ulong _runeDropBit)
		{
			DropRuneSet = _runeDropBit;
			ListDropRuneSetType.Clear();
			SetRuneOption(ListDropRuneSetType, _runeDropBit);
		}

		public ulong ExtractSelectedDropRunType(SortedList<GameDB.E_RuneSetType, bool> _runeList)
		{
			ulong runeBitOption = 0;

			for (int i = 0; i < _runeList.Keys.Count; i++)
			{
				if (_runeList.Values[i])
				{
					ulong bitMask = (ulong)1 << (int)_runeList.Keys[i];
					runeBitOption |= bitMask;
				}
			}
			return runeBitOption;
		}

		private void SetRuneOption(SortedList<GameDB.E_RuneSetType, bool> _runeList, ulong _runeDropBit)
		{
			int maxBit = (int)GameDB.E_RuneSetType.Mana;

			for (int i = 1; i <= maxBit; i++)
			{
				bool select = false;
				if ((_runeDropBit & (ulong)1 << i) != 0)
				{
					select = true;
				}
				_runeList.Add((GameDB.E_RuneSetType)i, select);
			}
		}

		// Event ---------

		public event Action<AttendEventData> OnUpdateAttendEvent = delegate { };

		public void AddAttendData(AccountAttendInfo data)
		{
			if (dicAttendEventData.TryGetValue(data.GroupId, out var attendData))
			{
				attendData.Reset(data);
			}
			else
			{
				dicAttendEventData.Add(data.GroupId, new AttendEventData(data));
			}

			OnUpdateAttendEvent?.Invoke(dicAttendEventData[data.GroupId]);
		}

		public void AddAttendDataList(List<AccountAttendInfo> listData)
		{
			foreach (var iter in listData)
			{
				AddAttendData(iter);
			}
		}

		public bool GetAttendData(uint groupId, out AttendEventData data)
		{
			return dicAttendEventData.TryGetValue(groupId, out data);
		}

		public LoginEventData GetLoginEvent() => loginEventData;

		public void SetLoginEvent(LoginEventData data)
		{
			loginEventData = data;
		}

		public void ClearLoginEvent()
		{
			loginEventData = null;
		}

		// 접속시 갱신해줘야하는 이벤트 (유료출석이벤트)
		public AttendEventData GetAttendRefreshEventTarget()
		{
			foreach (var iter in dicAttendEventData.Values)
			{
				if (iter.subType != E_EVENT_ATTEND_TYPE.PAID_ATTEND)
					continue;

				if (iter.rewardDt < TimeHelper.GetTodayStartTime())
					return iter;
			}
			return null;
		}

		// --------- Event
	}

	/// <summary>
	/// 
	/// </summary>
	public class CharacterData
	{
		public uint ServerIdx { get; private set; }
		public ulong ID { get; private set; }
		public uint TID { get; private set; }
		public ulong UserID { get; private set; }
		public string Nickname { get; internal set; }
		public uint Level { get; private set; }
		public uint LastLevel { get; private set; }
		public ulong PreExp { get; private set; }
		public ulong Exp { get; private set; }
		public uint STR { get; private set; }
		public uint DEX { get; private set; }
		public uint INT { get; private set; }
		public uint VIT { get; private set; }
		public uint WIS { get; private set; }

		public uint LastStageTID { get; private set; }
		public uint LastPortalTID { get; private set; }

		public uint LastHp;     // 마지막 HP
		public uint LastMp;     // 마지막 MP
		public uint PkCnt;      //pk cnt
		public ulong last_exp;   // 최고 달성 경험치
		public uint LastArea;  // 마지막 접속 지역
		public uint LastPortalTid;     //마지막 포탈 지역
		public E_CharStateType State;   // 상태값
		public uint MainPet;    // 메인 펫 
		public uint PetEquip1;  // 장착된 공격형 펫
		public uint PetEquip2;  // 장착된 방어형 펫
		public uint PetEquip3;  // 장착된 유틸형 펫
		public ulong PetExpireDt; // 메인 펫 사용 만료시간
		public uint MainChange;    // 메인 변신체
		public uint MainAbilChange;    // 메인 어빌 적용용 변신체

		public uint MainVehicle; //장착된 메인 탈것
		public ulong VehicleEndCoolTime; //탑승 가능한 서버 시각 (초단위)

		public class SkillData
		{
			public uint SkillTid;
			public ulong EndCoolTimeMs;
			public ulong BuffEndTime;

			public SkillData(uint skillTid, ulong endCoolTimeMs, ulong buffEndTime)
			{
				SkillTid = skillTid;
				EndCoolTimeMs = TimeManager.NowMs + endCoolTimeMs * (ulong)TimeHelper.Unit_SecToMs;
				BuffEndTime = buffEndTime;
			}
		}

		public Dictionary<uint, SkillData> SkillDataDic { get; private set; } = new Dictionary<uint, SkillData>();

		public void SetSkillCoolTime(uint skillTid, ulong addCoolTime)
		{
			if (SkillDataDic.TryGetValue(skillTid, out SkillData data))
			{
				data.EndCoolTimeMs = TimeManager.NowMs + addCoolTime * (ulong)TimeHelper.Unit_SecToMs;
			}
			else
			{
				SkillDataDic.Add(skillTid, new SkillData(skillTid, addCoolTime, 0));
			}
		}

		public void SetSkillBuffEndTime(uint skillTid, ulong buffEndTime)
		{
			if (SkillDataDic.TryGetValue(skillTid, out SkillData data))
			{
				data.BuffEndTime = buffEndTime;
			}
			else
			{
				SkillDataDic.Add(skillTid, new SkillData(skillTid, 0, buffEndTime));
			}
		}

		public ulong GetSkillEndCoolTime(uint skillTid)
		{
			if (SkillDataDic.TryGetValue(skillTid, out SkillData data))
			{
				return data.EndCoolTimeMs;
			}

			return 0;
		}

		public ulong GetSkillBuffEndTime(uint skillTid)
		{
			if (SkillDataDic.TryGetValue(skillTid, out SkillData data))
			{
				return data.BuffEndTime;
			}

			return 0;
		}

		/// <summary> MainChange값과 현재 변신중인지 함께 체크한다 </summary>
		public uint CurrentMainChange
		{
			get
			{
				if (MainChange != 0 && ChangeExpireDt > TimeManager.NowSec)
				{
					return MainChange;
				}
				else
				{
					return 0;
				}
			}
		}

		public ulong ChangeExpireDt;  // 메인 변신체 사용 만료시간
									  //public E_BotFlag IsBot;    // 봇 여부 (0: 일반, 1: 봇)
		public byte AddRingSlot;  // 반지 슬롯 확장 개수
		public uint InvenMaxCnt;            // 인벤 확장 개수
		public uint QuickSlotMaxCnt;       // 퀵슬롯 확장 개수
		public List<uint> SpecialSkillTids = new List<uint>();     // 궁극기 TID 들
		public List<uint> GainSkillTids = new List<uint>();         //습득한 추가 스킬 tid들
		public int Tendency;    // 성향치
		public ulong CreateDt; // 케릭터 생성날짜
		public ulong DeleteDt; // 케릭터 삭제날짜
		public ulong LastLogoutDt; // 마지막 접속 시간
		public ulong DailyResetDt;    // 데일리 이벤트 마지막 갱신 시간
		public ulong WeekResetDt;  // 주간 이벤트 마지막 갱신 시간.
		public ulong MonthResetDt;     // 월간 이벤트 마지막 갱신 시간.
		public byte RestoreExpCnt;    // 경험치 무료 복구 횟수
		public byte RestoreDieBuffCnt;   // 사망 디버프 무료 복구 횟수
		public byte RestoreExpNotFreeCnt;   // 경험치 복구 유료 횟수
		public float LastPosX;     // 마지막 위치 x좌표
		public float LastPosY;     // 마지막 위치 y좌표
		public float LastPosZ;     // 마지막 위치 z좌표
		private ushort _lastChannelId;
		public ushort PrevChannelId;    // 이전 채널 아이디
		public ushort LastChannelId
		{
			get { return _lastChannelId; }
			set { PrevChannelId = _lastChannelId; _lastChannelId = value; }
		}    // 마지막 채널 아이디

		public uint ColosseumScore;                    // 콜로세움 점수
		public uint ColosseumRank;                    // 콜로세움 순위

		public bool IsColosseumSeasonReward;           // 콜로세움 보상 유무

		public uint ColosseumOldSeasonScore;        // 콜로세움 이전 시즌 점수
		public uint ColosseumOldSeasonRank;         // 콜로세움 이전 시즌 순위

		/// <summary>인스턴스 던전 클리어 마지막 스테이지 정보</summary>
		public uint InstanceDungeonStageTID;
		/// <summary>인스턴스 던전 클리어 횟수</summary>
		public uint InstanceDungeonClearCnt;

		public ulong GuildId;                       // 길드 아이디
		public string GuildName;                    // 길드 이름
		public byte GuildMarkTid;                   // 길드 마크
		public E_GuildMemberGrade GuildGrade;       // 길드 맴버 등급
		public ulong GuildExp;                      // 길드 경험치

		public ulong GuildChatId;                        // 연맹 채팅 번호
		public E_GuildAllianceChatGrade GuildChatGrade;  // 연맹 채팅 등급
		public E_GuildAllianceChatState GuildChatState;  // 연맹 채팅 상태

		public ulong Artifact_Pet;
		public ulong Artifact_Vehicle;

		public ulong ChatBlockExpireDt;

		public int SelectEquipSetNo = 1;

		public float ColosseumHPAutoPer = 1f;

		// Block Character
		public List<BlockCharacterData> BlockCharList { get; private set; } = new List<BlockCharacterData>();

		// Restore Exp
		private List<RestoreExpData> RestoreExpList = new List<RestoreExpData>();

		// Storage
		private List<ZItem> StorageList = new List<ZItem>();
		public ulong LastRefreshTime = 0;

		// Inventory
		public List<ZItem> InvenList = new List<ZItem>();
		private uint ShowInvenSlotCnt;

		// Limit
		private List<BuyLimitData> BuyLimitList = new List<BuyLimitData>();

		/// <summary> 장착중인 아이템 </summary>
		public class EquippedItemData
		{
			public ulong ItemId;
			public uint ItemTid;
		}

		/// <summary> 장착중인 아이템 </summary>
		public Dictionary<byte, EquippedItemData> EquippedItemList { get; private set; } = new Dictionary<byte, EquippedItemData>();

		// Mail
		public ulong MailRefreshTime, MessageRefreshTime, ExchangeMessageRefreshTime;
		public List<MailData> MailList = new List<MailData>();
		public List<MessageData> MessageList = new List<MessageData>();
		public List<MessageData> ExchangeMessageList = new List<MessageData>();

		// Cash Mail
		public ulong CashMailRefreshDt;
		public List<CashMailData> CashMailList = new List<CashMailData>();

		// Mark
		public Dictionary<GameDB.E_MarkAbleType, MarkData> MarkDic = new Dictionary<GameDB.E_MarkAbleType, MarkData>();

		// Collect
		public Dictionary<uint, CollectData> ItemCollectDic = new Dictionary<uint, CollectData>();
		public List<CollectData> ItemCollectCompleteList = new List<CollectData>();
		public Dictionary<uint, CollectData> petCollectDic = new Dictionary<uint, CollectData>();
		public List<CollectData> petCollectCompleteList = new List<CollectData>();
		public Dictionary<uint, CollectData> rideCollectDic = new Dictionary<uint, CollectData>();
		public List<CollectData> rideCollectCompleteList = new List<CollectData>();
		public Dictionary<uint, CollectData> changeCollectDic = new Dictionary<uint, CollectData>();
		public List<CollectData> changeCollectCompleteList = new List<CollectData>();

		// Change
		private List<ChangeData> ChangeList = new List<ChangeData>();

		// Pet
		private Dictionary<uint, PetData> PetDic = new Dictionary<uint, PetData>();
		private List<PetGachaKeepData> PetGachaKeepList = new List<PetGachaKeepData>();

		// Ride
		private Dictionary<uint, PetData> RideDic = new Dictionary<uint, PetData>();
		private List<RideGachaKeepData> RideGachaKeepData = new List<RideGachaKeepData>();

		// Rune
		public Dictionary<ulong, PetRuneData> RuneDic = new Dictionary<ulong, PetRuneData>();

		// Change Keep
		private List<ChangeGachaKeepData> ChangeGachaKeepList = new List<ChangeGachaKeepData>();

		// Guild
		private List<GuildBuffData> GuildbuffList = new List<GuildBuffData>();
		private Dictionary<ulong, GuildSimpleData> AllianceGuildDic = new Dictionary<ulong, GuildSimpleData>();
		private Dictionary<ulong, GuildSimpleData> EnemyGuildDic = new Dictionary<ulong, GuildSimpleData>();

		// Pet Adventure
		private List<PetAdvData> PetAdventureList = new List<PetAdvData>();

		// Option
		public Dictionary<E_CharacterOptionKey, OptionInfo> OptionInfoDic = new Dictionary<E_CharacterOptionKey, OptionInfo>();
		private List<uint> AutoPutList = new List<uint>();
		private List<uint> BookMarkPortalList = new List<uint>();
		private List<OptionEquipInfo> EquipSet1List = new List<OptionEquipInfo>();
		private List<OptionEquipInfo> EquipSet2List = new List<OptionEquipInfo>();
		private List<OptionEquipInfo> EquipSet3List = new List<OptionEquipInfo>();
		public Dictionary<int, QuickSlotInfo> QuickSlotSet1Dic = new Dictionary<int, QuickSlotInfo>();
		public Dictionary<int, QuickSlotInfo> QuickSlotSet2Dic = new Dictionary<int, QuickSlotInfo>();
		private Dictionary<System.ValueTuple<NetItemType, ulong>, uint> InvenSortList = new Dictionary<(NetItemType, ulong), uint>();
		public ChatFilter chatFilter = ChatFilter.TYPE_CHECK_DEFAULT;
		private Dictionary<int, QuickSlotInfo> ColosseumQuickSlotSet1Dic = new Dictionary<int, QuickSlotInfo>();
		private Dictionary<int, QuickSlotInfo> ColosseumQuickSlotSet2Dic = new Dictionary<int, QuickSlotInfo>();
		private List<uint> RuneDropSelectList = new List<uint>();

		// Quest
		private QuestData mainQuest = null;
		private List<QuestData> QuestList = new List<QuestData>();

		//groupid : listquestEvent
		private Dictionary<uint, List<QuestEventData>> dicQuestEventData = new Dictionary<uint, List<QuestEventData>>();

		// Friend
		public List<Friend> friendList { get; private set; } = new List<Friend>();
		public List<Friend> requestfriendList { get; private set; } = new List<Friend>();

		// Attribute (속성)
		public Dictionary<GameDB.E_UnitAttributeType, AttributeData> attributeDic = new Dictionary<GameDB.E_UnitAttributeType, AttributeData>();

		// Gain History
		public List<uint> ItemGainHistory = new List<uint>();
		public List<uint> CompleteArtifactDestinys = new List<uint>();

		// Make
		public List<MakeLimitData> MakeLimitList = new List<MakeLimitData>();

		// Artifact 
		// GroupID / ID 
		public Dictionary<uint, ArtifactData> ArtifactItemList = new Dictionary<uint, ArtifactData>();

		/// <summary> 강림 파견 </summary>        
		public List<ChangeQuestData> ChangeQuestDataList = new List<ChangeQuestData>();

		// Skill Use Order
		public List<SkillOrderData> SkillUseOrder = new List<SkillOrderData>();

		/// <summary> 컨테이너 리스트 컨테이너의 기능추가시 일괄처리등의 관리목적  </summary>        
		private List<ContainerBase> containerList = new List<ContainerBase>();

		/// <summary> 투기장 데이타  </summary>        
		public ColosseumContainer ColosseumContainer = new ColosseumContainer();

		/// <summary> 성지 데이타  </summary>        
		public GodLandContainer GodLandContainer = new GodLandContainer();

		/// <summary> 시련의 성역 데이터 </summary>
		public TrialSanctuaryContainer TrialSanctuaryContainer = new TrialSanctuaryContainer();

		/// <summary> 시련의 성역 데이터 </summary>
		public InfinityTowerContainer InfinityTowerContainer = new InfinityTowerContainer();

		/// <summary> 보스전 데이터 </summary>
		public BossWarContainer BossWarContainer = new BossWarContainer();

		/// <summary> 길드 던전 데이터 </summary>
		public GuildDungeonContainer GuildDungeonContainer = new GuildDungeonContainer();

		/// <summary> 유적 데이터  </summary>        
		public TempleInfoContainer TempleInfo = new TempleInfoContainer();

		/// <summary> 서버이벤트 데이터  </summary>        
		public ServerEventContainer ServerEventContainer = new ServerEventContainer();

		/// <summary>요리</summary>
		public List<CookData> CookRecipeList = new List<CookData>();

		/// <summary>귀환의 돌 사용 시간</summary>
		public ulong ReturnStoneUseTime { get; private set; } // 서버 요청으로 클라에서 자체적으로 체크

		/// <summary>획득 아이템 리스트</summary>
		public List<ZItem> NewGainItemList = new List<ZItem>();

		/// <param name="_newChar"></param>

		public CharacterData(ref Character _newChar)
		{
			Reset(ref _newChar);

			// 데이타 컨테이너 생성
			ZLog.Log(ZLogChannel.Default, $"@contianer 생성 {this.ID}");
			containerList.Add(ColosseumContainer);
			containerList.Add(GodLandContainer);
			containerList.Add(TrialSanctuaryContainer);
			containerList.Add(InfinityTowerContainer);
			containerList.Add(BossWarContainer);
			containerList.Add(TempleInfo);
			containerList.Add(ServerEventContainer);
			containerList.Add(GuildDungeonContainer);
		}

		public void Reset(ref Character _newChar)
		{
			this.UserID = _newChar.UserId;
			this.ID = _newChar.CharId;
			this.TID = _newChar.CharTid;
			this.Nickname = _newChar.Nick;
			this.Level = _newChar.Lv;
			this.LastLevel = _newChar.LastLv;
			this.Exp = _newChar.Exp;
			this.PreExp = _newChar.Exp;
			this.STR = _newChar.Str;
			this.DEX = _newChar.Dex;
			this.INT = _newChar.Intellect;
			this.VIT = _newChar.Vit;
			this.WIS = _newChar.Wis;

			this.LastStageTID = _newChar.LastArea;


			ZLog.Log(ZLogChannel.WebSocket, $"CharacterData.Reset() LastHp: {this.LastHp}, LastMp: {this.LastMp}");

			this.LastArea = _newChar.LastArea;
			this.LastPortalTid = _newChar.LastPortalTid;
			this.State = _newChar.State;
			this.MainPet = _newChar.MainPet;
			this.PetExpireDt = _newChar.PetExpireDt;
			this.MainChange = _newChar.MainChange;
			this.MainVehicle = _newChar.MainVehicle;
			this.ChangeExpireDt = _newChar.ChangeExpireDt;
			//this.IsBot = _newChar.IsBot;
			this.AddRingSlot = _newChar.AddRingSlot;
			this.InvenMaxCnt = _newChar.InvenMaxCnt;
			this.Tendency = _newChar.Tendency;
			this.CreateDt = _newChar.CreateDt;
			this.DeleteDt = _newChar.DeleteDt;
			this.LastLogoutDt = _newChar.LastLogoutDt;
			this.DailyResetDt = _newChar.DailyResetDt;
			this.WeekResetDt = _newChar.WeekResetDt;
			this.MonthResetDt = _newChar.MonthResetDt;
			this.RestoreExpCnt = _newChar.RestoreExpCnt;
			//this.RestoreDieBuffCnt = _newChar.RestoreDieBuffCnt;
			this.RestoreExpNotFreeCnt = _newChar.RestoreExpNotFreeCnt;
			this.LastPosX = _newChar.LastPosX;
			this.LastPosY = _newChar.LastPosY;
			this.LastPosZ = _newChar.LastPosZ;
			this.PrevChannelId = this.LastChannelId = _newChar.LastChannelId;

			this.QuickSlotMaxCnt = _newChar.QuickSlotMaxCnt;

			this.SpecialSkillTids.Clear();
			for (int i = 0; i < _newChar.SpecialSkillTidsLength; i++)
				if (_newChar.SpecialSkillTids(i) != 0)
					this.SpecialSkillTids.Add(_newChar.SpecialSkillTids(i));

			this.ColosseumScore = _newChar.ColosseumScore;
			this.ColosseumRank = _newChar.ColosseumRank;

			this.IsColosseumSeasonReward = _newChar.IsColosseumSeasonReward;

			this.ColosseumOldSeasonScore = _newChar.ColosseumOldSeasonScore;
			this.ColosseumOldSeasonRank = _newChar.ColosseumOldSeasonRank;

			this.InstanceDungeonStageTID = _newChar.InstanceDungeonStageTid;
			this.InstanceDungeonClearCnt = _newChar.InstanceDungeonCnt;

			this.GuildId = _newChar.GuildId;
			this.GuildName = _newChar.GuildName;
			this.GuildMarkTid = _newChar.GuildMarkTid;
			this.GuildGrade = _newChar.GuildGrade;
			this.GuildExp = _newChar.GuildExp;

			this.GuildChatId = _newChar.GuildChatId;
			this.GuildChatGrade = _newChar.GuildChatGrade;
			this.GuildChatState = _newChar.GuildChatState;

			this.Artifact_Pet = _newChar.ArtifactPet;
			this.Artifact_Vehicle = _newChar.ArtifactVehicle;

			this.ServerIdx = _newChar.ServerIdx;
			this.PkCnt = _newChar.PkCnt;

			//// 캐릭터 정보에 기본으로 포함되어옴
			//this.PartyID = charInfo.PartyUid;
		}

		public void ClearData()
		{
			ClearBlockCharacterList();
			ClearRestoreExpList();
			ClearStorageList();
			ClearInventoryList();
			//ClearMarkList();
			ClearItemCollect();
			ClearPetCollect();
			ClearRideCollect();
			ClearChangeCollect();
			ClearChangeList();
			ClearPetList();
			ClearPetKeepList();
			ClearRideKeepList();
			ClearGuildBuffList();
			ClearAllianceGuildList();
			//ClearGuildInfo();
			ClearGainSkills(); SetGainSkills(GainSkillTids);
			ClearPetAdventureList();
			ClearQuestList();
			ClearQuestEventList();
			ClearFriendList();
			ClearRequestFriendList();
			ClearRuneList();
			ClearItemGainHistory();
			ClearChangeQuestList();
			ClearAttribute();
			ClearArtifact();
			ClearEquippedItemList(); //장착중인 아이템 제거
			ClearSkillUseOrderList();

			// 데이타 컨테이너 삭제
			ZLog.Log(ZLogChannel.Default, $"@contianer 삭제 {this.ID}");
			for (int i = 0; i < containerList.Count; ++i)
			{
				containerList[i].Clear();
			}
			containerList.Clear();
		}

		/// <param name="_deleteDt">케릭터 삭제 시간</param>
		public void UpdateDeleteState(E_CharStateType _stateType, ulong _deleteDt)
		{

		}

		#region ====:: Default Information ::====

		public event Action<uint, uint> LevelUpdated;
		public event Action<ulong, ulong, bool> ExpUpdated;
		public event Action<CharacterData> UpdateBonusStat;
		/// <summary> 성향치 갱신시 호출되는 이벤트 (음수~양수) </summary>
		public event Action<int, int> TendencyUpdated;
		public event Action<uint> UpdateInvenMaxCount;
		public event Action UpdateQuickSlotMaxCnt;

		public void UpdateBonusStatList(List<UseStatPoint> stats)
		{
			//foreach (var stat in stats)
			//{
			//	switch (stat.Stat)
			//	{
			//		case (uint)GameDB.E_AbilityType.FINAL_STR: STR = stat.Cnt; break;
			//		case (uint)GameDB.E_AbilityType.FINAL_DEX: DEX = stat.Cnt; break;
			//		case (uint)GameDB.E_AbilityType.FINAL_INT: INT = stat.Cnt; break;
			//		case (uint)GameDB.E_AbilityType.FINAL_WIS: WIS = stat.Cnt; break;
			//		case (uint)GameDB.E_AbilityType.FINAL_VIT: VIT = stat.Cnt; break;
			//	}
			//}

			//UpdateBonusStat?.Invoke(Me.CurCharData);
		}

		public void ClearStatList()
		{
			//STR = 0;
			//DEX = 0;
			//INT = 0;
			//WIS = 0;
			//VIT = 0;
			//UpdateBonusStat?.Invoke(Me.CurCharData);
		}

		public void UpdateLevel(uint _newLevel)
		{
			if (this.Level == _newLevel)
				return;

			uint preLevel = Level;

			this.Level = _newLevel;

			if (this.Level > Me.CurUserData.MaxLevel)
				Me.CurUserData.SetMaxLevel(this.Level);

			LastLevel = Math.Max(LastLevel, Level);
			LevelUpdated?.Invoke(preLevel, _newLevel);
		}

		public void UpdateExp(ulong _newExp, bool isMosterKill)
		{
			if (this.Exp == _newExp)
				return;

			PreExp = Exp;

			this.Exp = _newExp;
			ExpUpdated?.Invoke(PreExp, _newExp, isMosterKill);
		}

		public void UpdateTendency(int _newTendency)
		{
			if (this.Tendency == _newTendency)
				return;

			int preTendency = Tendency;

			this.Tendency = _newTendency;
			TendencyUpdated?.Invoke(preTendency, _newTendency);
		}

		#endregion

		#region Block Character
		public void ClearBlockCharacterList()
		{
			if (BlockCharList != null)
				BlockCharList.Clear();
		}

		public BlockCharacterData GetBlockCharacter(ulong _blockCharacterId)
		{
			BlockCharacterData data = BlockCharList.Find(item => item.CharID == _blockCharacterId);

			if (data != null)
				return data;

			return null;
		}

		public BlockCharacterData GetBlockCharacter(string _blockNick)
		{
			BlockCharacterData data = BlockCharList.Find(item => item.Nick == _blockNick);

			if (data != null)
				return data;

			return null;
		}

		public List<BlockCharacterData> GetBlockCharacter() => BlockCharList;

		public void AddBlockCharacter(CharBlock? _blockChar)
		{
			BlockCharacterData data = GetBlockCharacter(_blockChar.Value.CharId);

			if (data != null)
			{
				data.Reset(_blockChar.Value);
				return;
			}

			BlockCharList.Add(new BlockCharacterData(_blockChar.Value));
		}

		public void RemoveBlockCharacter(ulong _blockCharacterId)
		{
			BlockCharacterData data = GetBlockCharacter(_blockCharacterId);

			if (data != null)
				BlockCharList.Remove(data);
		}

		public bool IsBlockUser(ulong _blockCharId)
		{
			BlockCharacterData data = GetBlockCharacter(_blockCharId);

			if (data != null)
				return true;

			return false;
		}

		public bool IsBlockUser(string _blockNick)
		{
			BlockCharacterData data = GetBlockCharacter(_blockNick);

			if (data != null)
				return true;
			return false;
		}
		#endregion

		#region Buff
		//public event Action<BuffData> BuffUpdated;
		//public event Action<uint> BuffDeleted;

		//public void ClearBuffList()
		//{
		//	if (BuffList != null)
		//		BuffList.Clear();

		//	BuffUpdated?.Invoke(null);
		//}

		//public void AddBuff(Buff? _buffInfo)
		//{
		//	BuffData data = GetBuffData(_buffInfo.Value.AbilityAcid);

		//	if (data == null)
		//	{
		//		BuffList.Add(new BuffData(_buffInfo.Value));
		//		data = GetBuffData(_buffInfo.Value.AbilityAcid);
		//	}
		//	else
		//		data.Reset(_buffInfo.Value);

		//	BuffUpdated?.Invoke(data);
		//}

		//public BuffData GetBuffData(uint _buffId)
		//{
		//	BuffData data = BuffList.Find(item => item.AbilityActionId == _buffId);

		//	if (data != null)
		//	{
		//		var tableData = DBAbility.GetAction(data.AbilityActionId);
		//		if (tableData.BuffSupportType == GameDB.E_BuffSupportType.RealTime && data.ExpireDt > TimeManager.NowSec)
		//			return data;
		//		else if (tableData.BuffSupportType == GameDB.E_BuffSupportType.GameTime && data.RemainSec > 0)
		//			return data;
		//	}

		//	return null;
		//}

		//public void RemoveBuff(uint _buffId)
		//{
		//	BuffData data = GetBuffData(_buffId);

		//	if (data != null)
		//	{
		//		BuffList.Remove(data);

		//		BuffDeleted?.Invoke(_buffId);
		//	}
		//}

		//public void RefreshBuff(List<Buff> _buffList)
		//{
		//	foreach (var buffInfo in _buffList)
		//	{
		//		BuffData data = BuffList.Find(item => item.AbilityActionId == buffInfo.AbilityAcid);
		//		bool bRefreshBuff = false;

		//		if (data != null)
		//		{
		//			if (!data.Equal(buffInfo))
		//			{
		//				data.Reset(buffInfo);
		//				bRefreshBuff = true;
		//			}
		//		}
		//		else
		//		{
		//			BuffList.Add(data = new BuffData(buffInfo));
		//			bRefreshBuff = true;
		//		}

		//		if (bRefreshBuff)
		//			BuffUpdated?.Invoke(data);
		//	}
		//}
		#endregion

		#region Restore Exp

		public event Action RestoreExpUpdated;

		public void ClearRestoreExpList()
		{
			if (RestoreExpList != null)
				RestoreExpList.Clear();

			RestoreExpUpdated?.Invoke();
		}

		public void AddRestoreExp(RestoreExp? _restoreInfo)
		{
			RestoreExpData data = RestoreExpList.Find(item => item.RestoreId == _restoreInfo.Value.RestoreId);

			if (data == null)
				RestoreExpList.Add(new RestoreExpData(_restoreInfo.Value));
			else
				data.Reset(_restoreInfo.Value);

			RestoreExpUpdated?.Invoke();
		}

		public List<RestoreExpData> GetRestoreExpData()
		{
			var data = RestoreExpList.FindAll(item => item.ExpireDt > TimeManager.NowSec);

			if (data != null)
				return data;

			return null;
		}

		public void RemoveRestoreExp(uint _restoreId)
		{
			RestoreExpData data = RestoreExpList.Find(item => item.RestoreId == _restoreId);

			if (data != null)
			{
				RestoreExpList.Remove(data);

				RestoreExpUpdated?.Invoke();
			}
		}

		public int GetRestoreExpCount()
		{
			if (RestoreExpList.Count > 0)
				return RestoreExpList.FindAll(item => item.ExpireDt > TimeManager.NowSec).Count;

			return 0;
		}

		public void UpdateRestoreExpCnt(byte cnt)
		{
			this.RestoreExpCnt = cnt;
		}

		public void UpdateRestoreExpNotFreeCnt(byte cnt)
		{
			this.RestoreExpNotFreeCnt = cnt;
		}
		#endregion

		#region Storage
		public void ClearStorageList()
		{
			if (StorageList != null)
				StorageList.Clear();
		}

		public void AddStorage(ItemEquipment? _addStorageItem)
		{
			StorageList.Add(new ZItem(_addStorageItem.Value));
		}

		public void AddStorage(ItemStack? _addStorageItem)
		{
			ZItem data = StorageList.Find(item => item.netType == NetItemType.TYPE_STACK && item.item_id == _addStorageItem.Value.ItemId);

			if (data != null)
				data.cnt = _addStorageItem.Value.Cnt;
			else
				StorageList.Add(new ZItem(_addStorageItem.Value));
		}

		public void AddStorage(List<ItemEquipment> _equipList)
		{
			foreach (ItemEquipment equip in _equipList)
				AddStorage(equip);
		}

		public void AddStorage(List<ItemStack> _stackList)
		{
			foreach (ItemStack stack in _stackList)
				AddStorage(stack);
		}

		public List<ZItem> GetStorageData()
		{
			if (StorageList == null)
				return null;

			return StorageList.FindAll(item => DBItem.IsEquipItem(item.item_tid) || (!DBItem.IsEquipItem(item.item_tid) && item.cnt > 0));
		}

		public void RemoveStorage(ulong _removeStorageId, NetItemType _removeStorageType, ulong _removeCnt)
		{
			ZItem data = StorageList.Find(item => item.item_id == _removeStorageId && item.netType == _removeStorageType);

			if (data != null)
			{
				switch (_removeStorageType)
				{
					case NetItemType.TYPE_EQUIP:
						StorageList.Remove(data);
						break;

					case NetItemType.TYPE_STACK:
						data.cnt -= _removeCnt;

						if (data.cnt <= 0)
							StorageList.Remove(data);
						break;
				}
			}
		}

		public void RemoveStorage(List<ItemEquipment> listEquip)
		{
			foreach (var equip in listEquip)
			{
				RemoveStorage(equip.ItemId, NetItemType.TYPE_EQUIP, 1);
			}
		}

		#endregion

		#region Inventory
		public event Action<uint> Itemupdate;
		public event Action<bool> InvenUpdate;
		public event Action<ulong, byte, uint, List<uint>> EquipChangeUpdate;
		public event Action<ulong, byte, uint> UnEquipChangeUpdate;

		public void ClearInventoryList()
		{
			if (InvenList != null)
			{
				InvenList.Clear();
				UpdateSlotCount();
			}
		}

		public uint GetAvailableInvenCount()
		{
			if (ShowInvenSlotCnt > InvenMaxCnt)
				return 0;
			return InvenMaxCnt - ShowInvenSlotCnt;
		}

		public bool IsFullInven()
		{
			if (InvenList.Count > 0)
				return ShowInvenSlotCnt >= InvenMaxCnt;
			return false;
		}

		public IList<ZItem> GetInvenItems()
		{
			if (InvenList.Count > 0)
				return InvenList.AsReadOnly();

			return null;
		}

		public IList<ZItem> GetShowInvenItems()
		{
			if (InvenList.Count > 0)
				return InvenList.FindAll(item => DBItem.IsShowInven(item.item_tid)&&item.cnt > 0).AsReadOnly();

			return null;
		}

		public ZItem GetItemData(ulong _itemId, NetItemType _itemType)
		{
			ZItem data = InvenList.Find(item => item.item_id == _itemId && item.netType == _itemType);

			if (data != null)
				return data;

			return null;
		}

		public ZItem GetItem(uint _itemTid, NetItemType _itemType)
		{
			ZItem data = InvenList.Find(item => item.item_tid == _itemTid && item.netType == _itemType);

			if (data != null)
				return data;

			return null;
		}

		public ZItem GetItem(uint _itemTid)
		{
			ZItem data = InvenList.Find(item => item.item_tid == _itemTid);

			if (data != null)
				return data;
			return null;
		}

		public void AddItemList(AccountItemStack? _item)
		{
			ZItem data = GetItemData(_item.Value.ItemId, NetItemType.TYPE_ACCOUNT_STACK);

			if (data != null)
			{
				data.Reset(_item.Value);
				if (data.cnt <= 0)
					InvenUpdate?.Invoke(true);
				else
					InvenUpdate?.Invoke(false);
			}
			else
			{
				InvenList.Add(data = new ZItem(_item.Value));
				InvenUpdate?.Invoke(true);
			}

			Itemupdate?.Invoke(data.item_tid);
			UpdateSlotCount();
		}

		public void AddItemList(List<AccountItemStack> listItem)
		{
			bool setCheck = false;
			foreach (var iter in listItem)
			{
				ZItem data = GetItemData(iter.ItemId, NetItemType.TYPE_ACCOUNT_STACK);

				if (data != null)
				{
					data.Reset(iter);
					if (data.cnt <= 0)
						setCheck = true;
				}
				else
				{
					InvenList.Add(data = new ZItem(iter));
					setCheck = true;
				}

				Itemupdate?.Invoke(iter.ItemTid);
			}

			InvenUpdate?.Invoke(setCheck);
			UpdateSlotCount();
		}

		public void AddItemList(MmoNet.AccountItemStack? _item)
		{
			ZItem data = GetItemData(_item.Value.ItemId, NetItemType.TYPE_ACCOUNT_STACK);

			if (data != null)
			{
				data.Reset(_item.Value);
				if (data.cnt <= 0)
					InvenUpdate?.Invoke(true);
				else
					InvenUpdate?.Invoke(false);
			}
			else
			{
				InvenList.Add(data = new ZItem(_item.Value));
				InvenUpdate?.Invoke(true);
			}

			Itemupdate?.Invoke(data.item_tid);
			UpdateSlotCount();
		}

		public void AddItemList(ItemStack? _item)
		{
			ZItem data = GetItemData(_item.Value.ItemId, NetItemType.TYPE_STACK);

			if (data != null)
			{
				data.Reset(_item.Value);
				if (data.cnt <= 0)
					InvenUpdate?.Invoke(true);
				else
                {
					NewGainItemList.Add(new ZItem(_item.Value));
					InvenUpdate?.Invoke(false);
				}
			}
			else
			{
				InvenList.Add(data = new ZItem(_item.Value));
				NewGainItemList.Add(new ZItem(_item.Value));
				InvenUpdate?.Invoke(true);
			}

			Itemupdate?.Invoke(data.item_tid);
			UpdateSlotCount();
		}

		public void AddItemList(MmoNet.ItemStack? _item)
		{
			ZItem data = GetItemData(_item.Value.ItemId, NetItemType.TYPE_STACK);

			if (data != null)
			{
				data.Reset(_item.Value);
				if (data.cnt <= 0)
					InvenUpdate?.Invoke(true);
				else
                {
					NewGainItemList.Add(new ZItem(_item.Value));
					InvenUpdate?.Invoke(false);
				}
			}
			else
			{
				InvenList.Add(data = new ZItem(_item.Value));
				NewGainItemList.Add(new ZItem(_item.Value));
				InvenUpdate?.Invoke(true);
			}

			Itemupdate?.Invoke(data.item_tid);
			UpdateSlotCount();
		}

		public void AddItemList(List<ItemStack> _listItem)
		{
			bool setCheck = false;
			foreach (var iter in _listItem)
			{
				ZItem data = GetItemData(iter.ItemId, NetItemType.TYPE_STACK);

				if (data != null)
				{
					data.Reset(iter);
					if (data.cnt <= 0)
						setCheck = true;
					else
						NewGainItemList.Add(new ZItem(iter));
				}
				else
				{
					InvenList.Add(data = new ZItem(iter));
					NewGainItemList.Add(new ZItem(iter));
					setCheck = true;
				}

				Itemupdate?.Invoke(data.item_tid);
			}

			InvenUpdate?.Invoke(setCheck);
			UpdateSlotCount();
		}

		public void AddItemList(WebNet.ItemEquipment? _item)
		{
			if (_item.HasValue == false) return;

			AddItemList(_item.Value);
			UpdateSlotCount();
		}

		public void AddItemList(List<WebNet.ItemEquipment> _item)
		{
			for (int i = 0; i < _item.Count; i++)
			{
				AddItemList(_item[i]);
			}

			UpdateSlotCount();
		}

		private void AddItemList(WebNet.ItemEquipment _item)
		{
			ZItem data = GetItemData(_item.ItemId, NetItemType.TYPE_EQUIP);
			byte savedSlot = 0;
			bool setCheck = false;
			
			if (data != null)
			{
				savedSlot = data.slot_idx;
				data.Reset(_item);
				if (data.cnt <= 0)
					setCheck = true;
			}
			else
			{
				InvenList.Add(data = new ZItem(_item));
				NewGainItemList.Add(new ZItem(_item));
				setCheck = true;
			}

			if (data.slot_idx != 0)
			{
				List<uint> tmpaddAbilitys = new List<uint>();

				if (EquipChangeUpdate != null)
				{
					tmpaddAbilitys.Clear();

					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_01));
					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_02));
					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_03));

					foreach (var gemTid in data.Sockets)
						if (gemTid != 0 && DBItem.GetItem(gemTid, out var gemData))
						{
							if (gemData.AbilityActionID_01 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_01);
							if (gemData.AbilityActionID_02 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_02);
							if (gemData.AbilityActionID_03 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_03);
						}

					tmpaddAbilitys.RemoveAll(tid => tid == 0);

					EquipChangeUpdate?.Invoke(data.item_id, data.slot_idx, data.item_tid, tmpaddAbilitys);
				}
			}
			else if (savedSlot != 0)
				UnEquipChangeUpdate?.Invoke(data.item_id, savedSlot, data.item_tid);

			Itemupdate?.Invoke(data.item_tid);
			InvenUpdate?.Invoke(setCheck);

			Me.AddGainItem(DBItem.GetGroupId(data.item_tid));
		}

		public void AddItemList(MmoNet.ItemEquipment? _item)
		{
			ZItem data = GetItemData(_item.Value.ItemId, NetItemType.TYPE_EQUIP);
			List<uint> tmpaddAbilitys = new List<uint>();
			byte savedSlot = 0;
			bool setCheck = false;

			if (data != null)
			{
				savedSlot = data.slot_idx;
				data.Reset(_item.Value);
				if (data.cnt <= 0)
					setCheck = true;
			}
			else
			{
				InvenList.Add(data = new ZItem(_item.Value));
				NewGainItemList.Add(new ZItem(_item.Value));
				setCheck = true;
			}

			Me.AddGainItem(DBItem.GetGroupId(_item.Value.ItemTid));

			if (_item.Value.SlotIdx != 0)
			{
				if (EquipChangeUpdate != null)
				{
					tmpaddAbilitys.Clear();

					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_01));
					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_02));
					tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(data.ResmeltOptionId_03));

					foreach (var gemTid in data.Sockets)
						if (gemTid != 0 && DBItem.GetItem(gemTid, out var gemData))
						{
							if (gemData.AbilityActionID_01 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_01);
							if (gemData.AbilityActionID_02 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_02);
							if (gemData.AbilityActionID_03 > 0) tmpaddAbilitys.Add(gemData.AbilityActionID_03);
						}

					tmpaddAbilitys.RemoveAll(tid => tid == 0);

					EquipChangeUpdate?.Invoke(_item.Value.ItemId, _item.Value.SlotIdx, _item.Value.ItemTid, tmpaddAbilitys);
				}
			}
			else if (savedSlot != 0)
				UnEquipChangeUpdate?.Invoke(_item.Value.ItemId, savedSlot, _item.Value.ItemTid);

			Itemupdate?.Invoke(_item.Value.ItemTid);
			InvenUpdate?.Invoke(setCheck);

			UpdateSlotCount();
		}

		public void RemoveItemList(ulong _removeStorageId, NetItemType _removeStorageType)
		{
			ZItem data = InvenList.Find(item => item.item_id == _removeStorageId && item.netType == _removeStorageType);

			if (data != null)
			{
				InvenList.Remove(data);
				Itemupdate?.Invoke(data.item_tid);

				switch (_removeStorageType)
				{
					case NetItemType.TYPE_EQUIP:
						if (data.slot_idx != 0)
							UnEquipChangeUpdate?.Invoke(data.item_id, data.slot_idx, data.item_tid);
						break;
				}
			}

			UpdateSlotCount();
			InvenUpdate?.Invoke(true);
		}

		public void RemoveItemList(List<ItemEquipment> removeList)
		{
			foreach (ItemEquipment item in removeList)
				RemoveItemList(item.ItemId, NetItemType.TYPE_EQUIP);
		}

		public void RemoveItemList(List<ItemStack> removeList)
		{
			foreach (ItemStack item in removeList)
				RemoveItemList(item.ItemId, NetItemType.TYPE_STACK);
		}

		public ZItem GetInvenItemUsingMaterial(uint materialTid)
		{
			ZItem matItem = InvenList.Find((item) => item.item_tid == materialTid);

			if (matItem == null)
				return null;

			if (matItem.netType == NetItemType.TYPE_EQUIP && matItem.slot_idx != 0)
				return null;

			if (matItem.IsLock)
				return null;

			return matItem;
		}

		public bool CheckCountInvenItemUsingMaterial(uint ItemTid, ulong CheckCount)
		{
			return GetInvenCntUsingMaterial(ItemTid) >= CheckCount;
		}

		//재료용이라 장착 중이거나 잠금 상태면 사용 불가
		public ulong GetInvenCntUsingMaterial(uint ItemTid)
		{
			ulong sumCount = 0;

			if (ItemTid == DBConfig.Exp_Item)
			{
				if (DBLevel.IsMaxLevel(DBCharacter.GetClassTypeByTid(TID), Level))
					sumCount = Exp - DBLevel.GetExp(DBCharacter.GetClassTypeByTid(TID), Level);
			}
			else
			{
				for (int i = 0; i < InvenList.Count; i++)
				{
					if (InvenList[i].item_tid == ItemTid)
					{
						if (InvenList[i].slot_idx != 0)
							continue;

						if (InvenList[i].IsLock)
							continue;

						sumCount += InvenList[i].cnt;
					}
				}
			}
			return sumCount;
		}

		//재료용이라 장착 중이거나 잠금 상태면 사용 불가
		public List<ZItem> GetAllInvenItemsUsingMaterial(uint ItemTid, ulong Count = 0)
		{
			List<ZItem> returnList = new List<ZItem>();
			ulong sumCount = 0;
			for (int i = 0; i < InvenList.Count; i++)
			{
				if (InvenList[i].item_tid == ItemTid)
				{
					if (InvenList[i].netType == NetItemType.TYPE_EQUIP && InvenList[i].slot_idx != 0)
						continue;

					if (InvenList[i].IsLock)
						continue;

					returnList.Add(InvenList[i]);
					sumCount += InvenList[i].cnt;

					if (Count != 0 && sumCount >= Count)
						break;
				}
			}
			return returnList;
		}

		public bool HasTargetItemExistInInventory(uint itemTid)
		{
			if (InvenList == null)
				return false;

			foreach (var inven in InvenList)
			{
				if(inven.item_tid == itemTid)
				{
					return true; 
				}
			}

			return false; 
		}

		public void UpdateSlotCount()
		{
			uint newInvenSlotCnt = 0;

			InvenList.ForEach((item) =>
			{
				if (DBItem.IsShowInven(item.item_tid))
				{
					if (item.netType == NetItemType.TYPE_EQUIP)
					{
						newInvenSlotCnt++;
					}
					else
					{
						if (item.cnt > 0)
							newInvenSlotCnt++;
					}
				}
			});

			ShowInvenSlotCnt = newInvenSlotCnt;
		}

		public void EquipItems(List<EquipResultInfo> equips)
		{
			List<uint> tmpaddAbilitys = new List<uint>();

			foreach (var equip in equips)
			{
				var findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_EQUIP && item.item_id == equip.ItemId);
				if (findItem != null)
				{
					findItem.SetEquipSlotIndex(equip.SlotIdx);

					//CheckCallItemUpdate(CharId, findItem.item_tid);

					if (UnEquipChangeUpdate != null)
					{
						tmpaddAbilitys.Clear();

						tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(findItem.ResmeltOptionId_01));
						tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(findItem.ResmeltOptionId_02));
						tmpaddAbilitys.Add(DBResmelting.GetResmeltingAbilityActionId(findItem.ResmeltOptionId_03));

						foreach (var gemTid in findItem.Sockets)
							if (gemTid != 0 && DBItem.GetItem(gemTid, out var gemData))
							{
								if (gemData.AbilityActionID_01 > 0)
									tmpaddAbilitys.Add(gemData.AbilityActionID_01);
								if (gemData.AbilityActionID_02 > 0)
									tmpaddAbilitys.Add(gemData.AbilityActionID_02);
								if (gemData.AbilityActionID_03 > 0)
									tmpaddAbilitys.Add(gemData.AbilityActionID_03);
							}

						tmpaddAbilitys.RemoveAll(tid => tid == 0);

						EquipChangeUpdate?.Invoke(findItem.item_id, findItem.slot_idx, findItem.item_tid, tmpaddAbilitys);
					}
				}
			}

			InvenUpdate?.Invoke(false);
		}

		public void UnEquipItem(ulong ItemID)
		{
			for (int i = 0; i < InvenList.Count; i++)
			{
				var itemData = InvenList[i];
				if (itemData.item_id == ItemID && itemData.netType == NetItemType.TYPE_EQUIP)
				{
					byte savedSlotIdx = itemData.slot_idx;
					itemData.SetEquipSlotIndex(0);

					InvenUpdate?.Invoke(false);
					UnEquipChangeUpdate?.Invoke(itemData.item_id, savedSlotIdx, itemData.item_tid);
				}
			}
		}

		public void UnEquipItems(List<ulong> ItemIDs)
		{
			foreach (var item in InvenList.FindAll(item => item.netType == NetItemType.TYPE_EQUIP && item.slot_idx != 0))
			{
				if (ItemIDs.Contains(item.item_id))
				{
					byte savedSlotIdx = item.slot_idx;
					item.SetEquipSlotIndex(0);

					//CheckCallItemUpdate(CharId, item.item_tid);

					UnEquipChangeUpdate?.Invoke(item.item_id, savedSlotIdx, item.item_tid);
				}
			}

			InvenUpdate?.Invoke(false);
		}

		public byte GetCharacterEquipPrioritySlot(uint _itemTid)
		{
			List<GameDB.E_EquipSlotType> equipslotTypes = DBItem.GetEquipSlots(_itemTid);

			if (equipslotTypes != null)
			{
				GameDB.E_CharacterType useCharType = DBItem.GetUseCharacterType(_itemTid);

				GameDB.E_CharacterType characterType = GameDB.E_CharacterType.None;

				// 장착할 때 직업 여부 상관 없이 장착이 가능하도록 기획이 변경되었으므로, 해당 예외처리는 주석
				/*if (MainChange != 0 && ChangeExpireDt > TimeManager.NowSec)
                {
                    characterType = DBChange.GetCharacterType(MainChange);
                }
                else
                {
                    characterType = DBCharacter.GetClassTypeByTid(TID);
                }

				if (characterType != GameDB.E_CharacterType.None)
                {
					
                    if (EnumHelper.CheckFlag(useCharType, characterType))
                    {*/
				uint groupIdx = DBItem.GetGroupId(_itemTid);
				List<ZItem> sameItemList = InvenList.FindAll(item => item.slot_idx != 0 && DBItem.GetGroupId(item.item_tid) == groupIdx);

				if (sameItemList.Count >= DBItem.GetSameEquipCount(_itemTid))
				{
					return sameItemList[0].slot_idx;
				}

				if (equipslotTypes.Contains(GameDB.E_EquipSlotType.Ring) || equipslotTypes.Contains(GameDB.E_EquipSlotType.Ring_2) || equipslotTypes.Contains(GameDB.E_EquipSlotType.Ring_3) || equipslotTypes.Contains(GameDB.E_EquipSlotType.Ring_4))
				{
					equipslotTypes = equipslotTypes.GetRange(0, AddRingSlot);
				}

				foreach (var type in equipslotTypes)
				{
					var findItem = InvenList.Find(item => item.slot_idx == (byte)type);
					if (findItem == null)
						return (byte)type;
				}

				return (byte)equipslotTypes[0];
				//}
				//}
			}

			return 0;
		}

		/// <summary>해당 슬롯에 장착중인 아이템 정보 리턴</summary>
		public ZItem GetEquipSlotItem(ulong _slotIdx)
		{
			int count = InvenList.Count;
			for (int i = 0; i < count; i++)
			{
				var item = InvenList[i];
				if (item.slot_idx == _slotIdx && item.netType == NetItemType.TYPE_EQUIP)
					return item;
			}
			return null;
		}

		public bool IsCharacterUsable(uint _itemTid)
		{
			/*
			 * 조건
			 */

			return true;
		}

		public bool IsCharacterEquipable(uint _itemTid)
		{
			if (DBItem.IsEquipItem(_itemTid) && DBItem.GetEquipSlot(_itemTid) != GameDB.E_EquipSlotType.None)
			{
				GameDB.E_CharacterType useCharType = DBItem.GetUseCharacterType(_itemTid);
				//강림중일시 강림한 강림체의 직업에 따라 장비 장착

				// 현재 강림중이다.
				if (Me.CurCharData.CurrentMainChange != 0)
				{
					// 강림중인데, 데이터가 없다면!? 리턴
					if (DBChange.TryGet(MainChange, out GameDB.Change_Table table) == false)
					{
						ZLog.LogError(ZLogChannel.Stat, "강림중이나, 강림 데이터가 없음!!");
						return false;
					}

					// 장비플래그에 강림체의 클래스가 존재하나
					if (EnumHelper.CheckFlag(useCharType, table.UseAttackType))
					{
						return true;
					}
				}
				else
				{
					if (EnumHelper.CheckFlag(useCharType, DBCharacter.GetClassTypeByTid(TID)))
					{
						return true;
					}
				}
			}

			return false;
		}

		public void RemoveItemList(List<DelItemInfo> delitems)
		{
			foreach (var delitem in delitems)
			{
				ZItem findItem = null;
				switch (DBItem.GetItemStackType(delitem.ItemTid))
				{
					case GameDB.E_ItemStackType.Not:
						findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_EQUIP && item.item_id == delitem.ItemId);
						break;
					case GameDB.E_ItemStackType.Stack:
						findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_STACK && item.item_id == delitem.ItemId);
						break;
					case GameDB.E_ItemStackType.AccountStack:
						findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_ACCOUNT_STACK && item.item_id == delitem.ItemId);
						break;
				}

				if (findItem != null)
				{
					if (findItem.cnt < delitem.ItemCnt)
					{
						ZLog.LogWarn(ZLogChannel.System, $"UserID: {UserID} CharID:{ID}, <<OverFlow Minus Count>> itemid : {findItem.item_id} , itemtid : {findItem.item_tid} , cnt : {findItem.cnt} , minuscnt : {delitem.ItemCnt} , Date : {System.DateTime.Now}");
						findItem.cnt = 0;
					}
					else
						findItem.cnt -= delitem.ItemCnt;

					if (DBItem.IsEquipItem(findItem.item_tid) || findItem.cnt <= 0)
					{
						InvenList.Remove(findItem);
						if (UIManager.Instance.Find(out UIFrameInventory _inventory)) _inventory.RemoveItem(findItem.item_id);
					}

					//CheckCallItemUpdate(CharId, delitem.ItemTid);

					//if (findItem.slot_idx != 0)
					//{
					//	if (OnUnEquipChangeUpdate.ContainsKey(CharId))
					//		OnUnEquipChangeUpdate[CharId]?.Invoke(findItem.item_id, findItem.slot_idx, findItem.item_tid);
					//}
				}
			}

			//invenList.Sort(SortInven);
			//UpdateWeight(CharId);
			//if (OnInvenUpdate.ContainsKey(CharId))
			InvenUpdate?.Invoke(true);
		}

		public void RemoveItem(ulong ItemId, uint ItemTid, ulong ItemCnt)
		{
			switch (DBItem.GetItemStackType(ItemTid))
			{
				case GameDB.E_ItemStackType.Not:
					RemoveEquipItem(ItemId);
					break;
				case GameDB.E_ItemStackType.Stack:
					RemoveStackItem(ItemId, ItemCnt);
					break;
				case GameDB.E_ItemStackType.AccountStack:
					RemoveAccountStackItem(ItemId, ItemCnt);
					break;
			}
		}

		public void RemoveItem(NetItemType itemType, ulong ItemId)
		{
			switch (itemType)
			{
				case NetItemType.TYPE_EQUIP:
					RemoveEquipItem(ItemId);
					break;
				case NetItemType.TYPE_STACK:
					RemoveStackItem(ItemId);
					break;
				case NetItemType.TYPE_ACCOUNT_STACK:
					RemoveAccountStackItem(ItemId);
					break;
			}
		}

		public string RemoveEquipItemCurrentSet(ulong ItemId)
		{
			switch (SelectEquipSetNo)
			{
				case 1:
					{
						OptionEquipInfo findItem = EquipSet1List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
							EquipSet1List.Remove(findItem);

						return GetEquipSetValue(1);
					}
				case 2:
					{
						OptionEquipInfo findItem = EquipSet2List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
						{
							//Log.Log("findItem remove : " + findItem.UniqueID);
							EquipSet2List.Remove(findItem);
						}

						return GetEquipSetValue(2);
					}
				case 3:
					{
						OptionEquipInfo findItem = EquipSet3List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
						{
							//Log.Log("findItem remove : " + findItem.UniqueID);
							EquipSet3List.Remove(findItem);
						}

						return GetEquipSetValue(3);
					}
			}

			return "";
		}

		public bool SetEquipPetChangeRide(uint tid, OptionEquipType equipType)
		{

			switch (SelectEquipSetNo)
			{
				case 1:
					EquipSet1List.RemoveAll(item => item.EquipType == equipType);
					EquipSet1List.Add(new OptionEquipInfo() { UniqueID = tid, EquipType = equipType });
					return true;
				case 2:
					EquipSet2List.RemoveAll(item => item.EquipType == equipType);
					EquipSet2List.Add(new OptionEquipInfo() { UniqueID = tid, EquipType = equipType });
					return true;
				case 3:
					EquipSet3List.RemoveAll(item => item.EquipType == equipType);
					EquipSet3List.Add(new OptionEquipInfo() { UniqueID = tid, EquipType = equipType });
					return true;
			}
			return false;
		}

		public List<OptionEquipInfo> GetEquipSetItemList(int SetNo)
		{
			switch (SetNo)
			{
				case 1:
					return EquipSet1List.FindAll(item => item.EquipType == OptionEquipType.TYPE_EQUIP && InvenList.Find(item2 => item2.netType == NetItemType.TYPE_EQUIP && item2.item_id == item.UniqueID) != null);
				case 2:
					return EquipSet2List.FindAll(item => item.EquipType == OptionEquipType.TYPE_EQUIP && InvenList.Find(item2 => item2.netType == NetItemType.TYPE_EQUIP && item2.item_id == item.UniqueID) != null);
				case 3:
					return EquipSet3List.FindAll(item => item.EquipType == OptionEquipType.TYPE_EQUIP && InvenList.Find(item2 => item2.netType == NetItemType.TYPE_EQUIP && item2.item_id == item.UniqueID) != null);
			}

			return null;
		}

		public List<ZItem> GetEquipItems()
		{
			return InvenList.FindAll(item => item.slot_idx != 0 && item.netType == NetItemType.TYPE_EQUIP); ;
		}

		public void RemoveStackItem(ulong itemId, ulong cnt = 0)
		{
			ZItem findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_STACK && item.item_id == itemId);

			if (findItem != null)
			{
				if (cnt > 0)
				{
					findItem.cnt = (ulong)Mathf.Max(0, findItem.cnt - cnt);
				}
				else
					InvenList.Remove(findItem);

				UpdateSlotCount();

				InvenUpdate?.Invoke(true);
			}
		}

		public void RemoveAccountStackItem(ulong itemId, ulong cnt = 0)
		{
			ZItem findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_ACCOUNT_STACK && item.item_id == itemId);

			if (findItem != null)
			{
				if (cnt > 0)
				{
					findItem.cnt = (ulong)Mathf.Max(0, findItem.cnt - cnt);
				}
				else
					InvenList.Remove(findItem);

				UpdateSlotCount();

				InvenUpdate?.Invoke(true);
			}
		}

		public void SetUseItemTime(uint ItemTid, ulong Time)
		{
			var itemList = InvenList;

			uint groupId = DBItem.GetGroupId(ItemTid);
			foreach (var item in itemList)
			{
				if (item.netType == NetItemType.TYPE_EQUIP)
					continue;

				if (DBItem.GetGroupId(item.item_tid) == groupId)
				{
					item.UseTime = Time;

					//if (!PlatformSpecific.IsUnityServer)
					//	ItemSlot.DelayCoolTime(DBItem.GetItemCoolTime(item.item_tid), DBItem.GetItemCoolTime(item.item_tid), item.item_tid);
				}
			}
		}

		public void RemoveEquipItem(ulong itemId)
		{
			ZItem findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_EQUIP && item.item_id == itemId);

			if (findItem != null)
			{
				InvenList.Remove(findItem);

				UpdateSlotCount();

				InvenUpdate?.Invoke(true);

				if (findItem.slot_idx != 0)
				{
					UnEquipChangeUpdate?.Invoke(findItem.item_id, findItem.slot_idx, findItem.item_tid);
				}
			}
		}

		public string GetEquipSetValue(int SetNo)
		{
			string returnValue = "";

			switch (SetNo)
			{
				case 1:
					{
						for (int i = 0; i < EquipSet1List.Count; i++)
						{
							if (string.IsNullOrEmpty(returnValue))
								returnValue = string.Format("{0}:{1}:{2}", EquipSet1List[i].UniqueID, EquipSet1List[i].SlotIdx, EquipSet1List[i].EquipType);
							else
								returnValue += string.Format(",{0}:{1}:{2}", EquipSet1List[i].UniqueID, EquipSet1List[i].SlotIdx, EquipSet1List[i].EquipType);
						}
					}
					break;
				case 2:
					{
						for (int i = 0; i < EquipSet2List.Count; i++)
						{
							if (string.IsNullOrEmpty(returnValue))
								returnValue = string.Format("{0}:{1}:{2}", EquipSet2List[i].UniqueID, EquipSet2List[i].SlotIdx, EquipSet2List[i].EquipType);
							else
								returnValue += string.Format(",{0}:{1}:{2}", EquipSet2List[i].UniqueID, EquipSet2List[i].SlotIdx, EquipSet2List[i].EquipType);
						}
					}
					break;
				case 3:
					{
						for (int i = 0; i < EquipSet3List.Count; i++)
						{
							if (string.IsNullOrEmpty(returnValue))
								returnValue = string.Format("{0}:{1}:{2}", EquipSet3List[i].UniqueID, EquipSet3List[i].SlotIdx, EquipSet3List[i].EquipType);
							else
								returnValue += string.Format(",{0}:{1}:{2}", EquipSet3List[i].UniqueID, EquipSet3List[i].SlotIdx, EquipSet3List[i].EquipType);
						}
					}
					break;
			}
			return returnValue;
		}

		public void AddItem(ItemEquipment equipItem)
		{
			List<ZItem> charInven = InvenList;

			ZItem findItem = charInven.Find(item => item.netType == NetItemType.TYPE_EQUIP && item.item_id == equipItem.ItemId);
			if (findItem != null)
			{
				findItem.Reset(equipItem);
				InvenUpdate?.Invoke(false);
			}
			else
			{
				charInven.Add(new ZItem(equipItem));
				InvenUpdate?.Invoke(true);
			}
			//charInven.Sort(SortInven);
			UpdateSlotCount();


			AddGainItem(DBItem.GetGroupId(equipItem.ItemTid));
		}

		public void AddItem(List<ItemEquipment> equipList)
		{
			foreach (var equip in equipList)
				AddItem(equip);
		}

		public void AddItem(ItemStack stackItem)
		{
			ZItem findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_STACK && item.item_id == stackItem.ItemId);
			if (findItem != null)
			{
				findItem.Reset(stackItem);
				InvenUpdate?.Invoke(false);
			}
			else
			{
				InvenList.Add(new ZItem(stackItem));
				InvenUpdate?.Invoke(true);
				//invenList.Sort(SortInven);
			}

			UpdateSlotCount();
		}

		public void AddItem(List<ItemStack> itemList)
		{
			foreach (var stack in itemList)
				AddItem(stack);
		}

		public void AddItem(AccountItemStack stackItem)
		{
			ZItem findItem = InvenList.Find(item => item.netType == NetItemType.TYPE_ACCOUNT_STACK && item.item_id == stackItem.ItemId);
			if (findItem != null)
			{
				findItem.Reset(stackItem);
				InvenUpdate?.Invoke(false);
			}
			else
			{
				InvenList.Add(new ZItem(stackItem));
				//invenList.Sort(SortInven);
				InvenUpdate?.Invoke(true);
			}

		}

		public List<ZItem> GetEquipSlotList()
		{
			List<ZItem> equipList = new List<ZItem>(InvenList.FindAll(item => item.slot_idx != 0));

			return equipList;
		}

		public void UpdateInvenMaxCnt(uint MaxCnt)
		{
			InvenMaxCnt = MaxCnt;

			UpdateInvenMaxCount?.Invoke(MaxCnt);
		}
		public void ClearBuyLimitInfo()
		{
			BuyLimitList.Clear();
		}

		public void AddBuyLimitInfo(List<BuyLimitInfo> listLimit)
		{
			foreach (var iter in listLimit)
			{
				var findItem = BuyLimitList.Find(item => item.ShopTid == iter.ShopTid);

				if (findItem != null)
					findItem.Reset(iter);
				else
					BuyLimitList.Add(new BuyLimitData(iter));
			}
		}

		public BuyLimitData GetBuyLimitInfo(uint shopTid)
		{
			return BuyLimitList.Find(t => t.ShopTid == shopTid);
		}

		public void UpdateItemCollect(uint CollectTid, uint SlotIdx, uint ItemTid)
		{
			if (ItemCollectDic.TryGetValue(CollectTid, out CollectData findCollect))
			{
				if (findCollect.UpdateMaterial(SlotIdx, ItemTid) && findCollect.curState == CollectState.STATE_COMPLETE)
				{
					if (!ItemCollectCompleteList.Contains(findCollect))
					{
						ItemCollectCompleteList.Add(findCollect);

						UpdateCompleteItemCollect?.Invoke(CollectTid);
					}
				}
			}
			else
			{
				ItemCollectDic.Add(CollectTid, findCollect = new CollectData(CollectionType.TYPE_ITEM, CollectTid, SlotIdx, ItemTid));

				if (findCollect.curState == CollectState.STATE_COMPLETE)
				{
					if (!ItemCollectCompleteList.Contains(findCollect))
					{
						ItemCollectCompleteList.Add(findCollect);

						UpdateCompleteItemCollect?.Invoke(CollectTid);
					}
				}
			}
		}
		#endregion

		#region Mail
		public ulong GetMailRefreshTime()
		{
			if (MailRefreshTime > 0)
				return MailRefreshTime;

			return 0;
		}

		public void SetMailRefreshTime(ulong RefreshTime)
		{
			MailRefreshTime = RefreshTime;
		}

		public void ClearMailList()
		{
			if (MailList != null)
				MailList.Clear();
		}

		public void AddMailList(List<MailInfo> mailinfos, List<MailInfo> accountmailinfos)
		{
			foreach (var mailinfo in mailinfos)
			{
				var findMail = MailList.Find(item => item.mailType == GameDB.E_MailReceiver.Character && item.MailIdx == mailinfo.MailIdx);

				if (findMail != null)
					findMail.Reset(GameDB.E_MailReceiver.Character, mailinfo);
				else
					MailList.Add(new MailData(GameDB.E_MailReceiver.Character, mailinfo));

			}

			foreach (var mailinfo in accountmailinfos)
			{
				var findMail = MailList.Find(item => item.mailType == GameDB.E_MailReceiver.Account && item.MailIdx == mailinfo.MailIdx);

				if (findMail != null)
					findMail.Reset(GameDB.E_MailReceiver.Account, mailinfo);
				else
					MailList.Add(new MailData(GameDB.E_MailReceiver.Account, mailinfo));
			}
		}

		public void RemoveMail(ulong _mailIdx)
		{
			MailData removeData = GetMailData(_mailIdx);
			if (removeData != null)
				MailList.Remove(removeData);
		}

		public void RemoveMailList(List<TakeMailInfo> removeMails)
		{
			foreach (var delMail in removeMails)
			{
				var findItem = MailList.Find(item => item.mailType == (GameDB.E_MailReceiver)delMail.MailKind && item.MailIdx == delMail.MailIdx);

				if (findItem != null)
					MailList.Remove(findItem);
			}
		}

		public IList<MailData> GetMailDatas()
		{
			if (MailList.Count > 0)
				return MailList.AsReadOnly();

			return null;
		}

		public MailData GetMailData(ulong _mailIdx)
		{
			if (MailList.Count > 0)
				return MailList.Find(item => item.MailIdx == _mailIdx);

			return null;
		}
		#endregion

		#region CashMail
		public void ClearCashMailList()
		{
			this.CashMailList.Clear();
		}

		public void AddCashMailList(List<CashMailInfo> mailInfo)
		{
			for (int i = 0; i < mailInfo.Count; i++)
			{
				var target = this.CashMailList.Find(t => t.MailIdx == mailInfo[i].MailIdx);

				if (target == null)
				{
					this.CashMailList.Add(new CashMailData(mailInfo[i].MailIdx, mailInfo[i].ShopTid));
				}
				else
				{
					target.Reset(mailInfo[i]);
				}
			}
		}

		public void SetCashMailRefreshDt(ulong dt)
		{
			this.CashMailRefreshDt = dt;
		}

		public void RemoveCashMailList(List<ulong> idxList)
		{
			foreach (var idx in idxList)
			{
				var targetRemoveItem = CashMailList.Find(t => t.MailIdx == idx);

				if (targetRemoveItem != null)
				{
					CashMailList.Remove(targetRemoveItem);
				}
			}
		}
		#endregion

		#region Message
		public ulong GetMessageRefreshTime()
		{
			return MessageRefreshTime;
		}

		public void SetMessageRefreshTime(ulong RefreshTime)
		{
			MessageRefreshTime = RefreshTime;
		}

		public void ClearMessageList()
		{
			MessageList.Clear();
		}

		public void RemoveMessage(ulong _messageIdx)
		{
			MessageData removeData = GetMessageData(_messageIdx);
			if (removeData != null)
				MessageList.Remove(removeData);
		}

		public void RemoveMessageList(List<ulong> _messageIdxs)
		{
			foreach (ulong MessageIdx in _messageIdxs)
			{
				var findItem = MessageList.Find(item => item.MessageIdx == MessageIdx);

				if (findItem != null)
					MessageList.Remove(findItem);
			}
		}

		public void AddMessage(MessageInfo _messageinfo)
		{
			MessageList.Add(new MessageData(_messageinfo));
		}

		public void AddMessageList(List<MessageInfo> _messageinfos)
		{
			foreach (var messageinfo in _messageinfos)
			{
				var findItem = MessageList.Find(item => item.MessageIdx == messageinfo.MessageIdx);

				if (findItem != null)
					findItem.Reset(messageinfo);
				else
					MessageList.Add(new MessageData(messageinfo));
			}
		}

		public IList<MessageData> GetMessageDatas()
		{
			return MessageList.AsReadOnly();
		}

		public MessageData GetMessageData(ulong _messageIdx)
		{
			return MessageList.Find(item => item.MessageIdx == _messageIdx);
		}

		public bool GetNotReadMessage()
		{
			var message = MessageList.Find(item => item.IsRead == 0);

			return message != null;
		}

		public void ReadMessage(ulong _messageIdx)
		{
			MessageData getMessage = GetMessageData(_messageIdx);
			if (getMessage != null)
				getMessage.IsRead = 1;
		}

		public bool IsUnreadMessage()
		{
			return MessageList.Find(item => item.IsRead == 0) != null;
		}
		#endregion

		#region ExchangeMessage
		public ulong GetExchangeMessageRefreshTime()
		{
			return MessageRefreshTime;
		}

		public void SetExchangeMessageRefreshTime(ulong RefreshTime)
		{
			ExchangeMessageRefreshTime = RefreshTime;
		}

		public void ClearExchangeMessageList()
		{
			ExchangeMessageList.Clear();
		}

		public void RemoveExchangeMessage(ulong ExchangeMessageIdx)
		{
			MessageData removeData = GetExchangeMessageData(ExchangeMessageIdx);
			if (removeData != null)
				ExchangeMessageList.Remove(removeData);
		}

		public void RemoveExchangeMessageList(List<ulong> ExchangeMessageIdxs)
		{
			foreach (ulong ExchangeMessageIdx in ExchangeMessageIdxs)
			{
				var findItem = ExchangeMessageList.Find(item => item.MessageIdx == ExchangeMessageIdx);

				if (findItem != null)
					ExchangeMessageList.Remove(findItem);
			}
		}

		public void AddExchangeMessage(MessageInfo ExchangeMessageinfo)
		{
			ExchangeMessageList.Add(new MessageData(ExchangeMessageinfo));
		}

		public void AddExchangeMessageList(List<MessageInfo> ExchangeMessageinfos)
		{
			foreach (var ExchangeMessageinfo in ExchangeMessageinfos)
			{
				var findItem = ExchangeMessageList.Find(item => item.MessageIdx == ExchangeMessageinfo.MessageIdx);

				if (findItem != null)
					findItem.Reset(ExchangeMessageinfo);
				else
					ExchangeMessageList.Add(findItem = new MessageData(ExchangeMessageinfo));
			}
		}

		public IList<MessageData> GetExchangeMessageDatas()
		{
			ExchangeMessageList.AsReadOnly();

			return null;
		}

		public MessageData GetExchangeMessageData(ulong ExchangeMessageIdx)
		{
			return ExchangeMessageList.Find(item => item.MessageIdx == ExchangeMessageIdx);
		}

		public void ReadExchangeMessage(ulong ExchangeMessageIdx)
		{
			MessageData getExchangeMessage = GetExchangeMessageData(ExchangeMessageIdx);
			if (getExchangeMessage != null)
				getExchangeMessage.IsRead = 1;
		}
		#endregion
		#region Mark
		public event Action<uint> MarkUpdate;

		public void ClearMarkDic()
		{
			if (MarkDic != null)
				MarkDic.Clear();
		}

		public void AddMarkTID(uint tid)
		{
			var data = DBMark.GetMarkData(tid);

			if (data == null)
				return;

			if (MarkDic.ContainsKey(data.MarkAbleType) == false)
			{
				MarkDic.Add(data.MarkAbleType, new MarkData());
			}

			MarkDic[data.MarkAbleType].MarkTid = tid;
			MarkDic[data.MarkAbleType].Step = data.Step;
			MarkUpdate?.Invoke(tid);
		}

		public Dictionary<GameDB.E_MarkAbleType, MarkData>.ValueCollection GetMarkDataList()
		{
			return MarkDic.Values;
		}

		public MarkData GetMarkDataByType(GameDB.E_MarkAbleType type)
		{
			if (MarkDic.ContainsKey(type) == false)
				return null;

			return MarkDic[type];
		}

		public uint GetMarkTidByType(GameDB.E_MarkAbleType type)
		{
			if (MarkDic.ContainsKey(type) == false)
				return 0;

			return MarkDic[type].MarkTid;
		}

		public byte GetMarkStep(GameDB.E_MarkAbleType type)
		{
			if (MarkDic.ContainsKey(type) == false)
				return 0;

			return MarkDic[type].Step;
		}

		public void RemoveMark(uint _markTId)
		{
			foreach (var mark in MarkDic)
			{
				if (mark.Value.MarkTid == _markTId)
				{
					MarkDic.Remove(mark.Key);
					MarkUpdate?.Invoke(_markTId);
					return;
				}
			}
		}

		/// <summary>
		///  내가 해당 문양을 보유중인지 
		/// </summary>
		public bool IsMarkObtained_ByID(uint tid)
		{
			var data = DBMark.GetMarkData(tid);

			if (data == null
				|| MarkDic.ContainsKey(data.MarkAbleType) == false)
				return false;

			return MarkDic[data.MarkAbleType].Step >= data.Step;
		}

		public bool IsMarkObtained_ByStep(GameDB.E_MarkAbleType type, byte step)
		{
			if (MarkDic.ContainsKey(type) == false)
			{
				return false;
			}

			return MarkDic[type].Step >= step;
		}

		public bool GetMyNextMarkTID(GameDB.E_MarkAbleType type, out uint result)
		{
			result = 0;

			if (MarkDic.ContainsKey(type) == false || IsMarkMaxStep(type))
			{
				return false;
			}

			byte targetStep = MarkDic[type].Step;

			if (targetStep + 1 > byte.MaxValue)
			{
				ZLog.LogError(ZLogChannel.Default, "this will occur overflow.");
			}

			targetStep++;
			result = DBMark.GetMarkTidByStep(type, targetStep);

			return true;
		}

		public bool IsMarkMinStep(GameDB.E_MarkAbleType type)
		{
			if (MarkDic.ContainsKey(type) == false)
				return false;

			var targetMinStep = DBMark.GetMarkTypeNormalMinStep(type);
			return MarkDic[type].Step == targetMinStep;
		}

		/// <summary>
		/// 나의 문양 레벨이 가장 마지막 레벨인지 
		/// </summary>
		public bool IsMarkMaxStep(GameDB.E_MarkAbleType type)
		{
			if (MarkDic.ContainsKey(type) == false)
				return false;

			var targetMaxStep = DBMark.GetMarkTypeNormalMaxStep(type);
			return MarkDic[type].Step == targetMaxStep;
		}
		#endregion

		#region Collect
		public event Action<uint> UpdateCompleteItemCollect;
		public event Action<uint> UpdateCompleteChangeCollect;
		public event Action<uint> UpdateCompletePetCollect;
		public event Action<uint> UpdateCompleteRideCollect;

		public void ClearItemCollect()
		{
			ItemCollectDic.Clear();
			ItemCollectCompleteList.Clear();
		}

		public void ClearPetCollect()
		{
			petCollectDic.Clear();
			petCollectCompleteList.Clear();
		}

		public void ClearChangeCollect()
		{
			changeCollectDic.Clear();
			changeCollectCompleteList.Clear();
		}

		public void ClearRideCollect()
		{
			rideCollectDic.Clear();
			rideCollectCompleteList.Clear();
		}

		public CollectData GetCollectData(uint _collectTid, CollectionType _collectType)
		{
			switch (_collectType)
			{
				case CollectionType.TYPE_ITEM:
					if (ItemCollectDic.ContainsKey(_collectTid))
						return ItemCollectDic[_collectTid];
					break;
				case CollectionType.TYPE_CHANGE:
					if (changeCollectDic.ContainsKey(_collectTid))
						return changeCollectDic[_collectTid];
					break;
				case CollectionType.TYPE_PET:
					if (petCollectDic.ContainsKey(_collectTid))
						return petCollectDic[_collectTid];
					break;
				case CollectionType.TYPE_RIDE:
					if (rideCollectDic.ContainsKey(_collectTid))
						return rideCollectDic[_collectTid];
					break;
			}

			return null;
		}

		public IList<CollectData> GetCompleteCollectItems(CollectionType _type)
		{
			switch (_type)
			{
				case CollectionType.TYPE_ITEM:
					if (ItemCollectCompleteList.Count > 0)
						return ItemCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_CHANGE:
					if (changeCollectCompleteList.Count > 0)
						return changeCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_PET:
					if (petCollectCompleteList.Count > 0)
						return petCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_RIDE:
					if (rideCollectCompleteList.Count > 0)
						return rideCollectCompleteList.AsReadOnly();
					break;
			}
			return null;
		}

		public IList<CollectData> GetInCompleteCollectItems(CollectionType _type)
		{
			switch (_type)
			{
				case CollectionType.TYPE_ITEM:
					if (ItemCollectCompleteList.Count > 0)
						return ItemCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_CHANGE:
					if (changeCollectCompleteList.Count > 0)
						return changeCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_PET:
					if (petCollectCompleteList.Count > 0)
						return petCollectCompleteList.AsReadOnly();
					break;
				case CollectionType.TYPE_RIDE:
					if (rideCollectCompleteList.Count > 0)
						return rideCollectCompleteList.AsReadOnly();
					break;

			}
			return null;
		}

		public void AddCollection(Collect? _collectInfo, CollectionType _collectType)
		{
			switch (_collectType)
			{
				case CollectionType.TYPE_ITEM:
					if (ItemCollectDic.TryGetValue(_collectInfo.Value.CollectTid, out CollectData collectItemData))
					{
						if (!collectItemData.Equal(_collectInfo.Value))
							collectItemData.Reset(CollectionType.TYPE_ITEM, _collectInfo.Value);
					}
					else
					{
						collectItemData = new CollectData(CollectionType.TYPE_ITEM, _collectInfo.Value);
						ItemCollectDic.Add(_collectInfo.Value.CollectTid, collectItemData);
					}

					if (collectItemData.curState == CollectState.STATE_COMPLETE)
					{
						if (!ItemCollectCompleteList.Contains(collectItemData))
						{
							ItemCollectCompleteList.Add(collectItemData);

							//UpdateCompleteItemCollect?.Invoke(collectItemData.CollectTid);
						}
					}
					break;
				case CollectionType.TYPE_CHANGE:
					if (!changeCollectDic.TryGetValue(_collectInfo.Value.CollectTid, out var collectChangeData))
						changeCollectDic.Add(_collectInfo.Value.CollectTid, collectChangeData = new CollectData(CollectionType.TYPE_CHANGE, _collectInfo.Value));
					else
						collectChangeData.Reset(CollectionType.TYPE_CHANGE, _collectInfo.Value);

					if (collectChangeData.curState == CollectState.STATE_COMPLETE)
					{
						if (!changeCollectCompleteList.Contains(collectChangeData))
						{
							changeCollectCompleteList.Add(collectChangeData);

							//UpdateCompleteChangeCollect?.Invoke(_collectInfo.Value.CollectTid);
						}
					}
					break;
				case CollectionType.TYPE_PET:
					if (!petCollectDic.TryGetValue(_collectInfo.Value.CollectTid, out var collectPetData))
						petCollectDic.Add(_collectInfo.Value.CollectTid, collectPetData = new CollectData(CollectionType.TYPE_PET, _collectInfo.Value));
					else
						collectPetData.Reset(CollectionType.TYPE_PET, _collectInfo.Value);

					if (collectPetData.curState == CollectState.STATE_COMPLETE)
					{
						if (!petCollectCompleteList.Contains(collectPetData))
						{
							petCollectCompleteList.Add(collectPetData);

							//UpdateCompletePetCollect?.Invoke(_collectInfo.Value.CollectTid);
						}
					}
					break;
				case CollectionType.TYPE_RIDE:
					if (!rideCollectDic.TryGetValue(_collectInfo.Value.CollectTid, out var collectRideData))
						rideCollectDic.Add(_collectInfo.Value.CollectTid, collectRideData = new CollectData(CollectionType.TYPE_RIDE, _collectInfo.Value));
					else
						collectRideData.Reset(CollectionType.TYPE_RIDE, _collectInfo.Value);

					if (collectRideData.curState == CollectState.STATE_COMPLETE)
					{
						if (!rideCollectCompleteList.Contains(collectRideData))
						{
							rideCollectCompleteList.Add(collectRideData);

							//UpdateCompleteRideCollect?.Invoke(_collectInfo.Value.CollectTid);
						}
					}
					break;
			}
		}

		public void UpdateChangeCollect(uint CollectTid, uint SlotIdx, uint ChangeTid)
		{
			if (changeCollectDic.TryGetValue(CollectTid, out var collectData))
			{
				if (collectData.UpdateMaterial(SlotIdx, ChangeTid))
				{
					if (collectData.curState == CollectState.STATE_COMPLETE)
					{
						if (!changeCollectCompleteList.Contains(collectData))
						{
							changeCollectCompleteList.Add(collectData);
							UpdateCompleteChangeCollect?.Invoke(CollectTid);
							//UIManager.ShowCompleteCollectChange(CollectTid);
						}
					}
				}
			}
			else
			{
				changeCollectDic.Add(CollectTid, collectData = new CollectData(CollectionType.TYPE_CHANGE, CollectTid, SlotIdx, ChangeTid));

				if (collectData.curState == CollectState.STATE_COMPLETE)
				{
					if (!changeCollectCompleteList.Contains(collectData))
					{
						changeCollectCompleteList.Add(collectData);
						UpdateCompleteChangeCollect?.Invoke(CollectTid);
						//UIManager.ShowCompleteCollectChange(CollectTid);
					}
				}
			}
		}

		public void UpdatePetCollect(uint CollectTid, uint SlotIdx, uint PetTid)
		{
			if (petCollectDic.TryGetValue(CollectTid, out var collectData))
			{
				if (collectData.UpdateMaterial(SlotIdx, PetTid))
				{
					if (collectData.curState == CollectState.STATE_COMPLETE)
					{
						if (!petCollectCompleteList.Contains(collectData))
						{
							petCollectCompleteList.Add(collectData);
							UpdateCompletePetCollect?.Invoke(CollectTid);
							//UIManager.ShowCompleteCollectPet(CollectTid);
						}
					}
				}
			}
			else
			{
				petCollectDic.Add(CollectTid, collectData = new CollectData(CollectionType.TYPE_PET, CollectTid, SlotIdx, PetTid));

				if (collectData.curState == CollectState.STATE_COMPLETE)
				{
					if (!petCollectCompleteList.Contains(collectData))
					{
						petCollectCompleteList.Add(collectData);
						UpdateCompletePetCollect?.Invoke(CollectTid);
						//UIManager.ShowCompleteCollectPet(CollectTid);
					}
				}
			}
		}

		public void UpdateRideCollect(uint CollectTid, uint SlotIdx, uint RideTid)
		{
			if (rideCollectDic.TryGetValue(CollectTid, out var collectData))
			{
				if (collectData.UpdateMaterial(SlotIdx, RideTid))
				{
					if (collectData.curState == CollectState.STATE_COMPLETE)
					{
						if (!rideCollectCompleteList.Contains(collectData))
						{
							rideCollectCompleteList.Add(collectData);
							UpdateCompleteRideCollect?.Invoke(CollectTid);
							//UIManager.ShowCompleteCollectPet(CollectTid);
						}
					}
				}
			}
			else
			{
				rideCollectDic.Add(CollectTid, collectData = new CollectData(CollectionType.TYPE_RIDE, CollectTid, SlotIdx, RideTid));

				if (collectData.curState == CollectState.STATE_COMPLETE)
				{
					if (!rideCollectCompleteList.Contains(collectData))
					{
						rideCollectCompleteList.Add(collectData);
						UpdateCompleteRideCollect?.Invoke(CollectTid);
						//UIManager.ShowCompleteCollectPet(CollectTid);
					}
				}
			}
		}



		public bool IsCompletePetCollection(uint collectTid)
		{
			return petCollectCompleteList.Find(collect => collect.CollectTid == collectTid) != null;
		}

		public bool IsCompleteRideCollection(uint collectTid)
		{
			return rideCollectCompleteList.Find(collect => collect.CollectTid == collectTid) != null;

		}

		#endregion

		#region Change
		public event Action<uint, ulong> OnChangeUpdate = delegate { };

		public void ClearChangeList()
		{
			if (ChangeList != null)
				ChangeList.Clear();
		}

		public void UpdateMainChange(uint _mainChange, ulong _expireDt)
		{
			MainChange = _mainChange;
			ChangeExpireDt = _expireDt;

			OnChangeUpdate?.Invoke(MainChange, ChangeExpireDt);
		}

		public List<ChangeData> GetChangeDataList() => ChangeList;

		public ChangeData GetChangeData(ulong _changeId)
		{
			ChangeData data = ChangeList.Find(item => item.ChangeId == _changeId);

			if (data != null)
				return data;

			return null;
		}

		public ChangeData GetChangeDataByTID(uint _changeTid)
		{
			ChangeData data = ChangeList.Find(item => item.ChangeTid == _changeTid);

			if (data != null)
				return data;

			return null;
		}

		public int GetChangeCount(uint _changeTid)
		{
			var targetChange = GetChangeDataByTID(_changeTid);

			if (targetChange == null)
				return 0;

			return (int)targetChange.Cnt;
		}

		public void AddChangeList(Change? _change)
		{
			// 캐싱 꼭해서 사용해야함. Flatbuffer의 property는 부를때마다 파싱함.
			ulong findChangeId = _change.Value.ChangeId;

			ChangeData data = GetChangeData(findChangeId);

			if (data != null)
				data.Reset(_change.Value);
			else
				ChangeList.Add(new ChangeData(_change.Value));

			CheckChangeCollect(_change.Value.ChangeTid);
		}

		public void AddChangeList(List<Change> listChange)
		{
			foreach (var iter in listChange)
			{
				AddChangeList(iter);
			}
		}

		#endregion

		#region Pet
		public delegate void OnPetUpdate(uint petId, ulong expireDt);
		//public event Action<uint, GameDB.E_PetType_02> OnPetEquipUpdate;

		/// <summary> [Value] PetTID, RuneID </summary>
		public event Action<uint, ulong> RuneEquipUpdate;
		public event Action<uint, ulong> RuneUnEquipUpdate;
		/// <summary> 여러개 한번에 작동할때 </summary>
		public event Action<uint, List<ulong>> RunesEquipUpdate;
		public event Action<uint, List<ulong>> RunesUnEquipUpdate;
		public event Action<uint, ulong> RuneEnchantUpdate;

		// RuneID
		public event Action<ulong> RuneRemoveUpdate = delegate { };

		public void ClearPetList()
		{
			if (PetDic != null)
				PetDic.Clear();
		}

		public void UpdateMainPet(uint petTid, ulong expireSec)
		{
			MainPet = petTid;
			PetExpireDt = expireSec;
		}

		public PetData GetPetData(uint _petTId)
		{
			if (DBPet.TryGet(_petTId, out var table) == false)
				return null;

			if (table.PetType == GameDB.E_PetType.Pet)
			{
				if (PetDic.TryGetValue(_petTId, out var petData))
					return petData;

			}
			else if (table.PetType == GameDB.E_PetType.Vehicle)
			{
				if (RideDic.TryGetValue(_petTId, out var petData))
					return petData;

			}
			return null;
		}

		public int GetPetCount(uint _petTid)
		{
			var targetPet = GetPetData(_petTid);

			if (targetPet == null)
				return 0;

			return (int)targetPet.Cnt;
		}

		public Dictionary<uint, PetData>.ValueCollection GetPetDataList() => PetDic.Values;

		public void AddPetList(Pet? _petInfo)
		{
			PetData data = GetPetData(_petInfo.Value.PetTid);

			if (data != null)
				data.Reset(_petInfo.Value);
			else
				PetDic.Add(_petInfo.Value.PetTid, new PetData(_petInfo.Value));

			CheckPetCollect(_petInfo.Value.PetTid);
		}

		public void AddPetList(List<Pet> listPet)
		{
			foreach (var iter in listPet)
			{
				AddPetList(iter);
			}
		}

		public void CheckChangeCollect(uint Tid)
		{
			List<uint> collectionTids = new List<uint>();

			var affectCollections = DBChangeCollect.GetAffectCollections(Tid);
			if (null != affectCollections)
			{
				foreach (uint collectionTid in affectCollections)
				{
					if (!changeCollectDic.ContainsKey(collectionTid))
					{
						collectionTids.Add(collectionTid);
						continue;
					}
					else
					{
						if (!changeCollectDic[collectionTid].MaterialTids.Contains(Tid))
							collectionTids.Add(collectionTid);
					}
				}
			}

			List<SendCollectData> sendList = new List<SendCollectData>();
			for (int i = 0; i < collectionTids.Count; i++)
			{
				foreach (int slot in DBChangeCollect.GetCollectionSlot(collectionTids[i], Tid))
				{
					sendList.Add(new SendCollectData() { CollectTid = collectionTids[i], Slot = (uint)slot });
				}
			}
			if (sendList.Count > 0)
				ZWebManager.Instance.WebGame.REQ_RegistChangeCollection(sendList.AsReadOnly(), GetChangeDataByTID(Tid).ChangeId, Tid, null);
		}


		public void CheckPetCollect(uint Tid)
		{
			List<uint> collectionTids = new List<uint>();

			var affectCollections = DBPetCollect.GetAffectPetCollections(Tid);
			if (null != affectCollections)
			{
				foreach (uint collectionTid in affectCollections)
				{
					if (!petCollectDic.ContainsKey(collectionTid))
					{
						collectionTids.Add(collectionTid);
						continue;
					}
					else
					{
						if (!petCollectDic[collectionTid].MaterialTids.Contains(Tid))
							collectionTids.Add(collectionTid);
					}
				}
			}

			List<SendCollectData> sendList = new List<SendCollectData>();
			for (int i = 0; i < collectionTids.Count; i++)
			{
				foreach (int slot in DBPetCollect.GetCollectionSlot(collectionTids[i], Tid))
				{
					sendList.Add(new SendCollectData() { CollectTid = collectionTids[i], Slot = (uint)slot });
				}
			}
			if (sendList.Count > 0)
				ZWebManager.Instance.WebGame.REQ_RegistPetCollection(sendList.AsReadOnly(), GetPetData(Tid).PetId, Tid, null);
		}

		public void CheckRideCollect(uint Tid)
		{
			List<uint> collectionTids = new List<uint>();

			var affectCollections = DBPetCollect.GetAffectRideCollections(Tid);
			if (null != affectCollections)
			{
				foreach (uint collectionTid in affectCollections)
				{
					if (!rideCollectDic.ContainsKey(collectionTid))
					{
						collectionTids.Add(collectionTid);
						continue;
					}
					else
					{
						if (!rideCollectDic[collectionTid].MaterialTids.Contains(Tid))
							collectionTids.Add(collectionTid);
					}
				}
			}

			List<SendCollectData> sendList = new List<SendCollectData>();
			for (int i = 0; i < collectionTids.Count; i++)
			{
				foreach (int slot in DBPetCollect.GetCollectionSlot(collectionTids[i], Tid))
				{
					sendList.Add(new SendCollectData() { CollectTid = collectionTids[i], Slot = (uint)slot });
				}
			}
			if (sendList.Count > 0)
				ZWebManager.Instance.WebGame.REQ_RegistPetCollection(sendList.AsReadOnly(), GetRideData(Tid).PetId, Tid, null);
		}


		/// <summary>현재 등록된 Pet리스트</summary>
		public List<PetData> GetEquipedPets()
		{
			List<PetData> equipedList = new List<PetData>();

			if (PetDic.TryGetValue(PetEquip1, out var equipedPetData1)) equipedList.Add(equipedPetData1);
			if (PetDic.TryGetValue(PetEquip2, out var equipedPetData2)) equipedList.Add(equipedPetData2);
			if (PetDic.TryGetValue(PetEquip3, out var equipedPetData3)) equipedList.Add(equipedPetData3);

			return equipedList;
		}

		public OptionEquipInfo GetEquipPetSet(int SetNo)
		{
			switch (SetNo)
			{
				case 1:
					return EquipSet1List.Find(item => item.EquipType == OptionEquipType.TYPE_PET && PetDic.ContainsKey((uint)item.UniqueID));
				case 2:
					return EquipSet2List.Find(item => item.EquipType == OptionEquipType.TYPE_PET && PetDic.ContainsKey((uint)item.UniqueID));
				case 3:
					return EquipSet3List.Find(item => item.EquipType == OptionEquipType.TYPE_PET && PetDic.ContainsKey((uint)item.UniqueID));
			}
			return null;
		}

		public OptionEquipInfo GetEquipChangeSet(int SetNo)
		{
			switch (SetNo)
			{
				case 1:
					return EquipSet1List.Find(item => item.EquipType == OptionEquipType.TYPE_CHANGE && GetChangeDataByTID((uint)item.UniqueID) != null);
				case 2:
					return EquipSet2List.Find(item => item.EquipType == OptionEquipType.TYPE_CHANGE && GetChangeDataByTID((uint)item.UniqueID) != null);
				case 3:
					return EquipSet3List.Find(item => item.EquipType == OptionEquipType.TYPE_CHANGE && GetChangeDataByTID((uint)item.UniqueID) != null);
			}
			return null;
		}

		public OptionEquipInfo GetEquipRideSet(int SetNo)
		{
			switch (SetNo)
			{
				case 1:
					return EquipSet1List.Find(item => item.EquipType == OptionEquipType.TYPE_RIDE && RideDic.ContainsKey((uint)item.UniqueID));
				case 2:
					return EquipSet2List.Find(item => item.EquipType == OptionEquipType.TYPE_RIDE && RideDic.ContainsKey((uint)item.UniqueID));
				case 3:
					return EquipSet3List.Find(item => item.EquipType == OptionEquipType.TYPE_RIDE && RideDic.ContainsKey((uint)item.UniqueID));
			}
			return null;
		}

		/// <summary> 펫에게 룬을 장착한다. </summary>
		public void EquipRune(uint _petTid, ulong _runeId)
		{
			PetData data = GetPetData(_petTid);

			if (null != data && false == data.EquippedRunes.Contains(_runeId))
				data.EquippedRunes.Add(_runeId);

			//룬 세트 어빌리티 갱신
			UpdateRuneSetAbility(_petTid);

			RuneEquipUpdate?.Invoke(_petTid, _runeId);
		}

		/// <summary> 펫에게 룬을 장착한다. </summary>
		public void EquipRunes(uint _petTid, List<ulong> _runeIds)
		{
			PetData petData = GetPetData(_petTid);

			foreach (var runeId in _runeIds)
				if (null != petData && false == petData.EquippedRunes.Contains(runeId))
					petData.EquippedRunes.Add(runeId);

			//룬 세트 어빌리티 갱신
			UpdateRuneSetAbility(_petTid);

			RunesEquipUpdate?.Invoke(_petTid, _runeIds);
		}

		/// <summary> 펫에게 룬을 해제한다. </summary>
		public void UnequipRune(uint _petTid, ulong _runeId)
		{
			PetData petData = GetPetData(_petTid);
			if (null != petData && petData.EquippedRunes.Contains(_runeId))
				petData.EquippedRunes.Remove(_runeId);

			//룬 세트 어빌리티 갱신
			UpdateRuneSetAbility(_petTid);

			RuneUnEquipUpdate?.Invoke(_petTid, _runeId);
		}

		/// <summary> 펫에게 룬을 해제한다. </summary>
		public void UnequipRunes(uint _petTid, List<ulong> _runeIds, bool _bUpdate)
		{
			PetData petData = GetPetData(_petTid);

			foreach (var runeId in _runeIds)
				if (null != petData && petData.EquippedRunes.Contains(runeId))
					petData.EquippedRunes.Remove(runeId);

			//룬 세트 어빌리티 갱신
			UpdateRuneSetAbility(_petTid);

			if (_bUpdate)
				RunesUnEquipUpdate?.Invoke(_petTid, _runeIds);
		}

		/// <summary> 펫에게 장착된 룬 리스트 </summary>
		public List<PetRuneData> GetEquipRuneList(uint _petTid)
		{
			PetData petData = GetPetData(_petTid);

			if (null == petData)
				return null;

			List<PetRuneData> runeList = new List<PetRuneData>(petData.EquippedRunes.Count);

			foreach (var runeId in petData.EquippedRunes)
				if (RuneDic.TryGetValue(runeId, out var foundRune))
					runeList.Add(foundRune);

			return runeList;
		}

		// 펫에게 장착된 룬 리스트, 딕셔너리버젼
		public Dictionary<GameDB.E_EquipSlotType, PetRuneData> GetEquipRuneDic(uint _petTid)
		{
			var dicEquipData = new Dictionary<GameDB.E_EquipSlotType, PetRuneData>();

			PetData petData = GetPetData(_petTid);

			if (petData != null)
			{
				foreach (var iter in petData.EquippedRunes)
				{
					if (RuneDic.TryGetValue(iter, out var rune))
						dicEquipData.Add(rune.SlotType, rune);
				}
			}
			return dicEquipData;
		}

		/// <summary> 펫의 해당 슬롯에 장착된 룬 </summary>
		public PetRuneData GetEquipRune(uint petTid, GameDB.E_EquipSlotType slotType)
		{
			PetData petData = GetPetData(petTid);

			if (null == petData)
				return null;

			foreach (var runeId in petData.EquippedRunes)
			{
				if (RuneDic.TryGetValue(runeId, out var foundRune))
					if (foundRune.SlotType == slotType)
						return foundRune;
			}

			return null;
		}

		/// <summary> 룬 세트 어빌리티 갱신 </summary>
		private void UpdateRuneSetAbility(uint _petTid)
		{
			PetData petData = GetPetData(_petTid);

			if (null != petData)
			{
				//룬 세트 어빌리티 제거
				petData.RuneSetAbilityActionIds.Clear();

				//적용된 세트 테이블과 몇 번 적용되야 하는지.
				var setTables = DBRune.GetAppliedSetOptionList(GetEquipRuneList(_petTid));

				foreach (var table in setTables)
					for (int i = 0; i < table.Value; ++i)
						petData.RuneSetAbilityActionIds.Add(table.Key.AbilityActionID);
			}
		}

		/// <summary> 발동한 룬 세트 어빌리티 id </summary>
		public IList<uint> GetRuneSetAbilityActionIds(uint petTid)
		{
			PetData petData = GetPetData(petTid);

			if (null != petData)
				return petData.RuneSetAbilityActionIds.AsReadOnly();

			return null;
		}

		/// <summary> 펫이 탐험중인지 </summary>
		public bool IsPetAV()
		{
			PetData petData = GetPetData(PetEquip1);

			if (null != petData && petData.AdvId > 0)
				return true;

			petData = GetPetData(PetEquip2);

			if (null != petData && petData.AdvId > 0)
				return true;

			petData = GetPetData(PetEquip3);

			if (null != petData && petData.AdvId > 0)
				return true;

			return false;
		}





		#endregion

		#region Vehicle
		public event Action<uint> OnVehicleUpdate = delegate { };

		public void UpdateMainVehicle(uint _mainVehicle)
		{
			MainVehicle = _mainVehicle;

			OnVehicleUpdate?.Invoke(MainVehicle);
		}

		public void ClearRideList()
		{
			if (RideDic != null)
				RideDic.Clear();
		}

		public PetData GetRideData(uint _rideTId)
		{
			if (RideDic.TryGetValue(_rideTId, out var rideData))
				return rideData;

			return null;
		}

		public Dictionary<uint, PetData>.ValueCollection GetRideDataList() => RideDic.Values;

		public void AddRideList(Pet? _petInfo)
		{
			PetData data = GetRideData(_petInfo.Value.PetTid);

			if (data != null)
				data.Reset(_petInfo.Value);
			else
				RideDic.Add(_petInfo.Value.PetTid, new PetData(_petInfo.Value));

			CheckRideCollect(_petInfo.Value.PetTid);
		}

		public void AddRideList(List<Pet> listRide)
		{
			foreach (var iter in listRide)
				AddRideList(iter);
		}

		#endregion

		#region Pet Keep
		public void ClearPetKeepList()
		{
			if (PetGachaKeepList != null)
				PetGachaKeepList.Clear();
		}

		public void AddPetKeepList(PetGachaKeep? _petKeepInfo)
		{
			PetGachaKeepData data = PetGachaKeepList.Find(item => item.Id == _petKeepInfo.Value.PetId);

			if (data != null)
				data.Reset(_petKeepInfo.Value);
			else
				PetGachaKeepList.Add(new PetGachaKeepData(_petKeepInfo.Value));
		}

		public void AddPetKeepList(List<PetGachaKeep> listKeep)
		{
			listKeep.ForEach(item => AddPetKeepList(item));
		}

		public void RemovePetKeep(ulong _petKeepId)
		{
			PetGachaKeepData data = PetGachaKeepList.Find(item => item.Id == _petKeepId);

			if (data != null)
				PetGachaKeepList.Remove(data);
		}

		public List<PetGachaKeepData> GetPetKeepList() => PetGachaKeepList;

		#endregion

		#region RideKeep

		public void ClearRideKeepList()
		{
			if (RideGachaKeepData != null)
				RideGachaKeepData.Clear();
		}

		public void AddRideKeepList(PetGachaKeep? _petKeepInfo)
		{
			RideGachaKeepData data = RideGachaKeepData.Find(item => item.Id == _petKeepInfo.Value.PetId);

			if (data != null)
				data.Reset(_petKeepInfo.Value);
			else
				RideGachaKeepData.Add(new RideGachaKeepData(_petKeepInfo.Value));
		}

		public void AddRideKeepList(List<PetGachaKeep> listKeep)
		{
			listKeep.ForEach(item => AddRideKeepList(item));
		}

		public void RemoveRideKeep(ulong _petKeepId)
		{
			RideGachaKeepData data = RideGachaKeepData.Find(item => item.Id == _petKeepId);

			if (data != null)
				RideGachaKeepData.Remove(data);
		}

		public List<RideGachaKeepData> GetRideKeepList() => RideGachaKeepData;

		#endregion

		#region Change Keep
		public void ClearChangeKeepList()
		{
			if (ChangeGachaKeepList != null)
				ChangeGachaKeepList.Clear();
		}

		public void AddChangeKeepList(ChangeGachaKeep? _changeKeepInfo)
		{
			ChangeGachaKeepData data = ChangeGachaKeepList.Find(item => item.Id == _changeKeepInfo.Value.ChangeId);

			if (data != null)
				data.Reset(_changeKeepInfo.Value);
			else
				ChangeGachaKeepList.Add(new ChangeGachaKeepData(_changeKeepInfo.Value));
		}

		public void AddChangeKeepList(List<ChangeGachaKeep> listKeepChange)
		{
			foreach (var iter in listKeepChange)
			{
				AddChangeKeepList(iter);
			}
		}

		public void RemoveChangeKeep(ulong _changeKeepId)
		{
			ChangeGachaKeepData data = ChangeGachaKeepList.Find(item => item.Id == _changeKeepId);

			if (data != null)
				ChangeGachaKeepList.Remove(data);

		}

		public List<ChangeGachaKeepData> GetChangeKeepDataList() => ChangeGachaKeepList;
		#endregion

		#region Guild Info 
		// 이전 GuildID, 지금 GuildID
		public event Action<ulong, ulong> OnGuildInfoUpdated;

		public void SetGuildInfo(GuildInfo? info)
		{
			if (info.HasValue == false)
			{
				return;
			}

			ulong preGuildID = GuildId;

			var value = info.Value;

			GuildId = value.GuildId;
			GuildName = value.Name;
			GuildExp = value.Exp;
			GuildChatGrade = value.ChatGrade;
			GuildMarkTid = value.MarkTid;
			GuildChatState = value.ChatState;
			GuildChatId = value.ChatId;

			OnGuildInfoUpdated?.Invoke(preGuildID, GuildId);
		}
		public void SetGuildInfo(ulong guildID, string guildName, byte guildMarkTid)
		{
			ulong preGuildID = GuildId;
			GuildId = guildID;
			GuildName = guildName;
			GuildMarkTid = guildMarkTid;
			OnGuildInfoUpdated?.Invoke(preGuildID, GuildId);
		}
		public void SetGuildInfo(ulong guildID)
		{
			ulong preGuildID = GuildId;
			GuildId = guildID;
			OnGuildInfoUpdated?.Invoke(preGuildID, GuildId);
		}
		public void SetGuildMarkInfo(byte guildMarkTid)
		{
			GuildMarkTid = guildMarkTid;
			OnGuildInfoUpdated?.Invoke(GuildId, GuildId);
		}
		public void SetGuildChatInfo(ulong chatID, E_GuildAllianceChatState state, E_GuildAllianceChatGrade grade)
		{
			GuildChatId = chatID;
			GuildChatState = state;
			GuildChatGrade = grade;
			OnGuildInfoUpdated?.Invoke(GuildId, GuildId);
		}

		public void ClearGuildInfo()
		{
			ulong preGuildID = GuildId;
			AllianceGuildDic.Clear();
			EnemyGuildDic.Clear();
			GuildbuffList.Clear();
			GuildId = 0;
			GuildName = string.Empty;
			GuildExp = 0;
			GuildGrade = E_GuildMemberGrade.None;
			GuildChatGrade = E_GuildAllianceChatGrade.None;
			GuildMarkTid = 0;
			GuildChatState = E_GuildAllianceChatState.None;
			GuildChatId = 0;
			OnGuildInfoUpdated?.Invoke(preGuildID, 0);
		}
		#endregion

		#region Guild Buff
		public event Action<GuildBuffData> UpdateGuildBuffList;
		public event Action<GuildBuffData> RemoveGuildBuffList;

		public void ClearGuildBuffList()
		{
			for (int i = 0; i < GuildbuffList.Count; i++)
			{
				RemoveGuildBuffList?.Invoke(GuildbuffList[i]);

				GuildbuffList.RemoveAt(i);
				i--;
			}
		}

		public GuildBuffData GetGuildBuff(uint _AbilityActionId)
		{
			GuildBuffData data = GuildbuffList.Find(item => item.AbilityActionId == _AbilityActionId);

			if (data != null)
				return data;

			return null;
		}

		public void RefreshGuildBuff(List<GuildBuff> _guildBuffList)
		{
			foreach (var guildbuffInfo in _guildBuffList)
			{
				var findBuff = GuildbuffList.Find(item => item.AbilityActionId == guildbuffInfo.AbilityId);
				bool bRefeshBuff = false;

				if (findBuff != null)
				{
					if (!findBuff.Equal(guildbuffInfo))
					{
						findBuff.Reset(guildbuffInfo);
						bRefeshBuff = true;
					}
				}
				else
				{
					findBuff = new GuildBuffData(guildbuffInfo);
					GuildbuffList.Add(findBuff);
					bRefeshBuff = true;
				}

				if (bRefeshBuff)
					UpdateGuildBuffList?.Invoke(findBuff);
			}
		}

		public void AddGuildBuff(GuildBuff? _guildBuffList)
		{
			GuildBuffData data = GuildbuffList.Find(item => item.AbilityActionId == _guildBuffList.Value.AbilityId);

			if (data != null)
				data.Reset(_guildBuffList.Value);
			else
				GuildbuffList.Add(data = new GuildBuffData(_guildBuffList.Value));

			UpdateGuildBuffList?.Invoke(data);
		}
		#endregion

		#region Guild Alliance
		public event Action UpdateAllianceGuild;
		public event Action UpdateEnemyGuild;

		public void ClearAllianceGuildList()
		{
			AllianceGuildDic.Clear();
			EnemyGuildDic.Clear();

			UpdateAllianceGuild?.Invoke();
			UpdateEnemyGuild?.Invoke();
		}

		public void AddAllianceGuild(GuildAllianceSimpleInfo? _simpleinfo)
		{
			bool bUpdateAlliacneGuild = false;
			bool bUpdateEnemyGuild = false;

			if (_simpleinfo.Value.State == E_GuildAllianceState.Alliance)
			{
				AllianceGuildDic.TryGetValue(_simpleinfo.Value.GuildId, out var findInfo);

				if (findInfo != null)
				{
					if (!findInfo.Equal(_simpleinfo.Value))
					{
						findInfo.Reset(_simpleinfo.Value);
						bUpdateAlliacneGuild = true;
					}
				}
				else
				{
					AllianceGuildDic.Add(_simpleinfo.Value.GuildId, new GuildSimpleData(_simpleinfo.Value));
					bUpdateAlliacneGuild = true;
				}
			}
			else if (_simpleinfo.Value.State == E_GuildAllianceState.Enemy)
			{
				EnemyGuildDic.TryGetValue(_simpleinfo.Value.GuildId, out var findInfo);

				if (findInfo != null)
				{
					if (!findInfo.Equal(_simpleinfo.Value))
					{
						findInfo.Reset(_simpleinfo.Value);
						bUpdateEnemyGuild = true;
					}
				}
				else
				{
					EnemyGuildDic.Add(_simpleinfo.Value.GuildId, new GuildSimpleData(_simpleinfo.Value));
					bUpdateEnemyGuild = true;
				}
			}

			if (bUpdateAlliacneGuild)
				UpdateAllianceGuild?.Invoke();

			if (bUpdateEnemyGuild)
				UpdateEnemyGuild?.Invoke();
		}

		public void AddAllianceGuild(GuildSimpleData simpleInfo)
		{
			Dictionary<ulong, GuildSimpleData> dic = null;
			bool notifyUpdate_alliance = false;
			bool notifyUpdate_enemy = false;

			if (simpleInfo.State == E_GuildAllianceState.Alliance)
			{
				dic = AllianceGuildDic;
			}
			else if (simpleInfo.State == E_GuildAllianceState.Enemy)
			{
				dic = EnemyGuildDic;
			}

			if (dic != null && dic.TryGetValue(simpleInfo.GuildId, out var data))
			{
				if (data == null)
				{
					if (simpleInfo.State == E_GuildAllianceState.Alliance)
						notifyUpdate_alliance = true;
					else notifyUpdate_enemy = true;

					dic.Add(simpleInfo.GuildId, new GuildSimpleData()
					{
						GuildId = simpleInfo.GuildId,
						State = simpleInfo.State
					});
				}
			}

			if (notifyUpdate_alliance)
			{
				UpdateAllianceGuild?.Invoke();
			}
			else if (notifyUpdate_enemy)
			{
				UpdateEnemyGuild?.Invoke();
			}
		}

		public int GetAllienceGuildCount() => AllianceGuildDic.Keys.Count;

		public void AddEnemyGuild(ulong GuildId)
		{
			if (!EnemyGuildDic.ContainsKey(GuildId))
			{
				EnemyGuildDic.Add(GuildId, new GuildSimpleData() { GuildId = GuildId, State = E_GuildAllianceState.Enemy });

				UpdateEnemyGuild?.Invoke();
			}
		}

		public void RemoveEnemyGuild(ulong GuildId)
		{
			if (EnemyGuildDic.ContainsKey(GuildId))
			{
				EnemyGuildDic.Remove(GuildId);

				UpdateEnemyGuild?.Invoke();
			}
		}

		public void AddAllianceGuild(ulong GuildId)
		{
			if (!AllianceGuildDic.ContainsKey(GuildId))
			{
				AllianceGuildDic.Add(GuildId, new GuildSimpleData() { GuildId = GuildId, State = E_GuildAllianceState.Alliance });

				UpdateAllianceGuild?.Invoke();
			}
		}

		public void RemoveAllianceGuild(ulong GuildId)
		{
			if (AllianceGuildDic.ContainsKey(GuildId))
			{
				AllianceGuildDic.Remove(GuildId);

				UpdateAllianceGuild?.Invoke();
			}
		}

		public bool IsAllienceGuild(ulong guildId)
		{
			return AllianceGuildDic.ContainsKey(guildId);
		}

		public bool IsEnemyGuild(ulong guildId)
		{
			return EnemyGuildDic.ContainsKey(guildId);
		}

		#endregion

		#region Gain Skill
		public event Action<List<uint>> UpdateGainSkills;

		public bool HasGainSkill(uint SkillTid)
		{
			if (GainSkillTids.Contains(SkillTid))
				return GainSkillTids.Contains(SkillTid);

			return false;
		}

		public List<uint> GetGainSkills()
		{
			if (GainSkillTids.Count > 0)
				return GainSkillTids;

			return null;
		}

		public void ClearGainSkills()
		{
			GainSkillTids.Clear();

			UpdateGainSkills?.Invoke(GainSkillTids);
		}

		public void AddGainSkill(uint _newGainSkill)
		{
			if (!GainSkillTids.Contains(_newGainSkill))
			{
				GainSkillTids.Add(_newGainSkill);

				UpdateGainSkills?.Invoke(new List<uint>() { _newGainSkill });
			}
		}

		public void SetGainSkills(List<uint> _newGainSkills)
		{
			GainSkillTids.Clear();
			GainSkillTids.AddRange(_newGainSkills);

			UpdateGainSkills?.Invoke(_newGainSkills);
		}
		#endregion

		#region Pet Adventure
		public void ClearPetAdventureList()
		{
			if (PetAdventureList != null)
				PetAdventureList.Clear();
		}

		public List<PetAdvData> GetPetAdventureList() => PetAdventureList;

		public void AddPetAdventure(PetAdv? _petAdvInfo)
		{
			PetAdvData data = PetAdventureList.Find(item => item.AdvId == _petAdvInfo.Value.AdvId);

			if (data != null)
				data.Reset(_petAdvInfo.Value);
			else
				PetAdventureList.Add(new PetAdvData(_petAdvInfo.Value));
		}

		public void AddPetAdventureList(List<PetAdv> listPetAdv)
		{
			foreach (var iter in listPetAdv)
				AddPetAdventure(iter);
		}

		public void RemovePetAdventure(ulong _petAdvId)
		{
			PetAdvData data = PetAdventureList.Find(item => item.AdvId == _petAdvId);

			if (data != null)
				PetAdventureList.Remove(data);

		}
		#endregion

		#region Option (Optoin-Common)
		public event Action ChatFilterUpdate;

		public OptionInfo GetOptionInfo(E_CharacterOptionKey GetKey)
		{
			if (OptionInfoDic.ContainsKey(GetKey))
				return OptionInfoDic[GetKey];

			return null;
		}

		public void AddOptionInfo(CharacterOption netOptionInfo)
		{
			if (!OptionInfoDic.ContainsKey((E_CharacterOptionKey)netOptionInfo.Key))
				OptionInfoDic.Add((E_CharacterOptionKey)netOptionInfo.Key, new OptionInfo() { OptionKey = (E_CharacterOptionKey)netOptionInfo.Key, OptionValue = netOptionInfo.Value });
			else
				OptionInfoDic[(E_CharacterOptionKey)netOptionInfo.Key].OptionValue = netOptionInfo.Value;

			ChangeOption((E_CharacterOptionKey)netOptionInfo.Key, netOptionInfo.Value);
		}

		public void AddOptionInfos(List<CharacterOption> netOptionInfos)
		{
			OptionInfoDic.Clear();

			foreach (var netOptionInfo in netOptionInfos)
			{
				OptionInfoDic.Add((E_CharacterOptionKey)netOptionInfo.Key, new OptionInfo() { OptionKey = (E_CharacterOptionKey)netOptionInfo.Key, OptionValue = netOptionInfo.Value });
				ChangeOption((E_CharacterOptionKey)netOptionInfo.Key, netOptionInfo.Value);
			}
		}

		public void RemoveOptionInfo(E_CharacterOptionKey RemoveKey)
		{
			if (OptionInfoDic.ContainsKey(RemoveKey))
				OptionInfoDic.Remove(RemoveKey);
		}

		public void ChangeOption(E_CharacterOptionKey Key, string Value)
		{
			switch (Key)
			{
				case E_CharacterOptionKey.Storage_Auto_Put:
					{
						AutoPutList.Clear();
						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
							AutoPutList.Add(uint.Parse(splitItem));
					}
					break;
				case E_CharacterOptionKey.QuickSlot_Set1:
					{
						List<int> slotList = new List<int>(QuickSlotSet1Dic.Keys);
						QuickSlotSet1Dic.Clear();

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strSlotInfos = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strSlotInfos.Length > 0 && int.TryParse(strSlotInfos[0], out int slot))
							{
								int Slot = slot;
								QuickSlotType slotType = (QuickSlotType)int.Parse(strSlotInfos[1]);
								uint TableID = uint.Parse(strSlotInfos[3]);

								if (slotType == QuickSlotType.TYPE_SKILL && !DBSkill.Has(TableID))
									continue;
								else if (slotType == QuickSlotType.TYPE_ITEM && !DBItem.Has(TableID))
									continue;

								if (!QuickSlotSet1Dic.ContainsKey(Slot))
									QuickSlotSet1Dic.Add(Slot, new QuickSlotInfo());

								var slotInfo = QuickSlotSet1Dic[Slot];
								slotInfo.SlotType = slotType;
								slotInfo.UniqueID = ulong.Parse(strSlotInfos[2]);
								slotInfo.TableID = TableID;
								slotInfo.bAuto = strSlotInfos.Length > 4 && bool.Parse(strSlotInfos[4]);

								QuickSlotUpdate?.Invoke(0, Slot);

								slotList.Remove(Slot);
							}
						}

						for (int i = 0; i < slotList.Count; i++)
							QuickSlotUpdate?.Invoke(0, slotList[i]);
					}
					break;
				case E_CharacterOptionKey.QuickSlot_Set2:
					{
						List<int> slotList = new List<int>(QuickSlotSet2Dic.Keys);
						QuickSlotSet2Dic.Clear();

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strSlotInfos = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strSlotInfos.Length > 0 && int.TryParse(strSlotInfos[0], out int slot))
							{
								int Slot = slot;

								QuickSlotType slotType = (QuickSlotType)int.Parse(strSlotInfos[1]);
								uint TableID = uint.Parse(strSlotInfos[3]);

								if (slotType == QuickSlotType.TYPE_SKILL && !DBSkill.Has(TableID))
									continue;
								else if (slotType == QuickSlotType.TYPE_ITEM && !DBItem.Has(TableID))
									continue;

								if (!QuickSlotSet2Dic.ContainsKey(Slot))
									QuickSlotSet2Dic.Add(Slot, new QuickSlotInfo());

								var slotInfo = QuickSlotSet2Dic[Slot];
								slotInfo.SlotType = slotType;
								slotInfo.UniqueID = ulong.Parse(strSlotInfos[2]);
								slotInfo.TableID = TableID;
								slotInfo.bAuto = strSlotInfos.Length > 4 && bool.Parse(strSlotInfos[4]);

								QuickSlotUpdate?.Invoke(1, Slot);

								slotList.Remove(Slot);
							}
						}

						for (int i = 0; i < slotList.Count; i++)
							QuickSlotUpdate?.Invoke(1, slotList[i]);
					}
					break;
				case E_CharacterOptionKey.EQUIP_SET1:
					{
						EquipSet1List.Clear();

						bool bEquipedChange = false;

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strequipInfo = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strequipInfo.Length > 0)
							{
								var equipInfo = new OptionEquipInfo() { UniqueID = ulong.Parse(strequipInfo[0]), SlotIdx = byte.Parse(strequipInfo[1]), EquipType = (OptionEquipType)System.Enum.Parse(typeof(OptionEquipType), strequipInfo[2]) };

								//check
								switch (equipInfo.EquipType)
								{
									case OptionEquipType.TYPE_EQUIP:
										{
											var findItem = InvenList.Find(item => item.item_id == equipInfo.UniqueID && item.netType == NetItemType.TYPE_EQUIP);
											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_CHANGE:
										{
											var findItem = GetChangeDataByTID((uint)equipInfo.UniqueID);

											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_PET:
										{
											PetDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;

									case OptionEquipType.TYPE_RIDE:
										{
											RideDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;
								}

								EquipSet1List.Add(equipInfo);
							}
						}
					}
					break;
				case E_CharacterOptionKey.EQUIP_SET2:
					{
						EquipSet2List.Clear();

						bool bEquipedChange = false;
						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strequipInfo = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strequipInfo.Length > 0)
							{
								var equipInfo = new OptionEquipInfo() { UniqueID = ulong.Parse(strequipInfo[0]), SlotIdx = byte.Parse(strequipInfo[1]), EquipType = (OptionEquipType)System.Enum.Parse(typeof(OptionEquipType), strequipInfo[2]) };
								//check
								switch (equipInfo.EquipType)
								{
									case OptionEquipType.TYPE_EQUIP:
										{
											var findItem = InvenList.Find(item => item.item_id == equipInfo.UniqueID && item.netType == NetItemType.TYPE_EQUIP);

											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_CHANGE:
										{
											var findItem = GetChangeDataByTID((uint)equipInfo.UniqueID);

											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_PET:
										{
											PetDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_RIDE:
										{
											RideDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;
								}

								EquipSet2List.Add(equipInfo);
							}
						}
					}
					break;
				case E_CharacterOptionKey.EQUIP_SET3:
					{
						EquipSet3List.Clear();

						bool bEquipedChange = false;
						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strequipInfo = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strequipInfo.Length > 0)
							{
								var equipInfo = new OptionEquipInfo() { UniqueID = ulong.Parse(strequipInfo[0]), SlotIdx = byte.Parse(strequipInfo[1]), EquipType = (OptionEquipType)System.Enum.Parse(typeof(OptionEquipType), strequipInfo[2]) };
								//check
								switch (equipInfo.EquipType)
								{
									case OptionEquipType.TYPE_EQUIP:
										{
											var findItem = InvenList.Find(item => item.item_id == equipInfo.UniqueID && item.netType == NetItemType.TYPE_EQUIP);

											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_CHANGE:
										{
											var findItem = GetChangeDataByTID((uint)equipInfo.UniqueID);

											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_PET:
										{
											PetDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;
									case OptionEquipType.TYPE_RIDE:
										{
											RideDic.TryGetValue((uint)equipInfo.UniqueID, out var findItem);
											if (findItem == null)
												continue;
										}
										break;
								}

								EquipSet3List.Add(equipInfo);
							}
						}
					}
					break;
				case E_CharacterOptionKey.EQUIP_SELECT_SET:
					{
						SelectEquipSetNo = string.IsNullOrEmpty(Value) ? 0 : int.Parse(Value);
					}
					break;
				case E_CharacterOptionKey.BOOK_MARK_PORTAL:
					{
						BookMarkPortalList.Clear();
						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							BookMarkPortalList.Add(uint.Parse(splitItem));
						}
					}
					break;
				case E_CharacterOptionKey.CHAT_CHANNEL:
					{
						chatFilter = string.IsNullOrEmpty(Value) ? ChatFilter.TYPE_ALL : (ChatFilter)int.Parse(Value);

						ChatFilterUpdate?.Invoke();
					}
					break;
				case E_CharacterOptionKey.INVEN_SORT_LIST:
					{
						InvenSortList.Clear();

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] splitEachData = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (System.Enum.TryParse<NetItemType>(splitEachData[0], out var type))
							{
								ulong key = ulong.Parse(splitEachData[1]);

								if (!InvenSortList.ContainsKey((type, key)))
									InvenSortList.Add((type, key), uint.Parse(splitEachData[2]));
							}
						}

						var invenList = InvenList;
						foreach (var item in invenList)
						{
							if (InvenSortList.ContainsKey((item.netType, item.item_id)))
							{
								item.SortIndex = InvenSortList[(item.netType, item.item_id)];
							}
						}
					}
					break;
				case E_CharacterOptionKey.Colosseum_QuickSlot_Set1:
					{
						QuickSlotSet1Dic.Clear();

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strSlotInfos = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strSlotInfos.Length > 0 && int.TryParse(strSlotInfos[0], out int slot))
							{
								int Slot = slot;

								if (!QuickSlotSet1Dic.ContainsKey(Slot))
									QuickSlotSet1Dic.Add(Slot, new QuickSlotInfo());

								var slotInfo = QuickSlotSet1Dic[Slot];
								slotInfo.SlotType = (QuickSlotType)int.Parse(strSlotInfos[1]);
								slotInfo.UniqueID = ulong.Parse(strSlotInfos[2]);
								slotInfo.TableID = uint.Parse(strSlotInfos[3]);
								slotInfo.bAuto = strSlotInfos.Length > 4 && bool.Parse(strSlotInfos[4]);
							}
						}
					}
					break;
				case E_CharacterOptionKey.Colosseum_QuickSlot_Set2:
					{
						QuickSlotSet2Dic.Clear();

						foreach (string splitItem in Value?.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
						{
							string[] strSlotInfos = splitItem.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
							if (strSlotInfos.Length > 0 && int.TryParse(strSlotInfos[0], out int slot))
							{
								int Slot = slot;

								if (!QuickSlotSet2Dic.ContainsKey(Slot))
									QuickSlotSet2Dic.Add(Slot, new QuickSlotInfo());

								var slotInfo = QuickSlotSet2Dic[Slot];
								slotInfo.SlotType = (QuickSlotType)int.Parse(strSlotInfos[1]);
								slotInfo.UniqueID = ulong.Parse(strSlotInfos[2]);
								slotInfo.TableID = uint.Parse(strSlotInfos[3]);
								slotInfo.bAuto = strSlotInfos.Length > 4 && bool.Parse(strSlotInfos[4]);
							}
						}
					}
					break;
				case E_CharacterOptionKey.Colosseum_HP_Auto_Per:
					{
						ColosseumHPAutoPer = string.IsNullOrEmpty(Value) ? 0 : float.Parse(Value);
					}
					break;
				case E_CharacterOptionKey.RUNE_DROP_SELECT:
					{
						var list = RuneDropSelectList;
						list.Clear();

						if (Value == "0")//default => all select
						{
							foreach (var type in (GameDB.E_RuneSetType[])Enum.GetValues(typeof(GameDB.E_RuneSetType)))
							{
								if (type == GameDB.E_RuneSetType.None)
									continue;
								list.Add((uint)type);
							}
						}
						else
							foreach (string splitItem in Value?.Split(','))
								if (System.Enum.TryParse<GameDB.E_RuneSetType>(splitItem, out var type))
									list.Add((uint)type);
					}
					break;
			}
		}

		public string AddEquipItemCurrentSet(ulong ItemId, byte Slot, OptionEquipType _EquipType = OptionEquipType.TYPE_EQUIP)
		{
			switch (SelectEquipSetNo)
			{
				case 1:
					{
						OptionEquipInfo findItem = EquipSet1List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
							findItem.SlotIdx = Slot;
						else
							EquipSet1List.Add(new OptionEquipInfo() { UniqueID = ItemId, SlotIdx = Slot, EquipType = _EquipType });

						return GetEquipSetValue(1);
					}
				case 2:
					{
						OptionEquipInfo findItem = EquipSet2List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
							findItem.SlotIdx = Slot;
						else
							EquipSet2List.Add(new OptionEquipInfo() { UniqueID = ItemId, SlotIdx = Slot, EquipType = _EquipType });

						return GetEquipSetValue(2);
					}
				case 3:
					{
						OptionEquipInfo findItem = EquipSet3List.Find(item => item.UniqueID == ItemId);
						if (findItem != null)
							findItem.SlotIdx = Slot;
						else
							EquipSet3List.Add(new OptionEquipInfo() { UniqueID = ItemId, SlotIdx = Slot, EquipType = _EquipType });

						return GetEquipSetValue(3);
					}
			}

			return "";
		}

		public QuickSlotInfo GetQuickSlotInfo(int Set, int Slot)
		{
			switch (Set)
			{
				case 0:
					{
						if (!QuickSlotSet1Dic.ContainsKey(Slot))
							QuickSlotSet1Dic.Add(Slot, new QuickSlotInfo());

						return QuickSlotSet1Dic[Slot];
					}
				case 1:
					{
						if (!QuickSlotSet2Dic.ContainsKey(Slot))
							QuickSlotSet2Dic.Add(Slot, new QuickSlotInfo());

						return QuickSlotSet2Dic[Slot];
					}
			}

			return null;
		}

		public void RemoveQuickSlotData(int Set, int Slot)
		{
			switch (Set)
			{
				case 0:
					if (QuickSlotSet1Dic.ContainsKey(Slot))
					{
						QuickSlotSet1Dic.Remove(Slot);
					}
					break;
				case 1:
					if (QuickSlotSet2Dic.ContainsKey(Slot))
					{
						QuickSlotSet2Dic.Remove(Slot);
					}
					break;
			}
		}

		public void RemoveAllQuickSlotData()
		{
			QuickSlotSet1Dic.Clear();
			QuickSlotSet2Dic.Clear();
		}

		public Dictionary<int, QuickSlotInfo> UpdateQuickSlotItem(int Set, int Slot, QuickSlotType slotType, ulong UniqueID, uint TableID, bool bAuto)
		{
			switch (Set)
			{
				case 0:
					{
						if (!QuickSlotSet1Dic.ContainsKey(Slot))
							QuickSlotSet1Dic.Add(Slot, new QuickSlotInfo());

						QuickSlotSet1Dic[Slot].SlotType = slotType;
						QuickSlotSet1Dic[Slot].UniqueID = UniqueID;
						QuickSlotSet1Dic[Slot].TableID = TableID;
						QuickSlotSet1Dic[Slot].bAuto = bAuto;

						QuickSlotUpdate?.Invoke(Set, Slot);

						return QuickSlotSet1Dic;
					}
				case 1:
					{
						if (!QuickSlotSet2Dic.ContainsKey(Slot))
							QuickSlotSet2Dic.Add(Slot, new QuickSlotInfo());

						QuickSlotSet2Dic[Slot].SlotType = slotType;
						QuickSlotSet2Dic[Slot].UniqueID = UniqueID;
						QuickSlotSet2Dic[Slot].TableID = TableID;
						QuickSlotSet2Dic[Slot].bAuto = bAuto;

						QuickSlotUpdate?.Invoke(Set, Slot);

						return QuickSlotSet2Dic;
					}
			}

			return null;
		}

		public string GetQuickSlotValue(int Set)
		{
			string returnValue = "";

			switch (Set)
			{
				case 0:
					{
						foreach (int Slot in QuickSlotSet1Dic.Keys)
						{
							if (string.IsNullOrEmpty(returnValue))
								returnValue = Slot.ToString() + ":" + ((int)QuickSlotSet1Dic[Slot].SlotType).ToString() + ":" + QuickSlotSet1Dic[Slot].UniqueID.ToString() + ":" + QuickSlotSet1Dic[Slot].TableID.ToString() + ":" + QuickSlotSet1Dic[Slot].bAuto;
							else
								returnValue += "," + Slot.ToString() + ":" + ((int)QuickSlotSet1Dic[Slot].SlotType).ToString() + ":" + QuickSlotSet1Dic[Slot].UniqueID.ToString() + ":" + QuickSlotSet1Dic[Slot].TableID.ToString() + ":" + QuickSlotSet1Dic[Slot].bAuto;
						}
					}
					break;
				case 1:
					{
						foreach (int Slot in QuickSlotSet2Dic.Keys)
						{
							if (string.IsNullOrEmpty(returnValue))
								returnValue = Slot.ToString() + ":" + ((int)QuickSlotSet2Dic[Slot].SlotType).ToString() + ":" + QuickSlotSet2Dic[Slot].UniqueID.ToString() + ":" + QuickSlotSet2Dic[Slot].TableID.ToString() + ":" + QuickSlotSet2Dic[Slot].bAuto;
							else
								returnValue += "," + Slot.ToString() + ":" + ((int)QuickSlotSet2Dic[Slot].SlotType).ToString() + ":" + QuickSlotSet2Dic[Slot].UniqueID.ToString() + ":" + QuickSlotSet2Dic[Slot].TableID.ToString() + ":" + QuickSlotSet2Dic[Slot].bAuto;
						}
					}
					break;
			}

			return returnValue;
		}

		public void SetQuickSlotCount(uint MaxCnt)
		{
			QuickSlotMaxCnt = MaxCnt;

			UpdateQuickSlotMaxCnt?.Invoke();
		}
		#endregion

		#region Option-InvenSort
		public string ChangeInvenSortListValue(List<ScrollInvenData> sortedList)
		{
			string sortValue = "";

			for (int i = 0; i < sortedList.Count; i++)
			{
				if (sortedList[i].Item != null)
				{
					if (string.IsNullOrEmpty(sortValue))
						sortValue = string.Format("{0}:{1}:{2}", sortedList[i].Item.netType.ToString(), sortedList[i].Item.item_id, (i + 1));
					else
						sortValue = string.Format("{0},{1}:{2}:{3}", sortValue, sortedList[i].Item.netType.ToString(), sortedList[i].Item.item_id, (i + 1));
				}
			}
			return sortValue;
		}

		public Dictionary<(NetItemType, ulong), uint> GetInvenSortList()
		{
			return InvenSortList;
		}

		public uint GetInvenSortValue(NetItemType type, ulong ItemId)
		{
			if (InvenSortList.ContainsKey((type, ItemId)))
				return InvenSortList[(type, ItemId)];

			return 0;
		}
		#endregion

		#region Quest
		public event Action<QuestData> QuestUpdate;
		public event Action QuestEventUpdate;
		public event Action<int, int> QuickSlotUpdate;
		public event Action<uint, uint> MainQuestChanged;

		public void ClearQuestList()
		{
			if (QuestList != null)
				QuestList.Clear();
		}

		// group==0, clearall
		public void ClearQuestEventList(uint groupId = 0)
		{
			if (groupId == 0)
			{
				dicQuestEventData.Clear();
			}
			else
			{
				if (dicQuestEventData.TryGetValue(groupId, out var list))
				{
					list.Clear();
				}
			}
		}

		public QuestData GetQuest(uint _questTid)
		{
			return QuestList.Find(item => item.QuestTid == _questTid);
		}

		public void NotifyQuestAll()
		{
			if (QuestUpdate != null)
			{
				for (int i = 0; i < QuestList.Count; i++)
				{
					QuestUpdate(QuestList[i]);
				}
			}
		}

		public void AddQuestList(Quest? _questInfo, bool _newQuest)
		{
			QuestData data = QuestList.Find((QuestData _data) =>
			{
				return _data.QuestId == _questInfo.Value.QuestId;
			});

			if (data != null)
			{
				data.Reset(_questInfo.Value);
			}
			else
			{
				data = new QuestData(_questInfo.Value);
				QuestList.Add(data);
			}
			data.NewQuest = _newQuest;

			var type = DBQuest.GetQuestType(data.QuestTid);
			if (type == GameDB.E_QuestType.Main /*|| type == GameDB.E_QuestType.Tutorial*/)
				mainQuest = data;

			QuestUpdate?.Invoke(data);
			data.NewQuest = false;
		}

		public void DeleteQuestList(ulong _questSID)
		{
			QuestData data = QuestList.Find(item => item.QuestId == _questSID);

			if (data == null) ZLog.LogError(ZLogChannel.UI, string.Format("[UI Quest] ============= Invalid Quest Delete ID : {0}", _questSID));

			data.DeleteQuest = true;
			QuestUpdate?.Invoke(data);
			QuestList.Remove(data);
		}

		public void RewardQuest(uint _questTID)
		{
			QuestData data = QuestList.Find(item => item.QuestTid == _questTID);
			if (data != null)
			{
				data.State = E_QuestState.Reward;
				QuestUpdate?.Invoke(data);
			}
		}

		public void CompleteQuest(ulong _questSID, bool _groupComplete)
		{
			QuestData data = QuestList.Find(item => item.QuestId == _questSID);
			if (data == null)
			{
				ZLog.LogError(ZLogChannel.WebSocket, "[Quest] There are No QuestInstance : " + _questSID.ToString());
			}
			else
			{
				data.State = E_QuestState.Complete;
				data.GroupComplete = _groupComplete;
				QuestList.Remove(data);
				QuestUpdate?.Invoke(data);
			}
		}

		//--------------------------------------------------------
		public void AddEventQuestData(QuestEvent _eventInfo, bool invokeEvent = true)
		{
			if (dicQuestEventData.ContainsKey(_eventInfo.GroupTid) == false)
			{
				dicQuestEventData.Add(_eventInfo.GroupTid, new List<QuestEventData>());
			}

			var list = dicQuestEventData[_eventInfo.GroupTid];
			var data = list.Find(item => item.QuestID == _eventInfo.QuestId);

			if (data == null)
			{
				list.Add(new QuestEventData(_eventInfo));
			}
			else
			{
				data.Reset(_eventInfo);
			}

			if(invokeEvent)
				QuestEventUpdate?.Invoke();
		}

		public void AddEventQuestDataList(List<QuestEvent> listEventInfo)
		{
			foreach(var iter in listEventInfo)
				AddEventQuestData(iter, false);

			QuestEventUpdate?.Invoke();
		}

		public QuestEventData GetEventData(uint groupId, uint eventQuestTid)
		{
			if (dicQuestEventData.TryGetValue(groupId, out var listData) == false)
			{
				return null;
			}

			return listData.Find(item => item.QuestTid == eventQuestTid);
		}

		public List<QuestEventData> GetBattlePassDataList(uint groupId, byte date)
		{
			List<QuestEventData> list = new List<QuestEventData>();
			if (dicQuestEventData.TryGetValue(groupId, out var listData) == false)
				return list;

			foreach (var iter in listData)
			{
				if (iter.Day == date)
					list.Add(iter);
			}
			return list;
		}

		//---------------------------------------------------

		public void NextQuest(Quest _nextQuestInfo, ulong _prevQuestId)
		{
			uint PrevQuestTid = 0;
			var findQuest = QuestList.Find(item => item.QuestId == _prevQuestId);

			if (findQuest != null)
			{
				PrevQuestTid = findQuest.QuestTid;
				QuestList.Remove(findQuest);
			}

			findQuest = QuestList.Find(item => item.QuestId == _nextQuestInfo.QuestId);

			if (findQuest != null)
				findQuest.Reset(_nextQuestInfo);
			else
				QuestList.Add(findQuest = new QuestData(_nextQuestInfo));

			QuestUpdate?.Invoke(findQuest);

			var type = DBQuest.GetQuestType(_nextQuestInfo.QuestTid);
			if (type == GameDB.E_QuestType.Main /*|| type == GameDB.E_QuestType.Tutorial*/)
			{
				mainQuest = findQuest;
				MainQuestChanged?.Invoke(PrevQuestTid, _nextQuestInfo.QuestTid);
			}
		}
		#endregion

		#region Common
		public void SetReturnStoneUseTime(ulong _time)
		{
			ReturnStoneUseTime = _time;
		}

		public void AddRemainItems(ItemInfo RemainItems)
		{
			if (RemainItems.AccountStackLength > 0 || RemainItems.StackLength > 0 || RemainItems.EquipLength > 0)
			{
				List<ZItem> newItem = new List<ZItem>();

				List<AccountItemStack> accountItemList = new List<AccountItemStack>();
				List<ItemStack> stackList = new List<ItemStack>();
				List<ItemEquipment> equipList = new List<ItemEquipment>();

				for (int i = 0; i < RemainItems.AccountStackLength; i++)
				{
					var accountItem = RemainItems.AccountStack(i).Value;
					accountItemList.Add(accountItem);
					var accData = Me.CurCharData.GetItemData(accountItem.ItemId, NetItemType.TYPE_ACCOUNT_STACK);
					if (accData == null)
						newItem.Add(new ZItem(accountItem));
				}
				for (int i = 0; i < RemainItems.StackLength; i++)
				{
					var remainItem = RemainItems.Stack(i).Value;
					stackList.Add(remainItem);
					var stackData = Me.CurCharData.GetItemData(remainItem.ItemId, NetItemType.TYPE_STACK);
					if (stackData == null)
						newItem.Add(new ZItem(remainItem));
				}

				for (int i = 0; i < RemainItems.EquipLength; i++)
				{
					var equipItem = RemainItems.Equip(i).Value;
					equipList.Add(equipItem);
					var equipData = Me.CurCharData.GetItemData(equipItem.ItemId, NetItemType.TYPE_EQUIP);
					if (equipData == null)
						newItem.Add(new ZItem(equipItem));
				}

				AddItemList(accountItemList);
				AddItemList(stackList);
				AddItemList(equipList);

				//if (newItem.Count > 0 && UIManager.Instance.Find(out UIFrameInventory _inventory))
				//	_inventory.SetNewList(newItem);
			}

			if (UIManager.Instance.Find(out UIFrameHUD _hud)) _hud.RefreshCurrency();

			if (RemainItems.PetLength > 0)
			{
				List<Pet> petList = new List<Pet>();
				List<Pet> rideList = new List<Pet>();
				for (int i = 0; i < RemainItems.PetLength; i++)
				{

					var resultData = RemainItems.Pet(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petList.Add(resultData);
					}
					else
					{
						rideList.Add(resultData);
					}
				}

				AddPetList(petList);
				AddRideList(rideList);
			}

			if (RemainItems.PetGachaKeepLength > 0)
			{
				List<PetGachaKeep> petKeepList = new List<PetGachaKeep>();
				List<PetGachaKeep> rideKeepList = new List<PetGachaKeep>();

				for (int i = 0; i < RemainItems.PetGachaKeepLength; i++)
				{
					var resultData = RemainItems.PetGachaKeep(i).Value;

					if (DBPet.TryGet(resultData.PetTid, out var table) == false)
						continue;

					if (table.PetType == GameDB.E_PetType.Pet)
					{
						petKeepList.Add(resultData);
					}
					else if (table.PetType == GameDB.E_PetType.Vehicle)
					{
						rideKeepList.Add(resultData);
					}
				}

				Me.CurCharData.AddPetKeepList(petKeepList);
				Me.CurCharData.AddRideKeepList(rideKeepList);
			}

			if (RemainItems.ChangeLength > 0)
			{
				List<Change> changeList = new List<Change>();

				for (int i = 0; i < RemainItems.ChangeLength; i++)
					changeList.Add(RemainItems.Change(i).Value);

				AddChangeList(changeList);
			}

			if (RemainItems.ChangeGachaKeepLength > 0)
			{
				List<ChangeGachaKeep> changeKeepList = new List<ChangeGachaKeep>();

				for (int i = 0; i < RemainItems.ChangeGachaKeepLength; i++)
					changeKeepList.Add(RemainItems.ChangeGachaKeep(i).Value);

				Me.CurCharData.AddChangeKeepList(changeKeepList);

			}

			if (RemainItems.RuneLength > 0)
			{
				//룬 획득
				List<Rune> runeList = new List<Rune>();

				for (int i = 0; i < RemainItems.RuneLength; i++)
					runeList.Add(RemainItems.Rune(i).Value);

				Me.CurCharData.AddRuneList(runeList);
			}
		}

		public SRewardItemList ExtractItemList(ItemInfo RemainItems)
		{
			SRewardItemList itemList = new SRewardItemList();

			for (int i = 0; i < RemainItems.AccountStackLength; i++)
			{
				AccountItemStack item = RemainItems.AccountStack(i).Value;
				itemList.ItemID.Add(item.ItemTid);
				itemList.ItemCount.Add((uint)item.Cnt);
			}
			for (int i = 0; i < RemainItems.StackLength; i++)
			{
				ItemStack item = RemainItems.Stack(i).Value;
				itemList.ItemID.Add(item.ItemTid);
				itemList.ItemCount.Add(item.Cnt);
			}

			for (int i = 0; i < RemainItems.EquipLength; i++)
			{
				ItemEquipment item = RemainItems.Equip(i).Value;
				itemList.ItemID.Add(item.ItemTid);
				itemList.ItemCount.Add(1); // 장비품은 유일하니까 
			}
			return itemList;
		}

		#endregion

		#region Friend
		public Friend GetFirend(ulong friendID)
		{
			return friendList.Find(item => item.CharId == friendID);
		}

		public Friend GetRequestFriend(ulong friendID)
		{
			return requestfriendList.Find(item => item.CharId == friendID);
		}

		public void ClearFriendList()
		{
			if (friendList != null)
				friendList.Clear();
		}

		public void ClearRequestFriendList()
		{
			if (requestfriendList != null)
				requestfriendList.Clear();
		}

		public void AddFriend(FriendInfo? _friendinfo)
		{
			var findFriend = friendList.Find(item => item.CharId == _friendinfo.Value.CharId);

			if (findFriend != null)
				findFriend.Reset(_friendinfo.Value);
			else
				friendList.Add(new Friend(_friendinfo.Value));
		}

		public void ResetFriendList(List<FriendInfo> listFriend)
		{
			friendList.Clear();
			foreach (var iter in listFriend)
			{
				friendList.Add(new Friend(iter));
			}
		}

		public void RemoveFriend(ulong _friendId, E_FriendState _state)
		{
			var findFriend = friendList.Find(item => item.CharId == _friendId);
			if (findFriend != null)
			{
				switch (_state)
				{
					case E_FriendState.Friend:
						findFriend.IsFriend = false;
						break;
					case E_FriendState.AlertFriend:
						findFriend.IsAlert = false;
						break;
				}

				if (!findFriend.IsFriend && !findFriend.IsAlert)
					friendList.Remove(findFriend);
			}
		}

		public void AddRequestFriend(FriendRequestInfo? _friendinfo)
		{
			var findFriend = requestfriendList.Find(item => item.CharId == _friendinfo.Value.CharId);

			if (findFriend != null)
				findFriend.Reset(_friendinfo.Value);
			else
				requestfriendList.Add(new Friend(_friendinfo.Value));
		}

		public void RemoveRequestFriend(ulong _friendId)
		{
			var findFriend = requestfriendList.Find(item => item.CharId == _friendId);
			if (findFriend != null)
				requestfriendList.Remove(findFriend);
		}

		public List<Friend> GetReceiveRequestFriend()
		{
			List<Friend> reqFriend = new List<Friend>();

			foreach (var list in requestfriendList)
			{
				if (list.friendReqState == E_FriendRequestState.Receive)
					reqFriend.Add(list);
			}

			return reqFriend;
		}
		#endregion

		#region Rune (Pet)

		public void ClearRuneList()
		{
			if (RuneDic != null)
				RuneDic.Clear();
		}

		public PetRuneData GetRune(ulong _runeId)
		{
			if (RuneDic.TryGetValue(_runeId, out var foundRune))
				return foundRune;

			return null;
		}

		public void AddRune(WebNet.Rune? _runeInfo)
		{
			ulong runeId = _runeInfo.Value.ItemId;
			uint equipPetTid = _runeInfo.Value.EquipPetTid;

			if (RuneDic.TryGetValue(runeId, out var foundRune))
			{
				if (foundRune.OwnerPetTid > 0)
				{
					if (equipPetTid != foundRune.OwnerPetTid)
					{
						UnequipRune(foundRune.OwnerPetTid, runeId);
						if (equipPetTid > 0)
							EquipRune(equipPetTid, runeId);
					}
				}
				else if (equipPetTid > 0)
					EquipRune(equipPetTid, runeId);


				foundRune.Reset(_runeInfo.Value);
			}
			else
			{
				if (equipPetTid > 0)
					EquipRune(equipPetTid, runeId);

				RuneDic.Add(runeId, new PetRuneData(_runeInfo.Value));
			}
		}

		public void AddRuneList(List<WebNet.Rune> listRune)
		{
			foreach (var iter in listRune)
				AddRune(iter);
		}
		public void AddRune(MmoNet.Rune? _runeInfo)
		{
			ulong runeId = _runeInfo.Value.ItemId;
			uint equipPetTid = _runeInfo.Value.EquipPetTid;

			if (RuneDic.TryGetValue(runeId, out var foundRune))
			{
				if (foundRune.OwnerPetTid > 0)
				{
					if (equipPetTid != foundRune.OwnerPetTid)
					{
						UnequipRune(foundRune.OwnerPetTid, runeId);
						if (equipPetTid > 0)
							EquipRune(equipPetTid, runeId);
					}
				}
				else if (equipPetTid > 0)
					EquipRune(equipPetTid, runeId);


				foundRune.Reset(_runeInfo.Value);
			}
			else
			{
				if (equipPetTid > 0)
					EquipRune(equipPetTid, runeId);

				RuneDic.Add(runeId, new PetRuneData(_runeInfo.Value));
			}

		}

		public void EnchantRune(Rune _runeInfo)
		{
			ulong runeId = _runeInfo.ItemId;
			uint equipPetTid = _runeInfo.EquipPetTid;

			if (RuneDic.TryGetValue(runeId, out var foundRune))
			{
				if (foundRune.BaseEnchantTid != _runeInfo.BaseEnchantTid ||
					foundRune.OptTidList_01.Count != _runeInfo.OptTidList1Length ||
					foundRune.OptTidList_02.Count != _runeInfo.OptTidList2Length ||
					foundRune.OptTidList_03.Count != _runeInfo.OptTidList3Length ||
					foundRune.OptTidList_04.Count != _runeInfo.OptTidList4Length)
				{
					foundRune.Reset(_runeInfo);
					// 강화 성공시 이벤트 호출
					RuneEnchantUpdate?.Invoke(equipPetTid, runeId);
				}
			}
		}

		public void RemoveRune(ulong _runeId)
		{
			if (RuneDic.TryGetValue(_runeId, out var foundRune))
			{
				if (foundRune.OwnerPetTid > 0)
					UnequipRune(foundRune.OwnerPetTid, _runeId);

				RuneDic.Remove(_runeId);
				RuneRemoveUpdate.Invoke(_runeId);
			}
		}

		public void RemoveRuneList(List<ulong> listRune)
		{
			foreach (var iter in listRune)
				RemoveRune(iter);
		}

		/// <summary> 펫 해당 룬 슬롯에 룬 장착 여부 </summary>
		public bool IsEquipRuneList(uint _petTid, GameDB.E_EquipSlotType _runeSlotType)
		{
			foreach (var rune in RuneDic.Values)
			{
				if (rune.OwnerPetTid == _petTid && rune.SlotType == _runeSlotType)
					return true;
			}

			return false;
		}

		/// <summary> 모든 룬 개수 </summary>
		public int GetRuneCountAll()
		{
			if (RuneDic.Count > 0)
				return RuneDic.Count;

			return 0;
		}

		/// <summary> 룬인벤이 꽉 찼는지 여부 </summary>
		public bool IsFullRuneInven()
		{
			if (RuneDic != null)
				return RuneDic.Count >= DBConfig.Rune_Inventory_Max_Count;

			return false;
		}

		#endregion

		#region Gain History
		public event Action<uint> UpdateArtifactDestiny;

		public void ClearItemGainHistory()
		{
			ItemGainHistory.Clear();
			ClearCompleteArtifactDestiny();
		}

		private void ClearCompleteArtifactDestiny()
		{
			CompleteArtifactDestinys.Clear();
		}

		public void AddGainItem(uint _addGroupId)
		{
			if (!ItemGainHistory.Contains(_addGroupId))
			{
				ItemGainHistory.Add(_addGroupId);
				CheckUpdateCompleteArtifactDestiny(true);
			}
		}

		private void CheckUpdateCompleteArtifactDestiny(bool _bCheckNewAlram = false)
		{
			//갱신되면 전체를 다시 체크 해야함...
			foreach (var tableData in DBArtifact.GetAllLink())
			{
				if (CompleteArtifactDestinys.Contains(tableData.LinkID))
					continue;

				bool bComplete = true;

				//foreach (var tid in tableData.MaterialArtifactID)
				//{
				//	if (!IsGainedItem(DBItem.GetGroupId(tid)))
				//		bComplete = false;

				//	if (!bComplete)
				//		break;
				//}

				if (bComplete)
				{
					CompleteArtifactDestinys.Add(tableData.LinkID);

					if (_bCheckNewAlram)
						DeviceSaveDatas.AddCharacterKey(ID, "COMPLETE_ARTIFACT_DESTINY", tableData.LinkID);

					UpdateArtifactDestiny?.Invoke(tableData.LinkID);
				}
			}
		}

		public bool IsGainedItem(uint _itemGroupId)
		{
			return ItemGainHistory.Contains(_itemGroupId);
		}
		#endregion

		#region Artifact (아티팩트)
		public void AddArtifactID(ArtifactInfo info)
		{
			var targetData = DBArtifact.GetArtifactByID(info.ArtifactTid);

			if (targetData == null)
				return;

			if (ArtifactItemList.ContainsKey(targetData.ArtifactGroupID) == false)
			{
				ArtifactItemList.Add(targetData.ArtifactGroupID, new ArtifactData());
			}

			var target = ArtifactItemList[targetData.ArtifactGroupID];
			target.ArtifactGroupID = targetData.ArtifactGroupID;
			target.ArtifactID = info.ArtifactId;
			target.ArtifactTid = info.ArtifactTid;
			target.Step = targetData.Step;
		}

		public void DeleteArtifactID(ulong id)
		{
			ArtifactData target = null;

			foreach (var t in ArtifactItemList)
			{
				if (t.Value.ArtifactID == id)
				{
					target = t.Value;
					break;
				}
			}

			if (target != null)
			{
				ArtifactItemList.Remove(target.ArtifactGroupID);
			}
		}

		public void ClearArtifact()
		{
			ArtifactItemList.Clear();
		}

		public uint GetArtifactStep(uint groupID)
		{
			if (ArtifactItemList.ContainsKey(groupID) == false)
				return 0;

			return ArtifactItemList[groupID].Step;
		}

		public bool IsArtifactObtained(uint artifactTid)
		{
			var targetData = DBArtifact.GetArtifactByID(artifactTid);

			if (targetData == null)
				return false;

			if (ArtifactItemList.ContainsKey(targetData.ArtifactGroupID) == false)
				return false;

			return ArtifactItemList[targetData.ArtifactGroupID].Step >= targetData.Step;
		}

		public bool IsMyArtifact(uint artifactID)
		{
			foreach (var t in ArtifactItemList)
			{
				if (t.Value.ArtifactTid == artifactID)
					return true;
			}

			return false;
		}

		/// <summary>
		/// ArtifactGroup ID 를 받아서 해당 그룹의 내가 소유중인 Artifact 가 존재하면 
		/// Step 을 리턴해줌 
		/// </summary>
		public uint GetArtifactStepByGroupID(uint groupID)
		{
			if (ArtifactItemList.ContainsKey(groupID) == false)
				return 0;

			return ArtifactItemList[groupID].Step;
		}

		public uint GetMyArtifactTIDByGroupID(uint groupID)
		{
			if (ArtifactItemList.ContainsKey(groupID) == false)
				return 0;

			return ArtifactItemList[groupID].ArtifactTid;
		}

		/// <summary>
		/// ArtifactID 를 받아서 해당 그룹의 내가 소유중인 Artifact 가 존재하면 
		/// Step 을 리턴해줌 
		/// </summary>
		public uint GetMyArtifactTIDMatchedToArtifactTID(uint artifactID)
		{
			var targetData = DBArtifact.GetArtifactByID(artifactID);

			if (targetData == null)
				return 0;

			if (ArtifactItemList.ContainsKey(targetData.ArtifactGroupID) == false)
				return 0;

			return ArtifactItemList[targetData.ArtifactGroupID].ArtifactTid;
		}

		public ulong GetMyArtifactIDByTid(uint tid)
		{
			foreach (var t in ArtifactItemList)
			{
				if (t.Value.ArtifactTid == tid)
					return t.Value.ArtifactID;
			}

			return 0;
		}

		public bool GetMyLink(uint linkGroup, out uint myLinkTid)
		{
			List<uint> myArtifacts = new List<uint>();
			myLinkTid = 0;

			foreach (var data in ArtifactItemList)
			{
				myArtifacts.Add(data.Value.ArtifactTid);
			}

			var t = DBArtifact.GetLinkDataQualified(linkGroup, myArtifacts);

			if (t != null)
			{
				myLinkTid = t.LinkID;
			}

			return t != null;
		}

		public uint GetMyArtifactPetTid()
		{
			if (Artifact_Pet == 0)
				return 0;

			uint tid = 0;

			foreach (var t in ArtifactItemList)
			{
				if (t.Value.ArtifactID == Artifact_Pet)
				{
					tid = t.Value.ArtifactTid;
					break;
				}
			}

			return tid;
		}

		public uint GetMyArtifactVehicleTid()
		{
			if (Artifact_Vehicle == 0)
				return 0;

			uint tid = 0;

			foreach (var t in ArtifactItemList)
			{
				if (t.Value.ArtifactID == Artifact_Vehicle)
				{
					tid = t.Value.ArtifactTid;
					break;
				}
			}

			return tid;
		}

		public uint GetMyArtifactTidByType(GameDB.E_PetType type)
		{
			if (type == GameDB.E_PetType.Pet)
			{
				if (Artifact_Pet == 0)
					return 0;
				else return GetMyArtifactPetTid();
			}
			else if (type == GameDB.E_PetType.Vehicle)
			{
				if (Artifact_Vehicle == 0)
					return 0;
				else return GetMyArtifactVehicleTid();
			}

			return 0;
		}

		public bool IsThisArtifactEquipped(uint tid)
		{
			if (tid == 0)
				return false;

			ulong id = GetMyArtifactIDByTid(tid);

			if (id == 0)
				return false;

			return Artifact_Pet == id || Artifact_Vehicle == id;
		}

		public bool IsArtifactEquippedByType(GameDB.E_PetType type)
		{
			if (type == GameDB.E_PetType.Pet)
				return Artifact_Pet != 0;
			if (type == GameDB.E_PetType.Vehicle)
				return Artifact_Vehicle != 0;
			return false;
		}

		public bool IsArtifactEquippedByTID(uint tid)
		{
			if (tid == 0)
				return false;

			if (Artifact_Pet != 0)
			{
				uint petTid = GetMyArtifactPetTid();

				if (petTid == tid)
					return true;
			}

			if (Artifact_Vehicle != 0)
			{
				uint vehicleTid = GetMyArtifactVehicleTid();

				if (vehicleTid == tid)
					return true;
			}

			return false;
		}
		#endregion

		#region 강림 파견
		public void ClearChangeQuestList()
		{
			ChangeQuestDataList.Clear();
		}

		public void AddChangeQuest(ChangeQuest? _questInfo)
		{
			var findQuest = ChangeQuestDataList.Find(item => item.QuestTid == _questInfo.Value.QuestTid);

			if (findQuest != null)
				findQuest.Reset(_questInfo.Value);
			else
				ChangeQuestDataList.Add(findQuest = new ChangeQuestData(_questInfo.Value));
		}

		public void AddChangeQuestList(List<ChangeQuest> questList)
		{
			ClearChangeQuestList();

			foreach (var iter in questList)
			{
				AddChangeQuest(iter);
			}
		}

		public ChangeQuestData GetChangeQuestData(uint _changeQuestTid)
		{
			var findQuest = ChangeQuestDataList.Find(item => item.QuestTid == _changeQuestTid);
			return findQuest;
		}

		public IList<ChangeQuestData> GetChangeQuestDataList()
		{
			return ChangeQuestDataList;
		}
		#endregion

		#region Make
		public void AddMakeLimitInfo(MakeLimitInfo limitInfo)
		{
			var findItem = MakeLimitList.Find(item => item.MakeTid == limitInfo.MakeTid);

			if (findItem != null)
			{
				findItem.Reset(limitInfo);
			}
			else
			{
				MakeLimitList.Add(new MakeLimitData(limitInfo));
			}
		}

		public void AddMakeLimitInfo(List<MakeLimitInfo> limitList)
		{
			foreach (var limitInfo in limitList)
			{
				var findItem = MakeLimitList.Find(item => item.MakeTid == limitInfo.MakeTid);

				if (findItem != null)
				{
					findItem.Reset(limitInfo);
				}
				else
				{
					MakeLimitList.Add(new MakeLimitData(limitInfo));
				}
			}
		}

		public MakeLimitData GetMakeLimitData(uint makeTid)
		{
			return MakeLimitList.Find(item => item.MakeTid == makeTid);
		}
		#endregion

		#region 속성 
		public void ClearAttribute()
		{
			attributeDic.Clear();
		}

		public void AddAttributeTID(GameDB.E_UnitAttributeType type, uint tid)
		{
			if (attributeDic.ContainsKey(type) == false)
				attributeDic.Add(type, new AttributeData());

			if (DBAttribute.GetAttributeByID(tid, out var tableData))
			{
				attributeDic[type].Tid = tid;
				attributeDic[type].Level = tableData.AttributeLevel;
			}
		}

		// 유저의 속성 Tid 를 가져옵니다 .
		public uint GetAttributeTIDByType(GameDB.E_UnitAttributeType type)
		{
			if (attributeDic.ContainsKey(type) == false)
				return 0;

			return attributeDic[type].Tid;
		}

		// 유저의 해당 속성 레벨을 가져옵니다 . 
		public uint GetAttributeLevelByType(GameDB.E_UnitAttributeType type)
		{
			if (attributeDic.ContainsKey(type) == false)
				return 0;

			uint result = 0;
			GameDB.Attribute_Table tableData;

			if (DBAttribute.GetAttributeByID(attributeDic[type].Tid, out tableData))
			{
				result = tableData.AttributeLevel;
			}

			return result;
		}

		public uint GetAttributeIDByType(GameDB.E_UnitAttributeType type)
		{
			if (attributeDic.ContainsKey(type) == false)
				return 0;

			return attributeDic[type].Tid;
		}

		// 유저의 속성들중 가장 낮은 레벨을 가져옵니다 . 
		public uint GetAttributeMinLevel()
		{
			uint minLevel = uint.MaxValue;

			if (attributeDic.Count > 0)
			{
				foreach (var keyPair in attributeDic)
				{
					if (minLevel > keyPair.Value.Level)
					{
						minLevel = keyPair.Value.Level;
					}
				}
			}
			else
			{
				minLevel = 0;
			}

			return minLevel;
		}

		// 유저의 속성들중 가장 높은 레벨을 가져옵니다.
		public uint GetAttributeMaxLevel()
		{
			uint maxLevel = uint.MinValue;

			if (attributeDic.Count > 0)
			{
				foreach (var keyPair in attributeDic)
				{
					if (maxLevel < keyPair.Value.Level)
					{
						maxLevel = keyPair.Value.Level;
					}
				}
			}
			else
			{
				maxLevel = 0;
			}

			return maxLevel;
		}

		// 유저가 해당 속성을 보유중인지 
		public bool IsThisAttributeObtained_ByID(uint attributeID)
		{
			if (DBAttribute.GetAttributeByID(attributeID, out var tableData))
			{
				if (attributeDic.ContainsKey(tableData.AttributeType) == false)
					return false;

				return attributeDic[tableData.AttributeType].Level >= tableData.AttributeLevel;
			}

			return false;
		}

		public bool IsThisAttributeObtainedOrNextAvailableLevel_ByID(uint attributeID)
		{
			bool result = IsThisAttributeObtained_ByID(attributeID);

			if (result)
				return true;

			GameDB.Attribute_Table data;
			DBAttribute.GetAttributeByID(attributeID, out data);
			uint curLevel = attributeDic[data.AttributeType].Level;

			// 현재 레벨의 다음 레벨인지 체크 . 거보다 크다면 바로 false 
			if (data.AttributeLevel > curLevel + 1)
				return false;

			uint reacheableLevel = DBAttribute.GetAttributeReachableMaxLevelAtCurrentChainLevel(GetAttributeChainEffectLevel());

			// 최종적으로 도달가능한 레벨인지 체크 . 
			return data.AttributeLevel <= reacheableLevel;
		}

		// 유저가 해당 속성을 보유중인지 
		public bool IsThisAttributeObtained_ByLevel(GameDB.E_UnitAttributeType attributeType, uint attributeLevel)
		{
			if (attributeDic.ContainsKey(attributeType) == false)
				return false;

			return attributeDic[attributeType].Level >= attributeLevel;
		}

		// 유저의 속성 연계 레벨 
		public uint GetAttributeChainEffectLevel()
		{
			return DBAttribute.GetCurrentAttributeChainLevel(GetAttributeMinLevel());
		}

		public bool CanEnhanceAttributeByChainCondition(GameDB.E_UnitAttributeType attributeType)
		{
			if (attributeDic.ContainsKey(attributeType) == false)
			{
				return false;
			}

			var target = attributeDic[attributeType];

			/// 완전 최대 레벨 도달 
			if (DBAttribute.GetMaxLevelByType(attributeType) == target.Level)
				return false;

			/// 현재 체인 레벨 기준으로 해당 속성을 더 강화할수있는지 체크 
			/// 여기서 빠꾸먹는 경우는 다른 속성 레벨이 부족한데 자기만 끝 레벨에 도달한 경우 (현재 체인렙에서 도달가능한)
			if (target.Level == DBAttribute.GetAttributeReachableMaxLevelAtCurrentChainLevel(GetAttributeChainEffectLevel()))
			{
				return false;
			}

			return true;
		}

		public bool IsThisAttributeChainEffectObtained(uint chainLevel)
		{
			return GetAttributeChainEffectLevel() >= chainLevel;
		}

		public bool IsAttributeAllMaxLevel()
		{
			foreach (var byType in attributeDic)
			{
				if (DBAttribute.GetMaxLevelByType(byType.Key) != byType.Value.Level)
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region 스킬 사용 우선 순위
		public void ClearSkillUseOrderList()
		{
			SkillUseOrder.Clear();
		}

		public void AddSkillUseOrder(SkillUseOrder? _order)
		{
			var skillOrder = SkillUseOrder.Find(item => item.Tid == _order.Value.SkillTid);

			if (skillOrder != null)
				skillOrder.Reset(_order.Value);
			else
				SkillUseOrder.Add(skillOrder = new SkillOrderData(_order.Value));
		}

		public SkillOrderData GetSkillUseOrderData(uint _skillUseOrderTid)
		{
			var skillOrder = SkillUseOrder.Find(item => item.Tid == _skillUseOrderTid);
			return skillOrder;
		}
		#endregion

		#region 요리
		public void AddCookRecipeData(CookHistory? _info)
		{
			var cookItem = CookRecipeList.Find(item => item.CookTid == _info.Value.CookTid);

			if (cookItem != null)
			{
				cookItem.Reset(_info);
			}
			else
			{
				CookRecipeList.Add(new CookData(_info));
			}
		}

		public CookData GetCookRecipeData(uint _cookTid)
		{
			return CookRecipeList.Find(item => item.CookTid == _cookTid);
		}
        #endregion

        #region 획득 아이템 정보
		public void ClearNewGainItemList()
        {
			NewGainItemList.Clear();
		}

		public List<ZItem> GetNewGainItemList()
        {
			return NewGainItemList;
		}
        #endregion

        #region ===== :: 이벤트 :: =====
        public void UpdateCheckDailyTime(ulong LastCheckTime)
		{
			DailyResetDt = LastCheckTime;
		}

		public long GetRemainCheckDailyNextTime()
		{
			System.DateTime LastCheckDate = TimeHelper.ParseTimeStamp((long)(DailyResetDt));
			return (long)(TimeHelper.DaySecond - (ulong)(LastCheckDate.Hour * (int)TimeHelper.HourSecond + LastCheckDate.Minute * (int)TimeHelper.MinuteSecond + LastCheckDate.Second)) + DBConfig.Event_Reset_Time * (int)TimeHelper.HourSecond;
		}
		#endregion

		#region ===== :: 장착중인 장비 캐싱 :: =====
		public void ClearEquippedItemList()
		{
			EquippedItemList.Clear();
		}

		/// <summary> 해당 슬롯에 아이템을 캐싱해놓는다. </summary>
		public void SetEquippedItem(byte preIndex, byte nextIndex, ZItem item)
		{
			if (0 < nextIndex)
			{
				SetEquippedItem(nextIndex, item.item_id, item.item_tid);
			}
			else if (0 < preIndex)
			{
				SetEquippedItem(preIndex, 0, 0);
			}
		}

		/// <summary> 해당 슬롯에 아이템을 캐싱해놓는다. </summary>
		private void SetEquippedItem(byte index, ulong itemId, uint itemTid)
		{
			if (false == EquippedItemList.ContainsKey(index))
			{
				EquippedItemList.Add(index, new EquippedItemData());
			}

			EquippedItemList[index].ItemId = itemId;
			EquippedItemList[index].ItemTid = itemTid;
		}

		/// <summary> 해당 슬롯에 장착중인 아이템을 얻어온다. </summary>
		public EquippedItemData GetEquippedItem(GameDB.E_EquipSlotType slotType)
		{
			EquippedItemData item = null;
			EquippedItemList.TryGetValue((byte)slotType, out item);
			return item;
		}

		/// <summary> 해당 슬롯에 장착중인 아이템을 얻어온다. </summary>
		public bool TryGetEquippedItem(GameDB.E_EquipSlotType slotType, out EquippedItemData item)
		{
			return EquippedItemList.TryGetValue((byte)slotType, out item);
		}
		#endregion
	}
}

// to do : Enum Define으로 이동
#region Enum List
//GameDB
public enum E_MarkAbleType
{
	None = 0, /*없음*/
	RecoveryMark = 1, /*회복의 문양*/
	AttackMark = 2, /*공격의 문양*/
	DefenseMark = 3, /*방어의 문양*/
}

//Network
public enum NetItemType
{
	TYPE_STACK,
	TYPE_EQUIP,
	TYPE_ACCOUNT_STACK,
}

public enum CollectionType
{
	TYPE_ITEM,
	TYPE_CHANGE,
	TYPE_PET,
	TYPE_RIDE,
}

public enum CollectState
{
	STATE_NONE = 0,
	STATE_PROGRESS = 1,
	STATE_COMPLETE = 2,
}

public enum OptionEquipType
{
	TYPE_EQUIP,
	TYPE_PET,
	TYPE_CHANGE,
	TYPE_RIDE,
}

public enum QuickSlotType
{
	TYPE_ITEM = 0,
	TYPE_SKILL = 1,
}
#endregion