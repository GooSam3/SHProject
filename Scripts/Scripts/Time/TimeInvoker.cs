using System.Collections.Generic;
using UnityEngine;
using Zero;

/// <summary>
/// [wisreal][2019.03.27]
/// 시간에 따른 invoke 관리자
/// </summary>

public class TimeInvoker : Singleton<TimeInvoker>
{
    public class InvokeInfo
    {
        public System.Action Func;
        public float WaitTime;
        public float RequestTime;
    }

    List<InvokeInfo> CallFuncList = new List<InvokeInfo>();

    public void RemoveFunction(System.Action InvokeFunc)
    {
        RemoveSameFunc(InvokeFunc);
    }

    public void RequestInvoke(System.Action InvokeFunc,float WaitTime)
    {
        if (WaitTime <= 0)
        {
            //ZLog.LogError("///////TimeInvoker////////// >> " + "RemainTime is Zero!!");
            return;
        }

        RemoveSameFunc(InvokeFunc);

        InvokeInfo NewInvokeInfo = new InvokeInfo();
        NewInvokeInfo.Func = InvokeFunc;
        NewInvokeInfo.WaitTime = WaitTime;
        NewInvokeInfo.RequestTime = Time.realtimeSinceStartup;

        //ZLog.Log("///////TimeInvoker////////// >> " + InvokeFunc.Method.Name + " - RequestInvoke : " + WaitTime);
        CallFuncList.Add(NewInvokeInfo);

        CheckLeastTimeCallFunc();
    }

    void RemoveSameFunc(System.Action InvokeFunc)
    {
        for (int i = 0; i < CallFuncList.Count; i++)
        {
            if (CallFuncList[i].Func == InvokeFunc)
            {
                //ZLog.Log("///////TimeInvoker////////// >> " + "RemoveSameFunc : "+ InvokeFunc.Method.Name);
                CallFuncList.RemoveAt(i);
                i--;
            }
        }
    }

    float CheckLeastTimeCallFunc()
    {
        float CallRemainTime = 10;
        //find least time
        for (int i = 0; i < CallFuncList.Count; i++)
        {
            if(CallRemainTime > CallFuncList[i].WaitTime - (Time.realtimeSinceStartup - CallFuncList[i].RequestTime))
                CallRemainTime = CallFuncList[i].WaitTime - (Time.realtimeSinceStartup - CallFuncList[i].RequestTime);
        }

        return CallRemainTime;
    }

    void CallFunc()
    {
        for (int i = 0; i < CallFuncList.Count; i++)
        {
            if (Time.realtimeSinceStartup - CallFuncList[i].RequestTime > CallFuncList[i].WaitTime)
            {
                //ZLog.Log("///////TimeInvoker////////// >> CallFunc "+ CallFuncList[i].Func.Method.Name);
                CallFuncList[i].Func?.Invoke();
                CallFuncList.RemoveAt(i);
                i--;
            }
        }
        CheckLeastTimeCallFunc();
    }

    public void ClearInvoke()
    {
        //ZLog.Log("///////TimeInvoker////////// >> ClearInvoke");
        CancelInvoke();
        CallFuncList?.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CancelInvoke();
        CallFuncList?.Clear();
    }

    float LastCheckTime = 0;
    private void LateUpdate()
    {
        if (Time.realtimeSinceStartup - LastCheckTime > 1f)
        {
            LastCheckTime = Time.realtimeSinceStartup;
            if (CheckLeastTimeCallFunc() <= 0)
            {
                CallFunc();
            }
        }
    }
}
