using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIBuffViewer : CUIWidgetBase
{
	[System.Serializable]
	public class SBuffWidget
	{
		public uint BuffID;
		public GameObject BuffWidget;
	}


	[SerializeField]
	private List<SBuffWidget> BuffWidget = null;

	//--------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		PrivBuffViewerReset();
	}

	//-------------------------------------------------------------------
	public void DoUIBuffViewerUnit(SHUnitBase pUnit)
	{
		PrivBuffViewerReset();
		pUnit.AddUnitBuffNotify(HandleBuffViewerOnOff);
	}

	//---------------------------------------------------------------------
	private void HandleBuffViewerOnOff(bool bOnOff, uint hBuffID)
	{
		GameObject pWidget = FindBuffViewerWidget(hBuffID);
		if (pWidget != null)
		{
			if (bOnOff)
			{
				pWidget.SetActive(true);
			}
			else
			{
				pWidget.SetActive(false);
			}
		}
	}
	
	private GameObject FindBuffViewerWidget(uint hBuffID)
	{
		GameObject pWidget = null;

		for (int i = 0; i < BuffWidget.Count; i++)
		{
			if (BuffWidget[i].BuffID == hBuffID)
			{
				pWidget = BuffWidget[i].BuffWidget;
			}
		}

		return pWidget;
	}

	private void PrivBuffViewerReset()
	{
		for (int i = 0; i < BuffWidget.Count; i++)
		{
			BuffWidget[i].BuffWidget.SetActive(false);
		}
	}
}
