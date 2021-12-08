using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBGodBuff : IGameDBHelper
{
	/// <summary> Key : Stack Count </summary>
	public static Dictionary<uint, GodBuff_Table> GodTear_TableForStack = new Dictionary<uint, GodBuff_Table>();
	public static Dictionary<uint, GodBuff_Table> GodPower_TableForStack = new Dictionary<uint, GodBuff_Table>();

	public void OnReadyData()
	{
		GodTear_TableForStack.Clear();
		GodPower_TableForStack.Clear();

		foreach (var table in GameDBManager.Container.GodBuff_Table_data.Values)
		{
			if (table.GodBuffType == E_GodBuffType.GodBless)
			{
				GodTear_TableForStack.Add(table.Stack, table);
			}
			else
			{
				GodPower_TableForStack.Add(table.Stack, table);
			}
		}
	}

	public static bool Get(uint tid, out GodBuff_Table table)
	{
		return GameDBManager.Container.GodBuff_Table_data.TryGetValue(tid, out table);
	}

	/// <summary> 현재 스택 카운트에 맞는 버프 정보 얻기</summary>
	public static GodBuff_Table GetGodTearByStack(uint _inStackCount)
	{
		GodBuff_Table resultTable = null;
		foreach (var pair in GodTear_TableForStack)
		{
			if (_inStackCount >= pair.Key)
				resultTable = pair.Value;
		}

		return resultTable;
	}

	/// <summary> 현재 스택 카운트에 맞는 버프 정보 얻기</summary>
	public static GodBuff_Table GetGodPowerByStack(uint _inStackCount)
	{
		GodBuff_Table resultTable = null;
		foreach (var pair in GodPower_TableForStack)
		{
			if (_inStackCount >= pair.Key)
				resultTable = pair.Value;
		}

		return resultTable;
	}

	public static int GetBuffLevel(E_GodBuffType type, uint stackCnt)
    {
		int level = 0;

		if (stackCnt > 0)
			level++;


		foreach(var iter in GameDBManager.Container.GodBuff_Table_data.Values)
        {
			if (iter.GodBuffType != type)
				continue;

			if (iter.Stack <= stackCnt)
				level++;
        }
		return level;
    }

	public static Dictionary<uint, GodBuff_Table>.ValueCollection GetGodBuffList(E_GodBuffType godBuffType)
	{
		switch (godBuffType)
		{
			case E_GodBuffType.GodBless:
				return GodTear_TableForStack.Values;
			case E_GodBuffType.GodPower:
				return GodPower_TableForStack.Values;
		}

		return null;
	}
}
