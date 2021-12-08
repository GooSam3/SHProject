using Cinemachine;
using GameDB;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ZNet.Data;

/// <summary> 사당 입구 </summary>
public class ZTempleEntrance : MonoBehaviour
{
	[Header("TempleTable의 ID")]
	[SerializeField]
	private uint TempleId;

	[Header("다른 이벤트(튜토리얼 등)에서 활성화됨")]
	[SerializeField]
	private bool IsEnableByEvent = false;

	private Temple_Table mTempleTable;
	private Stage_Table mStageTable;
	private TempleObject_Table mObjectTable;

	/// <summary> 스폰되어있는 사당 이펙트 </summary>
	private ZEffectComponent Fx_TempleState = null;

	/// <summary> 사당 입구의 현재 상태 </summary>
	private E_TempleEntranceState CurrentState = E_TempleEntranceState.Disable;

	private bool IsReEnter = false;

	/// <summary> 상호작용 ui </summary>
	private UISubHUDTemple SubHudTemple = null;

	/// <summary> 입구 위치 </summary>
	[Header("연출시 캐릭터가 이동할 위치")]
	[SerializeField]
	private Transform TargetPosition = null;

	[Header("입구 오픈/입장 상호작용시 사용할 카메라")]
	[SerializeField]
	private CinemachineVirtualCamera VirtualCamera = null;

	private bool IsEntering = false;

	private void Start()
	{
		if (null != VirtualCamera)
		{
			VirtualCamera.gameObject.SetActive(false);
		}

		if (false == ZGameManager.hasInstance)
		{
			return;
		}

		if (0 >= TempleId)
		{
			ZLog.LogError(ZLogChannel.Temple, "사당 입구의 TempleID가 셋팅되지 않았다.");
			gameObject.SetActive(false);
			return;
		}

		ZPawnManager.Instance.DoAddEventCreateMyEntity(HandleCreateMyEntity);
	}

	private void HandleCreateMyEntity()
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HandleCreateMyEntity);

		if (false == DBTemple.TryGet(TempleId, out mTempleTable))
		{
			ZLog.LogError(ZLogChannel.Temple, $"사당 입구의 TempleID({TempleId})가 없다.");
			gameObject.SetActive(false);
			return;
		}

		var collider = gameObject.GetComponent<SphereCollider>();

		if (null == collider)
		{
			collider = gameObject.AddComponent<SphereCollider>();
			collider.radius = 5f;
		}

		collider.isTrigger = true;

		//재입장 가능 여부
		IsReEnter = mTempleTable.Replay == E_Replay.Replay;

		ZGimmickManager.Instance.AddTempleEntrance(TempleId, this);

		if (null == TargetPosition)
		{
			TargetPosition = transform;
		}

		//유적 레이어 변경
		gameObject.SetLayersRecursively(UnityConstants.Layers.Entity);

		Refresh();

		//비활성상태일때는 이벤트 등록해서 갱신하자.
		if (CurrentState == E_TempleEntranceState.Disable)
		{
			Me.FindCurCharData?.TempleInfo.DoAddEventAddStage(HandleEventChangeTempleEntranceState);
		}
	}

	/// <summary> 유적 입구 상태 변경시 호출 (퀘스트 완료 응답에서만 사용하자) </summary>
	private void HandleEventChangeTempleEntranceState(TempleData data)
	{
		//Refresh();

		if (null == gameObject)
			return;

		if (mStageTable.StageID != data.StageTid)
			return;

		SetCurrentState();

		Me.FindCurCharData?.TempleInfo.DoRemoveEventAddStage(HandleEventChangeTempleEntranceState);
	}

	private void OnDestroy()
	{
		if (false == ZGimmickManager.hasInstance)
			return;

		ZGimmickManager.Instance.RemoveTempleEntrance(TempleId);

		ZGimmickManager.Instance.DoRemoveEventSpawnGimmick(HandelSpawnGimmick);
		Me.FindCurCharData?.TempleInfo.DoRemoveEventAddStage(HandleEventChangeTempleEntranceState);
	}

	/// <summary> 갱신 </summary>
	public void Refresh()
	{
		//사당 입구 이펙트 제거        
		mStageTable = DBStage.GetStageTableByTempleTid(TempleId);

		if (null == mStageTable)
		{
			ZLog.LogError(ZLogChannel.Temple, $"사당 입구의 TempleID({TempleId})와 매칭되는 StageId가 없다.");
			return;
		}
		//var templeInfo = Me.CurCharData.TempleInfo.GetStage(mStageTable.StageID);

		SetCurrentState();

		RegistOpenClearGimmick();
	}

	private void SetCurrentState()
	{
		CurrentState = E_TempleEntranceState.Disable;

		var templeInfo = Me.FindCurCharData?.TempleInfo?.GetStage(mStageTable.StageID);

		if (null != templeInfo)
		{
			if (0 >= templeInfo.ClearDts)
			{
				if (false == IsEnableByEvent)
					CurrentState = E_TempleEntranceState.Enable;
			}	
			else
				CurrentState = E_TempleEntranceState.Clear;
		}
		else
		{
			if (mStageTable.StageOpenType == E_StageOpenType.Always)
			{
				if(false == IsEnableByEvent)
					CurrentState = E_TempleEntranceState.Enable;
			}
				
		}

		uint effectTid = 0;
		switch (CurrentState)
		{
			case E_TempleEntranceState.Enable:
				effectTid = mTempleTable.ResourcePrefabFX1;
				break;
			case E_TempleEntranceState.Open:
				effectTid = mTempleTable.ResourcePrefabFX2;
				break;
			case E_TempleEntranceState.Clear:
				effectTid = mTempleTable.ResourcePrefabFX3;
				break;
			case E_TempleEntranceState.Disable:
				effectTid = mTempleTable.ResourcePrefabFX4;
				break;
		}

		ZEffectManager.Instance.SpawnEffect(effectTid, transform, -1, 1, (comp) =>
		{
			DestroyEffect();

			if (null == this)
				return;

			Fx_TempleState = comp;
		});
	}

	/// <summary> 기믹 등록 </summary>
	private void RegistOpenClearGimmick()
	{
		if (mStageTable.OpenClearGimmickID > 0)
		{
			if (DBTempleObject.TryGet(mStageTable.OpenClearGimmickID, out mObjectTable))
			{
				if (ZGimmickManager.Instance.TryGetValue(mObjectTable.FieldGimmickId, out var gimmicks))
				{
					//기믹을 찾았다면 노티 등록
					foreach (var gimmick in gimmicks)
					{
						SetGimmick(gimmick);
					}
				}
				else
				{
					//기믹 생성 대기
					ZGimmickManager.Instance.DoAddEventSpawnGimmick(HandelSpawnGimmick);
				}
			}
		}
	}

	/// <summary> 기믹이 소환(등록)되었을 경우 알림 </summary>
	private void HandelSpawnGimmick(string id, ZGimmick gimmick)
	{
		if (mObjectTable.FieldGimmickId != id)
		{
			return;
		}

		SetGimmick(gimmick);
	}

	private void SetGimmick(ZGimmick gimmick)
	{
		//기믹을 찾았다면 노티 등록
		var notify = gimmick.GetComponentInChildren<ZGA_NotifyForTempleEntrance>();

		if (null != notify)
		{
			notify.TempleTableId = mTempleTable.TempleID;
		}

		var openEntrance = gimmick.GetComponentInChildren<ZGA_OpenTempleEntrance>();

		if (null != openEntrance)
		{
			openEntrance.OpenTempleTableId = mTempleTable.TempleID;
		}

		var miniGame = gimmick.GetComponentInChildren<ZTempleMiniGameBase>();

		if (null != miniGame)
		{
			miniGame.OpenTempleTableId = mTempleTable.TempleID;
		}

		PostRegistGimmick();

		ZGimmickManager.Instance.DoRemoveEventSpawnGimmick(HandelSpawnGimmick);
	}

	/// <summary> 기믹 등록후 처리 </summary>
	private void PostRegistGimmick()
	{
		//이미 오픈되어있다면 기믹 비활성화(or 제거)하자
		if (CurrentState != E_TempleEntranceState.Disable)
		{
			if (ZGimmickManager.Instance.TryGetValue(mObjectTable.FieldGimmickId, out var gimmicks))
			{
				foreach (var gimmick in gimmicks)
				{
					gimmick.SetEnable(false, E_AttributeLevel.Level_1);

					if (mObjectTable.HideOnOpen > 0)
						gimmick.gameObject.SetActive(false);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (IsEntering)
			return;

		var myPc = other.gameObject.GetComponent<ZPawnMyPc>();

		if (null == myPc)
		{
			return;
		}

		ShowInteractionUI(() =>
		{
			HideInteractionUI();

			switch (CurrentState)
			{
				case E_TempleEntranceState.Disable:
					{
						DisableStateProccessByTrigger();
					}
					break;
				case E_TempleEntranceState.Enable:
					{
						// 활성화 연출
						StartCoroutine(Co_Open());
					}
					break;
				case E_TempleEntranceState.Open:
					{
						StartCoroutine(Co_Enter());
					}
					break;
				case E_TempleEntranceState.Clear:
					{
						if (false == IsReEnter)
						{
							//재입장 불가
							UICommon.SetNoticeMessage(DBLocale.GetText("Temple_Replay_None"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
						}
						else
						{
							StartCoroutine(Co_Enter());
						}

					}
					break;
			}
		});
	}

	private void OnTriggerExit(Collider other)
	{
		var myPc = other.gameObject.GetComponent<ZPawnMyPc>();

		if (null == myPc)
		{
			return;
		}
		//ui disable
		HideInteractionUI();
	}

	/// <summary> 비활성 상태일 경우 처리 </summary>
	private void DisableStateProccessByTrigger()
	{
		//가이드 노출 여부
		bool bShowGuide = true;

		//비활성 상태에서는 조건에 따라 처리하자.            
		switch (mStageTable.StageOpenType)
		{
			case E_StageOpenType.Quest:
				{
					//TODO :: 퀘스트 타입이면 퀘스트 완료 체크
					//mStageTable.ClearQuest
				}
				break;
			case E_StageOpenType.Item:
				{
					// 아이템 타입이면 아이템 체크
					var item = Me.CurCharData.GetInvenItemUsingMaterial(mStageTable.OpenUseItemID);

					if (null != item && item.cnt >= mStageTable.OpenUseItemCount)
					{
						bShowGuide = false;
						ZWebManager.Instance.WebGame.REQ_TempleItemOpen(mStageTable.OpenUseItemID, (packet, res) =>
						{
							StartCoroutine(Co_Enable());
						});
					}
				}
				break;
			case E_StageOpenType.Gimmick:
				{

					// 기믹 타입이면 체크안함!!
				}
				break;
			default:
				{
					ZLog.LogError(ZLogChannel.Temple, $"해당 스테이지({mStageTable.StageID}) 오픈 타입이 사당 관련 타입이 아니다. ({mStageTable.StageOpenType})");
				}
				break;
		}

		//가이드 노출
		if (bShowGuide == true)
		{
			if (string.IsNullOrEmpty(mTempleTable.GuideLocale) == false)
			{
				UICommon.SetNoticeMessage(DBLocale.GetText(mTempleTable.GuideLocale), Color.red, 1.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
			else
			{
				UICommon.SetNoticeMessage("사당 입장 가이드 필요", Color.red, 1.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
			}
		}
	}

	#region ===== :: 연출 :: =====
	/// <summary> 활성화 연출 </summary>
	private IEnumerator Co_Enable()
	{
		yield return Co_CommonDirect(E_TempleEntranceState.Enable, mTempleTable.ResourcePrefabFX1);
	}

	/// <summary> 오픈 연출 </summary>
	private IEnumerator Co_Open()
	{
		yield return Co_CommonDirect(E_TempleEntranceState.Open, mTempleTable.ResourcePrefabFX2);

		ShowInteractionUI(() =>
		{
			HideInteractionUI();
			//입장 연출
			StartCoroutine(Co_Enter());
		});
	}

	private IEnumerator Co_CommonDirect(E_TempleEntranceState nextState, uint effectId)
	{
		//이동 멈추고
		var myPc = ZPawnManager.Instance.MyEntity;
		myPc.StopMove(myPc.Position);
		myPc.IsBlockMoveMyPc = true;

		//연출 하고
		if (null != VirtualCamera)
		{
			VirtualCamera.gameObject.SetActive(true);
			CameraManager.Instance.DoSetBrainBlendStyle(CinemachineBlendDefinition.Style.EaseIn);
			yield return new WaitForSeconds(0.5f);
		}

		CameraManager.Instance.DoShake(myPc.Position, Vector3.up);

		DestroyEffect();

		ZEffectManager.Instance.SpawnEffect(effectId, transform, -1, 1, (comp) =>
		{
			Fx_TempleState = comp;
		});

		if (null != VirtualCamera)
		{
			yield return new WaitForSeconds(0.5f);
			VirtualCamera.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.5f);
		}

		//입장 UI 뛰우고
		CurrentState = nextState;

		myPc.IsBlockMoveMyPc = false;
	}

	/// <summary> 입장 연출 </summary>
	private IEnumerator Co_Enter()
	{
		IsEntering = true;

		//이동 멈추고
		var myPc = ZPawnManager.Instance.MyEntity;
		myPc.StopMove(myPc.Position);
		myPc.IsBlockMoveMyPc = true;

		if (null != VirtualCamera)
		{
			VirtualCamera.gameObject.SetActive(true);
			CameraManager.Instance.DoSetBrainBlendStyle(CinemachineBlendDefinition.Style.EaseIn);
			VirtualCamera.LookAt = myPc.transform;
		}

		//네비 메시 끄고
		var agent = myPc.GetComponent<NavMeshAgent>();
		if (null != agent)
		{
			agent.enabled = false;
		}
		//이동 시키고        

		Vector3 goalPos = TargetPosition.position;
		Vector3 forward = (goalPos - myPc.Position).normalized;// VectorHelper.XZForward(myPc.Position, goalPos);
		float moveSpeed = 3f;

		myPc.transform.forward = forward;
		myPc.MoveAnim(true);
		myPc.SetMoveSpeed(3);

		while (true)
		{
			myPc.transform.position += (forward * Time.smoothDeltaTime * moveSpeed);

			//var hits = Physics.RaycastAll(myPc.transform.position + Vector3.up * 0.5f, Vector3.down, 1f);
			//if(null != hits)
			//{
			//    Vector3 position = myPc.transform.position;
			//    float minMagnitude = 0.5f;
			//    foreach (var hit in hits)
			//    {
			//        if (hit.collider.isTrigger)
			//            continue;

			//        float magnitude = (myPc.transform.position - hit.point).magnitude;

			//        if(minMagnitude > magnitude)
			//        {
			//            position = hit.point;
			//            minMagnitude = magnitude;
			//        }                 
			//    }

			//    myPc.transform.position = position;
			//}

			if (VectorHelper.XZMagnitude(myPc.transform.position, goalPos) <= 0.1f)
			{
				break;
			}

			yield return null;
		}

		myPc.MoveAnim(false);

		//회전
		ZDirty dirty = new ZDirty(0.2f);
		var rotation = myPc.transform.rotation;
		var lerpRot = Quaternion.LookRotation(VectorHelper.XZForward(myPc.Position, CameraManager.Instance.Main.transform.position));

		dirty.GoalValue = 1f;
		dirty.IsDirty = true;

		while (dirty.Update())
		{
			myPc.transform.rotation = Quaternion.Lerp(rotation, lerpRot, dirty.CurrentValue);
		}

		yield return new WaitForSeconds(1f);

		CameraManager.Instance.DoShake(myPc.Position, Vector3.up);

		//Fx_Temple_Enter_Teleport 유적 입장 이펙트
		myPc.SpawnEffect(9000000);

		yield return new WaitForSeconds(0.2f);
		//모델 가리기        
		myPc.ModelGo.SetActive(false);

		//UI Fade out
		yield return new WaitForSeconds(1f);

		//bool bWait = true;
		//UICommon.FadeInOut(() =>
		//{
		//    bWait = false;
		//}, E_UIFadeType.FadeIn, 1f);

		//while (bWait)
		//{
		//    yield return null;
		//}

		//입장 처리
		ZGameManager.Instance.TryEnterStage(mTempleTable.EntrancePortalID, false, 0, 0);
	}
	#endregion

	/// <summary> 기믹이 동작했다고 알림 </summary>
	public void OnGimmickActionNotify()
	{
		//비활성 상태에서만 처리
		if (CurrentState != E_TempleEntranceState.Disable)
		{
			return;
		}

		if (mObjectTable == null)
		{
			return;
		}

		if (mObjectTable.NeedItemID > 0)
		{
			//아이템을 보유하고 있는지 체크해서 없다면 에러 메시지 출력            
			var item = Me.CurCharData.GetInvenItemUsingMaterial(mObjectTable.NeedItemID);

			if (null == item)
			{
				if (string.IsNullOrEmpty(mObjectTable.ErrorLocale) == false)
				{
					UICommon.SetNoticeMessage(DBLocale.GetText(mObjectTable.ErrorLocale), Color.red, 1.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				}
				else
				{
					UICommon.SetNoticeMessage("사당 기믹 사용 가이드 필요", Color.red, 1.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
				}
				ZLog.Log(ZLogChannel.Temple, "아이템이 없다!!!");
			}
			else
			{
				ZLog.Log(ZLogChannel.Temple, "아이템이 있다!!!");

				if (string.IsNullOrEmpty(mObjectTable.ItemPrefab))
				{
					//아이템 있을 경우 바로 오픈
					EnableTempleEntranceForGimmick();
				}
				else
				{
					//추가 기믹 소환
					ZPoolManager.Instance.Spawn(E_PoolType.Character, mObjectTable.ItemPrefab, (go) =>
					{
						go.transform.position = ZPawnManager.Instance.MyEntity.Position;
						go.transform.rotation = ZPawnManager.Instance.MyEntity.Rotation;
						go.transform.localScale = Vector3.one * mObjectTable.ItemScale * 0.01f;
					});
				}
			}
		}
		else
		{
			//그냥 활성화
			StartCoroutine(Co_Enable());
		}
	}

	/// <summary> 튜토리얼용 활성화 </summary>
	public void ForceOpenByTutorial(bool bImmediate = true)
	{
		if (CurrentState != E_TempleEntranceState.Disable)
			return;

		if (false == bImmediate)
		{						
			StartCoroutine(Co_Enable());
		}
		else
		{			
			CurrentState = E_TempleEntranceState.Enable;

			ZEffectManager.Instance.SpawnEffect(mTempleTable.ResourcePrefabFX1, transform, -1, 1, (comp) =>
			{
				DestroyEffect();

				if (null == this)
					return;

				Fx_TempleState = comp;
			});
		}
	}

	public void EnableTempleEntranceForGimmick()
	{
		if (false == ZWebManager.hasInstance)
			return;

		if (null == mObjectTable || null == mStageTable)
			return;

		//아이템 있을 경우 바로 오픈
		ZWebManager.Instance.WebGame.REQ_TempleGimmickOpen(mObjectTable.TempleObjectID, mStageTable.StageID, (packet, res) =>
		{
			StartCoroutine(Co_Enable());
		});
	}

	#region ===== :: UI :: =====
	private void ShowInteractionUI(Action onFinish)
	{
		if (null != SubHudTemple)
		{
			return;
		}

		SubHudTemple = UIManager.Instance.Find<UISubHUDTemple>();

		//준비된 상태가 아니라면 패스
		if (SubHudTemple?.IsReady == false)
			return;

		SubHudTemple?.SetInteractionGimmick(true, onFinish);
	}

	private void HideInteractionUI()
	{

		SubHudTemple?.SetInteractionGimmick(false);
		SubHudTemple = null;
	}
	#endregion

	private void DestroyEffect()
	{
		if (null != Fx_TempleState)
		{
			Fx_TempleState.Despawn();
		}

		Fx_TempleState = null;
	}
}
