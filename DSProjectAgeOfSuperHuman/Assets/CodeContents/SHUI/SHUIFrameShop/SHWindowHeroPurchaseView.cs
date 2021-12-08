using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHWindowHeroPurchaseView : CUIWidgetBase
{
	[SerializeField]
	private CImage PurchaseComplet;
	[SerializeField]
	private EHeroType HeroType = EHeroType.None;

	//---------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		PurchaseComplet.gameObject.SetActive(false);
	}

	//--------------------------------------------------------------------
	public void HandlePurchaseViewComplete(string strProductID, string strReceipt)
	{
		// ������ ���� 
	}

	public void HandlePurchaseViewFailure(string strException)
	{
		UIManager.Instance.DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.Ok, string.Format("���Ÿ� ���� �߽��ϴ�."), null, null);
	}

}
