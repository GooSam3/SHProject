/// <summary> 내용만 들어가고, 버튼 1개인 팝업 </summary>
public class TutorialSequence_TouchMessagePopupOnlyOk : TutorialSequence_FocusButton<UIMessagePopupDefault>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Msg_Popup/Body/Bottom/Bt_ Conform"; } }

	protected override bool Check()
	{
		return true;
	}
}