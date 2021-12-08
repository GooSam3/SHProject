/// <summary> 클래스 장착 버튼 클릭 </summary>
public class TutorialSequence_TouchClassChange: TutorialSequence_FocusButton<UIFrameChange>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Change_Panel/Right/Change_Char_Info_R/Bt_Field/BTS/Bt_Equip"; } }

	protected override bool Check()
	{
		return true;
	}
}