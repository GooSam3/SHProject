using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public sealed class UISubHUDCurrencyItem : MonoBehaviour
{
	[SerializeField] private Image CurrenyImage;
	[SerializeField] private Text AmountText;

	public uint itemTid;

	public void Show(uint _itemTid)
	{
		gameObject.SetActive(true);

		// 아이템 id가 변경되었을때만 아이콘 수정
		if (itemTid != _itemTid) {
			if (_itemTid == DBConfig.Diamond_ID) {
				CurrenyImage.sprite = ZManagerUIPreset.Instance.GetSprite("icon_hud_dia");
			}
			else if (_itemTid == DBConfig.Gold_ID) {
				CurrenyImage.sprite = ZManagerUIPreset.Instance.GetSprite("icon_hud_coin_01");
			}
			else if (_itemTid == DBConfig.Essence_ID) {
				CurrenyImage.sprite = ZManagerUIPreset.Instance.GetSprite("Consume_Coin1_003");
			}
			else if (_itemTid == DBConfig.Crown_ID) {
				CurrenyImage.sprite = ZManagerUIPreset.Instance.GetSprite("icon_occupy_01");
			}
			else {
				var itemTable = DBItem.GetItem(_itemTid);
				if (itemTable != null) {
					CurrenyImage.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
				}
				else {
					ZLog.LogError(ZLogChannel.Default, $"해당재화에 해당하는 아이템을 찾을수 없다, itemTid:{_itemTid}");
				}
			}
		}

		itemTid = _itemTid;

		Refresh();
	}

	public void Refresh()
	{
		if (gameObject.activeSelf == false) {
			return;
		}

		if (itemTid == DBConfig.Crown_ID) {
			AmountText.text = $"{Me.GetCurrency(itemTid)}/{DBConfig.GodLand_Require_Stamina_Max}";
		}
		else {
			AmountText.text = $"{Me.GetCurrency(itemTid)}";
		}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

}