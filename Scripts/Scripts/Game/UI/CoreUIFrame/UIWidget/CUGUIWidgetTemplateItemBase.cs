using System.Collections.Generic;
using UnityEngine;

abstract public class CUGUIWidgetTemplateItemBase : CUGUIWidgetBase
{
	[SerializeField]
	private CUGUIWidgetSlotItemBase ItemTemplate = null;

	[SerializeField]
	private uint ItemReserve = 1; 

	private List<CUGUIWidgetSlotItemBase> m_listTemplateInstance = new List<CUGUIWidgetSlotItemBase>();

	//-----------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);	
		PrivUIScrollReserveTemplate();
	}

	protected override void OnUIWidgetDuplication()
	{
		Transform TemplateRoot = ItemTemplate.transform.parent;
		int TotalCount = TemplateRoot.childCount;

		for (int i = TotalCount - 1; i > 0; i--)
		{
			Transform Child = TemplateRoot.GetChild(i);
			if (ItemTemplate.transform != Child)
			{
				DestroyImmediate(Child.gameObject);
			}
		}
	}

	//-----------------------------------------------------------------------------
	protected virtual CUGUIWidgetSlotItemBase ProtUIScrollSlotItemRequest(Transform _parent = null)
	{
		CUGUIWidgetSlotItemBase SlotItem = null;
		for (int i = 0; i < m_listTemplateInstance.Count; i++)
		{
			if (m_listTemplateInstance[i].gameObject.activeSelf == false)
			{
				SlotItem = m_listTemplateInstance[i];
				break;
			}
		}

		if (SlotItem == null)
		{
			SlotItem = PrivUIScrollAllocateItem();
		}

		if (_parent)
		{
			SlotItem.transform.SetParent(_parent, false);
		}
		SlotItem.DoSlotItemShow();
		SlotItem.gameObject.SetActive(true);

		return SlotItem;
	}

	protected void ProtUIScrollSlotItemReturn(CUGUIWidgetSlotItemBase _SlotItem)
	{
		_SlotItem.DoSlotItemHide();
		_SlotItem.transform.SetParent(ItemTemplate.transform.parent, false);
		_SlotItem.gameObject.SetActive(false);		
	}

	protected void ProtUIScrollSlotItemReturnAll()
	{
		for (int i = 0; i < m_listTemplateInstance.Count; i++)
		{
			ProtUIScrollSlotItemReturn(m_listTemplateInstance[i]);
		}
	}

	protected List<TEMPLATE> ExtractUIScrollSlotItemList<TEMPLATE>() where TEMPLATE : CUGUIWidgetSlotItemBase
	{
		List<TEMPLATE> listReturn = new List<TEMPLATE>();
		for (int i = 0;  i < m_listTemplateInstance.Count; i++)
		{
			if (m_listTemplateInstance[i].gameObject.activeSelf)
			{
				listReturn.Add(m_listTemplateInstance[i] as TEMPLATE);
			}
		}

		return listReturn;
	}

	//-----------------------------------------------------------------------------
	private void PrivUIScrollReserveTemplate()
	{
		if (ItemTemplate == null) return;
		ItemTemplate.gameObject.SetActive(false);

		for (uint i = 0; i < ItemReserve; i++)
		{
			PrivUIScrollAllocateItem();
		}
	}

	private CUGUIWidgetSlotItemBase PrivUIScrollAllocateItem()
	{
		GameObject NewInstance = Instantiate(ItemTemplate.gameObject);
		NewInstance.transform.SetParent(ItemTemplate.transform.parent, false);
		NewInstance.SetActive(false);

		List<CUGUIWidgetBase> listWidget = new List<CUGUIWidgetBase>();
		NewInstance.gameObject.GetComponentsInChildren<CUGUIWidgetBase>(true, listWidget);

		for (int i = 0; i < listWidget.Count; i++)
		{
			listWidget[i].DoUIWidgetInitialize(mUIFrameParent);
		}

		for (int i = 0; i < listWidget.Count; i++)
		{
			listWidget[i].DoUIWidgetInitializePost(mUIFrameParent);
		}

		CUGUIWidgetSlotItemBase NewComponent = NewInstance.GetComponent<CUGUIWidgetSlotItemBase>();	
		m_listTemplateInstance.Add(NewComponent);
		return NewComponent;
	}
}
