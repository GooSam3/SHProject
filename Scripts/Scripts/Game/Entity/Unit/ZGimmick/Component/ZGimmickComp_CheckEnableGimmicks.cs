using System.Collections.Generic;
using UnityEngine;

/// <summary> 활성화된 기믹을 체크한다 </summary>
public class ZGimmickComp_CheckEnableGimmicks : ZGimmickComp_CheckBase
{
    [Header("활성화 체크할 기믹 id")]
    [SerializeField]
    private List<string> m_listCheckGimmickId = new List<string>();

    private List<ZGimmick> m_listCachedGimmick = new List<ZGimmick>();

    // Start 하면 여기 먼저 들어옴
    protected override void InitailizeChecker()
    {
        m_listCachedGimmick.Clear();
        foreach (var id in m_listCheckGimmickId)
        {
            if (false == ZGimmickManager.Instance.TryGetValue(id, out var gimmicks))
                continue;

            foreach(var gimmick in gimmicks)
            {
                m_listCachedGimmick.Add(gimmick);
                gimmick.DoAddEventActionInvoke(HandleActionInvoke);
            }
        }
    }

    /// <summary> 이벤트 제거 </summary>
    protected override void RemoveEvents()
    {
        foreach (var gimmick in m_listCachedGimmick)
        {
            if (null == gimmick)
                continue;

            gimmick.DoRemoveEventActionInvoke(HandleActionInvoke);
        }
    }

    private void HandleActionInvoke(ZGimmick gimmick, E_GimmickActionInvokeType type)
    {
        if(type == E_GimmickActionInvokeType.Enable || type == E_GimmickActionInvokeType.Disable)
            CheckAndAction();
    }

    protected override bool CheckEnableAll()
    {
        foreach (var gimmick in m_listCachedGimmick)
        {
            if (null == gimmick)
                continue;

            if (true == gimmick.IsEnabled)
                continue;

            return false;
        }

        return true;
    }
}