using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHUIWidgetHeroTag : CUIWidgetBase
{
	[SerializeField]
	private CText			HeroLevel = null;
	[SerializeField]
	private SHUIGaugeHero	GaugeHeroHP = null;
	[SerializeField]
	private SHUIGaugeHeroTag GaugeHeroTag = null;
	[SerializeField]
	private SHUIBuffViewer BuffViewer = null;

	private SHUnitHero m_pTargetHero = null;				public SHUnitHero GetHeroTagUnit() { return m_pTargetHero; }	
	private SHUIFrameCombatHero m_pUIFrameCombatHero = null;
	//---------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pUIFrameCombatHero = pParentFrame as SHUIFrameCombatHero;
	}

	//-----------------------------------------------------
	public void DoHeroTagSetting(SHUnitHero pHero)
	{
		m_pTargetHero = pHero;
		GaugeHeroHP.DoUIGaugeHero(pHero);
		GaugeHeroTag.DoUIGaugeHero(pHero);
		BuffViewer.DoUIBuffViewerUnit(pHero);
		uint hHeroID = pHero.GetUnitID();
		PrivHeroTagLevel(hHeroID);
	}

	public void DoHeroDamageHeal(float fValue, bool bDamage, bool bCritical)
	{
		Vector3 vecScreenPos = GaugeHeroTag.GetUIGaugeHeroFaceCenterPosition();
		vecScreenPos = CUIFrameBase.WorldToCanvas(vecScreenPos);
		if (bDamage)
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagDamage(fValue, bCritical, vecScreenPos);
		}
		else
		{
			UIManager.Instance.DoUIMgrFind<SHUIFrameNumberTag>().DoNumberTagHeal(fValue, bCritical, vecScreenPos);
		}
	}
	//-------------------------------------------------------
	private void PrivHeroTagLevel(uint hHeroID)
	{
		if (HeroLevel == null) return;
		int iLevel = SHManagerGameDB.Instance.GetGameDBHeroLevel(hHeroID);
		HeroLevel.text = string.Format("Lv {0}", iLevel.ToString());
	}

	//-----------------------------------------------------
	public void HandleHeroTagStart()
	{
		float fCoolTime = m_pTargetHero.ISHGetTagCoolTime();
		if (fCoolTime == 0)
		{
			SHManagerUnit.Instance.DoMgrUnitHeroTagOn(m_pTargetHero, true);
		}
	}
	

}
