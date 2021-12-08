using GameDB;
using UnityEngine;

public class ZUIWidgetPortalInfo : CUGUIWidgetBase
{
	[SerializeField]
	private ZText		PortalName = null;
	[SerializeField]
	private ZText		PriceChaos = null;
	[SerializeField]
	private ZText		PriceNormal = null;
	[SerializeField]
	private ZImage	IconChaos = null;
	[SerializeField]
	private ZImage	IconNormal = null;
	[SerializeField]
	private ZUIButtonToggle Toggle = null;
	[SerializeField] ZButton ButtonMonsterInfo = null;
	[SerializeField] ZButton ButtonChaos = null;
	[SerializeField] ZButton ButtonNormal = null;


	private uint mPortalID = 0;
	private uint mPortalStage = 0;
	private UIFrameWorldMap mWorldMap = null;
	//---------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mWorldMap = _UIFrameParent as UIFrameWorldMap;
	}

	//---------------------------------------------------------------
	public void DoPortalInfo(uint _portalID, bool _favorite, bool _noMonster)
	{
		Portal_Table portalTable;
		DBPortal.TryGet(_portalID, out portalTable);

		if (portalTable == null) return;

		Item_Table itemTable = null;
		mPortalID = _portalID;
		mPortalStage = portalTable.StageID;
		PortalName.text = UIFrameWorldMap.ConvertPortalName(portalTable);

		if (portalTable.UseItemID == 0 || portalTable.UseItemCount == 0)
		{
			PriceNormal.text = "";
			IconNormal.gameObject.SetActive(false);
			ButtonNormal.interactable = true;
		}
		else
		{
			itemTable = DBItem.GetItem(portalTable.UseItemID);
			if (itemTable != null)
			{
				IconNormal.gameObject.SetActive(true);
				IconNormal.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
				PriceNormal.text = portalTable.UseItemCount.ToString();
				ButtonNormal.interactable = true;
			}
			else
			{
				PriceNormal.text = "";
				IconNormal.gameObject.SetActive(false);
				ButtonNormal.interactable = false;
			}
		}

		itemTable = DBItem.GetItem(portalTable.ChaosChannelUseItemID);
		if (itemTable != null)
		{
			IconChaos.gameObject.SetActive(true);
			IconChaos.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
			PriceChaos.text = portalTable.ChaosChannelUseItemCnt.ToString();
			ButtonChaos.interactable = true;
		}
		else
		{
			PriceChaos.text = "";
			IconChaos.gameObject.SetActive(false);
			ButtonChaos.interactable = false;
		}

		ButtonMonsterInfo.interactable = !_noMonster;

		Toggle.SetUIButtonArgument((int)mPortalID);
		Toggle.DoToggleAction(_favorite);
	}

	public void DoPortalInfoFavoriteOnOff(uint _portalID, bool _on)
	{
		if (mPortalID == _portalID)
		{
			Toggle.DoToggleAction(_on);
		}
	}

	//---------------------------------------------------------------
	public void HandleMonsterQuestInfo()
	{
		mWorldMap.DoUIMonsterDropOpenClose(mPortalID, mPortalStage, true);
	}

	public void HandleMoveWalk()
	{
		mWorldMap.DoMoveLocalMap(mPortalID, E_MapMoveType.Walk);
	}

	public void HandleMoveTeleportDanger()
	{
		mWorldMap.DoMoveLocalMap(mPortalID, E_MapMoveType.Teleport_Danger);
	}

	public void HandleMoveTeleportSafe()
	{
		mWorldMap.DoMoveLocalMap(mPortalID, E_MapMoveType.Teleport_Safe);
	}
}
