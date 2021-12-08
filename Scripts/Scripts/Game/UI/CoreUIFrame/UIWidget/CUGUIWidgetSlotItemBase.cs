using System.Collections.Generic;
using UnityEngine.UI;
abstract public class CUGUIWidgetSlotItemBase : CUGUIWidgetBase
{
	public interface ISlotItemOwner
    {
		void ISlotItemSelect(CUGUIWidgetSlotItemBase _SelectItem);	
    }

	private int mSlotItemIndex = -1;
	protected ISlotItemOwner mSlotItemOwner;
	protected CUGUIScrollRectBase mOwnerScrollRect = null;
	private List<Selectable> mSelectable = new List<Selectable>();
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		GetComponentsInChildren<Selectable>(true, mSelectable);
	}

	//-------------------------------------------------------------
	public void DoSlotItemShow()
	{
		OnSlotItemShow();
	}

	public void DoSlotItemHide()
	{
		OnSlotItemHide();
	}

	public void DoSlotItemSelect()
	{
		OnSlotItemSelect();
	}	

	public void DoSlotItemDeSelect()
	{
		OnSlotItemDeSelect();
	}

	public void DoSlotItemSelectableUnSelect()
	{
		for (int i = 0; i < mSelectable.Count; i++)
		{
			mSelectable[i].OnDeselect(null);
		}
	}

	public void SetSlotItemIndex(int _SlotItemIndex)
	{
		mSlotItemIndex = _SlotItemIndex;
	}

	public void SetSlotItemOwner(ISlotItemOwner _ItemOwner)
    {
		mSlotItemOwner = _ItemOwner;
    }

	public void SetSlotItemOwnerScrollRect(CUGUIScrollRectBase _scrollRect)
	{
		mOwnerScrollRect = _scrollRect;
	}

	public int GetSlotItemIndex()
	{
		return mSlotItemIndex;
	}
	//-------------------------------------------------------------
	protected virtual void OnSlotItemShow() { }
	protected virtual void OnSlotItemHide() { }
	protected virtual void OnSlotItemSelect() { }
	protected virtual void OnSlotItemDeSelect() { }
}
