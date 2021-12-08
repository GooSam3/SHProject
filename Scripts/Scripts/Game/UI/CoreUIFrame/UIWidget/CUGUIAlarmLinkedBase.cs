using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 알람은 포인트 마커 + 알람 카운터로 구성 

abstract public class CUGUIAlarmLinkedBase : CUGUIWidgetBase
{
    public enum E_AlarmLevel
    {
        None,           //  알람 없음
        Always,         //  항상 알람 발생 
        UpLevel_1,      //  위로 1단계만 알람 발생 
        UpLevel_2,
        UpLevel_3,
        UpLevel_4,
        UpLevel_5,
    }
    [SerializeField] CUGUIAlarmLinkedBase LinkedParent = null;
    [SerializeField] GameObject AlarmObject = null;
    [SerializeField] E_AlarmLevel AlarmLevel = E_AlarmLevel.Always;
    private int mAlarmCount = 0;  public int pAlarmCount { get { return mAlarmCount; } } 
    private List<CUGUIAlarmLinkedBase> m_listLinkedButtonChild = new List<CUGUIAlarmLinkedBase>();
	//------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
        if (AlarmObject != null)
		{
            AlarmObject.SetActive(false);
		}
    }

	protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitializePost(_UIFrameParent);
	}

	//----------------------------------------------------------------------
	public void DoAlarmActionAdd()
	{
        PrivAlarmActionAdd((int)E_AlarmLevel.Always);
	}

    public void DoAlarmActionDelete()
	{
        PrivAlarmActionDelete((int)E_AlarmLevel.Always);
    }

    public void DoAlarmActionAdd(E_AlarmLevel _alarmLevel)
    {
        PrivAlarmActionAdd((int)_alarmLevel);
    }

    public void DoAlarmActionDelete(E_AlarmLevel _alarmLevel)
    {
        PrivAlarmActionDelete((int)_alarmLevel);
    }

    public void DoAlarmActionAdd(int _alarmCount)
    {
        PrivAlarmActionAdd(_alarmCount);
    }

    public void DoAlarmActionDelete(int _alarmCount)
    {
        PrivAlarmActionDelete(_alarmCount);
    }

    //------------------------------------------------------------------------
    private void ImportRecursiveAlarmAdd(int _alarmLevel)
	{
        if (mAlarmCount == 0)
		{
            mAlarmCount++;
            PrivAlarmOnOff(true);
        }


        if (_alarmLevel < 0 && _alarmLevel != 0)
		{
            if (LinkedParent != null)
            {
                LinkedParent.ImportRecursiveAlarmAdd(_alarmLevel--);
            }
        }
    }

    private void ImportRecursiveAlramDelete(int _alarmLevel)
	{     
        if (_alarmLevel <= 0 && _alarmLevel != 0)
		{
            if (LinkedParent != null)
			{
                LinkedParent.ImportRecursiveAlramDelete(_alarmLevel--);
			}
		}

        mAlarmCount--;
        if (mAlarmCount < 0)
            mAlarmCount = 0;

        if (mAlarmCount == 0)
            PrivAlarmOnOff(false);
    }

    //----------------------------------------------------------------------

    private void PrivAlarmActionAdd(int _alarmCount)
    {
        PrivAlarmOnOff(true);

        if (LinkedParent != null)
        {
            LinkedParent.ImportRecursiveAlarmAdd(_alarmCount);
        }
    }

    private void PrivAlarmActionDelete(int _alarmCount)
    {
        PrivAlarmOnOff(false);

        if (LinkedParent != null)
        {
            LinkedParent.ImportRecursiveAlramDelete(_alarmCount);
        }
    }



    private void PrivAlarmOnOff(bool _on)
	{
        if (AlarmObject != null)
		{
            AlarmObject.SetActive(_on);
		}
        OnAlarmOnOff(_on, mAlarmCount);
    }

    //-------------------------------------------------------------------
    protected virtual void OnAlarmOnOff(bool _on, int _alarmCount) { }
}
