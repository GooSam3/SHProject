using System.Collections.Generic;


// CObjectBase 
// ZeroGames : 정구삼 
// [개요] Mono가 아닌 일반 메모리 오브젝트를 표현한다. 
// 1) Struct가 아닌 객체는 GC에 부담을 주므로 별도로 관리할 필요가 있다.
// 2) 필요시 할당을 하고 사용한 객체는 반환을 받는 간단한 메모리 풀 구조이다. (메모리 단편화때문에 삭제하지 않는다)
// 3) 패킷 관리용 객체나 리소스 핸들등 런타임 할당 객체에 적합하다. 
// 4) [주의] 상속객체는 반환 전에 모든 레퍼런스를 초기화 해야 한다.(OnPoolObjectDeactivate 에 기술할것) 죽은 메모리 풀 때문에 GC가 안될 수도 있다.

abstract public class CObjectInstanceBase
{
    private int mInstanceID = 0;  protected int hInstanceID { get { return mInstanceID; } }
    
    public CObjectInstanceBase()
    {
        mInstanceID = GetHashCode();
    }
}

abstract public class CObjectMemoryPoolBase<TEMPLATE> : CObjectInstanceBase where TEMPLATE : CObjectMemoryPoolBase<TEMPLATE>, new()
{
    private static List<TEMPLATE> g_listPoolInstance = new List<TEMPLATE>();
    private bool   mInstanceActive = false; protected bool IsInstanceActive() { return mInstanceActive; }

    public static TEMPLATE InstanceActivate() 
    {
        TEMPLATE newObject = SearchObjectInstance();
        newObject.OnPoolObjectActivate();
        return newObject;
    }

    public static void InstanceDeactivate(TEMPLATE instance)
    {
        instance.mInstanceActive = false;
        instance.OnPoolObjectDeactivate();
    }

    //-------------------------------------------------------------------------------------------------------
    private static TEMPLATE SearchObjectInstance() 
    {
        TEMPLATE SearchInstance = null;

        for (int i = 0; i < g_listPoolInstance.Count; i++)
        {
            TEMPLATE template = g_listPoolInstance[i];
            if (template.IsInstanceActive() == false)
            {
                SearchInstance = g_listPoolInstance[i] as TEMPLATE;
                break;
            }
        }

        if (SearchInstance == null) 
        {
            SearchInstance = new TEMPLATE();
            g_listPoolInstance.Add(SearchInstance);
        }

        SearchInstance.mInstanceActive = true;
        return SearchInstance;
    }
    //---------------------------------------------------------------
    protected virtual void OnPoolObjectActivate() { }
    protected virtual void OnPoolObjectDeactivate() { }
}



