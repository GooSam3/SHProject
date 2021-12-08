/// <summary> 컨트롤러 </summary>
public abstract class EntityComponentControllerBase : EntityComponentBase<ZPawn>
{
    /// <summary> Normal Update 등록 </summary>
    protected override bool EnableUpdate => true;

    /// <summary> 내 PC </summary>
    protected ZPawnMyPc MyPc;

    protected override void OnInitializeComponentImpl()
    {
        base.OnInitializeComponentImpl();

        MyPc = Owner.To<ZPawnMyPc>();
    }
}
