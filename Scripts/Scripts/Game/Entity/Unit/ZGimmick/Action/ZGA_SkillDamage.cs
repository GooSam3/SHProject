using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZGA_SkillDamage : ZGimmickActionBase
{
	[Header("퍼센트 데미지 사용 여부")]
	[SerializeField]
	private bool IsDamagePercent;

	[Header("스킬 데미지")]
	[SerializeField]
	private uint Damage;

	[Header("스킬 TableID")]
	[SerializeField]
	private uint SkillTId;

	[Header("스킬 PosMoveType")]
	[SerializeField]
	private E_PosMoveType PosMoveType;

	// 데미지 효과를 발동시켜줄 타겟
	private List<EntityBase> Targets = null;

	private bool IsActive;

	protected override void InitializeImpl()
	{
		base.InitializeImpl();
		Targets = new List<EntityBase>();
		IsActive = false;
	}

	protected override void CancelImpl()
	{
		throw new System.NotImplementedException();
	}

	protected override void InvokeImpl()
	{
		if (IsActive)
			return;

		IsActive = true;
		InvokeSkillAction();
	}

	private void InvokeSkillAction()
	{
		if (100 <= Damage)
			Damage = 100;

		uint totalDamage = 0;
		foreach (var entity in Targets)
		{
			var pawn = ZPawnManager.Instance.GetEntity(entity.EntityId);
			if (entity.IsDead)
				continue;

			if (IsDamagePercent)
			{
				var hp = (uint)entity.EntityData.MaxHp;
				var damage = Damage * 0.01;
				totalDamage = (uint)(hp * damage);
			}

			ZTempleHelper.TempleEntityAttack(Gimmick, entity, SkillTId, totalDamage);
			if(entity.IsMyPc)
			{
				ForceMoveEffect_MyPC(entity);
			}
			else
			{
				ForceMoveEffect(entity);
			}
		}
	}

	/// <summary>
	/// 몬스터에게 스킬을 적용하는 경우
	/// </summary>
	/// <param name="target"></param>
	private void ForceMoveEffect(EntityBase target)
	{
		Vector3 MovePosition;
		switch (PosMoveType)
		{
			case E_PosMoveType.TargetKnockBack:
				{
					MovePosition = target.transform.position + (-2 * target.transform.forward);
					target.ForceMove(MovePosition, 1f, E_PosMoveType.TargetKnockBack);
				}
				break;
			default: break;
		}
	}

	/// <summary>
	/// PC에게 스킬을 적용하는 경우
	/// </summary>
	/// <param name="target"></param>
	private void ForceMoveEffect_MyPC(EntityBase target)
	{
		var myPC = target.GetComponent<ZPawnMyPc>();
		switch (PosMoveType)
		{
			case E_PosMoveType.TargetKnockBack:
				{
					var movement = target.GetComponent<EntityComponentMovement_Temple>();
					Vector3 moveVector = target.transform.position - Gimmick.transform.position;
					moveVector.x *= 10;
					moveVector.y = target.transform.position.y;
					moveVector.z *= 10;
					if (movement.CurrentState is TempleCharacterControlState_Default defaultMovement)
					{
						defaultMovement.DoAddMomentum(moveVector, 0.3f);
					}
					myPC.SetAnimParameter(E_AnimParameter.Hit_001);
				}
				break;
			default: break;
		}
	}


	private void OnTriggerEnter(Collider other)
	{
		var Entity = other.GetComponent<EntityBase>();
		if (null == Entity)
			return;

		if (Entity.EntityType != E_UnitType.Monster && Entity.EntityType != E_UnitType.Character)
			return;

		if (null != Targets.Find(d => d == Entity))
			return;

		Targets.Add(Entity);
	}

	private void OnTriggerExit(Collider other)
	{
		var Entity = other.GetComponent<EntityBase>();
		if (null == Entity)
			return;

		if (Entity.EntityType != E_UnitType.Monster && Entity.EntityType != E_UnitType.Character)
			return;

		if (null == Targets.Find(d => d == Entity))
			return;

		Targets.Remove(Entity);
	}
}
