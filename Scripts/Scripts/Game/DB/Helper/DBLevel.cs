using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBLevel : IGameDBHelper
{
	/// <summary> <see cref="E_CharacterType"/>별 레벨 데이터 모음</summary>
	static Dictionary<E_CharacterType, List<Level_Table>> LevelDic = new Dictionary<E_CharacterType, List<Level_Table>>();

	public void OnReadyData()
	{
		LevelDic.Clear();

		foreach (Level_Table table in GameDBManager.Container.Level_Table_data.Values)
		{
			if (!LevelDic.ContainsKey(table.CharacterType))
				LevelDic.Add(table.CharacterType, new List<Level_Table>());

			LevelDic[table.CharacterType].Add(table);
		}

		foreach (var key in LevelDic.Keys)
		{
			LevelDic[key].Sort((x, y) => {
				if (x.CharacterLevel < y.CharacterLevel)
					return -1;
				else if (x.CharacterLevel > y.CharacterLevel)
					return 1;

				return 0;
			});
		}
	}

	public static Level_Table Get(E_CharacterType charType, uint level)
	{
		int findIndex = (int)level - 1; // index 0이 1 Level이다.

		if (findIndex >= 0 && findIndex < LevelDic[charType].Count)
		{
			return LevelDic[charType][findIndex];
		}
		else
		{
			//Debug.LogError($"존재하지 않는 레벨 데이터를 가져가려고 시도하였습니다. CharacterType : {type.ToString()}, Level : {level}");
			return null;
		}
	}

	public static bool IsMaxLevel(E_CharacterType charType, uint level)
	{
		int levelIdx = Mathf.Clamp((int)level - 1, 0, LevelDic[charType].Count - 1);

		return LevelDic[charType][levelIdx].LevelUpType == E_LevelUpType.End;
	}

	/// <summary>다음 레벨업이 존재하는지 여부</summary>
	public static bool IsExistLevelUp(E_CharacterType charType, uint level)
	{
		int levelIdx = Mathf.Clamp((int)level - 1, 0, LevelDic[charType].Count);

		return LevelDic[charType][levelIdx].LevelUpType == E_LevelUpType.Up;
	}

	public static ulong GetExp(E_CharacterType charType, uint level)
	{
		int findIndex = (int)level - 1; // index 0이 1 Level이다.

		if (findIndex >= 0 && findIndex < LevelDic[charType].Count)
		{
			return LevelDic[charType][findIndex].LevelUpExp;
		}
		else
		{
			return 0;
		}
	}

	public static float GetExpRate(ulong checkExp, uint checkLevel, E_CharacterType checkCharType)
	{
		ulong curLevelUpExp = GetExp(checkCharType, checkLevel);
		ulong nextLevelUpExp = GetExp(checkCharType, checkLevel + 1);
		
		ulong checkMaxExp = 0;

		if (curLevelUpExp == 0 && nextLevelUpExp == 0)
		{
			return 0;	
		}
		else if (nextLevelUpExp == 0)
		{
			//만렙
			checkMaxExp = curLevelUpExp;
		}
		else
		{
			checkMaxExp = nextLevelUpExp - curLevelUpExp;
		}

		//ZLog.Log(ZLogChannel.Loading, $"GetExpRate| inExp: {checkExp}, checkLevel: {checkLevel} || checkMaxExp: {checkMaxExp}, curLevelUpExp: {curLevelUpExp}, nextLevelUpExp: {nextLevelUpExp}");
        
        if (checkMaxExp < checkExp)
		{
            
			return 1 + GetExpRate(checkExp - checkMaxExp, checkLevel + 1, checkCharType);
		}

        return (float)checkExp / (float)checkMaxExp;
	}

	public static float GetExpRate(ulong checkExp, uint checkLevel, uint charTid)
	{
		return GetExpRate(checkExp, checkLevel, DBCharacter.GetClassTypeByTid(charTid));
	}
}
