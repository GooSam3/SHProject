using GameDB;
using UnityEngine;
using UnityEngine.AI;

/// <summary> Monster </summary>
public class ZObject : ZPawn
{
    public override E_UnitType EntityType { get { return E_UnitType.Object; } }

    /// <summary> 사망 여부 </summary>
    public override bool IsDead { get { return false; } }

    public bool IsGathered = false;

    ZEffectComponent ObjectEffect = null;

    protected override void SetAttributeType()
    {
        UnitAttributeType = E_UnitAttributeType.None;
    }

    #region ===== :: Empty Component :: ===== 
    /// <summary> 이동 컴포넌트 셋팅 </summary>
    protected override EntityComponentMovementBase OnSetMovementComponent()
    {
        return null;
    }

    /// <summary> 스탯 컴포넌트 셋팅 </summary>
    protected override EntityComponentStatBase OnSetStatComponent()
    {
        return null;
    }

    /// <summary> 전투 컴포넌트 셋팅 </summary>
    protected override EntityComponentCombat OnSetCombatComponent()
    {
        return null;
    }
    /// <summary> 어빌리티 액션 관련 컴포넌트 </summary>
    protected override EntityComponentAbilityAction OnSetAbilityActionComponent()
    {
        return null;
    }

    /// <summary> AI 관련 컴포넌트 </summary>
    protected override EntityComponentAI OnSetAIComponent()
    {
        return null;
    }
    #endregion

    protected override void OnPostInitializeImpl()
    {
        base.OnPostInitializeImpl();

        //컬리젼 셋팅.
        if(DBObject.TryGet(TableId, out var table))
        {
            float radius = Mathf.Max(table.CollisionRadius, 0.5f);

            var col = gameObject.GetOrAddComponent<CapsuleCollider>();

            // TODO :: 사이즈 어떻게 처리할지..?
            col.radius = 3f;
            col.height = 2f;
            col.center = new Vector3( 0f, 0.5f, 0f);
            col.isTrigger = true;

            ZEffectManager.Instance.SpawnEffect(table.IdleEffect, this.transform, -1, 1, (comp) =>
            {
                ObjectEffect = comp;
            });
        }

        if (NavMesh.SamplePosition(this.Position, out var hit, 5f, NavMesh.AllAreas))
        {
            this.transform.position = hit.position;
        }
        else
		{
            ZLog.LogError(ZLogChannel.Entity, $"Hit Position is null {this.Position}");
		}
    }
	void OnTriggerEnter(Collider _col)
	{
        if (_col.GetComponent<ZPawnMyPc>() == null)
		{
            return;
		}

		if(UIManager.Instance.Find(out UISubHUDTemple temple))
		{
            temple.SetInteractionGimmick(true, () => ZPawnManager.Instance.MyEntity.ObjectGathering(this));
        }
	}

	private void OnDestroy()
	{
        if(ObjectEffect != null)
		{
            ObjectEffect.Despawn();
		}

        ObjectEffect = null;

        if (UIManager.Instance.Find(out UISubHUDTemple temple))
        {
            temple.SetInteractionGimmick(false);
        }
    }

	void OnTriggerExit(Collider _col)
	{
        if (_col.GetComponent<ZPawnMyPc>() == null)
        {
            return;
        }

        if (UIManager.Instance.Find(out UISubHUDTemple temple))
        {
            temple.SetInteractionGimmick(false);
        }
    }
}

