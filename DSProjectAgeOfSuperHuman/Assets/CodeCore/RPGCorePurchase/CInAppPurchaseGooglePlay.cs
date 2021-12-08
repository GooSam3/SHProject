using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
public class CInAppPurchaseGooglePlay : CInAppPurchaseBase
{
    private IStoreController m_pStoreController = null;
    //------------------------------------------------------
    protected override void OnPurchaseInitialize() 
    {
        m_pStoreController = CodelessIAPStoreListener.Instance.StoreController;
	}

	protected override string OnPurchaseReceipt(string strProductID)
	{
        string strReceipt = string.Empty;
        Product pProduct = CodelessIAPStoreListener.Instance.StoreController.products.WithID(strProductID);
        if (pProduct != null)
		{
            strReceipt = pProduct.receipt;
		}
        
        return strReceipt;
	}

	protected override bool OnPurchaseConsumeItem(string strProductID) 
    {
        bool bResult = false;
		Product pProduct = CodelessIAPStoreListener.Instance.StoreController.products.WithID(strProductID);
		if (pProduct != null)
		{
            if (pProduct.definition.type == ProductType.Consumable)
			{
                bResult = true;
			}
		}
		return bResult; 
    }

	protected override bool OnPurchaseSubscription(string strProductID) 
    {
        bool bResult = false;
		Product pProduct = m_pStoreController.products.WithID(strProductID);
		if (pProduct != null)
		{
			if (pProduct.definition.type == ProductType.Subscription)
			{
				bResult = true;
			}
		}
		return bResult;
    }

	protected override void OnPurchaseBuy(string strProductID)
	{
		CodelessIAPStoreListener.Instance.InitiatePurchase(strProductID);
	}

	protected override void OnPurchaseRestore()
	{
		if (Application.platform == RuntimePlatform.WSAPlayerX86 ||
				Application.platform == RuntimePlatform.WSAPlayerX64 ||
				Application.platform == RuntimePlatform.WSAPlayerARM)
		{
			CodelessIAPStoreListener.Instance.ExtensionProvider.GetExtension<IMicrosoftExtensions>()
				.RestoreTransactions();
			Debug.Log(string.Format("[IAP] RestoreItem IMicrosoftExtensions "));
		}
		else if (Application.platform == RuntimePlatform.IPhonePlayer ||
				 Application.platform == RuntimePlatform.OSXPlayer ||
				 Application.platform == RuntimePlatform.tvOS)
		{
			CodelessIAPStoreListener.Instance.ExtensionProvider.GetExtension<IAppleExtensions>()
				.RestoreTransactions((bool bSucess)=> { });
			Debug.Log(string.Format("[IAP] RestoreItem IAppleExtensions "));
		}
		else if (Application.platform == RuntimePlatform.Android &&
				 StandardPurchasingModule.Instance().appStore == AppStore.SamsungApps)
		{
			CodelessIAPStoreListener.Instance.ExtensionProvider.GetExtension<ISamsungAppsExtensions>()
				.RestoreTransactions((bool bSucess) => { });
			Debug.Log(string.Format("[IAP] RestoreItem ISamsungAppsExtensions "));
		}
		else if (Application.platform == RuntimePlatform.Android &&
			StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay)
		{
			CodelessIAPStoreListener.Instance.ExtensionProvider.GetExtension<IGooglePlayStoreExtensions>()
				.RestoreTransactions((bool bSucess) => { });

			Debug.Log(string.Format("[IAP] RestoreItem IGooglePlayStoreExtensions "));
		}
		else
		{
			Debug.LogWarning(Application.platform.ToString() +
							 " is not a supported platform for the Codeless IAP restore button");
		}
	}
}
