using UnityEngine;

/// <summary> 체크 포인트 </summary>
public class ZGA_CheckPoint : ZGimmickActionBase
{
    [Header("체크 포인트 도달시 저장할 위치(셋팅이 안되어있다면 현재 플레이어의 위치)")]
    [SerializeField]
    private Transform CheckPointPosition;

    [Header("해당 액션이 발동할때 위치를 저장할 경우 사용")]
    [SerializeField]
    private bool IsCheckPointByInvoke;

    /// <summary> 위치 저장을 했는지 여부 </summary>
    private bool IsSavePosition;

    protected override void InvokeImpl()
    {
        if(true == IsCheckPointByInvoke)
        {
            SavePosition();
        }
    }

    protected override void CancelImpl()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (false == IsInvoked)
            return;

        var pc = other.GetComponent<ZPawnMyPc>();

        if (null == pc)
            return;

        SavePosition();
    }

    private void SavePosition()
    {
        if (true == IsSavePosition)
            return;

        if (null != CheckPointPosition)
        {
            ZPawnManager.Instance.TempleCheckPosition = CheckPointPosition.position;
            ZPawnManager.Instance.TempleCheckRotation = CheckPointPosition.rotation;
        }
        else
        {
            ZPawnManager.Instance.TempleCheckPosition = ZPawnManager.Instance.MyEntity.Position;
            ZPawnManager.Instance.TempleCheckRotation = ZPawnManager.Instance.MyEntity.Rotation;
        }

        IsSavePosition = true;
    }
}