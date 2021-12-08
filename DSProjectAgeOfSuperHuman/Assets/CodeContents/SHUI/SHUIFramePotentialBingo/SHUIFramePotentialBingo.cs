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
		UIManager.Instance.DoUIMgrMessagePopupCurrency(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "잠재력 개방", "잠재력을 다시 개방 하시겠습니까? \n잠긴 슬롯은 갱신되지 않습니다.", ECurrencyType.Gold, 120000, ()=> { 
			// OK
		}, ()=> {
			// Cancel

		});
	}
}
