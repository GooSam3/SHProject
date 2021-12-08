using System.Collections;
using UnityEngine;

/// <summary> 슬라이딩 퍼즐 </summary>
public class ZTempleMiniGame_SlidingPuzzle : ZTempleMiniGameBase
{
    public override E_TempleUIType ControlType => E_TempleUIType.Cancel;

    protected override void StartMiniGame()
    {
        //캐릭터 컨트롤러 변경
        ZPawnManager.Instance.MyEntity.ChangeController<EntityComponentController_MiniGame>();

        DestroyPuzzleTiles();

        //퍼즐 생성
        CreatePuzzleTiles();

        //퍼즐 섞기
        StartCoroutine(Co_JugglePuzzle());
    }

    protected override void EndMiniGame()
    {
        StopAllCoroutines();
        //캐릭터 컨트롤러 변경
        ZPawnManager.Instance.MyEntity.ChangeController<EntityComponentController>();
    }

    protected override bool CheckCompleteImpl()
    {
        return IsCompletePuzzle;
    }

    #region ===== :: 퍼즐!!!! :: =====
    [Header("퍼즐의 가로 사이즈")]
    [SerializeField]
    private int Width = 3;

    [Header("퍼즐의 세로 사이즈")]
    [SerializeField]
    private int Height = 3;    

    [Header("0 부터 Width 보다 작은 데이터 입력해야함")]
    [SerializeField]
    private int EmptyWidthIndex = 1;

    [Header("0 부터 Height 보다 작은 데이터 입력해야함")]
    [SerializeField]
    private int EmptyHeightIndex = 1;

    [SerializeField]
    private float TileSize = 10.0f;

    [SerializeField]
    private float SeperationBetweenTiles = 0.5f;

    [SerializeField]
    private GameObject Tile;
    
    private ZSlidingTileBase[,] TileDisplayArray;

    public bool IsCompletePuzzle { get; private set; } 

    public Vector3 GetTargetLocation(ZSlidingTileBase tile)
    {
        ZSlidingTileBase moveTo = CheckIfWeCanMove((int)tile.GridLocation.x, (int)tile.GridLocation.y, tile);

        if (moveTo != tile)
        {            
            Vector3 targetPos = moveTo.TargetPosition;
            Vector2 gridLocation = tile.GridLocation;
            tile.GridLocation = moveTo.GridLocation;
                        
            moveTo.UpdatePosition(tile.TargetPosition);
            moveTo.GridLocation = gridLocation;
                        
            return targetPos;
        }

        return tile.TargetPosition;
    }

    private ZSlidingTileBase CheckMoveLeft(int Xpos, int Ypos, ZSlidingTileBase tile)
    {        
        if ((Xpos - 1) >= 0)
        {            
            return GetTileAtThisGridLocation(Xpos - 1, Ypos, tile);
        }

        return tile;
    }

    private ZSlidingTileBase CheckMoveRight(int Xpos, int Ypos, ZSlidingTileBase tile)
    {        
        if ((Xpos + 1) < Width)
        {
            return GetTileAtThisGridLocation(Xpos + 1, Ypos, tile);
        }

        return tile;
    }

    private ZSlidingTileBase CheckMoveDown(int Xpos, int Ypos, ZSlidingTileBase tile)
    {        
        if ((Ypos - 1) >= 0)
        {
            return GetTileAtThisGridLocation(Xpos, Ypos - 1, tile);
        }

        return tile;
    }

    private ZSlidingTileBase CheckMoveUp(int Xpos, int Ypos, ZSlidingTileBase tile)
    {        
        if ((Ypos + 1) < Height)
        {
            return GetTileAtThisGridLocation(Xpos, Ypos + 1, tile);
        }

        return tile;
    }

    private ZSlidingTileBase CheckIfWeCanMove(int Xpos, int Ypos, ZSlidingTileBase tile)
    {
        if (CheckMoveLeft(Xpos, Ypos, tile) != tile)
        {
            return CheckMoveLeft(Xpos, Ypos, tile);
        }

        if (CheckMoveRight(Xpos, Ypos, tile) != tile)
        {
            return CheckMoveRight(Xpos, Ypos, tile);
        }

        if (CheckMoveDown(Xpos, Ypos, tile) != tile)
        {
            return CheckMoveDown(Xpos, Ypos, tile);
        }

        if (CheckMoveUp(Xpos, Ypos, tile) != tile)
        {
            return CheckMoveUp(Xpos, Ypos, tile);
        }

        return tile;
    }

    private ZSlidingTileBase GetTileAtThisGridLocation(int x, int y, ZSlidingTileBase tile)
    {
        for (int j = Height - 1; j >= 0; j--)
        {
            for (int i = 0; i < Width; i++)
            {                
                if ((TileDisplayArray[i, j].GridLocation.x == x) &&
                   (TileDisplayArray[i, j].GridLocation.y == y))
                {
                    if (true == TileDisplayArray[i, j].IsEmpty)
                    {
                        return TileDisplayArray[i, j];
                    }
                }
            }
        }

        return tile;
    }

    /// <summary> 완료됐는지 체크 </summary>
    private bool CheckForComplete()
    {
        bool bComplete = true;        
        for (int j = Height - 1; j >= 0; j--)
        {
            for (int i = 0; i < Width; i++)
            {
                if (TileDisplayArray[i, j].IsCorrectLocation == false)
                {
                    bComplete = false;
                }
            }
        }

        return bComplete;
    }

    /// <summary> 완료 체크 </summary>
    private IEnumerator Co_CheckForComplete()
    {
        while (IsCompletePuzzle == false)
        {            
            IsCompletePuzzle = CheckForComplete();
            yield return null;
        }

        CheckComplete();
    }

    /// <summary> 타일이 이동중인지 여부 </summary>
    public bool IsMovineTile()
    {
        for (int j = Height - 1; j >= 0; j--)
        {
            for (int i = 0; i < Width; i++)
            {
                if (false == TileDisplayArray[i, j].IsMoving)
                    continue;

                return true;
            }
        }
        return false;
    }

    /// <summary> 퍼즐 섞기 </summary>
    private IEnumerator Co_JugglePuzzle()
    {
        TileDisplayArray[EmptyWidthIndex, EmptyHeightIndex].SetEmpty(true);

        yield return new WaitForSeconds(1.0f);

        //한번 섞고 제대로 섞이지 않았다면 다시 섞음.
        do
        {
            int count = Random.Range(15, 30);
            for (int k = 0; k < count; k++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        if (false == TileDisplayArray[i, j].ExecuteAdditionalMove())
                            continue;

                        yield return new WaitForSeconds(0.02f);
                    }
                }
            }
        } while (CheckForComplete());
        

        StartCoroutine(Co_CheckForComplete());
    }

    /// <summary> 퍼즐 생성 </summary>
    private void CreatePuzzleTiles()
    {        
        TileDisplayArray = new ZSlidingTileBase[Width, Height];

        EmptyWidthIndex = Mathf.Max(Mathf.Min(EmptyWidthIndex, Width - 1), 0);
        EmptyHeightIndex = Mathf.Max(Mathf.Min(EmptyHeightIndex, Height - 1), 0);

        for (int j = Height - 1; j >= 0; j--)
        {
            for (int i = 0; i < Width; i++)
            {                
                var position = new Vector3((((i + 0.5f)) - ((Width / 2.0f))) * (TileSize + SeperationBetweenTiles),
                                       0.0f,
                                      (((j + 0.5f)) - ((Height / 2.0f))) * (TileSize + SeperationBetweenTiles));
                
                var go = Instantiate(Tile, transform) as GameObject;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;

                TileDisplayArray[i, j] = go.GetComponent<ZSlidingTileBase>();                

                ZSlidingTileBase tile = TileDisplayArray[i, j];
                tile.Initialize(this, new Vector2(i, j), position);    
            }
        }
    }

    private void DestroyPuzzleTiles()
    {
        if (null == TileDisplayArray)
            return;

        var tiles = GetComponentsInChildren<ZSlidingTileBase>();

        foreach(var tile in tiles)
        {
            GameObject.Destroy(tile.gameObject);            
        }
    }
    #endregion
}
