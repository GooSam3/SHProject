using GameDB;
using System;
using System.Collections.Generic;
using ZDefine;
using ZNet.Data;
using static DBArtifact;

public class UIFrameArtifactManufactureMaterialHandler : UIFrameArtifactMaterialManagementBase
{
    UIFrameArtifactManufacture Manufacture;
    UIFrameArtifactOverlayPopup_SelectMaterial popUp;

    Func<uint> artifactIDGetter;

    //private uint curArtifactID;
    //private uint prevArtifactID;

    private List<MaterialType> curRequiredList;

    private Action onRegisterDone;

    /// <summary>
    /// 팝업에도 이 데이터를 씀 
    /// </summary>
    private List<MaterialDataOnSlot> Data_MyMaterialSelectSlots;
    private List<MaterialDataOnSlot> Data_RegisteredSlots;

    public override void Initialize(UIFrameArtifact artifact, bool showCount, bool disableOnCountZero)
    {
        base.Initialize(artifact, showCount, disableOnCountZero);
        Data_MyMaterialSelectSlots = new List<MaterialDataOnSlot>();
        Data_RegisteredSlots = new List<MaterialDataOnSlot>();
    }

    public override void Release()
    {
        base.Release();
    }

    public void SetGetter_SelectedArtifactID(Func<uint> callback)
    {
        artifactIDGetter = callback;
    }

    public void AddListener_OnRegisterDone(Action callback)
    {
        onRegisterDone += callback;
    }

    /// <summary>
    /// 해당 아티팩트의 필요한 재료 개수만큼 슬롯 데이터 할당 및 클리어함 .
    /// ArtifactID 가 0 은 더이상 제작 불가함을 의미함 . 
    /// </summary>
    public void ResetAll(uint artifactID)
    {
        var data = DBArtifact.GetArtifactByID(artifactID);

        if (data == null ||
            artifactID == 0)
        {
            ClearList();
            Data_MyMaterialSelectSlots.Clear();
            Data_RegisteredSlots.Clear();
            RefreshScroll();
            return;
        }

        // 일단 빈 데이터 확보 
        AssignEmptyList(DBArtifact.GetArtifactMaterialCountRequired(artifactID));

        // 현재 아티팩트에 필요한 재료 업데이트 
        curRequiredList = DBArtifact.BuildMaterialList(artifactID);

        /// 정렬 
        curRequiredList.Sort((t01, t02) =>
        {
            if (t01.type == E_ArtifactMaterialType.Pet && t02.type == E_ArtifactMaterialType.Pet ||
            t01.type == E_ArtifactMaterialType.Vehicle && t02.type == E_ArtifactMaterialType.Vehicle)
            {
                return t01.grade.CompareTo(t02.grade);
            }
            else
            {
                return t01.type.CompareTo(t02.type);
            }
        });

        ResetContextDataList();

        // 내가 재료로 사용가능한 재료들 세팅 
        Data_MyMaterialSelectSlots = GetMyMaterialInventory(true, artifactID, curRequiredList);
        // 등록되는 슬롯 빈슬롯으로 초기화 
        AssignEmptyList(ref Data_RegisteredSlots, curRequiredList.Count);

        /// 일단 RegisterSlot 은 자신이 어떤 Type, Grade 의 데이터가 꽃혀야하는지
        /// 알아야하기때문에 먼저 세팅. 
        for (int i = 0; i < Data_RegisteredSlots.Count; i++)
        {
            var t = Data_RegisteredSlots[i];
            string tierBG = DBUIResouce.GetBGByTier((byte)curRequiredList[i].grade);

            /// 우선 자신이 어떤 전용 슬롯인지만 식별을 위해서 
            /// type,grade,bg 만 세팅함. 
            t.matGrade = (byte)curRequiredList[i].grade;
            t.matType = curRequiredList[i].type;
            t.gradeSpriteName = tierBG;

            t = Data[i];
            t.matGrade = (byte)curRequiredList[i].grade;
            t.matType = curRequiredList[i].type;
            t.gradeSpriteName = tierBG;
        }

        // 어댑터 업데이트 
        RefreshScroll();
    }

    protected override void OnSlotClicked(int index, MaterialDataOnSlot data)
    {
        if (artifactIDGetter == null)
        {
            ZLog.LogError(ZLogChannel.UI, " please set Artifact ID Getter Method ");
            return;
        }

        curRequiredList = DBArtifact.BuildMaterialList(artifactIDGetter());
        popUp = base.FrameArtifact.OpenSelectMaterialPopup();
        popUp.SetupScroll(artifactIDGetter(), Data_MyMaterialSelectSlots, Data_RegisteredSlots, curRequiredList);
        popUp.SetListener_OnRegisterDone(OnRegisterEnd);
        popUp.SetDataEqualChecker(IsRegisteredDataDifferent);
    }

    /// <summary>
    /// 
    /// </summary>
    private void ResetContextDataList()
    {
        Data_RegisteredSlots.Clear();
        Data_MyMaterialSelectSlots.Clear();
    }

    private void OnRegisterEnd(List<MaterialDataOnSlot> myMats, List<MaterialDataOnSlot> regMats)
    {
        /// 팝업에 사용할 재료로 선택돼 등록되어 있던 재료들을 Manufacture 메인 화면의 우측에 있는
        /// 재료 리스트에 Copy 함 ( *** 새로운 인스턴스를 할당함 . 리스트 참조 변경하는게 아님 *** ) 
        CopyData(regMats, Data);
        /// 등록된 재료 , 등록 가능한 나의 재료들 Copy . 마찬가지로 참조 변경 X 
        CopyData(regMats, Data_RegisteredSlots);
        CopyData(myMats, Data_MyMaterialSelectSlots);
        RefreshScroll();
        onRegisterDone?.Invoke();
    }

    /// <summary>
    /// 재료로 사용할수 있는 나의 펫들을 더해서 뱉어줌 
    /// 참고로 카운팅됨 
    /// </summary>
    private List<MaterialDataOnSlot> GetMyMaterialInventory(bool sort, uint artifactID, List<MaterialType> requiredList)
    {
        var data = DBArtifact.GetArtifactByID(artifactID);
        List<MaterialDataOnSlot> result = new List<MaterialDataOnSlot>(requiredList.Count);

        /// 해당 아티팩트 승급을 위해서 조건에 맞는 
        /// 내가 가지고 있는 모든 재료들을 순차대로 세팅함 

        // 펫 순회 
        foreach (var pet in Me.CurCharData.GetPetDataList())
        {
            if (pet.Cnt > 0)
            {
                // 내가 가지고 있는 펫의 정보를 가져옴 
                var petData = DBPet.GetPetData(pet.PetTid);

                if (petData != null)
                {
                    // 잠재적으로 재료인 나의 펫을 
                    // requiredList 에 존재하는지 테스트함 . 존재하면 재료로 더함 
                    if (requiredList.Exists(t =>
                    t.type == E_ArtifactMaterialType.Pet
                    && t.grade == petData.Grade))
                    {
                        result.Add(new MaterialDataOnSlot(
                            pet.PetTid
                            , E_ArtifactMaterialType.Pet
                            , petData.Grade
                            , DBUIResouce.GetBGByTier(petData.Grade)
                            , pet.Cnt
                            , false));
                    }
                }
            }
        }

        // 탈것 순회 
        foreach (var vehicle in Me.CurCharData.GetRideDataList())
        {
            if (vehicle.Cnt > 0)
            {
                // 내가 가지고 있는 펫의 정보를 가져옴 
                var vehicleData = DBPet.GetPetData(vehicle.PetTid);

                if (vehicleData != null)
                {
                    if (requiredList.Exists(t =>
                    t.type == E_ArtifactMaterialType.Vehicle
                    && t.grade == vehicleData.Grade))
                    {
                        result.Add(new MaterialDataOnSlot(
                            vehicle.PetTid
                            , GameDB.E_ArtifactMaterialType.Vehicle
                            , vehicleData.Grade
                            , DBUIResouce.GetBGByTier(vehicleData.Grade)
                            , vehicle.Cnt
                            , false));
                    }
                }
            }
        }

        if (sort)
        {
            /// grade, artifactID 기준 정렬 
            result.Sort((t01, t02) =>
            {
                if (t01.matGrade != t02.matGrade)
                {
                    return t01.matGrade.CompareTo(t02.matGrade);
                }
                else
                {
                    return t01.tid.CompareTo(t02.tid);
                }
            });
        }

        return result;
    }

    public bool FillMaterialsAuto()
    {
        if (Data_MyMaterialSelectSlots.Count == 0)
        {
            return false;
        }

        FillMaterialsAuto(Data_MyMaterialSelectSlots, Data_RegisteredSlots, curRequiredList);
        CopyData(Data_RegisteredSlots, Data);
        RefreshScroll();

        return true;
    }

    /// <summary>
    /// dest 를 채울정도로만 source 에서 dest 로 이동시킴 
    /// </summary>
    public void FillMaterialsAuto(
        List<MaterialDataOnSlot> source
        , List<MaterialDataOnSlot> dest
        , List<MaterialType> allRequiredList)
    {
        var matsNeeded = GetCurRequiredMaterialList(dest, allRequiredList);

        for (int i = 0; i < matsNeeded.Count; i++)
        {
            // 해당 데이터를 이동 시도를 시킴 . 
            MoveData(false, source, dest, matsNeeded[i].type, matsNeeded[i].grade);
        }
    }

    /// <summary>
    ///  등록 되고 있는 재료 슬롯 데이터 리스트 
    ///  현재 제작을 위해 등록해되어야 하는 전체 재료 리스트를 받아서 
    ///  어떤걸 추가로 등록해줘야하는지 리스트를 구함 
    /// </summary>
    public List<MaterialType> GetCurRequiredMaterialList(
        List<MaterialDataOnSlot> curRegisteredMaterials
        , List<MaterialType> requiredList)
    {
        List<MaterialType> result = new List<MaterialType>(requiredList.Count);
        List<int> excludeCheckingIndices = new List<int>(curRegisteredMaterials.Count);

        /// 현재 필요한 요구 재료들을 순회한다 
        for (int i = 0; i < requiredList.Count; i++)
        {
            bool obtain = false;
            var checkType = requiredList[i].type;
            var checkGrade = requiredList[i].grade;

            for (int j = 0; j < curRegisteredMaterials.Count; j++)
            {
                var slotData = curRegisteredMaterials[j];

                if (excludeCheckingIndices.Contains(j) == false
                    && slotData.cntByContext > 0
                    && slotData.matType == checkType
                    && slotData.matGrade == checkGrade)
                {
                    excludeCheckingIndices.Add(j);
                    obtain = true;
                    break;
                }
            }

            if (obtain == false)
            {
                result.Add(new MaterialType(checkType, checkGrade));
            }
        }

        return result;
    }

    bool IsRegisteredDataDifferent(List<MaterialDataOnSlot> dest)
	{
        if (Data_RegisteredSlots.Count != dest.Count)
            return true;

		for (int i = 0; i < Data_RegisteredSlots.Count; i++)
		{
			if (Data_RegisteredSlots[i].tid != dest[i].tid || Data_RegisteredSlots[i].cntByContext != dest[i].cntByContext)
			{
                return true; 
			}
		}

        return false;
	}

    void OnClickedSlot_RequiredMaterial(MaterialDataOnSlot data)
    {
        ZLog.LogError(ZLogChannel.UI, " no imple ");
    }

    void OnClickedSlot_SelectMaterial(MaterialDataOnSlot data)
    {
        ZLog.LogError(ZLogChannel.UI, " no imple ");
    }
}
