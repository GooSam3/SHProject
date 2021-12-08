using System.Collections.Generic;
using WebNet;
using ZNet;
using ZNet.Data;

/// <summary>
/// WebSocket 기반 브로드캐스트 패킷 처리하는 클래스
/// </summary>
public class ZWebBroadcast
{
	static Dictionary<Code, System.Action<ZWebRecvPacket>> DicBroadcastCB = new Dictionary<Code, System.Action<ZWebRecvPacket>>();

	public static void AddCallback(Code code, System.Action<ZWebRecvPacket> _cb)
	{
		if (!DicBroadcastCB.ContainsKey(code))
			DicBroadcastCB.Add(code, null);

		DicBroadcastCB[code] += _cb;
	}

	public static void RemoveCallback(Code code, System.Action<ZWebRecvPacket> _cb)
	{
		if (DicBroadcastCB.ContainsKey(code))
			DicBroadcastCB[code] -= _cb;
	}

	public static void ClearCallback(Code _code)
	{
		DicBroadcastCB.Remove(_code);
	}

	public static void ClearAllCallback()
	{
		DicBroadcastCB.Clear();
	}

	/// <summary>
	/// <see cref="ZWebCommunicator"/>로 부터 Broadcast관련 메시지 받아서 처리되는 함수.
	/// </summary>
	public static void RecvBroadCastMessage(ZWebRecvPacket _recvPacket)
	{
		switch (_recvPacket.ID)
		{
			case Code.BROADCAST_PARTY_INVITE:
            case Code.BROADCAST_PARTY_JOIN:
            case Code.BROADCAST_PARTY_REFUSE:
            case Code.BROADCAST_PARTY_OUT:
            case Code.BROADCAST_PARTY_KICK_OUT:
            case Code.BROADCAST_PARTY_CHANGE_MASTER:
            //case Code.BROADCAST_PARTY_CHAT:
            //case Code.BROADCAST_INTER_PARTY_CHAT:
                {
                    ZPartyManager.Instance.RecvBroadcastMessage(_recvPacket);
                }
                break;
			// ============ CHATTING =============
			case Code.BROADCAST_GLOBAL_CHAT:
                RecvGlobalChat(_recvPacket);
				break;
			case Code.BROADCAST_SERVER_CHAT:
                RecvSeverChat(_recvPacket);
				break;
			case Code.BROADCAST_NORMAL_CHAT:
                RecvNormalChat(_recvPacket);
				break;
			case Code.BROADCAST_USER_CHAT:
                RecvUserChat(_recvPacket);
				break;
			case Code.BROADCAST_GUILD_CHAT:
                RecvGuildChat(_recvPacket);
				break;
			case Code.BROADCAST_PARTY_CHAT:
                RecvPartyChat(_recvPacket);
				break;
			case Code.BROADCAST_ALLIANCE_CHAT:
                RecvAllianceChat(_recvPacket);
				break;
			case Code.BROADCAST_INTER_NORMAL_CHAT:
                RecvInterNormalChat(_recvPacket);
				break;
			case Code.BROADCAST_INTER_PARTY_CHAT:
                RecvInterPartyChat(_recvPacket);
				break;
			case Code.BROADCAST_CHAT_BLOCK:
                RecvChatBlock(_recvPacket);
				break;
            case Code.BROADCAST_QUEST_UPDATE:
                RecvQuestUpdate(_recvPacket);
                break;
            case Code.BROADCAST_NEW_QUEST:
                RecvQuestNew(_recvPacket);
                break;
            // 콜로세움
            case Code.BROADCAST_REWARD_COLOSSEUM_SERVER:
                ZGameModeColosseum.RECV_RewardColosseum( _recvPacket );
                break;
            case Code.BROADCAST_JOIN_COLOSSEUM:
                ZGameModeColosseum.RECV_JoinColosseum( _recvPacket );
                break;
            case Code.BROADCAST_LEAVE_COLOSSEUM:
                ZGameModeColosseum.RECV_LeaveColosseum( _recvPacket );
                break;
            case Code.BROADCAST_LEAVE_COLOSSEUM_QUEUE:
                ZGameModeColosseum.RECV_LeaveColosseumQueue( _recvPacket );
                break;

            // 이벤트
            case Code.BROADCAST_QUEST_EVENT_UPDATE:
                RECV_Broadcast_QuestEvent(_recvPacket);
                break;
        }

		if (DicBroadcastCB.ContainsKey(_recvPacket.ID))
			DicBroadcastCB[_recvPacket.ID]?.Invoke(_recvPacket);
	}

	private static void Recv_BROADCAST_REWARD(ZWebRecvPacket _recvPacket)
	{
		BroadcastPartyInvite broadcastPartyInvite = _recvPacket.Get<BroadcastPartyInvite>();
	}

    private static void RECV_Broadcast_QuestEvent(ZWebRecvPacket _recvPacket)
	{
        var recvMsgPacket = _recvPacket.Get<BroadcastQuestEventUpdate>();

        var listQuestEvent = new List<QuestEvent>();
        for(int i =0;i<recvMsgPacket.UpdatedQuestsLength;i++)
		{
            listQuestEvent.Add(recvMsgPacket.UpdatedQuests(i).Value);
        }

        Me.CurCharData.AddEventQuestDataList(listQuestEvent);
	}

    #region Chatting =============================================
    public static void RecvGlobalChat(ZWebRecvPacket recvPacket)
    {
        BroadcastGlobalChat recvMsgPacket = recvPacket.Get<BroadcastGlobalChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvSeverChat(ZWebRecvPacket recvPacket)
    {
        BroadcastServerChat recvMsgPacket = recvPacket.Get<BroadcastServerChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvNormalChat(ZWebRecvPacket recvPacket)
    {
        BroadcastNormalChat recvMsgPacket = recvPacket.Get<BroadcastNormalChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvUserChat(ZWebRecvPacket recvPacket)
    {
        BroadcastUserChat recvMsgPacket = recvPacket.Get<BroadcastUserChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvGuildChat(ZWebRecvPacket recvPacket)
    {
        BroadcastGuildChat recvMsgPacket = recvPacket.Get<BroadcastGuildChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvPartyChat(ZWebRecvPacket recvPacket)
    {
        BroadcastPartyChat recvMsgPacket = recvPacket.Get<BroadcastPartyChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvAllianceChat(ZWebRecvPacket recvPacket)
    {
        BroadcastAllianceChat recvMsgPacket = recvPacket.Get<BroadcastAllianceChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvInterNormalChat(ZWebRecvPacket recvPacket)
    {
        BroadcastInterNormalChat recvMsgPacket = recvPacket.Get<BroadcastInterNormalChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvInterPartyChat(ZWebRecvPacket recvPacket)
    {
        BroadcastInterPartyChat recvMsgPacket = recvPacket.Get<BroadcastInterPartyChat>();
        ZWebChatData.AddChatMsg(recvMsgPacket);
    }

    public static void RecvChatBlock(ZWebRecvPacket recvPacket)
    {
        BroadcastChatBlock recvMsgPacket = recvPacket.Get<BroadcastChatBlock>();
        ZNet.Data.Me.CurCharData.ChatBlockExpireDt = recvMsgPacket.ExpireDt;
            UIMessagePopup.ShowPopupOk(string.Format(
                DBLocale.GetText("WChat_Input_Limit"), 
                UnityEngine.Mathf.CeilToInt((float)(ZNet.Data.Me.CurCharData.ChatBlockExpireDt - TimeManager.NowSec) / (float)TimeHelper.MinuteSecond)
        ));
    }

    #endregion ============================================= Chatting

    #region Quest =============================================
    public static void RecvQuestUpdate(ZWebRecvPacket recvPacket)
	{
        BroadcastQuestUpdate recvMsgPacket = recvPacket.Get<BroadcastQuestUpdate>();
        int length = recvMsgPacket.UpdatedQuestsLength;
        for (int i = 0; i < length; i++)
		{
            ZNet.Data.Me.CurCharData.AddQuestList(recvMsgPacket.UpdatedQuests(i).Value, false);
		}
    }

    public static void RecvQuestNew(ZWebRecvPacket recvPacket)
	{

	}

    #endregion
}
