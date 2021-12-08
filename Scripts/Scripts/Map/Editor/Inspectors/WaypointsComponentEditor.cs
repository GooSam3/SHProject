using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(WaypointsComponent))]
[CanEditMultipleObjects()]
public class WaypointsComponentEditor : Editor
{
	WaypointsComponent waypointsGroup;
	List<WaypointData> waypoints;

	WaypointData selectedWaypoint = null;

	static GUIContent travelLabel = EditorGUIUtility.TrTextContent("TravelDirection", "이동 방향");
	static GUIContent endPointLabel = EditorGUIUtility.TrTextContent("EndpointBehavior", "마지막 지점 까지 이동후, 다음 동작");

	static GUIContent randStopLabel = EditorGUIUtility.TrTextContent("RandomStopProbability", "이동 포인트마다 이동하다가 잠시 멈출 확률(%)");
	static GUIContent randDelayLabel = EditorGUIUtility.TrTextContent("RandomStopResumeDelay", "이동 멈추고나서, 다시 움직이기전까지 딜레이 시간.(초)");

	private void OnEnable()
	{
		waypointsGroup = target as WaypointsComponent;
		waypoints = waypointsGroup.GetWaypointChildren();
	}

	private void OnSceneGUI()
	{
		DrawWaypoints(waypoints);
	}

	override public void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		EditorGUILayout.BeginVertical();

		var data = waypointsGroup.Data;
		data.TravelDirection = (E_TravelDirection)EditorGUILayout.EnumPopup(travelLabel, data.TravelDirection);
		data.EndpointBehavior = (E_EndpointBehavior)EditorGUILayout.EnumPopup(endPointLabel, data.EndpointBehavior);

		EditorGUILayout.Space();

		data.RandomStopProbability = EditorGUILayout.Slider(randStopLabel, data.RandomStopProbability, 0, 100);

		EditorGUILayout.LabelField(randDelayLabel);
		using (var h1 = new EditorGUILayout.HorizontalScope())
		{
			EditorGUIUtility.labelWidth = 40f;
			data.RandomStopResumeDelay.x = EditorGUILayout.FloatField("Min", data.RandomStopResumeDelay.x);
			data.RandomStopResumeDelay.y = EditorGUILayout.FloatField("Max", data.RandomStopResumeDelay.y);
		}

		EditorGUILayout.Space();

		ZGUIStyles.Separator();

		bool dorepaint = false;

		if (waypoints != null)
		{
			int delIndex = -1;
			for (int cnt = 0; cnt < waypoints.Count; cnt++)
			{
				Color guiColor = GUI.color;

				WaypointData cwp = waypoints[cnt];

				if (cwp == selectedWaypoint)
					GUI.color = Color.green;

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("S", GUILayout.Width(20)))
				{
					if (selectedWaypoint == cwp)
					{
						selectedWaypoint = null;
					}
					else
					{
						selectedWaypoint = cwp;
					}

					dorepaint = true;

				}

				EditorGUI.BeginChangeCheck();
				Vector3 oldV = cwp.GetPosition();
				Vector3 newV = EditorGUILayout.Vector3Field("", oldV);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(waypointsGroup, "Waypoint Moved");
					cwp.UpdatePosition(newV - oldV);
				}



				if (GUILayout.Button("D", GUILayout.Width(25)))
				{
					delIndex = cnt;
					dorepaint = true;

				}
				GUI.color = guiColor;
				EditorGUILayout.EndHorizontal();

			}

			if (delIndex > -1)
			{
				if (waypoints[delIndex] == selectedWaypoint)
					selectedWaypoint = null;
				waypoints.RemoveAt(delIndex);
			}

		}


		if (GUILayout.Button("Add"))
		{
			Undo.RecordObject(waypointsGroup, "Waypoint Added");
			int ndx = -1;
			if (selectedWaypoint != null)
			{
				ndx = waypoints.IndexOf(selectedWaypoint);
				if (ndx == -1)
					selectedWaypoint = null;
				else
					ndx += 1;
			}


			WaypointData wp = new WaypointData();
			wp.CopyOther(selectedWaypoint);
			waypointsGroup.AddWaypoint(wp, ndx);
			selectedWaypoint = wp;
			dorepaint = true;
		}

		GUILayout.Space(10);

		if (GUILayout.Button("NavMesh에 위치 맞추기"))
		{
			bool isModified = false;
			// 위치 유효성 검사.
			for (int index = 0; index < waypoints.Count; index++)
			{
				WaypointData cwp = waypoints[index];

				Vector3 originalPos = cwp.GetPosition();
				Vector3 newPos = originalPos;

				if (NavMesh.SamplePosition(originalPos, out var sampleHit, 1000, -1))
				{
					newPos = sampleHit.position;
				}
				else
				{
					if (NavMesh.FindClosestEdge(originalPos, out var closeHit, -1))
					{
						newPos = sampleHit.position;
					}
				}

				if (originalPos != newPos)
				{
					Debug.Log($"<color=red>이동됨 | [{index}]: {originalPos} ==> {newPos}</color>");

					isModified = true;
					cwp.UpdatePosition(newPos - originalPos);
				}
			}

			if (!isModified)
			{
				Debug.Log("<color=green>축하드립니다. 모두 정상 위치에 있습니다!</color>");
			}
		}

		EditorGUILayout.EndVertical();
		if (dorepaint)
		{
			SceneView.RepaintAll();
		}

	}

	public void DrawWaypoints(List<WaypointData> waypoints)
	{
		bool doRepaint = false;
		if (waypoints != null)
		{
			int cnt = 0;
			foreach (WaypointData wp in waypoints)
			{
				doRepaint |= DrawInScene(wp);

				// Draw a pointer line 
				if (cnt < waypoints.Count - 1)
				{
					WaypointData wpnext = waypoints[cnt + 1];
					Handles.DrawLine(wp.GetPosition(), wpnext.GetPosition());
				}
				else
				{
					if (waypointsGroup.Data.EndpointBehavior != E_EndpointBehavior.Loop)
						continue;

					WaypointData wpnext = waypoints[0];
					Color c = Handles.color;
					Handles.color = Color.gray;
					Handles.DrawLine(wp.GetPosition(), wpnext.GetPosition());
					Handles.color = c;
				}
				cnt += 1;
			}
		}

		if (doRepaint)
		{
			Repaint();
		}
	}


	public bool DrawInScene(WaypointData waypoint, int controlID = -1)
	{
		if (waypoint == null)
		{
			Debug.Log("NO WP!");
			return false;
		}

		bool doRepaint = false;
		//None serialized field, gets "lost" during serailize updates;
		waypoint.SetWaypointGroup(waypointsGroup);

		if (selectedWaypoint == waypoint)
		{
			Color c = Handles.color;
			Handles.color = Color.green;

			//Vector3 newPos = Handles.FreeMoveHandle(waypoint.GetPosition(), waypoint.rotation, 1.0f, Vector3.zero, Handles.SphereHandleCap);
			EditorGUI.BeginChangeCheck();
			Vector3 oldpos = waypoint.GetPosition();
			Vector3 newPos = Handles.PositionHandle(oldpos, waypoint.Rotation);

			float handleSize = HandleUtility.GetHandleSize(newPos);

			Handles.SphereHandleCap(-1, newPos, waypoint.Rotation, 0.25f * handleSize, EventType.Repaint);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(waypointsGroup, "Waypoint Moved");
				waypoint.UpdatePosition(newPos - oldpos);
			}

			Handles.color = c;
		}
		else
		{
			Vector3 currPos = waypoint.GetPosition();
			float handleSize = HandleUtility.GetHandleSize(currPos);
			if (Handles.Button(currPos, waypoint.Rotation, 0.25f * handleSize, 0.25f * handleSize, Handles.SphereHandleCap))
			{
				doRepaint = true;
				selectedWaypoint = waypoint;
			}
		}
		return doRepaint;
	}

	public static void CreateWaypointGroup(string name)
	{
		GameObject go = new GameObject("WaypointsGroup");
		go.AddComponent<WaypointsComponent>();
		// Select it:
		Selection.activeGameObject = go;
	}
}