using DG.Tweening;
using UnityEngine;
using System;

/// <summary> 걍 서칭 및 표시용 </summary>
public class ZMiniGameWayPoint : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
    }
#endif
}
