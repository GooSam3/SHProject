using System.Collections.Generic;

/// <summary> DetectAction을 체크한다 </summary>
public class ZGimmickComp_CheckDetectBase<DETECT_TYPE> : ZGimmickComp_CheckBase where DETECT_TYPE : ZGimmickActionDetectEventBase
{
    private List<DETECT_TYPE> m_listCachedDetect = new List<DETECT_TYPE>();

    protected override void InitailizeChecker()
    {
        m_listCachedDetect.Clear();

        foreach (var comp in GetComponentsInChildren<DETECT_TYPE>())
        {
            m_listCachedDetect.Add(comp);
            //이벤트 추가
            comp.DoAddEventUpdate(HandleUpdate);
        }
    }

    /// <summary> 이벤트 제거 </summary>
    protected override void RemoveEvents()
    {
        foreach (var detect in m_listCachedDetect)
        {
            if (null == detect)
                continue;

            detect.DoRemoveEventUpdate(HandleUpdate);
        }
    }
    private void HandleUpdate()
    {
        CheckAndAction();            
    }

    protected override bool CheckEnableAll()
    {
        foreach (var detect in m_listCachedDetect)
        {
            if (null == detect)
                continue;

            if (true == detect.IsDetect)
                continue;

            return false;
        }

        return true;
    }
}