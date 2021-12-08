using System.Collections.Generic;
using UnityEngine;

/// <summary> 점프대! </summary>
public class ZGA_JumpDivice : ZGimmickActionBase
{
    [Header("리지드 바디")]
    [SerializeField]
    private Rigidbody mRBody;

    [Header("자동 반복 여부")]
    [SerializeField]
    private bool IsAutoPlay = false;

    [Header("자동 반복시 다시 작동하기까지 대기 시간")]
    [SerializeField]
    private float AutoPlayDelayTime = 3f;

    [Header("작동 후 되감기까지 대기 시간")]
    [SerializeField]
    private float ResetDelayTime = 1f;

    [Header("작동 후 되감기를 진행할 건지 여부")]
    [SerializeField]
    private bool IsResetAfterPlay = true;

    [Header("pc를 날릴때 파워")]
    [SerializeField]
    private float Power = 10f;

    private bool IsEnterMyPc;

    private bool IsPlaying;

    private List<EntityBase> m_listEntity = new List<EntityBase>();

    private Vector3 CachecdPosition;

    /// <summary> 트리거 처리 </summary>
    private TriggerArea mTriggerArea;

    [SerializeField]
    private Collider JumpDiviceCollider;

    protected void Awake()
    {
        JumpDiviceCollider.enabled = false;
    }

    protected override void InitializeImpl()
    {
        Gimmick.SetAnimParameter(E_AnimParameter.End_001);
        Gimmick.PlayByNormalizeTime(E_AnimStateName.End_001, 1f);
        JumpDiviceCollider.enabled = true;
        CancelImpl();
    }

    protected override void InvokeImpl()
    {
        if (IsPlaying)
            return;

        JumpDiviceCollider.enabled = true;

        mTriggerArea = GetComponentInChildren<TriggerArea>();

        IsPlaying = true;
        CachecdPosition = mRBody.position;
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 1f);
        InvokePlay();        
        //StartCoroutine(LateFixedUpdate());
    }

    protected override void CancelImpl()
    {
        IsPlaying = false;
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
        CancelInvoke();
    }

    /// <summary> FixedUpdate 이후 호출 </summary>
    //IEnumerator LateFixedUpdate()
    //{
    //    WaitForFixedUpdate _instruction = new WaitForFixedUpdate();
    //    while (true == IsPlaying)
    //    {
    //        yield return _instruction;
    //        //MoveJumpDivice();
    //    }
    //}

    void FixedUpdate()
    {
        if (null == mRBody)
        {
            ZLog.LogError(ZLogChannel.Temple, "Rigidbody가 셋팅되지 않았다!!!!!!");
            return;
        }
            

        Vector3 offsetPosition = mRBody.position - CachecdPosition;

        CachecdPosition = mRBody.position;

        if (mTriggerArea == null)
            return;

        var list = mTriggerArea.GetEnteredRigidbody();

        for (int i = 0; i < list.Count; i++)
        {
            list[i].MovePosition(list[i].position + offsetPosition);
        }
    }

    private void InvokePlay()
    {
        CancelInvoke(nameof(InvokePlay));
        Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
        float length = Gimmick.GetAnimLength(E_AnimStateName.Start_001);

        Invoke(nameof(InvokeAction), 0.10f/*length*/);

        if (IsResetAfterPlay || IsAutoPlay)
            Invoke(nameof(InvokeReset), length + ResetDelayTime);
    }

    private void InvokeAction()
    {
        CancelInvoke(nameof(InvokeAction));
        
        //점프!
        if(IsEnterMyPc)
        {
            var myPc = ZPawnManager.Instance.MyEntity;
            EntityComponentMovement_Temple movement = myPc.GetMovement<EntityComponentMovement_Temple>();
            if (movement.CurrentState is TempleCharacterControlState_Default defaultMovement)
            {
                defaultMovement.DoAddMomentum(Gimmick.transform.up * Power, 1f);
            }

            IsEnterMyPc = false;
        }

        foreach(var entity in m_listEntity)
        {
            var rBody = entity.GetComponent<Rigidbody>();

            if (null == rBody)
                continue;
            
            rBody.velocity = (Gimmick.transform.up * Power);
        }
    }

    private void InvokeReset()
    {
        CancelInvoke(nameof(InvokeReset));
        Gimmick.SetAnimParameter(E_AnimParameter.End_001);
        float length = Gimmick.GetAnimLength(E_AnimStateName.End_001);

        Invoke(nameof(InvokeReady), length);
    }

    private void InvokeReady()
    {
        CancelInvoke(nameof(InvokeReady));
        if (false == IsAutoPlay)
        {
            IsPlaying = false;
        }   
        else
        {
            Invoke(nameof(InvokePlay), AutoPlayDelayTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<EntityBase>();

        if (null == entity)
            return;

        if (true == entity.IsMyPc && true == other.isTrigger)
        {
            IsEnterMyPc = true;
        }
        else
        {
            if(false == m_listEntity.Contains(entity))
                m_listEntity.Add(entity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var entity = other.GetComponent<EntityBase>();

        if (null == entity)
            return;

        if (true == entity.IsMyPc && true == other.isTrigger)
        {
            IsEnterMyPc = false;
        }
        else
        {
            if (m_listEntity.Contains(entity))
                m_listEntity.Remove(entity);
        }
    }
}