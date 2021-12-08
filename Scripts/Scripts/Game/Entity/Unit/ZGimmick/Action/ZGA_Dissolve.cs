using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class ZGA_Dissolve : ZGimmickActionBase
{
	[Header("Dissolve 처리될 오브젝트들")]
	[SerializeField]
	private List<GameObject> DissolveObjectList = new List<GameObject>();

	[Header("Dissolve Fade In/Out 여부")]
	[SerializeField]
	private bool IsFadeInDissolve = true;

	[Header("Dissolve 시간")]
	[SerializeField]
	private float DissolveTime = 3f;

	[Header("오브젝트 활성/비활성 시작상태")]
	[SerializeField]
	private bool IsObjectActive = true;

	protected override void InvokeImpl()
	{
		SetModelRenderers();
		Dissolve(IsObjectActive, DissolveTime);

		IsObjectActive = !IsObjectActive;
	}
	protected override void CancelImpl()
	{
	}

	/// <summary> 렌더러 캐싱 </summary>
	private List<Renderer> m_listCachedRenderers = new List<Renderer>();

	private const string DISSOLVE_KEYWORD = "_USEDISSOLVE_ON";
	private int DISSOLVE_ID = Shader.PropertyToID("_Dissolve_Progress");

	/// <summary> 디졸브 연출 </summary>
	private Tweener TweenDissolve;

	private void SetModelRenderers()
	{
		m_listCachedRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>(true));

		m_listCachedRenderers.Clear();

		foreach (var go in DissolveObjectList)
		{
			m_listCachedRenderers.AddRange(new List<Renderer>(go.GetComponentsInChildren<Renderer>(true)));
		}
		ResetDissolve();
	}

	/// <summary> 그림자 on/off </summary>
	private void SetShadowCasting(bool bActive)
	{
		foreach (Renderer r in m_listCachedRenderers)
		{
			if (null == r)
				continue;

			if (bActive)
			{
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			}
			else
			{
				r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
		}
	}

	/// <summary> 디졸브 연출 </summary>
	private void Dissolve(bool bDissolve, float duration = 3f, float delayTime = 0f, Action onFinish = null)
	{
		TweenDissolve?.Kill(true);

		//디졸브 관련 키워드 활성화
		EnableKeywordForRenderer(DISSOLVE_KEYWORD);

		if (bDissolve)
		{
			//사라지기
			TweenDissolve = DOTween.To(() => 0f, (value) => ChangeMaterialFloat(DISSOLVE_ID, value), 1f, duration).OnComplete(() =>
			{
				onFinish?.Invoke();
			}).SetDelay(delayTime).OnStart(() =>
			{
				// 그림자 꺼주기
				SetShadowCasting(false);
			});
		}
		else
		{
			//나타나기
			TweenDissolve = DOTween.To(() => 1f, (value) => ChangeMaterialFloat(DISSOLVE_ID, value), 0f, duration).OnComplete(() =>
			{
				//디졸브 비활성화
				ResetDissolve();
				onFinish?.Invoke();
			});
		}
	}

	/// <summary> 디졸브 리셋 </summary>
	private void ResetDissolve()
	{
		DisableKeywordForRenderer(DISSOLVE_KEYWORD);

		TweenDissolve?.Kill(true);
		TweenDissolve = null;
		ChangeMaterialFloat(DISSOLVE_ID, 0);
	}

	/// <summary> keyword 화성화 </summary>
	private void EnableKeywordForRenderer(string keyword)
	{
		foreach (Renderer r in m_listCachedRenderers)
		{
			if (null == r)
				continue;

			foreach (var mat in r.materials)
			{
				mat.EnableKeyword(keyword);
			}
		}
	}

	/// <summary> keyword 비화성화 </summary>
	private void DisableKeywordForRenderer(string keyword)
	{
		foreach (Renderer r in m_listCachedRenderers)
		{
			if (null == r)
				continue;

			foreach (var mat in r.materials)
			{
				mat.DisableKeyword(keyword);
			}
		}
	}

	/// <summary> 메테리얼 프로퍼티 변경 </summary>
	public void ChangeMaterialFloat(int propertyId, float newValue)
	{
		foreach (Renderer r in m_listCachedRenderers)
		{
			if (null == r)
				continue;

			foreach (var mat in r.materials)
			{
				ChangeMaterialFloat(mat, propertyId, newValue);
			}
		}
	}

	/// <summary> 메테리얼 프로퍼티 변경 </summary>
	private void ChangeMaterialFloat(Material srcMat, int propertyId, float newValue)
	{
		if (!srcMat.HasProperty(propertyId))
			return;

		srcMat.SetFloat(propertyId, newValue);
	}
}