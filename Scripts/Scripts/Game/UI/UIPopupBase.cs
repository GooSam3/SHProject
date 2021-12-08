using System;
using UnityEngine.UI;

abstract public class UIPopupBase : ZUIFrameBase
{
    protected void Set(Button[] _button, Text[] _content, Action[] _callback, string[] _btnTxt, Action[] _callBack)
    {
        if (_btnTxt.Length == 0 || _callBack.Length == 0)
            return;

        for (int i = 0; i < _btnTxt.Length; i++)
        {
            _button[i].gameObject.SetActive(true);
            _content[i].text = _btnTxt[i];
            _callback[i] = _callBack[i];
        }
    }

    protected virtual void Active(bool _active) { }
}