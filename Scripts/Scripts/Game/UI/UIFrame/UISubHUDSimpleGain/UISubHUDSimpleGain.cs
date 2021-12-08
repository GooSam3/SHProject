using GameDB;
using System.Collections.Generic;
using UnityEngine;

public sealed class UISubHUDSimpleGain : ZUIFrameBase
{
	private enum E_SimpleGainType
	{
		Gold,
		Exp,
	}

	private List<UISimpleGainBase> gainItemList = new List<UISimpleGainBase>();

	private bool isSpawned;
	private Transform followTarget;
	private Transform thisTransform;

	protected override void OnInitialize()
	{
		base.OnInitialize();

		ZPoolManager.Instance.Spawn( E_PoolType.UI, nameof( UISimpleGainGold ), objGold=> {
			ZPoolManager.Instance.Spawn( E_PoolType.UI, nameof( UISimpleGainExp ), objExp=> {
				isSpawned = true;

				ZPoolManager.Instance.Return(objGold);
				ZPoolManager.Instance.Return(objExp);
			}, 0, 1, false );
		}, 0, 1, false );
	}

	protected override void OnShow( int _LayerOrder )
	{
		AddEvent();
	}

	protected override void OnRemove()
	{
		for( int i = 0; i < gainItemList.Count; ++i ) {
			gainItemList[ i ].DoStop();
		}
		gainItemList.Clear();

		RemoveEvent();
	}

	protected override void OnHide()
	{
		for( int i = 0; i < gainItemList.Count; ++i ) {
			gainItemList[ i ].DoStop();
		}

		RemoveEvent();
	}

	private void AddEvent()
	{
		ZPawnManager.Instance.DoAddEventCreateMyEntity( OnCreatedMyEntity );

		ZNet.Data.Me.CurCharData.ExpUpdated -= OnUpdateExp;
		ZNet.Data.Me.CurCharData.ExpUpdated += OnUpdateExp;

		DropItemSpawner.DropGoldUpdate -= OnUpdateGold;
		DropItemSpawner.DropGoldUpdate += OnUpdateGold;
	}

	private void RemoveEvent()
	{
		ZPawnManager.Instance.DoRemoveEventCreateMyEntity( OnCreatedMyEntity );

		if( ZPawnManager.Instance.MyEntity != null ) {
			ZPawnManager.Instance.MyEntity.DoRemoveEventLoadedModel( OnLoadedModel );
		}

		if( CameraManager.hasInstance ) {
			CameraManager.Instance.DoRemoveEventCameraUpdated( OnCameraUpdated );
		}

		ZNet.Data.Me.CurCharData.ExpUpdated -= OnUpdateExp;

		DropItemSpawner.DropGoldUpdate -= OnUpdateGold;
	}

	private void OnCreatedMyEntity()
	{
		ZPawnManager.Instance.MyEntity.DoAddEventLoadedModel( OnLoadedModel );
	}

	private void OnLoadedModel()
	{
		followTarget = ZPawnManager.Instance.MyEntity.GetSocket( E_ModelSocket.Head );

		CameraManager.Instance.DoAddEventCameraUpdated( OnCameraUpdated );
	}

	private void OnCameraUpdated()
	{
		UpdatePosition( CameraManager.Instance.Main, mUIFrameSize );
	}

	private void OnUpdateExp( ulong preExp, ulong newExp, bool isMonsterKill )
	{
		if( isMonsterKill == false ) {
			return;
		}

		if (!ZGameOption.Instance.bShowExpGainEffect)
			return;

		ulong exp = newExp - preExp;
		ShowItem( E_SimpleGainType.Exp, exp );
	}

	private void OnUpdateGold( ulong amount )
	{
		if (!ZGameOption.Instance.bShowGoldGainEffect)
			return;

		ShowItem( E_SimpleGainType.Gold, amount );
	}

	private void UpdatePosition( Camera projectionCamera, Vector2 screenSize )
	{
		if( followTarget == null ) {
			return;
		}

		Vector3 viewportPosition = projectionCamera.WorldToViewportPoint( followTarget.position );
		Vector2 screenPosition = new Vector2(
			( viewportPosition.x * screenSize.x ) - screenSize.x / 2,
			( viewportPosition.y * screenSize.y ) - screenSize.y / 2 );

		mRectTransform.anchoredPosition = screenPosition;

		DoActivateAll( viewportPosition.z >= 0 );
	}

	private void DoActivateAll( bool bActive )
	{
		for( int i = 0; i < gainItemList.Count; ++i ) {
			gainItemList[ i ].DoActivate( bActive );
		}
	}

	private void ShowItem( E_SimpleGainType gainType, ulong amount )
	{
		if( isSpawned == false ) {
			ZLog.Log( ZLogChannel.Default, $"@yarny 프리팹 아직스폰안됨 {gainType}/{amount}" );
			return;
		}

		if( followTarget == null ) {
			return;
		}

		switch( gainType ) {
			case E_SimpleGainType.Gold: {
				var item = GetItem<UISimpleGainGold>( new Vector3( 150, -100, 0 ) );
				item.DoStart( amount );
				break;
			}
			case E_SimpleGainType.Exp: {
				var item = GetItem<UISimpleGainExp>( new Vector3( -180, -100, 0 ) );
				item.DoStart( amount );
				break;
			}
		}
	}

	private UISimpleGainBase GetItem<T>( Vector3 startPos ) where T : UISimpleGainBase
	{
		var gainItem = gainItemList.Find( v => v.gameObject.activeSelf == false && v.GetType() == typeof( T ) );
		if( gainItem == null ) {
			var x = ZPoolManager.Instance.FindClone( E_PoolType.UI, typeof( T ).Name );
			UISimpleGainBase uiObj = x.GetComponent<T>();

			if( thisTransform == null ) {
				var simpleGain = UIManager.Instance.Find<UISubHUDSimpleGain>();
				if( simpleGain != null ) {
					thisTransform = simpleGain.transform;
				}
			}

			uiObj.transform.SetParent( thisTransform );
			uiObj.transform.localScale = Vector3.one;
			uiObj.transform.localPosition = startPos;
			uiObj.Initialized();

			gainItem = uiObj;
			gainItemList.Add( uiObj );
		}
		return gainItem;
	}
}