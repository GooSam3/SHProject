using System.Collections.Generic;
using UnityEngine.Events;

public class UIPopupServerSelect : ZUIFrameBase
{
	public class SServerInfomation
	{
		public string ServerName;
		public uint ServerID = 0;
		public WebNet.E_ServerTrafficStatus TrafficStatus = WebNet.E_ServerTrafficStatus.None; 
		public bool IsCreateCharacter = false;
		public bool IsNewServer = false;
		public bool IsRecommend = false;
	}
	private List<SServerInfomation> m_listServerSelect = new List<SServerInfomation>();
	private UnityAction<SServerInfomation> mEventServerInfo = null;
	//------------------------------------------------------------
	public void ImportUIPopupServerSelect(string _ServerName, uint _ServerID, WebNet.E_ServerTrafficStatus _TrafficStatus,  bool _IsCreateChar, bool _IsNewServer, bool _IsRecommend)
	{
		SServerInfomation ServerInfo = new SServerInfomation();
		ServerInfo.ServerName = _ServerName;
		ServerInfo.ServerID = _ServerID;
		ServerInfo.IsCreateCharacter = _IsCreateChar;
		ServerInfo.IsNewServer = _IsNewServer;
		ServerInfo.IsRecommend = _IsRecommend;
		ServerInfo.TrafficStatus = _TrafficStatus;

		m_listServerSelect.Add(ServerInfo);
	}

	public void DoUIPopupServerSelect(UnityAction<SServerInfomation> _EventServerInfo)
	{
		mEventServerInfo = _EventServerInfo;
		PrivUIPopupServerSelectRefresh();
	}

	//---------------------------------------------------------------


	//---------------------------------------------------------------
	private void PrivUIPopupServerSelectRefresh()
	{
		for (int i = 0; i < m_listServerSelect.Count; i++)
		{

		}
	}

}
