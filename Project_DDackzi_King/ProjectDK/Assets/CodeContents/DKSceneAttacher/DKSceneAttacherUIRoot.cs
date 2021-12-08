using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSceneAttacherUIRoot : DKSceneAttacher
{
	//---------------------------------------------------------
	protected override void OnUnityAwake()
	{
		ProtSceneAttacherLoadResourcePrefab(MasterPrefabPath, MasterPrefabName, (bool bAttach) => {
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
