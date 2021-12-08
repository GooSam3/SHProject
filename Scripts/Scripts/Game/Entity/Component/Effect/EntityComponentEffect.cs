using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> ZPawn 이펙트 관리용 </summary>
public class EntityComponentEffect : EntityComponentBase<ZPawn>
{
    private List<ZEffectComponent> m_listEffect = new List<ZEffectComponent>();
    
    protected override void OnDestroyImpl()
    {
        Clear(true);
    }

    public void PreClear()
    {
        for (int i = m_listEffect.Count - 1; i >= 0; i--) {
            var effect = m_listEffect[i];

            if (null == effect) {
                continue;
            }

            if (effect.RemoveImmediatelyPawnDie) {
                effect.Despawn(false);
                m_listEffect.Remove(effect);
            }
        }
    }

    public void Clear(bool bReturn)
    {
        if(true == bReturn)
        {
            foreach (var effect in m_listEffect)
            {
                if (null == effect)
                    continue;

                effect.Despawn(false);
            }
        }

        m_listEffect.Clear();
    }

    private void Add(ZEffectComponent comp)
    {
        if (null == comp)
            return;

        if (m_listEffect.Contains(comp))
            return;

        comp.onDespawn += Remove;

        m_listEffect.Add(comp);
    }

    private void Remove(ZEffectComponent comp)
    {
        if (null == comp)
            return;

        comp.onDespawn -= Remove;

        m_listEffect.Remove(comp);
    }

    public void SpawnEffect(Effect_Table table, float duration = 0f, float speed = 1f, Action<ZEffectComponent> onFinish = null)
    {
        if (null == table)
        {
            onFinish?.Invoke(null);
            return;
        }

        Transform parent = Owner.GetSocket(table.ModelSocket);

        switch (table.ModelSocket)
        {
            case E_ModelSocket.Head:
            case E_ModelSocket.Hit:
            case E_ModelSocket.Projectile:
                {
                    //해당 소켓은 zpawn의 회전값으로 처리.
                    ZEffectManager.Instance.SpawnEffect(table, parent, Owner.Rotation, duration, speed, (effectComp) =>
                    {
                        Add(effectComp);
                        onFinish?.Invoke(effectComp);
                    });
                }
                break;
            default:
                {
                    ZEffectManager.Instance.SpawnEffect(table, parent, duration, speed, (effectComp) =>
                    {
                        Add(effectComp);
                        onFinish?.Invoke(effectComp);
                    });
                }
                break;
        }
    }

    public void SpawnEffect(uint effectTid, float duration = 0f, float speed = 1f, Action<ZEffectComponent> onFinish = null)
    {
        if (false == DBResource.TryGetEffect(effectTid, out Effect_Table table))
        {
            onFinish?.Invoke(null);
            return;
        }

        SpawnEffect(table, duration, speed, onFinish);
    }

    /// <summary> 히트 이펙트 연출시 사용 </summary>
    public void SpawnHitEffect(ZPawn attacker, uint effectTid, Action<ZEffectComponent> onFinish = null)
    {
        if (false == DBResource.TryGetEffect(effectTid, out Effect_Table table))
        {
            onFinish?.Invoke(null);
            return;
        }

        Transform parent = Owner.GetSocket(table.ModelSocket);
        Vector3 position = parent.position;
        Quaternion rot = parent.rotation;

        switch (table.EffectOffsetType)
        {
            case E_EffectOffsetType.Attacker:
                {
                    if (null != attacker && null != Owner.Collider && null != attacker.Collider)
                    {
                        //공격자 위치 출력은 루트 기준으로
                        parent = Owner.GetSocket(E_ModelSocket.Hit);
                        position = parent.position;
                        rot = parent.rotation;

                        float owanerSize = Owner.Collider.size.y;
                        float attackSize = attacker.Collider.size.y;

                        //공격자의 크기가 작다면
                        if (attacker.Collider.size.y < Owner.Collider.size.y)
                        {
                            position = attacker.GetSocket(E_ModelSocket.Projectile).position;
                        }

                        rot = Quaternion.LookRotation(VectorHelper.XZForward(attacker.Position, parent.position));
                    }
                }
                break;
        }

        ZEffectManager.Instance.SpawnEffect(table, parent, position, rot, 0f, 1f, (effectComp) =>
        {
            Add(effectComp);
            onFinish?.Invoke(effectComp);
        });
    }

    public void SpawnDamageEffect( ZPawn attacker, Effect_Table table )
    {
        Transform parent = Owner.GetSocket( table.ModelSocket );
        Vector3 position = parent.position;
        Quaternion rot = parent.rotation;

        ZEffectManager.Instance.SpawnEffect( table, parent, position, rot, 0f, 1f, ( effectComp ) => {
            Add( effectComp );
        } );
    }

}
