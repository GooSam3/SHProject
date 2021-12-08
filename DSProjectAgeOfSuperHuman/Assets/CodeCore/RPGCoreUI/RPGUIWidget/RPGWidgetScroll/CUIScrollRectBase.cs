using UnityEngine;

[RequireComponent(typeof(CScrollRect))]
abstract public class CUIScrollRectBase : CUIWidgetTemplate
{
	[SerializeField] bool ParentDragEvent = false;  // 스크롤 랙트가 스크롤 랙트를 포함하고 있을 경우

	protected CScrollRect mScrollRect = null;			public CScrollRect GetScrollRect() { return mScrollRect; }
	protected RectTransform mContentTransform = null;
	private int m_iSelectedIndex = -1; public void SetScrollRectSelectedIndex(int iIndex) { m_iSelectedIndex = iIndex; } public int GetScrollRectSelectedIndex() { return m_iSelectedIndex; }
	//-------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mScrollRect = GetComponent<CScrollRect>();
		mScrollRect.onValueChanged.AddListener(HandleUIScrollValueChange);
		mContentTransform = mScrollRect.content.transform as RectTransform;
		if (mScrollRect.viewport == null)
		{
			mScrollRect.viewport = mScrollRect.transform as RectTransform;
		}

		if (ParentDragEvent)
		{
			mScrollRect.FindParentsScrollRect();
		}
	}  

	protected override void OnUITemplateItem(CUIWidgetTemplateItemBase pItem)
	{
		pItem.transform.SetParent(mScrollRect.content, false);
	}

	//-------------------------------------------------------

	protected void ProtUIScrollMoveTopItem(RectTransform _topItem, float _offsetY)
	{
		int totalChild = mScrollRect.content.childCount;
		float topPosition = 0;
		Vector3 scrollPosition = Vector3.zero;
		for (int i = 0; i < totalChild; i++)
		{
			RectTransform child = mScrollRect.content.GetChild(i) as RectTransform;
			if (i == 0)
			{
				scrollPosition = child.position;
			}
			else
			{
				if (scrollPosition.y != child.position.y)
				{
					scrollPosition = child.position;
					topPosition += child.rect.height;
				}
			}

			if (child == _topItem)
			{
				break;
			}
		}

		if (topPosition != 0) topPosition += _offsetY;

		Vector2 position = mScrollRect.content.anchoredPosition;
		position.y = topPosition;
		mScrollRect.content.anchoredPosition = position;
	}

	protected CUIWidgetTemplateItemBase ProtUIScrollChildItem(int iIndex)
	{
		CUIWidgetTemplateItemBase pChildItem = null;

		if ( iIndex < mScrollRect.content.childCount)
		{
			pChildItem = mScrollRect.content.GetChild(iIndex).gameObject.GetComponent<CUIWidgetTemplateItemBase>();
		}

		return pChildItem;
	}
	//-------------------------------------------------------	
	private void HandleUIScrollValueChange(Vector2 _ChangeValue)
	{
		OnUIScrollValueChange(_ChangeValue);
	}

	//-----------------------------------------------------------
	protected virtual void OnUIScrollValueChange(Vector2 _ChangeValue) { }

}
