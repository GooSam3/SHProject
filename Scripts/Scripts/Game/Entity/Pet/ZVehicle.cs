using GameDB;
using UnityEngine;

/// <summary> 펫 - 탈것 </summary>
public class ZVehicle : ZPetBase
{
    private Transform mTargetSocket = null;
    private Transform mOwnerModel = null;

    public bool IsLoadedModel { get { return ModelComponent?.ModelGo ?? false; } }

    /// <summary> 이동 컴포넌트 제거 </summary>
    protected override EntityComponentMovementBase OnSetMovementComponent()
    {
        return null;
    }

    protected override void OnPostInitializeImpl()
    {
        transform.parent = mOwnerCharacter.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one * ModelComponent.ModelScaleFactor;        
    }

    /// <summary> 모델 로드 완료시 </summary>
    protected override void OnLoadedModelImpl()
    {
        transform.localScale = Vector3.one * ModelComponent.ModelScaleFactor;

        // 탈것 소환 이펙트
        ZEffectManager.Instance.SpawnEffect(DBResource.Fx_Summon_Vehicle, mOwnerCharacter.Position, transform.rotation, 0f, 1f, null);
                
        mOwnerCharacter.DoAddEventLoadedModel(HandleLoadedOwnerModel);        
    }

    private void HandleLoadedOwnerModel()
    {
        AttachModel();
        mOwnerCharacter?.DoAddEventChangeAnimController(AttachModel);
    }

    protected override void OnDestroyImpl()
    {
        DetachModel();
        mOwnerCharacter?.DoRemoveEventChangeAnimController(AttachModel);
    }

    //private void FollowTarget()
    //{
    //    if (null == mOwnerModel || null == mTargetSocket)
    //        return;

    //    mOwnerModel.position = mTargetSocket.position;
    //}

    private void AttachModel()
    {
        if (null == mOwnerCharacter)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        mOwnerModel = mOwnerCharacter.ModelGo.transform;
        mTargetSocket = GetSocket(E_ModelSocket.Riding);

        mOwnerModel.parent = mTargetSocket;
        mOwnerModel.localPosition = Vector3.zero;
        mOwnerModel.localRotation = Quaternion.identity;

        mOwnerCharacter.SetAnimParameter(E_AnimParameter.Riding_001, true);

        mOwnerCharacter.DoAddEventChangeAnimParameter(HandleChangeAnimParameter);
        mOwnerCharacter.DoAddEventChangeMoveMotion(HandleChangeMove);
        MoveAnim(mOwnerCharacter.IsMoving());

        //스피트 셋팅
        SetMoveSpeed(mOwnerCharacter.MoveSpeed);

        Invoke(nameof(RepeatIdleAction), Random.Range(30f, 60f));

        //ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, FollowTarget);
    }

    private void DetachModel()
    {
        //if (ZMonoManager.hasInstance)
        //{
        //    ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, FollowTarget);
        //}

        if (null == mOwnerCharacter || null == mOwnerCharacter.gameObject)
            return;

        if (mOwnerCharacter.MyVehicle != null && mOwnerCharacter.MyVehicle != this)
            return;

        var modelGo = mOwnerCharacter.ModelGo;
        if (null == modelGo)
            return;

        var model = modelGo.transform;

        if(ZGameManager.hasInstance) {
            model.parent = mOwnerCharacter.transform;
        }

        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;

        mOwnerCharacter.SetAnimParameter(E_AnimParameter.Riding_001, false);

        mOwnerCharacter.DoRemoveEventChangeAnimParameter(HandleChangeAnimParameter);
        mOwnerCharacter.DoRemoveEventChangeMoveMotion(HandleChangeMove);
        mOwnerCharacter.DoRemoveEventLoadedModel(HandleLoadedOwnerModel);
                
        CancelInvoke(nameof(RepeatIdleAction));
    }

    private void HandleChangeAnimParameter(E_AnimParameter type, object value)
    {
        var parameterType = EntityAnimatorParameter.GetParameterType(type);

        switch(parameterType)
        {
            case AnimatorControllerParameterType.Bool:
                SetAnimParameter(type, (bool)value);
                break;
            case AnimatorControllerParameterType.Float:
                SetAnimParameter(type, (float)value);
                break;
            case AnimatorControllerParameterType.Int:
                SetAnimParameter(type, (int)value);
                break;
            case AnimatorControllerParameterType.Trigger:
                SetAnimParameter(type);
                break;
        }        
    }

    private void HandleChangeMove(bool bMove)
    {
        MoveAnim(bMove);
    }

    private void RepeatIdleAction()
    {
        CancelInvoke(nameof(RepeatIdleAction));

        Invoke(nameof(RepeatIdleAction), Random.Range(30f, 60f));

        SetAnimParameter(E_AnimParameter.Action_001);
    }
}
