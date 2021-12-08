#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Tools
{
	public class FXSoundItem : MonoBehaviour
	{
		[SerializeField] private ZText tableName;
		[SerializeField] private GameObject select;

		private uint effectTid = 0;
		private Action<uint> clickItem = null;

		public void SetData(FXSoundEffectData data, Action<uint> _clickItem)
		{
			select.SetActive(data.IsSelected);

			effectTid = data.EffectID;
			clickItem = _clickItem;

			string noSound = (data.EffectSoundID == 0) ? " : <color=#ffff00>no snd</color>" : "";

			if(data.IsChange || data.EffectDelayTime_SV > 0 ) {
				var ori = $"{ data.EffectFile}   <color=#ffff00>{data.EffectDelayTime}</color>";
				var chg = $"<color=#F14848>{data.EffectDelayTime_SV}</color>";
				tableName.text = $"{ori} > {chg} {noSound}";
			}
			else {
				if (data.EffectDelayTime > 0) {
					tableName.text = $"{ data.EffectFile}   <color=#ffff00>{data.EffectDelayTime}</color> {noSound}";
				}
				else {
					tableName.text = $"{ data.EffectFile} {noSound}";
				}
			}
		}

		public void OnClickItem()
		{
			clickItem?.Invoke(effectTid);
		}
	}
}

#endif