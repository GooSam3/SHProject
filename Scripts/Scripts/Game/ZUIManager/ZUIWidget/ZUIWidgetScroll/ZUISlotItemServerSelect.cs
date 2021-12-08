using UnityEngine;

public class ZUISlotItemServerSelect : CUGUIWidgetSlotItemBase
{
    [SerializeField]
    private UnityEngine.UI.Text  ServerName;
    [SerializeField]
    private GameObject          ExistCharacter;
    [SerializeField]
    private CWidgetContainer     Focus;
    [SerializeField]
    private CWidgetContainer     Status;

    private uint mServerID = 0;  public uint pServerID { get { return mServerID; } }
    //-----------------------------------------------------
    public void DoServerSelectInfo(UIPopupServerSelect.SServerInfomation _ServerInfo)
	{
        ServerName.text = _ServerInfo.ServerName;
        mServerID = _ServerInfo.ServerID;
        if (_ServerInfo.IsCreateCharacter)
		{
            ExistCharacter.SetActive(true);
		}
        else
		{
            ExistCharacter.SetActive(false);
		}

        if (_ServerInfo.IsRecommend)
		{
            Focus.SwitchContainerObject("Recommend");
        }

        if (_ServerInfo.IsNewServer)
		{
            Focus.SwitchContainerObject("New");
		}

        if (_ServerInfo.TrafficStatus == WebNet.E_ServerTrafficStatus.Bush)
		{
            Status.SwitchContainerObject("Busy");
		}
        else if (_ServerInfo.TrafficStatus == WebNet.E_ServerTrafficStatus.Heavy)
		{
            Status.SwitchContainerObject("Heavy");
		}
        else if (_ServerInfo.TrafficStatus == WebNet.E_ServerTrafficStatus.Lock)
		{
            Status.SwitchContainerObject("Lock");
		}
        else if (_ServerInfo.TrafficStatus == WebNet.E_ServerTrafficStatus.Smooth)
        {
            Status.SwitchContainerObject("Smooth");
        }
    }
}
