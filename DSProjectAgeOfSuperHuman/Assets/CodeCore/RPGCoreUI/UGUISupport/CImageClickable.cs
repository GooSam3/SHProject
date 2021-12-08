using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 일반적인 이미지는 클릭 메시지를 발생시키지 않기때문에 새롭게 구성 
// 클릭 메시지 발생과 동시에 스크롤랙트에 전달하여 스크롤링이 되는 구조 
// 스크롤이 되면서도 클릭에 의한 이미지 팝업등을 수행가능 

[AddComponentMenu("UICustom/CImageClickable", 12)]
public class CImageClickable : Image, IPointerDownHandler, IPointerClickHandler,  IPointerUpHandler, IInitializePotentialDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IBeginDragHandler
{
    private UnityAction<Vector2> mPointerDrag = null;
    private UnityAction<Vector2> mPointerDrop = null;
    private UnityAction<Vector2> mPointerDragBegin = null;
    private UnityAction<Vector2> mPointerDragEnd = null;
    private UnityAction<Vector2> mPointerPotentialDrag = null;
    private UnityAction<Vector2> mPointerClick = null;

    private IDragHandler mHandlerDrag = null;
	private IBeginDragHandler mHandlerDragBegin = null;
	private IEndDragHandler mHandlerDragEnd = null;
    private bool mDragable = false;
    private bool mClickCancle = false;
	//------------------------------------------------------------
	public void SetImageInputEvent(UnityAction<Vector2> _pointerClick,  bool bDragable = false, UnityAction<Vector2> _pointerDrag = null, UnityAction<Vector2> _pointerDragStart = null, UnityAction<Vector2> _pointerDragEnd = null)
    {
        mPointerClick = _pointerClick;    
        mDragable = bDragable;
        mPointerDrag = _pointerDrag;
        mPointerDragBegin = _pointerDragStart;
        mPointerDragEnd = _pointerDragEnd;
        SearchParentsDragReceiver();
    }

    //------------------------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        mClickCancle = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
	{
        if (mClickCancle == false)
		{
			mPointerClick?.Invoke(eventData.pressPosition);
		}
	}

    public void OnInitializePotentialDrag(PointerEventData eventData)
	{
        mPointerPotentialDrag?.Invoke(eventData.position);
	}

    public void OnDrag(PointerEventData eventData)
	{
        mClickCancle = true;
        if (mDragable)
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
        if (mDragable)
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
        if (mDragable)
		{
            mPointerDragBegin?.Invoke(eventData.position);
        }
        else
		{
            mHandlerDragBegin?.OnBeginDrag(eventData);
        }
    }
    //-----------------------------------------------------------
    private void SearchParentsDragReceiver()
	{
        Transform parents = transform;
        bool bBreak = true;
        while(bBreak)
		{
            parents = parents.parent;
            if (parents == null) break;

            mHandlerDragBegin = parents.gameObject.GetComponent<IBeginDragHandler>();
            if (mHandlerDragBegin != null) bBreak = false;
            
            mHandlerDrag = parents.gameObject.GetComponent<IDragHandler>();
            if (mHandlerDrag != null) bBreak = false;

            mHandlerDragEnd = parents.gameObject.GetComponent<IEndDragHandler>();
            if (mHandlerDragEnd != null) bBreak = false;
        }
	}
}
