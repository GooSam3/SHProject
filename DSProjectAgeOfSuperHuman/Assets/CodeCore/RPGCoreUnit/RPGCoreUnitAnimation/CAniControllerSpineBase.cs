using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public abstract class CAniControllerSpineBase : CAniControllerBase
{
	[SerializeField]
	private string IdleAniName = "idle";

	private MeshRenderer m_pMeshRenderer = null;
	private SkeletonAnimation m_pSpineInstance = null;
	private Spine.AnimationState m_pSpineControl = null;

	private bool m_bAnimationDuration = false;
	private float m_fAnimationDuration = 0;
	private float m_fAnimationCurrent = 0;
	private string m_strAnimationCurrent;

	private Dictionary<string, Spine.Animation> m_mapTrackInstance = new Dictionary<string, Spine.Animation>();
	//-----------------------------------------------------------
	protected override void OnAniControllerInitialize(CAnimationBase pAnimationOwner) 
	{
		m_pMeshRenderer = GetComponent<MeshRenderer>();
		m_pAnimationOwner = pAnimationOwner;
		m_pSpineInstance = GetComponent<SkeletonAnimation>();
		m_pSpineInstance.Initialize(true, true);
		m_pSpineInstance.AnimationName = IdleAniName;
		m_pSpineInstance.loop = true;

		m_pSpineControl = m_pSpineInstance.AnimationState;
		m_pSpineControl.Start		+= HandleSpineEventStart;
		m_pSpineControl.Interrupt	+= HandleSpineEventInterrupt;
		m_pSpineControl.End		+= HandleSpineEventEnd;
		m_pSpineControl.Dispose	+= HandleSpineEventDispose;
		m_pSpineControl.Complete	+= HandleSpineEventComplete;
		m_pSpineControl.Event		+= HandleSpineEventCustom;
		
		ExposedList<Spine.Animation>.Enumerator it = m_pSpineControl.Data.SkeletonData.Animations.GetEnumerator();
		while (it.MoveNext())
		{
			m_mapTrackInstance.Add(it.Current.Name, it.Current);
		}		
	}

	protected override void OnAniControllerAnimationStart(CAnimationBase.SAnimationUsage rAnimUsage) 
	{
		if (m_mapTrackInstance.ContainsKey(rAnimUsage.AnimName) == false)
		{
			ProtAniControllerDelegateReset();
			return;
		}
		SetMonoActive(true);
		m_pSpineControl.ClearListenerNotifications();	
		m_pSpineControl.SetAnimation(0, rAnimUsage.AnimName, rAnimUsage.bLoop);
		m_pSpineControl.TimeScale = rAnimUsage.fAniSpeed;
		m_strAnimationCurrent = rAnimUsage.AnimName;
		if (rAnimUsage.bLoop && rAnimUsage.fDuration != 0f)
		{
			m_bAnimationDuration = true;
			m_fAnimationDuration = rAnimUsage.fDuration;
			m_fAnimationCurrent = 0;
		}
	}

	protected override void OnUnityUpdate()
	{
		PrivSpineAnimationUpdateDuration();
	}

	protected override void OnAniControllerShaderReset()
	{
		m_pSpineInstance.CustomMaterialOverride.Clear();
	}

	protected override void OnAniControllerShaderChange(CAnimationBase.SAnimationShaderInfo pShaderInfo) 
	{
		AtlasAssetBase[] aAtlas = m_pSpineInstance.skeletonDataAsset.atlasAssets;
		for (int i = 0; i < aAtlas.Length; i++)
		{
			IEnumerator<Material> itMat = aAtlas[i].Materials.GetEnumerator();
			while(itMat.MoveNext())
			{
				Material pNewMat = new Material(itMat.Current);
				pNewMat.shader = pShaderInfo.ChangeShader;
				m_pSpineInstance.CustomMaterialOverride[itMat.Current] = pNewMat;
				pShaderInfo.ShaderMaterial.Add(pNewMat);
			}
		}
	}

	protected override void OnAniControllerIdle()
	{
		PrivSpineAnimationIdle();
	}

	protected override void OnAniControllerSkinChange(string strSkinName)
	{
		PrivSpineAnimationSkinChange(strSkinName);
	}

	//--------------------------------------------------------------------------
	public override bool HasAnimation(string strAniName)
	{
		return m_mapTrackInstance.ContainsKey(strAniName);
	}

	protected void ProtSpineAnimationOrderInLayer(int iLayer)
	{
		m_pMeshRenderer.sortingOrder = iLayer;
	}

	//---------------------------------------------------------------------------
	private void PrivSpineAnimationIdle()
	{
		if (HasAnimation(IdleAniName))
		{
			m_pSpineControl.ClearListenerNotifications();
			m_pSpineControl.SetAnimation(0, IdleAniName, true);
			m_pSpineControl.TimeScale = 1f;
		}
		else
		{
			SetMonoActive(false);
		}
	}

	private void PrivSpineAnimationFinish(TrackEntry trackEntry)
	{
		m_bAnimationDuration = false;
		ProtAniControllerFinish(trackEntry.Animation.Name, true);
		OnSpineEventAnimationEnd(trackEntry);
	}

	private void PrivSpineAnimationUpdateDuration()
	{
		if (m_bAnimationDuration == false) return;

		m_fAnimationCurrent += Time.deltaTime;
		if (m_fAnimationCurrent >= m_fAnimationDuration)
		{
			PrivSpineAnimationFinish(m_pSpineControl.GetCurrent(0));
		}
	}

	private void PrivSpineAnimationSkinChange(string strSkinName, bool bPoseReset = false)
	{
		Skin pSkin = m_pSpineInstance.Skeleton.Data.FindSkin(strSkinName);
		if (pSkin != null)
		{
			m_pSpineInstance.Skeleton.SetSkin(pSkin);
		}

		if (bPoseReset)
		{
			m_pSpineInstance.Skeleton.SetSlotsToSetupPose();
		}

		m_pSpineInstance.LateUpdate();
	}
	//---------------------------------------------------------------------------

	private void HandleSpineEventStart(TrackEntry trackEntry)
	{

	}
	private void HandleSpineEventInterrupt(TrackEntry trackEntry)
	{

	}

	private void HandleSpineEventEnd(TrackEntry trackEntry)
	{

	}

	private void HandleSpineEventDispose(TrackEntry trackEntry)
	{

	}

	private void HandleSpineEventComplete(TrackEntry trackEntry)
	{
		if (trackEntry.Loop == false)
		{
			if (m_strAnimationCurrent == trackEntry.Animation.Name)
			{
				PrivSpineAnimationFinish(trackEntry);
			}
		}
	}

	private void HandleSpineEventCustom(TrackEntry trackEntry, Spine.Event eventType)
	{
		ProtAniControllerEvent(eventType.Data.Name, eventType.Int, eventType.Float);
		OnSpineEventCustom(trackEntry, eventType);
	}

	protected virtual void OnSpineEventCustom(TrackEntry trackEntry, Spine.Event eventType) { }
	protected virtual void OnSpineEventAnimationEnd(TrackEntry trackEntry) { }

	//--------------------------------------------------------------------------------

}
