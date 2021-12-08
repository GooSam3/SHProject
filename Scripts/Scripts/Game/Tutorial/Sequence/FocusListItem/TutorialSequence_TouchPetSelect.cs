using ZNet.Data;

/// <summary> 펠로우에서 펠로우를 선택한다. </summary>
public class TutorialSequence_TouchPetSelect : TutorialSequence_FocusListItemPetChangeBase<UIFramePet>
{
	protected override bool Check()
	{
		if (true == TryGetParam(out var tid))
		{
			if (null != Me.CurCharData.GetPetData(tid))
				return true;
		}

		return false;
	}
}