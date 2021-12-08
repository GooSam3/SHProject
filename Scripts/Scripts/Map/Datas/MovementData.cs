using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MovementCollectionData
{
	public enum E_MovementType
	{
		/// <summary> 이동 데이터 없음 </summary>
		None,
		/// <summary> Roaming, Patrol, Waypoint 이동방식에 사용 </summary>
		Waypoint,
		/// <summary> 일정 범위내 랜덤하게이동. </summary>
		RandomRadius,
	}

	/// <summary> 사용하는 데이터구분 </summary>
	public E_MovementType UsingType = E_MovementType.None;

	public MovementData_RandomRadius RandomRadiusData;
	public MovementData_WaypointData WaypointsData;

	public MovementCollectionData()
	{
	}

	public MovementCollectionData(MovementData_RandomRadius _data)
	{
		this.UsingType = E_MovementType.RandomRadius;
		this.RandomRadiusData = _data;
	}

	public MovementCollectionData(MovementData_WaypointData _data)
	{
		this.UsingType = E_MovementType.Waypoint;
		this.WaypointsData = _data;
	}

	public MovementCollectionData Clone()
	{
		var data = new MovementCollectionData();
		data.UsingType = this.UsingType;
		data.RandomRadiusData = null != this.RandomRadiusData ? this.RandomRadiusData.Clone() : null;
		data.WaypointsData = null != this.WaypointsData ? this.WaypointsData.Clone() : null;

		return data;
	}
}

[System.Serializable]
public class MovementDataBase
{
	public virtual MovementCollectionData.E_MovementType MovementType => MovementCollectionData.E_MovementType.None;
	
	public MovementDataBase Clone()
	{
		return new MovementDataBase();
	}
}


[System.Serializable]
public class MovementData_RandomRadius : MovementDataBase
{
	public override MovementCollectionData.E_MovementType MovementType => MovementCollectionData.E_MovementType.RandomRadius;

	[Header("랜덤 이동 범위 (반지름)")]
	public float Radius;
	[Header("한번 이동하고나서 대기할 최소 시간(초)")]
	public float MinRestTime;
	[Header("한번 이동하고나서 대기할 최대 시간(초)")]
	public float MaxRestTime;

	new public MovementData_RandomRadius Clone()
	{
		return new MovementData_RandomRadius()
		{
			Radius = this.Radius,
			MinRestTime = this.MinRestTime,
			MaxRestTime = this.MaxRestTime,
		};
	}
}

/// <summary>
/// 경로 이동방향 결정
/// </summary>
public enum E_TravelDirection
{
	Forward,
	Reverse
}
public enum E_EndpointBehavior
{
	/// <summary> 마지막 지점까지 도달하면 이동 멈춤 </summary>
	Stop,
	/// <summary> 계속 순환 </summary>
	Loop,
	/// <summary> 마지막 지점까지 도달하면 역으로 이동 </summary>
	PingPong,
}


/// <summary>
/// 
/// </summary>
[System.Serializable]
public class MovementData_WaypointData : MovementDataBase
{
	public override MovementCollectionData.E_MovementType MovementType => MovementCollectionData.E_MovementType.Waypoint;

	/// <summary> 이동 방향 </summary>
	public E_TravelDirection TravelDirection;
	/// <summary> 마지막 지점 까지 이동후, 다음 동작 </summary>
	public E_EndpointBehavior EndpointBehavior;

	/// <summary> 이동 포인트마다 이동하다가 잠시 멈출 확률(%)</summary>
	[Range(0, 100)]
	public float RandomStopProbability;
	/// <summary> 이동 멈추고나서, 다시 움직이기전까지 딜레이 시간.(초) </summary>
	public Vector2 RandomStopResumeDelay;

	public List<WaypointData> Waypoints;

	new public MovementData_WaypointData Clone()
	{
		var data = new MovementData_WaypointData();
		data.TravelDirection = this.TravelDirection;
		data.EndpointBehavior = this.EndpointBehavior;
		data.RandomStopProbability = this.RandomStopProbability;
		data.RandomStopResumeDelay = this.RandomStopResumeDelay;
		data.Waypoints = new List<WaypointData>();
		if (null != this.Waypoints)
		{
			foreach (var point in this.Waypoints)
			{
				data.Waypoints.Add(point.Clone());
			}
		}

		return data;
	}
}

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class WaypointData
{
	[SerializeField]
	public Vector3 Position;

	[SerializeField]
	public Quaternion Rotation = Quaternion.identity;

	/// <summary> offset 기준이되는 객체 </summary>
	WaypointsComponent mOffsetGroup;

	public void SetWaypointGroup(WaypointsComponent groupComp)
	{
		mOffsetGroup = groupComp;
	}

	public WaypointData Clone()
	{
		return new WaypointData()
		{
			Position = this.Position,
			Rotation = this.Rotation,
		};
	}

	public void CopyOther(WaypointData other)
	{
		if (other == null)
			return;

		Position = other.Position;
	}

	public Vector3 GetPosition()
	{
		if (mOffsetGroup != null)
			return mOffsetGroup.transform.position + Position;
		else
			return Position;
	}

	public void UpdatePosition(Vector3 newPos)
	{
		Position.x += newPos.x;
		Position.y += newPos.y;
		Position.z += newPos.z;
	}
}