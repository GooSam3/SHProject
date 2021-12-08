using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIFrameMailbox : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private ZToggle[] TopTab;
    [SerializeField] private ZToggle[] WriteTab;
    [SerializeField] private GameObject RecvMailIcon;
    [SerializeField] private Text RecvMailCnt;
    [SerializeField] private GameObject MailWindow;
    [SerializeField] private GameObject MessageWindow;
    [SerializeField] private GameObject MessageWriteWindow;
    [SerializeField] private GameObject PaperInsideWindow;
    [SerializeField] private UIMailboxScrollAdapter ScrollAdapter;
    [SerializeField] private UIMailboxMessageScrollAdapter MessageScrollAdapter;
    [SerializeField] private Text BgText;
    #endregion

    #region Message Variable
    private WebNet.E_MessageType WriteMessageType;
    [SerializeField] private InputField ReceiverText, WriteTitleText, WriteMessageText;
    [SerializeField] private Text ReceiverTitleText, WriteMessageGoldCntText;
    [SerializeField] private Text RemainText;
    [SerializeField] private Text TitleInputText, MessageInputText;
    private uint WriteMessageGoldCnt = 0;
    [SerializeField] private GameObject RecvMessageIcon;
    [SerializeField] private Text RecvMessageCnt;
    [SerializeField] private Text RecvBottomMessageCnt;
    [SerializeField] private Text RemainTime;
    [SerializeField] private Text PaperType;
    [SerializeField] private Text ReadTitleText;
    [SerializeField] private Text SenderText;
    [SerializeField] private Text ReadMessageText;
    [SerializeField] private GameObject ObjDeleteMessageAllButton;
    [SerializeField] private GameObject ObjRecvMailAllButton;
    private ulong MessageIdx;
    #endregion

    #region System Variable
    [SerializeField] private UIPopupItemInfo InfoPopup;
    [SerializeField] private E_MailboxWindow CurMailboxWindow = E_MailboxWindow.Mail;

    public override bool IsBackable => true;

    private bool Isinit = false;
	#endregion

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

        if(!Isinit)
            return;

        TopTab[(int)E_MailboxWindow.Mail].SelectToggleAction((ZToggle _toggle) => {
            ShowWindow((int)E_MailboxWindow.Mail);
        });

        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);

        if (UIManager.Instance.Find(out UISubHUDMenu _menu))
            _menu.ActiveRedDot(E_HUDMenu.Mailbox, false);
    }

	protected override void OnHide()
	{
		base.OnHide();

        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame();

        if (UIManager.Instance.Find(out UISubHUDMenu _menu))
            _menu.ActiveRedDot(E_HUDMenu.Mailbox, Me.CurCharData.MailList.Count > 0 || Me.CurCharData.GetNotReadMessage());

        WriteTab[(int)WebNet.E_MessageType.Normal].SelectToggle();

        RemoveInfoPopup();
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIMailboxMailListItem));
        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIMailboxMessageListItem));
    }

    public void Init()
    {
        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMailboxMailListItem), objMail=>
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMailboxMessageListItem), objMessage=>
            {
                Isinit = true;

                Initialize();

                ZPoolManager.Instance.Return(objMail);
                ZPoolManager.Instance.Return(objMessage);

            }, 0, 1, false);
        }, 0, 1, false);
    }

    private void Initialize()
    {
        TopTab[(int)E_MailboxWindow.Mail].SelectToggleAction((ZToggle _toggle) => {
            ShowWindow((int)E_MailboxWindow.Mail);
        });

        #region OSA ScrollView Initialize (최초 1번 세팅)
        if (ScrollAdapter.Parameters.ItemPrefab == null)
        {
            GameObject mailItem = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIMailboxMailListItem));
            ScrollAdapter.Parameters.ItemPrefab = mailItem.GetComponent<RectTransform>();
            ScrollAdapter.Parameters.ItemPrefab.SetParent(ScrollAdapter.GetComponent<Transform>());
            ScrollAdapter.Parameters.ItemPrefab.transform.localScale = Vector2.one;
            ScrollAdapter.Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
            ScrollAdapter.Parameters.ItemPrefab.gameObject.SetActive(false);
        }

        ScrollAdapter.Init();

        // MessageAdapter..
        if (MessageScrollAdapter.Parameters.ItemPrefab == null)
        {
            GameObject messageItem = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIMailboxMessageListItem));
            MessageScrollAdapter.Parameters.ItemPrefab = messageItem.GetComponent<RectTransform>();
            MessageScrollAdapter.Parameters.ItemPrefab.SetParent(MessageScrollAdapter.GetComponent<Transform>());
            MessageScrollAdapter.Parameters.ItemPrefab.transform.localScale = Vector2.one;
            MessageScrollAdapter.Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
            MessageScrollAdapter.Parameters.ItemPrefab.gameObject.SetActive(false);
        }

        MessageScrollAdapter.Init();
        //
        #endregion
    }

    public void RefreshMailList(bool _newAlarm)
    {
        ZWebManager.Instance.WebGame.REQ_GetMailList(delegate
        {
            ZWebManager.Instance.WebGame.REQ_GetMessageList(delegate
            {
                ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMailboxMailListItem), objMail=>
                {
                    ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMailboxMessageListItem), objMessage=>
                    {
                        SetMailList(_newAlarm);

                        ZPoolManager.Instance.Return(objMail);
                        ZPoolManager.Instance.Return(objMessage);
                    });
                });
            });
        });
    }

    /// <summary> 메일 리스트 Setting</summary>
    /// <param name="_newAlarm">신규 메일 Alarm.</param>
    private void SetMailList(bool _newAlarm)
    {
        #region OSA Scroll View Value Setting
        ScrollAdapter.SetScrollData();
        MessageScrollAdapter.SetScrollData();
        #endregion

        MailCountCheck();
        ShowWindow((int)CurMailboxWindow);
    }

    public void RemoveMail(ulong _mailIdx)
    {
        for (int i = 0; i < ScrollAdapter.Data.Count; i++)
            if (ScrollAdapter.Data[i].MailData.MailIdx == _mailIdx)
            {
                ScrollAdapter.Data.RemoveOne(i);
                RecvMailCnt.text = Me.CurCharData.MailList.Count.ToString();
                MailCountCheck();
                return;
            }
    }

    public void ShowWindow(int _idx)
    {
        CurMailboxWindow = (E_MailboxWindow)_idx;
        BgText.text = string.Empty;
        WriteTab[(int)WebNet.E_MessageType.Normal].SelectToggle();
        switch ((E_MailboxWindow)_idx)
        {
            case E_MailboxWindow.Mail:
                BgText.text = Me.CurCharData.MailList.Count > 0 ? string.Empty : DBLocale.GetText("Not_Mail_Message");
                MailWindow.SetActive(true);
                MessageWindow.SetActive(false);
                MessageWriteWindow.SetActive(false);
                PaperInsideWindow.SetActive(false);
                break;
            case E_MailboxWindow.Message:
                BgText.text = Me.CurCharData.MessageList.Count > 0 ? string.Empty : DBLocale.GetText("Not_Note_Message");
                MailWindow.SetActive(false);
                MessageWindow.SetActive(true);
                MessageWriteWindow.SetActive(false);
                PaperInsideWindow.SetActive(false);                
                break;
            case E_MailboxWindow.MessageInside:
                ShowMessageInside();
                break;
        }
    }

    public void RecvAllMail()
    {
        List<MailData> dataCharList = new List<MailData>();
        List<MailData> dataAccList = new List<MailData>();

        for (int i = 0; i < ScrollAdapter.Data.Count; i++)
            if(ScrollAdapter.Data[i].MailData.mailType == GameDB.E_MailReceiver.Character)
                dataCharList.Add(ScrollAdapter.Data[i].MailData);
            else if(ScrollAdapter.Data[i].MailData.mailType == GameDB.E_MailReceiver.Account)
                dataAccList.Add(ScrollAdapter.Data[i].MailData);

        if (ScrollAdapter.Data.Count > 0)
        {
            if (dataCharList.Count > 0)
                ZWebManager.Instance.WebGame.REQ_TakeMailItems(GameDB.E_MailReceiver.Character, dataCharList, (recvPacket, msgCharacter) =>
                {
                    ReceiveEffect(msgCharacter);
                });

            if(dataAccList.Count > 0)
                ZWebManager.Instance.WebGame.REQ_TakeMailItems(GameDB.E_MailReceiver.Account, dataAccList, (recvPacket, msgAccount) =>
                {
                    ReceiveEffect(msgAccount);
                });

            RemoveAllMails();

            void ReceiveEffect(WebNet.ResTakeMailItem _msg)
            {
                List<GainInfo> gainItemList = new List<GainInfo>();
                for (int i = 0; i < _msg.TakeMailInfosLength; i++)
                    gainItemList.Add(new GainInfo(_msg.TakeMailInfos(i).Value));

                UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
                {
                    frame.AddItem(gainItemList);
                });
            }
        }        
    }

    public void RemoveAllMails()
    {
        ScrollAdapter.Data.RemoveItemsFromEnd(ScrollAdapter.Data.Count);
        MailCountCheck();
    }

    public void Close()
    {
        RemoveInfoPopup();

        UIManager.Instance.Close<UIFrameMailbox>();
    }

    public void SetInfoPopup(UIPopupItemInfo _infoPopup)
    {
        if (InfoPopup != null)
        {
            if (InfoPopup.gameObject != null)
                Destroy(InfoPopup.gameObject);

            InfoPopup = null;
        }

        InfoPopup = _infoPopup;
    }

    public void RemoveInfoPopup()
    {
        if(InfoPopup != null)
        {
            Destroy(InfoPopup.gameObject);
            InfoPopup = null;
        }
    }

    public void MailCountCheck()
    {
        if (Me.CurCharData.MailList.Count != 0)
        {
            RecvMailIcon.SetActive(true);
            ObjRecvMailAllButton.SetActive(true);
        }
        else
        {
            RecvMailIcon.SetActive(false);
            ObjRecvMailAllButton.SetActive(false);
        }

        RecvMailCnt.text = Me.CurCharData.MailList.Count.ToString();

        if (Me.CurCharData.MessageList.Count != 0)
        {
            RecvMessageIcon.SetActive(true);
            ObjDeleteMessageAllButton.SetActive(true);
            RecvMessageCnt.text = Me.CurCharData.MessageList.Count.ToString();            
        }
        else
        {
            RecvMessageIcon.SetActive(false);
            ObjDeleteMessageAllButton.SetActive(false);
        }            

        RecvBottomMessageCnt.text = $"{Me.CurCharData.MessageList.Count}/100";

        ShowWindow((int)CurMailboxWindow);
    }

    #region MessageAction
    public void SendMailMessage()
    {
        if (ReceiverText.text == string.Empty || WriteTitleText.text == string.Empty || WriteMessageText.text == string.Empty)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                //"내용을 입력 해주세요."
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("Message_Message_Tooltip"),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if (Me.CurUserData.MaxLevel < DBConfig.Message_Use_Level)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                //메시지를 사용할수있는 레벨이 아닙니다.
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("CANNOT_USE_MESSAGE_LEVEL") + "\n필요 레벨 : " + DBConfig.Message_Use_Level.ToString(),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if (ZNet.Data.Me.GetCurrency(DBConfig.Gold_ID) < DBConfig.Message_Gold_Cost)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("골드가 부족해서 메시지를 보낼 수 없습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if (Me.CurCharData.IsBlockUser(ReceiverText.text))
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("차단한 유저에게 메시지를 보낼 수 없습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if(true == Me.CurCharData.Nickname.Equals(ReceiverText.text))
        {
            //나한테 메지시 전송요청함.
            //자신을 대상으로 할 수 없습니다
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("Chatting_Error_Whisper_NoMe"),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        ZWebManager.Instance.WebGame.REQ_SendMailMessage((ushort)WriteMessageType, ReceiverText.text, WriteTitleText.text, WriteMessageText.text, delegate
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("메시지를 전송했습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close();
                    UIManager.Instance.Find<UIFrameHUD>().RefreshCurrency();
                    MailCountCheck();
                    UpdateRemainCount();
                }
                });
            });
        });
    }

    public void BackMessageList()
    {
        MailWindow.SetActive(false);
        MessageWindow.SetActive(true);
        MessageWriteWindow.SetActive(false);
        PaperInsideWindow.SetActive(false);
    }

    public void ResendMessage()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("답장을 하시겠습니까?"),
            new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
            new Action[] { 
                delegate {
                    _popup.Close();
                    
                },
                delegate {
                    ResetWriteMessageInfo();
                    
                    ShowWindow(2);  // 메시지 쓰기 화면..
                    ReceiverText.text = SenderText.text;
                    //PaperInsideRadioBtn.DoRadioButtonToggleOn();
                    _popup.Close();
                }
            });
        });
    }

    public void DeleteMessage()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("메시지를 삭제 하시겠습니까?"),
            new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
            new Action[] {
                delegate {
                    _popup.Close();
                    
                },
                delegate {
                    ZWebManager.Instance.WebGame.REQ_DeleteMessage(MessageIdx, delegate
                    {
                        for (int i = 0; i < MessageScrollAdapter.Data.Count; i++)
                        {
                            if (MessageScrollAdapter.Data[i].messageData.MessageIdx == MessageIdx)
                            {
                                MessageScrollAdapter.Data.RemoveOne(i);                                
                                BackMessageList();
                                MailCountCheck();
                                return;
                            }
                        }
                    });
                    _popup.Close();
                }
            });
        });
    }

    public void ClickDeleteMessageAll()
    {
        if (0 >= Me.CurCharData.MessageList.Count)
            return;

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("모든 메시지를 삭제 하시겠습니까?"),
            new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
            new Action[] {
                delegate {
                    _popup.Close();

                },
                delegate {
                    ZWebManager.Instance.WebGame.REQ_DeleteMessages(Me.CurCharData.MessageList, delegate
                    {
                        MessageScrollAdapter.Data.RemoveItemsFromEnd(MessageScrollAdapter.Data.Count);                        
                        MailCountCheck();
                    });
                    _popup.Close();
                }
            });
        });
    }

    public void BlockClick()
    {
        if (Me.CurCharData.Nickname == SenderText.text)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("자신을 차단할 수 없습니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }
        else if (Me.CurCharData.IsBlockUser(SenderText.text))
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("차단한 유저입니다."),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
        }
        else
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.LOCALE_OK_BUTTON, DBLocale.GetText("차단 하시겠습니까?"),
                new string[] { ZUIString.LOCALE_CANCEL_BUTTON, ZUIString.LOCALE_OK_BUTTON },
                new Action[] { 
                    delegate {
                        _popup.Close();
                        
                    },
                    delegate {
                        ZWebManager.Instance.WebGame.REQ_AddBlockCharacter(SenderText.text, delegate
                        {

                        });
                        _popup.Close();
                    }
                });
            });
            return;
        }
    }
    #endregion

    #region Write
    public void ShowMessageInside()
    {
        MailWindow.SetActive(false);
        MessageWindow.SetActive(false);
        MessageWriteWindow.SetActive(true);
        PaperInsideWindow.SetActive(false);

        ResetWriteMessageInfo();

        MessageWriteType(0);
    }

    public void MessageWriteType(int _idx)
    {
        switch (_idx)
        {
            case (int)WebNet.E_MessageType.Normal:
                WriteMessageType = WebNet.E_MessageType.Normal;
                ReceiverTitleText.text = DBLocale.GetText("Message_Receiver");
                break;

            case (int)WebNet.E_MessageType.Guild:
                WriteMessageType = WebNet.E_MessageType.Guild;
                ReceiverTitleText.text = DBLocale.GetText("Message_Receiver_Guild");
                break;
        }

        ReceiverText.characterLimit = DBConfig.NickName_Length_Max;
        WriteTitleText.characterLimit = DBConfig.Message_Title_Text_Max;
        WriteMessageText.characterLimit = DBConfig.Message_Content_Text_Max;

        UpdateWirteMessageGold();
        UpdateRemainCount();
        UpdateTitleInputText();
        UpdateMessageInputText();
    }

    public void UpdateTitleInputText()
    {
        WriteTitleText.characterLimit = DBConfig.Message_Title_Text_Max;
        TitleInputText.text = string.Format("{0}/{1}", WriteTitleText.text.Length, WriteTitleText.characterLimit);
    }

    public void UpdateMessageInputText()
    {
        WriteMessageText.characterLimit = DBConfig.Message_Content_Text_Max;
        MessageInputText.text = string.Format("{0}/{1}", WriteMessageText.text.Length, WriteMessageText.characterLimit);
    }

    private void UpdateWirteMessageGold()
    {
        switch (WriteMessageType)
        {
            case WebNet.E_MessageType.Normal:
                WriteMessageGoldCnt = DBConfig.Message_Gold_Cost;
                break;
            case WebNet.E_MessageType.Guild:
                WriteMessageGoldCnt = DBConfig.Guild_Message_Gold_Cost;
                break;
            case WebNet.E_MessageType.Exchange:
                WriteMessageGoldCnt = DBConfig.Message_Gold_Cost;
                break;
        }
        WriteMessageGoldCntText.text = string.Format("수수료 : {0}", WriteMessageGoldCnt);
    }
    
    private void UpdateRemainCount()
    {
        uint normalCnt = Me.CurUserData.GetNormalMsgSendCnt();
        uint guildCnt = Me.CurUserData.GetGuildMsgSendCnt();
        switch (WriteMessageType)
        {
            case WebNet.E_MessageType.Normal:
            case WebNet.E_MessageType.Exchange:
                RemainText.text = string.Format("{0}", DBConfig.Day_Message_Send_Limit - normalCnt);
                break;
            case WebNet.E_MessageType.Guild:
                RemainText.text = string.Format("{0}", DBConfig.Day_GuildMessage_Send_Limit - guildCnt);
                break;
        }
    }

    private void ResetWriteMessageInfo()
    {
        ReceiverText.text = "";
        WriteTitleText.text = "";
        WriteMessageText.text = "";

        UpdateWirteMessageGold();
    }
    #endregion

    #region Read
    public void OpenMessage(string _remainTime, string _paperType, string _titleText, string _sender, string _messageText, ulong _messageIdx)
    {
        ZWebManager.Instance.WebGame.REQ_ReadMessage(_messageIdx, delegate
        {
            MessageScrollAdapter.RefreshData();
        });
        MailWindow.SetActive(false);
        MessageWindow.SetActive(false);
        MessageWriteWindow.SetActive(false);
        PaperInsideWindow.SetActive(true);

        RemainTime.text = _remainTime;
        PaperType.text = _paperType;
        ReadTitleText.text = _titleText;
        SenderText.text = _sender;
        ReadMessageText.text = _messageText;
        MessageIdx = _messageIdx;
    }
    #endregion

    public void ClickSearch()
    {
        if(string.IsNullOrEmpty(ReceiverText.text))
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                //닉네임을 다시 확인 해 주세요.
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("AddFriendAlret_FindFail"),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        if (true == Me.CurCharData.Nickname.Equals(ReceiverText.text))
        {
            //나한테 메지시 전송요청함.
            //자신을 대상으로 할 수 없습니다
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(ZUIString.WARRING, DBLocale.GetText("Chatting_Error_Whisper_NoMe"),
                new string[] { ZUIString.LOCALE_OK_BUTTON },
                new Action[] { delegate { _popup.Close(); } });
            });
            return;
        }

        ZWebManager.Instance.WebGame.REQ_FindFriend(ReceiverText.text, (packet, res) =>
        {

        });
    }
}