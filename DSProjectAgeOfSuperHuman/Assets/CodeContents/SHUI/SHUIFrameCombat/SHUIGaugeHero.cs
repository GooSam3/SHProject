using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIGaugeHero : CUIWidgetGaugeBase
{
	private SHUnitHero m_pUpdateHero = null;
	//-----------------------------------------------
	protected override void OnUIWidgetGaugeUpdate()
	{
		base.OnUIWidgetGaugeUpdate();

		if (m_pUpdateHero != null)
		{
			UpdateUIGaugeHero(m_pUpdateHero);
		}
	}

	//------------------------------------------------
	public void DoUIGaugeHero(SHUnitHero pUpdateHero)
	{
		m_pUpdateHero = pUpdateHero;
		Vector2 vecHP = m_pUpdateHero.GetUnitHP();
		ProtGaugeReset(vecHP.y);
		UpdateUIGaugeHero(pUpdateHero);
	}

	//-------------------------------------------------------
	private void UpdateUIGaugeHero(SHUnitHero pUpdateHero)
	{
		Vector2 vecHP = pUpdateHero.GetUnitHP();
		ProtGaugeValueUpdate(vecHP.x);
	}

}
