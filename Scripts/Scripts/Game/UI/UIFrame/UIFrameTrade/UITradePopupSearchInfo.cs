using System.Collections.Generic;
using UnityEngine;

public class UITradePopupSearchInfo : MonoBehaviour
{
	#region Variable
	private List<uint> SelectEnhanceList = new List<uint>();
	public ZToggle SelectEnhanceAll = null;
	public List<ZToggle> SelectEnhance = new List<ZToggle>();

	public ZToggleGroup EnchantOption = null;
	#endregion

	public void Initialize(bool _open)
	{
		gameObject.SetActive(_open);

		if(!_open)
			OnRefresh();
	}

	public void OnSearch()
	{
		if(UIManager.Instance.Find(out UIFrameTrade _trade))
		{
			_trade.ScrollAdapter.SetData(E_TradeSearchMainType.Detail, _trade.ScrollAdapter.CurSearchItemId);
			
			OnClose();
		}
	}

	public List<uint> GetSelectEnhanceList()
	{
		SelectEnhanceList.Clear();

		for (int i = 0; i < SelectEnhance.Count; i++)
			if (SelectEnhance[i].isOn)
				SelectEnhanceList.Add((uint)i);

		return SelectEnhanceList;
	}

	public void OnClickAll()
	{
		SetAllSelectEnhanceList(SelectEnhanceAll.isOn);
	}

	public void OnClickEnhance()
	{
		SelectEnhanceAll.SelectToggleSingle(GetSelectEnhanceList().Count == ZUIConstant.ENHANCS_MAX, false);
	}

	public void OnRefresh()
	{
		SelectEnhanceList.Clear();
		SelectEnhanceAll.SelectToggleSingle(true);

		SetAllSelectEnhanceList(true);

		EnchantOption.GetToggle(0)?.SelectToggle();
	}

	public void OnClose()
	{
		gameObject.SetActive(false);
	}

	private void SetAllSelectEnhanceList(bool _isOn)
	{
		for (int i = 0; i < SelectEnhance.Count; i++)
			SelectEnhance[i].SelectToggleSingle(_isOn);
	}
}