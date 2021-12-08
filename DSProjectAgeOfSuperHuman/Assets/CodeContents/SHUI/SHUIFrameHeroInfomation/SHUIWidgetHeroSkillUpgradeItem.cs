using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetHeroSkillUpgradeItem : CUIWidgetBase
{
    [SerializeField]
    private uint UpgradeID = 0;                 public uint GetHeroSkillUpgradeID() { return UpgradeID; }
    [SerializeField]
    private CImage SkillIcon = null;
    [SerializeField]
    private CText SkillLevel = null;
    [SerializeField]
    private CText SkillName = null;
    [SerializeField]
    private CUIWidgetNumberTextChart PowerCurrent = null;
    [SerializeField]
    private CUIWidgetNumberTextChart PowerNext = null;
    [SerializeField]
    private CUIWidgetNumberTextChart GoldCost = null;

    private uint m_hSkillID = 0;               public uint GetHeroSkillUpgradeSkillID() { return m_hSkillID; }
    private uint m_iPointOrigin = 0;    
    private uint m_iPointConsume = 0;           public uint GetHeroSkillConsumePoint() { return m_iPointConsume; }
    private uint m_iUpgradeValue = 0;
    private uint m_iConsumeGold = 0;
    private SHUIWindowHeroUpgradeSkill m_pSkillProcessor = null;
    //--------------------------------------------------------------
    public void DoHeroSkillUpgradeRefresh(uint hHeroID, SHUIWindowHeroUpgradeSkill pSkillProcessor)
	{
        m_pSkillProcessor = pSkillProcessor;
        m_iPointOrigin = SHManagerGameDB.Instance.GetGameDBHeroStatUpgradePoint(hHeroID, UpgradeID);
        m_iPointConsume = 0;
        SHScriptTableDescriptionHero.SDescriptionHero   pTableHero = SHManagerScriptData.Instance.ExtractTableHero().GetTableDescriptionHero(hHeroID);
        SHScriptTableHeroUpgrade.SHeroUpgradeItem pTableUpgradeItem = SHManagerScriptData.Instance.ExtractTableHeroUpgrade().GetTableHeroUpgrade(UpgradeID);
        m_iUpgradeValue = pTableUpgradeItem.UpgradeValue;
        PrivHeroSkillUpgradeProcessSkill(pTableHero, pTableUpgradeItem.EUpgradeStatType);
        PrivHeroSkillUpgradePoint(m_iPointOrigin);
    } 

    public void DoHeroSkillUpgradePlus()
	{
        HandleHeroSkillPlus();
    }

    //---------------------------------------------------------------
    private void PrivHeroSkillUpgradeIcon(string strIconName)
	{
        Sprite pSprite = SHManagerAtlasLoader.Instance.DoMgrAtlasFindSprite(SHManagerAtlasLoader.EAtlasType.Skill, strIconName);
        if (pSprite)
		{
            SkillIcon.sprite = pSprite;
		}
	}

    private void PrivHeroSkillUpgradeProcessSkill(SHScriptTableDescriptionHero.SDescriptionHero pTableHero, SHScriptTableHeroUpgrade.EUpgradeStatType eUpgradeStatType)
	{
        if (eUpgradeStatType != SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot1 && eUpgradeStatType != SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot2 && eUpgradeStatType != SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot3) return;

        if (eUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot1)
		{
            m_hSkillID = pTableHero.SkillSlot1;
		}
        else if (eUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot2)
		{
            m_hSkillID = pTableHero.SkillSlot2;
		}
        else if (eUpgradeStatType == SHScriptTableHeroUpgrade.EUpgradeStatType.SkillSlot3)
		{
            m_hSkillID = pTableHero.SkillSlot3;
		}

		string strIconName = SHManagerScriptData.Instance.ExtractTableSkill().GetTableDescSkillIcon(m_hSkillID);
        PrivHeroSkillUpgradeIcon(strIconName);
        SkillName.text = SHManagerScriptData.Instance.ExtractTableSkill().GetTableDescSkillName(m_hSkillID);
    }

    private void PrivHeroSkillUpgradePoint(uint iPoint)
	{
        uint iValue = m_iUpgradeValue * iPoint;
        uint iGold = SHManagerScriptData.Instance.ExtractTableHeroUpgrade().GetTableHeroUpgradeCost(UpgradeID, iPoint);
        m_iConsumeGold = iGold;
        PrivHeroSkillUpgradeValue(iPoint, iValue, iValue + m_iUpgradeValue, iGold);
    }

    private void PrivHeroSkillUpgradeValue(uint iLevel, uint iValueCurrent, uint iValueNext, uint iGold)
	{
        SkillLevel.text = string.Format("Lv {0}", iLevel.ToString()); 
        PowerCurrent.DoTextNumber(iValueCurrent);
        PowerNext.DoTextNumber(iValueNext);
        GoldCost.DoTextNumber(iGold);
	}

    //---------------------------------------------------------
    public void HandleHeroSkillPlus()
	{
        if (m_pSkillProcessor.DoHeroUpgradePlus((int)m_iConsumeGold))
		{
            m_iPointConsume++;
            uint iCurrentPoint = m_iPointOrigin + m_iPointConsume;
            PrivHeroSkillUpgradePoint(iCurrentPoint);
            m_pSkillProcessor.DoHeroUpgradeSkillView(m_hSkillID, iCurrentPoint, m_iConsumeGold, false);
		}
	}

    public void HandleHeroSkillView()
	{
        m_pSkillProcessor.DoHeroUpgradeSkillView(m_hSkillID, m_iPointOrigin + m_iPointConsume, m_iConsumeGold, true);
	}
}
