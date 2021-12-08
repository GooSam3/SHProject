public enum E_CameraUpdateType
{
    Update,
    LateUpdate,
    FixedUpdate,
}

public enum E_CameraMotorType
{
    /// <summary> ���� ī�޶� ��ġ, ȸ�� ���� �״�� �ƹ����۵� ���� �ʴ´�. </summary>
    Empty,
    /// <summary> �⺻ ��, �����¿� ȸ�� �� �� </summary>
    Free,
    /// <summary> ž �� </summary>
    Top,
    /// <summary> ���� �� </summary>
    Quarter,
    /// <summary> ��� ��, ĳ���Ͱ� �̵��ϴ� �������� �ڿ������� ī�޶� ȸ�� </summary>
    Shoulder,
    /// <summary> Ÿ���� ������ �� ĳ���� �ڿ��� Ÿ���� �ٶ󺻴�. ������ ���õ� �ٸ� Ÿ���� motor�� ����? </summary>
    LookTarget,
    /// <summary> 1��Ī ī�޶� </summary>
    Pov,
    /// <summary> ���� ī�޶� (��)</summary>
    North,
    /// <summary> ���� ī�޶� (��)</summary>
    East,
    /// <summary> ���� ī�޶� (��)</summary>
    South,
    /// <summary> ���� ī�޶� (��)</summary>
    West,
    /// <summary> ��忬��� </summary>
    ModeDirector,
    /// <summary> �� �� </summary>
    Back,
}

public enum E_RenderType
{
    InGameDefault = 0,
    InGameNoOutline = 1,
    IngameShrine = 2,
    InGameScreenSaver = 6,
}