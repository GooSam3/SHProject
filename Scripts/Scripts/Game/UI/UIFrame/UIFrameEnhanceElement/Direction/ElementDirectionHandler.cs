using System.Collections.Generic;
using UnityEngine;
using Devcat;
using System;
using static ElementDirectionHandler;
using GameDB;

public class ElementDirectionHandler : MonoBehaviour
{
    /// <summary>
    /// 강화 연출관련 종류
    /// </summary>
    public enum EnhanceElementDirection
    {
        None = 0,
        ElementEnhance, /// 속성 강화 연출 
        ElementEnhance_Success, /// 속성 강화 성공 연출 
        ElementEnhance_Fail, /// 속성 강화 실패 연출 
        ElementEnhance_ReachedChain, /// 체인 레벨에 도달했을때 연출 
    }

    private EnumDictionary<EnhanceElementDirection, ElementDirectionControllerBase> directions;

    [Header("-- 속성 강화시 출력되는 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing;

    [Header("-- 속성 강화후 성공 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing_success;

    [Header("-- 속성 강화후 실패 이펙트 --")]
    public DirectionResourceProp directionResource_enhancing_fail;

    [Header("-- 속성 체인 도달후 출력 이펙트 --")]
    public DirectionResourceProp directionResource_reachedChain;

    private Action<EnhanceElementDirection> onPlayDirection;
    private Action onAllDirectionTerminated;

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
        directions = new EnumDictionary<EnhanceElementDirection, ElementDirectionControllerBase>();

        directions.Add(EnhanceElementDirection.None, new ElementDirectionDummy());
        directions.Add(EnhanceElementDirection.ElementEnhance, new EnhanceElementDir_Enhance());
        directions.Add(EnhanceElementDirection.ElementEnhance_Success, new EnhanceElementDir_Success());
        directions.Add(EnhanceElementDirection.ElementEnhance_Fail, new EnhanceElementDir_Fail());
        directions.Add(EnhanceElementDirection.ElementEnhance_ReachedChain, new EnhanceElementDir_ReachedChain());

        directions[EnhanceElementDirection.None].Initialize(null);
        directions[EnhanceElementDirection.ElementEnhance].Initialize(directionResource_enhancing);
        directions[EnhanceElementDirection.ElementEnhance_Success].Initialize(directionResource_enhancing_success);
        directions[EnhanceElementDirection.ElementEnhance_Fail].Initialize(directionResource_enhancing_fail);
        directions[EnhanceElementDirection.ElementEnhance_ReachedChain].Initialize(directionResource_reachedChain);
    }

    public void AddListener_OnPlay(Action<EnhanceElementDirection> callback)
    {
        onPlayDirection += callback;
    }

    public void RemoveListener_OnPlay(Action<EnhanceElementDirection> callback)
    {
        onPlayDirection -= callback;
    }

    public void AddListener_OnAllTerminated(Action callback)
    {
        onAllDirectionTerminated += callback;
    }

    public void RemoveListener_OnAllTerminated(Action callback)
    {
        onAllDirectionTerminated -= callback;
    }

    public bool IsPlaying(EnhanceElementDirection type)
    {
        if (directions.ContainsKey(type) == false)
        {
            return false;
        }

        return directions[type].IsPlaying;
    }

    public float GetCurrentNormalizedTime(EnhanceElementDirection type)
    {
        if (directions.ContainsKey(type) == false)
        {
            return 0f;
        }

        return directions[type].NormalizedTime;
    }

    public void Release()
    {
        this.onAllDirectionTerminated = null;
        this.onPlayDirection = null;

        if (directions != null)
        {
            foreach (var t in directions.Values)
            {
                t.Reset();
            }
        }
    }

    public float GetLength(EnhanceElementDirection type)
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

        if (param.DirectionType == EnhanceElementDirection.None)
        {
            ZLog.LogError(ZLogChannel.UI, "None Direction Type not exist");
            return false;
        }

        return PlayDirection(param, out finalLength);
    }

    public bool Stop(EnhanceElementDirection type)
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
    #endregion

    #region Define
    [Serializable]
    public class DirectionResourceByType
    {
        /// <summary>
        /// 타입이 필요한 경우는 사용 , 아니면 사용하는측에서 하나만 쓴다하면 None 으로 판정  
        /// </summary>
        public GameDB.E_UnitAttributeType type;
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
        public enum PositionSet
        {
            None = 0, /// 포지션 세팅 X 
            All,
            X,
            Y
        }

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

        public EnhanceElementDirection DirectionType { get; private set; }
        public GameDB.E_UnitAttributeType AttributeType { get; private set; }
        public Action<float> OnUpdated { get; private set; }
        public Action OnFinished { get; private set; }
        public List<EventWithNormalizedTime> EventCallbackByNormalizedTime { get; private set; }
        public float DesiredSpeed { get; private set; }

        public PositionSet PositionSelectOption { get; private set; }
        public Vector2 Position { get; private set; }
        public bool IsWorldPosition { get; private set; }

        public DirectionParam(EnhanceElementDirection directionType)
        {
            DirectionType = directionType;
            DesiredSpeed = 1f;
            PositionSelectOption = PositionSet.None;
        }

        public DirectionParam SetAttributeType(E_UnitAttributeType type)
        {
            AttributeType = type;
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
        public DirectionParam SetPositionSelectOption(PositionSet option)
        {
            PositionSelectOption = option;
            return this;
        }
        public DirectionParam SetWorldPos(Vector2 worldPos)
        {
            if (PositionSelectOption == PositionSet.None)
                PositionSelectOption = PositionSet.All;

            IsWorldPosition = true;
            Position = worldPos;
            return this;
        }
        public DirectionParam SetLocalPos(Vector2 localPos)
        {
            if (PositionSelectOption == PositionSet.None)
                PositionSelectOption = PositionSet.All;

            IsWorldPosition = false;
            Position = localPos;
            return this;
        }
    }
    #endregion
}

/// <summary>
/// 연출 베이스 클래스 
/// </summary>
public abstract class ElementDirectionControllerBase
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
    protected virtual void SetPosition(DirectionParam param)
    {
        if (DirectionResource != null)
        {
            /// 포지션 사용안함 옵션
            if (param.PositionSelectOption == DirectionParam.PositionSet.None)
                return;

            foreach (var byType in DirectionResource.resByType)
            {
                for (int i = 0; i < byType.objs.Count; i++)
                {
                    foreach (var obj in byType.objs)
                    {
                        if (obj != null)
                        {
                            Vector2 pos = Vector2.zero;

                            if(param.IsWorldPosition)
                            {
                                pos = obj.transform.position;
                            }
                            else
                            {
                                pos = obj.transform.localPosition;
                            }

                            /// X 포지션 세팅 
                            if (param.PositionSelectOption == DirectionParam.PositionSet.All
                                || param.PositionSelectOption == DirectionParam.PositionSet.X)
                            {
                                pos.x = param.Position.x;
                            }

                            /// Y 포지션 세팅 
                            if (param.PositionSelectOption == DirectionParam.PositionSet.All
                                || param.PositionSelectOption == DirectionParam.PositionSet.Y)
                            {
                                pos.y = param.Position.y;
                            }

                            if (param.IsWorldPosition)
                            {
                                obj.transform.position = new Vector3(pos.x, pos.y, obj.transform.position.z);
                            }
                            else
                            {
                                obj.transform.localPosition = new Vector3(pos.x, pos.y, obj.transform.localPosition.z);
                            }
                        }
                    }
                }
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
        this.SetPosition(param);

        IsPlaying = true;

        if (DirectionResource != null)
        {
            var resourceByType = DirectionResource.resByType.Find(t => t.type == param.AttributeType);
            var defaultResource = DirectionResource.resByType.Find(t => t.type == E_UnitAttributeType.None);

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

public class ElementDirectionDummy : ElementDirectionControllerBase
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