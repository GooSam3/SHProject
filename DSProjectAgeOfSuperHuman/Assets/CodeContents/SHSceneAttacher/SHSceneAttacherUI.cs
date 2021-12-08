using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSceneAttacherUI : SHSceneAttacherMaster
{
	protected override void OnUnityAwake()
	{
		ProtSceneAttacherLoadResourcePrefab(DefaultPrefabPath, MasterPrefabName, (bool bAttach) =>
		{
			if (bAttach)
			{
				ProtSceneAttacherLoadAddressablePrefab(ScriptPrefabName, (bool bAttach) =>
				{

				});
			}
			else
			{
				ProtSceneAttacherDestroy();
			}
		});
	}
}
