using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[AddComponentMenu("UICustom/CButton", 1)]
public class CButton : Button
{
	private bool m_bEventOnPress = false;
	private bool m_bEventLongPress = false;
	private bool m_bEventLongPressStart = false;
	private bool m_bEventLongPressRefresh = false;
	
	private float m_fEventLongPressDelay = 0.2f;
	private float m_fEventLognPressCurrent = 0;
	private int	 m_iEventLongPressCount = 0;

	private UnityAction			m_delPointUp = null;
	private UnityAction<bool>		m_delLongPressStartEnd = null;
	private UnityAction<int>		m_delLongPressCount = null;
	//---------------------------------------------------------
	public override void OnPointerClick(PointerEventData eventData)
	{
		if (m_bEventOnPress == false)
		{
			base.OnPointerClick(eventData);
			if (m_delLongPressCount != null)
			{
				m_delLongPressCount(0);
			}
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (m_bEventOnPress)
		{
			base.OnPointerClick(eventData);
		}

		if (m_bEventLongPress)
		{
			m_bEventLongPressStart = true;
			m_fEventLognPressCurrent = 0;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);

		if (!IsActive() || !IsInteractable())
			return;

		m_delPointUp?.Invoke();

		if (m_bEventLongPress)
		{
			PrivButtonLongPressStartEnd(false);
		}
	}

	private void Update()
	{
		if (m_bEventLongPress)
		{
			if (interactable == false)
			{
				if (m_bEventLongPressStart)
				{
					PrivButtonLongPressStartEnd(false);
				}
			}
			else
			{
				UpdateButtonEventLongPress(Time.deltaTime);
			}
		}
	}

	

	//---------------------------------------------------------
	public void DoButtonClick()
	{
		OnSubmit(null);
	}

	public void SetButtonEventOnPress(bool bOn, UnityAction delPointUp)
	{
		m_bEventOnPress = bOn;
		m_delPointUp = delPointUp;
	}

	public void SetButtonEventLongPress(bool bLongPress, float fLongPressDelay, UnityAction<bool> delLongPressStartEnd, UnityAction<int> delLongPressCounter)
	{
		m_bEventLongPress = bLongPress;
		m_fEventLongPressDelay = fLongPressDelay;
		m_delLongPressStartEnd = delLongPressStartEnd;
		m_delLongPressCount = delLongPressCounter;
	}

	//-------------------------------------------------------------
	private void UpdateButtonEventLongPress(float fDeltaTime)
	{
		if (m_bEventLongPressStart)
		{
			if (m_bEventLongPressRefresh)
			{
				m_fEventLognPressCurrent += fDeltaTime;
				if (m_fEventLognPressCurrent >= 0.05f)
				{
					m_fEventLognPressCurrent = 0;
					m_delLongPressCount?.Invoke(++m_iEventLongPressCount);
				}
			}
			else
			{
				m_fEventLognPressCurrent += fDeltaTime;
				if (m_fEventLognPressCurrent >= m_fEventLongPressDelay)
				{
					PrivButtonLongPressStartEnd(true);
				}
			}
		}
	}

	private void PrivButtonLongPressStartEnd(bool bStart)
	{
		if (bStart)
		{
			m_bEventLongPressRefresh = true;
			m_fEventLognPressCurrent = 0;
			m_iEventLongPressCount = 0;
			m_delLongPressStartEnd?.Invoke(true);
		}
		else
		{
			m_bEventLongPressStart = false;
			m_bEventLongPressRefresh = false;
			m_delLongPressStartEnd?.Invoke(false);
		}
	}
}
