/// <summary> 퀘스트 알림 영역 터치 </summary>
public class TutorialSequence_TouchQuestHud : TutorialSequence_FocusButton<UISubHUDQuest>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "HUD_Quest_Sub/Body/Quest_List/Scroll View/Viewport/Content/HUD_Quest_Slot/Btn_QuestMainUI"; } }
	protected override string HighlightPath { get { return "HUD_Quest_Sub/Body/Quest_List/Scroll View/Viewport/Content/HUD_Quest_Slot"; } }

	protected override bool Check()
	{
		return true;
	}
}