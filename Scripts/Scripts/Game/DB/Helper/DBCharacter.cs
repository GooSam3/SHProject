using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBCharacter : IGameDBHelper
{
	private static List<Character_Table> mUsableCharTableList = new List<Character_Table>();

	public void OnReadyData()
	{
		foreach (var table in DBCharacter.DicCharacter.Values)
		{
			if (table.CharacterSelect == E_CharacterSelect.Not)
				continue;

			mUsableCharTableList.Add(table);
		}
	}

	public static Dictionary<uint, Character_Table> DicCharacter
	{
		get { return GameDBManager.Container.Character_Table_data; }
	}

	/// <summary> 사용가능한 캐릭터들 정보만 가지고 있음 </summary>
	public static List<Character_Table> UsableCharTableList => mUsableCharTableList;

	public static bool TryGet(uint _tid, out Character_Table outTable)
	{
		return GameDBManager.Container.Character_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Character_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Character_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

	public static E_CharacterType GetClassTypeByTid(uint tid)
	{
		if (GameDBManager.Container.Character_Table_data.ContainsKey(tid))
			return GameDBManager.Container.Character_Table_data[tid].CharacterType;

		return E_CharacterType.None;
	}

	/// <summary> 캐릭터의 속성 타입(E_UnitAttributeType) 반환 </summary>
	public static E_UnitAttributeType GetAttributeType(uint _charTid)
	{
		if (GameDBManager.Container.Character_Table_data.TryGetValue(_charTid, out var table))
		{
			return table.AttributeType;
		}
		else
		{
			return E_UnitAttributeType.None;
		}
	}

	public static uint GetClassTid(E_CharacterType charType)
	{
		foreach (var charTable in GameDBManager.Container.Character_Table_data.Values)
		{
			if (charTable.CharacterType == charType)
				return charTable.CharacterID;
		}
		return 0;
	}

	public static string GetClassIconName(uint CharTid)
	{
		if (GameDBManager.Container.Character_Table_data.ContainsKey(CharTid))
			return GameDBManager.Container.Character_Table_data[CharTid].Icon;

		return "";
	}
}
