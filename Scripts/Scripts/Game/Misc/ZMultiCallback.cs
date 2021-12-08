using System;
using System.Collections.Generic;

public static class ZMultiCallback
{
    #region Variable
    public static List<MultiCallbackData> CallbackList = new List<MultiCallbackData>();
    #endregion

    public static void AddMultiCallback(MultiCallbackData data)
    {
        CallbackList.Add(data);
    }

    public static void CheckMultiCallback(string _name)
    {
        for(int i = 0; i < CallbackList.Count; i++)
            if(CallbackList[i].Name == _name)
            {
                CallbackList[i].Cnt -= 1;

                if(CallbackList[i].Cnt == 0)
                {
                    CallbackList[i].Cb();
                    CallbackList.Remove(CallbackList[i]);
                }
            }
    }
}

public class MultiCallbackData
{
    public Action Cb;
    public string Name;
    public int Cnt;

    public MultiCallbackData(string _name, int _cnt, Action _cb)
    {
        Cb = _cb;
        Name = _name;
        Cnt = _cnt;
    }
}