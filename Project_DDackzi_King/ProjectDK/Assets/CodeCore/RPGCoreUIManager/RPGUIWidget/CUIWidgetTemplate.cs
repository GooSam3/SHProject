using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIWidgetTemplate : CUIWidgetBase
{
	[SerializeField]
	private CUIWidgetTemplateItemBase TemplateItem = null;

	private List<CUIWidgetTemplateItemBase> m_listCloneInstance = new List<CUIWidgetTemplateItemBase>();
	//-----------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		if (TemplateItem != null)
		{
			TemplateItem.SetMonoActive(false);
		}
	}

	//------------------------------------------------------
	public CUIWidgetTemplateItemBase DoUIWidgetTemplateRequestItem(Transform pParent = null)
	{
		CUIWidgetTemplateItemBase pItem = null;

		for (int i = 0; i < m_listCloneInstance.Count; i++)
		{
			if (m_listCloneInstance[i].gameObject.activeSelf == false)
			{
				pItem = m_listCloneInstance[i];
				break;
			}
		}

		if (pItem == null)
		{
			pItem = MakeUIWidgetTemplateItem();
		}

		if (pParent)
		{
			pItem.transform.SetParent(pParent, false);
		}

		pItem.DoWidgetItemShow(true);
		return pItem;
	}

	public void DoUIWidgetTemplateReturnAll()
	{
		for (int i = 0; i < m_listCloneInstance.Count; i++)
		{
			PrivUIWidgetTemplateReturn(m_listCloneInstance[i]);
		}
	}

	public void DoUIWidgetTemplateReturn(CUIWidgetTemplateItemBase pItem)
	{
		PrivUIWidgetTemplateReturn(pItem);
	}

	//--------------------------------------------------------
	private CUIWidgetTemplateItemBase MakeUIWidgetTemplateItem()
	{
		GameObject NewInstance = Instantiate(TemplateItem.gameObject);
		NewInstance.transform.SetParent(TemplateItem.transform.parent, false);		
		CUIWidgetTemplateItemBase pNewItem = NewInstance.GetComponent<CUIWidgetTemplateItemBase>();
		m_listCloneInstance.Add(pNewItem);

		pNewItem.ImportSetTemplateParent(this);

		List<CUIWidgetBase> listWidget = new List<CUIWidgetBase>();
		NewInstance.GetComponentsInChildren(true, listWidget);
		for (int i = 0; i < listWidget.Count; i++)
		{
			listWidget[i].ImportUIWidgetInitialize(GetUIWidgetParent());
		}

		for (int i = 0; i < listWidget.Count; i++)
		{
			listWidget[i].ImportUIWidgetInitializePost(GetUIWidgetParent());
		}
		NewInstance.SetActive(false);
		return pNewItem;
	}

	private void PrivUIWidgetTemplateReturn(CUIWidgetTemplateItemBase pItem)
	{
		pItem.DoWidgetItemShow(false);
		pItem.transform.SetParent(TemplateItem.transform.parent, false);
	}
}
