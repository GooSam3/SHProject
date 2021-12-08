using MmoNet;
using System.Collections;
using UnityEngine;
using Zero;
using ZNet;
using System;

/// <summary>
/// 인게임상에서 드랍 연출을 위한 기능 클래스
/// </summary>
public class DropItemSpawner
{
	/*
	 * 리소스명 직접 입력. 한번개발후 바뀐적이 없어서 상수로 넣어둠.
	 */
	static string Prefab_Fx_Tier1 = "Fx_DropItem_Tier1";
	static string Prefab_Fx_Tier2 = "Fx_DropItem_Tier2";
	static string Prefab_Fx_Tier3 = "Fx_DropItem_Tier3";
	static string Prefab_Fx_Tier4 = "Fx_DropItem_Tier4";
	static string Prefab_Fx_Tier5 = "Fx_DropItem_Tier5";
	static string Prefab_Fx_Tier6 = "Fx_DropItem_Tier6";

	static string Prefab_Item_Armor = "Model_DropItem_Armor";
	static string Prefab_Item_Sword = "Model_DropItem_Sword";
	static string Prefab_Item_Money = "Model_DropItem_Money"; //재화류
	static string Prefab_Item_Pack = "Model_DropItem_Pack"; //기타
	static string Prefab_Item_Rune = "Model_DropItem_Rune"; //룬
	static string Prefab_Item_EnchantScroll = "Model_DropItem_EnchantScroll"; //주문서
	static string Prefab_Item_Case = "Model_DropItem_Case"; //상자

	public static Action<ulong> DropGoldUpdate;

	/// <summary> 이카루스~ </summary>
	public static void DropItem(ref S2C_DropItemInfos dropItemInfos)
	{
		Vector3 startPos = ZMmoFieldReceiver.ToVector3(dropItemInfos.Pos);
		int dropCount = dropItemInfos.ItemsLength;

		float spawnDelay = 0;
		float spawnRange = (float)DBConfig.ItemDropEffect_Max_Radius * ((float)dropCount / DBConfig.ItemDropEffect_Max_Standard_Cnt);
		spawnRange = Mathf.Max(spawnRange, 2f);

		for (int i = 0; i < dropCount; i++)
		{
			var dropItem = dropItemInfos.Items(i).Value;
			ZPawn targetPawn = ZPawnManager.Instance.GetEntity(dropItem.Objectid);

			new Task(SpawnDropItem(targetPawn?.transform, 
				spawnDelay, 
				startPos, 
				spawnRange, 
				dropItem.ItemTid, 
				dropItem.Cnt));

			spawnDelay += 0.08f;

			if(true == targetPawn?.IsMyPc && dropItem.ItemTid == DBConfig.Gold_ID ) {
				DropGoldUpdate?.Invoke( dropItem.Cnt );
			}
		}
	}

	public static IEnumerator SpawnDropItem(Transform lootingTarget, float delay, Vector3 centerPos, float range, uint itemTid, ulong itemCount = 1)
	{
		yield return new WaitForSeconds(delay);

		DropItem(lootingTarget, centerPos, range, itemTid, itemCount);
	}

	/// <summary> 드랍될 아이템 연출 생성 </summary>
	public static void DropItem(Transform lootingTarget, Vector3 centerPos, float range, uint itemTid, ulong itemCount = 1)
	{
		if (!DBItem.GetItem(itemTid, out var itemTable))
			return;

		string prefabName = GetModelPrefab(itemTable);
		if (string.IsNullOrEmpty(prefabName))
			return;

		Vector3 dropPos = VectorHelper.RandomCircleXZ(centerPos, range);

		// 위치 보정
		dropPos = NavHelper.GetAdjustedNavPos(dropPos, 5f, 1 /*Walkable*/);

		ZPoolManager.Instance.Spawn(E_PoolType.Effect, prefabName, (spawnedGO) =>
		{
			spawnedGO.transform.SetPositionAndRotation(dropPos, Quaternion.identity);
			// 객체 반환시 사용하기 위해 이름 설정.
			spawnedGO.name = prefabName;

			var dropComponent = spawnedGO.GetOrAddComponent<DropItemComponent>();
			dropComponent.SetAndStart(lootingTarget, spawnedGO, GetFxPrefab(itemTable));

			var nameTag = UIManager.Instance.Find<UIFrameNameTag>();
			if( nameTag != null ) {
				nameTag.DoUINameTagDropItem( dropComponent, itemTable );
			}
			
			//ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIFloatingText), (uiGO) =>
			//{
			//	UIFloatingText targetUI = uiGO.GetComponent<UIFloatingText>();
			//	if (null == targetUI)
			//		return;

			//	if (null != dropComponent)
			//	{
			//		dropComponent.OnShowTierEffect = () =>
			//		{
			//			if (null != targetUI)
			//			{
			//				targetUI.Set(spawnedGO.transform, DBLocale.GetText(itemTable.ItemTextID), GetTierColor(itemTable.DropEffectGrade));
			//				// TODO : 모델을 가리기때문에, 조절 기능 필요
			//				//targetUI.OffsetY = 20f;
			//			}
			//		};

			//		dropComponent.OnHideTierEffect = () =>
			//		{
			//			// 반환필요
			//			if (null != targetUI)
			//				GameObject.Destroy(targetUI.gameObject);
			//		};
			//	}
			//	else
			//	{
			//		// 반환필요
			//		if (null != targetUI)
			//			GameObject.Destroy(targetUI.gameObject);
			//	}
			//});
		});
	}

	public static string GetModelPrefab(GameDB.Item_Table itemTable)
	{
		string prefabName = null;

		uint itemTid = itemTable.ItemID;

		switch (itemTable.DropModelType)
		{
			case GameDB.E_DropModelType.Pack: /*주머니*/
				prefabName = Prefab_Item_Pack;
				break;
			case GameDB.E_DropModelType.Money: /*골드*/
				prefabName = Prefab_Item_Money;
				break;
			case GameDB.E_DropModelType.Sword: /*무기,보조무기*/
				prefabName = Prefab_Item_Sword;
				break;
			case GameDB.E_DropModelType.Armor: /*방어구*/
				prefabName = Prefab_Item_Armor;
				break;
			case GameDB.E_DropModelType.Rune:
				prefabName = Prefab_Item_Rune;
				break;
			case GameDB.E_DropModelType.EnchantScroll:
				prefabName = Prefab_Item_EnchantScroll;
				break;
			case GameDB.E_DropModelType.Case:
				prefabName = Prefab_Item_Case;
				break;
		}

		return prefabName;
	}

	public static string GetFxPrefab(GameDB.Item_Table itemTable)
	{
		switch (itemTable.DropEffectGrade)
		{
			case 1: return Prefab_Fx_Tier1;
			case 2: return Prefab_Fx_Tier2;
			case 3: return Prefab_Fx_Tier3;
			case 4: return Prefab_Fx_Tier4;
			case 5: return Prefab_Fx_Tier5;
			case 6: return Prefab_Fx_Tier6;

			default:
				return null;
		}
	}

	public static Color GetTierColor(byte grade)
	{
		switch (grade)
		{
			case 1: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier1;
			case 2: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier2;
			case 3: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier3;
			case 4: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier4;
			case 5: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier5;
			case 6: return ResourceSetManager.Instance.SettingRes.Palette.Item_Tier6;

			default:
				return Color.white;
		}
	}
}
