using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;
using static DBArtifact;
using static UIFrameArtifactAbilityActionBuilder;
// using static UIFrameArtifactAbilityActionBuilder;

public class UIFrameArtifactLink : MonoBehaviour
{
    public class SingleAbilityAction
    {
        public uint linkTid;
        public uint linkGroupTid;
        public uint grade;
        public bool isObtained;
        public bool isMyGrade;
        public List<AbilityActionTitleValuePair> data;

        public SingleAbilityAction(uint linkTid, uint linkGroupTid, uint grade, bool isObtained, bool isMyGrade, List<AbilityActionTitleValuePair> data)
        {
            this.linkTid = linkTid;
            this.linkGroupTid = linkGroupTid;
            this.grade = grade;
            this.isObtained = isObtained;
            this.isMyGrade = isMyGrade;
            this.data = data;
        }

        //        public UIFrameArtifactLinkSingleAbilityAction ui;

    }

    public struct LinkDataExtended
    {
        public struct MaterialCommon
        {
            /// <summary>
            /// 만약 해당 링크의 재료 Material 그룹과 내것이 같은게 있다면 
            /// 내것이 들어가게됨 . 즉, 만약 링크가 가장 하위 레벨이 적용되고 있는데
            /// Mat01 이 해당 재료보다 높은 레벨의 아티팩트를 내가 가지고 있고 
            /// Mat02 는 해당 재료와 딱 매칭되는 애를 가지고 있다 , 그러면 
            /// 이 tid 및 grade, gradeSprite 에는 실제 내가 보유중인 기본 재료보다 더 높은 아티팩트가 들어감
            /// </summary>
            public uint oriMatTid;
            public byte oriGrade;
            public Sprite oriGradeSprite;

            public uint tid_forDisplay;
            public byte grade_forDisplay;
            public Sprite gradeSprite_forDisplay;
            /// <summary>
            /// 테이블에 데이터 존재하는가 ? 
            /// </summary>
            public bool isDataExistOnTable;
            public bool isObtained;
            public string artifactIcon;
            public string linkIcon;
            // public Color gradeColor;
        }

        public ArtifactLink baseData;

        public bool isObtained;
        public bool isSelected;

        public uint grade;

        public string titleKey;

        public MaterialCommon material01;
        public MaterialCommon material02;

        //public LinkDataExtended(
        //    ArtifactLink baseData
        //    , bool isObtained
        //    , bool isSelected
        //    , uint grade
        //    , string titleKey
        //    , MaterialCommon material01
        //    , MaterialCommon material02)
        //{
        //    this.baseData = baseData;
        //    this.isObtained = isObtained;
        //    this.isSelected = isSelected;
        //    this.grade = grade;
        //    this.titleKey = titleKey;
        //    this.material01 = material01;
        //    this.material02 = material02;
        //}
    }

    public class AbilityActionDataIntegrated
    {
        public uint groupID;
        public List<SingleAbilityAction> abilities;
    }

    [Serializable]
    public class UIGroup_Look
    {
        [Serializable]
        public class MaterialArtifact
        {
            public Image imgArtifactIcon;
            public List<GameObject> activeOnObtained;
            public List<Image> gradeImgs;

            public bool isArtifactObtained;
            public uint artifactID;

            public void Set(LinkDataExtended.MaterialCommon data, Color inactiveColor)
            {
                bool active = data.isObtained && data.isDataExistOnTable;

                if (data.isDataExistOnTable)
                {
                    imgArtifactIcon.sprite = ZManagerUIPreset.Instance.GetSprite(data.artifactIcon);
                    gradeImgs.ForEach(t => t.sprite = data.gradeSprite_forDisplay);
                }

                Color colorByActive = active ? Color.white : inactiveColor;
                imgArtifactIcon.color = colorByActive;
                gradeImgs.ForEach(t => t.color = colorByActive);

                imgArtifactIcon.gameObject.SetActive(true);
                gradeImgs.ForEach(t => t.gameObject.SetActive(true));
                activeOnObtained.ForEach(t => t.gameObject.SetActive(active));

                isArtifactObtained = data.isObtained;
                artifactID = data.tid_forDisplay;
            }
        }

        [Serializable]
        public class ArtifactItemEffect
        {
            public byte grade;
            public GameObject obj;
        }

        public List<ArtifactItemEffect> effects;

        public List<GameObject> activeOnObtained;

        public Color inactiveColor;

        public MaterialArtifact artifact_left;
        public MaterialArtifact artifact_right;
    }

    [Serializable]
    public class UIGroup_Stat
    {
        public Text txtName;

        public ScrollRect scrollRect;
        public RectTransform scrollRectRectTransform;

        public RectTransform abilityGroupParent;
        public RectTransform abilitySeparatorObj;

        //        public Color obtainedAbilityTxtColor;
        //     public Color notObtainedAbilityTxtColor;

        /// <summary>
        ///  Grade , Ability List 
        /// </summary>
        [HideInInspector] public Dictionary<uint, UIFrameArtifactLinkSingleAbilityAction> abilityUIgroups = new Dictionary<uint, UIFrameArtifactLinkSingleAbilityAction>();
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollArtifactLinkListAdapter ScrollAdapter;
    [SerializeField] private List<GameObject> activeObjs;

    [SerializeField] private UIGroup_Look uiGroup_Look;
    [SerializeField] private UIGroup_Stat uiGroup_Stat;

    [SerializeField] private UIFrameArtifactSingleAbilityAction abilityActionTextSourceObj;
    [SerializeField] private UIFrameArtifactLinkSingleAbilityAction abilityActionGradeGroupSourceObj;
    #endregion
    #endregion

    #region System Variables
    private UIFrameArtifact ArtifactFrame;

    private List<LinkDataExtended> LinkData;
    private LinkDataExtended curSelectedLinkData;

    List<uint> myArtifacts;
    /// <summary>
    ///  LinkGroupID , AbilityAction 통합 데이터 
    /// </summary>
    private Dictionary<uint, AbilityActionDataIntegrated> linkAbilityActionCached;

    private uint curSelectedLinkID;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    public void Initialize(UIFrameArtifact artifactFrame)
    {
        myArtifacts = new List<uint>(Me.CurCharData.ArtifactItemList.Count);
        int groupCnt = DBArtifact.GetLinkGroupCount();
        ArtifactFrame = artifactFrame;
        linkAbilityActionCached = new Dictionary<uint, AbilityActionDataIntegrated>();
        LinkData = new List<LinkDataExtended>(groupCnt > 0 ? groupCnt : 1);
        InitScrollAdapter();
        activeObjs.ForEach(t => t.SetActive(false));
    }

    public void Open()
    {
        BuildLinkData();

        UpdateBySlotSelected(LinkData.Count > 0 ? LinkData.First().baseData.tid : 0);

        gameObject.SetActive(true);
        activeObjs.ForEach(t => t.SetActive(true));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        activeObjs.ForEach(t => t.SetActive(false));
    }
    #endregion

    #region Private Methods
    private void InitScrollAdapter()
    {
        var slot = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(ScrollArtifactLinkListSlot));
        ScrollAdapter.Parameters.ItemPrefab = slot.GetComponent<RectTransform>();
        var pf = ScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);
        ScrollAdapter.AddListener_OnClick(OnSlotClicked);
        ScrollAdapter.AddListener_OnClickedLeftMat(OnSlotClicked_LeftMat);
        ScrollAdapter.AddListener_OnClickedRightMat(OnSlotClicked_RightMat);
        ScrollAdapter.Initialize();
    }

    private void SetSelectedLink(uint id)
    {
        curSelectedLinkID = id;
        curSelectedLinkData = LinkData.Find(t => t.baseData.tid == id);

        for (int i = 0; i < LinkData.Count; i++)
        {
            var t = LinkData[i];
            t.isSelected = id != 0 && t.baseData.tid == id;
            LinkData[i] = t;
        }

        if (id != 0)
        {
            DBArtifact.GetLinkData(id, out var data);

            /// 해당 링크 그룹의 AbilityAction 이미 추가됐는지 체킹후 안돼있으면 추가후 캐싱해놓음 
            if (linkAbilityActionCached.ContainsKey(data.LinkGroup) == false)
            {
                var linkListByGroup = DBArtifact.GetLinkGroupList(data.LinkGroup);

                if (linkListByGroup != null)
                {
                    AbilityActionDataIntegrated integratedData = new AbilityActionDataIntegrated();
                    integratedData.groupID = data.LinkGroup;
                    integratedData.abilities = new List<SingleAbilityAction>(linkListByGroup.Count);

                    /// 해당 그룹의 링크 데이터 순회 
                    for (int i = 0; i < linkListByGroup.Count; i++)
                    {
                        List<AbilityActionTitleValuePair> txtPair = new List<AbilityActionTitleValuePair>();
                        ArtifactFrame.AbilityActionBuildHelper.BuildAbilityActionTexts(ref txtPair, linkListByGroup[i].AbilityActionID);

                        // 링크 그룹의 하나의 링크 AbilityAciton 정보 추가 
                        integratedData.abilities.Add(new SingleAbilityAction(
                            linkListByGroup[i].LinkID
                            , linkListByGroup[i].LinkGroup
                            , linkListByGroup[i].LinkGrade
                            , false
                            , false
                            , txtPair));
                    }

                    linkAbilityActionCached.Add(data.LinkGroup, integratedData);
                }
            }

            // isObtained 정보 Update 
            if (linkAbilityActionCached.ContainsKey(data.LinkGroup))
            {
                /// 데이터는 전에 세팅돼었으니
                ///  Context Data 를 세팅함 
                foreach (var integratedAB in linkAbilityActionCached)
                {
                    foreach (var singleAB in integratedAB.Value.abilities)
                    {
                        uint myLinkTid = 0;
                        Me.CurCharData.GetMyLink(singleAB.linkGroupTid, out myLinkTid);

                        singleAB.isObtained = DBArtifact.IsLinkObtained(singleAB.linkTid, myArtifacts);
                        singleAB.isMyGrade = singleAB.grade == DBArtifact.GetLinkGrade(myLinkTid);
                    }
                }
            }
        }
    }

    private void UpdateBySlotSelected(uint linkTid)
    {
        SetSelectedLink(linkTid);
        ScrollAdapter.SetSelectedData_Tid(linkTid);
        UpdateUI();
    }

    private void OnSlotClicked(int dataIndex, LinkDataExtended data)
    {
        UpdateBySlotSelected(data.baseData.tid);
    }

    private void OnSlotClicked_LeftMat(uint id)
    {
        ArtifactFrame.OpenArtifactInfoPopup(id);
    }

    private void OnSlotClicked_RightMat(uint id)
    {
        ArtifactFrame.OpenArtifactInfoPopup(id);
    }

    /// <summary>
    /// 현재 스크롤에 띄어줄 Link Data 를 세팅함 . 
    /// </summary>
    private void BuildLinkData()
    {
        myArtifacts.Clear();
        List<ArtifactLink> linksToShow = new List<ArtifactLink>();

        /// 내 아티팩트 ++ 
        foreach (var artifact in Me.CurCharData.ArtifactItemList)
        {
            myArtifacts.Add(artifact.Value.ArtifactTid);
        }

        DBArtifact.GetLinkIDsByArtifactIDs(true, myArtifacts, ref linksToShow);

        for (int i = 0; i < linksToShow.Count; i++)
        {
            var set = default(LinkDataExtended);
            DBArtifact.GetLinkData(linksToShow[i].tid, out var linkTableData);
            uint tid = linksToShow[i].tid;
            bool isLinkObtained = DBArtifact.IsLinkObtained(tid, myArtifacts);

            set.baseData = linksToShow[i];
            set.isObtained = isLinkObtained;
            set.isSelected = curSelectedLinkID == tid;

            ///// 1 번째 재료 세팅 
            var matData01 = DBArtifact.GetArtifactByID(linkTableData.MaterialArtifactID_1);

            if (matData01 == null)
            {
                ZLog.LogError(ZLogChannel.UI,
                    string.Format(
                        "Artifact (tid : {0}) referenced by ArtifactLink (tid : {1}) does not exist. Fix this"
                        , set.material01.oriMatTid, tid));
                set.material01.isObtained = false;
                set.material01.isDataExistOnTable = false;
            }
            else
            {
                set.material01.oriMatTid = linkTableData.MaterialArtifactID_1;
                set.material01.oriGrade = matData01.Grade;
                set.material01.oriGradeSprite = set.material01.gradeSprite_forDisplay = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(matData01.Grade));
                set.material01.tid_forDisplay = linkTableData.MaterialArtifactID_1;
                set.material01.grade_forDisplay = matData01.Grade;

                set.material01.isDataExistOnTable = true;
                set.material01.isObtained = Me.CurCharData.IsArtifactObtained(matData01.ArtifactID);
                set.material01.artifactIcon = matData01.Icon;
                set.material01.linkIcon = linkTableData.LinkImg_1;

                if (set.material01.isObtained)
                {
                    var myArtifactDataInSameGroup = DBArtifact.GetArtifactByID(Me.CurCharData.GetMyArtifactTIDByGroupID(matData01.ArtifactGroupID));

                    /// ** 내가 지금 가지고 있는 아티팩트가 요구 아티팩트보다 레벨이 높다면 
                    /// 내가 가지고 있는 애로 보이게끔 하기 위해서 데이터 갈아치움 
                    if (myArtifactDataInSameGroup.Step > matData01.Step)
                    {
                        set.material01.tid_forDisplay = myArtifactDataInSameGroup.ArtifactID;
                        set.material01.grade_forDisplay = myArtifactDataInSameGroup.Grade;
                        set.material01.gradeSprite_forDisplay = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(myArtifactDataInSameGroup.Grade));
                    }
                }
            }

            ///// 2 번째 재료 세팅 
            var matData02 = DBArtifact.GetArtifactByID(linkTableData.MaterialArtifactID_2);

            if (matData02 == null)
            {
                ZLog.LogError(ZLogChannel.UI,
                    string.Format(
                        "Artifact (tid : {0}) referenced by ArtifactLink (tid : {1}) does not exist. Fix this"
                        , linkTableData.MaterialArtifactID_2, tid));
                set.material02.isObtained = false;
                set.material02.isDataExistOnTable = false;
            }
            else
            {
                set.material02.oriMatTid = linkTableData.MaterialArtifactID_2;
                set.material02.tid_forDisplay = linkTableData.MaterialArtifactID_2;
                set.material02.oriGrade = matData02.Grade;
                set.material02.oriGradeSprite = set.material02.gradeSprite_forDisplay = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(matData02.Grade));

                set.material02.isDataExistOnTable = true;
                set.material02.isObtained = Me.CurCharData.IsArtifactObtained(matData02.ArtifactID);
                set.material02.grade_forDisplay = matData02.Grade;
                set.material02.artifactIcon = matData02.Icon;
                set.material02.linkIcon = linkTableData.LinkImg_2;

                if (set.material02.isObtained)
                {
                    var myArtifactDataInSameGroup = DBArtifact.GetArtifactByID(Me.CurCharData.GetMyArtifactTIDByGroupID(matData02.ArtifactGroupID));

                    if (myArtifactDataInSameGroup.Step > matData02.Step)
                    {
                        set.material02.tid_forDisplay = myArtifactDataInSameGroup.ArtifactID;
                        set.material02.grade_forDisplay = myArtifactDataInSameGroup.Grade;
                        set.material02.gradeSprite_forDisplay = ZManagerUIPreset.Instance.GetSprite(DBUIResouce.GetBGByTier(myArtifactDataInSameGroup.Grade));
                    }
                }
            }

            //bool matObtained01 = set.material01.isObtained;
            //bool matObtained02 = set.material02.isObtained;

            set.grade = linkTableData.LinkGrade;

            set.titleKey = linkTableData.LinkTitle;

            #region Ability Setting 

            //SingleAbilityAction ability = null;

            //if (i < LinkData.Count)
            //{
            //    set.ability = LinkData[i].ability;
            //}

            ///// Ability 데이터 할당 및 세팅 
            //if (set.ability == null)
            //{
            //    List<uint> abilityActionIDs = linkTableData.AbilityActionID;
            //    List<AbilityActionTitleValuePair> txtPair = new List<AbilityActionTitleValuePair>();

            //    ArtifactFrame.AbilityActionBuildHelper.BuildAbilityActionTexts(txtPair, abilityActionIDs);
            //    set.ability = new SingleAbilityAction(linkTableData.LinkGrade, false, txtPair);
            //}

            //set.ability.isObtained = isLinkObtained;

            #endregion

            #region TEST
            //set = TEST_GetRandom();
            #endregion

            if (i < LinkData.Count)
            {
                LinkData[i] = set;
            }
            else
            {
                LinkData.Add(set);
            }
        }

        //#region TEST 
        //ZLog.LogError(ZLogChannel.UI, " --------------------my arti links----------- ");

        //foreach (var t in LinkData)
        //{
        //    ZLog.LogError(ZLogChannel.UI, t.tid.ToString());
        //}
        //#endregion
    }

    private void UpdateUI()
    {
        ScrollAdapter.Refresh(LinkData);
        UpdateUI_Look();
        UpdateUI_Stat();
    }

    private void UpdateUI_Look()
    {
        if (curSelectedLinkID == 0)
            return;

        #region Artifact Material Slot Setting

        bool isObtained = curSelectedLinkData.isObtained;

        //SetEffectOn(isObtained ? curSelectedLinkData.grade : 0);
        SetEffectOn(curSelectedLinkData.grade);

        uiGroup_Look.artifact_left.Set(curSelectedLinkData.material01, uiGroup_Look.inactiveColor);
        uiGroup_Look.artifact_right.Set(curSelectedLinkData.material02, uiGroup_Look.inactiveColor);

        uiGroup_Look.activeOnObtained.ForEach(t => t.gameObject.SetActive(isObtained));

        #endregion
    }

    private void UpdateUI_Stat()
    {
        if (curSelectedLinkID == 0)
            return;

        DBArtifact.GetLinkData(curSelectedLinkID, out var linkData);

        uiGroup_Stat.txtName.text = DBLocale.GetText(curSelectedLinkData.titleKey);
        uiGroup_Stat.txtName.color = UIFrameArtifact.GetColorByGrade((byte)curSelectedLinkData.grade);

        /// ____________________ Stat Ability 스크롤 데이터 세팅_________________________
        /// 
        var abilityData = linkAbilityActionCached[linkData.LinkGroup];

        /// 현재 선택된 Link Group 의 AbilityAction 데이터 리스트를 순회함 
        foreach (var byGrade in abilityData.abilities)
        {
            /// 만약 출력해줄 UI 가 생성되어 있지 않다면 생성함. (등급별) 
            if (uiGroup_Stat.abilityUIgroups.ContainsKey(byGrade.grade) == false)
            {
                var obj = Instantiate(abilityActionGradeGroupSourceObj, uiGroup_Stat.abilityGroupParent);
                uiGroup_Stat.abilityUIgroups.Add(byGrade.grade, obj);
            }
        }

        int activeInactiveSeparatorIndex = 0;
        bool nothingIsObtained = true;
        bool allObtained = true;

        /// 캐싱돼있는 UI 를 순회하면서 데이터가 존재하면 세팅해주고 , 데이터가 없는 부분은 
        /// 오브젝트를 꺼줌 
        foreach (var uiByGrade in uiGroup_Stat.abilityUIgroups)
        {
            var txtData = abilityData.abilities.Find(t => t.grade == uiByGrade.Key);

            if (txtData != null)
            {
                uiByGrade.Value.Set(
                    ArtifactFrame.AbilityActionBuildHelper
                    , abilityActionTextSourceObj
                    , DBLocale.GetText(DBUIResouce.GetTierText((byte)txtData.grade))
                    , UIFrameArtifact.GetColorByGrade((byte)txtData.grade)
                    , txtData.isObtained
                    , txtData.isMyGrade
                    , txtData.data);

                uiByGrade.Value.gameObject.SetActive(true);
            }
            else
            {
                uiByGrade.Value.gameObject.SetActive(false);
            }

            if (txtData.isObtained)
            {
                nothingIsObtained = false;
                activeInactiveSeparatorIndex++;
            }
            else
            {
                allObtained = false;
            }
        }

        /// 하나도 활성화가 안되거나 전부 활성화 상태면 분리 오브젝트 보이면안됨 
        if (nothingIsObtained || allObtained)
        {
            uiGroup_Stat.abilitySeparatorObj.gameObject.SetActive(false);
            uiGroup_Stat.abilitySeparatorObj.SetAsLastSibling();
        }
        else
        {
            uiGroup_Stat.abilitySeparatorObj.transform.SetSiblingIndex(activeInactiveSeparatorIndex);
            uiGroup_Stat.abilitySeparatorObj.gameObject.SetActive(true);
        }

        /// ********* Snap 처리전에 레이아웃 업데이트 **********
        foreach (var t in uiGroup_Stat.abilityUIgroups)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(t.Value.RectTransform);
        }

        /// 현재 적용되고 있는 Grade 로 스크롤 Snap 처리 
        float visibleHeight = uiGroup_Stat.scrollRectRectTransform.rect.height;
        float contentHeight = uiGroup_Stat.scrollRect.content.rect.height;
        bool snapDone = false;

        /// LayoutGroup 업데이트가 끝나있어야 정확한 계산이됨. 
        /// 현재 Content Height 가 Visible Height 보다 더 클때만 스크롤이 의미있음 . 
        if (visibleHeight < contentHeight)
        {
            foreach (var group in uiGroup_Stat.abilityUIgroups)
            {
                if (group.Value.IsMyGrade)
                {
                    float targetCellHeight = group.Value.RectTransform.rect.height;
                    //float curPos = 1 - Mathf.Abs(group.Value.RectTransform.localPosition.y) / contentHeight;

                    uiGroup_Stat.scrollRect.content.anchoredPosition =
                        (Vector2)uiGroup_Stat.scrollRectRectTransform.transform.InverseTransformPoint(uiGroup_Stat.scrollRect.content.position)
                        - (Vector2)uiGroup_Stat.scrollRectRectTransform.transform.InverseTransformPoint(group.Value.RectTransform.position)
                        - new Vector2(0, targetCellHeight * 0.5f);
                    snapDone = true;
                    break;
                }
            }
        }

        if (snapDone == false)
        {
            uiGroup_Stat.scrollRect.verticalNormalizedPosition = 1;
        }
    }

    private void SetEffectOn(uint grade)
    {
        uiGroup_Look.effects.ForEach(t => t.obj.SetActive(false));

        var target = uiGroup_Look.effects.Find(t => t.grade == grade);
        if (target != null)
        {
            target.obj.SetActive(true);
        }
    }
    #endregion

    #region Inspector Events 
    #region OnClick
    public void OnClickArtifactInMiddle(bool isLeftOrRight)
    {
        if (isLeftOrRight)
        {
            // if (uiGroup_Look.artifact_left.isArtifactObtained)
            {
                // 이 부분 id 이상하게 넘어감 체크해야함
                ArtifactFrame.OpenArtifactInfoPopup(uiGroup_Look.artifact_left.artifactID);
            }
        }
        else
        {
            //   if (uiGroup_Look.artifact_right.isArtifactObtained)
            {
                ArtifactFrame.OpenArtifactInfoPopup(uiGroup_Look.artifact_right.artifactID);
            }
        }
    }
    #endregion
    #endregion
}
