using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UITradeBuyAllInfoListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Text BuyTotalPrice;
	[SerializeField] private GameObject ResultObj;
	[SerializeField] private Image ResultGardiant;
	[SerializeField] private Text ResultText;
	#endregion

	#region System Variable
	public ExchangeItemData Data;
	#endregion

	public void Initialize(ExchangeItemData _data)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		if (_data == null)
			return;

		Data = _data;

		Icon.sprite = UICommon.GetItemIconSprite(Data.ItemTId);
		BuyTotalPrice.text = Data.ItemTotalPrice.ToString();
		ResultObj.SetActive(false);
		ResultText.text = string.Empty;
	}

	public void SetBuyUI(bool _buyState)
	{
		ResultObj.SetActive(true);

		switch (_buyState)
		{
			case true:
				ResultText.text = "성공";
				ResultGardiant.color = new Color(38, 103, 157, 123);
				break;

			case false:
				ResultText.text = "실패";
				ResultGardiant.color = new Color(157, 38, 53, 128);
				break;
		}
	}
}