using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHUIFrameMessagePopup : SHUIFrameBase
{
	public enum EMessagePopupType
	{
		Ok,
		CancleOk,
	}
	[System.Serializable]
	public class SMessageBox
	{
		[SerializeField]
		public EMessagePopupType       MessageType = EMessagePopupType.Ok;
		[SerializeField]
		public SHUIWindowMessageBoxBase MessageBox = null;
 	}
	[SerializeField]
	private SHUIWidgetCurrencyCount CurrencyCount = null;
	[SerializeField]
	private List<SMessageBox> PopupWindow = new List<SMessageBox>();


	protected class SMessageInfo
	{
		public EMessagePopupType PopupType = EMessagePopupType.Ok;
		public string Title;
		public string Message;
		public UnityAction Ok;
		public UnityAction Cancel;
		public ECurrencyType CurrencyType = ECurrencyType.None;
		public long CurrencyCount = 0;
		public SHUIWindowMessageBoxBase WidgetInstance = null;
	}

	private SMessageInfo m_pCurrentMessageInfo = null;
	private Stack<SMessageInfo> m_stackMessageInfo = new Stack<SMessageInfo>();
	//----------------------------------------------------------
	public void DoMessagePopup(EMessagePopupType ePopupType, string strTitle, string strMessage, UnityAction delOk, UnityAction delCancel)
	{
		SMessageInfo pMessageInfo = new SMessageInfo();
		pMessageInfo.PopupType = ePopupType;
		pMessageInfo.Message = strMessage;
		pMessageInfo.Ok = delOk;
		pMessageInfo.Cancel = delCancel;
		pMessageInfo.Title = strTitle;

		if (m_pCurrentMessageInfo != null)
		{
			m_stackMessageInfo.Push(m_pCurrentMessageInfo);
		}
		PrivMessagePopupShowBox(pMessageInfo);
	}

	public void DoMessagePopupCurrency(EMessagePopupType ePopupType, string strTitle, string strMessage, UnityAction delOk, UnityAction delCancel, ECurrencyType eCurrency, long iCurrencyCount)
	{
		SMessageInfo pMessageInfo = new SMessageInfo();
		pMessageInfo.PopupType = ePopupType;
		pMessageInfo.Message = strMessage;
		pMessageInfo.Ok = delOk;
		pMessageInfo.Cancel = delCancel;
		pMessageInfo.Title = strTitle;
		pMessageInfo.CurrencyType = eCurrency;
		pMessageInfo.CurrencyCount = iCurrencyCount;
		if (m_pCurrentMessageInfo != null)
		{
			m_stackMessageInfo.Push(m_pCurrentMessageInfo);
		}
		PrivMessagePopupShowBox(pMessageInfo);
	}

	//--------------------------------------------------------------
	private SHUIWindowMessageBoxBase FindAndActivateMessageBox(EMessagePopupType ePopupType)
	{
		SHUIWindowMessageBoxBase pBox = null;
		for (int i = 0; i < PopupWindow.Count; i++)
		{
			if (PopupWindow[i].MessageType == ePopupType)
			{
				pBox = PopupWindow[i].MessageBox;
				pBox.DoUIWidgetShowHide(true);
			}
			else
			{
				PopupWindow[i].MessageBox.DoUIWidgetShowHide(false);
			}
		}
		return pBox;
	}

	private void PrivMessagePopupOkCancle(bool bOk)
	{
		m_pCurrentMessageInfo.WidgetInstance.DoUIWidgetShowHide(false);
		if (m_stackMessageInfo.Count > 0)
		{
			PrivMessagePopupShowBox(m_stackMessageInfo.Pop());
		}
		else
		{
			UIManager.Instance.DoUIMgrHide(this);
		}

		if (bOk)
		{
			m_pCurrentMessageInfo.Ok?.Invoke();
		}
		else
		{
			m_pCurrentMessageInfo.Cancel?.Invoke();
		}
		m_pCurrentMessageInfo = null;
	}

	private void PrivMessagePopupShowBox(SMessageInfo pMessageInfo)
	{
		m_pCurrentMessageInfo = pMessageInfo;
		SHUIWindowMessageBoxBase pBox = FindAndActivateMessageBox(pMessageInfo.PopupType);
		m_pCurrentMessageInfo.WidgetInstance = pBox;
		pBox.DoMessageBox(pMessageInfo.Title, pMessageInfo.Message, HandleMessageOk, HandleMessageCancel);

		if (pMessageInfo.CurrencyType != ECurrencyType.None)
		{
			CurrencyCount.SetMonoActive(true);
			CurrencyCount.DoCurrencyCount(pMessageInfo.CurrencyType, pMessageInfo.CurrencyCount);
		}
		else
		{
			CurrencyCount.SetMonoActive(false);
		}
	}

	//-------------------------------------------------------------
	private void HandleMessageOk()
	{
		PrivMessagePopupOkCancle(true);
	}

	public void HandleMessageCancel()
	{
		PrivMessagePopupOkCancle(false);
	}
}
