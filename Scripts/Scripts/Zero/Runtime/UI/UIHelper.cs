using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHelper
{
	static PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
	static List<RaycastResult> results = new List<RaycastResult>();
	/// <returns>
	/// true if mouse or first touch is over any event system object ( usually gui elements )
	/// </returns>
	public static bool IsPointerOverGameObject()
	{
		//check mouse
		if (null != EventSystem.current && EventSystem.current.IsPointerOverGameObject())
			return true;

		//check touch
		if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
		{
			if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
				return true;
		}

		//if (null == eventDataCurrentPosition)
		//{
		//	eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		//}

		//results.Clear();
		//eventDataCurrentPosition.position = Input.mousePosition;

		//EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

		//return results.Count > 0;

		return false;
	}

	public static bool IsPointerOverGameObject(ref Touch touch)
	{
		//check mouse
		if (null != EventSystem.current && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
			return true;

		//check touch
		if (Input.touchCount > 0 && touch.phase == TouchPhase.Began)
		{
			if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
				return true;
		}

		return false;
	}
}
