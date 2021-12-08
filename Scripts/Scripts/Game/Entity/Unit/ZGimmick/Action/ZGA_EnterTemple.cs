using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

/// <summary> 사당 입장시 연출 </summary>
public class ZGA_EnterTemple : ZGimmickActionBase
{
    [Header("입장 연출")]
    [SerializeField]
    private PlayableDirector Director = null;

    [Header("연출 후 위치")]
    [SerializeField]
    private Transform TargetPosition = null;

    [Header("Dissolve 시간")]
    [SerializeField]
    private float DissolveFadeDuration = 0.3f;

    protected void OnDestroy()
    {
        //TODO :: 확산 속성 체크 초기화. 초기화할 위치 정해야할듯.
        if(ZGimmickManager.hasInstance)
        {
            ZGimmickManager.Instance.ClearSpreadAttribute();
        }
    }

	protected override void InitializeImpl()
	{
        Director.gameObject.SetActive(false);
    }

	protected override void InvokeImpl()
    {
        if (null == TargetPosition)
        {
            ZLog.LogError(ZLogChannel.Temple, "TargetPosition이 셋팅되지 않았다.");
            return;
        }

        //TODO :: 확산 속성 체크 초기화. 초기화할 위치 정해야할듯.
        ZGimmickManager.Instance.ClearSpreadAttribute();

        //내 캐릭터 기본 위치 저장.
        ZPawnManager.Instance.TempleCheckPosition = TargetPosition.position;
        ZPawnManager.Instance.TempleCheckRotation = TargetPosition.rotation;

        ZPawnManager.Instance.DoAddEventCreateMyEntity(OnCreateEntity);
    }

    protected override void CancelImpl()
    {

    }

    private void OnCreateEntity()
    {
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(OnCreateEntity);

        Director.stopped += HandleEventStopDirector;

        CameraManager.Instance.DoSetBrainBlendStyle(CinemachineBlendDefinition.Style.Cut);

        var templeMover = ZPawnManager.Instance.MyEntity.GetMovement<EntityComponentMovement_Temple>();
        templeMover.ChangeCharacterControlState(E_TempleCharacterControlState.Empty);

        ZPawnManager.Instance.MyEntity.DoAddEventLoadedModel(HandleEventLoadModel);
    }

    private void HandleEventLoadModel()
	{
        ZPawnManager.Instance.MyEntity.DoRemoveEventLoadedModel(HandleEventLoadModel);
        PlayDirector();
    }

    private void PlayDirector()
	{
        Director.gameObject.SetActive(true);
        Director.played += HandleEventPlayDirector;
        Director.stopped += HandleEventStopDirector;
        Director.Play();        
    }

    private void HandleEventPlayDirector(PlayableDirector director)
	{
        Director.played -= HandleEventPlayDirector;

        ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

        myPc.MoveAnim(false);
        myPc.Dissolve(true, 0);
        //myPc.ModelGo.SetActive(false);
        myPc.ModelGo.transform.localPosition = Vector3.one * 10000f;
        myPc.Warp(TargetPosition.position);
        myPc.transform.forward = TargetPosition.forward;

        Invoke(nameof(InvokePlayLanding), 0.3f);
    }

    private void InvokePlayLanding()
	{
        ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

        //myPc.ModelGo.SetActive(true);
        myPc.ModelGo.transform.localPosition = Vector3.zero;
        myPc.SetAnimParameter(E_AnimParameter.Landing_001);

        //디졸브 처리        
        myPc.Dissolve(false, DissolveFadeDuration);
        //myPc.MyVehicle?.Dissolve(false, DissolveFadeDuration);

        Invoke(nameof(InvokeEndLanding), 0.5f);
    }

    private void InvokeEndLanding()
	{
        ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

        myPc.Spasiticity(2.5f, 0f);
        //디졸브 연출 애니 멈춤
        myPc.PlayByNormalizeTime(E_AnimStateName.Landing_001, 0.5f);

        CameraManager.Instance.DoSetBrainBlendStyle(CinemachineBlendDefinition.Style.EaseIn);

    }

    private void HandleEventStopDirector(PlayableDirector director)
	{
        if (false == ZPawnManager.hasInstance)
            return;

        Director.stopped -= HandleEventStopDirector;

        Director.gameObject.SetActive(false);

        var hudTemple = UIManager.Instance.Find<UISubHUDTemple>();
        if (null != hudTemple)
        {
            hudTemple.ShowTempleTitle();
        }

        var templeMover = ZPawnManager.Instance.MyEntity.GetMovement<EntityComponentMovement_Temple>();
        templeMover.ChangeCharacterControlState(E_TempleCharacterControlState.Default);
    }
}