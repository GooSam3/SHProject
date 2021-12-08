using System.Collections.Generic;
using UnityEngine;

/// <summary> 캐릭터 컨트롤러 </summary>
public class EntityComponentController_MiniGame : EntityComponentControllerBase
{
    /// <summary> 플레이어가 터치할 수 있는 Layer만 모아놓은 마스크값. </summary>
    static protected int HitCheckLayerMask = UnityConstants.Layers.OnlyIncluding(UnityConstants.Layers.MiniGameObject);

    private List<Touch> m_listCurTouch = new List<Touch>();

    protected override void OnUpdateImpl()
    {
        if (Owner == null)
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

                        m_listCurTouch.Add(tempTouch);
                    }
                }
            }

            UpdateMobileInput();
        }
        else
        {
            // 첫 인풋시작이 UI인풋이라면 무시
            if (UIHelper.IsPointerOverGameObject())
                return;

            UpdateInput();
        }
    }

    private void UpdateMobileInput()
    {
        m_listCurTouch.RemoveAll((touch) =>
        {
            ProcessInput(touch.position);
            return true;
        });
    }

    private void UpdateInput()
    {
        //TouchDown 시작
        if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
    }

    private void ProcessInput(Vector2 inputPosition)
    {
        Ray ray = CameraManager.Instance.Main.ScreenPointToRay(inputPosition);
        RaycastHit[] inputHitResults = new RaycastHit[5];
        int amountOfHits = Physics.RaycastNonAlloc(ray.origin, ray.direction, inputHitResults, 1000f, HitCheckLayerMask);
        if (amountOfHits == 0)
            return;

        // RaycastNonAlloc에서는 순서 보장하지 않아서 정렬필요
        if (amountOfHits > 1)
            System.Array.Sort(inputHitResults, 0, amountOfHits, PhysicsHelper.RaycastHitDistanceComparer.Default);

        for (int hitIdx = 0; hitIdx < amountOfHits; ++hitIdx)
        {
            var hit = inputHitResults[hitIdx];
            
            var tile = hit.transform.GetComponent<ZSlidingTileBase>();

            if (null == tile)
                continue;

            tile.ExecuteAdditionalMove(true);
            break;
        }
    }
}
