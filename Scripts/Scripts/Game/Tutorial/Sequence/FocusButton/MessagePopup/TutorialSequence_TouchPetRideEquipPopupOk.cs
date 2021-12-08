/// <summary> 펠로우/탈것 장착/탈착에서 확인/취소 팝업 </summary>
public class TutorialSequence_TouchPetRideEquipPopupOk : TutorialSequence_FocusButton<UIMessagePopupPREquip>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Msg_Popup/Body/Bottom/Bt_Muilti (2)"; } }

	protected override bool Check()
	{
		return true;
	}
}