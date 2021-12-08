using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UITradePopupSellInfo : MonoBehaviour
{
	public enum E_SellInputType
	{
		Count = 0,
		Price = 1,
	}

	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Text Name;
	[SerializeField] private Text Tex;
	[SerializeField] private Text[] Ablilty = new Text[ZUIConstant.ITEM_SMELT_COUNT]; // 제련 옵션
	[SerializeField] private Text[] AbliltyValue = new Text[ZUIConstant.ITEM_SMELT_COUNT]; // 제련 옵션
	[SerializeField] private Text[] GetWay = new Text[ZUIConstant.ITEM_GET_WAY_COUNT]; // 획득처
	[SerializeField] private Text TradeMinPrice;
	[SerializeField] private Text TradeMaxPrice;
	[SerializeField] private Text SellTradeCount;
	[SerializeField] private Text SellPriceSea; // 개당 판매 금액
	[SerializeField] private Text SellPriceTotal; // 총 판매 금액
	[SerializeField] private Text SellInputValue;
	[SerializeField] private Text SellTexTitle;
	[SerializeField] private Text SellTex; // 판매 수수료
	#endregion

	#region System Variable
	[SerializeField] private ZItem TradeData;
	[SerializeField] private uint InputValue;
	[SerializeField] private E_SellInputType CurInputType = E_SellInputType.Count;
	#endregion

	private void OnEnable()
	{
		UIManager.Instance.Close<UISubHUDCurrency>();
	}

	private void OnDisable()
	{
		UIManager.Instance.Open<UISubHUDCurrency>();
	}

	public void Initialize(ZItem _item, float _minPrice, float _maxPrice)
	{
		if (_item == null)
			return;

		gameObject.SetActive(true);

		TradeData = _item;

		var itemData = DBItem.GetItem(TradeData.item_tid);

		Icon.sprite = UICommon.GetItemIconSprite(_item.item_tid);
		Name.text = DBLocale.GetText(itemData.ItemTextID);
		Tex.text = DBConfig.Exchange_Selling_Commission + "%";
		TradeMinPrice.text = _minPrice.ToString();
		TradeMaxPrice.text = _maxPrice.ToString();
		SellTradeCount.text = 1.ToString();
		SellPriceSea.text = 0.ToString();
		SellPriceTotal.text = 0.ToString();
		SellTexTitle.text = string.Format(DBLocale.GetText("Sales_Commission"), DBConfig.Exchange_Selling_Commission);
		SellTex.text = 0.ToString();
		SellInputValue.text = 1.ToString();

		ClickSellInput((int)E_SellInputType.Price);

		UpdateNum();

		for(int i = 0; i < Ablilty.Length; i++)
		{
			Ablilty[i].text = string.Empty;
			AbliltyValue[i].text = string.Empty;
		}

		for (int i = 0; i < _item.Options.Count; i++)
		{
			if (false == DBResmelting.GetResmeltScrollOption(_item.Options[i], out var table))
			{
				Ablilty[i].text = "-";
				AbliltyValue[i].text = "-";
				continue;
			}

			var abilityActionTable = DBAbility.GetAction(table.AbilityActionID);
			string abilityName = DBAbility.GetAbilityName(abilityActionTable.AbilityID_01);
			uint abilityId = (uint)abilityActionTable.AbilityID_01;
			float abilityValue = abilityActionTable.AbilityPoint_01_Min;

			Ablilty[i].text = abilityName;
			AbliltyValue[i].text = DBAbility.ParseAbilityValue(abilityId, abilityValue).ToString();
		}

		for (int i = 0; i < GetWay.Length; i++)
		{
			// to do :아직 테이블에 정보가 없어서 미기입
			GetWay[i].text = "-";
		}
	}

	public void ClickNum(int _num)
	{
		if (Convert.ToUInt32(SellInputValue.text) == 0)
			InputValue = Convert.ToUInt32(_num);
		else
		{
			if (InputValue > 0)
			{
				InputValue *= 10;
				InputValue += Convert.ToUInt32(_num);

				if (InputValue > DBConfig.Exchange_SellPrice_Max)
					InputValue = DBConfig.Exchange_SellPrice_Max;
			}
		}

		UpdateNum();
	}

	public void ClickBack()
	{
		if (InputValue > 9)
			InputValue /= 10;
		else
			InputValue = 0;

		UpdateNum();
	}

	public void ClickMax()
	{
		InputValue = DBConfig.Exchange_SellPrice_Max;
		UpdateNum();
	}

	public void ClickClose()
	{
		gameObject.SetActive(false);
	}

	public void ClickConfirm()
	{
		if(SellTradeCount.text == string.Empty || SellPriceTotal.text == string.Empty || 
		   Convert.ToUInt32(SellPriceTotal.text) > DBConfig.Exchange_SellPrice_Max || 
		   Convert.ToUInt32(SellPriceTotal.text) < DBConfig.Exchange_SellPrice_Min ||
		   Convert.ToUInt32(SellTradeCount.text) > TradeData.cnt)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("판매 수량 및 가격을 확인해주세요."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		if(Convert.ToUInt32(SellTradeCount.text) >= DBConfig.Exchange_SellRegister_Max)
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("판매 등록한 물품 수량이 최대입니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		ZItem havItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Gold_ID);

		if(havItem.cnt < Convert.ToUInt64(SellTex.text))
		{
			UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
				_popup.Open(ZUIString.WARRING, DBLocale.GetText("수수료가 부족합니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
					_popup.Close(); } });
			});
			return;
		}

		UICommon.OpenSystemPopup((UIPopupSystem _popupBuy) => {
			_popupBuy.Open(ZUIString.WARRING, string.Format(DBLocale.GetText("Exchange_Registration_Message"), DBLocale.GetText(DBItem.GetItem(TradeData.item_tid).ItemTextID), SellPriceTotal.text, SellTex.text), new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate{ _popupBuy.Close(); }, delegate {
					_popupBuy.Close();
					ZWebManager.Instance.WebGame.REQ_ExchangeSellRegist(TradeData.item_id, TradeData.item_tid, Convert.ToUInt32(SellTradeCount.text), Convert.ToUInt32(SellPriceTotal.text), havItem.item_id, (recvPacket, recvPacketMsg) =>
					{
						UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
							_popup.Open("거래소", DBLocale.GetText("판매 등록에 성공하였습니다."), new string[] { ZUIString.LOCALE_OK_BUTTON }, new Action[] { delegate {
							_popup.Close(); } });
						});

						if (UIManager.Instance.Find(out UIFrameTrade _trade))
						{
							_trade.ListUpScrollAdapter.SetData();
							_trade.ListUpInvenScrollAdapter.SetData(_trade.CurSearchInvenType, delegate { _trade.ListUpInvenScrollAdapter.RefreshData(); });
							_trade.SellInfoPopup.gameObject.SetActive(false);
						}
				    });
			} });
		});
	}

	public void ClickChangeNum(int _num)
	{
		if (InputValue + _num < 0)
			InputValue = 0;
		else if (InputValue + _num > DBConfig.Exchange_SellPrice_Max)
			InputValue = DBConfig.Exchange_SellPrice_Max;
		else
			InputValue += (uint)_num;

		UpdateNum();
	}

	public void ClickSellInput(int _idx)
	{
		CurInputType = (E_SellInputType)_idx;

		switch(CurInputType)
		{
			case E_SellInputType.Count:
				InputValue = Convert.ToUInt32(SellTradeCount.text);
				break;

			case E_SellInputType.Price:
				InputValue = Convert.ToUInt32(SellPriceTotal.text);
				break;
		}

		UpdateNum();
	}

	private void UpdateNum()
	{
		SellInputValue.text = InputValue.ToString();

		switch (CurInputType)
		{
			case E_SellInputType.Count:
				if (InputValue <= 0)
					InputValue = 1;

				SellTradeCount.text = InputValue.ToString();
				break;

			case E_SellInputType.Price:
				if (InputValue > DBConfig.Exchange_SellPrice_Max)
					InputValue = DBConfig.Exchange_SellPrice_Max;

				SellPriceTotal.text = SellInputValue.text;
				break;
		}

		if (SellTradeCount.text != string.Empty && SellPriceTotal.text != string.Empty)
		{
			int cnt = Convert.ToInt32(SellTradeCount.text);
			int price = Convert.ToInt32(SellPriceTotal.text);
			SellPriceSea.text = price != 0 ? (price / cnt).ToString() : 0.ToString();
			SellTex.text = (((ulong)DBConfig.Exchange_Selling_Commission * (ulong)price) / 100).ToString();
		}
	}
}