using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollRuneDropItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] ZImage ImageRune = null;


	//----------------------------------------------------------------
	public void DoRuneDropItem(string _runeImage)
	{
		ZManagerUIPreset.Instance.SetSprite(ImageRune, _runeImage);
	}

}
