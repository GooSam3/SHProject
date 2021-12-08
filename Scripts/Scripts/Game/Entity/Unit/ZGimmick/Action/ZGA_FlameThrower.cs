using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZGTemple;

/// <summary> 화염 방사기 </summary>
public class ZGA_FlameThrower : ZGimmickActionBase
{
    [Header("사용할 이펙트 - Level_1 = 0 ~ Max = 7")]
    public GameObjectDictionary AttributeEffectDic;

    [Header("방사기의 속성")]
    [SerializeField]
    private E_UnitAttributeType AttributeType = E_UnitAttributeType.Fire;

    [Header("플레이어가 피해를 입을 경우 발생할 액션")]
    [SerializeField]
    private E_TemplePresetAction PresetAction = E_TemplePresetAction.WarpCheckPoint;

    [Header("벽같은 곳에 부딪쳤을때 발생할 이펙트")]
    [SerializeField]
    private GameObject FxElementalBlockHit;

    [Header("멈출때 나올 이펙트")]
    [SerializeField]
    private GameObject FxStopElemental;

    [Header("방사기 충돌체")]
    [SerializeField]
    private BoxCollider Collider;

    [Header("속성 공격을 받았을 때 멈추는 시간")]
    [SerializeField]
    private float StopDuration = 10f;

    [Header("충돌을 무시할 Collider (자기 자신의 자식들은 제외)")]
    [SerializeField]
    private List<Collider> IgnoreCollider = new List<Collider>();

    /// <summary> 방사기가 활성화 중인지 여부 </summary>
    private bool IsEnableFlameThrower;

    private float DefaultHeight;
    private Vector3 DefaultCenter;
    private float CurrentHeight;
    private Vector3 CurrentCenter;
    private ZGimmick mHitGimmick = null;

    protected static int HitCheckLayerMask = UnityConstants.Layers.OnlyIncluding(UnityConstants.Layers.Floor, UnityConstants.Layers.Entity, UnityConstants.Layers.Player, UnityConstants.Layers.Gimmick, UnityConstants.Layers.Default, UnityConstants.Layers.IgnoreCollision);

    protected override void InitializeImpl()
    {
        ZTempleHelper.SetActiveFx(GetAttributeEffect(), false);
        ZTempleHelper.SetActiveFx(FxElementalBlockHit, false);
        ZTempleHelper.SetActiveFx(FxStopElemental, false);

        if (null != Collider)
        {
            DefaultCenter = Collider.center;
            CurrentCenter = Collider.center;
            DefaultHeight = Collider.size.z;
            CurrentHeight = Collider.size.z;
        }

        IgnoreCollider.AddRange(Gimmick.GetComponentsInChildren<Collider>());
        AttributeEffectReset();
    }

    /// <summary>
    /// 속성 , 속성레벨 별 이펙트 ActiveFalse
    /// </summary>
    private void AttributeEffectReset()
	{
        foreach(var key in AttributeEffectDic.Keys)
		{
            for(int index = 0; index < AttributeEffectDic[key].Length; index++)
			{
                var obj = (GameObject)AttributeEffectDic[key].GetValue(index);
                if (null == obj)
                    continue;

                obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 속성 , 속성레벨 별 이펙트 가져오기
    /// </summary>
    /// <returns></returns>
    private GameObject GetAttributeEffect()
	{
        if (false == AttributeEffectDic.ContainsKey(AttributeType))
            return null;

        int levelIndex = (int)InvokeAttributeLevel;
        if (AttributeEffectDic[AttributeType].Length < levelIndex)
            return null;

        var obj = (GameObject)AttributeEffectDic[AttributeType].GetValue(levelIndex - 1);
        if (null == obj)
            return null;

        return obj;
    }

    protected override void InvokeImpl()
    {
        //이미 활성화중이다.
        if (true == IsEnableFlameThrower)
        {
            return;
        }

        ZTempleHelper.SetActiveFx(GetAttributeEffect(), true);
        ZTempleHelper.SetActiveFx(FxStopElemental, false);

        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, OnUpdate);
        IsEnableFlameThrower = true;
    }

    protected override void CancelImpl()
    {
        if (ZMonoManager.hasInstance)
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, OnUpdate);
        IsEnableFlameThrower = false;
    }

    protected override void DestroyImpl()
    {
        CancelImpl();
    }

    private void OnUpdate()
    {
        if (false == IsEnableFlameThrower)
            return;

        var hits = Physics.BoxCastAll(transform.position, transform.lossyScale, transform.forward, transform.rotation, Collider.size.z, HitCheckLayerMask);

        bool bHitBlock = false;
        Vector3 hitPoint = Vector3.zero;
        ZGimmick hitGimmick = null;

        CurrentHeight = DefaultHeight;

        foreach (var hit in hits)
        {
            if (IgnoreCollider.Contains(hit.collider))
                continue;

            ZPawn pawn = hit.collider.gameObject.GetComponent<ZPawn>();

            if (null != pawn)
            {
                continue;
            }

            if (hit.collider.isTrigger)
            {
                continue;
            }

            float distance = (transform.position - hit.point).magnitude;

            if (DefaultHeight > distance && CurrentHeight > distance)
            {
                CurrentHeight = distance;
                hitPoint = hit.point;
                bHitBlock = true;
                CurrentCenter.z = CurrentHeight * 0.5f;

                hitGimmick = hit.collider.GetComponentInParent<ZGimmick>();
            }
        }

        if (bHitBlock)
        {
            transform.localScale = new Vector3(1f, 1f, CurrentHeight / DefaultHeight);
            if (null != FxElementalBlockHit)
            {
                FxElementalBlockHit.transform.position = hitPoint;

                if (false == FxElementalBlockHit.activeSelf)
                    ZTempleHelper.SetActiveFx(FxElementalBlockHit, true);
            }

            if (null != hitGimmick && (true == hitGimmick.IsTakeAttributeLoop || hitGimmick != mHitGimmick))
                hitGimmick.TakeAttribute(AttributeType, InvokeAttributeLevel);

            mHitGimmick = hitGimmick;
        }
        else
        {
            mHitGimmick = null;
            CurrentHeight = DefaultHeight;
            CurrentCenter = DefaultCenter;

            transform.localScale = new Vector3(1f, 1f, 1f);
            ZTempleHelper.SetActiveFx(FxElementalBlockHit, false);
        }
    }

    protected override void TakeAttribute(ZGimmick gimmick, E_UnitAttributeType type)
    {
        CancelInvoke(nameof(InvokeImpl));

        if (E_UnitAttributeType.Water != type)
            return;

        ZTempleHelper.SetActiveFx(FxStopElemental, true);
        ZTempleHelper.SetActiveFx(GetAttributeEffect(), false);
        ZTempleHelper.SetActiveFx(FxElementalBlockHit, false);

        IsEnableFlameThrower = false;

        if (0 < StopDuration)
        {
            Invoke(nameof(InvokeImpl), StopDuration);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (false == IsEnableFlameThrower)
            return;

        ZPawnMyPc pawn = other.gameObject.GetComponent<ZPawnMyPc>();

        if (null == pawn)
            return;

        ZTempleHelper.PlayPresetAction(PresetAction);
    }
}