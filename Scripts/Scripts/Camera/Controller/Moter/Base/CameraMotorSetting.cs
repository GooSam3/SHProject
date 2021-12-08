using Cinemachine;
using System;
using UnityEngine;

/// <summary> ī�޶� ���� ������ </summary>
[ Serializable]
public class CameraMotorSettingBase
{
    /// <summary> �⺻ ���� </summary>
    public Vector3 Angle = new Vector3(54f, -135f, 0f);

    /// <summary> Target�� Offset </summary>
    public Vector3 TargetOffset = new Vector3(0f, 0f, 0f);

    /// <summary> �ּ� ī�޶� �Ÿ� </summary>
    public float MinDistance = 5f;

    /// <summary> �ִ� ī�޶� �Ÿ� </summary>
    public float MaxDistance = 20f;

    /// <summary> ���� �⺻ ī�޶� �Ÿ� </summary>
    public float Distance = 10;

	[Tooltip("ī�޶� �Ÿ������, �ڿ������� �̵��� �ӵ�")]
	public float Smoothing = 10;

    /// <summary> ��ũ�� �ӵ� </summary>
    public float SpeedScroll = 0.5f;  
}

[Serializable]
public class CameraMotorSettingFixed : CameraMotorSettingBase
{
}

[Serializable]
public class CameraMotorSettingLookTarget : CameraMotorSettingBase
{
    public float LookTargetOffsetY;
}

[Serializable]
public class CameraMotorSettingFree : CameraMotorSettingBase
{
    /// <summary> ���� �ּ� ���� </summary>
    [Range( -85, 20 )]
    public float MinAngleX = 0.0f;

    /// <summary> ���� �ִ� ���� </summary>
    [Range( 20, 85 )]
    public float MaxAngleX = 80.0f;
}

[Serializable]
public class CameraMotorSettingShoulder : CameraMotorSettingFree
{
    public AxisState.Recentering RecenteringY;
}
