using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UIFrameScreenSaver : ZUIFrameBase
{
	[SerializeField] private ZButton BackgroundButton;
	[SerializeField] private Text AutoText, AutoOffText;
	[SerializeField] private ScreenSaverInfo screenSaverInfo;

	public override bool IsBackable => true;
	private int CharacterStateOrder;
	private int CurrencyOrder;
	protected override void OnInitialize()
	{
		base.OnInitialize();

		BackgroundButton.interactable = false;
	}

    protected override void OnRefreshOrder(int _LayerOrder)
    {
        base.OnRefreshOrder(_LayerOrder);

		//원래 Back에 있던 UI들을 Front로 옮겨준다.
		var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Front);

		var uiFrame = UIManager.Instance.TopMost<UISubHUDCharacterState>(true);
		uiFrame.transform.SetParent(targetCanvas.gameObject.transform, false);
		UIManager.Instance.SetLayer(uiFrame.gameObject, LayerMask.NameToLayer("UIFront"));

		var uiHudCurrency = UIManager.Instance.TopMost<UISubHUDCurrency>(true);
		uiHudCurrency.transform.SetParent(targetCanvas.gameObject.transform, false);
		UIManager.Instance.SetLayer(uiHudCurrency.gameObject, LayerMask.NameToLayer("UIFront"));
	}

    protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);

		BackgroundButton.interactable = true;
		CameraManager.Instance.DoSetVisible(false);
		UIManager.Instance.ScreenSaver = this;

		var Entry = ZPawnManager.Instance.MyEntity;
		if (Entry != null)
		{
			AutoText.gameObject.SetActive(Entry.IsAutoPlay);
			AutoOffText.gameObject.SetActive(!Entry.IsAutoPlay);
		}

		//전투정보 세팅
		screenSaverInfo.Initialize();
		InvokeRepeating(nameof(UpdateInfo), 0f, 1f);

		//골드, 경험치 갱신
		DropItemSpawner.DropGoldUpdate += OnUpdateGold;
		ZNet.Data.Me.CurCharData.ExpUpdated += OnUpdateExp;
	}

	public void CloseScreenSaverPanel()
	{
		if (Show)
		{
			CancelInvoke(nameof(UpdateInfo));

			BackgroundButton.interactable = false;
			CameraManager.Instance.DoSetVisible(true);
			UIManager.Instance.ScreenSaver = null;
			

			DropItemSpawner.DropGoldUpdate -= OnUpdateGold;
			ZNet.Data.Me.CurCharData.ExpUpdated -= OnUpdateExp;

			//레이어 원래대로 
			var targetCanvas = UIManager.Instance.GetCanvas(CManagerUIFrameFocusBase.E_UICanvas.Back);

			var uiFrame = UIManager.Instance.TopMost<UISubHUDCharacterState>(false);
			uiFrame.transform.SetParent(targetCanvas.gameObject.transform, false);
			UIManager.Instance.SetLayer(uiFrame.gameObject, LayerMask.NameToLayer("UI"));

			var uiHudCurrency = UIManager.Instance.TopMost<UISubHUDCurrency>(false);
			uiHudCurrency.transform.SetParent(targetCanvas.gameObject.transform, false);

			UIManager.Instance.SetLayer(uiHudCurrency.gameObject, LayerMask.NameToLayer("UI"));

			UIManager.Instance.Close<UIFrameScreenSaver>();

		}
	}

	public void UpdateInfo()
    {
		//ZLog.Log(ZLogChannel.UI, "Update ScreenSaver Info");
		screenSaverInfo.UpdateInfo();
    }

	public void OnUpdateGold(ulong amount)
    {
		//ZLog.Log(ZLogChannel.UI, "ScreenSaver Gold : " + amount);
		screenSaverInfo.UpdateGold(amount);
    }

	public void OnUpdateExp(ulong preExp, ulong newExp, bool isKill)
    {
		//ZLog.Log(ZLogChannel.UI, "ScreenSaver Exp : " + preExp + " ==> " + newExp + "// kill : " + isKill);
		screenSaverInfo.updateExp(preExp, newExp, isKill);
	}
}

[Serializable]
public class ScreenSaverInfo
{
	[SerializeField] private Text LocalAreaNameText, HuntTimeText, TotalGoldText, GetGoldText, TotalExpText, ExpPerHourText, KillMonsterText, KillPerHourText;

	public ulong HuntStartTime; // 사냥 시작 시간
	public ulong GetGold;
	public ulong TotalExp;
	public ulong ExpPerHour;
	public ulong KillMonster;
	public ulong KillPerHour;

	public ulong SaverTime; // 켜진 시간
	public void Initialize()
    {
		if (ZGameModeManager.Instance.StageTid != 0)
			LocalAreaNameText.text = DBLocale.GetStageName(ZGameModeManager.Instance.StageTid);
		else
			LocalAreaNameText.text = "";

		HuntStartTime = TimeManager.NowSec;

		GetGold = 0;

		TotalExp = 0;

		ExpPerHour = 0;

		KillMonster = 0;

		KillPerHour = 0;

		SaverTime = 0;
	}

	public void UpdateInfo()
    {
		SaverTime++;
		HuntTimeText.text = TimeHelper.CompareNow(HuntStartTime);
		TotalGoldText.text = ZNet.Data.Me.GetCurrency(DBConfig.Gold_ID).ToString();
		GetGoldText.text = GetGold.ToString();
		TotalExpText.text = TotalExp.ToString();
		ExpPerHourText.text = ExpPerHour.ToString() + "/h";
		KillMonsterText.text = KillMonster.ToString();
		KillPerHourText.text = KillPerHour.ToString() + "/h";
	}

	public void UpdateGold(ulong amount)
    {
		GetGold += amount;
    }

	public void updateExp(ulong preExp, ulong newExp, bool iskill)
    {
		if(newExp >= preExp)
		{
			TotalExp += newExp - preExp;
		}
		else
		{
			return;
		}

		//지금까지 얻은 경험치 / 지금까지 지난 시간
		// exp/s
		ExpPerHour = (TotalExp * 3600) / SaverTime;

		if(iskill)
			KillMonster++;

		KillPerHour = (KillMonster * 3600) / SaverTime;

    }

}