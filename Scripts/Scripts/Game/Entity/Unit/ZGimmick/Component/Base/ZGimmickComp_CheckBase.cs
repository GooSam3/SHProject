using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary> DetectAction을 체크한다 </summary>
public abstract class ZGimmickComp_CheckBase : MonoBehaviour
{
    [Header("활성화 완료시 발동할 기믹")]
    [SerializeField]
    protected List<string> m_listEnableGimmickId = new List<string>();

    [Header("발동시 적용할 속성 레벨")]
    [SerializeField]
    protected E_AttributeLevel AttributeLevel = E_AttributeLevel.Level_1;

    [Header("활성화/비활성화 토글할 건지 여부")]
    [SerializeField]
    protected bool IsToggle = false;

    [Header("활성화/비활성화 여부에 상관없이 강제로 처리할 경우")]
    [SerializeField]
    protected bool IsForce = false;

    [Header("활성화시 카메라 연출 (Optional)")]
    [SerializeField]
    protected CinemachineVirtualCamera VCamera;

    [Header("활성화시 카메라 연출 시간 (Optional)")]
    [SerializeField]
    protected float VCameraActiveDuration = 2f;

    [Header("플레이시 카메라 연출 블렌딩 시간 (Optional)")]
    [SerializeField]
    protected float VCameraBlendDuration = 0.5f;

    [Header("활성화시 카메라 연출 블렌딩 스타일 (Optional)")]
    [SerializeField]
    protected CinemachineBlendDefinition.Style VCameraBlendStyle = CinemachineBlendDefinition.Style.EaseIn;
        
    private bool IsEnableAll = false;

    private void Awake()
    {
        DisableVirtualCamera();
    }

    private IEnumerator Start()
    {
        //한 틱 뒤에 초기화
        yield return null;

        InitailizeChecker();
    }

    protected abstract void InitailizeChecker();

    /// <summary> 이벤트 제거 </summary>
    protected abstract void RemoveEvents();

    protected abstract bool CheckEnableAll();
    

    /// <summary> 활성화 체크 및 발동 </summary>
    protected void CheckAndAction()
    {
        bool bEnableAll = CheckEnableAll();

        //발동!
        if(true == bEnableAll)
        {            
            if(false == IsEnableAll)
            {
                foreach (var id in m_listEnableGimmickId)
                {
                    ZTempleHelper.EnableGimmicks(id, true, AttributeLevel, IsForce);
                }

                if (false == IsToggle)
                {
                    RemoveEvents();
                }

                SetVirtualCamera(true);
            }

            IsEnableAll = true;
        }
        else
        {
            if(true == IsEnableAll)
            {
                foreach (var id in m_listEnableGimmickId)
                {
                    ZTempleHelper.EnableGimmicks(id, false, AttributeLevel, IsForce);
                }
            }

            IsEnableAll = false;
        }
    }

    private void SetVirtualCamera(bool bActive)
    {
        if (null == VCamera)
        {
            return;
        }
            
        if(bActive)
        {
            CameraManager.Instance.DoSetBrainBlendStyle(VCameraBlendStyle, VCameraBlendDuration);
            Invoke(nameof(DisableVirtualCamera), VCameraActiveDuration);
        }

        VCamera.gameObject.SetActive(bActive);
    }

    private void DisableVirtualCamera()
    {
        SetVirtualCamera(false);
    }
}