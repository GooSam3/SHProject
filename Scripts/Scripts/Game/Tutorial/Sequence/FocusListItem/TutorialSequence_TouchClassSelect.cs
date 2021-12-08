using ZNet.Data;

/// <summary> 클래스에서 클래스를 선택한다. </summary>
public class TutorialSequence_TouchClassSelect : TutorialSequence_FocusListItemPetChangeBase<UIFrameChange>
{	

	protected override bool Check()
	{
		if(true == TryGetParam(out var tid))
		{
			if (null != Me.CurCharData.GetChangeDataByTID(tid))
				return true;
		}

		return false;
	}
}