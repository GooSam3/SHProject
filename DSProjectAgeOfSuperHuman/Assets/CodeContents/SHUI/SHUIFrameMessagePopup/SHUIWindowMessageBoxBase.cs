using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class SHUIWindowMessageBoxBase : CUIWidgetWindowBase
{
	[SerializeField]
	private CButton ButtonOk = null;
	[SerializeField]
	private CButton ButtonCancle = null;
	[SerializeField]
	private CText MessageText = null;
	[SerializeField]
	private CText MessageTitle = null;
	//-------------------------------------------------	
	public void DoMessageBox(string strTitle, string strMessage, UnityAction delOk, UnityAction delCancel)
	{
		MessageTitle.text = strTitle;
		MessageText.text = strMessage;
		ButtonOk.onClick.RemoveAllListeners();
		ButtonOk.onClick.AddListener(delOk);
		ButtonCancle.onClick.RemoveAllListeners();
		ButtonCancle.onClick.AddListener(delCancel);		
		OnUIWindowMessageBox(strMessage);
	}

	//-------------------------------------------------
	protected virtual void OnUIWindowMessageBox(string strMessage) { }
}
