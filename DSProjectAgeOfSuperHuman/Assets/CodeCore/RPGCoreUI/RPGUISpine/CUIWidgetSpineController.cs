using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Spine.Unity;
using Spine;

[RequireComponent(typeof(SkeletonGraphic))]
public abstract class CUIWidgetSpineController : CUIWidgetBase
{
	private string m_strPlayAnimationName;
	private SkeletonGraphic m_pSpineInstance = null;
	private Spine.AnimationState m_pSpineControl = null;
	private Dictionary<string, Spine.Animation> m_mapTrackInstance = new Dictionary<string, Spine.Animation>();
	//---------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pSpineInstance = GetComponent<SkeletonGraphic>();
		m_pSpineInstance.Initialize(true);

		m_pSpineControl = m_pSpineInstance.AnimationState;
		m_pSpineControl.Start += HandleSpineEventStart;
		m_pSpineControl.Interrupt += HandleSpineEventInterrupt;
		m_pSpineControl.End += HandleSpineEventEnd;
		m_pSpineControl.Dispose += HandleSpineEventDispose;
		m_pSpineControl.Complete += HandleSpineEventComplete;
		m_pSpineControl.Event += HandleSpineEventCustom;

		ExposedList<Spine.Animation>.Enumerator it = m_pSpineControl.Data.SkeletonData.Animations.GetEnumerator();
		while (it.MoveNext())
		{
			m_mapTrackInstance.Add(it.Current.Name, it.Current);
		}
	}

	//---------------------------------------------------------
	protected void ProtSpineControllerAnimation(string strAniName, bool bLoop = false)
	{
		if (m_mapTrackInstance.ContainsKey(strAniName) == false) return;
		m_strPlayAnimationName = strAniName;
		m_pSpineControl.SetAnimation(0, strAniName, bLoop);
	}

	//----------------------------------------------------------
	private void HandleSpineEventStart(TrackEntry trackEntry)
	{

	}
	private void HandleSpineEventInterrupt(TrackEntry trackEntry)
	{
		OnSpineControllerAnimationInterrupt(trackEntry.Animation.Name);
	}

	private void HandleSpineEventEnd(TrackEntry trackEntry)
	{
		 
	}

	private void HandleSpineEventDispose(TrackEntry trackEntry)
	{

	}

	private void HandleSpineEventComplete(TrackEntry trackEntry)
	{
		if (m_strPlayAnimationName != trackEntry.Animation.Name) return;
		OnSpineControllerAnimationEnd(trackEntry.Animation.Name);
	}

	private void HandleSpineEventCustom(TrackEntry trackEntry, Spine.Event eventType)
	{
		
	}

	//---------------------------------------------------------------------------------
	protected virtual void OnSpineControllerAnimationEnd(string strAniName) { }
	protected virtual void OnSpineControllerAnimationInterrupt(string strAniName) { }
}
