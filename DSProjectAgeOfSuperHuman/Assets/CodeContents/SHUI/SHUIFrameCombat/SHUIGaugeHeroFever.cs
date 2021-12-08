using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIGaugeHeroFever : CUIWidgetGaugeBase
{
	[SerializeField]
	private CText FeverCount = null;
	[SerializeField]
	private uTools.uTweenRotation AutoPilotRotation = null;
	[SerializeField]
	private CText AutoPilotSwitch = null;

	private bool m_bFeverOn = false;
	private SHUnitHero m_pUpdateHero = null;
	//-----------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		DoUIGaugeHeroAutoPilot(false);
	}

	protected override void OnUIWidgetGaugeUpdate()
	{
		base.OnUIWidgetGaugeUpdate();

		if (m_pUpdateHero != null)
		{
			UpdateUIGaugeHeroFever(m_pUpdateHero.GetUnitHeroRage());
		}
	}

	//------------------------------------------------
	public void DoUIGaugeHeroRage(SHUnitHero pUpdateHero)
	{
		m_pUpdateHero = pUpdateHero;
		float fFever = m_pUpdateHero.GetUnitHeroRage();
		ProtGaugeReset(SHStatComponentBase.c_FeverMax);
		UpdateUIGaugeHeroFever(fFever);
	}

	public void DoUIGaugeHeroAutoPilot(bool bOn)
	{
		if (bOn)
		{
			AutoPilotSwitch.text = "AUTO ON";
			AutoPilotRotation.enabled = true;
		}
		else
		{
			AutoPilotSwitch.text = "AUTO OFF";
			AutoPilotRotation.enabled = false;
		}
	}

	//-------------------------------------------------------
	private void UpdateUIGaugeHeroFever(float fValue)
	{
		ProtGaugeValueUpdate(fValue);
		int iPercent = (int)((fValue / SHStatComponentBase.c_FeverMax) * 100f);
		if (iPercent == 100)
		{
			PrivUIGaugeHeroFeverOn();
		}
		else
		{
			m_bFeverOn = false;
		}

		FeverCount.text = string.Format("RAGE {0}%", iPercent);
	}

	private void PrivUIGaugeHeroFeverOn()
	{
		if (m_bFeverOn) return;
		m_bFeverOn = true;

	}

}
