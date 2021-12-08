using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameResource : SHUIFrameBase
{
	[SerializeField]
	private CUIWidgetNumberTextChart ResourceDiamond = null;
	[SerializeField]
	private CUIWidgetNumberTextChart ResourceGold = null;

	//---------------------------------------------------------------------
	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		PrivUIFrameRefresh();
	}

	//---------------------------------------------------------------------
	public void DoUIFrameResourceRefresh()
	{
		PrivUIFrameRefresh();
	}

	//----------------------------------------------------------------------
	private void PrivUIFrameRefresh()
	{
		ResourceGold.DoTextNumber(SHManagerGameDB.Instance.GetGameDBCurrency(ECurrencyType.Gold));
		ResourceDiamond.DoTextNumber(SHManagerGameDB.Instance.GetGameDBCurrency(ECurrencyType.Diamond));
	}


}
