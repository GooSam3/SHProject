using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetHeroEquipInfo : CUIWidgetBase
{
    [SerializeField]
    private CText EquipName = null;
    [SerializeField]
    private CText EquipLevel = null;
    [SerializeField]
    private CText EquipDescription = null;
    [SerializeField]
    private SHUIButtonToggleNormal EquipToggle = null;

    [SerializeField]
    private SHUIIconEquipmentInventory EquipIcon = null;

    private SItemData m_pItemDataCurrent = null;
    private uint m_hHeroID = 0;
    //--------------------------------------------------------
    public void DoHeroEquipViewItem(SItemData pItemData, uint hHeroID)
	{
        m_hHeroID = hHeroID;
        m_pItemDataCurrent = pItemData;
        EquipIcon.DoIconItemData(pItemData, null);
        EquipName.text = pItemData.ItemTable.ItemName;
        EquipLevel.text = string.Format("강화 단계 : {0}", pItemData.ItemIDB.ItemLevel);
        EquipDescription.text = pItemData.ItemTable.ItemDescription;

        if (pItemData.ItemEquip)
		{
			EquipToggle.DoButtonToggleOn(false);
		}
        else
		{
            EquipToggle.DoButtonToggleOff(false);
		}
	}

    public void DoHeroEquipViewItemRefresh()
	{
        if (m_pItemDataCurrent == null) return;

        SItemData pNewData = SHManagerGameDB.Instance.GetGameDBHeroEquip(m_hHeroID, m_pItemDataCurrent.ItemID);
        DoHeroEquipViewItem(pNewData, m_hHeroID);
	}

    //--------------------------------------------------------
    public void HandleHeroEquipInfoClose()
	{
        DoUIWidgetShowHide(false);
	}

    public void HandleHeroEquipMount()
	{
        SHManagerGameSession.Instance.RequestEquipMount(m_hHeroID, m_pItemDataCurrent.ItemID);
	}

    public void HandleHeroEquipUnMount()
	{
        SHManagerGameSession.Instance.RequestEquipUnMount(m_hHeroID, m_pItemDataCurrent.ItemID);
    }

}
