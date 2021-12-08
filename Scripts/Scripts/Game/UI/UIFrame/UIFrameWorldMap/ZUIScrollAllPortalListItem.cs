using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class ZUIScrollAllPortalListItem : CUGUIWidgetSlotItemBase
{
	[System.Serializable]
	public class STargetColor
	{
		public Graphic TargetGraphic = null;
		public Color SelectColor = Color.white;
		public Color DeSelectColor = Color.white;

		public STargetColor() { SelectColor = Color.white; DeSelectColor = Color.white; }
	}

	[SerializeField] ZUIButtonToggle  Favorite = null;
	[SerializeField] ZText			   PortalName = null;
	[SerializeField] ZImage		   PortalType = null;
	[SerializeField] ZImage		   FocusMarker = null;
	[SerializeField] List<STargetColor> Transition = new List<STargetColor>();           
	private uint mPortalID = 0;
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		PortalType.gameObject.SetActive(true);
		FocusMarker.gameObject.SetActive(false);
		SlotItemColor(false);
	}

	protected override void OnSlotItemSelect()
	{
		base.OnSlotItemSelect();
		SlotItemColor(true);
	}

	protected override void OnSlotItemDeSelect()
	{
		base.OnSlotItemDeSelect();
		SlotItemColor(false);
	}

	//--------------------------------------------------------------
	public void SetPortalListItem(uint _PortalID, string _PortalName, bool _toggleOn)
	{
		mPortalID = _PortalID;
		PortalName.text = _PortalName;

		Favorite.SetUIButtonArgument((int)_PortalID);
		Favorite.DoToggleAction(_toggleOn);
	}

	public void SetPortalFavorite(bool _toggleOn)
	{
		Favorite.DoToggleAction(_toggleOn);
	}

	public void SetPortalFocus(bool _focus)
	{
		FocusMarker.gameObject.SetActive(_focus);
	}

	public uint GetPortalID()
    {
		return mPortalID;
    }

	//-------------------------------------------------------------
	private void SlotItemColor(bool _select)
	{
		for (int i = 0; i < Transition.Count; i++)
		{
			if (_select)
			{
				Transition[i].TargetGraphic.color = Transition[i].SelectColor;
			}
			else
			{
				Transition[i].TargetGraphic.color = Transition[i].DeSelectColor;
			}
		}
	}

	//-------------------------------------------------------------
	public void HandlePortalSelect()
	{
		UIFrameWorldMap worldMap = mUIFrameParent as UIFrameWorldMap;
		worldMap.DoUIPortalInfoOpenClose(true, mPortalID);
        worldMap.DoUIPortalListOpenClose(true);
        mOwnerScrollRect.ImportUIScrolSlotItemSelect(this);
    }
}
