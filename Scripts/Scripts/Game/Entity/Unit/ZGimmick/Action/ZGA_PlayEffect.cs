using UnityEngine;

public class ZGA_PlayEffect : ZGimmickActionBase
{
    [Header("플레이할 이펙트 ID")]
    [SerializeField]
    private uint EffectId;

    [Header("플레이할 이펙트 Object (씬에 있어야함.)")]
    [SerializeField]
    private GameObject ObjEffect;

    private ZEffectComponent Effect;

    protected override void InitializeImpl()
    {
        ZTempleHelper.SetActiveFx(ObjEffect, false);
    }

    protected override void InvokeImpl()
    {
        ZTempleHelper.SetActiveFx(ObjEffect, true);
        ZEffectManager.Instance.SpawnEffect(EffectId, Gimmick.transform, -1, 1f, (comp) =>
        {
            if (null != Effect)
                Effect.Despawn(false);

            Effect = comp;
        });
    }

    protected override void DestroyImpl()
    {
        base.DestroyImpl();

        if (null != Effect)
            Effect.Despawn(false);

        Effect = null;
    }

    protected override void CancelImpl()
    {
        if (null != Effect)
            Effect.Despawn(false);

        Effect = null;

        ZTempleHelper.SetActiveFx(ObjEffect, false);
    }
}