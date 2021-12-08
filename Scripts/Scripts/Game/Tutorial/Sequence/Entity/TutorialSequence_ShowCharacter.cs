using GameDB;
/// <summary> 모든 캐릭터를 화면상에서 보인다. </summary>
public class TutorialSequence_ShowCharacter : TutorialSequence_None
{
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		var list = ZPawnManager.Instance.GetAllPawn(E_UnitType.Character);

		foreach(var pawn in list)
		{
			pawn.SetActive(true);
		}

		EndSequence(false);
	}
}