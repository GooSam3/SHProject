using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBossWarScheduleGridItem : MonoBehaviour
{
	[SerializeField] private ZText SpawnTime;
	[SerializeField] private GameObject CurrentSpawnTimeImage;
	[SerializeField] private ZImage SpawnTimeBackground;
	
	public void SetData(UIBossWarScheduleGridItemData data)
	{
		uint hour = (uint)data.SpawnTime / 3600;
		uint minute = (uint)data.SpawnTime % 3600 / 60;

		SpawnTime.text = $"{hour} : {minute}";

		CurrentSpawnTimeImage.SetActive(data.IsCurrentSpawnTime);

		if (data.IsExpire)
		{
			SpawnTimeBackground.color = new Color(SpawnTimeBackground.color.r, SpawnTimeBackground.color.g, SpawnTimeBackground.color.b, 0.25f);
			SpawnTime.color = new Color32(83, 85, 94, 255);
		}
		else
		{
			SpawnTimeBackground.color = new Color(SpawnTimeBackground.color.r, SpawnTimeBackground.color.g, SpawnTimeBackground.color.b, 1f);
			SpawnTime.color = new Color32(255, 255, 255, 255);
		}
	}
}
