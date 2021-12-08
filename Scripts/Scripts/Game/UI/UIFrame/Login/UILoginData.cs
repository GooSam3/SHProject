using WebNet;

public class UILoginData
{
    /// <summary>서버 상태 테이블 데이터 로드</summary>
	public static string GetTrafficStateTxt(E_ServerTrafficStatus _trafficState)
    {
        string trafficTxt = string.Empty;

        switch (_trafficState)
        {
            case E_ServerTrafficStatus.Smooth: trafficTxt = DBLocale.GetText("ServerState_Good"); break;
            case E_ServerTrafficStatus.Heavy:  trafficTxt = DBLocale.GetText("ServerState_Busy"); break;
            case E_ServerTrafficStatus.Lock:   trafficTxt = DBLocale.GetText("ServerState_Off"); break;
        }

        return trafficTxt;
    }

    /// <summary>서버 추천, 신규 알람 표시</summary>
    public static string GetStateText(UILoginEnum.E_ServerState _state)
    {
        string stateTxt = string.Empty;

        switch(_state)
		{
            case UILoginEnum.E_ServerState.Recommend: stateTxt = DBLocale.GetText("ServerState_Recommend"); break;
            case UILoginEnum.E_ServerState.New:       stateTxt = DBLocale.GetText("ServerState_New"); break;
		}

        return stateTxt;
    }
}