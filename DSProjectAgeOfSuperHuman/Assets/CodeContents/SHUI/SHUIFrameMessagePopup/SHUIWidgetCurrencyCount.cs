using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetCurrencyCount : CUIWidgetBase
{
	[System.Serializable]
	public class SCurrencyInfo
	{
		public ECurrencyType CurrencyType = ECurrencyType.None;
		public CImage IconImage;
	}
	[SerializeField]
	private CUIWidgetNumberTextChart NumberCount = null;

	[SerializeField]
	private List<SCurrencyInfo> CurrencyList = null;

	//-------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		for (int i = 0; i < CurrencyList.Count; i++)
		{
			CurrencyList[i].IconImage.gameObject.SetActive(false);
		}
	}


	//------------------------------------------------------------------
	public void DoCurrencyCount(ECurrencyType eCurrency, long iCount)
	{
		NumberCount.DoTextNumber(iCount);

		for (int i = 0; i < CurrencyList.Count; i++)
		{
			if (CurrencyList[i].CurrencyType == eCurrency)
			{
				CurrencyList[i].IconImage.gameObject.SetActive(true);
			}
			else
			{
				CurrencyList[i].IconImage.gameObject.SetActive(false);
			}
		}
	}



}
