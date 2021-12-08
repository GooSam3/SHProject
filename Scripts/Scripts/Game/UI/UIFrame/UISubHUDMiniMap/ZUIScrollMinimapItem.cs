using UnityEngine;
using UnityEngine.UI;

public class ZUIScrollMinimapItem : CUGUIWidgetSlotItemBase
{
	private Image mMinimapParts = null;
	//----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mMinimapParts = GetComponent<Image>();
		mMinimapParts.useSpriteMesh = true;
	}

	//----------------------------------------------------------
	public void DoMinimapItemSprite(Sprite _sprite, int _index)
	{
		mMinimapParts.sprite = _sprite;
	}
}
