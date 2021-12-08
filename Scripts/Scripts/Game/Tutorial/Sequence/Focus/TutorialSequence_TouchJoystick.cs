/// <summary> 조이스틱 터치 </summary>
public class TutorialSequence_TouchJoystick : TutorialSequence_Focus<UISubHUDJoyStick>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "MoveJoyStick/BackGround"; } }
}