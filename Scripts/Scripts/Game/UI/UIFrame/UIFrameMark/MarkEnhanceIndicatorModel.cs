using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using static UIFrameMark;
using static UIFrameMark.MarkElementSharedInfo;
using ZNet.Data;
using frame8.Logic.Misc.Other.Extensions;

public class MarkEnhanceIndicatorModel : MonoBehaviour
{
    [Serializable]
    public class SubMark
    {
        public RectTransform rectTransform;
        /// <summary>
        ///  0 .. 1 .. 2 오름차순 
        /// </summary>
        public int order;
        public Image imgGauge;
        public List<Image> activeOnCompleted;
        public GameObject effectObjsRoot;

        [HideInInspector] public List<EffectListByType> effectObjsOnObtainedByType = new List<EffectListByType>();
        [HideInInspector] public uint tid;
        [HideInInspector] public bool isObtained;
        [HideInInspector] public byte step;

        public void SetInfo(uint _tid, byte _step, bool _obtained)
        {
            tid = _tid;
            isObtained = _obtained;
            step = _step;
        }
        public void ResetInfo()
        {
            tid = 0;
            isObtained = false;
            step = 0;
        }
    }

    public class EffectObjsByType
    {
        public GameDB.E_MarkAbleType type;
        public List<GameObject> objs = new List<GameObject>();
    }

    public class EffectListByType
    {
        public GameDB.E_MarkAbleType type;
        public List<GameObject> effects = new List<GameObject>();
    }

    #region Preference Variable
    [Header("강화 연출 시간")]
    [SerializeField] private float fillAnimationDuration;
    #endregion
    #region SerializedField

    #region UI Variables
    [SerializeField] private List<Image> activeOnObtained;
    [SerializeField] private Image imgCenterGroupStep;

    [SerializeField] private Transform doneEffectRoot;

    [Header("Inactive Color 는 UIFrameMark 의 inactiveColor 로 리세팅중")]
    [SerializeField] private List<ColorChangerByObtained> graphicsForColorChangeByObtained;

    [SerializeField] private List<SubMark> subMarks;

    [SerializeField] private RectTransform protectionEffectRoot;

    [SerializeField] private RectTransform _RectTransform;
    #endregion
    #endregion

    #region System Variables
    private Color representColor;
    private Color inactiveColor;

    private Sprite centerGroupStepSprite;
    private Sprite gaugeSprite;

    private SubMark targetFillAniSubMark;

    private List<EffectObjsByType> doneEffectsByType;

    /// <summary>
    /// list index, markTid, subMark rectTransform
    /// </summary>
    public Action<int, uint, RectTransform> onSubMarkClicked;
    #endregion

    #region Properties 
    public RectTransform RectTransform { get { return _RectTransform; } }
    public GameDB.E_MarkAbleType MarkType { get; private set; }
    public uint StartTID { get; private set; }
    public byte TopStep { get; private set; }
    public int ID { get; private set; }
    public float EnhanceAnimationDuration { get { return fillAnimationDuration; } }
    public bool IsAllObtained
    {
        get
        {
            return Me.CurCharData.IsMarkObtained_ByStep(MarkType, TopStep);
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
    // public bool CanEnhance { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        /// 애가 0 이면 계산과정에서 dividedByZero 가능성있음. 혹시 모르니 예외처리 
        if (fillAnimationDuration == 0f)
        {
            fillAnimationDuration = 0.01f;
        }
    }

    #endregion

    #region Public Methods
    public void Initialize(Action<int, uint, RectTransform> onClicked)
    {
        doneEffectsByType = new List<EffectObjsByType>();

        for (int i = 0; i < subMarks.Count; i++)
        {
            if (i > 0)
            {
                if (subMarks[i].order <= subMarks[i - 1].order)
                {
                    /// Order 값 순서대로 설정해줘야함 . 
                    /// 자동으로 세팅하는거 없음 . 무조건 명확하게 명시해줘야함 . 
                    ZLog.LogError(ZLogChannel.UI, "SubMark Order Value is not wrong, set it in ascending order");
                }
            }

            /// subMark 당 타입별 Effect 캐싱용 리스트 항목들 할당함.  
            subMarks[i].effectObjsOnObtainedByType.Add(new EffectListByType() { type = GameDB.E_MarkAbleType.None, effects = new List<GameObject>() });
            subMarks[i].effectObjsOnObtainedByType.Add(new EffectListByType() { type = GameDB.E_MarkAbleType.RecoveryMark, effects = new List<GameObject>() });
            subMarks[i].effectObjsOnObtainedByType.Add(new EffectListByType() { type = GameDB.E_MarkAbleType.AttackMark, effects = new List<GameObject>() });
            subMarks[i].effectObjsOnObtainedByType.Add(new EffectListByType() { type = GameDB.E_MarkAbleType.DefenseMark, effects = new List<GameObject>() });
        }

        onSubMarkClicked = onClicked;
    }

    public void SetBaseInfo(
        GameDB.E_MarkAbleType markType
        , uint myStep
        , uint startTID
        , int id
        , Sprite centerGroupStepSprite
        , Sprite gaugeSprite
        , Color representColor
        , Color inactiveColor)
    {
        this.MarkType = markType;
        this.StartTID = startTID;
        this.ID = id;
        this.representColor = representColor;
        this.inactiveColor = inactiveColor;
        this.centerGroupStepSprite = centerGroupStepSprite;
        this.gaugeSprite = gaugeSprite;

        graphicsForColorChangeByObtained.ForEach(t => t.onNotObtained = inactiveColor);

        TopStep = 0;

        /// subMark 순회 및 필요 데이터 세팅 
        for (int i = 0; i < subMarks.Count; i++)
        {
            uint tid = startTID + (uint)i;
            var data = DBMark.GetMarkData(tid);

            subMarks[i].SetInfo(tid, data.Step, myStep >= data.Step);

            /// 현재 마크 그룹에 설정된 최대 Step 세팅 
            if (data.Step > TopStep)
            {
                TopStep = data.Step;
            }
        }

        Vector2 lastObtainedSubMarkPos = GetLastObtainedSubMarkWorldPosXY();
        protectionEffectRoot.position = new Vector3(lastObtainedSubMarkPos.x, lastObtainedSubMarkPos.y, protectionEffectRoot.position.z);
    }

    public void SetFirstNotObtainedMarkFillAmount(float normalized)
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].isObtained == false)
            {
                subMarks[i].imgGauge.fillAmount = normalized;
                return;
            }
        }

        UnityEngine.Debug.LogError("why here? all obtained");
    }

    public uint GetFirstNotObtainedSubMarkTid()
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].isObtained == false)
            {
                return subMarks[i].tid;
            }
        }

        return 0;
    }

    public uint GetLastObtainedSubMarkTid()
    {
        uint tid = 0;

        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].isObtained)
            {
                tid = subMarks[i].tid;
            }
        }

        return tid;
    }

    public RectTransform GetSubMarkRectTransform(int index)
    {
        if (index >= subMarks.Count)
        {
            return null;
        }

        return subMarks[index].rectTransform;
    }

    public Vector2 GetFirstNotObtainedSubMarkWorldPosXY()
    {
        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].isObtained == false)
            {
                return new Vector2(subMarks[i].rectTransform.position.x, subMarks[i].rectTransform.position.y);
            }
        }

        return Vector2.zero;
    }

    public Vector2 GetLastObtainedSubMarkWorldPosXY()
    {
        Vector2 pos = Vector2.zero;

        for (int i = 0; i < subMarks.Count; i++)
        {
            if (subMarks[i].isObtained)
            {
                pos = new Vector2(subMarks[i].rectTransform.position.x, subMarks[i].rectTransform.position.y);
            }
        }

        return pos;
    }

    public Vector2 GetSubMarkWorldPosXY(int index)
    {
        if (index >= subMarks.Count)
        {
            return Vector2.zero;
        }

        return new Vector2(subMarks[index].rectTransform.position.x, subMarks[index].rectTransform.position.y);
    }

    public void ReleaseState()
    {
        this.MarkType = GameDB.E_MarkAbleType.None;
        this.StartTID = 0;
        this.ID = 0;
        this.centerGroupStepSprite = null;
        this.TopStep = 0;

        for (int i = 0; i < subMarks.Count; i++)
        {
            subMarks[i].ResetInfo();
        }
    }

    public void UpdateUI_All(
        UIFrameMark frameMark
        , GameDB.E_MarkAbleType type
        , bool useProtection
        , uint myStep
        //  , List<Sprite> gaugeSpritesByStepOrder
        , List<GameObjectList> effectSubMarkSourceObjsOnObtainedObjsByStepOrder
        , List<GameObject> effectDoneSourceObjsOnObtainedObjs)
    {
        bool isObtained = myStep >= TopStep;

        imgCenterGroupStep.sprite = centerGroupStepSprite;
        imgCenterGroupStep.color = isObtained ? Color.white /*representColor*/ : inactiveColor;
        imgCenterGroupStep.gameObject.SetActive(true);

        for (int i = 0; i < subMarks.Count; i++)
        {
            bool isSubMarkObtained = myStep >= subMarks[i].step;

            if (subMarks[i].imgGauge.sprite != gaugeSprite)
                subMarks[i].imgGauge.sprite = gaugeSprite;
            subMarks[i].imgGauge.fillAmount = isSubMarkObtained ? 1f : 0f;
            subMarks[i].activeOnCompleted.ForEach(t => t.gameObject.SetActive(isSubMarkObtained));

            var curTypeEffects = subMarks[i].effectObjsOnObtainedByType.Find(t => t.type == type);

            /// i 가 Step 그리고 effectSubMarkSourceObjsOnObtainedObjsByStepOrder 에는 Step 별로 EffectList 가 들어가있음
            if (curTypeEffects != null
                && effectSubMarkSourceObjsOnObtainedObjsByStepOrder.Count > i)
            {
                var curStepEffectObjList = effectSubMarkSourceObjsOnObtainedObjsByStepOrder[i].gameObjects;

                /// Effect 캐싱안돼있으면 Spawn 및 캐싱 
                if (curStepEffectObjList.Count > curTypeEffects.effects.Count)
                {
                    frameMark.SpawnGameObject(curStepEffectObjList, subMarks[i].effectObjsRoot.transform, (createdObj) => curTypeEffects.effects.Add(createdObj));
                    //SpawnGameObject(curStepEffectObjList, subMarks[i].effectObjsRoot.transform, curTypeEffects.effects);
                }
            }

            for (int j = 0; j < subMarks[i].effectObjsOnObtainedByType.Count; j++)
            {
                var effectsByType = subMarks[i].effectObjsOnObtainedByType[j];
                bool active = effectsByType.type == type && isSubMarkObtained;

                for (int k = 0; k < effectsByType.effects.Count; k++)
                {
                    if (effectsByType.effects[k].gameObject != null)
                    {
                        effectsByType.effects[k].gameObject.SetActive(active);
                    }
                }
            }

            subMarks[i].effectObjsRoot.SetActive(curTypeEffects != null);
        }

        /// 나의 현재 Step 과 동일한 애가 있고 , protection 을 사용하는 경우 Effect On
        UpdateProtectionEffect(useProtection, myStep);

        graphicsForColorChangeByObtained.ForEach(t => t.Set(isObtained));

        var targetDoneEffect = doneEffectsByType.Find(t => t.type == type);
        if (targetDoneEffect == null)
        {
            targetDoneEffect = new EffectObjsByType() { type = type };
            doneEffectsByType.Add(targetDoneEffect);
            frameMark.SpawnGameObject(effectDoneSourceObjsOnObtainedObjs, doneEffectRoot, (createdObj) => targetDoneEffect.objs.Add(createdObj));
        }

        foreach (var byType in doneEffectsByType)
        {
            byType.objs.ForEach(t => t.SetActive(byType.type == type && isObtained));
        }
    }

    public void UpdateProtectionEffect(bool useProtection, uint myStep)
    {
        protectionEffectRoot.gameObject.SetActive(subMarks.Exists(t => t.step == myStep && useProtection));
    }

    ///// <summary>
    ///// 내부에서는 강화 가능 여부 체킹하지않음 .
    ///// 랩퍼 클래스에서 체킹해야함 
    ///// </summary>
    //public bool StartFillAnimation(uint subMarkTid, Action<uint> onFinished)
    //{
    //    if (ani == null)
    //    {
    //        return false;
    //    }

    //    targetFillAniSubMark = subMarks.Find(t => t.tid == subMarkTid);

    //    if (targetFillAniSubMark == null)
    //    {
    //        ZLog.LogError(ZLogChannel.UI, "Could not find targetSubMark To Enhance , tid : " + subMarkTid);
    //        subMarks.ForEach(t => ZLog.LogError(ZLogChannel.UI, "this mark has following markTid : " + t.tid));
    //        return false;
    //    }

    //    ani.duration = fillAnimationDuration;
    //    ani.elapsedTime = 0f;
    //    onEnhanceAnimationFinished = onFinished;

    //    return true;
    //}
    #endregion

    #region Private Methods
    //void SpawnGameObject(List<GameObject> sourceList, Transform root, List<GameObject> addList)
    //{
    //    if (sourceList != null)
    //    {
    //        for (int i = 0; i < sourceList.Count; i++)
    //        {
    //            if (sourceList[i] != null)
    //            {
    //                var t = Instantiate(sourceList[i], root);
    //                ResetTransform(t.transform);
    //                if (addList != null)
    //                    addList.Add(t);
    //            }
    //        }
    //    }
    //}

    //private void SpawnGameObject(List<GameObjectList> effectList, Transform transform, List<GameObject> effects)
    //{

    //}

    void ResetTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    //private void Update_EnhanceAnimation()
    //{
    //    if (ani == null || ani.elapsedTime >= ani.duration)
    //        return;

    //    ani.elapsedTime += Time.unscaledDeltaTime;
    //    float t = ani.elapsedTime / ani.duration;

    //    if (ani.elapsedTime >= ani.duration)
    //    {
    //        t = 1f;
    //    }

    //    targetFillAniSubMark.imgGauge.fillAmount = Mathf.Lerp(0f, 1f, t);

    //    onEnhanceAnimationFinished?.Invoke(targetFillAniSubMark.tid);
    //    onEnhanceAnimationFinished = null;

    //    ani.Reset();
    //    targetFillAniSubMark = null;
    //}
    #endregion

    #region Inspector Events
    public void OnClickSubMark(int index)
    {
        if (subMarks == null
            || index >= subMarks.Count)
        {
            return;
        }

        onSubMarkClicked?.Invoke(index, subMarks[index].tid, subMarks[index].rectTransform);
    }
    #endregion
}
