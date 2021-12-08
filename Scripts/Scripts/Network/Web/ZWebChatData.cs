using GameDB;
using MmoNet;
using System.Collections.Generic;
using UnityEngine;
using WebNet;
using ZDefine;
using ZNet.Data;

public class ZWebChatData
{
    public static Dictionary<ChatFilter, List<ChatData>> chatDic = new Dictionary<ChatFilter, List<ChatData>>();

    public static List<float> ChatSendTimes = new List<float>();

    static float BanExpireTime = 0f;

    public static void AddSendTime()
    {
        ChatSendTimes.Add(Time.time);
        if (ChatSendTimes.Count > 50)
            ChatSendTimes.RemoveRange(0, ChatSendTimes.Count - 50);
    }

    public static float CheckSend()
    {
        if (BanExpireTime != 0 && BanExpireTime > Time.time)
        {
            return BanExpireTime - Time.time;
        }

        int chatCnt = ChatSendTimes.FindAll(item => (Time.time - item) < DBConfig.Chat_Ban_CheckTime).Count;

        if (chatCnt > DBConfig.Chat_Ban_CheckCount)
        {
            BanExpireTime = Time.time + DBConfig.Chat_Ban_Time;
            return BanExpireTime - Time.time;
        }

        return 0;
    }

    public static void ClearChatList()
    {
        chatDic.Clear();
        OnClearMsg?.Invoke();
    }

    public static List<ChatData> GetChatList(ChatFilter filter)
    {
        List<ChatData> retList = new List<ChatData>();

        foreach (ChatFilter key in chatDic.Keys)
        {
            if (filter.HasFlag(key))
            {
                retList.AddRange(chatDic[key]);
            }
        }

        retList.Sort((x, y) =>
        {
            if (x.RecvTime < y.RecvTime)
                return -1;
            else if (x.RecvTime > y.RecvTime)
                return 1;
            return 0;
        });

        return retList;
    }

    public static List<ChatData> GetContainer(ChatFilter filter)
    {
        if (!chatDic.ContainsKey(filter))
            chatDic.Add(filter, new List<ChatData>());

        return chatDic[filter];
    }

    public static void RemoveChatMsg(ChatFilter type, ChatData ChatData)
    {
        var container = GetContainer(type);
        container.Remove(ChatData);
    }

    public static void AddChatMsg(ChatFilter type, ChatData ChatData)
    {
        if (Me.CurCharData.chatFilter.HasFlag(type) || type.HasFlag(ChatFilter.TYPE_TRADE))
        {
            var container = GetContainer(type);
            container.Add(ChatData);
            container.Sort((x, y) =>
            {
                if (x.RecvTime < y.RecvTime)
                    return -1;
                else if (x.RecvTime > y.RecvTime)
                    return 1;
                return 0;
            });

            if (container.Count > DBConfig.Chatting_View_Max)
                container.RemoveRange(0, container.Count - DBConfig.Chatting_View_Max);
        }

        OnAddMsg?.Invoke(type, ChatData);

        if (IsUserChat(ChatData.type) && OnChatMsg.ContainsKey(ChatData.CharId))
            OnChatMsg[ChatData.CharId]?.Invoke(ChatData.CharId, ChatData);
    }

    static bool IsUserChat(ChatViewType type)
    {
        if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.GodLand)
            return false;

        switch (type)
        {
            case ChatViewType.TYPE_NORMAL_CHAT:
                return true;
            case ChatViewType.TYPE_SERVER_CHAT:
            case ChatViewType.TYPE_PARTY_CHAT:
            case ChatViewType.TYPE_GUILD_CHAT:
            case ChatViewType.TYPE_ALLIANCE_CHAT:
            case ChatViewType.TYPE_TRADE_CHAT:
                if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum)
                    return false;
                else
                    return true;
            default:
                return false;
        }
    }

    static bool CheckEmoticon(ulong CharId, string Msg)
    {
        if (DBEmoticon.IsEmoticonMsg(Msg))
        {
            if (OnEmoticonMsg.ContainsKey(CharId))
                OnEmoticonMsg[CharId]?.Invoke(CharId, DBEmoticon.GetEmoticonIcon(Msg));

            return true;
        }

        return false;
    }

    static bool CheckBlock(ulong CharId)
    {
        if (Me.CurCharData.GetBlockCharacter().Find(item => item.CharID == CharId) != null)
            return true;

        return false;
    }

    public static void AddChatMsg(BroadcastGlobalChat recvMsg)
    {
        ChatData addChatData = null;

        switch (recvMsg.SubType)
        {
            //case E_ChatGlobalSubType.Sneer:
            //    {
            //        //이모티콘, 조롱하기 휘발성으로 브로드 캐스팅
            //        //addChatData = new ChatData(ChatViewType.no, recvMsg.CharId, recvMsg.CharNick, string.Format("조롱하기 첫번째 {0},두번째 {1}",recvMsg.Args(0), recvMsg.Args(1)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //        //AddChatMsg(ChatFilter.TYPE_NONE, addChatData);
            //        //UIManager.NoticeMessage(string.Format("{0}가 {1}를 처치 하였습니다.",recvMsg.CharNick,recvMsg.Args(1)), GetChatViewColor(ChatViewType.TYPE_SYSTEM_CHAT));
            //
            //        string killerServerName = NetData.Instance.ServerDic.TryGetValue(recvMsg.ServerIdx, out var killerserverInfo) ? string.Format(DBLocale.GetLocaleText("Stage_Server_Name"), killerserverInfo.Name) : "";
            //        string dieServerName = NetData.Instance.ServerDic.TryGetValue(uint.Parse(recvMsg.Args(0)), out var dieserverInfo) ? string.Format(DBLocale.GetLocaleText("Stage_Server_Name"), dieserverInfo.Name) : "";
            //
            //        var panel = UIManager.instance.GetActiveUI<HudBasePanel>();
            //        if (panel != null)
            //        {
            //            UIManager.GetSubUI("SneerUI", panel, (loadUI) => {
            //                loadUI.GetComponent<SneerUI>().ShowMsg(
            //                    string.Format(DBLocale.GetLocaleText("PK_Sneer_Global"),
            //                    killerServerName,
            //                    recvMsg.CharNick,
            //                    dieServerName,
            //                    recvMsg.Args(2)));
            //            });
            //        }
            //
            //        var rankingpanel = UIManager.instance.GetActiveUI<RankingPanel>();
            //        if (rankingpanel != null)
            //        {
            //            UIManager.GetSubUI("SneerUI", panel, (loadUI) => {
            //                loadUI.GetComponent<SneerUI>().ShowMsg(
            //                    string.Format(DBLocale.GetLocaleText("PK_Sneer_Global"),
            //                    killerServerName,
            //                    recvMsg.CharNick,
            //                    dieServerName,
            //                    recvMsg.Args(2)));
            //            });
            //        }
            //
            //        addChatData = new ChatData(ChatViewType.TYPE_NOTICE_WARNING_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //            string.Format(DBLocale.GetLocaleText("PK_Sneer_Global"),
            //            killerServerName,
            //            recvMsg.CharNick,
            //            dieServerName,
            //            recvMsg.Args(2)),
            //            recvMsg.GuildId,
            //            recvMsg.GuildMarkTid,
            //            recvMsg.GuildName);
            //        AddChatMsg(ChatFilter.TYPE_ALL, addChatData);
            //    }
            //    break;
            //case E_ChatGlobalSubType.ServerBossSpawn:
            //    {
            //        addChatData = new ChatData(ChatViewType.TYPE_NOTICE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("MessageGlobal_BossSummon2"), DBLocale.GetMonsterName(DBStage.GetSummonBoss(uint.Parse(recvMsg.Args(0))))), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //        AddChatMsg(ChatFilter.TYPE_ALL, addChatData);
            //
            //        UIManager.NoticeMessage(string.Format(DBLocale.GetLocaleText("MessageGlobal_BossSummon2"), DBLocale.GetMonsterName(DBStage.GetSummonBoss(uint.Parse(recvMsg.Args(0))))));
            //    }
            //    break;
            case E_ChatGlobalSubType.GetItem:
                {
                    string Msg = "";
                    string ItemName = "";
                    byte Grade = 0;

                    uint tableID = uint.Parse(recvMsg.Args(3));

                    switch ((GameDB.E_GoodsKindType)System.Enum.Parse(typeof(GameDB.E_GoodsKindType), recvMsg.Args(2)))
                    {
                        case GameDB.E_GoodsKindType.Item:
                            Item_Table table = DBItem.GetItem(tableID);
                            ItemName = DBLocale.GetItemLocale(table);
                            Grade = table.Grade;
                            break;
                        case GameDB.E_GoodsKindType.Change:
                            ItemName = DBChange.GetChangeFullName(tableID);
                            Grade = DBChange.GetChangeGrade(tableID);
                            break;
                        case GameDB.E_GoodsKindType.Pet:
                            ItemName = DBPet.GetPetFullName(tableID);
                            Grade = DBPet.GetPetGrade(tableID);
                            break;
                    }

                    if (recvMsg.Args(0) != "0")
                    {
                        string LocationName = "";

                        switch ((E_ChatServerNoticeLocationType)System.Enum.Parse(typeof(E_ChatServerNoticeLocationType), recvMsg.Args(0)))
                        {
                            case E_ChatServerNoticeLocationType.Stage:
                                LocationName = DBLocale.GetStageName(uint.Parse(recvMsg.Args(1)));
                                break;
                            case E_ChatServerNoticeLocationType.Gacha:
                                LocationName = DBLocale.GetItemLocale(DBItem.GetItem(uint.Parse(recvMsg.Args(1))));
                                break;
                                //case E_ChatServerNoticeLocationType.PetAdv:
                                //    if (DBPetAdventure.TryGetAdventureData(uint.Parse(recvMsg.Args(1)), out var petAdvTableData))
                                //        LocationName = DBLocale.GetLocaleText(string.Format("AdventureTab_{0}", petAdvTableData.AdventureTab.ToString()));
                                //    break;
                        }

                        //if (recvMsg.Args(4) != "0")
                        //    Msg = string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_DropNotice"),
                        //        recvMsg.ServerIdx, string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(4))), ItemName);
                        //else
                        //    Msg = string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_Drop"), recvMsg.ServerIdx, string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, ItemName, LocationName);
                    }
                    else
                    {
                        Msg = string.Format(DBLocale.GetText("Inter_Server_Boss_Default"), recvMsg.ServerIdx, string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, ItemName);
                    }

                    addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, Msg, Msg, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(addChatData.Message, Color.white, DBConfig.MessageSystem_Time, UIMessageNoticeEnum.E_MessageType.SubNotice);
                }
                break;
            //case E_ChatGlobalSubType.KillMonster:
            //    {
            //        addChatData = new ChatData(ChatViewType.TYPE_NOTICE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //            string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_KillNotice"), recvMsg.ServerIdx, recvMsg.CharNick, DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(0)))), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
            //
            //        UIManager.NoticeMessage(string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_KillNotice"), recvMsg.ServerIdx, recvMsg.CharNick, DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(0)))));
            //    }
            //    break;
            case E_ChatGlobalSubType.None:
            case E_ChatGlobalSubType.Chat:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;

                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SERVER_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_ALL, addChatData);
                }
                break;
        }
    }

    public static void AddChatMsg(BroadcastServerChat recvMsg)
    {
        ChatData addChatData = null;

        switch (recvMsg.SubType)
        {
            case E_ChatServerSubType.Sneer:
                {
                    //이모티콘, 조롱하기 휘발성으로 브로드 캐스팅
                    //addChatData = new ChatData(ChatViewType.no, recvMsg.CharId, recvMsg.CharNick, string.Format("조롱하기 첫번째 {0},두번째 {1}",recvMsg.Args(0), recvMsg.Args(1)), recvMsg.GuildId, recvMsg.GuildMarkTid, ecvMsg.GuildName);
                    //AddChatMsg(ChatFilter.TYPE_NONE, addChatData);
                    //UIManager.NoticeMessage(string.Format("{0}가 {1}를 처치 하였습니다.",recvMsg.CharNick,recvMsg.Args(1)), GetChatViewColor(ChatViewType.TYPE_SYSTEM_CHAT));

                    //var panel = UIManager.instance.GetActiveUI<HudBasePanel>();
                    //if (panel != null)
                    //{
                    //    UIManager.GetSubUI("SneerUI", panel, (loadUI) => {
                    //        loadUI.GetComponent<SneerUI>().ShowMsg(string.Format(DBLocale.GetLocaleText("PK_Sneer"), recvMsg.CharNick, recvMsg.Args(2)));
                    //    });
                    //}

                    //var rankingpanel = UIManager.instance.GetActiveUI<RankingPanel>();
                    //if (rankingpanel != null)
                    //{
                    //    UIManager.GetSubUI("SneerUI", panel, (loadUI) => {
                    //        loadUI.GetComponent<SneerUI>().ShowMsg(string.Format(DBLocale.GetLocaleText("PK_Sneer"), recvMsg.CharNick, recvMsg.Args(2)));
                    //    });
                    //}
                    // SubType = Sneer, CharId = 836, CharNick = aasssfd, ServerIdx = 1, Message =, GuildId = 0, GuildMarkTid = 0, GuildName =, Args =[{ 1}, { 1012}, { jb0043}]
                    addChatData = new ChatData(ChatViewType.TYPE_NOTICE_WARNING_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("PK_Sneer"), recvMsg.CharNick, recvMsg.Args(2)), recvMsg.Args(2), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    //addChatData = new ChatData(ChatViewType.TYPE_NOTICE_WARNING_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("PK_Sneer_Global"), recvMsg.CharNick, "None", recvMsg.Args(2)), recvMsg.Args(2), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_ALL, addChatData);

                    UICommon.SetNoticeMessage(addChatData.Message, Color.white, DBConfig.MessageSystem_Time, UIMessageNoticeEnum.E_MessageType.SubNotice);
                }
                break;
            //case E_ChatServerSubType.Notice:
            //    {
            //        addChatData = new ChatData(ChatViewType.TYPE_NOTICE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
            //
            //        UIManager.AlretNoticeMessage(addChatData.Message, GetChatViewColor(ChatViewType.TYPE_SYSTEM_CHAT));
            //    }
            //    break;
            case E_ChatServerSubType.GetItem:
                {
                    string Msg = "";
                    string ItemName = "";
                    byte Grade = 0;

                    uint tableID = uint.Parse(recvMsg.Args(3));

                    switch ((GameDB.E_GoodsKindType)System.Enum.Parse(typeof(GameDB.E_GoodsKindType), recvMsg.Args(2)))
                    {
                        case GameDB.E_GoodsKindType.Item:
                            Item_Table table = DBItem.GetItem(tableID);
                            ItemName = DBLocale.GetItemLocale(table);
                            Grade = table.Grade;
                            break;
                        case GameDB.E_GoodsKindType.Change:
                            ItemName = DBChange.GetChangeFullName(tableID);
                            Grade = DBChange.GetChangeGrade(tableID);
                            break;
                        case GameDB.E_GoodsKindType.Pet:
                            ItemName = DBPet.GetPetFullName(tableID);
                            Grade = DBPet.GetPetGrade(tableID);
                            break;
                    }


                    if (recvMsg.Args(0) != "0")
                    {
                        string LocationName = "";

                        switch ((E_ChatServerNoticeLocationType)System.Enum.Parse(typeof(E_ChatServerNoticeLocationType), recvMsg.Args(0)))
                        {
                            case E_ChatServerNoticeLocationType.Stage:
                                LocationName = DBLocale.GetStageName(uint.Parse(recvMsg.Args(1)));
                                break;
                            case E_ChatServerNoticeLocationType.Gacha:
                                LocationName = DBLocale.GetItemLocale(DBItem.GetItem(uint.Parse(recvMsg.Args(1))));
                                break;
                                //case E_ChatServerNoticeLocationType.PetAdv:
                                //    if (DBPetAdventure.TryGetAdventureData(uint.Parse(recvMsg.Args(1)), out var petAdvTableData))
                                //        LocationName = DBLocale.GetLocaleText(string.Format("AdventureTab_{0}", petAdvTableData.AdventureTab.ToString()));
                                //    break;
                        }

                        Msg = string.Format(DBLocale.GetText("MessageGlobal_Drop"), string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, ItemName, LocationName);
                    }
                    else
                    {
                        Msg = string.Format(DBLocale.GetText("MessageGlobal_Default"), string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, ItemName);
                    }

                    addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, Msg, Msg, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(addChatData.Message, Color.white, DBConfig.MessageSystem_Time, UIMessageNoticeEnum.E_MessageType.SubNotice);
                }
                break;
            //case E_ChatServerSubType.EnchantItem:
            //    {
            //        uint tableID = uint.Parse(recvMsg.Args(0));
            //
            //        addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //                string.Format(DBLocale.GetLocaleText("MessageGlobal_Enchant"), string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, DBItem.GetItemFullName(tableID, true, false)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
            //        UIManager.NoticeMessage(addChatData.Message);
            //    }
            //    break;
            //case E_ChatServerSubType.GetGold:
            //    {
            //        uint goldCnt = uint.Parse(recvMsg.Args(0));
            //
            //        addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //                string.Format(DBLocale.GetLocaleText("MessageGlobal_Silver"), string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, MathHelper.CountString(goldCnt)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
            //        UIManager.NoticeMessage(addChatData.Message);
            //    }
            //    break;
            case E_ChatServerSubType.Trade:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_TRADE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_TRADE, addChatData);
                }
                break;
			case E_ChatServerSubType.KillMonster:
				{
                    if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.BossWar)
					{
                        ZGameModeManager.Instance.CurrentGameMode<ZGameModeBossWar>().RECV_BossDead();

                    }
                    
					//addChatData = new ChatData(ChatViewType.TYPE_NOTICE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
					//	string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_KillNotice"), recvMsg.ServerIdx, recvMsg.CharNick, DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(0)))), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
					//AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

					//UIManager.NoticeMessage(string.Format(DBLocale.GetLocaleText("Inter_Server_Boss_KillNotice"), recvMsg.ServerIdx, recvMsg.CharNick, DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(0)))));
				}
				break;
			//case E_ChatServerSubType.EnchantRune:
			//    {
			//        uint tableID = uint.Parse(recvMsg.Args(0));
			//
			//        addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
			//                string.Format(DBLocale.GetLocaleText("Rune_Enchant_Success"), string.IsNullOrEmpty(recvMsg.CharNick) ? "(익명)" : recvMsg.CharNick, DBItem.GetItemFullName(tableID, true, false), recvMsg.Args(1)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
			//        UIManager.NoticeMessage(addChatData.Message);
			//    }
			//    break;
			case E_ChatServerSubType.ServerBossSpawn:
				{
                    ZGameModeManager.Instance.mEventBossSpawn?.Invoke();
                    string bossName = string.Empty;
                    uint stageTid = uint.Parse(recvMsg.Args(0));

                    if(DBStage.TryGet(stageTid, out Stage_Table table))
					{
                        if(DBMonster.TryGet(table.SummonBossID, out Monster_Table monster))
						{
                            bossName = DBLocale.GetText(monster.MonsterTextID);
						}
					}

					addChatData = new ChatData(ChatViewType.TYPE_NOTICE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, DBLocale.GetText("MessageGlobal_BossSummon2", bossName), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(DBLocale.GetText("MessageGlobal_BossSummon2", bossName), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.SubNotice);
                }
				break;
			case E_ChatServerSubType.None:
            case E_ChatServerSubType.Chat:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SERVER_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_ALL, addChatData);
                }
                break;
                //case E_ChatServerSubType.SiegeReady:
                //    {
                //        // SiegeReady=11,
                //        // 공성 오픈전 알림
                //        // args =[stage_tid, remain_sec]
                //        ulong.TryParse(recvMsg.Args(1), out ulong remainSec);
                //        string strLocaleFormat = DBLocale.GetLocaleText("Siege_Broadcast_Ready");
                //        // string remainSecText = TimeHelper.RemainTimeText(remainSec);
                //        string strMessage = string.Format(strLocaleFormat, remainSec / 60);
                //        UIManager.NoticeMessage(strMessage);
                //        break;
                //    }
                //case E_ChatServerSubType.SiegeBarricadeBreak:
                //    // SiegeBarricadeBreak=12,
                //    // 공성 바리케이트 파괴 알림
                //    // args =[stage_tid, barricade_num]
                //
                //    ZLog.Log("바리케이트 파괴 서버챗.");
                //
                //    if (uint.TryParse(recvMsg.Args(1), out var barricadeId))
                //    {
                //        string messageText = "";
                //        // Barricade_East  701010 바리게이트 남 - Barricade_Destroy_South
                //        if (barricadeId == DBConfig.Barricade_South)
                //        {
                //            messageText = DBLocale.GetLocaleText("Barricade_Destroy_South");
                //        }
                //        // Barricade_South 701020 바리게이트 서 - Barricade_Destroy_West
                //        else if (barricadeId == DBConfig.Barricade_West)
                //        {
                //            messageText = DBLocale.GetLocaleText("Barricade_Destroy_West");
                //        }
                //        // Barricade_West  701030 바리게이트 동 - Barricade_Destroy_East
                //        else if (barricadeId == DBConfig.Barricade_East)
                //        {
                //            messageText = DBLocale.GetLocaleText("Barricade_Destroy_East");
                //        }
                //
                //        UIManager.NoticeMessage(messageText);
                //        ZLog.Log("바리케이트 파괴 서버챗." + barricadeId + " || " + messageText);
                //    }
                //    else
                //    {
                //        ZLog.Log("바리케이트 파괴 서버챗 파싱 실패");
                //    }
                //    break;
                //
                //case E_ChatServerSubType.SiegeGuardTowerBreak:
                //    {
                //        // SiegeGuardTowerBreak=13,
                //        // 공성 수호탑 파괴 알림
                //        // args =[stage_tid, break_char_nick, break_guild_name]
                //        string guildName = recvMsg.Args(2);
                //        string strLocaleFormat = DBLocale.GetLocaleText("Siege_Broadcast_GuardTower");
                //        string strMessage = string.Format(strLocaleFormat, guildName);
                //        UIManager.NoticeMessage(strMessage);
                //        break;
                //    }
        }
    }

    public static void AddChatMsg(BroadcastUserChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
            case E_ChatUserSubType.FriendConnect:
                {
                    if (!ZGameOption.Instance.bAlram_Friend_Connect)
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WFriend_Friend_Access"), recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Friend_Access"), recvMsg.CharNick), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                break;
            case E_ChatUserSubType.EnemyConnect:
                {
                    if (!ZGameOption.Instance.bAlram_Enemy_Connect)
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WFriend_Enemy_Accees"), recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Enemy_Accees"), recvMsg.CharNick), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                }
                break;
            case E_ChatUserSubType.RequestFriend:
                {
                    if (!ZGameOption.Instance.bAlram_Enemy_Connect)
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WFriend_Request"), recvMsg.CharNick), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);


                    UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Request"), recvMsg.CharNick), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);

                    ZWebManager.Instance.WebGame.REQ_GetRequestFriendList((recvPacket, recvMsgPacket) => {
                        if (UIManager.Instance.Find(out UIFrameFriend _friend) && _friend.Show)
                            _friend.SetReceiveRequestNewIcon();
                        else
                            if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                            _menu.ActiveRedDot(E_HUDMenu.Friend, Me.CurCharData.GetReceiveRequestFriend().Count > 0);
                    });
                }
                break;
            case E_ChatUserSubType.RequestFriendAccept:
                {
                    if (!ZGameOption.Instance.bAlram_Enemy_Connect)
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WFriend_Require_Ok"), recvMsg.CharNick), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("WFriend_Require_Ok"), recvMsg.CharNick), new Color(255, 255, 255), 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
                    DeviceSaveDatas.AddCharacterKey(Me.CharID, "ADD_FRIEND", recvMsg.CharNick);

                    ZWebManager.Instance.WebGame.REQ_GetFriendList((recvPacket, recvMsgPacket) => {
                        ZWebManager.Instance.WebGame.REQ_GetRequestFriendList((x, y) => {
                            // TODO : 알람 체크..
                        });
                    });
                }
                break;
            case E_ChatUserSubType.GuildJoin:
                {
                    addChatData = new ChatData(ChatViewType.TYPE_GUILD_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
                            string.Format("{0} 길드에 가입하셨습니다.", recvMsg.GuildName), recvMsg.GuildName, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);

                    // UIManager.NoticeMessage(addChatData.Message);

                    //ZNetGame.GetGuildInfo(recvMsg.GuildId, null);
                    //NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvMsg.GuildId,recvMsg.GuildName,(byte)recvMsg.GuildMarkTid);

                    if (Me.CurCharData.ID == recvMsg.CharId)
                    {
                        Me.CurCharData.SetGuildInfo(recvMsg.GuildId, recvMsg.GuildName, (byte)recvMsg.GuildMarkTid);
                        ZWebManager.Instance.WebGame.REQ_GetGuildInfo(recvMsg.GuildId
                            , (revPacket, resList) =>
                            {
                                OnJoinGuildUser_AfterGetInfoDone?.Invoke(recvMsg.CharId, resList);
                                ZWebManager.Instance.WebChat.CheckEnterChannel();
                            }, null);
                    }

                    OnJoinGuildUser?.Invoke(recvMsg.CharId, recvMsg.GuildId, recvMsg.CharNick, (byte)recvMsg.GuildMarkTid);

                    //var joinPanel = UIManager.instance.GetActiveUI<GuildJoinPanel>();
                    //if (joinPanel != null)
                    //{
                    //    UIManager.instance.HidePanel(joinPanel);
                    //    UIManager.instance.OpenPanel("GuildPanel");
                    //}
                }
                break;
            case E_ChatUserSubType.Chat:
            case E_ChatUserSubType.None:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;

                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_WHISPER_RECV_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("<<[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_WHISPER, addChatData);
                }
                break;
        }
    }

    public static void AddChatMsg(BroadcastNormalChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
            case E_ChatNormalSubType.Chat:
            case E_ChatNormalSubType.None:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_NORMAL_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);               
                    AddChatMsg(ChatFilter.TYPE_NORMAL, addChatData);
                }
                break;
        }
    }

    public static void AddChatMsg(BroadcastInterNormalChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
            case E_ChatInterNormalSubType.Chat:
            case E_ChatInterNormalSubType.None:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_NORMAL_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);                   
                    AddChatMsg(ChatFilter.TYPE_NORMAL, addChatData);
                }
                break;
        }
    }

    // 파티 초대받음
    public static void AddChatMsg(BroadcastPartyInvite recvMsg)
    {
        if (ZPartyManager.Instance.IsParty)
            return;

        AddSystemMsg(DBLocale.GetText("Party_Notice_Invite", recvMsg.Sender.Value.Nick));
    }

    // 파티원 입장
    public static void AddChatMsg(BroadcastPartyJoin recvMsg)
    {
        AddSystemMsg(DBLocale.GetText("Party_Notice_JoinPlayer", recvMsg.JoinMember.Value.Nick));
    }

    // 파티 초대 거절
    public static void AddChatMsg(BroadcastPartyRefuse recvMsg)
    {
        AddSystemMsg(DBLocale.GetText("Party_Notice_Refuse", recvMsg.RefuseCharNick));
    }

    // 파티 나가기
    public static void AddChatMsg(BroadcastPartyOut recvMsg)
    {
        if (recvMsg.OutMember.Value.CharId == Me.CharID)
            AddSystemMsg(DBLocale.GetText("Party_Secession_Check"));
        else
            AddSystemMsg(DBLocale.GetText("Party_Secession_Notice", recvMsg.OutMember.Value.Nick));
    }

    // 파티 강퇴
    public static void AddChatMsg(BroadcastPartyKickOut recvMsg)
    {
        var kickMember = recvMsg.KickMember.Value;

        if (ZPartyManager.Instance.IsMaster)
        {
            AddSystemMsg(DBLocale.GetText("Party_Banishment_Notice", kickMember.Nick));
        }
        else
        {
            if(kickMember.CharId == Me.CharID)
                AddSystemMsg(DBLocale.GetText("Party_Banishment_Notice_Me"));
            else
                AddSystemMsg(DBLocale.GetText("Party_Banishment_Notice_Player", kickMember.Nick));

        }
    }

    // 파티장 변경
    public static void AddChatMsg(BroadcastPartyChangeMaster recvMsg)
    {
        AddSystemMsg(DBLocale.GetText("Party_Delegate_Notice", recvMsg.NewMasterMember.Value.Nick));
    }

    public static void AddChatMsg(BroadcastPartyChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
            //case E_ChatPartySubType.GetItem:
            //    {
            //        string ItemName = "";
            //
            //        uint ItemTid = uint.Parse(recvMsg.Args(0));
            //        ItemName = DBItem.GetItemFullName(ItemTid);
            //
            //        addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //             string.Format(DBLocale.GetLocaleText("Wparty_ItemGain"), recvMsg.CharNick, ItemName, recvMsg.Args(1)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_PARTY, addChatData);
            //    }
            //    break;
            case E_ChatPartySubType.Chat:
            case E_ChatPartySubType.None:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_PARTY_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_PARTY, addChatData);
                }
                break;
        }
    }

    public static void AddChatMsg(BroadcastInterPartyChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
            //case E_ChatInterPartySubType.GetItem:
            //    {
            //        string ItemName = "";
            //
            //        uint ItemTid = uint.Parse(recvMsg.Args(0));
            //        ItemName = DBItem.GetItemFullName(ItemTid);
            //
            //        addChatData = new ChatData(ChatViewType.TYPE_GET_ITEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
            //             string.Format(DBLocale.GetLocaleText("Wparty_ItemGain"), recvMsg.CharNick, ItemName, recvMsg.Args(1)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_PARTY, addChatData);
            //    }
            //    break;
            case E_ChatInterPartySubType.Chat:
            case E_ChatInterPartySubType.None:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_PARTY_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_PARTY, addChatData);
                }
                break;
        }
    }

    public static System.Action<ulong, ulong, string, byte> OnJoinGuildUser;
    // ulong : characterID
    public static System.Action<ulong, ResGetGuildInfo> OnJoinGuildUser_AfterGetInfoDone;
    public static System.Action<ulong, ulong> OnLeaveGuildUser;
    public static System.Action<ulong, byte> OnGuildMarkChange;

    public static void AddChatMsg(BroadcastGuildChat recvMsg)
    {
        ChatData addChatData = null;
        ZLog.Log(ZLogChannel.UI, $"## {recvMsg.SubType}");
        switch (recvMsg.SubType)
        {
            //case E_ChatGuildSubType.ChangeNotice:
            //    {
            //        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguild_Notice_Change"), recvMsg.Args(0)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
            //
            //        var guildPanel = UIManager.instance.GetActivePanel("GuildPanel") as GuildPanel;
            //        if (guildPanel != null)
            //        {
            //            guildPanel.guildInfo.Notice = recvMsg.Args(0);
            //            if (guildPanel.guildInfoUI.gameObject.activeSelf)
            //                guildPanel.guildInfoUI.NoticeInput.text = recvMsg.Args(0);
            //        }
            //    }
            //    break;
            case E_ChatGuildSubType.Connect:
                {
                    if (recvMsg.CharId == Me.CharID)
                        return;
            
                    if (!ZGameOption.Instance.bAlram_Guild_Member_Connect)
                        return;
            
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguild_Connection_Notice"), recvMsg.CharNick),recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
            
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_GUILD_GREETING, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WChat_GuildGreeting_Link"), recvMsg.CharNick), recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
                }
                break;
            case E_ChatGuildSubType.Join:
                {
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguild_Join_Notice"), recvMsg.CharNick), recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_GUILD_GREETING, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("WChat_GuildGreeting_Link"), recvMsg.CharNick), recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    OnJoinGuildUser?.Invoke(recvMsg.CharId, recvMsg.GuildId, recvMsg.CharNick, (byte)recvMsg.GuildMarkTid);
                }
                break;
            case E_ChatGuildSubType.KickOut:
                {
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_Kick_Notice"), recvMsg.CharNick), recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    if (Me.CurCharData.ID == recvMsg.CharId)
                    {
                        Me.CurCharData.ClearGuildInfo();
                        ZWebManager.Instance.WebChat.CheckEnterChannel();
                    }

                    //if (recvMsg.CharNick == NetData.CurrentCharacter.Nick)
                    //{
                    //    NetData.Instance.ClearGuildBuffList(NetData.UserID, NetData.CharID);
                    //    NetData.Instance.ClearGuildData(NetData.UserID, NetData.CharID);

                    //    UIManager.instance.HidePanel("GuildPanel");

                    //    ZNetGame.GetGuildInfo(recvMsg.GuildId, null);//서버 버프 갱신 용
                    //}

                    OnLeaveGuildUser?.Invoke(recvMsg.CharId, recvMsg.GuildId);
                }
                break;
            case E_ChatGuildSubType.Out:
                {
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_WithDraw_Notice"), recvMsg.CharNick), recvMsg.CharNick, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    //if (recvMsg.CharNick == NetData.CurrentCharacter.Nick)
                    //{
                    //    NetData.Instance.ClearGuildBuffList(NetData.UserID, NetData.CharID);
                    //    NetData.Instance.ClearGuildData(NetData.UserID, NetData.CharID);

                    //    UIManager.instance.HidePanel("GuildPanel");
                    //}

                    if (Me.CurCharData.ID == recvMsg.CharId)
                    {
                        Me.CurCharData.ClearGuildInfo();
                    }

                    OnLeaveGuildUser?.Invoke(recvMsg.CharId, recvMsg.GuildId);
                }
                break;
            //case E_ChatGuildSubType.LevelUp:
            //    {
            //        //addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_LevelUp_Notice"), recvMsg.Args(0)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //        //AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
            //
            //        // Args[0] = GuildExp
            //        // Args[1] = GuildLevel
            //        NetData.Instance.UpdateGuildExp(NetData.UserID, NetData.CharID, recvMsg.GuildId, ulong.Parse(recvMsg.Args(0)), uint.Parse(recvMsg.Args(1)));
            //    }
            //    break;
            //case E_ChatGuildSubType.BuyBuff:
            //    {
            //        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_Buffbuy_Notice"), DBLocale.GetLocaleText(DBGuildBuff.GetGuildBuff(uint.Parse(recvMsg.Args(0))).GuildBuffTextID)), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
            //
            //        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
            //
            //        NetData.Instance.AddGuildBuff(NetData.UserID, NetData.CharID, uint.Parse(recvMsg.Args(1)), ulong.Parse(recvMsg.Args(2)), int.Parse(recvMsg.Args(3)) == 1);
            //    }
            //    break;
            case E_ChatGuildSubType.MarkChange:
                {
                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_MarkChange_Notice")), "None", recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    if (recvMsg.GuildId == Me.CurCharData.GuildId)
                    {
                        Me.CurCharData.SetGuildMarkInfo((byte)recvMsg.GuildMarkTid);
                    }

                    // NetData.Instance.UpdateGuildMark(NetData.UserID, NetData.CharID, recvMsg.GuildId, (byte)recvMsg.GuildMarkTid, TimeManager.NowSec, NetData.CurrentCharacter.myGuildData != null ? NetData.CurrentCharacter.myGuildData.Money : 0);
                    OnGuildMarkChange?.Invoke(recvMsg.GuildId, (byte)recvMsg.GuildMarkTid);
                }
                break;
			//case E_ChatGuildSubType.NewMaster:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_MasterChange_Notice"), recvMsg.CharNick), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//
			//        if (NetData.CurrentCharacter.CharId == recvMsg.CharId)
			//        {
			//            NetData.CurrentCharacter.GuildGrade = E_GuildMemberGrade.Master;
			//        }
			//    }
			//    break;
			//case E_ChatGuildSubType.NewSubMaster:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_SubMasterChange_Notice"), recvMsg.CharNick), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//
			//        if (NetData.CurrentCharacter.CharId == recvMsg.CharId)
			//        {
			//            NetData.CurrentCharacter.GuildGrade = E_GuildMemberGrade.SubMaster;
			//        }
			//    }
			//    break;
			//case E_ChatGuildSubType.NewElite:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId,
			//            recvMsg.CharNick,
			//            string.Format(DBLocale.GetLocaleText("Guild_Named_HonorMember_Notice"), recvMsg.CharNick),
			//            recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        if (NetData.CurrentCharacter.CharId == recvMsg.CharId)
			//        {
			//            NetData.CurrentCharacter.GuildGrade = E_GuildMemberGrade.Elite;
			//        }
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//    }
			//    break;
			//case E_ChatGuildSubType.DisElite:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId,
			//            recvMsg.CharNick,
			//            string.Format(DBLocale.GetLocaleText("Guild_Dismiss_HonorMember_Notice"), recvMsg.CharNick),
			//            recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        if (NetData.CurrentCharacter.CharId == recvMsg.CharId)
			//        {
			//            NetData.CurrentCharacter.GuildGrade = E_GuildMemberGrade.Normal;
			//        }
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//    }
			//    break;
			//
			//case E_ChatGuildSubType.DisSubMaster:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_SubMasterDismiss_Notice"), recvMsg.CharNick), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        if (NetData.CurrentCharacter.CharId == recvMsg.CharId)
			//        {
			//            NetData.CurrentCharacter.GuildGrade = E_GuildMemberGrade.Normal;
			//        }
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//    }
			//    break;
			case E_ChatGuildSubType.ReleaseEnemy:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_EnemyClear_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    Me.CurCharData.RemoveEnemyGuild(ulong.Parse(recvMsg.Args(1)));

                    UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                }
                break;
			case E_ChatGuildSubType.ReleaseAlliance:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_AlliClear_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    Me.CurCharData.RemoveAllianceGuild(ulong.Parse(recvMsg.Args(1)));

                    bool refreshed = UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);

                    if (Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster)
                    {    
                        if (refreshed == false)
                        {
                            if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                                _menu.ActiveRedDot(E_HUDMenu.Guild, true);
                        }
                    }
                }
                break;
			case E_ChatGuildSubType.SetAlliance:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_Alli_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    Me.CurCharData.AddAllianceGuild(ulong.Parse(recvMsg.Args(1)));

                    bool refreshed = UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);

                    if (Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster)
                    {
                        if(refreshed == false)
						{
                            if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                                _menu.ActiveRedDot(E_HUDMenu.Guild, true);
                        }
					}
                }
				break;
			case E_ChatGuildSubType.SetEnemy:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_Enemy_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    Me.CurCharData.AddEnemyGuild(ulong.Parse(recvMsg.Args(1)));

                    UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                }
				break;
			case E_ChatGuildSubType.GuildDungeonOpen:
				{
                    string stageName = string.Empty;

                    if(DBStage.TryGet(uint.Parse(recvMsg.Args(0)), out Stage_Table stage))
					{
                        stageName = DBLocale.GetText(stage.StageTextID);
					}

                    addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_DungeonOpen_Notice"), DBLocale.GetText(stageName)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    ZGameModeManager.Instance.mEventGuildDungeonStateChange?.Invoke();
				}
				break;
            case E_ChatGuildSubType.GuildDungeonFinish:
				{
                    //string stageName = string.Empty;

                    //if (DBStage.TryGet(uint.Parse(recvMsg.Args(0)), out Stage_Table stage))
                    //{
                    //    stageName = DBLocale.GetText(stage.StageTextID);
                    //}

                    //addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_DungeonOpen_Notice"), DBLocale.GetText(stageName)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    //AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

                    ZGameModeManager.Instance.mEventGuildDungeonStateChange?.Invoke();
                }
                break;
            case E_ChatGuildSubType.GuildDungeonClose:
				{
                    ZGameModeManager.Instance.mEventGuildDungeonStateChange?.Invoke();
                }
                break;
			//case E_ChatGuildSubType.GuildDungeonReward:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetLocaleText("Wguid_DungeonResult_Notice"), DBLocale.GetStageName(uint.Parse(recvMsg.Args(0))), DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(1))), int.Parse(recvMsg.Args(2)) == 1 ? "성공" : "실패"), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//    }
			//    break;
			case E_ChatGuildSubType.RequestJoin:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_JoinRequest_Notice"), recvMsg.CharNick), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

					if (Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster)
					{
                        ///TODO: RedDot
                        //AlramUI.AddNoti(NetData.CharID, "GUILD_JOIN");
                        //AlramUI.CheckAlram(UIDefine.Alram.GUILD_JOIN);

                        /// 길드창 열려있으면 내부에서 레드닷 처리되게끔 처리 
                        if (UIManager.Instance.Find<UIFrameGuild>(out var frame) && frame.Show)
                        {
                            frame.NotifyUpdateEvent(UpdateEventType.RequestGuildJoinRequests);
						}
                        /// 안열려있으면 메인메뉴 레드닷 On 
						else
						{
                            if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                                _menu.ActiveRedDot(E_HUDMenu.Guild, true);
                        }
                    }
				}
				break;
			case E_ChatGuildSubType.RequestAlliance:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_AlliRequest_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

					if (Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster)
					{
                        /// TODO:RedDot
                        //AlramUI.AddNoti(NetData.CharID, "GUILD_ALLIANCE");
                        //AlramUI.CheckAlram(UIDefine.Alram.GUILD_ALLIANCE);
                        bool refreshed = UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                        if(refreshed == false)
						{
                            if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                                _menu.ActiveRedDot(E_HUDMenu.Guild, true);
                        }
                    }
				}
				break;
			case E_ChatGuildSubType.InviteAllianceChat:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_UnionRequest_Notice"), recvMsg.Args(0)), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);

					if (Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master || Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster)
					{
                        ///TODO: RedDot
                        //AlramUI.AddNoti(NetData.CharID, "GUILD_ALLIANCE_CHAT");
                        //AlramUI.CheckAlram(UIDefine.Alram.GUILD_ALLIANCE_CHAT);
                        bool refreshed =UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
						if (refreshed == false)
						{
							if (UIManager.Instance.Find(out UISubHUDMenu _menu))
                                _menu.ActiveRedDot(E_HUDMenu.Guild, true);
                        }
                    }
				}
				break;
			//case E_ChatGuildSubType.ChangeName:
			//    {
			//        if (NetData.CurrentCharacter.GuildId == recvMsg.GuildId)
			//        {
			//            NetData.Instance.UpdateGuildData(NetData.UserID, NetData.CharID, recvMsg.GuildId, recvMsg.GuildName, NetData.CurrentCharacter.GuildMarkTid);
			//            UIManager.NoticeMessage("길드명이 변경되었습니다.");
			//        }
			//    }
			//    break;
			//case E_ChatGuildSubType.BossReward:
			//    {
			//        addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick,
			//            string.Format(DBLocale.GetLocaleText("Wguid_DungeonResult_BossReward"), recvMsg.Args(2), DBLocale.GetStageName(uint.Parse(recvMsg.Args(0))),
			//            DBLocale.GetMonsterName(uint.Parse(recvMsg.Args(1)))), recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
			//
			//        AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
			//
			//        ZNetGame.GetMailRefreshTime();
			//    }
			//    break;
			case E_ChatGuildSubType.None:
            case E_ChatGuildSubType.Chat:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_GUILD_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_GUILD, addChatData);
                }
                break;
        }
    }

    public static void AddChatMsg(BroadcastAllianceChat recvMsg)
    {
        ChatData addChatData = null;
        switch (recvMsg.SubType)
        {
			case E_ChatAllianceSubType.Join:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_Union_Notice"), recvMsg.GuildName), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
					AddChatMsg(ChatFilter.TYPE_ALLIANCE, addChatData);

                    UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                }
				break;
			case E_ChatAllianceSubType.Out:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_UnionClear_Notice"), recvMsg.GuildName), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);
					AddChatMsg(ChatFilter.TYPE_ALLIANCE, addChatData);
					if (Me.CurCharData.GuildId == recvMsg.GuildId)
					{
                        Me.CurCharData.GuildChatState = E_GuildAllianceChatState.None;
						Me.CurCharData.GuildChatGrade = E_GuildAllianceChatGrade.None;
                        Me.CurCharData.GuildChatId = 0;
                        //ZNetChat.UpdateGuildAlliance();

                        if (UIManager.Instance.Find<UIFrameChatting>(out var frame))
                        {
                            if (frame.Show)
                                UIManager.Instance.Close<UIFrameChatting>();
                        }

                        UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                    }
				}
				break;
			case E_ChatAllianceSubType.Ban:
				{
					addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format(DBLocale.GetText("Wguid_UnionKick_Notice"), recvMsg.GuildName), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

					AddChatMsg(ChatFilter.TYPE_ALLIANCE, addChatData);

					if (Me.CurCharData.GuildId == recvMsg.GuildId) // && Me.CurCharData.myGuildData != null)
					{
                        Me.CurCharData.GuildChatState = E_GuildAllianceChatState.None;
                        Me.CurCharData.GuildChatGrade = E_GuildAllianceChatGrade.None;
                        Me.CurCharData.GuildChatId = 0;

                        if(UIManager.Instance.Find<UIFrameChatting>(out var frame))
						{
                            if (frame.Show)
                                UIManager.Instance.Close<UIFrameChatting>();
                        }

                        //ZNetChat.UpdateGuildAlliance();

                        //var allianceSetPopup = UIManager.instance.GetActiveUI<GuildAllianceChatSetPopup>();
                        //if (allianceSetPopup != null)
                        //{
                        //	allianceSetPopup.RefreshDatas();
                        //}

                        UIFrameGuild.RequestRefreshAllianceInfo_ViaBroadcast(recvMsg.CharId);
                    }
				}
				break;
			case E_ChatAllianceSubType.None:
            case E_ChatAllianceSubType.Chat:
                {
                    if (CheckBlock(recvMsg.CharId))
                        return;
                    if (CheckEmoticon(recvMsg.CharId, recvMsg.Message))
                        return;

                    addChatData = new ChatData(ChatViewType.TYPE_ALLIANCE_CHAT, recvMsg.ServerIdx, recvMsg.CharId, recvMsg.CharNick, string.Format("[{0}] {1}", recvMsg.CharNick, recvMsg.Message), recvMsg.Message, recvMsg.GuildId, recvMsg.GuildMarkTid, recvMsg.GuildName);

                    AddChatMsg(ChatFilter.TYPE_ALLIANCE, addChatData);
                }
                break;
        }
    }

    //client add
    public static void AddSystemMsg(List<string> Msgs)
    {
        foreach (string Msg in Msgs)
        {
            AddSystemMsg(Msg);
        }
    }

    public static void AddSystemMsg(string msg)
    {
        ChatData addChatData = new ChatData(ChatViewType.TYPE_SYSTEM_CHAT, 0, 0, "", msg, msg, 0, 0, "");

        AddChatMsg(ChatFilter.TYPE_SYSTEM, addChatData);
    }

    //client add
    public static void AddChatMsg(ChatViewType chatViewType, ChatFilter chatType, ulong charId, string sendernick, string msg, ulong GuildId, uint GuildMarkTid, string GuildName)
    {
        if (CheckEmoticon(charId, msg))
            return;

        ChatData addChatData = null;
        switch (chatViewType)
        {
            case ChatViewType.TYPE_SERVER_CHAT:
            case ChatViewType.TYPE_NORMAL_CHAT:
            case ChatViewType.TYPE_GUILD_CHAT:
            case ChatViewType.TYPE_PARTY_CHAT:
            case ChatViewType.TYPE_ALLIANCE_CHAT:
            case ChatViewType.TYPE_TRADE_CHAT:
                addChatData = new ChatData(chatViewType, Me.SelectedServerID, charId, sendernick, string.Format("[{0}] {1}", sendernick, msg), msg, GuildId, GuildMarkTid, GuildName);
                break;
            case ChatViewType.TYPE_WHISPER_SEND_CHAT:
                addChatData = new ChatData(chatViewType, Me.SelectedServerID, charId, sendernick, string.Format(">>[{0}] {1}", sendernick, msg), msg, GuildId, GuildMarkTid, GuildName);
                break;
        }

        foreach (ChatFilter type in EnumHelper.Values<ChatFilter>())
        {
            if (chatType.HasFlag(type))
                AddChatMsg(type, addChatData);
        }
    }

    #region Action

    // MMO => 아이템 획득
    public static void OnRemainItemInfos(S2C_DropItemInfos remainItemInfos)
    {
        List<string> msgs = new List<string>();

		int itemCount = remainItemInfos.ItemsLength;
		for (int i = 0; i < itemCount; i++)
        {
            DropItem item = remainItemInfos.Items(i).Value;

            // 골드 출력ㄴㄴ~
            //[박윤성] 자동판매로 골드를 줄때는 예외
            if (DBConfig.Gold_ID == item.ItemTid && item.SellItemTid == 0)
                continue;

            if (DBItem.GetItem(item.ItemTid, out Item_Table table) == false)
                continue;

            ZLog.Log(ZLogChannel.UI, $"Name : {DBLocale.GetItemLocale(table)}, BreakItemTid : {item.BreakItemTid}, SellIteTid : {item.SellItemTid}, ItemTid : {item.ItemTid}");

            string msg = string.Empty;

            //ZLog.Log(ZLogChannel.UI, "ItemStack Type : " + table.ItemStackType.ToString());
            switch (table.ItemStackType)
            {
                
                case E_ItemStackType.Not:
                    if (item.BreakItemTid > 0)
                    {
                        //msg = $"{DBLocale.GetItemLocale(table)}을 분해하였습니다.";
                        DBItem.GetItem(item.BreakItemTid, out Item_Table breakItemTable);
                        msg = string.Format(DBLocale.GetText("Decomposition_Acquisition"), DBLocale.GetItemLocale(breakItemTable), item.Cnt);
                    }
                    else if (item.SellItemTid > 0)
                    {
                        //msg = $"{DBLocale.GetItemLocale(table)}을 판매하였습니다.";
                        DBItem.GetItem(item.SellItemTid, out Item_Table sellItemTable);
                        msg = string.Format(DBLocale.GetText("Sales_Acquisition"), DBLocale.GetItemLocale(sellItemTable), item.Cnt);
                    }
                    else
                    {
                        msg = string.Format(DBLocale.GetText("System_Notice_Gain_Item"), DBLocale.GetItemLocale(table));
                    }
                    break;
                case E_ItemStackType.Stack:
                case E_ItemStackType.AccountStack:
                    if (item.BreakItemTid > 0)
                    {
                        //msg = $"{DBLocale.GetItemLocale(table)}({item.Cnt}) 분해하였습니다.";
                        DBItem.GetItem(item.BreakItemTid, out Item_Table breakItemTable);
                        msg = string.Format(DBLocale.GetText("Decomposition_Acquisition"), DBLocale.GetItemLocale(breakItemTable), item.Cnt);
                    }
                    else if (item.SellItemTid > 0)
                    {
                        //msg = $"{DBLocale.GetItemLocale(table)}({item.Cnt}) 판매하였습니다.";
                        DBItem.GetItem(item.SellItemTid, out Item_Table sellItemTable);
                        msg = string.Format(DBLocale.GetText("Sales_Acquisition"), DBLocale.GetItemLocale(sellItemTable), item.Cnt);
                    }
                    else
                    {
                        msg = string.Format(DBLocale.GetText("System_Notice_Gain_Stack"), DBLocale.GetItemLocale(table), item.Cnt.ToString());
                    }
                    break;
                    
                default:
                    continue;
            }

            var entityData = ZPawnManager.Instance.GetEntityData(item.Objectid);
            if (null != entityData)
            {
                if (entityData.EntityId != ZPawnManager.Instance.MyEntityId)
                    msg = string.Format(DBLocale.GetText("Party_GainItem_Player"), entityData.Name, msg);
            }

            
         
            msgs.Add(msg);
        }

        AddSystemMsg(msgs);
    }
    //public static void OnKillMonster(ResMonsterKill recvMsgPacket)
    //{
    //    List<string> msgs = new List<string>();

    //    if (recvMsgPacket.BreakDownItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.BreakDownItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 분해되었습니다.", DBItem.GetItemFullName(recvMsgPacket.BreakDownItemTids(i))));
    //    }

    //    if (recvMsgPacket.SellRuneItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.SellRuneItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 판매되었습니다.", DBItem.GetItemFullName(recvMsgPacket.SellRuneItemTids(i))));
    //    }

    //    //msgs.Add(string.Format("경험치를 {0} 획득하였습니다.", recvMsgPacket.GetCharExp));

    //    if (recvMsgPacket.GetEquipsLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetEquipsLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetEquips(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetRunesLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetRunesLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetRunes(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetStacksLength; j++)
    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetStacks(j).Value.ItemTid), recvMsgPacket.GetStacks(j).Value.Cnt));
    //    }

    //    if (recvMsgPacket.GetAccountStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetAccountStacksLength; j++)
    //        {
    //            if (recvMsgPacket.GetAccountStacks(j).Value.ItemTid == DBConfig.Gold_ID)
    //                continue;

    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetAccountStacks(j).Value.ItemTid), recvMsgPacket.GetAccountStacks(j).Value.Cnt));
    //        }
    //    }

    //    /*if (recvMsgPacket.GetCharExp > 0)
    //    {
    //        msgs.Add(string.Format("경험치({0}) 획득하였습니다.", recvMsgPacket.GetCharExp));
    //    }*/

    //    AddSystemMsg(msgs);
    //}

    //public static void OnKillMonsterReward(ResMonsterKillReward recvMsgPacket)
    //{
    //    List<string> msgs = new List<string>();

    //    if (recvMsgPacket.BreakDownItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.BreakDownItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 분해되었습니다.", DBItem.GetItemFullName(recvMsgPacket.BreakDownItemTids(i))));
    //    }

    //    if (recvMsgPacket.SellRuneItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.SellRuneItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 판매되었습니다.", DBItem.GetItemFullName(recvMsgPacket.SellRuneItemTids(i))));
    //    }

    //    if (recvMsgPacket.GetEquipsLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetEquipsLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetEquips(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetRunesLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetRunesLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetRunes(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetStacksLength; j++)
    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetStacks(j).Value.ItemTid), recvMsgPacket.GetStacks(j).Value.Cnt));
    //    }

    //    if (recvMsgPacket.GetAccountStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetAccountStacksLength; j++)
    //        {
    //            if (recvMsgPacket.GetAccountStacks(j).Value.ItemTid == DBConfig.Gold_ID)
    //                continue;

    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetAccountStacks(j).Value.ItemTid), recvMsgPacket.GetAccountStacks(j).Value.Cnt));
    //        }
    //    }

    //    /*if (recvMsgPacket.GetCharExp > 0)
    //    {
    //        msgs.Add(string.Format("경험치({0}) 획득하였습니다.", recvMsgPacket.GetCharExp));
    //    }*/

    //    AddSystemMsg(msgs);
    //}

    //public static void OnKillMonsterReward(ResRaidReward recvMsgPacket)
    //{
    //    List<string> msgs = new List<string>();

    //    if (recvMsgPacket.BreakDownItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.BreakDownItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 분해되었습니다.", DBItem.GetItemFullName(recvMsgPacket.BreakDownItemTids(i))));
    //    }

    //    if (recvMsgPacket.SellRuneItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.SellRuneItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 판매되었습니다.", DBItem.GetItemFullName(recvMsgPacket.SellRuneItemTids(i))));
    //    }

    //    if (recvMsgPacket.GetEquipsLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetEquipsLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetEquips(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetRunesLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetRunesLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetRunes(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetStacksLength; j++)
    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetStacks(j).Value.ItemTid), recvMsgPacket.GetStacks(j).Value.Cnt));
    //    }

    //    if (recvMsgPacket.GetAccountStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetAccountStacksLength; j++)
    //        {
    //            if (recvMsgPacket.GetAccountStacks(j).Value.ItemTid == DBConfig.Gold_ID)
    //                continue;

    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetAccountStacks(j).Value.ItemTid), recvMsgPacket.GetAccountStacks(j).Value.Cnt));
    //        }
    //    }

    //    /*if (recvMsgPacket.GetCharExp > 0)
    //    {
    //        msgs.Add(string.Format("경험치({0}) 획득하였습니다.", recvMsgPacket.GetCharExp));
    //    }*/

    //    AddSystemMsg(msgs);
    //}

    //public static void OnKillMonsterReward(ResRaidRoomReward recvMsgPacket)
    //{
    //    List<string> msgs = new List<string>();

    //    if (recvMsgPacket.BreakDownItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.BreakDownItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 분해되었습니다.", DBItem.GetItemFullName(recvMsgPacket.BreakDownItemTids(i))));
    //    }

    //    if (recvMsgPacket.SellRuneItemTidsLength > 0)
    //    {
    //        for (int i = 0; i < recvMsgPacket.SellRuneItemTidsLength; i++)
    //            msgs.Add(string.Format("{0} 판매되었습니다.", DBItem.GetItemFullName(recvMsgPacket.SellRuneItemTids(i))));
    //    }

    //    if (recvMsgPacket.GetEquipsLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetEquipsLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetEquips(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetRunesLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetRunesLength; j++)
    //            msgs.Add(string.Format("{0} 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetRunes(j).Value.ItemTid)));
    //    }

    //    if (recvMsgPacket.GetStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetStacksLength; j++)
    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetStacks(j).Value.ItemTid), recvMsgPacket.GetStacks(j).Value.Cnt));
    //    }

    //    if (recvMsgPacket.GetAccountStacksLength > 0)
    //    {
    //        for (int j = 0; j < recvMsgPacket.GetAccountStacksLength; j++)
    //        {
    //            if (recvMsgPacket.GetAccountStacks(j).Value.ItemTid == DBConfig.Gold_ID)
    //                continue;

    //            msgs.Add(string.Format("{0}({1}) 획득하였습니다.", DBItem.GetItemFullName(recvMsgPacket.GetAccountStacks(j).Value.ItemTid), recvMsgPacket.GetAccountStacks(j).Value.Cnt));
    //        }
    //    }

    //    /*if (recvMsgPacket.GetCharExp > 0)
    //    {
    //        msgs.Add(string.Format("경험치({0}) 획득하였습니다.", recvMsgPacket.GetCharExp));
    //    }*/

    //    AddSystemMsg(msgs);
    //}
    #endregion

    #region Delegate
    public static event System.Action<ChatFilter, ChatData> OnAddMsg;
    public static event System.Action OnClearMsg;
    public static System.Action<ChatFilter> OnSelectChannel;
    public static Dictionary<ulong, System.Action<ulong, ChatData>> OnChatMsg = new Dictionary<ulong, System.Action<ulong, ChatData>>();
    public static Dictionary<ulong, System.Action<ulong, string>> OnEmoticonMsg = new Dictionary<ulong, System.Action<ulong, string>>();

    public static void AddOnChatMsg(ulong CharId, System.Action<ulong, ChatData> _OnChatMsg)
    {
        if (!OnChatMsg.ContainsKey(CharId))
            OnChatMsg.Add(CharId, null);

        OnChatMsg[CharId] += _OnChatMsg;
    }

    public static void RemoveOnChatMsg(ulong CharId, System.Action<ulong, ChatData> _OnChatMsg)
    {
        if (OnChatMsg.ContainsKey(CharId))
            OnChatMsg[CharId] -= _OnChatMsg;
    }

    public static void AddOnEmoticonMsg(ulong CharId, System.Action<ulong, string> _OnEmoticonMsg)
    {
        if (!OnEmoticonMsg.ContainsKey(CharId))
            OnEmoticonMsg.Add(CharId, null);

        OnEmoticonMsg[CharId] += _OnEmoticonMsg;
    }

    public static void RemoveOnEmoticonMsg(ulong CharId, System.Action<ulong, string> _OnEmoticonMsg)
    {
        if (OnEmoticonMsg.ContainsKey(CharId))
            OnEmoticonMsg[CharId] -= _OnEmoticonMsg;
    }
    #endregion

    #region Enter
    static List<ChatEnterData> ChatEnterInfoContainner = new List<ChatEnterData>();
    public static void ClearEnterInfos()
    {
        ChatEnterInfoContainner.Clear();
    }

    public static bool IsEnter(E_ChatType chatType)
    {
        return ChatEnterInfoContainner.Find(item => item.chatType == chatType && item.Args.Count > 0) != null;
    }

    public static ChatEnterData GetEnterInfo(E_ChatType chatType)
    {
        return ChatEnterInfoContainner.Find(item => item.chatType == chatType);
    }

    public static ChatEnterData AddEnterInfo(ChatInfo _chatInfo)
    {
        var chatInfo = ChatEnterInfoContainner.Find(item => item.chatType == _chatInfo.ChatType);

        if (chatInfo == null)
        {
            ChatEnterInfoContainner.Add(chatInfo = new ChatEnterData(_chatInfo));
        }
        else
            chatInfo.Reset(_chatInfo);

        return chatInfo;
    }

    public static ChatEnterData AddEnterInfo(E_ChatType chatType, params string[] args)
    {
        var chatInfo = ChatEnterInfoContainner.Find(item => item.chatType == chatType);

        if (chatInfo == null)
        {
            ChatEnterInfoContainner.Add(chatInfo = new ChatEnterData(chatType, args));
        }

        return chatInfo;
    }

    public static void RemoveEnterInfo(E_ChatType chatType)
    {
        var chatInfo = ChatEnterInfoContainner.Find(item => item.chatType == chatType);

        if (chatInfo != null)
        {
            ChatEnterInfoContainner.Remove(chatInfo);
        }
    }

    public static List<ChatEnterData> GetEnterList()
    {
        return ChatEnterInfoContainner;
    }
    #endregion

    #region Util
    public static Color GetChatViewColor(ChatViewType chatViewType)
    {
        switch (chatViewType)
        {
            case ChatViewType.TYPE_SERVER_CHAT:
                return ResourceSetManager.Palette.Chat_Server;
            case ChatViewType.TYPE_NORMAL_CHAT:
                return ResourceSetManager.Palette.Chat_Normal;
            case ChatViewType.TYPE_WHISPER_RECV_CHAT:
                return ResourceSetManager.Palette.Chat_Whisper_Recv;
            case ChatViewType.TYPE_WHISPER_SEND_CHAT:
                return ResourceSetManager.Palette.Chat_Whisper_Send;
            case ChatViewType.TYPE_PARTY_CHAT:
                return ResourceSetManager.Palette.Chat_Party;
            case ChatViewType.TYPE_GUILD_CHAT:
                return ResourceSetManager.Palette.Chat_Guild;
            case ChatViewType.TYPE_SYSTEM_GUILD_GREETING:
                return ResourceSetManager.Palette.Chat_Guild;
            case ChatViewType.TYPE_ALLIANCE_CHAT:
                return ResourceSetManager.Palette.Chat_Alliance;
            case ChatViewType.TYPE_NOTICE_CHAT:
                return ResourceSetManager.Palette.Chat_Notice;
            case ChatViewType.TYPE_NOTICE_WARNING_CHAT:
                return ResourceSetManager.Palette.Chat_Warnning;
            case ChatViewType.TYPE_NOTICE_EVENT_CHAT:
                return ResourceSetManager.Palette.Chat_Event;
            case ChatViewType.TYPE_GET_ITEM_CHAT:
                return ResourceSetManager.Palette.Chat_GetItem;
            case ChatViewType.TYPE_SYSTEM_CHAT:
                return ResourceSetManager.Palette.Chat_System;
            case ChatViewType.TYPE_TRADE_CHAT:
                return ResourceSetManager.Palette.Chat_Trade;
        }

        return Color.white;
    }
    #endregion
}
