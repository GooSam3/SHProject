using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHWindowHeroPurchase : CUIWidgetDialogBase
{
	[System.Serializable]
	public class SPurchaseHero
	{
		public EHeroType				 HeroType = EHeroType.None;
		public SHWindowHeroPurchaseView PurchaseView = null;
	}

	[SerializeField]
	private List<SPurchaseHero> PurchaseView = new List<SPurchaseHero>();
	//-----------------------------------------------------------





	//-----------------------------------------------------------
	private void PrivHeroPurchaseViewShow(EHeroType eHeroType)
	{
		for (int i = 0; i < PurchaseView.Count;i++)
		{
			if (PurchaseView[i].HeroType == eHeroType)
			{
				PurchaseView[i].PurchaseView.DoUIWidgetShowHide(true);
			}
			else
			{
				PurchaseView[i].PurchaseView.DoUIWidgetShowHide(false);
			}
		}
	}
	//-------------------------------------------------------
	public void HandleHeroPurchase(int iHeroType)
	{
		EHeroType eHeroType = (EHeroType)iHeroType;
		PrivHeroPurchaseViewShow(eHeroType);
	}

}
