public class UILoginPopupServerList : UIFrameLogin
{
    #region Variable
    private UIFrameLogin Frame = null;
    #endregion

    protected override void Initialize(ZUIFrameBase _frame)
    {
        base.Initialize(_frame);

        Frame = _frame as UIFrameLogin;
    }

    /// <summary>서버 리스트 정렬</summary>
    public void SortServerList()
    {
        for (int i = 0; i < Frame.ServerList.Count; i++)
        {
            // 추천 서버 리스트
            if (Frame.SelectServerTab == UILoginEnum.E_ServerListTab.Recommend)
            {
                if (Frame.ServerList[i].ServerStateType == UILoginEnum.E_ServerState.New)
                    Frame.ServerList[i].gameObject.SetActive(Frame.ServerList[i].ServerStateType == UILoginEnum.E_ServerState.New);
            }
            // 전체 서버 리스트
            else
                Frame.ServerList[i].gameObject.SetActive(true);
        }
    }

    /// <summary>서버 리스트 클리어</summary>
    public void ClearServerList()
    {
        for (int i = 0; i < Frame.ServerList.Count; i++)
            Destroy(Frame.ServerList[i].gameObject);

        Frame.ServerList.Clear();
    }

    /// <summary> 서버 리스트 창 On/Off</summary>
    /// <param name = "_on"> 창 활성화 / 비활성화 </param>
    public void SwitchServerList(bool _on)
    {
        Frame.ServerListPopupObject.SetActive(_on);
    }

    /// <summary>서버 리스트 탭 On/Off</summary>
    /// <param name = "_tab"> 탭 Idx </param>
    public void SwitchServerTab(int _tab)
    {
        if ((UILoginEnum.E_ServerListTab)_tab != Frame.SelectServerTab)
        {
            for (int i = 0; i < ZUIConstant.LOGIN_SERVER_LIST_TAB_COUNT; i++)
                if (i == _tab)
                    Frame.SetSelectServerTab((UILoginEnum.E_ServerListTab)_tab);
        }

        SortServerList();
    }
}