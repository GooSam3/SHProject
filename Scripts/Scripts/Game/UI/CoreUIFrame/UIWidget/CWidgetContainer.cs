using System.Collections.Generic;
using UnityEngine;

public class CWidgetContainer : CMonoBase
{
	[System.Serializable]
	public class SContainer
	{
		[SerializeField]
		public string ObjectName = "None";
		[SerializeField]
		public GameObject Object = null;
	}

	[SerializeField]
	private List<SContainer> Container = new List<SContainer>();
	//---------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		HideAll();
	}

	//---------------------------------------------------------------------------
	public GameObject SwitchContainerObject(string _ObjectName)
	{
		GameObject SwitchObject = null;

		for (int i = 0;  i < Container.Count; i++)
		{
			if (Container[i].ObjectName == _ObjectName)
			{
				SwitchObject = Container[i].Object;
				SwitchObject.SetActive(true);				
			}
			else
			{
				Container[i].Object.SetActive(false);
			}
		}

		return SwitchObject;
	}
	
	public void HideAll()
	{
		for (int i = 0; i < Container.Count; i++)
		{
			Container[i].Object.SetActive(false);
		}
	}

}
