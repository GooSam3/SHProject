using UnityEngine;

#pragma warning disable CS0649
#pragma warning disable 649

// CObjectMonoBase 
// [개요] 유니티용 Mono 클레스 
// 1) 유니티 예약 함수를  Protected 가상 함수로 맵핑
// 2) 주로 컴포넌트에서 자주 쓰는 함수의 가상화로 유틸성을 증대하기 위한 목적이다.
// 3) SetActive는 브레이크 포인트가 걸리지 않아서 디버깅할때 불편함을 해소하기 위한 목적이다. 

abstract public class CMonoBase : MonoBehaviour
{
    private void Awake()        { OnUnityAwake();                           }
    private void Start()        { OnUnityStart();                           }
    private void OnEnable()     { OnUnityEnable();                          }
    private void OnDisable()    { OnUnityDisable();                         }
    private void OnDestroy()    { OnUnityDestroy();                         }

    public void SetMonoActive(bool Activate)
    {   
        gameObject.SetActive(Activate);  // 여기에 브레이크 포인트를 걸기 위해서이다.
    }

	public static void ResetGameObjectName(GameObject _GameObject)
	{
        _GameObject.name = _GameObject.name.Replace("(Clone)", "").Trim();
	}

	//----------------------------------------------------------
	protected Transform FindTransformRoot(Transform _Target)
    {
		Transform Result = _Target;
		if (Result.parent != null)
        {
			Result = FindTransformRoot(Result.parent);
        }
		return Result;
    }

	//--------------------------------------------------------
	protected virtual void OnUnityStart() { }
    protected virtual void OnUnityAwake() { }   
    protected virtual void OnUnityEnable() { }
    protected virtual void OnUnityDisable() { }
    protected virtual void OnUnityDestroy() { }
}


public static class ExtensionFunction
{
	public static T ToEnum<T>(this string value)
	{
		if (!System.Enum.IsDefined(typeof(T), value))
			return default(T);
		return (T)System.Enum.Parse(typeof(T), value, true);
	}

}

public struct SStageExportEnemy
{
    public uint EnmemyID;
    public uint EnemyLevel;
    public uint EnemyGaugeScale;
}

