using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetGoldViewer : CUIWidgetNumberTextChart
{

	//--------------------------------------------------------
	private void Update()
	{
		uint iGold = SHManagerGameDB.Instance.GetGameDBCurrency(ECurrencyType.Gold);
		DoTextNumber(iGold);
	}
}
