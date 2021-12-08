using UnityEngine;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(UnityEngine.UI.GraphicRaycaster))]
abstract public class CUIWidgetNameTagBase : CUGUIWidgetBase
{
	private GameObject	mOwner = null;
	private Transform		mFollowTarget = null;
	private Transform		mRaycastTarget = null;
	private Canvas		mCanvas = null;
	private float			mUpdateDelay = 0;
	private float			mUpdateDelayOrigin = 0;
	private float			mCurrentUpdateDelay = 0;
	private int			mRayCastMask = 0;
	private bool			mRayCastFilter = false;
	private bool			mActiveUpdate = true; 

	//---------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mCanvas = GetComponent<Canvas>();
		mRectTransform.SetAnchor(AnchorPresets.MiddleCenter);		
	}

	//---------------------------------------------------------------
	public void UpdateNameTag(Camera _projectionCamera, Vector2 _screenSize, int _sortLayer)
	{
		if (mActiveUpdate == false) return;

		mCanvas.sortingOrder = _sortLayer;
		UpdateNameTagPosition(_projectionCamera, _screenSize);
		UpdateNameTagRayCast(_projectionCamera);
		OnNameTagUpdate();
	}

	public void DoNameTagRemove()
	{
		OnNameTagRemove();
	}

	public void DoNameTagActive(bool _active)
	{
		if (_active)
		{
			NameTagActiveUpdate(true);
		}
		else
		{
			NameTagActiveUpdate(false);
			DoUIWidgetShowHide(false);
		}
	}

	public Vector3 GetNameTagWorldPosition()
	{
		Vector3 position = Vector3.zero;
		if (mFollowTarget != null)
		{
			position = mFollowTarget.position;
		}	

		return position;
	}

	public void SetNameTagChangeFrequecy(float _updateDelay)
	{
		if (mUpdateDelayOrigin != _updateDelay)
		{
			ResetRandomDelay(_updateDelay);
		}
	}
	public bool IsNameTagValid()
	{
		bool valid = true;

		if (mFollowTarget == null || mRaycastTarget == null)
		{
			valid = false;
		}

		return valid;
	}

	//---------------------------------------------------------------
	protected void NameTagFollowTarget(GameObject _owner, Transform _followTarget, Transform _raycastTarget, int _raycastMask)
	{
		mOwner = _owner;
		mFollowTarget = _followTarget;
		mRaycastTarget = _raycastTarget;
		mRayCastMask = _raycastMask;
	}

	protected void NameTagActiveUpdate(bool _active)
	{
		mActiveUpdate = _active;
	}
	//---------------------------------------------------------------
	private void UpdateNameTagPosition(Camera _projectionCamera, Vector2 _screenSize)
	{
		if (mFollowTarget == null)
		{
			return;
		}

		Vector3 viewportPosition = _projectionCamera.WorldToViewportPoint(mFollowTarget.position);
		Vector2 screenPosition = new Vector2(
		(viewportPosition.x * _screenSize.x) - _screenSize.x / 2,
		(viewportPosition.y * _screenSize.y) - _screenSize.y / 2);

		mRectTransform.anchoredPosition = screenPosition;

		if (viewportPosition.z < 0)
		{
			DoUIWidgetShowHide(false);
		}
		else
		{
			if (mRayCastFilter == false)
			{
				DoUIWidgetShowHide(true);
			}
		}	
	}

	private void UpdateNameTagRayCast(Camera _projectionCamera)
	{		
		if (mRaycastTarget == null) return;

		if (mCurrentUpdateDelay == 0)
		{
			Raycast(_projectionCamera);
		}

		mCurrentUpdateDelay += Time.deltaTime;
		if (mCurrentUpdateDelay >= mUpdateDelay)
		{
			mCurrentUpdateDelay = 0;
		}
	}

	private void ResetRandomDelay(float _delaySeed)
	{
		float RandomValue = Random.Range(0.01f, 0.1f);  // 한번에 레이케스트 되지 않도록 빈도를 무작위로 섞는다.
		mUpdateDelay = _delaySeed + RandomValue;
		mUpdateDelayOrigin = _delaySeed;
		mCurrentUpdateDelay = 0f;
	}

	private void Raycast(Camera _projectionCamera)
	{
		Vector3 origin = _projectionCamera.transform.position;
		Vector3 dest = mRaycastTarget.position;
		RaycastHit rHit;
		if (Physics.Linecast(origin, dest, out rHit, mRayCastMask, QueryTriggerInteraction.Collide))
		{
			mRayCastFilter = true;
			DoUIWidgetShowHide(false);
			OnNameTagRaycast(ref rHit);
		}
		else
		{
			mRayCastFilter = false;
		}
	}

	//---------------------------------------------------------------
	protected virtual void OnNameTagUpdate() { }
	protected virtual void OnNameTagRemove() { }
	protected virtual void OnNameTagRaycast(ref RaycastHit _rayHit) { }
	protected virtual void OnNameTagVisibleCheck() { }
}
