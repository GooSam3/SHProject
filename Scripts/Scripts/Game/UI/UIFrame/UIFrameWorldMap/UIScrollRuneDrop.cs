using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScrollRuneDrop : CUGUIScrollRectBase
{


	//------------------------------------------------------
	public void DoRuneDropList(List<string> _runeImage)
	{
		ProtUIScrollSlotItemReturnAll();

		for(int i = 0; i < _runeImage.Count; i++)
		{
			UIScrollRuneDropItem item = ProtUIScrollSlotItemRequest() as UIScrollRuneDropItem;
			item.DoRuneDropItem(_runeImage[i]);
		}
	}

}
