using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devcat;
using System;
using GameDB;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Ocsp;
using static MarkDirectionHandler;
using ParadoxNotion.Serialization;

public class MarkDirectionHandler : MonoBehaviour
{
    /// <summary>
    /// 강화 연출관련 종류
    /// </summary>
    public enum MarkDirectionType
    {
        None = 0,
        MarkEnhance, /// 문장 강화 연출 
        MarkEnhance_Success, /// 문장 강화 성공 연출 
        MarkEnhance_Fail, /// 문장 강화 실패 연출 
        GroupObtained, /// 하나의 그룹이 강화 성공됐을때의 연출 
        DragonImpact_First, /// 특정 레벨 도달 드래곤 임팩트 연출 01 
        DragonImpact_Second, /// 특정 레벨 도달 드래곤 임팩트 연출 02 
        DragonImpact_Third, /// 특정 레벨 도달 드래곤 임팩트 연출 03

        // GroupObtainedAdvancedOnWideView_Partial, /// 하나의 그룹이 강화성공한 후 WideView 로 이동했을때 나오는 연출 
    }

    //[Serializable]
    //public class DirectionDuration
    //{
    //    public MarkDirectionType type;
    //    public float duration;
    //}

    //public List<DirectionDuration> DurationInfo;

    private EnumDictionary<MarkDirectionType, MarkDirectionControllerBase> directions;

    [Header("-- 문장 강화시 출력되는 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing;

    [Header("-- 문장 강화후 성공 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing_sucess;

    [Header("-- 문장 강화휴 실패 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing_fail;

    //[Header("-- 문장 강화후 실패후 보호제로 인해 파괴되지 않았을때 이펙트 --")]
    //public DirectionResourceProp directionResource_enhancing_protected;

    [Header("-- 문장 그룹 강화 완성시 출력 이펙트 --")]
    public DirectionResourceProp directionResource_onGroupObtained;

    [Header("-- 문장 특정 레벨 도달후 WideView 드래곤 임팩트 연출 01 --")]
    public DirectionResourceProp directionResource_dragonImpactFirst;

    [Header("-- 문장 특정 레벨 도달후 WideView 드래곤 임팩트 연출 02 --")]
    public DirectionResourceProp directionResource_dragonImpactSecond;

    [Header("-- 문장 특정 레벨 도달후 WideView 드래곤 임팩트 연출 03--")]
    public DirectionResourceProp directionResource_dragonImpactThird;

    #region Properties
    public bool IsAnyPlaying
    {
        get
        {
            if (directions == null)
                return false;

            foreach (var t in directions.Values)
            {
                if (t.IsPlaying)
                {
                    return true;
                }
            }

            return false;
        }
    }

    #endregion

    #region Public Methods
    public void Initialize()
    {
        directions = new EnumDictionary<MarkDirectionType, MarkDirectionControllerBase>();

        directions.Add(MarkDirectionType.None, new MarkDirectionDummy());
        directions.Add(MarkDirectionType.MarkEnhance, new MarkDirection_Enhance());
        directions.Add(MarkDirectionType.MarkEnhance_Success, new MarkDirection_Success());
        directions.Add(MarkDirectionType.MarkEnhance_Fail, new MarkDirection_Fail());
        directions.Add(MarkDirectionType.GroupObtained, new MarkDirection_GroupObtained());
        directions.Add(MarkDirectionType.DragonImpact_First, new MarkDirection_DragonImpactFirst());
        directions.Add(MarkDirectionType.DragonImpact_Second, new MarkDirection_DragonImpactSecond());
        directions.Add(MarkDirectionType.DragonImpact_Third, new MarkDirection_DragonImpactThird());

        directions[MarkDirectionType.None].Initialize(null);
        directions[MarkDirectionType.MarkEnhance].Initialize(directionResource_enhancing);
        directions[MarkDirectionType.MarkEnhance_Success].Initialize(directionResource_enhancing_sucess);
        directions[MarkDirectionType.MarkEnhance_Fail].Initialize(directionResource_enhancing_fail);
        directions[MarkDirectionType.GroupObtained].Initialize(directionResource_onGroupObtained);
        directions[MarkDirectionType.DragonImpact_First].Initialize(directionResource_dragonImpactFirst);
        directions[MarkDirectionType.DragonImpact_Second].Initialize(directionResource_dragonImpactSecond);
        directions[MarkDirectionType.DragonImpact_Third].Initialize(directionResource_dragonImpactThird);
    }

    public bool IsPlaying(MarkDirectionType type)
    {
        if (directions.ContainsKey(type) == false)
        {
            return false;
        }

        return directions[type].IsPlaying;
    }

    public float GetCurrentNormalizedTime(MarkDirectionType type)
    {
        if (directions.ContainsKey(type) == false)
        {
            return 0f;
        }

        return directions[type].NormalizedTime;
    }

    public void Release()
    {
        if (directions != null)
        {
            foreach (var t in directions.Values)
            {
                t.Reset();
            }
        }
    }

    public float GetLength(MarkDirectionType type)
    {
        if (directions.ContainsKey(type) == false)
        {
            return 0f;
        }

        return directions[type].Length;
    }

    public bool Play(DirectionParam param, out float finalLength)
    {
        finalLength = 0f;

        if (param.DirectionType == MarkDirectionType.None)
        {
            ZLog.LogError(ZLogChannel.UI, "None Direction Type not exist");
            return false;
        }

        return PlayDirection(param, out finalLength);
    }

    public bool Stop(MarkDirectionType type)
    {
        if (directions.ContainsKey(type) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Target type not exist");
            return false;
        }

        directions[type].Stop();

        return true;
    }

    #endregion

    #region Private Methods
    private void Update()
    {
        if (directions == null)
            return;

        foreach (var t in directions.Values)
        {
            if (t.IsPlaying)
            {
                t.Update();
            }
        }
    }

    private void OnDisable()
    {
        if (directions == null)
            return;

        foreach (var t in directions.Values)
        {
            if (t.IsPlaying && t.AutoStopOnResourceDeactived)
            {
                //   ZLog.LogError(ZLogChannel.UI, "direction exit on OnDisable");
                t.Stop();
            }
        }
    }

    private bool PlayDirection(
        DirectionParam param
        , out float finalLength)
    {
        finalLength = 0f;

        if (directions.ContainsKey(param.DirectionType) == false)
        {
            ZLog.LogError(ZLogChannel.UI, "Target type not exist");
            return false;
        }

        var direction = directions[param.DirectionType];

        direction.Play(param);
        finalLength = direction.Length;

        return true;
    }

    //private bool PlayEnhancing(
    //DirectionParam param
    //, out float finalLength)
    //{
    //    return PlayDirection(
    //     MarkDirectionType.MarkEnhance
    //     , param.MarkType
    //     , out finalLength
    //     , param.OnUpdated
    //     , param.OnFinished
    //     , param.DesiredSpeed);
    //}

    //private bool PlayGroupObtained(
    //    DirectionParam param
    //    , out float finalLength)
    //{
    //    return PlayDirection(
    //        MarkDirectionType.GroupObtained
    //        , param.MarkType
    //        , out finalLength
    //        , param.OnUpdated
    //        , param.OnFinished
    //        , param.DesiredSpeed);
    //}
    #endregion

    #region Define
    [Serializable]
    public class DirectionResourceByType
    {
        /// <summary>
        /// 타입이 필요한 경우는 사용 , 아니면 사용하는측에서 하나만 쓴다하면 None 으로 판정  
        /// </summary>
        public GameDB.E_MarkAbleType type;
        public List<GameObject> objs;
    }

    [Serializable]
    public class DirectionResourceProp
    {
        public float duration;
        public List<DirectionResourceByType> resByType;
    }
    public struct DirectionParam_EventWithNormalizedTime
    {
        public Action callback;
        public float triggerNormalizedTime;

        public DirectionParam_EventWithNormalizedTime(Action callback, float triggerNormalizedTime)
        {
            this.callback = callback;
            this.triggerNormalizedTime = triggerNormalizedTime;
        }
    }

    public class DirectionParam
    {
        public class EventWithNormalizedTime
        {
            public Action callback;
            public float triggerNormalizedTime;
            public bool triggered;

            public EventWithNormalizedTime(DirectionParam_EventWithNormalizedTime param)
            {
                callback = param.callback;
                triggerNormalizedTime = param.triggerNormalizedTime;
                triggered = false;
            }
        }

        public MarkDirectionType DirectionType { get; private set; }
        public GameDB.E_MarkAbleType MarkType { get; private set; }
        public Action<float> OnUpdated { get; private set; }
        public Action OnFinished { get; private set; }
        public List<EventWithNormalizedTime> EventCallbackByNormalizedTime { get; private set; }
        public float DesiredSpeed { get; private set; }
        public Vector2 WorldPos { get; private set; }

        public DirectionParam(MarkDirectionType directionType)
        {
            DirectionType = directionType;
            DesiredSpeed = 1f;
        }

        public DirectionParam SetMarkType(GameDB.E_MarkAbleType type)
        {
            MarkType = type;
            return this;
        }
        public DirectionParam SetBaseCallback(Action<float> updatedCallback, Action finishedCallback)
        {
            OnUpdated = updatedCallback;
            OnFinished = finishedCallback;
            return this;
        }
        public DirectionParam SetEventCallback(DirectionParam_EventWithNormalizedTime param)
        {
            if (EventCallbackByNormalizedTime == null)
                EventCallbackByNormalizedTime = new List<EventWithNormalizedTime>();

            if (param.triggerNormalizedTime < 0f)
            {
                ZLog.LogError(ZLogChannel.UI, "not designed to be used with normalizedTime 0");
                return this;
            }

            EventCallbackByNormalizedTime.Add(new EventWithNormalizedTime(param));

            return this;
        }
        public DirectionParam SetDesiredSpeed(float speed)
        {
            DesiredSpeed = speed;
            return this;
        }
        public DirectionParam SetWorldPos(Vector2 worldPos)
        {
            WorldPos = worldPos;
            return this;
        }
    }
    #endregion
}

/// <summary>
/// 연출 베이스 클래스 
/// </summary>
public abstract class MarkDirectionControllerBase
{
    protected DirectionResourceProp DirectionResource;

    public float ElapsedTime { get; protected set; }
    public Action<float> OnUpdated { get; protected set; }
    public Action OnFinished { get; protected set; }
    public List<DirectionParam.EventWithNormalizedTime> EventCallbacks { get; protected set; }
    public float SpeedMultiplier { get; protected set; }
    public float NormalizedTime { get => ElapsedTime / Length; }
    public bool IsPlaying { get; protected set; }

    public float Length { get; protected set; }

    abstract public bool SupportSpeedControl { get; }
    /// <summary>
    /// 외부에서 이펙트를 포함한 게임오브젝트를 직접 끄거나 상위에서 꺼지는경우 
    /// 중간에 그냥 종료시킬지에 대한 옵션.
    /// </summary>
    abstract public bool AutoStopOnResourceDeactived { get; }

    public abstract void ProcessUpdate();
    public virtual float OriginalLength
    {
        get
        {
            return DirectionResource != null ? DirectionResource.duration : 0f;
        }
    }

    public virtual void Initialize(DirectionResourceProp resource)
    {
        DirectionResource = resource;
    }

    protected virtual void ConfigureResource(DirectionParam param) { }
    protected virtual void SetResourceWorldPos(Vector2 pos)
    {
        if (DirectionResource != null)
        {
            foreach (var byType in DirectionResource.resByType)
            {
                byType.objs.ForEach(t => t.transform.position = new Vector3(pos.x, pos.y, t.transform.position.z));
            }
        }
    }

    protected virtual void SetResourceLocalPos(Vector2 pos)
    {
        if (DirectionResource != null)
        {
            foreach (var byType in DirectionResource.resByType)
            {
                byType.objs.ForEach(t => t.transform.localPosition = new Vector3(pos.x, pos.y, t.transform.localPosition.z));
            }
        }
    }

    public virtual void Play(DirectionParam param)
    {
        this.SetBaseInfo(param.DesiredSpeed, param.OnUpdated, param.OnFinished, param.EventCallbackByNormalizedTime);
        IsPlaying = true;

        if (DirectionResource != null)
        {
            var resourceByType = DirectionResource.resByType.Find(t => t.type == param.MarkType);
            var defaultResource = DirectionResource.resByType.Find(t => t.type == GameDB.E_MarkAbleType.None);

            if (defaultResource != null)
            {
                foreach (var res in defaultResource.objs)
                {
                    if (res != null)
                    {
                        res.SetActive(true);
                    }
                    else
                    {
                        ZLog.LogError(ZLogChannel.UI, "Empty GameObject Found");
                    }
                }
            }

            if (resourceByType != null)
            {
                foreach (var res in resourceByType.objs)
                {
                    if (res != null)
                    {
                        res.SetActive(true);
                    }
                    else
                    {
                        ZLog.LogError(ZLogChannel.UI, "Empty GameObject Found");
                    }
                }
            }
        }
    }

    protected void SetBaseInfo(float desiredSpeed, Action<float> onUpdated, Action onFinished, List<DirectionParam.EventWithNormalizedTime> eventCallbacks)
    {
        if (eventCallbacks != null)
            eventCallbacks.ForEach(t => t.triggered = false);

        OnUpdated = onUpdated;
        OnFinished = onFinished;
        EventCallbacks = eventCallbacks;

        if (SupportSpeedControl)
        {
            SpeedMultiplier = desiredSpeed;
        }
        else
        {
            SpeedMultiplier = 1f;
        }

        Length = OriginalLength / SpeedMultiplier;
    }

    public virtual void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        ElapsedTime += deltaTime;
        float normalizedTime = ElapsedTime / Length;
        ProcessUpdate();
        OnUpdated?.Invoke(normalizedTime);

        if (EventCallbacks != null)
        {
            foreach (var callback in EventCallbacks)
            {
                if (callback.triggered == false)
                {
                    if (normalizedTime >= callback.triggerNormalizedTime)
                    {
                        callback.callback?.Invoke();
                        callback.triggered = true;
                    }
                }
            }
        }

        if (ElapsedTime >= Length)
        {
            Stop();
        }
        else
        {
            /// Resource 의 active 로 자동 종료 체킹 
            if (AutoStopOnResourceDeactived)
            {
                bool isAllDisabled = true;

                foreach (var byType in this.DirectionResource.resByType)
                {
                    /// 켜져있는게 있다면 autoDisable 적용 X 
                    if (byType.objs.Exists(t => t.activeInHierarchy))
                    {
                        isAllDisabled = false;
                        break;
                    }
                }

                if (isAllDisabled)
                {
                    ///  ZLog.LogError(ZLogChannel.UI, "AutoExit on Update");
                    Stop();
                }
            }
        }
    }

    public virtual void Stop()
    {
        bool isPlaying = IsPlaying;
        var finished = OnFinished;
        Reset();

        if (isPlaying)
        {
            finished?.Invoke();
        }
    }

    public virtual void Reset()
    {
        OnUpdated = null;
        OnFinished = null;
        SpeedMultiplier = 1f;
        IsPlaying = false;
        ElapsedTime = 0f;

        if (DirectionResource != null)
        {
            foreach (var resByType in DirectionResource.resByType)
            {
                if (resByType != null)
                {
                    foreach (var res in resByType.objs)
                    {
                        res.SetActive(false);
                    }
                }
            }
        }
    }
}

public class MarkDirectionDummy : MarkDirectionControllerBase
{
    public override bool SupportSpeedControl => false;
    public override bool AutoStopOnResourceDeactived => false;
    public override void Play(DirectionParam param)
    {
        base.Play(param);
    }

    public override void Update()
    {
        ZLog.LogWarn(ZLogChannel.UI, "why use this?");
    }

    public override void Stop()
    {
        ZLog.LogWarn(ZLogChannel.UI, "why use this?");
    }

    public override void ProcessUpdate()
    {

    }
}