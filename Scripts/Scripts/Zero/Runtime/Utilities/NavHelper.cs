using UnityEngine;
using UnityEngine.AI;

namespace Zero
{
	public class NavHelper
	{
		static NavMeshHit hitResult;

		/// <param name="maxDistance">이동가능한 위치 찾을 거리 (너무 높으면 무리갈듯?)</param>
		static public Vector3 GetAdjustedNavPos(Vector3 inPos, float maxDistance)
		{
			UnityEngine.AI.NavMesh.SamplePosition(inPos, out hitResult, maxDistance, UnityEngine.AI.NavMesh.AllAreas);
			return hitResult.position;
		}

		static public Vector3 GetAdjustedNavPos(Vector3 inPos, float maxDistance, int areaMask)
		{
			UnityEngine.AI.NavMesh.SamplePosition(inPos, out hitResult, maxDistance, areaMask);
			return hitResult.position;
		}
	}
}