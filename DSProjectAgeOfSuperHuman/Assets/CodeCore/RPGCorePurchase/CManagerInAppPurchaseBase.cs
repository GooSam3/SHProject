using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CManagerInAppPurchaseBase : CManagerTemplateBase<CManagerInAppPurchaseBase>
{

	private CInAppPurchaseBase m_pPurchase = null;
	//-----------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		if (CApplication.Instance.GetMarketType() == CApplication.EOAuth2Type.Andorid)
		{
			m_pPurchase = new CInAppPurchaseGooglePlay();
		}
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		m_pPurchase.DoPurchaseInitialize();
	}
	//-------------------------------------------------------------------
	public void DoIAPItemBuy(string strProductID)
	{
		m_pPurchase.DoItemBuy(strProductID);
	}

	public void DoIAPRestore()
	{
		m_pPurchase.DoItemRestore();
	}

	//-------------------------------------------------------------------
	public string GetIAPReceipt(string strProductID)
	{
		return m_pPurchase.GetIAPReceipt(strProductID);
	}

	public bool IsIAPConsume(string strProductID)
	{
		return m_pPurchase.IsIAPConsumeItem(strProductID);
	}

	public bool IsIAPSubscription(string strProductID)
	{
		return m_pPurchase.IsIAPSubscription(strProductID);
	}


}
