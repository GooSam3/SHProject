using Cinemachine;
using System;
using UnityEngine;

/// <summary> 카메라 셋팅 데이터 </summary>
[ Serializable]
public class CameraMotorSettingBase
{
    /// <summary> 기본 각도 </summary>
    public Vector3 Angle = new Vector3(54f, -135f, 0f);

    /// <summary> Target의 Offset </summary>
    public Vector3 TargetOffset = new Vector3(0f, 0f, 0f);

    /// <summary> 최소 카메라 거리 </summary>
    public float MinDistance = 5f;

    /// <summary> 최대 카메라 거리 </summary>
    public float MaxDistance = 20f;

    /// <summary> 최초 기본 카메라 거리 </summary>
    public float Distance = 10;

	[Tooltip("카메라 거리변경시, 자연스럽게 이동될 속도")]
	public float Smoothing = 10;

    /// <summary> 스크롤 속도 </summary>
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
    /// <summary> 상하 최소 각도 </summary>
    [Range( -85, 20 )]
    public float MinAngleX = 0.0f;

    /// <summary> 상하 최대 각도 </summary>
    [Range( 20, 85 )]
    public float MaxAngleX = 80.0f;
}

[Serializable]
public class CameraMotorSettingShoulder : CameraMotorSettingFree
{
    public AxisState.Recentering RecenteringY;
}
