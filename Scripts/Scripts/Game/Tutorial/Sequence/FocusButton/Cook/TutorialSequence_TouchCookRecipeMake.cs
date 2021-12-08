/// <summary> 요리 ui 레시피 페이지에서 만들기 버튼을 터치한다 </summary>
public class TutorialSequence_TouchCookRecipeMake : TutorialSequence_FocusButton<UIFrameCook>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Cook_Panel/Middle/Recipe/Bottom/Bt_Cook"; } }

	/// <summary> 제작 가능한지 확인해야함 </summary>
	protected override bool Check()
	{
		return true;
	}
}