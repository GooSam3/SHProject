using GameDB;
/// <summary> 모든 몬스터를 화면상에서 보인다. </summary>
public class TutorialSequence_ShowMonster : TutorialSequence_None
{
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		uint tid = 0 < TutorialTable.GuideParams.Count ? uint.Parse(TutorialTable.GuideParams[0]) : 0;

		var list = ZPawnManager.Instance.GetAllPawn(E_UnitType.Monster, tid);

		foreach(var pawn in list)
		{
			pawn.SetActive(true);
		}

		EndSequence(false);
	}
}