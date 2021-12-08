using DG.Tweening;
using UnityEngine;

/// <summary> 슬라이딩 퍼즐의 타일 </summary>
public abstract class ZSlidingTileBase : ZMiniGameObject
{    
    /// <summary> 목표 위치 </summary>
    public Vector3 TargetPosition { get; private set; }    
    /// <summary> 본래 위치에 있는지 여부 </summary>
    public bool IsCorrectLocation { get; private set; }

    /// <summary> 본래 위치 </summary>
    public Vector2 CorrectLocation = new Vector2();
    /// <summary> 현재 위치 </summary>
    public Vector2 GridLocation = new Vector2();

    public ZTempleMiniGame_SlidingPuzzle mOwner { get; private set; }

    /// <summary> 빈 타일 여부 </summary>
    public bool IsEmpty { get; private set; }

    /// <summary> 이동중인지 여부 </summary>
    public bool IsMoving { get; private set; }

    private Tweener TweenMove;
    
    public void Initialize(ZTempleMiniGame_SlidingPuzzle owner, Vector2 correctLocation, Vector3 launchPosition)
    {
        mOwner = owner;
        SetEmpty(false);
        CorrectLocation = correctLocation;
        GridLocation = correctLocation;        
        UpdatePosition(launchPosition);
        InitializeImpl();
    }

    protected abstract void InitializeImpl();

    public void SetEmpty(bool bEmpty)
    {
        IsEmpty = bEmpty;
        foreach(var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = !IsEmpty;
        }

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = !IsEmpty;
        }
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        TargetPosition = newPosition;
        UpdatePosition();
    }

    /// <summary> 이동 연출 </summary>
    private void UpdatePosition()
    {
        if (null != TweenMove)
            TweenMove.Kill(false);
        IsMoving = true;

        TweenMove = transform.DOLocalMove(TargetPosition, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            IsMoving = false;

            if (CorrectLocation == GridLocation)
            {
                IsCorrectLocation = true;
            }
            else
            {
                IsCorrectLocation = false;
            }
        });
    }

    /// <summary> 이동 가능한 위치로 이동한다. </summary>
    public bool ExecuteAdditionalMove(bool bCheckMoving = false)
    {
        if (true == bCheckMoving && mOwner.IsMovineTile())
            return false;

        //플레이중인지 체크
        if (false == mOwner.IsPlaying)
            return false;

        var position = mOwner.GetTargetLocation(this);
        if (TargetPosition == position)
            return false;

        UpdatePosition(position);

        return true; 
    }
}
