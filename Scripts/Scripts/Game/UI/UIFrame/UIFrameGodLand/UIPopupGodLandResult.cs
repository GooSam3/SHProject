using GameDB;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

public class UIPopupGodLandResult : MonoBehaviour
{
	[SerializeField] private UIPopupGodLandResultItem winItem;
	[SerializeField] private UIPopupGodLandResultItem loseItem;
	[SerializeField] private UIPopupGodLandResultItem completeItem;

	public void ShowCompletePopup(uint getItemTid, ulong getCnt, uint timeCnt)
	{
		gameObject.SetActive(true);

		winItem.Hide();
		loseItem.Hide();

		completeItem.ShowCompletePopup(getItemTid, getCnt, timeCnt);
	}

	public void ShowResultPopup(uint godLandTid, bool iWin)
	{
		gameObject.SetActive(true);

		if (iWin) {
			loseItem.Hide();
			completeItem.Hide();

			winItem.ShowResultPopup(godLandTid, true);
		}
		else {
			winItem.Hide();
			completeItem.Hide();

			loseItem.ShowResultPopup(godLandTid, false);
		}
	}

	public void Hide()
	{
		winItem.Hide();
		loseItem.Hide();
		completeItem.Hide();

		gameObject.SetActive(false);
	}

	public void OnClickGameEnd()
	{
		UIManager.Instance.Close<UISubHUDGodLand>();

		ZGameManager.Instance.DoForceMapMove(DBConfig.Town_Stage_ID);

		//밖에 나가면 성지창을 다시 띄어줘야 한다
		ZGameModeManager.Instance.ReserveActionSceneLoadedComplete((modeType) => {
			if (modeType == E_GameModeType.Field) {
				UIManager.Instance.Open<UIFrameGodLandMap>();
			}
		});
	}
}