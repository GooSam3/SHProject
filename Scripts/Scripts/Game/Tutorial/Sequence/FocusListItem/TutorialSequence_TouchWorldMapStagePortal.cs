using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;

/// <summary> 스테이지 터치 </summary>
public class TutorialSequence_TouchWorldMapStagePortal : TutorialSequence_TouchWorldMapStage
{
	protected override string Path { get { return "Local_Spot_Slot"; } }

	protected override bool CheckData(C_WorldMapData data, uint tid)
	{
		return data.localInfo.PortalID == tid;
	}
}