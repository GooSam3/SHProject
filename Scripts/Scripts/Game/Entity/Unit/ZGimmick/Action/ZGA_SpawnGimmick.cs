using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> 기믹을 스폰한다. </summary>
public class ZGA_SpawnGimmick : ZGimmickActionBase
{
    [Header("스폰될 기믹 프리펩")]
    public GameObject GimmickPrefab;

    [Header("스폰될 기믹에 부여될 ID")]
    public string SpawnedGimmickId ="";

    [Header("기믹이 스폰될 위치")]
    public Transform SpawnPosition = null;

    [Header("스폰될 최대 갯수")]
    public uint MaxSpawnCount = 1;

    [Header("스폰될 기믹이 Destroy됐을 때 다시 스폰할지 여부")]
    public bool IsRespawn = true;

    [Header("여러개 스폰시 다음 기믹 스폰까지 대기 시간")]
    public float DelayTime = 5f;

    private List<ZGimmick> m_listGimmick = new List<ZGimmick>();

    /// <summary> 마지막 스폰 시간 </summary>
    private float LastSpawnedTime = 0f;

    private int CurrentSpawnCount = 0;

    protected override void InvokeImpl()
    {
        StartCoroutine(Co_CheckSpawn());
    }

    protected override void CancelImpl()
    {
    }

    private IEnumerator Co_CheckSpawn()
    {
        var waitInstruction = new WaitForSeconds(0.1f);

        if (null == GimmickPrefab)
        {
            ZLog.LogError(ZLogChannel.Temple, "프리펩이 존재하지 않음!!!");
            yield break;
        }            

        while (true)
        {
            //대기
            if(Time.time > 0 && LastSpawnedTime + DelayTime > Time.time)
            {
                yield return new WaitForSeconds(LastSpawnedTime + DelayTime - Time.time);
                continue;
            }

            //다시 스폰되는 기믹이 아닐 경우 최대 갯수 이상 스폰되었으면 종료.
            if (false == IsRespawn && CurrentSpawnCount >= MaxSpawnCount)
            {
                yield break;
            }

            //제거된 기믹 리스트에서 제거
            m_listGimmick.RemoveAll(gimmick => null == gimmick);

            //최대 스폰 개수보다 작을 경우에만 스폰
            if (m_listGimmick.Count < MaxSpawnCount)
            {
                ++CurrentSpawnCount;

                var go = GameObject.Instantiate(GimmickPrefab, SpawnPosition.position, Quaternion.identity);
                ZGimmick gimmick = go.GetComponent<ZGimmick>();

                if(gimmick)
                {
                    gimmick.gameObject.SetActive(true);
                    gimmick.SetGimmickId(SpawnedGimmickId);
                    m_listGimmick.Add(gimmick);
                }

                LastSpawnedTime = Time.time;
            }

            yield return waitInstruction;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, Vector3.one);

        if(null != GimmickPrefab)
            Handles.Label(transform.position + Vector3.right + Vector3.up, $"Spawn : {GimmickPrefab.name}", EditorStyles.popup);
    }
#endif
}