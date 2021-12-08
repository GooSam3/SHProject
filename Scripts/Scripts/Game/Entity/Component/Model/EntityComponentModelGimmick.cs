using System;
using UnityEngine;
using GameDB;

/// <summary> Gimmick 모델 관련 처리 </summary>
public class EntityComponentModelGimmick : EntityComponentModelBase
{
    protected override void OnSetTable()
    {
        //소켓만 설정하자
        SetSocket();
    }

    protected override void OnPostSetModel()
    {
        // TODO :: 관련 처리 해야함
    }

    protected override void SetSocket()
    {
        foreach (E_ModelSocket socket in Enum.GetValues(typeof(E_ModelSocket)))
        {
            Transform socketTrans = Owner.gameObject.transform.FindTransform($"Socket_{socket}");

            if (null == socketTrans)
                continue;

            m_dicSocket[socket] = socketTrans;
        }
    }
}
