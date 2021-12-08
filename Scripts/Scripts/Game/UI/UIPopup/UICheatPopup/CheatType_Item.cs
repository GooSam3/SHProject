using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZNet;

public class C_CheatFavoriteHelper
{
	private const string PREFS_KEY = "cheat_favorite";

	private class SaveCheatData
	{
		public uint[] item;
		public uint[] monster;
	}

	private static Dictionary<uint, Item_Table> dicFavoriteItem = new Dictionary<uint, Item_Table>();
	private static Dictionary<uint, Monster_Table> dicFavoriteMonster = new Dictionary<uint, Monster_Table>();

	public static event Action OnFavoriteItemChanged = delegate { };
	public static event Action OnFavoriteMonsterChanged = delegate { };

	private static void SaveData()
	{
		PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(new SaveCheatData
		{
			item = dicFavoriteItem.Keys.ToArray(),
			monster = dicFavoriteMonster.Keys.ToArray()
		}));
	}

	private static SaveCheatData LoadData()
	{
		return JsonUtility.FromJson<SaveCheatData>(PlayerPrefs.GetString(PREFS_KEY));
	}

	public static void Reload()
	{
		if (string.IsNullOrEmpty(PlayerPrefs.GetString(PREFS_KEY)))
		{
			SaveData();
		}

		SaveCheatData json = null;
		json = LoadData();

		if (json.item == null || json.monster == null)// json 형식이 SaveCheatData와 다를시 재생성
		{
			SaveData();


			json = LoadData();
		}

		dicFavoriteItem.Clear();

		foreach (var iter in json.item)
		{
			if (DBItem.GetItem(iter, out Item_Table table) == false)
				continue;
			if (dicFavoriteItem.ContainsKey(table.ItemID))
				continue;

			dicFavoriteItem.Add(iter, table);
		}

		foreach (var iter in json.monster)
		{
			if (DBMonster.TryGet(iter, out Monster_Table table) == false)
				continue;
			if (dicFavoriteMonster.ContainsKey(table.MonsterID))
				continue;

			dicFavoriteMonster.Add(iter, table);
		}
	}

	public static void Save()
	{
		SaveData();
	}

	public static List<Item_Table> GetAllFavoriteItem()
	{
		return dicFavoriteItem.Values.ToList();
	}

	public static bool HasItemValue(Item_Table table)
	{
		return dicFavoriteItem.ContainsKey(table.ItemID);
	}

	public static bool AddFavoriteItem(Item_Table table)
	{
		if (dicFavoriteItem.ContainsKey(table.ItemID))
			return false;

		dicFavoriteItem.Add(table.ItemID, table);
		OnFavoriteItemChanged?.Invoke();

		return true;
	}

	public static bool RemoveFavoriteItem(Item_Table table)
	{
		if (dicFavoriteItem.ContainsKey(table.ItemID) == false)
			return false;

		dicFavoriteItem.Remove(table.ItemID);
		OnFavoriteItemChanged?.Invoke();

		return true;
	}

	public static List<Monster_Table> GetAllFavoriteMonster()
	{
		return dicFavoriteMonster.Values.ToList();
	}

	public static bool HasMonsterValue(Monster_Table table)
	{
		return dicFavoriteMonster.ContainsKey(table.MonsterID);
	}

	public static bool AddFavoriteMonster(Monster_Table table)
	{
		if (dicFavoriteMonster.ContainsKey(table.MonsterID))
			return false;

		dicFavoriteMonster.Add(table.MonsterID, table);
		OnFavoriteMonsterChanged?.Invoke();


		return true;
	}

	public static bool RemoveFavoriteMonster(Monster_Table table)
	{
		if (dicFavoriteMonster.ContainsKey(table.MonsterID) == false)
			return false;

		dicFavoriteMonster.Remove(table.MonsterID);
		OnFavoriteMonsterChanged?.Invoke();

		return true;
	}


	// 슬롯을 공유하는것때문에.. 밖으로뺐습니다..
	// ex) 즐겨찾기 리스트에서 즐겨찾기 해제시 이벤트호출 -> 슬롯변경 -> 슬롯이미지변경
	public static void InvokeItem()
	{
		OnFavoriteItemChanged?.Invoke();
	}

	public static void InvokeMonster()
	{
		OnFavoriteMonsterChanged?.Invoke();
	}
}

public class CheatType_Item : CheatPanel
{
	private class Itemcheat
	{
		public uint item_tid;
		public uint cnt;

		public Itemcheat() { }
		public Itemcheat(uint _tid, uint _cnt)
		{
			item_tid = _tid;
			cnt = _cnt;
		}
	}

	[SerializeField] private GameObject noticeFavorite;

	[SerializeField] private Dropdown dropDown;
	[SerializeField] private InputField inputField;

	[SerializeField] private Image favoriteButtonImg;

	[SerializeField] private UICheatListAdapter osaShopItem;
	[SerializeField] private UICheatListAdapter osaWishItem;
	[SerializeField] private UICheatListAdapter osaTab;

	[SerializeField] private CheatItemDetailPopup detailPopup;

	private List<OSA_CheatData> listAllItem = new List<OSA_CheatData>();

	private List<OSA_CheatData> listWistData = new List<OSA_CheatData>();


	//---filter

	private E_CharacterType charType = E_CharacterType.All;

	private string searchValue = string.Empty;

	//---------

	private bool isFavoriteMode = false;

	private bool isEnchantMerge = true;

	private Action postAction = null;
	private bool isInitialized = false;

	public override void Initialize()
	{
		if (DBItem.GetAllItem(out var list) == false)
		{
			ZLog.LogError(ZLogChannel.Default, "치트 : 아이템데이터가 없슴다!!");
		}
		else
		{
			foreach (var iter in list)
			{
				listAllItem.Add(new OSA_CheatData() { type = E_CheatDataType.Item_Shop, itemTable = iter });
			}
		}

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICheatListItem), obj =>
		{
			osaShopItem.Initialize(OnClickShopItem, OnQuickItem);
			osaWishItem.Initialize(OnClickWishItem, OnQuickItem);
			osaTab.Initialize(OnClickTab);

			var listTab = new List<OSA_CheatData>();

			listTab.Add(new OSA_CheatData() { type = E_CheatDataType.Item_Tab, itemType = 0, isSelectable = true });

			foreach (var iter in EnumHelper.Values<E_ItemType>())
			{
				listTab.Add(new OSA_CheatData() { type = E_CheatDataType.Item_Tab, itemType = iter, isSelectable = true });
			}
			osaTab.ResetListData(listTab);

			postAction?.Invoke();

			isInitialized = true;

			ZPoolManager.Instance.Return(obj);
		});
	}

	public override void SetActive(bool state)
	{
		if (isInitialized == false)
		{
			postAction = () => SetActive(true);
			return;
		}

		if (state)
		{
			C_CheatFavoriteHelper.OnFavoriteItemChanged += RefreshFavoriteList;

			OnClickFavorite();

			osaWishItem.ResetListData(new List<OSA_CheatData>());
		}
		else
		{
			C_CheatFavoriteHelper.OnFavoriteItemChanged -= RefreshFavoriteList;
		}

		base.SetActive(state);
	}

	private void OnClickTab(OSA_CheatData data)
	{
		if (isFavoriteMode)
			SetFavoriteTab(false);

		searchValue = string.Empty;
		inputField.SetTextWithoutNotify(string.Empty);

		RefreshFilteredData();
	}

	private void OnClickShopItem(OSA_CheatData data)
	{
		detailPopup.Open(data.itemTable, (cnt, enchant) =>
		{
			if (cnt <= 0)
				return;

			var table = DBItem.GetItem(data.itemTable.ItemID + ((uint)enchant));

			if (table.ItemUseType == E_ItemUseType.Equip ||
				table.ItemUseType == E_ItemUseType.Rune)
			{
				osaWishItem.Data.InsertOneAtEnd(new OSA_CheatData() { type = E_CheatDataType.Item_Wish, itemTable = table, count = 1 });
			}
			else
			{
				var idx = osaWishItem.Data.List.FindIndex(item => item.itemTable.ItemID == table.ItemID);

				if (idx < 0)
				{
					osaWishItem.Data.InsertOneAtEnd(new OSA_CheatData() { type = E_CheatDataType.Item_Wish, itemTable = table, count = (ulong)cnt });
				}
				else
				{
					osaWishItem.Data.List[idx].count = (ulong)cnt;
				}
			}

			osaWishItem.RefreshData();
		});
	}

	private void OnClickWishItem(OSA_CheatData data)
	{
		detailPopup.Open(data, (count, enchant) =>
		{
			if ((ulong)count == data.count)
			{
				var idx = osaWishItem.Data.List.FindIndex(item => item.itemTable.ItemID == data.itemTable.ItemID);

				osaWishItem.Data.RemoveOne(idx);

				osaWishItem.RefreshData();
			}
		});

	}

	private void OnQuickItem(OSA_CheatData data)
	{
		switch (data.type)
		{
			case E_CheatDataType.Item_Shop:

				if (data.itemTable.ItemUseType == E_ItemUseType.Equip ||
					data.itemTable.ItemUseType == E_ItemUseType.Rune)
					osaWishItem.Data.InsertOneAtEnd(new OSA_CheatData() { type = E_CheatDataType.Item_Wish, itemTable = data.itemTable, count = 1 });
				else
				{
					var idx = osaWishItem.Data.List.FindIndex(item => item.itemTable.ItemID == data.itemTable.ItemID);

					if (idx < 0)
					{
						osaWishItem.Data.InsertOneAtEnd(new OSA_CheatData() { type = E_CheatDataType.Item_Wish, itemTable = data.itemTable, count = 1 });
					}
					else
					{
						osaWishItem.Data.List[idx].count += 1;
					}
				}

				osaWishItem.RefreshData();
				break;

			case E_CheatDataType.Item_Wish:
				if (data.count == 1)
				{
					var idx = osaWishItem.Data.List.FindIndex(item => item.itemTable.ItemID == data.itemTable.ItemID);

					if (idx >= 0)
						osaWishItem.Data.RemoveOne(idx);
				}
				else
				{
					data.count -= 1;
				}

				break;
		}

		osaWishItem.RefreshData();

	}

	// FAVORITE

	public void OnClickFavorite()
	{
		osaTab.SetSelectedData(null);

		dropDown.SetValueWithoutNotify(0);

		SetFavoriteTab(true);

		RefreshFavoriteList();
	}

	private void SetFavoriteTab(bool state)
	{
		isFavoriteMode = state;
		favoriteButtonImg.color = state ? Color.cyan : Color.white;
		RefreshFavoriteList();
	}

	private void RefreshFavoriteList()
	{
		List<Item_Table> favorite = C_CheatFavoriteHelper.GetAllFavoriteItem();
		noticeFavorite.SetActive(isFavoriteMode && favorite.Count <= 0);

		if (isFavoriteMode == false)
			return;

		List<OSA_CheatData> listFavorite = new List<OSA_CheatData>();

		foreach (var iter in favorite)
		{
			listFavorite.Add(new OSA_CheatData() { type = E_CheatDataType.Item_Shop, itemTable = iter });
		}

		osaShopItem.ResetListData(listFavorite);
	}

	// SEARCH

	public void RefreshFilteredData()
	{
		List<OSA_CheatData> listData = new List<OSA_CheatData>();

		if (string.IsNullOrEmpty(searchValue) == false) // 검색
		{
			// 검색시 charType = all, tab = all
			dropDown.SetValueWithoutNotify(0);
			charType = E_CharacterType.All;
			osaTab.SelectFirst(false);

			if (uint.TryParse(searchValue, out uint tid))
			{
				if (DBItem.GetItem(tid, out var table))
				{
					listData.Add(new OSA_CheatData() { type = E_CheatDataType.Item_Shop, itemTable = table });
				}
			}
			else
			{
				listData.AddRange(listAllItem.FindAll((item) =>
				{
					if (DBLocale.TryGet(item.itemTable.ItemTextID, out var table) == false)
						return false;

					return table.Text.Contains(searchValue);
				}));
			}
		}
		else// 필터링
		{
			var filterTab = osaTab.selectedData?.itemType ?? 0;

			listData.AddRange(listAllItem.FindAll(item =>
			{
				// 강화 가능 그룹 병합
				if (isEnchantMerge && item.itemTable.ItemUseType == E_ItemUseType.Equip && item.itemTable.GroupID != item.itemTable.ItemID)
					return false;

				if (charType == E_CharacterType.All)
				{
					if (filterTab == 0)
						return true;
					else
						return item.itemTable.ItemType == filterTab;
				}
				else if (item.itemTable.UseCharacterType.HasFlag(charType))
				{
					if (filterTab == 0)
						return true;
					else
						return item.itemTable.ItemType == filterTab;
				}
				return false;
			}));
		}

		osaShopItem.ResetListData(listData);
	}

	public void SetSearchInputValue(string str)
	{
		searchValue = str;

		SetFavoriteTab(false);

		RefreshFilteredData();
	}
	public void OnClickSearchButton()
	{
		SetSearchInputValue(inputField.text);
	}

	public void OnSortByClass(int idx)
	{
		if (idx == 0)
			charType = E_CharacterType.All;
		else
			charType = (E_CharacterType)(1 << (idx - 1));

		RefreshFilteredData();
	}

	//---------------장바구니

	public void OnEnchantMergeValueChanged(bool b)
	{
		isEnchantMerge = b;

		if(isFavoriteMode == false)
			RefreshFilteredData();
	}

	public void OnClickClearWishList()
	{
		osaWishItem.ResetListData(new List<OSA_CheatData>());
	}

	public void OnClickClearInven()
	{
		List<ZDefine.ZItem> deletingItems = new List<ZDefine.ZItem>();
		var invenList = ZNet.Data.Me.CurCharData.InvenList;
		int invenCnt = invenList.Count;
		for (int i = 0; i < invenCnt; i++)
		{
			ZDefine.ZItem item = invenList[i];

			// 삭제 가능 아이템 필터링.
			if (null != item && ZDefine.ZItem.IsDeletable(item))
			{
				deletingItems.Add(item);
			}
		}

		ZWebManager.Instance.WebGame.REQ_DeleteItem(deletingItems, (recvPacket, recvMsgPacket) =>
		{
			UICommon.SetNoticeMessage("인벤 비우기 성공", Color.red, 3f, UIMessageNoticeEnum.E_MessageType.SubNotice);
		});
	}

	//----------------REQ

	private void SendCheatData(List<Itemcheat> cheatData)
	{
		ZWebManager.Instance.WebGame.REQ_CheatSend(1000, TinyJSON.JSON.Dump(cheatData), delegate (ZWebRecvPacket recvPacket, WebNet.ResCheatSendMail recvMsgPacket)
		{
			ZWebManager.Instance.WebGame.REQ_GetMailList(delegate
			{
				if (UIManager.Instance.Find(out UIFrameMailbox _mailbox))
					_mailbox.RefreshMailList(true);

				UIMessagePopup.ShowPopupOk("우편으로 아이템이 발송되었습니다.");
			});
		});
	}
	public void OnRequest()
	{
		var wishlist = osaWishItem.Data.List;

		if (wishlist.Count <= 0)
		{
			ZLog.LogWarn(ZLogChannel.Default, "장바구니가 비었슴다");
			return;
		}

		List<Itemcheat> cheatData = new List<Itemcheat>();

		foreach (var iter in wishlist)
		{
			uint count = (uint)iter.count;
			if (iter.itemTable.ItemUseType == E_ItemUseType.Goods)
				count *= 100;

			cheatData.Add(new Itemcheat(iter.itemTable.ItemID, count));
		}

		SendCheatData(cheatData);
	}

	public void ReqSkillBook(int _type)
	{
		List<Skill_Table> table = new List<Skill_Table>();

		List<Itemcheat> cheatData = new List<Itemcheat>();

		switch ((E_CharacterType)_type)
		{
			case E_CharacterType.Knight: table = DBSkill.GetSkillListAll((E_CharacterType)_type, E_WeaponType.Sword); break;
			case E_CharacterType.Assassin: table = DBSkill.GetSkillListAll((E_CharacterType)_type, E_WeaponType.TwoSwords); break;
			case E_CharacterType.Archer: table = DBSkill.GetSkillListAll((E_CharacterType)_type, E_WeaponType.Bow); break;
			case E_CharacterType.Wizard: table = DBSkill.GetSkillListAll((E_CharacterType)_type, E_WeaponType.Wand); break;
			case E_CharacterType.All: table = DBSkill.GetSkillListAll((E_CharacterType)_type, E_WeaponType.None); break;
		}

		if (null == table)
			return;

		for (int i = 0; i < table.Count; i++)
		{
			if (table[i].OpenItemID != 0)
			{
				Itemcheat item = new Itemcheat
				{
					item_tid = table[i].OpenItemID,
					cnt = 1
				};

				cheatData.Add(item);
			}
		}

		SendCheatData(cheatData);
	}

}