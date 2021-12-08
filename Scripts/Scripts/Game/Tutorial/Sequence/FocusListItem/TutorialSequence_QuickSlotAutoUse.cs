using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> 해당 index의 QuickSlot에서 Auto 를 활성화 한다. </summary>
public class TutorialSequence_QuickSlotAutoUse : TutorialSequence_QuickSlotBase
{
	private const string mAutoButtonPath = "ItemSlot_Quick/AutoSwitch_Parts/Auto_On/AutoCheck";

	private const string mAutoButtonHighlightPath = "ItemSlot_Quick/AutoSwitch_Parts";
	/// <summary> 오토가 켜져있는 상태 여부 </summary>
	private bool IsShowAuto = false;

	/// <summary> AutoButton </summary>
	private EventTrigger mAutoButtonTrigger;

	protected override Transform GetHighlightObject()
	{
		if(false == IsShowAuto)
		{
			return base.GetHighlightObject();
		}
		else
		{
			return mButton.transform.Find(mAutoButtonHighlightPath);
		}
	}

	protected override void ButtonAction()
	{
	}

	protected override void HandleEventPointUp(PointerEventData eventData)
	{
		if (false == IsShowAuto)
		{
			QuickSlot.OnPointerUp(eventData);
			CancelInvoke(nameof(InvokeCheckAuto));			
		}	
		else
		{
			QuickSlot.AutoOnOffEvent(true);
			//mButton?.onClick.Invoke();

			EndSequence(false);
			//TriggerEventByAutoButton(EventTriggerType.PointerUp, eventData);
		}	
	}

	protected override void HandleEventPointDown(PointerEventData eventData)
	{
		if (false == IsShowAuto)
		{
			QuickSlot.OnPointerDown(eventData);
			Invoke(nameof(InvokeCheckAuto), 0.1f);
		}
		else
		{
			//TriggerEventByAutoButton(EventTriggerType.PointerDown, eventData);
		}
	}

	private void InvokeCheckAuto()
	{
		CancelInvoke(nameof(InvokeCheckAuto));
		if(false == QuickSlot.AutoGroup[0].activeInHierarchy)
		{
			Invoke(nameof(InvokeCheckAuto), 0.1f);
			return;
		}

		if (true == IsShowAuto)
			return;

		IsShowAuto = true;

		SetAutoButton();
		ShowGuide(mAutoButtonTrigger.gameObject, ButtonAction, HandleEventPointUp, HandleEventPointDown);
	}

	private void SetAutoButton()
	{
		if (null != mAutoButtonTrigger)
			return;
		
		mAutoButtonTrigger = mButton.transform.Find(mAutoButtonPath).GetComponent<EventTrigger>();
	}

	//private void TriggerEventByAutoButton(EventTriggerType type, PointerEventData eventData)
	//{
	//	if (false == IsShowAuto)
	//		return;

	//	var trigger = mAutoButtonTrigger.triggers.Find((item) => item.eventID == type);

	//	if(null != trigger)
	//	{
	//		trigger.callback.Invoke(eventData);
	//	}
	//}
}