using UnityEngine;
using System.Collections.Generic;

abstract public class CUGUIIconCheckBase : CUGUIIconBase
{
    public enum E_CheckGroup
	{
        Firest,
        Second,
        Thrid,
	}

    [System.Serializable]
    private class SCheckObject
	{
        public E_CheckGroup CheckGroup = E_CheckGroup.Firest;
        public GameObject Object = null;
	}

    [SerializeField] List<SCheckObject> CheckObject = new List<SCheckObject>();   

    private bool mCheck = false;  public bool pCheck { get { return mCheck; } }
    //--------------------------------------------------------
    public void DoUIIconCheck(bool _check, E_CheckGroup _checkGroup = E_CheckGroup.Firest)
	{
        mCheck = _check;
        PrivIconCheck(_check, _checkGroup);
        OnUIIconCheck(_check, _checkGroup);
    }

    //-------------------------------------------------------
    private void PrivIconCheck(bool _check, E_CheckGroup _checkGroup)
	{
        for (int i = 0; i < CheckObject.Count; i++)
		{
            if (CheckObject[i].CheckGroup == _checkGroup)
			{
                CheckObject[i].Object.SetActive(_check);
			}
		}
	}


    //-------------------------------------------------------
    protected virtual void OnUIIconCheck(bool _check, E_CheckGroup _checkGroup) { }

}
