using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEffectTransformBase : CEffectBase
{
	[System.Serializable]
	public class SEffectTransform
	{
		public Transform TargetTransform = null;
		public Jun_TweenRuntime TweenInstance = null;

		[HideInInspector]
		public Vector3 OriginPosition = Vector3.zero;
		[HideInInspector]
		public Vector3 OriginScale = Vector3.zero;
		[HideInInspector]
		public Vector3 OriginRotation = Vector3.zero;
		[HideInInspector]
		public Vector3 TweenFrom = Vector3.zero;
		[HideInInspector]
		public Vector3 TweenTo = Vector3.zero;
	}
	
	[SerializeField]
	private List<SEffectTransform> EffectTransform = new List<SEffectTransform>();
	
	//--------------------------------------------------------------------------------
	protected override void OnEffectInitialize()
	{
		base.OnEffectInitialize();
		for(int i = 0; i < EffectTransform.Count; i++)
		{
			if (EffectTransform[i].TargetTransform)
			{
				PrivEffectTransformInitialize(EffectTransform[i]);
			}
		}
	}

	protected override void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams)
	{
		base.OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);
		ProtEffectTransformStart();
	}

	protected override void OnEffectEnd(bool bForce)
	{
		base.OnEffectEnd(bForce);
		PrivEffectTransformReset();
	}
	//--------------------------------------------------------------------------------

	protected void ProtEffectTransformRefreshInstance(int iIndex, Transform pTargetTransform, Vector3 vecDest)
	{
		if (iIndex < EffectTransform.Count)
		{
			SEffectTransform pEffectTransform = EffectTransform[iIndex];
			pEffectTransform.TargetTransform = pTargetTransform;
			pEffectTransform.TweenInstance.firstTween.toVector = vecDest;
			PrivEffectTransformInitialize(pEffectTransform);
		}
	}
	protected void ProtEffectTransformStart()
	{
		for (int i = 0; i < EffectTransform.Count; i++)
		{
			EffectTransform[i].TweenInstance.enabled = true;
			EffectTransform[i].TweenInstance.Rewind();
			EffectTransform[i].TweenInstance.firstTween.toVector = EffectTransform[i].OriginPosition + EffectTransform[i].TweenTo;
			EffectTransform[i].TweenInstance.firstTween.fromVector = EffectTransform[i].OriginPosition + EffectTransform[i].TweenFrom;
			EffectTransform[i].TweenInstance.Play();
		}
	}

	protected void ProtEffectTransformReset()
	{
		for (int i = 0; i < EffectTransform.Count; i++)
		{
			EffectTransform[i].TweenInstance.enabled = false;
			EffectTransform[i].TargetTransform.localPosition = EffectTransform[i].OriginPosition;
			Quaternion rQuat = Quaternion.identity;
			rQuat.eulerAngles = EffectTransform[i].OriginRotation;
			EffectTransform[i].TargetTransform.rotation = rQuat;
			EffectTransform[i].TargetTransform.localScale = EffectTransform[i].OriginScale;
			EffectTransform[i].TweenInstance.firstTween.toVector = EffectTransform[i].TweenTo;
			EffectTransform[i].TweenInstance.firstTween.fromVector = EffectTransform[i].TweenFrom;
			EffectTransform[i].TweenInstance.firstTween.Init(null);
		}
	}

	//-----------------------------------------------------------
	private void PrivEffectTransformInitialize(SEffectTransform pEffectTransform)
	{
		pEffectTransform.OriginPosition = pEffectTransform.TargetTransform.localPosition;
		pEffectTransform.OriginRotation = pEffectTransform.TargetTransform.rotation.eulerAngles;
		pEffectTransform.OriginScale = pEffectTransform.TargetTransform.localScale;
		pEffectTransform.TweenFrom = pEffectTransform.TweenInstance.firstTween.fromVector;
		pEffectTransform.TweenTo = pEffectTransform.TweenInstance.firstTween.toVector;
		pEffectTransform.TweenInstance.firstTween.Init(pEffectTransform.TargetTransform);
		pEffectTransform.TweenInstance.enablePlay = false;
		pEffectTransform.TweenInstance.awakePlay = false;
		pEffectTransform.TweenInstance.ignoreTimeScale = false;
	}

	private void PrivEffectTransformReset()
	{
		for (int i = 0; i < EffectTransform.Count; i++)
		{
			EffectTransform[i].TweenInstance.enabled = false;
			EffectTransform[i].TargetTransform.localPosition = EffectTransform[i].OriginPosition;
			Quaternion rQuat = Quaternion.identity;
			rQuat.eulerAngles = EffectTransform[i].OriginRotation;
			EffectTransform[i].TargetTransform.rotation = rQuat;
			EffectTransform[i].TargetTransform.localScale = EffectTransform[i].OriginScale;
			EffectTransform[i].TweenInstance.firstTween.toVector = EffectTransform[i].TweenTo;
			EffectTransform[i].TweenInstance.firstTween.fromVector = EffectTransform[i].TweenFrom;
			EffectTransform[i].TweenInstance.firstTween.Init(null);
		}
	}


}
