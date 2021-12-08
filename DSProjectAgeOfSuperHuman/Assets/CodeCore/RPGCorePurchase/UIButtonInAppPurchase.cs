using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Events;
/*
 * [����!] �ѹ� IAP Catalog���� Non Consumable �����ؼ� �����ϸ� �ش� �������� �ٽ� ������ ���� �ʴ´�. �ſ� �����Ұ�
 * 
 */
[RequireComponent(typeof(Button))]
public class UIButtonInAppPurchase : IAPListener
{
    [SerializeField]
    private string ProductID;
	[SerializeField]
	private CImage ProductDisable;
	[SerializeField]
	private UnityEvent<string, string> EventComplete = null;
	[SerializeField]
	private UnityEvent<string> EventFailure = null;

    private Button m_pPurchaseButton = null;
	//---------------------------------------------------------------
	private void Awake()
	{
		dontDestroyOnLoad = false;
		m_pPurchaseButton = GetComponent<Button>();
		m_pPurchaseButton.onClick.AddListener(HandleInAppPurchaseButtonOn);
		ProductDisable.gameObject.SetActive(false);
		onPurchaseComplete.AddListener(HandleInAppPurchaseComplete);
		onPurchaseFailed.AddListener(HandleInAppPurchaseFailed);		
	}

	private void Start()
	{
		PrivButtonIAPProductStatus();
	}
	//-----------------------------------------------------------------
	private void PrivButtonIAPProductStatus()
	{		
		string strReceipt = CManagerInAppPurchaseBase.Instance.GetIAPReceipt(ProductID);
		if (strReceipt == string.Empty || strReceipt == null)
		{
			PrivButtonIAPButtonEnable();
		}
		else
		{
			PrivButtonIAPHasReceipt(strReceipt);
		}
	}

	private void PrivButtonIAPHasReceipt(string strReceipt)
	{
		if (CManagerInAppPurchaseBase.Instance.IsIAPConsume(ProductID)) // ������ �ִ� ���¿��� ����Ÿ�� ��� 
		{
			PrivButtonIAPButtonEnable();
		}
		else
		{
			PrivButtonIAPButtonDisable();
		}
		Debug.Log(string.Format("[IAP] Receipt {0} / {1}", ProductID, strReceipt));
	}

	private void PrivButtonIAPButtonEnable()
	{
		ProductDisable.gameObject.SetActive(false);
		m_pPurchaseButton.interactable = true;
	}

	private void PrivButtonIAPButtonDisable()
	{
		ProductDisable.gameObject.SetActive(true);
		m_pPurchaseButton.interactable = false;
	}

	//---------------------------------------------------------------
	private void HandleInAppPurchaseButtonOn()
	{
		CManagerInAppPurchaseBase.Instance.DoIAPItemBuy(ProductID);
	}

	private void HandleInAppPurchaseComplete(Product pProdect)
	{
		if (pProdect.definition.id != ProductID) return;
		PrivButtonIAPProductStatus(); 
		EventComplete?.Invoke(pProdect.definition.id, pProdect.receipt);
		Debug.Log(string.Format("[IAP] Success {0} / {1}", pProdect.definition.id, pProdect.receipt));
	}

	private void HandleInAppPurchaseFailed(Product pProdect, PurchaseFailureReason eFailReason)
	{
		if (pProdect.definition.id != ProductID) return;
		EventFailure?.Invoke(eFailReason.ToString());
		Debug.Log(string.Format("[IAP] Failed {0} / {1} / {2}", pProdect.definition.id, pProdect.receipt, eFailReason.ToString()));		
	}
}
