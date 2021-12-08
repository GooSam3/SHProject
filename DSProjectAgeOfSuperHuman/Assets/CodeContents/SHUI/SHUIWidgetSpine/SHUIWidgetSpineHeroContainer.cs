using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetSpineHeroContainer : CUIWidgetBase
{
	[System.Serializable]
	public class SHeroSpineInfo
	{
		public EHeroType HeroType;
		public SHUIWidgetSpineHero HeroSpine;
	}
	[SerializeField]
	private List<SHeroSpineInfo> HeroInfo = null;

	private SHUIWidgetSpineHero m_pSpineHero = null;
	//-----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);

		for (int i = 0; i < HeroInfo.Count; i++)
		{
			HeroInfo[i].HeroSpine.DoUIWidgetShowHide(false);
		}
	}

	//-----------------------------------------------------------------
	public void DoSpineHeroContainer(uint hHeroID)
	{
		EHeroType eHeroType = (EHeroType)hHeroID;
		SHUIWidgetSpineHero pSpineHero = FindSpineContainer(eHeroType);

		if (pSpineHero != null && m_pSpineHero != pSpineHero)
		{
			if (m_pSpineHero != null)
			{
				m_pSpineHero.DoUIWidgetShowHide(false);
			}
			m_pSpineHero = pSpineHero;
			m_pSpineHero.DoUIWidgetShowHide(true);
		}
	}

	//--------------------------------------------------------------
	private SHUIWidgetSpineHero FindSpineContainer(EHeroType eHeroType)
	{
		SHUIWidgetSpineHero pSpineHero = null;
		for (int i = 0; i < HeroInfo.Count; i++)
		{
			if (HeroInfo[i].HeroType == eHeroType)
			{
				pSpineHero = HeroInfo[i].HeroSpine;
				break;
			}
		}
		return pSpineHero;
	}
}
