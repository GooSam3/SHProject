using UnityEngine;

/// <summary>
/// WorldObject를 따라다니는 UI
/// </summary>
public class UIFollowTargetBase : MonoBehaviour
{
	/// <summary>현재 위치 계산에 사용되는 MainCamera</summary>
	public virtual Camera BaseCamera
	{
		get { return CameraManager.Instance.Main; }
	}

	[ReadOnly]
	protected Transform mTarget;

	protected RectTransform myRectTrans;
	protected RectTransform parentRectTrans;
	protected Vector2 parentSizeDelta;

	private void Awake()
	{
		myRectTrans = GetComponent<RectTransform>();
		if (null == myRectTrans)
		{
			ZLog.LogError(ZLogChannel.UI, $"{GetType().Name} 클래스는 반드시 RectTransform 기반이여야함.");
		}
	}
	
	/// <summary> </summary>
	/// <param name="_target"></param>
	/// <param name="_parentRT">부모 대상이 존재한다면, 대상 자식으로 편입. null이라면, HUD의 자식으로 편입.</param>
	public virtual void FollowTarget(Transform _target, RectTransform _parentRT = null)
	{
		if (null == _target)
			return;

		if (null == _parentRT)
		{
			// 현재 HUD의 자식기반으로 작동되는중..
			this.parentRectTrans = UIManager.Instance.Find<UIFrameHUD>().GetComponent<RectTransform>();
		}
		else
		{
			this.parentRectTrans = _parentRT;
		}

		this.myRectTrans.SetParent(this.parentRectTrans, false);
		this.myRectTrans.localScale = Vector2.one;

		// caching
		this.mTarget = _target;
		this.parentSizeDelta = this.myRectTrans.sizeDelta;

		CameraManager.Instance.DoAddEventCameraUpdated(UpdateUI);
	}

	/// <summary> </summary>
	public void Visibile(bool _bVisible)
	{
		this.myRectTrans.gameObject.SetActive(_bVisible);
	}

	protected virtual void OnEnable()
	{
		if (CameraManager.hasInstance && null != mTarget)
		{
			CameraManager.Instance.DoAddEventCameraUpdated(UpdateUI);
		}
		else
		{
			UpdateUI();
		}
	}

	protected virtual void OnDisable()
	{
		if (CameraManager.hasInstance)
		{
			CameraManager.Instance.DoRemoveEventCameraUpdated(UpdateUI);
		}
	}

	protected virtual void OnDestroy()
	{
		if (CameraManager.hasInstance)
		{
			CameraManager.Instance.DoRemoveEventCameraUpdated(UpdateUI);
		}
	}

	private void UpdateUI()
	{
		RepositionRectTransform();
	}

	protected virtual void RepositionRectTransform()
	{
		if (null == mTarget)
		{
			// TODO : 일단 다른데서도 파괴로 없애니까.
			Destroy(gameObject);
			return;
		}

		/*
		 * UI상 위치값 계산
		 * 
		 * Target의 위치를 Viewport(0~1) 기준값으로 구한다음, 현재 UI해상도에 맞게 좌표 변환
		 */
		Vector3 targetViewPoint = BaseCamera.WorldToViewportPoint(mTarget.position);
		Vector2 targetScreenPos = new Vector2(
		(targetViewPoint.x * parentSizeDelta.x) - parentSizeDelta.x * 0.5f,
		(targetViewPoint.y * parentSizeDelta.y) - parentSizeDelta.y * 0.5f);

		AdjustePositionResult(ref targetScreenPos);

		// UI상 위치
		myRectTrans.anchoredPosition = targetScreenPos;
	}

	/// <summary> 최종 위치 나온곳 보정이 필요하다면 사용 </summary>
	/// <param name="targetScreenPos"></param>
	protected virtual void AdjustePositionResult(ref Vector2 targetScreenPos)
	{
	}
}