using Cinemachine;
using UnityEngine;

/// <summary> 유저가 조작하는 패널 (EX : 자이로스코프 패널)</summary>
public class ZGA_ControlPanel : ZGimmickActionBase
{
    [Header("조작할 오브젝트")]
    public Rigidbody RBody;

    [Header("바라보는 카메라")]
    [SerializeField]
    private CinemachineVirtualCamera VirtualCamera;

    [Header("카메라 활성화 여부")]
    [SerializeField]
    private bool IsEnableVirtualCamera = true;
    
    /// <summary> 리셋시 회전 </summary>
    //private Quaternion ResetQuaternion;

    private ZDirty RotateDirty = new ZDirty(1f);

    [Header("회전 감도")]
    public float RotateSensitivity = 1f;

    [Header("X 축 회전 제어")]
    [Range(0f, 90f)]
    public float RotateLimitX = 90f;
    [Header("Z 축 회전 제어")]
    [Range(0f, 90f)]
    public float RotateLimitZ = 90f;

    private float InputX = 0f;
    private float InputZ = 0f;

    private void Start()
    {
        if(null != VirtualCamera)
        {
            VirtualCamera.gameObject.SetActive(false);
        }

        //ResetQuaternion = RBody.rotation;
    }
    protected override void InvokeImpl()
    {
        if(false == RBody)
        {
            RBody = GetComponent<Rigidbody>();
        }
        
        if(null == RBody)
        {
            ZLog.LogError(ZLogChannel.Temple, "조작할 오브젝트가 셋팅되지 않음");
            return;
        }

        RBody.freezeRotation = true;
        RBody.useGravity = false;
        RBody.isKinematic = true;

        ZTempleHelper.ChangeCharacterControlState(E_TempleCharacterControlState.ControllPanel, this);

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, FixedUpdateControlPanel);
    }

    protected override void DestroyImpl()
    {
        if (false == ZMonoManager.hasInstance)
            return;

        ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.FixedUpdate, FixedUpdateControlPanel);
    }

    protected override void CancelImpl()
    {
        ZTempleHelper.CancelCharacterControlState(E_TempleCharacterControlState.ControllPanel);
    }

    public void SetDir(float inputX, float inputZ)
    {
        InputX += (inputX * RotateSensitivity);
        InputZ += (inputZ * RotateSensitivity);

        InputX = Mathf.Clamp(InputX, -RotateLimitX, RotateLimitX);
        InputZ = Mathf.Clamp(InputZ, -RotateLimitZ, RotateLimitZ);
        //Quaternion rot = Quaternion.AngleAxis(1, dir.normalized);        
        //RBody.rotation = (RBody.rotation * rot);
    }

    private void FixedUpdateControlPanel()
    {
        RBody.MoveRotation(Quaternion.Euler(InputX, 0f, InputZ));
    }

    /// <summary> 리셋 연출 </summary>
    private void UpdateResetDirty()
    {
        //if (RotateDirty.Update())
        //{
        //    var newRotation = Quaternion.Slerp(RBody.rotation, ResetQuaternion, RotateDirty.CurrentValue);
        //    RBody.MoveRotation(newRotation);
        //}
        //else
        //{
        //    ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateResetDirty);
        //}   
    }

    /// <summary> 리셋 처리 </summary>
    public void ResetControlPanel()
    {
        //RotateDirty.CurrentValue = 0f;
        //RotateDirty.GoalValue = 1f;
        //RotateDirty.IsDirty = true;

        //InputX = 0f;
        //InputZ = 0f;

        //ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateResetDirty);
    }

    public void SetVirtualCamera(bool bActive, float blendDuration = 0.5f)
    {
        if (false == IsEnableVirtualCamera)
            return;

        CameraManager.Instance.DoSetBrainBlendStyle(CinemachineBlendDefinition.Style.EaseIn, blendDuration);
        
        if (null != VirtualCamera)
            VirtualCamera.gameObject.SetActive(bActive);
    }
}