using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 하나의 아이템의 실시간으로 변할 수 있는 구매 제한 정보를 담음.
/// </summary>
public class BuyLimitStateInfo
{
    public E_BuyLimitType buyLimitType;
    //public bool buyable;
    public uint limitBuyCnt;
    public uint curBuyCnt;

    public void Set(E_BuyLimitType buyLimitType, uint limitBuyCnt, uint curBuyCnt)
    {
        this.buyLimitType = buyLimitType;
        this.limitBuyCnt = limitBuyCnt;
        this.curBuyCnt = curBuyCnt;
    }

    public void Reset()
    {
        buyLimitType = E_BuyLimitType.Infinite;
        limitBuyCnt = 0;
        curBuyCnt = 0;
    }

    public void CopyTo(BuyLimitStateInfo dest)
    {
        dest.buyLimitType = this.buyLimitType;
        dest.limitBuyCnt = this.limitBuyCnt;
        dest.curBuyCnt = this.curBuyCnt;
    }

    // public bool HasLimit { get { return buyLimitType == E_BuyLimitType.} }
}