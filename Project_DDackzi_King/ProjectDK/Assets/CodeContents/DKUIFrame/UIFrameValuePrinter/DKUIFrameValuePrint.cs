using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 데미지를 특정 방법으로 출력 
public class DKUIFrameValuePrint : DKUIFrameBase
{
	public enum EPrintType
	{
		DamageNormal,
		DamageCritical,
		HealNormal,
		HealCritical,
		Evade,			// 공격회피, 디버프도 들어가지 않음 
		Block,			// 데미지 감쇄 적용,  디버프는 들어감
	}

	[SerializeField]
	private CUIWidgetTemplate DamageNormal;
	[SerializeField]
	private CUIWidgetTemplate DamageCritical;
	[SerializeField]
	private CUIWidgetTemplate HealNormal;
	[SerializeField]
	private CUIWidgetTemplate HealCritical;
	[SerializeField]
	private CUIWidgetTemplate Evade;
	[SerializeField]
	private CUIWidgetTemplate Block;

	//------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
	}
	//---------------------------------------------------------------------------
	public void DoUIFrameValuePrint(EPrintType ePrintType, uint iValue, Vector3 vecWorldPosition)
	{
		switch(ePrintType)
		{
			case EPrintType.DamageNormal:
				PrivValuePrint(DamageNormal, iValue, vecWorldPosition);
				break;
			case EPrintType.DamageCritical:
				PrivValuePrint(DamageCritical, iValue, vecWorldPosition);
				break;
			case EPrintType.HealNormal:
				PrivValuePrint(HealNormal, iValue, vecWorldPosition);
				break;
			case EPrintType.HealCritical:
				PrivValuePrint(HealCritical, iValue, vecWorldPosition);
				break;
			case EPrintType.Evade:
				PrivValuePrint(Evade, 0, vecWorldPosition);
				break;
			case EPrintType.Block:
				PrivValuePrint(Block, 0, vecWorldPosition);
				break;
		}
	}

	public void DoUIFrameValuePrintClear()
	{
		DamageCritical.DoUIWidgetTemplateReturnAll();
	
	}

	//---------------------------------------------------------------------------
	private void PrivValuePrint(CUIWidgetTemplate pTemplate, uint iValue, Vector3 vecWorldPosition)
	{
		CUIWidgetImageNumber pItem = pTemplate.DoUIWidgetTemplateRequestItem(transform) as CUIWidgetImageNumber;
		List<uint> pDigitList = ExtractValuePrintDigit(iValue);
		pItem.DoImageNumber(pDigitList, vecWorldPosition);
	}

	

	private List<uint> ExtractValuePrintDigit(uint iValue)
	{
		List<uint> pListOut = new List<uint>();
		int iDigitCount = ExtractValuePrintDigitCount(iValue);
		uint iDigitNumber = 1;
		for (uint i = 0; i < iDigitCount; i++)
		{
			uint iDigitValue = (iValue / iDigitNumber) % 10;
			pListOut.Add(iDigitValue);
			iDigitNumber *= 10;
		}

		return pListOut;
	}

	private int ExtractValuePrintDigitCount(uint iValue)
	{
		int iCount = 0;
		uint iNumber = iValue;
		while(true)
		{
			iNumber /= 10;
			iCount++;

			if (iNumber == 0) break;
		}
		return iCount;
	}
}
