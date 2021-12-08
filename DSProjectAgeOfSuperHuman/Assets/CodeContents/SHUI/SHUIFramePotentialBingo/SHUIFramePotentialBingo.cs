using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPotentialBingoColor
{
	None,
	Red,
	Blue,
	Green,
}

public class SHUIFramePotentialBingo : SHUIFrameBase
{

	//---------------------------------------------------------------
	protected override void OnSHUIFrameCloseSelf()
	{
		UIManager.Instance.DoUIMgrHide<SHUIFrameNavigationBar>();
	}
	//--------------------------------------------------------------
	public void DoUIFramePotentialBingo(uint hHeroID)
	{
		
	}
	//--------------------------------------------------------------

	public void HandlePotentialReOpen()
	{
		UIManager.Instance.DoUIMgrMessagePopupCurrency(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "����� ����", "������� �ٽ� ���� �Ͻðڽ��ϱ�? \n��� ������ ���ŵ��� �ʽ��ϴ�.", ECurrencyType.Gold, 120000, ()=> { 
			// OK
		}, ()=> {
			// Cancel

		});
	}
}
