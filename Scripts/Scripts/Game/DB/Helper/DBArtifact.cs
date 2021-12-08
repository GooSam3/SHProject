using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBArtifact : IGameDBHelper
{
    /// <summary>
    /// 승급에 필요한 하나의 재료를 정의함 
    /// </summary>
    public struct MaterialType
    {
        public E_ArtifactMaterialType type;
        public uint grade;

        public MaterialType(E_ArtifactMaterialType type, uint grade)
        {
            this.type = type;
            this.grade = grade;
        }
    }

    public struct ArtifactLink
    {
        public uint tid;

        public ArtifactLink(uint tid)
        {
            this.tid = tid;
        }
    }

    public class MaterialItem
    {
        public uint tid;
        public ulong id;
        public uint cnt;
    }

    static List<Artifact_Table> MakeList = new List<Artifact_Table>();

    /// <summary>
    /// Key : Artifact_Table.ArtifactGroupID, Value[Key: Artifact_Table.Step]
    /// </summary>
    static Dictionary<uint, Dictionary<uint, Artifact_Table>> UpgradeList = new Dictionary<uint, Dictionary<uint, Artifact_Table>>();

    static Dictionary<uint, List<ArtifactLink_Table>> LinkGroups = new Dictionary<uint, List<ArtifactLink_Table>>();

    public void OnReadyData()
    {
        MakeList.Clear();
        UpgradeList.Clear();

        foreach (var tableData in GameDBManager.Container.Artifact_Table_data.Values)
        {
            //사라짐??
            //if (tableData.ArtifactActionType == E_ArtifactActionType.Make)
            //	MakeList.Add(tableData);

            if (!UpgradeList.ContainsKey(tableData.ArtifactGroupID))
                UpgradeList.Add(tableData.ArtifactGroupID, new Dictionary<uint, Artifact_Table>());

            if (!UpgradeList[tableData.ArtifactGroupID].ContainsKey(tableData.Step))
                UpgradeList[tableData.ArtifactGroupID].Add(tableData.Step, tableData);
            else
                UpgradeList[tableData.ArtifactGroupID][tableData.Step] = tableData;
        }

        LinkGroups.Clear();

        foreach (var tableData in GameDBManager.Container.ArtifactLink_Table_data.Values)
        {
            if (!LinkGroups.ContainsKey(tableData.LinkGroup))
                LinkGroups.Add(tableData.LinkGroup, new List<ArtifactLink_Table>());
            LinkGroups[tableData.LinkGroup].Add(tableData);
        }
    }

    public static List<Artifact_Table> GetMakeList()
    {
        return MakeList;
    }

    public static Dictionary<uint, Dictionary<uint, Artifact_Table>> GetUpgradeList()
    {
        return UpgradeList;
    }

    public static Artifact_Table GetArtifactByID(uint ArtifactID)
    {
        foreach (var group in UpgradeList)
        {
            foreach (var t in group.Value)
            {
                if (t.Value.ArtifactID == ArtifactID)
                {
                    return t.Value;
                }
            }
        }

        return null;
    }

    public static void ForeachArtifactUpgradeList(Action<Artifact_Table> artifactTable)
    {
        foreach (var byGroup in UpgradeList)
        {
            foreach (var byStep in byGroup.Value)
            {
                artifactTable.Invoke(byStep.Value);
            }
        }
    }

    public static Dictionary<uint, Artifact_Table>.ValueCollection GetArtifactGroup(uint GroupId)
    {
        return UpgradeList[GroupId].Values;
    }

    public static uint GetArtifactGroupIDByArtifactID(uint artifactID)
    {
        foreach (var tableData in UpgradeList)
        {
            foreach (var byStep in tableData.Value)
            {
                if (byStep.Value.ArtifactID == artifactID)
                {
                    return tableData.Key;
                }
            }
        }

        return 0;
    }

    public static Artifact_Table GetFirstStepArtifact(uint artifactID)
    {
        var artifactData = GetArtifactByID(artifactID);

        if (artifactData == null)
            return null;

        var group = GetArtifactGroup(artifactData.ArtifactGroupID);
        return group != null && group.Count > 0 ? group.First() : null;
    }

    public static uint GetArtifactStep(uint artifactID)
    {
        foreach (var t in GameDBManager.Container.Artifact_Table_data)
        {
            if (t.Value.ArtifactID == artifactID)
                return t.Value.Step;
        }

        return 0;
    }

    /// <summary>
    /// 승급에 필요한 재료 타입 및 등급을 리턴함 
    /// </summary>
    public static List<MaterialType> BuildMaterialList(uint artifactID, bool enableDuplicate = true)
    {
        var data = GetArtifactByID(artifactID);

        if (data == null)
            return new List<MaterialType>();

        /// 이 MaterialCount 는 2020-09-22 에 기획 변경으로 인해 
        /// 필요한 재료 총 개수 -> Material_1 ~ 3 까지 1 부터 몇 까지 재료를 필요로 하느냐로
        /// 변경됐음 . 
        int cnt = data.MaterialCount;
        var result = new List<MaterialType>();

        if (cnt > 0)
        {
            for (int i = 0; i < data.Material_1_Count; i++)
            {
                result.Add(new MaterialType(data.MaterialType, data.Material_1_Grade));

                if (enableDuplicate == false)
                    break;
            }
        }

        if (cnt > 1)
        {
            for (int i = 0; i < data.Material_2_Count; i++)
            {
                result.Add(new MaterialType(data.MaterialType, data.Material_2_Grade));

                if (enableDuplicate == false)
                    break;
            }
        }

        if (cnt > 2)
        {
            for (int i = 0; i < data.Material_3_Count; i++)
            {
                result.Add(new MaterialType(data.MaterialType, data.Material_3_Grade));

                if (enableDuplicate == false)
                    break;
            }
        }

        if (cnt > 3)
            ZLog.LogError(ZLogChannel.UI, " not updated for this ");

        return result;
    }

    public static int GetArtifactMaterialCountRequired(uint artifactID)
    {
        var data = GetArtifactByID(artifactID);

        if (data == null)
            return 0;

        uint result = 0;

        if (data.MaterialCount > 0)
        {
            for (int i = 0; i < data.Material_1_Count; i++)
            {
                result++;
            }
        }

        if (data.MaterialCount > 1)
        {
            for (int i = 0; i < data.Material_2_Count; i++)
            {
                result++;
            }
        }

        if (data.MaterialCount > 2)
        {
            for (int i = 0; i < data.Material_3_Count; i++)
            {
                result++;
            }
        }

        return (int)result;
    }

    public static bool IsArtifactPrevStepExist(uint artifactID)
    {
        var targetArtifact = GetArtifactByID(artifactID);

        if (targetArtifact == null)
            return false;

        var group = GetArtifactGroup(targetArtifact.ArtifactGroupID);

        if (group == null)
            return false;

        foreach (var byStep in group)
        {
            // 해당 그룹에서 타겟 아티팩트 보다 낮은 Step 이 존재한다면 
            if (byStep.Step < targetArtifact.Step)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsArtifactIDExist(uint artifactID)
    {
        foreach (var tableData in GameDBManager.Container.Artifact_Table_data)
        {
            if (tableData.Value.ArtifactID == artifactID)
            {
                return true;
            }
        }

        return false;
    }

    public static uint GetNextArtifactID(uint artifactID)
    {
        var targetArtifact = GetArtifactByID(artifactID);

        if (targetArtifact == null)
            return 0;

        var group = GetArtifactGroup(targetArtifact.ArtifactGroupID);

        if (group == null)
            return 0;

        foreach (var byStep in group)
        {
            if (byStep.Step > targetArtifact.Step)
            {
                return byStep.ArtifactID;
            }
        }

        return 0;
    }

    public static bool IsArtifactNextStepExist(uint artifactID)
    {
        var targetArtifact = GetArtifactByID(artifactID);

        if (targetArtifact == null)
            return false;

        var group = GetArtifactGroup(targetArtifact.ArtifactGroupID);

        if (group == null)
            return false;

        foreach (var byStep in group)
        {
            // 해당 그룹에서 타겟 아티팩트 보다 높은 Step 이 존재한다면 
            if (byStep.Step > targetArtifact.Step)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsSameGroup(uint artifactId01, uint artifactId02)
    {
        uint groupID01 = GetArtifactGroupIDByArtifactID(artifactId01);
        var groupID02 = GetArtifactGroupIDByArtifactID(artifactId02);

        return groupID01 == groupID02;
    }

    public static Artifact_Table GetPrevArtifact(uint GroupId, int Step)
    {
        if (Step == 0)
            return null;

        foreach (var tableData in UpgradeList[GroupId])
        {
            if (tableData.Value.Step == Step - 1)
                return tableData.Value;
        }

        return null;
    }

    public static Artifact_Table GetNextArtifact(uint GroupId, int Step)
    {
        foreach (var tableData in UpgradeList[GroupId])
        {
            if (tableData.Value.Step == Step + 1)
                return tableData.Value;
        }

        return null;
    }

    public static Dictionary<uint, List<ArtifactLink_Table>>.KeyCollection GetLinkGroups()
    {
        return LinkGroups.Keys;
    }

    public static List<ArtifactLink_Table> GetLinkGroupList(uint GroupId)
    {
        return LinkGroups[GroupId];
    }

    public static uint GetLinkGrade(uint linkTid)
    {
        foreach (var t in LinkGroups)
        {
            foreach (var t02 in t.Value)
            {
                if (t02.LinkID == linkTid)
                    return t02.LinkGrade;
            }
        }

        return 0;
    }

    public static bool GetLinkData(uint linkTid, out ArtifactLink_Table data)
    {
        if (GameDBManager.Container.ArtifactLink_Table_data.TryGetValue(linkTid, out data))
            return true;

        return false;
    }

    public static Dictionary<uint, ArtifactLink_Table>.ValueCollection GetAllLink()
    {
        return GameDBManager.Container.ArtifactLink_Table_data.Values;
    }

    //public static bool IsLinkObtained(uint linkID, List<uint> artifactIDs)
    //{
    //    GetLinkData(linkID, out var linkData);

    //    if (linkData == null)
    //        return false;

    //    return artifactIDs.Contains(linkData.MaterialArtifactID_1) && artifactIDs.Contains(linkData.MaterialArtifactID_2);
    //}

    public static bool IsLinkObtained(uint linkID, List<uint> obtainedArtifactIDs)
    {
        GetLinkData(linkID, out var linkData);

        if (linkData == null)
            return false;

        var requiredArtifact01 = GetArtifactByID(linkData.MaterialArtifactID_1);
        var requiredArtifact02 = GetArtifactByID(linkData.MaterialArtifactID_2);

        if (requiredArtifact01 == null)
            return false;
        if (requiredArtifact02 == null)
            return false;

        bool obtained01 = false;
        bool obtained02 = false;

        for (int i = 0; i < obtainedArtifactIDs.Count; i++)
        {
            var artifactData = GetArtifactByID(obtainedArtifactIDs[i]);

            /// 같은 그룹의 아티팩트 데이터 검색 성공 
            if (artifactData.ArtifactGroupID == requiredArtifact01.ArtifactGroupID)
            {
                if (artifactData.Step >= requiredArtifact01.Step)
                {
                    obtained01 = true;
                    if (obtained02)
                        break;
                }
            }

            if (artifactData.ArtifactGroupID == requiredArtifact02.ArtifactGroupID)
            {
                if (artifactData.Step >= requiredArtifact02.Step)
                {
                    obtained02 = true;
                    if (obtained01)
                        break;
                }
            }
        }

        return obtained01 && obtained02;
    }

    public static bool IsArtifactObtained(uint artifactTid, List<uint> myArtifactIDs)
    {
        var targetData = DBArtifact.GetArtifactByID(artifactTid);

        if (targetData == null)
            return false;

        for (int i = 0; i < myArtifactIDs.Count; i++)
        {
            var myArtifactData = DBArtifact.GetArtifactByID(myArtifactIDs[i]);

            /// 같은 그룹 찾았음 
            if (myArtifactData.ArtifactGroupID == targetData.ArtifactGroupID)
            {
                return myArtifactData.Step >= targetData.Step;
            }
        }

        return false;
    }

    /// <summary>
    /// 그룹마다 하나씩 획득된 아티팩트 링크 ID 추가 . 
    /// 존재하면 가장 높은 등급을 등록 및 존재하지않으면 가장 하위 등급 등록 . 
    /// </summary>
    public static void GetLinkIDsByArtifactIDs(
        bool addMinIfNotFound
        , List<uint> artifactIDs
        , ref List<ArtifactLink> outputLinks)
    {
        if (outputLinks == null)
            outputLinks = new List<ArtifactLink>();

        foreach (var group in LinkGroups)
        {
            uint checkGrade = 0;
            uint linkTid = 0;
            uint minGradeLink = uint.MaxValue;
            uint minGradeTid = 0;

            foreach (var linkData in group.Value)
            {
                /// 해당 아티팩트 링크 효과 조건 만족시키는 아티팩트를 발견 
                if (IsArtifactObtained(linkData.MaterialArtifactID_1, artifactIDs)
                    && IsArtifactObtained(linkData.MaterialArtifactID_2, artifactIDs))
                {
                    /// 조건 만족시키는 가장 높은 등급 체킹 
                    if (linkData.LinkGrade > checkGrade)
                    {
                        checkGrade = linkData.LinkGrade;
                        linkTid = linkData.LinkID;
                    }
                }

                if (linkData.LinkGrade < minGradeLink)
                {
                    minGradeLink = linkData.LinkGrade;
                    minGradeTid = linkData.LinkID;
                }
            }

            /// 못찾음 
            if (linkTid == 0)
            {
                /// 못찾았을때는 해당 변수가 true 일때만 등록함 . (최소 레벨)
                if (addMinIfNotFound)
                    outputLinks.Add(new ArtifactLink(minGradeTid));
            }
            /// 찾음 
            else
            {
                outputLinks.Add(new ArtifactLink(linkTid));
            }
        }
    }

    public static ArtifactLink_Table GetLinkDataQualified(uint linkGroupID, List<uint> artifactIDs)
    {
        ArtifactLink_Table result = null;
        uint checkGrade = 0;

        foreach (var linkData in LinkGroups[linkGroupID])
        {
            /// 해당 아티팩트 링크 효과 조건 만족시키는 아티팩트를 발견 
            if (IsArtifactObtained(linkData.MaterialArtifactID_1, artifactIDs)
                && IsArtifactObtained(linkData.MaterialArtifactID_2, artifactIDs))
            {
                /// 조건 만족시키는 가장 높은 등급 체킹 
                if (linkData.LinkGrade > checkGrade)
                {
                    result = linkData;
                    checkGrade = linkData.LinkGrade;
                }
            }
        }

        /// 조건 맞는애없으면 NULL 이 될 수있음
        return result;
    }

    public static uint GetArtifactLinkGroupIDByLinkID(uint linkID)
    {
        GetLinkData(linkID, out var t);

        if (t == null)
            return 0;

        return t.LinkGroup;
    }

    public static int GetLinkGroupCount()
    {
        return LinkGroups.Count;
    }
}