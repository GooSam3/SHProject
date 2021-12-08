using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ¿¢¼¿ CSVÆ÷¸ä ±â¹Ý
abstract public class CScriptDataTableBase : CScriptDataBase
{
	public class SDataTableLine
	{
		public List<string> contentsList = new List<string>();
	}

	private List<string> m_listHeader = new List<string>();
	private List<SDataTableLine> m_listLine = new List<SDataTableLine>();

	private StringBuilder m_TableNote = new StringBuilder();
	//---------------------------------------------------------
	protected override void OnScriptDataInitialize(string strTextData)
	{
		int iIndex = PrivDataTableReadHeader(strTextData);
		PrivDataTableReadLine(strTextData, iIndex);
		ProtScriptDataClear();
	}

	//---------------------------------------------------------
	public int GetHeaderCount() { return m_listHeader.Count; }
	public int GetLineCount() { return m_listLine.Count; }
	public string GetHeader(int iIndex)
	{
		if (iIndex < m_listHeader.Count)
		{
			return m_listHeader[iIndex];
		}
		return null;
	}
	public SDataTableLine GetLine(int iIndex)
	{
		if (iIndex < m_listLine.Count)
		{
			return m_listLine[iIndex];
		}
		return null;
	}

	//----------------------------------------------------------
	private int PrivDataTableReadHeader(string strTextData)
	{
		m_TableNote.Clear();
		int iIndex = 0;
		for (int i = 0; i < strTextData.Length; i++)
		{
			iIndex++;
			char C = strTextData[i];
			if (C == System.Environment.NewLine[0])
			{
				if (i + 1 < strTextData.Length)
				{
					iIndex++;
				}

				m_listHeader.Add(m_TableNote.ToString());
				m_TableNote.Clear();
				break;
			}
			else if (C == ',')
			{
				m_listHeader.Add(m_TableNote.ToString());
				m_TableNote.Clear();
			}
			else
			{
				m_TableNote.Append(C);
			}
		}

		return iIndex;
	}

	private void PrivDataTableReadLine(string strTextData, int iIndex)
	{
		m_TableNote.Clear();

		int iLineCount = 0;

		SDataTableLine pLineData = new SDataTableLine();

		bool bArray = false;
		for (int i = iIndex; i < strTextData.Length; i++)
		{
			char C = strTextData[i];
			if (C == System.Environment.NewLine[0])
			{
				if (m_TableNote.Length > 0)
				{
					pLineData.contentsList.Add(m_TableNote.ToString());
				}
				m_TableNote.Clear();
				m_listLine.Add(pLineData);
				iLineCount++;
				i++;
				pLineData = new SDataTableLine();
			}
			else if (C == ',')
			{
				if (bArray == false)
				{
					pLineData.contentsList.Add(m_TableNote.ToString());
					m_TableNote.Clear();
				}
				else
				{
					m_TableNote.Append(C);
				}
			}
			else if (C == '"')
			{
				if (bArray)
				{
					bArray = false;
					pLineData.contentsList.Add(m_TableNote.ToString());
					m_TableNote.Clear();
				}
				else
				{
					bArray = true;
				}
			}
			else
			{
				m_TableNote.Append(C);
			}
		}
	}
}
