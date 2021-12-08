using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEffectTransformTweenBase : CEffectBase
{
	[System.Serializable]
	public class STransformTween
	{
		[SerializeField]
		public Jun_TweenRuntime Tween;

		[HideInInspector]
		public Vector3 OriginVectorFrom = Vector3.zero;
		[HideInInspector]
		public Vector3 OriginVectorTo = Vector3.zero;
		[HideInInspector]
		public Color OriginColorFrom = Color.white;
		[HideInInspector]
		public Color OriginColorTo = Color.white;
		[HideInInspector]
		public float OriginValueFrom = 0;
		[HideInInspector]
		public float OriginValueTo = 0;

		public void InitTransfromTween()
		{
			if (Tween != null)
			{
				Tween.enablePlay = false;
				Tween.awakePlay = false;
				Tween.ignoreTimeScale = false;

				OriginVectorFrom = Tween.firstTween.fromVector;
				OriginVectorTo = Tween.firstTween.toVector;
				OriginColorFrom = Tween.firstTween.fromColor;
				OriginColorTo = Tween.firstTween.toColor;
				OriginValueFrom = Tween.firstTween.fromValue;
				OriginValueTo = Tween.firstTween.toValue;
			}
		}
	}

	[SerializeField]
	private List<STransformTween> TweenList = new List<STransformTween>();

	//-----------------------------------------------------------
	protected override void OnEffectInitialize()
	{
		base.OnEffectInitialize();
		for (int i = 0; i < TweenList.Count; i++)
		{
			TweenList[i].InitTransfromTween();
		}
	}


	//---------------------------------------------------------------
	protected STransformTween FindTransformTween(int iIndex)
	{
		STransformTween pTransTween = null;

		if (iIndex < TweenList.Count)
		{
			pTransTween = TweenList[iIndex];
		}
		return pTransTween;
	}

	protected void ProtTransformTweenStopAll()
	{
		for (int i = 0; i < TweenList.Count; i++)
		{
			TweenList[i].Tween.enabled = false;
			TweenList[i].Tween.firstTween.Init(null);
		}
	}
}
