using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUIWidgetFollowTag : CUIWidgetFollowBase
{
	[SerializeField]
	private DKUIWidgetGauge HPBar = null;

	private DKUnitBase m_pFollowUnit = null; public DKUnitBase GetFollowUnit() { return m_pFollowUnit; }
	//------------------------------------------------
	public void DoFollowTag(DKUnitBase pDKUnit)
	{
		m_pFollowUnit = pDKUnit;
		ProtFollowSetTrasform(pDKUnit.GetDKUnitSocketTransform(EUnitSocket.HPBar));
		HPBar.DoGaugeInitialize(pDKUnit);
	}

	public void DoFollowTagRemove()
	{
		DoWidgetItemReturn();
	}

}
