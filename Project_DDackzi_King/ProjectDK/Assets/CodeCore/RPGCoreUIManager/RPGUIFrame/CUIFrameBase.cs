using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class CUIFrameBase : CMonoBase
{
	[SerializeField]
	private CManagerUIFrameFocusBase.EUIFrameFocusType FocusType = CManagerUIFrameFocusBase.EUIFrameFocusType.Panel;

	private List<CUIWidgetBase> m_listWidgetInstance = new List<CUIWidgetBase>();

	private GraphicRaycaster		m_pRayCaster;
	protected Canvas				m_pCanvas;
	protected RectTransform		m_pRectTransform;
	protected Vector2				m_vecUIFrameSize = Vector2.zero;
	//--------------------------------------------------------------------------
	internal void ImportUIFrameInitialize()
	{
		m_pRayCaster = GetComponent<GraphicRaycaster>();
		m_pCanvas = GetComponent<Canvas>();
		m_pRectTransform = GetComponent<RectTransform>();

		m_vecUIFrameSize.x = m_pRectTransform.rect.width;
		m_vecUIFrameSize.y = m_pRectTransform.rect.height;

		SetMonoActive(true);
		m_pCanvas.overrideSorting = true;
		m_pCanvas.sortingLayerName = LayerMask.LayerToName(gameObject.layer);
		SetMonoActive(false);

		OnUIFrameInitialize();

		PrivUIFrameInitializeUIWidget();
	}

	internal void ImportUIFrameInitializePost()
	{
		PrivUIFrameInitializeUIWidgetPost();
		OnUIFrameInitializePost();
	}

	internal void ImportUIFrameShow(int iOrder)
	{
		SetMonoActive(true);
		PrivUIFrameRefreshOrder(iOrder);
		OnUIFrameShow(iOrder);
	}

	internal void ImportUIFrameRefreshOrder(int iOrder)
	{
		PrivUIFrameRefreshOrder(iOrder);
		OnUIFrameRefreshOrder(iOrder);
	}

	internal void ImportUIFrameHide()
	{
		OnUIFrameHide();
	}

	internal void ImportUIFrameForceHide()
	{
		OnUIFrameForceHide();
	}

	internal void ImportUIFrameRemove()
	{
		OnUIFrameRemove();
	}

	//-------------------------------------------------------------------------
	//-------------------------------------------------------------------------
	private void PrivUIFrameInitializeUIWidget()
	{
		GetComponentsInChildren<CUIWidgetBase>(true, m_listWidgetInstance);

		for (int i = 0; i < m_listWidgetInstance.Count; i++)
		{
			m_listWidgetInstance[i].ImportUIWidgetInitialize(this);
		}
	}

	private void PrivUIFrameInitializeUIWidgetPost()
	{
		for (int i = 0; i < m_listWidgetInstance.Count; i++)
		{
			m_listWidgetInstance[i].ImportUIWidgetInitializePost(this);
		}
	}

	private void PrivUIFrameRefreshOrder(int iOrder)
	{
		m_pCanvas.sortingOrder = iOrder;
		for (int i = 0; i < m_listWidgetInstance.Count; i++)
		{
			m_listWidgetInstance[i].ImportUIWidgetRefreshOrder(iOrder);
		}
	}
	//--------------------------------------------------------------------------
	protected virtual void OnUIFrameInitialize() { }
	protected virtual void OnUIFrameInitializePost() { }
	protected virtual void OnUIFrameShow(int iOrder) { }
	protected virtual void OnUIFrameRefreshOrder(int iOrder) { }
	protected virtual void OnUIFrameHide() { }
	protected virtual void OnUIFrameForceHide() { }
	protected virtual void OnUIFrameRemove() { }
	//--------------------------------------------------------------------------
	public CManagerUIFrameFocusBase.EUIFrameFocusType GetUIFrameFocusType()
	{
		return FocusType;
	}
}
