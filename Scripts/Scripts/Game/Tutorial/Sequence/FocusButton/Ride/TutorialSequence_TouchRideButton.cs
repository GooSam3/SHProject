/// <summary> 탈것 타기 버튼 터치 </summary>
public class TutorialSequence_TouchRideButton : TutorialSequence_FocusButton<UISubHUDQuickSlot>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "HUD_QuickSlot_Sub/Bt_Ride"; } }

	protected override bool Check()
	{
		return true;
	}
}