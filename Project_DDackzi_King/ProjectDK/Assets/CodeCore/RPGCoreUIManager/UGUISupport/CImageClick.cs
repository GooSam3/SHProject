using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 일반적인 이미지는 클릭 메시지를 발생시키지 않기때문에 새롭게 구성 
// 클릭 메시지 발생과 동시에 스크롤랙트에 전달하여 스크롤링이 되는 구조 
// 스크롤이 되면서도 클릭에 의한 이미지 팝업등을 수행가능 

[AddComponentMenu("UICustom/CImageClickable", 12)]
public class CImageClick : UnityEngine.UI.Image, IPointerDownHandler, IPointerClickHandler,  IPointerUpHandler, IInitializePotentialDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] bool  DragDrop = false;
//    [SerializeField] float LongPressDelay = 0.5f;
//    private float mLongPressTime = 0;
//    private bool mLongPress = false;

    [SerializeField, HideInInspector]
    private IDragHandler        mHandlerDrag = null;
    [SerializeField, HideInInspector]
    private IBeginDragHandler   mHandlerDragBegin = null;
    [SerializeField, HideInInspector]
    private IEndDragHandler     mHandlerDragEnd = null;

    private UnityAction<Vector2> mPointerDown = null;
    private UnityAction<Vector2> mPointerUp = null;
    private UnityAction<Vector2> mPointerDrag = null;
    private UnityAction<Vector2> mPointerDrop = null;
    private UnityAction<Vector2> mPointerDragBegin = null;
    private UnityAction<Vector2> mPointerDragEnd = null;
    private UnityAction<Vector2> mPointerPotentialDrag = null;
    private UnityAction<Vector2> mPointerLongPress = null;

    private bool mDragNow = false;
    //------------------------------------------------------------
    public void SetImageInputEvent(UnityAction<Vector2> _pointerDown, UnityAction<Vector2> _pointerUp, UnityAction<Vector2> _pointerDrag = null, UnityAction<Vector2> _pointerLongPress = null)
    {
        mPointerDown = _pointerDown;
        mPointerUp = _pointerUp;
        mPointerDrag = _pointerDrag;
        mPointerLongPress = _pointerLongPress;
        if (DragDrop == false)
		{
            SearchParentsDragReceiver();
        }
    }

    //------------------------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        mPointerDown?.Invoke(eventData.pressPosition);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mDragNow == false)
		{
            mPointerUp?.Invoke(eventData.pressPosition);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
	{
        if (mDragNow == false)
        {
           
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
	{
        mPointerPotentialDrag?.Invoke(eventData.position);
	}

    public void OnDrag(PointerEventData eventData)
	{
        if (DragDrop)
		{
            mPointerDrag?.Invoke(eventData.position);
		}
        else
		{
            mHandlerDrag?.OnDrag(eventData);
        }
    }

    public void OnDrop(PointerEventData eventData)
	{
        mPointerDrop?.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
	{
        mDragNow = false;

        if (DragDrop)
		{
            mPointerDragEnd?.Invoke(eventData.position);
        }
        else
		{
            mHandlerDragEnd?.OnEndDrag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
	{
        mDragNow = true;

        if (DragDrop)
		{
            mPointerDragBegin?.Invoke(eventData.position);
        }
        else
		{
            CheckDragReceiver();
            mHandlerDragBegin?.OnBeginDrag(eventData);
        }
    }
    //-----------------------------------------------------------
    private void SearchParentsDragReceiver()
	{
        Transform parents = transform;
        bool breakWhile = true;
        while(breakWhile)
		{
            parents = parents.parent;
            if (parents == null) break;

            mHandlerDragBegin = parents.gameObject.GetComponent<IBeginDragHandler>();
            if (mHandlerDragBegin != null) breakWhile = false;
            
            mHandlerDrag = parents.gameObject.GetComponent<IDragHandler>();
            if (mHandlerDrag != null) breakWhile = false;

            mHandlerDragEnd = parents.gameObject.GetComponent<IEndDragHandler>();
            if (mHandlerDragEnd != null) breakWhile = false;
        }
	}

    private void CheckDragReceiver()
	{
        if (mHandlerDragBegin == null)
            SearchParentsDragReceiver();
    }
}
