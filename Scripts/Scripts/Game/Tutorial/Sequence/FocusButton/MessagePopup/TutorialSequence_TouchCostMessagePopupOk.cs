/// <summary> 비용 소모 관련 확인/취소 팝업 </summary>
public class TutorialSequence_TouchCostMessagePopupOk : TutorialSequence_FocusButton<UIMessagePopupCost>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Cost_Confirm_Popup/Body/Bottom/Grid/Bt_Accept"; } }

	protected override bool Check()
	{
		return true;
	}
}