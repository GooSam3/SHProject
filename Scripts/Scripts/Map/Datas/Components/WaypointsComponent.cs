using System.Collections.Generic;
using UnityEngine;

public class WaypointsComponent : MonoBehaviour
{
	[HideInInspector]
	public MovementData_WaypointData Data;

	private void Start()
	{
		if (null != Data && null != Data.Waypoints)
		{
			foreach (WaypointData wp in Data.Waypoints)
				wp.SetWaypointGroup(this);
		}
	}

	public void Assign(MovementData_WaypointData _data)
	{
		this.Data = _data;
		this.GetWaypointChildren();
	}

	/// <summary>
	/// Returns a list of  Waypoints; resets the parent transform if reparent == true
	/// </summary>
	/// <returns></returns>
	public List<WaypointData> GetWaypointChildren(bool reparent = true)
	{
		if (null == this.Data)
			return null;

		if (null != this.Data && null == this.Data.Waypoints)
			this.Data.Waypoints = new List<WaypointData>();

		if (reparent == true)
		{
			foreach (WaypointData wp in this.Data.Waypoints)
				wp.SetWaypointGroup(this);
		}

		return this.Data.Waypoints;
	}


	public void AddWaypoint(WaypointData wp, int ndx = -1)
	{
		if (null == this.Data)
			return;

		if (this.Data.Waypoints == null)
			this.Data.Waypoints = new List<WaypointData>();

		if (ndx == -1)
			this.Data.Waypoints.Add(wp);
		else
			this.Data.Waypoints.Insert(ndx, wp);
		wp.SetWaypointGroup(this);
	}

}
