using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUIManager : CManagerUILocalizedBase
{	public static new DKUIManager Instance	{get { return CManagerUIFrameBase.Instance as DKUIManager; }}

	//--------------------------------------------------------------------
	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		PrivUIManagerDefaultShowUI();
	}

	//---------------------------------------------------------------------
	public void DoUIFrameShow(string strFrameName)
	{
		ProtMgrUIFrameFocusShow(strFrameName);
	}

	public void DoUIFrameValuePrint(DKUIFrameValuePrint.EPrintType ePrintType, int iValue, Vector3 vecWorldPosition)
	{
		DKUIFrameValuePrint pUIFrame = FindUIFrame(nameof(DKUIFrameValuePrint)) as DKUIFrameValuePrint;
		if (pUIFrame != null)
		{
			pUIFrame.DoUIFrameValuePrint(ePrintType, (uint)iValue, vecWorldPosition);
		}
	}

	public void DoUIFrameNameTagRegist(DKUnitBase pUnit, DKUIFrameNameTag.ENameTagType eNameTagType)
	{
		DKUIFrameNameTag pUIFrame = FindUIFrame(nameof(DKUIFrameNameTag)) as DKUIFrameNameTag;
		if (pUIFrame != null)
		{
			pUIFrame.DoUIFrameNameTagMake(pUnit, eNameTagType);
		}
	}

	public void DoUIFrameNameTagUnRegist(DKUnitBase pUnit)
	{
		DKUIFrameNameTag pUIFrame = FindUIFrame(nameof(DKUIFrameNameTag)) as DKUIFrameNameTag;
		if (pUIFrame != null)
		{
			pUIFrame.DoUIFrameNameTagRemove(pUnit);
		}
	}
	//---------------------------------------------------------------------
	private void PrivUIManagerDefaultShowUI()
	{
		
	}
}
