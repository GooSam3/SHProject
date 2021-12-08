using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;

public class DKUnitTestTraveler : DKUnitHeroBase
{
	[SerializeField]
	private DKUnitBase TargetUnit = null;

	//--------------------------------------------------------------
	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		if (Input.GetKeyDown(KeyCode.Q))
		{
			DKSkillUsage pUsage = new DKSkillUsage();
			pUsage.UsageTarget = TargetUnit;
			ISkillPlay(10, pUsage);
		}

		float fMoveFactor = 0.1f;
		if (Input.GetKey(KeyCode.W))
		{
			Vector3 vecDirection = Vector3.forward * fMoveFactor;
			ProtNavMoveFoward(vecDirection);
		}

		if (Input.GetKey(KeyCode.S))
		{
			Vector3 vecDirection = Vector3.back * fMoveFactor;
			ProtNavMoveFoward(vecDirection);
		}

		if (Input.GetKey(KeyCode.A))
		{
			Vector3 vecDirection = Vector3.left * fMoveFactor;
			ProtNavMoveFoward(vecDirection);
		}
		
		if (Input.GetKey(KeyCode.D))
		{
			Vector3 vecDirection = Vector3.right * fMoveFactor;
			ProtNavMoveFoward(vecDirection);
		}

		if (Input.GetKey(KeyCode.Z))
		{
			ProtNavMoveDestination(new Vector3(-3f, 0, 0), 0, null);
		}

		if (Input.GetKey(KeyCode.C))
		{
			ProtNavMoveDestination(new Vector3(3f, 0, 0), 0, null);
		}
	}
}
