using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uTools;

public class UIMessageNoticeEffect
{
    public void Show(Text _txtObj, UIMessageNoticeData _data, Action _callback)
    {
        if (_txtObj == null || _data == null)
        {
            FinishCallback(_callback);
            return;
        }

        _txtObj.text = _data.Content;
        _txtObj.color = _data.Color;

        var uTweenAlpha = _txtObj.GetComponent<uTweenAlpha>();

        uTweenAlpha.duration = _data.FadeEndTime > 0.0f ? _data.FadeEndTime : 0.0f;
        uTweenAlpha.delay = _data.FadeHoldTime > 0.0f ? _data.FadeHoldTime : 0.0f;

        uTweenAlpha.Play(true);

        TimeInvoker.Instance.RequestInvoke(delegate {
            FinishCallback(_callback);
        }, _data.FadeEndTime + _data.FadeHoldTime);
    }

    private void FinishCallback(Action _callback)
    {
        _callback?.Invoke();
    }
}