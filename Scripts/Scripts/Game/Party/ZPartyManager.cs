using System;
using System.Collections.Generic;
using ZNet;
using ZNet.Data;
using WebNet;
using MmoNet;

/// <summary> 파티 관리 </summary>
public class ZPartyManager : Zero.Singleton<ZPartyManager>
{
    /// <summary> 파티 정보 업데이트 </summary>
    private Action<bool, ulong> mEventUpdatePartyInfo;
    /// <summary> 파티 맴버 추가 </summary>
    private Action<ZPartyMember> mEventAddMember;
    /// <summary> 파티 맴버 제거 </summary>
    private Action<ulong> mEventRemoveMember;
    /// <summary> 리더 변경 </summary>
    private Action<ZPartyMember> mEventChangeMaster;
    /// <summary> 파티 타겟 변경 </summary>
    private Action<uint> mEventChangePartyTargetEntityId;

    /// <summary> 파티 초대 받음 </summary>
    private Action<uint, ZPartyMember> mEventPartyInvite;
    /// <summary> 파티 초대 거절 </summary>
    private Action<uint, string> mEventPartyRefuse;

    /// <summary> 파티의 일부 정보가 업데이트 되었을 때 호출 (걍 이걸로 다 갱신하면 될듯) </summary>
    private Action mEventUpdateParty;

    /// <summary> Mmo party 업데이트 </summary>
    private Action<S2C_UpdatePartyInfo> mEventUpdatePartyInfoByMmo;

    /// <summary> characterId 기준으로 멤버 데이터 저장 </summary>
    public Dictionary<ulong, ZPartyMember> m_dicMember { get; private set; } = new Dictionary<ulong, ZPartyMember>();

    /// <summary> 현재 파티중인지 여부 </summary>
    public bool IsParty { get; private set; }
    
    /// <summary> 파티 식별 ID </summary>
    public uint PartyUid { get; private set; }

    /// <summary> 유저 id </summary>
    public ulong UserId { get { return Me.UserID; } }
    /// <summary> 현재 캐릭터 id </summary>
    public ulong CharacterId { get { return Me.CharID; } }
    /// <summary> 현재 캐릭터 Tid </summary>
    public uint CharacterTid { get { return Me.CurCharData.TID; } }
    /// <summary> 현재 캐릭터 닉네임 </summary>
    public string CharacterName { get { return Me.CurCharData.Nickname; } }
    /// <summary> 현재 서버 인덱스 </summary>
    public uint ServerIdx { get { return Me.SelectedServerID; } }

    /// <summary> 파티장 유저 id  </summary>
    public ulong MasterUserId { get; private set; }
    /// <summary> 파티장 캐릭터 id </summary>
    public ulong MasterCharacterId { get; private set; }
    /// <summary> 파티장 캐릭터 tid </summary>
    public ulong MasterCharacterTid { get; private set; }

    /// <summary> 내가 마스터인지 여부 </summary>
    public bool IsMaster { get { return MasterCharacterId == CharacterId; } }

    /// <summary> 파티 타겟 id </summary>
    public uint PartyTargetEntityId { get; private set; }

    private bool UpdatePartyDirty = false;

    protected override void Init()
    {
        base.Init();
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateDirty);
    }

    private void UpdateDirty()
    {
        if(true == UpdatePartyDirty)
        {
            mEventUpdateParty?.Invoke();
            UpdatePartyDirty = false;
        }
    }

    /// <summary> 파티 정보 업데이트 </summary>
    public void UpdatePartyInfo(bool bParty, uint partyUid)
    {
        IsParty = bParty;
        PartyUid = partyUid;
        mEventUpdatePartyInfo?.Invoke(IsParty, PartyUid);
        UpdatePartyDirty = true;
    }

    /// <summary> 멤버 추가 </summary>
    private void AddMember(PartyMemberDetail info)
    {
        if(info.IsMaster)
        {
            ChangeMaster(info.CharId, info.CharTid);
        }

        if(m_dicMember.TryGetValue(info.CharId, out var member))
        {
            member.Reset(info);
        }
        else
        {
            member = new ZPartyMember(info);
            m_dicMember.Add(info.CharId, member);
        }

        if (info.CharId == CharacterId)
        {
            member.SetEntityId(ZPawnManager.Instance.MyEntityId);
        }   

        mEventAddMember?.Invoke(member);
        UpdatePartyDirty = true;
    }

    /// <summary> 해당 멤버 제거 </summary>
    private void RemoveMember(ulong characterId)
    {
        if (m_dicMember.ContainsKey(characterId))
        {
            m_dicMember.Remove(characterId);
        }

        mEventRemoveMember?.Invoke(characterId);
        UpdatePartyDirty = true;
    }

    /// <summary> 모든 멤버 제거 </summary>
    private void ClearMember()
    {
        m_dicMember.Clear();
        UpdatePartyDirty = true;
    }

    /// <summary> 파티 나가기 </summary>
    private void PartyOut()
    {
        UpdatePartyInfo(false, 0);
        ClearMember();
        SetPartyTarget(0);
        UpdatePartyDirty = true;
    }

    /// <summary> 파티장 변경 </summary>
    private void ChangeMaster(ulong targetCharacterId, uint targetCharacterTid)
    {
        MasterCharacterId = targetCharacterId;
        MasterCharacterTid = targetCharacterTid;

        ZPartyMember master = null;

        foreach (var member in m_dicMember)
        {
            member.Value.ChangeMaster(MasterCharacterId);
            if(member.Value.IsMaster)
                master = member.Value;
        }

        if(null != master)
        {
            mEventChangeMaster?.Invoke(master);
        }
        UpdatePartyDirty = true;
    }

    private void SetPartyTarget(uint targetId)
    {
        PartyTargetEntityId = targetId;
        mEventChangePartyTargetEntityId?.Invoke(targetId);
    }

    /// <summary> 캐릭터 id로 해당 맴버 얻어옴 </summary>
    public bool TryGet(ulong characterId, out ZPartyMember member)
    {
        return m_dicMember.TryGetValue(characterId, out member);
    }

    #region ===== :: Request :: =====
    private bool IsPartyChecking = false;
    /// <summary> 파티 가입 여부를 체크하고 멤버 정보를 셋팅한다. </summary>
    public void Req_CheckRefreshParty(Action onFinish)
    {
        //이미 파티 체크중임
        if (true == IsPartyChecking)
            return;

        IsPartyChecking = true;

        REQ_CheckParty((pachet, res) =>
        {
            if(res.IsParty)
            {
                Req_PartyMemberInfo(delegate
                {
                    IsPartyChecking = false;
                    onFinish?.Invoke();
                }, delegate
                {
                    IsPartyChecking = false;
                });
            }
            else
            {
                IsPartyChecking = false;
                onFinish?.Invoke();
            }
        }, delegate
        {
            IsPartyChecking = false;
        });
    }

    /// <summary> 파티 여부 체크 </summary>
    public void REQ_CheckParty(Action<ZWebRecvPacket, ResPartyCheck> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {        
        ZWebManager.Instance.WebGame.Req_CheckParty(ServerIdx, CharacterId, (packet, res) =>
        {            
            UpdatePartyInfo(res.IsParty, res.PartyUid);            
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 맴버 정보를 얻어온다. </summary>
    public void Req_PartyMemberInfo(Action<ZWebRecvPacket, ResPartyMemberInfo> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        if(0 == PartyUid)
        {
            //party uid 가 셋팅되지 않았는다. 멤버 정보를 호출함.
            ZLog.LogError(ZLogChannel.Party, "PartyUid가 셋팅되지 않았다.");
            return;
        }

        ZWebManager.Instance.WebGame.Req_PartyMemberInfo(ServerIdx, CharacterId, PartyUid, (packet, res) =>
        {
            //파티 맴버 정보 처리
            for(int i = 0; i < res.MemberInfoLength; ++i)
            {
                AddMember(res.MemberInfo(i).Value);
            }            

            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 생성 </summary>
    public void Req_PartyCreate(Action<ZWebRecvPacket, ResPartyCreate> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyCreate(ServerIdx, CharacterId, CharacterTid, CharacterName, (packet, res) =>
        {
            UpdatePartyInfo(true, res.PartyUid);
            AddMember(res.PartyMember.Value);            
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 초대 </summary>
    public void Req_InviteParty(uint targetServerIdx, ulong targetcharacterId, string targetNickname, Action<ZWebRecvPacket, ResPartyInvite> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_InviteParty(ServerIdx, CharacterId, CharacterTid, CharacterName, targetServerIdx, targetcharacterId, targetNickname, (packet, res) =>
        {
            onRecvPacket?.Invoke(packet, res);            
        }, onError);
    }

    /// <summary> 파티 초대 (닉네임으로) </summary>
    public void Req_InviteParty(string nickname, Action<ZWebRecvPacket, ResPartyInvite> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        if(string.IsNullOrEmpty(nickname))
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("AddAlret_FindFail"));
            return;
        }

        Req_FindUser(nickname, (packet, res) =>
        {
            Req_InviteParty(ServerIdx, res.FindCharId, res.FindNick, onRecvPacket, onError);
        }, onError);
    }

    /// <summary> 파티 가입 </summary>
    public void Req_PartyJoin(uint partyUid, Action<ZWebRecvPacket, ResPartyJoin> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyJoin(ServerIdx, CharacterId, CharacterTid, CharacterName, partyUid,(packet, res) =>
        {
            UpdatePartyInfo(res.IsJoin, res.PartyUid);

            //파티 맴버 정보 처리
            for (int i = 0; i < res.PartyMembersLength; ++i)
            {
                AddMember(res.PartyMembers(i).Value);
            }            
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 초대 거절 </summary>
    public void Req_PartyRefuse(uint partyUid, ulong masterCharacterId, uint masterServerIdx, Action<ZWebRecvPacket, ResPartyRefuse> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyRefuse(ServerIdx, CharacterId, CharacterName, partyUid, masterServerIdx, masterCharacterId, (packet, res) =>
        {
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 나가기 </summary>
    public void Req_PartyOut(Action<ZWebRecvPacket, ResPartyOut> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyOut(ServerIdx, CharacterId, CharacterTid, CharacterName, PartyUid, (packet, res) =>
        {            
            PartyOut();
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 강퇴 </summary>
    public void Req_PartyKickOut(uint kickuserServerIdx, ulong kickuserCharId, uint kickuserCharTid, string kickuserNick, Action<ZWebRecvPacket, ResPartyKickOut> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyKickOut(ServerIdx, CharacterId, kickuserServerIdx, kickuserCharId, kickuserCharTid, kickuserNick, (packet, res) =>
        {
            RemoveMember(kickuserCharId);
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티장 변경 </summary>
    public void Req_PartyChangeMaster(uint targetServerId, ulong targetCharacterId, uint targetCharacterTid, string targetNickname, Action<ZWebRecvPacket, ResPartyChangeMaster> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        ZWebManager.Instance.WebGame.Req_PartyChangeMaster(ServerIdx, UserId, CharacterId, targetServerId, targetCharacterId, targetCharacterTid, targetNickname, (packet, res) =>
        {
            ChangeMaster(targetCharacterId, targetCharacterTid);
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }

    /// <summary> 파티 타겟 변경 </summary>
    public void Req_PartyTarget(uint targetEntityId)
    {
        //이미 타겟이다.
        if (targetEntityId == PartyTargetEntityId)
            return;

        ZMmoManager.Instance.Field.REQ_SetPartyTarget(targetEntityId);
    }

    /// <summary> 닉네임으로 유저 찾기 </summary>
    public void Req_FindUser(string nickname, Action<ZWebRecvPacket, ResFindFriend> onRecvPacket = null, PacketErrorCBDelegate onError = null)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("AddAlret_FindFail"));
            return;
        }

        ZWebManager.Instance.WebGame.REQ_FindFriend(nickname, (packet, res) =>
        {
            onRecvPacket?.Invoke(packet, res);
        }, onError);
    }
    #endregion
        
    #region ===== :: by Broadcast :: =====
    /// <summary> MMO Party 정보 업데이트 </summary>
    public void RecvBroadcastMessageByMMo(S2C_UpdatePartyInfo info)
    {         
        foreach (var member in m_dicMember)
        {
            uint  entityId = 0;
            for (int i = 0; i < info.PartymembersLength; ++i)
            {
                var memberInfo = info.Partymembers(i).Value;

                if (member.Value.CharacterId != memberInfo.CharId)
                    continue;

                entityId = memberInfo.Objectid;

                break;
            }
            if (member.Value.CharacterId == CharacterId)
                entityId = ZPawnManager.Instance.MyEntityId;

            member.Value.SetEntityId(entityId);
        }

        SetPartyTarget(info.TargetobjId);
        mEventUpdatePartyInfoByMmo?.Invoke(info);        
    }

    public void RecvBroadcastMessage(ZWebRecvPacket _recvPacket)
    {
        switch (_recvPacket.ID)
        {            
            case Code.BROADCAST_PARTY_INVITE:               //파티 초대 
                {
                    OnBCPartyInvite(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_JOIN:                 //파티원 입장
                {
                    OnBCPartyJoin(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_REFUSE:               //파티 초대 거절
                {
                    OnBCPartyRefuse(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_OUT:                  //파티 나가기
                {
                    OnBCPartyOut(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_KICK_OUT:             //파티 강퇴
                {
                    OnBCPartyKickOut(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_CHANGE_MASTER:        //파티장 변경
                {
                    OnBCPartyChangeMaster(_recvPacket);
                }
                break;
            case Code.BROADCAST_PARTY_CHAT:                 //파티 채팅
                {
                    OnBCPartyChat(_recvPacket);
                }
                break;
            case Code.BROADCAST_INTER_PARTY_CHAT:           //인터 서버 파티 채팅
                {
                    OnBCPartyInterChat(_recvPacket);
                }
                break;
        }
    }

    /// <summary> 파티 초대 받음 </summary>
    private void OnBCPartyInvite(ZWebRecvPacket _recvPacket)
    {
        if(!ZGameOption.Instance.bAllowPartyInvite)
        {
            return;
        }

        BroadcastPartyInvite info = _recvPacket.Get<BroadcastPartyInvite>();
        mEventPartyInvite?.Invoke(info.PartyUid, new ZPartyMember(info.Sender.Value));
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티원 입장 </summary>
    private void OnBCPartyJoin(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyJoin info = _recvPacket.Get<BroadcastPartyJoin>();
        AddMember(info.JoinMember.Value);
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티 초대 거절 </summary>
    private void OnBCPartyRefuse(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyRefuse info = _recvPacket.Get<BroadcastPartyRefuse>();        
        mEventPartyRefuse?.Invoke(info.RefuseCharServerIdx, info.RefuseCharNick);
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티 나가기 </summary>
    private void OnBCPartyOut(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyOut info = _recvPacket.Get<BroadcastPartyOut>();
        var outMember = info.OutMember.Value;        
        
        //나간 멤버가 나라면 파티 해산
        if (outMember.CharId == CharacterId)
        {
            PartyOut();
        }
        else
        {
            RemoveMember(outMember.CharId);
            if (true == info.NewMaster.HasValue)
            {
                var newMaster = info.NewMaster.Value;
                //마스터 변경
                ChangeMaster(newMaster.CharId, newMaster.CharTid);
            }
        }
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티 강퇴 </summary>
    private void OnBCPartyKickOut(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyKickOut info = _recvPacket.Get<BroadcastPartyKickOut>();
        var kickMember = info.KickMember.Value; 
        //강퇴된 멤버가 나라면 파티 해산
        if (kickMember.CharId == CharacterId)
        {
            PartyOut();
        }
        else
        {
            RemoveMember(kickMember.CharId);
        }
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티장 변경 </summary>
    private void OnBCPartyChangeMaster(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyChangeMaster info = _recvPacket.Get<BroadcastPartyChangeMaster>();
        var master = info.NewMasterMember.Value;
        ChangeMaster(master.CharId, master.CharTid);
        ZWebChatData.AddChatMsg(info);
    }

    /// <summary> 파티 채팅 </summary>
    private void OnBCPartyChat(ZWebRecvPacket _recvPacket)
    {
        BroadcastPartyChat info = _recvPacket.Get<BroadcastPartyChat>();
    }

    /// <summary> 인터 서버 파티 채팅 </summary>
    private void OnBCPartyInterChat(ZWebRecvPacket _recvPacket)
    {
        BroadcastInterPartyChat info = _recvPacket.Get<BroadcastInterPartyChat>();
    }

    #endregion

    #region ===== :: Event :: ===== 

    /// <summary> 파티 일부 정보가 업데이트되었을 때 </summary>    
    public void DoAddEventUpdateParty(Action action)
    {
        DoRemoveEventUpdateParty(action);
        mEventUpdateParty += action;
    }

    public void DoRemoveEventUpdateParty(Action action)
    {
        mEventUpdateParty -= action;
    }
    
    /// <summary> 파티 초대 받았을 때 </summary>
    public void DoAddEventPartyInvite(Action<uint, ZPartyMember> action)
    {
        DoRemoveEventPartyInvite(action);
        mEventPartyInvite += action;
    }

    public void DoRemoveEventPartyInvite(Action<uint, ZPartyMember> action)
    {
        mEventPartyInvite -= action;
    }

    /// <summary> mmo에서 파티 정보가 갱신되었을 때 </summary>
    public void DoAddEventUpdatePartyInfoByMmo(Action<S2C_UpdatePartyInfo> action)
    {
        DoRemoveEventUpdatePartyInfoByMmo(action);
        mEventUpdatePartyInfoByMmo += action;
    }

    public void DoRemoveEventUpdatePartyInfoByMmo(Action<S2C_UpdatePartyInfo> action)
    {
        mEventUpdatePartyInfoByMmo -= action;
    }

    /// <summary> 파티 타겟이 변경되었을 경우 호출 (처음 등록시 파티 타겟이 있다면 바로 알림) </summary>
    public void DoAddEventChangePartyTargetEntityId(Action<uint> action)
    {
        if(0 < PartyTargetEntityId)
        {
            action?.Invoke(PartyTargetEntityId);
        }
        DoRemoveEventChangePartyTargetEntityId(action);
        mEventChangePartyTargetEntityId += action;
    }

    public void DoRemoveEventChangePartyTargetEntityId(Action<uint> action)
    {
        mEventChangePartyTargetEntityId -= action;
    }
    #endregion
}
