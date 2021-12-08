using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CUIWidgetIconBase : CUIWidgetBase
{
	public enum EIconStickerType
	{
		None = -1,
		LeftTop = 0,
		RightTop,
		LeftBottom,
	}

	[SerializeField]
	private bool				DragAndDrop = false;
	[SerializeField]
	private CImage			IconLock = null;
	[SerializeField]
	private CImageClickable		IconBody = null;
	[SerializeField]
	private Text				TextCount = null;
	[SerializeField]
	private List<GameObject>	IconBoard = new List<GameObject>();
	[SerializeField]
	private List<GameObject>	IconEdge = new List<GameObject>();
	[SerializeField]
	private GameObject []		IconSticker = new GameObject[3];
	[SerializeField]
	private GameObject		IconFocus = null;

	private bool m_bInitialize = false;
	//-------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		PrivIconInitialize();
	}

	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		PrivIconInitialize();
	}

	//---------------------------------------------------------------------------
	protected void ProtIconCount(int iMin, int iMax = 0)
	{
		if (iMin == 0 || iMin == 1)
		{
			TextCount.gameObject.SetActive(false);
			return;
		}

		TextCount.gameObject.SetActive(true);
		if (iMax <= iMin)
		{
			TextCount.text = iMin.ToString();
		}
		else
		{
			TextCount.text = string.Format("{0} - {1}", iMin, iMax);
		}
	}

	protected void ProtIconGrade(int iGrade)
	{
		for (int i = 0; i < IconBoard.Count; i++)
		{
			if (i == iGrade)
			{
				IconBoard[i].SetActive(true);
			}
			else
			{
				IconBoard[i].SetActive(false);
			}
		}

		for (int i = 0; i < IconEdge.Count; i++)
		{
			if (i == iGrade)
			{
				IconEdge[i].SetActive(true);
			}
			else
			{
				IconEdge[i].SetActive(false);
			}
		}
	}

	protected void ProtIconSetting(Sprite pIconSprite, int iGrade, int iCountMin, int iCountMax)
	{
		ProtIconReset();
		IconBody.sprite = pIconSprite;
		IconBody.enabled = true;
		ProtIconGrade(iGrade);
		ProtIconCount(iCountMin, iCountMax);
	}

	protected GameObject ProtIconStickerEnable(EIconStickerType eStickerType, bool bEnable)
	{
		GameObject pGameObject = IconSticker[(int)eStickerType];
		if (pGameObject != null)
		{
			pGameObject.SetActive(bEnable);
		}

		return pGameObject;
	}

	protected void ProtIconStickerReset()
	{
		for(int i = 0; i < IconSticker.Length; i++)
		{
			if (IconSticker[i])
			{
				IconSticker[i].SetActive(false);
			}
		}
	}

	protected void ProtIconLock(bool bLock)
	{
		if (IconLock)
		{
			IconLock.gameObject.SetActive(bLock);
		}

		IconBody.raycastTarget = !bLock;
	}

	protected void ProtIconFocus(bool bEnable)
	{
		if (IconFocus)
		{
			IconFocus.gameObject.SetActive(bEnable);
		}
	}

	protected void ProtIconReset()
	{
		IconBody.enabled = false;
		IconFocus.gameObject.SetActive(false);
		ProtIconGrade(0);
		ProtIconCount(0);
		ProtIconLock(false);
		ProtIconStickerReset();
	}
	//-----------------------------------------------------------------------
	private void PrivIconInitialize()
	{
		if (m_bInitialize) return;
		m_bInitialize = true;
		if (DragAndDrop)
		{
			IconBody.SetImageInputEvent(HandleIconClick, true, HandleIconDragOn, HandleIconDragStart, HandleIconDragEnd);
		}
		else
		{
			IconBody.SetImageInputEvent(HandleIconClick);
		}
		ProtIconReset();
	}

	//------------------------------------------------------------------------
	private void HandleIconClick(Vector2 vecPosition)
	{
		OnUIIconClick(vecPosition);
	}

	private void HandleIconDragStart(Vector2 vecPosition)
	{
		OnUIIconDragStart(vecPosition);
	}

	private void HandleIconDragOn(Vector2 vecPosition)
	{
		OnUIIconDragOn(vecPosition);
	}

	private void HandleIconDragEnd(Vector2 vecPosition)
	{
		OnUIIconDragEnd(vecPosition);
	}

	//------------------------------------------------------------------------
	protected virtual void OnUIIconClick(Vector2 vecPosition) { }
	protected virtual void OnUIIconDragStart(Vector2 vecPosition) { }
	protected virtual void OnUIIconDragOn(Vector2 vecPosition) { }
	protected virtual void OnUIIconDragEnd(Vector2 vecPosition) { }

}
