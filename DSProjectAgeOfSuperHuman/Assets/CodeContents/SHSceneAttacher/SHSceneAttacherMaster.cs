using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSceneAttacherMaster : CSceneAttacherBase
{
	[SerializeField]
	protected string DefaultPrefabPath = "FrontPrefab";
	[SerializeField]
	protected string MasterPrefabName = "SHPrefabMaster";
	[SerializeField]
	protected string ScriptPrefabName = "SHPrefabScript";
	[SerializeField]
	protected string UISceneName = "UIMasterRoot";

	//----------------------------------------------------------------------------

	protected override void OnUnityAwake()
	{
		ProtSceneAttacherLoadResourcePrefab(DefaultPrefabPath, MasterPrefabName, (bool bAttach) =>
		{
			if (bAttach)
			{
				ProtSceneAttacherLoadUIScene(UISceneName, (bool bAttach) =>
				{
					if (bAttach)
					{
						ProtSceneAttacherLoadAddressablePrefab(ScriptPrefabName, (bool bAttach) =>
						{
							if (bAttach)
							{
								ProtSceneAttacherDestroy();
							}
						});
					}
				});
			}
		});
	}
}
