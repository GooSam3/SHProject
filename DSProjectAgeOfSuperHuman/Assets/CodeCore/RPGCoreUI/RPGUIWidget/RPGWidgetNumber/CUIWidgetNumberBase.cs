using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CText))]
public abstract class CUIWidgetNumberBase : CUIWidgetBase
{
	//-------------------------------------------------------------------
	public void DoTextNumber(long iNumber)
	{
		List<long> pListDigit = ExtractValuePrintDigit(iNumber);
		OnUIWidgetNumber(iNumber, pListDigit);
	}

	public void DoTextNumberPercent(long iNumber)
	{

	}

	private List<long> ExtractValuePrintDigit(long iValue)
	{
		List<long> pListOut = new List<long>();
		if (iValue == 0)
		{
			pListOut.Add(0);
			return pListOut;
		}

		int iDigitCount = ExtractValuePrintDigitCount(iValue);
		for (int i = iDigitCount; i >= 1; i--)
		{
			long iDigitNumber = ExtractValueSquare(10, i);
			long iDigitValue = iValue / iDigitNumber  % 10;
			pListOut.Add(iDigitValue);
		}
		return pListOut;
	}

	private int ExtractValuePrintDigitCount(long iValue)
	{
		int iCount = 0;
		long iNumber = iValue;
		while (true)
		{
			iNumber /= 10;
			iCount++;

			if (iNumber == 0) break;
		}
		return iCount;
	}

	private long ExtractValueSquare(long iValue, int iSquare)
	{
		long iResult = 1;
		for (int i = 1; i < iSquare; i++)
		{
			iResult *= iValue;
		}

		return iResult;
	}

	//-------------------------------------------------------
	protected virtual void OnUIWidgetNumber(long iNumber, List<long> pListDigit) { }
}
