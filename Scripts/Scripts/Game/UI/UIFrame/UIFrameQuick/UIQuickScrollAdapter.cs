using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using ZDefine;
using ZNet.Data;

public class UIQuickScrollAdapter : OSA<BaseParamsWithPrefab, UIQuickItemSlotPageHolder>
{
	public GameObject FocusBG = null;
	public RectTransform DotRect = null;
	public List<GameObject> DotsBG = new List<GameObject>();
	[SerializeField] private List<GameObject> Dots = new List<GameObject>();

	protected override void Start()
	{		
	}

	public SimpleDataHelper<QuickSlotScrollData> Data
	{
		get; private set;
	}

	protected override UIQuickItemSlotPageHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UIQuickItemSlotPageHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	protected override void UpdateViewsHolder(UIQuickItemSlotPageHolder newOrRecycled)
	{
		QuickSlotScrollData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
				UpdateViewsHolder(GetItemViewsHolder(i));
		}
	}

	public void Initialize()
	{
		if (Parameters.ItemPrefab == null)
		{
			GameObject quickSlot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIQuickItemSlotPageHolder));
			Parameters.ItemPrefab = quickSlot.GetComponent<RectTransform>();
			Parameters.ItemPrefab.SetParent(GetComponent<Transform>());
			Parameters.ItemPrefab.transform.localScale = Vector2.one;
			Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
			Parameters.ItemPrefab.gameObject.SetActive(false);
			gameObject.SetActive(true);

			// Dot Setting
			if (Me.CurCharData.QuickSlotMaxCnt < ZUIConstant.QUICK_SLOT_COUNT)
			{
				for (int i = 0; i < DotsBG.Count; i++)
					DotsBG[i].SetActive(false);
				DotRect.anchoredPosition = new Vector2(19.0f, 0.0f);
			}

			if (!IsInitialized)
				Init();
		}

		// Snapper
		ScrollSpeed = 0.9f;
		BaseParameters.Snapper.SnappingEndedOrCancelled += DotChange;
	}

	public void DotChange()
	{
		for (int i = 0; i < Dots.Count; i++)
			Dots[i].SetActive(false);

		Dots[BaseParameters.Snapper._LastSnappedItemIndex == -1 ? 0 : BaseParameters.Snapper._LastSnappedItemIndex].SetActive(true);

		object selectObject = null;
		if (UIManager.Instance.Find(out UIFrameSkill _skill) && _skill.gameObject.activeSelf)
			selectObject = _skill.GetSelectObject();
		else if (UIManager.Instance.Find(out UIFrameInventory _inventory) && _inventory.gameObject.activeSelf)
			selectObject = _inventory.GetSelectObject();

		if(UIManager.Instance.Find(out UISubHUDQuickSlot _quick))
			_quick.SelectQuickSlotEffect(selectObject != null);
	}

	public void SetScrollData(int _continerIdx, int _fixSlotIdx = 0)
	{
		if (Data == null)
			Data = new SimpleDataHelper<QuickSlotScrollData>(this);

		if (!IsInitialized)
			Initialize();

		ClearData();

		#region 사용자 변경 로직
		OptionInfo[] quickData = new OptionInfo[] { Me.CurCharData.GetOptionInfo(WebNet.E_CharacterOptionKey.QuickSlot_Set1),
													Me.CurCharData.GetOptionInfo(WebNet.E_CharacterOptionKey.QuickSlot_Set2) };
		string[,] QuickOption = new string[ZUIConstant.QUICK_SLOT_CONTAINER_COUNT, ZUIConstant.QUICK_SLOT_COUNT];

		if (quickData[_continerIdx] != null && quickData[_continerIdx].OptionValue != null)
		{
			string[] str = quickData[_continerIdx].OptionValue.Split(',');

			for (int j = 0; j < str.Length; j++)
			{
				string[] split = str[j].Split(':');

				if (!split[0].IsNullOrEmpty())
					QuickOption[_continerIdx, Convert.ToInt32(split[0])] = str[j];
			}
		}

		for (int j = 0; j < Me.CurCharData.QuickSlotMaxCnt / ZUIConstant.QUICK_SLOT_PAGE_COUNT; ++j)
		{
			QuickSlotInfo[] listQuickSlotData = new QuickSlotInfo[ZUIConstant.QUICK_SLOT_PAGE_COUNT];
			int[] slotIdx = new int[ZUIConstant.QUICK_SLOT_PAGE_COUNT];
			
			for (int k = 0; k < ZUIConstant.QUICK_SLOT_PAGE_COUNT; ++k)
			{
				if (!QuickOption[_continerIdx, j * ZUIConstant.QUICK_SLOT_PAGE_COUNT + k].IsNullOrEmpty())
				{
					string[] quickDataSlot = QuickOption[_continerIdx, j * ZUIConstant.QUICK_SLOT_PAGE_COUNT + k].Split(':');
					QuickSlotInfo quickSlotInfos = new QuickSlotInfo
					{
						SlotType = (QuickSlotType)Convert.ToInt32(quickDataSlot[1]),
						UniqueID = Convert.ToUInt64(quickDataSlot[2]),
						TableID = Convert.ToUInt32(quickDataSlot[3]),
						bAuto = Convert.ToBoolean(quickDataSlot[4])
					};

					listQuickSlotData[k] = quickSlotInfos;
				}
				else
				{
					listQuickSlotData[k] = null;
				}

				slotIdx[k] = j * ZUIConstant.QUICK_SLOT_PAGE_COUNT + k;
			}

			Data.List.Add(new QuickSlotScrollData() { QuickSlotInfos = listQuickSlotData, ContinerIdx =_continerIdx, HoldIdx = j });
		}
		#endregion

		Data.NotifyListChangedExternally();

		RefreshData();

		ScrollTo(_fixSlotIdx == -1 ? 0 : _fixSlotIdx);
	}

	private void ClearData()
	{
		Data.List.Clear();
	}
}

///<summary>Scroll Item Define (Scroll 전용 사용자 정의 자료구조 선언)</summary>
public class QuickSlotScrollData
{
	public QuickSlotInfo[] QuickSlotInfos = new QuickSlotInfo[ZUIConstant.QUICK_SLOT_PAGE_COUNT];
	public int ContinerIdx;
	public int HoldIdx;
}

public class UIQuickItemSlotPageHolder : BaseItemViewsHolder
{
	public UIQuickItemSlot[] SlotArr = new UIQuickItemSlot[ZUIConstant.QUICK_SLOT_PAGE_COUNT];

	public override void CollectViews()
	{
		base.CollectViews();

		for (int i = 0; i < SlotArr.Length; i++)
			root.GetComponentAtPath("UIQuickItemSlot0" + (i + 1).ToString(), out SlotArr[i]);
	}

	public void UpdateViews(QuickSlotScrollData _model)
	{
		if (_model == null)
			return;

		for (int i = 0; i < SlotArr.Length; i++)
			SlotArr[i].Initialize(_model, i);
	}
}