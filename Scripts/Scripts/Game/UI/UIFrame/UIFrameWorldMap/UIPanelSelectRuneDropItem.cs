using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class UIPanelSelectRuneDropItem : CUGUIWidgetBase
{	
	[SerializeField] ZUIButtonToggle	ButtonToggle = null;

	private GameDB.E_RuneSetType mRuneSetType = E_RuneSetType.None;
	private UIPanelSelectRuneDrop mRuneDropParents = null;
	//---------------------------------------------------
	public void SetRuneDropParents(UIPanelSelectRuneDrop _runeDropParents)
	{
		mRuneDropParents = _runeDropParents;
	}

	//---------------------------------------------------
	public void DoRuneSetDropInfo(UIPanelSelectRuneDrop.SRuneDropInfo _runeDropInfo)
	{
		ButtonToggle.gameObject.SetActive(true);
		Sprite runeSprite = ZManagerUIPreset.Instance.GetSprite(_runeDropInfo.RuneIcon);
		if (runeSprite != null)
		{
			ButtonToggle.SetToggleSprite(runeSprite);
		}
		ButtonToggle.SetUIWidgetText(_runeDropInfo.RuneName);
		mRuneSetType = _runeDropInfo.RuneSetType;

		if (_runeDropInfo.Selected)
		{
			ButtonToggle.DoToggleAction(true);
		}
		else
		{
			ButtonToggle.DoToggleAction(false);
		}
	}

	//---------------------------------------------------
	public void HandleRuneDropSelect()
	{
		mRuneDropParents.ImportRuneDropOnOff(mRuneSetType, true);
	}

	public void HandleRuneDropDeSelect()
	{
		mRuneDropParents.ImportRuneDropOnOff(mRuneSetType, false);
	}
}
