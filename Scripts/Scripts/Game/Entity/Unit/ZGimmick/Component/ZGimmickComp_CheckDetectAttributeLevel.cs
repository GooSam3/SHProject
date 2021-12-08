using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZGimmickComp_CheckDetectAttributeLevel : ZGimmickComp_CheckBase
{
    private List<ZGA_DetectTrigger> m_listCachedDetect = new List<ZGA_DetectTrigger>();

    private int currentDetectCount = 0;
    protected override void InitailizeChecker()
    {
        m_listCachedDetect.Clear();

        foreach (var comp in GetComponentsInChildren<ZGA_DetectTrigger>())
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
        int detectCount = 0;
        foreach (var detect in m_listCachedDetect)
        {
            if (null == detect)
                continue;

            if (detect.IsDetect)
                detectCount++;
        }

        int levelUpgrade = detectCount - currentDetectCount;
        if (0 == levelUpgrade)
            return;

        foreach (var id in m_listEnableGimmickId)
        {
            ZTempleHelper.ChangeGimmickAttributeLevel(id, 0 < levelUpgrade ? true : false);
        }

        currentDetectCount = detectCount;
    }

    protected override bool CheckEnableAll()
    {
        return true;
    }
}
