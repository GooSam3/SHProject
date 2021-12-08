using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameFriend : ZUIFrameBase
{
	#region Frame
	public override bool IsBackable => true;
	#endregion

	public enum FriendUIType
	{
		FRIEND,	// 친구
		REQUEST,// 요청
		ALERT,	// 경계
		NONE,
	}

	public enum ActionType
	{
		WHISPER,		// 귓말
		DELETE,			// 친구삭제
		CANCEL,			// 친구요청취소
		ACCEPT,			// 수락
		DENY,			// 거부
		ALERT_DELETE,	// 경계 삭제
	}

	#region UI Variable
	[SerializeField] private GameObject NewRequestIcon;
	[SerializeField] private GameObject FriendGroup;
	[SerializeField] private GameObject RequestGroup;
	[SerializeField] private GameObject AlertGroup;
	[SerializeField] private UIFriendScrollAdapter FriendScrollAdapter;
	[SerializeField] private UIFriendRequestScrollAdapter RequestScrollAdapter;
	[SerializeField] private UIFriendAlertScrollAdapter AlertScrollAdapter;

	[SerializeField] private Text FriendEmptyListText;
	[SerializeField] private Text RequestEmptyListText;
	[SerializeField] private Text AlertEmptyListText;

	[SerializeField] private Text FriendReqText;
	[SerializeField] private Text FriendRecvText;

	[SerializeField] ZToggle DefaultTab = null;
	#endregion

	#region System Variable
	private FriendUIType CurUIType = FriendUIType.NONE;
	private List<Friend> FriendList = new List<Friend>();
	#endregion

	protected override void OnInitialize()
	{
		base.OnInitialize();

		FriendScrollAdapter.Initialize();
		RequestScrollAdapter.Initialize();
		AlertScrollAdapter.Initialize();
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		var hudSubMenu = UIManager.Instance.Find<UISubHUDCharacterState>();
		if (hudSubMenu != null)
		{
			hudSubMenu.OnActiveInfoPopup(false);
		}
		var buffFrame = UIManager.Instance.Find<UIFrameBuffList>();
		if (buffFrame != null)
		{
			buffFrame.OnClickClose();
		}
		var chatFrame = UIManager.Instance.Find<UIFrameChatting>();
		if(chatFrame!=null)
        {
			chatFrame.Close();
        }

		CurUIType = FriendUIType.NONE;

		DefaultTab.SelectToggle();

		SetReceiveRequestNewIcon();
	}

	public void SetReceiveRequestNewIcon()
	{
		if (UIManager.Instance.Find(out UISubHUDMenu _menu))
			_menu.ActiveRedDot(E_HUDMenu.Friend, Me.CurCharData.GetReceiveRequestFriend().Count > 0);

		NewRequestIcon.SetActive(Me.CurCharData.GetReceiveRequestFriend().Count > 0);
	}

	protected override void OnHide()
	{
		base.OnHide();

		NewRequestIcon.SetActive(false);
	}

	/// <summary>
	/// 탭 클릭(친구, 요청, 경계).
	/// </summary>
	public void OnClickTab(int _changedUIType)
	{
		if (CurUIType == (FriendUIType)_changedUIType)
			return;

		CurUIType = (FriendUIType)_changedUIType;
        UpdateUIType();
	}

	private void UpdateUIType()
	{
		FriendGroup.SetActive(false);
		RequestGroup.SetActive(false);
		AlertGroup.SetActive(false);

		FriendList.Clear();

		switch (CurUIType)
		{
			case FriendUIType.FRIEND:
				{
					FriendGroup.SetActive(true);

					ZWebManager.Instance.WebGame.REQ_GetFriendList((recvPacket, _onError) =>
					{
						foreach (var friend in Me.CurCharData.friendList.FindAll(some => some.IsFriend))
						{
							FriendList.Add(friend);
						}

						// 친구 목록 정렬
						FriendList.Sort((x, y) => x.IsLogin.CompareTo(y.IsLogin));

						FriendEmptyListText.text = DBLocale.GetText("Empty_FriendList");
						FriendEmptyListText.gameObject.SetActive(FriendList.Count <= 0);

						FriendScrollAdapter.SetScrollData(FriendList);
					});
				}
				break;

			case FriendUIType.REQUEST:
				{
					RequestGroup.SetActive(true);

					ZWebManager.Instance.WebGame.REQ_GetRequestFriendList((recvPacket, _onError) =>
					{
						int ReqCnt = 0, RecvCnt = 0;
						foreach (var friend in Me.CurCharData.requestfriendList)
						{
							if (friend.friendReqState == WebNet.E_FriendRequestState.Request)
								ReqCnt++;
							else if (friend.friendReqState == WebNet.E_FriendRequestState.Receive)
								RecvCnt++;

							FriendList.Add(friend);
						}

						// 친구 목록 정렬
						FriendList.Sort((x, y) => x.IsLogin.CompareTo(y.IsLogin));

						RequestEmptyListText.text = DBLocale.GetText("Empty_FriendReqList");
						RequestEmptyListText.gameObject.SetActive(FriendList.Count <= 0);

						FriendReqText.text = string.Format(DBLocale.GetText("FriendReqCount"), ReqCnt, DBConfig.Friend_Invite_Max);
						FriendRecvText.text = string.Format(DBLocale.GetText("FriendRecvCount"), RecvCnt, DBConfig.Friend_Invited_Max);

						RequestScrollAdapter.SetScrollData(FriendList);
					});
				}
				break;

			case FriendUIType.ALERT:
				{
					AlertGroup.SetActive(true);

					ZWebManager.Instance.WebGame.REQ_GetFriendList((recvPacket, _onError) =>
					{
						foreach (var friend in Me.CurCharData.friendList.FindAll(some => some.IsAlert))
						{
							FriendList.Add(friend);
						}

						// 친구 목록 정렬
						FriendList.Sort((x, y) => x.IsAlert.CompareTo(y.IsAlert));

						AlertEmptyListText.text = DBLocale.GetText("Empty_FriendAlretList");
						AlertEmptyListText.gameObject.SetActive(FriendList.Count <= 0);

						AlertScrollAdapter.SetScrollData(FriendList);
					});
				}
				break;
		}
	}

	/// <summary>
	/// 친구 추가
	/// </summary>
	public void OnClickAddFriend()
	{
		// 친구 리스트가 가득 차있습니다.
		if (Me.CurCharData.friendList.FindAll(some=>some.IsFriend).Count >= DBConfig.Friend_Max_Character)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_Max"), new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 친구 요청 리스트가 가득 차있습니다.
		if (Me.CurCharData.requestfriendList.FindAll(some=>some.friendReqState == WebNet.E_FriendRequestState.Request).Count >= DBConfig.Friend_Invite_Max)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_ReqMax"), new Color(255, 0, 116), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 팝업 start
		UIMessagePopup.ShowInputPopup(DBLocale.GetText("AddFriendTitle"), DBLocale.GetText("AddFriendDesc"), (nick) =>
		{
			// 닉네임 길이 제한 조건에 맞지 않음 
			if(nick.Length < DBConfig.NickName_Length_Min)
            {
				UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("Chatting_Error_Whisper_NickLength"), DBConfig.NickName_Length_Min, DBConfig.NickName_Length_Max), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            }
			// 나의 캐릭터는 친구등록을 할 수 없습니다.
			else if (Me.CurUserData.GetCharacter(nick) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_Self"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 이미 친구 리스트에 존재 합니다.
			else if (Me.CurCharData.friendList.Find(some => some.Nick == nick && some.IsFriend) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyFriend"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 이미 경계 리스트에 존재 합니다.
			else if (Me.CurCharData.friendList.Find(some => some.Nick == nick && some.IsAlert) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyAlret"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 이미 요청 상태 입니다.
			else if (Me.CurCharData.requestfriendList.Find(some => some.Nick == nick) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddFriendAlret_AlreadyReq"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			else if (NTCommon.StringUtil.ValidateCommonName(nick) == false)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("닉네임에 특수문자를 포함할 수 없습니다."), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			else
			{
				// 유저 찾기 패킷.
				ZWebManager.Instance.WebGame.REQ_FindFriend(nick, (recvPacket, recvMsgPacket) =>
				{
					// 친구 추가 패킷.
					ZWebManager.Instance.WebGame.REQ_AddFriend(recvMsgPacket.FindCharId, (x, y) =>
					{
						UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("AddFriendAlret_Success"), nick), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
						UpdateUIType();
					});
				}, (_errorType, _reqPacket, _recvPacket) => 
				{
					if ((WebNet.ERROR)_recvPacket.ErrCode == WebNet.ERROR.CHARACTER_NOT_FIND)
						UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Register_NotingUser"), nick), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
					else
						ZWebManager.Instance.ProcessErrorPacket(_errorType, _reqPacket, _recvPacket, false);
				});
			}
		}
		, characterLimit : DBConfig.NickName_Length_Max);
		// 팝업 end
	}

	/// <summary>
	/// 경계 추가
	/// </summary>
	public void OnClickAddAlert()
	{
		// 경계 리스트가 가득 차있습니다.
		if (Me.CurCharData.friendList.FindAll(some => some.IsAlert).Count >= DBConfig.Alert_Max_Character)
		{
			UICommon.SetNoticeMessage(DBLocale.GetText("AddAlret_Max"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			return;
		}

		// 팝업 start
		UIMessagePopup.ShowInputPopup(DBLocale.GetText("AddAlretTitle"), DBLocale.GetText("AddAlretDesc"), (nick) =>
		{
			// 닉네임 길이 제한 조건에 맞지 않음 
			if (nick.Length < DBConfig.NickName_Length_Min)
            {
				UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("Chatting_Error_Whisper_NickLength"), DBConfig.NickName_Length_Min, DBConfig.NickName_Length_Max), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 나의 캐릭터는 경계등록을 할 수 없습니다.
			else if (Me.CurUserData.GetCharacter(nick) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddAlret_Self"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 이미 등록된 경계 대상입니다.
			else if (Me.CurCharData.friendList.Find(some => some.Nick == nick && some.IsAlert) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddAlret_AlreadyAlret"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			// 이미 요청 상태 입니다.
			else if (Me.CurCharData.requestfriendList.Find(some => some.Nick == nick) != null)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText("AddAlret_AlreadyReq"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			else
			{
				// 유저 찾기 패킷.
				ZWebManager.Instance.WebGame.REQ_FindFriend(nick, (recvPacket, recvMsgPacket) =>
				{
					// 경계 추가 패킷.
					ZWebManager.Instance.WebGame.REQ_AddAlertFriend(recvMsgPacket.FindCharId, (x, y) =>
					{
						UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("AddAlret_Success"), nick), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
						UpdateUIType();
					});
				}, (_errorType, _reqPacket, _recvPacket) =>
				{
					// 닉네임을 다시 확인 해 주세요.
					if ((WebNet.ERROR)_recvPacket.ErrCode == WebNet.ERROR.CHARACTER_NOT_FIND)
						UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Register_NotingUser"), nick), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
					else
						ZWebManager.Instance.ProcessErrorPacket(_errorType, _reqPacket, _recvPacket, false);
				});
			}
		}, characterLimit: DBConfig.NickName_Length_Max);
		// 팝업 end
	}

	/// <summary>
	///  버튼 액션
	/// </summary>
	/// <param name="actionType">버튼 타입</param>
	/// <param name="friendData">Friend 데이터</param>
	public void ClickHolderAction(ActionType actionType, Friend friendData)
	{
		switch (actionType)
		{
			case ActionType.WHISPER:
				// TODO : 귓속말
				UIFrameChatting chattingFrame = UIManager.Instance.Find<UIFrameChatting>();

				if (chattingFrame == null)
				{
					UIManager.Instance.Load(nameof(UIFrameChatting), (_loadName, _loadFrame) =>
					{
						(_loadFrame as UIFrameChatting).Init(() =>
						{
							UIManager.Instance.Open<UIFrameChatting>();
							UIManager.Instance.Find<UIFrameChatting>().SetWhisperUser(friendData.Nick, friendData.CharId);
							Close();
						});
					});
				}
				else
				{
					if (!chattingFrame.Show)
					{
						var hudSubMenu = UIManager.Instance.Find<UISubHUDCharacterState>();
						if (hudSubMenu != null)
						{
							hudSubMenu.OnActiveInfoPopup(false);
						}
						UIManager.Instance.Open<UIFrameChatting>();
						chattingFrame.SetWhisperUser(friendData.Nick, friendData.CharId);
						Close();
					}
					else
						UIManager.Instance.Close<UIFrameChatting>();
				}
				break;
			case ActionType.DELETE:
				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText(string.Format(DBLocale.GetText("FriendAlert_Delete"), friendData.Nick)),
					new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
					new Action[] {
							delegate { _popup?.Close(); },
							delegate
							{
								ZWebManager.Instance.WebGame.REQ_DelFriend(WebNet.E_FriendState.Friend, friendData.CharId, (recvPacket, recvMsgPacket) => {
									UpdateUIType();
									_popup?.Close();
								});
							}
					});
				});
				break;
			case ActionType.CANCEL:
				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText(string.Format(DBLocale.GetText("FriendAlert_CancelReq"), friendData.Nick)),
					new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
					new Action[] {
							delegate { _popup?.Close(); },
							delegate
							{
								ZWebManager.Instance.WebGame.REQ_CancelReqFriend(friendData.CharId, (recvPacket, recvMsgPacket) => {
									UpdateUIType();
									_popup?.Close();
								}
								, (err, req, res) =>
								{
									/// 상대가 이미 취소했다면 내 리스트에서 제거한다 
                                    if(res.ErrCode == WebNet.ERROR.ALREADY_CANCEL_FRIEND_REQUEST)
									{
										int removeIndex = Me.CurCharData.requestfriendList.FindIndex(t => t.CharId == friendData.CharId);

										if(removeIndex != -1)
										{
											Me.CurCharData.requestfriendList.RemoveAt(removeIndex);
										}

										UpdateUIType();
									}

									_popup?.Close();
								});
							}
					});
				});
				break;
			case ActionType.ACCEPT:
				if (Me.CurCharData.friendList.FindAll(some => some.IsFriend).Count >= DBConfig.Friend_Max_Character)
				{
					UICommon.SetNoticeMessage(DBLocale.GetText("WFriend_Friend_NoAdd"), new Color(255, 255, 255), 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
					return;
				}

				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText(string.Format(DBLocale.GetText("FriendAlert_AcceptReq"), friendData.Nick)),
					new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
					new Action[] {
							delegate { _popup?.Close(); },
							delegate
							{
								ZWebManager.Instance.WebGame.REQ_AcceptReqFriend(friendData.CharId, (recvPacket, recvMsgPacket) => {
									UpdateUIType();
									SetReceiveRequestNewIcon();
									_popup?.Close();
								});
							}
					});
				});
				break;
			case ActionType.DENY:
				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText(string.Format(DBLocale.GetText("FriendAlert_DenyReq"), friendData.Nick)),
					new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
					new Action[] {
							delegate { _popup?.Close(); },
							delegate
							{
								ZWebManager.Instance.WebGame.REQ_CancelReqFriend(friendData.CharId, (recvPacket, recvMsgPacket) => {
									UpdateUIType();
									SetReceiveRequestNewIcon();
									_popup?.Close();
								});
							}
					});
				});
				break;

			case ActionType.ALERT_DELETE:
				UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
				{
					_popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText(string.Format(DBLocale.GetText("FriendAlert_DeleteAlert"), friendData.Nick)),
					new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
					new Action[] {
							delegate { _popup?.Close(); },
							delegate
							{
								ZWebManager.Instance.WebGame.REQ_DelFriend(WebNet.E_FriendState.AlertFriend, friendData.CharId, (recvPacket, recvMsgPacket) => {
									UpdateUIType();
									_popup?.Close();
								});
							}
					});
				});
				break;
		}
	}

	public void Close()
	{
		UIManager.Instance.Close<UIFrameFriend>();
	}
}
