using System.Collections.Generic;

abstract public class CUIFrameWidgetBase : CUIFrameLocalizingBase
{
	private List<CUIWidgetPopupBase> m_listWidgetPopup = new List<CUIWidgetPopupBase>();
    private CMultiSortedDictionary<int, CUGUIButtonRadioBase> m_dicRadioGroup = new CMultiSortedDictionary<int, CUGUIButtonRadioBase>();
	//---------------------------------------------------
	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		WidgetPopupRefresh(_LayerOrder);
	}

	protected override void OnRefreshOrder(int _LayerOrder)
	{
		base.OnRefreshOrder(_LayerOrder);
		WidgetPopupRefresh(_LayerOrder);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();

		for(int i = 0; i < m_listWidgetInstance.Count; i++)
		{
			CUIWidgetPopupBase widgetPopup = m_listWidgetInstance[i] as CUIWidgetPopupBase;
			if (widgetPopup)
			{
				m_listWidgetPopup.Add(widgetPopup);
			}
		}
	}

	protected override void OnAddWidget(CUGUIWidgetBase _addWidget)
	{
		base.OnAddWidget(_addWidget);
		CUIWidgetPopupBase widgetPopup = _addWidget as CUIWidgetPopupBase;
		if (widgetPopup) m_listWidgetPopup.Add(widgetPopup);
	}

	protected override void OnDeleteWidget(CUGUIWidgetBase _addWidget)
	{
		base.OnDeleteWidget(_addWidget);
		CUIWidgetPopupBase widgetPopup = _addWidget as CUIWidgetPopupBase;
		if (widgetPopup) m_listWidgetPopup.Remove(widgetPopup);
	}

	//----------------------------------------------------
	public bool ImportUIWidgetRadioRegist(int _radioGroup, CUGUIButtonRadioBase _RadioButton)
	{
		bool FirstRadioButton = false;

		if (m_dicRadioGroup.ContainsKey(_radioGroup) == false)
		{
			FirstRadioButton = true;
		}

		m_dicRadioGroup.Add(_radioGroup, _RadioButton);

		return FirstRadioButton;
	}

	public void ImportUIWidgetRadioUnRegist(int _radioGroup, CUGUIButtonRadioBase _radioButton)
	{
		if (_radioButton == null) return;

		if (m_dicRadioGroup.ContainsKey(_radioGroup))
		{
			List<CUGUIButtonRadioBase> listRadioButton = m_dicRadioGroup[_radioGroup];
			listRadioButton.Remove(_radioButton);
		}
	}

	public void ImportUIWidgetRadioSelect(int _radioGroup, CUGUIButtonRadioBase _radioButton)
	{
		if (_radioButton == null) return;

		if (m_dicRadioGroup.ContainsKey(_radioGroup))
		{
			List<CUGUIButtonRadioBase> listRadioButton = m_dicRadioGroup[_radioGroup];

			for (int i = 0; i < listRadioButton.Count; i++)
			{
				CUGUIButtonRadioBase RadioButton = listRadioButton[i];
				if (RadioButton != _radioButton && RadioButton != null)
				{
					RadioButton.DoToggleAction(false);
				}
			}
		}
	}

	public void ImportUIWidgetPopupShow(CUIWidgetPopupBase _widgetPopup)
	{
		WidgetPopupShow(_widgetPopup);
	}

	//----------------------------------------------------------------------------
	private void WidgetPopupRefresh(int _layerOrder)
	{
		for (int i = 0; i < m_listWidgetPopup.Count; i++)
		{
			m_listWidgetPopup[i].ImportWidgetPopupRefreshLayer(_layerOrder);
		}
	}

	private void WidgetPopupShow(CUIWidgetPopupBase _widgetPopup)
	{
		for (int i = 0; i < m_listWidgetPopup.Count; i++)
		{
			if (m_listWidgetPopup[i] != _widgetPopup)
				m_listWidgetPopup[i].DoUIWidgetShowHide(false);
		}
	}
}
