
public class EntityStateEmpty : EntityStateBase
{
    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableUpdate  => false;

    /// <summary> 업데이트 활성화 여부 </summary>
    protected override bool EnableLateUpdate => false;
}