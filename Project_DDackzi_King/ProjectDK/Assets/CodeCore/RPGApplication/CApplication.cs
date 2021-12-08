using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CApplication : CManagerTemplateBase<CApplication>
{
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		Application.targetFrameRate = 60;
	}
}
