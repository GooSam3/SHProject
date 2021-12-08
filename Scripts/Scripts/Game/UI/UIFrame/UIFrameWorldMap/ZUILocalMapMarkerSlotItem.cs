using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class ZUILocalMapMarkerSlotItem : CUGUIWidgetSlotItemBase
{
	private uint mPortalID = 0;  public uint pPortalID { get { return mPortalID; } }
	private ZText				  mMarkerName = null;

	[SerializeField] private Image imgIcon;
	[SerializeField] private GameObject objSelect;

	private ZUIButtonCommand mButtonCommand = null;
	//----------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mMarkerName = GetComponentInChildren<ZText>();
		imgIcon.transform.localScale = Vector2.one;
		mButtonCommand = GetComponentInChildren<ZUIButtonCommand>();
		objSelect.SetActive(false);
	}
	//------------------------------------------------
	public void SetLocalMapMakerInfo(uint _PortalID, string _PortTalName, E_PortalType _PortalType)
	{
		mPortalID = _PortalID;
		mMarkerName.text = _PortTalName;
		mButtonCommand.SetUIButtonArgument((int)_PortalID);
	}

	public void SetFocus(bool state)
    {
		objSelect.SetActive(state);

		imgIcon.transform.localScale = state ? new Vector2(1.25f, 1.25f): Vector2.one;	
    }
}
