using FlatBuffers;
using System;
using System.Collections.Generic;
using WebNet;
using ZDefine;
using ZNet.Data;

namespace ZNet
{
	public class ZWebChat : ZWebClientBase
    {
        public override bool IsUsable => base.IsUsable;

        public override E_WebSocketType SocketType => E_WebSocketType.Chat;

        public override uint SecurityKey { get; protected set; }

        public bool IsUserConnected { get; private set; } = false;// 유저가 실제로 접속됨

        public override void Connect(string _serverUrl)
        {
            ZPartyManager.Instance.DoAddEventUpdateParty(OnUpdateParty);

            base.Connect(_serverUrl);
        }

        public override void Disconnect()
        {
            ZPartyManager.Instance.DoRemoveEventUpdateParty(OnUpdateParty);

            ZWebChatData.ClearChatList();

            base.Disconnect();
        }

        private void OnUpdateParty()
        {
            ChatEnterData enterInfo = ZWebChatData.GetEnterInfo(E_ChatType.Party);
            bool isParty = ZPartyManager.Instance.IsParty;
            List<ChatEnterData> enterList = new List<ChatEnterData>();

            if (enterInfo == null && isParty)
            {
                enterList.Add(new ChatEnterData(WebNet.E_ChatType.Party, ZPartyManager.Instance.PartyUid.ToString()));
                REQ_EnterChat(Me.CharID, enterList, null);
            }
            else if (enterInfo != null && isParty == false)
            {
                enterList.Add(enterInfo);
                REQ_ExitChannel(Me.CharID, enterList, null);
            }
        }

        protected override void OnSocket_ConnectOpened(ZWebSocket socket)
        {
            if (Me.CharID > 0)
            {
                ConnectInitialize(Me.SelectedServerID, Me.UserID, Me.CharID, () =>
                {
                    CheckEnterChannel();
                });
            }

            base.OnSocket_ConnectOpened(socket);
        }

        protected override void OnSocket_ConnectClosed(ZWebSocket socket)
        {
            IsUserConnected = false;

            base.OnSocket_ConnectClosed(socket);
        }

        protected override void OnSocket_Error(ZWebSocket socket, string reason)
        {
            base.OnSocket_Error(socket, reason);
        }

        public override void SendPacket(ZWebReqPacketBase _reqPacket, ReceiveCBDelegate _onReceive, PacketErrorCBDelegate _packetErrCB)
        {
            if (!IsUsable)
            {
                ZLog.Log(ZLogChannel.ChatServer, ZLogLevel.Warning, $">>>> [{SocketType}] SendPacket() | 사용불가능 상태에서 호출됨. ID: {_reqPacket.ID}");
                return;
            }

            mCommunicator?.SendPacket(this.SocketType, _reqPacket, _onReceive, _packetErrCB);
        }

        public void ConnectInitialize(uint serverID, ulong userID, ulong charID, Action onInitialize)
        {
            REQ_Connect(serverID, userID, charID, (recvPacket, recvMsgPacket) =>
             {
                 RefreshConnect();

                 onInitialize?.Invoke();
             });
        }

        public static void RefreshConnect()
        {
            //  NetData.Instance.RemoveOnUpdateParty(NetData.UserID, NetData.CharID, UpdateParty);
            //  NetData.Instance.RemoveOnUpdateGuildInfo(NetData.UserID, NetData.CharID, UpdateGuild);
            //  NetData.Instance.RemoveOnUpdateAllianceChatInfo(NetData.UserID, NetData.CharID, UpdateGuildAlliance);
            //
            //  NetData.Instance.AddOnUpdateParty(NetData.UserID, NetData.CharID, UpdateParty);
            //  NetData.Instance.AddOnUpdateGuildInfo(NetData.UserID, NetData.CharID, UpdateGuild);
            //  NetData.Instance.AddOnUpdateAllianceChatInfo(NetData.UserID, NetData.CharID, UpdateGuildAlliance);
        }

        public void CheckEnterChannel()
        {
            ZWebChatData.ClearEnterInfos();
            // TODO : 갱신되는 놈만 보내게 수정
            List<ChatEnterData> enterList = new List<ChatEnterData>();

            ZLog.Log(ZLogChannel.ChatServer, ">>> Check Enter Channel");

            if (Me.CurCharData.LastChannelId != (ushort)WebNet.E_StageChannelType.Single/* && Me.CurCharData.LastChannelId != (ushort)WebNet.E_StageChannelType.Private*/)
            {
                if( ZGameModeManager.Instance.Table.StageType == GameDB.E_StageType.GuildDungeon)
                    enterList.Add(new ChatEnterData(WebNet.E_ChatType.Normal, Me.CurCharData.LastArea.ToString(), ZGameManager.Instance.GuildDungeonRoomNo.ToString()));
                else if (ZGameModeManager.Instance.Table.StageType == GameDB.E_StageType.InterField)
                    enterList.Add(new ChatEnterData(WebNet.E_ChatType.InterNormal, Me.CurCharData.LastArea.ToString(), Me.CurCharData.LastChannelId.ToString()));
                else
                    enterList.Add(new ChatEnterData(WebNet.E_ChatType.Normal, Me.CurCharData.LastArea.ToString(), Me.CurCharData.LastChannelId.ToString()));
            }

            // ljh 2020.09.01 파티추가되면 수정
            if (ZPartyManager.Instance.PartyUid > 0)
            {
                enterList.Add(new ZDefine.ChatEnterData(WebNet.E_ChatType.Party, ZPartyManager.Instance.PartyUid.ToString()));
            }

            if (Me.CurCharData.GuildId != 0)
            {
                enterList.Add(new ZDefine.ChatEnterData(WebNet.E_ChatType.Guild, Me.CurCharData.GuildId.ToString()));

                if (Me.CurCharData.GuildChatId != 0 && Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Enter)
                    enterList.Add(new ZDefine.ChatEnterData(WebNet.E_ChatType.Alliance, Me.CurCharData.GuildChatId.ToString()));
            }

            if (enterList.Count > 0)
                REQ_EnterChat(Me.CharID, enterList, null);
        }

        public void REQ_Connect(uint ServerId, ulong UserId, ulong CharId, Action<ZWebRecvPacket, ResChatUserConnect> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqChatUserConnect.CreateReqChatUserConnect(mBuilder, UserId, CharId, ServerId);
            var reqPacket = ZChatPacket.Create<ReqChatUserConnect>(this, Code.CT_USER_CONNECT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ZLog.Log(ZLogChannel.ChatServer, ">>> ChatServer Connected");

                ResChatUserConnect recvMsgPacket = recvPacket.Get<ResChatUserConnect>();

                IsUserConnected = true;

                onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
            }, _onError);
        }

        #region Enter
        public void ExitChannelAll(Action onRecv)
        {
            if (ZWebChatData.GetEnterList().Count > 0)
                REQ_ExitChannel(Me.CharID, ZWebChatData.GetEnterList(), delegate { onRecv?.Invoke(); });
        }

        public void REQ_EnterChat(ulong CharId, List<ChatEnterData> enterList, System.Action<ZWebRecvPacket, ResEnterChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var chatoffs = new Offset<ChatInfo>[enterList.Count];

            for (int i = 0; i < enterList.Count; i++)
            {
                var argoffs = new StringOffset[enterList[i].Args.Count];
                for (int j = 0; j < enterList[i].Args.Count; j++)
                {
                    argoffs[j] = mBuilder.CreateString(enterList[i].Args[j]);
                }

                chatoffs[i] = ChatInfo.CreateChatInfo(mBuilder, enterList[i].chatType, ChatInfo.CreateArgsVector(mBuilder, argoffs));
            }

            var offset = ReqEnterChat.CreateReqEnterChat(mBuilder, CharId, ReqEnterChat.CreateChatInfosVector(mBuilder, chatoffs));

            var reqPacket = ZChatPacket.Create<ReqEnterChat>(this, Code.CT_ENTER_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ZLog.Log(ZLogChannel.ChatServer, ">>> ChatServer Enter");

                ResEnterChat recvMsgPacket = recvPacket.Get<ResEnterChat>();

                for (int i = 0; i < recvMsgPacket.ChatInfosLength; i++)
                {
                    ZWebChatData.AddEnterInfo(recvMsgPacket.ChatInfos(i).Value);
                }

                onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
            }, _onError);
        }

        public void REQ_ExitChannel(ulong CharId, List<ChatEnterData> exitList, System.Action<ZWebRecvPacket, ResOutChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var chatOffset = new Offset<ChatInfo>[exitList.Count];
            for (int i = 0; i < exitList.Count; i++)
            {
                var argOffset = new StringOffset[exitList[i].Args.Count];
                for (int j = 0; j < exitList[i].Args.Count; j++)
                {
                    argOffset[j] = mBuilder.CreateString(exitList[i].Args[j]);
                }

                chatOffset[i] = ChatInfo.CreateChatInfo(mBuilder, exitList[i].chatType, ChatInfo.CreateArgsVector(mBuilder, argOffset));
            }

            var offset = ReqOutChat.CreateReqOutChat(mBuilder, CharId, ReqOutChat.CreateChatInfosVector(mBuilder, chatOffset));

            var reqPacket = ZChatPacket.Create<ReqOutChat>(this, Code.CT_OUT_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResOutChat recvMsgPacket = recvPacket.Get<ResOutChat>();

                for (int i = 0; i < recvMsgPacket.ChatInfosLength; i++)
                {
                    ZWebChatData.RemoveEnterInfo(recvMsgPacket.ChatInfos(i).Value.ChatType);
                }

                onRecvPacket?.Invoke(recvPacket, recvMsgPacket);
            }, _onError);
        }
        #endregion

        #region SendMessage
        public void REQ_SendServerChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, System.Action<ZWebRecvPacket, ResServerChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqServerChat.CreateReqServerChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), mBuilder.CreateString(Message));

            var reqPacket = ZChatPacket.Create<ReqServerChat>(this, Code.CT_SERVER_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResServerChat recvPacketMsg = recvPacket.Get<ResServerChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendTradeChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, System.Action<ZWebRecvPacket, ResTradeChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqTradeChat.CreateReqTradeChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), mBuilder.CreateString(Message));
            var reqPacket = ZChatPacket.Create<ReqTradeChat>(this, Code.CT_TRADE_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResTradeChat recvPacketMsg = recvPacket.Get<ResTradeChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendNormalChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, uint StageTid, ulong ChannelId, System.Action<ZWebRecvPacket, ResNormalChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqNormalChat.CreateReqNormalChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), mBuilder.CreateString(Message), StageTid, ChannelId);
            var reqPacket = ZChatPacket.Create<ReqNormalChat>(this, Code.CT_NORMAL_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResNormalChat recvPacketMsg = recvPacket.Get<ResNormalChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendInterNormalChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, uint StageTid, ushort ChannelId, System.Action<ZWebRecvPacket, ResInterNormalChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqInterNormalChat.CreateReqInterNormalChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), StageTid, ChannelId, mBuilder.CreateString(Message));
            var reqPacket = ZChatPacket.Create<ReqInterNormalChat>(this, Code.CT_INTER_NORMAL_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResInterNormalChat recvPacketMsg = recvPacket.Get<ResInterNormalChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendPartyChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, ulong PartyId, System.Action<ZWebRecvPacket, ResPartyChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqPartyChat.CreateReqPartyChat(mBuilder,
                E_ChatPartySubType.Chat,
                CharId,
                mBuilder.CreateString(CharNick),
                Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName),
                PartyId,
                mBuilder.CreateString(Message));
            mBuilder.Finish(offset.Value);

            var reqPacket = ZChatPacket.Create<ReqPartyChat>(this, Code.CT_PARTY_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResPartyChat recvPacketMsg = recvPacket.Get<ResPartyChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendInterPartyChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, ulong PartyId, System.Action<ZWebRecvPacket, ResInterPartyChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqInterPartyChat.CreateReqInterPartyChat(mBuilder,
                CharId,
                mBuilder.CreateString(CharNick),
                Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName),
                PartyId,
                mBuilder.CreateString(Message));
            mBuilder.Finish(offset.Value);

            var reqPacket = ZChatPacket.Create<ReqInterPartyChat>(this, Code.CT_INTER_PARTY_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResInterPartyChat recvPacketMsg = recvPacket.Get<ResInterPartyChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendGuildChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, System.Action<ZWebRecvPacket, ResGuildChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqGuildChat.CreateReqGuildChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), mBuilder.CreateString(Message));
            var reqPacket = ZChatPacket.Create<ReqGuildChat>(this, Code.CT_GUILD_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResGuildChat recvPacketMsg = recvPacket.Get<ResGuildChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendAllianceChatMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, ulong RoomID, System.Action<ZWebRecvPacket, ResAllianceChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqAllianceChat.CreateReqAllianceChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), RoomID, mBuilder.CreateString(Message));
            var reqPacket = ZChatPacket.Create<ReqAllianceChat>(this, Code.CT_ALLIANCE_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResAllianceChat recvPacketMsg = recvPacket.Get<ResAllianceChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }

        public void REQ_SendWhisperMessage(ulong CharId, string CharNick, ulong GuildId, uint GuildMarkTid, string GuildName, string Message, ulong TargetId, string TargetNick, System.Action<ZWebRecvPacket, ResUserChat> onRecvPacket, PacketErrorCBDelegate _onError = null)
        {
            var offset = ReqUserChat.CreateReqUserChat(mBuilder, CharId, mBuilder.CreateString(CharNick), Me.SelectedServerID, GuildId, GuildMarkTid, string.IsNullOrEmpty(GuildName) ? default : mBuilder.CreateString(GuildName), TargetId, mBuilder.CreateString(TargetNick), mBuilder.CreateString(Message));
            var reqPacket = ZChatPacket.Create<ReqUserChat>(this, Code.CT_USER_CHAT, mBuilder, offset.Value);

            SendPacket(reqPacket, (recvPacket) =>
            {
                ResUserChat recvPacketMsg = recvPacket.Get<ResUserChat>();

                onRecvPacket?.Invoke(recvPacket, recvPacketMsg);
            }, _onError);
        }
        #endregion

        //#region Delegate
        //public static void UpdateParty(NetUserData.ePartyUpdateKind kind)
        //{
        //    ZLog.LogWarning("##################### UpdateParty $$$$$$$$$$$$$$$$$$$ " + NetData.CurrentCharacter.PartyID);
        //
        //    if (NetData.CurrentCharacter.PartyID == 0)
        //    {
        //        if (ZNetChatData.IsEnter(E_ChatType.Party))
        //            ExitChat(NetData.CharID, new List<ChatEnterData>() { ZNetChatData.GetEnterInfo(E_ChatType.Party) }, null);
        //        //else if(ZNetChatData.IsEnter(E_ChatType.InterParty))
        //        //  ExitChat(NetData.CharID, new List<ChatEnterData>() { ZNetChatData.GetEnterInfo(E_ChatType.InterParty) }, null);
        //    }
        //    else if (NetData.CurrentCharacter.PartyID != 0)
        //    {
        //        //if ((ZGameManager.instance.CurGameMode.StageTable.StageType == GameDB.E_StageType.InterServer || ZGameManager.instance.CurGameMode.StageTable.StageType == GameDB.E_StageType.Tower) && !ZNetChatData.IsEnter(E_ChatType.InterParty))
        //        //  EnterChat(NetData.CharID, new List<ChatEnterData>() { new ChatEnterData(E_ChatType.InterParty, NetData.CurrentCharacter.PartyID.ToString()) }, null);
        //        if (!ZNetChatData.IsEnter(E_ChatType.Party))
        //            EnterChat(NetData.CharID, new List<ChatEnterData>() { new ChatEnterData(E_ChatType.Party, NetData.CurrentCharacter.PartyID.ToString()) }, null);
        //    }
        //}
        //
        //public static void UpdateGuild()
        //{
        //    ZLog.Log("UpdateGuild " + NetData.CurrentCharacter.GuildId + " = " + ZNetChatData.IsEnter(E_ChatType.Guild));
        //    if (NetData.CurrentCharacter.GuildId == 0 && ZNetChatData.IsEnter(E_ChatType.Guild))
        //    {
        //        ExitChat(NetData.CharID, new List<ChatEnterData>() { ZNetChatData.GetEnterInfo(E_ChatType.Guild) }, null);
        //    }
        //    else if (NetData.CurrentCharacter.GuildId != 0 && !ZNetChatData.IsEnter(E_ChatType.Guild))
        //    {
        //        EnterChat(NetData.CharID, new List<ChatEnterData>() { new ChatEnterData(E_ChatType.Guild, NetData.CurrentCharacter.GuildId.ToString()) }, null);
        //    }
        //}
        //
        //public static void UpdateGuildAlliance()
        //{
        //    if ((NetData.CurrentCharacter.myGuildData == null || NetData.CurrentCharacter.myGuildData.ChatId == 0 || NetData.CurrentCharacter.myGuildData.ChatState != E_GuildAllianceChatState.Enter) && ZNetChatData.IsEnter(E_ChatType.Alliance))
        //    {
        //        ExitChat(NetData.CharID, new List<ChatEnterData>() { ZNetChatData.GetEnterInfo(E_ChatType.Alliance) }, null);
        //    }
        //    else if ((NetData.CurrentCharacter.myGuildData != null && NetData.CurrentCharacter.myGuildData.ChatId != 0 && NetData.CurrentCharacter.myGuildData.ChatState == E_GuildAllianceChatState.Enter) && !ZNetChatData.IsEnter(E_ChatType.Alliance))
        //    {
        //        EnterChat(NetData.CharID, new List<ChatEnterData>() { new ChatEnterData(E_ChatType.Alliance, NetData.CurrentCharacter.myGuildData.ChatId.ToString()) }, null);
        //    }
        //}
        //#endregion
    }
}