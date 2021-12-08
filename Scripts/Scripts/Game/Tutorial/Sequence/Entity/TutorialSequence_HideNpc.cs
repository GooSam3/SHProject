using GameDB;
/// <summary> 해당 npc를 화면상에서 숨긴다. </summary>
public class TutorialSequence_HideNpc : TutorialSequence_None
{
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		uint tid = 0 < TutorialTable.GuideParams.Count ? uint.Parse(TutorialTable.GuideParams[0]) : 0;

		var list = ZPawnManager.Instance.GetAllPawn(E_UnitType.NPC, tid);

		foreach(var pawn in list)
		{
			pawn.SetActive(false);
		}

		EndSequence(false);
	}
}