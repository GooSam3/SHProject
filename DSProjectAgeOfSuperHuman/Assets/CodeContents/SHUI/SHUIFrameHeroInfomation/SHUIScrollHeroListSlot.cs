using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SHUIScrollHeroListSlot : CUIWidgetTemplateItemBase
{
	[SerializeField]
	private CImage ImgLock = null;
	[SerializeField]
	private CImage HeroFace = null;
	[SerializeField]
	private CText HeroName = null;
	[SerializeField]
	private CText HeroLevel = null;

	private uint m_hHeroID = 0;			public uint GetHeroListSlotID() { return m_hHeroID; }
	private SHUIButtonRadioNormal m_pButtonRadio = null;
	//----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		ImgLock.gameObject.SetActive(false);
		m_pButtonRadio = GetComponent<SHUIButtonRadioNormal>(); 
	}

	//----------------------------------------------------------
	public void DoUIHeroListSlot(uint hHeroID, int iLevel, string strFaceName, string strHeroName, bool bLock)
	{
		m_hHeroID = hHeroID;
		Sprite pHeroFace = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.FaceHero, strFaceName);
		if (pHeroFace)
		{
			HeroFace.sprite = pHeroFace;
		}
		HeroName.text = strHeroName;
		HeroLevel.text = string.Format("Lv {0}", iLevel.ToString());
		PrivHeroListSlotLock(bLock);
	}

	public void DoUIHeroListSlotSelect()
	{
		m_pButtonRadio.DoButtonToggleOn(false);
	}

	//------------------------------------------------------------
	private void PrivHeroListSlotLock(bool bLock)
	{
		if (bLock)
		{
			m_pButtonRadio.DoButtonToggleOff(false);
			ImgLock.gameObject.SetActive(true);
		}
		else
		{
			ImgLock.gameObject.SetActive(false);
		}
	}


	//----------------------------------------------------------
	public void HandleHeroListSlot()
	{
		UIManager.Instance.DoUIMgrFind<SHUIFrameNavigationBar>().DoUINavigationTabRefresh(m_hHeroID);
	}

}
