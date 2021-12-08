using System;
using UnityEngine;
using WebNet;
using ZNet.Data;
using static UIFrameGuildNetCapturer;

public class UIFrameGuildTab_Members : UIFrameGuildTabBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildMemberListAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
    private bool ignoreClickEvents;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        base.Initialize(guildFrame, type);

        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollGuildMemberListSlot));
        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);

        ScrollAdapter.AddListener_OnClick(OnMemberSlotClicked);
        ScrollAdapter.AddListener_OnClickBan(OnMemberSlotClicked_Ban);
        ScrollAdapter.AddListener_OnClickAddFriend(OnMemberSlotClicked_AddFriend);
        ScrollAdapter.AddListener_OnClickDegradeViceMaster(OnMemberSlotClicked_DegradeViceMaster);
        ScrollAdapter.AddListener_OnClickViceMaster(OnMemberSlotClicked_ViceMaster);
        ScrollAdapter.AddListener_OnClickMaster(OnMemberSlotClicked_Master);
        ScrollAdapter.Initialize();
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        switch (type)
        {
            case UpdateEventType.DataAllRefreshed:
            case UpdateEventType.DataRefreshed_Members:
                {
                    ResetScroll();
                }
                break;
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        ignoreClickEvents = false;
        ResetScroll();
    }
    #endregion

    #region Private Methods
    private void ResetScroll(bool resetPos = true)
    {
        ScrollAdapter.SetSelectedMember(0);
        ScrollAdapter.RefreshMemberList();

        if (resetPos)
            ScrollAdapter.SetNormalizedPosition(1d);
    }

    private void OnMemberSlotClicked(GuildMemberInfoConverted data)
    {
        if (data.charID == Me.CurCharData.ID)
        {
            UIMessagePopup.ShowInputPopup("남김말", "남김말을 적어주세요.", (comment) =>
            {
                UIFrameGuildNetCapturer.ReqUpdateGuildMemberComment(Me.CurCharData.GuildId, comment
                    , (revPacketRec, resListRec) =>
                    {
                        if (gameObject != null && gameObject.activeSelf)
                        {
                            ResetScroll(false);
                        }
                    }, null);
            });
        }
        else
        {
            ScrollAdapter.SetSelectedMember(data.charID);
            ScrollAdapter.ApplySlots();
        }
    }

    private void ReqAppoint(GuildMemberInfoConverted target, string recheckQueryComment, E_GuildMemberGrade grade)
    {
        OpenTwoButtonQueryPopUp("확인", recheckQueryComment,
            onConfirmed: () =>
            {
                UIFrameGuildNetCapturer.ReqAppointGuildMember(
                    Me.CurCharData.GuildId
                    , target.charID
                    , (uint)grade
                    , (revPacketReq, resListReq) =>
                    {
                        ignoreClickEvents = false;
                        NotifyUpdateEvent(UpdateEventType.DataRefreshed_Members);
                    },
                    (error, req, res) =>
                    {
                        ignoreClickEvents = false;
                        HandleError(error, req, res);
                    });
            },
            onCanceled: () =>
            {
                ignoreClickEvents = false;
            });
    }

    // 슬롯안에 버튼들 클릭 핸들링 함수들 

    private void OnMemberSlotClicked_Master(GuildMemberInfoConverted obj)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;
        ReqAppoint(obj
            , string.Format("{0}님을 길드장으로 위임하시겠습니까?", obj.nick)
            , E_GuildMemberGrade.Master);
    }

    private void OnMemberSlotClicked_ViceMaster(GuildMemberInfoConverted obj)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

        if (UIFrameGuildNetCapturer.SubMasterCount >= DBConfig.Guild_SubMaster_Count)
        {
            OpenNotiUp("부길드장이 최대 인원에 도달하였습니다.", "확인");
            return;
        }

        ReqAppoint(obj
            , string.Format("{0}님을 부길드장으로 위임하시겠습니까?", obj.nick)
            , E_GuildMemberGrade.SubMaster);
    }

    private void OnMemberSlotClicked_DegradeViceMaster(GuildMemberInfoConverted obj)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;
        ReqAppoint(obj
            , string.Format("{0}님을 부길드장에서 해임하시겠습니까?", obj.nick)
            , E_GuildMemberGrade.Normal);
    }

    private void OnMemberSlotClicked_AddFriend(GuildMemberInfoConverted obj)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

        ZWebManager.Instance.WebGame.REQ_AddFriend(obj.charID, (friendRecvPacket, friendRecvMsgPacket) =>
        {
            ignoreClickEvents = false;
            OpenNotiUp(string.Format(DBLocale.GetText("AddFriendAlret_Success"), obj.nick), "알림");
        },
        (err, req, res) =>
        {
            ignoreClickEvents = false;
            HandleError(err, req, res);
        });
    }

    private void OnMemberSlotClicked_Ban(GuildMemberInfoConverted obj)
    {
        if (ignoreClickEvents)
            return;

        ignoreClickEvents = true;

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            // TODO : Locale
            _popup.Open("확인", obj.nick + "님을 정말 추방 하시겠습니까?", new string[] { "취소", "확인" },
                new Action[] {
                            () =>
                            {
                                  ignoreClickEvents=false;
                            }
                            ,
                            () =>
                            {
                                // 내길드원이니까 내 길드 아이디 넣어줌 
                                UIFrameGuildNetCapturer.ReqGuildMemberBan(
                                    Me.CurCharData.GuildId
                                    , obj.charID
                                    , (revPacketReq, resListReq) =>
                                    {
                                        ignoreClickEvents=false;
                                        _popup.Close();
                                        NotifyUpdateEvent(UpdateEventType.DataRefreshed_Members);
                                    },
                                    (err,req,res)=>
                                    {
                                        ignoreClickEvents=false;
                                        _popup.Close();
                                        HandleError(err,req,res);
                                    });


                                _popup.Close();
                            } });
        });
    }

    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
