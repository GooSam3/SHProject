/// <summary> 일반 포션 우선순위 버튼을 터치한다	 </summary>
public class TutorialSequence_TouchPotionAuto : TutorialSequence_FocusButton<UIFrameOption>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Option_Panel/Battle/Use/Grid/AutoUsePotion/Potion_01/Bt_Priority_Potion"; } }

	protected override bool Check()
	{
		return true;
	}
}