// [개요] 하위 클래스에 Generic 패턴을 노출시키지 않고 간단히 상속할 수 있도록 만든 레이어 

public abstract class CManagerResourceBase : CManagerAddressableBase<CManagerResourceBase, CAddressableProviderObject>
{
	public static new CManagerResourceBase Instance { get { return CManagerAddressableBase<CManagerResourceBase, CAddressableProviderObject>.Instance as CManagerResourceBase; } }	
	//-----------------------------------------------------------------------------------


}
