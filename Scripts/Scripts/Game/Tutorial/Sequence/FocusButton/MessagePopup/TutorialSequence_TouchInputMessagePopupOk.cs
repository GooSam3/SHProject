/// <summary> 텍스트 입력 가능한 확인/취소 팝업 </summary>
public class TutorialSequence_TouchInputMessagePopupOk : TutorialSequence_FocusButton<UIMessagePopupInput>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Input_Txt_Popup/Body/Bottom/Grid/Bt_Accept"; } }

	protected override bool Check()
	{
		return true;
	}
}