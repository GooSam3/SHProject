using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetTagHeroCutScene : CUIWidgetBase
{
	[System.Serializable]
	public class STagHeroInfo
	{
		public EHeroType HeroType = EHeroType.None;
		public SHUIWidgetTagHeroPanel HeroPanel = null;
	}
	[SerializeField]
	private SHEffectParticleNormal TagEffect = null;

	[SerializeField]
	private List<STagHeroInfo> TagHeroList = new List<STagHeroInfo>();
	//-------------------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		for (int i = 0; i < TagHeroList.Count; i++)
		{
			TagHeroList[i].HeroPanel.DoUIWidgetShowHide(false);
		}
	}

	//------------------------------------------------------------------------------------
	public void DoTagHeroCutSceneStart(SHUnitHero pHero)
	{
		uint hHeroID = pHero.GetUnitID();
		SHUIWidgetTagHeroPanel pTagPanel = FindTagHeroPanel(hHeroID);
		if (pTagPanel)
		{
			pTagPanel.DoTagHeroPanelStart(pHero.GetUnitLevel());
		}
		TagEffect.DoEffectStart(null);
	}

	//-------------------------------------------------------------------------------------
	private SHUIWidgetTagHeroPanel FindTagHeroPanel(uint hHeroID)
	{
		SHUIWidgetTagHeroPanel pTagHeroPanel = null;
		EHeroType eFindType = (EHeroType)hHeroID;
		for (int i = 0; i < TagHeroList.Count; i++)
		{
			if (TagHeroList[i].HeroType == eFindType)
			{
				pTagHeroPanel = TagHeroList[i].HeroPanel;
				break;
			}
		}

		return pTagHeroPanel;
	}

	
}
