using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SHUIWindowHeroSkillView : CUIWidgetBase
{
	[SerializeField]
	private CText SkillName = null;
	[SerializeField]
	private CText SkillLevel = null;
	[SerializeField]
	private CImage SkillImage = null;
	[SerializeField]
	private SHUITextSkillFactor SkillDescription = null;
	[SerializeField]
	private CUIWidgetNumberTextChart GoldCost = null;
	[SerializeField]
	private CButton ButtonSkillUpgrade = null;

	private uint m_hSkillID = 0;
	private UnityAction m_delPractice = null;
	//-------------------------------------------------------
	public void DoHeroSkillView(uint hSkillID, uint iLevel, uint iGold, UnityAction delPractice)
	{
		m_hSkillID = hSkillID;
		m_delPractice = delPractice;
		SkillLevel.text = $"수련 단계 : {iLevel}";
		GoldCost.DoTextNumber(iGold);
		SkillDescription.SetTextSkillFactor(hSkillID, iLevel);
		SHScriptTableDescriptionSkill.SDescriptionSkill pDescSkill = SHManagerScriptData.Instance.ExtractTableSkill().GetTableDescSkillCopy(hSkillID);
		SkillName.text = pDescSkill.SkillName;
		SkillDescription.text = pDescSkill.Description;
		PrivHeroSkillViewImage(pDescSkill.IconName);
	}

	//-------------------------------------------------------
	private void PrivHeroSkillViewImage(string strImageName)
	{
		Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.Skill, strImageName);
		if (pSprite)
		{
			SkillImage.sprite = pSprite;
		}
	}

	//-------------------------------------------------------
	public void HandleHeroSkillViewPractice()
	{
		m_delPractice?.Invoke();
	}

	public void HandleHeroSkillViewCloase()
	{
		DoUIWidgetShowHide(false);
	}
}
