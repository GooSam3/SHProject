using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBResource : IGameDBHelper
{
	/// <summary> [공용] 피격 이펙트 </summary>
	public static Effect_Table Fx_Hit;
	/// <summary> [공용] 바닥 터치시 Oneshot </summary>
	public static Effect_Table Fx_TouchGround;
	/// <summary> [공용] 텔레포트 효과 </summary>
	public static Effect_Table Fx_Teleport;
	/// <summary> [공용] 변신 시작시 이펙트 </summary>
	public static Effect_Table Fx_Summon_Change;
	/// <summary> [공용] 펫 소환시 이펙트 </summary>
	public static Effect_Table Fx_Summon_Pet;
    /// <summary> [공용] 탈것 소환시 이펙트 </summary>
	public static Effect_Table Fx_Summon_Vehicle;
    /// <summary> [공용] 레벨업 </summary>
    public static Effect_Table Fx_LevelUp; // TODO : 기획 테이블 생성하면.

    /// <summary> [공용] 타겟 </summary>
    public static Effect_Table Fx_Target;
    /// <summary> [공용] 파티 타겟 </summary>
    public static Effect_Table Fx_PartyTarget;
	/// <summary> [공용] 기믹 타겟 </summary>
	public static Effect_Table Fx_GimmickTarget;

	/// <summary> [공용] 크리 폰트 </summary>
	public static Effect_Table Fx_Critical;
	/// <summary> [공용] 미쓰 폰트 </summary>
	public static Effect_Table Fx_Miss;

	public void OnReadyData()
	{
		TryGetEffect(1000000, out Fx_Hit);
		TryGetEffect(1009001, out Fx_TouchGround);
		TryGetEffect(1010, out Fx_Teleport);
		TryGetEffect(1011, out Fx_Summon_Change);
		TryGetEffect(1012, out Fx_Summon_Pet);
        TryGetEffect(1012, out Fx_Summon_Vehicle);
        TryGetEffect(1015, out Fx_LevelUp);

        TryGetEffect(1009002, out Fx_Target);
        TryGetEffect(1009003, out Fx_PartyTarget);

		TryGetEffect( 1023, out Fx_Critical );
		TryGetEffect( 1024, out Fx_Miss );

		TryGetEffect(500001, out Fx_GimmickTarget);
	}

	public static Dictionary<uint, Resource_Table> DicCharacter
	{
		get { return GameDBManager.Container.Resource_Table_data; }
	}

	public static bool TryGet(uint _tid, out Resource_Table outTable)
	{
		return GameDBManager.Container.Resource_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Resource_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Resource_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	}

    public static string GetResourceFileName(uint _tid)
    {
        if(TryGet(_tid, out var table))
        {
            return table.ResourceFile;
        }

        return string.Empty;
    }

	#region ====================:: Effect ::====================

	public static bool TryGetEffect(uint _tid, out Effect_Table outTable)
	{
		return GameDBManager.Container.Effect_Table_data.TryGetValue(_tid, out outTable);
	}

	public static Effect_Table GetEffect(uint _tid)
	{
		if (GameDBManager.Container.Effect_Table_data.TryGetValue(_tid, out var foundTable))
		{
			return foundTable;
		}
		return null;
	} 

	#endregion
}
