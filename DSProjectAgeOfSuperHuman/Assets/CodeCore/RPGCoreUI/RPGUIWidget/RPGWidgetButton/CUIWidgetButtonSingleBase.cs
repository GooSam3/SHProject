using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(CButton))]
public abstract class CUIWidgetButtonSingleBase : CUIWidgetBase
{
	[SerializeField]
	private bool EventOnPress = false; // Click �̺�Ʈ�� ������ �߻���Ų��. ��¡��ų���� ��ư ���鶧 ��� 

	private CButton m_pUGUIButton = null; protected CButton pButton { get { return m_pUGUIButton; } }
	private CText m_pTextButtonName = null;
	private UnityEvent m_delButtonOnClick = null;
	//----------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pUGUIButton = GetComponent<CButton>();
		m_pTextButtonName = GetComponentInChildren<CText>();
		if (EventOnPress)
		{
			m_pUGUIButton.SetButtonEventOnPress(true, HandleButtonPointUP);
		}

		m_delButtonOnClick = m_pUGUIButton.onClick; // �ν����Ϳ� �Էµ� ���������ʹ� ���� ����
		m_pUGUIButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent(); // ���������� ���¹� Ŀ���� ����
		m_pUGUIButton.onClick.AddListener(HandleButtonClick);
	}

	//------------------------------------------------
	public void SetButtonInteraction(bool bEnable)
	{
		m_pUGUIButton.interactable = bEnable;
	}

	public void SetButtonName(string strName)
	{
		if (m_pTextButtonName != null)
		{
			m_pTextButtonName.text = strName;
		}
	}

	//-----------------------------------------------
	protected void ProtButtonActionPress()
	{
		m_pUGUIButton.DoButtonClick();
	}

	protected void ProtButtonActionEvent()
	{
		m_delButtonOnClick.Invoke();
	}

	//----------------------------------------------
	private void HandleButtonClick()
	{
		OnButtonClick();
	}

	private void HandleButtonPointUP()
	{
		OnButtonPointUp();
	}

	

	//------------------------------------------------
	protected virtual void OnButtonClick() { m_delButtonOnClick.Invoke(); }
	protected virtual void OnButtonPointUp() { }
}
