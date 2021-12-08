using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPetAdventureMapPoint : MonoBehaviour
{
	[Serializable]
	private class MapPoint
	{
		[SerializeField] private Text txtTitle;
		[SerializeField] private Text txtSubTitle;

		[SerializeField] private Image icon;

		private OSA_AdventureData data;

		public void Initialize(OSA_AdventureData _data)
		{
			data = _data;
			txtTitle.text = DBLocale.GetText(data.table.AdventureNameText);
			icon.sprite = UICommon.GetSprite(data.table.AdventureIcon);

			UpdateRemainTime((long)data.advData.EndDt - (long)TimeManager.NowSec);
		}

		public void UpdateRemainTime(long remaintime)
		{
			switch (data.advData.status)
			{
				case WebNet.E_PetAdvStatus.Wait:
					txtSubTitle.gameObject.SetActive(false);
					break;
				case WebNet.E_PetAdvStatus.Start:
					txtSubTitle.gameObject.SetActive(true);

					if (remaintime < 0)// 완료
					{
						txtSubTitle.text = DBLocale.GetText("PetAdventur_End");
					}
					else
					{
						txtSubTitle.text = TimeHelper.GetRemainTime((ulong)remaintime);
					}

					break;

				case WebNet.E_PetAdvStatus.Reward://시간갱신
				case WebNet.E_PetAdvStatus.Cancel:
					txtSubTitle.text = TimeHelper.GetRemainTime((ulong)remaintime);
					txtSubTitle.gameObject.SetActive(remaintime > 0);
					break;
			}
		}
	}

	[SerializeField] private MapPoint mapPointOn;
	[SerializeField] private MapPoint mapPointOff;

	[SerializeField] private Image imgMapPointPin;

	[SerializeField] private ZToggle toggle;

	// 인스펙터에서만 변경, 코드 내 변경금지!!!
	[SerializeField, Header("반드시 입력, 미입력시 꺼짐")] private uint targetAdventureTid;
	public uint TargetTID => targetAdventureTid;

	private OSA_AdventureData data;
	private Action<OSA_AdventureData> onClick;

	public void Initialize(OSA_AdventureData _data, Action<OSA_AdventureData> _onClick)
	{
		data = _data;
		onClick = _onClick;

		mapPointOn.Initialize(data);
		mapPointOff.Initialize(data);

		imgMapPointPin.color = UICommon.GetPetAdventurePinColor(data.table.AdventureIcon);
	}

	public void SetRadioOn()
	{
		toggle.SelectToggle(false);
	}

	// 유니티 이벤트
	public void OnClickSlot()
	{
		if (toggle.isOn == false)
			return;

		onClick?.Invoke(data);
	}

	public void RefrshRemainTime()
	{
		var remainTime = (long)data.advData.EndDt - (long)TimeManager.NowSec;
		mapPointOn.UpdateRemainTime(remainTime);
		mapPointOff.UpdateRemainTime(remainTime);
	}
}
