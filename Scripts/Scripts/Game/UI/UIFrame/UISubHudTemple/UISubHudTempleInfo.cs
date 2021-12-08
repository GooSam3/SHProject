using MmoNet;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UISubHudTempleInfo : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Transform ChestRoot;
	[SerializeField] private GameObject ChestPrefab;
	[SerializeField] private Text TitleText;
	#endregion

	/// <summary> 유적 정보 표시 (입장 연출 이후에 표시하자) </summary>
	public void ShowTempleInfo()
	{
		var gameMode = ZGameModeManager.Instance.CurrentGameMode<ZGameModeTemple>();

		ClearChest();

		if (null != gameMode)
		{
			gameObject.SetActive(true);
			TitleText.text = DBLocale.GetText(gameMode.Table.StageTextID);
			//보물상자 추가
			AddChest(gameMode);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void AddChest(ZGameModeTemple gameMode)
	{
		float interval = 0.3f;
		for(int i = 0; i < gameMode.ChestList.Count; ++i)
		{
			float delayTime = interval * i;
			var go = GameObject.Instantiate(ChestPrefab, ChestRoot);
			go.SetActive(true);
			var comp = go.GetComponent<UISubHudTempleInfoChest>();
			
			comp.Set(gameMode.ChestList[i], delayTime);
		}
	}

	/// <summary> 보물상자 ui 제거 </summary>
	public void ClearChest()
	{
		ChestRoot.DestroyChildren();
	}
}