using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
class DBWeightPenalty : IGameDBHelper
{
    // 패널티 적용시점
    public static float MinWeightRate = float.MaxValue;

    private static Dictionary<uint, WeightPenalty_Table> dicWeightPenalty = new Dictionary<uint, WeightPenalty_Table>();

    public void OnReadyData()
    {
        dicWeightPenalty = GameDBManager.Container.WeightPenalty_Table_data;

        foreach (var iter in dicWeightPenalty.Values)
        {
            if (MinWeightRate > iter.CharacterWeighRate)
                MinWeightRate = iter.CharacterWeighRate;
        }
    }

    // 무게 비교해서 해당무게범위의 패널티가있다면 테이블 반환
    public static bool TryGetPaneltyData(float weightRate, out WeightPenalty_Table table)
    {
        table = null;

        if (weightRate < MinWeightRate)
            return false;

        foreach (var iter in dicWeightPenalty.Values)
        {
            if (weightRate >= iter.CharacterWeighRate)
                table = iter;
        }

        return true;
    }

    /// <summary>
    /// [박윤성] ID넣어서 테이블 반환 => 옵션 표시용으로 만듬
    /// </summary>
    /// <param name="weightPenaltyID"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    public static bool TryGetPaneltyData(uint weightPenaltyID, out WeightPenalty_Table table)
    {
        table = null;

        if (dicWeightPenalty.ContainsKey(weightPenaltyID))
        {
            table = dicWeightPenalty[weightPenaltyID];
            return true;
        }
        else
            return false;
    }
}
