using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class UIPanelSelectRuneDrop : CUIWidgetPopupBase
{
	public class SRuneDropInfo
	{
		public E_RuneSetType RuneSetType = E_RuneSetType.None;
		public string		   RuneName = null;
		public string		   RuneIcon = null;
		public bool		   Selected = false;
	}

	private UIScrollRuneDrop mScrollRuneDrop = null;
	private SortedList<E_RuneSetType, SRuneDropInfo> m_listRuneSetDrop = new SortedList<E_RuneSetType, SRuneDropInfo>();
	private List<UIPanelSelectRuneDropItem> m_listRuneSetDropItem = new List<UIPanelSelectRuneDropItem>();
	//--------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);

		GetComponentsInChildren<UIPanelSelectRuneDropItem>(true, m_listRuneSetDropItem);

		for (int i = 0; i < m_listRuneSetDropItem.Count; i++)
		{
			m_listRuneSetDropItem[i].SetRuneDropParents(this);
		}

		InitializeRuneSet();
	}

	protected override void OnUIWidgetShowHide(bool _Show)
	{
		base.OnUIWidgetShowHide(_Show);
		if (_Show)
		{
			LoadRuneSetDropItem();
			RefreshRuneSetDropItem();
		}

	}
	//---------------------------------------------------------------
	public void ImportRuneDropOnOff(E_RuneSetType _runeType, bool _on)
	{
		if (m_listRuneSetDrop.ContainsKey(_runeType))
		{
			m_listRuneSetDrop[_runeType].Selected = _on;
		}
	}

	public void ImportRuneDropListScroll(UIScrollRuneDrop _runeListScroll)
	{
		mScrollRuneDrop = _runeListScroll;
	}
	//-----------------------------------------------------------------

	public void DoSelectRuneDropRefresh()
	{
		LoadRuneSetDropItem();
		RefreshRuneSetDropScroll();
	}

	//--------------------------------------------------------------
	private void InitializeRuneSet()
	{
		m_listRuneSetDrop.Clear();
		for(int i = 1; i <= (int)E_RuneSetType.Mana; i++)
		{
			E_RuneSetType runeType = (E_RuneSetType)i;
			SRuneDropInfo newInfo = new SRuneDropInfo();
			newInfo.RuneSetType = runeType;
			switch(runeType)
			{
				case E_RuneSetType.Physical:
					newInfo.RuneIcon = "icon_pet_con";
					newInfo.RuneName = "Physical_Pet_Equip_Name";
					break;
				case E_RuneSetType.Attack:
					newInfo.RuneIcon = "icon_pet_atk";
					newInfo.RuneName = "Attack_Pet_Equip_Name";
					break;
				case E_RuneSetType.MagicAttack:
					newInfo.RuneIcon = "icon_pet_mag";
					newInfo.RuneName = "MagicAttack_Pet_Equip_Name";
					break;
				case E_RuneSetType.Defense:
					newInfo.RuneIcon = "icon_pet_def";
					newInfo.RuneName = "Defense_Pet_Equip_Name";
					break;
				case E_RuneSetType.Resistance:
					newInfo.RuneIcon = "icon_pet_mess_reg_up";
					newInfo.RuneName = "Resistance_Pet_Equip_Name";
					break;
				case E_RuneSetType.Enemy:
					newInfo.RuneIcon = "icon_pet_mess_reg_down";
					newInfo.RuneName = "Enemy_Pet_Equip_Name";
					break;
				case E_RuneSetType.IronWall:
					newInfo.RuneIcon = "icon_pet_dmg_reduce";
					newInfo.RuneName = "IronWall_Pet_Equip_Name";
					break;
				case E_RuneSetType.Punitive:
					newInfo.RuneIcon = "icon_pet_dmg_reduce_break";
					newInfo.RuneName = "Punitive_Pet_Equip_Name";
					break;
				case E_RuneSetType.Evasive:
					newInfo.RuneIcon = "icon_pet_dodge";
					newInfo.RuneName = "Evasive_Pet_Equip_Name";
					break;
				case E_RuneSetType.Recovery:
					newInfo.RuneIcon = "icon_pet_heal";
					newInfo.RuneName = "Recovery_Pet_Equip_Name";
					break;
				case E_RuneSetType.FastPaced:
					newInfo.RuneIcon = "icon_pet_atk_spd";
					newInfo.RuneName = "FastPaced_Pet_Equip_Name";
					break;
				case E_RuneSetType.Swift:
					newInfo.RuneIcon = "icon_pet_move_spd";
					newInfo.RuneName = "Swift_Pet_Equip_Name";
					break;
				case E_RuneSetType.Spot:
					newInfo.RuneIcon = "icon_pet_accu";
					newInfo.RuneName = "Spot_Pet_Equip_Name";
					break;
				case E_RuneSetType.Protect:
					newInfo.RuneIcon = "icon_pet_reg";
					newInfo.RuneName = "Protect_Pet_Equip_Name";
					break;
				case E_RuneSetType.Mana:
					newInfo.RuneIcon = "icon_pet_mana";
					newInfo.RuneName = "Mana_Pet_Equip_Name";
					break;
			}
			m_listRuneSetDrop.Add(runeType, newInfo);
		}
	}

	private void LoadRuneSetDropItem()
	{
		if (ZNet.Data.Me.FindCurUserData != null)
		{
			SortedList<E_RuneSetType, bool> listServerData = ZNet.Data.Me.FindCurUserData.ListDropRuneSetType;

			for (int i = 0; i < listServerData.Keys.Count; i++)
			{
				E_RuneSetType key = listServerData.Keys[i];
				if (m_listRuneSetDrop.ContainsKey(key))
				{
					SRuneDropInfo dropInfo = m_listRuneSetDrop[key];
					dropInfo.Selected = listServerData.Values[i];
				}
			}
		}	
	}

	private void RefreshRuneSetDropItem()
	{
		for (int i = 0; i < m_listRuneSetDrop.Values.Count; i++)
		{
			if (i < m_listRuneSetDropItem.Count)
			{
				m_listRuneSetDropItem[i].DoRuneSetDropInfo(m_listRuneSetDrop.Values[i]);
			}
		}
	}

	private void RefreshRuneSetDropScroll()
	{
		List<string> runeImageList = new List<string>();
		for (int i = 0; i < m_listRuneSetDrop.Values.Count; i++)
		{
			if (m_listRuneSetDrop.Values[i].Selected)
			{
				runeImageList.Add(m_listRuneSetDrop.Values[i].RuneIcon);
			}
		}
		mScrollRuneDrop.DoRuneDropList(runeImageList);
	}

	

	private void SaveRuneSetDropInfo()
	{
		SortedList<E_RuneSetType, bool> listRuneSet = new SortedList<E_RuneSetType, bool>();
		for (int i = 0; i < m_listRuneSetDrop.Count; i++)
		{
			listRuneSet.Add(m_listRuneSetDrop.Keys[i], m_listRuneSetDrop.Values[i].Selected);
		}

		if (ZNet.Data.Me.FindCurUserData != null)
		{
			ulong runeBitMask = ZNet.Data.Me.FindCurUserData.ExtractSelectedDropRunType(listRuneSet);
			ZWebManager.Instance.WebGame.REQ_RuneSetOption(runeBitMask, () =>
			{
				DoUIWidgetShowHide(false);
				RefreshRuneSetDropScroll();
			});
		}
	}

	//--------------------------------------------------------------
	public void HandleRuneSetDropSelectAll()
	{
		for (int i = 0; i < m_listRuneSetDrop.Values.Count; i++)
		{
			m_listRuneSetDrop.Values[i].Selected = true;
		}
		RefreshRuneSetDropItem();
	}

	public void HandleRuneSetDropDeSelectAll()
	{
		for (int i = 0; i < m_listRuneSetDrop.Values.Count; i++)
		{
			m_listRuneSetDrop.Values[i].Selected = false;
		}
		RefreshRuneSetDropItem();
	}

	public void HandleRuneSetDropConfirm()
	{
		bool checkSelect = false;
		for (int i = 0; i < m_listRuneSetDrop.Values.Count; i++)
		{
			if (m_listRuneSetDrop.Values[i].Selected)
			{
				checkSelect = true;
				break;
			}
		}
		if (checkSelect == false)
		{
			UICommon.OpenSystemPopup_One("Select_Get_Rune", "RuneDrop_SelectWarn", "OK");
		}
		else
		{
			SaveRuneSetDropInfo();
		}
	}

	public void HandleRuneSetDropClose()
	{
		DoUIWidgetShowHide(false);
	}


}


/*
Physical
Attack
MagicAttack
Defense
Resistance
Enemy
IronWall
Punitive
Evasive
Recovery
FastPaced
Swift
Spot
Protect
Mana
*/

/*
icon_pet_con
icon_pet_atk
icon_pet_mag
icon_pet_def
icon_pet_mess_reg_up
icon_pet_mess_reg_down
icon_pet_dmg_reduce
icon_pet_dmg_reduce_break
icon_pet_dodge
icon_pet_heal
icon_pet_atk_spd
icon_pet_move_spd
icon_pet_accu
icon_pet_reg
icon_pet_mana
*/

/*
Physical_Pet_Equip_Name
Attack_Pet_Equip_Name
MagicAttack_Pet_Equip_Name
Defense_Pet_Equip_Name
Resistance_Pet_Equip_Name
Enemy_Pet_Equip_Name
IronWall_Pet_Equip_Name
Punitive_Pet_Equip_Name
Evasive_Pet_Equip_Name
Recovery_Pet_Equip_Name
FastPaced_Pet_Equip_Name
Swift_Pet_Equip_Name
Spot_Pet_Equip_Name
Protect_Pet_Equip_Name
Mana_Pet_Equip_Name
 */

/*
Select_Get_Rune Text 획득 장식 선택
Select_Get_Rune_Des Text 선택된 장식이 획득됩니다.
RuneDrop_SelectWarn Text 한 개 이상의 장식을 선택해 주세요.
*/