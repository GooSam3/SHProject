using UnityEngine;

// CManagerBase 
// [개요] 유니티용 싱글톤 전역 클래스 랩퍼
// 1)  GlobalManagerScriptLoaded : 모든 테이블등이 로드된 이후 한번 만 호출해 준다. 각 메니저는 로드이후 데이터를 구성한다. 
// 2)  하위 인스턴스에서 Instance를 변수 오버라이드 해서 사용한다. 
//     public static new [인스턴스 이름] Instance { get { return g_Instance as [인스턴스 이름]; } }

abstract public class CManagerBase : CMonoBase
{
    [SerializeField]
    private bool DontDestroy = true;
    private static bool g_ScriptLoaded = false;
    
    public static void GlobalManagerScriptLoaded()  
    {
        if (g_ScriptLoaded) return;
        g_ScriptLoaded = true;

        CManagerBase[] aManager = FindObjectsOfType<CManagerBase>();
        for (int i = 0; i < aManager.Length; i++)
        {
            aManager[i].OnManagerScriptLoaded();
        }
    }

    protected override void OnUnityAwake()
    {
        if (DontDestroy)
        {
            Transform RootTransform = FindTransformRoot(gameObject.transform);
            DontDestroyOnLoad(RootTransform.gameObject);
        }
    }
	private void Update()
	{
        OnUnitUpdate();
    }

	//-------------------------------------------------------
	protected virtual void OnManagerScriptLoaded() { }
    protected virtual void OnUnitUpdate() { }
}

abstract public class CManagerTemplateBase<TEMPLATE> : CManagerBase where TEMPLATE : class
{
    private static CManagerTemplateBase<TEMPLATE> StaticInstance = null;

    public CManagerTemplateBase()
    {
        StaticInstance = this;
    }

    public static TEMPLATE Instance
    {
        get
        {
            return StaticInstance as TEMPLATE;
        }
    }
}