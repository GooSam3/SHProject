using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIWidgetButtonLockBase : CUIWidgetButtonNoticeBase
{
	private static List<int>						g_listButtonLockID = new List<int>();
	private static List<CUIWidgetButtonLockBase>	g_listButtonLockInstance = new List<CUIWidgetButtonLockBase>();
    [SerializeField][Header("[Button Lock]")]
    private CImage LockImage = null;
    [SerializeField]
    private int LockID = 0;

	private bool m_bLock = false; 
	//-----------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		if (LockImage) 
		{
			LockImage.gameObject.SetActive(false);
			if (LockID != 0)
			{
				g_listButtonLockInstance.Add(this);
				PrivButtonLockRefresh(LockID);
			}
		}
	}

	protected override void OnButtonClick()
	{
		if (m_bLock == false)
		{
			base.OnButtonClick();
		}
		else
		{
			OnButtonLockInteractionWithLock(LockID);
		}
	}

	//------------------------------------------------------------------------
	public static void GlobalButtonLock(int hLockID, bool bAdd)
	{
		if (bAdd)
		{
			if (g_listButtonLockID.Contains(hLockID) == false)
			{
				g_listButtonLockID.Add(hLockID);
			}
		}
		else
		{
			if (g_listButtonLockID.Contains(hLockID))
			{
				g_listButtonLockID.Remove(hLockID);
			}
		}

		for(int i = 0; i < g_listButtonLockInstance.Count; i++)
		{
			g_listButtonLockInstance[i].ImportButtonLockRefresh(hLockID, bAdd);
		}
	}

	internal void ImportButtonLockRefresh(int hLockID, bool bAdd)
	{
		PrivButtonLockEnable(hLockID, bAdd);
	}

	//-----------------------------------------------------------------------
	private void PrivButtonLockRefresh(int hLockID)
	{
		if (g_listButtonLockID.Contains(hLockID))
		{
			PrivButtonLockEnable(hLockID, true);
		}
		else
		{
			PrivButtonLockEnable(hLockID, false);
		}
	}

	private void PrivButtonLockEnable(int hLockID, bool bEnable)
	{
		m_bLock = bEnable;
		if (bEnable)
		{
			LockImage.gameObject.SetActive(true);
		}
		else
		{
			LockImage.gameObject.SetActive(false);
		}
		OnButtonLockRefresh(hLockID, bEnable);
	}

	//------------------------------------------------------------------------
	protected virtual void OnButtonLockRefresh(int hLockID, bool bAdd) { }
	protected virtual void OnButtonLockInteractionWithLock(int hLockID) { }
}
