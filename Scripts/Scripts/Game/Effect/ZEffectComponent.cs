using System;
using System.Collections;
using UnityEngine;

/// <summary> 이펙트 생명주기 처리 </summary>
public class ZEffectComponent : MonoBehaviour
{
    public enum eDespawnAfter
    {
        Time = 0,
        EffectPlayed = 1,
    }

    /// <summary> 어떤 기능에 필요한지 타입 선택 </summary>
    public eDespawnAfter despawnAfter = eDespawnAfter.Time;

    /// <summary> 스폰시, 바로 시작하는지 여부 </summary>
    public bool autoStart = false;

    /// <summary> 리얼타임 사용 여부 </summary>
    public bool useRealTime = false;

    /// <summary> Spawn후 유지 시간 </summary>
    public float duration = 0f;

    /// <summary> 폰 죽을때 즉시 제거 </summary>
    public bool RemoveImmediatelyPawnDie = false;

    /// <summary> despawn될떄 발생할 이벤트 </summary>
    public Action<ZEffectComponent> onDespawn;

    private ParticleSystem mParticleSystem;
    private ParticleSystem[] mAllParticles;
    /// <summary> </summary>
    public ParticleSystem ParticleSystem
    {
        get
        {
            if (mParticleSystem == null)
            {
                mAllParticles = GetComponentsInChildren<ParticleSystem>(false);
                mParticleSystem = mAllParticles.Length > 0 ? mAllParticles[0] : null;
            }
            return mParticleSystem;
        }
    }

    /// <summary> 파티클을 위한 speedRate </summary>
    public float ParticleSpeedRate
    {
        get { return ParticleSystem.main.simulationSpeed; }
        set
        {
            if (null == ParticleSystem)
                return;

            foreach (var ps in mAllParticles)
            {
                var mainModule = ps.main;
                mainModule.simulationSpeed = value;
            }
        }
    }

    public bool useParticleSystemDuration = true;
    public bool useParticleSystemStartDelay = false;
    public bool useParticleSystemStartLifetime = false;

    public float ParticleSystemDuration
    {
        get
        {
            if (ParticleSystem == null) { return 0; }
            var main = ParticleSystem.main;
            return (useParticleSystemDuration ? main.duration : 0)
                 + (useParticleSystemStartDelay ? main.startDelay.constant : 0)
                 + (useParticleSystemStartLifetime ? main.startLifetime.constant : 0);
        }
    }

    /// <summary> the despawner's Coroutine. </summary>
    private Coroutine timerRoutine;

    /// <summary> GC 발생 억제용 </summary>
    private WaitForSeconds despawnWaitForSeconds = null;
    private float despawnWFSCurrentDuration = 0f;
    /// <summary> GC 발생 억제용 </summary>
    private WaitForSecondsRealtime despawnWaitForSecondsRealTime = null;
    private float despawnWFSRealTimeCurrentDuration = 0f;
    
    private void Awake()
    {
        switch (despawnAfter)
        {
            case eDespawnAfter.Time: break;
            case eDespawnAfter.EffectPlayed: StopEffectOnAwake(); break;
        }
    }

    /// <summary>  </summary>
    public void PlayEffect()
    {
        if (ParticleSystem == null)
        {
            Debug.LogWarning("[ZPool] No ParticleSystem was found on the '" + gameObject.name + "' gameObject or its children. Despawning...");
            StartTimer();
            return;
        }

        ParticleSystem.Clear(true);
        ParticleSystem.Play();
        StartTimer(ParticleSystemDuration);
    }

    private void StopEffectOnAwake()
    {
        if (ParticleSystem == null)
        {
            Debug.LogWarning("[ZPool] No ParticleSystem was found on the '" + gameObject.name + "' gameObject or its children.");
            return;
        }

        var main = ParticleSystem.main;
        main.playOnAwake = true;

        if (ParticleSystem.isPlaying)
        {
            //ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.Clear(true);
        }
    }

    /// <summary> 주어진 유지 시간후 Despawn </summary>
    /// <param name="_duration"></param>
    public void StartTimer(float _duration = 0)
    {   
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        //time 0 보다 작을 경우 무제한 처리하자
        if (_duration <= 0)
        {            
            //ZManagerLoaderPrefab.Instance.DoReturn(mAssetName, gameObject);
            return;
        }

        timerRoutine = StartCoroutine(StartTimerRoutine(_duration));
    }

    /// <summary> 유지 시간 대기후 Despawn </summary>
    private IEnumerator StartTimerRoutine(float _duration)
    {
        if (useRealTime)
        {
            if (despawnWaitForSecondsRealTime == null || despawnWFSRealTimeCurrentDuration != _duration)
            {
                despawnWaitForSecondsRealTime = new WaitForSecondsRealtime(_duration);
                despawnWFSRealTimeCurrentDuration = _duration;
            }

            yield return despawnWaitForSecondsRealTime;
        }
        else
        {
            if (despawnWaitForSeconds == null || despawnWFSCurrentDuration != _duration)
            {
                despawnWaitForSeconds = new WaitForSeconds(_duration);
                despawnWFSCurrentDuration = _duration;
            }
            yield return despawnWaitForSeconds;
        }

        timerRoutine = null;

        Despawn();
    }

    /// <summary> 이펙트 제거 </summary>
    public void Despawn(bool bDespawnAction = true)
    {
        if (null == ZPoolManager.Instance)
            return;

        var followTarget = GetComponent<ZEffectFollowTarget>();

        if(null != followTarget)
        {
            GameObject.DestroyImmediate(followTarget);
        }

        ZPoolManager.Instance.Return(gameObject);
        if(bDespawnAction)
        {
            onDespawn?.Invoke(this);
        }   
    }
}
