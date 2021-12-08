using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetBase : CMonoBase
{
    private CUIFrameBase m_pParentFrame; public CUIFrameBase GetUIWidgetParent() { return m_pParentFrame; }
	private RectTransform m_pRectTransform = null;  protected RectTransform GetRectTransform() { return m_pRectTransform; }
	protected CUIWidgetBase m_pWidgetOwner = null;
    //-----------------------------------------------------------
    internal void ImportUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
        m_pParentFrame = pParentFrame;
		m_pRectTransform = GetComponent<RectTransform>();
        OnUIWidgetInitialize(pParentFrame);
    }

    internal void ImportUIWidgetInitializePost(CUIFrameBase pParentFrame)
	{
        OnUIWidgetInitializePost(pParentFrame);
    }

	internal void ImportUIWidgetOwner(CUIWidgetBase pWidgetOwner)
	{
		m_pWidgetOwner = pWidgetOwner;
		OnUIWidgetOwner(pWidgetOwner);
	}

    internal void ImportUIWidgetRefreshOrder(int iOrder)
	{
        OnUIWidgetRefreshOrder(iOrder);
    }

	internal void ImportUIWidgetDelete(CUIFrameBase pParentFrame)
	{
		OnUIWidgetDelete(pParentFrame);
	}

	internal void ImportUIWidgetAdd(CUIFrameBase pParentFrame)
	{
		OnUIWidgetAdd(pParentFrame);
	}
	//-----------------------------------------------------------
	public void DoUIWidgetShowHide(bool bShow)
	{
		if (gameObject.activeSelf != bShow)
		{
			SetMonoActive(bShow);
			OnUIWidgetShowHide(bShow);
		}
	}

	public void DoUIWidgetSelect(CUIWidgetBase pSelectedWidget)
	{
		OnUIWidgetSelect(pSelectedWidget);
	}

	//------------------------------------------------------------
	public void SetUIPositionX(float X)
	{
		m_pRectTransform.localPosition = new Vector3(X, m_pRectTransform.localPosition.y);
	}

	public void SetUIPositionY(float Y)
	{
		m_pRectTransform.localPosition = new Vector3(m_pRectTransform.localPosition.x, Y);
	}

	public void SetUIPosition(float X, float Y)
	{
		m_pRectTransform.localPosition = new Vector2(X, Y);
	}

	public void SetUIPositionMoveX(float X)
	{
		m_pRectTransform.localPosition = new Vector2(m_pRectTransform.localPosition.x + X, m_pRectTransform.localPosition.y);
	}

	public void SetUIPositionMoveY(float Y)
	{
		m_pRectTransform.localPosition = new Vector2(m_pRectTransform.localPosition.x, m_pRectTransform.localPosition.y + Y);
	}

	public void SetUISiblingLowest()
	{
		m_pRectTransform.SetSiblingIndex(0);
	}

	public void SetUISiblingTopMost()
	{
		Transform pParent = transform.parent;
		if (pParent)
		{
			m_pRectTransform.SetSiblingIndex(pParent.childCount);
		}
	}

	//-------------------------------------------------------------------------
	public float GetUIWidth()
	{
		return m_pRectTransform.rect.width;
	}

	public float GetUIHeight()
	{
		return m_pRectTransform.rect.height;
	}

	public Vector2 GetUISize()
	{
		return new Vector2(m_pRectTransform.rect.width, m_pRectTransform.rect.height);
	}

	public Vector3 GetUIPosition()
	{
		return m_pRectTransform.localPosition;
	}

	public float GetUIPositionX()
	{
		return m_pRectTransform.localPosition.x;
	}

	public float GetUIPositionY()
	{
		return m_pRectTransform.localPosition.y;
	}

	//-----------------------------------------------------------
	protected virtual void OnUIWidgetInitialize(CUIFrameBase pParentFrame) { }
    protected virtual void OnUIWidgetInitializePost(CUIFrameBase pParentFrame) { }
    protected virtual void OnUIWidgetRefreshOrder(int iOrder) { }
	protected virtual void OnUIWidgetShowHide(bool bShow) { }
	protected virtual void OnUIWidgetDelete(CUIFrameBase pParentFrame) { }
	protected virtual void OnUIWidgetAdd(CUIFrameBase pParentFrame) { }
	protected virtual void OnUIWidgetSelect(CUIWidgetBase pSelectedWidget) { }
	protected virtual void OnUIWidgetOwner(CUIWidgetBase pWidgetOwner) { }
}


public enum AnchorPresets
{
	TopLeft,
	TopCenter,
	TopRight,

	MiddleLeft,
	MiddleCenter,
	MiddleRight,

	BottomLeft,
	BottonCenter,
	BottomRight,
	BottomStretch,

	VertStretchLeft,
	VertStretchRight,
	VertStretchCenter,

	HorStretchTop,
	HorStretchMiddle,
	HorStretchBottom,

	StretchAll
}

public enum PivotPresets
{
	TopLeft,
	TopCenter,
	TopRight,

	MiddleLeft,
	MiddleCenter,
	MiddleRight,

	BottomLeft,
	BottomCenter,
	BottomRight,
}

public static class RectTransformExtensions
{
	public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
	{
		source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

		switch (allign)
		{
			case (AnchorPresets.TopLeft):
				{
					source.anchorMin = new Vector2(0, 1);
					source.anchorMax = new Vector2(0, 1);
					break;
				}
			case (AnchorPresets.TopCenter):
				{
					source.anchorMin = new Vector2(0.5f, 1);
					source.anchorMax = new Vector2(0.5f, 1);
					break;
				}
			case (AnchorPresets.TopRight):
				{
					source.anchorMin = new Vector2(1, 1);
					source.anchorMax = new Vector2(1, 1);
					break;
				}

			case (AnchorPresets.MiddleLeft):
				{
					source.anchorMin = new Vector2(0, 0.5f);
					source.anchorMax = new Vector2(0, 0.5f);
					break;
				}
			case (AnchorPresets.MiddleCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0.5f);
					source.anchorMax = new Vector2(0.5f, 0.5f);
					break;
				}
			case (AnchorPresets.MiddleRight):
				{
					source.anchorMin = new Vector2(1, 0.5f);
					source.anchorMax = new Vector2(1, 0.5f);
					break;
				}

			case (AnchorPresets.BottomLeft):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(0, 0);
					break;
				}
			case (AnchorPresets.BottonCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0);
					source.anchorMax = new Vector2(0.5f, 0);
					break;
				}
			case (AnchorPresets.BottomRight):
				{
					source.anchorMin = new Vector2(1, 0);
					source.anchorMax = new Vector2(1, 0);
					break;
				}

			case (AnchorPresets.HorStretchTop):
				{
					source.anchorMin = new Vector2(0, 1);
					source.anchorMax = new Vector2(1, 1);
					break;
				}
			case (AnchorPresets.HorStretchMiddle):
				{
					source.anchorMin = new Vector2(0, 0.5f);
					source.anchorMax = new Vector2(1, 0.5f);
					break;
				}
			case (AnchorPresets.HorStretchBottom):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(1, 0);
					break;
				}

			case (AnchorPresets.VertStretchLeft):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(0, 1);
					break;
				}
			case (AnchorPresets.VertStretchCenter):
				{
					source.anchorMin = new Vector2(0.5f, 0);
					source.anchorMax = new Vector2(0.5f, 1);
					break;
				}
			case (AnchorPresets.VertStretchRight):
				{
					source.anchorMin = new Vector2(1, 0);
					source.anchorMax = new Vector2(1, 1);
					break;
				}

			case (AnchorPresets.StretchAll):
				{
					source.anchorMin = new Vector2(0, 0);
					source.anchorMax = new Vector2(1, 1);
					break;
				}
		}
	}

	public static void SetPivot(this RectTransform source, PivotPresets preset)
	{

		switch (preset)
		{
			case (PivotPresets.TopLeft):
				{
					source.pivot = new Vector2(0, 1);
					break;
				}
			case (PivotPresets.TopCenter):
				{
					source.pivot = new Vector2(0.5f, 1);
					break;
				}
			case (PivotPresets.TopRight):
				{
					source.pivot = new Vector2(1, 1);
					break;
				}

			case (PivotPresets.MiddleLeft):
				{
					source.pivot = new Vector2(0, 0.5f);
					break;
				}
			case (PivotPresets.MiddleCenter):
				{
					source.pivot = new Vector2(0.5f, 0.5f);
					break;
				}
			case (PivotPresets.MiddleRight):
				{
					source.pivot = new Vector2(1, 0.5f);
					break;
				}

			case (PivotPresets.BottomLeft):
				{
					source.pivot = new Vector2(0, 0);
					break;
				}
			case (PivotPresets.BottomCenter):
				{
					source.pivot = new Vector2(0.5f, 0);
					break;
				}
			case (PivotPresets.BottomRight):
				{
					source.pivot = new Vector2(1, 0);
					break;
				}
		}
	}
}

