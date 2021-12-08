using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ZToggleGroup : ToggleGroup
{
	[SerializeField]
	List<ZToggle> ToggleList = new List<ZToggle>();

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();

		SetAllToggleList();
	}
#endif

    private void SetAllToggleList()
    {
		List<ZToggle> list = new List<ZToggle>();

		for (int i = 0; i < m_Toggles.Count; i++)
			list.Add((ZToggle)m_Toggles[i]);

		ToggleList = list;
	}

	/// <summary>등록된 모든 토글 리스트를 반환한다.</summary>
	public List<ZToggle> GetAllToggle()
	{
		SetAllToggleList();
		return ToggleList;
	}

	/// <summary>선택된 토글 오브젝트를 반환한다.</summary>
	public ZToggle GetSelectToggle()
	{
		SetAllToggleList();

		for (int i = 0; i < ToggleList.Count; i++)
			if (ToggleList[i].isOn)
				return ToggleList[i];

		return null;
	}

	/// <summary>찾으려는 인덱스에 해당하는 토글 오브젝트를 반환한다.</summary>
	public ZToggle GetToggle(int _toggleIndex)
	{
		SetAllToggleList();

		for (int i = 0; i < ToggleList.Count; i++)
			if (i == _toggleIndex)
				return ToggleList[i];

		return null;
	}

	/// <summary>선택된 토글 오브젝트의 인덱스를 반환한다.</summary>
	public byte GetSelectToggleIndex()
	{
		SetAllToggleList();

		for (int i = 0; i < ToggleList.Count; i++)
			if (ToggleList[i].isOn)
				return Convert.ToByte(i);

		return 0;
	}

	/// <summary>특정 토글 오브젝트를 선택한다.</summary>
	/// <param name="_actionEnable">토글에 연결된 콜백 실행 여부</param>
	/// <param name="_actionIdx">실행 또는 실행 취소할 콜백 Idx(Null이면 전체)</param>
	public void SelectToggle(ZToggle _toggle, bool _actionEnable = true, int[] _actionIdx = null)
	{
		SetAllToggleList();

		if (ToggleList.Count == 0)
			return;

		if (_actionEnable == false)
			_toggle.SetAction(_actionEnable, _actionIdx);

		if(!_toggle.group.allowSwitchOff)
			_toggle.isOn = true;

		for (int i = 0; i < ToggleList.Count; i++)
		{
			for (int j = 0; j < ToggleList[i].onValueChanged.GetPersistentEventCount(); j++)
			{
				if (ToggleList[i].isOn)
					if (_actionEnable == false)
						ToggleList[i].SetAction(!_actionEnable);
					else
						ToggleList[i].onValueChanged.SetPersistentListenerState(j, UnityEngine.Events.UnityEventCallState.Off);
				else
					ToggleList[i].onValueChanged.SetPersistentListenerState(j, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
			}

			ToggleList[i].GraphicUpdateComplete();
		}
    }

	/// <summary>특정 토글 오브젝트를 선택한다.</summary>
	public void SelectToggle(int _toggleIndex)
	{
		SetAllToggleList();

		ZToggle toggle = GetToggle(_toggleIndex);

		SelectToggle(toggle);
	}

	/// <summary>전체 토글 Off.</summary>
	public void SetAllToggleOff()
	{
		SetAllToggleList();

		if (!allowSwitchOff)
			return;

		for (int i = 0; i < ToggleList.Count; i++)
			ToggleList[i].isOn = false;
	}
}