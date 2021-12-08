using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSceneControl : CManagerTemplateBase<EditorSceneControl>
{

	protected override void OnManagerScriptLoaded()
	{
		base.OnManagerScriptLoaded();
		UIManager.Instance.DoUIMgrShow<SHUIFrameNavigationBar>();

		Invoke("Dummy1", 0.5f);
		Invoke("Dummy2", 0.5f);
		Invoke("Dummy3", 0.5f);
	}

	private void Dummy1()
	{
		UIManager.Instance.DoUImgrMessageNotify("테스트 메시지 1111111", true);
	}
	private void Dummy2()
	{
		UIManager.Instance.DoUImgrMessageNotify("테스트 메시지 22222222", true);
	}
	private void Dummy3()
	{
		UIManager.Instance.DoUImgrMessageNotify("테스트 메시지 33333333", true);
	}


	//-----------------------------------------------------------
	static int Temp = 0;

	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Q))
		{
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
		}

		if (Input.GetKey(KeyCode.S))
		{
		}

		if (Input.GetKey(KeyCode.A))
		{
		}

		if (Input.GetKey(KeyCode.D))
		{
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			UIManager.Instance.DoUImgrMessageNotify("테스트 메시지 " + Temp++, true);
		}
	}
}
