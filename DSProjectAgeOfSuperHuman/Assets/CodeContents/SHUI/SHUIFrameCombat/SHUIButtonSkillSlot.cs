using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIButtonSkillSlot : SHUIButtonNormal
{
	[SerializeField]
	private CImage SkillImage = null;
	[SerializeField]
	private CText CoolTime = null;
	[SerializeField]
	private SHEffectParticleNormal CoolTimeEffect = null;
	[SerializeField]
	private KeyCode simulationKey;

	private uint m_hSkillID = 0;
	private string m_strCoolTimeName;
	private bool m_bCoolTimeReady = false;
	private SHUnitBase m_pHero = null;
	private Color m_colDisableColor = Color.white;
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_colDisableColor = SkillImage.color;
	}

	private void Update()
	{
		if (Input.GetKeyDown(simulationKey))
		{
			ProtButtonActionPress();
		}

		if (m_pHero != null)
		{
			UpdateUIButtonCoolTime();
		}
	}

	//-------------------------------------------------------------

	public void DoUIButtonSkillInfo(SHUnitBase pHero, uint hSkillID, string strCoolTimeName)
	{
		m_hSkillID = hSkillID;
		m_strCoolTimeName = strCoolTimeName;
		m_pHero = pHero;
		string strSkillImage = SHManagerScriptData.Instance.ExtractTableSkill().GetTableDescSkillIcon(hSkillID);
		PrivUIButtonSkillImage(strSkillImage);
		UpdateUIButtonCoolTime();
	}

	public void DoUIButtonSkillFocus(uint hSkillID)
	{
		if (m_hSkillID == hSkillID && m_bCoolTimeReady == false)
		{
			CoolTimeEffect?.DoEffectStart(null);
		}
	}

	//---------------------------------------------------------------
	private void PrivUIButtonSkillImage(string strSkillImage)
	{
		Sprite pImage = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.Skill, strSkillImage);
		if (pImage)
		{
			SkillImage.sprite = pImage;
		}
	}

	private void UpdateUIButtonCoolTime()
	{
		float fCoolTime = m_pHero.IGetSkillCoolTime(m_strCoolTimeName);
		if (fCoolTime == 0)
		{
			if (m_bCoolTimeReady == false)
			{
				m_bCoolTimeReady = true;
				CoolTime.gameObject.SetActive(false);
				SkillImage.color = Color.white;
			}
		}
		else if (fCoolTime > 0)
		{
			if (m_bCoolTimeReady == true)
			{
				m_bCoolTimeReady = false;
				CoolTime.gameObject.SetActive(true);
				SkillImage.color = m_colDisableColor;
			}
			PrivUIButtonCoolTimeText(fCoolTime);
		}
	}


	private void PrivUIButtonCoolTimeText(float fCoolTime)
	{
		int iCoolTime = (int)fCoolTime;
		CoolTime.text = iCoolTime.ToString();
	}


	//------------------------------------------------------------------
	

}
