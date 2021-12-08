using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CUIWidgetTemplate : CUIWidgetBase
{
	[SerializeField]
	private CUIWidgetTemplateItemBase TemplateItem = null;

	private CUIWidgetBase m_pTemplateOwner = null;
	private UnityAction<CUIWidgetTemplateItemBase> m_delReturn = null;  public void SetWidgetTemplateReturnEvent(UnityAction<CUIWidgetTemplateItemBase> pEvent) { m_delReturn = pEvent; }
	private List<CUIWidgetTemplateItemBase> m_listCloneInstance = new List<CUIWidgetTemplateItemBase>(); protected List<CUIWidgetTemplateItemBase> GetWidgetTemplateList() { return m_listCloneInstance; }
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
	public CUIWidgetTemplateItemBase DoTemplateRequestItem(Transform pParent = null)
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

		pItem.DoWTemplatItemShow(true);

		if (m_pTemplateOwner)
		{
			pItem.ImportUIWidgetOwner(m_pTemplateOwner);
		}

		OnUITemplateItem(pItem);

		return pItem;
	}

	public void DoTemplateReturnAll()
	{
		for (int i = 0; i < m_listCloneInstance.Count; i++)
		{
			PrivUIWidgetTemplateReturn(m_listCloneInstance[i]);
		}
	}

	public void DoTemplateReturn(CUIWidgetTemplateItemBase pItem)
	{
		PrivUIWidgetTemplateReturn(pItem);
		m_delReturn?.Invoke(pItem);
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
			listWidget[i].ImportUIWidgetOwner(this);
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
		pItem.DoWTemplatItemShow(false);
		pItem.transform.SetParent(TemplateItem.transform.parent, false);
	}

	//---------------------------------------------------------------
	protected virtual void OnUITemplateItem(CUIWidgetTemplateItemBase pItem) { }
}
