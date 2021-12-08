using UnityEngine;

/// <summary> 슬라이딩 퍼즐의 타일 (방향성을 가지고 있다) </summary>
public class ZSlidingTile_Direction : ZSlidingTileBase
{
    [Header("안을 바라보는 경우")]
    [SerializeField]
    private bool IsInside = false;

    [Header("타일 모델")]
    [SerializeField]
    private GameObject TileModel;

    protected override void InitializeImpl()
    {
        Vector3 forward = TargetPosition.normalized;
        forward = IsInside ? -forward : forward;

        TileModel.transform.localRotation = Quaternion.LookRotation(forward);
    }
}
