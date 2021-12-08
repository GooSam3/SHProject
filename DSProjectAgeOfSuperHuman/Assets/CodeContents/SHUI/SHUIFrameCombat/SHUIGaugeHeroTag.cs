using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIGaugeHeroTag : CUIWidgetGaugeBase
{
	[SerializeField]
	private CImage HeroTagFace = null;
	[SerializeField]
	private CText TagCoolPercent = null;
	[SerializeField]
	private CText TagCoolCount = null;

	private SHUnitHero m_pUpdateHero = null;
	private float m_fTagCoolTimeMax = 0;
	private bool m_bCoolTime = false;			public bool GetUIGaugeCoolTime() { return m_bCoolTime; }
	private Color m_colDisableColor = Color.white;
	//------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_colDisableColor = HeroTagFace.color;
	}

	protected override void OnUIWidgetGaugeUpdate()
	{
		base.OnUIWidgetGaugeUpdate();

		if (m_pUpdateHero != null)
		{
			UpdateUIGaugeHero(m_pUpdateHero);
		}
	}

	public void DoUIGaugeHero(SHUnitHero pUpdateHero)
	{
		m_bCoolTime = false;
		PrivHeroTagFaceEnable(true);
		m_pUpdateHero = pUpdateHero;
		PrivHeroTagFace(m_pUpdateHero.GetUnitID());
		m_fTagCoolTimeMax = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.TagCoolTime);
		ProtGaugeReset(m_fTagCoolTimeMax);
		UpdateUIGaugeHero(pUpdateHero);
	}

	public Vector3 GetUIGaugeHeroFaceCenterPosition()
	{
		Vector3 vecScreenPos = Vector3.zero;
		vecScreenPos = HeroTagFace.transform.position;
		return vecScreenPos;
	}
	//-------------------------------------------------------
	private void UpdateUIGaugeHero(SHUnitHero pUpdateHero)
	{
		float fCoolTime = pUpdateHero.ISHGetTagCoolTime();
		if (TagCoolPercent != null)
		{
			UpdateUIGaugeCoolTime(fCoolTime);
		}

		if (TagCoolCount != null)
		{
			UpdateUIGaugeTagCount(fCoolTime);
			UpdateUIGaugeHeroFace(fCoolTime);
		}
	}

	private void UpdateUIGaugeCoolTime(float fCoolTime)
	{
		ProtGaugeValueUpdate(m_fTagCoolTimeMax - fCoolTime);
		int iValueReversePercent = (int)((1f - (fCoolTime / m_fTagCoolTimeMax)) * 100f);
		TagCoolPercent.text = string.Format("{0}%", iValueReversePercent.ToString());
	}

	private void UpdateUIGaugeTagCount(float fCoolTime)
	{
		if (fCoolTime == 0)
		{
			TagCoolCount.gameObject.SetActive(false);
		}
		else
		{
			TagCoolCount.gameObject.SetActive(true);
		}

		int iCoolTime = (int)fCoolTime + 1;
		TagCoolCount.text = iCoolTime.ToString();
	}

	private void UpdateUIGaugeHeroFace(float fCoolTime)
	{
		if (fCoolTime == 0)
		{
			if (m_bCoolTime)
			{
				PrivHeroTagFaceEnable(true);
			}

			m_bCoolTime = false;
		}
		else
		{
			if (m_bCoolTime == false)
			{
				PrivHeroTagFaceEnable(false);
			}
			m_bCoolTime = true;
		}

	}

	private void PrivHeroTagFace(uint hHeroID)
	{
		SHScriptTableDescriptionHero.SDescriptionHero pTableHero = SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHero(hHeroID);
		Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.FaceHero, pTableHero.UIFaceName);
		if (pSprite != null)
		{
			HeroTagFace.sprite = pSprite;
		}
	}

	private void PrivHeroTagFaceEnable(bool bEnable)
	{
		if (bEnable)
		{
			HeroTagFace.color = Color.white;
		}
		else
		{
			HeroTagFace.color = m_colDisableColor;
		}
	}
}
