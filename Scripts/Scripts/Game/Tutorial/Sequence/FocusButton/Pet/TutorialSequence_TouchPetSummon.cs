/// <summary> 펫 소환 </summary>
public class TutorialSequence_TouchPetSummon : TutorialSequence_FocusButton<UIFramePet>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Pet_Panel/ListView/Right/Change_Char_Info_R/Bt_Field/BTS/Bt_Change"; } }

	protected override bool Check()
	{
		return true;
	}
}