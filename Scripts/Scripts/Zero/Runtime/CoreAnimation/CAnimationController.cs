using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CAnimationController : CMonoBase
{
	private Animation mAnimationInstance = null;
	private UnityAction<string> mEventAnimationEnd = null;
	private bool mPlaying = false;
	public bool IsPlaying { get { return mAnimationInstance.isPlaying; } }

	//------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		mAnimationInstance = GetComponent<Animation>();
	}

	protected virtual void Update()
	{
		if (mPlaying)
		{
			if (mAnimationInstance.isPlaying == false)
			{
				AnimationEnd();			
			}
		}
	}

	//--------------------------------------------------
	public void DoAnimationPlay(string _aniName, UnityAction<string> _eventAnimationEnd = null,  bool _rewind = true,  bool _reverse = false, float _aniSpeed = 1)
	{
		AnimationClip aniClip = mAnimationInstance.GetClip(_aniName);
		AnimationState aniState = mAnimationInstance[_aniName];
		if (aniClip)
		{
			mEventAnimationEnd = _eventAnimationEnd;
			PlayAnimation(aniClip, aniState, _aniSpeed, _rewind, _reverse);
		}
	}

	public void DoAnimationStop()
	{
		mAnimationInstance.Stop();
		AnimationEnd();
	}

	//-----------------------------------------------------------------------
	private void PlayAnimation(AnimationClip _aniClip, AnimationState _aniState, float _aniSpeed, bool _rewind, bool _reverse)
	{
		if (_rewind)
		{
			mAnimationInstance.Rewind();
			mAnimationInstance.Play();
			mAnimationInstance.Sample();
			mAnimationInstance.Stop();			
		}

		if (_reverse)
		{
			_aniState.speed = -1 * _aniSpeed;
			_aniState.time = _aniState.length;
		}
		else
		{
			_aniState.speed = _aniSpeed;
			_aniState.time = 0;
		}
		mPlaying = true;
		mAnimationInstance.clip = _aniClip;


		mAnimationInstance.Play();
	}

	private void AnimationEnd()
	{
		mPlaying = false;
		mEventAnimationEnd?.Invoke(mAnimationInstance.clip.name);
		mEventAnimationEnd = null;
	}
}
