using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class CUIFrameBase : CMonoBase
{
	[SerializeField]
	private CManagerUIFrameFocusBase.EUIFrameFocusType FocusType = CManagerUIFrameFocusBase.EUIFrameFocusType.PanelChain;

	private GraphicRaycaster		m_pRayCaster;
	private bool					m_bShow = false;			public bool pShow { get { return m_bShow; } }			// 현재 보이는지 안 보이는지 
	private bool					m_bAppear = false;		public bool pAppear { get { return m_bAppear; } }	    // 다른 프레임에 의해 임으로 가려진 상태
	private int					m_iSortOrder = 0;			public int  pSortOrder { get { return m_iSortOrder; } }
	protected Canvas				m_pCanvas;				public Canvas GetUIFrameCanvas() { return m_pCanvas; }
	protected RectTransform		m_pRectTransform;
	protected Vector2				m_vecUIFrameSize = Vector2.zero;
	private List<CUIWidgetBase>				m_listWidgetInstance = new List<CUIWidgetBase>();
	private List<CUIWidgetDialogBase>			m_listWidgetDialog = new List<CUIWidgetDialogBase>();
	private LinkedList<CUIWidgetWindowBase>		m_listWidgetWindow = new LinkedList<CUIWidgetWindowBase>();
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
		
		PrivUIFrameInitializeUIWidget();

		OnUIFrameInitialize();
	}

	internal void ImportUIFrameInitializePost()
	{
		PrivUIFrameInitializeUIWidgetPost();
		OnUIFrameInitializePost();
	}

	internal void ImportUIFrameShow(int iOrder)
	{
		SetMonoActive(true);
		m_bShow = true;
		m_bAppear = true;
		PrivUIFrameRefreshOrder(iOrder);
		OnUIFrameShow(iOrder);
	}

	internal void ImportUIFrameAppear()
	{
		SetMonoActive(true);
		m_bAppear = true;
		OnUIFrameAppear();
	}

	internal void ImportUIFrameDisappear()
	{
		if (m_bAppear) return;
		SetMonoActive(false);
		m_bAppear = false;
		OnUIFrameDisappear();
	}

	internal void ImportUIFrameRefreshOrder(int iOrder)
	{
		PrivUIFrameRefreshOrder(iOrder);
		OnUIFrameRefreshOrder(iOrder);
	}

	internal void ImportUIFrameHide()
	{
		SetMonoActive(false);
		m_bShow = false;
		m_bAppear = false;
		OnUIFrameHide();
	}

	internal void ImportUIFrameForceHide()
	{
		SetMonoActive(false);
		m_bShow = false;
		OnUIFrameForceHide();
	}

	internal void ImportUIFrameRemove()
	{
		OnUIFrameRemove();
	}

	internal bool ImportUIFrameClose()  // 자신이 아닌 외부에 의해 종료되었다. (디바이스 뒤로가기 버튼)
	{
		bool bClose = false;

		if (m_listWidgetWindow.Count > 0)
		{
			CUIWidgetWindowBase pWindow = m_listWidgetWindow.Last.Value;
			pWindow.DoUIWidgetShowHide(false);
			m_listWidgetWindow.RemoveLast();
			OnUIFrameWindow(pWindow);
		}

		if (m_listWidgetWindow.Count == 0)
		{
			bClose = true;
			OnUIFrameClose();
		}

		return bClose;
	}
	internal void ImportUIFrameDialog(bool bAdd, CUIWidgetDialogBase pDialog)
	{
		if (bAdd)
		{
			if (m_listWidgetDialog.Contains(pDialog) == false)
			{
				m_listWidgetDialog.Add(pDialog);
			}
		}
		else
		{
			m_listWidgetDialog.Remove(pDialog);
		}
	}

	//---------------------------------------------------------------------
	private void Update()
	{
		OnUIFrameUpdate();
	}

	public static Vector2 WorldToCanvas(Vector3 vecWorldPosition, Camera pCamera = null)
	{
		if (pCamera == null)
		{
			pCamera = CManagerUIFrameBase.Instance.GetUIManagerCamara();
		}

		return pCamera.WorldToScreenPoint(vecWorldPosition);
	}

	//-----------------------------------------------------------------------
	public void InternalWidgetAdd(CUIWidgetBase pWidget)
	{
		m_listWidgetInstance.Add(pWidget);
		pWidget.ImportUIWidgetInitialize(this);
		pWidget.ImportUIWidgetInitializePost(this);
		pWidget.ImportUIWidgetAdd(this);
	}

	public void InternalWidgetDelete(CUIWidgetBase pWidget)
	{
		m_listWidgetInstance.Remove(pWidget);
		pWidget.ImportUIWidgetDelete(this);
	}

	public void InternalWindowShowHide(CUIWidgetWindowBase pWidgetWindow, bool bShow)
	{
		if (m_bShow)
		{
			m_listWidgetWindow.AddLast(pWidgetWindow);
		}
		else
		{
			m_listWidgetWindow.Remove(pWidgetWindow);
		}
	}

	public void InternalDialogShow(CUIWidgetDialogBase pWidgetDialog)
	{
		for (int i = 0; i < m_listWidgetDialog.Count; i++)
		{
			if (pWidgetDialog != m_listWidgetDialog[i])
			{
				m_listWidgetDialog[i].DoUIWidgetShowHide(false);
			}
		}
	}

	//-------------------------------------------------------------------------
	private void PrivUIFrameInitializeUIWidget()
	{
		GetComponentsInChildren(true, m_listWidgetInstance);

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
		m_iSortOrder = iOrder;
		m_pCanvas.overrideSorting = true;
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
	protected virtual void OnUIFrameUpdate() { }
	protected virtual void OnUIFrameAppear() { }
	protected virtual void OnUIFrameDisappear() { }
	protected virtual void OnUIFrameWindow(CUIWidgetWindowBase pWindow) { }
	protected virtual void OnUIFrameClose() { }
	//--------------------------------------------------------------------------
	public CManagerUIFrameFocusBase.EUIFrameFocusType GetUIFrameFocusType()
	{
		return FocusType;
	}
}
