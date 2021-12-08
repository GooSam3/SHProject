using UnityEngine;

public class ZUIMonsterDropItemLabel : CUGUIWidgetSlotItemBase
{
	[SerializeField] private ZText ItemName = null;

	//--------------------------------------------------------
	public void DoMonsterDropItemLabel(UIFrameWorldMap.SMonsterDropItem _dropItem)
	{
		SetMonoActive(true);
	
	}
}
