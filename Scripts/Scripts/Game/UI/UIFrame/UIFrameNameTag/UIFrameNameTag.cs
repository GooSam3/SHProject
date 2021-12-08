using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class UIFrameNameTag : CUIFrameNameTagBase
{
	[System.Serializable]
	private class SNameTagRange
	{
		public float SquareRange = 1000;
		public float UpdateDelay = 0.5f;
	}

	[SerializeField] private E_ModelSocket FollowSocket = E_ModelSocket.Head;
	[SerializeField] private E_ModelSocket RaycastSocket = E_ModelSocket.Hit;
	[SerializeField] private List<SNameTagRange> TagRange = new List<SNameTagRange>();

	private ZPawnMyPc mMyPC = null;
	//--------------------------------------------------------------
	protected sealed override SUIFrameInfo Infomation()
    {
        SUIFrameInfo uiFrameInfo = new SUIFrameInfo();
        uiFrameInfo.ID = GetType().ToString();
        uiFrameInfo.LocalizingKey = (int)E_UILocalizing.Korean;
        uiFrameInfo.ThemaType = (int)E_UIThemaType.Default;
        return uiFrameInfo;
    }

	protected override void OnInitialize()
	{
		base.OnInitialize();
		ZPawnManager.Instance.DoAddEventCreateEntity(HandleEntityEnter);
		ZPawnManager.Instance.DoAddEventRemoveEntity(HandleEntityExit);
		ZPawnManager.Instance.DoAddEventDieEntity(HandleEntityDie);
		CameraManager.Instance.DoAddEventCameraUpdated(HandleCameraUpdate);
	}

	protected override void OnRemove()
	{
		base.OnRemove();

		DoUINameTagClearAll();

		if (ZPawnManager.hasInstance)
		{
			ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleEntityEnter);
			ZPawnManager.Instance.DoRemoveEventRemoveEntity(HandleEntityExit);
			ZPawnManager.Instance.DoRemoveEventDieEntity(HandleEntityDie);

			if (mMyPC != null)
			{
				mMyPC.DoRemoveEventChangeTarget(HandleEntryChangeTarget);
			}
		}

		if (CameraManager.hasInstance)
		{
			CameraManager.Instance.DoAddEventCameraUpdated(HandleCameraUpdate);
		}
	}

	protected override void OnNameTagRemove(CUIWidgetNameTagBase _nameTag) 
	{
		base.OnNameTagRemove(_nameTag);
		DeleteUIWidget(_nameTag);
		ZPoolManager.Instance.Return(_nameTag.gameObject);
	}

	protected override void OnNameTagDistance(CUIWidgetNameTagBase _nameTag, float _distance)
	{
		base.OnNameTagDistance(_nameTag, _distance);
		_nameTag.SetNameTagChangeFrequecy(ExtractNameTagFrameFrequency(_distance));
	}

	//----------------------------------------------------------------
	public void DoUINameTagClearAll()
	{
		mMyPC = null;
		NameTagClearAll();
		ZPoolManager.Instance.ClearCategory(E_PoolType.UI_NameTag);
	}

	public void DoUINameTagFocus(ZPawn _focusPawn, bool _focusOn)
	{
		CUIWidgetNameTagBase nameTag = FindNameTag(_focusPawn.gameObject);
		if (nameTag != null)
		{
			nameTag.DoUIWidgetFocus(_focusOn);
		}
		else
		{
			MakeNameTag(_focusPawn);
		}
	}

	public void DoUINameTagActive(ZPawn _pawn, bool _bActive)
	{
		CUIWidgetNameTagBase nameTag = FindNameTag(_pawn.gameObject);
		if (nameTag != null)
		{
			nameTag.DoNameTagActive(_bActive);
		}
	}

	public void DoUINameTagDropItem(DropItemComponent _followObject, Item_Table _itemTable)
	{
		_followObject.OnRemove = HandleDropItemRemove;
		ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagDropItem), (UINameTagDropItem _newInstance) =>
		{
			if (_followObject == null)
			{
				ZPoolManager.Instance.Return(_newInstance.gameObject);
			}

			_newInstance.DoNameTagInitialize(_followObject, _itemTable);
			NameTagAdd(_followObject.gameObject, _newInstance);
		});
	}

	public void DoUINameTagRefreshQuestTarget()
	{
		List<CUIWidgetNameTagBase> listNameTag = NameTagList();
		for (int i = 0; i < listNameTag.Count; i++)
		{
			UINameTagBase nameTag = listNameTag[i] as UINameTagBase;
			nameTag.DoNameTagRefreshTarget();
		}
	}

	public void DoUINameTagChattingMessage(ZPawn _pawn, string _message, Color _color, float _duration)
	{
		CUIWidgetNameTagBase nameTag = FindNameTag(_pawn.gameObject);
		if (nameTag != null && nameTag is UINameTagBase uiNameTag)
		{
			uiNameTag.SetNameTagChattingMessage(_message, _color, _duration);
		}
	}

	public void DoUINameTagChattingMessageHide(ZPawn _pawn)
	{
		CUIWidgetNameTagBase nameTag = FindNameTag(_pawn.gameObject);
		if (nameTag != null && nameTag is UINameTagBase uiNameTag)
		{
			uiNameTag.HideNameTagChattingMessage();
		}
	}

	//----------------------------------------------------------------	
	private void MakeNameTag(ZPawn _followTarget)
	{
		if (_followTarget == null) return;

		E_UnitType uinitType = _followTarget.EntityType;
		switch (uinitType)
		{
			case E_UnitType.Character:
				if (_followTarget.IsMyPc)
				{
					mMyPC = _followTarget as ZPawnMyPc;
					mMyPC.DoAddEventChangeTarget(HandleEntryChangeTarget);
					ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagMyPlayer), (UINameTagMyPlayer _newInstance) => {
						HandleLoadInstance(_newInstance, _followTarget);
					});
				}
				else
				{
					if (_followTarget.IsCustomConditionControl(E_CustomConditionControl.PartyMember))
					{
						ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagPartyPlayer), (UINameTagPlayer _newInstance) => {
							HandleLoadInstance(_newInstance, _followTarget);
						});
					}
					else
					{
						ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagPlayer), (UINameTagPlayer _newInstance) => {
							HandleLoadInstance(_newInstance, _followTarget);
						});
					}
				}
				break;

			case E_UnitType.Gimmick:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagGimmick), (UINameTagGimmick _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;

			case E_UnitType.Monster:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagMonster), (UINameTagMonster _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;

			case E_UnitType.NPC:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagNPC), (UINameTagNPC _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;

			case E_UnitType.Pet:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagPet), (UINameTagPet _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;

			case E_UnitType.Summon:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagSummon), (UINameTagSummon _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;

			case E_UnitType.Object:
				ZPoolManager.Instance.Spawn(E_PoolType.UI_NameTag, nameof(UINameTagFieldObject), (UINameTagFieldObject _newInstance) => {
					HandleLoadInstance(_newInstance, _followTarget);
				});
				break;
		}
	}

	private void RemoveNameTag(ZPawn _removePawn)
	{
		if (_removePawn == null)
		{
			ZLog.LogError(ZLogChannel.UI, "[NameTag] ====== RemoveNameTag Invalid");
			return;
		}
		NameTagRemove(_removePawn.gameObject);
	}


	private float ExtractNameTagFrameFrequency(float _range)
	{
		float updateDelay = 0;
		float minRange = 0;
		for (int i = 0; i < TagRange.Count; i++)
		{
			SNameTagRange rangeInfo = TagRange[i];
			updateDelay = rangeInfo.UpdateDelay;
			if (_range >= minRange && _range < rangeInfo.SquareRange)
			{
				break;
			}
			minRange = rangeInfo.SquareRange;
		}
		return updateDelay;
	}

	//---------------------------------------------------------------
	private void HandleCameraUpdate()
	{		
		UpdateNameTag(CameraManager.Instance.Main, CameraManager.Instance.Main.transform.position);
	}

	private void HandleEntityEnter(uint _entryID, ZPawn _pawn)
	{
		MakeNameTag(_pawn);
	}

	private void HandleEntityExit(uint _entryID)
	{
		ZPawn pawn = ZPawnManager.Instance.GetEntity(_entryID);
		RemoveNameTag(pawn);
	}

	private void HandleEntityDie(uint _attackerID, ZPawn _diePawn)
	{
		UINameTagBase nameTag = FindNameTag(_diePawn.gameObject) as UINameTagBase;
		if (nameTag != null)
		{
			nameTag.DoNameTagPawnDie();
		}
	}

	private void HandleLoadInstance(UINameTagBase _newInstance, ZPawn _followTarget)
	{
		if (_followTarget == null) // 태그 UI가 로딩되는 순간 폰이 패킷 해재가 되었다.
		{
			ZPoolManager.Instance.Return(_newInstance.gameObject);		
		}
		else
		{
			_newInstance.DoNameTagInitialize(_followTarget, FollowSocket, RaycastSocket);
			NameTagAdd(_followTarget.gameObject, _newInstance);
		}
	}

	private void HandleEntryChangeTarget(uint _prevTarget, uint _nextTarget)
	{
		if( _prevTarget != 0)
		{
			ZPawn pawn = ZPawnManager.Instance.GetEntity(_prevTarget);
			if (pawn != null)
			{
				DoUINameTagFocus(pawn, false);
			}
		}
		
		if (_nextTarget != 0)
		{
			ZPawn pawn = ZPawnManager.Instance.GetEntity(_nextTarget);
			if (pawn != null)
			{
				DoUINameTagFocus(pawn, true);
			}
		}
	}

	private void HandleDropItemRemove(DropItemComponent _dropItem)
	{
		if (_dropItem == null) return;

		NameTagRemove(_dropItem.gameObject);
	}

}
