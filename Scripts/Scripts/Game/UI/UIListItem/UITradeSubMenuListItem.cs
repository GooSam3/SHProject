using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UITradeSubMenuListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Text Name;
	#endregion

	#region System Variable
	[SerializeField] private E_TradeSubTapType Type = E_TradeSubTapType.None;
	#endregion

	public void Initialize(E_TradeSubTapType _type)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		Type = _type;
		Name.text = DBLocale.GetText(_type.ToString());
	}

	public void OnSubMenu()
	{
		if(UIManager.Instance.Find(out UIFrameTrade _trade))
		{
			_trade.CurTradeSearchSubTab = Type;

			_trade.ScrollAdapter.SetData(E_TradeSearchMainType.Main);

			_trade.SearchSubMenu.SetActive(false);

			_trade.SetSubMenuToggleChangeValue();
		}	
	}
}