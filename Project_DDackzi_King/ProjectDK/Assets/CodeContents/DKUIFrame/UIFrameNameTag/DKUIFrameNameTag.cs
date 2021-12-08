using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUIFrameNameTag : DKUIFrameBase
{
	public enum ENameTagType
	{
		None,
		HPBarHero,
		HPBarEnemy,
		Name,

	}

	[System.Serializable]
	private class SNameTagInfo
	{
		public ENameTagType NameTagType = ENameTagType.None;
		public CUIWidgetTemplate	Template = null;
	}
	[SerializeField]
	private List<SNameTagInfo> NameTagTemplate = new List<SNameTagInfo>();

	private LinkedList<DKUIWidgetFollowTag> m_listFollowTagInstance = new LinkedList<DKUIWidgetFollowTag>();
	//--------------------------------------------------------------------------
	public void DoUIFrameNameTagMake(DKUnitBase pUnitTarget, ENameTagType eNameTagType)
	{
		CUIWidgetTemplate pTemplate = FindNameTagTemplate(eNameTagType);
		if (pTemplate != null)
		{
			PrivUIFrameNameTagMakeInstance(pUnitTarget, pTemplate);
		}
	}

	public void DoUIFrameNameTagRemove(DKUnitBase pUnitTarget)
	{
		LinkedList<DKUIWidgetFollowTag>.Enumerator it = m_listFollowTagInstance.GetEnumerator();
		while(it.MoveNext())
		{
			if (it.Current.GetFollowUnit() == pUnitTarget)
			{
				it.Current.DoFollowTagRemove();
				m_listFollowTagInstance.Remove(it.Current);
				break;
			}
		}
	}

	public void DoUIFrameNameTagRemoveAll()
	{
		LinkedList<DKUIWidgetFollowTag>.Enumerator it = m_listFollowTagInstance.GetEnumerator();
		while (it.MoveNext())
		{
			it.Current.DoFollowTagRemove();
		}
		m_listFollowTagInstance.Clear();
	}

	//--------------------------------------------------------------------------
	private void PrivUIFrameNameTagMakeInstance(DKUnitBase pUnitTarget, CUIWidgetTemplate pTemplate)
	{
		DKUIWidgetFollowTag pFollowTag = pTemplate.DoUIWidgetTemplateRequestItem(transform) as DKUIWidgetFollowTag;
		if (pFollowTag != null)
		{
			pFollowTag.DoFollowTag(pUnitTarget);
			m_listFollowTagInstance.AddLast(pFollowTag);
		}
	}

	private CUIWidgetTemplate FindNameTagTemplate(ENameTagType eNameTagType)
	{
		CUIWidgetTemplate pTemplate = null;
		for (int i = 0; i < NameTagTemplate.Count; i++)
		{
			if (NameTagTemplate[i].NameTagType == eNameTagType)
			{
				pTemplate = NameTagTemplate[i].Template;
				break;
			}
		}

		return pTemplate;
	}
}
