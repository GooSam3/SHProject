using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class UIWorldMapMonsterDrop : CUGUIWidgetBase
{
	[SerializeField] ZText Title = null;
	[SerializeField] ZText MonsterName = null;
	[SerializeField] ZImage MonsterAttribute = null;
	[SerializeField] UIScrollMonsterDrop	MonsterList = null;
	[SerializeField] UIScrollDropItem		DropItem = null;
	[SerializeField] UIScrollRuneDrop		DropRune = null;
	[SerializeField] UIPanelSelectRuneDrop  DropRunePanel = null;

	private List<UIFrameWorldMap.SMonsterInfo> mMonsterList = null;
	//--------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		DropRunePanel.ImportRuneDropListScroll(DropRune);
	}

	//--------------------------------------------------------------
	public void DoMonsterDrop(Portal_Table _portalTable, List<UIFrameWorldMap.SMonsterInfo> _monsterList)
	{
		Title.text = UIFrameWorldMap.ConvertPortalName(_portalTable);
		mMonsterList = _monsterList;
		MonsterList.DoMonsterDrop(_monsterList, this);
		DropRunePanel.DoSelectRuneDropRefresh();
	}

	public void DoMonsterDropList(uint _monsterTID)
	{
		for (int i = 0; i < mMonsterList.Count; i++)
		{
			UIFrameWorldMap.SMonsterInfo monsterInfo = mMonsterList[i];
			if (monsterInfo.MontsetTID == _monsterTID)
			{
				MonsterName.text = monsterInfo.MonsterName;
				MonsterAttribute.sprite = ExtractMonsterAttributeSprite(monsterInfo.MonsterTable.AttributeType);
				DropItem.DoDropItemList(monsterInfo.ListMonsterDropInfo);
				break;
			}
		}
	}

	public static Sprite ExtractMonsterAttributeSprite(E_UnitAttributeType _attType)
	{
		string attImageName = "";

		switch(_attType)
		{
			case E_UnitAttributeType.Dark:
				attImageName = "icon_element_dark_s";
				break;
			case E_UnitAttributeType.Fire:
				attImageName = "icon_element_fire_s";
				break;
			case E_UnitAttributeType.Electric:
				attImageName = "icon_element_electric_s";
				break;
			case E_UnitAttributeType.Light:
				attImageName = "icon_element_light_s";
				break;
			case E_UnitAttributeType.Water:
				attImageName = "icon_element_water_s";
				break;
		}
		return ZManagerUIPreset.Instance.GetSprite(attImageName);
	}

	//-----------------------------------------------------------------
	public void HandleRuneSetDropOpen()
	{
		DropRunePanel.DoUIWidgetShowHide(true);
	}

}
