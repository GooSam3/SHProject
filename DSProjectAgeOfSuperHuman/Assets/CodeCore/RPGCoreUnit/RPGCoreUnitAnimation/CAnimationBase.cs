using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using uTools;

public abstract class CAnimationBase : CMonoBase
{
	public struct SAnimationUsage
	{
		public string AnimName;
		public bool bLoop;
		public float fDuration;
		public float fAniSpeed;
	}

	[System.Serializable]
	public class SAnimationShaderInfo
	{
		public string AniGroupName = "None";
		public Shader ChangeShader = null;
		public int	 ShaderType = 0;
		[HideInInspector]
		public List<Material> ShaderMaterial = new List<Material>(); 
		[SerializeField]
		public List<SAnimationMaterialValue> MaterialValue = new List<SAnimationMaterialValue>();
	}

	[System.Serializable]
	public class SAnimationMaterialValue
	{
		public string ValueName = "None";
		public uTweener ValueTween = null;
	}

	[SerializeField]
	private List<SAnimationShaderInfo> ShaderParts = new List<SAnimationShaderInfo>();

	private SAnimationShaderInfo m_pCurrentMaterial = null;
	private bool m_bUpdateMaterial = false;
	
	private CUnitAnimationBase m_pAnimationOwner = null;
	protected CMultiSortedDictionary<string, CAniControllerBase> m_mapAniController = new CMultiSortedDictionary<string, CAniControllerBase>();
	//-----------------------------------------------------------
	internal void ImportAnimationInitialize(CUnitAnimationBase pAnimOwner)
	{
		m_pAnimationOwner = pAnimOwner;

		List<CAniControllerBase> pListAniCtr = new List<CAniControllerBase>();
		GetComponentsInChildren(true, pListAniCtr);

		for (int i = 0; i < pListAniCtr.Count; i++)
		{
			pListAniCtr[i].ImportAniControllerInitialize(this);
			m_mapAniController.Add(pListAniCtr[i].GetAniGroupName(), pListAniCtr[i]);

		}
		OnAnimationInitialize(pAnimOwner);
	}

	internal void ImportAnimationAllGroupIdle()
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].ImportAniControllerIdle();
			}
		}
	}

	internal void ImportAnimationHideOther(string strWithoutGroup)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				if (pList[i].GetAniGroupName() != strWithoutGroup)
				{
					pList[i].ImportAniControllerHide();
				}
			}
		}
	}

	internal void ImportAnimationSkinChange(string strAniGroup, string strSkinName)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				if (pList[i].GetAniGroupName() == strAniGroup)
				{
					pList[i].ImportAniControllerSkinChange(strSkinName);
				}
			}
		}
	}
	//--------------------------------------------------------------------------------------

	public void DoAnimationStart(ref SAnimationUsage rAnimUsage, UnityAction<string, bool> delFinish, UnityAction<string, int, float> delAniEvent)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		bool bFindGroup = false;
		while (it.MoveNext())
		{			
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				if (pList[i].HasAnimation(rAnimUsage.AnimName))
				{
					if (bFindGroup)
					{
						pList[i].ImportAniControllerAnimationStart(rAnimUsage, null, null);
					}
					else
					{
						bFindGroup = true;
						pList[i].ImportAniControllerAnimationStart(rAnimUsage, delFinish, delAniEvent);
					}
				}
			}
		}

		if (bFindGroup == false)
		{
			delFinish?.Invoke(rAnimUsage.AnimName, false);
		}
	}

	public void DoAnimationIdle()
	{
		ImportAnimationAllGroupIdle();
	}

	public void DoAnimationSkinChange(string strAniGroupName, string strSkinName)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				if (pList[i].GetAniGroupName() == strAniGroupName)
				{
					pList[i].ImportAniControllerSkinChange(strSkinName);
				}
			}
		}
	}

	public void DoAnimationShapeShowHide(bool bShow)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].SetMonoActive(bShow);
			}
		}
	}

	//---------------------------------------------------------------------------------------
	private void Update()
	{
		if (m_bUpdateMaterial)
		{
			if (CheckAnimationMaterialTweenActive())
			{
				UpdateAnimationTween();
			}
			else
			{
				m_bUpdateMaterial = false;
			}
		}
		OnUnityUpdate();
	}

	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		for (int i = 0; i < ShaderParts.Count; i++)
		{
			for (int j = 0; j < ShaderParts[i].MaterialValue.Count; j++)
			{
				ShaderParts[i].MaterialValue[j].ValueTween.enabled = false;
				ShaderParts[i].MaterialValue[j].ValueTween.ignoreTimeScale = false;
			}
		}
	}
	//---------------------------------------------------------------------------------------
	protected void ProtAnimationMaterialChangeByPreSet(int ePresetType)
	{
		if (ePresetType == 0)
		{
			m_bUpdateMaterial = false;
			PrivAnimationMaterialReset();
		}
		else
		{
			SAnimationShaderInfo pMat = FindSpineMaterial(ePresetType);
			if (pMat != null)
			{
				PrivAnimationMaterialValueApply(pMat);
			}
		}
	}

	//---------------------------------------------------------------------------------------
	private SAnimationShaderInfo FindSpineMaterial(int eMatType)
	{
		SAnimationShaderInfo pMatInfo = null;
		for (int i = 0; i < ShaderParts.Count; i++)
		{
			if (ShaderParts[i].ShaderType == eMatType)
			{
				pMatInfo = ShaderParts[i];
				break;
			}
		}
		return pMatInfo;
	}

	private void PrivAnimationMaterialValueApply(SAnimationShaderInfo pShaderInfo)
	{
		m_bUpdateMaterial = true;
		m_pCurrentMaterial = pShaderInfo;
		
		pShaderInfo.ShaderMaterial.Clear();

		PrivAnimationShaderChange(pShaderInfo);
		for (int i = 0; i < pShaderInfo.MaterialValue.Count; i++)
		{
			pShaderInfo.MaterialValue[i].ValueTween.enabled = true;
			pShaderInfo.MaterialValue[i].ValueTween.ResetPlay();
			pShaderInfo.MaterialValue[i].ValueTween.Play();
		}
		UpdateAnimationTween();
	}

	private bool CheckAnimationMaterialTweenActive()
	{
		bool bActive = false;
		if (m_pCurrentMaterial != null)
		{
			for (int i = 0; i < m_pCurrentMaterial.MaterialValue.Count; i++)
			{
				if (m_pCurrentMaterial.MaterialValue[i].ValueTween.IsAcive == true)
				{
					bActive = true;
					break;
				}
				else
				{
					m_pCurrentMaterial.MaterialValue[i].ValueTween.Sample(1f, false);
				}
			}
		}

		return bActive;
	}

	private void UpdateAnimationTween()
	{
		if (m_pCurrentMaterial == null) return;

		for (int i = 0; i < m_pCurrentMaterial.MaterialValue.Count; i++)
		{
			SAnimationMaterialValue pMatValue = m_pCurrentMaterial.MaterialValue[i];
			if (pMatValue.ValueTween is uTweenValue)
			{
				uTweenValue pTweenValue = pMatValue.ValueTween as uTweenValue;

				for (int j = 0; j < m_pCurrentMaterial.ShaderMaterial.Count; j++)
				{
					m_pCurrentMaterial.ShaderMaterial[j].SetFloat(pMatValue.ValueName, pTweenValue.value);
				}
			}
			else if (pMatValue.ValueTween is uTweenColor)
			{
				
			} 
		}
	}

	private void PrivAnimationMaterialReset()
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while(it.MoveNext())
		{
			List<CAniControllerBase> pListAni = it.Current;
			for (int i = 0; i < pListAni.Count; i++)
			{
				pListAni[i].ImportAniControllerShaderReset();
			}
		}
	}
	
	private void PrivAnimationShaderChange(SAnimationShaderInfo pShaderInfo)
	{
		if (m_mapAniController.ContainsKey(pShaderInfo.AniGroupName))
		{
			List<CAniControllerBase> pList = m_mapAniController[pShaderInfo.AniGroupName];
			for (int i = 0; i < pList.Count; i++)
			{
				pList[i].ImportAniControllerShaderChange(pShaderInfo);
			}
		}
	}


	//----------------------------------------------------------
	protected virtual void OnAnimationInitialize(CUnitAnimationBase pAnimOwner) { }
	protected virtual void OnAnimationStart(ref SAnimationUsage rAnimUsage) { }
	protected virtual void OnUnityUpdate() { }
}
