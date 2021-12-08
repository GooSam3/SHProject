using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIWidgetContentsAdventure : CUIWidgetBase
{



	//---------------------------------------------------------
	public void HandleContentsAdventureSection1()
	{
		SHScriptTableStage.SStageTable pTable = SHManagerGameDB.Instance.GetGamgDBNextStage(1);
		if (pTable != null)
		{
			SHManagerStage.Instance.DoMgrStageEnter(pTable.StageID);
		}
	}



}
