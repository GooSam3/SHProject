using System.Collections.Generic;
using UnityEngine;

public class UIShaderClipingUpdater : CUGUIWidgetBase
{
	[SerializeField] bool			UpdateCliping = true;
	[SerializeField] int			SortingOffset = 0;
 	[SerializeField] RectTransform ClipingTransform = null;

	private ParticleSystem					mParticleMaster;
	private List<ParticleSystemRenderer>	m_listParticle = new List<ParticleSystemRenderer>();
	private List<Material>					m_listTargetMaterial = new List<Material>();
	private Vector3 []					mVectorNote = new Vector3[4];
	//------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mParticleMaster = GetComponent<ParticleSystem>();
		
		GetComponentsInChildren<ParticleSystemRenderer>(true, m_listParticle);
		
		for (int i = 0; i < m_listParticle.Count; i++)
		{
			Material [] arrMat = m_listParticle[i].materials;

			for (int j = 0; j < arrMat.Length; j++)
			{
				m_listTargetMaterial.Add(arrMat[j]);
			}
		}
		ConvertRectToWorldPosition();
	}

	public void SetClipingTransfrom(RectTransform _clipingTransfrom)
	{
		ClipingTransform = _clipingTransfrom;
		if (UpdateCliping)
		{
			ConvertRectToWorldPosition();
		}
	}

	//---------------------------------------------------------------------------
	protected virtual void Update()
	{
		UpdateCanvasLayer();

		if (UpdateCliping)
		{
			ConvertRectToWorldPosition();
		}
	}

	//---------------------------------------------------------------------------
	private void ConvertRectToWorldPosition()
	{
		if (ClipingTransform == null) return;

		ClipingTransform.GetWorldCorners(mVectorNote);
	
		float ClipRectX = 0;
		float ClipRectY = 0;
		float ClipRectZ = 0;
		float ClipRectW = 0;

		ClipRectX = mVectorNote[0].x;
		ClipRectY = mVectorNote[0].y;
		ClipRectZ = mVectorNote[2].x;
		ClipRectW = mVectorNote[2].y;

		for (int i = 0; i < m_listTargetMaterial.Count; i++)
		{
			Material mat = m_listTargetMaterial[i];
			mat.SetFloat("_ClipRectX", ClipRectX);
			mat.SetFloat("_ClipRectY", ClipRectY);
			mat.SetFloat("_ClipRectZ", ClipRectZ);
			mat.SetFloat("_ClipRectW", ClipRectW);
		}
	}

	private void UpdateCanvasLayer()
	{
		for (int i = 0; i < m_listParticle.Count; i++)
		{
			m_listParticle[i].sortingLayerID = mUIFrameParent.LayerID;
			m_listParticle[i].sortingOrder = mUIFrameParent.LayerOrder + 1 + SortingOffset;
		}
	}
}
