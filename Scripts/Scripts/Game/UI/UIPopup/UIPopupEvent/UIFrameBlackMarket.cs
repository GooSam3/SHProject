using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;

public class UIFrameBlackMarket : ZUIFrameBase
{
	[SerializeField] private RawImage imgBG;			// 배경
	[SerializeField] private Text txtBlackMarketDesc;	// 배경화면의 대사
	[SerializeField] private Text txtRemainTime;        // 남은시간 HH:MM:SS

	[SerializeField] private GameObject objBlackMarketEnd; // 암시장 종료 팝업

	[SerializeField] private UIBlackMarketListAdapter osaBlackMarket; // 암시장 osa

	private UISpecialShopItemInfoPopUp infoPopup = null;
	public override bool IsBackable => true;
	private BlackMarketData data;

	protected override void OnShow(int _LayerOrder)
	{
		objBlackMarketEnd.SetActive(false);

		UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
		UIManager.Instance.TopMost<UISubHUDCurrency>(true);
		UIManager.Instance.Find<UISubHUDCurrency>().ShowSpecialCurrency(
			new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID, DBConfig.Essence_ID });

		UIManager.Instance.ShowGlobalIndicator(true);
		base.OnShow(_LayerOrder);
	}

	protected override void OnHide()
	{
		base.OnHide();
		
		UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
		UIManager.Instance.TopMost<UISubHUDCurrency>(true);

		UIManager.Instance.Find<UISubHUDCurrency>().ShowBaseCurrency();


		if (infoPopup != null)
		{
			ZPoolManager.Instance.Return(infoPopup.gameObject);
			infoPopup = null;
		}
	}

	public void SetMarketData(BlackMarketData _eventData)
	{
		data = _eventData;

		LoadBGTexture();

		InvokeRepeating(nameof(RefreshRemainTime), 0f, 1f);

		var listSpecialShop = new List<SpecialShop_Table>();

		foreach(var iter in DBSpecialShop.GetShopList(E_SpecialShopType.BlackMarket))
		{
			if (iter.GroupID != _eventData.GroupId)
				continue;

			listSpecialShop.Add(iter);
		}

		if(osaBlackMarket.IsInitialized == false)
		{
			ZPoolManager.Instance.Spawn<UIBlackMarketListItem>(E_PoolType.UI, delegate
			{
				osaBlackMarket.Initialize(OnClickMarketItem);
				InitOSA(listSpecialShop);
			});
		}
		else
		{
			InitOSA(listSpecialShop);
		}
	}

	private void InitOSA(List<SpecialShop_Table> listSpecialShop)
	{
		osaBlackMarket.ResetListData(listSpecialShop);
		UIManager.Instance.ShowGlobalIndicator(false);

	}

	private void LoadBGTexture()
	{
		if (string.IsNullOrEmpty(data.EventData.bgUrl))
		{
			ZLog.Log(ZLogChannel.Event, $"BgUrl is Empty!! this type : {data.EventData.SubCategory}");
			imgBG.texture = null;
		}
		else
		{
			ZResourceManager.Instance.GetTexture2DFromUrl(data.EventData.bgUrl,
								data.EventData.bgHash,
								(tex) => imgBG.texture = tex, $"{nameof(E_ServerEventCategory)}_{data.EventData.Category}");
		}
	}

	private void RefreshRemainTime()
	{
		txtRemainTime.text = data.GetRemainString();

		if (data.IsOpen == false)
		{
			objBlackMarketEnd.SetActive(true);
			CancelInvoke();

			return;
		}
	}

	private void OnClickMarketItem(SpecialShop_Table data)
	{
		if (infoPopup != null)
		{
			SetInfoPopup(data);
			infoPopup.gameObject.SetActive(true);
		}
		else
		{
			ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UISpecialShopItemInfoPopUp), (obj) => {
				var target = obj.GetComponent<UISpecialShopItemInfoPopUp>();

				if (target != null)
				{
					infoPopup = target;
					obj.transform.SetParent(this.transform);
					obj.transform.SetAsLastSibling();
					obj.transform.SetLocalTRS(Vector3.zero, Quaternion.identity, Vector3.one);
					target.RectTransform.MatchParentSize(false);
					SetInfoPopup(data);
				}
			});
		}
	}

	private void SetInfoPopup(SpecialShop_Table data)
	{
		infoPopup.SetupAuto_NonCash(new UISpecialShopItemInfoPopUp.DisplayItemParam(data.SpecialShopID), 
			delegate
			{
				osaBlackMarket.Refresh();
			});
	}

	public void OnClickClose()
	{
		UIManager.Instance.Close<UIFrameBlackMarket>(true);
	}
}
