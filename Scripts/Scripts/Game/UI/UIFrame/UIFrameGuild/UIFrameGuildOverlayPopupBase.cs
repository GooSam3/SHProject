using UnityEngine;

public class UIFrameGuildOverlayPopupBase : MonoBehaviour
{
    protected UIFrameGuild GuildController;

    public bool IsOpen { get; protected set; }

    #region Private Fields
    // OverlayWindowPopUP Type;
    #endregion


    public virtual void Initialize(UIFrameGuild guildController) // , OverlayWindowPopUP type)
    {
        GuildController = guildController;
        //  Type = type;
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        IsOpen = true;
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        IsOpen = false;
    }

    //protected void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    //{
    //    OpenErrorPopUp(_recvPacket.ErrCode);
    //}

    //protected void OpenErrorPopUp(ERROR errorCode)
    //{
    //    Locale_Table table;

    //    switch (errorCode)
    //    {
    //        // FIX ME : 길드 관련 추가 쭉 해야함 . 
    //        case ERROR.GUILD_INTO_NOT_FOUND:
    //        case ERROR.NOT_ENOUGH_GUILD_LEVEL:
    //        case ERROR.NOT_ENOUGH_GOODS:
    //        case ERROR.NOT_ENOUGH_GOLD:
    //        case ERROR.NOT_ENOUGH_DIAMOND:
    //            {
    //                DBLocale.TryGet(errorCode.ToString(), out table);

    //                if (table != null)
    //                    OpenErrorNotiUp(table.Text);
    //            }
    //            break;
    //        default:
    //            OpenErrorNotiUp("문제가 발생하였습니다.");
    //            break;
    //    }
    //}

    //protected void OpenErrorNotiUp(string content)
    //{
    //    DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);

    //    if (table != null)
    //    {
    //        string btnName = table.Text;

    //        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
    //        {
    //            _popup.Open(ZUIString.ERROR, content, new string[] { btnName }, new Action[] { () => { _popup.Close(); } });
    //        });
    //    }
    //}
}
