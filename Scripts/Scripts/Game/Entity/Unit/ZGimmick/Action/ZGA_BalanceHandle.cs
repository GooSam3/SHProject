using DG.Tweening;
using GameDB;
using GrassBending;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 저울 무게 체크 트리거 </summary>
public class ZGA_BalanceHandle : MonoBehaviour
{
    private ZGA_Balance ZGABalance;

    private E_Balance EBalance;

    public Material material;

    /// <summary>
    /// 저울에 올라와있는 정보들
    /// </summary>
    private List<ZGimmick> finalGimmicks = new List<ZGimmick>();

    /// <summary>
    /// 트리거에 들어온 정보들
    /// </summary>
    private List<ZGimmick> enterGimmicks = new List<ZGimmick>();

    public void SetBalanceHandle(ZGA_Balance parent, E_Balance mBalance)
    {
        ZGABalance = parent;
        EBalance = mBalance;
    }

    private void OnTriggerEnter(Collider other)
    {
        ZGimmick mGimmick = null;
        if (false == IsGimmick(other, out mGimmick))
            return;

        if (null != enterGimmicks.Find(d => d == mGimmick))
            return;

        enterGimmicks.Add(mGimmick);
    }

    private void OnTriggerExit(Collider other)
    {
        ZGimmick mGimmick = null;
        if (false == IsGimmick(other, out mGimmick))
            return;

        RemoveGimmick(mGimmick);
    }

    /// <summary>
    /// 사용하는 기믹 여부
    /// </summary>
    /// <param name="other"></param>
    /// <param name="mRigidbody"></param>
    /// <param name="mGimmick"></param>
    /// <returns></returns>
    private bool IsGimmick(Collider other, out ZGimmick mGimmick)
    {
        var mRigidbody = other.GetComponent<Rigidbody>();
        mGimmick = other.GetComponent<ZGimmick>();
        if (null == mGimmick)
		{
            var mActionBase = other.GetComponent<ZGimmickActionBase>();
            if(null == mActionBase)
			{
                return false;
			}
            else
			{
                mGimmick = mActionBase.Gimmick;
                if (null == mGimmick)
                    return false;
			}
		}

        // 물리효과 사용여부
        if (true == mRigidbody.isKinematic)
            return false;

        return true;
    }

    /// <summary>
    /// 추가
    /// </summary>
    /// <param name="target"></param>
    private void AddGimmick(ZGimmick target)
    {
        enterGimmicks.Remove(target);

        // 이미 있던 아이인지
        if (null != finalGimmicks.Find(d => d == target))
            return;

        finalGimmicks.Add(target);
        SetWeight();
    }

    /// <summary>
    /// 삭제
    /// </summary>
    /// <param name="target"></param>
    private void RemoveGimmick(ZGimmick target)
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

        ZGABalance.SetWeight(EBalance, weight);
    }

    public void AddForce()
    {
        foreach(var gimmick in finalGimmicks)
		{
            gimmick.GetComponent<Rigidbody>().AddForce(Vector3.up * 20f);
		}
    }

    Rigidbody findRigidBody = null;
    public void CheckEnterGimmickVelocity()
	{
        foreach(var gimmick in enterGimmicks)
		{
            findRigidBody = gimmick.GetComponent<Rigidbody>();
            if (findRigidBody.velocity.magnitude < 0.01f)
			{
                AddGimmick(gimmick);
                break;
            }
		}
	}
}