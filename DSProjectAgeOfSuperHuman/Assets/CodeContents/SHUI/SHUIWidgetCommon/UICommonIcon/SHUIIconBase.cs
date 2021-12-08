using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SHUIIconBase : CUIWidgetIconBase
{
	private CText m_pLevelText = null;
	private uint m_hItemID = 0;				public uint GetSHIconTID() { return m_hItemID; }
	private ulong m_hItemSID = 0;				public ulong GetSHIconSID() { return m_hItemSID; }
	private UnityAction<SHUIIconBase>			m_delIconClick = null;
	//----------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		GameObject pSticker = ProtIconStickerEnable(EIconStickerType.RightTop, false);
		if (pSticker)
		{
			m_pLevelText = pSticker.GetComponent<CText>();
		}
	}

	protected override void OnUIIconClick(Vector2 vecPosition)
	{
		base.OnUIIconClick(vecPosition);
		m_delIconClick?.Invoke(this);
	}
	//----------------------------------------------------------------------------
	public void DoUIIconReset()
	{
		ProtIconReset();
	}

	public void DoUIIconFocus(bool bOn)
	{
		ProtIconFocus(bOn);
	}

	//----------------------------------------------------------------------------
	protected void ProtSHIconLevel(int iLevel)
	{
		if (iLevel == 0)
		{
			ProtIconStickerEnable(EIconStickerType.RightTop, false);
			return;
		}
		if (m_pLevelText == null) return;
		ProtIconStickerEnable(EIconStickerType.RightTop, true);
		m_pLevelText.text = string.Format("+{0}", iLevel.ToString());
	}

	protected void ProtSHIconValue(int iValue)
	{
		if (iValue == 0)
		{
			ProtIconStickerEnable(EIconStickerType.RightTop, false);
			return;
		}
		if (m_pLevelText == null) return;
		ProtIconStickerEnable(EIconStickerType.RightTop, true);

		int iFilter = iValue / 10000;
		if (iFilter > 0)
		{
			m_pLevelText.text = string.Format("{0}¸¸", iFilter.ToString());
		}
		else
		{
			m_pLevelText.text = string.Format("{0}", iValue);
		}
	}

	protected void ProtSHIconBody(string strIconName, EItemGradeUI eGrade, int iCountMin, int iCountMax)
	{
		Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.Icon, strIconName);
		if (pSprite)
		{
			ProtIconSetting(pSprite, (int)eGrade, iCountMin, iCountMax);
		}
	}

	protected void ProtSHIconInfo(uint hItemID, ulong hItemSessionID, UnityAction<SHUIIconBase> delIconClick)
	{
		m_hItemID = hItemID;
		m_hItemSID = hItemSessionID;
		m_delIconClick = delIconClick;
	}

}
