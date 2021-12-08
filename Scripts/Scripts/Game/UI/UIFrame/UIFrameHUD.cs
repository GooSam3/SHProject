using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;
public class UIFrameHUD : ZUIFrameBase
{
	#region System Variable
	public FollowTarget PlayerTargetUI;
	private List<string> Frame = new List<string>();
	private int FrameCnt;
	private Action Callback;
	public E_UIStyle CurUIType { get; private set; }
	#endregion

	/// <summary> 절전모드 </summary>
	public ulong LastInputTime;

	protected override void OnRemove()
	{
		base.OnRemove();
		Me.CurCharData.InvenUpdate -= RefreshAllItemData;
	}

    protected override void OnUnityEnable()
    {
        base.OnUnityEnable();

		LastInputTime = TimeManager.NowSec;
		InvokeRepeating(nameof(CheckScreenSaverTime), 0f, 1.0f);
	}

    protected override void OnUnityDisable()
    {
        base.OnUnityDisable();

		CancelInvoke(nameof(CheckScreenSaverTime));
    }

    public void LateUpdate()
    {
		//절전모드용
		if (Input.GetMouseButton(0))
		{
			LastInputTime = TimeManager.NowSec;
		}
	}

    /// <summary> 최초 1번 들어옴 </summary>
    public void Init(Action _callback)
	{
		Callback = _callback;

		Frame.Add(nameof(UIFrameQuest));
		Frame.Add(nameof(UISubHUDMiniMap));
		Frame.Add(nameof(UISubHudGuildDungeon));
		Frame.Add(nameof(UISubHudInfinityInfo));
		Frame.Add(nameof(UISubHudBossWarInfo));
		Frame.Add(nameof(UISubHudTrialInfo));
		Frame.Add(nameof(UISubHUDBottom));
		Frame.Add(nameof(UISubHUDCharacterAction));
		Frame.Add(nameof(UISubHUDLeftMenu));
		Frame.Add(nameof(UISubHUDQuickSlot));
		Frame.Add(nameof(UISubHUDCurrency));
		Frame.Add(nameof(UISubHUDQuest));
		Frame.Add(nameof(UISubHUDEvent));
		Frame.Add(nameof(UISubHUDPartyMenu));
		Frame.Add(nameof(UISubHUDJoyStick));
		Frame.Add(nameof(UISubHUDMenu));
		Frame.Add(nameof(UIAnotherUserInfo));
		Frame.Add(nameof(UIFrameMailbox));
		Frame.Add(nameof(UIFrameChange));
		Frame.Add(nameof(UIFramePet));
		Frame.Add(nameof(UIFrameRide));
		Frame.Add(nameof(UISubHUDCharacterState));
		Frame.Add(nameof(UISubHUDTemple));		
		Frame.Add(nameof(UIFrameWorldMap));
	
		
		Me.CurCharData.InvenUpdate += RefreshAllItemData;

#if UNITY_EDITOR || ZCHEAT
		Frame.Add(nameof(UICheatPopup));
#endif
		for (int i = 0; i < Frame.Count; i++)
			UIManager.Instance.Load(Frame[i], delegate { CheckLoadingUIFrame(); });
	}

	private void CheckLoadingUIFrame()
	{
		FrameCnt += 1;

		if (FrameCnt == Frame.Count)
		{
			if (UIManager.Instance.Find(out UIFrameMailbox _mailbox)) _mailbox.Init();
			if (UIManager.Instance.Find(out UISubHUDLeftMenu _chatHUD)) _chatHUD.Init();
			Callback();
		}
	}

	#region ScreenSaver
	public void CheckScreenSaverTime()
    {
		if (ZGameOption.Instance.Auto_Screen_Saver_Time != 0 && UIManager.Instance.ScreenSaver == null)
		{
			if ((TimeManager.NowSec - LastInputTime) > ZGameOption.Instance.Auto_Screen_Saver_Time)
			{
				UIManager.Instance.Open<UIFrameScreenSaver>();
			}
		}
	}


	#endregion


	#region Sub Hud
	/// <summary> Scene이 변경되는 경우 상황에 따라 UI 세팅 </summary>
	public void RefreshSubHud(E_StageType _PortalType, E_StageType _prevPortalType = E_StageType.None)
	{
		HideAllContentFrame();
		RefreshCurrency();

		// 향후 PortalType 확정되면 변경바람 (카오스 포탈타입이라 향후 수정될수 있음)
		UpdateSubHud(_PortalType, _prevPortalType);

		if (UIManager.Instance.Find(out UISubHUDMiniMap _minimap)) _minimap.DoMinimapRefresh();
		if (UIManager.Instance.Find(out UIFrameMailbox _mailbox)) _mailbox.RefreshMailList(true);
		//if (UIManager.Instance.Find(out UISubHUDQuickSlot _quick)) _quick.CheckAutoSlotItem();
	}

	/// <summary> 특정 UI의 스타일에 따라 UI 세팅 </summary>
	public void SetSubHudFrame(E_UIStyle _type = E_UIStyle.Normal)
	{
		CurUIType = _type;

		switch (_type)
		{
			case E_UIStyle.IncludeSubScene:
			case E_UIStyle.FullScreen:
				{
					RemoveAllInfoPopup();
					
					UIManager.Instance.Close<UISubHUDBottom>();
					UIManager.Instance.Close<UISubHUDCharacterAction>();
					UIManager.Instance.Close<UISubHUDLeftMenu>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHUDQuickSlot>();
					UIManager.Instance.Close<UISubHUDEvent>();
					UIManager.Instance.Close<UISubHUDPartyMenu>();
					UIManager.Instance.Close<UISubHUDJoyStick>();
					UIManager.Instance.Close<UISubHUDMenu>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UIFrameChatting>();
					UIManager.Instance.Close<UIFrameBuffList>();
					UIManager.Instance.Close<UIFrameFriend>();
					UIManager.Instance.Close<UIFrameSkill>();
					UIManager.Instance.Close<UIAnotherUserInfo>();
					UIManager.Instance.Close<UISubHUDSimpleGain>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();

					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Open<UISubHUDCurrency>();
				}
				break;

			default:
				UpdateSubHud(ZGameModeManager.Instance.Table.StageType);
				break;
		}
	}

	private void UpdateSubHud(E_StageType stageType, E_StageType prevstageType = E_StageType.None)
	{
        ZLog.Log( ZLogChannel.UI , "UpdateSubHud  [ StageType : " + stageType+" ] , [ PrevStageType : "+ prevstageType+" ]");
		// 관리적인 측면에서 모드별로 사용할 subHUD를 open할지 close할지 전부 명시적으로 호출하는게 좋겠음
		switch (stageType)
		{
			case E_StageType.Tutorial:
			case E_StageType.Town:
			case E_StageType.Field:
			case E_StageType.Tower:
				{
					//TODO :: 임시 처리..uimanager에서 hideall을 사용하고 다시 원복할때 autoshowhide 문제 수정해야함.
					UIManager.Instance.Open<UIFrameNameTag>();

					UIManager.Instance.Open<UISubHUDBottom>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDLeftMenu>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDQuest>();
					UIManager.Instance.Open<UISubHUDEvent>();
					UIManager.Instance.Open<UISubHUDPartyMenu>();
					UIManager.Instance.Open<UISubHUDMenu>();
					UIManager.Instance.Open<UIAnotherUserInfo>();
					UIManager.Instance.Close<UISubHUDColosseum>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDTemple>();
					UIManager.Instance.Open<UISubHUDSimpleGain>();
					UIManager.Instance.Open<UISubHUDMiniMap>();
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Open<UISubHUDCurrency>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();

                    //이전 스테이지 타입 기준 추가 처리
                    switch (prevstageType)
                    {
                        case E_StageType.Infinity:
                            if (stageType == E_StageType.Town)
                            {
                                UIManager.Instance.Open<UIFrameDungeon>((_uiName, _dungeonPanel) =>
                                {
                                    _dungeonPanel.OpenDungeonUI(E_StageType.Infinity);
                                });
                            }
                            break;
                    }

					break;
				}
			case E_StageType.Temple:
				{
					UIManager.Instance.Open<UISubHUDBottom>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDLeftMenu>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHUDMenu>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDEvent>();
					UIManager.Instance.Close<UISubHUDPartyMenu>();
					UIManager.Instance.Close<UIAnotherUserInfo>();
					UIManager.Instance.Close<UISubHUDColosseum>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDTemple>();
					UIManager.Instance.Open<UISubHUDSimpleGain>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDCurrency>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					break;
				}
			case E_StageType.Colosseum:
				{
					UIManager.Instance.Open<UISubHUDBottom>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDLeftMenu>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDMenu>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDEvent>();
					UIManager.Instance.Close<UISubHUDPartyMenu>();
					UIManager.Instance.Close<UIAnotherUserInfo>();
					UIManager.Instance.Open<UISubHUDColosseum>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHUDTemple>();
					UIManager.Instance.Close<UISubHUDSimpleGain>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDCurrency>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					break;
				}
			case E_StageType.Instance:
				{
					UIManager.Instance.Open<UISubHudTrialInfo>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					UIManager.Instance.Open<UISubHUDMenu>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					UIManager.Instance.Open<UISubHUDBottom>();
					break;
				}
			case E_StageType.Infinity:
				{
					UIManager.Instance.Open<UISubHudInfinityInfo>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					UIManager.Instance.Open<UISubHUDMenu>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					UIManager.Instance.Open<UISubHUDBottom>();
					break;
				}
			case E_StageType.GodLand: {
					UIManager.Instance.Open<UISubHUDBottom>();
					UIManager.Instance.Close<UISubHUDCharacterAction>();
					UIManager.Instance.Close<UISubHUDLeftMenu>();
					UIManager.Instance.Close<UISubHUDQuickSlot>();
					UIManager.Instance.Close<UISubHUDJoyStick>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDEvent>();
					UIManager.Instance.Close<UISubHUDPartyMenu>();
					UIManager.Instance.Close<UISubHUDMenu>();
					UIManager.Instance.Close<UIAnotherUserInfo>();
					UIManager.Instance.Close<UISubHUDColosseum>();
					UIManager.Instance.Close<UIFrameInventory>();

					//--------------------------------------------
					UIManager.Instance.Close<UISubHUDTemple>();
					UIManager.Instance.Close<UISubHUDSimpleGain>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDCurrency>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Open<UISubHUDGodLand>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					break;
			}
			case E_StageType.InterServer:
				{
					UIManager.Instance.Open<UISubHudBossWarInfo>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					UIManager.Instance.Open<UISubHUDMenu>();
					UIManager.Instance.Close<UIFrameInventory>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHudGuildDungeon>();
					UIManager.Instance.Close<UISubHUDEvent>();
					UIManager.Instance.Open<UISubHUDBottom>();
					break;
				}
			case E_StageType.GuildDungeon:
				{
					UIManager.Instance.Open<UISubHudGuildDungeon>();
					UIManager.Instance.Open<UISubHUDCharacterAction>();
					UIManager.Instance.Open<UISubHUDQuickSlot>();
					UIManager.Instance.Open<UISubHUDJoyStick>();
					UIManager.Instance.Open<UISubHUDMenu>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDCharacterState>();
					UIManager.Instance.Close<UISubHUDQuest>();
					UIManager.Instance.Close<UISubHUDMiniMap>();
					UIManager.Instance.Close<UISubHudTrialInfo>();
					UIManager.Instance.Close<UISubHUDGodLand>();
					//--------------------------------------------
					UIManager.Instance.Close<UISubHudInfinityInfo>();
					UIManager.Instance.Close<UISubHudBossWarInfo>();
					UIManager.Instance.Close<UIFrameInventory>();
					UIManager.Instance.Open<UISubHUDLeftMenu>();
					UIManager.Instance.Close<UISubHUDEvent>();
					//--------------------------------------------
					UIManager.Instance.Open<UISubHUDBottom>();
					break;
				}
		}
	}

	public void SetTempleInteractionUI(E_TempleUIType type)
	{
		UIManager.Instance.Close<UISubHUDBottom>();
		UIManager.Instance.Close<UISubHUDCharacterAction>();
		UIManager.Instance.Close<UISubHUDLeftMenu>();
		UIManager.Instance.Close<UISubHUDMiniMap>();
		UIManager.Instance.Close<UISubHUDQuickSlot>();
		UIManager.Instance.Close<UISubHUDEvent>();
		UIManager.Instance.Close<UISubHUDPartyMenu>();

		if (false == type.HasFlag(E_TempleUIType.Joystick))
		{
			UIManager.Instance.Close<UISubHUDJoyStick>();
		}
		else
		{
			UIManager.Instance.Open<UISubHUDJoyStick>();
		}

		UIManager.Instance.Close<UISubHUDMenu>();
		UIManager.Instance.Close<UISubHUDQuest>();
		UIManager.Instance.Close<UIFrameChatting>();
		UIManager.Instance.Close<UIFrameBuffList>();
		UIManager.Instance.Close<UIFrameFriend>();
		UIManager.Instance.Close<UIAnotherUserInfo>();
		UIManager.Instance.Close<UISubHudTrialInfo>();
		UIManager.Instance.Close<UISubHudInfinityInfo>();
		UIManager.Instance.Close<UISubHudBossWarInfo>();

		UIManager.Instance.Close<UISubHUDCharacterState>();
		UIManager.Instance.Close<UISubHUDCurrency>();
	}

	public void ResetTempleInteractionUI()
	{
		UpdateSubHud(ZGameModeManager.Instance.Table.StageType);
	}
	#endregion

	#region Refresh
	/// <summary> 재화가 표시되는 모든 UI 관련 데이터를 갱신한다.</summary>
	public void RefreshCurrency()
	{
		if (UIManager.Instance.Find(out UIFrameInventory _inventory)) if (_inventory.Show) _inventory.RefreshCoinText();
		if (UIManager.Instance.Find(out UISubHUDCurrency _currency)) if (_currency.Show) _currency.RefreshCurrency();
	}

    /// <summary> 재화가 표시되는 모든 UI 관련 데이터를 갱신한다.</summary>
	public void RefreshCurrency(uint ItemTid)
    {
        if (UIManager.Instance.Find(out UIFrameInventory _inventory) && _inventory.Show && ItemTid == DBConfig.Gold_ID)
            _inventory.RefreshCoinText();
        if (UIManager.Instance.Find(out UISubHUDCurrency _currency) && _currency.Show)
            _currency.RefreshCurrency(ItemTid);
    }

    /// <summary> Item이 표시되는 모든 UI 관련 데이터를 갱신한다.(단, 해당 UI가 오픈되어있지 않은 경우 갱신하지 않는다.)</summary>
    public void RefreshAllItemData(bool _setCheck)
	{
		if (UIManager.Instance.Find(out UIFrameInventory _inventory))
		{
			if (_setCheck)
				_inventory.ScrollAdapter.SetData();
			else
				_inventory.ScrollAdapter.RefreshData();

			if (_inventory.ScrollAdapter.IsInitialized && _inventory.ScrollAdapter.Data != null)
				_inventory.RefreshInvenVolume();
		}
		if (UIManager.Instance.Find(out UISubHUDCharacterState _charstate)) if (_charstate.Show) _charstate.UpdateEquipSlot();
		if (UIManager.Instance.Find(out UISubHUDCharacterAction _action)) if (_action.Show) _action.SetPotionInfo();
		if (UIManager.Instance.Find(out UISubHUDMenu _menu)) _menu.CheckSlotWeight();
		if (UIManager.Instance.Find(out UISubHUDQuickSlot _quickslot)) if (_quickslot.Show) _quickslot.RefreshAllSlot();

		if (Me.CurCharData.GetNewGainItemList().Count > 0 && _menu != null)
			_menu.ActiveNewAlarm(UISubHUDMenu.E_TopMenuButton.Bag, true);
	}

	/// <summary> 모든 개별 콘텐츠 Frame이 존재하는지 확인한 후 해당 정보 팝업을 제거한다.</summary>
	public void RemoveAllInfoPopup()
	{
		if (UIManager.Instance.Find(out UIFrameInventory _inventory)) _inventory.RemoveInfoPopup();
		if (UIManager.Instance.Find(out UIFrameMailbox _mailbox)) _mailbox.RemoveInfoPopup();
		if (UIManager.Instance.Find(out UISubHUDCharacterState _charstate)) _charstate.RemoveInfoPopup();
		if (UIManager.Instance.Find(out UIFrameDungeon frameDugeon)) frameDugeon.RemoveInfoPopup();
		if (UIManager.Instance.Find(out UIFrameColosseum frameColosseum)) frameColosseum.RemoveInfoPopup();
	}

	/// <summary> 모든 모달팝업들을 닫는다. 더 있다면 추가 바람 </summary>
	public void HideAllModalPopup()
	{
		UIManager.Instance.Close<UIMessagePopupNormal>();
		UIManager.Instance.Close<UIMessagePopupDefault>();
		UIManager.Instance.Close<UIMessagePopupCost>();
		UIManager.Instance.Close<UIMessagePopupInput>();
		UIManager.Instance.Close<UIPopupColosseumResult>();
	}

	/// <summary> 모든 개별 콘텐츠 Frame이 존재하는지 확인한 후 숨긴다.</summary>
	public void HideAllContentFrame()
	{
		UIManager.Instance.Close<UIFrameInventory>();
		UIManager.Instance.Close<UIFrameMailbox>();
		UIManager.Instance.Close<UIFrameChange>();
		UIManager.Instance.Close<UIFramePet>();
		UIManager.Instance.Close<UIFrameRide>();
		UIManager.Instance.Close<UIFramePetChangeSelect>();
		UIManager.Instance.Close<UIFrameSkill>();
		UIManager.Instance.Close<UIFrameBuffList>();
		UIManager.Instance.Close<UIFrameChatting>();
		UIManager.Instance.Close<UIFrameStorage>();
		UIManager.Instance.Close<UIFrameItemShop>();
		UIManager.Instance.Close<UIFrameChatting>();
		UIManager.Instance.Close<UIFrameItemDisassemble>();
		UIManager.Instance.Close<UIFrameItemEnchant>();
		UIManager.Instance.Close<UIFrameItemEnhance>();
		UIManager.Instance.Close<UIFrameItemUpgrade>();
		UIManager.Instance.Close<UIFrameOption>();
		UIManager.Instance.Close<UIFrameMark>();
		UIManager.Instance.Close<UIEnhanceElement>();
		UIManager.Instance.Close<UIFrameDungeon>();
		UIManager.Instance.Close<UIFrameTemple>();
		UIManager.Instance.Close<UIFrameColosseum>();
		UIManager.Instance.Close<UIFrameItemMake>();
		UIManager.Instance.Close<UIFrameCook>();
		UIManager.Instance.Close<UIFrameItemGem>();
		UIManager.Instance.Close<UIFrameArtifact>();
		UIManager.Instance.Close<UIFrameGuild>();
		UIManager.Instance.Close<UIFrameFriend>();
		UIManager.Instance.Close<UIFrameTrade>();
		UIManager.Instance.Close<UIFrameItemCollection>();
		UIManager.Instance.Close<UIFrameMileage>();
		UIManager.Instance.Close<UIFrameRank>();
		UIManager.Instance.Close<UIFrameDeathInfo>(true);   // 부활팝업은 로직상 Remove 필요
		UIManager.Instance.Close<UIFrameSpecialShop>();
		UIManager.Instance.Close<UIFrameBlessBuff>();
		UIManager.Instance.Close<UIFrameQuest>();
		UIManager.Instance.Close<UIPopupSystem>();
		UIManager.Instance.Close<UIFrameWorldMap>();
		UIManager.Instance.Close<UIFrameDungeonFailPopup>(true);
		UIManager.Instance.Close<UIMessagePopupNormal>();
		UIManager.Instance.Close<UIMessagePopupDefault>();
		UIManager.Instance.Close<UIMessagePopupCost>();
		UIManager.Instance.Close<UIMessagePopupInput>();
		UIManager.Instance.Close<UIFrameBlackMarket>(true);
	}
	#endregion
}