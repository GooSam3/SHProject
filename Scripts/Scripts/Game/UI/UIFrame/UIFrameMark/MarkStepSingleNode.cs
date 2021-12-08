using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using ZNet.Data;

public class MarkStepSingleNode : MonoBehaviour
{
    [Serializable]
    public class ObtainedEffect
    {
        // public GameObject glowObjectRoot;
        [HideInInspector] public GameObject glowObject;

        public Image imgCircleGlow;
    }

    [Serializable]
    public class SingleSubMark
    {
        public int order;
        public Image imgObtained;
        public Image imgSubMark;

        [HideInInspector] public uint tid;
        [HideInInspector] public bool isObtained;
        [HideInInspector] public byte step;
    }

    #region Preference Variable

    #endregion
    #region SerializedField

    #region UI Variables
    [SerializeField] private GameObject masteredObjRoot;
    [SerializeField] private GameObject notMasteredObjRoot;

    [SerializeField] private Transform undoneNotNextEffectRoot;
    [SerializeField] private Transform nextEnhanceEffectRoot;
    [SerializeField] private Transform undone_obtainedEffectRoot;
    [SerializeField] private Transform doneEffectRoot;

    [SerializeField] private ObtainedEffect obtainedEffect;
    [SerializeField] private Image imgGroupStep;

    [SerializeField] private List<SingleSubMark> subMarks;

    [SerializeField] private RectTransform _RectTransform;
    [SerializeField] private Button btn;
    #endregion
    #endregion

    #region System Variables

    /// <summary>
    /// int : Index , tid 가 아님 
    /// </summary>
    private Action<int> onClicked;

    //private List<GameObject> normalEffectObjList;
    //private List<GameObject> doneEffectObjList;
    #endregion

    #region Properties 
    public RectTransform RectTransform { get { return _RectTransform; } }
    public uint StartTID { get; private set; }
    public uint TopTID { get; private set; }
    public GameDB.E_MarkAbleType MarkType { get; private set; }
    public byte TopStep { get; private set; }
    public int ID { get; private set; }
    public bool IsAllObtained
    {
        get
        {
            return subMarks.TrueForAll(t => t.isObtained);
        }
    }
    public uint[] SubMarkTids
    {
        get
        {
            /// 정상적인 상황이라면 3 이 들어가야함 . 
            int validMarkCount = subMarks.Count(t => t.tid > 0);
            if (validMarkCount == 0)
                return null;
            uint[] tids = new uint[validMarkCount];
            for (int i = 0; i < subMarks.Count; i++)
            {
                tids[i] = subMarks[i].tid;
            }
            return tids;
        }
    }
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    /// <summary>
    /// id 는 현재 마크가 소유되어 있는 배열의 Index 를 의미함
    /// </summary>
    public void Initialize(
        UIFrameMark frameMark
        , uint startTID
        , byte myStep
        , int id
        , Sprite subMarkCommonSprite
        , Sprite centerGroupStepSprite
        , Sprite circleGlowSpriteOnObtained
        , GameObject glowEffectSourceObjectOnObtained
        , List<GameObject> undoneNotNextEffectSourceObjList
        , List<GameObject> nextEnhanceEffectSourceObjList
        , List<GameObject> undoneObtainedEffectSourceObjList
        , List<GameObject> doneEffectSourceObjList
        , Action<int> callback_onClicked)
    {
        this.StartTID = startTID;
        this.TopTID = startTID + (uint)subMarks.Count - 1;
        this.ID = id;

        this.obtainedEffect.imgCircleGlow.sprite = circleGlowSpriteOnObtained;
        frameMark.SpawnGameObject(new List<GameObject>() { glowEffectSourceObjectOnObtained }, doneEffectRoot, (obj) => obtainedEffect.glowObject = obj);
//        this.obtainedEffect.glowObject = Instantiate(glowEffectSourceObjectOnObtained, doneEffectRoot);
//        this.obtainedEffect.glowObject.SetLayersRecursively(desiredEffectLayer);

        this.imgGroupStep.sprite = centerGroupStepSprite;

        onClicked = callback_onClicked;

        var startMarkData = DBMark.GetMarkData(startTID);

        MarkType = startMarkData.MarkAbleType;

        btn.onClick.AddListener(OnClick);

        TopStep = 0;

        /// subMark 순회 및 필요 데이터 세팅 
        for (int i = 0; i < subMarks.Count; i++)
        {
            uint tid = startTID + (uint)i;
            var data = DBMark.GetMarkData(tid);
            subMarks[i].tid = tid;
            subMarks[i].step = data.Step;
            subMarks[i].isObtained = myStep >= data.Step;
            subMarks[i].imgSubMark.sprite = subMarkCommonSprite;

            /// 현재 마크 그룹에 설정된 최대 Step 세팅 
            if (data.Step > TopStep)
            {
                TopStep = data.Step;
            }
        }

        frameMark.SpawnGameObject(undoneNotNextEffectSourceObjList, undoneNotNextEffectRoot);
        frameMark.SpawnGameObject(nextEnhanceEffectSourceObjList, nextEnhanceEffectRoot);
        frameMark.SpawnGameObject(undoneObtainedEffectSourceObjList, undone_obtainedEffectRoot);
        frameMark.SpawnGameObject(doneEffectSourceObjList, doneEffectRoot);
    }

    public void UpdateInfo(uint myStep)
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            subMarks[i].isObtained = myStep >= subMarks[i].step;
        }
    }

    public void UpdateUI(
        bool isMastered
        , uint myStep
        , bool isNextEnhanceGroup
        , List<Sprite> stepSpritesByStepOrder
        , Color elementColor
        , Color inactiveColor)
    {
        bool isObtained = myStep >= TopStep;

        if (isMastered)
        {
            /// 처리 할거없음 
        }
        else
        {
            if (isObtained)
            {
                /// TODO : 뭐 컬러나 이미지 바꿔주는 처리할수도있음 . 
            }

            for (int i = 0; i < subMarks.Count; i++)
            {
                subMarks[i].imgObtained.gameObject.SetActive(myStep >= subMarks[i].step);
            }

            imgGroupStep.color = isObtained ? Color.black /*elementColor*/ : inactiveColor;
            obtainedEffect.imgCircleGlow.gameObject.SetActive(isObtained);
            obtainedEffect.glowObject.SetActive(isObtained);
        }

        masteredObjRoot.SetActive(isMastered);
        notMasteredObjRoot.SetActive(isMastered == false);

        undoneNotNextEffectRoot.gameObject.SetActive(isMastered == false && isObtained == false && isNextEnhanceGroup == false);
        nextEnhanceEffectRoot.gameObject.SetActive(isMastered == false && isObtained == false && isNextEnhanceGroup);
        undone_obtainedEffectRoot.gameObject.SetActive(isMastered == false && isObtained);
        doneEffectRoot.gameObject.SetActive(isMastered);
    }

    public int GetSubMarkIndexByTid(uint tid)
    {
        return subMarks.FindIndex(t => t.tid == tid);
    }

    public uint GetSubMarkTidByIndex(int index)
    {
        if (index >= subMarks.Count)
            return 0;
        return subMarks[index].tid;
    }

    public uint GetFirstNotObtainedSubMarkTid()
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            if (Me.CurCharData.IsMarkObtained_ByStep(MarkType, subMarks[i].step) == false)
            {
                return subMarks[i].tid;
            }
        }

        // 데이터 동기화의 이슈로 제때 제대로된 obtain 체크가 안됨  , 즉 실 데이터로 체킹함
        //for (int i = 0; i < subMarks.Count; i++)
        //{
        //    if (subMarks[i].isObtained == false)
        //    {
        //        return subMarks[i].tid;
        //    }
        //}

        return 0;
    }

    public bool HaveGivenStep(byte targetStep)
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].step == targetStep)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Override

    #endregion

    #region Private Methods

    #region Common

    #endregion
    #endregion

    #region Inspector Events
    public void OnClick()
    {
        onClicked?.Invoke(ID);
    }
    #endregion
}
