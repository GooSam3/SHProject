using GameDB;
using System.Collections.Generic;
using WebNet;

namespace ZDefine
{
	[System.Flags]
	public enum ChatFilter
	{
		TYPE_NONE = 1,
		TYPE_ALL = 2,
		TYPE_WHISPER = 4,
		TYPE_NORMAL = 8,
		TYPE_SYSTEM = 16,
		TYPE_GUILD = 32,
		TYPE_PARTY = 64,
		TYPE_ALLIANCE = 128,
		TYPE_TRADE = 256,

		TYPE_CHECK_DEFAULT = TYPE_ALL | TYPE_WHISPER | TYPE_NORMAL | TYPE_SYSTEM | TYPE_GUILD | TYPE_PARTY | TYPE_ALLIANCE,
	}

	public enum ChatViewType
	{
		TYPE_SERVER_CHAT,
		TYPE_NORMAL_CHAT,
		TYPE_WHISPER_SEND_CHAT,
		TYPE_WHISPER_RECV_CHAT,
		TYPE_PARTY_CHAT,
		TYPE_GUILD_CHAT,
		TYPE_ALLIANCE_CHAT,
		TYPE_NOTICE_EVENT_CHAT,
		TYPE_NOTICE_WARNING_CHAT,
		TYPE_NOTICE_CHAT,
		TYPE_GET_ITEM_CHAT,
		TYPE_SYSTEM_CHAT,
		TYPE_SYSTEM_GUILD_GREETING,
		TYPE_TRADE_CHAT,
	}

	public class BlockCharacterData
	{
		public ulong CharID;
		public uint CharTid;
		public string Nick;

		public BlockCharacterData() { }
		public BlockCharacterData(CharBlock _charBlock)
		{
			Reset(_charBlock);
		}

		public void Reset(CharBlock _charBlock)
		{
			this.CharID = _charBlock.CharId;
			this.CharTid = _charBlock.CharTid;
			this.Nick = _charBlock.Nick;
		}
	}
	public class RestoreExpData
	{
		public ulong RestoreId; //복구 고유 번호
		public ulong Exp; //복구해야할 경험치 량
		public int Tendency; //죽었을 당시의 성향 수치
		public ulong ExpireDt; //복구 만료시간
		public uint ServerIdx;
		public string KillerNick;

		public RestoreExpData() { }
		public RestoreExpData(RestoreExp _restoreInfo)
		{
			Reset(_restoreInfo);
		}

		public void Reset(RestoreExp _restoreInfo)
		{
			RestoreId = _restoreInfo.RestoreId;
			Exp = _restoreInfo.Exp;
			Tendency = _restoreInfo.Tendency;
			ExpireDt = _restoreInfo.ExpireDt;
			ServerIdx = _restoreInfo.KillerServerIdx;
			KillerNick = _restoreInfo.KillerCharNick;
		}
	}

	public class SendCollectData
	{
		public uint CollectTid;
		public uint Slot;
	}

	public class ZItem
	{
		public NetItemType netType;         //아이템 타입(stack,equip)
		public ulong item_id;               // 아이템 고유 id
		public uint item_tid;               // 아이템 테이블 id
		public byte slot_idx { get; private set; }               // 장착 슬롯 idx
		public ulong cnt;                    // 수량
		public ulong expire_dt;             // 아이템 만료시간
		public bool IsLock;                 //잠금
		public ulong CreateDt;              //아이템 생성 시간

		public bool bCheckedDt = false;           //아이템 생성시간관련 체크한적이 있는가?

		public uint SortIndex = 0;

		public ulong UseTime;               //마지막 사용 시간 (ms)

		/// <summary> 재련시 추가된 어빌리티 option Id </summary>
		public List<uint> Options = new List<uint>();
		public uint ResmeltOptionId_01;
		public uint ResmeltOptionId_02;
		public uint ResmeltOptionId_03;

		public List<uint> Sockets = new List<uint>();

		//클라이언트 사용용으로 만드는 키
		public string GetUniqueKey
		{
			get
			{
				return netType.ToString() + "_" + item_id.ToString();
			}
		}

		public ZItem() { }

		public ZItem(ZItem originItem)
		{
			this.netType = originItem.netType;
			this.item_id = originItem.item_id;
			this.item_tid = originItem.item_tid;
			SetEquipSlotIndex(originItem.slot_idx);
			this.cnt = originItem.cnt;
			this.expire_dt = originItem.expire_dt;
			this.IsLock = originItem.IsLock;
			this.Options.Clear();
			this.Options.AddRange(originItem.Options.ToArray());
			this.ResmeltOptionId_01 = originItem.ResmeltOptionId_01;
			this.ResmeltOptionId_02 = originItem.ResmeltOptionId_02;
			this.ResmeltOptionId_03 = originItem.ResmeltOptionId_03;
			this.Sockets.Clear();
			this.Sockets.AddRange(originItem.Sockets.ToArray());
		}

		public ZItem(ItemStack itemstack)
		{
			Reset(itemstack);
		}

		public ZItem(MmoNet.ItemStack itemstack)
		{
			Reset(itemstack);
		}

		public ZItem(ItemEquipment itemequip)
		{
			Reset(itemequip);
		}

		public ZItem(MmoNet.ItemEquipment itemequip)
		{
			Reset(itemequip);
		}

		public ZItem(AccountItemStack itemstack)
		{
			Reset(itemstack);
		}

		public ZItem(MmoNet.AccountItemStack itemstack)
		{
			Reset(itemstack);
		}

		public void Reset(ItemEquipment itemequip)
		{
			netType = NetItemType.TYPE_EQUIP;
			item_id = itemequip.ItemId;               // 아이템 고유 id
			item_tid = itemequip.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(itemequip.SlotIdx);                       // 장착 슬롯 idx
			cnt = 1;                         // 수량
			expire_dt = itemequip.ExpireDt;             // 아이템 만료시간
			IsLock = itemequip.IsLock == 1;

			int optionLength = itemequip.OptionLength;
			Options.Clear();

			for (int i = 0; i < itemequip.OptionLength; i++)
				Options.Add(itemequip.Option(i));

			ResmeltOptionId_01 = optionLength > 0 ? itemequip.Option(0) : 0;
			ResmeltOptionId_02 = optionLength > 1 ? itemequip.Option(1) : 0;
			ResmeltOptionId_03 = optionLength > 2 ? itemequip.Option(2) : 0;

			Sockets.Clear();
			for (int i = 0; i < itemequip.SocketsLength; i++)
				Sockets.Add(itemequip.Sockets(i));

			CreateDt = itemequip.CreateDt;
		}

		public void Reset(MmoNet.ItemEquipment itemequip)
		{
			netType = NetItemType.TYPE_EQUIP;
			item_id = itemequip.ItemId;               // 아이템 고유 id
			item_tid = itemequip.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(itemequip.SlotIdx);                       // 장착 슬롯 idx
			cnt = 1;                         // 수량
			expire_dt = itemequip.ExpireDt;             // 아이템 만료시간
			IsLock = itemequip.IsLock == 1;

			int optionLength = itemequip.OptionLength;
			Options.Clear();

			for (int i = 0; i < itemequip.OptionLength; i++)
				Options.Add(itemequip.Option(i));

			ResmeltOptionId_01 = optionLength > 0 ? itemequip.Option(0) : 0;
			ResmeltOptionId_02 = optionLength > 1 ? itemequip.Option(1) : 0;
			ResmeltOptionId_03 = optionLength > 2 ? itemequip.Option(2) : 0;

			Sockets.Clear();
			for (int i = 0; i < itemequip.SocketsLength; i++)
				Sockets.Add(itemequip.Sockets(i));

			CreateDt = itemequip.CreateDt;
		}

		public void Reset(AccountItemStack itemstack)
		{
			netType = NetItemType.TYPE_ACCOUNT_STACK;
			item_id = itemstack.ItemId;               // 아이템 고유 id
			item_tid = itemstack.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(0);                       // 장착 슬롯 idx
			cnt = itemstack.Cnt;                         // 수량

			Options.Clear();
			Sockets.Clear();

			IsLock = itemstack.IsLock == 1;
		}

		public void Reset(MmoNet.AccountItemStack itemstack)
		{
			netType = NetItemType.TYPE_ACCOUNT_STACK;
			item_id = itemstack.ItemId;               // 아이템 고유 id
			item_tid = itemstack.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(0);                       // 장착 슬롯 idx
			cnt = itemstack.Cnt;                         // 수량

			Options.Clear();
			Sockets.Clear();

			IsLock = itemstack.IsLock == 1;
		}

		public void Reset(ItemStack itemstack)
		{
			netType = NetItemType.TYPE_STACK;
			item_id = itemstack.ItemId;               // 아이템 고유 id
			item_tid = itemstack.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(0);                       // 장착 슬롯 idx
			cnt = itemstack.Cnt;                         // 수량
			IsLock = itemstack.IsLock == 1;

			Options.Clear();
			Sockets.Clear();
		}

		public void Reset(MmoNet.ItemStack itemstack)
		{
			netType = NetItemType.TYPE_STACK;
			item_id = itemstack.ItemId;               // 아이템 고유 id
			item_tid = itemstack.ItemTid;               // 아이템 테이블 id
			SetEquipSlotIndex(0);                       // 장착 슬롯 idx
			cnt = itemstack.Cnt;                         // 수량
			IsLock = itemstack.IsLock == 1;

			Options.Clear();
			Sockets.Clear();
		}

		public bool Equal(ItemEquipment itemequip)
		{
			return netType == NetItemType.TYPE_EQUIP && item_id == itemequip.ItemId && item_tid == itemequip.ItemTid && slot_idx == itemequip.SlotIdx && cnt == 1 && expire_dt == itemequip.ExpireDt;
		}
		public bool Equal(AccountItemStack itemstack)
		{
			return netType == NetItemType.TYPE_ACCOUNT_STACK && item_id == itemstack.ItemId && item_tid == itemstack.ItemTid && slot_idx == 0 && cnt == itemstack.Cnt;
		}
		public bool Equal(ItemStack itemstack)
		{
			return netType == NetItemType.TYPE_STACK && item_id == itemstack.ItemId && item_tid == itemstack.ItemTid && slot_idx == 0 && cnt == itemstack.Cnt;
		}

		/// <summary> 장착 슬롯 셋팅 </summary>
		public void SetEquipSlotIndex(byte index)
		{
			if (index == slot_idx)
				return;

			byte preIndex = slot_idx;
			slot_idx = index;

			ZNet.Data.Me.CurCharData.SetEquippedItem(preIndex, slot_idx, this);
		}

		/// <summary>
		/// 아이템 삭제 가능한지 여부 확장 함수.
		/// </summary>
		public static bool IsDeletable(ZItem item)
		{
			var itemTable = DBItem.GetItem(item.item_tid);
			if (null == itemTable)
				return false;

			return EnumHelper.CheckFlag(E_LimitType.Delete, itemTable.LimitType)
				&& item.netType != NetItemType.TYPE_ACCOUNT_STACK
				&& item.slot_idx == 0
				&& !item.IsLock;
		}
	}

	public class BuyLimitData
	{
		public uint ShopTid;
		public uint BuyCnt;
		public ulong ExpireDt;

		public BuyLimitData(BuyLimitInfo info)
		{
			Reset(info);
		}

		public void Reset(BuyLimitInfo info)
		{
			ShopTid = info.ShopTid;
			BuyCnt = info.BuyCnt;
			ExpireDt = info.ExpireDt;
		}
	}

	public class MailData
	{
		public ulong MailIdx;               // 우편 고유 id
		public ushort Type;                 // 우편 타입
		public string Title;                // 우편 제목
		public uint ItemTid;                // 아이템 테이블 아이디
		public uint Cnt;                    // 수량
		public ulong ExpireDt;              // 우편 만료 날짜

		public GameDB.E_MailReceiver mailType;

		public MailData() { }
		public MailData(GameDB.E_MailReceiver _mailType, MailInfo mailInfo)
		{
			Reset(_mailType, mailInfo);
		}

		public void Reset(GameDB.E_MailReceiver _mailType, MailInfo mailInfo)
		{
			this.mailType = _mailType;

			this.MailIdx = mailInfo.MailIdx;
			this.Type = mailInfo.Type;
			this.Title = mailInfo.Title;
			this.ItemTid = mailInfo.ItemTid;
			this.Cnt = mailInfo.Cnt;
			this.ExpireDt = mailInfo.ExpireDt;
		}
	}

	public class CashMailData
	{
		public ulong MailIdx;       /// 우편 고유 id 
		public uint ShopTid; /// 상점 테이블 tid

		public CashMailData(ulong mailIdx, uint shopTid)
		{
			this.MailIdx = mailIdx;
			this.ShopTid = shopTid;
		}

		public void Reset(CashMailInfo info)
		{
			MailIdx = info.MailIdx;
			ShopTid = info.ShopTid;
		}
	}

	public class MessageData
	{
		public ulong MessageIdx;            // 쪽지 고유 id
		public ulong SenderUserId;          //보낸 유저 id
		public string SenderCharNick;       // 보낸 사람 이름
		public uint Type;                   // 메시지 타입(E_MessageType 확인)
		public string Title;                // 제목
		public string Message;              // 내용
		public byte IsRead;                 // 읽음 유무
		public ulong ExpireDt;              // 만료 시간

		public MessageData() { }
		public MessageData(MessageInfo messageInfo)
		{
			Reset(messageInfo);
		}
		public void Reset(MessageInfo messageInfo)
		{
			this.MessageIdx = messageInfo.MessageIdx;
			this.SenderUserId = messageInfo.SenderUserId;
			this.SenderCharNick = messageInfo.SenderCharNick;
			this.Type = messageInfo.Type;
			this.Title = messageInfo.Title;
			this.Message = messageInfo.Message;
			this.IsRead = messageInfo.IsRead;
			this.ExpireDt = messageInfo.ExpireDt;
		}
	}

	public class ChatData
	{
		public ChatViewType type;
		public uint ServerIdx;
		public ulong CharId;
		public string CharNick;
		public string Message;
		public string MessageOrigin;

		public ulong GuildID;
		public uint GuildMarkTid;
		public string GuildName;

		public ulong RecvTime;

		public ChatData(ChatViewType _type, uint _ServerIdx, ulong _CharId, string _CharNick, string _Message, string _MessageOrigin, ulong _GuildId, uint _GuildMarkTid, string _GuildName)
		{
			type = _type;
			ServerIdx = _ServerIdx;
			CharId = _CharId;
			CharNick = _CharNick;
			Message = _Message;
			MessageOrigin = _MessageOrigin;

			GuildID = _GuildId;
			GuildMarkTid = _GuildMarkTid;
			GuildName = _GuildName;

			RecvTime = TimeManager.NowSec;
		}

		public ChatData(ChatViewType _type, BroadcastServerChat recvMsg)
		{
			type = _type;
			ServerIdx = recvMsg.ServerIdx;
			CharId = recvMsg.CharId;
			CharNick = recvMsg.CharNick;
			Message = recvMsg.Message;

			GuildID = recvMsg.GuildId;
			GuildMarkTid = recvMsg.GuildMarkTid;
			GuildName = recvMsg.GuildName;

			RecvTime = TimeManager.NowSec;
		}
	}

	public class ChatEnterData
	{
		public E_ChatType chatType;
		public List<string> Args = new List<string>();

		public ChatEnterData(E_ChatType _chatType, params string[] _args)
		{
			Reset(_chatType, _args);
		}

		public void Reset(E_ChatType _chatType, params string[] _args)
		{
			chatType = _chatType;
			if (_args.Length > 0)
			{
				Args.Clear();
				Args.AddRange(_args);
			}
		}

		public ChatEnterData(ChatInfo chatInfo)
		{
			Reset(chatInfo);
		}

		public void Reset(ChatInfo chatInfo)
		{
			chatType = chatInfo.ChatType;

			Args.Clear();
			for (int i = 0; i < chatInfo.ArgsLength; i++)
			{
				Args.Add(chatInfo.Args(i));
			}
		}
	}

	public class MarkData
	{
		public uint MarkTid;
		public byte Step;

		public MarkData() { }

		//public bool Equal(Mark markInfo)
		//{
		//    return MarkTid == markInfo.MarkTid;// && EnchantTryCnt == markInfo.EnchantCnt;
		//}
	}

	public class CollectData
	{
		public uint CollectTid;
		public CollectionType curType;
		public CollectState curState = CollectState.STATE_NONE;
		public List<uint> MaterialTids = new List<uint>(256); //보통 컬렉션 개수만큼 할당해놓는게 이득.
		public byte TotalCnt;
		public byte DataCnt;

		public CollectData(CollectionType Type, uint _CollectTid, uint SlotIdx, uint ItemTid)
		{
			curType = Type;
			CollectTid = _CollectTid;
			UpdateMaterial(SlotIdx, ItemTid);
		}

		public CollectData(CollectionType Type, Collect collectInfo)
		{
			Reset(Type, collectInfo);
		}

		public void Reset(CollectionType Type, Collect collectInfo)
		{
			curType = Type;
			CollectTid = collectInfo.CollectTid;
			MaterialTids.Clear();

			DataCnt = 0;
			var materialTidArr = collectInfo.GetMaterialTidsArray();
			for (int i = 0; i < materialTidArr.Length; i++)
			{
				uint materialTid = materialTidArr[i];

				if (materialTid != 0)
					DataCnt++;
				MaterialTids.Add(materialTid);
			}

			TotalCnt = 0;

			switch (curType)
			{
				case CollectionType.TYPE_ITEM:
					TotalCnt = DBItemCollect.GetItemCollection(CollectTid).CollectionItemCount;
					break;
				case CollectionType.TYPE_CHANGE:
					TotalCnt = DBChangeCollect.GetChangeCollection(CollectTid).CollectionChangeCount;
					break;
				case CollectionType.TYPE_PET:
					if (DBPetCollect.GetPetCollection(CollectTid, out var petTable) == false)
						TotalCnt = 0;
					else
						TotalCnt = petTable.CollectionPetCount;
					break;
				case CollectionType.TYPE_RIDE:
					if (DBPetCollect.GetRideCollection(CollectTid, out var rideTable) == false)
						TotalCnt = 0;
					else
						TotalCnt = rideTable.CollectionPetCount;
					break;
			}

			if (TotalCnt <= DataCnt)
			{
				curState = CollectState.STATE_COMPLETE;
			}
			else if (DataCnt > 0)
				curState = CollectState.STATE_PROGRESS;
			else
				curState = CollectState.STATE_NONE;
		}

		public bool UpdateMaterial(uint SlotIdx, uint ItemTid)
		{
			TotalCnt = 0;

			switch (curType)
			{
				case CollectionType.TYPE_ITEM:
					TotalCnt = DBItemCollect.GetItemCollection(CollectTid).CollectionItemCount;
					break;
				case CollectionType.TYPE_CHANGE:
					TotalCnt = DBChangeCollect.GetChangeCollection(CollectTid).CollectionChangeCount;
					break;
				case CollectionType.TYPE_PET:
					if (DBPetCollect.GetPetCollection(CollectTid, out var petTable) == false)
						TotalCnt = 0;
					else
						TotalCnt = petTable.CollectionPetCount;
					break;
				case CollectionType.TYPE_RIDE:
					if (DBPetCollect.GetRideCollection(CollectTid, out var rideTable) == false)
						TotalCnt = 0;
					else
						TotalCnt = rideTable.CollectionPetCount;
					break;
			}

			DataCnt = 0;
			if (TotalCnt > 0 && MaterialTids.Count <= 0)
			{
				//init
				for (int i = 0; i < TotalCnt; i++)
					MaterialTids.Add(0);
			}

			for (int i = 0; i < MaterialTids.Count; i++)
			{
				if (i == SlotIdx)
				{
					MaterialTids[i] = ItemTid;
				}

				if (MaterialTids[i] != 0)
					DataCnt++;
			}

			if (TotalCnt <= DataCnt)
			{
				if (curState != CollectState.STATE_COMPLETE)
				{
					curState = CollectState.STATE_COMPLETE;
					return true;
				}
			}
			else if (DataCnt > 0)
			{
				if (curState != CollectState.STATE_PROGRESS)
				{
					curState = CollectState.STATE_PROGRESS;
					return true;
				}
			}
			else
			{
				if (curState != CollectState.STATE_NONE)
				{
					curState = CollectState.STATE_NONE;
					return true;
				}
			}

			return false;
		}

		public bool Equal(Collect collectInfo)
		{
			if (CollectTid != collectInfo.CollectTid)
				return false;

			if (MaterialTids.Count != collectInfo.MaterialTidsLength)
				return false;

			for (int i = 0; i < collectInfo.MaterialTidsLength; i++)
			{
				if (MaterialTids[i] != collectInfo.MaterialTids(i))
					return false;
			}

			return true;
		}
	}

	public class ChangeData
	{
		public ulong ChangeId;
		public uint ChangeTid;
		public uint Cnt;
		public bool IsLock;
		public ulong CreateDt;              //아이템 생성 시간

		/// <summary> 강림 파견 quest id </summary>
		public uint ChangeQuestTid;
		/// <summary> 강림 파견 종료 시간 </summary>
		public ulong ChangeQuestExpireDt;

		/// <summary>강화시, 동적으로 추가되는 능력 (현재 6티어만)</summary>
		public List<uint> AbilityActionIds = new List<uint>();

		public ChangeData(Change changeInfo)
		{
			Reset(changeInfo);
		}

		public ChangeData(ChangeData originData)
		{
			ChangeId = originData.ChangeId;
			ChangeTid = originData.ChangeTid;
			Cnt = originData.Cnt;
			IsLock = originData.IsLock;
			CreateDt = originData.CreateDt;
			ChangeQuestTid = originData.ChangeQuestTid;
			ChangeQuestExpireDt = originData.ChangeQuestExpireDt;

			AbilityActionIds.Clear();
			AbilityActionIds.AddRange(originData.AbilityActionIds);
		}

		public void Reset(Change changeInfo)
		{
			this.ChangeId = changeInfo.ChangeId;
			this.ChangeTid = changeInfo.ChangeTid;
			this.Cnt = changeInfo.Cnt;
			this.IsLock = changeInfo.IsLock == 1;

			ChangeQuestTid = changeInfo.ChangeQuestTid;
			ChangeQuestExpireDt = changeInfo.ChangeQuestExpireDt;

			this.AbilityActionIds.Clear();
			for (int i = 0; i < changeInfo.AbilityAcidsLength; i++)
			{
				uint newActionTid = changeInfo.AbilityAcids(i);
				if (0 != newActionTid) //필터링
					this.AbilityActionIds.Add(newActionTid);
			}
		}
	}

	public class PetData
	{
		public ulong PetId;
		public uint PetTid;
		public uint Cnt;
		public bool IsLock;
		public ulong CreateDt;              //아이템 생성 시간

		public List<uint> AbilityActionIds = new List<uint>(); /// <summary>강화시, 동적으로 추가되는 능력 (현재 6티어만)</summary>

		public ulong Exp;                   //펫 경험치
		public ulong AdvId;                 //탐험 고유 아이디

		/// <summary>ulong : RuneID | 해당 펫에게 장착된 룬 데이터 </summary>
		// TODO : 최적화 여지, 어짜피 존재하는 PetRuneData 클래스 참조로 가지고 있는게 나을듯?
		public List<ulong> EquippedRunes = new List<ulong>();

		/// <summary> 장착된 룬에 의해 발동한 세트 Ability </summary>
		public List<uint> RuneSetAbilityActionIds = new List<uint>();

		public PetData(Pet petInfo)
		{
			Reset(petInfo);
		}

		public void Reset(Pet petInfo)
		{
			this.PetId = petInfo.PetId;
			this.PetTid = petInfo.PetTid;
			this.Cnt = petInfo.Cnt;
			this.IsLock = petInfo.IsLock == 1;

			this.AbilityActionIds.Clear();
			for (int i = 0; i < petInfo.AbilityAcidsLength; i++)
			{
				uint newActionTid = petInfo.AbilityAcids(i);
				if (0 != newActionTid) //필터링
					this.AbilityActionIds.Add(newActionTid);
			}

			this.Exp = petInfo.Exp;
			this.AdvId = petInfo.AdvId;
		}
	}

	public enum E_GachaKeepType
	{
		Pet = 0,// 펫
		Change = 1,
		Ride = 2// 탈것
	}

	public abstract class GachaKeepData
	{
		public abstract E_GachaKeepType KeepType { get; }

		public ulong Id;
		public uint Tid;
		public ulong CreateDt;
		public byte ReOpenCnt;
	}

	public class PetGachaKeepData : GachaKeepData
	{
		public override E_GachaKeepType KeepType => E_GachaKeepType.Pet;

		public PetGachaKeepData(PetGachaKeep keepInfo)
		{
			Reset(keepInfo);
		}

		public virtual void Reset(PetGachaKeep keepInfo)
		{
			Id = keepInfo.PetId;
			Tid = keepInfo.PetTid;
			CreateDt = keepInfo.CreateDt;
			ReOpenCnt = (byte)keepInfo.TryCnt;
		}
	}

	public class RideGachaKeepData : GachaKeepData
	{
		public override E_GachaKeepType KeepType => E_GachaKeepType.Ride;

		public RideGachaKeepData(PetGachaKeep keepInfo)
		{
			Reset(keepInfo);
		}

		public virtual void Reset(PetGachaKeep keepInfo)
		{
			Id = keepInfo.PetId;
			Tid = keepInfo.PetTid;
			CreateDt = keepInfo.CreateDt;
			ReOpenCnt = (byte)keepInfo.TryCnt;
		}
	}

	public class ChangeGachaKeepData : GachaKeepData
	{
		public override E_GachaKeepType KeepType => E_GachaKeepType.Change;

		public ChangeGachaKeepData(ChangeGachaKeep keepInfo)
		{
			Reset(keepInfo);
		}

		public void Reset(ChangeGachaKeep keepInfo)
		{
			Id = keepInfo.ChangeId;
			Tid = keepInfo.ChangeTid;
			CreateDt = keepInfo.CreateDt;
			ReOpenCnt = (byte)keepInfo.TryCnt;
		}
	}

	public class PetAdvData
	{
		public ulong AdvId;//탐험 고유 번호
		public uint AdvTid;//탐험테이블 아이디
						   //public uint AdvGrp;
		public ulong StartDt;
		public ulong EndDt;

		public E_PetAdvStatus status;
		public uint rewardCnt;

		public PetAdvData(PetAdv petadvInfo)
		{
			Reset(petadvInfo);
		}

		public void Reset(PetAdv petadvInfo)
		{
			AdvId = petadvInfo.AdvId;
			AdvTid = petadvInfo.AdvTid;
			//AdvGrp = petadvInfo.AdvGrp;
			StartDt = petadvInfo.StartDt;
			EndDt = petadvInfo.EndDt;

			status = petadvInfo.Status;
			rewardCnt = petadvInfo.RewardCnt;
		}
	}

	public class OptionInfo
	{
		public E_CharacterOptionKey OptionKey;
		public string OptionValue;
	}

	public class OptionEquipInfo
	{
		public ulong UniqueID;
		public byte SlotIdx;
		public OptionEquipType EquipType;
	}

	public class QuickSlotInfo
	{
		public ulong UniqueID;
		public uint TableID;
		public QuickSlotType SlotType;
		public bool bAuto = false;
		public uint Count = 0;

		public uint GetItemCount(QuickSlotType _slotType, uint _tableID)
		{
			ZItem item = ZNet.Data.Me.CurCharData.GetItem(_tableID);
			if (item == null || _slotType != QuickSlotType.TYPE_ITEM)
				return 0;
			else
				return (uint)item.cnt;
		}
	}

	public class QuestData
	{
		public ulong QuestId;           // 퀘스트 고유 아이디
		public uint QuestTid;           // 퀘스트 테이블 아이디
		public uint Value1;             // 퀘스트 조건 value1    // 퀘스트 진행 값
		public uint Value2;             // 퀘스트 조건 value2
		public E_QuestState State;      // 상태 (0: 진행중, 1:보상받을수 있음, 2:완료)
		public E_QuestType Type;
		public bool NoCheck = false;
		public bool DeleteQuest = false;
		public bool NewQuest = false;
		public bool GroupComplete = false;

		public void Init()
		{
			QuestId = 0;
			QuestTid = 0;
			Value1 = 0;
			Value2 = 0;
			State = default;
			Type = default;
			NoCheck = false;
			DeleteQuest = false;
			NewQuest = false;
		}

		public QuestData(Quest? questInfo)
		{
			if (!questInfo.HasValue)
				return;

			Reset(questInfo);
		}

		public QuestData()
		{

		}

		public void Reset(Quest? questInfo)
		{
			Init();

			var info = questInfo.Value;

			this.QuestId = info.QuestId;
			this.QuestTid = info.QuestTid;
			this.Value1 = info.Value1;
			this.Value2 = info.Value2;
			this.State = info.State;
			this.Type = DBQuest.GetQuestType(this.QuestTid);
			Quest_Table questTable = DBQuest.GetQuestData(questInfo.Value.QuestTid);
			if (questTable != null)
			{
				if (questTable.CompleteCheck == E_CompleteCheck.DeliveryItem || questTable.CompleteCheck == E_CompleteCheck.MapMove || questTable.CompleteCheck == E_CompleteCheck.MapPos || questTable.CompleteCheck == E_CompleteCheck.NPCTalk || questTable.CompleteCheck == E_CompleteCheck.Tutorial)
				{
					NoCheck = true;
				}
			}
		}
	}

	public class DailyQuestData
	{
		public ulong QuestId;           // 퀘스트 고유 아이디
		public uint QuestTid;           // 퀘스트 테이블 아이디
		public uint QuestListTid;       // 퀘스트 리스트 테이블 아이디
		public uint Value;              // 퀘스트 조건 value1    // 퀘스트 진행 값
		public E_QuestState State;      // 상태 (0: 진행중, 1:보상받을수 있음, 2:완료)
		public byte CondType;           // 퀘스트 완료 타입 Enum:DailyQuestType
		public uint CondId;             // 퀘스트 완료 타입에 따른 값
		public uint ClearCnt;           // 퀘스트 목표 값
		public long CreateDt;           // 생성 시간

		public void Init()
		{
			QuestId = 0;
			QuestTid = 0;
			QuestListTid = 0;
			Value = 0;
			State = default;
			CondType = 0;
			CondId = 0;
			ClearCnt = 0;
			CreateDt = 0;
		}

		public DailyQuestData(QuestDaily questInfo)
		{
			Reset(questInfo);
		}

		public void Reset(QuestDaily questInfo)
		{
			Init();

			this.QuestId = questInfo.QuestId;
			this.QuestTid = questInfo.QuestTid;
			this.QuestListTid = questInfo.QuestListTid;
			this.Value = questInfo.Value;
			this.State = questInfo.State;
			this.CondType = questInfo.CondType;
			this.CondId = questInfo.CondId;
			this.ClearCnt = questInfo.ClearCnt;
			this.CreateDt = questInfo.CreateDt;
		}
	}

	public class QuestEventData
	{
		public ulong QuestID;
		public uint QuestTid;
		public uint GroupTid;
		public byte Day;
		public uint Value;
		public E_QuestState State;
		public ulong CreateDt;

		public QuestEventData(QuestEvent questeventInfo)
		{
			Reset(questeventInfo);
		}

		public void Reset(QuestEvent questeventInfo)
		{
			this.QuestID = questeventInfo.QuestId;
			this.QuestTid = questeventInfo.QuestTid;
			this.GroupTid = questeventInfo.GroupTid;
			this.Day = questeventInfo.Day;
			this.Value = questeventInfo.Value;
			this.State = questeventInfo.State;
			this.CreateDt = questeventInfo.CreateDt;
		}
	}

	public class LateSyncUpdateWeight
	{
		public float LastCallTime;
		public float LateSyncTime = 1f;
		public System.Action<float, uint, uint> OnLateWeightUpdate;
		public List<object> parameters = new List<object>();
		public void SetParameters(float fweight, uint slotCnt, uint maxslotCnt)
		{
			parameters.Clear();
			parameters.Add(fweight);
			parameters.Add(slotCnt);
			parameters.Add(maxslotCnt);
		}
		public void LateCall()
		{
			OnLateWeightUpdate?.Invoke((float)parameters[0], (uint)parameters[1], (uint)parameters[2]);
		}
	}

	public class PetRuneData
	{
		public ulong RuneId;
		public uint RuneTid;
		public GameDB.E_EquipSlotType SlotType;
		public bool IsLock;
		/// <summary>현재 룬을 소유중인 펫정보. 없다면 0</summary>
		public uint OwnerPetTid;
		public uint BaseEnchantTid;
		/// <summary>없으면 0</summary>
		public uint FirstOptTid;
		public List<uint> OptTidList_01 = new List<uint>();
		public List<uint> OptTidList_02 = new List<uint>();
		public List<uint> OptTidList_03 = new List<uint>();
		public List<uint> OptTidList_04 = new List<uint>();
		public ulong CreateDt;              //아이템 생성 시간

		private bool CheckDt;

		public PetRuneData(WebNet.Rune runeInfo)
		{
			Reset(runeInfo);
		}

		public PetRuneData(MmoNet.Rune runeInfo)
		{
			Reset(runeInfo);
		}

		public PetRuneData(PetRuneData runeInfo)
		{
			Reset(runeInfo);
		}

		public void Reset(PetRuneData runeInfo)
		{
			this.RuneId = runeInfo.RuneId;
			this.RuneTid = runeInfo.RuneTid;
			this.OwnerPetTid = runeInfo.OwnerPetTid;
			this.SlotType = runeInfo.SlotType;
			this.IsLock = runeInfo.IsLock;
			this.BaseEnchantTid = runeInfo.BaseEnchantTid;
			this.FirstOptTid = runeInfo.FirstOptTid;

			this.CreateDt = runeInfo.CreateDt;

			this.OptTidList_01.Clear();

			foreach (var iter in runeInfo.OptTidList_01)
			{
				this.OptTidList_01.Add(iter);
			}
			this.OptTidList_02.Clear();

			foreach (var iter in runeInfo.OptTidList_02)
			{
				this.OptTidList_02.Add(iter);
			}
			this.OptTidList_03.Clear();

			foreach (var iter in runeInfo.OptTidList_03)
			{
				this.OptTidList_03.Add(iter);
			}
			this.OptTidList_04.Clear();

			foreach (var iter in runeInfo.OptTidList_04)
			{
				this.OptTidList_04.Add(iter);
			}

			this.CheckDt = runeInfo.CheckDt;
		}

		public void Reset(WebNet.Rune runeInfo)
		{
			this.RuneId = runeInfo.ItemId;
			this.RuneTid = runeInfo.ItemTid;
			this.OwnerPetTid = runeInfo.EquipPetTid;
			this.SlotType = (GameDB.E_EquipSlotType)runeInfo.SlotIdx;
			this.IsLock = runeInfo.IsLock == 1;
			this.BaseEnchantTid = runeInfo.BaseEnchantTid;
			this.FirstOptTid = runeInfo.FirstOptTid;

			this.CreateDt = runeInfo.CreateDt;

			this.OptTidList_01.Clear();
			int optCount = runeInfo.OptTidList1Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList1(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_01.Add(newRunOptTid);
			}

			this.OptTidList_02.Clear();
			optCount = runeInfo.OptTidList2Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList2(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_02.Add(newRunOptTid);
			}

			this.OptTidList_03.Clear();
			optCount = runeInfo.OptTidList3Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList3(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_03.Add(newRunOptTid);
			}

			this.OptTidList_04.Clear();
			optCount = runeInfo.OptTidList4Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList4(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_04.Add(newRunOptTid);
			}

			if (!CheckDt && CreateDt > ZNet.Data.Me.CurCharData.LastRefreshTime)
				CheckDt = true;
		}

		public void Reset(MmoNet.Rune runeInfo)
		{
			this.RuneId = runeInfo.ItemId;
			this.RuneTid = runeInfo.ItemTid;
			this.OwnerPetTid = runeInfo.EquipPetTid;
			this.SlotType = (GameDB.E_EquipSlotType)runeInfo.SlotIdx;
			this.IsLock = runeInfo.IsLock == 1;
			this.BaseEnchantTid = runeInfo.BaseEnchantTid;
			this.FirstOptTid = runeInfo.FirstOptTid;

			this.CreateDt = runeInfo.CreateDt;

			this.OptTidList_01.Clear();
			int optCount = runeInfo.OptTidList1Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList1(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_01.Add(newRunOptTid);
			}

			this.OptTidList_02.Clear();
			optCount = runeInfo.OptTidList2Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList2(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_02.Add(newRunOptTid);
			}

			this.OptTidList_03.Clear();
			optCount = runeInfo.OptTidList3Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList3(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_03.Add(newRunOptTid);
			}

			this.OptTidList_04.Clear();
			optCount = runeInfo.OptTidList4Length;
			for (int i = 0; i < optCount; i++)
			{
				uint newRunOptTid = runeInfo.OptTidList4(i);
				if (0 != newRunOptTid) //필터링
					this.OptTidList_04.Add(newRunOptTid);
			}

			if (!CheckDt && CreateDt > ZNet.Data.Me.CurCharData.LastRefreshTime)
				CheckDt = true;
		}

		/// <summary> 옵션들 하나의 리스트로 만들어서 리턴해준다. </summary>
		public List<uint> UnitedOptionTids()
		{
			// 할당은 한번만 수행되도록 한다.
			List<uint> tidList = new List<uint>(OptTidList_01.Count + OptTidList_02.Count + OptTidList_03.Count + OptTidList_04.Count);

			tidList.AddRange(OptTidList_01);
			tidList.AddRange(OptTidList_02);
			tidList.AddRange(OptTidList_03);
			tidList.AddRange(OptTidList_04);

			return tidList;
		}

		public int GetSubOptionCount()
		{
			int count = 0;
			if (OptTidList_01.Count > 0) ++count;
			if (OptTidList_02.Count > 0) ++count;
			if (OptTidList_03.Count > 0) ++count;
			if (OptTidList_04.Count > 0) ++count;

			return count;
		}
	}

	public class AttendEventData
	{
		public E_ATTEND_TYPE mainType;      // 출석 타입
		public E_EVENT_ATTEND_TYPE subType; // 출석 세부타입
		public uint groupId;                // AttendTable 그룹 id
		public bool isOwnPassOne;           // 패스1 소유 여부
		public bool isOwnPassTwo;           // 패스2 소유 여부
		public uint attendPos;				// 출석 위치
		public uint rewardNormalPos;        // 일반 보상 받은 위치
		public uint rewardOnePos;           // 패스1 보상 받은 위치
		public uint rewardTwoPos;           // 패스2 보상 받은 위치
		public ulong rewardDt;              // 보상 받은 시각

		public AttendEventData(AccountAttendInfo attendInfo)
		{
			Reset(attendInfo);
		}

		public void Reset(AccountAttendInfo attendInfo)
		{
			mainType = attendInfo.MainType;
			subType = attendInfo.SubType;
			groupId = attendInfo.GroupId;
			isOwnPassOne = attendInfo.Pass1;
			isOwnPassTwo = attendInfo.Pass2;
			rewardNormalPos = attendInfo.RewardSeq;
			rewardOnePos = attendInfo.RewardSeq1;
			rewardTwoPos = attendInfo.RewardSeq2;
			rewardDt = attendInfo.RewardDt;
			attendPos = attendInfo.AttendSeq;
		}
	}

	public class LoginEventData
	{
		public ulong loginEventDt;
		public bool isHaveMail;
		public List<uint> reward_Item;
		public string bgUrl;
		public string bgHash;
		public string title;
	}

	public class GuildBuffData
	{
		public uint AbilityActionId; //어빌리티 액션 테이블 아이디
		public ulong ExpireDt; //만료 날짜
		public bool IsAutoBuy;

		public GuildBuffData() { }

		public GuildBuffData(GuildBuff buffInfo)
		{
			Reset(buffInfo);
		}

		public void Reset(GuildBuff buffInfo)
		{
			AbilityActionId = buffInfo.AbilityId;
			ExpireDt = buffInfo.ExpireDt;
			IsAutoBuy = buffInfo.IsAutoBuy;
		}

		public bool Equal(GuildBuff buffInfo)
		{
			return AbilityActionId == buffInfo.AbilityId && ExpireDt == buffInfo.ExpireDt && IsAutoBuy == buffInfo.IsAutoBuy;
		}
	}

	public class GuildSimpleData
	{
		public ulong GuildId;
		public E_GuildAllianceState State;

		public GuildSimpleData() { }

		public GuildSimpleData(GuildAllianceSimpleInfo simpleInfo)
		{
			Reset(simpleInfo);
		}

		public void Reset(GuildAllianceSimpleInfo simpleInfo)
		{
			this.GuildId = simpleInfo.GuildId;
			this.State = simpleInfo.State;
		}

		public bool Equal(GuildAllianceSimpleInfo simpleInfo)
		{
			return GuildId == simpleInfo.GuildId && State == simpleInfo.State;
		}
	}

	public class ArtifactData
	{
		public ulong ArtifactID;
		public uint ArtifactTid;
		public uint ArtifactGroupID;
		public uint Step;

		public ArtifactData()
		{ }
	}

	public class Friend
	{
		public ulong CharId;
		public uint CharTid;
		public uint CharLv;
		public string Nick;
		public bool IsFriend;
		public bool IsAlert;
		public E_FriendRequestState friendReqState;
		public bool IsLogin;
		public ulong logoutDt;

		public Friend(FriendInfo friendInfo)
		{
			Reset(friendInfo);
		}
		public void Reset(FriendInfo friendInfo)
		{
			CharId = friendInfo.CharId;
			CharTid = friendInfo.CharTid;
			CharLv = friendInfo.Lv;
			Nick = friendInfo.Nick;
			IsFriend = friendInfo.IsFriend;
			IsAlert = friendInfo.IsAlert;
			IsLogin = friendInfo.IsLogin;
			logoutDt = friendInfo.LogoutDt;
		}

		public Friend(FriendRequestInfo friendInfo)
		{
			Reset(friendInfo);
		}
		public void Reset(FriendRequestInfo friendInfo)
		{
			CharId = friendInfo.CharId;
			CharTid = friendInfo.CharTid;
			CharLv = friendInfo.Lv;
			Nick = friendInfo.Nick;
			friendReqState = friendInfo.State;
			IsLogin = friendInfo.IsLogin;
			logoutDt = friendInfo.LogoutDt;
		}
	}

	public class ChangeQuestData
	{
		public uint QuestTid;
		public ulong StartDt;
		public ulong EndDt;
		public ulong RewardDt;
		public ulong CreateDt;

		public ChangeQuestData(ChangeQuest questInfo)
		{
			Reset(questInfo);
		}

		public void Reset(ChangeQuest questInfo)
		{
			QuestTid = questInfo.QuestTid;
			StartDt = questInfo.StartDt;
			EndDt = questInfo.EndDt;
			RewardDt = questInfo.RewardDt;
			CreateDt = questInfo.CreateDt;
		}
	}

	// 속성 데이터 
	public class AttributeData
	{
		public uint Tid;
		public uint Level;

		public AttributeData() { }
		public AttributeData(uint tid, uint level)
		{
			Tid = tid;
			Level = level;
		}
	}

	public class SkillOrderData
	{
		public uint Tid;
		public uint Order;
		public uint CoolTime;
		public bool IsChanged;
		public byte IsUseSkillCycle;

		public SkillOrderData(uint _tid, uint _order, uint _coolTime, byte _isUseSkillCycle, bool _isChanged = false)
		{
			Tid = _tid;
			Order = _order;
			CoolTime = _coolTime;
			IsUseSkillCycle = _isUseSkillCycle;
			IsChanged = _isChanged;
		}

		public SkillOrderData(SkillUseOrder _data)
		{
			Reset(_data);
		}

		public void Reset(SkillUseOrder _data)
		{
			Tid = _data.SkillTid;
			Order = _data.UseOrder;
			IsUseSkillCycle = _data.IsActive;
			CoolTime = _data.CoolTime;
		}
	}

	public class ExchangeItemData
	{
		public ulong ExchangeID;
		public ulong SellerUserId;
		public ulong ItemID;
		public uint ItemTId;
		public uint ItemCnt;
		public uint ItemTotalPrice;
		public float ItemPrice;
		public ulong StartDt;
		public ulong ExpireDt;
		public uint[] ItemOptions;

		public ExchangeItemData(ExchangeItem exchangeInfo)
		{
			Reset(exchangeInfo);
		}

		public void Reset(ExchangeItem exchangeInfo)
		{
			ExchangeID = exchangeInfo.ExchangeId;
			SellerUserId = exchangeInfo.SellerUserId;
			ItemID = exchangeInfo.ItemId;
			ItemTId = exchangeInfo.ItemTid;
			ItemCnt = exchangeInfo.ItemCnt;
			ItemTotalPrice = exchangeInfo.ItemTotalPrice;
			ItemPrice = exchangeInfo.ItemPrice;
			StartDt = exchangeInfo.SellStartDt;
			ExpireDt = exchangeInfo.ExpireDt;
			ItemOptions = exchangeInfo.GetItemOptionsArray();
		}
	}

	public class SoldOutItemData
	{
		public ulong TransactionId;
		public ulong ExchangeID;
		public ulong ItemID;
		public uint ItemTId;
		public uint ItemCnt;
		public uint ItemTotalPrice;
		public uint ItemTotalPriceTex;
		public float ItemPrice;
		public ulong SellDt;

		public SoldOutItemData(ExchangeTransaction soldoutInfo)
		{
			Reset(soldoutInfo);
		}

		public void Reset(ExchangeTransaction soldoutInfo)
		{
			TransactionId = soldoutInfo.TransactionId;
			ExchangeID = soldoutInfo.ExchangeId;
			ItemID = soldoutInfo.ItemId;
			ItemTId = soldoutInfo.ItemTid;
			ItemCnt = soldoutInfo.ItemCnt;
			ItemTotalPrice = soldoutInfo.ItemTotalPrice;
			ItemTotalPriceTex = soldoutInfo.ItemTotalPriceTax;
			ItemPrice = soldoutInfo.ItemPrice;
			SellDt = soldoutInfo.CreateDt;
		}
	}

	public class RankingUser
	{
		public uint Rank;                // 랭킹
		public uint BeforRank;           // 이전 랭킹
		public uint ServerId;
		public ulong CharId;             // 케릭터 ID
		public uint CharTid;             // 클래스
		public ulong Score;              // 경험치
		public string Nick;              // 닉네임
		public ulong GuildId;            // 길드 아이디
		public string GuildName;         // 길드 이름
		public byte GuildMarkTid;        // 길드 문양

		public RankingUser()
		{
		}

		public RankingUser(RankInfo rankInfo)
		{
			Reset(rankInfo);
		}

		public void Reset(RankInfo rankInfo)
		{
			this.Rank = rankInfo.Rank;
			this.BeforRank = rankInfo.BeforRank;
			this.ServerId = rankInfo.ServerIdx;
			this.CharId = rankInfo.CharId;
			this.CharTid = rankInfo.CharTid;
			this.Score = rankInfo.Score;
			this.Nick = rankInfo.Nick;
			this.GuildId = rankInfo.GuildId;
			this.GuildName = rankInfo.GuildName;
			this.GuildMarkTid = rankInfo.GuildMarkTid;
		}
	}

	public class PkLogData
	{
		public ulong LogID;
		public ulong CharID;
		public uint CharTid;
		public string CharNick;
		public string GuildName;
		public uint GuildMarkTid;
		public bool IsSneer;
		public ulong CreateDt;

		public PkLogData(PkLog pklogInfo)
		{
			Reset(pklogInfo);
		}

		public void Reset(PkLog pklogInfo)
		{
			LogID = pklogInfo.PkId;
			CharID = pklogInfo.DieCharId;
			CharTid = pklogInfo.DieCharTid;
			CharNick = pklogInfo.DieCharNick;
			GuildName = pklogInfo.DieGuildName;
			GuildMarkTid = pklogInfo.DieGuildMarkTid;
			IsSneer = pklogInfo.IsSneer == 1;
			CreateDt = pklogInfo.CreateDt;
		}
	}

	public class MakeLimitData
	{
		public uint MakeTid;
		public uint ServerMakeCnt;
		public uint AccountMakeCnt;
		public uint CharMakeCnt;
		public ulong ExpireDt;
		public byte ReOpenCnt;

		public MakeLimitData(MakeLimitInfo info)
		{
			Reset(info);
		}

		public void Reset(MakeLimitInfo info)
		{
			MakeTid = info.MakeTid;
			ServerMakeCnt = info.ServerMakeCnt;
			AccountMakeCnt = info.AccountMakeCnt;
			CharMakeCnt = info.CharMakeCnt;
			ExpireDt = info.ExpireDt;
		}
	}

	public class CookData
	{
		public uint CookTid;

		public CookData(CookHistory? info)
		{
			Reset(info);
		}

		public void Reset(CookHistory? info)
		{
			CookTid = info.Value.CookTid;
		}
	}

	public class SRewardItemList
	{
		public List<uint> ItemID = new List<uint>();
		public List<uint> ItemCount = new List<uint>();
	}

}