[UnityEngine.Scripting.Preserve]
public class DBSound : IGameDBHelper
{
	/// <summary> 로그인화면용 BGM </summary>
	public const uint BGM_MainTitleID = 10000;
	/// <summary> 캐릭터 선택/생성 단계용 BGM </summary>
	public const uint BGM_CharacterSelect = 10001;

	public void OnReadyData()
	{
	}

	public static bool TryGet(uint _tid, out GameDB.Sound_Table outTable)
	{
		return GameDBManager.Container.Sound_Table_data.TryGetValue(_tid, out outTable);
	}

	public static GameDB.Sound_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Sound_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}

		return null;
	}
}
