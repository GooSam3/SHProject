using UnityEngine;

[RequireComponent(typeof(ZScrollRect))]
abstract public class CUGUIScrollRectBase : CUGUIWidgetTemplateItemBase
{
	[SerializeField] bool ParentDragEvent = false;

	protected ZScrollRect mScrollRect = null;     public ZScrollRect GetScrollRect() { return mScrollRect; }
	protected RectTransform mContentTransform = null;
	private CUGUIWidgetSlotItemBase mSelectedCharacter = null;
	//-------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mScrollRect = GetComponent<ZScrollRect>();
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

	//-------------------------------------------------------
	protected override CUGUIWidgetSlotItemBase ProtUIScrollSlotItemRequest(Transform _parent = null)
	{
		CUGUIWidgetSlotItemBase NewItem = base.ProtUIScrollSlotItemRequest(mScrollRect.content.transform);
		NewItem.SetSlotItemOwnerScrollRect(this);
		NewItem.DoSlotItemDeSelect();
		return NewItem;
	}

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

	public void ImportUIScrolSlotItemSelect(CUGUIWidgetSlotItemBase _selectedItem)
	{
		if (mSelectedCharacter != null)
		{
			mSelectedCharacter.DoSlotItemDeSelect();
		}

		mSelectedCharacter = _selectedItem;
		mSelectedCharacter.DoSlotItemSelect();
	}

	//-------------------------------------------------------
	
	private void HandleUIScrollValueChange(Vector2 _ChangeValue)
	{
		OnUIScrollValueChange(_ChangeValue);
	}

	//-----------------------------------------------------------
	protected virtual void OnUIScrollValueChange(Vector2 _ChangeValue) { }

}
