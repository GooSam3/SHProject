using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetBase : CMonoBase
{
    private CUIFrameBase m_pParentFrame; public CUIFrameBase GetUIWidgetParent() { return m_pParentFrame; }
	private RectTransform m_pRectTransform = null;
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

    internal void ImportUIWidgetRefreshOrder(int iOrder)
	{
        OnUIWidgetRefreshOrder(iOrder);
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

	//------------------------------------------------------------
	public void SetUIPositionX(float X)
	{
		m_pRectTransform.anchoredPosition = new Vector2(X, m_pRectTransform.anchoredPosition.y);
	}

	public void SetUIPositionY(float Y)
	{
		m_pRectTransform.anchoredPosition = new Vector2(m_pRectTransform.anchoredPosition.x, Y);
	}

	public void SetUIPosition(float X, float Y)
	{
		m_pRectTransform.anchoredPosition = new Vector2(X, Y);
	}

	public void SetUIPositionMoveX(float X)
	{
		m_pRectTransform.anchoredPosition = new Vector2(m_pRectTransform.anchoredPosition.x + X, m_pRectTransform.anchoredPosition.y);
	}

	public void SetUIPositionMoveY(float Y)
	{
		m_pRectTransform.anchoredPosition = new Vector2(m_pRectTransform.transform.localPosition.x, m_pRectTransform.transform.localPosition.y + Y);
	}

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

	//-----------------------------------------------------------
	protected virtual void OnUIWidgetInitialize(CUIFrameBase pParentFrame) { }
    protected virtual void OnUIWidgetInitializePost(CUIFrameBase pParentFrame) { }
    protected virtual void OnUIWidgetRefreshOrder(int iOrder) { }
	protected virtual void OnUIWidgetShowHide(bool bShow) { }

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