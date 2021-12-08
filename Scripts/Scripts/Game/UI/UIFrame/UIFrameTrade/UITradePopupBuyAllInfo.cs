using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UITradePopupBuyAllInfo : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Text Name;
	[SerializeField] private Text BuyCount;
	[SerializeField] private Text BuySucCount;
	[SerializeField] private Text BuyFailCount;
	[SerializeField] private Text BuyTotalPrice;
	[SerializeField] private ScrollRect BuyListScroll;
	#endregion

	#region System Variable
	[SerializeField] private List<GameObject> Data = new List<GameObject>();
	[SerializeField] private int BuyCnt = 0;
	#endregion

	public void Initialize(List<ExchangeItemData> _data)
	{
		gameObject.SetActive(true);

		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		ClearData();

		if (_data.Count == 0)
			return;

		var item = DBItem.GetItem(_data[0].ItemTId);

		Icon.sprite = UICommon.GetItemIconSprite(item.ItemID);
		Name.text = DBLocale.GetText(item.ItemTextID);
		BuyCount.text = "0 / " + _data.Count.ToString();
		BuySucCount.text = 0.ToString();
		BuyFailCount.text = 0.ToString();
		BuyTotalPrice.text = 0.ToString();

		for (int i = 0; i < _data.Count; i++)
		{
			UITradeBuyAllInfoListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UITradeBuyAllInfoListItem)).GetComponent<UITradeBuyAllInfoListItem>();

			if (obj != null)
			{
				obj.transform.SetParent(BuyListScroll.content, false);
				obj.Initialize(_data[i]);
				Data.Add(obj.gameObject);
			}
		}

		BuyAllItem();
	}

	private void BuyAllItem()
	{
		BuyCnt += 1;

		BuyCount.text = BuyCnt.ToString() + " / " + Data.Count.ToString();

		UITradeBuyAllInfoListItem item = Data[BuyCnt - 1].GetComponent<UITradeBuyAllInfoListItem>();

		ZWebManager.Instance.WebGame.REQ_BuyExchangeItem(item.Data, (recvPacket, recvPacketMsg) => {

			if (recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
			{
				bool buy = recvPacketMsg.ResultEquipItem != null || recvPacketMsg.ResultStackItem != null;
				item.SetBuyUI(buy);

				if (buy)
				{
					BuySucCount.text = (Convert.ToInt32(BuySucCount.text) + 1).ToString();
					BuyTotalPrice.text = (Convert.ToInt32(BuyTotalPrice.text) + item.Data.ItemTotalPrice).ToString();
				}
				else
					BuyFailCount.text = (Convert.ToInt32(BuyFailCount.text) + 1).ToString();

				if (BuyCnt < Data.Count)
					BuyAllItem();
				else
				{
					if (UIManager.Instance.Find(out UIFrameTrade _trade))
					{
						_trade.ScrollAdapter.SetData(E_TradeSearchMainType.Detail, _trade.ScrollAdapter.CurSearchItemId);
					}
				}
			}
			else
				ClearData();
		});
	}

	private void ClearData()
	{
		for (int i = 0; i < Data.Count; i++)
			Destroy(Data[i]);

		Data.Clear();
		BuyCnt = 0;
	}

	public void ClickConfirm()
	{
		ClearData();

		gameObject.SetActive(false);
	}
}