using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 무게 감지 스위치 </summary>
public class ZGA_DetectWeightSwitch : ZGimmickActionDetectEventBase
{
    [Header("트리거가 작동할 경우 활성화될 Gimmick들")]
    [SerializeField]
    private List<string> m_listEnableGimmickId = new List<string>();

    [Header("트리거가 작동할 경우 비활성화될 Gimmick들")]
    [SerializeField]
    private List<string> m_listDisableGimmickId = new List<string>();

    [Header("트리거가 작동할 경우 처리될 Gimmick들")]
    [SerializeField]
    private List<GimmickActionInvokeType> m_listDetectedGimmickAction = new List<GimmickActionInvokeType>();

    [Header("해당 기믹에 발동시킬 속성 레벨")]
    [SerializeField]
    private E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("트리거가 On/Off 될 때마다 연결된 기믹들을 다시 활성화/비활성화 시킨다. ")]
    [SerializeField]
    private bool IsToggle;

    [Header("체크할 무게!!!")]
    [SerializeField]
    private float GoalWeight = 10f;

    [Header("내 캐릭터의 무게만 체크할 경우 ")]
    [SerializeField]
    private bool IsMyPcOnly = false;

    [Header("다음 스위치 동작까지 대기 시간 ")]
    [SerializeField]
    private float ResetDelayTime = 0f;

    [Header("비활성화시 이펙트")]
    [SerializeField]
    private GameObject Fx_Off;

    [Header("활성화시 이펙트 ")]
    [SerializeField]
    private GameObject Fx_On;

    /// <summary> 트리거 안에 들어온 ZPawn, ZGimmick </summary>
    private List<EntityBase> m_listEnity = new List<EntityBase>();

    private bool IsDetected = false;

    private bool WeightDirty = false;

    private float DetectTime = 0f;

    protected override void InvokeImpl()
    {
        IsDetect = false;

        ZPawnManager.Instance.DoAddEventChangeWeight(HandleChangeWeight);
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateImpl);

        WeightDirty = true;
        ZTempleHelper.SetActiveFx(Fx_Off, true);
        ZTempleHelper.SetActiveFx(Fx_On, false);
    }

    protected override void DestroyImpl()
    {
        if (false == ZPawnManager.hasInstance)
            return;

        ZPawnManager.Instance.DoRemoveEventChangeWeight(HandleChangeWeight);
        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateImpl);
    }

    protected override void CancelImpl()
    {   
    }

    private void HandleChangeWeight(EntityBase entity, float preWeight, float newWeight)
    {
        if (false == m_listEnity.Contains(entity))
            return;

        GoalWeight -= preWeight;
        GoalWeight += preWeight;

        WeightDirty = true;
    }

    private void UpdateImpl()
    {
        if (true == WeightDirty)
        {
            WeightDirty = false;
            CheckWeight();
        }
    }

    private void CheckWeight()
    {        
        float weight = 0f;
        m_listEnity.RemoveAll((entity) =>
        {
            if(null == entity)
            {
                return true;
            }

            if(IsMyPcOnly)
            {
                if (entity is ZPawnMyPc)
                {
                    //캐릭터의 무게
                    weight += entity.Weight;
                }                
            }
            else
            {
                if (entity is ZPawn)
                {
                    //캐릭터의 무게
                    weight += entity.Weight;
                }
                else if (entity is ZGimmick gimmick)
                {
                    //기믹의 무게
                    weight += gimmick.Weight;
                }
            }

            return false;
        });

        InvokeSwitch(weight >= GoalWeight);
    }

    public void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<EntityBase>();

        if (null == entity)
            return;

        if (true == m_listEnity.Contains(entity))
            return;

        m_listEnity.Add(entity);
        WeightDirty = true;
    }

    public void OnTriggerExit(Collider other)
    {
        var entity = other.GetComponent<EntityBase>();

        if (null == entity)
            return;

        //내 pc는 collider 가 두개다!!!!!
        if (entity.IsMyPc && false == other.isTrigger)
            return;

        if (false == m_listEnity.Contains(entity))
            return;

        m_listEnity.Remove(entity);
        WeightDirty = true;
    }

    private void InvokeSwitch(bool bEnable)
    {
        //토글이 아니면 한번 적용된 이후부터는 패스
        if (true == IsDetect && false == IsToggle)
            return;
        
        if (IsDetected != bEnable)
        {
            //동작한 이후 다시 동작하기까지 대기 시간 처리
            if (DetectTime > Time.time)
            {
                CancelInvoke(nameof(CheckWeight));
                Invoke(nameof(CheckWeight), DetectTime - Time.time);
                return;
            }

            DetectTime = ResetDelayTime + Time.time;

            Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, bEnable ? 1f : -1f);
            Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
        }

        IsDetected = bEnable;

        ZTempleHelper.SetActiveFx(Fx_Off, false == IsDetected);
        ZTempleHelper.SetActiveFx(Fx_On, IsDetected);

        bool bChange = IsDetect != bEnable;

        if (bChange)
            EnableGimmick(bEnable);

        IsDetect = bEnable;
    }

    /// <summary> 연결된 기믹 활성화/비활성화 처리 </summary>
    private void EnableGimmick(bool bEnable)
    {
        foreach (var id in m_listEnableGimmickId)
        {
            ZTempleHelper.EnableGimmicks(id, bEnable, AttributeLevel, false);            
        }

        foreach (var id in m_listDisableGimmickId)
        {
            ZTempleHelper.EnableGimmicks(id, !bEnable, AttributeLevel, false);
        }

        foreach(var gimmick in m_listDetectedGimmickAction)
        {
            if(bEnable)
                gimmick.Invoke(AttributeLevel);
            else
                gimmick.Cancel();
        }
    }
}