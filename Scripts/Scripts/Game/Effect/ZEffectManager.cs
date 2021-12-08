using GameDB;
using System;
using UnityEngine;
using Zero;

public class ZEffectManager : Singleton<ZEffectManager>
{
    public void SpawnEffect(uint effectId, Transform parent, Quaternion rotation, float duration = 0f, float speedRate = 1f, Action<ZEffectComponent> onFinish = null)
    {
        if (0 >= effectId)
        {
            onFinish?.Invoke(null);
            return;
        }

        if (false == DBResource.TryGetEffect(effectId, out Effect_Table table))
        {
            onFinish?.Invoke(null);
            return;
        }

        SpawnEffect(table, parent, parent.position, rotation, duration, speedRate, onFinish);
    }

    public void SpawnEffect(Effect_Table table, Transform parent, Quaternion rotation, float duration = 0f, float speedRate = 1f, Action<ZEffectComponent> onFinish = null)
    {
        SpawnEffect(table, parent, parent.position, rotation, duration, speedRate, onFinish);
    }

    public void SpawnEffect(uint effectId, Transform parent, float duration = 0f, float speedRate = 1f, Action<ZEffectComponent> onFinish = null)
    {
        if (0 >= effectId)
        {
            onFinish?.Invoke(null);
            return;
        }

        if (false == DBResource.TryGetEffect(effectId, out Effect_Table table))
        {
            onFinish?.Invoke(null);
            return;
        }

        SpawnEffect(table, parent, parent.position, parent.rotation, duration, speedRate ,onFinish);
    }

    public void SpawnEffect(Effect_Table table, Transform parent, float duration = 0f, float speedRate = 1f, Action<ZEffectComponent> onFinish = null)
    {
        SpawnEffect(table, parent, parent.position, parent.rotation, duration, speedRate, onFinish);
    }

    public void SpawnEffect(uint effectId, Vector3 position, Quaternion rotation, float duration = 0f, float speedRate = 1f,Action<ZEffectComponent> onFinish = null)
    {
        if (0 >= effectId)
        {
            onFinish?.Invoke(null);
            return;
        }   

        if (false == DBResource.TryGetEffect(effectId, out Effect_Table table))
        {
            onFinish?.Invoke(null);
            return;
        }

        SpawnEffect(table, null, position, rotation, duration, speedRate, onFinish);
    }

    public void SpawnEffect(Effect_Table table, Vector3 position, Quaternion rotation, float duration = 0f, float speedRate = 1f, Action<ZEffectComponent> onFinish = null)
    {
        SpawnEffect(table, null, position, rotation, duration, speedRate, onFinish);
    }

    public void SpawnEffect(Effect_Table table, Transform parent, Vector3 position, Quaternion rotation, float duration, float speedRate,Action<ZEffectComponent> onFinish )
    {
        if (null == table)
        {
            onFinish?.Invoke(null);
            return;
        }

        Vector3 offset = null != parent ? position - parent.position : Vector3.zero;

        ZPoolManager.Instance.Spawn(E_PoolType.Effect, table.EffectFile, (go) =>
        {
            if( go == null ) {
                ZLog.LogError( ZLogChannel.Default, $"SpawnEffect가 null 이다 확인바람!!! EffectFile:{table.EffectFile}" );
                return;
            }

            ZEffectComponent comp = go.GetOrAddComponent<ZEffectComponent>();            

            // 이펙트를 지정된 시간 동안 계속 플레이해야한다면, edespawnAfter는 Time으로 설정,
            if (table.EffectType == GameDB.E_EffectType.Loop)
            {
                comp.despawnAfter = ZEffectComponent.eDespawnAfter.Time;
            }
            else
            {
                comp.despawnAfter = null != comp.ParticleSystem ? ZEffectComponent.eDespawnAfter.EffectPlayed : ZEffectComponent.eDespawnAfter.Time;
            }

            comp.ParticleSpeedRate = speedRate;

            if (comp.despawnAfter == ZEffectComponent.eDespawnAfter.Time)
            {
                //time 0 보다 작을 경우 무제한 처리하자
                comp.StartTimer(duration == 0 ? (comp.duration > 0 ? comp.duration : 1f) : duration);
            }
            else if (comp.despawnAfter == ZEffectComponent.eDespawnAfter.EffectPlayed)
            {
                comp.PlayEffect();
            }

            if( table.EffectDelayTime > 0) {
                CoroutineManager.Instance.StartTimer(table.EffectDelayTime, () => {
                    AudioManager.Instance.PlaySFX(table.EffectSoundID, position);
                });
            }
            else {
                AudioManager.Instance.PlaySFX(table.EffectSoundID, position);
            }

            //이펙트 크기 조절
            float scale = table.EffectSize > 0 ? table.EffectSize * 0.01f : 1f;            

            go.transform.position = position;
            go.transform.rotation = rotation;

            var followTarget = go.GetOrAddComponent<ZEffectFollowTarget>();

            if (table.PlayType == E_PlayType.Move)
            {
                go.transform.parent = null;
                if(null != parent)
                {                    
                    followTarget.SetTarget(parent, offset);
                    go.transform.localScale = parent.localScale * scale;
                }
                else
                {
                    go.transform.localScale = Vector3.one * scale;
                }
            }
            else
            {
                if (null != followTarget)
                {
                    GameObject.DestroyImmediate(followTarget);
                }

                if (table.PlayType == E_PlayType.MoveRot)
                {
                    go.transform.parent = parent;
                    go.transform.localPosition = offset;
                }

                go.transform.localScale = Vector3.one * scale;
            }

            //parent가 null이면 dontdestory 되서 예외처리.
            if (null == go.transform.parent)
            {
                SetSceneEffectRoot(go);
            }

            onFinish?.Invoke(comp);
        });
    }

    #region ===== :: 현재씬의 이펙트 루트 :: =====
    private static Transform mSceneEffectRoot;

    public static void SetSceneEffectRoot(GameObject go)
    {
        if (mSceneEffectRoot == null)
            mSceneEffectRoot = (new GameObject("_SceneEffectRoot_")).transform;
        go.transform.parent = mSceneEffectRoot;
    }
    #endregion
}
