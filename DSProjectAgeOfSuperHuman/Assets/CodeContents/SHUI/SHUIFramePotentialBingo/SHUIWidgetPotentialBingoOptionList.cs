using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetPotentialBingoOptionList : CUIWidgetBase
{
	[SerializeField]
	private GameObject ItemRoot = null;

	private List<SHUISlotPotentialOptionItem> m_listOptionInstance = new List<SHUISlotPotentialOptionItem>();
	//-------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);

		int iCount = ItemRoot.transform.childCount;
		for(int i = 0; i < iCount; i++)
		{
			Transform pChild = ItemRoot.transform.GetChild(i);
			SHUISlotPotentialOptionItem pOptionItem = pChild.gameObject.GetComponent<SHUISlotPotentialOptionItem>();
			if (pOptionItem != null)
			{
				m_listOptionInstance.Add(pOptionItem);
				pChild.gameObject.SetActive(false);
			}
		}
	}

	//--------------------------------------------------------
}
