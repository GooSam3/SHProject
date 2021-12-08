using GameDB;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 최초 얼린상태 , 불타는상태 관련 
/// </summary>
public class ZGA_AwakeAttribute : ZGimmickActionBase
{
	[Header("최초시작 속성")]
    [SerializeField]
	public E_UnitAttributeType AwakeAttributeType = E_UnitAttributeType.None;

	private List<Collider> colliders = new List<Collider>();
	private List<ZGimmickActionBase> actionBases = new List<ZGimmickActionBase>();

	public System.Action AwakeAttributeCompleteAction;

	private bool _isUseAwakeAttribute = false;

	protected override void InitializeImpl()
	{
		if (null == Gimmick)
			return;

		_isUseAwakeAttribute = AwakeAttributeType != E_UnitAttributeType.None;
		if (false == _isUseAwakeAttribute)
		{
			gameObject.SetActive(false);
			return;
		}

		Gimmick.IsTargetable = true;
		Gimmick.IsUseAwakeAttribute = true;
		IsAwakeAttribute = true;
		Gimmick.SetAttributeMaterialColor(AwakeAttributeType);
	}

	protected override void InvokeImpl()
	{
		if (Gimmick.IsUseAwakeAttribute == false)
			return;

		bool isComplete = false;
		switch(AwakeAttributeType)
		{
			case E_UnitAttributeType.Fire:
				{
					isComplete = InvokeType == E_GimmickActionInvokeType.Attribute_Water;
						
				}
				break;
			case E_UnitAttributeType.Water:
				{
					isComplete = InvokeType == E_GimmickActionInvokeType.Attribute_Fire;
				}
				break;
			default: break;
		}

		if (false == isComplete)
			return;

		Gimmick.IsUseAwakeAttribute = false;
		if (null != AwakeAttributeCompleteAction)
			AwakeAttributeCompleteAction.Invoke();

		Gimmick.SetAttributeMaterialColor(E_UnitAttributeType.None);
		enabled = false;
		gameObject.SetActive(false);
    }

	protected override void CancelImpl()
	{
	}
}
