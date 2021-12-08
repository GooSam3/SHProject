using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CInAppPurchaseBase : object
{
	//-------------------------------------------------------------
	public void DoPurchaseInitialize()
	{
		OnPurchaseInitialize();
	}

	public void DoItemBuy(string strProductID)
	{
		OnPurchaseBuy(strProductID);
	}

	public void DoItemRestore()
	{
		OnPurchaseRestore();
	}

	public string GetIAPReceipt(string strProductID)
	{
		return OnPurchaseReceipt(strProductID);
	}

	public bool IsIAPConsumeItem(string strProductID)
	{
		return OnPurchaseConsumeItem(strProductID);
	}

	public bool IsIAPSubscription(string strProductID)
	{
		return OnPurchaseSubscription(strProductID);
	}

	//-------------------------------------------------------------
	protected virtual void		OnPurchaseInitialize() { }
	protected virtual string	OnPurchaseReceipt(string strProductID) { return string.Empty; }
	protected virtual bool		OnPurchaseConsumeItem(string strProductID) { return false;}
	protected virtual bool		OnPurchaseSubscription(string strProductID) { return false;}
	protected virtual void		OnPurchaseBuy(string strProductID) { }
	protected virtual void		OnPurchaseRestore() { }
} 
