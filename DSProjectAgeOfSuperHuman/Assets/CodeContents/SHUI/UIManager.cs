using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EGameSceneType
{
	None,
	SHSceneCombatCity,
	SHSceneCombatHarbor,
	SHSceneLobby,
	SHSceneLogin,
	SHSceneScenario,
}

public class UIManager : CManagerUILocalizedBase
{ public static new UIManager Instance { get { return CManagerUILocalizedBase.Instance as UIManager; } }

	private float m_fRefreshTime = 0; public float GetUIInputRefresh() { return m_fRefreshTime; }
	private bool m_bRefreshLock = false;
	private bool m_bSceneLoadingStart = false;
	//-----------------------------------------------------------------------------
	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		if (m_bRefreshLock == false)
		{
			m_fRefreshTime += Time.deltaTime;
		}
	}

	protected override void OnUIMgrInitializeCanvas(Canvas pRootCanvas)
	{
		base.OnUIMgrInitializeCanvas(pRootCanvas);
	}

	//--------------------------------------------------------------------------------
	public TEMPLATE DoUIMgrShow<TEMPLATE>() where TEMPLATE : CUIFrameBase
	{
		return ProtMgrUIFrameFocusShow(typeof(TEMPLATE).Name) as TEMPLATE;
	}

	public TEMPLATE DoUIMgrHide<TEMPLATE>() where TEMPLATE : CUIFrameBase
	{
		return ProtMgrUIFrameFocusHide(typeof(TEMPLATE).Name) as TEMPLATE;
	}

	public void DoUIMgrHide(SHUIFrameBase pUIFrameHide)
	{
		ProtMgrUIFrameFocusHide(pUIFrameHide);
	}

	public void DoUIMgrClose()
	{
		ProtMgrUIFrameFocusClose();
	}

	public void DoUIMgrScreenLoadingScreenHide()
	{
		DoUIMgrHide<SHUIFrameLoadingScreen>();
	}

	public TEMPLATE DoUIMgrFind<TEMPLATE>() where TEMPLATE : CUIFrameBase
	{
		return FindUIFrame(typeof(TEMPLATE).Name) as TEMPLATE;
	}

	public void DoUIMgrFind<TEMPLATE>(UnityAction<TEMPLATE> delFinish) where TEMPLATE : CUIFrameBase
	{
		string strFrameName = typeof(TEMPLATE).Name;
		CUIFrameBase pUIFrame = FindUIFrame(strFrameName);
		if (pUIFrame)
		{
			delFinish?.Invoke(pUIFrame as TEMPLATE);
		}
		else
		{
			SHManagerResourceLoader.Instance.LoadComponent(strFrameName, (string strName, TEMPLATE pInstance) => { 
				if (pInstance)
				{
					ProtMgrUIFrameLoad(pInstance);
					delFinish?.Invoke(pInstance);
				}
			});
		}
	}

	public void DoUIMgrHidePanelAll()
	{
		ProtMgrUIFrameFocusPanelHideAll();
	}

	public void DoUIMgrInputRefresh(bool bLock = false)
	{
		m_fRefreshTime = 0;
		m_bRefreshLock = bLock;
	}

	public void DoUIMgrScreenIdle(bool bEnable)
	{
		SHUIFrameScreenIdleMode pScreen = DoUIMgrFind<SHUIFrameScreenIdleMode>();
		if (pScreen)
		{
			if (bEnable)
			{
				if (pScreen.pShow == false)
				{
					ProtMgrUIFrameFocusShow(pScreen);
				}
			}
			else
			{
				if (pScreen.pShow == true)
				{
					ProtMgrUIFrameFocusHide(pScreen);
				}
			}
		}
	}

	public void DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType PopupType, string strTitle, string strMessage, UnityAction delOk, UnityAction delCancel = null)
	{
		DoUIMgrShow<SHUIFrameMessagePopup>().DoMessagePopup(PopupType, strTitle, strMessage, delOk, delCancel);
	}

	public void DoUIMgrMessagePopupCurrency(SHUIFrameMessagePopup.EMessagePopupType PopupType, string strTitle, string strMessage, ECurrencyType eCurrencyType, long iCurrencyCount, UnityAction delOk, UnityAction delCancel = null)
	{
		DoUIMgrShow<SHUIFrameMessagePopup>().DoMessagePopupCurrency(PopupType, strTitle, strMessage, delOk, delCancel, eCurrencyType, iCurrencyCount);
	}

	public void DoUImgrMessageNotify(string strMessage, bool bStack)
	{
		if (bStack)
		{
			DoUIMgrShow<SHUIFrameMessageNotify>().DoUIFrameMessageStack(strMessage);
		}
		else
		{
			DoUIMgrShow<SHUIFrameMessageNotify>().DoUIFrameMessageFlow(strMessage);
		}
	}

	public void DoUIMgrScreenIndicator(bool bShow)
	{
		if (bShow)
		{
			DoUIMgrShow<SHUIFrameScreenIdicator>();
		}
		else
		{
			DoUIMgrHide<SHUIFrameScreenIdicator>();
		}
	}

	public void DoUIMgrRefreshCurrency()
	{
		DoUIMgrFind<SHUIFrameResource>().DoUIFrameResourceRefresh();	
	}

	//-------------------------------------------------------------------------------------------------
	public void DoUIMgrSceneLoadingStart(EGameSceneType eSceneName, UnityAction delFinish)
	{
		DoUIMgrSceneLoadingStart(eSceneName.ToString(), delFinish);
	}

	public void DoUIMgrSceneLoadingStart(string strSceneName, UnityAction delFinish)
	{
		if (m_bSceneLoadingStart)
		{
			Debug.LogError("[MainScene] ========================= Already Loading Start : " + strSceneName);
			return;
		}
		m_bSceneLoadingStart = true;

		ProtMgrUIFrameFocusPanelHideAll();
		SHManagerUnit.Instance.DoMgrUnitClearAll();
		SHManagerPrefabPool.Instance.ClearAll(true);
		SHUIFrameLoadingScreen pLoadingScreen = DoUIMgrShow<SHUIFrameLoadingScreen>();

		SHManagerSceneLoader.Instance.DoOpenScenMain(strSceneName, (float fProgress) =>
		{
			pLoadingScreen.DoUILoadingProgress(fProgress);
		}, (string strLoadedName) =>
		{
			m_bSceneLoadingStart = false;
			delFinish?.Invoke();
		});
	}

	public void DoUIMgrGotoLobby()
	{
		DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "게임 시스템",   "전투를 포기하고 로비로 이동하시겠습니까?", () => {
			DoUIMgrSceneLoadingStart(EGameSceneType.SHSceneLobby, ()=> {
				DoUIMgrHide<SHUIFrameLoadingScreen>();
				PrivUIMgrGotoLobby();
			});
		});
	}

	//----------------------------------------------------------------------------------
	private void PrivUIMgrGotoLobby()
	{
		DoUIMgrScreenIdle(false);
		DoUIMgrShow<SHUIFrameLobby>();
	}
}
