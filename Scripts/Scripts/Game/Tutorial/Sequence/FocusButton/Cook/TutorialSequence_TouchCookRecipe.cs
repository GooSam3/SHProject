/// <summary> 요리 레시피 탭 클릭 </summary>
public class TutorialSequence_TouchCookRecipe : TutorialSequence_FocusButton<UIFrameCook>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Cook_Panel/Middle/Tabs/Cook_Tab_List/Panel_Tap_List_01/Tap_List_H/Contents/ZToggle_0 (1)"; } }

	/// <summary> 제작 가능한지 확인해야함 </summary>
	protected override bool Check()
	{
		return true;
	}
}