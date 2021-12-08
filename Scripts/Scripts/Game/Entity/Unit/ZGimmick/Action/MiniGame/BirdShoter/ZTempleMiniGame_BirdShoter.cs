using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ParadoxNotion;
using DG.Tweening;

/// <summary> 새잡기 </summary>
public class ZTempleMiniGame_BirdShoter : ZTempleMiniGameBase
{
    public override E_TempleUIType ControlType => E_TempleUIType.Joystick_CancelActionButton;

    [Header("새 프리펩")]
    [SerializeField]
    private GameObject BirdPrefab;

    [Header("총알 프리펩")]
    [SerializeField]
    private GameObject ProjectilePrefab;

    [Header("총알 속도")]
    [SerializeField]
    [Range(1, 100)]
    private float ProjectileSpeed = 20f;

    [Header("위, 아래 최대 각도")]
    [SerializeField]
    [Range(0f, 90f)]
    private float MaxAngleX = 60f;

    [Header("좌, 우 최대 각도")]
    [SerializeField]
    [Range(0f, 90f)]
    private float MaxAngleY = 60f;

    [Header("총알 장전 시간")]
    [SerializeField]
    private float ReloadDuration = 1f;

    [Header("목표 횟수")]
    [SerializeField]
    private int GoalCount = 5;

    [Header("등장 횟수")]
    [SerializeField]
    private int AppearCount = 10;

    [Header("등장 위치")]
    [SerializeField]
    private GameObject StartPointGroup;
    private ZMiniGameWayPoint[] StartPoints;

    [Header("경로")]
    [SerializeField]
    private GameObject WaypointGroup;
    private ZMiniGameWayPoint[] Waypoints;

    [Header("퇴장 위치")]
    [SerializeField]
    private GameObject EndGroup;
    private ZMiniGameWayPoint[] EndPoints;

    [Header("다음 등장 시간")]
    [SerializeField]
    private float NextAppearDelayTime = 5f;

    [Header("거쳐갈 경로 개수")]
    [SerializeField]
    private int WaypointCount = 3;

    [Header("새 최소 속도")]
    [SerializeField]
    private float MinBirdSpeed = 3f;
    [Header("새 최대 속도")]
    [SerializeField]
    private float MaxBirdSpeed = 10f;

    [Header("감도")]
    [SerializeField]
    private float Sensitivity = 1f;

    [Header("타겟팅 이펙트")]
    [SerializeField]
    private GameObject Fx_Target;
    [Header("타겟팅 온 이펙트")]
    [SerializeField]
    private GameObject Fx_TargetOn;
    [Header("대기중 이펙트")]
    [SerializeField]
    private GameObject Fx_WaitTime;

    /// <summary> 발사 시간 </summary>
    private float FireTime;

    /// <summary> 현재 잡은 횟수 </summary>
    private int CurrentKillCount = 0;

    private float AngleY = 0;
    private float AngleX = 0;

    private Vector3 DefaultAngle;

    private bool IsReload = false;

    private RaycastHit[] mHitResults = new RaycastHit[5];

    private void Awake()
    {
        StartPoints = StartPointGroup.GetComponentsInChildren<ZMiniGameWayPoint>();
        Waypoints = WaypointGroup.GetComponentsInChildren<ZMiniGameWayPoint>();
        EndPoints = EndGroup.GetComponentsInChildren<ZMiniGameWayPoint>();

        DefaultAngle = VirtualCamera.transform.rotation.eulerAngles;

        ZTempleHelper.SetActiveFx(Fx_Target, false);
        ZTempleHelper.SetActiveFx(Fx_TargetOn, false);
        ZTempleHelper.SetActiveFx(Fx_WaitTime, false);
    }

    protected override void StartMiniGame()
    {
        CurrentKillCount = 0;
        
        ZTempleHelper.SetActiveFx(Fx_Target, true);

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);

        StartCoroutine(Co_RunMiniGame());
    }

    protected override void EndMiniGame()
    {
        StopAllCoroutines();

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    private void OnDestroy()
    {
        if(ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLateUpdate);
    }

    protected override bool CheckCompleteImpl()
    {
        return GoalCount <= CurrentKillCount;
    }

    private IEnumerator Co_RunMiniGame()
    {
        var delay = new WaitForSeconds(NextAppearDelayTime);

        var appearCount = AppearCount;

        while (IsPlaying && 0 < appearCount)
        {
            
            //새 출현 및 이동
            List<Vector3> points = new List<Vector3>();

            int startIndex = Random.Range(0, StartPoints.Length);

            points.Add(StartPoints[startIndex].transform.position);

            //덤블 흔들리기 연출
            ShakeBush(startIndex);

            List<ZMiniGameWayPoint> list = new List<ZMiniGameWayPoint>(Waypoints);

            list.Shuffle();

            int count = WaypointCount;

            for(int i = 0; i < list.Count; ++i)
            {
                points.Add(list[i].transform.position);
                --count;

                if (0 >= count)
                    break;
            }

            points.Add(EndPoints[Random.Range(0, EndPoints.Length)].transform.position);

            CreateBird(ref points);

            --appearCount;

            yield return delay;
        }

        yield return delay;

        Cancel();
    }

    private void ShakeBush(int startIndex)
    {
        var bushs = StartPoints[startIndex].GetComponentsInChildren<Transform>();

        if (bushs.Length <= 1)
        {
            bushs[0].DOShakePosition(0.2f, 0.5f);
        }
        else
        {
            for(int i = 1; i < bushs.Length; ++i)
            {
                bushs[i].DOShakePosition(0.3f, 0.5f);
            }
        }

    }

    private void CreateBird(ref List<Vector3> points)
    {
        var go = Instantiate(BirdPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        var bird = go.GetComponent<ZMiniGameBird>();
        bird.Run(MinBirdSpeed, MaxBirdSpeed, points.ToArray());
    }

    private void HandleHit(ZMiniGameBird bird)
    {
        ++CurrentKillCount;

        Invoke(nameof(CheckComplete), 2f);
    }

    public override void OnClickAction()
    {
        //TODO :: 총 발사

        if (FireTime + ReloadDuration > Time.time)
        {
            ZLog.LogWarn(ZLogChannel.Temple, "장전 중이다.");
            return;
        }

        ZTempleHelper.SetActiveFx(Fx_Target, false);
        ZTempleHelper.SetActiveFx(Fx_TargetOn, false);
        ZTempleHelper.SetActiveFx(Fx_WaitTime, true);

        FireTime = Time.time;
        IsReload = true;
        Fire();
        Invoke(nameof(Reload), ReloadDuration);

        CameraManager.Instance.DoShake(VirtualCamera.transform.position, Vector3.up);
    }


    /// <summary> 발사 </summary>
    private void Fire()
    {
        ZMiniGameBirdProjectile.Fire(ProjectilePrefab, VirtualCamera.transform.position, VirtualCamera.transform.forward, ProjectileSpeed, HandleHit);
    }

    private void Reload()
    {
        CancelInvoke(nameof(Reload));
        IsReload = false;
        FireTime = 0;
        ZTempleHelper.SetActiveFx(Fx_Target, true);
        ZTempleHelper.SetActiveFx(Fx_TargetOn, false);
        ZTempleHelper.SetActiveFx(Fx_WaitTime, false);
    }

    public override void MoveJoystick(Vector2 joysticDir)
    {
        //Gimmick.transform.rot.MoveRotation(Quaternion.Euler(InputX, 0f, InputZ));
        AngleY += (joysticDir.x * Sensitivity);
        AngleX -= (joysticDir.y * Sensitivity);

        AngleY = Mathf.Max(Mathf.Min(AngleY, MaxAngleY), -MaxAngleY);
        AngleX = Mathf.Max(Mathf.Min(AngleX, MaxAngleX), -MaxAngleX);        
    }

    private void HandleLateUpdate()
    {
        Vector3 goal = DefaultAngle + new Vector3(AngleX, AngleY, 0f);
        VirtualCamera.transform.rotation = Quaternion.Lerp(VirtualCamera.transform.rotation, Quaternion.Euler(goal), Time.smoothDeltaTime * 10f);
        //VirtualCamera.transform.rotation = Quaternion.Euler((DefaultAngle + new Vector3(AngleX, AngleY, 0f)));

        if(false == IsReload)
        {
            //타겟 온 체크
            int hitCount = Physics.SphereCastNonAlloc(VirtualCamera.transform.position, 0.2f, VirtualCamera.transform.forward, mHitResults, VirtualCamera.transform.localPosition.z * 2f, UnityConstants.Layers.MiniGameObjectMask);
            ZTempleHelper.SetActiveFx(Fx_TargetOn, 0 < hitCount);
        }
    }
}
