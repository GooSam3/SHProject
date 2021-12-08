public class UINameTagPlayer : UINameTagBase
{
	private ulong mCharacterID = 0;
	//----------------------------------------------------------
	protected override void OnNameTagInitialize(ZPawn _followPawn)
	{
		base.OnNameTagInitialize(_followPawn);
		ZPawnCharacter pawnCharacter = _followPawn as ZPawnCharacter;
		if (pawnCharacter == null) return;

		RefreshNameTagColor();

		mFollowPawn.DoAddEventUpdateCustomConditionControl(HandlePvPCondition);
		mFollowPawn.DoAddEventChangeTendency(HandleTendency);

		mCharacterID = pawnCharacter.CharacterData.CharacterId;
		ZWebChatData.AddOnChatMsg(mCharacterID, HandleNameTagChat);
		ZWebChatData.AddOnEmoticonMsg(mCharacterID, HandleNameTagEmoji);

		// 옵션 설정관련 처리 
		ZGameOption.Instance.OnOptionChanged += HandleNameTagVisibleOption;

		//탈것 탑승 및 해제 관련 처리
		pawnCharacter.DoAddEventChangeVehicle(HandleEventChangeVehicle);
	}

	private void HandleEventChangeVehicle(ZVehicle vehicle)
    {
		if(null != vehicle)
        {
			ChangeFollowTarget(vehicle.gameObject, vehicle.GetSocket(mFollowSocket), vehicle.GetSocket(mRaycastSocket));
		}
		else
        {
			HandleNameTagChangeModel();

		}
	}

	protected override void OnNameTagRemove()
	{
		base.OnNameTagRemove();

		if (mFollowPawn)
		{
			mFollowPawn.DoRemoveEventUpdateCustomConditionControl(HandlePvPCondition);
			mFollowPawn.DoRemoveEventChangeTendency(HandleTendency);
			mFollowPawn.To<ZPawnCharacter>().DoRemoveEventChangeVehicle(HandleEventChangeVehicle);
		}

		ZWebChatData.RemoveOnChatMsg(mCharacterID, HandleNameTagChat);
		ZWebChatData.RemoveOnEmoticonMsg(mCharacterID, HandleNameTagEmoji);
		ZGameOption.Instance.OnOptionChanged -= HandleNameTagVisibleOption;
	}

	//----------------------------------------------------------
	private void HandlePvPCondition(E_CustomConditionControl _condition, bool _bApply)
	{
		if (_condition.HasFlag(E_CustomConditionControl.Pk))
			RefreshNameTagColor();
	}

	private void HandleTendency(int _tendencyValue)
	{
		RefreshNameTagColor();
	}

	private void HandleNameTagChat(ulong _charID, ZDefine.ChatData _message)
	{
		ZPawn chatOwner = ZPawnManager.Instance.FindEntityByCharID(_charID) as ZPawn;
		if (chatOwner)
		{
			OutputChatMassage(chatOwner, _message);
		}
	}

	private void HandleNameTagEmoji(ulong _charID, string _spriteName)
	{
		SetNameTagEmoji(_spriteName);
	}

	private void HandleNameTagVisibleOption(ZGameOption.OptionKey _optionKey)
	{
		if (_optionKey == ZGameOption.OptionKey.Option_ShowCharacterName)
		{
			SetNameTagNameOnOff(ZGameOption.Instance.bShowCharacterName);
		}
	}

	//--------------------------------------------------------
	private void RefreshNameTagColor()
	{
		switch(ZGameModeManager.Instance.CurrentGameModeType) {
			case E_GameModeType.GodLand: {
				if (mFollowPawn.IsMyPc) {
					SetNameTagColorNormal();
				}
				else {
					SetNameTagColorEnemyPC();
				}
				break;
			}
			case E_GameModeType.Colosseum: {
				if (mFollowPawn.IsMyPc) {
					SetNameTagColorNormal();
					return;
				}

				var myEntity = ZPawnManager.Instance.MyEntity;
				if (myEntity != null && mFollowPawn.EntityData.IsEnemy(myEntity.EntityData)) {
					SetNameTagColorEnemyPC();
				}
				else {
					SetNameTagColorNormal();
				}
				break;
			}
			default: {
				if (mFollowPawn.IsCustomConditionControl(E_CustomConditionControl.Pk)) 
				{
					SetNameTagColorPK();
				}
				else 
				{
					SetNameTagColorTendency(mFollowPawn.Tendency);
				}
				break;
			}
		}
	}

	private void OutputChatMassage(ZPawn _chatOwner, ZDefine.ChatData _message)
	{
		bool outPutMessage = true;
		if (_message.type == ZDefine.ChatViewType.TYPE_ALLIANCE_CHAT)
		{
			if (_chatOwner.IsCustomConditionControl(E_CustomConditionControl.AllianceGuild) == false)
			{
				outPutMessage = false;
			}
		}
		else if (_message.type == ZDefine.ChatViewType.TYPE_GUILD_CHAT)
		{
			if (_chatOwner.IsCustomConditionControl(E_CustomConditionControl.GuildMember) == false)
			{
				outPutMessage = false;
			}
		}
		else if (_message.type == ZDefine.ChatViewType.TYPE_PARTY_CHAT)
		{
			if (_chatOwner.IsCustomConditionControl(E_CustomConditionControl.PartyMember) == false)
			{
				outPutMessage = false;
			}
		}
		
		if (outPutMessage)
		{
			SetNameTagChattingMessage(_message.MessageOrigin, ZWebChatData.GetChatViewColor(_message.type));
		}
	}
}
