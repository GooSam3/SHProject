/// <summary> 공격 버튼 터치 </summary>
public class TutorialSequence_TouchAttack : TutorialSequence_FocusButton<UISubHUDCharacterAction>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "HUD_Action_Buttons_Sub/Body/Bt_Attack"; } }

	protected override bool Check()
	{
		return true;
	}
}