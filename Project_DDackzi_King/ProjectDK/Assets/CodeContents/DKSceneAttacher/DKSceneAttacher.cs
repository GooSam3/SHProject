using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKSceneAttacher : CSceneAttacherBase
{
	[SerializeField]
	protected string MasterPrefabPath = "DefaultPrefab";
	[SerializeField]
	protected string MasterPrefabName = "MasterPrefab";
	[SerializeField]
	protected string ScriptPrefabName = "DKScriptData";
	[SerializeField]
	protected string UISceneName = "UIMasterRoot";
	//-----------------------------------------------------------
	protected override void OnUnityAwake()
	{		
		ProtSceneAttacherLoadResourcePrefab(MasterPrefabPath, MasterPrefabName, (bool bAttach) =>
		{
			if (bAttach)
			{
				ProtSceneAttacherLoadUIScene(UISceneName, (bool bAttach) => { 
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
