using System;
using System.Collections.Generic;
using WebNet;
using ZNet;
using ZNet.Data;

public enum CheckGuildCreateChatRoomResult
{ 
    Positive,
    Denied_AlreadyJoined,
    Denied_Requested,
    Denied_Invited
}

/// <summary>
/// 레드닷 출력 여부 상태값입니다.
/// </summary>
public enum GuildRedDotStatusFlag
{
    None = 0,
    GuildInfo_ReceivedGuildJoinRequest = 0x1,
    GuildInfo_AllianceTab = 0x1 << 1,
    GuildInfo_AllianceChatTab = 0x1 << 2, 
}

/*
 * 길드 통신 관련 중간 다리 역할 클래스 
 * CharacterData에 보관할 필요없는 길드 멤버 데이터같은 것들을 주로 보관함. 
 * */
public partial class UIFrameGuildNetCapturer
{
    #region Data Define
    #region Custom
    // 내 길드 관련 정보 . ChracterData 에 속하지않는 것으로 판단되는것들 모음 
    public class MyGuildInfo
    {
        public GuildInfoConverted myGuildInfo;

        // 나의 길드 멤버들 
        public List<GuildMemberInfoConverted> members = new List<GuildMemberInfoConverted>();
        public Dictionary<E_GuildAllianceState, List<GuildAllianceInfoConverted>> myGuildAllianceInfo = new Dictionary<E_GuildAllianceState, List<GuildAllianceInfoConverted>>();
        public List<GuildRequestListForGuildConverted> receivedJoinRequestList = new List<GuildRequestListForGuildConverted>();

        public GuildMemberInfoConverted MyMemberInfo
        {
            get
            {
                foreach (var t in members)
                {
                    // 나 발견 
                    if (t.charID == Me.CurCharData.ID)
                    {
                        return t;
                    }
                }

                return null;
            }
        }

        public void SetGuildInfo(GuildInfo guildInfo)
        {
            myGuildInfo = new GuildInfoConverted(guildInfo);
        }
        public void ClearAll()
        {
            if (myGuildInfo != null)
                myGuildInfo.Reset();

            ClearMemberInfo();
            ClearAllianceInfo();
            ClearReceivedJoinRequestList();
        }
        public void ClearMemberInfo()
        {
            members.Clear();
        }
        public void ClearAllianceInfo(E_GuildAllianceState state)
        {
            if (myGuildAllianceInfo.ContainsKey(state))
                myGuildAllianceInfo[state].Clear();
        }
        public void ClearAllianceInfo()
        {
            foreach (var t in myGuildAllianceInfo)
            {
                t.Value.Clear();
            }
        }
        public void SetAllianceRequestChat(GuildInfo masterGuildInfo)
		{
			/// 사실 Alliance Key 가 없는건 굉장히 희박한 확률로 발생 가능할수도 있음 .
			/// 대부분의 경우에는 이미 연맹 채팅 신청한 Master 길드가 연맹 리스트에 존재할것임 .
			if (myGuildAllianceInfo.ContainsKey(E_GuildAllianceState.Alliance) == false)
			{
				return;
			}

			var allianceList = myGuildAllianceInfo[E_GuildAllianceState.Alliance];
			var targetGuild = allianceList.Find(t => t.guild_id == masterGuildInfo.GuildId);

			if (targetGuild != null)
			{
                targetGuild.chat_state = E_GuildAllianceChatState.Request;
			}
		}

        public void ForeachAlliance(Action<E_GuildAllianceState, GuildAllianceInfoConverted> action)
        {
            foreach (var byType in myGuildAllianceInfo)
            {
                foreach (var info in byType.Value)
                {
                    action(byType.Key, info);
                }
            }
        }

        public GuildAllianceInfoConverted FindAlliance(Predicate<GuildAllianceInfoConverted> predicate)
        {
            foreach (var byType in myGuildAllianceInfo)
            {
                foreach (var info in byType.Value)
                {
                    if(predicate(info))
					{
                        return info;
					}
                }
            }

            return null;
        }

		public bool IsReceivedRequestAllianceExist()
		{
			foreach (var byType in myGuildAllianceInfo)
			{
				foreach (var info in byType.Value)
				{
					/// 내가 받은 연맹 요청이 존재하는지 체킹 
					if (info.state == E_GuildAllianceState.ReceiveAlliance)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool IsReceivedRequestAllianceChatExist()
		{
            /// 참여 초대를 받은 경우 
            if (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Receive)
                return true;

            /// 내 길드가 채팅방에서 마스터 길드면 
            if (IsMyGuildChatRoomMaster)
            {
                foreach (var byType in myGuildAllianceInfo)
                {
                    foreach (var info in byType.Value)
                    {
                        /// 참여 요청을 받음  
                        if (Me.CurCharData.GuildChatId == info.chat_id
                            && info.chat_state == E_GuildAllianceChatState.Request)
                        {
                            return true;
                        }
                    }
                }
            }

            return false; 
        }

        public bool IsReceivedGuildJoinRequestExist()
		{
            return receivedJoinRequestList.Count > 0;
        }

        public void ClearReceivedJoinRequestList()
        {
            receivedJoinRequestList.Clear();
        }
        public void SetMemberInfo(List<GuildMemberInfo> memberInfo)
        {
            ClearMemberInfo();
            members.Capacity = memberInfo.Count;
            for (int i = 0; i < memberInfo.Count; i++)
            {
                members.Add(new GuildMemberInfoConverted(memberInfo[i]));
            }
        }
        public void AddMemberInfo(GuildMemberInfo memberInfo)
        {
            members.Add(new GuildMemberInfoConverted(memberInfo));
        }
        public void SortMemberListByDisplayOrder()
        {
            members.Sort((t01, t02) =>
            {
                if (t01.isLogin && t02.isLogin == false)
                {
                    return -1;
                }
                else if (t01.isLogin == false && t02.isLogin)
                {
                    return 1;
                }

                return t02.grade.CompareTo(t01.grade);
            });
        }
        //public void SetAllianceInfo(E_GuildAllianceState[] states, List<GuildAllianceInfo> info)
        //{
        //    for (int i = 0; i < states.Length; i++)
        //    {
        //        if (myGuildAllianceInfo.ContainsKey(states[i]) == false)
        //        {
        //            myGuildAllianceInfo.Add(states[i], new List<GuildAllianceInfoConverted>());
        //        }

        //        myGuildAllianceInfo[states[i]].Clear();
        //    }

        //    for (int i = 0; i < info.Count; i++)
        //    {
        //        myGuildAllianceInfo[info[i].State].Add(new GuildAllianceInfoConverted(info[i]));
        //    }
        //}

        public void AddAllianceInfo(GuildAllianceInfo info)
        {
            if (myGuildAllianceInfo.ContainsKey(info.State) == false)
                myGuildAllianceInfo.Add(info.State, new List<GuildAllianceInfoConverted>());

            var target = myGuildAllianceInfo[info.State].Find(t => t.guild_id == info.GuildId);
            if (target == null)
            {
                myGuildAllianceInfo[info.State].Add(new GuildAllianceInfoConverted(info));
            }
            else
            {
                target.Set(info);
            }

            TrimOldStateAllianceInfo(info.GuildId, info.State);
        }

		//public void AddAllianceInfo(GuildAllianceInfoConverted info)
  //      {
  //          if (myGuildAllianceInfo.ContainsKey(info.state) == false)
  //              myGuildAllianceInfo.Add(info.state, new List<GuildAllianceInfoConverted>());

  //          myGuildAllianceInfo[info.state].Add(info);
  //      }

        /// <summary>
        /// 연맹 State 가 업데이트되면 이전 State 로 등록된 연맹 정보를 제거해야하므로 
        /// 해당 길드의 최신 상태를 받아서 중복되는 다른 상태의 같은 연맹 정보 제거 
        /// </summary>
        public void TrimOldStateAllianceInfo(ulong guildID, E_GuildAllianceState curState)
		{
			foreach (var alliance in myGuildAllianceInfo)
			{
                /// 최신화된 State 와 다른 Target GuildID 를 가진 연맹이 존재한다 
                /// => 삭제 
                if(alliance.Key != curState)
				{
                    /// 사실 2개이상이 들어갈일은 없지만 , RemoveAll 로 전부 제거 시도.
                    alliance.Value.RemoveAll(t => t.guild_id == guildID);
				}
			}
		}
    }
    #endregion

    #region Server Table Based Converted Data
    // 길드 정보 컨버팅 ( GuildInfo ) 
    public class GuildInfoConverted
    {
        public ulong guildID;

        public byte markTid;
        public string guildName;
        public string masterName;
        public string introduction;
        public string notice;

        public uint rank;
        public ulong masterID;
        public ulong exp;
        public uint level;
        public uint curTotalMemberCount;
        public uint maxMemberCntBySystem;
        public ulong money;

        public ulong chatID;
        public E_GuildAllianceChatState chatState;
        public E_GuildAllianceChatGrade chatGrade;

        public ushort loginBanStep;
        public ushort donateBanStep;

        public ulong markUpdateDt;

        public bool isQuickJoin;

        public GuildInfoConverted(GuildInfo guildInfo)
        {
            this.guildID = guildInfo.GuildId;
            this.notice = guildInfo.Notice;
            this.rank = guildInfo.GuildRank;
            this.markTid = guildInfo.MarkTid;
            this.guildName = guildInfo.Name;
            this.masterID = guildInfo.MasterCharId;
            this.masterName = guildInfo.MasterCharNick;
            this.introduction = guildInfo.Intro;
            this.exp = guildInfo.Exp;
            this.money = guildInfo.Money;
            this.level = DBGuild.GetGuildLevel(guildInfo.Exp);
            this.curTotalMemberCount = guildInfo.MemberCnt;
            this.maxMemberCntBySystem = DBConfig.Guild_Max_Character;
            this.chatID = guildInfo.ChatId;
            this.chatState = guildInfo.ChatState;
            this.chatGrade = guildInfo.ChatGrade;
            this.isQuickJoin = guildInfo.IsQuickJoin;
            this.markUpdateDt = guildInfo.MarkUpdateDt;
            this.loginBanStep = guildInfo.LoginBanStep;
            this.donateBanStep = guildInfo.DonateBanStep;
        }

        public void Reset()
        {
            this.guildID = 0;
            this.notice = string.Empty;
            this.rank = 0;
            this.markTid = 0;
            this.guildName = string.Empty;
            this.masterID = 0;
            this.masterName = string.Empty;
            this.introduction = string.Empty;
            this.exp = 0;
            this.money = 0;
            this.level = 0;
            this.curTotalMemberCount = 0;
            this.maxMemberCntBySystem = 0;
            this.chatID = 0;
            this.chatState = E_GuildAllianceChatState.None;
            this.chatGrade = E_GuildAllianceChatGrade.None;
            this.isQuickJoin = false;
            this.markUpdateDt = 0;
            this.loginBanStep = 0;
            this.donateBanStep = 0;
        }

        // 정보 세팅 
        public void SetInfoUpdated(
            ulong guildID
            , string intro
            , string notice
            , bool isQuickJoin
            , ushort loginBanStep
            , ushort donateBanStep)
        {
            this.guildID = guildID;
            this.introduction = intro;
            this.notice = notice;
            this.isQuickJoin = isQuickJoin;
            this.loginBanStep = loginBanStep;
            this.donateBanStep = donateBanStep;
        }

        /// 마크 세팅
        public void SetMarkInfoUpdated(
            ulong guildID
            , byte markTid
            , ulong markUpdateDt
            , ulong resultGuildMoney)
        {
            this.guildID = guildID;
            this.markTid = markTid;
            this.markUpdateDt = markUpdateDt;
            this.money = resultGuildMoney;
        }

        /// 채팅 정보 세팅
        public void SetChatInfo(ulong chatID, E_GuildAllianceChatState state, E_GuildAllianceChatGrade grade)
		{
            this.chatID = chatID;
            this.chatState = state;
            this.chatGrade = grade;
		}
    }

    // 내가 신청한 길드 정보 컨버팅 ( GuildRequestInfoForChar )
    public class GuildRequestInfoForCharConverted
    {
        public uint level;
        public ulong guildID;
        public ulong exp;
        public string guildName;
        public string guildIntro;
        public bool isQuickJoin;
        public ulong masterCharID;
        public string masterCharNick;
        public uint curMemberCnt;
        public uint maxMemberCnt;
        public byte markTid;

        public GuildRequestInfoForCharConverted(uint level, ulong guildID, ulong exp, string guildName, string guildIntro, bool isQuickJoin, ulong masterCharID, string masterCharNick, uint curMemberCnt, uint maxMemberCnt, byte markTid)
        {
            this.level = level;
            this.guildID = guildID;
            this.exp = exp;
            this.guildName = guildName;
            this.guildIntro = guildIntro;
            this.isQuickJoin = isQuickJoin;
            this.masterCharID = masterCharID;
            this.masterCharNick = masterCharNick;
            this.curMemberCnt = curMemberCnt;
            this.maxMemberCnt = maxMemberCnt;
            this.markTid = markTid;
        }
    }

	// 길드멤버 컨버팅 ( GuildMemberInfo )
	public class GuildMemberInfoConverted
    {
        public ulong charID;
        public uint charTid;
        public string nick;
        public E_GuildMemberGrade grade;
        public ulong exp;
        public ulong weekExp;
        public ulong donateExp;
        public ulong weekDonateExp;
        public E_GuildJoinState state;
        public uint donateCnt;
        /// <summary>
        /// 해당 유저가 마지막으로 출석 보상을 받은 Dt 임 
        /// 즉 출석 인원 카운트를 이 변수로함 
        /// </summary>
        public ulong attendRewardDt;
        public string comment;
        public bool isLogin;
        public ulong logoutDt;

        public GuildMemberInfoConverted(GuildMemberInfo memberInfo)
        {
            this.charID = memberInfo.CharId;
            this.charTid = memberInfo.CharTid;
            this.nick = memberInfo.Nick;
            this.grade = memberInfo.Grade;
            this.exp = memberInfo.Exp;
            this.weekExp = memberInfo.WeekExp;
            this.donateExp = memberInfo.DonateExp;
            this.weekDonateExp = memberInfo.WeekDonateExp;
            this.state = memberInfo.State;
            this.donateCnt = memberInfo.DonateCnt;
            this.attendRewardDt = memberInfo.AttendRewardDt;
            this.comment = memberInfo.Comment;
            this.isLogin = memberInfo.IsLogin;
            this.logoutDt = memberInfo.LogoutDt;
        }

        public GuildMemberInfoConverted(ulong charID, uint charTid, string nick, E_GuildMemberGrade grade, ulong exp, ulong weekExp, ulong donateExp, ulong weekDonateExp, E_GuildJoinState state, uint donateCnt, ulong attendRewardDt, string comment, bool isLogin, ulong logoutDt)
        {
            this.charID = charID;
            this.charTid = charTid;
            this.nick = nick;
            this.grade = grade;
            this.exp = exp;
            this.weekExp = weekExp;
            this.donateExp = donateExp;
            this.weekDonateExp = weekDonateExp;
            this.state = state;
            this.donateCnt = donateCnt;
            this.attendRewardDt = attendRewardDt;
            this.comment = comment;
            this.isLogin = isLogin;
            this.logoutDt = logoutDt;
        }
    }

    // 연맹길드 컨버팅 ( GuildAllianceInfo ) 
    public class GuildAllianceInfoConverted
    {
        public ulong guild_id;
        public string name;
        public ulong master_char_id;
        public string master_char_nick;
        public ulong exp;
        public uint member_cnt;
        public byte mark_tid;
        public ulong chat_id;
        public E_GuildAllianceChatGrade chat_grade;
        public E_GuildAllianceChatState chat_state;
        public E_GuildAllianceState state;

        public GuildAllianceInfoConverted(GuildAllianceInfo info)
        {
            guild_id = info.GuildId;
            name = info.Name;
            master_char_id = info.MasterCharId;
            master_char_nick = info.MasterCharNick;
            member_cnt = info.MemberCnt;
            mark_tid = info.MarkTid;
            exp = info.GuildExp;
            chat_id = info.ChatId;
            chat_grade = info.ChatGrade;
            chat_state = info.ChatState;
            state = info.State;
        }

        //public GuildAllianceInfoConverted(ulong guild_id, string name, ulong exp, ulong master_char_id, string master_char_nick, uint member_cnt, byte mark_tid, ulong chat_id, E_GuildAllianceChatGrade chat_grade, E_GuildAllianceChatState chat_state, E_GuildAllianceState state)
        //{
        //    this.guild_id = guild_id;
        //    this.name = name;
        //    this.exp = exp;
        //    this.master_char_id = master_char_id;
        //    this.master_char_nick = master_char_nick;
        //    this.member_cnt = member_cnt;
        //    this.mark_tid = mark_tid;
        //    this.chat_id = chat_id;
        //    this.chat_grade = chat_grade;
        //    this.chat_state = chat_state;
        //    this.state = state;
        //}

        public void Set(GuildAllianceInfo info)
        {
            guild_id = info.GuildId;
            name = info.Name;
            master_char_id = info.MasterCharId;
            master_char_nick = info.MasterCharNick;
            member_cnt = info.MemberCnt;
            mark_tid = info.MarkTid;
            exp = info.GuildExp;
            chat_id = info.ChatId;
            chat_grade = info.ChatGrade;
            chat_state = info.ChatState;
            state = info.State;
        }
    }

    // 길드 랭킹 컨버팅 ( GuildRankInfo ) 
    public class GuildRankInfoConverted
    {
        public uint rank;
        public ulong guild_id;
        public string name;
        public ulong exp;
        public string intro;
        public bool is_quick_join;
        public ulong master_char_id;
        public string master_char_nick;
        public uint member_cnt;
        public byte mark_tid;
        public ulong create_dt;

        public GuildRankInfoConverted(GuildRankInfo info)
        {
            this.rank = info.Rank;
            guild_id = info.GuildId;
            name = info.Name;
            exp = info.Exp;
            intro = info.Intro;
            is_quick_join = info.IsQuickJoin;
            master_char_id = info.MasterCharId;
            master_char_nick = info.MasterCharNick;
            member_cnt = info.MemberCnt;
            mark_tid = info.MarkTid;
            create_dt = info.CreateDt;
        }

        public GuildRankInfoConverted(uint rank, ulong guild_id, string name, ulong exp, string intro, bool is_quick_join, ulong master_char_id, string master_char_nick, uint member_cnt, byte mark_tid, ulong create_dt)
        {
            this.rank = rank;
            this.guild_id = guild_id;
            this.name = name;
            this.exp = exp;
            this.intro = intro;
            this.is_quick_join = is_quick_join;
            this.master_char_id = master_char_id;
            this.master_char_nick = master_char_nick;
            this.member_cnt = member_cnt;
            this.mark_tid = mark_tid;
            this.create_dt = create_dt;
        }
    }

	// 길드 가입 요청 정보 (길드기준)
	public class GuildRequestListForGuildConverted
    {
        public ulong char_id;
        public uint char_tid;
        public string char_nick;
        public uint char_lv;
        public string comment;
        public bool is_login;
        public ulong logout_dt;

        public GuildRequestListForGuildConverted(ulong char_id, uint char_tid, string char_nick, uint char_lv, string comment, bool is_login, ulong logout_dt)
        {
            this.char_id = char_id;
            this.char_tid = char_tid;
            this.char_nick = char_nick;
            this.char_lv = char_lv;
            this.comment = comment;
            this.is_login = is_login;
            this.logout_dt = logout_dt;
        }

        public GuildRequestListForGuildConverted(GuildRequestInfoForGuild info)
        {
            this.char_id = info.CharId;
            this.char_tid = info.CharTid;
            this.char_nick = info.CharNick;
            this.char_lv = info.CharLv;
            this.comment = info.Comment;
            this.is_login = info.IsLogin;
            this.logout_dt = info.LogoutDt;
        }
    }
    #endregion
    #endregion

    #region Fields
    public static ZWebGame Web { get { return ZWebManager.Instance.WebGame; } }

    public static List<GuildInfoConverted> GuildsRecommended = new List<GuildInfoConverted>();
    public static List<GuildRequestInfoForCharConverted> GuildsRequestInfoForChar = new List<GuildRequestInfoForCharConverted>();
    public static List<GuildRankInfoConverted> GuildRankInfoList = new List<GuildRankInfoConverted>();
    //    public static List<GuildMemberInfoConverted> GuildMemberInfo = new List<GuildMemberInfoConverted>();

    public static MyGuildInfo MyGuildData = new MyGuildInfo();

    private static GuildRedDotStatusFlag RedDotStatusFlag = GuildRedDotStatusFlag.None;

	public UIFrameGuildNetCapturer()
	{
        RedDotStatusFlag = GuildRedDotStatusFlag.None;
    }
	#endregion

	#region Util 
	public static bool AmIMaster { get => Me.CurCharData.GuildGrade == E_GuildMemberGrade.Master; }
	public static bool AmISubMaster { get => Me.CurCharData.GuildGrade == E_GuildMemberGrade.SubMaster; }
	public static bool AmIMasterOrSubMaster { get => AmIMaster || AmISubMaster; }
	public static ulong MyGuildID { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.guildID : 0; }
	public static string MyGuildName { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.guildName : string.Empty; }
    public static string MyGuildMasterNick { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.masterName : string.Empty; }
    public static E_GuildAllianceChatState MyGuildChatState { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.chatState : E_GuildAllianceChatState.None; }
    public static E_GuildAllianceChatGrade MyGuildChatGrade { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.chatGrade : E_GuildAllianceChatGrade.None; }
	public static byte MyGuildMarkTid { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.markTid : (byte)0; }
	public static bool IsMyGuildChatRoomMaster
    {
        get => MyGuildData.myGuildInfo.chatID != 0
            && MyGuildData.myGuildInfo.chatGrade == E_GuildAllianceChatGrade.Master;
    }
    public static ulong MyGuildExp { get => MyGuildData.myGuildInfo != null ? MyGuildData.myGuildInfo.exp : 0; }
	public static bool IsReceivedRequestAllianceExist => MyGuildData.IsReceivedRequestAllianceExist();
	public static bool IsReceivedRequestAllianceChatExist => MyGuildData.IsReceivedRequestAllianceChatExist();
	public static bool IsReceivedGuildJoinRequestExist => MyGuildData.IsReceivedGuildJoinRequestExist();
	public static List<ulong> GuildIDListParticipatedInChat
	{
		get
		{
            List<ulong> list = new List<ulong>();
            ulong targetGuildChatID = Me.CurCharData.GuildChatId;
            if (targetGuildChatID == 0)
                return list;

            /// 첫 인덱스에는 마스터 길드를 고정으로 넣어줌 
            if(IsMyGuildChatRoomMaster)
			{
                list.Add(Me.CurCharData.GuildId);
			}
			else
			{
                var targetMaster = MyGuildData.FindAlliance((info) =>
                {
                    return info.chat_id == targetGuildChatID
                    && info.chat_state == E_GuildAllianceChatState.Enter
                    && info.chat_grade == E_GuildAllianceChatGrade.Master;
                });

                if(targetMaster == null)
				{
                    ZLog.LogWarn(ZLogChannel.UI, "Failed to Find Master Guild .");
				}
				else
				{
                    list.Add(targetMaster.guild_id);
				}

                /// 두번째로는 내 길드 넣음 
                list.Add(Me.CurCharData.GuildId);
            }

            MyGuildData.ForeachAlliance((allianceState, allianceInfo) =>
            {
                /// 같은 채팅방에 있는 길드일때 ,
                /// 중복 체킹후 Add
                if(allianceInfo.chat_id == targetGuildChatID 
                    && allianceInfo.chat_state == E_GuildAllianceChatState.Enter
                    && list.Exists(guildIDDuplicate => guildIDDuplicate == allianceInfo.guild_id) == false)
				{
                    list.Add(allianceInfo.guild_id);
				}
            });

            return list;
		}
	}

	public static uint TodayAttendanceCount
	{
        get
        {
            if (MyGuildData == null)
            {
                return 0;
            }

            uint cnt = 0;

            if (MyGuildData.members != null)
            {
                foreach (var t in MyGuildData.members)
                {
                    /// 마지막 출석 보상 ServerTime 기준으로 오늘인지 체킹함 
                    if (TimeHelper.IsGivenDtToday(t.attendRewardDt, 5))
                    {
                        cnt++;
                    }
                }
            }

            return cnt;
        }
    }

    public static uint SubMasterCount
    {
        get
        {
            if (MyGuildData == null)
                return 0;

            uint cnt = 0;

            for (int i = 0; i < MyGuildData.members.Count; i++)
            {
                if (MyGuildData.members[i].grade == E_GuildMemberGrade.SubMaster)
                    cnt++;
            }

            return cnt;
        }
    }

    public static bool IsThisMemberGuildMasterInMyGuild(ulong id)
    {
        if (MyGuildData == null)
            return false;

        return MyGuildData.myGuildInfo.masterID == id;
    }

    static public bool ValidateGuildName(string name, out string errorMsg)
	{
        errorMsg = string.Empty;

        /// TODO : LOCALE
        if (name.Length < DBConfig.GuildName_Length_Min)
        {
            errorMsg = "길드 이름이 너무 짧습니다.";
            return false; 
        }

        /// TODO : LOCALE
        if (NTCommon.StringUtil.ValidateCommonName(name) == false)
        {
            errorMsg = "유효하지 않은 길드 이름입니다";
            return false; 
        }

        return true; 
    }

	public static bool IsTargetRedDotFlagOn(GuildRedDotStatusFlag flag)
	{
		return (RedDotStatusFlag & flag) != 0;
	}

    public static bool IsRedDotStatusDirty()
	{
        return RedDotStatusFlag != GuildRedDotStatusFlag.None;
	}

	public static bool IsAllianceGuild(ulong guildId)
    {
        return IsAllianceGuildTargetState(guildId, E_GuildAllianceState.Alliance);
    }

    public static bool IsEnemy(ulong guildId)
    {
        return IsAllianceGuildTargetState(guildId, E_GuildAllianceState.Enemy);
    }

    public static bool IsEnemy(string guildName)
	{
        return IsAllianceGuildTargetState(guildName, E_GuildAllianceState.Enemy);
	}

    public static GuildAllianceInfoConverted FindAllianceInfoByID(ulong guildID)
    {
        return MyGuildData.FindAlliance(t => t.guild_id == guildID);
    }

    /// <summary>
    /// 해당 길드가 TargetState 인지 체킹  
    /// </summary>
    public static bool IsAllianceGuildTargetState(ulong guildID, E_GuildAllianceState targetState)
	{
		if (MyGuildData.myGuildAllianceInfo.ContainsKey(targetState) == false)
			return false;

        return MyGuildData.myGuildAllianceInfo[targetState].Exists(t => t.guild_id == guildID);
	}

    public static bool IsAllianceGuildTargetState(string guildName, E_GuildAllianceState targetState)
	{
		if (MyGuildData.myGuildAllianceInfo.ContainsKey(targetState) == false)
			return false;

		return MyGuildData.myGuildAllianceInfo[targetState].Exists(t => t.name == guildName);
	}

	///*
	// * 계산방식 : 
	// * 지금 시간 - 오늘 출석 기준 시간 ( 새벽 5시 , 오늘 새벽 5시 또는 이전날의 새벽 5시가 될수있음 )
	// * 지금 시간 - 해당 유저의 마지막 로그인 시간을 체크해서 
	// * 전자가 더 크다면 해당 유저는 출석한거임 . 
	// * */
	//public static bool IsGivenDtToday(ulong logoutDt, int hourSinceCheck)
	//{
	//    var today = DateTime.Today;

	//    var timeLastLogoutSince = new DateTime(today.Year, today.Month, today.Day, hourSinceCheck, 0, 0);
	//    double secondsSinceCheckTimeTilNow = 0;

	//    // 만약 지금 Hour 이 체킹 기준 hour 보다 낮다면 이전날의 시간으로 체킹해야함 . 
	//    if (DateTime.Now.Hour < hourSinceCheck)
	//    {
	//        timeLastLogoutSince = timeLastLogoutSince.Subtract(TimeSpan.FromDays(1));
	//    }

	//    // 마지막 새벽 5 시로부터 지금까지의 시간 
	//    secondsSinceCheckTimeTilNow = (DateTime.Now - timeLastLogoutSince).TotalSeconds;

	//    return TimeManager.NowSec - logoutDt < secondsSinceCheckTimeTilNow;
	//}

	public static bool CanChangeGuildMark(out ulong remainedSeconds)
    {
        remainedSeconds = 0;

        if (MyGuildData.myGuildInfo == null)
            return false;

        ulong secFromLastUpdateTilNow = (TimeManager.NowSec - MyGuildData.myGuildInfo.markUpdateDt);
        ulong secResetDurationWaitTime = DBConfig.GuildMark_Change_Time;

        bool canChange = secFromLastUpdateTilNow >= secResetDurationWaitTime;

        if (canChange == false)
        {
            remainedSeconds = secResetDurationWaitTime - secFromLastUpdateTilNow;
        }

        return canChange;
    }

    public static bool CanCreateAllianceChatRoom(out CheckGuildCreateChatRoomResult result)
	{
        result = CheckGuildCreateChatRoomResult.Positive;

		if (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Enter)
		{
			result = CheckGuildCreateChatRoomResult.Denied_AlreadyJoined;
			return false;
		}
		else if (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Request)
		{
            result = CheckGuildCreateChatRoomResult.Denied_Requested;
            return false; 
		}
		else if (Me.CurCharData.GuildChatState == E_GuildAllianceChatState.Receive)
		{
            result = CheckGuildCreateChatRoomResult.Denied_Invited;
            return false; 
		}

		return true; 
	}

    public static GuildAllianceInfoConverted GetMasterChatAllianceInfo()
	{
        if (Me.CurCharData.GuildChatId == 0)
		{
            ZLog.LogError(ZLogChannel.UI, "Not supposed to be called when there is no chatRoom");
            return null;
		}

        if (MyGuildData.myGuildAllianceInfo.ContainsKey(E_GuildAllianceState.Alliance) == false)
            return null;

		foreach (var allianceInfo in MyGuildData.myGuildAllianceInfo[E_GuildAllianceState.Alliance])
		{
            /// 찾았다 
			if (Me.CurCharData.GuildChatId == allianceInfo.chat_id
                && allianceInfo.chat_state == E_GuildAllianceChatState.Enter
                && allianceInfo.chat_grade == E_GuildAllianceChatGrade.Master)
			{
                return allianceInfo;
			}
		}

        return null;
	}
	#endregion

	//public static void RefreshRedDotFlag()
	//{
	//	RedDotFlag = GuildRedDotStatusFlag.None;

	//	if (IsReceivedGuildJoinRequestExist)
	//	{
	//		RedDotFlag |= GuildRedDotStatusFlag.GuildInfo_ReceivedGuildJoinRequest;
	//	}
	//	if (IsReceivedRequestAllianceExist)
	//	{
	//		RedDotFlag |= GuildRedDotStatusFlag.GuildInfo_AllianceTab;
	//	}
	//	if (IsReceivedRequestAllianceChatExist)
	//	{
	//		RedDotFlag |= GuildRedDotStatusFlag.GuildInfo_AllianceChatTab;
	//	}
	//}

	public static void SetRedDotStatus(GuildRedDotStatusFlag flag, bool isDirty)
	{
		if (isDirty)
		{
			RedDotStatusFlag |= flag;
		}
		else
		{
			RedDotStatusFlag &= ~flag;
		}
    }

	public static void ReqRecommendGuildInfo(Action<ZWebRecvPacket, List<GuildInfoConverted>> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        GuildsRecommended.Clear();

        Web.REQ_RecommendGuildInfo(
            (revPacket, resList) =>
            {
                for (int i = 0; i < resList.GuildInfosLength; i++)
                {
                    var t = resList.GuildInfos(i).Value;

                    GuildsRecommended.Add(new GuildInfoConverted(t));
                }

                _onReceive(revPacket, GuildsRecommended);
            }, _onError);
    }

    public static void ReqGuildRequestListForChar(Action<ZWebRecvPacket, List<GuildRequestInfoForCharConverted>> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        GuildsRequestInfoForChar.Clear();

        Web.REQ_GuildRequestListForChar(
            (revPacket, resList) =>
            {
                for (int i = 0; i < resList.GuildRequestInfosLength; i++)
                {
                    var t = resList.GuildRequestInfos(i).Value;

                    GuildsRequestInfoForChar.Add(
                        new GuildRequestInfoForCharConverted(
                            DBGuild.GetLevel(t.Exp),
                            t.GuildId,
                            t.Exp,
                            t.Name,
                            t.Intro,
                            t.IsQuickJoin,
                            t.MasterCharId,
                            t.MasterCharNick,
                            t.MemberCnt,
                            DBConfig.Guild_Max_Character,
                            t.MarkTid));
                }

                _onReceive(revPacket, GuildsRequestInfoForChar);
            }, _onError);
    }

	public static void ReqCreateGuild(
        string guildName
        , string guildIntroduction
       , string guildNoti
        , byte guildMarkTID
        , ulong useItemID
        , bool isQuickJoin
        , Action<ZWebRecvPacket, List<GuildRequestInfoForCharConverted>> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        MyGuildData.ClearMemberInfo();

        Web.REQ_CreateGuild(
             guildName
             , guildIntroduction
             , guildNoti
             , guildMarkTID
             , useItemID
             , isQuickJoin
             , (revPacket, resList) =>
             {
                 MyGuildData.SetGuildInfo(resList.GuildInfo.Value);

                 for (int i = 0; i < resList.GuildMemberInfosLength; i++)
                 {
                     var t = resList.GuildMemberInfos(i).Value;

                     MyGuildData.AddMemberInfo(t);
                 }

                 MyGuildData.SortMemberListByDisplayOrder();

                 _onReceive(revPacket, GuildsRequestInfoForChar);
             }
             , _onError);
    }

    // 길드 해산 
    public static void ReqDismissGuild(
        ulong guildID
        , ulong guildChatID
        , E_GuildAllianceChatGrade guildChatGrade
        , Action<ZWebRecvPacket, ResDismissGuild> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_DismissGuild(guildID, guildChatID, guildChatGrade
            , (revPacket, resList) =>
            {
                MyGuildData.ClearAll();

                // 길드해산 성공 
                _onReceive(revPacket, resList);
            }, _onError);
    }

    // 길드 가입 수락 
    public static void ReqGuildRequestAccept(
        ulong guildID
        , ulong acceptCharID
        , Action<ZWebRecvPacket, ResGuildRequestAccept> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildRequestAccept(
            guildID
            , acceptCharID
            , (revPacket, resList) =>
            {
                ReqGuildRequestListForGuild(
                    guildID
                    , (revPacketRec_, resListRec_) =>
                    {
                        ReqGetGuildInfo(guildID, (revPacketRec__, resListRec__) =>
                        {
                            _onReceive(revPacket, resList);
                        }, _onError);
                    }, _onError);
            }
            , _onError);
    }

    // 길드입장에서의 길드 가입 신청 거절 
    public static void ReqGuildRequestReject(
        ulong guildID
        , ulong rejectCharID
        , Action<ZWebRecvPacket, ResGuildRequestReject> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildRequestReject(
            guildID
            , rejectCharID
            , (revPacket, resList) =>
            {
                ReqGuildRequestListForGuild(
                    guildID
                    , (revPacketRec_, resListRec_) =>
                    {
                        ReqGetGuildInfo(guildID, (revPacketRec__, resListRec__) =>
                        {
                            _onReceive(revPacket, resList);
                        }, _onError);
                    }, _onError);
            }
            , _onError);
    }

    // 길드 가입 요청하기 
    //    public static void ReqGuildRequestJoin(ulong guildID, string comment,
    //        Action<ZWebRecvPacket, ResGuildRequestJoin, bool> _onReceive, PacketErrorCBDelegate _onError = null)
    //    {
    //#if _GTEST_
    //        _onReceive(null, default(ResGuildRequestJoin), false);
    //        return;
    //#endif

    //        ZWebManager.Instance.WebGame.Req_GuildRequestJoin(guildID, comment,
    //            (revPacket, resList, joined) =>
    //            {
    //                // 즉시 가입이 됐다면 
    //                if (joined)
    //                {
    //                    // 내가 가입한 길드 정보를 Update 해줘야하기때문에 호출함  

    //                    //ReqGetGuildInfo(guildID,
    //                    //    (revPacket_info, resList_info) =>
    //                    //    {
    //                    //        _onReceive(revPacket, resList, joined);
    //                    //    }
    //                    //    , _onError);
    //                }
    //                else
    //                {
    //                    _onReceive(revPacket, resList, joined);
    //                }
    //            }, _onError);
    //    }

    // 길드 탈퇴하기 (길드원 입장) 
    public static void ReqGuildMemberLeave(
        ulong guildID,
        Action<ZWebRecvPacket, ResGuildMemberLeave> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_GuildMemberLeave(guildID,
            (revPacket, resList) =>
            {
                MyGuildData.ClearAll();
                _onReceive(revPacket, resList);
            }, _onError);
    }

    // 길드원 추방시키기 
    public static void ReqGuildMemberBan(
        ulong guildID
        , ulong banCharID
        , Action<ZWebRecvPacket, ResGuildMemberBan> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_GuildMemberBan(
            guildID, banCharID
            , (revPacket, resList) =>
            {
                ReqGetGuildInfo(guildID,
                    (revPacket_, resList_) =>
                    {
                        _onReceive(revPacket, resList);
                    }, _onError);
            }, _onError);
    }

    // 길드 기부하기
    public static void ReqGuildDonation(
        ulong guildID
        , E_GuildDonationType donationType
        , ulong useItemID
        , Action<ZWebRecvPacket, ResGuildDonation> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_GuildDonation(
            guildID, donationType, useItemID
            , (revPacket, resList) =>
            {
                MyGuildData.myGuildInfo = new GuildInfoConverted(resList.GuildInfo.Value);

                for (int i = 0; i < MyGuildData.members.Count; i++)
                {
                    if (MyGuildData.members[i].charID == resList.GuildMemberInfo.Value.CharId)
                    {
                        MyGuildData.members[i] = new GuildMemberInfoConverted(resList.GuildMemberInfo.Value);
                        break;
                    }
                }

                _onReceive(revPacket, resList);
            }, _onError);
    }

    // 길드 정보 가져오기 
    public static void ReqGetGuildInfo(
        ulong guildID
        , Action<ZWebRecvPacket, ResGetGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_GetGuildInfo(guildID
            , (revPacket, resList) =>
             {
                 // 내가 가입한 길드면은 
                 if (Me.CurCharData.GuildId == resList.GuildInfo.Value.GuildId)
                 {
                     // 길드 정보 세팅 
                     MyGuildData.SetGuildInfo(resList.GuildInfo.Value);

                     // 길드 멤버 세팅 
                     MyGuildData.ClearMemberInfo();
                     for (int i = 0; i < resList.GuildMemberInfosLength; i++)
                     {
                         var v = resList.GuildMemberInfos(i).Value;
                         MyGuildData.AddMemberInfo(v);
                     }
                     MyGuildData.SortMemberListByDisplayOrder();
                 }

                 _onReceive(revPacket, resList);
             }, _onError);
    }

    // 길드 출석 보상 요청하기 
    public static void ReqGuildAttendReward(
        ulong guildID
        , Action<ZWebRecvPacket, ResGuildAttendReward> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        ZWebManager.Instance.WebGame.REQ_GuildAttendReward(guildID
            , (revPacket, resList) =>
            {
                for (int i = 0; i < MyGuildData.members.Count; i++)
                {
                    /// 내 데이터 찾아서 rewardDt 수정 
                    if (MyGuildData.members[i].charID == Me.CurCharData.ID)
                    {
                        MyGuildData.members[i].attendRewardDt = resList.AttendRewardDt;
                        break;
                    }
                }

                _onReceive(revPacket, resList);
            }, _onError);
    }

    #region Alliance (연맹 관련)
    // 길드 연맹 정보 리스트 가져오기 
    public static void ReqGetGuildAllianceList(
          ulong guildID, E_GuildAllianceState[] states
        , Action<ZWebRecvPacket, ResGetGuildAllianceList> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GetGuildAllianceList(guildID, states,
            (revPacket, resList) =>
            {
                // 내가 가입한 길드면은 
                if (Me.CurCharData.GuildId == guildID)
                {
                    // 업뎃 
                    for (int i = 0; i < states.Length; i++)
                    {
                        MyGuildData.ClearAllianceInfo(states[i]);
                    }

                    // 연맹 정보 갱신 
                    for (int i = 0; i < resList.GuildAllianceInfosLength; i++)
                    {
                        MyGuildData.AddAllianceInfo(resList.GuildAllianceInfos(i).Value);
                    }

                    if(AmIMasterOrSubMaster)
					{
                        SetRedDotStatus(GuildRedDotStatusFlag.GuildInfo_AllianceChatTab, IsReceivedRequestAllianceChatExist);
                        SetRedDotStatus(GuildRedDotStatusFlag.GuildInfo_AllianceTab, IsReceivedRequestAllianceExist);
                    }
					else
					{
                        SetRedDotStatus(GuildRedDotStatusFlag.GuildInfo_AllianceChatTab, false);
                        SetRedDotStatus(GuildRedDotStatusFlag.GuildInfo_AllianceTab, false);
                    }
                }

                _onReceive(revPacket, resList);
            }, _onError);
    }

    // 연맹 추가 요청 
    public static void ReqGuildAllianceRequest(
        ulong guildId
        , bool isEnemy
        , string targetGuildName
        , Action<ZWebRecvPacket, ResGuildAllianceRequest> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceRequest(
			guildId
			, isEnemy
			, targetGuildName
			, (revPacket, resList) =>
			 {
                 //이거는 외부에서 처리함. Event 때문에 
                 //ReqGetGuildAllianceList(guildId
                 //  , new E_GuildAllianceState[] { E_GuildAllianceState.RequestAlliance }
                 //  , (revPacketReq_, resListReq_) =>
                 //  {
                 //      _onReceive(revPacket, resList);
                 //  }, _onError);
                 _onReceive(revPacket, resList);
			 }, _onError);
	}


    // 길드 연맹 추가 요청 승인 
    public static void ReqGuildAllianceAccept(
        ulong guildId
        , string guildName
        , ulong targetGuildId
        , Action<ZWebRecvPacket, ResGuildAllianceAccept> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceAccept(
             guildId
            , guildName
            , targetGuildId
            , (revPacket, resList) =>
             {
                 // 이벤트 관리해야하므로 외부에서 Refresh 
                 //ReqGetGuildAllianceList(guildId
                 //    , new E_GuildAllianceState[] { E_GuildAllianceState.Alliance }
                 //    , (revPacketReq_, resListReq_) =>
                 //    {
                 //        _onReceive(revPacket, resList);
                 //    }, _onError);
                 _onReceive?.Invoke(revPacket, resList);
             }, _onError);
    }

    // 길드 연맹 추가 요청 거부 or 취소  
    public static void ReqGuildAllianceReject(
        ulong guildId
        , ulong targetGuildId
        , Action<ZWebRecvPacket, ResGuildAllianceReject> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceReject(
            guildId
            , targetGuildId
            , (revPacket, resList) =>
            {
				// 내가 요청을 거부한거니까 나한테 신청한 애들 연맹 정보 다시 얻어옴 
				// +++ 이벤트 관리해야하므로 , 외부에서 Refresh 관리 
				//ReqGetGuildAllianceList(guildId
				//    , new E_GuildAllianceState[] { E_GuildAllianceState.ReceiveAlliance }
				//    , (revPacketReq_, resListReq_) =>
				//    {
				//        _onReceive(revPacket, resList);
				//    }, _onError);
				_onReceive?.Invoke(revPacket, resList);
			}, _onError);
    }

    /// <summary>
    ///  연맹 Or 적대 연맹 삭제 
    /// </summary>
	public static void ReqGuildAllianceRemove(
		ulong guildId
        , string guildName
        , bool isEnemy
        , ulong targetGuildID
        , string targetGuildName
        , Action<ZWebRecvPacket, ResGuildAllianceRemove> _onReceive, PacketErrorCBDelegate _onError = null)
	{
        Web.REQ_GuildAllianceRemove(
          guildId
          , guildName
          , isEnemy
          , targetGuildID
          , targetGuildName
          , (revPacket, resList) =>
          {
              _onReceive?.Invoke(revPacket, resList);
          }, _onError);
    }

	/// <summary>
	/// 길드 채팅방 생성 
	/// </summary>
	/// <param name="_onError"></param>
	public static void ReqGuildAllianceCreateChat(
        ulong guildId
        , Action<ZWebRecvPacket, ResGuildAllianceCreateChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceCreateChat(
            guildId
            , (revPacket, resList) =>
			{
				if (Me.CurCharData.GuildId == guildId)
				{
					var guildInfo = resList.GuildInfo.Value;
					MyGuildData.myGuildInfo.SetChatInfo(guildInfo.ChatId, guildInfo.ChatState, guildInfo.ChatGrade);
				}
                _onReceive?.Invoke(revPacket, resList);
			}, _onError);
    }

    /// <summary>
	/// 길드 채팅방 탈퇴 
	/// </summary>
	/// <param name="_onError"></param>
	public static void ReqGuildAllianceLeaveChat(
        ulong guildId
        , ulong chatId
        , E_GuildAllianceChatGrade chatGrade
        , Action<ZWebRecvPacket, ResGuildAllianceLeaveChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        /// TODO : 길드장일때 탈퇴하면 뭐 방 자체가 해체가 되는거 ? 안에있던애들은 ? 
        /// 아니면 자동으로 마스터가 다음애로 넘어감 ? 뭐임/ ? 
        Web.REQ_GuildAllianceLeaveChat(
            guildId
            , chatId
            , chatGrade
            , (revPacket, resList) =>
            {
                if (Me.CurCharData.GuildId == guildId)
                {
                    var guildInfo = resList.GuildInfo.Value;
                    MyGuildData.myGuildInfo.SetChatInfo(guildInfo.ChatId, guildInfo.ChatState, guildInfo.ChatGrade);
                }
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 길드 채팅방 강퇴
    /// </summary>
	public static void ReqGuildAllianceBanChat(
        ulong guildId
        , ulong targetGuildId
        , Action<ZWebRecvPacket, ResGuildAllianceBanChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceBanChat(
            guildId
            , targetGuildId
            , (revPacket, resList) =>
            {
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 요청 . 
    /// </summary>
    public static void ReqGuildAllianceRequestChat(
        ulong guildId
        , ulong masterGuildID
        , Action<ZWebRecvPacket, ResGuildAllianceRequestChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceRequestChat(
            guildId
            , masterGuildID
            , (revPacket, resList) =>
            {
                /// TODO : 확인 필요 . Res 로 온 GuildInfo 가 Master Guild ? 내 Guild ? 
                if (Me.CurCharData.GuildId == guildId)
                {
                    MyGuildData.myGuildInfo.SetChatInfo(resList.GuildInfo.Value.ChatId, resList.GuildInfo.Value.ChatState, resList.GuildInfo.Value.ChatGrade);
                }

                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 요청 취소 및 초대 요청 받은거 거절 
    /// </summary>
    public static void ReqGuildAllianceCancelChat(
        ulong guildId
        , Action<ZWebRecvPacket, ResGuildAllianceCancelChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceCancelChat(
            guildId
            , (revPacket, resList) =>
            {
                if (Me.CurCharData.GuildId == guildId)
                {
                    MyGuildData.myGuildInfo.SetChatInfo(resList.GuildInfo.Value.ChatId, resList.GuildInfo.Value.ChatState, resList.GuildInfo.Value.ChatGrade);
                }
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 초대 요청하기
    /// </summary>
    public static void ReqGuildAllianceInviteRequestChat(
        ulong guildId
        , ulong inviteGuildId
        , Action<ZWebRecvPacket, ResGuildAllianceInviteRequestChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceInviteRequestChat(
            guildId
            , inviteGuildId
            , (revPacket, resList) =>
            {
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 초대 요청하기
    /// </summary>
    public static void ReqGuildAllianceInviteAcceptChat(
        ulong guildId
        , Action<ZWebRecvPacket, ResGuildAllianceInviteAcceptChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceInviteAcceptChat(
            guildId
            , (revPacket, resList) =>
            {
                MyGuildData.myGuildInfo.SetChatInfo(resList.GuildInfo.Value.ChatId, resList.GuildInfo.Value.ChatState, resList.GuildInfo.Value.ChatGrade);
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 초대 요청 취소하기 
    /// </summary>
    public static void ReqGuildAllianceInviteCancelChat(
        ulong guildId
        , ulong cancelGuildId
        , Action<ZWebRecvPacket, ResGuildAllianceInviteCancelChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceInviteCancelChat(
            guildId
            , cancelGuildId
            , (revPacket, resList) =>
            {
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 입장 수락 
    /// </summary>
    public static void ReqGuildAllianceAcceptChat(
        ulong guildId
        , ulong acceptGuildID
        , Action<ZWebRecvPacket, ResGuildAllianceAcceptChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceAcceptChat(
            guildId
            , acceptGuildID
            , (revPacket, resList) =>
            {
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }

    /// <summary>
    /// 연맹 채팅 입장 거절  
    /// </summary>
    public static void ReqGuildAllianceRejectChat(
        ulong guildId
        , ulong rejectGuildID
        , Action<ZWebRecvPacket, ResGuildAllianceRejectChat> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildAllianceRejectChat(
            guildId
            , rejectGuildID
            , (revPacket, resList) =>
            {
                _onReceive?.Invoke(revPacket, resList);
            }, _onError);
    }
    #endregion

    // 길드 랭킹 리스트 가져오기 
    public static void ReqGetGuildExpRank(
        Action<ZWebRecvPacket, ResGetGuildExpRank> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GetGuildExpRank(
            (revPacket, resList) =>
            {
                GuildRankInfoList.Clear();
                GuildRankInfoList.Capacity = resList.GuildInfosLength;

                for (int i = 0; i < resList.GuildInfosLength; i++)
                {
                    GuildRankInfoList.Add(new GuildRankInfoConverted(resList.GuildInfos(i).Value));
                }

                GuildRankInfoList.Sort((t01, t02) => t01.rank.CompareTo(t02.rank));

                _onReceive(revPacket, resList);
            }, _onError);
    }

    // 길드원 임명 
    public static void ReqAppointGuildMember(
        ulong guildId
        , ulong memberCharID
        , uint guildGrade
        , Action<ZWebRecvPacket, ResAppointGuildMember> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_AppointGuildMember(
            guildId
            , memberCharID
            , guildGrade
            , (revPacket, resList) =>
            {
                ReqGetGuildInfo(guildId,
                  (revPacket_, resList_) =>
                  {
                      _onReceive(revPacket, resList);
                  }, _onError);
            }, _onError);
    }

    // 길드 입장에서의 유저들의 가입 요청 가져오기 
    public static void ReqGuildRequestListForGuild(ulong guildID, Action<ZWebRecvPacket, ResGuildRequestListForGuild> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_GuildRequestListForGuild(guildID,
            (revPacket, resList) =>
            {
                if (Me.CurCharData.GuildId == guildID)
                {
                    MyGuildData.receivedJoinRequestList.Clear();

                    for (int i = 0; i < resList.GuildRequestInfosLength; i++)
                    {
                        MyGuildData.receivedJoinRequestList.Add(new GuildRequestListForGuildConverted(resList.GuildRequestInfos(i).Value));
                    }

                    SetRedDotStatus(GuildRedDotStatusFlag.GuildInfo_ReceivedGuildJoinRequest, IsReceivedGuildJoinRequestExist);
                }

                _onReceive(revPacket, resList);
            }
            , _onError);
    }

    // 길드 정보 세팅  
    public static void ReqUpdateGuildInfo(
        ulong guildID
        , string intro
        , string notice
        , bool isQuickJoin
        , ushort loginBanStep
        , ushort donateBanStep
        , Action<ZWebRecvPacket, ResUpdateGuildInfo> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_UpdateGuildInfo(
            guildID
            , intro
            , notice
            , isQuickJoin
            , loginBanStep
            , donateBanStep
            , (revPacket, resList) =>
            {
                MyGuildData.myGuildInfo.SetInfoUpdated(
                    resList.GuildId
                    , resList.Intro
                    , resList.Notice
                    , resList.IsQuickJoin
                    , resList.LoginBanStep
                    , resList.DonateBanStep);
                _onReceive(revPacket, resList);
            }
            , _onError);
    }

    // 길드 정보 세팅  
    public static void ReqUpdateGuildMark(
        ulong guildID
        , byte markTid
        , Action<ZWebRecvPacket, ResUpdateGuildMark> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_UpdateGuildMark(
            guildID
            , markTid
            , (revPacket, resList) =>
            {
                MyGuildData.myGuildInfo.SetMarkInfoUpdated(
                    resList.GuildId
                    , resList.MarkTid
                    , resList.MarkUpdateDt
                    , resList.ResultGuildMoney);

                _onReceive(revPacket, resList);
            }
            , _onError);
    }

    public static void ReqUpdateGuildMemberComment(
        ulong guildID
        , string comment
        , Action<ZWebRecvPacket, ResUpdateGuildMemberComment> _onReceive, PacketErrorCBDelegate _onError = null)
    {
        Web.REQ_UpdateGuildMemberComment(
           guildID
           , comment
           , (revPacket, resList) =>
           {
               var myMemberInfo = MyGuildData.MyMemberInfo;

               if(myMemberInfo != null)
               {
                   myMemberInfo.comment = comment;
               }

               _onReceive(revPacket, resList);
           }
           , _onError);
    }

    #region Private Methods

    #endregion
}
