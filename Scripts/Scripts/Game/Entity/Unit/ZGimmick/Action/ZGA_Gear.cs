using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rotation_Angle
{
	X = 1,
	Y = 2,
	Z = 3,
}

/// <summary>
/// 기어가 움직일때 같이 움직여줄 오브젝트
/// </summary>
[Serializable]
public class RorationOther
{
	//[Header("타겟 GameObject")]
	[SerializeField]
	public GameObject Target;

	//[Header("회전축")]
	[SerializeField]
	public Rotation_Angle RotateAngle = Rotation_Angle.Y;

	//[Header("속도")]
	public float Speed = 1f;

	[HideInInspector]
	public float currentAngle;
}

public class ZGA_Gear : ZGimmickActionBase
{
	[Header("기어 회전축")]
	[SerializeField]
	public Rotation_Angle RotateAngle = Rotation_Angle.Y;

	[Header("기어가 회전하는 속도")]
	[SerializeField]
	private float RotationSpeed = 1f;

	[Header("기어가 회전할때 같이 회전시켜줄 오브젝트들")]
	[SerializeField]
	private List<RorationOther> OtherRotationObj = new List<RorationOther>();

	// 얼렸는지 여부
	private bool isFreeze = false;

	// 현제 톱니바퀴의 회전값
	private float CurrentAngle = 0f;

	protected override void InitializeImpl()
	{
		StartCoroutine(nameof(LateFixedUpdate));
		CurrentAngle = GetStartEulerAngle(RotateAngle, gameObject.transform.eulerAngles);
	}

	protected override void InvokeImpl()
	{
		StopAllCoroutines();
		isFreeze = false;
		StartCoroutine(nameof(LateFixedUpdate));
	}

	protected override void CancelImpl()
	{
		isFreeze = true;
		StopAllCoroutines();
	}

	protected override void DestroyImpl()
	{
	}

	private IEnumerator LateFixedUpdate()
	{
		WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
		while (true)
		{
			yield return _instruction;

			if (isFreeze)
				yield break;

			CurrentAngle += Time.deltaTime * RotationSpeed;
			transform.rotation = GetQuaternion(RotateAngle, CurrentAngle);

			foreach (var other in OtherRotationObj)
			{
				if (null == other.Target)
					continue;

				if(other.currentAngle == 0)
				{
					other.currentAngle = GetStartEulerAngle(other.RotateAngle, other.Target.transform.eulerAngles);
				}
				other.currentAngle += Time.deltaTime * other.Speed;
				other.Target.transform.rotation = GetQuaternion(other.RotateAngle, other.currentAngle);
			}
		}
	}

	private Quaternion GetQuaternion(Rotation_Angle angle, float currentAngle)
	{
		switch(angle)
		{
			case Rotation_Angle.X: return Quaternion.Euler(currentAngle, 0, 0);
			case Rotation_Angle.Y: return Quaternion.Euler(0, currentAngle, 0);
			case Rotation_Angle.Z: return Quaternion.Euler(0, 0, currentAngle);
			default: return Quaternion.Euler(0, currentAngle, 0);
		}
	}

	private float GetStartEulerAngle(Rotation_Angle angle, Vector3 eulerAngles)
	{
		switch (angle)
		{
			case Rotation_Angle.X: return eulerAngles.x;
			case Rotation_Angle.Y: return eulerAngles.y;
			case Rotation_Angle.Z: return eulerAngles.z;
			default: return eulerAngles.y;
		}
	}
}
