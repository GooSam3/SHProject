using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary> 캐릭터 컨트롤러 </summary>
public class EntityComponentController : EntityComponentControllerBase
{
    /// <summary> 플레이어가 터치할 수 있는 Layer만 모아놓은 마스크값. </summary>
    static protected int HitCheckLayerMask = UnityConstants.Layers.OnlyIncluding(UnityConstants.Layers.Floor, UnityConstants.Layers.Entity, UnityConstants.Layers.Player, UnityConstants.Layers.Gimmick);

    private List<int> m_listCurTouchId = new List<int>();
    private bool mIsEnableZoomTouchZero = false;
    private bool mIsEnableZoomTouchOne = false;

    private Touch[] mTempTouches = null;
    private Vector2 mLastestTouchPosVec2;

    private bool mIsTouchStarted = false;
    private bool mIsTouchStarted2 = false;
    private bool mIsDragging = false;
    private bool mIsTouchEnded = false;

    private Vector3 mLastestTouchPos;

    private bool mIsMoveMode = false;
    private float mLatestMovedTime = 0;

    protected override void OnUpdateImpl()
    {
        if(Owner == null)
        {
            return;
        }
        
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // 모든 터치에 대해서 검사해줘야한다.
            if (Input.touchCount > 0)
            {    //터치가 1개 이상이면.
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var tempTouch = Input.GetTouch(i);
                    if (tempTouch.phase == TouchPhase.Began)
                    {
                        // 첫 인풋시작이 UI인풋이라면 무시
                        if (UIHelper.IsPointerOverGameObject(ref tempTouch))
                            continue;

                        if(false == m_listCurTouchId.Contains(tempTouch.fingerId))
                            m_listCurTouchId.Add(tempTouch.fingerId);
                    }                    
                }
            }
            else if(0 < m_listCurTouchId.Count)
            {
                m_listCurTouchId.Clear();

                mIsTouchStarted = false;
                mIsTouchStarted2 = false;
                mIsDragging = false;
                mIsTouchEnded = false;
                mIsMoveMode = false;
            }

            UpdateMobileInput();
        }
        else
        {
            // 첫 인풋시작이 UI인풋이라면 무시
            if (UIHelper.IsPointerOverGameObject() && !mIsTouchStarted)
                return;

            UpdateInput();
        }

#if UNITY_EDITOR
        //임시 공격 버튼 및 스킬 사용
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MyPc.UseNormalAttack();
            return;
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            Owner.SetTarget(null);
            return;
        }
#endif
    }

    private bool UpdateMobileZoom()
    {
        bool ret = false;

        if (m_listCurTouchId.Count >= 2)
        {
            List<Touch> touchs = new List<Touch>();


            for (int i = 0; i < mTempTouches.Length; ++i)
            {
                if (!m_listCurTouchId.Contains(mTempTouches[i].fingerId))
                    continue;

                touchs.Add(mTempTouches[i]);
            }
            
            if(2 <= touchs.Count)
            {
                mIsEnableZoomTouchZero = m_listCurTouchId.Contains(touchs[0].fingerId);
                mIsEnableZoomTouchOne = m_listCurTouchId.Contains(touchs[1].fingerId);

                if (mIsEnableZoomTouchZero && mIsEnableZoomTouchOne)
                {
                    Vector2 touchZeroPrevPos = touchs[0].position - touchs[0].deltaPosition;
                    Vector2 touchOnePrevPos = touchs[1].position - touchs[1].deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchs[0].position - touchs[1].position).magnitude;

                    CameraManager.Instance.DoUpdateMobileZoom((touchDeltaMag - prevTouchDeltaMag) * Time.deltaTime * 0.25f);

                    ret = true;
                }
            }
        }

        if (ret)
        {
            mIsTouchStarted = false;
            mIsTouchStarted2 = false;
            mIsDragging = false;
            mIsTouchEnded = false;
            mIsMoveMode = false;
        }

        return ret;
    }

    private void UpdateMobileInput()
    {
        mTempTouches = Input.touches;

        if (UpdateMobileZoom())
            return;

        foreach (var touch in mTempTouches)   
        {
            if (!m_listCurTouchId.Contains(touch.fingerId))
                continue;

            // TouchDown상태지만, 이동모드가 아니라면
            if (mIsTouchStarted && !mIsMoveMode)
            {
                Vector2 offset = touch.position - mLastestTouchPosVec2;
                float moveDelta = Vector2.SqrMagnitude(offset);
                if (false == mIsDragging)
                    mIsDragging = moveDelta > 100f;

                if (mIsDragging)
                {
                    mLastestTouchPosVec2 = touch.position;
                    CameraManager.Instance.DoUpdateMobileDrag(offset, true);
                }
            }

            // Touch Down
            if (touch.phase == TouchPhase.Began)
            {
                mIsTouchStarted = true;
                mIsTouchStarted2 = true;
                mIsDragging = false;
                mLastestTouchPosVec2 = touch.position;
            }

            //TouchUp
            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                mIsTouchStarted = false;
                mIsTouchEnded = true;

                mIsMoveMode = true;
            }

            // 1. [이동 모드 ON] or TouchUp
            // 2. 이동 모드 상태 유지시, 일정 시간마다 입력 처리되도록함.
            if (false == mIsDragging)
            {
                if (mIsTouchStarted2 && mIsMoveMode && Time.realtimeSinceStartup - mLatestMovedTime > 0.4f)
                {
                    if (mIsTouchEnded)
                    {
                        // 드래그 끝날때, 선택된 Pawn이 없으면 이동기능 작동하자.
                        ProcessInput(touch.position);
                    }
                    else
                    {
                        // 누른상태의 클릭 효과
                        ProcessInput(touch.position, true);
                    }

                    mLatestMovedTime = Time.realtimeSinceStartup;
                }
            }

            if (mIsTouchEnded)
            {
                mIsTouchStarted = false;
                mIsTouchStarted2 = false;
                mIsDragging = false;
                mIsTouchEnded = false;
                mIsMoveMode = false;

                m_listCurTouchId.Remove(touch.fingerId);
            }
        }
    }

    private void UpdateInput()
    {
        // TouchDown상태지만, 이동모드가 아니라면
        if (mIsTouchStarted && !mIsMoveMode)
        {
            float moveDelta = Vector3.SqrMagnitude(Input.mousePosition - mLastestTouchPos);
            mIsDragging = moveDelta > 0.1f;

            if (mIsDragging)
            {
                mLastestTouchPos = Input.mousePosition;
            }
        }

        //TouchDown 시작
        if (Input.GetMouseButtonDown(0))
        {
            mIsTouchStarted = true;
            mIsTouchStarted2 = true;
            mIsDragging = false;
            mLastestTouchPos = Input.mousePosition;
        }

        //TouchUp
        if (Input.GetMouseButtonUp(0))
        {
            mIsTouchStarted = false;
            mIsTouchEnded = true;

            mIsMoveMode = true;
        }

        // 1. [이동 모드 ON] or TouchUp
        // 2. 이동 모드 상태 유지시, 일정 시간마다 입력 처리되도록함.
        if (mIsTouchStarted2 && mIsMoveMode && Time.realtimeSinceStartup - mLatestMovedTime > 0.4f)
        {
            if (mIsTouchEnded)
            {                
                ProcessInput(Input.mousePosition);
            }
            else
            {
                // 누른상태의 클릭 효과
                ProcessInput(Input.mousePosition, true);
            }

            mLatestMovedTime = Time.realtimeSinceStartup;
        }

        if (mIsTouchEnded)
        {
            mIsTouchStarted = false;
            mIsTouchStarted2 = false;
            mIsDragging = false;
            mIsTouchEnded = false;
            mIsMoveMode = false;
        }
    }

    private void ProcessInput(Vector2 inputPosition, bool bOnlyMove = false)
    {
        if (ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.GodLand) {
            return;
        }

        Ray ray = CameraManager.Instance.Main.ScreenPointToRay(inputPosition);
        RaycastHit[] inputHitResults = new RaycastHit[15];

        int amountOfHits = Physics.RaycastNonAlloc(ray.origin, ray.direction, inputHitResults, 1000f, HitCheckLayerMask);
        if (amountOfHits == 0)
            return;

        // RaycastNonAlloc에서는 순서 보장하지 않아서 정렬필요
        if (amountOfHits > 1)
            System.Array.Sort(inputHitResults, 0, amountOfHits, PhysicsHelper.RaycastHitDistanceComparer.Default);

        for (int hitIdx = 0; hitIdx < amountOfHits; ++hitIdx)
        {
            var hit = inputHitResults[hitIdx];
            int hitLayer = hit.transform.gameObject.layer;
            
            if (bOnlyMove == false && (hitLayer == UnityConstants.Layers.Entity || hitLayer == UnityConstants.Layers.Player || hitLayer == UnityConstants.Layers.Gimmick))
            {
                var entity = hit.transform.GetComponent<EntityBase>();

                if (entity == null)
                    continue;

                // 나 자신은 선택 Skip해놨음.
                if (entity.IsMyPc)
                    continue;

                // 콜로세움은 우리팀 클릭 skip
                if( ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Colosseum ) {
                    var myEntity = ZPawnManager.Instance.MyEntity;
                    if( myEntity != null ) {
                        if( myEntity.EntityData.IsEnemy( entity.EntityData ) == false ) {
                            continue;
                        }
                    }
                }

                bool doEndLoop = true;

                switch (entity.EntityType)
                {
                    case GameDB.E_UnitType.NPC:
                        {
                            //entity.GetComponent<Zero.IInteractable>()?.Interact(Owner);
                            MyPc.InteractionToNpc(entity);
                            MyPc.SetTarget(entity);
                        }
                        break;
                    case GameDB.E_UnitType.Gimmick:
                        {
                            ZGimmick gimmick = entity.To<ZGimmick>();

                            if (gimmick.IsTargetable && false == hit.collider.isTrigger)
                            {
                                MyPc.SetTarget(entity);
                            }
                            else
                            {
                                doEndLoop = false;
                            }
                        }
                        break;
                    case GameDB.E_UnitType.Summon:
                        {
                            doEndLoop = false;
                        }
                        break;
                    case GameDB.E_UnitType.Object:
                        {
                            //오브젝트 타겟팅 후 상호작용                            
                            MyPc.ObjectGathering(entity);
                        }
                        break;
                    default:
                        {
                            var type = MyPc.GetAttackTargetType(entity);
                            ZLog.Log(ZLogChannel.UI, "## Target Type : " + type);

                            if (true == entity.IsDead)
                            {
                                doEndLoop = false;
                            }
                            else
                            {
                                switch (type)
                                {
                                    case E_EntityAttackTargetType.None:
                                        {
                                            MyPc.SetTarget(entity);
                                        }
                                        break;
                                    case E_EntityAttackTargetType.MainTarget:
                                        {
                                            if (MyPc.IsAttacking)
                                            {
                                                return;
                                            }

                                            MyPc.UseNormalAttack();
                                        }
                                        break;
                                    case E_EntityAttackTargetType.SecondTarget:
                                        {
                                            MyPc.DoTargetSwitch();
                                            //MyPc.UseNormalAttack();
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                }

                if (doEndLoop)
                    break;
            }
            else
            {
                if (NavMesh.SamplePosition(hit.point, out var navMeshHit, 10f, NavMesh.AllAreas))
                {
                    MyPc.SetTarget(null);
                    MyPc.MoveToInputPosition(navMeshHit.position, Owner.MoveSpeed);
                    ZEffectManager.Instance.SpawnEffect(DBResource.Fx_TouchGround, navMeshHit.position, Quaternion.identity, 0f, 1f, null);
                    break;
                }                      
            }
        }
    }
}
