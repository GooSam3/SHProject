using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DBArtifact;
using static UIFrameArtifactMaterialManagementBase;

public class UIFrameArtifactOverlayPopup_SelectMaterial : UIFrameArtifactOverlayPopupBase
{
    /// <summary>
    ///  요구 재료 텍스트 띄어주기 위한 데이터를 담는 클래스 
    /// </summary>
    public class RequiredMaterialText
    {
        public uint grade;
        public E_PetType petType;
        public uint curCnt;
        public uint requiredCnt;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    [SerializeField] private UIFrameArtifactSelectMaterialMaterialHandler materialSelectHandler;
    [SerializeField] private UIFrameArtifactSelectRequiredMaterialHandler registeredMaterialHandler;

    #region UI Variables
    //[SerializeField] private Text txtRequiredMatCnt;
    [SerializeField] private GameObject noSelectableMaterialObj;

    [SerializeField] private UIFrameArtifactRequiredMatCountText requiredGuideSourceObj;
    [SerializeField] private RectTransform requiredGuideObjParent;
    [SerializeField] private ZScrollRect requiredGuideScrollRect;

    [SerializeField] private Button btnRegister;
    #endregion
    #endregion

    #region System Variables
    public delegate void OnRegisterEnd(List<MaterialDataOnSlot> myMaterials, List<MaterialDataOnSlot> registeredMaterials);
    /// <summary>
    ///  등록이 끝나면은 호출 . 
    /// </summary>
    private OnRegisterEnd onRegisterDone;
    private List<UIFrameArtifactRequiredMatCountText> requiredGuides;

    private List<MaterialType> requiredMats;

    private Func<List<MaterialDataOnSlot> , bool> isRegisteredDataDirtyChecker;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void SetListener_OnRegisterDone(OnRegisterEnd callback)
    {
        onRegisterDone = callback;
    }
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameArtifact frameArtifact)
    {
        base.Initialize(frameArtifact);
        requiredGuides = new List<UIFrameArtifactRequiredMatCountText>();
        materialSelectHandler.Initialize(frameArtifact, true, false);
        materialSelectHandler.AddListener_OnClick(OnSlotClicked_SelectMaterial);
        registeredMaterialHandler.Initialize(frameArtifact, false, true);
        registeredMaterialHandler.AddListener_OnClick(OnSlotClicked_RegisteredList);
    }

    /// <summary>
    /// 지금 재료 선택을 진행하고 있는 아티팩트의 ID 
    /// 이걸 알면 필요한 재료 및 내가 어떤 재료를 띄어줘야하는지를 알 수 있음  
    /// </summary>
    public void SetupScroll(
        uint artifactID
        , List<MaterialDataOnSlot> myMaterials
        , List<MaterialDataOnSlot> matsRegistered
        , List<MaterialType> requiredMats)
    {
        this.requiredMats = requiredMats;
        materialSelectHandler.Set(myMaterials);
        registeredMaterialHandler.Set(matsRegistered, requiredMats);
        UpdateExtraUI();
    }

    public void SetDataEqualChecker(Func<List<MaterialDataOnSlot>, bool> checker)
    {
        this.isRegisteredDataDirtyChecker = checker;
    }

    public override void Open()
    {
        base.Open();
        btnRegister.interactable = false;
    }

    public override void Close()
    {
        base.Close();
        onRegisterDone = null;
        btnRegister.interactable = false;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 이미 재료로 등록된 재료 슬롯칸에 있는 재료가 클릭되면 
    /// 다시 나의 재료칸으로 넘어감. 
    /// </summary>
    private void OnSlotClicked_RegisteredList(int index, MaterialDataOnSlot data)
    {
        // 빈 슬롯 클릭 
        if (data == null 
            || data.cntByContext == 0)
            return;

        // registeredMaterialHandler.MoveDataIndex(index, materialSelectHandler.DataList);
        registeredMaterialHandler.MoveSpecificData_DestCounting(index, materialSelectHandler.DataList);
        RefreshExtraScroll();
        UpdateExtraUI();

        btnRegister.interactable = isRegisteredDataDirtyChecker(registeredMaterialHandler.DataList);

        //materialSelectHandler.IncreaseMaterial(data.tid);
        //materialSelectHandler.RefreshScroll();
        //registeredMaterialHandler.InsertData(null, index);
        //registeredMaterialHandler.RefreshScroll();
        //UpdateExtraUI();
    }

    private void RefreshExtraScroll()
    {
        registeredMaterialHandler.RefreshScroll();
        materialSelectHandler.RefreshScroll();
    }

    /// <summary>
    /// 재료 자체 이외 UI 들 업데이트 
    /// </summary>
    private void UpdateExtraUI()
    {
        // txtRequiredMatCnt.text = string.Format("{0} / {1}", registeredMaterialHandler.DataExistCount, registeredMaterialHandler.DataTotalCount);
        noSelectableMaterialObj.SetActive(materialSelectHandler.DataList.Count == 0);

        /// 요구되는 재료 텍스트 세팅 
        if (requiredMats != null)
        {
            var textsByGrade = new List<RequiredMaterialText>();

            for (int i = 0; i < requiredMats.Count; i++)
            {
                var target = textsByGrade.Find(t => t.grade == requiredMats[i].grade);

                if (target == null)
                {
                    target = new RequiredMaterialText();
                    target.grade = requiredMats[i].grade;
                    target.petType = requiredMats[i].type == E_ArtifactMaterialType.Pet ? E_PetType.Pet : E_PetType.Vehicle;
                    target.curCnt = (uint)registeredMaterialHandler.GetExistCountByGrade(requiredMats[i].type, requiredMats[i].grade);
                    textsByGrade.Add(target);
                }

                target.requiredCnt++;
            }

            if (requiredGuides.Count < textsByGrade.Count)
            {
                int addCnt = textsByGrade.Count - requiredGuides.Count;

                for (int i = 0; i < addCnt; i++)
                {
                    var newTxt = Instantiate(requiredGuideSourceObj, requiredGuideObjParent);
                    requiredGuides.Add(newTxt);
                }
            }

            requiredGuides.ForEach(t => t.gameObject.SetActive(false));

            for (int i = 0; i < textsByGrade.Count; i++)
            {
                requiredGuides[i].Set((byte)textsByGrade[i].grade, textsByGrade[i].petType, textsByGrade[i].curCnt, textsByGrade[i].requiredCnt);
                requiredGuides[i].gameObject.SetActive(true);
            }

            requiredGuideScrollRect.verticalNormalizedPosition = 1;
        }
    }

    /// <summary>
    ///  재료로 사용할 재료를 선택하면 해당 재료가 오른쪽 재료칸으로 넘어감 
    /// </summary>
    private void OnSlotClicked_SelectMaterial(int index, MaterialDataOnSlot data)
    {
        // 정상적인 상황이라면 슬롯이 부족한 현상은 없어야함 . 고로 예외처리 하지않음 . 

        // 해당 재료가 더 등록될 수 있는지 체크 . 
        if (registeredMaterialHandler.CanRegister(data) == false)
        {
            FrameArtifact.OpenNotiUp(DBLocale.GetText("ArtifactAlreadyMax"), "알림");
        }
        // 해당 재료를 하나 이상 가지고 있는지 체크 
        else if (data.cntByContext > 0)
        {
            // materialSelectHandler.MoveData(registeredMaterialHandler.DataList, data.matType, data.matGrade);
            materialSelectHandler.MoveSpecificData(data, registeredMaterialHandler.DataList);
            RefreshExtraScroll();
            UpdateExtraUI();

            btnRegister.interactable = isRegisteredDataDirtyChecker(registeredMaterialHandler.DataList);
        }
    }

    //private void Register(MaterialDataOnSlot data)
    //{
    //    materialSelectHandler.DecreaseMaterial(data.tid);
    //    materialSelectHandler.RefreshScroll();
    //    registeredMaterialHandler.RegisterMaterial(data);
    //    registeredMaterialHandler.RefreshScroll();
    //    UpdateExtraUI();
    //}
    #endregion

    #region Inspector Events 
    #region OnClick
    public void OnClick_Register()
    {
        onRegisterDone?.Invoke(materialSelectHandler.DataList, registeredMaterialHandler.DataList);
        onRegisterDone = null;
        Close();
    }
    #endregion
    #endregion

}
