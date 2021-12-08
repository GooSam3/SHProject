using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using System;
using static SpecialShopCategoryDescriptor;
using frame8.Logic.Misc.Other.Extensions;

public class UIColosseumShop : MonoBehaviour, ITabContents
{
	private enum E_ShopTabType
	{
		Small,
		Medium,
		Large,
	}

	[SerializeField] private UIColosseumShopItemAdapter ScrollAdapter;
	[SerializeField] private RectTransform overlayRoot;
	[SerializeField] ZToggle firstTabToggle;
	[SerializeField] ZText[] tabTitleList;

	private UISpecialShopItemInfoPopUp infoPopUp;

	// 콜로세움 상점 전용 주화
	private const uint Consume_Coin1_ID = 2650;
	private const uint Consume_Coin2_ID = 2660;
	private const uint Consume_Coin3_ID = 2670;

	public int Index { get; set; }

	private E_ShopTabType currTabType = E_ShopTabType.Small;

	public void Initialize()
	{
		tabTitleList[0].text = DBLocale.GetText("ColosseumCoin03_Name");
		tabTitleList[1].text = DBLocale.GetText("ColosseumCoin02_Name");
		tabTitleList[2].text = DBLocale.GetText("ColosseumCoin01_Name");

		ScrollAdapter.Initialize(OnListItemClicked);
	}

	public void Open()
	{
		this.gameObject.SetActive(true);

		firstTabToggle.SelectToggle(false);
		OpenSubContents(E_ShopTabType.Small);
	}

	public void Refresh()
	{
	}

	public void Close()
	{
		UIManager.Instance.Find<UISubHUDCurrency>().ShowBaseCurrency();

		if (infoPopUp != null) {
			infoPopUp.gameObject.SetActive(false);
		}

		this.gameObject.SetActive(false);
	}

	private void OpenSubContents(E_ShopTabType openTabType)
	{
		currTabType = openTabType;

		E_SpecialSubTapType subTapType = E_SpecialSubTapType.Colosseum_Small;
		switch (currTabType) {
			case E_ShopTabType.Small: {
				subTapType = E_SpecialSubTapType.Colosseum_Small;

				UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
					new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, Consume_Coin1_ID });
				break;
			}
			case E_ShopTabType.Medium: {
				subTapType = E_SpecialSubTapType.Colosseum_Medium;

				UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
					new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, Consume_Coin2_ID });
				break;
			}
			case E_ShopTabType.Large: {
				subTapType = E_SpecialSubTapType.Colosseum_Large;

				UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
					new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, Consume_Coin3_ID });
				break;
			}
		}

		Me.CurCharData.ColosseumContainer.GetShopItemInfoList(subTapType, (shopList) => {
			ScrollAdapter.SetNormalizedPosition(1);
			ScrollAdapter.RefreshShopItemList(shopList);
		});
	}

	// 버튼 이벤트

	public void OnToggleValueChanged(int type)
	{
		if (currTabType == (E_ShopTabType)type) {
			return;
		}

		OpenSubContents((E_ShopTabType)type);
	}

	private void OnListItemClicked(SingleDataInfo info)
	{
		ZLog.Log(ZLogChannel.UI, "Clicked Shop Item , TID : " + info.specialShopId);

		if (infoPopUp != null) {
			SetupInfoPopUp(info);
			infoPopUp.gameObject.SetActive(true);
		}
		else {
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UISpecialShopItemInfoPopUp), (obj) => {
				var target = obj.GetComponent<UISpecialShopItemInfoPopUp>();

				if (target != null) {
					infoPopUp = target;
					obj.transform.SetParent(overlayRoot);
					obj.transform.SetAsLastSibling();
					obj.transform.localPosition = Vector3.zero;
					obj.transform.localScale = Vector3.one;
					target.RectTransform.MatchParentSize(false);
					SetupInfoPopUp(info);
				}
			});
		}
	}

	void SetupInfoPopUp(SingleDataInfo info)
	{
		infoPopUp.SetupAuto_NonCash(
			new UISpecialShopItemInfoPopUp.DisplayItemParam(info.specialShopId)
			, onPreDirecting: () => { }
			, onPurchased_nonCash: (resList) => { } 
		   , onClosed: () => 
		   { 
		   });
	}
}
