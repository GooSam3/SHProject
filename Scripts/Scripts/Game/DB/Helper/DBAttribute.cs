using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;

// 속성(elemental)
[UnityEngine.Scripting.Preserve]
public class DBAttribute : IGameDBHelper
{
    // 속성 각 레벨의 강화 단계를 나타내는 구조체 
    public struct AttributeEnhanceSimpleRateInfo
    {
        public List<uint> LevelUpItem;
        public List<uint> LevelUpItemCnt;
        public uint LevelUpMaxRawRate;
        public uint AdditionalRateUnit;

        public AttributeEnhanceSimpleRateInfo(List<uint> levelUpItem, List<uint> levelUpItemCnt, uint levelUpRate, uint additionalRateUnit)
        {
            LevelUpItem = levelUpItem;
            LevelUpItemCnt = levelUpItemCnt;
            LevelUpMaxRawRate = levelUpRate;
            AdditionalRateUnit = additionalRateUnit;
        }

        public void AddCost(AttributeEnhanceSimpleRateInfo info)
        {
            for (int i = 0; i < info.LevelUpItem.Count; i++)
            {
                // 만약에 해당 재화가 현재 리스트에 없으면 더해줌 . 
                if (LevelUpItem.Exists(t => t == info.LevelUpItem[i]) == false)
                {
                    LevelUpItem.Add(info.LevelUpItem[i]);
                    LevelUpItemCnt.Add(0);
                }

                uint addedCost = info.LevelUpItemCnt[i];
                var targetItemIndex = LevelUpItem.FindIndex(t => t == info.LevelUpItem[i]);

                LevelUpItemCnt[targetItemIndex] += addedCost;
            }
        }
    }

    public delegate void SingleAttributeDelegator(E_UnitAttributeType type, Attribute_Table data);
    static Dictionary<E_UnitAttributeType, List<Attribute_Table>> attributeDicByType = new Dictionary<E_UnitAttributeType, List<Attribute_Table>>();
    static Dictionary<uint, AttributeChain_Table> attributeChainByLevel = new Dictionary<uint, AttributeChain_Table>();

    static E_UnitAttributeType[] types;

    // 테이블상에서 검색된 슬롯의 가장 최소레벨 및 최대레벨 (직접계산)
    static uint attributeMinLevel;
    static uint attributeMaxLevel;

    // '' 체인 최소/최대 레벨 
    static uint attributeChainMinLevel;
    static uint attributeChainMaxLevel;

    public int DataCount
    {
        get
        {
            if (attributeDicByType == null)
                return 0;

            int cnt = 0;

            foreach (var keyPair in attributeDicByType)
            {
                cnt += keyPair.Value.Count;
            }

            return cnt;
        }
    }

    public void OnReadyData()
    {
        types = (E_UnitAttributeType[])Enum.GetValues(typeof(E_UnitAttributeType));

        #region 속성 슬롯 데이터 세팅 
        attributeDicByType.Clear();

        foreach (var data in GameDBManager.Container.Attribute_Table_data.Values)
        {
            if (!attributeDicByType.ContainsKey(data.AttributeType))
                attributeDicByType.Add(data.AttributeType, new List<Attribute_Table>());

            attributeDicByType[data.AttributeType].Add(data);
        }

        attributeMinLevel = 0;
        attributeMaxLevel = 0;

        if (attributeDicByType.Count > 0)
        {
            attributeMinLevel = byte.MaxValue;
            attributeMaxLevel = byte.MinValue;

            foreach (var keyPair in attributeDicByType)
            {
                var minLevelByThisType = keyPair.Value.Min(t => t.AttributeLevel);
                var maxLevelByThisType = keyPair.Value.Max(t => t.AttributeLevel);

                if (minLevelByThisType < attributeMinLevel)
                    attributeMinLevel = minLevelByThisType;
                if (maxLevelByThisType > attributeMaxLevel)
                    attributeMaxLevel = maxLevelByThisType;
            }
        }

        #endregion

        #region 속성 연계 데이터 세팅 
        attributeChainByLevel.Clear();

        attributeChainMinLevel = 0;
        attributeChainMaxLevel = 0;

        var chainData = GameDBManager.Container.AttributeChain_Table_data;

        if (chainData.Count > 0)
        {
            attributeChainMinLevel = byte.MaxValue;
            attributeChainMaxLevel = byte.MinValue;

            foreach (var data in chainData.Values)
            {
                if (data.ChainLevel < attributeChainMinLevel)
                    attributeChainMinLevel = data.ChainLevel;
                if (data.ChainLevel > attributeChainMaxLevel)
                    attributeChainMaxLevel = data.ChainLevel;

                if (attributeChainByLevel.ContainsKey(data.ChainLevel) == false)
                    attributeChainByLevel.Add(data.ChainLevel, null);

                attributeChainByLevel[data.ChainLevel] = data;
            }
        }
        #endregion
    }

    #region 속성

    /// <summary>
    /// TID 로 속성을 가져옵니다. 
    /// </summary>
    public static bool GetAttributeByID(uint tId, out Attribute_Table table)
    {
        return GameDBManager.Container.Attribute_Table_data.TryGetValue(tId, out table);
    }

    /// <summary>
    /// Level 로 속성을 가져옵니다.
    /// </summary>
    public static Attribute_Table GetAttributeByLevel(E_UnitAttributeType type, uint level)
    {
        var list = GetListByType(type);

        if (list == null)
            return null;

        Attribute_Table result = null;

        foreach (var data in list)
        {
            if (data.AttributeType.Equals(type) &&
                data.AttributeLevel == level)
            {
                result = data;
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 해당 타입의 속성 리스트를 가져옵니다.
    /// </summary>
    public static List<Attribute_Table> GetListByType(E_UnitAttributeType type)
    {
        List<Attribute_Table> targetList = null;

        if (attributeDicByType.ContainsKey(type))
        {
            targetList = attributeDicByType[type];
        }

        return targetList;
    }

    /// <summary>
    /// 해당 타입의 속성 리스트의 가장 앞 데이터를 가져옵니다 .
    /// </summary>
    public static bool GetFirstByType(E_UnitAttributeType type, out Attribute_Table table)
    {
        table = null;
        if (attributeDicByType.TryGetValue(type, out var list) == false)
            return false;

        if (list.Count <= 0)
            return false;

        table = list[0];

        return true;
    }

    /// <summary>
    /// 해당 속성의 타입을 가져옵니다.
    /// </summary>
    public static E_UnitAttributeType GetTypeByTID(uint tid)
    {
        if (GetAttributeByID(tid, out var result))
            return result.AttributeType;
        return E_UnitAttributeType.None;
    }

    /// <summary>
    /// 해당 타입의 최대 레벨을 가져옵니다. 
    /// </summary>
    public static uint GetMaxLevelByType(E_UnitAttributeType type)
    {
        var list = GetListByType(type);

        if (list == null || list.Count == 0)
            return 0;

        return list.Max(_t => _t.AttributeLevel);
    }

    /// <summary>
    /// 모든 속성 데이터들을 순회합니다. 순서는 각 레벨마다 모든 속성을 한번씩 순회하는 방식입니다.
    /// </summary>
    public static void ForeachAllTypes_ByEachLevel(SingleAttributeDelegator callback)
    {
        if (callback == null)
            return;

        for (uint i = attributeMinLevel; i <= attributeMaxLevel; i++)
        {
            uint level = i;

            // Fire~Dark
            for (int j = 0; j < types.Length; j++)
            {
                if (types[j].Equals(E_UnitAttributeType.None))
                    continue;

                callback(types[j], GetAttributeByLevel(types[j], level));
            }
        }
    }

    /// <summary>
    /// 모든 속성 데이터들을 순회합니다. 순서는 각 속성마다 모든 레벨을 한번씩 순회하는 방식입니다.
    /// </summary>
    public static void ForeachAllTypes_ByEachElementType(SingleAttributeDelegator callback)
    {
        if (callback == null)
            return;

        for (int i = 0; i < types.Length; i++)
        {
            if (types[i].Equals(E_UnitAttributeType.None))
                continue;

            for (uint j = attributeMinLevel; j < attributeMaxLevel; j++)
            {
                uint level = j;

                callback(types[i], GetAttributeByLevel(types[i], level));
            }
        }
    }

    /// <summary>
    /// 특정 속성의 레벨로 TID 를 가져옵니다.
    /// </summary>
    static public uint GetAttributeTIDByLevel(E_UnitAttributeType type, uint level)
    {
        var list = GetListByType(type);

        if (list == null)
            return 0;

        var result = list.Find(t => t.AttributeLevel == level);
        return result != null ? result.AttributeID : 0;
    }

    /// <summary>
    /// 특정 속성 타입의 모든 속성 TID 리스트를 가져옵니다.
    /// </summary>
    static public void GetAttributeTIDListByType(E_UnitAttributeType type, List<uint> outputList)
    {
        var list = GetListByType(type);

        if (list == null)
            return;

        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
                outputList.Add(list[i].AttributeID);
        }
    }

    /// <summary>
    /// 파라미터 속성연계 레벨을 기준으로 레벨업 가능한 최대 속성 레벨을 가져옵니다.
    /// ex) 연계 레벨 1 (공통요구 속성레벨 : 2) 연계 레벨 2 ('' : 4) 일때 1을 넣으면 4 가 리턴.  
    /// 왜냐면 현재의 연계 레벨 1 에서 2 가 되려면 속성 레벨들이 4 가 되어야 하기 때문
    /// </summary>
    static public uint GetAttributeReachableMaxLevelAtCurrentChainLevel(uint currentChainLevel)
    {
        uint nextLevel = currentChainLevel + 1;

        if (nextLevel > attributeChainMaxLevel)
            nextLevel = attributeChainMaxLevel;

        var data = GetAttributeChainByLevel(nextLevel);

        if (data == null)
            return 0;

        return data.FireLevelReq;
    }

    /// <summary>
    /// 다음 속성의 레벨을 가져옵니다 . 
    /// </summary>
    static public bool GetNextAttributeID(uint id, out uint output)
    {
        output = 0;

        GetAttributeByID(id, out var source);

        if (source == null)
            return false;

        byte vNextLevel = (byte)(source.AttributeLevel + 1);
        var resultData = GetAttributeByLevel(source.AttributeType, vNextLevel);
        output = resultData != null ? resultData.AttributeID : 0;
        return resultData != null;
    }

    #region 강화 관련 

    /// <summary>
    /// 확률 단위 1:10000 를 백분율 단위로 변환합니다.
    /// </summary>
    public static uint ConvertEnhanceRawRateToActualRate(uint rate)
    {
        return rate / 10000;
    }

    /// <summary>
    /// 백분율 단위를 1:10000 단위로 변환합니다.
    /// </summary>
    public static uint ConvertEnhanceActualRateToRawRate(uint rate)
    {
        return rate * 10000;
    }

    /// <summary>
    /// 해당 속성의 기본 강화 확률을 가져옵니다. 
    /// </summary>
    public static uint GetAttributeMinRawRate(uint attributeID)
    {
        uint resultRawRate = 0;

        if (GetAttributeByID(attributeID, out var data))
        {
            resultRawRate = data.LevelUpRate;
        }

        return resultRawRate;
    }

    /// <summary>
    /// 해당 속성의 특정 강화 확률에 맞는 데이터를 가져옵니다.
    /// Return 값은 해당 값으로 강화확률을 높일수 있는지 없는지에 대한 값 
    /// </summary>
    public static bool GetAttributeEnhanceInfoAtSpecificRate(
        uint attributeID
        , uint rawRate
        , ref AttributeEnhanceSimpleRateInfo result)
    {
        bool enhanceable = false;

        // Build Cost Item 
        List<AttributeEnhanceSimpleRateInfo> costItemSteps = new List<AttributeEnhanceSimpleRateInfo>();

        if (GetAttributeByID(attributeID, out var data))
        {
            BuildEnhanceRateInfo(ref costItemSteps, data);
            enhanceable = GetAttributeEnhanceSpecificRateData(rawRate, ref result, costItemSteps);
        }

        return enhanceable;
    }

    /// <summary>
    /// 해당 속성의 특정 강화 확률에서의 최종 비용 데이터를 가져옵니다 . 
    /// </summary>
    public static AttributeEnhanceSimpleRateInfo GetAttributeEnhanceCost(
    uint attributeID
   , uint rawRate)
    {
        AttributeEnhanceSimpleRateInfo result = new AttributeEnhanceSimpleRateInfo(new List<uint>(), new List<uint>(), 0, 0);

        // Build Cost Item 
        List<AttributeEnhanceSimpleRateInfo> costItemSteps = new List<AttributeEnhanceSimpleRateInfo>();

        if (GetAttributeByID(attributeID, out var data))
        {
            BuildEnhanceRateInfo(ref costItemSteps, data);

            uint minRate = GetAttributeMinRawRate(attributeID);
            uint maxRate = GetMaxAttributeEnhanceableRawRate(attributeID);

            // 범위 벗어나는 rate 를 clamp함
            if (rawRate < minRate)
                rawRate = minRate;
            else if (rawRate > maxRate)
                rawRate = maxRate;

            // 예로 700000 을 넣었다 . 그럼 70 이 나옴 . 
            uint rateConverted = ConvertEnhanceRawRateToActualRate(rawRate);
            uint minRateConverted = ConvertEnhanceRawRateToActualRate(minRate);
            uint maxRateConverted = ConvertEnhanceRawRateToActualRate(maxRate);

            // 올려줘야하는 횟수를 계산함 . 
            uint countToGoRateConverted = rateConverted - minRateConverted;

            result.LevelUpItem.Clear();
            result.LevelUpItemCnt.Clear();

            // 일단은 기본 비용 데이터를 넣어줌 . 
            result.LevelUpItem.AddRange(data.LevelUpItem);
            result.LevelUpItemCnt.AddRange(data.UpItemCnt);
            result.LevelUpMaxRawRate = rawRate;

            // 이제 하나 하나 additional 비용 계산함 . 
            for (int i = 0; i < countToGoRateConverted; i++)
            {
                // 지금 만약 1 번째면 10000 이 들어가겠지 ? additional 임 즉 
                // 기본 50 프로 일때는 51 프로 즉 그 1 프로를 계산하기 위함 
                uint curAdditionalRawRate = ConvertEnhanceActualRateToRawRate((uint)i + 1);
                // 해당 확률에 맞는 데이터를 가져오기위해서 단계별 확률을 넣어줌 . (rawRate)
                uint curTargetRawRate = minRate + curAdditionalRawRate;
                AttributeEnhanceSimpleRateInfo resultSpecificRate = default(AttributeEnhanceSimpleRateInfo);
                GetAttributeEnhanceSpecificRateData(curTargetRawRate, ref resultSpecificRate, costItemSteps);
                result.AddCost(resultSpecificRate);
            }
        }

        return result;
    }

    // static public uint GetAdditionalRate

    /// <summary>
    /// 해당 속성의 강화 최대 확률을 가져옵니다.
    /// </summary>
    static public uint GetMaxAttributeEnhanceableRawRate(uint attributeID)
    {
        uint maxRawRate = 0;

        if (GetAttributeByID(attributeID, out var data))
        {
            AttributeEnhanceSimpleRateInfo dummy = default(AttributeEnhanceSimpleRateInfo);
            GetAttributeEnhanceInfoAtSpecificRate(
                attributeID
                , ConvertEnhanceActualRateToRawRate(100)
                , ref dummy);
            maxRawRate = dummy.LevelUpMaxRawRate;
        }

        return maxRawRate;
    }

    /// <summary>
    /// 해당 속성이 desiredRawRate 까지 도달하기 위해 몇번을 
    /// Rate 를 Add 해야하는지 Count 계산과 ,비용 계산 . 
    /// 그리고 후에도 만약 단계별로 점층적으로 확률을 더한것에 대한 
    /// 누적값을 계산하거나 한다면 여기서 처리하도록 함 . 
    /// </summary>
    static public uint GetSequentialDesiredRawRateInfoFromMinRate(uint attributeID, uint desiredRawRate, List<uint> costItemIDs = null, List<uint> costItemCounts = null)
    {
        List<AttributeEnhanceSimpleRateInfo> steps = new List<AttributeEnhanceSimpleRateInfo>();

        if (GetAttributeByID(attributeID, out var data))
        {
            BuildEnhanceRateInfo(ref steps, data);
        }
        else
        {
            return 0;
        }

        bool getCostInfo = costItemIDs != null && costItemCounts != null;

        if (getCostInfo)
        {
            costItemIDs.Clear();
            costItemCounts.Clear();

            /// 요구 아이템 , 카운트 개수가안맞음 . 테이블 에러임 
            if (data.LevelUpItem.Count != data.UpItemCnt.Count)
            {
                ZLog.LogError(ZLogChannel.UI, "LevelUPItem and UpItemCnt not matching , count must be match");
                return 0;
            }

            /// 기본 비용 추가 
            for (int i = 0; i < data.LevelUpItem.Count; i++)
            {
                costItemIDs.Add(data.LevelUpItem[i]);
                costItemCounts.Add(data.UpItemCnt[i]);
            }
        }

        /// 일단 시작은 최소로 잡아놓고 
        uint curCheckingRawRate = GetAttributeMinRawRate(attributeID);
        uint resultAddCnt = 0;

        /// 현재 Desired 수치까지 루프돌면서 ++ 해줌 . 
        while (curCheckingRawRate < desiredRawRate)
        {
            AttributeEnhanceSimpleRateInfo result = default(AttributeEnhanceSimpleRateInfo);
            GetAttributeEnhanceSpecificRateData(curCheckingRawRate + 1, ref result, steps);

            /// 이 값이 0 이면 무한루프임 . 우선 탈출 조치함 . 
            /// 그리고 0 이면은 어차피 desiredRawRate 까지 도달할 수가 없기 때문에
            /// 이 함수 자체를 사용할일이 없음 . 알고써야함 . 
            if (result.AdditionalRateUnit == 0)
            {
                ZLog.LogWarn(ZLogChannel.UI, " this canont be zero here ");
                return 0;
            }

            curCheckingRawRate += result.AdditionalRateUnit;

            if (getCostInfo)
            {
                /// 마찬가지로 개수 테이블 에러 체킹함 . 
                if (result.LevelUpItem.Count != result.LevelUpItemCnt.Count)
                {
                    ZLog.LogError(ZLogChannel.UI, " LevelUpItem counts and LevelUpItemCnts Must be match");
                    return 0;
                }

                for (int i = 0; i < result.LevelUpItem.Count; i++)
                {
                    /// 같은 타입의 비용 아이템을 찾아야함 . (기본에서 추가된)
                    for (int j = 0; j < costItemIDs.Count; j++)
                    {
                        if (result.LevelUpItem[i] == costItemIDs[j])
                        {
                            costItemCounts[j] += result.LevelUpItemCnt[i];
                            break;
                        }
                    }
                }
            }

            resultAddCnt++;
        }

        return resultAddCnt;
    }

    static private void BuildEnhanceRateInfo(ref List<AttributeEnhanceSimpleRateInfo> target, Attribute_Table data)
    {
        // 기본 데이터 
        target.Add(new AttributeEnhanceSimpleRateInfo(data.LevelUpItem, data.UpItemCnt, data.LevelUpRate, 0));

        // 1~10 단계
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_1, data.AdditionalItemCnt_1, data.AdditionalRateMaxRate_1, data.AdditionalRateUnit_1));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_2, data.AdditionalItemCnt_2, data.AdditionalRateMaxRate_2, data.AdditionalRateUnit_2));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_3, data.AdditionalItemCnt_3, data.AdditionalRateMaxRate_3, data.AdditionalRateUnit_3));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_4, data.AdditionalItemCnt_4, data.AdditionalRateMaxRate_4, data.AdditionalRateUnit_4));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_5, data.AdditionalItemCnt_5, data.AdditionalRateMaxRate_5, data.AdditionalRateUnit_5));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_6, data.AdditionalItemCnt_6, data.AdditionalRateMaxRate_6, data.AdditionalRateUnit_6));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_7, data.AdditionalItemCnt_7, data.AdditionalRateMaxRate_7, data.AdditionalRateUnit_7));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_8, data.AdditionalItemCnt_8, data.AdditionalRateMaxRate_8, data.AdditionalRateUnit_8));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_9, data.AdditionalItemCnt_9, data.AdditionalRateMaxRate_9, data.AdditionalRateUnit_9));
        target.Add(new AttributeEnhanceSimpleRateInfo(data.AdditionalItem_10, data.AdditionalItemCnt_10, data.AdditionalRateMaxRate_10, data.AdditionalRateUnit_10));

        target.RemoveAll(t => t.LevelUpItem.Count == 0);
    }

    //public static uint GetAdditionalRateUnitAtSpecificRate(
    //    uint 
    //    , uint rawRate)
    //{

    //}

    /// <summary>
    /// 특정 확률에 대한 해당 속성의 강화 정보를 가져옵니다.
    /// 해당 속성의 최소/최대 확률 값에 대하여 Clamp 됩니다. 
    /// </summary>
    public static bool GetAttributeEnhanceSpecificRateData(
        uint rawRate
        , ref AttributeEnhanceSimpleRateInfo result
        , List<AttributeEnhanceSimpleRateInfo> queryData)
    {
        if (queryData.Count == 0)
            return false;

        var curData = queryData[0];

        for (int i = 0; i < queryData.Count; i++)
        {
            if (rawRate > curData.LevelUpMaxRawRate)
            {
                if (i < queryData.Count - 1)
                {
                    curData = queryData[i + 1];
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        result = curData;

        return true;
    }
    #endregion
    #endregion

    #region 속성 연계
    /// <summary>
    /// 속성 연계 최소 레벨을 가져옵니다.
    /// </summary>
    static public uint GetAttributeChainMinLevel()
    {
        return attributeChainMinLevel;
    }

    /// <summary>
    /// 속성 연계 최대 레벨을 가져옵니다. 
    /// </summary>
    static public uint GetAttributeChainMaxLevel()
    {
        return attributeChainMaxLevel;
    }

    /// <summary>
    /// 속성 연계 TID 로 데이터를 가져옵니다.
    /// </summary>
    static public AttributeChain_Table GetAttributeChainByID(uint id)
    {
        foreach (var data in attributeChainByLevel)
        {
            if (data.Value.AttributeChainID == id)
                return data.Value;
        }

        return null;
    }

    /// <summary>
    /// 속성 연계 레벨로 데이터를 가져옵니다.
    /// </summary>
    static public AttributeChain_Table GetAttributeChainByLevel(uint level)
    {
        if (attributeChainByLevel.ContainsKey(level) == false)
            return null;

        return attributeChainByLevel[level];
    }

    /// <summary>
    /// 해당 속성 연계의 요구 속성레벨을 가져옵니다. 
    /// </summary>
    static public uint GetAttributeChainRequestLevel(uint chainLevel)
    {
        var data = GetAttributeChainByLevel(chainLevel);

        if (data == null)
            return 0;

        // WARNING : 
        // 모든 속성의 요구 레벨이 같기때문에 Fire Level 로 대표하여 리턴함. 
        // 만약 기획이 바뀌어서 이 전제가 틀려지면 
        // 이 코드 수정해야 할수있음. 
        return data.FireLevelReq;
    }

    /// <summary>
    /// 속성 연계를 순회합니다.
    /// </summary>
    static public void ForeachAttributeChain(Action<AttributeChain_Table> callback)
    {
        foreach (var data in attributeChainByLevel)
        {
            callback?.Invoke(data.Value);
        }
    }

    /// <summary>
    /// 속성 연계 레벨을 현재 최소 속성 레벨로 가져옵니다.
    /// </summary>
    static public uint GetCurrentAttributeChainLevel(uint minAttributeLevel)
    {
        uint curChainLevel = 0;

        foreach (var data in attributeChainByLevel)
        {
            if ((data.Value.FireLevelReq == data.Value.WaterLevelReq) &&
                (data.Value.FireLevelReq == data.Value.ElectricLevelReq) &&
                (data.Value.FireLevelReq == data.Value.LightLevelReq) &&
                (data.Value.FireLevelReq == data.Value.DarkLevelReq))
            {
                uint thresholdLevel = data.Value.FireLevelReq;

                if (minAttributeLevel >= thresholdLevel)
                {
                    if (curChainLevel < data.Value.ChainLevel)
                    {
                        curChainLevel = data.Value.ChainLevel;
                    }
                }
            }
            else
            {
                ZLog.LogError(ZLogChannel.Loading, "속성연계의 달성 조건 레벨들이 같아야하는데 다른게 있습니다. 기획이 바뀐건지 ??");
            }
        }

        return curChainLevel;
    }

    static public bool IsThisAttributeChainLevelObtained(uint chainLevel, uint myMinAttributeLevel)
    {
        var myChainLevel = GetCurrentAttributeChainLevel(myMinAttributeLevel);
        return myChainLevel >= chainLevel;
    }
    #endregion
}
