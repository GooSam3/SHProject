/// <summary> 제목, 내용이 들어가는 확인/취소 팝업 </summary>
public class TutorialSequence_TouchTitleMessagePopupOk : TutorialSequence_FocusButton<UIMessagePopupNormal>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Normal_Popup/Body/Bottom/Grid/Bt_Accept"; } }

	protected override bool Check()
	{
		return true;
	}
}