using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WebNet;

public class CheatType_Spawn : CheatPanel
{
	private const string CHEAT_KEY_FORM = "/addmon {0} {1} {2}";

	[SerializeField] private GameObject noticeFavorite;

	[SerializeField] private InputField inputField;

	[SerializeField] private UICheatListAdapter osaMonsterType;
	[SerializeField] private UICheatListAdapter osaMonster;

	[SerializeField] private Dropdown dropSpawnCount;
	[SerializeField] private Dropdown dropSpawnRange;

	[SerializeField] private Image favoriteButtonImg;


	private List<OSA_CheatData> listAllMonster = new List<OSA_CheatData>();

	private string searchValue = string.Empty;

	private bool isFavoriteMode = false;

	private Action postAction = null;
	private bool isInitialized = false;


	public override void Initialize()
	{
		try
		{
			foreach (var iter in DBMonster.DicMonster.Values.ToList())
			{
				listAllMonster.Add(new OSA_CheatData() { type = E_CheatDataType.Monster, monsterTable = iter });
			}
		}
		catch
		{
			ZLog.LogError(ZLogChannel.Default, "치트 : 몬스터 데이터가 없슴다!!");
		}

		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UICheatListItem), obj =>
		{
			osaMonster.Initialize(OnClickSpawn);
			osaMonsterType.Initialize(OnClickMonsterType);

			var listTab = new List<OSA_CheatData>();

			listTab.Add(new OSA_CheatData() { type = E_CheatDataType.Monster_Tab, monsterType = (E_MonsterType)(-1), isSelectable = true });


			foreach (var iter in EnumHelper.Values<E_MonsterType>())
			{
				listTab.Add(new OSA_CheatData() { type = E_CheatDataType.Monster_Tab, monsterType = iter, isSelectable = true });
			}

			osaMonsterType.ResetListData(listTab);

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
			C_CheatFavoriteHelper.OnFavoriteMonsterChanged += RefreshFavoriteList;

			OnClickFavorite();
		}
		else
		{
			C_CheatFavoriteHelper.OnFavoriteMonsterChanged -= RefreshFavoriteList;
		}
		base.SetActive(state);
	}

	public void OnClickSearchButton()
	{
		SetSearchInputValue(inputField.text);
	}

	public void SetSearchInputValue(string str)
	{
		searchValue = str;
		SetFavoriteTab(false);

		RefreshFilteredData();
	}


	public void RefreshFilteredData()
	{
		List<OSA_CheatData> listData = new List<OSA_CheatData>();

		if (string.IsNullOrEmpty(searchValue) == false) // 검색
		{
			osaMonsterType.SelectFirst(false);

			if (uint.TryParse(searchValue, out uint tid))
			{
				if (DBMonster.TryGet(tid, out var table))
				{
					listData.Add(new OSA_CheatData() { type = E_CheatDataType.Monster, monsterTable = table });
				}
			}
			else
			{
				listData.AddRange(listAllMonster.FindAll((monster) =>
				{
					if (DBLocale.TryGet(monster.monsterTable.MonsterTextID, out var table) == false)
						return false;

					return table.Text.Contains(searchValue);
				}));
			}
		}
		else// 필터링
		{
			E_MonsterType filterTab = osaMonsterType.selectedData?.monsterType ?? (E_MonsterType)(-1);

			listData.AddRange(listAllMonster.FindAll(item =>
			{
				if ((int)filterTab == -1)
					return true;

				return item.monsterType == filterTab;
			}));
		}
		osaMonster.ResetListData(listData);
	}

	private void SetFavoriteTab(bool state)
	{
		isFavoriteMode = state;
		favoriteButtonImg.color = state ? Color.cyan : Color.white;
		RefreshFavoriteList();
	}

	private void OnClickMonsterType(OSA_CheatData monster)
	{
		if (isFavoriteMode)
			SetFavoriteTab(false);


		searchValue = string.Empty;
		inputField.SetTextWithoutNotify(string.Empty);

		RefreshFilteredData();
	}

	public void OnClickFavorite()
	{
		osaMonsterType.SetSelectedData(null);
		SetFavoriteTab(true);

		RefreshFavoriteList();
	}


	public void OnClickSpawn(OSA_CheatData data)
	{
		int count = 1;
		int range = 5;

		string strCnt = dropSpawnCount.options[dropSpawnCount.value].text;
		string strRange = dropSpawnRange.options[dropSpawnRange.value].text;

		int.TryParse(strCnt, out count);
		int.TryParse(strRange, out range);

		ReqSpawn(data.monsterTable.MonsterID, count, range);
	}

	private void RefreshFavoriteList()
	{

		List<Monster_Table> favorite = C_CheatFavoriteHelper.GetAllFavoriteMonster();
		noticeFavorite.SetActive(isFavoriteMode && favorite.Count <= 0);
		if (isFavoriteMode == false)
			return;



		List<OSA_CheatData> listFavorite = new List<OSA_CheatData>();

		foreach (var iter in favorite)
		{
			listFavorite.Add(new OSA_CheatData() { type = E_CheatDataType.Monster, monsterTable = iter });
		}

		osaMonster.ResetListData(listFavorite);
	}


	private void ReqSpawn(uint tid, int count, int range)
	{
		string msg = string.Format(CHEAT_KEY_FORM, tid, count, range);

		ZMmoManager.Instance.Field.REQ_MapChat(ZPawnManager.Instance.MyEntityId, msg);

		UIMessagePopup.ShowPopupOk("몬스터 소환 요청됬습니다.");
	}
}
