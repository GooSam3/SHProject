using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using GameDB;

public class UIAnotherUserInfo : ZUIFrameBase
{
	#region UI Variable
	[SerializeField] private GameObject ObjRoot;
	[SerializeField] private GameObject ObjGuild;
	[SerializeField] private Image GuildIcon;
	[SerializeField] private Text CharacterName;
	[SerializeField] private Text GuildName;
	[SerializeField] private GameObject FriendIcon;
	#endregion

	#region System Variable
	private ulong CharacterId;
	ulong TargetDisplayGuildID;
	#endregion

	private ZPawnMyPc mMyPc;

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
		ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);
    }

    protected override void OnHide()
    {
        base.OnHide();

		RemoveEvents();
	}

    protected override void OnRemove()
    {
        base.OnRemove();

		RemoveEvents();
	}

	private void RemoveEvents()
    {
		if (null != mMyPc)
			mMyPc.DoRemoveEventChangeTarget(HandleChangeTarget);

		if (ZPawnManager.hasInstance)
			ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);
	}

	/// <summary> 내 캐릭터 생성시(이미 생성되어있다면 바로) 알림 </summary>
    private void HandleCreateMyEntity()
	{
        if (null != mMyPc)
            mMyPc.DoRemoveEventChangeTarget(HandleChangeTarget);

		mMyPc = ZPawnManager.Instance.MyEntity;

		mMyPc.DoAddEventChangeTarget(HandleChangeTarget);

		//ui 한번 갱신
		UpdateUI();
	}

	/// <summary> 타겟 변경될 때 호출됨 </summary>
	private void HandleChangeTarget(uint preTargetEntityId, uint targetEntityId)
    {
		UpdateUI();		
	}

	private void SetGuildInfo(ulong guildID)
    {
        if (guildID == 0)
        {
            GuildIcon.gameObject.SetActive(false);
            GuildName.gameObject.SetActive(false);
        }

        TargetDisplayGuildID = guildID;

        ZWebManager.Instance.WebGame.REQ_GetGuildInfo(guildID
            , (revPacket, resList) =>
            {
                if (resList.GuildInfo.HasValue && resList.GuildInfo.Value.GuildId == TargetDisplayGuildID)
                {
					ObjGuild.gameObject.SetActive(true);
					GuildIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBGuild.GetGuildMark(resList.GuildInfo.Value.MarkTid));
					GuildIcon.gameObject.SetActive(true);
					GuildName.text = resList.GuildInfo.Value.Name;
					GuildName.gameObject.SetActive(string.IsNullOrEmpty(resList.GuildInfo.Value.Name) == false);
                }
                else
                {
					ObjGuild.gameObject.SetActive(false);
					GuildIcon.gameObject.SetActive(false);
					GuildName.gameObject.SetActive(false);
				}

				TargetDisplayGuildID = 0;
			},
			(err, req, res) =>
			{
				TargetDisplayGuildID = 0;

				ObjGuild.gameObject.SetActive(false);
				GuildIcon.sprite = null;
				GuildIcon.gameObject.SetActive(false);
				GuildName.text = string.Empty;
				GuildName.gameObject.SetActive(false);
			});
	}

	/// <summary> ui 갱신 </summary>
	private void UpdateUI()
    {
		var target = mMyPc.GetTarget();

		//캐릭터 일 경우에만 처리
		if (null == target || target.EntityType != E_UnitType.Character)
		{
			//ui 가리기
			ObjRoot.SetActive(false);
			//UIManager.Instance.Open<UISubHUDCurrency>();
			var currency = UIManager.Instance.Find<UISubHUDCurrency>();
			if( currency != null ) {
				currency.WeakShowCurrency();
			}
			return;
		}

		ObjRoot.SetActive(true);
		//UIManager.Instance.Close<UISubHUDCurrency>();
		var currencyHud = UIManager.Instance.Find<UISubHUDCurrency>();
		if( currencyHud != null ) {
			currencyHud.WeakHideCurrency();
		}
		var characterData = target.To<ZPawnCharacter>().CharacterData;

		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		CharacterId = characterData.CharacterId;

		SetGuildInfo(characterData.GuildId);
		
		CharacterName.text = characterData.Name;

		ObjGuild.gameObject.SetActive(Me.CurCharData.GuildMarkTid != 0);

		var friend = Me.CurCharData.friendList.Find(item => item.CharId == CharacterId);

		FriendIcon.SetActive(friend == null);
	}

	public void OnFriendAdd()
	{
		// 친구 리스트가 가득 차있습니다.
		if (Me.CurCharData.friendList.FindAll(some => some.IsFriend).Count >= DBConfig.Friend_Max_Character)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_Max"), new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 친구 요청 리스트가 가득 차있습니다.
		if (Me.CurCharData.requestfriendList.FindAll(some => some.friendReqState == WebNet.E_FriendRequestState.Request).Count >= DBConfig.Friend_Invite_Max)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_ReqMax"), new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 나의 캐릭터는 친구등록을 할 수 없습니다.
		if (Me.CurUserData.GetCharacter(CharacterName.text) != null)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_Self"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}
		// 이미 친구 리스트에 존재 합니다.
		else if (Me.CurCharData.friendList.Find(some => some.Nick == CharacterName.text && some.IsFriend) != null)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyFriend"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}
		// 이미 경계 리스트에 존재 합니다.
		else if (Me.CurCharData.friendList.Find(some => some.Nick == CharacterName.text && some.IsAlert) != null)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyAlret"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}
		// 이미 요청 상태 입니다.
		else if (Me.CurCharData.requestfriendList.Find(some => some.Nick == CharacterName.text) != null)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyReq"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 유저 찾기 패킷.
		ZWebManager.Instance.WebGame.REQ_FindFriend(CharacterName.text, (recvPacket, recvMsgPacket) =>
		{
			// 친구 추가 패킷.
			ZWebManager.Instance.WebGame.REQ_AddFriend(recvMsgPacket.FindCharId, (x, y) =>
			{
				UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("AddFriendAlret_Success"), CharacterName.text), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			});
		}, (_errorType, _reqPacket, _recvPacket) =>
		{
			if ((WebNet.ERROR)_recvPacket.ErrCode == WebNet.ERROR.CHARACTER_NOT_FIND)
				UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Register_NotingUser"), CharacterName.text), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			else
				ZWebManager.Instance.ProcessErrorPacket(_errorType, _reqPacket, _recvPacket, false);
		});

	}

	public void OnPartyInvite()
	{
		ZPartyManager.Instance.Req_InviteParty(ZPartyManager.Instance.ServerIdx, CharacterId, CharacterName.text, (recvPacket, Msg) => { 
			if(recvPacket.ErrCode == WebNet.ERROR.NO_ERROR)
			{
				UICommon.OpenSystemPopup_One(DBLocale.GetText("Party_Apply"),
				"파티에 초대하였습니다.", ZUIString.LOCALE_OK_BUTTON);
			}
		});
	}

	public void OnWhisper()
	{
		if(UIManager.Instance.Find(out UIFrameChatting _chatting))
		{
			_chatting.OnClickChatTab((int)UIFrameChatting.E_ChatTabType.Chatting);
			_chatting.SetSendType(UIFrameChatting.E_ChatSendType.Whisper, CharacterName.text, CharacterId);
		}
		else
		{
			UIManager.Instance.Open<UIFrameChatting>((_frameName, _frame) => {
				_frame.Init(() => {
					_frame.OnClickChatTab((int)UIFrameChatting.E_ChatTabType.Chatting);
					_frame.SetSendType(UIFrameChatting.E_ChatSendType.Whisper, CharacterName.text, CharacterId);
				});
			});
		}
	}
}