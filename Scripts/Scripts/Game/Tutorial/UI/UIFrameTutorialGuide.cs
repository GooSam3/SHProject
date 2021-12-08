using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 튜토리얼 UI Dialogue </summary>
public class UIFrameTutorialGuide : MonoBehaviour
{
	[SerializeField]
	private Text TextName;

	[SerializeField]
	private Text TextDesc;

	[SerializeField]
	private RawImage NpcImage;

	public void Set(string name, string desc, string resourceName)
	{
		TextDesc.text = DBLocale.GetText(desc);
		TextName.text = DBLocale.GetText(name);

		if(false == string.IsNullOrEmpty(resourceName))
		{			
			ZResourceManager.Instance.Load<Texture>(resourceName, (stringName, tex) => 
			{
				if(null == tex)
				{
					NpcImage.gameObject.SetActive(true);
					NpcImage.texture = tex;
				}
				else
				{
					ZLog.LogError(ZLogChannel.Quest, $"[{resourceName}] 해당 Texture가 없습니다. (다른 프리펩 이름과 중복 확인바람.)");
				}
			});
		}
		else
		{
			NpcImage.gameObject.SetActive(false);
		}
	}
}