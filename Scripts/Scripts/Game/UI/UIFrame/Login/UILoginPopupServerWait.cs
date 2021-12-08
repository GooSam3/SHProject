using System;

public class UILoginPopupServerWait : UIFrameLogin
{
    #region Variable
    private UIFrameLogin Frame = null;

    public Action ReconnectCallback { get; private set; } = null;

    private const float RECONNECT_TIME = 10.0f;
    #endregion

    protected override void Initialize(ZUIFrameBase _frame)
    {
        base.Initialize(_frame);

        Frame = _frame as UIFrameLogin;
    }

    public void SwitchWaitPopup(bool _active)
    {
        Frame.ServerWaitPopupObject.SetActive(_active);

        if(!_active)
        {
            SetReconnectCallback(null);
            Frame.SetServerWaitCountText(string.Empty);
        }
    }

    public void BindReconnectCallback(uint _waitCnt)
    {
        SwitchWaitPopup(true);

        Frame.SetServerWaitCountText(_waitCnt.ToString());

        SetReconnectCallback(delegate {
            Frame.Network.CheckAccount();
        });

        if(ReconnectCallback != null)
            TimeInvoker.Instance.RequestInvoke(delegate { ReconnectCallback?.Invoke(); }, RECONNECT_TIME);
    }

    #region Data
    private void SetReconnectCallback(Action _callback) { ReconnectCallback = _callback; }
    #endregion
}