using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
public class ZUIScrollFavoriteListItem : CUGUIWidgetSlotItemBase
{
    [System.Serializable]
    public class STargetColor
    {
        public Graphic TargetGraphic = null;
        public Color SelectColor = Color.white;
        public Color DeSelectColor = Color.white;

        public STargetColor() { SelectColor = Color.white; DeSelectColor = Color.white; }
    }

    [SerializeField] private ZUIButtonToggle FavoriteToggle = null;
    [SerializeField] private ZText ItemText;
    [SerializeField] List<STargetColor> Transition = new List<STargetColor>();

    private uint mPortalTID = 0;    public uint pPortalTID { get { return mPortalTID; } }
	//----------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
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
    //---------------------------------------------------
    public void DoFavoriteListItem(uint _portalTID, string _portalName)
	{
        mPortalTID = _portalTID;
        ItemText.text = _portalName;
        
        if (FavoriteToggle != null)
            FavoriteToggle.DoToggleAction(true);
	}

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

    //-----------------------------------------------------
    public void HandleFavoriteOnOff(bool _On)
	{
        UIFrameWorldMap worldMap = mUIFrameParent as UIFrameWorldMap;
        worldMap.SetWorldMapFavoriteItem(mPortalTID, _On);
	}

    public void HandleFavoriteSelectPortal()
	{
        UIFrameWorldMap worldMap = mUIFrameParent as UIFrameWorldMap;
        worldMap.DoUIPortalInfoOpenClose(true, mPortalTID);
        worldMap.DoUIPortalFavoriteOpenClose(true);
        mOwnerScrollRect.ImportUIScrolSlotItemSelect(this); 
    }
    //-------------------------------------------------------

}
