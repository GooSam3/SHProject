using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SHUIWidgetHeroStatus : CUIWidgetBase
{
	[SerializeField]
	private CImage	ReaderFace = null;
	[SerializeField]
	private Text		ReaderLevel = null;
	[SerializeField]
	private CSlider	ReaderEXP = null;
	[SerializeField]
	private CText		PlayerName = null;
	[SerializeField]
	private CUIWidgetNumberTextChart DeckPower = null;

	private uint m_hReaderID = 0;
	//-------------------------------------------------------
	public void DoHeroStatusRefresh(uint hReaderID)
	{
		SHScriptTableDescriptionHero.SDescriptionHero pReaderTable = SHManagerGameDB.Instance.GetGameDBReaderTable();
		if (pReaderTable == null)
		{
			// Error!
			return;
		}
		m_hReaderID = hReaderID;
		int iHeroLevel = SHManagerGameDB.Instance.GetGameDBHeroLevel(m_hReaderID);
		PrivHeroStatusFace(pReaderTable.UIFaceName);
		PrivHeroStatusLevel(iHeroLevel);
		PrivHeroStatusPlayerName(SHManagerGameDB.Instance.GetGameDBPlayerName());
		uint iEXPMax = SHManagerScriptData.Instance.ExtractTableLevel().GetTableLevelEXP(iHeroLevel);
		uint iEXPCurrent = SHManagerGameDB.Instance.GetGameDBHeroEXP(m_hReaderID);
		PrivHeroStatusHeroEXP(iEXPCurrent, iEXPMax);
		uint iPower = (uint)SHManagerGameDB.Instance.GetGameDBPowerDeck();
		PrivHeroStatusDeckPower(iPower);
	}

	//--------------------------------------------------------
	private void PrivHeroStatusFace(string strFaceName)
	{
		Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.FaceHero, strFaceName);
		if (pSprite)
		{
			ReaderFace.sprite = pSprite;
		}
	}

	private void PrivHeroStatusLevel(int iLevel)
	{
		ReaderLevel.text = iLevel.ToString();
	}

	private void PrivHeroStatusPlayerName(string strPlayerName)
	{
		PlayerName.text = strPlayerName;
	}

	private void PrivHeroStatusHeroEXP(uint iCurrent, uint iMax)
	{
		ReaderEXP.value = (float)iCurrent / (float)iMax;
	}

	private void PrivHeroStatusDeckPower(uint iPower)
	{
		DeckPower.DoTextNumber(iPower);
	}


}
