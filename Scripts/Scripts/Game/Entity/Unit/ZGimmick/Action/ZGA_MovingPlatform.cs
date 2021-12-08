using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 이동하는 발판 </summary>
public class ZGA_MovingPlatform : ZGimmickActionBase
{    
    /// <summary> 발판 이동 속도 </summary>
    [Header("발판 이동 속도")]
    public float PlatformMoveSpeed = 10f;

    /// <summary> 이동 타입 </summary>
    [Header("이동 타입")]
    public E_MovingPlatformMoveType MoveType = E_MovingPlatformMoveType.Loop;

    /// <summary> 다음 위치 타입 </summary>
    [Header("다음 위치 셋팅 타입")]
    public E_MovingPlatformNextPositionType NextPositionType = E_MovingPlatformNextPositionType.PingPong;

    /// <summary> 웨이포인트 리스트 </summary>
    [Header("웨이포인트 리스트")]
    public List<Transform> Waypoint = new List<Transform>();

    /// <summary> 각 웨이포인트에서 기다리는 시간 </summary>
    [Header("각 웨이포인트에서 기다리는 시간")]
    public float WaitTime = 1f;

    /// <summary> 마지막 웨이포인트에서 기다리는 시간 </summary>
    [Header("마지막 웨이포인트에서 별도로 기다리는 시간")]
    public float LastWaitTime = 1f;

    /// <summary> 정방향, 반대방향 처리 </summary>    
    private bool mReverseDirection = false;

    /// <summary> 기다리는 중인지 여부 </summary>    
    private bool mIsWaiting = false;

    /// <summary> 다음 발동까지 대기중인지 여부 </summary>
    private bool mIsWaitingNextInvoke = false;

    /// <summary> 마지막 웨이포인트에서 기다리고있는지 여부 </summary>    
    private bool mIsLastWaiting = false;
        
    private Rigidbody RBody;
    
    /// <summary> 트리거 처리 </summary>
    private TriggerArea mTriggerArea;
    
    /// <summary> 현재 웨이포인트 인덱스 </summary>
    private int mCurrentWaypointIndex = 0;

    /// <summary> 현재 웨이포인트 </summary>
    private Transform mCurrentWaypoint;

    private bool IsMoving;
        
    protected override void InvokeImpl()
    {
        if (true == IsMoving && MoveType != E_MovingPlatformMoveType.Step)
            return;

        IsMoving = true;
        mIsWaitingNextInvoke = false;

        RBody = Gimmick.gameObject.GetComponent<Rigidbody>();
        mTriggerArea = GetComponentInChildren<TriggerArea>();

        RBody.freezeRotation = true;
        RBody.useGravity = false;
        RBody.isKinematic = true;
                
        if (Waypoint.Count <= 0)
        {
            Debug.LogWarning("웨이포인트가 등록되어 있지 않다!");
        }
        else
        {
            mCurrentWaypoint = Waypoint[mCurrentWaypointIndex];
        }

        StartCoroutine(WaitRoutine());
        StartCoroutine(LateFixedUpdate());
    }

    protected override void CancelImpl()
    {
        IsMoving = false;
    }

    /// <summary> FixedUpdate 이후 호출 </summary>
    IEnumerator LateFixedUpdate()
    {
        WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
        while (true == IsMoving)
        {
            yield return _instruction;
            MovePlatform();
        }
    }

    void MovePlatform()
    {
        if (Waypoint.Count <= 0)
            return;

        if (mIsWaitingNextInvoke)
            return;

        if (mIsWaiting)
            return;
     
        Vector3 _toCurrentWaypoint = mCurrentWaypoint.position - Gimmick.transform.position;

        Vector3 _movement = _toCurrentWaypoint.normalized;

        _movement *= PlatformMoveSpeed * Time.deltaTime;

        if (_movement.magnitude >= _toCurrentWaypoint.magnitude || _movement.magnitude == 0f)
        {
            RBody.transform.position = mCurrentWaypoint.position;
            UpdateWaypoint();
        }
        else
        {
            RBody.transform.position += _movement;
        }

        if (mTriggerArea == null)
            return;

        var list = mTriggerArea.GetEnteredRigidbody();

        for (int i = 0; i < list.Count; i++)
        {
            list[i].MovePosition(list[i].position + _movement);
        }
    }

    private void UpdateWaypoint()
    {
        bool bFinalePosition = false;
        //이동 타입이 랜덤일 경우 처리
        if(NextPositionType == E_MovingPlatformNextPositionType.Random)
        {
            mCurrentWaypointIndex = UnityEngine.Random.Range(0, Waypoint.Count - 1);
            mCurrentWaypoint = Waypoint[mCurrentWaypointIndex];            
        }
        else
        {
            if (mReverseDirection)
                mCurrentWaypointIndex--;
            else
                mCurrentWaypointIndex++;

            //If end of list has been reached, reset index;
            if (mCurrentWaypointIndex >= Waypoint.Count)
            {
                bFinalePosition = true;
                switch (NextPositionType)
                {
                    case E_MovingPlatformNextPositionType.Loop:
                        {
                            mCurrentWaypointIndex = 0;
                        }
                        break;
                    case E_MovingPlatformNextPositionType.PingPong:
                        {
                            mCurrentWaypointIndex = mCurrentWaypointIndex - 1;
                            mReverseDirection = true;
                        }
                        break;
                }
            }

            if (mCurrentWaypointIndex < 0)
            {
                bFinalePosition = true;
                switch (NextPositionType)
                {
                    case E_MovingPlatformNextPositionType.Loop:
                        {
                            mCurrentWaypointIndex = Waypoint.Count - 1;
                        }
                        break;
                    case E_MovingPlatformNextPositionType.PingPong:
                        {
                            mCurrentWaypointIndex = 0;
                            mReverseDirection = false;
                        }
                        break;
                }
            }

            mIsLastWaiting = true == bFinalePosition ? true : false ;

            mCurrentWaypoint = Waypoint[mCurrentWaypointIndex];
        }

        mIsWaiting = true;

        switch(MoveType)
        {
            case E_MovingPlatformMoveType.OneShot:
                mIsWaitingNextInvoke = bFinalePosition;
                break;
            case E_MovingPlatformMoveType.Step:
                mIsWaitingNextInvoke = true;
                break;
        }
    }

    /// <summary> 도착후 대기 </summary>
    IEnumerator WaitRoutine()
    {
        WaitForSeconds _waitInstruction = null;
        WaitForSeconds _lastWaitInstruction = null;

        if (0 < WaitTime) _waitInstruction = new WaitForSeconds(WaitTime);
        if (0 < LastWaitTime) _lastWaitInstruction = new WaitForSeconds(LastWaitTime);

        while (true)
        {
            if(mIsWaitingNextInvoke)
            {                
                yield return null;
                continue;
            }

            if (mIsWaiting)
            {
                yield return _waitInstruction;
                mIsWaiting = false;
            }

            if(mIsLastWaiting)
			{
                yield return _lastWaitInstruction;
                mIsLastWaiting = false;
            }

            yield return null;
        }
    }
}