using GameDB;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    private ZPawn mSelf;
    private EntityBase mTarget;
    private Transform mTargetTrans;

    private Skill_Table mSkillTable;
    private Vector3 mDir;

    private E_MissileType Type { get { return mSkillTable.MissileType; } }
    private float Speed { get { return mSkillTable.MissileSpeed; } }
    private float Range { get { return mSkillTable.MissileRange; } }
    private float Distance { get { return mSkillTable.MissileDistance; } }
    private float DelayTime { get { return mSkillTable.MissileDelayTime; } }
    private byte Count { get { return mSkillTable.MissileCnt; } }

    private bool IsFire;

    public void Fire(ZPawn self, EntityBase target, uint skillId, Vector3 dir)
    {
        if(null == target || false == DBSkill.TryGet(skillId, out mSkillTable))
        {
            GameObject.Destroy(gameObject);
            return;
        }
        
        mSelf = self;
        mTarget = target;

        mTargetTrans = mTarget.GetSocket(E_ModelSocket.Hit);
        mDir = dir;

        switch (Type)
        {
            case E_MissileType.Target:
                {                   
                    ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateProjectile_Target);
                }                
                break;
            case E_MissileType.Explosion:
            case E_MissileType.Penetration:
                {
                    //ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateProjectile_Dir);
                    ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateProjectile_Target);
                }
                break;
        }

        IsFire = true;

        //발사체 이펙트
        uint missileEffectId = self.ResourceTable.MissileEffect_Base;

        switch (mSkillTable.MissileEffectType)
        {
            case E_MissileEffectType.MissileEffect_01:
                missileEffectId = self.ResourceTable.MissileEffect_01;
                break;
            case E_MissileEffectType.MissileEffect_02:
                missileEffectId = self.ResourceTable.MissileEffect_02;
                break;
            case E_MissileEffectType.MissileEffect_03:
                missileEffectId = self.ResourceTable.MissileEffect_03;
                break;
            case E_MissileEffectType.MissileEffect_04:
                missileEffectId = self.ResourceTable.MissileEffect_04;
                break;
        }

        ZEffectManager.Instance.SpawnEffect(missileEffectId, transform, -1f, 1f, (effect) => 
        {
            if(null == this && null != effect)
            {
                effect.Despawn();
            }
            else
            {
                if (null == effect)
                    return;

                var followTarget = effect.GetComponent<ZEffectFollowTarget>();

                if(null != followTarget)
                {
                    //발사체는 그냥 부착시키자.                
                    DestroyImmediate(followTarget);
                }

                effect.transform.parent = transform;
                effect.transform.localPosition = Vector3.zero;
                effect.transform.localRotation = Quaternion.identity;                
            }
        });
    }


    private void UpdateProjectile_Target()
    {
        if (false == IsFire)
        {
            return;
        }   

        if (null == mTarget || null == mTargetTrans)
        {
            Explode();
            return;
        }

        Vector3 targetPos = mTargetTrans.position;
        mDir = (targetPos - transform.position);
        mDir.Normalize();
        transform.forward = mDir;

        float moveDistance = Speed * Time.smoothDeltaTime;

        transform.position += (mDir * moveDistance);

        if((targetPos - transform.position).magnitude <= 0.5f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        IsFire = false;
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, UpdateProjectile_Target);
        
        if(null != mSelf)
        {
            if (null != mTarget)
            {
                if (mTarget.EntityType == E_UnitType.Gimmick)
                {
                    mTarget.To<ZGimmick>().TakeAttribute(mSelf.UnitAttributeType, mSelf.AttributeLevel);
                }

                if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Temple)
                {
                    //사당에선 따로 데미지요청 보냄.
                    ZMmoManager.Instance.Field.REQ_DamageReq(mSelf.EntityId, mTarget.EntityId, mSkillTable.SkillID);
                }
            }
        }

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        GameObject.Destroy(gameObject);
    }
}
