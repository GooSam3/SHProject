using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class UIMinimapChannelPopup : CUIWidgetPopupBase
{
	[SerializeField] ZText StageTitle = null;
	[SerializeField] ZText Channel = null;
	[SerializeField] UIChannelScroll ChannelScroll = null;
	[SerializeField] GameObject ChaosDescription = null;

	private uint mPortalID = 0;
	//--------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		UIManager.Instance.RegisterFocusNotify(HandleChannelOtherFrameOnOff, true);
	}

	//--------------------------------------------------------------
	public void DoMinimapChannelPopup(Stage_Table _stageTable, uint _currentChannel)
	{
		ChannelScroll.DoChannelScrollReset();
		StageTitle.text = _stageTable.StageTextID;
		
		mPortalID = _stageTable.DefaultPortal;
		Portal_Table portalTable = DBPortal.Get(_stageTable.DefaultPortal);

		if (portalTable == null) return;

		if (ZGameModeManager.Instance.IsChaosChannel())
		{
			ChaosDescription.SetActive(true);
			Channel.text = string.Format("{0} {1}", DBLocale.GetText("CHAOS_Channel_Name"), _currentChannel.ToString());
		}
		else
		{
			ChaosDescription.SetActive(false);
			Channel.text = string.Format("채널 {0}", _currentChannel.ToString());
		}

		ZGameModeManager.Instance.RefreshChannelList((channelList) => {			
			for (int i = 0; i < channelList.Count; i++)
			{
				if (channelList[i].IsBossZone == false && channelList[i].IsPvPZone == false)
				{
					ChannelScroll.DoChannelScrollAdd(channelList[i], _currentChannel, portalTable, this);
				}
			}
		});
	}

	public void DoMinimapChannelSelect(uint _channelID, bool _chaosChannel)
	{
		DoUIWidgetShowHide(false);
		if (_chaosChannel)
		{   // 사용되지 않음
			//UIManager.Instance.Open<UIPopupStageMove>((name, _uiStageMove) => {
			//	_uiStageMove.DoUIStageMove(UIPopupStageMove.E_ChannelType.ChaosChannel, mPortalID, _channelID);
			//});
		}
		else
		{
			ZGameManager.Instance.DoChangeChannel(_channelID);
			DoUIWidgetShowHide(false);
		}
	}
	//---------------------------------------------------------------
	public void HandleChannelHide()
	{
		DoUIWidgetShowHide(false);
	}

	private void HandleChannelOtherFrameOnOff(CUIFrameBase _otherFrame, bool _on)
	{
		if (_on)
		{
			DoUIWidgetShowHide(false);
		}
	}


}
