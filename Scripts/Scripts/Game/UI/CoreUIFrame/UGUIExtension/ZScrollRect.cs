using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[AddComponentMenu("ZUI/ZScroll Rect", 37)]
public class ZScrollRect : ScrollRect
{

	private ScrollRect mParentsScrollRect = null;
	//---------------------------------------------------------------------------------------
	protected override void Awake()
	{
		base.Awake();
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.OnBeginDrag(eventData);
		mParentsScrollRect?.OnBeginDrag(eventData);		
	}

	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		mParentsScrollRect?.OnDrag(eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.OnEndDrag(eventData);
		mParentsScrollRect?.OnEndDrag(eventData);
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		base.OnInitializePotentialDrag(eventData);
		mParentsScrollRect?.OnInitializePotentialDrag(eventData);
	}

	//--------------------------------------------------------------------------------------
	public void FindParentsScrollRect()
	{
		Transform parentTransform = gameObject.transform;
		while(parentTransform = parentTransform.parent)
		{
			mParentsScrollRect = parentTransform.gameObject.GetComponent<ScrollRect>();
			if (mParentsScrollRect != null)
				break;
		}
	}
}
