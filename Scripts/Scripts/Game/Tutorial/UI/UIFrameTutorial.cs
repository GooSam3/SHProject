using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary> 튜토리얼 UI </summary>
public class UIFrameTutorial : MonoBehaviour
{ 
	[SerializeField]
	private UIFrameTutorialDialogue Dialogue;

	[SerializeField]
	private UIFrameTutorialGuide Guide;

	[SerializeField]
	private GameObject ObjArrow;

	[SerializeField]
	private GameObject ObjFocus;

	[SerializeField]
	private GameObject ObjSkip;

	private RectTransform mFocusTarget;

	private RectTransform mHighlightTarget;

	[SerializeField]
	private Image DimmedScreen;

	/// <summary> 포커스 오브젝트 클릭 </summary>
	private Action mEventFocusClick;

	private Action<PointerEventData> mEventFocusDown;

	private Action<PointerEventData> mEventFocusUp;

	private float mFocusTime = 0f;

	private GameObject mHighlightObject;

	private Tweener mTweener;

	private void Awake()
	{
		Dialogue.gameObject.SetActive(false);
		Guide.gameObject.SetActive(false);
		ObjArrow.SetActive(false);
		ObjFocus.SetActive(false);
		ObjSkip.SetActive(false);

		DimmedScreen.color = new Color(0f, 0f, 0f, 0f);
	}

	private void OnDestroy()
	{
		if(ZMonoManager.hasInstance)
			ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateFocus);
	}

	public void Show()
	{
		ShowDimmedScreen();
	}

	public void ShowDimmedScreen(float alpha = 0.4f, Action onFinish = null)
	{
		if (null != mTweener)
			mTweener.Kill();

		Color color = Color.black;
		color.a = alpha;
		mTweener = DimmedScreen.DOColor(color, 0.5f).OnComplete(() =>
		{
			onFinish?.Invoke();
		});
	}

	public void SetDialogue(bool bActive, string name = "", string text = "", string imgName = "", Action onFinish = null)
	{
		Dialogue.gameObject.SetActive(bActive);
		Guide.gameObject.SetActive(false);

		if(true == bActive)
		{
			Dialogue.Set(name, text, imgName, onFinish);
		}		
	}

	public void SetGuide(bool bActive, string name = "", string text = "", string imgName = "")
	{
		Guide.transform.localPosition = Vector3.zero;
		Guide.gameObject.SetActive(bActive);
		Dialogue.gameObject.SetActive(false);

		if (true == bActive)
		{
			Guide.Set(name, text, imgName);
		}
	}

	/// <summary> 가이드 위치 조정 </summary>
	private void UpdateGuideUI()
	{
		Vector3 dir = ObjFocus.transform.localPosition.normalized;

		var addGuideSize = Guide.GetComponent<RectTransform>().rect.size;
		var addFocusSize = ObjFocus.GetComponent<RectTransform>().rect.size;
		var addArrowSize = ObjArrow.GetComponent<RectTransform>().rect.size;

		float addX = -dir.x * ((addGuideSize.x + addFocusSize.x) * 0.5f + addArrowSize.x);
		float addY = -dir.y * ((addGuideSize.y + addFocusSize.y) * 0.5f + addArrowSize.y);

		Guide.transform.localPosition = ObjFocus.transform.localPosition + (new Vector3(addX, addY));
	}

	public void FocusUI(GameObject target, Transform highlightObject, Action onClick, Action<PointerEventData> onFocusPointUp = null, Action<PointerEventData> onFocusPointDown = null)
	{
		if (null != target)
		{
			if(null != highlightObject)
			{
				mFocusTarget = target.GetComponent<RectTransform>();
				mHighlightTarget = highlightObject.GetComponent<RectTransform>();
			}				
			else
			{
				mFocusTarget = target.GetComponent<RectTransform>();
				mHighlightTarget = mFocusTarget;
			}

			CreateHighlightObject(mHighlightTarget);

			ObjFocus.SetActive(true);

			mEventFocusClick = onClick;
			mEventFocusDown = onFocusPointDown;
			mEventFocusUp = onFocusPointUp;

			mFocusTime = Time.time;

			ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateFocus);
		}
		else
		{
			HideFocusUI();
		}
	}

	private void CreateHighlightObject(Transform target)
	{
		DestroyHighlightObject();

		mHighlightObject = GameObject.Instantiate(target.gameObject, transform);

		foreach(var comp in mHighlightObject.GetComponentsInChildren<Graphic>())
		{
			comp.raycastTarget = false;
		}

		foreach (var comp in mHighlightObject.GetComponentsInChildren<Canvas>())
		{
			comp.overrideSorting = false;
		}

		var trans = mHighlightObject.GetComponent<RectTransform>();

		trans.anchorMin = Vector2.one * 0.5f;
		trans.anchorMax = Vector2.one * 0.5f;
		trans.pivot = Vector2.one * 0.5f;
	}

	private void DestroyHighlightObject()
	{
		if (null != mHighlightObject)
			GameObject.Destroy(mHighlightObject);

		mHighlightObject = null;
	}

	private void UpdateFocus()
	{
		var rect = mFocusTarget.rect;

		ObjFocus.transform.position = mFocusTarget.transform.position;// + new Vector3(rect.center.x, rect.center.y , 0f);
		ObjFocus.transform.localPosition += new Vector3(rect.center.x, rect.center.y, 0f);
		ObjFocus.GetComponent<RectTransform>().sizeDelta = rect.size;// rectTrans.sizeDelta;

		rect = mHighlightTarget.rect;

		mHighlightObject.transform.position = mHighlightTarget.position;
		mHighlightObject.transform.localPosition += new Vector3(rect.center.x, rect.center.y, 0f);
		mHighlightObject.GetComponent<RectTransform>().sizeDelta = rect.size;

		//가이드 위치 수정후 표시
		ShowArrowUI();

		//포커스가 활성화 되었을 경우 가이드 위치 수정
		UpdateGuideUI();
	}

	public void HideFocusUI()
	{
		mFocusTarget = null;
		ObjFocus.SetActive(false);
		
		mEventFocusClick = null;
		mEventFocusDown = null;
		mEventFocusUp = null;
		
		ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.LateUpdate, UpdateFocus);
		HideArrowUI();
		DestroyHighlightObject();
	}

	public void OnClickFocus()
	{
		if (mFocusTime + 0.5f > Time.time)
		{
			ZLog.Log(ZLogChannel.Quest, "바로 클릭 막기!");
			return;
		}

		mEventFocusClick?.Invoke();
	}

	/// <summary>퀵슬롯 오토 게이지 활성화 이벤트 콜백</summary>
	public void OnPointerDown(BaseEventData eventData)
	{
		mEventFocusDown?.Invoke(eventData as PointerEventData);
	}

	public void OnPointerUp(BaseEventData eventData)
	{
		mEventFocusUp?.Invoke(eventData as PointerEventData);
	}

	private void ShowArrowUI()
	{
		ObjArrow.SetActive(true);

		Vector3 dir = ObjFocus.transform.localPosition.normalized;

		var addFocusSize = ObjFocus.GetComponent<RectTransform>().rect.size;
		var addArrowSize = ObjArrow.GetComponent<RectTransform>().rect.size;
		
		float addX = -dir.x * (addFocusSize.x + addArrowSize.x);
		float addY = -dir.y * (addFocusSize.y + addArrowSize.y);

		ObjArrow.transform.localPosition = ObjFocus.transform.localPosition + (new Vector3(addX, addY) * 0.5f);


		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;// + 270;
		ObjArrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	private void HideArrowUI()
	{
		ObjArrow.SetActive(false);
	}

	public void SetBockScreen(bool bActive, bool bAlpha)
	{
		if(true == bActive)
			DimmedScreen.gameObject.SetActive(true);

		if (false == bActive)
			bAlpha = false;

		float alpha = bAlpha ? 0.4f : 0f;

		ShowDimmedScreen(alpha, () =>
		{
			if (false == bActive)
				DimmedScreen.gameObject.SetActive(false);
		});
	}

	public void ShowSkip()
	{
		ObjSkip.SetActive(true);
	}

	public void HideSkip()
	{
		ObjSkip.SetActive(false);
	}

	public void OnClickSkip()
	{
		if (true == TutorialSystem.hasInstance)
			TutorialSystem.Instance.TutorialSkip();
	}
}