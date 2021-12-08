public enum E_CameraUpdateType
{
    Update,
    LateUpdate,
    FixedUpdate,
}

public enum E_CameraMotorType
{
    /// <summary> 현재 카메라 위치, 회전 상태 그대로 아무동작도 하지 않는다. </summary>
    Empty,
    /// <summary> 기본 뷰, 상하좌우 회전 및 줌 </summary>
    Free,
    /// <summary> 탑 뷰 </summary>
    Top,
    /// <summary> 쿼터 뷰 </summary>
    Quarter,
    /// <summary> 숄더 뷰, 캐릭터가 이동하는 방향으로 자연스럽게 카메라도 회전 </summary>
    Shoulder,
    /// <summary> 타겟이 있으면 내 캐릭터 뒤에서 타겟을 바라본다. 없으면 셋팅된 다른 타입의 motor로 동작? </summary>
    LookTarget,
    /// <summary> 1인칭 카메라 </summary>
    Pov,
    /// <summary> 방향 카메라 (북)</summary>
    North,
    /// <summary> 방향 카메라 (동)</summary>
    East,
    /// <summary> 방향 카메라 (남)</summary>
    South,
    /// <summary> 방향 카메라 (서)</summary>
    West,
    /// <summary> 모드연출용 </summary>
    ModeDirector,
    /// <summary> 백 뷰 </summary>
    Back,
}

public enum E_RenderType
{
    InGameDefault = 0,
    InGameNoOutline = 1,
    IngameShrine = 2,
    InGameScreenSaver = 6,
}