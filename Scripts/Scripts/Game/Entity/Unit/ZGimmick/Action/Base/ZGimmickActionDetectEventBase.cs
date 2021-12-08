using System;
public abstract class ZGimmickActionDetectEventBase : ZGimmickActionBase
{
    private Action mEventUpdate;

    private bool mIsDetect = false;
    public bool IsDetect {
        get { return mIsDetect; }
        protected set
        {
            if(mIsDetect != value)
            {
                mIsDetect = value;
                mEventUpdate?.Invoke();
            }
        }
    }

    public void DoAddEventUpdate(Action action)
    {
        DoRemoveEventUpdate(action);
        mEventUpdate += action;
    }

    public void DoRemoveEventUpdate(Action action)
    {
        mEventUpdate -= action;
    }
}
