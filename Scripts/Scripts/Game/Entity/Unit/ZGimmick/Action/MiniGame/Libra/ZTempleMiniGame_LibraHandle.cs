using System.Collections.Generic;
using UnityEngine;

/// <summary> 저울 무게 체크 트리거 </summary>
public class ZTempleMiniGame_LibraHandle : MonoBehaviour
{
    private ZTempleMiniGame_Libra ZMinigameLibra;

    private E_Balance EBalance;

    public Material material;

    /// <summary>
    /// 저울에 올라와있는 정보들
    /// </summary>
    private List<ZGimmick> finalGimmicks = new List<ZGimmick>();

    public void SetBalanceHandle(ZTempleMiniGame_Libra parent, E_Balance mBalance)
    {
        ZMinigameLibra = parent;
        EBalance = mBalance;
    }

    /// <summary>
    /// 추가
    /// </summary>
    /// <param name="target"></param>
    public void AddGimmick(ZGimmick target)
    {
        // 이미 있던 아이인지
        if (null != finalGimmicks.Find(d => d == target))
            return;

        Debug.Log("Coin Handle Add");
        finalGimmicks.Add(target);
        target.transform.parent = transform;
        target.transform.localPosition = Vector3.zero;
        SetWeight();
    }

    /// <summary>
    /// 삭제
    /// </summary>
    /// <param name="target"></param>
    public void RemoveGimmick(ZGimmick target)
    {
        // 있는지 검사
        if (null == finalGimmicks.Find(d => d == target))
            return;

        finalGimmicks.Remove(target);
        SetWeight();
    }

    /// <summary>
    /// 부모에게 무게 전달
    /// </summary>
    private void SetWeight()
    {
        float weight = 0;
        foreach (var gimmick in finalGimmicks)
            weight += gimmick.Weight;

        ZMinigameLibra.SetWeight(EBalance, weight);
    }
}