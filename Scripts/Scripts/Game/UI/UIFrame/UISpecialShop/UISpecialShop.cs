using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebNet;
using ZNet;

public class UISpecialShop : ZUIFrameBase
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    #region Common
    public void OpenTwoButtonQueryPopUp(
        string title, string content, Action onConfirmed, Action onCanceled = null
        , string cancelText = ""
        , string confirmText = "")
    {
        if (string.IsNullOrEmpty(cancelText))
        {
            cancelText = DBLocale.GetText("Cancel_Button");
        }

        if (string.IsNullOrEmpty(confirmText))
        {
            confirmText = DBLocale.GetText("OK_Button");
        }

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { cancelText, confirmText }, new Action[] {
                () =>
                {
                    onCanceled?.Invoke();
                    _popup.Close();
                },
                () =>
                {
                     onConfirmed?.Invoke();
                    _popup.Close();
                }});
        });
    }

    public void OpenNotiUp(string content, string title = "확인", Action onConfirmed = null)
    {
        DBLocale.TryGet(ZUIString.LOCALE_OK_BUTTON, out Locale_Table table);

        if (table != null)
        {
            title = DBLocale.GetText(table.Text);
        }

        //if (string.IsNullOrEmpty(title))
        //{
        //	title = ZUIString.ERROR;
        //}

        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(title, content, new string[] { title }, new Action[] { () =>
                {
                    onConfirmed?.Invoke();
                    _popup.Close();
                }});
        });
    }

    public void HandleError(ZWebCommunicator.E_ErrorType _errorType, ZWebReqPacketBase _reqPacket, ZWebRecvPacket _recvPacket)
    {
        OpenErrorPopUp(_recvPacket.ErrCode);
    }

    public void OpenErrorPopUp(ERROR errorCode, Action onConfirmed = null)
    {
        Locale_Table table;

        // 에러코드 확인누르고 특별한 처리가 필요한경우 여기서 처리함 (onConfirmed)
        // if(errorCode == e)

        DBLocale.TryGet(errorCode.ToString(), out table);

        if (table != null)
        {
            OpenNotiUp(table.Text, onConfirmed: onConfirmed);
        }
        else
        {
            OpenNotiUp("문제가 발생하였습니다.", onConfirmed: onConfirmed);
        }
    }
    #endregion
    #endregion

    #region Overrides 
    protected override void OnInitialize()
    {
        base.OnInitialize();
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        base.OnHide();
    }
    #endregion

    #region Private Methods
    #endregion

    #region Inspector Events 
    #region OnClick

    #endregion
    #endregion
}
