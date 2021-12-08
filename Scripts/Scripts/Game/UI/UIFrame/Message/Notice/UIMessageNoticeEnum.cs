public class UIMessageNoticeEnum
{
    public enum E_MessageType
    {
        None = 0,

		/// <summary> Notice = 메인 공지 </summary>
		Notice,
		/// <summary> NoticeSub = 인게임 상단 공지 또는 알림(메인 공지 아래 위치) </summary>
		SubNotice,
		/// <summary> BackNotice = 쿨타임 같은 화면 중앙에 작게 표시해주는 알림(Hud보다도 레이어가 낮음) </summary>
		BackNotice
    }

    public enum E_NoticeType
    {
        Main = 0,
        Sub
    }
}