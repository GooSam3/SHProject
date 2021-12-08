using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUISlotPotentialOptionItem : CUIWidgetBase
{
	[SerializeField]
	private ESHStatType ItemStat = ESHStatType.None;

	private ESHStatType m_eStatType = ESHStatType.None;  public ESHStatType GetSlotPotentialOptionType() { return m_eStatType; }
	private uint m_iStatValue = 0;
	//-----------------------------------------------------



	//-------------------------------------------------------
	public ESHStatType GetPotentialOptionItemStatType() { return ItemStat; }
}
