
using UnityEngine;
using DigitalRuby.ThunderAndLightning;
using System;
using System.Collections.Generic;

/// <summary> 중력건용 자석 발사체 </summary>
public class ZTempleProjectile_Magnetic : MonoBehaviour
{
    [SerializeField]    
    private LightningSplineScript Magnetic;

    [Header("발사체 속도")]
    [SerializeField]
    private float Speed = 15f;

    private ZTempleMiniGame_GravityControl GravityControl = null;

    /// <summary> 발사 종료시 이벤트 </summary>
    private Action OnFinished;

    private ZGimmick Owner = null;

    /// <summary> 발사체 최대 거리 </summary>
    private float MaxProjectileDistance = 0f;

    /// <summary> 연결된 상태 </summary>
    public bool IsConnected { get { return null != ConnectedRBody; } }

    public Rigidbody ConnectedRBody { get; private set; } = null;

    private Vector3? GoalPosition = null;

    private void Start()
    {
        Magnetic.gameObject.SetActive(false);
    }

	private void OnDestroy()
	{
        if (false == ZMonoManager.hasInstance)
            return;

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLinkedPathUpdate);
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, HandleFixedUpdate);
    }

	/// <summary> 자석 발사체 초기화 </summary>
	public void Initialize(ZTempleMiniGame_GravityControl control, float distance, Action onFinished)
    {
        GravityControl = control;
        Owner = control.Gimmick;
        MaxProjectileDistance = distance;
        OnFinished = onFinished;

        Magnetic.gameObject.SetActive(false);

    }

    /// <summary> 발사! </summary>
    public void Fire()
    {
        if(null == GravityControl)
        {
            ZLog.LogError(ZLogChannel.Temple, "ZTempleMiniGame_GravityControl이 셋팅되지 않음");
            return;
        }
        var forward = GravityControl.transform.forward;

        Magnetic.gameObject.SetActive(true);

        ResetPath();

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleFireProjectileUpdate);
    }

    public void End()
    {
        OnHit(null);
    }

    private void OnHit(ZGimmick hitGimmick)
    {
        if (null != hitGimmick && hitGimmick.Meterial == GravityControl.ApplyMeterial)
        {
            ConnectedRBody = hitGimmick.GetComponentInChildren<Rigidbody>();
            if(null != ConnectedRBody)
            {
                ConnectedRBody.useGravity = false;
                ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLinkedPathUpdate);
                ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, HandleFixedUpdate);

                hitGimmick.SetGrivityControlTargeted(true);
            }
        }
        else
        {
            if(null != ConnectedRBody)
            {
                var gimmick = ConnectedRBody.GetComponentInParent<ZGimmick>();
                if(null != gimmick)
                {
                    gimmick.SetGrivityControlTargeted(false);
                }

                ConnectedRBody.useGravity = true;
                ConnectedRBody = null;
            }

            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleLinkedPathUpdate);
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, HandleFixedUpdate);
            GoalPosition = null;
        }

        Magnetic.gameObject.SetActive(IsConnected);
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, HandleFireProjectileUpdate);

        OnFinished?.Invoke();
    }

    private void ResetPath()
    {
        var path = Magnetic.GetCurrentPathObjects();

        for (int i = 1; i < path.Count; ++i)
        {
            path[i].transform.position = GravityControl.SocketProjectile.position;
        }
    }
    /// <summary> 발사체 업데이트 </summary>
    private void HandleFireProjectileUpdate()
    {
        var path = Magnetic.GetCurrentPathObjects();

        lock(path)
        {
            if (path.Count < 2)
            {
                OnHit(null);
                return;
            }

            var offsetRate = 1f / (path.Count - 1);
            var startTrans = path[0].transform;
            var endTrans = path[path.Count - 1].transform;

            path[0].transform.position = GravityControl.SocketProjectile.position;

            if ((startTrans.position - endTrans.position).magnitude > MaxProjectileDistance)
            {
                OnHit(null);
                return;
            }

            endTrans.position += (GravityControl.Forward * Time.smoothDeltaTime * Speed);

            var dist = (endTrans.position - startTrans.position).magnitude;

            for (int i = 1; i < path.Count - 1; ++i)
            {
                path[i].transform.position = startTrans.position + (GravityControl.Forward * dist * offsetRate * i);
            }

            var hits = Physics.SphereCastAll(endTrans.position, 0.25f, Vector3.up, 0f);

            foreach(var hit in hits)
            {
                if (hit.collider.isTrigger)
                    continue;

                var gimmick = hit.collider.gameObject.GetComponent<ZGimmick>();

                if (null == gimmick)
                {
                    OnHit(null);
                    break;
                }   

                if (gimmick == Owner)
                    continue;

                OnHit(gimmick);

                break;
            }
        }
    }

    private void HandleLinkedPathUpdate()
    {
        float angle = GravityControl.SocketProjectile.transform.eulerAngles.x - GravityControl.transform.eulerAngles.x;// VectorHelper.Axis3Angle(GravityControl.Forward); //Vector3.Angle(GravityControl.transform.forward, GravityControl.Forward);        
        
        float value = Mathf.Cos(Mathf.Deg2Rad * angle);
        float distance = 0 < value ? GravityControl.CurrentDistance / value : GravityControl.CurrentDistance;


        distance = Mathf.Abs(distance);

        var goalPosition = GravityControl.SocketProjectile.position + GravityControl.Forward * distance;

        goalPosition.y = Mathf.Clamp(goalPosition.y, Owner.Position.y, Owner.Position.y + GravityControl.HeightOffset);

        GoalPosition = goalPosition;

        var path = Magnetic.GetCurrentPathObjects();

        if (path.Count < 2)
        {
            OnHit(null);
            return;
        }

        var offsetRate = 1f / (path.Count - 1);
        var startTrans = path[0].transform;
        var endTrans = path[path.Count - 1].transform;

        path[0].transform.position = GravityControl.SocketProjectile.position;

        endTrans.position = (ConnectedRBody.worldCenterOfMass);//Vector3.Lerp(endTrans.position, goalPosition, Time.smoothDeltaTime * 10f); //goalPosition;

        var dist = (endTrans.position - startTrans.position).magnitude;

        for (int i = 1; i < path.Count - 1; ++i)
        {
            path[i].transform.position = startTrans.position + (GravityControl.Forward * dist * offsetRate * i);// Vector3.Lerp(path[i].transform.position, startTrans.position + (GravityControl.Forward * dist * offsetRate * i), Time.smoothDeltaTime * 10f);
        }
    }

    private void HandleFixedUpdate()
    {
        if (null == ConnectedRBody || null == GoalPosition)
            return;

        ConnectedRBody.velocity = (GoalPosition.Value - ConnectedRBody.worldCenterOfMass) * Time.smoothDeltaTime * 100f;
    }
}
