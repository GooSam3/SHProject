using ZNet.Data;

/// <summary> 탈것에서 탈것을 선택한다. </summary>
public class TutorialSequence_TouchRideSelect : TutorialSequence_FocusListItemPetChangeBase<UIFrameRide>
{
	protected override bool Check()
	{
		if (true == TryGetParam(out var tid))
		{
			if (null != Me.CurCharData.GetRideData(tid))
				return true;
		}

		return false;
	}
}