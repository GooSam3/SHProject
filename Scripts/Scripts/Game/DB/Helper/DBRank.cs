using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBRank : IGameDBHelper
{
	//======== 검사해야할 랭킹 범위값들.
	static uint PK_MinRank = 1;
	static uint PK_MaxRank = 1;

	static List<RankBuff_Table> PK_Tables = new List<RankBuff_Table>();

	static Dictionary<E_CharacterType, (uint, uint)> EXP_MinMaxRank = new Dictionary<E_CharacterType, (uint, uint)>();
	static Dictionary<E_CharacterType, List<RankBuff_Table>> EXP_ClassTables = new Dictionary<E_CharacterType, List<RankBuff_Table>>();

	static Dictionary<E_CharacterType, (uint, uint)> EXPJob_MinMaxRank = new Dictionary<E_CharacterType, (uint, uint)>();
	static Dictionary<E_CharacterType, List<RankBuff_Table>> EXPJob_ClassTables = new Dictionary<E_CharacterType, List<RankBuff_Table>>();

	public void OnReadyData()
	{
		PK_Tables.Clear();

		EXP_MinMaxRank.Clear();
		EXP_ClassTables.Clear();

		EXPJob_MinMaxRank.Clear();
		EXPJob_ClassTables.Clear();

		foreach (var table in GameDBManager.Container.RankBuff_Table_data.Values)
		{
			if (table.RankBuffType == E_RankBuffType.PK)
			{
				if (table.MinRanking < PK_MinRank)
					PK_MinRank = table.MinRanking;
				if (table.MaxRanking > PK_MaxRank)
					PK_MaxRank = table.MaxRanking;

				PK_Tables.Add(table);
			}
			else if (table.RankBuffType == E_RankBuffType.Exp)
			{
				if (!EXP_MinMaxRank.ContainsKey(table.CharacterType))
				{
					EXP_MinMaxRank.Add(table.CharacterType, (table.MinRanking, table.MaxRanking));
				}
				else
				{
					var tuple = EXP_MinMaxRank[table.CharacterType];

					if (table.MinRanking < tuple.Item1)
					{
						tuple.Item1 = table.MinRanking;
						EXP_MinMaxRank[table.CharacterType] = tuple;
					}
					if (table.MaxRanking > tuple.Item2)
					{
						tuple.Item2 = table.MaxRanking;
						EXP_MinMaxRank[table.CharacterType] = tuple;
					}
				}

				// 클래스에 맞는 테이블들 그룹화
				if (!EXP_ClassTables.ContainsKey(table.CharacterType))
					EXP_ClassTables.Add(table.CharacterType, new List<RankBuff_Table>());
				EXP_ClassTables[table.CharacterType].Add(table);
			}
			else if (table.RankBuffType == E_RankBuffType.ExpClassType)
			{
				if (!EXPJob_MinMaxRank.ContainsKey(table.CharacterType))
				{
					EXPJob_MinMaxRank.Add(table.CharacterType, (table.MinRanking, table.MaxRanking));
				}
				else
				{
					var tuple = EXPJob_MinMaxRank[table.CharacterType];

					if (table.MinRanking < tuple.Item1)
					{
						tuple.Item1 = table.MinRanking;
						EXPJob_MinMaxRank[table.CharacterType] = tuple;
					}
					if (table.MaxRanking > tuple.Item2)
					{
						tuple.Item2 = table.MaxRanking;
						EXPJob_MinMaxRank[table.CharacterType] = tuple;
					}
				}

				// 클래스에 맞는 테이블들 그룹화
				if (!EXPJob_ClassTables.ContainsKey(table.CharacterType))
					EXPJob_ClassTables.Add(table.CharacterType, new List<RankBuff_Table>());
				EXPJob_ClassTables[table.CharacterType].Add(table);
			}
		}

		/* 2차 개선안 생각
		 * 
		 * - 순위별로 적용되야할 테이블 리스트들 만들어두기
		 * 1~100위 까지면, List[100] 짜리 만들어서 순위별 테이블 설정해두면,
		 * 직접접근 가능!
		 * 
		 * 메모리 조금 더 쓰는거말고는 
		 */
	}

	/// <summary> 랭킹에 맞는 테이블 정보 리턴해준다. (없으면null) </summary>
	static public RankBuff_Table GetPkRank(uint _inRank)
	{
		if (_inRank < PK_MinRank || _inRank > PK_MaxRank)
			return null;

		foreach (var table in PK_Tables)
		{
			if (table.MinRanking <= _inRank && table.MaxRanking >= _inRank)
			//if (_inRank == Mathf.Clamp(_inRank, table.MinRanking, table.MaxRanking))
			{
				return table;
			}
		}

		return null;
	}

	/// <summary> 랭킹에 맞는 테이블 정보 리턴해준다. (없으면null) </summary>
	static public RankBuff_Table GetExpRank(E_CharacterType characterType, uint _inRank)
	{
		if (!EXP_MinMaxRank.ContainsKey(characterType))
			return null;

		var minmaxTuple = EXP_MinMaxRank[characterType];
		if (_inRank < minmaxTuple.Item1 || _inRank > minmaxTuple.Item2)
			return null;

		foreach (var table in EXP_ClassTables[characterType])
		{
			if (_inRank < table.MinRanking || _inRank > table.MaxRanking)
				continue;

			return table;
		}

		return null;
	}

	/// <summary> 랭킹에 맞는 테이블 정보 리턴해준다. (없으면null) </summary>
	static public RankBuff_Table GetExpJobRank(E_CharacterType characterType, uint _inRank)
	{
		if (!EXPJob_MinMaxRank.ContainsKey(characterType))
			return null;

		var minmaxTuple = EXPJob_MinMaxRank[characterType];
		if (_inRank < minmaxTuple.Item1 || _inRank > minmaxTuple.Item2)
			return null;

		foreach (var table in EXPJob_ClassTables[characterType])
		{
			if (_inRank < table.MinRanking || _inRank > table.MaxRanking)
				continue;

			return table;
		}

		return null;
	}
}
