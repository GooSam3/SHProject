/// <summary> 요리 제작 버튼 클릭 </summary>
public class TutorialSequence_TouchCookMake : TutorialSequence_FocusButton<UIFrameCook>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Cook_Panel/Middle/Mixing/Bts/Bt_Cook"; } }

	/// <summary> 제작 가능한지 확인해야함 </summary>
	protected override bool Check()
	{
		return true;
	}
}