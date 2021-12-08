using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 오브젝트 좌표 기반 이름표 출력 기능 구현 
// 출력되는 UI가 아니라 갱신 및 관리기능만 집약해서 구현 

abstract public class CUIFrameNameTagBase : CUIFrameBase
{
	private class KeyCompare<TKey> : IComparer<TKey> where TKey : IComparable
	{	public int Compare(TKey x, TKey y)
		{   int Result = x.CompareTo(y);
			if (Result == 0)
				return 1;
			else
				return Result;
		}
	}

	private bool mUpdateNameTag = true;
	private Dictionary<GameObject, CUIWidgetNameTagBase>  m_dicNameTagInstance = new Dictionary<GameObject, CUIWidgetNameTagBase>();
	private SortedList<float, CUIWidgetNameTagBase>       m_listSortNote = new SortedList<float, CUIWidgetNameTagBase>(new KeyCompare<float>());
	
	//--------------------------------------------------------------
	protected override void OnInitialize()
	{
		base.OnInitialize();
		mRectTransform.SetAnchor(AnchorPresets.StretchAll);
		m_listSortNote.Capacity = 100;
	}

	//--------------------------------------------------------------
	protected void UpdateNameTag(Camera _mainCamera, Vector3 _originPosition)
	{
		if (mUpdateNameTag)
		{
			UpdateNameTagVerify();
			UpdateNameTagInternal(_mainCamera, _originPosition);
		}
	}

	//--------------------------------------------------------------
	protected void NameTagAdd(GameObject _owner,  CUIWidgetNameTagBase _nameTagInstance)
	{
		_nameTagInstance.transform.SetParent(transform, false);
		AddUIWidget(_nameTagInstance);
		m_dicNameTagInstance[_owner] = _nameTagInstance;
	}

	protected void NameTagRemove(GameObject _owner)
	{
		if (m_dicNameTagInstance.ContainsKey(_owner))
		{
			CUIWidgetNameTagBase NameTag = m_dicNameTagInstance[_owner];
			NameTag.DoNameTagRemove();
			m_dicNameTagInstance.Remove(_owner);
			OnNameTagRemove(NameTag);			
		}		
	}

	protected void NameTagRemove(CUIWidgetNameTagBase _nameTagInstance)
	{
		Dictionary<GameObject, CUIWidgetNameTagBase>.Enumerator it = m_dicNameTagInstance.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.Value == _nameTagInstance)
			{
				_nameTagInstance.DoNameTagRemove();
				OnNameTagRemove(_nameTagInstance);
				m_dicNameTagInstance.Remove(it.Current.Key);
				break;
			}
		}
	}

	protected CUIWidgetNameTagBase FindNameTag(GameObject _owner)
	{
		CUIWidgetNameTagBase find = null;
		if (m_dicNameTagInstance.ContainsKey(_owner))
		{
			find = m_dicNameTagInstance[_owner];
		}

		return find;
	}

	protected void NameTagClearAll()
	{
		List<GameObject> listNameTag = m_dicNameTagInstance.Keys.ToList();
		for (int i = 0; i < listNameTag.Count; i++)
		{
			NameTagRemove(listNameTag[i]);
		}
		m_listSortNote.Clear();
	}

	protected List<CUIWidgetNameTagBase> NameTagList()
	{
		return m_dicNameTagInstance.Values.ToList();
	}

	//--------------------------------------------------------------------
	private void UpdateNameTagVerify()
	{
		List<CUIWidgetNameTagBase> listNameTag = m_dicNameTagInstance.Values.ToList();

		for (int i = 0; i < listNameTag.Count; i++)
		{
			CUIWidgetNameTagBase nameTag = listNameTag[i];
			if (nameTag.IsNameTagValid() == false)
			{
				NameTagRemove(nameTag);
			}
		}
	}
	private void UpdateNameTagInternal(Camera _mainCamera, Vector3 _originPosition)
	{
		m_listSortNote.Clear();
		UpdateNameTagArrange(m_listSortNote, _originPosition);
		UpdateNameTagInstance(m_listSortNote, _mainCamera);
	}

	private void UpdateNameTagArrange(SortedList<float, CUIWidgetNameTagBase> _listSortNote, Vector3 _originPosition)
	{
		Dictionary<GameObject, CUIWidgetNameTagBase>.Enumerator it = m_dicNameTagInstance.GetEnumerator();
		while(it.MoveNext())
		{
			CUIWidgetNameTagBase nameTag = it.Current.Value;

			Vector3 origin = _originPosition;
			origin.y = 0;
			Vector3 dest = nameTag.GetNameTagWorldPosition();
			dest.y = 0;
			Vector3 Distance = dest - origin;
			float Length = Distance.sqrMagnitude;

			_listSortNote.Add(Length, nameTag);
			OnNameTagDistance(nameTag, Length);
		}
	}

	private void UpdateNameTagInstance(SortedList<float, CUIWidgetNameTagBase> _listSortNote, Camera _mainCamera)
	{
		IEnumerator<KeyValuePair<float, CUIWidgetNameTagBase>> it = _listSortNote.GetEnumerator();
		int count = 10000;
		while(it.MoveNext())
		{
			it.Current.Value.UpdateNameTag(_mainCamera, mUIFrameSize, count--);
		}
		_listSortNote.Clear();
	}

	//--------------------------------------------------------------
	protected virtual void OnNameTagRemove(CUIWidgetNameTagBase _nameTag) { }
	protected virtual void OnNameTagDistance(CUIWidgetNameTagBase _nameTag, float _distance) { }
}
