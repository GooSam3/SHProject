#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Tools
{
	public class FXSoundAudioItem : MonoBehaviour
	{
		[SerializeField] private ZText tableName;
		[SerializeField] private GameObject select;

		private uint soundTid;
		private Action<uint> clickItem;

		public void SetData(FXSoundData data, Action<uint> _clickItem)
		{
			select.SetActive(data.IsSelected);

			soundTid = data.SoundID;
			clickItem = _clickItem;

			//string noSound = (data.SoundID == 0) ? " : <color=#ffff00>no snd</color>" : "";

			//if (data.IsChange || data.EffectDelayTime_SV > 0) {
			//	var ori = $"{ data.EffectFile}   <color=#ffff00>{data.EffectDelayTime}</color>";
			//	var chg = $"<color=#F14848>{data.EffectDelayTime_SV}</color>";
			//	tableName.text = $"{ori} > {chg} {noSound}";
			//}
			//else {
			//	if (data.EffectDelayTime > 0) {
			//		tableName.text = $"{ data.EffectFile}   <color=#ffff00>{data.EffectDelayTime}</color> {noSound}";
			//	}
			//	else {
			//		tableName.text = $"{ data.SoundFile} {noSound}";
			//	}
			//}

			tableName.text = $"{ data.SoundFile}";
		}

		public void OnClickItem()
		{
			clickItem?.Invoke(soundTid);
		}
	}
}

#endif