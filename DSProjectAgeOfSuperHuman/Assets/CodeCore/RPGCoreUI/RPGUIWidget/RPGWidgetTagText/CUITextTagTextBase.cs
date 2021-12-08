using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
[RequireComponent(typeof(CText))]
public abstract class CUITextTagTextBase : CUIWidgetBase
{
	[SerializeField]
	private char TagOpen = '[';
	[SerializeField]
	private char TagClose = ']';

	private CText m_pUGUIText = null;
	private StringBuilder m_pNote = new StringBuilder(256);
	
	public string text { set { m_pUGUIText.text = value; } get { return m_pUGUIText.text; } }
	//----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pUGUIText = GetComponent<CText>();
		m_pUGUIText.SetTextReference(HandleTagText);
	}

	//-----------------------------------------------------------------
	public string HandleTagText(string strText)
	{
		return PrivTagTextParse(strText);
	}
	//--------------------------------------------------------------------
	private string PrivTagTextParse(string strText)
	{
		m_pNote.Clear();
		
		bool bTagOpen = false;
		int iOpneStart = 0;
		int iOpenEnd = 0;
		int iIndex = 0;
		for (int i = 0; i < strText.Length; i++)
		{
			if (bTagOpen)
			{
				if (strText[i] == TagClose)
				{
					if (i + 1 < strText.Length)
					{
						if (strText[i+1] == TagClose)
						{
							bTagOpen = false;
							iOpenEnd = i + 2;
							string strReplace = OnUITagText(m_pNote.ToString(), iIndex);
							m_pNote.Clear();
							strText = strText.Remove(iOpneStart, iOpenEnd - iOpneStart);
							strText = strText.Insert(iOpneStart, strReplace);
							i = iOpneStart;
							iIndex++;
						}
						else
						{
							m_pNote.Append(strText[i]);
						}
					}
					else
					{
						m_pNote.Append(strText[i]);
					}
				}
				else
				{
					m_pNote.Append(strText[i]);
				}				
			}
			else
			{
				if (strText[i] == TagOpen)
				{
					if (i + 1 < strText.Length)
					{
						if (strText[i + 1] == TagOpen)
						{
							bTagOpen = true;
							iOpneStart = i;
							i++;
						}
					}
				}
			}
		}
		return strText;
	}

	protected virtual string OnUITagText(string strContents, int iIndex) { return string.Empty; }
}
