using DG.Tweening;
using GameDB;
using UnityEngine;

/// <summary> 기울기패널 / 그저 기울여서 '공' 을 굴리는 용도이므로 Ani 사용 안함 </summary>
public class ZGA_EulerPanel : ZGimmickActionBase
{
    [Header("기울여지는 각도")]
    [SerializeField] private float Euler;

    [Header("기울여지는 Speed")]
    [SerializeField] private float Speed;

	protected override void InvokeImpl()
    {
        transform.DORotateQuaternion(Quaternion.Euler(Euler, 0, 0), Speed);
    }

    protected override void CancelImpl()
    {
        transform.DORotateQuaternion(Quaternion.Euler(0, 0, 0), Speed);
    }
}